using System.Collections.Generic;
using UnityEngine;

namespace SlipUp
{
    public class PlayerMover : MonoBehaviour
    {
        private const float GroundNormalY = 0.5f;
        private const float SpeedPerCoin = 0.02f;
        private const float DashSpeedMultiplier = 3f;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _jumpSpeed = 7f;
        [SerializeField] private float _dashDistance = 5f;
        [SerializeField] private PlayerCollector _collector;

        private readonly List<Vector3> _wallNormals = new List<Vector3>(4);
        private Vector3 _planarInput;
        private Vector3 _dashDirection;
        private bool _isJumpRequested;
        private bool _isDashRequested;
        private bool _isDashing;
        private bool _hasDashed;
        private bool _isGrounded;
        private float _dashDistanceLeft;
        private float _dashSpeed;

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

            if (Input.GetKeyDown(KeyCode.F))
            {
                _isDashRequested = true;
            }

            if (Input.GetKeyDown(KeyCode.Space) == false)
            {
                return;
            }

            _isJumpRequested = true;
        }

        private void FixedUpdate()
        {
            float coinBonus = 1f + _collector.Score * SpeedPerCoin;
            float maxSpeed = _moveSpeed * coinBonus;

            if (_isDashRequested)
            {
                _isDashRequested = false;
                TryStartDash(maxSpeed);
            }

            if (_isDashing)
            {
                ApplyDash();
                ClearGrounded();
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
            Vector3 wishDirection = forward * _planarInput.z + right * _planarInput.x;
            Vector3 targetVelocity = wishDirection * maxSpeed;

            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            float acceleration = _acceleration * coinBonus;
            float rate = _planarInput.sqrMagnitude > 0.01f ? acceleration : _deceleration;
            float maxSpeedChange = rate * Time.fixedDeltaTime;
            Vector3 nextPlanarVelocity = Vector3.MoveTowards(currentPlanarVelocity, targetVelocity, maxSpeedChange);
            nextPlanarVelocity = SlideAlongWalls(nextPlanarVelocity);
            _wallNormals.Clear();
            _rigidbody.linearVelocity = new Vector3(nextPlanarVelocity.x, currentVelocity.y, nextPlanarVelocity.z);
            ApplyJump();
            ClearGrounded();
        }

        private void OnCollisionStay(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;

                if (normal.y > GroundNormalY)
                {
                    _isGrounded = true;
                    _hasDashed = false;
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

        private void ClearGrounded()
        {
            if (_rigidbody.IsSleeping())
            {
                return;
            }

            _isGrounded = false;
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

        private void TryStartDash(float maxSpeed)
        {
            if (_isGrounded || _hasDashed)
            {
                return;
            }

            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);

            if (planarVelocity.sqrMagnitude <= 0.01f)
            {
                return;
            }

            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;
            _dashDirection = planarVelocity.normalized;
            _dashSpeed = maxSpeed * DashSpeedMultiplier;
            _dashDistanceLeft = _dashDistance;
            _isDashing = true;
            _hasDashed = true;
        }

        private void ApplyDash()
        {
            float step = _dashSpeed * Time.fixedDeltaTime;

            if (step >= _dashDistanceLeft)
            {
                float endSpeed = _dashDistanceLeft / Time.fixedDeltaTime;
                _rigidbody.linearVelocity = new Vector3(_dashDirection.x * endSpeed, 0f, _dashDirection.z * endSpeed);
                _dashDistanceLeft = 0f;
                _isDashing = false;
                return;
            }

            _dashDistanceLeft -= step;
            _rigidbody.linearVelocity = new Vector3(_dashDirection.x * _dashSpeed, 0f, _dashDirection.z * _dashSpeed);
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