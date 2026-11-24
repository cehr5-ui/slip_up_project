using UnityEngine;

namespace SlipUp
{
    public class PlayerFx : MonoBehaviour
    {
        private static readonly Color JumpColor = new Color(0.6f, 0.9f, 1f, 1f);
        private static readonly Color DashColor = new Color(1f, 0.85f, 0.2f, 1f);
        private static readonly Color HitColor = new Color(1f, 0.35f, 0.15f, 1f);
        private static readonly Color CoinColor = new Color(1f, 0.84f, 0.2f, 1f);
        private static readonly Color FinishColor = new Color(0.4f, 1f, 0.55f, 1f);

        internal void PlayJump(Vector3 position)
        {
            Play(position, JumpColor, 10);
        }

        internal void PlayDash(Vector3 position)
        {
            Play(position, DashColor, 18);
        }

        internal void PlayHit(Vector3 position)
        {
            Play(position, HitColor, 16);
        }

        internal void PlayCoin(Vector3 position)
        {
            Play(position, CoinColor, 12);
        }

        internal void PlayFinish(Vector3 position)
        {
            Play(position, FinishColor, 22);
        }

        private static void Play(Vector3 position, Color color, int count)
        {
            if (FxPool.Current == null)
            {
                return;
            }

            FxPool.Current.Play(position, color, count);
        }
    }
}
