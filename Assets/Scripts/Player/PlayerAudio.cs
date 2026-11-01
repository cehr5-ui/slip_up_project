using UnityEngine;

namespace SlipUp
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private AudioService _audio;
        [SerializeField] private AudioClip _collectClip;
        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private float _collectVolume = 0.4f;

        private void OnDestroy()
        {
            _audio = null;
        }

        public void PlayCollect()
        {
            _audio.PlaySfx(_collectClip, _collectVolume);
        }

        public void PlayHit()
        {
            _audio.PlaySfx(_hitClip);
        }
    }
}
