using UnityEngine;

namespace SlipUp
{
    public class FinishGate : MonoBehaviour
    {
        private Track _track;

        internal void Initialize(Track track)
        {
            _track = track;
        }

        private void OnDestroy()
        {
            _track = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_track == null)
            {
                return;
            }

            if (HasRunner(other) == false)
            {
                return;
            }

            _track.RaiseFinish();
        }

        private static bool HasRunner(Collider other)
        {
            if (other.TryGetComponent<Runner>(out _))
            {
                return true;
            }

            Rigidbody body = other.attachedRigidbody;

            if (body == null)
            {
                return false;
            }

            return body.TryGetComponent<Runner>(out _);
        }
    }
}
