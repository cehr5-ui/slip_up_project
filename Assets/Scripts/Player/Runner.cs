using System;
using UnityEngine;

namespace SlipUp
{
    public class Runner : MonoBehaviour
    {
        [SerializeField] private Transform _focus;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private PlayerCollector _collector;
        [SerializeField] private PlayerRespawn _respawn;
        [SerializeField] private PlayerAudio _audio;

        internal event Action<Runner> Released;
        internal event Action<Runner> Fell;

        internal Transform Focus => _focus;

        internal bool IsControlEnabled => _mover.IsControlEnabled;

        internal bool IsFrozen => _mover.IsFrozen;

        internal void Initialize(RunModel model, Arena arena, Transform cameraTransform)
        {
            _mover.Initialize(model, cameraTransform);
            _collector.Initialize(model);
            _respawn.Initialize(arena, this);
        }

        internal void Bounce(Vector3 velocity)
        {
            _mover.Bounce(velocity);
            _audio.PlayHit();
        }

        internal void PauseCollection()
        {
            _collector.PauseCollection();
        }

        internal void ResumeCollection()
        {
            _collector.ResumeCollection();
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
            _respawn.AllowReport();
        }

        internal void NotifyFell()
        {
            Fell?.Invoke(this);
        }

        private void OnDestroy()
        {
            Released?.Invoke(this);
            Released = null;
            Fell = null;
            _focus = null;
            _mover = null;
            _collector = null;
            _respawn = null;
            _audio = null;
        }
    }
}
