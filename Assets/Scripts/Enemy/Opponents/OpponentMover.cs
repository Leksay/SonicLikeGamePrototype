using System.Collections;
using System.Collections.Generic;
using Data.DataScripts;
using Dreamteck.Splines;
using Helpers;
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
		[SerializeField] private OpponentsData data;
		/*
		public float defaultSpeed;
		public float changeRoadTime;
		public float changeRoadTreshold;
		public float accelerationSpeed;
		public float minSpeed;
		public float maxSpeed;
		/**/

		[Header("Jump")]
		private float _jumpY;
		private bool _isJump;

		[Header("Slide")]
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

		void Awake()
		{
			_follower              = GetComponent<SplineFollower>();
			_follower.autoFollow   = true;
			_follower.physicsMode  = SplineTracer.PhysicsMode.Transform;
			_follower.updateMethod = SplineUser.UpdateMethod.LateUpdate;
			_animator              = GetComponent<PlayerAnimator>();
			_collider              = GetComponent<CapsuleCollider>();
			_opponent              = GetComponent<OpponentMovement>();
			_defaultColliderHeight = _collider.height;
			_boosters              = new List<IBoostable>();
			_boosters.AddRange(GetComponents<IBoostable>());
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
			var p       = _follower.result.percent;
			var surface = levelHolder.intervals[currentRoadId].GetSurface(p);
			_animator.SetSurfaceAnimation(surface == TrackGenerator.TrackSurface.Normal ? 0 : 1);
			ChangeSpeed();
		}


		private void Accelerate()
		{
			if (_isPaused) return;
		}
		private void Damping()
		{
			if (_isPaused) return;
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
				_isAccelerating = true;
			}
			else if (_actualSpeed > _desiredSpeed)
			{
				_isDamping = true;
			}
			_actualSpeed          = Mathf.Clamp(Mathf.MoveTowards(_actualSpeed, _desiredSpeed, data.accelerationSpeed * Time.deltaTime), data.minSpeed, data.maxSpeed);
			_follower.followSpeed = _actualSpeed;
			_animator.SetAnimatorSpeed(_actualSpeed / data.defaultSpeed);
		}


		public void Initialize(OpponentsData opponentData, LevelData levelData)
		{
			if (_initialized) return;
			data              = opponentData.Clone();
			levelHolder       = levelData.levelHolder;
			data.defaultSpeed = opponentData.defaultSpeed + Random.Range(-1, 1) + Time.realtimeSinceStartup % 3 * Random.Range(-1, 1);

			_actualSpeed          = data.defaultSpeed - 2f;
			_desiredSpeed         = data.defaultSpeed;
			_follower.computer    = levelHolder._lines[currentRoadId];
			_follower.followSpeed = data.defaultSpeed;
			SetupOffset();
			ChangeSpeed();
			_initialized = true;
		}


		public void AddSpeed(float speed)
		{
			_desiredSpeed = Mathf.Clamp(_desiredSpeed + speed, data.defaultSpeed, data.maxSpeed);
			ChangeSpeed();
		}
		public void ReduceSpeed(float speed)
		{
			_desiredSpeed = Mathf.Clamp(_desiredSpeed - speed, data.defaultSpeed, data.maxSpeed);
			ChangeSpeed();
		}


		public void BarrierHit(float value, float time)
		{
			if (_defended)
			{
				_boosters.ForEach(b => b.StopShield());
				return;
			}
			_boosters.ForEach(b => b.StopAllBoosters());
			_boosters[0]?.BoostSpeed(time, value);
			ChangeSpeed();
		}


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


		private void SetupOffset()
		{
			if (_isPaused) return;
			_follower.motion.offset = new Vector2(_follower.motion.offset.x, _jumpY);
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
			var timer = 0f;
			while (timer < data.slideTime)
			{
				_jumpY =  Mathf.Lerp(0, -slideOffset, (timer) / data.slideTime);
				timer  += Time.deltaTime;
				SetupOffset();
				yield return null;
			}
			timer = 0;
			while (timer < data.slideTime / 3)
			{
				_jumpY =  Mathf.Lerp(-slideOffset, 0, (timer * 3) / data.slideTime);
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
			float upJumpTimer = 0;
			while (upJumpTimer < data.upJumpTime)
			{
				SetupOffset();
				_jumpY      =  Mathf.Lerp(0, data.jumpHeigh, upJumpTimer / data.upJumpTime);
				upJumpTimer += Time.deltaTime;
				yield return null;
			}

			float inAirTimer = 0;
			while (inAirTimer < data.inAirTime)
			{
				inAirTimer += Time.deltaTime;
				yield return null;
			}
			_animator.SetJumpAnimation(false);

			float downJumpTimer = 0;
			while (downJumpTimer < data.downJumpTime)
			{
				SetupOffset();
				_jumpY        =  Mathf.Lerp(data.jumpHeigh, 0, downJumpTimer / data.downJumpTime);
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
			var dir             = -Mathf.Sign(baseOffset.x);
			var changeRoadTimer = 0f;
			while (changeRoadTimer < data.changeRoadTime)
			{
				var t = changeRoadTimer / data.changeRoadTime;
				_follower.motion.offset = Vector3.Lerp(baseOffset, Vector3.zero, t * t);
				_animator.SetAnimatorRotationSpeed(dir * Mathf.PingPong(t, 0.5f) * 2f);
				yield return null;
				changeRoadTimer += Time.deltaTime;
			}
			_follower.motion.offset = Vector3.zero;
			_changeRoadCoroutine    = null;
		}


		private void        SetMovementType(MovementType    newMovementType) => _opponent.SetMovementType(newMovementType);
		void IOpponentMover.ChangePath(SwipeInput.SwipeType swipeType)       => ChangePath2(swipeType);

		public void DoJump()  => StartJump();
		public void DoSlide() => StartSlide();
		public void SetRoad(int roadId)
		{
			Initialize(DataHolder.GetOpponentData(), DataHolder.GetLevelData());
			currentRoadId      = roadId;
			_follower.computer = levelHolder._lines[currentRoadId];
			SetupOffset();
		}
		public void  SetStartRoad(int roadId)   => SetRoad(roadId);
		public float GetPercent()               => (float)_follower.clampedPercent;
		public void  RegisterPausable()         => PauseController.RegisterPausable(this);
		public void  SetDefend(bool isDefended) => _defended = isDefended;
	}
}
