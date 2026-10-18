using UnityEngine;

namespace SlipUp
{
    public class PusherMove : Pusher
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _pointA;
        [SerializeField] private Transform _pointB;
        [SerializeField] private float _travelDuration = 2.5f;

        private float _travelTime;

        private void OnEnable()
        {
            _travelTime = 0f;
        }

        private void Update()
        {
            float progress = Mathf.PingPong(_travelTime / _travelDuration, 1f);
            Vector3 position = Vector3.Lerp(_pointA.position, _pointB.position, progress);
            _rigidbody.MovePosition(position);
            _travelTime += Time.deltaTime;
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
