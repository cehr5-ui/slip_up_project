using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlipUp
{
    public class MainMenuView : MonoBehaviour
    {
        private const string LastRunMissing = "Последняя игра: —";
        private const string LastRunToday = "Последняя игра: сегодня";
        private const string LastRunYesterday = "Последняя игра: вчера";
        private const string LastRunDaysAgoFormat = "Последняя игра: {0} дн. назад";

        [SerializeField] private Button _playButton;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private TextMeshProUGUI _lastRunLabel;
        [SerializeField] private AudioService _audio;

        private RunFlow _runFlow;
        private SaveService _save;

        internal void Initialize(RunFlow runFlow)
        {
            _runFlow = runFlow;
        }

        private void Awake()
        {
            CreateSave();
            ApplyStoredSound();
        }

        private void OnEnable()
        {
            CreateSave();
            ApplyStoredSound();
            _playButton.onClick.AddListener(OnPlayClicked);
            _soundToggle.onValueChanged.AddListener(OnSoundChanged);
            ShowLastPlayed();
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
            _audio = null;
        }

        private void CreateSave()
        {
            if (_save != null)
            {
                return;
            }

            _save = new SaveService();
        }

        private void ApplyStoredSound()
        {
            bool isSoundEnabled = _save.IsSoundEnabled;
            _soundToggle.SetIsOnWithoutNotify(isSoundEnabled);
            ApplySound(isSoundEnabled);
        }

        private void ShowLastPlayed()
        {
            if (_save.TryGetLastPlayedUtc(out DateTime lastPlayedUtc) == false)
            {
                _lastRunLabel.text = LastRunMissing;
                return;
            }

            _lastRunLabel.text = FormatLastPlayed(lastPlayedUtc);
        }

        private static string FormatLastPlayed(DateTime lastPlayedUtc)
        {
            DateTime lastPlayedLocal = lastPlayedUtc.ToUniversalTime().ToLocalTime();
            DateTime nowLocal = DateTime.UtcNow.ToLocalTime();
            int dayCount = (nowLocal.Date - lastPlayedLocal.Date).Days;

            if (dayCount <= 0)
            {
                return LastRunToday;
            }

            if (dayCount == 1)
            {
                return LastRunYesterday;
            }

            return string.Format(LastRunDaysAgoFormat, dayCount);
        }

        private void ApplySound(bool isEnabled)
        {
            _audio.SetEnabled(isEnabled);
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
