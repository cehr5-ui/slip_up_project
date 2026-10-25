using UnityEngine;

namespace SlipUp
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _collectClip;
        [SerializeField] private AudioClip _hitClip;

        public void PlayCollect()
        {
            _audioSource.PlayOneShot(_collectClip);
        }

        public void PlayHit()
        {
            _audioSource.PlayOneShot(_hitClip);
        }
    }
}
