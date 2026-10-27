using UnityEngine;

namespace SlipUp
{
    public class PlayerMover : MonoBehaviour
    {
        private const float GroundNormalY = 0.5f;
        private const float SpeedPerCoin = 0.02f;
        private const float DashSpeedMultiplier = 3f;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _jumpSpeed = 7f;
        [SerializeField] private float _maxFallSpeed = 40f;
        [SerializeField] private float _dashDistance = 5f;
        [SerializeField] private float _dashCooldown = 2f;
        [SerializeField] private float _bounceDuration = 0.15f;
        [SerializeField] private PlayerCollector _collector;

        private Level _level;
        private Vector3 _planarInput;
        private Vector3 _dashDirection;
        private bool _isJumpRequested;
        private bool _isDashRequested;
        private bool _isDashing;
        private bool _isGrounded;
        private float _dashCooldownLeft;
        private float _dashDistanceLeft;
        private float _dashSpeed;
        private float _bounceTimeLeft;
        private Vector3 _bounceVelocity;
        private bool _isRunStarted;
        private bool _isFrozen;

        internal bool IsControlEnabled => _isRunStarted && _isFrozen == false;

        private bool IsDashReady => _dashCooldownLeft <= 0f;

        internal void Initialize(Level level)
        {
            _level = level;
        }

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnDestroy()
        {
            _level = null;
        }

        private void Update()
        {
            if (_dashCooldownLeft > 0f)
            {
                _dashCooldownLeft -= Time.deltaTime;

                if (_dashCooldownLeft < 0f)
                {
                    _dashCooldownLeft = 0f;
                }
            }

            _level.ShowDash(IsDashReady, _dashCooldownLeft);

            if (IsControlEnabled == false)
            {
                _planarInput = Vector3.zero;
                _isJumpRequested = false;
                _isDashRequested = false;
                return;
            }

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
            if (_isFrozen)
            {
                return;
            }

            if (_bounceTimeLeft > 0f)
            {
                _bounceTimeLeft -= Time.fixedDeltaTime;
                _rigidbody.linearVelocity = _bounceVelocity;
                ClearGrounded();
                return;
            }

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

            Vector3 wishDirection = GetWishDirection();
            Vector3 targetVelocity = wishDirection * maxSpeed;

            Vector3 currentVelocity = _rigidbody.linearVelocity;
            Vector3 currentPlanarVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

            float acceleration = _acceleration * coinBonus;
            float rate = _planarInput.sqrMagnitude > 0.01f ? acceleration : _deceleration;
            float maxSpeedChange = rate * Time.fixedDeltaTime;
            Vector3 nextPlanarVelocity = Vector3.MoveTowards(currentPlanarVelocity, targetVelocity, maxSpeedChange);
            float verticalSpeed = currentVelocity.y;

            if (verticalSpeed < 0f)
            {
                verticalSpeed += Physics.gravity.y * Time.fixedDeltaTime;
            }

            if (verticalSpeed < -_maxFallSpeed)
            {
                verticalSpeed = -_maxFallSpeed;
            }

            _rigidbody.linearVelocity = new Vector3(nextPlanarVelocity.x, verticalSpeed, nextPlanarVelocity.z);
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
                }
            }
        }

        internal void Bounce(Vector3 velocity)
        {
            _isDashing = false;
            _isDashRequested = false;
            _isJumpRequested = false;
            _bounceVelocity = velocity;
            _bounceTimeLeft = _bounceDuration;
            _rigidbody.linearVelocity = velocity;
        }

        internal void BeginRun()
        {
            _isRunStarted = true;
        }

        internal void Freeze()
        {
            _isFrozen = true;
            _planarInput = Vector3.zero;
            _isJumpRequested = false;
            _isDashRequested = false;
            _isDashing = false;
            _dashDistanceLeft = 0f;
            _bounceTimeLeft = 0f;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }

        internal void Release()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.WakeUp();
            _isFrozen = false;
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
            if (IsDashReady == false)
            {
                return;
            }

            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 direction = new Vector3(velocity.x, 0f, velocity.z);

            if (direction.sqrMagnitude <= 0.01f)
            {
                direction = GetWishDirection();
            }

            if (direction.sqrMagnitude <= 0.01f)
            {
                return;
            }

            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;
            _dashDirection = direction.normalized;
            _dashSpeed = maxSpeed * DashSpeedMultiplier;
            _dashDistanceLeft = _dashDistance;
            _isDashing = true;
            _dashCooldownLeft = _dashCooldown;
        }

        private Vector3 GetWishDirection()
        {
            Vector3 forward = Vector3.ProjectOnPlane(_level.CameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_level.CameraTransform.right, Vector3.up).normalized;
            return forward * _planarInput.z + right * _planarInput.x;
        }

        private void ApplyDash()
        {
            float step = _dashSpeed * Time.fixedDeltaTime;

            if (step >= _dashDistanceLeft)
            {
                _rigidbody.linearVelocity = new Vector3(_dashDirection.x * _dashSpeed, 0f, _dashDirection.z * _dashSpeed);
                _dashDistanceLeft = 0f;
                _isDashing = false;
                return;
            }

            _dashDistanceLeft -= step;
            _rigidbody.linearVelocity = new Vector3(_dashDirection.x * _dashSpeed, 0f, _dashDirection.z * _dashSpeed);
        }
    }
}