using UnityEngine;

namespace SlipUp
{
    public class Arena : MonoBehaviour
    {
        private const float SpawnMarkerRadius = 0.5f;

        [SerializeField] private Collider _location;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private float _outsideMargin = 8f;

        internal Vector3 StartPosition => _spawnPoint.position;

        internal Quaternion StartRotation => _spawnPoint.rotation;

        private void OnDestroy()
        {
            _location = null;
            _spawnPoint = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_spawnPoint.position, SpawnMarkerRadius);
        }

        internal bool IsBelowArena(Vector3 position)
        {
            return position.y < _location.bounds.min.y - _outsideMargin;
        }
    }
}
