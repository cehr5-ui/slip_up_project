using System.Collections.Generic;
using UnityEngine;

namespace SlipUp
{
    public class PlayerMover : MonoBehaviour
    {
        private const float GroundNormalY = 0.5f;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _jumpSpeed = 7f;

        private readonly List<Vector3> _wallNormals = new List<Vector3>(4);
        private Vector3 _planarInput;
        private bool _isJumpRequested;
        private bool _isGrounded;

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

            if (Input.GetKeyDown(KeyCode.Space) == false)
            {
                return;
            }

            _isJumpRequested = true;
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
            nextPlanarVelocity = SlideAlongWalls(nextPlanarVelocity);
            _wallNormals.Clear();
            _rigidbody.linearVelocity = new Vector3(nextPlanarVelocity.x, currentVelocity.y, nextPlanarVelocity.z);
            ApplyJump();

            if (_rigidbody.IsSleeping())
            {
                return;
            }

            _isGrounded = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;

                if (normal.y > GroundNormalY)
                {
                    _isGrounded = true;
                    continue;
                }

                _wallNormals.Add(normal);
            }
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

        private void ApplyJump()
        {
            if (_isJumpRequested == false)
            {
                return;
            }

            _isJumpRequested = false;

            if (_isGrounded == false)
            {
                return;
            }

            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = _jumpSpeed;
            _rigidbody.linearVelocity = velocity;
        }

        private Vector3 SlideAlongWalls(Vector3 planarVelocity)
        {
            for (int i = 0; i < _wallNormals.Count; i++)
            {
                Vector3 normal = _wallNormals[i];
                normal.y = 0f;

                if (normal.sqrMagnitude < 0.0001f)
                {
                    continue;
                }

                normal.Normalize();
                float intoWall = Vector3.Dot(planarVelocity, normal);

                if (intoWall >= 0f)
                {
                    continue;
                }

                planarVelocity -= normal * intoWall;
            }

            return planarVelocity;
        }
    }
}