using System.Collections.Generic;
using UnityEngine;

namespace SlipUp
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private Transform[] _points;

        private readonly List<GameObject> _coins = new List<GameObject>();

        public int CoinCount => _points.Length;

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
