using DG.Tweening;
using UnityEngine;

namespace SlipUp
{
    public class PusherMove : Pusher
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _pointA;
        [SerializeField] private Transform _pointB;
        [SerializeField] private float _travelDuration = 2.5f;

        private Tween _move;

        private void OnEnable()
        {
            _rigidbody.position = _pointA.position;
            _move = _rigidbody
                .DOMove(_pointB.position, _travelDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(UpdateType.Fixed);
        }

        private void OnDisable()
        {
            base.OnDisable();

            if (_move == null)
            {
                return;
            }

            _move.Kill();
            _move = null;
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
