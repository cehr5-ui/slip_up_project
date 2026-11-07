using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace SlipUp
{
    public class HudView : MonoBehaviour
    {
        private const string ScoreFormat = "Монеты: {0}";
        private const string TimeFormat = "Время: {0:00}:{1:00}";
        private const string DashReadyText = "Рывок";
        private const string DashCooldownFormat = "Рывок {0:0.0}";
        private const string LivesFormat = "Жизни: {0}";

        private static readonly Color DashReadyColor = Color.white;
        private static readonly Color DashCooldownColor = new Color(1f, 1f, 1f, 0.35f);

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private TextMeshProUGUI _timeLabel;
        [SerializeField] private TextMeshProUGUI _messageLabel;
        [SerializeField] private TextMeshProUGUI _dashLabel;
        [SerializeField] private TextMeshProUGUI _livesLabel;
        [SerializeField] private float _punchStrength = 0.3f;
        [SerializeField] private float _punchDuration = 0.35f;

        private RunModel _model;
        private RectTransform _labelTransform;
        private Vector3 _labelScale;
        private Tween _punch;

        internal void Initialize(RunModel model)
        {
            _model = model;
        }

        private void Awake()
        {
            _labelTransform = _label.rectTransform;
            _labelScale = _labelTransform.localScale;
        }

        private void OnEnable()
        {
            _model.ScoreChanged += OnScoreChanged;
            _model.TimeChanged += OnTimeChanged;
            _model.LivesChanged += OnLivesChanged;
            _model.DashChanged += OnDashChanged;
            _model.MessageChanged += OnMessageChanged;
            _model.RunnerPresenceChanged += OnRunnerPresenceChanged;
            Refresh();
        }

        private void OnDisable()
        {
            _model.ScoreChanged -= OnScoreChanged;
            _model.TimeChanged -= OnTimeChanged;
            _model.LivesChanged -= OnLivesChanged;
            _model.DashChanged -= OnDashChanged;
            _model.MessageChanged -= OnMessageChanged;
            _model.RunnerPresenceChanged -= OnRunnerPresenceChanged;

            if (_punch != null)
            {
                _punch.Kill();
                _punch = null;
            }

            _labelTransform.localScale = _labelScale;
        }

        private void OnDestroy()
        {
            _model = null;
        }

        private void Refresh()
        {
            ApplyScore(_model.Score);
            ApplyTime(_model.Time);
            ApplyLives(_model.Lives);
            ApplyMessage(_model.Message);
            ApplyDash(_model.IsDashReady, _model.DashCooldownLeft);
        }

        private void OnScoreChanged(int score)
        {
            ApplyScore(score);
            Punch();
        }

        private void OnTimeChanged(TimeSpan duration)
        {
            ApplyTime(duration);
        }

        private void OnLivesChanged(int livesLeft)
        {
            ApplyLives(livesLeft);
        }

        private void OnDashChanged(bool isReady, float cooldownLeft)
        {
            ApplyDash(isReady, cooldownLeft);
        }

        private void OnMessageChanged(string message)
        {
            ApplyMessage(message);
        }

        private void OnRunnerPresenceChanged()
        {
            ApplyDash(_model.IsDashReady, _model.DashCooldownLeft);
        }

        private void ApplyScore(int score)
        {
            _label.text = string.Format(ScoreFormat, score);
        }

        private void ApplyTime(TimeSpan duration)
        {
            int minutes = (int)duration.TotalMinutes;
            int seconds = duration.Seconds;
            _timeLabel.text = string.Format(TimeFormat, minutes, seconds);
        }

        private void ApplyMessage(string message)
        {
            _messageLabel.text = message;
        }

        private void ApplyLives(int livesLeft)
        {
            _livesLabel.text = string.Format(LivesFormat, livesLeft);
        }

        private void ApplyDash(bool isReady, float cooldownLeft)
        {
            if (_model.HasRunner == false)
            {
                _dashLabel.text = DashReadyText;
                _dashLabel.color = DashCooldownColor;
                return;
            }

            if (isReady)
            {
                _dashLabel.text = DashReadyText;
                _dashLabel.color = DashReadyColor;
                return;
            }

            _dashLabel.text = string.Format(DashCooldownFormat, cooldownLeft);
            _dashLabel.color = DashCooldownColor;
        }

        private void Punch()
        {
            if (_punch != null)
            {
                _punch.Kill();
                _punch = null;
            }

            _labelTransform.localScale = _labelScale;
            _punch = _labelTransform.DOPunchScale(new Vector3(_punchStrength, _punchStrength, 0f), _punchDuration);
        }
    }
}
