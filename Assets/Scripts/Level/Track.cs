using System;
using UnityEngine;

namespace SlipUp
{
    public class Track : MonoBehaviour
    {
        [SerializeField] private Chunk[] _chunks;

        internal event Action FinishReached;

        internal void Initialize()
        {
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

            for (int i = 0; i < _chunks.Length; i++)
            {
                _chunks[i] = null;
            }

            _chunks = null;
        }
    }
}
