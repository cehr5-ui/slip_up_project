using UnityEngine;

namespace SlipUp
{
    public class CameraFollow : MonoBehaviour
    {
        private const float DirectionEpsilonSqr = 0.0001f;
        private const float EmbeddedHitDistance = 0.001f;

        [SerializeField] private float _distance = 8.75f;
        [SerializeField] private float _pitch = 32f;
        [SerializeField] private float _lookHeight = 1.1f;
        [SerializeField] private float _yawSpeed = 2.5f;
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private float _collisionPadding = 0.15f;
        [SerializeField] private float _minDistance = 1.15f;
        [SerializeField] private LayerMask _collisionMask = ~0;

        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private Transform _target;
        private float _yaw;
        private bool _hasTarget;
        private bool _hasPose;
        private bool _isLookEnabled;
        private bool _isLockedToPlayer;

        internal void SetTarget(Transform target)
        {
            _target = target;
            _hasTarget = true;
            _hasPose = false;
        }

        internal void ClearTarget()
        {
            _target = null;
            _hasTarget = false;
            _isLookEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            _target = null;
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
                return;
            }

            Transform playerFocus = _target;
            Vector3 focusPosition = playerFocus.position;

            if (_isLookEnabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (_hasPose == false || _isLockedToPlayer)
            {
                _yaw = GetPlanarYaw(playerFocus.forward);
                _hasPose = true;
            }
            else if (_isLookEnabled)
            {
                _yaw += Input.GetAxis("Mouse X") * _yawSpeed;
            }

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 lookPosition = focusPosition + Vector3.up * _lookHeight;
            Vector3 desiredPosition = lookPosition - rotation * Vector3.forward * _distance;
            Vector3 position = KeepOutOfSurfaces(lookPosition, desiredPosition);
            transform.SetPositionAndRotation(position, rotation);
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

        private static float GetPlanarYaw(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude <= DirectionEpsilonSqr)
            {
                return 0f;
            }

            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }

        private Vector3 KeepOutOfSurfaces(Vector3 origin, Vector3 desiredPosition)
        {
            Vector3 direction = desiredPosition - origin;
            float distance = direction.magnitude;

            if (distance < _minDistance)
            {
                return origin + direction.normalized * _minDistance;
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

            return origin + direction * allowedDistance;
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
