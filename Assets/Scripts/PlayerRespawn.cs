using UnityEngine;

namespace SlipUp
{
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Collider _location;
        [SerializeField] private float _outsideMargin = 2f;

        private Vector3 _startPosition;

        private void Awake()
        {
            _startPosition = _rigidbody.position;
        }

        private void FixedUpdate()
        {
            if (IsOutsideLocation == false)
            {
                return;
            }

            ReturnToStart();
        }

        private bool IsOutsideLocation
        {
            get
            {
                Bounds bounds = _location.bounds;
                bounds.Expand(_outsideMargin);
                Vector3 position = _rigidbody.position;

                if (position.y < bounds.min.y)
                {
                    return true;
                }

                if (position.x < bounds.min.x || position.x > bounds.max.x)
                {
                    return true;
                }

                return position.z < bounds.min.z || position.z > bounds.max.z;
            }
        }

        private void ReturnToStart()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = _startPosition;
        }
    }
}
