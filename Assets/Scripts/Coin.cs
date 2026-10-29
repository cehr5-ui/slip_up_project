using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace SlipUp
{
    public class Coin : MonoBehaviour
    {
        private const float FullTurn = 360f;

        [SerializeField] private Transform _visual;
        [SerializeField] private Collider _collider;
        [SerializeField] private float _degreesPerSecond = 180f;
        [SerializeField] private float _collectDuration = 0.2f;

        private ObjectPool<Coin> _pool;
        private Vector3 _visualScale;
        private Tween _spin;
        private Tween _collect;
        private bool _isCollected;

        internal void Initialize(ObjectPool<Coin> pool)
        {
            _pool = pool;
        }

        private void Awake()
        {
            _visualScale = _visual.localScale;
        }

        private void OnDestroy()
        {
            _pool = null;
            _spin = null;
            _collect = null;
        }

        private void OnEnable()
        {
            _isCollected = false;
            _collider.enabled = true;
            _visual.localScale = _visualScale;
            float turnDuration = FullTurn / _degreesPerSecond;
            _spin = _visual
                .DORotate(new Vector3(0f, FullTurn, 0f), turnDuration, RotateMode.WorldAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }

        private void OnDisable()
        {
            if (_spin != null)
            {
                _spin.Kill();
                _spin = null;
            }

            if (_collect != null)
            {
                _collect.Kill();
                _collect = null;
            }
        }

        internal bool TryCollect()
        {
            if (_isCollected)
            {
                return false;
            }

            _isCollected = true;
            _collider.enabled = false;
            _spin.Kill();
            _spin = null;
            _collect = _visual.DOScale(Vector3.zero, _collectDuration).OnComplete(OnCollectCompleted);
            return true;
        }

        private void OnCollectCompleted()
        {
            _collect = null;
            _pool.Release(this);
        }
    }
}
