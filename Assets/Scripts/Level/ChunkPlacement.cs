using UnityEngine;

namespace SlipUp
{
    public static class ChunkPlacement
    {
        public const float PositionTolerance = 0.02f;
        public const float AngleTolerance = 0.5f;
        public const float MaxRearOverhang = 0.25f;

        public static bool AreCompatible(ConnectorType exitType, ConnectorType entryType)
        {
            return exitType == entryType;
        }

        public static void AlignToExit(Transform chunk, Transform entry, Transform previousExit)
        {
            Quaternion worldRotation = previousExit.rotation * Quaternion.Inverse(entry.localRotation);
            Vector3 worldPosition = previousExit.position - worldRotation * entry.localPosition;
            chunk.SetPositionAndRotation(worldPosition, worldRotation);
            chunk.localScale = Vector3.one;
        }

        public static bool IsSeamAligned(Transform previousExit, Transform nextEntry)
        {
            if (Vector3.Distance(previousExit.position, nextEntry.position) > PositionTolerance)
            {
                return false;
            }

            float angle = Quaternion.Angle(previousExit.rotation, nextEntry.rotation);
            return angle <= AngleTolerance;
        }
    }
}
