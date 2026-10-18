using TMPro;
using UnityEngine;

namespace SlipUp
{
    public class ScoreView : MonoBehaviour
    {
        private const string ScoreFormat = "Монеты: {0}";
        private const string ClearedText = "Арена зачищена";

        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private TextMeshProUGUI _clearedLabel;

        private void Awake()
        {
            _clearedLabel.text = ClearedText;
            _clearedLabel.gameObject.SetActive(false);
        }

        public void SetScore(int score)
        {
            _label.text = string.Format(ScoreFormat, score);
        }

        public void SetCleared(bool isCleared)
        {
            _clearedLabel.gameObject.SetActive(isCleared);
        }
    }
}
