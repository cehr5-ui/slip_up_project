using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SlipUp
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private Coin _coinPrefab;
        [SerializeField] private Transform[] _points;

        private readonly List<Coin> _coins = new List<Coin>();
        private ObjectPool<Coin> _pool;
        private Transform _point;

        public int CoinCount => _points.Length;

        private void Awake()
        {
            _pool = new ObjectPool<Coin>(CreateCoin, OnGetCoin, OnReleaseCoin, null, true, _points.Length);
        }

        private void Start()
        {
            RespawnCoins();
        }

        private void OnDestroy()
        {
            if (_pool == null)
            {
                return;
            }

            _pool.Dispose();
            _pool = null;
            _point = null;
            _coins.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.84f, 0f, 0.9f);

            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = _points[i];
                Vector3 markerPosition = point.position + Vector3.up * 0.5f;
                Gizmos.DrawSphere(markerPosition, 0.25f);
                Gizmos.DrawLine(point.position, markerPosition);
            }
        }

        public void RespawnCoins()
        {
            ReleaseCoins();
            SpawnCoins();
        }

        private void SpawnCoins()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                _point = _points[i];
                _pool.Get();
            }
        }

        private void ReleaseCoins()
        {
            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                _pool.Release(_coins[i]);
            }
        }

        private Coin CreateCoin()
        {
            Coin coin = Instantiate(_coinPrefab, transform);
            coin.Initialize(_pool);
            coin.gameObject.SetActive(false);
            return coin;
        }

        private void OnGetCoin(Coin coin)
        {
            Transform point = _point;
            coin.transform.SetPositionAndRotation(point.position, point.rotation);
            _coins.Add(coin);
            coin.gameObject.SetActive(true);
        }

        private void OnReleaseCoin(Coin coin)
        {
            _coins.Remove(coin);
            coin.gameObject.SetActive(false);
        }
    }
}
