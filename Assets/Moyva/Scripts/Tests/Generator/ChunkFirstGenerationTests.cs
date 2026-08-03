using System.Collections.Generic;
using System.Text.RegularExpressions;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

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
        public void Resolver_HigherTerrainWinsOverLowerBaseTerrain()
        {
            var cell = new TileStackCell();
            cell.Add(new GraphTileLayerSample(
                "ground",
                "Ground",
                "ground-blueprint",
                "ground-build",
                "Grass",
                "Grass",
                LayerKind.BaseTerrain,
                sortingOrder: 500,
                graphLayerOrder: 500,
                terrainPriority: 1000,
                height: 0f,
                surfaceHeight: 1f,
                sourceNodeId: "ground-node"));
            cell.Add(new GraphTileLayerSample(
                "plateau",
                "Plateau",
                "plateau-blueprint",
                "plateau-build",
                "Cliff",
                "Cliff",
                LayerKind.Cliff,
                sortingOrder: 0,
                graphLayerOrder: 0,
                terrainPriority: 1,
                height: 3f,
                surfaceHeight: 4f,
                sourceNodeId: "plateau-node"));

            var neighborhood = new TileNeighborhood(
                cell,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var resolved = new ResolvedTileCompositionResolver()
                .Resolve(new Vector2Int(1, 1), neighborhood);

            Assert.IsTrue(resolved.HasMainTerrain);
            Assert.AreEqual("Cliff", resolved.MainTerrain.TileId);
            Assert.AreEqual(3f, resolved.MainTerrain.Height, 0.0001f);
        }

        [Test]
        public void GraphLogicalTileMap_PublishesSurfaceOfRenderedElevatedWinner()
        {
            var map = new GraphLogicalTileMap(1, 1);
            map.AddSample(
                0,
                0,
                new GraphTileLayerSample(
                    "lower",
                    "Lower",
                    "lower-blueprint",
                    "lower-build",
                    "Grass",
                    "Grass",
                    LayerKind.BaseTerrain,
                    sortingOrder: 500,
                    graphLayerOrder: 500,
                    terrainPriority: 1000,
                    height: 0f,
                    surfaceHeight: 1f,
                    sourceNodeId: "lower-node"));
            map.AddSample(
                0,
                0,
                new GraphTileLayerSample(
                    "raised",
                    "Raised",
                    "raised-blueprint",
                    "raised-build",
                    "Cliff",
                    "Cliff",
                    LayerKind.Cliff,
                    sortingOrder: 0,
                    graphLayerOrder: 0,
                    terrainPriority: 1,
                    height: 2f,
                    surfaceHeight: 3f,
                    sourceNodeId: "raised-node"));

            Assert.AreEqual("Cliff", map.TileIds[0, 0]);
            Assert.AreEqual("raised", map.GraphLayerIds[0, 0]);
            Assert.AreEqual(2f, map.LayerHeights[0, 0], 0.0001f);
            Assert.AreEqual(3f, map.SurfaceHeights[0, 0], 0.0001f);
        }

        [Test]
        public void GraphLogicalTileMap_OverlapWinnerIgnoresPrefabPivotBaseHeight()
        {
            var map = new GraphLogicalTileMap(1, 1);
            map.AddSample(
                0,
                0,
                new GraphTileLayerSample(
                    "high-base-low-surface",
                    "High Base Low Surface",
                    "high-base-blueprint",
                    "high-base-build",
                    "Recessed",
                    "Recessed",
                    LayerKind.Cliff,
                    sortingOrder: 100,
                    graphLayerOrder: 100,
                    terrainPriority: 100,
                    height: 2f,
                    surfaceHeight: 1f,
                    sourceNodeId: "recessed-node"));
            map.AddSample(
                0,
                0,
                new GraphTileLayerSample(
                    "low-base-high-surface",
                    "Low Base High Surface",
                    "low-base-blueprint",
                    "low-base-build",
                    "Visible",
                    "Visible",
                    LayerKind.BaseTerrain,
                    sortingOrder: 0,
                    graphLayerOrder: 0,
                    terrainPriority: 0,
                    height: 0f,
                    surfaceHeight: 1.5f,
                    sourceNodeId: "visible-node"));

            Assert.AreEqual("Visible", map.TileIds[0, 0]);
            Assert.AreEqual(
                "low-base-high-surface",
                map.GraphLayerIds[0, 0]);
            Assert.AreEqual(1.5f, map.SurfaceHeights[0, 0], 0.0001f);

            ResolvedTileComposition resolved =
                new ResolvedTileCompositionResolver().Resolve(
                    Vector2Int.zero,
                    new TileNeighborhood(
                        map.GetCellStack(0, 0),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null));
            Assert.AreEqual(
                "low-base-high-surface",
                resolved.MainTerrain.GraphLayerId);
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
        public void TwcTileTypeResolution_UsesPresetGridType()
        {
            int normalConfiguration =
                TileConfigurations.NRMGRD_edgeFill_configurations[0];
            int dualConfiguration =
                TileConfigurations.DUALGRD_edge_configurations[0];

            TilePreset.TileType normalType =
                TwcTileMeshSourceProvider.ResolveTileType(
                    TilePreset.GridType.standard,
                    normalConfiguration,
                    out _);

            TilePreset.TileType dualType =
                TwcTileMeshSourceProvider.ResolveTileType(
                    TilePreset.GridType.dual,
                    dualConfiguration,
                    out _);

            TilePreset.TileType normalConfigurationAsDual =
                TwcTileMeshSourceProvider.ResolveTileType(
                    TilePreset.GridType.dual,
                    normalConfiguration,
                    out _);

            Assert.AreEqual(
                TilePreset.TileType.NRMGRD_edgeFill,
                normalType);
            Assert.AreEqual(
                TilePreset.TileType.DUALGRD_edge,
                dualType);
            Assert.AreEqual(
                TilePreset.TileType.none,
                normalConfigurationAsDual);
        }

        [Test]
        public void TwcSurfaceAlignedPlacement_NormalizesVariantTopOffsets()
        {
            const float expectedSurface = 1f;

            float regularRoot = TwcTileMeshSourceProvider.ResolveSurfaceAlignedPlacementHeight(
                expectedSurface,
                fallbackPlacementHeight: 1f,
                prefabTopOffset: 0f);
            float loweredPivotRoot = TwcTileMeshSourceProvider.ResolveSurfaceAlignedPlacementHeight(
                expectedSurface,
                fallbackPlacementHeight: 1f,
                prefabTopOffset: -1f);

            Assert.AreEqual(expectedSurface, regularRoot + 0f, 0.0001f);
            Assert.AreEqual(expectedSurface, loweredPivotRoot - 1f, 0.0001f);
            Assert.AreEqual(2f, loweredPivotRoot, 0.0001f);
        }

        [Test]
        public void TwcSurfaceAlignedPlacement_PreservesSurfaceDeltaAcrossElevations()
        {
            float lowerRoot =
                TwcTileMeshSourceProvider.ResolveSurfaceAlignedPlacementHeight(
                    expectedSurfaceHeight: 1f,
                    fallbackPlacementHeight: 1f,
                    prefabTopOffset: -1f);
            float higherRoot =
                TwcTileMeshSourceProvider.ResolveSurfaceAlignedPlacementHeight(
                    expectedSurfaceHeight: 3f,
                    fallbackPlacementHeight: 3f,
                    prefabTopOffset: -1f);

            float lowerTop = lowerRoot - 1f;
            float higherTop = higherRoot - 1f;

            Assert.AreEqual(1f, lowerTop, 0.0001f);
            Assert.AreEqual(3f, higherTop, 0.0001f);
            Assert.AreEqual(2f, higherTop - lowerTop, 0.0001f);
        }

        [Test]
        public void TwcSurfaceAlignedPlacement_FallsBackForInvalidSurfaceData()
        {
            float resolved = TwcTileMeshSourceProvider.ResolveSurfaceAlignedPlacementHeight(
                float.NaN,
                fallbackPlacementHeight: 3f,
                prefabTopOffset: -1f);

            Assert.AreEqual(3f, resolved, 0.0001f);
        }

        [Test]
        public void TwcPrefabAlignment_UsesTopOfAllChildMeshes()
        {
            Mesh lower = CreateFlatQuadMesh(1f);
            Mesh upper = CreateFlatQuadMesh(1f);
            var templates = new[]
            {
                new TwcTileMeshSourceProvider.PrefabMeshTemplate(
                    lower,
                    Matrix4x4.identity,
                    null),
                new TwcTileMeshSourceProvider.PrefabMeshTemplate(
                    upper,
                    Matrix4x4.Translate(new Vector3(0f, 3f, 0f)),
                    null)
            };

            float top = TwcTileMeshSourceProvider.ResolveAggregateTransformedBoundsTop(
                templates,
                Matrix4x4.identity);

            Assert.AreEqual(4f, top, 0.0001f);
        }

        [Test]
        public void TwcSurfaceOnly_PrefersDedicatedFlatTemplateOverVolumeTemplate()
        {
            Mesh volume = CreateClosedTileMeshWithChannels();
            Mesh surface = CreateFlatQuadMesh(1f);
            Mesh lowerSurface = CreateFlatQuadMesh(0f);
            var volumeTemplate = new TwcTileMeshSourceProvider.PrefabMeshTemplate(
                volume,
                Matrix4x4.identity,
                null);
            var surfaceTemplate = new TwcTileMeshSourceProvider.PrefabMeshTemplate(
                surface,
                Matrix4x4.identity,
                null);
            var lowerSurfaceTemplate = new TwcTileMeshSourceProvider.PrefabMeshTemplate(
                lowerSurface,
                Matrix4x4.identity,
                null);
            var templates = new[]
            {
                volumeTemplate,
                surfaceTemplate,
                lowerSurfaceTemplate
            };

            Assert.IsTrue(
                TwcTileMeshSourceProvider.HasFlatSurfaceTemplate(
                    templates,
                    Matrix4x4.identity));
            Assert.IsFalse(
                TwcTileMeshSourceProvider.IsFlatSurfaceTemplate(
                    volumeTemplate,
                    Matrix4x4.identity));
            Assert.IsTrue(
                TwcTileMeshSourceProvider.IsFlatSurfaceTemplate(
                    surfaceTemplate,
                    Matrix4x4.identity));
            Assert.IsFalse(
                TwcTileMeshSourceProvider.ShouldIncludeMeshTemplate(
                    volumeTemplate,
                    Matrix4x4.identity,
                    flatSurfaceTemplatesOnly: true,
                    selectedSurfaceTop: 1f));
            Assert.IsTrue(
                TwcTileMeshSourceProvider.ShouldIncludeMeshTemplate(
                    surfaceTemplate,
                    Matrix4x4.identity,
                    flatSurfaceTemplatesOnly: true,
                    selectedSurfaceTop: 1f));
            Assert.IsFalse(
                TwcTileMeshSourceProvider.ShouldIncludeMeshTemplate(
                    lowerSurfaceTemplate,
                    Matrix4x4.identity,
                    flatSurfaceTemplatesOnly: true,
                    selectedSurfaceTop: 1f),
                "Lower decorative or bottom planes must not survive SurfaceOnly selection.");
            Assert.IsTrue(
                TwcTileMeshSourceProvider.ShouldIncludeMeshTemplate(
                    volumeTemplate,
                    Matrix4x4.identity,
                    flatSurfaceTemplatesOnly: false,
                    selectedSurfaceTop: float.NaN),
                "A SurfaceOnly prefab without a dedicated plane must retain the volume-top fallback.");
        }

        [Test]
        public void GraphLogicalLayerHeight_PreservesGraphHeightAndTwcSurfaceOffset()
        {
            float noOffset = GraphLogicalTileMapBuilderService.ResolveAuthoritativeSurfaceHeight(
                graphLayerHeight: 1f,
                blueprintLayerHeight: 0.05f,
                projectedSurfaceHeight: 0.05f);
            float withOffset = GraphLogicalTileMapBuilderService.ResolveAuthoritativeSurfaceHeight(
                graphLayerHeight: 1f,
                blueprintLayerHeight: 0.05f,
                projectedSurfaceHeight: 0.30f);

            Assert.AreEqual(1f, noOffset, 0.0001f);
            Assert.AreEqual(1.25f, withOffset, 0.0001f);
        }

        [Test]
        public void GraphLogicalLayer_PreservesExplicitGeometryPolicy()
        {
            var data = new GraphLogicalTileLayerData(
                "water",
                "Water",
                "water",
                layerHeight: 0f,
                surfaceHeight: 0.2f,
                tileGeometryMode: TileGeometryMode.SurfaceOnly,
                authoredClosurePolicy: AuthoredClosurePolicy.PreserveAuthored);

            GraphTileLayerSample sample = data.ToSample();

            Assert.AreEqual(TileGeometryMode.SurfaceOnly, sample.TileGeometryMode);
            Assert.AreEqual(
                AuthoredClosurePolicy.PreserveAuthored,
                sample.AuthoredClosurePolicy);
        }

        [Test]
        public void TwcFillSurfaceHeight_UsesPresetGridType()
        {
            var standard = ScriptableObject.CreateInstance<TilePreset>();
            var dual = ScriptableObject.CreateInstance<TilePreset>();
            _created.Add(standard);
            _created.Add(dual);
            standard.gridtype = TilePreset.GridType.standard;
            dual.gridtype = TilePreset.GridType.dual;

            Assert.AreEqual(
                TilePreset.TileType.NRMGRD_fill,
                TileWorldCreatorFillTileSurfaceHeightUtility.ResolveFillTileType(standard));
            Assert.AreEqual(
                TilePreset.TileType.DUALGRD_fill,
                TileWorldCreatorFillTileSurfaceHeightUtility.ResolveFillTileType(dual));
        }

        [Test]
        public void TwcOccludedSides_UsesMatchingCardinalNeighbors()
        {
            var composition = new ResolvedTileComposition(
                Vector2Int.zero,
                default,
                default,
                true,
                false,
                string.Empty,
                northMatches: true,
                eastMatches: true,
                southMatches: false,
                westMatches: false);

            TileMeshOccludedSides sides =
                TwcTileMeshSourceProvider.ResolveOccludedSides(composition);

            Assert.IsTrue((sides & TileMeshOccludedSides.North) != 0);
            Assert.IsTrue((sides & TileMeshOccludedSides.East) != 0);
            Assert.IsFalse((sides & TileMeshOccludedSides.South) != 0);
            Assert.IsFalse((sides & TileMeshOccludedSides.West) != 0);
        }

        [Test]
        public void TwcDualOccludedSides_UsesOccupiedQuadrantPairs()
        {
            TileMeshOccludedSides sides =
                TwcTileMeshSourceProvider.ResolveDualOccludedSides(
                    topLeft: true,
                    topRight: true,
                    bottomLeft: true,
                    bottomRight: false);

            Assert.IsTrue((sides & TileMeshOccludedSides.North) != 0);
            Assert.IsTrue((sides & TileMeshOccludedSides.West) != 0);
            Assert.IsFalse((sides & TileMeshOccludedSides.East) != 0);
            Assert.IsFalse((sides & TileMeshOccludedSides.South) != 0);
        }

        [Test]
        public void TwcDualOwnership_DeduplicatesOnlyMatchingTerrainIdentity()
        {
            bool differentIdentityOwnsComplementaryFragment =
                TwcTileMeshSourceProvider.ShouldCurrentOwnDualFragment(
                    westMatchesIdentity: false,
                    southMatchesIdentity: false,
                    southWestMatchesIdentity: false);
            bool matchingIdentityDuplicatesFragment =
                TwcTileMeshSourceProvider.ShouldCurrentOwnDualFragment(
                    westMatchesIdentity: true,
                    southMatchesIdentity: false,
                    southWestMatchesIdentity: false);

            Assert.IsTrue(differentIdentityOwnsComplementaryFragment);
            Assert.IsFalse(matchingIdentityDuplicatesFragment);
        }

        [Test]
        public void TwcDualGrid_DifferentTerrainLayersBothEmitSharedSeamFragments()
        {
            TwcTileMeshSourceProvider provider =
                CreateDualGridProvider("terrain-a", "terrain-b");
            var westSources = new List<TileMeshSource>();
            var eastSources = new List<TileMeshSource>();
            GraphTileLayerSample westSample =
                CreateHeightSample("terrain-a", 0f);
            GraphTileLayerSample eastSample =
                CreateHeightSample("terrain-b", 0f);
            var west = new ResolvedTileComposition(
                Vector2Int.zero,
                westSample,
                default,
                true,
                false,
                string.Empty,
                eastMatches: false,
                eastSurfaceHeight: 0f);
            var east = new ResolvedTileComposition(
                Vector2Int.right,
                eastSample,
                default,
                true,
                false,
                string.Empty,
                westMatches: false,
                westSurfaceHeight: 0f);

            provider.CollectMeshSources(west, westSources);
            provider.CollectMeshSources(east, eastSources);

            Assert.AreEqual(4, westSources.Count);
            Assert.AreEqual(4, eastSources.Count);
            Assert.AreEqual(2, CountSourcesAtX(westSources, 0.5f));
            Assert.AreEqual(2, CountSourcesAtX(eastSources, 0.5f));
        }

        [Test]
        public void TwcDualGrid_SameTerrainLayerEmitsSharedSeamFragmentsOnce()
        {
            TwcTileMeshSourceProvider provider =
                CreateDualGridProvider("terrain-a");
            var westSources = new List<TileMeshSource>();
            var eastSources = new List<TileMeshSource>();
            GraphTileLayerSample sample =
                CreateHeightSample("terrain-a", 0f);
            var west = new ResolvedTileComposition(
                Vector2Int.zero,
                sample,
                default,
                true,
                false,
                string.Empty,
                eastMatches: true,
                eastSurfaceHeight: 0f);
            var east = new ResolvedTileComposition(
                Vector2Int.right,
                sample,
                default,
                true,
                false,
                string.Empty,
                westMatches: true,
                westSurfaceHeight: 0f);

            provider.CollectMeshSources(west, westSources);
            provider.CollectMeshSources(east, eastSources);

            Assert.AreEqual(2, CountSourcesAtX(westSources, 0.5f));
            Assert.AreEqual(0, CountSourcesAtX(eastSources, 0.5f));
        }

        [Test]
        public void TwcOccludedSides_UsesNeighborSurfaceInsteadOfLayerIdentity()
        {
            GraphTileLayerSample main = CreateHeightSample("high", 2f);
            var composition = new ResolvedTileComposition(
                Vector2Int.zero,
                main,
                default,
                true,
                false,
                string.Empty,
                northSurfaceHeight: 2f,
                eastSurfaceHeight: 3f,
                southSurfaceHeight: 1f);

            TileMeshOccludedSides sides =
                TwcTileMeshSourceProvider.ResolveOccludedSides(composition);

            Assert.IsTrue((sides & TileMeshOccludedSides.North) != 0);
            Assert.IsTrue((sides & TileMeshOccludedSides.East) != 0);
            Assert.IsFalse((sides & TileMeshOccludedSides.South) != 0);
        }

        [Test]
        public void TwcHeightTransition_HasExactlyOneDeterministicWallOwner()
        {
            var high = new ResolvedTileComposition(
                Vector2Int.zero,
                CreateHeightSample("high", 2f),
                default,
                true,
                false,
                string.Empty,
                eastSurfaceHeight: 1f);
            var low = new ResolvedTileComposition(
                Vector2Int.right,
                CreateHeightSample("low", 1f),
                default,
                true,
                false,
                string.Empty,
                westSurfaceHeight: 2f);

            bool highEmitsEast =
                (TwcTileMeshSourceProvider.ResolveOccludedSides(high)
                 & TileMeshOccludedSides.East) == 0;
            bool lowEmitsWest =
                (TwcTileMeshSourceProvider.ResolveOccludedSides(low)
                 & TileMeshOccludedSides.West) == 0;

            Assert.IsTrue(highEmitsEast);
            Assert.IsFalse(lowEmitsWest);
        }

        [Test]
        public void TwcVisibleBottom_UsesResolvedSupportHeight()
        {
            var composition = new ResolvedTileComposition(
                Vector2Int.zero,
                default,
                default,
                true,
                false,
                string.Empty,
                supportHeight: 0.75f);

            Assert.AreEqual(
                0.75f,
                TwcTileMeshSourceProvider.ResolveVisibleBottomY(composition),
                0.0001f);
        }

        [Test]
        public void Resolver_SupportHeight_UsesRenderedSurfaceNotBaseHeight()
        {
            var cell = new TileStackCell();
            cell.Add(new GraphTileLayerSample(
                "lower",
                "Lower",
                "lower-blueprint",
                "lower-build",
                "Lower",
                "Lower",
                LayerKind.BaseTerrain,
                sortingOrder: 0,
                graphLayerOrder: 0,
                terrainPriority: 0,
                height: 2f,
                surfaceHeight: 1f,
                sourceNodeId: "lower-node"));
            cell.Add(new GraphTileLayerSample(
                "upper",
                "Upper",
                "upper-blueprint",
                "upper-build",
                "Upper",
                "Upper",
                LayerKind.Cliff,
                sortingOrder: 1,
                graphLayerOrder: 1,
                terrainPriority: 1,
                height: 2f,
                surfaceHeight: 3f,
                sourceNodeId: "upper-node"));

            ResolvedTileComposition resolved =
                new ResolvedTileCompositionResolver().Resolve(
                    Vector2Int.zero,
                    new TileNeighborhood(
                        cell,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null,
                        null));

            Assert.IsTrue(resolved.HasMainTerrain);
            Assert.AreEqual("upper", resolved.MainTerrain.GraphLayerId);
            Assert.AreEqual(1f, resolved.SupportHeight, 0.0001f);
        }

        [Test]
        public void VerticalFill_CullsOnlyMatchedCardinalBoundary()
        {
            var source = new TileMeshSource(
                null,
                null,
                Matrix4x4.identity,
                visibleBottomY: 0f,
                occludedSides: TileMeshOccludedSides.North,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f);

            bool northHidden = TileVerticalFillMeshUtility.IsBoundaryEdgeOccluded(
                source,
                new Vector3(-0.5f, 1f, 0.5f),
                new Vector3(0.5f, 1f, 0.5f));
            bool westVisible = TileVerticalFillMeshUtility.IsBoundaryEdgeOccluded(
                source,
                new Vector3(-0.5f, 1f, -0.5f),
                new Vector3(-0.5f, 1f, 0.5f));

            Assert.IsTrue(northHidden);
            Assert.IsFalse(westVisible);
        }

        [Test]
        public void VerticalFill_FullySurroundedFlatTileAddsNoInternalSkirts()
        {
            Mesh mesh = CreateFlatQuadMesh();
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.identity,
                visibleBottomY: 0f,
                occludedSides: TileMeshOccludedSides.North
                    | TileMeshOccludedSides.East
                    | TileMeshOccludedSides.South
                    | TileMeshOccludedSides.West,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f);

            bool created = TileVerticalFillMeshUtility.TryCreate(
                source,
                out Mesh result);

            Assert.IsFalse(created);
            Assert.IsNull(result);
        }

        [Test]
        public void VerticalFill_BoundaryTileAddsSkirtOnlyOnExposedSide()
        {
            Mesh mesh = CreateFlatQuadMesh();
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.identity,
                visibleBottomY: 0f,
                occludedSides: TileMeshOccludedSides.North
                    | TileMeshOccludedSides.South
                    | TileMeshOccludedSides.West,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f);

            bool created = TileVerticalFillMeshUtility.TryCreate(
                source,
                out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.IsTrue(created);
            Assert.IsNotNull(result);
            Assert.AreEqual(12, result.triangles.Length);
            Assert.AreEqual(8, result.vertexCount);
        }

        [Test]
        public void VerticalFill_PreserveAuthoredDoesNotDeformVolumeMesh()
        {
            Mesh mesh = CreateTwoLevelTriangleMesh();
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.identity,
                visibleBottomY: 0f,
                authoredClosurePolicy: AuthoredClosurePolicy.PreserveAuthored);

            bool created = TileVerticalFillMeshUtility.TryCreate(source, out Mesh result);

            Assert.IsFalse(created);
            Assert.IsNull(result);
        }

        [Test]
        public void VerticalFill_PreserveAuthoredAddsOnlyMissingClosureBelowVolume()
        {
            Mesh mesh = CreateTwoLevelTriangleMesh(
                topY: 0.515f,
                bottomY: -0.535f);
            Vector3[] authoredVertices = mesh.vertices;
            Matrix4x4 placement = Matrix4x4.Translate(
                new Vector3(0f, 2f, 0f));
            var source = new TileMeshSource(
                mesh,
                null,
                placement,
                visibleBottomY: 0.515f,
                occludedSides: TileMeshOccludedSides.North
                    | TileMeshOccludedSides.South
                    | TileMeshOccludedSides.West,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f,
                authoredClosurePolicy:
                    AuthoredClosurePolicy.PreserveAuthored,
                edgeBottoms: new TileMeshEdgeBottoms(
                    north: 1f,
                    east: 0.515f,
                    south: 1f,
                    west: 1f),
                generateMissingClosure: true);

            bool created = TileVerticalFillMeshUtility.TryCreate(
                source,
                out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.IsTrue(created);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.subMeshCount);
            Assert.AreEqual(12, result.triangles.Length);
            Assert.AreEqual(-1.485f, result.bounds.min.y, 0.0001f);
            Assert.AreEqual(0.515f, result.bounds.max.y, 0.0001f);
            Assert.AreEqual(
                0.515f,
                placement.MultiplyPoint3x4(result.bounds.min).y,
                0.0001f);
            Assert.AreEqual(
                2.515f,
                placement.MultiplyPoint3x4(result.bounds.max).y,
                0.0001f);
            Assert.That(mesh.vertices, Is.EqualTo(authoredVertices));
        }

        [Test]
        public void VerticalFill_PreserveAuthoredFullyOccludedAddsNoClosure()
        {
            Mesh mesh = CreateTwoLevelTriangleMesh();
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.Translate(new Vector3(0f, 2f, 0f)),
                visibleBottomY: 0.515f,
                occludedSides: TileMeshOccludedSides.North
                    | TileMeshOccludedSides.East
                    | TileMeshOccludedSides.South
                    | TileMeshOccludedSides.West,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f,
                authoredClosurePolicy:
                    AuthoredClosurePolicy.PreserveAuthored,
                generateMissingClosure: true);

            bool created = TileVerticalFillMeshUtility.TryCreate(
                source,
                out Mesh result);

            Assert.IsFalse(created);
            Assert.IsNull(result);
        }

        [Test]
        public void TwcProvider_RaisedPreserveAuthoredCliffClosesToResolvedSupport()
        {
            var managerObject = new GameObject("Raised Cliff Test Manager");
            _created.Add(managerObject);
            var manager =
                managerObject.AddComponent<TileWorldCreatorManager>();
            var configuration =
                ScriptableObject.CreateInstance<Configuration>();
            _created.Add(configuration);
            configuration.cellSize = 1f;
            manager.configuration = configuration;

            Mesh authoredMesh = CreateTwoLevelTriangleMesh(
                topY: 0.515f,
                bottomY: -0.535f);
            Vector3[] authoredVertices = authoredMesh.vertices;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            Assert.IsNotNull(shader);
            var material = new Material(shader);
            _created.Add(material);
            var prefab = new GameObject("Raised Authored Cliff");
            _created.Add(prefab);
            prefab.AddComponent<MeshFilter>().sharedMesh = authoredMesh;
            prefab.AddComponent<MeshRenderer>().sharedMaterial = material;

            var preset = ScriptableObject.CreateInstance<TilePreset>();
            _created.Add(preset);
            preset.name = "Raised Authored Cliff Preset";
            preset.tileId = "raised-cliff";
            preset.gridtype = TilePreset.GridType.standard;
            preset.NRMGRD_singleTile = prefab;

            var buildLayer =
                ScriptableObject.CreateInstance<TilesBuildLayer>();
            _created.Add(buildLayer);
            buildLayer.guid = "raised-build";
            buildLayer.assignedBlueprintLayerGuid = "raised-blueprint";
            buildLayer.scaleTileToCellSize = false;
            buildLayer.scaleOffset = Vector3.one;
            buildLayer.tilePresetsTop.Add(
                new TilesBuildLayer.TilePresetSelection
                {
                    preset = preset,
                    weight = 1f
                });
            var folder = new BuildLayerFolder("Raised Cliff");
            folder.buildLayers.Add(buildLayer);
            configuration.buildLayerFolders.Add(folder);

            var mapping =
                ScriptableObject.CreateInstance<TileWorldCreatorIdMappingSO>();
            _created.Add(mapping);
            var provider = new TwcTileMeshSourceProvider(
                new TileWorldCreatorBuildEnvironment(
                    manager,
                    mapping,
                    new TileWorldCreatorBuildOptions()));
            var sample = new GraphTileLayerSample(
                "raised-layer",
                "Raised Layer",
                "raised-blueprint",
                "raised-build",
                "raised-cliff",
                "raised-cliff",
                LayerKind.BaseTerrain,
                sortingOrder: 1,
                graphLayerOrder: 1,
                terrainPriority: 1,
                height: 2f,
                surfaceHeight: 2.515f,
                sourceNodeId: "raised-node",
                tileGeometryMode: TileGeometryMode.SolidTerrain,
                authoredClosurePolicy:
                    AuthoredClosurePolicy.PreserveAuthored);
            var composition = new ResolvedTileComposition(
                Vector2Int.zero,
                sample,
                default,
                true,
                false,
                string.Empty,
                supportHeight: 0.515f);
            var sources = new List<TileMeshSource>();

            int count = provider.CollectMeshSources(
                composition,
                sources);

            Assert.AreEqual(1, count);
            Assert.AreEqual(1, sources.Count);
            TileMeshSource source = sources[0];
            Assert.IsTrue(source.GenerateMissingClosure);
            Assert.AreEqual(2f, source.LocalMatrix.m13, 0.0001f);

            bool created = TileVerticalFillMeshUtility.TryCreate(
                source,
                out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.IsTrue(created);
            Assert.IsNotNull(result);
            Assert.AreEqual(
                0.515f,
                TwcTileMeshSourceProvider.ResolveTransformedBoundsBottom(
                    result.bounds,
                    source.LocalMatrix),
                0.0001f);
            Assert.AreEqual(
                2.515f,
                TwcTileMeshSourceProvider.ResolveTransformedBoundsTop(
                    result.bounds,
                    source.LocalMatrix),
                0.0001f);
            Assert.That(authoredMesh.vertices, Is.EqualTo(authoredVertices));
        }

        [Test]
        public void VerticalFill_RemovesUnreferencedVerticesAfterTriangleCulling()
        {
            Mesh mesh = CreateTwoLevelTriangleMesh();
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.identity,
                visibleBottomY: 0f,
                authoredClosurePolicy: AuthoredClosurePolicy.GeneratedClosure);

            bool created = TileVerticalFillMeshUtility.TryCreate(source, out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.IsTrue(created);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.vertexCount);
            Assert.AreEqual(3, result.triangles.Length);
        }

        [Test]
        public void VerticalFill_HeightTransitionCreatesOnlyDeltaWall()
        {
            Mesh mesh = CreateFlatQuadMesh(2f);
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.identity,
                visibleBottomY: 0f,
                occludedSides: TileMeshOccludedSides.North
                    | TileMeshOccludedSides.South
                    | TileMeshOccludedSides.West,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f,
                edgeBottoms: new TileMeshEdgeBottoms(
                    north: 2f,
                    east: 1f,
                    south: 2f,
                    west: 2f));

            bool created = TileVerticalFillMeshUtility.TryCreate(source, out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.IsTrue(created);
            Assert.AreEqual(12, result.triangles.Length);
            Assert.AreEqual(1f, result.bounds.min.y, 0.0001f);
            Assert.AreEqual(2f, result.bounds.max.y, 0.0001f);
        }

        [Test]
        public void VerticalFill_ElevatedTile_PreservesTopHeightAndExtendsOnlyBottom()
        {
            Mesh mesh = CreateFlatQuadMesh(1f);
            Matrix4x4 placement = Matrix4x4.Translate(new Vector3(0f, 2f, 0f));
            var source = new TileMeshSource(
                mesh,
                null,
                placement,
                visibleBottomY: 0f,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: 0.5f,
                edgeBottoms: new TileMeshEdgeBottoms(
                    north: 0f,
                    east: 0f,
                    south: 0f,
                    west: 0f));

            bool created = TileVerticalFillMeshUtility.TryCreate(source, out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.IsTrue(created);
            Assert.IsNotNull(result);
            Assert.AreEqual(
                0f,
                TwcTileMeshSourceProvider.ResolveTransformedBoundsBottom(
                    result.bounds,
                    source.LocalMatrix),
                0.0001f);
            Assert.AreEqual(
                3f,
                TwcTileMeshSourceProvider.ResolveTransformedBoundsTop(
                    result.bounds,
                    source.LocalMatrix),
                0.0001f);
        }

        [Test]
        public void SurfaceOnly_RemovesAuthoredSidesAndBottomAndPreservesStreams()
        {
            Mesh mesh = CreateClosedTileMeshWithChannels();
            var source = new TileMeshSource(
                mesh,
                null,
                Matrix4x4.Scale(new Vector3(-1f, 1f, 1f)),
                tileGeometryMode: TileGeometryMode.SurfaceOnly);

            SurfaceOnlyMeshBuildStatus status =
                TileSurfaceOnlyMeshUtility.Create(source, out Mesh result);
            if (result != null)
                _created.Add(result);

            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Created, status);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.vertexCount);
            Assert.AreEqual(2, result.subMeshCount);
            Assert.AreEqual(6, result.GetIndices(0).Length);
            Assert.AreEqual(0, result.GetIndices(1).Length);
            Assert.AreEqual(1f, result.bounds.min.y, 0.0001f);
            Assert.AreEqual(1f, result.bounds.max.y, 0.0001f);
            Assert.AreEqual(4, result.normals.Length);
            Assert.AreEqual(4, result.tangents.Length);
            Assert.AreEqual(4, result.colors32.Length);
            for (int channel = 0; channel < 8; channel++)
            {
                var uvs = new List<Vector4>();
                result.GetUVs(channel, uvs);
                Assert.AreEqual(
                    4,
                    uvs.Count,
                    $"UV{channel} must survive SurfaceOnly compaction.");
            }
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

        [Test]
        public void ChunkTerrainMeshBuilder_UsesFinalOptimizedMeshForCollider()
        {
            Mesh sourceMesh = CreateFlatQuadMesh();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            Assert.IsNotNull(shader, "A built-in test shader is required.");
            var material = new Material(shader);
            var chunkObject = new GameObject("Chunk Collider Test");
            _created.Add(material);
            _created.Add(chunkObject);

            var registry = new ChunkFirstRuntimeMeshRegistry();
            var builder = new ChunkTerrainMeshBuilder(
                registry,
                new ChunkFirstBuildDiagnostics());
            var resolved = new Dictionary<Vector2Int, ResolvedTileComposition>
            {
                [Vector2Int.zero] = new ResolvedTileComposition(
                    Vector2Int.zero,
                    CreateHeightSample("ground", 1f),
                    default,
                    true,
                    false,
                    string.Empty)
            };
            var source = new SingleMeshSource(sourceMesh, material);
            var area = new ChunkBuildArea(
                default,
                new RectInt(0, 0, 1, 1),
                new RectInt(0, 0, 1, 1));

            int built = builder.Build(chunkObject.transform, area, resolved, source);

            Transform terrain = chunkObject.transform.Find("TerrainMesh");
            Assert.AreEqual(1, built);
            Assert.IsNotNull(terrain);
            Mesh rendered = terrain.GetComponent<MeshFilter>().sharedMesh;
            Mesh collided = terrain.GetComponent<MeshCollider>().sharedMesh;
            Assert.AreSame(rendered, collided);
            Assert.LessOrEqual(rendered.bounds.min.x, -0.5f);
            Assert.GreaterOrEqual(rendered.bounds.max.x, 0.5f);
            Assert.LessOrEqual(rendered.bounds.min.z, -0.5f);
            Assert.GreaterOrEqual(rendered.bounds.max.z, 0.5f);

            registry.Clear();
        }

        [Test]
        public void ChunkTerrainMeshBuilder_SurfaceOnlyEmitsNoVolumeFaces()
        {
            Mesh sourceMesh = CreateClosedTileMeshWithChannels();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            Assert.IsNotNull(shader, "A built-in test shader is required.");
            var material = new Material(shader);
            var chunkObject = new GameObject("SurfaceOnly Chunk Test");
            _created.Add(material);
            _created.Add(chunkObject);

            var registry = new ChunkFirstRuntimeMeshRegistry();
            var builder = new ChunkTerrainMeshBuilder(
                registry,
                new ChunkFirstBuildDiagnostics());
            var resolved = new Dictionary<Vector2Int, ResolvedTileComposition>
            {
                [Vector2Int.zero] = new ResolvedTileComposition(
                    Vector2Int.zero,
                    CreateHeightSample("water", 1f),
                    default,
                    true,
                    false,
                    string.Empty)
            };
            var source = new SingleMeshSource(
                sourceMesh,
                material,
                TileGeometryMode.SurfaceOnly);
            var area = new ChunkBuildArea(
                default,
                new RectInt(0, 0, 1, 1),
                new RectInt(0, 0, 1, 1));

            LogAssert.Expect(
                LogType.Log,
                new Regex(
                    @"^\[MoyvaChunkFirst\] CHUNK mesh='SurfaceOnly Chunk Test'.*sourceVertices=8, .*sourceTriangles=12, .*emittedVertices=4, .*emittedTriangles=2, culledFaces=10, unreferencedVerticesRemoved=[1-9][0-9]*, exactDuplicateVerticesRemoved=[0-9]+\.$"));

            int built = builder.Build(
                chunkObject.transform,
                area,
                resolved,
                source);

            Transform terrain = chunkObject.transform.Find("TerrainMesh");
            Assert.AreEqual(1, built);
            Assert.IsNotNull(terrain);
            Mesh rendered = terrain.GetComponent<MeshFilter>().sharedMesh;
            Assert.IsNotNull(rendered);
            Assert.AreEqual(6, rendered.triangles.Length);
            foreach (Vector3 vertex in rendered.vertices)
                Assert.AreEqual(1f, vertex.y, 0.0001f);

            registry.Clear();
        }

        [Test]
        public void ChunkTerrainMeshBuilder_PreservesLocalMatrixYDuringCombine()
        {
            Mesh sourceMesh = CreateFlatQuadMesh(1f);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            Assert.IsNotNull(shader, "A built-in test shader is required.");
            var material = new Material(shader);
            var chunkObject = new GameObject("Chunk Elevated Matrix Test");
            _created.Add(material);
            _created.Add(chunkObject);

            var registry = new ChunkFirstRuntimeMeshRegistry();
            var builder = new ChunkTerrainMeshBuilder(
                registry,
                new ChunkFirstBuildDiagnostics());
            var resolved = new Dictionary<Vector2Int, ResolvedTileComposition>
            {
                [Vector2Int.zero] = new ResolvedTileComposition(
                    Vector2Int.zero,
                    CreateHeightSample("ground", 3f),
                    default,
                    true,
                    false,
                    string.Empty)
            };
            var source = new SingleMeshSource(
                sourceMesh,
                material,
                Matrix4x4.Translate(new Vector3(0f, 2f, 0f)));
            var area = new ChunkBuildArea(
                default,
                new RectInt(0, 0, 1, 1),
                new RectInt(0, 0, 1, 1));

            int built = builder.Build(
                chunkObject.transform,
                area,
                resolved,
                source);

            Transform terrain = chunkObject.transform.Find("TerrainMesh");
            Assert.AreEqual(1, built);
            Assert.IsNotNull(terrain);
            Mesh rendered = terrain.GetComponent<MeshFilter>().sharedMesh;
            Assert.IsNotNull(rendered);
            Assert.AreEqual(3f, rendered.bounds.min.y, 0.0001f);
            Assert.AreEqual(3f, rendered.bounds.max.y, 0.0001f);

            registry.Clear();
        }

        [Test]
        public void ExactVertexWeld_MergesOnlyFullyIdenticalVertexPayloads()
        {
            Mesh source = CreateDuplicateVertexMesh(
                duplicateUv: new Vector2(0f, 0f));

            bool created =
                ExactVertexWeldMeshUtility.TryCreate(source, out Mesh welded);
            _created.Add(welded);

            Assert.IsTrue(created);
            Assert.IsNotNull(welded);
            Assert.AreEqual(3, welded.vertexCount);
            Assert.AreEqual(2, welded.subMeshCount);
            CollectionAssert.AreEqual(
                new[] { 0, 1, 2 },
                welded.GetIndices(0));
            CollectionAssert.AreEqual(
                new[] { 0, 2, 1 },
                welded.GetIndices(1));
            CollectionAssert.AreEqual(
                new[] { 0, 1, 2, 0, 2, 1 },
                welded.triangles);
            Assert.AreEqual(3, welded.normals.Length);
            Assert.AreEqual(3, welded.tangents.Length);
            Assert.AreEqual(3, welded.colors32.Length);
            for (int channel = 0; channel < 8; channel++)
            {
                var uvs = new List<Vector4>();
                welded.GetUVs(channel, uvs);
                Assert.AreEqual(
                    3,
                    uvs.Count,
                    $"UV{channel} must survive exact welding.");
            }
        }

        [Test]
        public void ExactVertexWeld_PreservesUvSeamAtSamePosition()
        {
            Mesh source = CreateDuplicateVertexMesh(
                duplicateUv: new Vector2(1f, 1f));

            bool created =
                ExactVertexWeldMeshUtility.TryCreate(source, out Mesh welded);

            Assert.IsFalse(created);
            Assert.IsNull(welded);
            Assert.AreEqual(4, source.vertexCount);
        }

        [Test]
        public void ExactVertexWeld_RemovesUnreferencedVerticesWithoutDuplicates()
        {
            var source = new Mesh
            {
                name = "Unreferenced Vertex Test",
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    Vector3.one,
                },
                triangles = new[] { 0, 1, 2 },
            };
            source.RecalculateBounds();
            _created.Add(source);

            bool created =
                ExactVertexWeldMeshUtility.TryCreate(source, out Mesh compact);
            _created.Add(compact);

            Assert.IsTrue(created);
            Assert.IsNotNull(compact);
            Assert.AreEqual(3, compact.vertexCount);
            CollectionAssert.AreEqual(
                new[] { 0, 1, 2 },
                compact.GetIndices(0));
        }

        [Test]
        public void ExactVertexWeld_PreservesSubMeshMetadataAndMeshState()
        {
            var source = new Mesh
            {
                name = "SubMesh Metadata Test",
                indexFormat = IndexFormat.UInt32,
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    new Vector3(2f, 0f, 0f),
                    new Vector3(3f, 0f, 0f),
                    new Vector3(99f, 99f, 99f),
                },
                bindposes = new[]
                {
                    Matrix4x4.Translate(new Vector3(1f, 2f, 3f)),
                },
            };
            source.subMeshCount = 2;
            source.SetIndices(
                new[] { 0, 1, 2 },
                MeshTopology.Triangles,
                0,
                calculateBounds: false,
                baseVertex: 0);
            source.SetIndices(
                new[] { 0, 1 },
                MeshTopology.Lines,
                1,
                calculateBounds: false,
                baseVertex: 3);

            var meshBounds = new Bounds(
                new Vector3(5f, 6f, 7f),
                new Vector3(8f, 9f, 10f));
            var firstBounds = new Bounds(
                Vector3.one,
                Vector3.one * 2f);
            var secondBounds = new Bounds(
                Vector3.right * 3f,
                Vector3.one * 4f);
            source.bounds = meshBounds;
            SetSubMeshBounds(source, 0, firstBounds);
            SetSubMeshBounds(source, 1, secondBounds);
            _created.Add(source);

            bool created =
                ExactVertexWeldMeshUtility.TryCreate(source, out Mesh compact);
            _created.Add(compact);

            Assert.IsTrue(created);
            Assert.AreEqual(5, compact.vertexCount);
            Assert.AreEqual(IndexFormat.UInt32, compact.indexFormat);
            Assert.AreEqual(2, compact.subMeshCount);
            Assert.AreEqual(MeshTopology.Triangles, compact.GetTopology(0));
            Assert.AreEqual(MeshTopology.Lines, compact.GetTopology(1));
            Assert.AreEqual(0, compact.GetBaseVertex(0));
            Assert.AreEqual(3, compact.GetBaseVertex(1));
            CollectionAssert.AreEqual(
                new[] { 0, 1, 2 },
                compact.GetIndices(0, applyBaseVertex: false));
            CollectionAssert.AreEqual(
                new[] { 0, 1 },
                compact.GetIndices(1, applyBaseVertex: false));
            Assert.AreEqual(meshBounds, compact.bounds);
            Assert.AreEqual(firstBounds, compact.GetSubMesh(0).bounds);
            Assert.AreEqual(secondBounds, compact.GetSubMesh(1).bounds);
            CollectionAssert.AreEqual(source.bindposes, compact.bindposes);
        }

        [Test]
        public void ExactVertexWeld_CompactsPastEmptySubMeshPadding()
        {
            var source = new Mesh
            {
                name = "Empty SubMesh Padding Test",
                indexFormat = IndexFormat.UInt32,
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    Vector3.one,
                    Vector3.one * 2f,
                    Vector3.one * 3f,
                },
            };
            source.subMeshCount = 2;
            source.SetIndices(
                new[] { 0, 1, 2 },
                MeshTopology.Triangles,
                0,
                calculateBounds: false);
            source.SetIndices(
                System.Array.Empty<int>(),
                MeshTopology.Lines,
                1,
                calculateBounds: false,
                baseVertex: 5);
            _created.Add(source);

            bool created =
                ExactVertexWeldMeshUtility.TryCreate(source, out Mesh compact);
            _created.Add(compact);

            Assert.IsTrue(created);
            Assert.AreEqual(3, compact.vertexCount);
            Assert.AreEqual(MeshTopology.Lines, compact.GetTopology(1));
            Assert.AreEqual(0, compact.GetIndexCount(1));
            Assert.AreEqual(0, compact.GetBaseVertex(1));
        }

        [Test]
        public void ExactVertexWeld_ExaminesEveryRawVertexStream()
        {
            Mesh seamSource = CreateMultiStreamDuplicateMesh(
                matchingUv7: false);

            bool seamCreated =
                ExactVertexWeldMeshUtility.TryCreate(
                    seamSource,
                    out Mesh seamResult);

            Assert.IsFalse(seamCreated);
            Assert.IsNull(seamResult);

            Mesh matchingSource = CreateMultiStreamDuplicateMesh(
                matchingUv7: true);
            bool matchingCreated =
                ExactVertexWeldMeshUtility.TryCreate(
                    matchingSource,
                    out Mesh matchingResult);
            _created.Add(matchingResult);

            Assert.IsTrue(matchingCreated);
            Assert.IsNotNull(matchingResult);
            Assert.AreEqual(3, matchingResult.vertexCount);
            Assert.AreEqual(4, matchingResult.vertexBufferCount);
            var uv7 = new List<Vector2>();
            matchingResult.GetUVs(7, uv7);
            Assert.AreEqual(3, uv7.Count);
        }

        private TwcTileMeshSourceProvider CreateDualGridProvider(
            params string[] terrainIds)
        {
            var managerObject = new GameObject("Dual Grid Test Manager");
            _created.Add(managerObject);
            var manager =
                managerObject.AddComponent<TileWorldCreatorManager>();
            var configuration =
                ScriptableObject.CreateInstance<Configuration>();
            _created.Add(configuration);
            configuration.cellSize = 1f;
            manager.configuration = configuration;

            var folder = new BuildLayerFolder("Dual Grid Test");
            configuration.buildLayerFolders.Add(folder);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            Assert.IsNotNull(shader, "A built-in test shader is required.");

            for (int index = 0; index < terrainIds.Length; index++)
            {
                string terrainId = terrainIds[index];
                Mesh mesh = CreateFlatQuadMesh(0f);
                var material = new Material(shader)
                {
                    name = terrainId + "-material"
                };
                _created.Add(material);
                var prefab = new GameObject(terrainId + "-dual-tile");
                _created.Add(prefab);
                prefab.AddComponent<MeshFilter>().sharedMesh = mesh;
                prefab.AddComponent<MeshRenderer>().sharedMaterial = material;

                var preset = ScriptableObject.CreateInstance<TilePreset>();
                _created.Add(preset);
                preset.name = terrainId;
                preset.tileId = terrainId;
                preset.gridtype = TilePreset.GridType.dual;
                preset.DUALGRD_cornerTile = prefab;
                preset.DUALGRD_invertedCornerTile = prefab;
                preset.DUALGRD_edgeTile = prefab;
                preset.DUALGRD_fillTile = prefab;
                preset.DUALGRD_doubleInteriorCornerTile = prefab;

                var buildLayer =
                    ScriptableObject.CreateInstance<TilesBuildLayer>();
                _created.Add(buildLayer);
                buildLayer.guid = terrainId + "-build";
                buildLayer.assignedBlueprintLayerGuid =
                    terrainId + "-blueprint";
                buildLayer.scaleTileToCellSize = false;
                buildLayer.scaleOffset = Vector3.one;
                buildLayer.tilePresetsTop.Add(
                    new TilesBuildLayer.TilePresetSelection
                    {
                        preset = preset,
                        weight = 1f
                    });
                folder.buildLayers.Add(buildLayer);
            }

            var mapping =
                ScriptableObject.CreateInstance<TileWorldCreatorIdMappingSO>();
            _created.Add(mapping);
            return new TwcTileMeshSourceProvider(
                new TileWorldCreatorBuildEnvironment(
                    manager,
                    mapping,
                    new TileWorldCreatorBuildOptions()));
        }

        private static int CountSourcesAtX(
            IReadOnlyList<TileMeshSource> sources,
            float expectedX)
        {
            int count = 0;
            for (int index = 0; index < sources.Count; index++)
            {
                if (Mathf.Abs(
                        sources[index].LocalMatrix.m03 - expectedX)
                    <= 0.0001f)
                {
                    count++;
                }
            }

            return count;
        }

        private static void SetSubMeshBounds(
            Mesh mesh,
            int subMesh,
            Bounds bounds)
        {
            SubMeshDescriptor descriptor = mesh.GetSubMesh(subMesh);
            descriptor.bounds = bounds;
            mesh.SetSubMesh(
                subMesh,
                descriptor,
                MeshUpdateFlags.DontRecalculateBounds
                | MeshUpdateFlags.DontValidateIndices);
        }

        private Mesh CreateMultiStreamDuplicateMesh(bool matchingUv7)
        {
            var mesh = new Mesh { name = "Multi Stream Exact Weld Test" };
            mesh.SetVertexBufferParams(
                4,
                new VertexAttributeDescriptor(
                    VertexAttribute.Position,
                    VertexAttributeFormat.Float32,
                    3,
                    stream: 0),
                new VertexAttributeDescriptor(
                    VertexAttribute.Normal,
                    VertexAttributeFormat.Float32,
                    3,
                    stream: 1),
                new VertexAttributeDescriptor(
                    VertexAttribute.TexCoord0,
                    VertexAttributeFormat.Float32,
                    2,
                    stream: 2),
                new VertexAttributeDescriptor(
                    VertexAttribute.TexCoord7,
                    VertexAttributeFormat.Float32,
                    2,
                    stream: 3));
            mesh.SetVertexBufferData(
                new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    Vector3.zero,
                },
                0,
                0,
                4,
                stream: 0);
            mesh.SetVertexBufferData(
                new[]
                {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                },
                0,
                0,
                4,
                stream: 1);
            mesh.SetVertexBufferData(
                new[]
                {
                    Vector2.zero,
                    Vector2.right,
                    Vector2.up,
                    Vector2.zero,
                },
                0,
                0,
                4,
                stream: 2);
            mesh.SetVertexBufferData(
                new[]
                {
                    Vector2.zero,
                    Vector2.right,
                    Vector2.up,
                    matchingUv7 ? Vector2.zero : Vector2.one,
                },
                0,
                0,
                4,
                stream: 3);
            mesh.subMeshCount = 2;
            mesh.SetIndices(
                new[] { 0, 1, 2 },
                MeshTopology.Triangles,
                0,
                calculateBounds: false);
            mesh.SetIndices(
                new[] { 3, 2, 1 },
                MeshTopology.Triangles,
                1,
                calculateBounds: false);
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
        }

        private Mesh CreateDuplicateVertexMesh(Vector2 duplicateUv)
        {
            var mesh = new Mesh
            {
                name = "Exact Weld Test",
                vertices = new[]
                {
                    Vector3.zero,
                    Vector3.right,
                    Vector3.up,
                    Vector3.zero,
                },
                normals = new[]
                {
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                    Vector3.forward,
                },
                tangents = new[]
                {
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                },
                colors32 = new[]
                {
                    new Color32(255, 0, 0, 255),
                    new Color32(0, 255, 0, 255),
                    new Color32(0, 0, 255, 255),
                    new Color32(255, 0, 0, 255),
                },
                uv = new[]
                {
                    Vector2.zero,
                    Vector2.right,
                    Vector2.up,
                    duplicateUv,
                }
            };
            for (int channel = 1; channel < 8; channel++)
            {
                float value = channel * 0.1f;
                mesh.SetUVs(
                    channel,
                    new List<Vector4>
                    {
                        new Vector4(value, 0f, 0f, 1f),
                        new Vector4(value, 1f, 0f, 1f),
                        new Vector4(value, 0f, 1f, 1f),
                        new Vector4(value, 0f, 0f, 1f),
                    });
            }

            mesh.subMeshCount = 2;
            mesh.SetTriangles(new[] { 0, 1, 2 }, 0, false);
            mesh.SetTriangles(new[] { 3, 2, 1 }, 1, false);
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
        }

        private Mesh CreateClosedTileMeshWithChannels()
        {
            var vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 1f, -0.5f),
                new Vector3(0.5f, 1f, -0.5f),
                new Vector3(0.5f, 1f, 0.5f),
                new Vector3(-0.5f, 1f, 0.5f)
            };
            var mesh = new Mesh
            {
                name = "Closed SurfaceOnly Test",
                vertices = vertices,
                normals = new[]
                {
                    Vector3.down,
                    Vector3.down,
                    Vector3.down,
                    Vector3.down,
                    Vector3.up,
                    Vector3.up,
                    Vector3.up,
                    Vector3.up
                },
                tangents = new[]
                {
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f),
                    new Vector4(1f, 0f, 0f, 1f)
                },
                colors32 = new[]
                {
                    new Color32(10, 20, 30, 255),
                    new Color32(20, 30, 40, 255),
                    new Color32(30, 40, 50, 255),
                    new Color32(40, 50, 60, 255),
                    new Color32(50, 60, 70, 255),
                    new Color32(60, 70, 80, 255),
                    new Color32(70, 80, 90, 255),
                    new Color32(80, 90, 100, 255)
                }
            };

            for (int channel = 0; channel < 8; channel++)
            {
                float value = channel * 0.1f;
                mesh.SetUVs(
                    channel,
                    new List<Vector4>
                    {
                        new Vector4(0f, 0f, value, 1f),
                        new Vector4(1f, 0f, value, 1f),
                        new Vector4(1f, 1f, value, 1f),
                        new Vector4(0f, 1f, value, 1f),
                        new Vector4(0f, 0f, value, 1f),
                        new Vector4(1f, 0f, value, 1f),
                        new Vector4(1f, 1f, value, 1f),
                        new Vector4(0f, 1f, value, 1f)
                    });
            }

            mesh.subMeshCount = 2;
            mesh.SetTriangles(
                new[] { 4, 6, 5, 4, 7, 6 },
                0,
                false);
            mesh.SetTriangles(
                new[]
                {
                    0, 1, 2, 0, 2, 3,
                    0, 4, 5, 0, 5, 1,
                    1, 5, 6, 1, 6, 2,
                    2, 6, 7, 2, 7, 3,
                    3, 7, 4, 3, 4, 0
                },
                1,
                false);
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
        }

        private Mesh CreateFlatQuadMesh(float height = 1f)
        {
            var mesh = new Mesh
            {
                name = "Flat Tile Test",
                vertices = new[]
                {
                    new Vector3(-0.5f, height, -0.5f),
                    new Vector3(0.5f, height, -0.5f),
                    new Vector3(0.5f, height, 0.5f),
                    new Vector3(-0.5f, height, 0.5f)
                },
                triangles = new[] { 0, 2, 1, 0, 3, 2 }
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
        }

        private static GraphTileLayerSample CreateHeightSample(string id, float surfaceHeight)
        {
            return new GraphTileLayerSample(
                id,
                id,
                id + "-blueprint",
                id + "-build",
                id,
                id,
                LayerKind.BaseTerrain,
                sortingOrder: 0,
                graphLayerOrder: 0,
                terrainPriority: 0,
                height: surfaceHeight,
                surfaceHeight: surfaceHeight,
                sourceNodeId: id + "-node");
        }

        private Mesh CreateTwoLevelTriangleMesh(
            float topY = 1f,
            float bottomY = -1f)
        {
            var mesh = new Mesh
            {
                name = "Two Level Tile Test",
                vertices = new[]
                {
                    new Vector3(-0.5f, topY, -0.5f),
                    new Vector3(0.5f, topY, -0.5f),
                    new Vector3(0f, topY, 0.5f),
                    new Vector3(-0.5f, bottomY, -0.5f),
                    new Vector3(0.5f, bottomY, -0.5f),
                    new Vector3(0f, bottomY, 0.5f)
                },
                triangles = new[] { 0, 2, 1, 3, 4, 5 }
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
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

        private sealed class SingleMeshSource : IResolvedTileMeshSource
        {
            private readonly Mesh _mesh;
            private readonly Material _material;
            private readonly Matrix4x4 _localMatrix;
            private readonly TileGeometryMode _tileGeometryMode;

            public SingleMeshSource(
                Mesh mesh,
                Material material,
                Matrix4x4? localMatrix = null,
                TileGeometryMode tileGeometryMode =
                    TileGeometryMode.SolidTerrain)
            {
                _mesh = mesh;
                _material = material;
                _localMatrix = localMatrix ?? Matrix4x4.identity;
                _tileGeometryMode = tileGeometryMode;
            }

            public int CollectMeshSources(
                ResolvedTileComposition composition,
                List<TileMeshSource> results)
            {
                results.Add(new TileMeshSource(
                    _mesh,
                    new[] { _material },
                    _localMatrix,
                    tileGeometryMode: _tileGeometryMode));
                return 1;
            }
        }
    }
}
