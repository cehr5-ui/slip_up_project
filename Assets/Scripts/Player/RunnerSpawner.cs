using UnityEngine;

namespace SlipUp
{
    public class RunnerSpawner : MonoBehaviour
    {
        [SerializeField] private Runner _template;

        private Runner _runner;

        private void Awake()
        {
            _template.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _runner = null;
            _template = null;
        }

        internal Runner Spawn(Vector3 position, Quaternion rotation)
        {
            Despawn();
            _runner = Instantiate(_template, position, rotation, transform);
            _runner.gameObject.SetActive(true);
            return _runner;
        }

        internal void Despawn()
        {
            if (_runner == null)
            {
                return;
            }

            Runner runner = _runner;
            _runner = null;
            runner.gameObject.SetActive(false);
            Destroy(runner.gameObject);
        }
    }
}
