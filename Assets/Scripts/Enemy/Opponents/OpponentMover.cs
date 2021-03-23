using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using Level;
using Players;
using UnityEngine;
namespace Enemy.Opponents
{
	[RequireComponent(typeof(SplineFollower))]
	[RequireComponent(typeof(PlayerAnimator))]
	[RequireComponent(typeof(IBoostable))]
	[RequireComponent(typeof(OpponentMovement))]
	public class OpponentMover : MonoBehaviour, IMover, IBarrierAffected, IPausable, IOpponentMover, IDefendable
	{
    #region Debug
		[SerializeField] private bool changeRight;
		[SerializeField] private bool changeLeft;
    #endregion

		private OpponentMovement _opponent;

		[Header("Move")]
		public float defaultSpeed;
		public float changeRoadTime;
		public float changeRoadTreshold;
		public float accelerationSpeed;
		public float maxSpeed;

		[Header("Jump")]
		[SerializeField] private float jumpHeigh;
		[SerializeField] private float upJumpTime;
		[SerializeField] private float downJumpTime;
		[SerializeField] private float inAirTime;
		private                  float _jumpY;
		private                  bool  _isJump;

		[Header("Slide")]
		[SerializeField] private float slideTime;
		private float _slideOffset = 0.5f;
		private int   _currenOffsetId;
		private bool  _isSliding;
		private float slideOffset = 0.5f;

		[Header("Level")]
		[SerializeField] private LevelHolder levelHolder;
		[SerializeField] private int currentRoadId;

		private SplineFollower   _follower;
		private bool             _isPaused;
		private PlayerAnimator   _animator;
		private bool             _isAccelerating;
		private bool             _isDamping;
		private List<IBoostable> _boosters;

		private float _desiredSpeed;
		private float _actualSpeed;

		private new CapsuleCollider _collider;
		private     float           _defaultColliderHeight;
		public      int             CurrentRoadID => _currenOffsetId;

		private float     _nextRoadOffset;
		private float     _changeRoadTimer;
		private Coroutine _changeRoadCoroutine;
		private bool      _initialized;
		private bool      _defended;

		void Start()
		{
			if (levelHolder == null)
				levelHolder = FindObjectOfType<LevelHolder>();
			_follower              = GetComponent<SplineFollower>();
			_animator              = GetComponent<PlayerAnimator>();
			_collider              = GetComponent<CapsuleCollider>();
			_opponent              = GetComponent<OpponentMovement>();
			_follower.computer     = levelHolder._lines[currentRoadId];
			_defaultColliderHeight = _collider.height;
			_boosters              = new List<IBoostable>();
			_boosters.AddRange(GetComponents<IBoostable>());
			Initialize(DataHolder.GetOpponentData(), DataHolder.GetLevelData());
			RegisterPausable();
		}

		void Update()
		{
			if (_isPaused) return;
			if (_isAccelerating) Accelerate();
			if (_isDamping) Damping();
			if (_changeRoadCoroutine == null)
			{
				if (changeLeft)
				{
					ChangePath2(SwipeInput.SwipeType.Left);
					changeLeft = false;
				}
				if (changeRight)
				{
					ChangePath2(SwipeInput.SwipeType.Right);
					changeRight = false;
				}
			}

		}

		private void Accelerate()
		{
			if (_isPaused) return;
			_actualSpeed += accelerationSpeed * Time.deltaTime;
			if (_actualSpeed >= _desiredSpeed)
			{
				_actualSpeed    = _desiredSpeed;
				_isAccelerating = false;
				_isDamping      = false;
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

		public void Initialize(OpponentsData opponentData, LevelData levelData)
		{
			if (_initialized) return;

			levelHolder = levelData.levelHolder;
			_follower   = GetComponent<SplineFollower>();
			_animator   = GetComponent<PlayerAnimator>();

			defaultSpeed       = opponentData.defaultSpeed + Random.Range(-1, 1) + Time.realtimeSinceStartup % 3 * Random.Range(-1, 1);
			changeRoadTime     = opponentData.changeRoadTime;
			changeRoadTreshold = opponentData.changeRoadTreshold;
			accelerationSpeed  = opponentData.accelerationSpeed;
			maxSpeed           = opponentData.maxSpeed;

			jumpHeigh    = opponentData.jumpHeigh;
			upJumpTime   = opponentData.upJumpTime;
			downJumpTime = opponentData.downJumpTime;
			inAirTime    = opponentData.inAirTime;

			slideTime = opponentData.slideTime;

			_actualSpeed  = 2;
			_desiredSpeed = defaultSpeed;

			_follower.autoFollow = true;
			SetupOffset();
			ChangeSpeed();
			_initialized = true;
		}

		public void AddSpeed(float speed)
		{
			_desiredSpeed = Mathf.Clamp(_desiredSpeed + speed, defaultSpeed, maxSpeed);
			ChangeSpeed();
		}

		public void BarrierHit()
		{
			if (_defended)
			{
				_boosters.ForEach(b => b.StopShild());
				return;
			}
			_desiredSpeed = defaultSpeed;
			_actualSpeed  = 0;
			_boosters.ForEach(b => b.StopAllBoosters());
			ChangeSpeed();
		}

		public void Pause()
		{
			_follower.followSpeed = 0;
			_isPaused             = true;
		}

		public void ReduceSpeed(float speed)
		{
			_desiredSpeed = Mathf.Clamp(_desiredSpeed - speed, defaultSpeed, maxSpeed);
			ChangeSpeed();
		}

		public void Resume()
		{
			_follower.followSpeed = _actualSpeed;
			_isPaused             = false;
		}

		private void SetupOffset()
		{
			if (_isPaused) return;
			_follower.motion.offset = new Vector2(_follower.motion.offset.x, _jumpY);
		}

		private void ChangeSpeed()
		{
			if (_isPaused) return;
			if (_actualSpeed < _desiredSpeed)
			{
				_isAccelerating = true;
			}
			else if (_actualSpeed > _desiredSpeed)
			{
				_isDamping = true;
			}
			_follower.followSpeed = _actualSpeed;
			_animator.SetAnimatorSpeed(_actualSpeed);
		}

		private void StartSlide()
		{
			if (_isPaused) return;
			if (!_isJump && !_isSliding)
			{
				_animator.SetSlideAnimation(true);
				_isSliding          = true;
				_collider.direction = 2;
				SetMovementType(MovementType.Slide);
				StartCoroutine(Slide());
			}
		}
		private IEnumerator Slide()
		{
			while (_isPaused)
				yield return null;
			Debug.Log($"[OpponentMover] [{this.gameObject.name}] Slide");
			float timer = 0;
			while (timer < slideTime)
			{
				_jumpY =  Mathf.Lerp(0, -slideOffset, (timer) / slideTime);
				timer  += Time.deltaTime;
				SetupOffset();
				yield return null;
			}
			timer = 0;
			while (timer < slideTime / 3)
			{
				_jumpY =  Mathf.Lerp(-slideOffset, 0, (timer * 3) / slideTime);
				timer  += Time.deltaTime;
				SetupOffset();
				yield return null;
			}
			StopSlide();
			yield return new WaitForSeconds(Time.deltaTime);
		}
		private void StopSlide()
		{
			_isSliding          = false;
			_jumpY              = 0;
			_collider.direction = 1;
			SetupOffset();
			SetMovementType(MovementType.Run);
			_animator.SetSlideAnimation(false);
		}

		private void StartJump()
		{
			if (_isPaused) return;
			if (!_isJump && !_isSliding)
			{
				_isJump          = true;
				_jumpY           = 0;
				_collider.height = _defaultColliderHeight / 2;
				_animator.SetJumpAnimation(true);
				SetMovementType(MovementType.Jump);
				StartCoroutine(HandleJump());
			}
		}
		private IEnumerator HandleJump()
		{
			Debug.Log($"[OpponentMover] [{this.gameObject.name}] Jump");
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
			SetMovementType(MovementType.Run);
			_isJump = false;
		}


		private void ChangePath2(SwipeInput.SwipeType swipeType)
		{
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
			var p0 = _follower.result;                                                                             // point on current spline
			var p1 = levelHolder._lines[targetLine].Evaluate(levelHolder._lines[targetLine].Project(p0.position)); // closest point on side line
			var d  = p0.position - p1.position;
			if (!PlayerMover.CheckLineSwap(levelHolder._lines[targetLine], p0, levelHolder._lineWidth)) return;
			currentRoadId = targetLine;
			d.Normalize();
			var d2 = new Vector2(Vector3.Dot(d, p1.right), Vector3.Dot(d, p1.normal)); // offset is [X * point.right + Y * point.normal]
			_follower.computer   = levelHolder._lines[currentRoadId];
			_changeRoadCoroutine = StartCoroutine(SwitchLine(d2));
		}
		private IEnumerator SwitchLine(Vector3 baseOffset)
		{
			var changeRoadTimer = 0f;
			while (changeRoadTimer < changeRoadTime)
			{
				_follower.motion.offset = Vector3.Lerp(baseOffset, Vector3.zero, changeRoadTimer / changeRoadTime);
				yield return null;
				changeRoadTimer += Time.deltaTime;
			}
			_follower.motion.offset = Vector3.zero;
			_changeRoadCoroutine    = null;
		}

		/*
	private bool ChangePath(SwipeInput.SwipeType swipeType)
	{
		if (_isPaused) return false;
		if (levelHolder.TryChangePathId(ref _currenOffsetId, swipeType))
		{
			_nextRoadOffset      = levelHolder.GetOffsetById(_currenOffsetId);
			_changeRoadTimer     = 0;
			_startOffset         = currentOffset;
			_changeRoadCoroutine = StartCoroutine(MoveNextRoad());
			_animator.SetRotation(swipeType);
			return true;
		}
		return false;
	}
	private IEnumerator MoveNextRoad()
	{
		if (_isPaused) yield return null;
		while (Mathf.Abs(_nextRoadOffset - currentOffset) > changeRoadTreshold)
		{
			currentOffset    =  Mathf.Lerp(_startOffset, _nextRoadOffset, _changeRoadTimer / changeRoadTime);
			_changeRoadTimer += Time.deltaTime;
			SetupOffset();
			yield return null;
		}
		currentOffset = _nextRoadOffset;
		SetupOffset();
		if (_changeRoadCoroutine != null)
			StopCoroutine(_changeRoadCoroutine);
		_changeRoadCoroutine = null;
		yield return new WaitForSeconds(Time.deltaTime);
	}
	/**/

		private void SetMovementType(MovementType newMovementType) => _opponent.SetMovementType(newMovementType);

		void IOpponentMover.ChangePath(SwipeInput.SwipeType swipeType) => ChangePath2(swipeType);

		public void DoJump() => StartJump();

		public void DoSlide() => StartSlide();

		public void SetRoad(int roadId)
		{
			Initialize(DataHolder.GetOpponentData(), DataHolder.GetLevelData());
			currentRoadId      = roadId;
			_follower.computer = levelHolder._lines[currentRoadId];
			SetupOffset();
		}

		public void SetStartRoad(int roadId) => SetRoad(roadId);

		public float GetPercent() => (float)_follower.clampedPercent;

		public void RegisterPausable()         => PauseController.RegisterPausable(this);
		public void SetDefend(bool isDefended) => _defended = isDefended;
	}
}
