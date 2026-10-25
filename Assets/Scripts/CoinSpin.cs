using UnityEngine;

namespace SlipUp
{
    public class CoinSpin : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private float _degreesPerSecond = 180f;

        private void Update()
        {
            _visual.Rotate(Vector3.up, _degreesPerSecond * Time.deltaTime, Space.World);
        }
    }
}
