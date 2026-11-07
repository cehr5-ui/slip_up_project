using DG.Tweening;
using UnityEngine;

namespace SlipUp
{
    public class Pusher : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private float _upwardBounce = 0.5f;
        [SerializeField] private float _hitPunchStrength = 0.2f;
        [SerializeField] private float _hitPunchDuration = 0.25f;
        [SerializeField] private float _bounceMultiplier = 1f;
        [SerializeField] private float _minimumBounceSpeed = 18f;

        private Vector3 _visualScale;
        private Tween _hitPunch;

        private void Awake()
        {
            _visualScale = _visual.localScale;
        }

        protected virtual void OnDisable()
        {
            if (_hitPunch == null)
            {
                return;
            }

            _hitPunch.Kill();
            _hitPunch = null;
            _visual.localScale = _visualScale;
        }

        private void OnDestroy()
        {
            _hitPunch = null;
            _visual = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody body = collision.rigidbody;

            if (body == null)
            {
                return;
            }

            if (body.TryGetComponent(out Runner runner) == false)
            {
                return;
            }

            if (TryGetBounceVelocity(collision, out Vector3 bounceVelocity) == false)
            {
                return;
            }

            runner.Bounce(bounceVelocity);
            PlayHit();
        }

        private void PlayHit()
        {
            if (_hitPunch != null)
            {
                _hitPunch.Kill();
                _hitPunch = null;
            }

            _visual.localScale = _visualScale;
            Vector3 scale = transform.localScale;
            Vector3 punch = new Vector3(
                _hitPunchStrength / scale.x,
                _hitPunchStrength / scale.y,
                _hitPunchStrength / scale.z);
            _hitPunch = _visual.DOPunchScale(punch, _hitPunchDuration, 1, 0.5f);
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

            bounceVelocity = reflected * _bounceMultiplier;
            float minimumSpeed = _minimumBounceSpeed * _bounceMultiplier;
            float minimumSpeedSqr = minimumSpeed * minimumSpeed;

            if (bounceVelocity.sqrMagnitude >= minimumSpeedSqr)
            {
                return true;
            }

            if (bounceVelocity.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            bounceVelocity = bounceVelocity.normalized * minimumSpeed;
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
