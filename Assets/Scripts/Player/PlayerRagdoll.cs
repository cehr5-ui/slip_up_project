using System.Collections.Generic;
using UnityEngine;

namespace SlipUp
{
    public class PlayerRagdoll : MonoBehaviour
    {
        private static readonly string[] BoneTokens =
        {
            "hips", "spine", "chest", "neck", "head",
            "arm", "hand", "leg", "foot", "thigh", "calf", "shoulder",
        };

        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _body;
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private PlayerAnimator _playerAnimator;

        private readonly List<Rigidbody> _bones = new List<Rigidbody>();
        private readonly List<Collider> _boneColliders = new List<Collider>();
        private bool _isRagdoll;

        internal bool IsRagdoll => _isRagdoll;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_body == null)
            {
                _body = GetComponent<Rigidbody>();
            }

            if (_capsule == null)
            {
                _capsule = GetComponentInChildren<CapsuleCollider>();
            }

            if (_playerAnimator == null)
            {
                _playerAnimator = GetComponentInChildren<PlayerAnimator>();
            }

            BuildBones();
            SetRagdoll(false, Vector3.zero);
        }

        internal void Activate(Vector3 velocity)
        {
            if (_isRagdoll)
            {
                return;
            }

            SetRagdoll(true, velocity);
        }

        private void BuildBones()
        {
            if (_animator == null)
            {
                return;
            }

            Transform[] transforms = _animator.GetComponentsInChildren<Transform>();

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform bone = transforms[i];

                if (IsBone(bone) == false)
                {
                    continue;
                }

                Rigidbody rigidbody = bone.GetComponent<Rigidbody>();

                if (rigidbody == null)
                {
                    rigidbody = bone.gameObject.AddComponent<Rigidbody>();
                }

                rigidbody.mass = 0.8f;
                rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                CapsuleCollider collider = bone.GetComponent<CapsuleCollider>();

                if (collider == null)
                {
                    collider = bone.gameObject.AddComponent<CapsuleCollider>();
                    collider.radius = 0.08f;
                    collider.height = 0.28f;
                    collider.direction = 1;
                }

                _bones.Add(rigidbody);
                _boneColliders.Add(collider);
            }
        }

        private void SetRagdoll(bool isRagdoll, Vector3 velocity)
        {
            _isRagdoll = isRagdoll;

            if (_playerAnimator != null)
            {
                _playerAnimator.SetEnabled(isRagdoll == false);
            }

            if (_animator != null)
            {
                _animator.enabled = isRagdoll == false;
            }

            if (_capsule != null)
            {
                _capsule.enabled = isRagdoll == false;
            }

            if (_body != null)
            {
                if (isRagdoll)
                {
                    _body.linearVelocity = Vector3.zero;
                    _body.angularVelocity = Vector3.zero;
                    _body.isKinematic = true;
                    _body.detectCollisions = false;
                }
                else
                {
                    _body.detectCollisions = true;
                    _body.isKinematic = false;
                }
            }

            for (int i = 0; i < _bones.Count; i++)
            {
                Rigidbody bone = _bones[i];
                _boneColliders[i].enabled = isRagdoll;
                bone.detectCollisions = isRagdoll;
                bone.isKinematic = isRagdoll == false;

                if (isRagdoll == false)
                {
                    continue;
                }

                bone.linearVelocity = velocity;
                bone.angularVelocity = Vector3.zero;
            }
        }

        private static bool IsBone(Transform transform)
        {
            if (transform.GetComponent<SkinnedMeshRenderer>() != null)
            {
                return false;
            }

            string name = transform.name.ToLowerInvariant();

            for (int i = 0; i < BoneTokens.Length; i++)
            {
                if (name.Contains(BoneTokens[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
