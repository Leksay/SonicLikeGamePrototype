using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Game;
using Helpers;
using Internal;
using Level;
using Players.Camera;
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
		public delegate void        UAction();
		public static event UAction OnSlide;
		public static event UAction OnSpeedBoost;
		public static event Action  OnSwipeAction;
		public static event Action  OnJumpAction;

		[SerializeField] private Player player;

		[Header("Move")]
		[SerializeField] private float defaultSpeed = 26f;
		[SerializeField] private float changeRoadTime;
		[SerializeField] private float accelerationSpeed;
		[SerializeField] private float minSpeed = 10f;
		[SerializeField] private float maxSpeed = 50f;
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
		[SerializeField] private SplineFollower _follower;

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

		private PlayerAnimator     _animator;
		private float              _jumpY;
		private bool               _isJump;
		private bool               _isPaused;
		private bool               _isUnderControl;
		private float              _actualSpeed;
		private bool               _defended;
		private bool               _inDeathLoop;
		private bool               _slideBreak;
		private PlayerFollowCamera _followCamera;

#region UNITY EVENTS
		private void Awake()
		{
			RegisterPausable();
			_collider              = GetComponent<CapsuleCollider>();
			_defaultColliderHeight = _collider.height;
			if (player == null)
				player = GetComponent<Player>();
			if (_follower == null)
				_follower = GetComponent<SplineFollower>();
			_actualSpeed           = defaultSpeed;
			_desiredSpeed          = defaultSpeed;
			_follower.followSpeed  = defaultSpeed;
			_follower.physicsMode  = SplineTracer.PhysicsMode.Transform;
			_follower.updateMethod = SplineUser.UpdateMethod.LateUpdate;
			levelHolder            = Locator.GetObject<LevelHolder>();
			_animator              = GetComponent<PlayerAnimator>();
			_collider              = GetComponent<CapsuleCollider>();
			_boosters              = new List<IBoostable>();
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
			RegisterControllable();
			_followCamera = Locator.GetObject<PlayerFollowCamera>();
			_followCamera.SetFollowType(PlayerFollowCamera.FollowType.StartTrack, true);
			if (GameProcess.isTutorial)
				TutorialController.OnTutorialFakeInput += OnSwipe;
		}

		private void Update()
		{
			if (_isPaused) return;
			if (isAccelerating) Accelerate();
			if (_isDamping) Damping();
			var p       = _follower.result.percent;
			var surface = levelHolder.intervals[currentRoadId].GetSurface(p);
			_animator.SetSurfaceAnimation(surface == TrackGenerator.TrackSurface.Normal ? 0 : 1);
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

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent<StartLine>(out var startLine))
			{
				_followCamera.SetFollowType(PlayerFollowCamera.FollowType.Normal);
			}
			else if (other.TryGetComponent<Finish>(out var finishLine))
			{
				_followCamera.SetFollowType(PlayerFollowCamera.FollowType.FinishTrack);
			}
			else if (other.TryGetComponent<DeathLoopStart>(out var deathloopstart))
			{
				_followCamera.SetFollowType(PlayerFollowCamera.FollowType.DeathLoop);
			}
			else if (other.TryGetComponent<DeathLoopEnd>(out var deathloopend))
			{
				_followCamera.SetFollowType(PlayerFollowCamera.FollowType.Normal);
			}
		}

		private void OnDrawGizmos()
		{
			if (_follower == null) return;
			var p = _follower.result;
			var i = currentRoadId - 1;
			if (i >= 0)
			{
				var pL = levelHolder._lines[i].EvaluatePosition(levelHolder._lines[i].Project(p.position));
				Gizmos.color = CheckLineSwap(levelHolder._lines[currentRoadId - 1], p, levelHolder._lineWidth) ? Color.green : Color.red;
				Gizmos.DrawLine(p.position, pL);
			}
			i = currentRoadId + 1;
			if (i < levelHolder._lines.Length)
			{
				var pL = levelHolder._lines[i].EvaluatePosition(levelHolder._lines[i].Project(p.position));
				Gizmos.color = CheckLineSwap(levelHolder._lines[i], p, levelHolder._lineWidth) ? Color.green : Color.red;
				Gizmos.DrawLine(p.position, pL);
			}
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
				_actualSpeed   = Mathf.Clamp(_actualSpeed, minSpeed, maxSpeed);
				isAccelerating = false;
				_isDamping     = false;
				_followCamera.SetSpeedType(PlayerFollowCamera.SpeedType.None);
			}
			ChangeSpeed();
		}
		private void Damping()
		{
			if (_isPaused) return;
			_actualSpeed -= accelerationSpeed * Time.deltaTime;
			if (_actualSpeed <= _desiredSpeed)
			{
				_actualSpeed = Mathf.Clamp(_actualSpeed, minSpeed, maxSpeed);
				_isDamping   = false;
				_followCamera.SetSpeedType(PlayerFollowCamera.SpeedType.None);
			}
			ChangeSpeed();
		}
		private void ChangeSpeed()
		{
			if (_isPaused)
			{
				_follower.followSpeed = 0;
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
			_follower.followSpeed = _actualSpeed;
			_animator.SetAnimatorSpeed(_actualSpeed/defaultSpeed);
		}


		private void OnSwipe(SwipeInput.SwipeType swipeType)
		{
			if (_isUnderControl == false) return;
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
				case SwipeInput.SwipeType.Left:
				case SwipeInput.SwipeType.Right:
					ChangePath(swipeType);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(swipeType), swipeType, null);
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
				_followCamera.SetFollowType(PlayerFollowCamera.FollowType.Slide);
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
			_followCamera.SetFollowType(PlayerFollowCamera.FollowType.Normal);
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
					_animator.SetSlideAnimation(false);
				}
				_jumpY           = 0;
				_collider.height = _defaultColliderHeight / 2;
				_animator.SetJumpAnimation(true);
				SetPlayerMovementType(MovementType.Jump);
				StartCoroutine(HandleJump());
				_followCamera.SetFollowType(PlayerFollowCamera.FollowType.Jump);
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
			_followCamera.SetFollowType(PlayerFollowCamera.FollowType.Normal);
		}


		public static bool CheckLineSwap(SplineComputer dest, SplineResult pos, float lineWidth)
		{
			var p1 = dest.Evaluate(dest.Project(pos.position)); // closest point on side line
			var d  = pos.position - p1.position;
			var v  = Vector3.Project(d, pos.right * lineWidth); // project vector RIGHT from current on vector between points on lines
			var v2 = Vector3.Dot(d.normalized, pos.normal);
			return (v.magnitude <= lineWidth * 1.1f) && (Mathf.Abs(v2) <= 0.01f);
		}
		private void ChangePath(SwipeInput.SwipeType swipeType)
		{
			if (_changeRoadCoroutine != null) return;
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

			var p0 = _follower.result; // point on current spline
			if (CheckLineSwap(levelHolder._lines[targetLine], p0, levelHolder._lineWidth))
			{
				_followCamera._isActive = false;
				var p1 = levelHolder._lines[targetLine].Evaluate(levelHolder._lines[targetLine].Project(p0.position)); // closest point on side line
				var d  = p0.position - p1.position;
				d.Normalize();
				currentRoadId      = targetLine;
				_follower.enabled  = false;
				_follower.computer = levelHolder._lines[currentRoadId];
				var d2 = new Vector2(Vector3.Dot(d, p1.right), Vector3.Dot(d, p1.normal)); // offset is [X * point.right + Y * point.normal]
				_follower.motion.offset = d2;
				_changeRoadCoroutine    = StartCoroutine(SwitchLine(d2));
			}
		}
		private IEnumerator SwitchLine(Vector3 baseOffset)
		{
			_follower.motion.offset = baseOffset;
			_follower.enabled       = true;
			var dir             = -Mathf.Sign(baseOffset.x);
			var changeRoadTimer = 0f;
			while (changeRoadTimer < changeRoadTime)
			{
				var t = changeRoadTimer / changeRoadTime;
				_follower.motion.offset = Vector3.Lerp(baseOffset, Vector3.zero, t * t);
				_animator.SetAnimatorRotationSpeed(dir * Mathf.PingPong(t, 0.5f) * 2f);
				yield return null;
				_followCamera._isActive =  true;
				changeRoadTimer         += Time.deltaTime;
			}
			_follower.motion.offset = Vector3.zero;
			_changeRoadCoroutine    = null;
		}


		private void SetupOffset()
		{
			if (_isPaused) return;
			_follower.motion.offset = new Vector2(_follower.motion.offset.x, _jumpY);
		}
		public  void SetPlayerMovementType(MovementType newMovementType) => player.SetMovementType(newMovementType);
		private void DeathLoopEnter()                                    => _inDeathLoop = true;
		private void DeathLoopExit()                                     => _inDeathLoop = false;

#region IPausable
		public void Pause()
		{
			Debug.Log($"[OpponentMover] ({gameObject.name}) paused");
			_follower.followSpeed = 0;
			_isPaused             = true;
		}
		public void Resume()
		{
			Debug.Log($"[OpponentMover] ({gameObject.name}) resumed");
			_follower.followSpeed = _actualSpeed;
			_isPaused             = false;
		}
		public void RegisterPausable() => PauseController.RegisterPausable(this);
  #endregion

#region IBarrierAffected
		public void BarrierHit(float value, float time)
		{
			if (_defended)
			{
				_boosters.ForEach(b => b.StopShield());
				_defended = false;
				return;
			}
			_boosters.ForEach(b => b.StopAllBoosters());
			_boosters.ForEach(b => b.BoostSpeed(time, value));

			//_actualSpeed  = 0;
			ChangeSpeed();
		}
  #endregion

#region IMover
		public void AddSpeed(float speed)
		{
			OnSpeedBoost?.Invoke();
			_desiredSpeed = defaultSpeed + speed;
			_followCamera.SetSpeedType(speed > 0 ? PlayerFollowCamera.SpeedType.Accel : PlayerFollowCamera.SpeedType.Deaccel);
			ChangeSpeed();
		}
		public void ReduceSpeed(float speed)
		{
			_desiredSpeed = defaultSpeed - speed;
			_followCamera.SetSpeedType(speed > 0 ? PlayerFollowCamera.SpeedType.Accel : PlayerFollowCamera.SpeedType.Deaccel);
			ChangeSpeed();
		}
		public void SetStartRoad(int roadId)
		{
			currentRoadId      = roadId;
			_follower.computer = levelHolder._lines[currentRoadId];
			SetupOffset();
		}
		public float GetPercent() => (float)_follower.clampedPercent;
  #endregion

#region IPlayerControllable
		public void StartPlayerControl()   => _isUnderControl = true;
		public void StopPlayerControl()    => _isUnderControl = false;
		public void RegisterControllable() => ControllManager.Instance.RegisterControllable(this);
  #endregion

#region IDefenable
		public void SetDefend(bool isDefended) => _defended = isDefended;
  #endregion
	}
}
