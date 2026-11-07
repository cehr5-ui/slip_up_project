using UnityEngine;

namespace SlipUp
{
    [DefaultExecutionOrder(100)]
    public class PlayerAnimator : MonoBehaviour
    {
        private const float TurnSpeed = 720f;
        private const float FaceDirectionEpsilonSqr = 0.01f;

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
    }
}

