using UnityEngine;

namespace SlipUp
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
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
        private float _yaw;
        private float _pitch;
        private Vector3 _pivotPosition;
        private bool _hasPivot;

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _yaw += Input.GetAxis("Mouse X") * _yawSpeed;
            _pitch += Input.GetAxis("Mouse Y") * _pitchSpeed;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            Quaternion orbitRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            if (_hasPivot == false)
            {
                _pivotPosition = _target.position;
                _hasPivot = true;
            }

            if (_useSmoothing)
            {
                float t = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
                _pivotPosition = Vector3.Lerp(_pivotPosition, _target.position, t);
            }
            else
            {
                _pivotPosition = _target.position;
            }

            Vector3 desiredPosition = _pivotPosition + orbitRotation * _offset;
            transform.position = KeepOutOfSurfaces(desiredPosition);
            transform.LookAt(_target.position);
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
            if (hitTransform == _target || hitTransform.IsChildOf(_target))
            {
                return true;
            }

            return _target.IsChildOf(hitTransform);
        }
    }
}