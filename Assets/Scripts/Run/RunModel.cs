using System;

namespace SlipUp
{
    public sealed class RunModel
    {
        internal event Action<int> ScoreChanged;
        internal event Action<TimeSpan> TimeChanged;
        internal event Action<int> LivesChanged;
        internal event Action<bool, float> DashChanged;
        internal event Action<string> MessageChanged;
        internal event Action RunnerPresenceChanged;

        private int _score;
        private TimeSpan _time;
        private int _lives;
        private bool _isDashReady;
        private float _dashCooldownLeft;
        private string _message = string.Empty;
        private RunPhase _phase;
        private bool _hasRunner;
        private bool _isFinish;
        private bool _hasBestTime;
        private float _bestTime;

        internal int Score => _score;

        internal TimeSpan Time => _time;

        internal int Lives => _lives;

        internal bool IsDashReady => _isDashReady;

        internal float DashCooldownLeft => _dashCooldownLeft;

        internal string Message => _message;

        internal RunPhase Phase => _phase;

        internal bool HasRunner => _hasRunner;

        internal bool IsFinish => _isFinish;

        internal bool HasBestTime => _hasBestTime;

        internal float BestTime => _bestTime;

        internal void AddScore()
        {
            SetScore(_score + 1);
        }

        internal void SetScore(int score)
        {
            _score = score;
            ScoreChanged?.Invoke(_score);
        }

        internal void SetTime(TimeSpan time)
        {
            _time = time;
            TimeChanged?.Invoke(_time);
        }

        internal void SetLives(int lives)
        {
            _lives = lives;
            LivesChanged?.Invoke(_lives);
        }

        internal void SetDash(bool isReady, float cooldownLeft)
        {
            _isDashReady = isReady;
            _dashCooldownLeft = cooldownLeft;
            DashChanged?.Invoke(_isDashReady, _dashCooldownLeft);
        }

        internal void SetMessage(string message)
        {
            _message = message;
            MessageChanged?.Invoke(_message);
        }

        internal void SetPhase(RunPhase phase)
        {
            _phase = phase;
        }

        internal void SetRunnerPresent(bool hasRunner)
        {
            if (_hasRunner == hasRunner)
            {
                return;
            }

            _hasRunner = hasRunner;
            RunnerPresenceChanged?.Invoke();
        }

        internal void SetResult(bool isFinish, bool hasBestTime, float bestTime)
        {
            _isFinish = isFinish;
            _hasBestTime = hasBestTime;
            _bestTime = bestTime;
        }

        internal void Clear()
        {
            ScoreChanged = null;
            TimeChanged = null;
            LivesChanged = null;
            DashChanged = null;
            MessageChanged = null;
            RunnerPresenceChanged = null;
        }
    }
}
