using Unity.Collections;
using UnityEngine;

namespace SlipUp
{
    public class PlayerMover : MonoBehaviour
    {
        private const float GroundNormalY = 0.5f;
        private const float GroundProbeSkin = 0.05f;
        private const float GroundedMaxRiseSpeed = 0.1f;
        private const float SpeedPerCoin = 0.02f;
        private const float DashSpeedMultiplier = 3f;
        private const float SideContactRadiusScale = 0.35f;
        private const float FloorEdgeBelowCenter = 0.35f;
        private const int DashWallTouchCapacity = 4;
        internal const int DashWallLayerIndex = 6;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _jumpSpeed = 7f;
        [SerializeField] private float _maxFallSpeed = 40f;
        [SerializeField] private float _dashDistance = 5f;
        [SerializeField] private float _dashCooldown = 2f;
        [SerializeField] private float _bounceDuration = 0.15f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        [SerializeField] private float _coyoteDuration = 0.15f;
        [SerializeField] private PlayerCollector _collector;

        private Level _level;
        private Vector3 _planarInput;
        private Vector3 _dashDirection;
        private bool _isJumpRequested;
        private bool _isDashRequested;
        private bool _isDashing;
        private bool _isPassingDashWall;
        private bool _isGrounded;
        private bool _hasAirJump;
        private float _coyoteTimeLeft;
        private float _dashCooldownLeft;
        private float _dashDistanceLeft;
        private float _dashSpeed;
        private float _bounceTimeLeft;
        private Vector3 _bounceVelocity;
        private bool _isRunStarted;
        private bool _isFrozen;
        private int _capsuleId;
        private float _sideContactDistanceSqr;
        private Vector3 _capsuleCenter;
        private PhysicsMaterial _slideMaterial;
        private readonly RaycastHit[] _groundHits = new RaycastHit[8];
        private readonly Collider[] _touchingDashWalls = new Collider[DashWallTouchCapacity];
        private readonly Collider[] _ignoredDashWalls = new Collider[DashWallTouchCapacity];
        private int _touchingDashWallCount;
        private int _ignoredDashWallCount;

        internal bool IsControlEnabled => _isRunStarted && _isFrozen == false;

        private bool IsDashReady => _dashCooldownLeft <= 0f;

        private bool IsTouchingDashWall => _touchingDashWallCount > 0;

        private bool IsTouchingGround
        {
            get
            {
                Vector3 bottom = _rigidbody.position + Vector3.up * (_capsule.center.y - _capsule.height * 0.5f);
                Vector3 origin = bottom + Vector3.up * GroundProbeSkin;
                float distance = GroundProbeSkin + _groundCheckDistance;
                int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, _groundHits, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

                for (int i = 0; i < hitCount; i++)
                {
                    if (_groundHits[i].collider == _capsule)
                    {
                        continue;
                    }

                    if (_groundHits[i].normal.y > GroundNormalY)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

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
            _capsuleId = _capsule.GetInstanceID();
            _capsuleCenter = _capsule.center;
            float sideContactDistance = _capsule.radius * SideContactRadiusScale;
            _sideContactDistanceSqr = sideContactDistance * sideContactDistance;
            CreateSlideMaterial();
        }

        private void OnEnable()
        {
            _capsule.hasModifiableContacts = true;
            Physics.ContactModifyEvent += OnContactModify;
            Physics.ContactModifyEventCCD += OnContactModify;
        }

        private void OnDisable()
        {
            EndDashPass();
            Physics.ContactModifyEvent -= OnContactModify;
            Physics.ContactModifyEventCCD -= OnContactModify;
        }

        private void OnDestroy()
        {
            EndDashPass();
            ClearTouchingDashWalls();
            Destroy(_slideMaterial);
            _slideMaterial = null;
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

            RefreshGround();

            if (_bounceTimeLeft > 0f)
            {
                _bounceTimeLeft -= Time.fixedDeltaTime;
                _rigidbody.linearVelocity = _bounceVelocity;
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
                return;
            }

            EndDashPass();

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
        }

        internal void Bounce(Vector3 velocity)
        {
            _isDashing = false;
            _isDashRequested = false;
            _isJumpRequested = false;
            EndDashPass();
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
            EndDashPass();
            _bounceTimeLeft = 0f;
            _isGrounded = false;
            _hasAirJump = false;
            _coyoteTimeLeft = 0f;

            if (_rigidbody.isKinematic == false)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            _rigidbody.isKinematic = true;
        }

        internal void Release()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
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

        private void RefreshGround()
        {
            if (IsTouchingGround && _rigidbody.linearVelocity.y <= GroundedMaxRiseSpeed)
            {
                _isGrounded = true;
                _coyoteTimeLeft = _coyoteDuration;
                _hasAirJump = true;
                return;
            }

            _isGrounded = false;

            if (_coyoteTimeLeft <= 0f)
            {
                return;
            }

            _coyoteTimeLeft -= Time.fixedDeltaTime;

            if (_coyoteTimeLeft < 0f)
            {
                _coyoteTimeLeft = 0f;
            }
        }

        private void ApplyJump()
        {
            if (_isJumpRequested == false)
            {
                return;
            }

            _isJumpRequested = false;

            if (_isGrounded || _coyoteTimeLeft > 0f)
            {
                _coyoteTimeLeft = 0f;
                _isGrounded = false;
                SetJumpVelocity();
                return;
            }

            if (_hasAirJump == false)
            {
                return;
            }

            _hasAirJump = false;
            SetJumpVelocity();
        }

        private void SetJumpVelocity()
        {
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
            bool isAimingThroughWall = IsTouchingDashWall && _planarInput.sqrMagnitude > 0.01f;

            if (isAimingThroughWall || direction.sqrMagnitude <= 0.01f)
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
            BeginDashPass();
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

        private void BeginDashPass()
        {
            _isPassingDashWall = true;
            LayerMask dashWallMask = 1 << DashWallLayerIndex;
            _capsule.excludeLayers = dashWallMask;
            _rigidbody.excludeLayers = dashWallMask;
            IgnoreTouchingDashWalls();
        }

        private void EndDashPass()
        {
            if (_isPassingDashWall == false)
            {
                return;
            }

            _isPassingDashWall = false;
            _capsule.excludeLayers = default;
            _rigidbody.excludeLayers = default;
            RestoreDashWallCollisions();
        }

        private void IgnoreTouchingDashWalls()
        {
            _ignoredDashWallCount = _touchingDashWallCount;

            for (int i = 0; i < _ignoredDashWallCount; i++)
            {
                _ignoredDashWalls[i] = _touchingDashWalls[i];
            }

            for (int i = 0; i < _ignoredDashWallCount; i++)
            {
                Physics.IgnoreCollision(_capsule, _ignoredDashWalls[i], true);
            }
        }

        private void RestoreDashWallCollisions()
        {
            for (int i = 0; i < _ignoredDashWallCount; i++)
            {
                Physics.IgnoreCollision(_capsule, _ignoredDashWalls[i], false);
                _ignoredDashWalls[i] = null;
            }

            _ignoredDashWallCount = 0;
        }

        private void RememberDashWall(Collider dashWall)
        {
            if (dashWall.gameObject.layer != DashWallLayerIndex)
            {
                return;
            }

            for (int i = 0; i < _touchingDashWallCount; i++)
            {
                if (_touchingDashWalls[i] == dashWall)
                {
                    return;
                }
            }

            if (_touchingDashWallCount >= DashWallTouchCapacity)
            {
                return;
            }

            _touchingDashWalls[_touchingDashWallCount] = dashWall;
            _touchingDashWallCount++;
        }

        private void ForgetDashWall(Collider dashWall)
        {
            for (int i = 0; i < _touchingDashWallCount; i++)
            {
                if (_touchingDashWalls[i] != dashWall)
                {
                    continue;
                }

                int lastIndex = _touchingDashWallCount - 1;
                _touchingDashWalls[i] = _touchingDashWalls[lastIndex];
                _touchingDashWalls[lastIndex] = null;
                _touchingDashWallCount = lastIndex;
                return;
            }
        }

        private void ClearTouchingDashWalls()
        {
            for (int i = 0; i < _touchingDashWallCount; i++)
            {
                _touchingDashWalls[i] = null;
            }

            _touchingDashWallCount = 0;
        }

        private void CreateSlideMaterial()
        {
            _slideMaterial = new PhysicsMaterial();
            _slideMaterial.dynamicFriction = 0f;
            _slideMaterial.staticFriction = 0f;
            _slideMaterial.bounciness = 0f;
            _slideMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
            _slideMaterial.bounceCombine = PhysicsMaterialCombine.Minimum;
            _capsule.sharedMaterial = _slideMaterial;
        }

        private bool IsPlayerPair(ModifiableContactPair pair)
        {
            if (pair.colliderInstanceID == _capsuleId)
            {
                return true;
            }

            return pair.otherColliderInstanceID == _capsuleId;
        }

        private bool TryAdjustContacts(ref ModifiableContactPair pair)
        {
            Vector3 axis = pair.position;
            Quaternion rotation = pair.rotation;

            if (pair.colliderInstanceID != _capsuleId)
            {
                axis = pair.otherPosition;
                rotation = pair.otherRotation;
            }

            axis += rotation * _capsuleCenter;
            float floorEdgeHeight = axis.y - FloorEdgeBelowCenter;
            bool isChanged = false;

            for (int contactIndex = 0; contactIndex < pair.contactCount; contactIndex++)
            {
                Vector3 point = pair.GetPoint(contactIndex);
                Vector3 normal = pair.GetNormal(contactIndex);

                if (TryClearFloorEdge(ref pair, contactIndex, point, normal, floorEdgeHeight))
                {
                    isChanged = true;
                    continue;
                }

                Vector3 offset = point - axis;
                offset.y = 0f;

                if (offset.sqrMagnitude <= _sideContactDistanceSqr)
                {
                    continue;
                }

                Vector3 flatNormal = new Vector3(normal.x, 0f, normal.z);

                if (flatNormal.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                pair.SetNormal(contactIndex, flatNormal.normalized);
                isChanged = true;
            }

            return isChanged;
        }

        private static bool TryClearFloorEdge(ref ModifiableContactPair pair, int contactIndex, Vector3 point, Vector3 normal, float floorEdgeHeight)
        {
            if (point.y >= floorEdgeHeight)
            {
                return false;
            }

            if (normal.y > GroundNormalY)
            {
                pair.SetNormal(contactIndex, Vector3.up);
                return true;
            }

            if (normal.y < -GroundNormalY)
            {
                pair.SetNormal(contactIndex, Vector3.down);
                return true;
            }

            pair.IgnoreContact(contactIndex);
            return true;
        }

        private void OnContactModify(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
        {
            for (int i = 0; i < pairs.Length; i++)
            {
                ModifiableContactPair pair = pairs[i];

                if (IsPlayerPair(pair) == false)
                {
                    continue;
                }

                if (TryAdjustContacts(ref pair) == false)
                {
                    continue;
                }

                pairs[i] = pair;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            RememberDashWall(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            RememberDashWall(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            ForgetDashWall(collision.collider);
        }
    }
}
