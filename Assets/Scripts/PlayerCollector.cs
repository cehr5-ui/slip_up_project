using System.Collections;
using UnityEngine;

namespace SlipUp
{
    public class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private PlayerAudio _audio;
        [SerializeField] private float _flashDuration = 0.15f;

        private Level _level;
        private int _score;
        private Material _material;
        private Color _originalColor;
        private WaitForSeconds _flashDelay;
        private Coroutine _flash;
        private bool _isCollecting = true;

        public int Score => _score;

        internal void Initialize(Level level)
        {
            _level = level;
        }

        private void Awake()
        {
            _material = _meshRenderer.material;
            _originalColor = _material.color;
            _flashDelay = new WaitForSeconds(_flashDuration);
        }

        private void OnDestroy()
        {
            _level = null;
            _flash = null;
            _flashDelay = null;
            Destroy(_material);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollecting == false)
            {
                return;
            }

            if (_level.TryTakeCoin(other) == false)
            {
                return;
            }

            _score++;
            _level.ShowScore(_score);
            _audio.PlayCollect();
            Flash();
        }

        public void ResetScore()
        {
            _score = 0;
            _level.ShowScore(_score);
            StopFlash();
        }

        public void PauseCollection()
        {
            _isCollecting = false;
        }

        public void ResumeCollection()
        {
            _isCollecting = true;
        }

        private void Flash()
        {
            StopFlash();
            _material.color = Color.white;
            _flash = StartCoroutine(RestoreColor());
        }

        private void StopFlash()
        {
            if (_flash != null)
            {
                StopCoroutine(_flash);
                _flash = null;
            }

            _material.color = _originalColor;
        }

        private IEnumerator RestoreColor()
        {
            yield return _flashDelay;
            _material.color = _originalColor;
            _flash = null;
        }
    }
}
