using UnityEngine;

namespace SlipUp
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private Transform _focus;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private PlayerCollector _collector;
        [SerializeField] private PlayerRespawn _respawn;
        [SerializeField] private PlayerAudio _audio;

        internal Transform Focus => _focus;

        internal bool IsControlEnabled => _mover.IsControlEnabled;

        internal void Initialize(Level level)
        {
            _mover.Initialize(level);
            _collector.Initialize(level);
            _respawn.Initialize(level);
        }

        internal void Bounce(Vector3 velocity)
        {
            _mover.Bounce(velocity);
            _audio.PlayHit();
        }

        internal void BeginRun()
        {
            _mover.BeginRun();
        }

        internal void Freeze()
        {
            _mover.Freeze();
        }

        internal void Release()
        {
            _mover.Release();
        }
    }
}
