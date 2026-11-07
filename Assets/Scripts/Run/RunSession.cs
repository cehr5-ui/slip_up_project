using System;
using System.Collections;
using UnityEngine;

namespace SlipUp
{
    public class RunSession : MonoBehaviour
    {
        private const int FallLimit = 3;
        private const float CountStepDuration = 1f;
        private const float FallFreezeDuration = 1f;
        private const int CountdownFrom = 3;
        private const string StartMessage = "Поехали";

        [SerializeField] private Track _track;
        [SerializeField] private Arena _arena;
        [SerializeField] private RunnerSpawner _spawner;
        [SerializeField] private CameraFollow _camera;
        [SerializeField] private ScreenRouter _screens;

        private RunModel _model;
        private SaveService _save;
        private Runner _runner;
        private DateTime _runStartedAt;
        private float _bestTime;
        private bool _hasBestTime;
        private int _fallCount;
        private bool _isRunning;
        private bool _isFinished;
        private bool _isRecovering;
        private WaitForSeconds _countStep;
        private WaitForSeconds _fallFreeze;
        private Coroutine _countdown;
        private Coroutine _fallRecover;

        internal void Initialize(RunModel model, SaveService save)
        {
            _model = model;
            _save = save;
            _countStep = new WaitForSeconds(CountStepDuration);
            _fallFreeze = new WaitForSeconds(FallFreezeDuration);
            _hasBestTime = _save.TryGetBestTime(out _bestTime);
            _track.Initialize();
            _track.FinishReached += OnFinishReached;
        }

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

            StopFallRecover();
            _fallCount = 0;
            _isFinished = false;
            _isRunning = false;
            SetPhase(RunPhase.Countdown);
            PrepareAttempt();
            _countdown = StartCoroutine(Countdown());
        }

        internal void OpenMenu()
        {
            if (_countdown != null)
            {
                StopCoroutine(_countdown);
                _countdown = null;
            }

            StopFallRecover();
            _fallCount = 0;
            _isRunning = false;
            _isFinished = false;
            PrepareAttempt();
            SetPhase(RunPhase.Menu);
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

            StopFallRecover();
            DetachRunner();

            if (_track != null)
            {
                _track.FinishReached -= OnFinishReached;
                _track = null;
            }

            _arena = null;
            _spawner = null;
            _camera = null;
            _screens = null;
            _model = null;
            _save = null;
            _countStep = null;
            _fallFreeze = null;
        }

        private void PrepareAttempt()
        {
            _model.SetScore(0);
            _model.SetTime(TimeSpan.Zero);
            _model.SetMessage(string.Empty);
            ShowLives();
            ReplaceRunner(true);
            _runner.PauseCollection();
            _track.Restore();
        }

        private void ReplaceRunner(bool isFrozen)
        {
            DetachRunner();
            Runner runner = _spawner.Spawn(_arena.StartPosition, _arena.StartRotation);
            AttachRunner(runner);
            runner.gameObject.SetActive(true);

            if (isFrozen == false)
            {
                return;
            }

            _runner.Freeze();
            _camera.SetLookEnabled(false);
            _camera.LockToPlayer();
        }

        private void AttachRunner(Runner runner)
        {
            _runner = runner;
            _runner.Released += OnRunnerReleased;
            _runner.Fell += OnRunnerFell;
            _runner.Initialize(_model, _arena, _camera.transform);
            _camera.SetTarget(_runner.Focus);
            _model.SetRunnerPresent(true);
        }

        private void DetachRunner()
        {
            if (_runner == null)
            {
                _camera.ClearTarget();
                _model.SetRunnerPresent(false);
                return;
            }

            Runner runner = _runner;
            _runner = null;
            runner.Released -= OnRunnerReleased;
            runner.Fell -= OnRunnerFell;
            _camera.ClearTarget();
            _model.SetRunnerPresent(false);
        }

        private void ShowElapsed()
        {
            TimeSpan duration = DateTime.UtcNow - _runStartedAt;
            _model.SetTime(duration);
        }

        private void ShowLives()
        {
            _model.SetLives(FallLimit - _fallCount);
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

        private void FailRun()
        {
            StopFallRecover();
            _isFinished = true;
            _isRunning = false;
            TimeSpan duration = DateTime.UtcNow - _runStartedAt;
            _model.SetTime(duration);
            _model.SetResult(false, _hasBestTime, _bestTime);
            FreezeRunner();
            SetPhase(RunPhase.Result);
        }

        private void FinishRun()
        {
            _isFinished = true;
            _isRunning = false;
            TimeSpan duration = DateTime.UtcNow - _runStartedAt;
            _model.SetTime(duration);
            RememberBest(duration);
            _model.SetResult(true, _hasBestTime, _bestTime);
            FreezeRunner();
            SetPhase(RunPhase.Result);
        }

        private void FreezeRunner()
        {
            _camera.SetLookEnabled(false);

            if (_runner == null)
            {
                return;
            }

            _runner.Freeze();
            _runner.PauseCollection();
        }

        private void StopFallRecover()
        {
            _isRecovering = false;

            if (_fallRecover == null)
            {
                return;
            }

            StopCoroutine(_fallRecover);
            _fallRecover = null;
        }

        private void SetPhase(RunPhase phase)
        {
            _model.SetPhase(phase);
            _screens.Show(phase);
        }

        private IEnumerator RecoverAfterFall()
        {
            ReplaceRunner(true);
            _runner.PauseCollection();
            SetPhase(RunPhase.Recover);
            yield return _fallFreeze;

            if (_isFinished)
            {
                _fallRecover = null;
                _isRecovering = false;
                yield break;
            }

            if (_runner == null)
            {
                ReplaceRunner(false);
            }

            _runner.ResumeCollection();
            _runner.BeginRun();
            _runner.Release();
            _camera.UnlockFollow();
            _camera.SetLookEnabled(_runner.IsControlEnabled);
            SetPhase(RunPhase.Running);
            _fallRecover = null;
            _isRecovering = false;
        }

        private IEnumerator Countdown()
        {
            for (int count = CountdownFrom; count > 0; count--)
            {
                _model.SetMessage(count.ToString());
                yield return _countStep;
            }

            _model.SetMessage(StartMessage);
            StartRunning();
            yield return _countStep;
            _model.SetMessage(string.Empty);
            _countdown = null;
        }

        private void StartRunning()
        {
            if (_isFinished)
            {
                return;
            }

            if (_runner == null)
            {
                ReplaceRunner(true);
            }

            _runStartedAt = DateTime.UtcNow;
            _isRunning = true;
            _save.SetLastPlayedUtc(_runStartedAt);
            _runner.ResumeCollection();
            _runner.BeginRun();
            _runner.Release();
            _camera.UnlockFollow();
            _camera.SetLookEnabled(_runner.IsControlEnabled);
            SetPhase(RunPhase.Running);
            ShowElapsed();
        }

        private void OnRunnerFell(Runner runner)
        {
            if (_runner != runner)
            {
                return;
            }

            if (_isFinished)
            {
                return;
            }

            if (_isRunning == false || _isRecovering)
            {
                return;
            }

            _fallCount++;
            ShowLives();
            FreezeRunner();

            if (_fallCount >= FallLimit)
            {
                FailRun();
                return;
            }

            _isRecovering = true;
            _fallRecover = StartCoroutine(RecoverAfterFall());
        }

        private void OnRunnerReleased(Runner runner)
        {
            runner.Released -= OnRunnerReleased;
            runner.Fell -= OnRunnerFell;

            if (_runner != runner)
            {
                return;
            }

            _runner = null;
            _camera.ClearTarget();
            _model.SetRunnerPresent(false);
        }

        private void OnFinishReached()
        {
            if (_isFinished)
            {
                return;
            }

            if (_isRunning == false)
            {
                return;
            }

            FinishRun();
        }
    }
}
