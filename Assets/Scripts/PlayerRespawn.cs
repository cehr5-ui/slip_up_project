using UnityEngine;

namespace SlipUp
{
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PlayerCollector _collector;

        private Level _level;
        private Vector3 _startPosition;
        private bool _isReturning;
        private bool _hasRespawnedCoins;

        private bool IsOutsideLocation => _level.IsBelowArena(_rigidbody.position);

        internal void Initialize(Level level)
        {
            _level = level;
        }

        private void Awake()
        {
            _startPosition = _rigidbody.position;
        }

        private void OnDestroy()
        {
            _level = null;
        }

        private void FixedUpdate()
        {
            if (IsOutsideLocation)
            {
                if (_isReturning)
                {
                    return;
                }

                TeleportToStart();
                _collector.ResetScore();
                _collector.PauseCollection();
                _isReturning = true;
                _hasRespawnedCoins = false;
                return;
            }

            if (_isReturning == false)
            {
                return;
            }

            if (_hasRespawnedCoins == false)
            {
                _level.RespawnCoins();
                _hasRespawnedCoins = true;
                return;
            }

            _collector.ResumeCollection();
            _isReturning = false;
        }

        private void TeleportToStart()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = _startPosition;
        }
    }
}
