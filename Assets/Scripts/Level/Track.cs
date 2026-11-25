using System;
using UnityEngine;

namespace SlipUp
{
    public class Track : MonoBehaviour
    {
        [SerializeField] private Chunk[] _chunks;

        private TrackGenerator _generator;

        internal event Action FinishReached;

        internal int Seed => _generator == null ? 0 : _generator.Seed;

        internal Vector3 SpawnPosition => _generator.SpawnPosition;

        internal Quaternion SpawnRotation => _generator.SpawnRotation;

        internal float KillHeight => _generator.KillHeight;

        internal void Generate(int seed)
        {
            if (_generator == null)
            {
                _generator = GetComponent<TrackGenerator>();
            }

            if (_generator == null)
            {
                _generator = gameObject.AddComponent<TrackGenerator>();
            }

            _chunks = _generator.Build(seed);
        }

        internal void Initialize()
        {
            if (_chunks == null)
            {
                return;
            }

            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i].Initialize(this);
            }
        }

        internal void Restore()
        {
            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i].Restore();
            }
        }

        internal void RaiseFinish()
        {
            FinishReached?.Invoke();
        }

        private void OnDestroy()
        {
            FinishReached = null;
            _generator = null;

            if (_chunks == null)
            {
                return;
            }

            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i] = null;
            }

            _chunks = null;
        }
    }
}
