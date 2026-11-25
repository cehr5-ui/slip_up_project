using UnityEngine;

namespace SlipUp
{
    [DefaultExecutionOrder(100)]
    public class PlayerAnimator : MonoBehaviour
    {
        private const float TurnSpeed = 720f;
        private const float FaceDirectionEpsilonSqr = 0.01f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int DashHash = Animator.StringToHash("Dash");

        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PlayerMover _mover;

        private void Awake()
        {
            _animator.applyRootMotion = false;

            if (_mover == null)
            {
                _mover = GetComponentInParent<PlayerMover>();
            }
        }

        private void OnDestroy()
        {
            _animator = null;
            _rigidbody = null;
            _mover = null;
        }

        private void FixedUpdate()
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);
            _animator.SetFloat(SpeedHash, planarVelocity.magnitude);
            _animator.SetBool(JumpHash, _mover != null && _mover.IsGrounded == false);
            _animator.SetBool(DashHash, _mover != null && _mover.IsDashing);
            FacePlanarVelocity(planarVelocity);
        }

        private void FacePlanarVelocity(Vector3 planarVelocity)
        {
            if (planarVelocity.sqrMagnitude <= FaceDirectionEpsilonSqr)
            {
                return;
            }

            Quaternion lookRotation = Quaternion.LookRotation(planarVelocity, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, TurnSpeed * Time.fixedDeltaTime);
        }

        internal void SetEnabled(bool isEnabled)
        {
            _animator.enabled = isEnabled;
        }
    }
}
