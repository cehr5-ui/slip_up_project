using UnityEngine;

namespace SlipUp
{
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _location;
        [SerializeField] private PlayerCollector _collector;
        [SerializeField] private float _outsideMargin = 20f;

        private Vector3 _startPosition;
        private bool _isReturning;
        private bool _hasRespawnedCoins;

        private void Awake()
        {
            _startPosition = _rigidbody.position;
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
                CoinSpawner.RespawnAll();
                _hasRespawnedCoins = true;
                return;
            }

            _collector.ResumeCollection();
            _isReturning = false;
        }

        private bool IsOutsideLocation
        {
            get
            {
                Bounds bounds = _location.bounds;
                bounds.Expand(_outsideMargin);
                Vector3 position = _rigidbody.position;

                if (position.y < bounds.min.y)
                {
                    return true;
                }

                if (position.x < bounds.min.x || position.x > bounds.max.x)
                {
                    return true;
                }

                return position.z < bounds.min.z || position.z > bounds.max.z;
            }
        }

        private void TeleportToStart()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = _startPosition;
        }
    }
}
