using UnityEngine;

namespace SlipUp
{
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;

        private Arena _arena;
        private Runner _runner;
        private bool _isReported;

        private bool IsOutsideLocation => _arena.IsBelowArena(_rigidbody.position);

        internal void Initialize(Arena arena, Runner runner)
        {
            _arena = arena;
            _runner = runner;
            TeleportToStart();
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        internal void AllowReport()
        {
            _isReported = false;
        }

        private void OnDestroy()
        {
            _arena = null;
            _runner = null;
        }

        private void FixedUpdate()
        {
            if (_arena == null)
            {
                return;
            }

            if (_runner.IsFrozen)
            {
                return;
            }

            if (_isReported)
            {
                return;
            }

            if (IsOutsideLocation == false)
            {
                return;
            }

            _isReported = true;
            _runner.NotifyFell();
        }

        private void TeleportToStart()
        {
            _isReported = false;
            Vector3 position = _arena.StartPosition;
            Quaternion rotation = _arena.StartRotation;
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
