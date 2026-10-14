using UnityEngine;

namespace SlipUp
{
    public class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private float _flashDuration = 0.15f;

        private int _score;
        private Material _material;
        private Color _originalColor;

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
            if (other.TryGetComponent(out CoinSpin coin) == false)
            {
                return;
            }

            Destroy(coin.gameObject);
            _score++;
            Debug.Log(_score);
            Flash();
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
