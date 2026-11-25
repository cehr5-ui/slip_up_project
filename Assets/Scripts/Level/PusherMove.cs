using DG.Tweening;
using UnityEngine;

namespace SlipUp
{
    public class PusherMove : Pusher
    {
        [SerializeField] private Transform _pointA;
        [SerializeField] private Transform _pointB;
        [SerializeField] private float _travelDuration = 2.5f;

        private Tween _move;

        private void OnEnable()
        {
            transform.position = _pointA.position;
            _move = transform
                .DOMove(_pointB.position, _travelDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(UpdateType.Fixed);
        }

        protected override void HandleDisable()
        {
            if (_move != null)
            {
                _move.Kill();
                _move = null;
            }

            base.HandleDisable();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_pointA.position, _pointB.position);
            Gizmos.DrawWireSphere(_pointA.position, 0.25f);
            Gizmos.DrawWireSphere(_pointB.position, 0.25f);
        }
    }
}
