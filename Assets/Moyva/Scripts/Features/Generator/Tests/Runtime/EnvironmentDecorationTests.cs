using System;
using System.Collections.Generic;
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
                forestWorld.BiomeMap[x, y] = "forest-sparse";
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

            public bool TryGetDefinition(string id, out MapObjectDefinition definition)
            {
                return _definitions.TryGetValue(id, out definition);
            }
        }
    }
}
