using System.Collections.Generic;
using UnityEngine;

namespace SlipUp
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private Transform[] _points;

        private static readonly List<CoinSpawner> _spawners = new List<CoinSpawner>();
        private readonly List<GameObject> _coins = new List<GameObject>();

        public static int CoinCount
        {
            get
            {
                int coinCount = 0;

                for (int i = 0; i < _spawners.Count; i++)
                {
                    coinCount += _spawners[i]._points.Length;
                }

                return coinCount;
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            _spawners.Add(this);
        }

        private void OnDisable()
        {
            _spawners.Remove(this);
        }

        private void Start()
        {
            SpawnCoins();
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

        public static void RespawnAll()
        {
            for (int i = 0; i < _spawners.Count; i++)
            {
                _spawners[i].RespawnCoins();
            }
        }

        public void RespawnCoins()
        {
            ClearCoins();
            SpawnCoins();
        }

        private void SpawnCoins()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = _points[i];
                GameObject coin = Instantiate(_coinPrefab, point.position, point.rotation, transform);
                _coins.Add(coin);
            }
        }

        private void ClearCoins()
        {
            for (int i = 0; i < _coins.Count; i++)
            {
                GameObject coin = _coins[i];

                if (coin == null)
                {
                    continue;
                }

                coin.SetActive(false);
                Destroy(coin);
            }

            _coins.Clear();
        }
    }
}
