using System.Collections;
using UnityEngine;

namespace SlipUp
{
    public class PlayerRespawn : MonoBehaviour
    {
        private const float FreezeDuration = 1f;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PlayerCollector _collector;

        private Level _level;
        private bool _isReturning;
        private WaitForSeconds _freezeDelay;
        private WaitForFixedUpdate _physicsStep;
        private Coroutine _return;

        private bool IsOutsideLocation => _level.IsBelowArena(_rigidbody.position);

        internal void Initialize(Level level)
        {
            _level = level;
            TeleportToStart();
        }

        private void Awake()
        {
            _freezeDelay = new WaitForSeconds(FreezeDuration);
            _physicsStep = new WaitForFixedUpdate();
        }

        private void OnDestroy()
        {
            if (_return != null)
            {
                StopCoroutine(_return);
                _return = null;
            }

            _level = null;
            _freezeDelay = null;
            _physicsStep = null;
        }

        private void FixedUpdate()
        {
            if (IsOutsideLocation == false)
            {
                return;
            }

            if (_isReturning)
            {
                return;
            }

            _isReturning = true;
            _return = StartCoroutine(ReturnToStart());
        }

        private IEnumerator ReturnToStart()
        {
            _collector.ResetScore();
            _collector.PauseCollection();
            TeleportToStart();
            _level.FreezePlayer();
            yield return _freezeDelay;
            yield return _physicsStep;
            _level.RespawnCoins();
            yield return _physicsStep;
            _collector.ResumeCollection();
            _level.ReleasePlayer();
            _isReturning = false;
            _return = null;
        }

        private void TeleportToStart()
        {
            _rigidbody.position = _level.StartPosition;
            _rigidbody.rotation = _level.StartRotation;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
