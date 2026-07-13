using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class ChunkFirstGenerationTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _created.Count; i++)
            {
                if (_created[i] != null)
                    Object.DestroyImmediate(_created[i]);
            }

            _created.Clear();
        }

        [Test]
        public void GraphLogicalTileMap_PreservesStackSamplesAndCompatibilityProjection()
        {
            var map = new GraphLogicalTileMap(2, 2);
            var lower = Sample("lower", "Grass", LayerKind.BaseTerrain, terrainPriority: 10, sortingOrder: 0);
            var upper = Sample("upper", "Flowers", LayerKind.OverlayTerrain, terrainPriority: 100, sortingOrder: 99);

            map.AddSample(0, 0, lower);
            map.AddSample(0, 0, upper);

            Assert.AreEqual(2, map.GetCellStack(0, 0).Count);
            Assert.AreEqual("Grass", map.TileIds[0, 0]);
            Assert.AreEqual("lower", map.GraphLayerIds[0, 0]);
        }

        [Test]
        public void Resolver_HighSortingOverlayDoesNotBecomeMainTerrain()
        {
            var cell = new TileStackCell();
            cell.Add(Sample("grass", "Grass", LayerKind.BaseTerrain, terrainPriority: 10, sortingOrder: 0));
            cell.Add(Sample("flowers", "Flowers", LayerKind.OverlayTerrain, terrainPriority: 1000, sortingOrder: 999));
            var neighborhood = new TileNeighborhood(cell, null, null, null, null, null, null, null, null);

            var resolved = new ResolvedTileCompositionResolver().Resolve(new Vector2Int(1, 1), neighborhood);

            Assert.IsTrue(resolved.HasMainTerrain);
            Assert.AreEqual("Grass", resolved.MainTerrain.TileId);
            Assert.IsTrue(resolved.HasOverlay);
            Assert.AreEqual("Flowers", resolved.Overlay.TileId);
        }

        [Test]
        public void Resolver_TerrainPriorityBeatsSortingOrder()
        {
            var cell = new TileStackCell();
            cell.Add(Sample("sand", "Sand", LayerKind.BaseTerrain, terrainPriority: 10, sortingOrder: 500));
            cell.Add(Sample("grass", "Grass", LayerKind.BaseTerrain, terrainPriority: 80, sortingOrder: 1));
            var neighborhood = new TileNeighborhood(cell, null, null, null, null, null, null, null, null);

            var resolved = new ResolvedTileCompositionResolver().Resolve(new Vector2Int(1, 1), neighborhood);

            Assert.IsTrue(resolved.HasMainTerrain);
            Assert.AreEqual("Grass", resolved.MainTerrain.TileId);
        }

        [Test]
        public void Resolver_ReportsMatchingTerrainNeighborsForTileVariantSelection()
        {
            var center = new TileStackCell();
            center.Add(Sample("water", "Water", LayerKind.BaseTerrain, terrainPriority: 80, sortingOrder: 1));
            var north = new TileStackCell();
            north.Add(Sample("water", "Water", LayerKind.BaseTerrain, terrainPriority: 80, sortingOrder: 1));
            var east = new TileStackCell();
            east.Add(Sample("grass", "Grass", LayerKind.BaseTerrain, terrainPriority: 80, sortingOrder: 1));
            var neighborhood = new TileNeighborhood(center, north, east, null, null, null, null, null, null);

            var resolved = new ResolvedTileCompositionResolver().Resolve(new Vector2Int(1, 1), neighborhood);

            Assert.IsTrue(resolved.NorthMatches);
            Assert.IsFalse(resolved.EastMatches);
            Assert.IsFalse(resolved.SouthMatches);
            Assert.IsFalse(resolved.WestMatches);
        }

        [Test]
        public void ChunkBuildAreaPlanner_UsesExistingMapChunksAndAddsClampedHalo()
        {
            var settings = ScriptableObject.CreateInstance<MapChunkSettingsSO>();
            _created.Add(settings);
            settings.ChunkSize = 4;
            var planner = new ChunkBuildAreaPlanner(new MapChunkLayoutService(settings));

            var areas = planner.Build(10, 10, 1f, false, default, 1);

            Assert.AreEqual(9, areas.Count);
            Assert.AreEqual(new RectInt(0, 0, 4, 4), areas[0].CoreRect);
            Assert.AreEqual(new RectInt(0, 0, 5, 5), areas[0].SampleRect);
            Assert.AreEqual(new RectInt(8, 8, 2, 2), areas[8].CoreRect);
            Assert.AreEqual(new RectInt(7, 7, 3, 3), areas[8].SampleRect);
        }

        [Test]
        public void StableHash_DoesNotUseChunkCoordForTileIdentity()
        {
            var cell = new Vector2Int(5, 7);

            uint a = ChunkFirstStableHash.TileVariant(123, cell, "ground", "grass", 0, "tile");
            uint b = ChunkFirstStableHash.TileVariant(123, cell, "ground", "grass", 0, "tile");

            Assert.AreEqual(a, b);
        }

        [Test]
        public void TwcConfigurationMasks_PreserveNeighborBitLayoutWithoutTemporaryArrays()
        {
            var composition = new ResolvedTileComposition(
                Vector2Int.zero,
                default,
                default,
                false,
                false,
                string.Empty,
                northMatches: true,
                eastMatches: true,
                southMatches: true,
                westMatches: false,
                northEastMatches: false,
                southEastMatches: true,
                southWestMatches: false,
                northWestMatches: true);

            int normal = TwcTileMeshSourceProvider.BuildNormalConfiguration(composition);
            int dual = TwcTileMeshSourceProvider.BuildDualConfiguration(
                topLeft: true,
                topRight: false,
                bottomLeft: true,
                bottomRight: true);

            Assert.AreEqual(1 | 2 | 16 | 32 | 128 | 256, normal);
            Assert.AreEqual(1 | 4 | 8, dual);
        }

        [Test]
        public void ChunkFirstPolicy_RoutesLegacySerializedNameToChunkFirst()
        {
            var policy = new TileWorldCreatorTerrainBuildPolicyResult(
                TileWorldCreatorTerrainBuildMode.MergedChunksWithPrecomputedHeights,
                8,
                true);

            Assert.IsTrue(policy.UsesChunkFirstComposite);
            Assert.IsTrue(policy.UsesPrecomputedHeights);
            Assert.IsFalse(policy.UsesLegacyHeightProjection);
        }

        [Test]
        public void ChunkFirstMeshRegistry_DestroysRegisteredRuntimeMeshes()
        {
            var registry = new ChunkFirstRuntimeMeshRegistry();
            var mesh = new Mesh { name = "ChunkFirst Test Mesh" };
            registry.Register(mesh);

            registry.Clear();

            Assert.IsTrue(mesh == null);
        }

        private static GraphTileLayerSample Sample(
            string id,
            string tileId,
            LayerKind kind,
            int terrainPriority,
            int sortingOrder)
        {
            return new GraphTileLayerSample(
                id,
                id,
                id + "-blueprint",
                id + "-build",
                tileId,
                tileId,
                kind,
                sortingOrder,
                sortingOrder,
                terrainPriority,
                0f,
                0f,
                id + "-node");
        }
    }
}
