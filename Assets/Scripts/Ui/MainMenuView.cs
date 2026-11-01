using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlipUp
{
    public class MainMenuView : MonoBehaviour
    {
        private const string LastRunMissing = "Последняя игра: —";
        private const string LastRunFormat = "Последняя игра: {0}";
        private const string LastRunStampFormat = "dd.MM.yyyy, HH:mm:ss";
        private const string LastRunTodaySuffix = " (сегодня)";
        private const string LastRunYesterdaySuffix = " (вчера)";
        private const string ControlsHint = "WASD — движение\nПробел — прыжок, ещё раз в воздухе\nF — рывок";

        [SerializeField] private Button _playButton;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private TextMeshProUGUI _lastRunLabel;
        [SerializeField] private TextMeshProUGUI _controlsLabel;
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
            ShowControls();
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
            _controlsLabel = null;
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

        private void ShowControls()
        {
            _controlsLabel.text = ControlsHint;
        }

        private static string FormatLastPlayed(DateTime lastPlayedUtc)
        {
            DateTime lastPlayedLocal = lastPlayedUtc.ToUniversalTime().ToLocalTime();
            DateTime nowLocal = DateTime.UtcNow.ToLocalTime();
            int dayCount = (nowLocal.Date - lastPlayedLocal.Date).Days;
            string stamp = lastPlayedLocal.ToString(LastRunStampFormat, CultureInfo.InvariantCulture);
            string text = string.Format(LastRunFormat, stamp);

            if (dayCount == 0)
            {
                return text + LastRunTodaySuffix;
            }

            if (dayCount == 1)
            {
                return text + LastRunYesterdaySuffix;
            }

            return text;
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
