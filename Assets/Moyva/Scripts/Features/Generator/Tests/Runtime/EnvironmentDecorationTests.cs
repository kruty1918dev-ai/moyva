using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Tests for environment decoration generation system.
    /// Verifies determinism, configuration, and integration requirements.
    /// </summary>
    [TestFixture]
    public sealed class EnvironmentDecorationTests
    {
        private EnvironmentDecorationConfig _config;
        private MockMapObjectRegistryService _mockRegistry;
        private EnvironmentDecorationGenerator _generator;

        [SetUp]
        public void Setup()
        {
            _config = new EnvironmentDecorationConfig
            {
                Enabled = true,
                GlobalDensity = 1f,
                SeedOffset = 0,
                MaxObjectsPerTile = 3,
                ClusterStrength = 0.7f,
                ClusterRadius = 3,
                TypeDensities = new EnvironmentTypeDensities
                {
                    TreeDensity = 0.15f,
                    BushDensity = 0.1f,
                    GrassDensity = 0.2f,
                    RockDensity = 0.08f
                },
                BiomeRules = new BiomeMultipliers
                {
                    Grassland = 1.2f,
                    Forest = 2.0f,
                    Rocky = 1.5f,
                    Coast = 0.5f,
                    Water = 0f
                },
                Exclusions = new ExclusionZones
                {
                    BuildingExclusionRadius = 1,
                    SettlementExclusionRadius = 2,
                    SuppressWaterDecorations = true
                },
                VisualVariation = new VisualVariation
                {
                    EnableRotation = true,
                    EnableScaleVariation = true,
                    MinScale = 0.85f,
                    MaxScale = 1.15f,
                    MaxPositionOffset = 0.2f
                },
                AssetPools = new Dictionary<string, string[]>
                {
                    { "tree", new[] { "green-tree-object-001", "green-tree-object-002" } },
                    { "rock", new[] { "stone-object-001" } }
                }
            };

            _mockRegistry = new MockMapObjectRegistryService();
            _generator = new EnvironmentDecorationGenerator(_config, _mockRegistry);
        }

        [Test]
        public void SameSeed_SameConfig_ProducesIdenticalPlacements()
        {
            // Arrange
            var worldData = CreateTestWorldData(10, 10, seed: 12345);
            
            // Act
            var result1 = _generator.Generate(worldData);
            var result2 = _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(result1.Count, result2.Count, "Placement count should be identical");
            
            for (int i = 0; i < result1.Count; i++)
            {
                Assert.AreEqual(result1.Placements[i].AssetId, result2.Placements[i].AssetId, 
                    $"Asset ID mismatch at index {i}");
                Assert.AreEqual(result1.Placements[i].TileX, result2.Placements[i].TileX, 
                    $"Tile X mismatch at index {i}");
                Assert.AreEqual(result1.Placements[i].TileY, result2.Placements[i].TileY, 
                    $"Tile Y mismatch at index {i}");
                Assert.AreEqual(result1.Placements[i].Position, result2.Placements[i].Position, 
                    $"Position mismatch at index {i}");
                Assert.AreEqual(result1.Placements[i].Rotation, result2.Placements[i].Rotation, 
                    $"Rotation mismatch at index {i}");
                Assert.AreEqual(result1.Placements[i].Scale, result2.Placements[i].Scale, 
                    $"Scale mismatch at index {i}");
            }
        }

        [Test]
        public void DifferentSeed_ChangesPlacements()
        {
            // Arrange
            var worldData1 = CreateTestWorldData(10, 10, seed: 12345);
            var worldData2 = CreateTestWorldData(10, 10, seed: 54321);

            // Act
            var result1 = _generator.Generate(worldData1);
            var result2 = _generator.Generate(worldData2);

            // Assert
            Assert.AreNotEqual(result1.Count, result2.Count, 
                "Different seeds should produce different placement counts");
        }

        [Test]
        public void EnvironmentGeneration_DoesNotModifyAuthoritativeMapData()
        {
            // Arrange
            var originalWorldData = CreateTestWorldData(10, 10, seed: 12345);
            var worldData = originalWorldData.Clone();

            // Act
            _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(originalWorldData.Width, worldData.Width);
            Assert.AreEqual(originalWorldData.Height, worldData.Height);
            Assert.AreEqual(originalWorldData.Seed, worldData.Seed);
            
            // Verify tile maps are unchanged
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                Assert.AreEqual(originalWorldData.BiomeMap[x, y], worldData.BiomeMap[x, y]);
                Assert.AreEqual(originalWorldData.GameplayTileMap[x, y], worldData.GameplayTileMap[x, y]);
                Assert.AreEqual(originalWorldData.ObjectMap[x, y], worldData.ObjectMap[x, y]);
                Assert.AreEqual(originalWorldData.BuildingMap[x, y], worldData.BuildingMap[x, y]);
            }
        }

        [Test]
        public void WaterDecorations_SuppressedWhenConfigured()
        {
            // Arrange
            var worldData = CreateTestWorldData(10, 10, seed: 12345);
            // Set all tiles to water
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = "water";
                worldData.GameplayTileMap[x, y] = "water";
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(0, result.Count, "No decorations should spawn on water when suppressed");
        }

        [Test]
        public void WaterFlora_SpawnsOnSuppressedWater_WhenPoolConfigured()
        {
            // Arrange: all water with a guaranteed waterplant roll.
            _mockRegistry.AddDefinition("test-lily-001");
            _config.AssetPools["waterplant"] = new[] { "test-lily-001" };
            _config.TypeDensities.WaterPlantDensity = 1f;
            var worldData = CreateTestWorldData(6, 6, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = "water";
                worldData.GameplayTileMap[x, y] = "water";
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.Greater(result.Count, 0, "Water flora should spawn on water cells");
            foreach (var placement in result.Placements)
            {
                Assert.AreEqual("test-lily-001", placement.AssetId);
                Assert.Greater(placement.YOffset, 0f,
                    "Water flora must float above the water sheet surface");
            }
        }

        [Test]
        public void WaterFlora_ZeroDensity_KeepsWaterEmpty()
        {
            // Arrange: pool configured but density zeroed out.
            _mockRegistry.AddDefinition("test-lily-001");
            _config.AssetPools["waterplant"] = new[] { "test-lily-001" };
            _config.TypeDensities.WaterPlantDensity = 0f;
            var worldData = CreateTestWorldData(6, 6, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = "water";
                worldData.GameplayTileMap[x, y] = "water";
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ShorelineExclusion_SuppressesHeavyPropsNearWater()
        {
            // Arrange: only trees can spawn; a single water cell at (5,5)
            // suppresses them within one cell of the shore.
            _config.Exclusions.ShorelineExclusionCells = 1;
            _config.MaxObjectsPerTile = 1;
            _config.TypeDensities = new EnvironmentTypeDensities
            {
                TreeDensity = 1f,
                BushDensity = 0f,
                GrassDensity = 0f,
                FlowerDensity = 0f,
                RockDensity = 0f
            };
            var worldData = CreateTestWorldData(11, 11, seed: 12345);
            worldData.BiomeMap[5, 5] = "water";
            worldData.GameplayTileMap[5, 5] = "water";

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.Greater(result.Count, 0, "Land away from the shore should still spawn trees");
            foreach (var placement in result.Placements)
            {
                int dx = Mathf.Abs(placement.TileX - 5);
                int dy = Mathf.Abs(placement.TileY - 5);
                Assert.Greater(Mathf.Max(dx, dy), 1,
                    $"Heavy prop at {placement.TileX},{placement.TileY} sits inside the shoreline buffer");
            }
        }

        [Test]
        public void ShorelineExclusion_AllowsGrassNearWater()
        {
            // Arrange: only grass can spawn; it is allowed right up to water.
            _config.Exclusions.ShorelineExclusionCells = 1;
            _config.MaxObjectsPerTile = 1;
            _mockRegistry.AddDefinition("test-grass-001");
            _config.AssetPools["grass"] = new[] { "test-grass-001" };
            _config.TypeDensities = new EnvironmentTypeDensities
            {
                TreeDensity = 0f,
                BushDensity = 0f,
                GrassDensity = 1f,
                FlowerDensity = 0f,
                RockDensity = 0f
            };
            var worldData = CreateTestWorldData(11, 11, seed: 12345);
            worldData.BiomeMap[5, 5] = "water";
            worldData.GameplayTileMap[5, 5] = "water";

            // Act
            var result = _generator.Generate(worldData);

            // Assert: grass may occupy cells adjacent to water.
            bool nearShore = result.Placements.Any(p =>
                Mathf.Max(Mathf.Abs(p.TileX - 5), Mathf.Abs(p.TileY - 5)) == 1);
            Assert.IsTrue(nearShore, "Grass should be allowed inside the shoreline buffer");
        }

        [Test]
        public void BuildingExclusion_PreventsDecorationsNearBuildings()
        {
            // Arrange
            var worldData = CreateTestWorldData(10, 10, seed: 12345);
            // Place a building at center
            worldData.BuildingMap[5, 5] = "castle";

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            foreach (var placement in result.Placements)
            {
                float distance = Vector2Int.Distance(new Vector2Int(placement.TileX, placement.TileY), new Vector2Int(5, 5));
                Assert.Greater(distance, _config.Exclusions.BuildingExclusionRadius, 
                    "Decorations should not spawn within building exclusion radius");
            }
        }

        [Test]
        public void DisabledConfig_ProducesNoDecorations()
        {
            // Arrange
            _config.Enabled = false;
            var worldData = CreateTestWorldData(10, 10, seed: 12345);

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(0, result.Count, "Disabled config should produce no decorations");
        }

        [Test]
        public void ZeroGlobalDensity_ProducesNoDecorations()
        {
            // Arrange
            _config.GlobalDensity = 0f;
            var worldData = CreateTestWorldData(10, 10, seed: 12345);

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(0, result.Count, "Zero global density should produce no decorations");
        }

        [Test]
        public void MissingAsset_DoesNotCrashGeneration()
        {
            // Arrange
            _config.AssetPools["tree"] = new[] { "non-existent-asset" };
            var worldData = CreateTestWorldData(10, 10, seed: 12345);

            // Act & Assert
            Assert.DoesNotThrow(() => _generator.Generate(worldData), 
                "Missing assets should not crash generation");
        }

        [Test]
        public void DeterministicHash_ProducesStableValues()
        {
            // Arrange
            int seed = 12345;
            int x = 10;
            int y = 20;
            int index = 0;

            // Act
            uint hash1 = DeterministicHash.CellHash(seed, x, y, index);
            uint hash2 = DeterministicHash.CellHash(seed, x, y, index);
            uint hash3 = DeterministicHash.CellHash(seed, x + 1, y, index);

            // Assert
            Assert.AreEqual(hash1, hash2, "Same inputs should produce same hash");
            Assert.AreNotEqual(hash1, hash3, "Different inputs should produce different hash");
        }

        [Test]
        public void HighDensity_ReachesMultipleObjectsPerTile()
        {
            // Arrange: forest biome doubles density, so density can exceed 1
            // and MaxObjectsPerTile=3 must be reachable.
            var worldData = CreateTestWorldData(10, 10, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = "forest-dense";
                worldData.GameplayTileMap[x, y] = "forest-dense";
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            var perTile = result.Placements
                .GroupBy(p => (p.TileX, p.TileY))
                .Select(g => g.Count())
                .ToArray();
            Assert.Greater(perTile.Length, 0, "Forest world should produce decorations");
            Assert.Greater(perTile.Max(), 1,
                "Density > 1 must be able to place more than one object per tile");
            Assert.LessOrEqual(perTile.Max(), _config.MaxObjectsPerTile,
                "Placement count must respect MaxObjectsPerTile");
        }

        [Test]
        public void ZeroMaxObjectsPerTile_ProducesNoDecorations()
        {
            // Arrange
            _config.MaxObjectsPerTile = 0;
            var worldData = CreateTestWorldData(10, 10, seed: 12345);

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void BushAndGrassTypes_SpawnWhenPoolsConfigured()
        {
            // Arrange: pools for every type emitted by SelectDecorationType.
            _mockRegistry.AddDefinition("test-bush-001");
            _mockRegistry.AddDefinition("test-grass-001");
            _config.AssetPools["bush"] = new[] { "test-bush-001" };
            _config.AssetPools["grass"] = new[] { "test-grass-001" };
            _config.TypeDensities = new EnvironmentTypeDensities
            {
                TreeDensity = 0.25f,
                BushDensity = 0.25f,
                GrassDensity = 0.25f,
                RockDensity = 0.25f
            };
            var worldData = CreateTestWorldData(20, 20, seed: 12345);

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            var ids = result.Placements.Select(p => p.AssetId).Distinct().ToArray();
            Assert.Contains("test-bush-001", ids,
                "Bush band must produce placements when a bush pool exists");
            Assert.Contains("test-grass-001", ids,
                "Grass band must produce placements when a grass pool exists");
        }

        [TestCase("stone")]
        [TestCase("rock-cliff")]
        public void RockyTiles_NeverSpawnTreesOrBushes(string rockyTileId)
        {
            // Arrange: register pools for every type so suppression is observable.
            _mockRegistry.AddDefinition("test-bush-001");
            _mockRegistry.AddDefinition("test-grass-001");
            _config.AssetPools["bush"] = new[] { "test-bush-001" };
            _config.AssetPools["grass"] = new[] { "test-grass-001" };

            var worldData = CreateTestWorldData(20, 20, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = rockyTileId;
                worldData.GameplayTileMap[x, y] = rockyTileId;
                worldData.HeightMap[x, y] = 2.0f;
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert
            Assert.Greater(result.Count, 0, "Rocky tiles should still produce decorations");
            var treePool = new HashSet<string>(_config.AssetPools["tree"]);
            var bushPool = new HashSet<string>(_config.AssetPools["bush"]);
            foreach (var placement in result.Placements)
            {
                Assert.IsFalse(treePool.Contains(placement.AssetId),
                    $"Tree asset {placement.AssetId} spawned on rocky tile {rockyTileId}");
                Assert.IsFalse(bushPool.Contains(placement.AssetId),
                    $"Bush (tree model) asset {placement.AssetId} spawned on rocky tile {rockyTileId}");
            }
        }

        [Test]
        public void RockyTiles_SpawnRocks()
        {
            // Arrange
            var worldData = CreateTestWorldData(20, 20, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = "stone";
                worldData.GameplayTileMap[x, y] = "stone";
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert: every placement on rocky terrain must come from the rock pool.
            var rockPool = new HashSet<string>(_config.AssetPools["rock"]);
            Assert.Greater(result.Count, 0, "Rocky tiles should produce rock decorations");
            foreach (var placement in result.Placements)
                Assert.IsTrue(rockPool.Contains(placement.AssetId),
                    $"Non-rock asset {placement.AssetId} spawned on stone tile");
        }

        [Test]
        public void GameplayTileMap_TakesPrecedenceOverStaleBiomeMarkers()
        {
            // Arrange: BiomeMap retains a pre-resolution marker while the
            // gameplay map already carries the resolved terrain id.
            var worldData = CreateTestWorldData(15, 15, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = "sand-shore-band";
                worldData.GameplayTileMap[x, y] = "stone";
                worldData.HeightMap[x, y] = 2.0f;
            }

            // Act
            var result = _generator.Generate(worldData);

            // Assert: rocky rules apply — no trees, only rocks.
            Assert.Greater(result.Count, 0, "Stone tiles should produce rock decorations");
            var treePool = new HashSet<string>(_config.AssetPools["tree"]);
            foreach (var placement in result.Placements)
                Assert.IsFalse(treePool.Contains(placement.AssetId),
                    $"Tree asset {placement.AssetId} spawned on resolved stone tile");
        }

        [TestCase("road")]
        [TestCase("footpath")]
        public void RoadTiles_ProduceNoDecorations(string roadTileId)
        {
            var worldData = CreateTestWorldData(15, 15, seed: 12345);
            for (int x = 0; x < worldData.Width; x++)
            for (int y = 0; y < worldData.Height; y++)
            {
                worldData.BiomeMap[x, y] = roadTileId;
                worldData.GameplayTileMap[x, y] = roadTileId;
            }

            var result = _generator.Generate(worldData);
            Assert.AreEqual(0, result.Count, $"No decorations should spawn on {roadTileId}");
        }

        [Test]
        public void BiomeMultipliers_AffectDecorationDensity()
        {
            // Arrange
            var grassWorld = CreateTestWorldData(10, 10, seed: 12345);
            var forestWorld = CreateTestWorldData(10, 10, seed: 12345);
            
            // Set different biomes
            for (int x = 0; x < grassWorld.Width; x++)
            for (int y = 0; y < grassWorld.Height; y++)
            {
                grassWorld.BiomeMap[x, y] = "grass";
                grassWorld.GameplayTileMap[x, y] = "grass";
                forestWorld.BiomeMap[x, y] = "forest-sparse";
                forestWorld.GameplayTileMap[x, y] = "forest-sparse";
            }

            // Act
            var grassResult = _generator.Generate(grassWorld);
            var forestResult = _generator.Generate(forestWorld);

            // Assert
            Assert.AreNotEqual(grassResult.Count, forestResult.Count, 
                "Different biome multipliers should affect decoration density");
        }

        private GeneratedWorldData CreateTestWorldData(int width, int height, int seed)
        {
            var worldData = new GeneratedWorldData
            {
                Width = width,
                Height = height,
                Seed = seed,
                BiomeMap = new string[width, height],
                GameplayTileMap = new string[width, height],
                ObjectMap = new string[width, height],
                BuildingMap = new string[width, height],
                HeightMap = new float[width, height]
            };

            // Fill with default grass terrain
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                worldData.BiomeMap[x, y] = "grass";
                worldData.GameplayTileMap[x, y] = "grass";
                worldData.HeightMap[x, y] = 0.5f;
            }

            return worldData;
        }

        public class MockMapObjectRegistryService : IMapObjectRegistryService
        {
            private readonly Dictionary<string, MapObjectDefinition> _definitions = new();

            public MockMapObjectRegistryService()
            {
                // Add mock definitions for test assets
                _definitions["green-tree-object-001"] = new MapObjectDefinition();
                _definitions["green-tree-object-002"] = new MapObjectDefinition();
                _definitions["stone-object-001"] = new MapObjectDefinition();
            }

            public void AddDefinition(string id)
            {
                _definitions[id] = new MapObjectDefinition();
            }

            public void SetDefinition(string id, GameObject visualPrefab)
            {
                _definitions[id] = new MapObjectDefinition(id, visualPrefab);
            }

            public bool TryGetDefinition(string id, out MapObjectDefinition definition)
            {
                return _definitions.TryGetValue(id, out definition);
            }
        }
    }
}
