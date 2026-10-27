using UnityEngine;

namespace SlipUp
{
    public class Level : MonoBehaviour
    {
        private const float SpawnMarkerRadius = 0.5f;

        [SerializeField] private Player _player;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private Collider _location;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private CoinSpawner[] _coinSpawners;
        [SerializeField] private float _outsideMargin = 20f;

        private static Level _instance;

        internal Transform CameraTransform => _camera.transform;

        internal Transform PlayerFocus => _player.Focus;

        internal Vector3 StartPosition => _spawnPoint.position;

        internal Quaternion StartRotation => _spawnPoint.rotation;

        private void Awake()
        {
            _instance = this;
            _player.Initialize(this);
            _camera.Initialize(this);
        }

        private void OnDestroy()
        {
            _instance = null;
            _spawnPoint = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_spawnPoint.position, SpawnMarkerRadius);
        }

        internal static void Connect(Pusher pusher)
        {
            pusher.Initialize(_instance);
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

        internal void BeginRun()
        {
            _player.BeginRun();
            _camera.SetLookEnabled(_player.IsControlEnabled);
        }

        internal void FreezePlayer()
        {
            _player.Freeze();
            _camera.SetLookEnabled(false);
        }

        internal void ReleasePlayer()
        {
            _player.Release();
            _camera.SetLookEnabled(_player.IsControlEnabled);
        }
    }
}
