using TMPro;
using UnityEngine;

namespace SlipUp
{
    public class ScoreView : MonoBehaviour
    {
        private const string ScoreFormat = "Монеты: {0}";
        private const string TimeFormat = "Время: {0:0.00}";
        private const string DashReadyText = "Рывок";
        private const string DashCooldownFormat = "Рывок {0:0.0}";

        private static readonly Color DashReadyColor = Color.white;
        private static readonly Color DashCooldownColor = new Color(1f, 1f, 1f, 0.35f);

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private TextMeshProUGUI _timeLabel;
        [SerializeField] private TextMeshProUGUI _messageLabel;
        [SerializeField] private TextMeshProUGUI _dashLabel;

        public void SetScore(int score)
        {
            _label.text = string.Format(ScoreFormat, score);
        }

        internal void SetTime(float time)
        {
            _timeLabel.text = string.Format(TimeFormat, time);
        }

        internal void SetMessage(string message)
        {
            _messageLabel.text = message;
        }

        internal void SetDash(bool isReady, float cooldownLeft)
        {
            if (isReady)
            {
                _dashLabel.text = DashReadyText;
                _dashLabel.color = DashReadyColor;
                return;
            }

            _dashLabel.text = string.Format(DashCooldownFormat, cooldownLeft);
            _dashLabel.color = DashCooldownColor;
        }
    }
}
