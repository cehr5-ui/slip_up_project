using NUnit.Framework;
using UnityEngine;

namespace SlipUp.Tests
{
    public class ChunkSeamTests
    {
        [Test]
        public void StraightAThenB_SeamMatches()
        {
            GameObject host = new GameObject("SeamTestHost");
            ChunkFactory factory = new ChunkFactory(host.transform);
            Chunk chunkA = factory.CreateStart();
            Chunk chunkB = factory.CreateStraightB();

            chunkA.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            chunkA.transform.localScale = Vector3.one;
            ChunkPlacement.AlignToExit(chunkB.transform, chunkB.Entry, chunkA.Exit);

            Assert.AreEqual(Vector3.one, chunkA.transform.localScale);
            Assert.AreEqual(Vector3.one, chunkB.transform.localScale);
            Assert.AreEqual(Vector3.zero, chunkA.Entry.localPosition);
            Assert.AreEqual(Quaternion.identity, chunkA.Entry.localRotation);
            Assert.IsTrue(ChunkPlacement.AreCompatible(chunkA.ExitType, chunkB.EntryType));
            Assert.IsTrue(ChunkPlacement.IsSeamAligned(chunkA.Exit, chunkB.Entry));
            Assert.LessOrEqual(Mathf.Abs(chunkA.Entry.localPosition.z), ChunkPlacement.MaxRearOverhang);

            Object.DestroyImmediate(host);
        }
    }
}
