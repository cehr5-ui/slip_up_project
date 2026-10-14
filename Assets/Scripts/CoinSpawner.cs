using UnityEngine;

namespace SlipUp
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private Transform[] _points;

        private void Start()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                Transform point = _points[i];
                Instantiate(_coinPrefab, point.position, point.rotation);
            }
        }
    }
}
