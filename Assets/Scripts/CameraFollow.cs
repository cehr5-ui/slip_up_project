using UnityEngine;

namespace SlipUp
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 3.5f, -8f);
        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private float _yawSpeed = 5f;
        [SerializeField] private bool _useSmoothing = true;

        private float _yaw;
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
            Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);

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

            transform.position = _pivotPosition + yawRotation * _offset;
            transform.LookAt(_target.position);
        }
    }
}