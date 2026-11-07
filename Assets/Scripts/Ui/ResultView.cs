using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlipUp
{
    public class ResultView : MonoBehaviour
    {
        private const string FinishTitle = "Финиш";
        private const string FallTitle = "Fail";
        private const string TimeFormat = "Время: {0:00}:{1:00}.{2:000}";
        private const string BestTimeFormat = "Лучшее: {0:00}:{1:00}.{2:000}";
        private const string BestTimeMissing = "Лучшее: —";

        internal event Action AgainPressed;
        internal event Action MenuPressed;

        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _timeLabel;
        [SerializeField] private TextMeshProUGUI _bestTimeLabel;
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _menuButton;

        private RunModel _model;

        internal void Initialize(RunModel model)
        {
            _model = model;
        }

        private void OnEnable()
        {
            _againButton.onClick.AddListener(OnAgainClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
            ShowCurrent();
        }

        private void OnDisable()
        {
            _againButton.onClick.RemoveListener(OnAgainClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            AgainPressed = null;
            MenuPressed = null;
            _model = null;
        }

        private void ShowCurrent()
        {
            if (_model.IsFinish)
            {
                _titleLabel.text = FinishTitle;
            }
            else
            {
                _titleLabel.text = FallTitle;
            }

            ShowTimes(_model.Time, _model.HasBestTime, _model.BestTime);
        }

        private void ShowTimes(TimeSpan duration, bool hasBestTime, float bestTime)
        {
            _timeLabel.text = FormatDuration(TimeFormat, duration);

            if (hasBestTime)
            {
                TimeSpan bestDuration = TimeSpan.FromSeconds(bestTime);
                _bestTimeLabel.text = FormatDuration(BestTimeFormat, bestDuration);
                return;
            }

            _bestTimeLabel.text = BestTimeMissing;
        }

        private static string FormatDuration(string format, TimeSpan duration)
        {
            int minutes = (int)duration.TotalMinutes;
            int seconds = duration.Seconds;
            int milliseconds = duration.Milliseconds;
            return string.Format(format, minutes, seconds, milliseconds);
        }

        private void OnAgainClicked()
        {
            AgainPressed?.Invoke();
        }

        private void OnMenuClicked()
        {
            MenuPressed?.Invoke();
        }
    }
}
