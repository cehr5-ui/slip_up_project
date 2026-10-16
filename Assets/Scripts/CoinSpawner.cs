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
    }
}
