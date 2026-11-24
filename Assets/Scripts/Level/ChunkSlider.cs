using UnityEngine;

namespace SlipUp
{
    public class ChunkSlider : MonoBehaviour
    {
        [SerializeField] private Vector3 _pointA;
        [SerializeField] private Vector3 _pointB;
        [SerializeField] private float _duration = 2.5f;

        private float _time;

        internal void Bind(Vector3 pointA, Vector3 pointB, float duration)
        {
            _pointA = pointA;
            _pointB = pointB;
            _duration = duration;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            float progress = Mathf.PingPong(_time / _duration, 1f);
            transform.localPosition = Vector3.Lerp(_pointA, _pointB, progress);
        }
    }
}
