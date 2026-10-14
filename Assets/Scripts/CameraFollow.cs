using UnityEngine;

namespace SlipUp
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 3.5f, -8f);
        [SerializeField] private float _followSpeed = 10f;
        [SerializeField] private bool _useSmoothing = true;

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 targetPosition = _target.position + _offset;

            if (_useSmoothing)
            {
                float t = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
                transform.position = Vector3.Lerp(transform.position, targetPosition, t);
            }
            else
            {
                transform.position = targetPosition;
            }

            transform.LookAt(_target.position);
        }
    }
}