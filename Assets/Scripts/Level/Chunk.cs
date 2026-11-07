using UnityEngine;

namespace SlipUp
{
    public class Chunk : MonoBehaviour
    {
        private const float MarkerRadius = 0.5f;

        [SerializeField] private Transform _entry;
        [SerializeField] private Transform _exit;
        [SerializeField] private CoinField _coinField;
        [SerializeField] private FinishGate _finishGate;

        private Track _track;

        internal Transform Entry => _entry;

        internal Transform Exit => _exit;

        internal void Initialize(Track track)
        {
            _track = track;

            if (_finishGate == null)
            {
                return;
            }

            _finishGate.Initialize(_track);
        }

        internal void Restore()
        {
            if (_coinField == null)
            {
                return;
            }

            _coinField.Restore();
        }

        private void OnDestroy()
        {
            _track = null;
            _entry = null;
            _exit = null;
            _coinField = null;
            _finishGate = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_entry.position, MarkerRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_exit.position, MarkerRadius);
        }
    }
}
