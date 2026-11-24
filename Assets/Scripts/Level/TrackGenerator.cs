using UnityEngine;

namespace SlipUp
{
    public class TrackGenerator : MonoBehaviour
    {
        internal Chunk[] BuildPair(Transform parent)
        {
            ChunkFactory factory = new ChunkFactory(parent);
            Chunk start = factory.CreateStart();
            Chunk next = factory.CreateStraightB();
            start.transform.SetPositionAndRotation(parent.position, parent.rotation);
            start.transform.localScale = Vector3.one;
            ChunkPlacement.AlignToExit(next.transform, next.Entry, start.Exit);
            return new[] { start, next };
        }
    }
}
