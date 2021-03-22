using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Level;
using UnityEngine;
namespace Players
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(SplineFollower))]
	[RequireComponent(typeof(PlayerAnimator))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Player))]
	[RequireComponent(typeof(IBoostable))]
	public class PlayerMover : MonoBehaviour, IPausable, IBarrierAffected, IMover, IPlayerControllable, IDefendable
	{
		public delegate void             UAction();
		public delegate void             UActionFloat(float desiredSpeed, bool justStop);
		public static event UAction      OnSlide;
		public static event UAction      OnSpeedBoost;
		public static event UActionFloat OnSpeedBoostStopped;
		public static event Action       OnSwipeAction;
		public static event Action       OnJumpAction;
		public static event Action       OnSlideBreak;

		[SerializeField] private Player player;

		[Header("Move")]
		[SerializeField] private float defaultSpeed;
		[SerializeField] private float changeRoadTime;
		[SerializeField] private float changeRoadTreshold;
		[SerializeField] private float accelerationSpeed;
		[SerializeField] private float maxSpeed;
		[SerializeField] private bool  isAccelerating;

		[Header("Jump")]
		[SerializeField] private float jumpHeigh;
		[SerializeField] private float upJumpTime;
		[SerializeField] private float downJumpTime;
		[SerializeField] private float inAirTime;

		[Header("Slide")]
		[SerializeField] private float slideTime;

		[Header("Level")]
		[SerializeField] private LevelHolder levelHolder;
		[SerializeField] private int currentRoadId;

		[Header("Dreamteck")]
		[SerializeField] private SplineFollower follower;

		[Header("Skills")]
		[SerializeField] private float speedSkill;
		[SerializeField] private float accelerationSkill;

		[Header("Effects")]
		[SerializeField] private ParticleSystem slideEffect;
		[SerializeField] private ParticleSystem jumpEffect;

		private float            _desiredSpeed; // Speed to accelerate to 
		private bool             _isDamping;
		private List<IBoostable> _boosters;

		private new CapsuleCollider _collider;
		private     float           _defaultColliderHeight;
		private     float           _startOffset;
		private     float           _nextRoadOffset;
		private     Coroutine       _changeRoadCoroutine;

		private float _playerSlideOffset = 0.5f;
		private bool  _isSliding;

		private PlayerAnimator _animator;
		private float          _jumpY;
		private bool           _isJump;
		private bool           _isPaused;
		private bool           _isUnderControl;
		private float          _actualSpeed;
		private bool           _defended;
		private bool           _inDeathLoop;
		private bool           _slideBreak;

#region UNITY EVENTS
		private void Awake()
		{
			_collider              = GetComponent<CapsuleCollider>();
			_defaultColliderHeight = _collider.height;
			if (player == null)
				player = GetComponent<Player>();
			if (follower == null)
				follower = GetComponent<SplineFollower>();
			if (levelHolder == null)
				levelHolder = FindObjectOfType<LevelHolder>();
			_animator = GetComponent<PlayerAnimator>();
			_collider = GetComponent<CapsuleCollider>();
			_boosters = new List<IBoostable>();
			_boosters.AddRange(GetComponents<IBoostable>());
		}

		private void Start()
		{
			SwipeInput.OnPlayerSwiped += OnSwipe;
			_desiredSpeed             =  defaultSpeed;
			_animator.SetRotationTime(changeRoadTime);
			SetPlayerMovementType(MovementType.Run);
			ChangeSpeed();
			SetupSkills();
			RegisterPausable();
			RegisterControllable();
			if (GameProcess.isTutorial)
			{
				TutorialController.OnTutorialFakeInput += OnSwipe;
			}
		}

		private void Update()
		{
			if (_isPaused) return;
			if (isAccelerating) Accelerate();
			if (_isDamping) Damping();
		}

		private void OnEnable()
		{
			DeathLoop.OnEnterDeathLoop += DeathLoopEnter;
			DeathLoop.OnExitDeathLoop  += DeathLoopExit;
		}

		private void OnDisable()
		{
			DeathLoop.OnEnterDeathLoop -= DeathLoopEnter;
			DeathLoop.OnExitDeathLoop  -= DeathLoopExit;
		}
  #endregion

		private void SetupSkills()
		{
			speedSkill        = PlayerDataHolder.GetSpeed()        / 10;
			accelerationSkill = PlayerDataHolder.GetAcceleration() / 10;

			defaultSpeed      += speedSkill;
			accelerationSpeed += accelerationSkill;
		}

		private void Accelerate()
		{
			if (_isPaused) return;
			_actualSpeed += accelerationSpeed * Time.deltaTime;
			if (_actualSpeed >= _desiredSpeed)
			{
				_actualSpeed   = _desiredSpeed;
				isAccelerating = false;
				_isDamping     = false;
			}
			ChangeSpeed();
		}

		private void Damping()
		{
			if (_isPaused) return;
			_actualSpeed -= accelerationSpeed * Time.deltaTime;
			if (_actualSpeed <= _desiredSpeed)
			{
				_actualSpeed = _desiredSpeed;
				_isDamping   = false;
			}
			ChangeSpeed();
		}

		private void ChangeSpeed()
		{
			if (_isPaused)
			{
				follower.followSpeed = 0;
				return;
			}
			if (_actualSpeed < _desiredSpeed)
			{
				isAccelerating = true;
			}
			else if (_actualSpeed > _desiredSpeed)
			{
				_isDamping = true;
			}
			follower.followSpeed = _actualSpeed;
			_animator.SetAnimatorSpeed(_actualSpeed);
		}

		private void OnSwipe(SwipeInput.SwipeType swipeType)
		{
			if (_isUnderControl == false && _inDeathLoop == false) return;
			if (_isUnderControl == false) return;
			if (_changeRoadCoroutine == null)
			{
				ChangePath2(swipeType);
			}
			else
			{
				switch (swipeType)
				{
					case SwipeInput.SwipeType.Up:
						StartJump();
						break;
					case SwipeInput.SwipeType.Down:
						StartSlide();
						break;
					case SwipeInput.SwipeType.Tap:
						break;
				}
			}
		}

		private void StartSlide()
		{
			if ( /*!isJump && */!_isSliding)
			{
				_animator.SetSlideAnimation(true);
				if (_isJump)
				{
					StopJump();
					_animator.SetJumpAnimation(false);
				}
				_slideBreak = false;
				slideEffect.Play();
				_isSliding          = true;
				_collider.direction = 2;
				SetPlayerMovementType(MovementType.Slide);
				OnSlide?.Invoke();
				StartCoroutine(Slide());
			}
		}
		private IEnumerator Slide()
		{
			float timer = 0;
			while (timer < slideTime && !_slideBreak)
			{
				_jumpY =  Mathf.Lerp(0, -_playerSlideOffset, (timer) / slideTime);
				timer  += Time.deltaTime;
				SetupOffset();
				yield return null;
			}
			timer = 0;
			while (timer < slideTime / 3 && !_slideBreak)
			{
				_jumpY =  Mathf.Lerp(-_playerSlideOffset, 0, (timer * 3) / slideTime);
				timer  += Time.deltaTime;
				SetupOffset();
				yield return null;
			}
			StopSlide();
		}
		private void StopSlide()
		{
			_isSliding = false;
			slideEffect.Stop();
			_jumpY              = 0;
			_collider.direction = 1;
			SetupOffset();
			SetPlayerMovementType(MovementType.Run);
			_animator.SetSlideAnimation(false);
		}


		private void StartJump()
		{
			if (!_isJump /*&& !isSliding*/)
			{
				_isJump = true;
				jumpEffect.Play();
				OnJumpAction?.Invoke();
				if (_isSliding)
				{
					_slideBreak = true;
					OnSlideBreak?.Invoke();
					_animator.SetSlideAnimation(false);
				}
				_jumpY           = 0;
				_collider.height = _defaultColliderHeight / 2;
				_animator.SetJumpAnimation(true);
				SetPlayerMovementType(MovementType.Jump);
				StartCoroutine(HandleJump());
			}
		}
		private IEnumerator HandleJump()
		{
			float upJumpTimer = 0;
			while (upJumpTimer < upJumpTime)
			{
				SetupOffset();
				_jumpY      =  Mathf.Lerp(0, jumpHeigh, upJumpTimer / upJumpTime);
				upJumpTimer += Time.deltaTime;
				yield return null;
			}

			float inAirTimer = 0;
			while (inAirTimer < inAirTime)
			{
				inAirTimer += Time.deltaTime;
				yield return null;
			}
			_animator.SetJumpAnimation(false);
			jumpEffect.Stop();
			float downJumpTimer = 0;
			while (downJumpTimer < downJumpTime)
			{
				SetupOffset();
				_jumpY        =  Mathf.Lerp(jumpHeigh, 0, downJumpTimer / downJumpTime);
				downJumpTimer += Time.deltaTime;
				yield return null;
			}
			StopJump();
			yield return new WaitForSeconds(Time.deltaTime);
		}
		private void StopJump()
		{
			_jumpY           = 0;
			_collider.height = _defaultColliderHeight;
			SetupOffset();
			SetPlayerMovementType(MovementType.Run);
			_isJump = false;
		}


		public static bool CheckLineSwap(SplineComputer dest, SplineResult pos, float lineWidth)
		{
			var p1 = dest.Evaluate(dest.Project(pos.position)); // closest point on side line
			var d  = pos.position - p1.position;
			var v  = Vector3.Project(d,            pos.right * lineWidth); // project vector RIGHT from current on vector between points on lines
			var v2 = Vector3.Dot(d.normalized, pos.normal);
			return (v.magnitude <= lineWidth * 1.1f) && (Mathf.Abs(v2) <=0.01f);
		}
		private void ChangePath2(SwipeInput.SwipeType swipeType)
		{
			Debug.Log($"[Swipe] {swipeType}");
			var targetLine = currentRoadId;
			switch (swipeType)
			{
				case SwipeInput.SwipeType.Left:
					targetLine = Mathf.Clamp(--targetLine, 0, levelHolder._lines.Length - 1);
					break;
				case SwipeInput.SwipeType.Right:
					targetLine = Mathf.Clamp(++targetLine, 0, levelHolder._lines.Length - 1);
					break;
			}
			if (targetLine == currentRoadId) return;
			
			var p0 = follower.result;                                                                              // point on current spline
			var p1 = levelHolder._lines[targetLine].Evaluate(levelHolder._lines[targetLine].Project(p0.position)); // closest point on side line
			var d  = p0.position - p1.position;
			if (!CheckLineSwap(levelHolder._lines[targetLine], p0, levelHolder._lineWidth)) return;
			currentRoadId = targetLine;
			d.Normalize();
			var d2 = new Vector2(Vector3.Dot(d,p1.right), Vector3.Dot(d,p1.normal)); // offset is [X * point.right + Y * point.normal]
			follower.computer      = levelHolder._lines[currentRoadId];
			_changeRoadCoroutine   = StartCoroutine(SwitchLine(d2));
		}
		private IEnumerator SwitchLine(Vector3 baseOffset)
		{
			var changeRoadTimer = 0f;
			while (changeRoadTimer < changeRoadTime)
			{
				follower.motion.offset = Vector3.Lerp(baseOffset, Vector3.zero, changeRoadTimer / changeRoadTime);
				yield return null;
				changeRoadTimer += Time.deltaTime;
			}
			follower.motion.offset = Vector3.zero;
			_changeRoadCoroutine   = null;
		}

		/*
		private bool ChangePath(SwipeInput.SwipeType swipeType)
		{
			if (levelHolder.TryChangePathId(ref currentRoadId, swipeType))
			{
				_nextRoadOffset = levelHolder.GetOffsetById(currentRoadId);
				OnSwipeAction?.Invoke();
				_startOffset         = _currentOffset;
				_changeRoadCoroutine = StartCoroutine(MoveNextRoad());
				_animator.SetRotation(swipeType);
				return true;
			}
			return false;
		}
		private IEnumerator MoveNextRoad()
		{
			var changeRoadTimer = 0f;
			while (Mathf.Abs(_nextRoadOffset - _currentOffset) > changeRoadTreshold)
			{
				_currentOffset  =  Mathf.Lerp(_startOffset, _nextRoadOffset, changeRoadTimer / changeRoadTime);
				changeRoadTimer += Time.deltaTime;
				SetupOffset();
				yield return null;
			}
			SetupOffset();
			_currentOffset = _nextRoadOffset;
			StopCoroutine(_changeRoadCoroutine);
			_changeRoadCoroutine = null;
			yield return new WaitForSeconds(Time.deltaTime);
		}
		/**/

		public float GetDefaultSpeed() => defaultSpeed;
		private void SetupOffset()
		{
			if (_isPaused) return;
			follower.motion.offset = new Vector2(follower.motion.offset.x, _jumpY);
		}
		public  void SetPlayerMovementType(MovementType newMovementType) => player.SetMovementType(newMovementType);
		private void DeathLoopEnter()                                    => _inDeathLoop = true;
		private void DeathLoopExit()                                     => _inDeathLoop = false;

#region IPausable
		public void Pause()
		{
			follower.followSpeed = 0;
			_isPaused            = true;
		}
		public void Resume()
		{
			follower.followSpeed = _actualSpeed;
			_isPaused            = false;
		}
		public void RegisterPausable() => PauseController.RegisterPausable(this);
  #endregion

#region IBarrierAffected
		public void BarrierHit()
		{
			if (_defended)
			{
				_boosters.ForEach(b => b.StopShild());
				_defended = false;
				return;
			}
			_desiredSpeed = defaultSpeed;
			_actualSpeed  = 0;
			OnSpeedBoostStopped?.Invoke(_desiredSpeed, true);
			_boosters.ForEach(b => b.StopAllBoosters());
			ChangeSpeed();
		}
  #endregion

#region IMover
		public void AddSpeed(float speed)
		{
			OnSpeedBoost?.Invoke();
			_desiredSpeed = Mathf.Clamp(_desiredSpeed + speed, defaultSpeed, maxSpeed);
			ChangeSpeed();
		}
		public void ReduceSpeed(float speed)
		{
			_desiredSpeed = Mathf.Clamp(_desiredSpeed - speed, defaultSpeed, maxSpeed);
			OnSpeedBoostStopped?.Invoke(_desiredSpeed, _desiredSpeed == defaultSpeed);
			ChangeSpeed();
		}
		public void SetStartRoad(int roadId)
		{
			currentRoadId     = roadId;
			follower.computer = levelHolder._lines[currentRoadId];
			SetupOffset();
		}
		public float GetPercent() => (float)follower.clampedPercent;
  #endregion

#region IPlayerControllable
		public void StartPlayerControl()   => _isUnderControl = true;
		public void StopPlayerControl()    => _isUnderControl = false;
		public void RegisterControllable() => ControllManager.RegisterControllable(this);
  #endregion

#region IDefenable
		public void SetDefend(bool isDefended) => _defended = isDefended;
  #endregion
	}
}
