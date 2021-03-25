using System;
using System.Collections;
using Internal;
using Level;
using UnityEngine;
namespace Players.Camera
{
	[RequireComponent(typeof(UnityEngine.Camera))]
	public class PlayerFollowCamera : MonoBehaviour, IRegistrable
	{
		public enum FollowType
		{
			None        = 0,
			Normal      = 1,
			Jump        = 2,
			Slide       = 3,
			DeathLoop   = 4,
			StartTrack  = 5,
			FinishTrack = 6,
		}
		public enum SpeedType
		{
			None,
			Accel,
			Deaccel
		}

		[Space]
		[SerializeField] private float defaultHeight;
		[SerializeField] private float rotateSmooth;

		[Header("Raycast settings")]
		[SerializeField] private LayerMask barrierLayer;
		[SerializeField] private float raycastDistance;
		[SerializeField] private float fadeDistance;

		[Header("---")]
		[SerializeField] private Vector3 _offsetStart;
		[SerializeField]                             private Vector3   _offsetNormal;
		[SerializeField]                             private Vector3   _offsetSlide;
		[SerializeField]                             private Vector3   _offsetJump;
		[SerializeField]                             private Vector3   _lookOffsetStart;
		[SerializeField]                             private Vector3   _lookOffsetNormal;
		[SerializeField]                             private Vector3   _lookOffsetSlide;
		[SerializeField]                             private Vector3   _lookOffsetJump;
		[SerializeField]                             private Transform _target;
		[Tooltip("Higher - faster")][SerializeField] private float     _smoothOffset = 1f;
		[Tooltip("Higher - faster")][SerializeField] private float     _smoothRotate = 1f;

		[Header("FOV settings")]
		[SerializeField] private float _accelFov = 75f;
		[SerializeField]                             private float _normalFov       = 60f;
		[SerializeField]                             private float _slowFov         = 40f;
		[Tooltip("Higher - faster")][SerializeField] private float _accelFovSpeed   = 25f;
		[Tooltip("Higher - faster")][SerializeField] private float _deaccelFovSpeed = 10f;

		[Header("---")]
		[SerializeField] private FollowType _followType;
		[SerializeField] private SpeedType _speedType;

		private                  UnityEngine.Camera _camera;
		private                  float              _currentFOV;
		private                  float              _desiredFOV;
		private                  bool               _isActive;
		private                  Transform          _transform;
		[SerializeField] private bool               _instant = false;

		private void Awake()
		{
			Register();

			_camera     = UnityEngine.Camera.main;
			_currentFOV = _normalFov;
			_transform  = transform;
		}
		private void OnEnable()  => Register();
		private void OnDisable() => Unregister();

		public void UpdateCamera()
		{
			if (!_isActive || _target == null) return;
			MoveAndRotateCamera();
			ChangeFOV();
		}
		private void LateUpdate() => UpdateCamera();

		private void FixedUpdate() => CheckBarrierTransparency();

		private void OnDestroy() => SetFollowType(FollowType.None, true);

		public void InitializePlayerCamera(PlayerCameraData data, Transform target)
		{
			_target   = target;
			_isActive = true;
		}

		public void SetFollowType(FollowType type, bool instant = false)
		{
			_followType = type;
		}
		public void SetSpeedType(SpeedType type)
		{
			if (type == SpeedType.Accel)
			{
				if (_speedType      == SpeedType.Deaccel) _speedType = SpeedType.None;
				else if (_speedType == SpeedType.None) _speedType    = SpeedType.Accel;
			}
			if (type == SpeedType.Deaccel)
			{
				if (_speedType      == SpeedType.Accel) _speedType = SpeedType.None;
				else if (_speedType == SpeedType.None) _speedType  = SpeedType.Deaccel;
			}
			switch (_speedType)
			{
				case SpeedType.None:
					_desiredFOV = _normalFov;
					break;
				case SpeedType.Accel:
					_desiredFOV = _accelFov;
					break;
				case SpeedType.Deaccel:
					_desiredFOV = _normalFov;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		private void MoveAndRotateCamera()
		{
			var offset     = Vector3.zero;
			var lookOffset = Vector3.zero;
			var time       = _instant ? 10000f : Time.deltaTime;
			switch (_followType)
			{
				case FollowType.None:
					break;
				case FollowType.Normal:
					offset     = _offsetNormal;
					lookOffset = _lookOffsetNormal;
					break;
				case FollowType.Jump:
					offset     = _offsetJump;
					lookOffset = _lookOffsetJump;
					break;
				case FollowType.Slide:
					offset     = _offsetSlide;
					lookOffset = _lookOffsetSlide;
					break;
				case FollowType.DeathLoop:
					break;
				case FollowType.StartTrack:
					offset     = _offsetStart;
					lookOffset = _lookOffsetStart;
					break;
				case FollowType.FinishTrack:
					_target = null;
					return;
				default:
					throw new ArgumentOutOfRangeException(nameof(_followType), _followType, null);
			}

			var targetPos = _target.position;
			var currPos   = _transform.position;
			var targetRot = _target.rotation;

			var tp = targetPos + targetRot * offset;
			var d  = Mathf.Clamp(Vector3.Distance(tp, currPos) /10f, 1f, 1000f);
			_transform.position = Vector3.Lerp(currPos, tp, _smoothOffset * time * d);

			var q = Quaternion.LookRotation(targetPos + targetRot * lookOffset - currPos, _target.up);
			_transform.rotation = Quaternion.Slerp(_transform.rotation, q, _smoothRotate * time);

		}

		private void ChangeFOV()
		{
			var smooth = 0f;
			switch (_speedType)
			{
				case SpeedType.None:
					_desiredFOV = _normalFov;
					smooth      = _accelFovSpeed;
					break;
				case SpeedType.Accel:
					_desiredFOV = _accelFov;
					smooth      = _accelFovSpeed;
					break;
				case SpeedType.Deaccel:
					_desiredFOV = _slowFov;
					smooth      = _deaccelFovSpeed;
					break;
			}
			_currentFOV         = Mathf.MoveTowards(_currentFOV, _desiredFOV, smooth * Time.deltaTime);
			_camera.fieldOfView = _currentFOV;
		}

		/*
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
			} 
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
			} 
			else
			{
				// normal movement 
				position =  cameraPointT.position;
				position += cameraPointT.up * _actualHeight;
			}
			_transform.position = Vector3.Lerp(_transform.position, position, rotateSmooth * Time.deltaTime);
		} /**/

		private void CheckBarrierTransparency()
		{
			var camForward = _transform.forward;
			camForward.y = 0;
			if (Physics.Raycast(_transform.position, camForward, out var hit, raycastDistance, barrierLayer))
			{
				if (hit.transform.TryGetComponent<Barrier>(out var barrier))
				{
					var distance = (_transform.position - hit.point).magnitude;
					barrier.SetTransparent(distance / fadeDistance);
				}
			}
		}


		public void Register()   => Locator.Register(typeof(PlayerFollowCamera), this);
		public void Unregister() => Locator.Unregister(typeof(PlayerFollowCamera));
	}
}
