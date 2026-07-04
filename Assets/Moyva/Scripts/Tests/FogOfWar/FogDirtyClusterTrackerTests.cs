using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    [TestFixture]
    public sealed class FogDirtyClusterTrackerTests
    {
        [Test]
        public void MarkChanges_MapsCellToCluster()
        {
            var settings = CreateSettings();
            var tracker = new FogDirtyClusterTracker(settings);

            tracker.MarkChanges(new[]
            {
                new FogCellVisualChange(new Vector2Int(0, 0), FogStateType.Unexplored, FogStateType.Visible, 0, 0)
            }, CreateContext(40, 40));

            var clusters = new HashSet<FogClusterKey>(tracker.ConsumeDirtyClusters());

            Assert.IsTrue(clusters.Contains(new FogClusterKey(0, 0)));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void MarkChanges_AddsPaddingNeighborClustersNearBoundary()
        {
            var settings = CreateSettings();
            var tracker = new FogDirtyClusterTracker(settings);

            tracker.MarkChanges(new[]
            {
                new FogCellVisualChange(new Vector2Int(15, 15), FogStateType.Unexplored, FogStateType.Visible, 0, 0)
            }, CreateContext(40, 40));

            var clusters = new HashSet<FogClusterKey>(tracker.ConsumeDirtyClusters());

            Assert.IsTrue(clusters.Contains(new FogClusterKey(0, 0)));
            Assert.IsTrue(clusters.Contains(new FogClusterKey(1, 0)));
            Assert.IsTrue(clusters.Contains(new FogClusterKey(0, 1)));
            Assert.IsTrue(clusters.Contains(new FogClusterKey(1, 1)));
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void MarkChanges_DeduplicatesDuplicateChanges()
        {
            var settings = CreateSettings();
            var tracker = new FogDirtyClusterTracker(settings);
            var change = new FogCellVisualChange(new Vector2Int(16, 16), FogStateType.Visible, FogStateType.Explored, 0, 0);

            tracker.MarkChanges(new[] { change, change }, CreateContext(40, 40));

            var clusters = tracker.ConsumeDirtyClusters();

            Assert.AreEqual(1, clusters.Count);
            Assert.AreEqual(new FogClusterKey(1, 1), clusters[0]);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void MarkChanges_DoesNotCreateVisibleMeshKeys()
        {
            var settings = CreateSettings();
            var tracker = new FogDirtyClusterTracker(settings);

            tracker.MarkChanges(new[]
            {
                new FogCellVisualChange(new Vector2Int(3, 3), FogStateType.Unexplored, FogStateType.Visible, 2, 2)
            }, CreateContext(40, 40));

            var clusters = new HashSet<FogClusterKey>(tracker.ConsumeDirtyClusters());

            Assert.IsTrue(clusters.Contains(new FogClusterKey(0, 0)));

            tracker.MarkChanges(new[]
            {
                new FogCellVisualChange(new Vector2Int(3, 3), FogStateType.Visible, FogStateType.Visible, 2, 3)
            }, CreateContext(40, 40));

            Assert.AreEqual(0, tracker.ConsumeDirtyClusters().Count);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void MarkChanges_HeightChangeMarksOnlySpatialCluster()
        {
            var settings = CreateSettings();
            var tracker = new FogDirtyClusterTracker(settings);

            tracker.MarkChanges(new[]
            {
                new FogCellVisualChange(new Vector2Int(4, 4), FogStateType.Unexplored, FogStateType.Explored, 1, 3)
            }, CreateContext(40, 40));

            var clusters = new HashSet<FogClusterKey>(tracker.ConsumeDirtyClusters());

            Assert.AreEqual(1, clusters.Count);
            Assert.IsTrue(clusters.Contains(new FogClusterKey(0, 0)));
            Object.DestroyImmediate(settings);
        }

        private static FogOfWarSettings CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<FogOfWarSettings>();
            settings.Volume.ClusterSize = 16;
            settings.Volume.ClusterPaddingCells = 1;
            settings.Volume.LogClusterUpdates = false;
            settings.Volume.EnsureDefaults();
            return settings;
        }

        private static FogWorldVisualContext CreateContext(int width, int height)
        {
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f,
                false,
                default,
                null,
                null);
        }
    }
}
