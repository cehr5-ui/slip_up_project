using UnityEngine;

namespace SlipUp
{
    [DefaultExecutionOrder(100)]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidbody;

        private void Awake()
        {
            _animator.applyRootMotion = false;
        }

        private void FixedUpdate()
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 planarVelocity = new Vector3(velocity.x, 0f, velocity.z);
            _animator.SetFloat(SpeedHash, planarVelocity.magnitude);
        }
    }
}

