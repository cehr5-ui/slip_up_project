using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlipUp
{
    public class MainMenuView : MonoBehaviour
    {
        private const string LastRunMissing = "Последняя игра: —";
        private const string LastRunFormat = "Последняя игра: {0:dd.MM.yyyy HH:mm}";

        [SerializeField] private Button _playButton;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private TextMeshProUGUI _lastRunLabel;

        private RunFlow _runFlow;
        private SaveService _save;

        internal void Initialize(RunFlow runFlow)
        {
            _runFlow = runFlow;
        }

        private void Awake()
        {
            _save = new SaveService();
            bool isSoundEnabled = _save.IsSoundEnabled;
            _soundToggle.SetIsOnWithoutNotify(isSoundEnabled);
            ApplySound(isSoundEnabled);
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _soundToggle.onValueChanged.AddListener(OnSoundChanged);
            ShowLastGameDate();
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _soundToggle.onValueChanged.RemoveListener(OnSoundChanged);
        }

        private void OnDestroy()
        {
            _runFlow = null;
            _save = null;
        }

        private void ShowLastGameDate()
        {
            if (_save.TryGetLastGameDate(out DateTime lastGameDate))
            {
                _lastRunLabel.text = string.Format(LastRunFormat, lastGameDate);
                return;
            }

            _lastRunLabel.text = LastRunMissing;
        }

        private void ApplySound(bool isEnabled)
        {
            AudioListener.volume = isEnabled ? 1f : 0f;
        }

        private void OnPlayClicked()
        {
            _runFlow.Play();
        }

        private void OnSoundChanged(bool isEnabled)
        {
            _save.SetIsSoundEnabled(isEnabled);
            ApplySound(isEnabled);
        }
    }
}
