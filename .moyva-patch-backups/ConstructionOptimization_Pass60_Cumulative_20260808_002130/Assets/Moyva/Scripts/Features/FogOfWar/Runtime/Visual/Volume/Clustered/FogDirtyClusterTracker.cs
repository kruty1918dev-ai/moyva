using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogDirtyClusterTracker : IFogDirtyClusterTracker
    {
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private readonly FogOfWarSettings _settings;
        private readonly HashSet<FogClusterKey> _dirtyClusters = new HashSet<FogClusterKey>();
        private readonly List<FogClusterKey> _consumeBuffer = new List<FogClusterKey>();

        public FogDirtyClusterTracker([InjectOptional] FogOfWarSettings settings = null)
        {
            _settings = settings;
        }

        public void MarkChanges(IReadOnlyList<FogCellVisualChange> changes, FogWorldVisualContext context)
        {
            _dirtyClusters.Clear();
            if (changes == null || changes.Count == 0 || !context.IsValid)
                return;

            int clusterSize = ResolveClusterSize();
            int padding = ResolveClusterPadding(clusterSize);
            int clusterCountX = Mathf.Max(1, Mathf.CeilToInt(context.Width / (float)clusterSize));
            int clusterCountY = Mathf.Max(1, Mathf.CeilToInt(context.Height / (float)clusterSize));

            for (int i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                if (!ShouldMarkChange(change))
                    continue;

                MarkCellCluster(change.Cell, clusterSize, padding, clusterCountX, clusterCountY);
            }

            if (ShouldLogClusterUpdates())
                Debug.Log($"{ClusterDiagTag} DirtyClustersResolved changes={changes.Count}, clusters={_dirtyClusters.Count}, clusterSize={clusterSize}, padding={padding}.");
        }

        public IReadOnlyList<FogClusterKey> ConsumeDirtyClusters()
        {
            _consumeBuffer.Clear();
            foreach (var key in _dirtyClusters)
                _consumeBuffer.Add(key);

            _dirtyClusters.Clear();
            return _consumeBuffer;
        }

        public void Clear()
        {
            _dirtyClusters.Clear();
            _consumeBuffer.Clear();
        }

        private void MarkCellCluster(
            Vector2Int cell,
            int clusterSize,
            int padding,
            int clusterCountX,
            int clusterCountY)
        {
            int clusterX = Mathf.Clamp(cell.x / clusterSize, 0, clusterCountX - 1);
            int clusterY = Mathf.Clamp(cell.y / clusterSize, 0, clusterCountY - 1);
            AddClusterWithHalo(cell, clusterX, clusterY, clusterSize, padding, clusterCountX, clusterCountY);
        }

        private void AddClusterWithHalo(
            Vector2Int cell,
            int clusterX,
            int clusterY,
            int clusterSize,
            int padding,
            int clusterCountX,
            int clusterCountY)
        {
            AddCluster(clusterX, clusterY, clusterCountX, clusterCountY);

            if (padding <= 0)
                return;

            int localX = PositiveModulo(cell.x, clusterSize);
            int localY = PositiveModulo(cell.y, clusterSize);
            bool nearLeft = localX < padding;
            bool nearRight = localX >= clusterSize - padding;
            bool nearBottom = localY < padding;
            bool nearTop = localY >= clusterSize - padding;

            if (nearLeft)
                AddCluster(clusterX - 1, clusterY, clusterCountX, clusterCountY);
            if (nearRight)
                AddCluster(clusterX + 1, clusterY, clusterCountX, clusterCountY);
            if (nearBottom)
                AddCluster(clusterX, clusterY - 1, clusterCountX, clusterCountY);
            if (nearTop)
                AddCluster(clusterX, clusterY + 1, clusterCountX, clusterCountY);

            if (nearLeft && nearBottom)
                AddCluster(clusterX - 1, clusterY - 1, clusterCountX, clusterCountY);
            if (nearLeft && nearTop)
                AddCluster(clusterX - 1, clusterY + 1, clusterCountX, clusterCountY);
            if (nearRight && nearBottom)
                AddCluster(clusterX + 1, clusterY - 1, clusterCountX, clusterCountY);
            if (nearRight && nearTop)
                AddCluster(clusterX + 1, clusterY + 1, clusterCountX, clusterCountY);
        }

        private void AddCluster(int clusterX, int clusterY, int clusterCountX, int clusterCountY)
        {
            if (clusterX < 0 || clusterY < 0 || clusterX >= clusterCountX || clusterY >= clusterCountY)
                return;

            _dirtyClusters.Add(new FogClusterKey(clusterX, clusterY));
        }

        private static bool ShouldMarkChange(FogCellVisualChange change)
            => change.HasVisualDelta
                && (change.OldState != FogStateType.Visible || change.NewState != FogStateType.Visible);

        private int ResolveClusterSize()
            => Mathf.Max(1, _settings?.Volume.ClusterSize ?? 16);

        private int ResolveClusterPadding(int clusterSize)
            => Mathf.Clamp(_settings?.Volume.ClusterPaddingCells ?? 1, 0, clusterSize);

        private bool ShouldLogClusterUpdates()
            => _settings == null || _settings.Volume.LogClusterUpdates;

        private static int PositiveModulo(int value, int modulo)
        {
            int result = value % modulo;
            return result < 0 ? result + modulo : result;
        }
    }
}
