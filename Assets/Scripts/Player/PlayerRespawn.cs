using UnityEngine;

namespace SlipUp
{
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;

        private Level _level;
        private bool _isReported;

        private bool IsOutsideLocation => _level.IsBelowArena(_rigidbody.position);

        internal void Initialize(Level level)
        {
            _level = level;
            TeleportToStart();
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnDestroy()
        {
            _level = null;
        }

        private void FixedUpdate()
        {
            if (_isReported)
            {
                return;
            }

            if (IsOutsideLocation == false)
            {
                return;
            }

            _isReported = true;
            _level.NotifyFell();
        }

        internal void TeleportToStart()
        {
            _isReported = false;
            Vector3 position = _level.StartPosition;
            Quaternion rotation = _level.StartRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _rigidbody.position = position;
            _rigidbody.rotation = rotation;
            _rigidbody.transform.SetPositionAndRotation(position, rotation);

            if (_rigidbody.isKinematic)
            {
                return;
            }

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
