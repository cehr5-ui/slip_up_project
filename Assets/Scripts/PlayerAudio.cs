using UnityEngine;

namespace SlipUp
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _collectClip;
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private float _collectVolume = 0.4f;

        public void PlayCollect()
        {
            _audioSource.PlayOneShot(_collectClip, _collectVolume);
        }

        public void PlayHit()
        {
            _audioSource.PlayOneShot(_hitClip);
        }
    }
}
