using UnityEngine;

namespace SlipUp
{
    public class ChunkSpinner : MonoBehaviour
    {
        [SerializeField] private Vector3 _axis = Vector3.up;
        [SerializeField] private float _degreesPerSecond = 80f;

        internal void Bind(Vector3 axis, float degreesPerSecond)
        {
            _axis = axis;
            _degreesPerSecond = degreesPerSecond;
        }

        private void Update()
        {
            transform.Rotate(_axis, _degreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}
