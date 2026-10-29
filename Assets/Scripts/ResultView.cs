using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlipUp
{
    public class ResultView : MonoBehaviour
    {
        private const string FinishTitle = "Финиш";
        private const string FallTitle = "Упал";
        private const string TimeFormat = "Время: {0:0.00}";
        private const string BestTimeFormat = "Лучшее: {0:0.00}";
        private const string BestTimeMissing = "Лучшее: —";

        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _timeLabel;
        [SerializeField] private TextMeshProUGUI _bestTimeLabel;
        [SerializeField] private Button _againButton;
        [SerializeField] private Button _menuButton;

        private RunFlow _runFlow;

        internal void Initialize(RunFlow runFlow)
        {
            _runFlow = runFlow;
        }

        internal void ShowFinish(float time, bool hasBestTime, float bestTime)
        {
            _titleLabel.text = FinishTitle;
            ShowTimes(time, hasBestTime, bestTime);
        }

        internal void ShowFall(float time, bool hasBestTime, float bestTime)
        {
            _titleLabel.text = FallTitle;
            ShowTimes(time, hasBestTime, bestTime);
        }

        private void OnEnable()
        {
            _againButton.onClick.AddListener(OnAgainClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDisable()
        {
            _againButton.onClick.RemoveListener(OnAgainClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            _runFlow = null;
        }

        private void ShowTimes(float time, bool hasBestTime, float bestTime)
        {
            _timeLabel.text = string.Format(TimeFormat, time);

            if (hasBestTime)
            {
                _bestTimeLabel.text = string.Format(BestTimeFormat, bestTime);
                return;
            }

            _bestTimeLabel.text = BestTimeMissing;
        }

        private void OnAgainClicked()
        {
            _runFlow.Play();
        }

        private void OnMenuClicked()
        {
            _runFlow.OpenMenu();
        }
    }
}
