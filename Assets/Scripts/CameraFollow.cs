using UnityEngine;

namespace SlipUp
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 _offset = new Vector3(0f, 3.5f, -8f);
        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private float _yawSpeed = 2f;
        [SerializeField] private float _pitchSpeed = 2f;
        [SerializeField] private float _minPitch = -25f;
        [SerializeField] private float _maxPitch = 60f;
        [SerializeField] private float _focusHeight = 0.5f;
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private float _collisionPadding = 0.1f;
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private LayerMask _collisionMask = ~0;
        [SerializeField] private bool _useSmoothing = true;

        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private Level _level;
        private float _yaw;
        private float _pitch;
        private Vector3 _pivotPosition;
        private bool _hasPivot;
        private bool _isLookEnabled;
        private bool _isLockedToPlayer;

        private Transform PlayerFocus => _level.PlayerFocus;

        internal void Initialize(Level level)
        {
            _level = level;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            _level = null;
        }

        private void LateUpdate()
        {
            Transform playerFocus = PlayerFocus;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_isLookEnabled)
            {
                _yaw += Input.GetAxis("Mouse X") * _yawSpeed;
                _pitch += Input.GetAxis("Mouse Y") * _pitchSpeed;
                _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }

            Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            if (_hasPivot == false || _useSmoothing == false || _isLockedToPlayer)
            {
                _pivotPosition = playerFocus.position;
                _hasPivot = true;
            }
            else
            {
                float t = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
                _pivotPosition = Vector3.Lerp(_pivotPosition, playerFocus.position, t);
            }

            Vector3 desiredPosition = _pivotPosition + orbitRotation * _offset;
            transform.position = KeepOutOfSurfaces(desiredPosition);
            transform.LookAt(playerFocus.position);
        }

        internal void LockToPlayer()
        {
            _pivotPosition = PlayerFocus.position;
            _hasPivot = true;
            _isLockedToPlayer = true;
        }

        internal void UnlockFollow()
        {
            _isLockedToPlayer = false;
        }

        internal void SetLookEnabled(bool isEnabled)
        {
            _isLookEnabled = isEnabled;
        }

        private Vector3 KeepOutOfSurfaces(Vector3 desiredPosition)
        {
            Vector3 focus = _pivotPosition + Vector3.up * _focusHeight;
            Vector3 direction = desiredPosition - focus;
            float distance = direction.magnitude;

            if (distance < _minDistance)
            {
                return focus + direction.normalized * _minDistance;
            }

            direction /= distance;
            int hitCount = Physics.SphereCastNonAlloc(focus, _collisionRadius, direction, _hits, distance, _collisionMask, QueryTriggerInteraction.Ignore);
            float allowedDistance = distance;

            for (int i = 0; i < hitCount; i++)
            {
                if (IsOwnCollider(_hits[i].transform))
                {
                    continue;
                }

                float surfaceDistance = _hits[i].distance - _collisionPadding;

                if (surfaceDistance < allowedDistance)
                {
                    allowedDistance = surfaceDistance;
                }
            }

            if (allowedDistance < _minDistance)
            {
                allowedDistance = _minDistance;
            }

            return focus + direction * allowedDistance;
        }

        private bool IsOwnCollider(Transform hitTransform)
        {
            Transform playerFocus = PlayerFocus;

            if (hitTransform == playerFocus || hitTransform.IsChildOf(playerFocus))
            {
                return true;
            }

            return playerFocus.IsChildOf(hitTransform);
        }
    }
}