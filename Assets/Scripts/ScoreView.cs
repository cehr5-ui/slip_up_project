using TMPro;
using UnityEngine;

namespace SlipUp
{
    public class ScoreView : MonoBehaviour
    {
        private const string ScoreFormat = "Монеты: {0}";

        [SerializeField] private TextMeshProUGUI _label;

        public void SetScore(int score)
        {
            _label.text = string.Format(ScoreFormat, score);
        }
    }
}
