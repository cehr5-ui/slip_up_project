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
        private const string ControlsHint = "Слева — движение, справа — камера\nПрыжок и рывок — кнопки справа\nКлавиатура: WASD, пробел, F";

        internal event Action PlayPressed;

        [SerializeField] private Button _playButton;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private TextMeshProUGUI _lastRunLabel;
        [SerializeField] private TextMeshProUGUI _controlsLabel;
        [SerializeField] private AudioService _audio;

        private SaveService _save;
        private Button _skinButton;
        private Action _onPlay;

        internal void Initialize(SaveService save, AudioService audio, Action onPlay)
        {
            _save = save;
            _audio = audio;
            _onPlay = onPlay;
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _playButton.onClick.AddListener(OnPlayClicked);
            ApplyStoredSound();
            ShowLastPlayed();
        }

        private void Awake()
        {
            _skinButton = CreateSkinButton();
        }

        private void OnEnable()
        {
            if (_save != null)
            {
                ApplyStoredSound();
                ShowLastPlayed();
            }

            _playButton.onClick.AddListener(OnPlayClicked);
            _skinButton.onClick.AddListener(OnSkinClicked);
            _soundToggle.onValueChanged.AddListener(OnSoundChanged);
            ShowControls();
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _skinButton.onClick.RemoveListener(OnSkinClicked);
            _soundToggle.onValueChanged.RemoveListener(OnSoundChanged);
        }

        private void OnDestroy()
        {
            PlayPressed = null;
            _onPlay = null;
            _save = null;
            _controlsLabel = null;
            _audio = null;
            _skinButton = null;
        }

        private Button CreateSkinButton()
        {
            GameObject buttonObject = Instantiate(_playButton.gameObject, _playButton.transform.parent);
            buttonObject.name = "SkinButton";
            RectTransform playRect = _playButton.GetComponent<RectTransform>();
            RectTransform skinRect = buttonObject.GetComponent<RectTransform>();
            skinRect.anchoredPosition = playRect.anchoredPosition + new Vector2(0f, -90f);
            TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            label.text = "Скин";
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            return button;
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
            if (_onPlay != null)
            {
                _onPlay.Invoke();
                return;
            }

            PlayPressed?.Invoke();
        }

        private void OnSkinClicked()
        {
            CharacterSkin.Cycle();
        }

        private void OnSoundChanged(bool isEnabled)
        {
            _save.SetIsSoundEnabled(isEnabled);
            ApplySound(isEnabled);
        }
    }
}
