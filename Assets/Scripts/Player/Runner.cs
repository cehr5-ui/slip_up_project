using System;
using UnityEngine;

namespace SlipUp
{
    public class Runner : MonoBehaviour
    {
        private const float KnockoutSpeed = 22f;

        [SerializeField] private Transform _focus;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private PlayerCollector _collector;
        [SerializeField] private PlayerRespawn _respawn;
        [SerializeField] private PlayerAudio _audio;

        private PlayerFx _fx;
        private PlayerRagdoll _ragdoll;

        internal event Action<Runner> Released;
        internal event Action<Runner> Fell;
        internal event Action<Runner> KnockedOut;

        internal Transform Focus => _focus;

        internal bool IsControlEnabled => _mover.IsControlEnabled;

        internal bool IsFrozen => _mover.IsFrozen;

        internal bool IsRagdoll => _ragdoll != null && _ragdoll.IsRagdoll;

        internal void Initialize(RunModel model, Arena arena, Transform cameraTransform, GameplayInput input)
        {
            _mover.Initialize(model, cameraTransform, input);
            _collector.Initialize(model);
            _respawn.Initialize(arena, this);
            _mover.Jumped += OnJumped;
            _mover.Dashed += OnDashed;
        }

        internal void Bounce(Vector3 velocity)
        {
            if (velocity.magnitude >= KnockoutSpeed)
            {
                KnockOut(velocity);
                return;
            }

            _audio.PlayHit();
            _fx.PlayHit(transform.position);
            _mover.Bounce(velocity);
        }

        internal void KnockOut(Vector3 velocity)
        {
            if (_ragdoll.IsRagdoll)
            {
                return;
            }

            _audio.PlayHit();
            _fx.PlayHit(transform.position);
            _mover.Freeze();
            _collector.PauseCollection();
            _ragdoll.Activate(velocity);
            KnockedOut?.Invoke(this);
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

        internal void PlayFinishFx()
        {
            _fx.PlayFinish(transform.position);
            _audio.PlayFinish();
        }

        internal void PlayCoinFx(Vector3 position)
        {
            _fx.PlayCoin(position);
        }

        private void Awake()
        {
            if (GetComponent<CharacterSkin>() == null)
            {
                gameObject.AddComponent<CharacterSkin>();
            }

            _fx = GetComponent<PlayerFx>();

            if (_fx == null)
            {
                _fx = gameObject.AddComponent<PlayerFx>();
            }

            _ragdoll = GetComponent<PlayerRagdoll>();

            if (_ragdoll == null)
            {
                _ragdoll = gameObject.AddComponent<PlayerRagdoll>();
            }
        }

        private void OnDestroy()
        {
            if (_mover != null)
            {
                _mover.Jumped -= OnJumped;
                _mover.Dashed -= OnDashed;
            }

            Released?.Invoke(this);
            Released = null;
            Fell = null;
            KnockedOut = null;
            _focus = null;
            _mover = null;
            _collector = null;
            _respawn = null;
            _audio = null;
            _fx = null;
            _ragdoll = null;
        }

        private void OnJumped()
        {
            _fx.PlayJump(transform.position);
            _audio.PlayJump();
        }

        private void OnDashed()
        {
            _fx.PlayDash(transform.position);
            _audio.PlayDash();
        }
    }
}
