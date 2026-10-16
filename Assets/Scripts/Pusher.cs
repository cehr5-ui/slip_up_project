using UnityEngine;

namespace SlipUp
{
    public class Pusher : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _pushSpeed = 28f;

        private void OnCollisionEnter(Collision collision)
        {
            if (TryPushAway(collision) == false)
            {
                return;
            }

            _audioSource.Play();
        }

        private void OnCollisionStay(Collision collision)
        {
            TryPushAway(collision);
        }

        private bool TryPushAway(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerMover _) == false)
            {
                return false;
            }

            Vector3 direction = collision.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            Rigidbody body = collision.rigidbody;
            Vector3 velocity = body.linearVelocity;
            velocity.x = direction.x * _pushSpeed;
            velocity.z = direction.z * _pushSpeed;
            body.linearVelocity = velocity;
            return true;
        }
    }
}
