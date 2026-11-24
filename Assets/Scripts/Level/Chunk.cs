using UnityEngine;

namespace SlipUp
{
    public class Chunk : MonoBehaviour
    {
        private const float MarkerRadius = 0.5f;
        private const float ForwardLength = 2f;

        [SerializeField] private Transform _entry;
        [SerializeField] private Transform _exit;
        [SerializeField] private ConnectorType _entryType = ConnectorType.Straight8;
        [SerializeField] private ConnectorType _exitType = ConnectorType.Straight8;
        [SerializeField] private CoinField _coinField;
        [SerializeField] private FinishGate _finishGate;
        [SerializeField] private Transform _spawnPoint;

        private Track _track;

        internal Transform Entry => _entry;

        internal Transform Exit => _exit;

        internal ConnectorType EntryType => _entryType;

        internal ConnectorType ExitType => _exitType;

        internal Transform SpawnPoint => _spawnPoint;

        internal void Bind(
            Transform entry,
            Transform exit,
            ConnectorType entryType,
            ConnectorType exitType,
            FinishGate finishGate = null,
            Transform spawnPoint = null)
        {
            _entry = entry;
            _exit = exit;
            _entryType = entryType;
            _exitType = exitType;
            _finishGate = finishGate;
            _spawnPoint = spawnPoint;
        }

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
            _spawnPoint = null;
        }

        private void OnDrawGizmos()
        {
            DrawConnector(_entry, Color.green);
            DrawConnector(_exit, Color.blue);
        }

        private static void DrawConnector(Transform connector, Color color)
        {
            if (connector == null)
            {
                return;
            }

            Gizmos.color = color;
            Gizmos.DrawSphere(connector.position, MarkerRadius);
            Gizmos.DrawLine(connector.position, connector.position + connector.forward * ForwardLength);
        }
    }
}
