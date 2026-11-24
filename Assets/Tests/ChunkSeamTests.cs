using NUnit.Framework;
using UnityEngine;

using SlipUp;

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

        [Test]
        public void SameSeed_BuildsMatchingSeams()
        {
            GameObject host = new GameObject("SeedTestHost");
            TrackGenerator generator = host.AddComponent<TrackGenerator>();
            Chunk[] first = generator.Build(12345);
            Vector3[] exits = new Vector3[first.Length];

            for (int i = 0; i < first.Length; i++)
            {
                exits[i] = first[i].Exit.position;
            }

            Chunk[] second = generator.Build(12345);
            Assert.AreEqual(exits.Length, second.Length);

            for (int i = 0; i < second.Length; i++)
            {
                Assert.IsTrue(Vector3.Distance(exits[i], second[i].Exit.position) <= ChunkPlacement.PositionTolerance);
            }

            for (int i = 1; i < second.Length; i++)
            {
                Assert.IsTrue(ChunkPlacement.IsSeamAligned(second[i - 1].Exit, second[i].Entry));
            }

            Assert.GreaterOrEqual(second.Length, 10);
            Assert.LessOrEqual(second.Length, 17);
            Object.DestroyImmediate(host);
        }
    }
}
