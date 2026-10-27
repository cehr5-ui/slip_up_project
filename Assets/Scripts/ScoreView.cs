using TMPro;
using UnityEngine;

namespace SlipUp
{
    public class ScoreView : MonoBehaviour
    {
        private const string ScoreFormat = "Монеты: {0}";
        private const string TimeFormat = "Время: {0:0.00}";

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private TextMeshProUGUI _timeLabel;

        public void SetScore(int score)
        {
            _label.text = string.Format(ScoreFormat, score);
        }

        internal void SetTime(float time)
        {
            _timeLabel.text = string.Format(TimeFormat, time);
        }
    }
}
