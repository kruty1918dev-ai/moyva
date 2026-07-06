using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorHeightProjectionOffsetService : ITileWorldCreatorHeightProjectionOffsetService
    {
        public TileWorldCreatorHeightProjectionStats ApplyOffsets(
            TileWorldCreatorHeightProjectionState state,
            Transform root,
            int width,
            int height,
            float minX,
            float minZ)
        {
            var stats = new TileWorldCreatorHeightProjectionStats();
            foreach (var sample in state.ScratchBuffer)
                ApplyOffset(state, root, width, height, minX, minZ, sample, ref stats);

            return stats;
        }

        private static void ApplyOffset(
            TileWorldCreatorHeightProjectionState state,
            Transform root,
            int width,
            int height,
            float minX,
            float minZ,
            TileWorldCreatorTileTransformSample sample,
            ref TileWorldCreatorHeightProjectionStats stats)
        {
            Vector3 local = root.InverseTransformPoint(sample.WorldCenter);
            int rawX = Mathf.FloorToInt((local.x - minX + 0.001f) / state.CellSize);
            int rawY = Mathf.FloorToInt((local.z - minZ + 0.001f) / state.CellSize);
            int cellX = Mathf.Clamp(rawX, 0, width - 1);
            int cellY = Mathf.Clamp(rawY, 0, height - 1);
            if (cellX != rawX || cellY != rawY)
                stats.Clamped++;

            int level = state.TerrainLevelMap[cellX, cellY];
            stats.RegisterLevel(level);
            state.UsedCells.Add(new Vector2Int(cellX, cellY));
            float previous = state.AppliedYOffsetByTransformId.TryGetValue(sample.Transform.GetInstanceID(), out float stored) ? stored : 0f;
            float next = level * state.HeightStep;
            if (Mathf.Approximately(previous, next))
            {
                stats.Unchanged++;
                return;
            }

            var pos = sample.Transform.position;
            pos.y += next - previous;
            sample.Transform.position = pos;
            state.AppliedYOffsetByTransformId[sample.Transform.GetInstanceID()] = next;
            stats.Changed++;
            AddSample(state, sample.Transform.name, local, rawX, rawY, cellX, cellY, level, previous, next);
        }

        private static void AddSample(
            TileWorldCreatorHeightProjectionState state,
            string name,
            Vector3 local,
            int rawX,
            int rawY,
            int cellX,
            int cellY,
            int level,
            float previous,
            float next)
        {
            if (state.SampleApplications.Count < 12)
                state.SampleApplications.Add($"'{name}' local=({local.x:0.##},{local.z:0.##}) rawCell=({rawX},{rawY}) cell=({cellX},{cellY}) level={level} offset {previous:0.##}->{next:0.##}");
        }
    }
}
