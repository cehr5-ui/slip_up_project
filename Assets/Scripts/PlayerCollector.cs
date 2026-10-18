using UnityEngine;

namespace SlipUp
{
    public class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private ScoreView _scoreView;
        [SerializeField] private PlayerAudio _audio;
        [SerializeField] private float _flashDuration = 0.15f;

        private int _score;
        private Material _material;
        private Color _originalColor;
        private bool _isCollecting = true;

        public int Score => _score;

        private void Awake()
        {
            _material = _meshRenderer.material;
            _originalColor = _material.color;
        }

        private void OnDestroy()
        {
            Destroy(_material);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollecting == false)
            {
                return;
            }

            if (other.TryGetComponent(out CoinSpin coin) == false)
            {
                return;
            }

            Destroy(coin.gameObject);
            _score++;
            _scoreView.SetScore(_score);
            int coinCount = CoinSpawner.CoinCount;
            _scoreView.SetCleared(coinCount > 0 && _score >= coinCount);
            Debug.Log(_score);
            _audio.PlayCollect();
            Flash();
        }

        public void ResetScore()
        {
            _score = 0;
            _scoreView.SetScore(_score);
            _scoreView.SetCleared(false);
            CancelInvoke(nameof(RestoreColor));
            RestoreColor();
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
            CancelInvoke(nameof(RestoreColor));
            _material.color = Color.white;
            Invoke(nameof(RestoreColor), _flashDuration);
        }

        private void RestoreColor()
        {
            _material.color = _originalColor;
        }
    }
}
