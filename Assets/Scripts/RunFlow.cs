using System;
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
        [SerializeField] private ScreenService _screens;
        [SerializeField] private MainMenuView _menu;
        [SerializeField] private ResultView _result;

        private DateTime _runStartedAt;
        private float _bestTime;
        private bool _hasBestTime;
        private bool _isRunning;
        private bool _isFinished;
        private WaitForSeconds _countStep;
        private Coroutine _countdown;
        private SaveService _save;

        internal void Play()
        {
            if (_countdown != null)
            {
                return;
            }

            if (_isRunning)
            {
                return;
            }

            _isFinished = false;
            _isRunning = false;
            _screens.Show(ScreenService.ScreenId.Hud);
            _level.PrepareAttempt();
            _scoreView.SetTime(TimeSpan.Zero);
            _scoreView.SetMessage(string.Empty);
            _countdown = StartCoroutine(Countdown());
        }

        internal void OpenMenu()
        {
            if (_countdown != null)
            {
                StopCoroutine(_countdown);
                _countdown = null;
            }

            _isRunning = false;
            _isFinished = false;
            _level.PrepareAttempt();
            _scoreView.SetTime(TimeSpan.Zero);
            _scoreView.SetMessage(string.Empty);
            _screens.Show(ScreenService.ScreenId.Menu);
        }

        internal void OnFell()
        {
            if (_isFinished)
            {
                return;
            }

            if (_isRunning == false)
            {
                return;
            }

            _isFinished = true;
            _isRunning = false;
            TimeSpan duration = DateTime.UtcNow - _runStartedAt;
            _scoreView.SetTime(duration);
            _level.FreezePlayer();
            _level.PauseCollection();
            _result.ShowFall(duration, _hasBestTime, _bestTime);
            _screens.Show(ScreenService.ScreenId.Result);
        }

        private void Awake()
        {
            _save = new SaveService();
            _countStep = new WaitForSeconds(CountStepDuration);
            _hasBestTime = _save.TryGetBestTime(out _bestTime);
            _menu.Initialize(this);
            _result.Initialize(this);
        }

        private void Start()
        {
            _isRunning = false;
            _isFinished = false;
            _level.FreezePlayer();
            _level.LockCameraToPlayer();
            _level.PauseCollection();
            _scoreView.SetTime(TimeSpan.Zero);
            _scoreView.SetMessage(string.Empty);
            _screens.Show(ScreenService.ScreenId.Menu);
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
            _screens = null;
            _menu = null;
            _result = null;
            _countStep = null;
            _save = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isFinished)
            {
                return;
            }

            if (_isRunning == false)
            {
                return;
            }

            if (_level.IsPlayer(other.gameObject) == false)
            {
                return;
            }

            _isFinished = true;
            _isRunning = false;
            TimeSpan duration = DateTime.UtcNow - _runStartedAt;
            _scoreView.SetTime(duration);
            RememberBest(duration);
            _level.FreezePlayer();
            _level.PauseCollection();
            _result.ShowFinish(duration, _hasBestTime, _bestTime);
            _screens.Show(ScreenService.ScreenId.Result);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private void ShowElapsed()
        {
            TimeSpan duration = DateTime.UtcNow - _runStartedAt;
            _scoreView.SetTime(duration);
        }

        private void RememberBest(TimeSpan duration)
        {
            float time = (float)duration.TotalSeconds;

            if (_hasBestTime && time >= _bestTime)
            {
                return;
            }

            _bestTime = time;
            _hasBestTime = true;
            _save.SetBestTime(time);
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
            StartRunning();
            yield return _countStep;
            _scoreView.SetMessage(string.Empty);
            _countdown = null;
        }

        private void StartRunning()
        {
            if (_isFinished)
            {
                return;
            }

            _runStartedAt = DateTime.UtcNow;
            _isRunning = true;
            _save.SetLastPlayedUtc(_runStartedAt);
            _level.BeginAttempt();
            ShowElapsed();
        }
    }
}
