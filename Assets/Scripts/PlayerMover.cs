using UnityEngine;

namespace SlipUp
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;

        private Vector3 _planarInput;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Update()
        {
            _planarInput = ReadPlanarInput();
        }

        private void FixedUpdate()
        {
            Vector3 forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
            Vector3 targetVelocity = (forward * _planarInput.z + right * _planarInput.x) * _moveSpeed;

            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            float rate = _planarInput.sqrMagnitude > 0.01f ? _acceleration : _deceleration;
            float maxSpeedChange = rate * Time.fixedDeltaTime;

            Vector3 nextPlanarVelocity = Vector3.MoveTowards(currentPlanarVelocity, targetVelocity, maxSpeedChange);
            _rigidbody.linearVelocity = new Vector3(nextPlanarVelocity.x, currentVelocity.y, nextPlanarVelocity.z);
        }

        private Vector3 ReadPlanarInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);

            if (inputDirection.sqrMagnitude > 1f)
            {
                inputDirection.Normalize();
            }

            return inputDirection;
        }
    }
}