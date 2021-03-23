using System;
using System.Collections;
using Dreamteck.Splines;
using Game;
using UnityEngine;
namespace Players.Camera
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class PlayerFollowCamera : MonoBehaviour
	{
		public static UnityEngine.Camera followCamera;

		[SerializeField] private Transform      cameraPointT;
		[SerializeField] private Transform      cameSlideT;
		[SerializeField] private Transform      playerT;
		[SerializeField] private Transform      cameraStartT;
		[SerializeField] private Transform      deathLoopT;
		[SerializeField] private SplineComputer roadSpline;
		[SerializeField] private PlayerMover    mover;

		[Space]
		[SerializeField] private float distanceToPlayer;
		[SerializeField] private float defaultHeight;
		[SerializeField] private float smooth;
		[SerializeField] private float rotateSmooth;

		[Header("Raycast settings")]
		[SerializeField] private LayerMask barrierLayer;
		[SerializeField] private float raycastDistance;
		[SerializeField] private float fadeDistance;

		[Header("Slide Settings")]
		[SerializeField] private float slideSmooth;
		[SerializeField] private bool isSliding;
		[SerializeField] private bool isReturning;

		[Header("Speed boost settings")]
		[SerializeField] private float speedBoostedFOV;
		[SerializeField] private float speedBoostedSmooth;
		[SerializeField] private float speedReturningSmooth;

		[Header("Death loop settings")]
		[SerializeField] private float returningSmooth;
		[SerializeField] private bool isSpeedBoosted;
		[SerializeField] private bool isSpeedReturning;

		private                  bool               _isOnDeathLoop;
		private                  bool               _isStarting;
		private                  float              _actualHeight;
		private                  UnityEngine.Camera _cam;
		private                  Quaternion         _cameraStartRotation;
		private                  float              _currentFOV;
		private                  float              _deathLoopTimer;
		[SerializeField] private float              defaultFOV;
		private                  float              _defaultSpeed;
		private                  float              _desiredFOV;
		private                  bool               _isLoopReturning;
		private                  bool               _isWorking;
		private                  Transform          _transform;
		private const            float              SlideTime = .8f;

		private void Update()
		{
			if (_isWorking == false) return;
			_actualHeight = defaultHeight;
			MoveAndRotateCamera();
			if (isSpeedBoosted || isSpeedReturning)
			{
				_currentFOV = _cam.fieldOfView;
				ChangeFOV();
			}
		}

		private void FixedUpdate()
		{
			if (_isWorking == false) return;
			Raycast();
		}

		private void OnDisable()
		{
			PlayerMover.OnSlide             -= OnSlide;
			PlayerMover.OnSpeedBoost        -= BoostSpeed;
			PlayerMover.OnSpeedBoostStopped -= SpeedBoostStopped;

			//DeathLoop.OnEnterDeathLoop      -= StartDeathLoop;
			//DeathLoop.OnExitDeathLoop       -= ExitDeathLoop;
		}

		private void OnDestroy()
		{
			StartLine.OnCrossStartLine -= OnPlayerCrossLine;
			PlayerMover.OnSlideBreak   -= StopSlideAction;
		}

		private void Intitalize()
		{
			if (playerT == null)
				Debug.LogError("plyaer T in CameraFollow script is null");
			_cam                            =  UnityEngine.Camera.main;
			followCamera                    =  _cam;
			PlayerMover.OnSlide             += OnSlide;
			PlayerMover.OnSpeedBoost        += BoostSpeed;
			PlayerMover.OnSpeedBoostStopped += SpeedBoostStopped;

			//DeathLoop.OnEnterDeathLoop      += StartDeathLoop;
			//DeathLoop.OnExitDeathLoop       += ExitDeathLoop;
			PlayerMover.OnSlideBreak   += StopSlideAction;
			_transform                 =  transform;
			_defaultSpeed              =  mover.GetDefaultSpeed();
			_actualHeight              =  defaultHeight;
			defaultFOV                 =  _cam.fieldOfView;
			_isWorking                 =  true;
			_isStarting                =  true;
			StartLine.OnCrossStartLine += OnPlayerCrossLine;
			_transform.parent          =  cameraPointT;
		}

		private void OnPlayerCrossLine() => StartCoroutine(MoveToPlayer(1));

		private IEnumerator MoveToPlayer(float moveTime)
		{
			float timer = 0;
			_isWorking  = false;
			_isStarting = false;
			while (timer <= moveTime)
			{
				timer               += Time.deltaTime;
				_transform.position =  Vector3.Lerp(cameraStartT.position, cameraPointT.position + cameraPointT.up * _actualHeight, timer                                                                                   / moveTime);
				_transform.rotation =  Quaternion.Lerp(_transform.rotation, Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(playerT.position - _transform.position, cameraPointT.up), timer / moveTime), timer / moveTime);
				yield return null;
			}
			var   startRotation = _transform.rotation;
			float rotateTimer   = 0;
			var   rotateTime    = 1.2f;
			while (rotateTimer <= rotateTime)
			{
				rotateTimer         += Time.deltaTime;
				_transform.position =  cameraPointT.position + cameraPointT.up * _actualHeight;
				_transform.rotation =  Quaternion.Lerp(_transform.rotation, cameraPointT.rotation, rotateTimer / rotateTime);
				yield return null;
			}
			_isStarting = false;
			_isWorking  = true;
		}

		private void ChangeFOV()
		{
			if (_isOnDeathLoop) return;
			if (_currentFOV != _desiredFOV)
			{
				var smooth = isSpeedBoosted ? speedBoostedSmooth : speedReturningSmooth;
				_currentFOV      = Mathf.Lerp(_currentFOV, _desiredFOV, smooth * Time.deltaTime + Time.deltaTime);
				_cam.fieldOfView = _currentFOV;
			}
			if (Mathf.Abs(_currentFOV - _desiredFOV) < .1f)
			{
				if (isSpeedBoosted)
				{
					isSpeedBoosted = false;
					SpeedBoostStopped(defaultFOV, true);
				}
				else if (isSpeedReturning)
				{
					isSpeedReturning = false;
					_cam.fieldOfView = _desiredFOV;
				}
			}
		}

		private void BoostSpeed()
		{
			isSpeedBoosted   = true;
			isSpeedReturning = false;
			_desiredFOV      = speedBoostedFOV;
		}
		private void SpeedBoostStopped(float desiredSpeed, bool justStop)
		{
			if (desiredSpeed > _defaultSpeed && !justStop) return;
			isSpeedBoosted   = false;
			isSpeedReturning = true;
			_desiredFOV      = defaultFOV;
		}


		private void MoveAndRotateCamera()
		{
			_transform.rotation = Quaternion.Lerp(_transform.rotation, _isStarting ? cameraStartT.rotation : cameraPointT.rotation, rotateSmooth * Time.deltaTime);
			Vector3 position;
			if (isSliding)
			{
				position = Vector3.Lerp(_transform.position, cameSlideT.position, slideSmooth * Time.deltaTime);
			}
			else if (isReturning)
			{
				position   = cameraPointT.position;
				position.y = Mathf.Lerp(_transform.position.y, (cameraPointT.position + cameraPointT.up * _actualHeight).y, slideSmooth * Time.deltaTime);
				if ((_transform.position - (cameraPointT.position + cameraPointT.up * _actualHeight)).magnitude < .15f)
					isReturning = false;
			}
			/*else if (_isLoopReturning)
			{
				_deathLoopTimer     += Time.deltaTime / 3;
				position            =  Vector3.Lerp(_transform.position, Vector3.Lerp(_transform.position, cameraPointT.position, _deathLoopTimer), _deathLoopTimer);
				_transform.rotation =  Quaternion.LookRotation(playerT.position + cameraPointT.up * (_actualHeight + 0.35f) - _transform.position, cameraPointT.up);
				if (_deathLoopTimer >= 9f)
				{
					_transform.rotation = Quaternion.Lerp(_transform.rotation, cameraPointT.rotation, rotateSmooth);
					if (_deathLoopTimer > 1.1f)
						_isLoopReturning = false;
				}
			} /**/
			else if (_isStarting)
			{
				position =  cameraStartT.position;
				position += cameraStartT.up * _actualHeight;
			}
			/* else if (_isOnDeathLoop)
			{
				_deathLoopTimer += Time.deltaTime / 11;
				position        =  Vector3.Lerp(_transform.position, deathLoopT.position, _deathLoopTimer);
				if (_deathLoopTimer < 0.35)
					_transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(playerT.position + cameraPointT.up * (_actualHeight + 0.25f) - _transform.position, Vector3.up), rotateSmooth);
				else
					_transform.rotation = Quaternion.LookRotation(playerT.position + cameraPointT.up * (_actualHeight + 0.25f) - _transform.position, Vector3.up);
			} /**/
			else
			{
				// normal movement 
				position =  cameraPointT.position;
				position += cameraPointT.up * _actualHeight;
			}
			_transform.position = Vector3.Lerp(_transform.position, position, rotateSmooth * Time.deltaTime);
		}

		/*
		private SplineResult GetSplineResult(float percent)
		{
			return roadSpline.Evaluate(percent);
		}
		private float GetPercent(Vector3 playerPosition)
		{
			return (float)roadSpline.Project(playerPosition);
		}
		private float DistanceToPercent(float distance)
		{
			var length = roadSpline.CalculateLength();
			return distance / length;
		}
		/**/

		private void Raycast()
		{
			var camForward = _transform.forward;
			camForward.y = 0;
			if (Physics.Raycast(_transform.position, camForward, out var hit, raycastDistance))
			{
				var barrier = hit.transform.GetComponent<Barrier>();
				if (barrier != null)
				{
					var distance = (_transform.position - hit.point).magnitude;
					barrier.SetTransparent(distance / fadeDistance);
				}
			}
		}

		private void OnSlide()
		{
			isSliding   = true;
			isReturning = false;
			StartCoroutine(WaitAndDoAction(SlideTime, StopSlideAction));
		}

		private IEnumerator WaitAndDoAction(float time, Action action)
		{
			yield return new WaitForSeconds(time);
			action();
		}

		private void StopSlideAction()
		{
			isSliding   = false;
			isReturning = true;
		}

		/*
		private void StartDeathLoop()
		{
			_isOnDeathLoop   = true;
			_isLoopReturning = false;
			_deathLoopTimer  = 0;
			ControllManager.RemoveControl();
		}
		private void ExitDeathLoop()
		{
			_isOnDeathLoop   = false;
			_isLoopReturning = true;
			_deathLoopTimer  = 0;
			ControllManager.GiveControl();
		}
		/**/
		public void InitializePlayerCamera(PlayerCameraData data)
		{
			playerT      = data.playerT;
			cameraPointT = data.cameraPositionT;
			cameSlideT   = data.slidePositionT;
			mover        = data.mover;
			cameraStartT = data.cameraStartT;
			deathLoopT   = data.deathLoopPositionT;
			Intitalize();
		}
	}
}
