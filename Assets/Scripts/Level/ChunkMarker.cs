using UnityEngine;

namespace SlipUp
{
    public class ChunkMarker : MonoBehaviour
    {
        private const float MarkerRadius = 0.5f;

        [SerializeField] private Transform _entry;
        [SerializeField] private Transform _exit;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_entry.position, MarkerRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_exit.position, MarkerRadius);
        }
    }
}
