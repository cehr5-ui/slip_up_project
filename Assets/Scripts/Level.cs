using UnityEngine;

namespace SlipUp
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private Collider _location;
        [SerializeField] private CoinSpawner[] _coinSpawners;
        [SerializeField] private Pusher[] _pushers;
        [SerializeField] private float _outsideMargin = 20f;

        internal Transform CameraTransform => _camera.transform;

        internal Transform PlayerFocus => _player.Focus;

        private int CoinCount
        {
            get
            {
                int coinCount = 0;

                for (int i = 0; i < _coinSpawners.Length; i++)
                {
                    coinCount += _coinSpawners[i].CoinCount;
                }

                return coinCount;
            }
        }

        private void Awake()
        {
            _player.Initialize(this);
            _camera.Initialize(this);

            for (int i = 0; i < _pushers.Length; i++)
            {
                _pushers[i].Initialize(this);
            }
        }

        internal bool TryTakeCoin(Collider other)
        {
            if (other.TryGetComponent(out CoinSpin _) == false)
            {
                return false;
            }

            Destroy(other.gameObject);
            return true;
        }

        internal void ShowScore(int score)
        {
            _scoreView.SetScore(score);
            _scoreView.SetCleared(CoinCount > 0 && score >= CoinCount);
        }

        internal void RespawnCoins()
        {
            for (int i = 0; i < _coinSpawners.Length; i++)
            {
                _coinSpawners[i].RespawnCoins();
            }
        }

        internal bool IsBelowArena(Vector3 position)
        {
            Bounds bounds = _location.bounds;
            bounds.Expand(_outsideMargin);
            return position.y < bounds.min.y;
        }

        internal bool IsPlayer(GameObject target)
        {
            return target == _player.gameObject;
        }

        internal void BouncePlayer(Vector3 velocity)
        {
            _player.Bounce(velocity);
        }
    }
}
