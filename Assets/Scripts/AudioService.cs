using UnityEngine;

namespace SlipUp
{
    public class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        private float _sfxVolume;
        private AudioClip _musicClip;
        private bool _isEnabled;

        private void Awake()
        {
            _sfxVolume = _sfxSource.volume;
            _musicClip = _musicSource.clip;
            SaveService save = new SaveService();
            SetEnabled(save.IsSoundEnabled);
        }

        private void OnDestroy()
        {
            _sfxSource = null;
            _musicSource = null;
            _musicClip = null;
        }

        internal void PlaySfx(AudioClip clip, float volume = 1f)
        {
            _sfxSource.PlayOneShot(clip, volume);
        }

        internal void PlayMusic(AudioClip clip)
        {
            _musicClip = clip;
            _musicSource.clip = clip;
            _musicSource.loop = true;

            if (_isEnabled == false)
            {
                _musicSource.Stop();
                return;
            }

            _musicSource.Play();
        }

        internal void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;

            if (isEnabled)
            {
                _sfxSource.volume = _sfxVolume;
                PlayStoredMusic();
                return;
            }

            _sfxSource.volume = 0f;
            _musicSource.Stop();
        }

        private void PlayStoredMusic()
        {
            if (_musicClip == null)
            {
                return;
            }

            if (_musicSource.isPlaying)
            {
                return;
            }

            _musicSource.clip = _musicClip;
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }
}
