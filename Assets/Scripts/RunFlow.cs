using System.Collections;
using UnityEngine;

namespace SlipUp
{
    public class RunFlow : MonoBehaviour
    {
        private const float CountStepDuration = 1f;
        private const string CountThree = "3";
        private const string CountTwo = "2";
        private const string CountOne = "1";
        private const string StartMessage = "Поехали";

        [SerializeField] private Level _level;
        [SerializeField] private ScoreView _scoreView;

        private float _startTime;
        private bool _isRunning;
        private bool _isFinished;
        private WaitForSeconds _countStep;
        private Coroutine _countdown;

        private void Awake()
        {
            _countStep = new WaitForSeconds(CountStepDuration);
        }

        private void Start()
        {
            _isRunning = false;
            _isFinished = false;
            _scoreView.SetTime(0f);
            _countdown = StartCoroutine(Countdown());
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
            if (_countdown != null)
            {
                StopCoroutine(_countdown);
                _countdown = null;
            }

            _level = null;
            _scoreView = null;
            _countStep = null;
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

        private IEnumerator Countdown()
        {
            _scoreView.SetMessage(CountThree);
            yield return _countStep;
            _scoreView.SetMessage(CountTwo);
            yield return _countStep;
            _scoreView.SetMessage(CountOne);
            yield return _countStep;
            _scoreView.SetMessage(StartMessage);
            BeginRun();
            yield return _countStep;
            _scoreView.SetMessage(string.Empty);
            _countdown = null;
        }

        private void BeginRun()
        {
            if (_isFinished)
            {
                return;
            }

            _startTime = Time.time;
            _isRunning = true;
            _level.BeginRun();
            ShowElapsed();
        }
    }
}
