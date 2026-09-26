using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Tests for the additive decoration layers in
    /// <see cref="EnvironmentDecorationGenerator"/> — determinism, terrain
    /// affinity rules and interaction with the legacy weighted pass.
    /// </summary>
    [TestFixture]
    public sealed class EnvironmentDecorationLayerTests
    {
        private EnvironmentDecorationConfig _config;
        private EnvironmentDecorationTests.MockMapObjectRegistryService _registry;

        [SetUp]
        public void Setup()
        {
            _registry = new EnvironmentDecorationTests.MockMapObjectRegistryService();
            _config = new EnvironmentDecorationConfig
            {
                Enabled = true,
                GlobalDensity = 1f,
                MaxObjectsPerTile = 3,
                ClusterStrength = 0.7f,
                ClusterRadius = 3,
                TypeDensities = new EnvironmentTypeDensities
                {
                    TreeDensity = 0f,
                    BushDensity = 0f,
                    GrassDensity = 0f,
                    FlowerDensity = 0f,
                    RockDensity = 0f,
                    WaterPlantDensity = 0f
                },
                Exclusions = new ExclusionZones
                {
                    BuildingExclusionRadius = 0,
                    SettlementExclusionRadius = 0,
                    SuppressWaterDecorations = true,
                    ShorelineExclusionCells = 1
                },
                VisualVariation = new VisualVariation
                {
                    EnableRotation = true,
                    EnableScaleVariation = true,
                    MinScale = 0.85f,
                    MaxScale = 1.15f,
                    MaxPositionOffset = 0.2f
                },
                AssetPools = new Dictionary<string, string[]>()
            };
        }

        private void AddLayerPool(string type, params string[] ids)
        {
            _config.AssetPools[type] = ids;
            foreach (string id in ids)
                _registry.AddDefinition(id);
        }

        private DecorationLayerRule Layer(
            string type,
            float weight = 0.9f,
            int maxPerTile = 1,
            DecorationWaterAffinity water = DecorationWaterAffinity.Any,
            DecorationForestAffinity forest = DecorationForestAffinity.Any,
            bool skipObjects = true)
        {
            return new DecorationLayerRule
            {
                Type = type,
                Weight = weight,
                MaxPerTile = maxPerTile,
                WaterAffinity = water,
                WaterRadius = 1,
                WaterBoost = 2f,
                ForestAffinity = forest,
                ForestEdgeRadius = 1,
                SkipObjectCells = skipObjects
            };
        }

        private GeneratedWorldData CreateWorld(int w, int h, int seed, string tileId = "grass")
        {
            var world = new GeneratedWorldData
            {
                Width = w,
                Height = h,
                Seed = seed,
                BiomeMap = new string[w, h],
                GameplayTileMap = new string[w, h],
                ObjectMap = new string[w, h],
                BuildingMap = new string[w, h],
                HeightMap = new float[w, h]
            };
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                world.BiomeMap[x, y] = tileId;
                world.GameplayTileMap[x, y] = tileId;
                world.HeightMap[x, y] = 0.5f;
            }
            return world;
        }

        [Test]
        public void NullLayers_MatchesLegacyPassExactly()
        {
            _config.Layers = null;
            var world = CreateWorld(8, 8, 42);
            var a = NewGenerator().Generate(world);
            _config.Layers = new DecorationLayerRule[0];
            var b = NewGenerator().Generate(world);

            Assert.AreEqual(a.Count, b.Count);
        }

        [Test]
        public void LayerSpawns_PlacementsOfLayerType()
        {
            AddLayerPool("grass", "veg-grass-a");
            _config.Layers = new[] { Layer("grass", weight: 2f, maxPerTile: 3) };
            var world = CreateWorld(6, 6, 42);

            var result = NewGenerator().Generate(world);

            Assert.Greater(result.Count, 0);
            Assert.IsTrue(result.Placements.All(p => p.AssetId == "veg-grass-a"));
            Assert.IsTrue(result.Placements.All(p => p.Type == "grass"));
        }

        [Test]
        public void SameSeed_LayersIdentical()
        {
            AddLayerPool("grass", "veg-grass-a", "veg-grass-b");
            _config.Layers = new[] { Layer("grass", weight: 1.5f, maxPerTile: 3) };
            var world = CreateWorld(8, 8, 777);

            var a = NewGenerator().Generate(world);
            var b = NewGenerator().Generate(world);

            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
                Assert.AreEqual(a.Placements[i].AssetId, b.Placements[i].AssetId);
        }

        [Test]
        public void DifferentSeed_LayersDiffer()
        {
            AddLayerPool("grass", "veg-grass-a");
            _config.Layers = new[] { Layer("grass", weight: 0.6f, maxPerTile: 2) };

            var a = NewGenerator().Generate(CreateWorld(8, 8, 42));
            var b = NewGenerator().Generate(CreateWorld(8, 8, 99));

            Assert.IsFalse(PositionsEqual(a, b),
                "Different seeds should change layer placement patterns");
        }

        private static bool PositionsEqual(
            DecorationPlacementResult a, DecorationPlacementResult b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (a.Placements[i].TileX != b.Placements[i].TileX
                    || a.Placements[i].TileY != b.Placements[i].TileY)
                    return false;
            return true;
        }

        [Test]
        public void RequireWater_OnlyAdjacentToWater()
        {
            AddLayerPool("reed", "veg-sedge-a");
            var world = CreateWorld(10, 10, 42);
            // water column at x==4
            for (int y = 0; y < 10; y++)
            {
                world.BiomeMap[4, y] = "water";
                world.GameplayTileMap[4, y] = "water";
            }
            _config.Layers = new[]
            {
                Layer("reed", weight: 2f, maxPerTile: 2,
                    water: DecorationWaterAffinity.Require)
            };

            var result = NewGenerator().Generate(world);

            Assert.Greater(result.Count, 0, "reed layer should spawn near water");
            foreach (var p in result.Placements)
            {
                Assert.AreEqual("reed", p.Type);
                Assert.IsTrue(p.TileX == 3 || p.TileX == 5,
                    $"reed at {p.TileX},{p.TileY} must be a water-adjacent land cell");
            }
        }

        [Test]
        public void AvoidWater_NeverAdjacent()
        {
            AddLayerPool("flower", "veg-flower-a");
            var world = CreateWorld(10, 10, 42);
            for (int y = 0; y < 10; y++)
            {
                world.BiomeMap[4, y] = "water";
                world.GameplayTileMap[4, y] = "water";
            }
            _config.Layers = new[]
            {
                Layer("flower", weight: 2f, maxPerTile: 1,
                    water: DecorationWaterAffinity.Avoid)
            };

            var result = NewGenerator().Generate(world);

            foreach (var p in result.Placements)
                Assert.IsFalse(p.TileX == 3 || p.TileX == 5,
                    $"flower at {p.TileX},{p.TileY} must avoid water-adjacent cells");
        }

        [Test]
        public void ForestAffinity_Interior_OnlyDeepForest()
        {
            AddLayerPool("fern", "veg-fern-a");
            var world = CreateWorld(10, 10, 42);
            // 4x4 forest block at (2..5, 2..5); interior = 3,3 / 3,4 / 4,3 / 4,4
            for (int x = 2; x <= 5; x++)
            for (int y = 2; y <= 5; y++)
            {
                world.BiomeMap[x, y] = "forest-dense";
                world.GameplayTileMap[x, y] = "forest-dense";
            }
            _config.Layers = new[]
            {
                Layer("fern", weight: 2f, maxPerTile: 2,
                    forest: DecorationForestAffinity.Interior)
            };

            var result = NewGenerator().Generate(world);

            Assert.Greater(result.Count, 0);
            foreach (var p in result.Placements)
            {
                Assert.IsTrue(p.TileX >= 3 && p.TileX <= 4 && p.TileY >= 3 && p.TileY <= 4,
                    $"interior fern at {p.TileX},{p.TileY} must be inside the deep-forest cells");
            }
        }

        [Test]
        public void ForestAffinity_Edge_OnlyForestRim()
        {
            AddLayerPool("sapling", "veg-sapling-a");
            var world = CreateWorld(10, 10, 42);
            for (int x = 2; x <= 5; x++)
            for (int y = 2; y <= 5; y++)
            {
                world.BiomeMap[x, y] = "forest-dense";
                world.GameplayTileMap[x, y] = "forest-dense";
            }
            _config.Layers = new[]
            {
                Layer("sapling", weight: 2f, maxPerTile: 1,
                    forest: DecorationForestAffinity.Edge)
            };

            var result = NewGenerator().Generate(world);

            Assert.Greater(result.Count, 0);
            foreach (var p in result.Placements)
            {
                bool inForest = p.TileX >= 2 && p.TileX <= 5 && p.TileY >= 2 && p.TileY <= 5;
                bool interior = p.TileX >= 3 && p.TileX <= 4 && p.TileY >= 3 && p.TileY <= 4;
                Assert.IsTrue(inForest && !interior,
                    $"edge sapling at {p.TileX},{p.TileY} must sit on the forest rim");
            }
        }

        [Test]
        public void TreeAnchorGroves_EnableForestAffinity_WithoutForestTiles()
        {
            // Recipe worlds never emit "forest-*" tile ids — the visible forest
            // is wherever legacy trees actually clustered. Tree anchors must
            // therefore mark grove cells so interior/edge affinities still fire.
            AddLayerPool("tree", "tree-a");
            AddLayerPool("fern", "veg-fern-a");
            var world = CreateWorld(10, 10, 42); // all "grass", no forest ids
            _config.TypeDensities.TreeDensity = 0.9f;
            _config.Layers = new[]
            {
                Layer("fern", weight: 2f, maxPerTile: 2,
                    forest: DecorationForestAffinity.Interior)
            };

            var result = NewGenerator().Generate(world);
            var ferns = result.Placements.Where(p => p.Type == "fern").ToList();

            Assert.Greater(ferns.Count, 0,
                "tree clusters on non-forest tiles must still create interior grove cells");
        }

        [Test]
        public void SkipObjectCells_RespectsObjectMap()
        {
            AddLayerPool("grass", "veg-grass-a");
            var world = CreateWorld(6, 6, 42);
            world.ObjectMap[2, 2] = "resource-lumber";
            world.ObjectMap[3, 4] = "resource-stone";
            _config.Layers = new[]
            {
                Layer("grass", weight: 2f, maxPerTile: 3, skipObjects: true)
            };

            var result = NewGenerator().Generate(world);

            foreach (var p in result.Placements)
                Assert.IsFalse(
                    (p.TileX == 2 && p.TileY == 2) || (p.TileX == 3 && p.TileY == 4),
                    $"layer placement on object cell {p.TileX},{p.TileY}");
        }

        [Test]
        public void LandLayers_NeverOnWater()
        {
            AddLayerPool("grass", "veg-grass-a");
            _config.Layers = new[] { Layer("grass", weight: 2f, maxPerTile: 3) };
            var world = CreateWorld(6, 6, 42, "water");

            var result = NewGenerator().Generate(world);

            Assert.AreEqual(0, result.Count, "land layers must not spawn on water");
        }

        [Test]
        public void NearTreeBoost_OnlyNearTreeAnchors()
        {
            AddLayerPool("tree", "kaykit-tree-single-a");
            AddLayerPool("litter", "veg-twig-a");
            _config.TypeDensities.TreeDensity = 0.6f; // seed some legacy trees
            var litter = Layer("litter", weight: 0f, maxPerTile: 1);
            litter.NearTreeBoost = 2f;
            litter.NearTreeRadius = 1;
            _config.Layers = new[] { litter };

            var world = CreateWorld(12, 12, 42);
            var result = NewGenerator().Generate(world);

            var treeCells = new HashSet<Vector2Int>(
                result.Placements.Where(p => p.Type == "tree")
                    .Select(p => new Vector2Int(p.TileX, p.TileY)));
            Assert.Greater(treeCells.Count, 0, "world must contain tree anchors for this test");

            foreach (var p in result.Placements.Where(p => p.Type == "litter"))
            {
                bool near = treeCells.Any(t =>
                    Mathf.Abs(t.x - p.TileX) <= 1 && Mathf.Abs(t.y - p.TileY) <= 1);
                Assert.IsTrue(near,
                    $"litter at {p.TileX},{p.TileY} must be within 1 cell of a tree anchor");
            }
        }

        [Test]
        public void SaplingLayer_FeedsTreeAffinity()
        {
            AddLayerPool("sapling", "veg-sapling-a");
            AddLayerPool("litter", "veg-twig-a");
            var saplings = Layer("sapling", weight: 2f, maxPerTile: 1);
            saplings.FeedsTreeAffinity = true;
            var litter = Layer("litter", weight: 0f, maxPerTile: 1);
            litter.NearTreeBoost = 2f;
            litter.NearTreeRadius = 1;
            _config.Layers = new[] { saplings, litter };

            var result = NewGenerator().Generate(CreateWorld(8, 8, 42));

            Assert.IsTrue(result.Placements.Any(p => p.Type == "litter"),
                "sapling anchors should feed litter placement");
        }

        [Test]
        public void LayerScaleOverride_UsesLayerRange()
        {
            AddLayerPool("grass", "veg-grass-a");
            var rule = Layer("grass", weight: 2f, maxPerTile: 2);
            rule.MinScale = 2.0f;
            rule.MaxScale = 2.5f;
            _config.Layers = new[] { rule };

            var result = NewGenerator().Generate(CreateWorld(6, 6, 42));

            Assert.Greater(result.Count, 0);
            foreach (var p in result.Placements)
            {
                Assert.GreaterOrEqual(p.Scale.x, 2.0f);
                Assert.LessOrEqual(p.Scale.x, 2.5f);
            }
        }

        [Test]
        public void MaxSlopeMeters_SkipsSteepCells()
        {
            AddLayerPool("pebble", "veg-pebble");
            var rule = Layer("pebble", weight: 2f, maxPerTile: 2);
            rule.MaxSlopeMeters = 0.3f;
            _config.Layers = new[] { rule };

            var world = CreateWorld(8, 8, 42);
            var map = new LogicalTileMap(8, 8);
            // cliff column: left half at 0, right half at 5
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                map.SurfaceHeights[x, y] = x < 4 ? 0f : 5f;
            world.LogicalTileMap = map;

            var result = NewGenerator().Generate(world);

            foreach (var p in result.Placements)
                Assert.IsFalse(p.TileX == 3 || p.TileX == 4,
                    $"pebble at {p.TileX},{p.TileY} must skip the cliff-adjacent cells");
        }

        [Test]
        public void MissingPool_LayerSkipped()
        {
            _config.Layers = new[] { Layer("nonexistent", weight: 2f) };

            var result = NewGenerator().Generate(CreateWorld(6, 6, 42));

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void MaxPerTile_Respected()
        {
            AddLayerPool("grass", "veg-grass-a");
            _config.Layers = new[] { Layer("grass", weight: 2f, maxPerTile: 2) };

            var result = NewGenerator().Generate(CreateWorld(8, 8, 42));

            var perCell = result.Placements
                .GroupBy(p => (p.TileX, p.TileY))
                .Select(g => g.Count())
                .ToArray();
            Assert.IsTrue(perCell.All(c => c <= 2),
                "layer must never exceed maxPerTile on one cell");
        }

        private EnvironmentDecorationGenerator NewGenerator()
            => new EnvironmentDecorationGenerator(_config, _registry);
    }
}
