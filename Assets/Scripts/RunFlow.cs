using UnityEngine;

namespace SlipUp
{
    public class RunFlow : MonoBehaviour
    {
        [SerializeField] private Level _level;
        [SerializeField] private ScoreView _scoreView;

        private float _startTime;
        private bool _isRunning;
        private bool _isFinished;

        private void Start()
        {
            _startTime = Time.time;
            _isRunning = true;
            _isFinished = false;
            ShowElapsed();
        }

        private void Update()
        {
            if (_isRunning == false)
            {
                return;
            }

            ShowElapsed();
        }

        private void OnDestroy()
        {
            _level = null;
            _scoreView = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isFinished)
            {
                return;
            }

            if (_level.IsPlayer(other.gameObject) == false)
            {
                return;
            }

            _isFinished = true;
            _isRunning = false;
            ShowElapsed();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private void ShowElapsed()
        {
            _scoreView.SetTime(Time.time - _startTime);
        }
    }
}
