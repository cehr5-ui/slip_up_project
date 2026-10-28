using DG.Tweening;
using UnityEngine;

namespace SlipUp
{
    public class Pusher : MonoBehaviour
    {
        [SerializeField] private float _upwardBounce = 0.5f;
        [SerializeField] private float _hitPunchStrength = 0.2f;
        [SerializeField] private float _hitPunchDuration = 0.25f;

        private Level _level;
        private Vector3 _scale;
        private Tween _hitPunch;

        internal void Initialize(Level level)
        {
            _level = level;
        }

        private void Awake()
        {
            _scale = transform.localScale;
        }

        protected void OnDisable()
        {
            if (_hitPunch == null)
            {
                return;
            }

            _hitPunch.Kill();
            _hitPunch = null;
            transform.localScale = _scale;
        }

        private void Start()
        {
            Level.Connect(this);
        }

        private void OnDestroy()
        {
            _level = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_level.IsPlayer(collision.gameObject) == false)
            {
                return;
            }

            if (TryGetBounceVelocity(collision, out Vector3 bounceVelocity) == false)
            {
                return;
            }

            _level.BouncePlayer(bounceVelocity);
            PlayHit();
        }

        private void PlayHit()
        {
            if (_hitPunch != null)
            {
                _hitPunch.Kill();
                _hitPunch = null;
            }

            transform.localScale = _scale;
            _hitPunch = transform.DOPunchScale(Vector3.one * _hitPunchStrength, _hitPunchDuration, 1, 0.5f);
        }

        private bool TryGetBounceVelocity(Collision collision, out Vector3 bounceVelocity)
        {
            bounceVelocity = Vector3.zero;

            if (TryGetBounceNormal(collision, out Vector3 normal) == false)
            {
                return false;
            }

            Vector3 incoming = collision.relativeVelocity;

            if (Vector3.Dot(incoming, normal) > 0f)
            {
                incoming = -incoming;
            }

            if (incoming.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            Vector3 reflected = Vector3.Reflect(incoming, normal);

            if (Vector3.Dot(reflected, normal) < 0f)
            {
                reflected = -reflected;
            }

            if (reflected.y > 0f)
            {
                reflected.y *= _upwardBounce;
            }

            bounceVelocity = reflected;
            return true;
        }

        private bool TryGetBounceNormal(Collision collision, out Vector3 normal)
        {
            normal = Vector3.zero;

            if (collision.contactCount == 0)
            {
                return false;
            }

            Vector3 motion = collision.relativeVelocity;
            ContactPoint contact = collision.GetContact(0);
            normal = contact.normal;
            float bestAlignment = Vector3.Dot(normal, motion);
            bestAlignment *= bestAlignment;

            for (int i = 1; i < collision.contactCount; i++)
            {
                ContactPoint nextContact = collision.GetContact(i);
                float alignment = Vector3.Dot(nextContact.normal, motion);
                alignment *= alignment;

                if (alignment <= bestAlignment)
                {
                    continue;
                }

                bestAlignment = alignment;
                contact = nextContact;
                normal = nextContact.normal;
            }

            Vector3 toPlayer = collision.rigidbody.worldCenterOfMass - contact.point;

            if (Vector3.Dot(normal, toPlayer) < 0f)
            {
                normal = -normal;
            }

            if (normal.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            normal.Normalize();
            return true;
        }
    }
}
