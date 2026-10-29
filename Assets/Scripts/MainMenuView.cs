using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlipUp
{
    public class MainMenuView : MonoBehaviour
    {
        private const string LastRunText = "Последняя игра: —";

        [SerializeField] private Button _playButton;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private TextMeshProUGUI _lastRunLabel;

        private RunFlow _runFlow;

        internal void Initialize(RunFlow runFlow)
        {
            _runFlow = runFlow;
            _lastRunLabel.text = LastRunText;
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _soundToggle.onValueChanged.AddListener(OnSoundChanged);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _soundToggle.onValueChanged.RemoveListener(OnSoundChanged);
        }

        private void OnDestroy()
        {
            _runFlow = null;
        }

        private void OnPlayClicked()
        {
            _runFlow.Play();
        }

        private void OnSoundChanged(bool isEnabled)
        {
            AudioListener.volume = isEnabled ? 1f : 0f;
        }
    }
}
