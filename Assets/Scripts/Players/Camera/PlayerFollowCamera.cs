using System;
using System.Collections;
using Internal;
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
		[SerializeField] private Vector3 _offsetNormal;
		[SerializeField] private Vector3   _offsetSlide;
		[SerializeField] private Vector3   _offsetJump;
		[SerializeField] private Transform _target;
		[SerializeField] private float     _smoothOffset = 1f;
		[SerializeField] private float     _smoothRotate = 1f;

		[Header("FOV settings")]
		[SerializeField] private float _accelFov = 75f;
		[SerializeField] private float _normalFov       = 60f;
		[SerializeField] private float _slowFov         = 40f;
		[SerializeField] private float _accelFovSpeed   = 25f;
		[SerializeField] private float _deaccelFovSpeed = 10f;

		[Header("---")]
		[SerializeField] private FollowType _followType;
		[SerializeField] private SpeedType _speedType;

		private UnityEngine.Camera _camera;
		private float              _currentFOV;
		private float              _desiredFOV;
		private bool               _isActive;
		private Transform          _transform;

		private void Awake()
		{
			Register();

			_camera    = UnityEngine.Camera.main;
			_transform = transform;
		}
		private void OnEnable()  => Register();
		private void OnDisable() => Unregister();

		private void LateUpdate()
		{
			if (!_isActive || _target == null) return;
			MoveAndRotateCamera();
			ChangeFOV();
		}

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
				else if (_speedType == SpeedType.None) _speedType  = SpeedType.Accel;
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
			var        offset = Vector3.zero;
			Quaternion rotate;
			switch (_followType)
			{
				case FollowType.None:
					break;
				case FollowType.Normal:
					offset = _offsetNormal;
					break;
				case FollowType.Jump:
					offset = _offsetJump;
					break;
				case FollowType.Slide:
					offset = _offsetSlide;
					break;
				case FollowType.DeathLoop:
					break;
				case FollowType.StartTrack:
					break;
				case FollowType.FinishTrack:
					_target = null;
					return;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(_followType), _followType, null);
			}
			_transform.rotation = Quaternion.Slerp(_transform.rotation, _target.rotation, _smoothRotate * Time.deltaTime);

			_transform.position = Vector3.Lerp(_transform.position, _target.position + _transform.rotation * offset, _smoothOffset * Time.deltaTime);

			//_transform.position = Vector3.Lerp(_transform.position, _target.position + _target.rotation * offset, _smoothOffset * Time.deltaTime);


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
