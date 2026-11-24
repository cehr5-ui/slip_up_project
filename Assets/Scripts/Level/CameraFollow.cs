using Unity.Cinemachine;
using UnityEngine;

namespace SlipUp
{
    [DefaultExecutionOrder(-10)]
    public class CameraFollow : MonoBehaviour
    {
        private const float DirectionEpsilonSqr = 0.0001f;
        private const float EmbeddedHitDistance = 0.001f;

        [SerializeField] private float _distance = 8.75f;
        [SerializeField] private float _pitch = 32f;
        [SerializeField] private float _lookHeight = 1.1f;
        [SerializeField] private float _yawSpeed = 2.5f;
        [SerializeField] private float _pitchSpeed = 2f;
        [SerializeField] private float _stickYawSpeed = 120f;
        [SerializeField] private float _stickPitchSpeed = 80f;
        [SerializeField] private float _minPitch = -10f;
        [SerializeField] private float _maxPitch = 70f;
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private float _collisionPadding = 0.15f;
        [SerializeField] private float _minDistance = 1.15f;
        [SerializeField] private LayerMask _collisionMask = ~0;

        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private GameplayInput _input;
        private Transform _target;
        private Transform _pivot;
        private CinemachineCamera _vcam;
        private CinemachineThirdPersonFollow _follow;
        private float _yaw;
        private float _currentPitch;
        private bool _hasTarget;
        private bool _hasPose;
        private bool _isLookEnabled;
        private bool _isLockedToPlayer;

        internal void Initialize(GameplayInput input)
        {
            _input = input;
        }

        internal void SetTarget(Transform target)
        {
            _target = target;
            _hasTarget = true;
            _hasPose = false;
            BindVirtualCamera();
        }

        internal void ClearTarget()
        {
            _target = null;
            _hasTarget = false;
            _isLookEnabled = false;
            BindVirtualCamera();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Awake()
        {
            _currentPitch = _pitch;
            EnsureCinemachine();
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            _target = null;
            _input = null;
            _follow = null;

            if (_vcam != null)
            {
                Destroy(_vcam.gameObject);
                _vcam = null;
            }

            if (_pivot != null)
            {
                Destroy(_pivot.gameObject);
                _pivot = null;
            }
        }

        private void LateUpdate()
        {
            if (_hasTarget == false)
            {
                return;
            }

            if (_target == null)
            {
                _hasTarget = false;
                BindVirtualCamera();
                return;
            }

            Transform playerFocus = _target;
            UpdateCursor();

            if (_hasPose == false || _isLockedToPlayer)
            {
                _yaw = GetPlanarYaw(playerFocus.forward);
                _currentPitch = _pitch;
                _hasPose = true;
            }
            else if (_isLookEnabled)
            {
                ApplyLook();
            }

            Quaternion rotation = Quaternion.Euler(_currentPitch, _yaw, 0f);
            Vector3 lookPosition = playerFocus.position + Vector3.up * _lookHeight;
            _pivot.SetPositionAndRotation(lookPosition, rotation);
            _follow.CameraDistance = KeepDistance(lookPosition, lookPosition - rotation * Vector3.forward * _distance);
        }

        internal void LockToPlayer()
        {
            _isLockedToPlayer = true;
            _hasPose = false;
        }

        internal void UnlockFollow()
        {
            _isLockedToPlayer = false;
        }

        internal void SetLookEnabled(bool isEnabled)
        {
            _isLookEnabled = isEnabled;
        }

        private void EnsureCinemachine()
        {
            if (GetComponent<CinemachineBrain>() == null)
            {
                gameObject.AddComponent<CinemachineBrain>();
            }

            if (_pivot != null)
            {
                return;
            }

            GameObject pivotObject = new GameObject("CinemachinePivot");
            _pivot = pivotObject.transform;
            GameObject vcamObject = new GameObject("RunnerCinemachineCamera");
            _vcam = vcamObject.AddComponent<CinemachineCamera>();
            _follow = vcamObject.AddComponent<CinemachineThirdPersonFollow>();
            _follow.CameraDistance = _distance;
            _follow.ShoulderOffset = Vector3.zero;
            _follow.VerticalArmLength = 0f;
            _follow.CameraSide = 0.5f;
            CinemachineThirdPersonFollow.ObstacleSettings obstacles = _follow.AvoidObstacles;
            obstacles.Enabled = true;
            obstacles.CameraRadius = _collisionRadius;
            obstacles.CollisionFilter = _collisionMask;
            _follow.AvoidObstacles = obstacles;
            CameraTarget cameraTarget = _vcam.Target;
            cameraTarget.TrackingTarget = _pivot;
            cameraTarget.LookAtTarget = _pivot;
            cameraTarget.CustomLookAtTarget = true;
            _vcam.Target = cameraTarget;
        }

        private void BindVirtualCamera()
        {
            if (_vcam == null)
            {
                return;
            }

            _vcam.enabled = _hasTarget && _target != null;
        }

        private void UpdateCursor()
        {
            bool lockCursor = _isLookEnabled && Input.touchCount == 0 && HasLookStick() == false;

            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ApplyLook()
        {
            _yaw += Input.GetAxis("Mouse X") * _yawSpeed;
            _currentPitch -= Input.GetAxis("Mouse Y") * _pitchSpeed;

            if (HasLookStick())
            {
                Vector2 stick = _input.LookStick;
                _yaw += stick.x * _stickYawSpeed * Time.deltaTime;
                _currentPitch -= stick.y * _stickPitchSpeed * Time.deltaTime;
            }

            _currentPitch = Mathf.Clamp(_currentPitch, _minPitch, _maxPitch);
        }

        private bool HasLookStick()
        {
            if (_input == null)
            {
                return false;
            }

            return _input.LookStick.sqrMagnitude > 0.0001f;
        }

        private static float GetPlanarYaw(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude <= DirectionEpsilonSqr)
            {
                return 0f;
            }

            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }

        private float KeepDistance(Vector3 origin, Vector3 desiredPosition)
        {
            Vector3 direction = desiredPosition - origin;
            float distance = direction.magnitude;

            if (distance < _minDistance)
            {
                return _minDistance;
            }

            direction /= distance;
            int collisionMask = _collisionMask & ~(1 << PlayerMover.DashWallLayerIndex);
            int hitCount = Physics.SphereCastNonAlloc(origin, _collisionRadius, direction, _hits, distance, collisionMask, QueryTriggerInteraction.Ignore);
            float allowedDistance = distance;

            for (int i = 0; i < hitCount; i++)
            {
                if (IsOwnCollider(_hits[i].transform))
                {
                    continue;
                }

                if (_hits[i].distance <= EmbeddedHitDistance)
                {
                    continue;
                }

                if (Vector3.Dot(_hits[i].normal, direction) > 0f)
                {
                    continue;
                }

                float surfaceDistance = _hits[i].distance - _collisionPadding;

                if (surfaceDistance < 0f)
                {
                    surfaceDistance = 0f;
                }

                if (surfaceDistance < allowedDistance)
                {
                    allowedDistance = surfaceDistance;
                }
            }

            if (allowedDistance < _minDistance)
            {
                allowedDistance = _minDistance;
            }

            return allowedDistance;
        }

        private bool IsOwnCollider(Transform hitTransform)
        {
            Transform body = _target.parent;

            if (body == null)
            {
                body = _target;
            }

            if (hitTransform == body)
            {
                return true;
            }

            return hitTransform.IsChildOf(body);
        }
    }
}
