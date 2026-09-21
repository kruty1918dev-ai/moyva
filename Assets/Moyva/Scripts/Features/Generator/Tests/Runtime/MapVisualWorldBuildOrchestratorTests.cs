using System.Reflection;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Tests for terrain data publication in <see cref="MapVisualWorldBuildOrchestrator"/>:
    /// an explicit surface map published earlier by the TileWorldCreator bridge must not be
    /// overwritten by the raw normalized HeightMap carried in GeneratedWorldData.
    /// </summary>
    [TestFixture]
    public sealed class MapVisualWorldBuildOrchestratorTests
    {
        private static readonly MethodInfo PublishTerrainDataMethod =
            typeof(MapVisualWorldBuildOrchestrator).GetMethod(
                "PublishTerrainData",
                BindingFlags.NonPublic | BindingFlags.Instance);

        private MapVisualWorldBuildOrchestrator _orchestrator;
        private GeneratorTerrainLevelService _terrainLevels;

        [SetUp]
        public void Setup()
        {
            Assert.NotNull(PublishTerrainDataMethod, "PublishTerrainData must exist for this test.");
            _terrainLevels = new GeneratorTerrainLevelService();
            _orchestrator = new MapVisualWorldBuildOrchestrator(
                state: null,
                dataFactory: null,
                integrity: null,
                gridWriter: null,
                signals: null,
                terrainLevelService: _terrainLevels);
        }

        [Test]
        public void PublishTerrainData_KeepsExplicitSurfaceHeightMap()
        {
            // Arrange: emulate the TileWorldCreator bridge having already published
            // level map + explicit surface heights (biome base + level offset).
            var levelMap = new int[4, 4];
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                levelMap[x, y] = 1;
            _terrainLevels.SetLevelMap(levelMap);
            var explicitSurface = new float[4, 4];
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                explicitSurface[x, y] = 7.5f;
            _terrainLevels.SetSurfaceHeightMap(explicitSurface);

            var worldData = new GeneratedWorldData
            {
                Width = 4,
                Height = 4,
                TerrainLevelMap = new int[4, 4],
                HeightMap = new float[4, 4]
            };

            // Act
            PublishTerrainDataMethod.Invoke(_orchestrator, new object[] { worldData });

            // Assert: explicit surface heights survive; raw zero HeightMap ignored.
            Assert.IsTrue(_terrainLevels.TryGetSurfaceHeight(new Vector2Int(1, 1), out float surfaceY));
            Assert.AreEqual(7.5f, surfaceY, 0.0001f);
            Assert.IsTrue(_terrainLevels.HasExplicitSurfaceHeightMap);
        }

        [Test]
        public void PublishTerrainData_PublishesMaps_WhenServiceEmpty()
        {
            // Arrange: no prior publication (fallback build path).
            var worldData = new GeneratedWorldData
            {
                Width = 4,
                Height = 4,
                TerrainLevelMap = new int[4, 4],
                HeightMap = new float[4, 4]
            };
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                worldData.TerrainLevelMap[x, y] = 2;

            // Act
            PublishTerrainDataMethod.Invoke(_orchestrator, new object[] { worldData });

            // Assert: level map published and the surface map is derived from
            // levels — not the raw normalized noise HeightMap (which is 0 here).
            Assert.IsTrue(_terrainLevels.HasLevelMap);
            Assert.IsTrue(_terrainLevels.TryGetSurfaceHeight(new Vector2Int(2, 2), out float surfaceY));
            Assert.AreEqual(2f, surfaceY, 0.0001f);
        }
    }
}
