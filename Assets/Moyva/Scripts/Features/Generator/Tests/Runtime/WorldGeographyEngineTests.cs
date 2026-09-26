using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Focused invariants for the deterministic world-geography engine:
    /// repeatability, terrain-level coherence, hydrology sinks, spawn
    /// validity and archetype diversity.
    /// </summary>
    public sealed class WorldGeographyEngineTests
    {
        private static WorldGenerationConfig Config() => new WorldGenerationConfig();

        private static WorldGenerationRequest Request(
            int seed, WorldArchetype archetype, int width = 48, int height = 48,
            int players = 2, WorldGenerationConfig config = null)
        {
            config ??= Config();
            var t = config.TerrainLevels;
            return new WorldGenerationRequest(
                seed, width, height, archetype, players, config,
                t.WaterLevel, t.ShoreLevel, t.LandLevel, t.HillLevel, t.MaxLevel,
                t.HeightStep, t.WaterSurfaceOffset);
        }

        [Test]
        public void Generate_SameSeed_ProducesIdenticalMaps()
        {
            var engine = new WorldGeographyEngine();
            var a = engine.Generate(Request(1337, WorldArchetype.Balanced));
            var b = engine.Generate(Request(1337, WorldArchetype.Balanced));

            Assert.AreEqual(a.Attempt, b.Attempt);
            Assert.AreEqual(a.SpawnHints.Length, b.SpawnHints.Length);
            for (int i = 0; i < a.SpawnHints.Length; i++)
                Assert.AreEqual(a.SpawnHints[i], b.SpawnHints[i]);

            for (int x = 0; x < a.Width; x++)
            for (int y = 0; y < a.Height; y++)
            {
                Assert.AreEqual(a.TileMap[x, y], b.TileMap[x, y], $"tile {x},{y}");
                Assert.AreEqual(a.TerrainLevelMap[x, y], b.TerrainLevelMap[x, y]);
                Assert.AreEqual(a.HeightMap[x, y], b.HeightMap[x, y], 1e-5f);
                Assert.AreEqual(a.ObjectMap[x, y], b.ObjectMap[x, y]);
            }
        }

        [Test]
        public void Generate_DifferentSeeds_ProduceDifferentMaps()
        {
            var engine = new WorldGeographyEngine();
            var a = engine.Generate(Request(1, WorldArchetype.Balanced));
            var b = engine.Generate(Request(2, WorldArchetype.Balanced));

            int diff = 0;
            for (int x = 0; x < a.Width; x++)
            for (int y = 0; y < a.Height; y++)
                if (a.TileMap[x, y] != b.TileMap[x, y]) diff++;
            Assert.Greater(diff, a.Width * a.Height / 10, "seeds should diverge meaningfully");
        }

        [Test]
        public void Generate_TerrainLevels_AreCoherentWithTiles()
        {
            var engine = new WorldGeographyEngine();
            var r = engine.Generate(Request(42, WorldArchetype.Continents));
            var t = Config().TerrainLevels;

            for (int x = 0; x < r.Width; x++)
            for (int y = 0; y < r.Height; y++)
            {
                int level = r.TerrainLevelMap[x, y];
                Assert.GreaterOrEqual(level, 0);
                Assert.LessOrEqual(level, t.MaxLevel);

                bool water = r.RiverMask[x, y] || r.LakeMask[x, y] || level == t.WaterLevel;
                if (water)
                    Assert.AreEqual("water", r.TileMap[x, y], $"water cell {x},{y}");
                else
                    Assert.AreNotEqual("water", r.TileMap[x, y], $"land cell {x},{y}");

                // Land surfaces sit at level*HeightStep; water sits at the
                // surface offset, raised slightly in shallow coastal bands or
                // up to the flooded rim level for lakes.
                if (water)
                {
                    float surface = r.HeightMap[x, y];
                    float minSurface = level * t.HeightStep + t.WaterSurfaceOffset;
                    Assert.GreaterOrEqual(surface, minSurface - 1e-4f,
                        $"water surface {x},{y} below its level floor");
                    float maxSurface = r.LakeMask[x, y]
                        ? t.MaxLevel * t.HeightStep + t.WaterSurfaceOffset
                        : minSurface + 0.11f;
                    Assert.LessOrEqual(surface, maxSurface + 1e-4f,
                        $"water surface {x},{y} above expected cap");
                }
                else
                {
                    Assert.AreEqual(level * t.HeightStep, r.HeightMap[x, y], 1e-4f, $"height {x},{y}");
                }
            }
        }

        [Test]
        public void Generate_Rivers_DrainToSinks()
        {
            var engine = new WorldGeographyEngine();
            var r = engine.Generate(Request(77, WorldArchetype.Highlands));

            Assert.Greater(r.Report.RiverCells, 0, "highlands should produce rivers");
            Assert.IsNotNull(r.FlowParent);

            // Every river cell's downstream flood-parent chain must terminate
            // at a sink: a water/lake cell or a map border (rivers may legally
            // flow off the world edge). The parent graph is acyclic by
            // construction, but guard anyway.
            for (int x = 0; x < r.Width; x++)
            for (int y = 0; y < r.Height; y++)
            {
                if (!r.RiverMask[x, y]) continue;

                int cx = x, cy = y;
                int guard = r.Width * r.Height;
                while (guard-- > 0)
                {
                    int parent = r.FlowParent[cx, cy];
                    if (parent < 0)
                    {
                        bool sinkIsWaterOrBorder =
                            r.TileMap[cx, cy] == "water"
                            || r.LakeMask[cx, cy]
                            || cx == 0 || cy == 0
                            || cx == r.Width - 1 || cy == r.Height - 1;
                        Assert.IsTrue(sinkIsWaterOrBorder,
                            $"river {x},{y} dead-ends on dry interior cell {cx},{cy}");
                        break;
                    }
                    cx = parent % r.Width;
                    cy = parent / r.Width;
                }
                Assert.Greater(guard, 0, $"river trace looped from {x},{y}");
            }
        }

        [Test]
        public void Generate_SpawnHints_ArePassableAndSeparated()
        {
            var engine = new WorldGeographyEngine();
            const int players = 4;
            var r = engine.Generate(Request(2024, WorldArchetype.Pangaea, players: players));

            Assert.GreaterOrEqual(r.SpawnHints.Length, players,
                "pangaea should offer at least one hint per player");

            var seen = new System.Collections.Generic.HashSet<Vector2Int>();
            foreach (var hint in r.SpawnHints)
            {
                Assert.GreaterOrEqual(hint.x, 0);
                Assert.GreaterOrEqual(hint.y, 0);
                Assert.Less(hint.x, r.Width);
                Assert.Less(hint.y, r.Height);
                Assert.AreNotEqual("water", r.TileMap[hint.x, hint.y],
                    $"spawn {hint} on water");
                Assert.IsFalse(r.RiverMask[hint.x, hint.y] || r.LakeMask[hint.x, hint.y]);
                Assert.IsTrue(seen.Add(hint), "duplicate spawn hint");
            }
        }

        [Test]
        public void Generate_Objects_OnlyOnPassableLand()
        {
            var engine = new WorldGeographyEngine();
            var r = engine.Generate(Request(555, WorldArchetype.Balanced));

            for (int x = 0; x < r.Width; x++)
            for (int y = 0; y < r.Height; y++)
            {
                if (string.IsNullOrEmpty(r.ObjectMap[x, y])) continue;
                Assert.AreNotEqual("water", r.TileMap[x, y], $"object {x},{y} on water");
                Assert.AreNotEqual("mountain", r.TileMap[x, y]);
                Assert.AreNotEqual("snow", r.TileMap[x, y]);
            }
        }

        [Test]
        public void Generate_Archetypes_DifferInLandAndSand()
        {
            var engine = new WorldGeographyEngine();
            var desert = engine.Generate(Request(9, WorldArchetype.Desert));
            var islands = engine.Generate(Request(9, WorldArchetype.Islands));
            var pangaea = engine.Generate(Request(9, WorldArchetype.Pangaea));

            int desertSand = CountTiles(desert, "sand");
            int islandSand = CountTiles(islands, "sand");
            Assert.Greater(desertSand, islandSand * 1.2f,
                "desert archetype should be far sandier than islands");

            Assert.Greater(pangaea.Report.LandFraction, islands.Report.LandFraction,
                "pangaea should have more land than islands");
        }

        [Test]
        public void Generate_InvalidWorld_RetriesAndReports()
        {
            // Tiny map + many players forces gate failures; engine must still
            // return a deterministic best-effort candidate with diagnostics.
            var engine = new WorldGeographyEngine();
            var r = engine.Generate(Request(31337, WorldArchetype.Islands, 20, 20, 8));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.TileMap);
            Assert.IsNotNull(r.Report);
            if (!r.Report.Accepted)
                Assert.IsTrue(
                    r.Report.GateFailures.Exists(f => f.Contains("accepted-as-best-effort")),
                    "rejected candidate should be marked best-effort");
        }

        [Test]
        public void Generate_Forests_AppearAcrossSeedSpace()
        {
            var engine = new WorldGeographyEngine();
            var archetypes = new[]
            {
                WorldArchetype.Balanced, WorldArchetype.Pangaea,
                WorldArchetype.Continents, WorldArchetype.Highlands
            };
            int forestWorlds = 0;
            foreach (var archetype in archetypes)
            {
                int aWorlds = 0, aMax = 0, aSeed = 0;
                for (int seed = 1; seed <= 24; seed++)
                {
                    var r = engine.Generate(Request(seed, archetype));
                    if (r.Report.ForestCells > 0)
                    {
                        forestWorlds++;
                        aWorlds++;
                        if (r.Report.ForestCells > aMax)
                        {
                            aMax = r.Report.ForestCells;
                            aSeed = seed;
                        }
                    }
                }
                TestContext.WriteLine(
                    $"{archetype}: forestWorlds={aWorlds}/24 maxCells={aMax} bestSeed={aSeed}");
            }
            Assert.Greater(forestWorlds, 0,
                "no forest tiles across 24 seeds x 4 archetypes — biome threshold unreachable");
        }

        private static int CountTiles(WorldGeographyResult r, string id)
        {
            int n = 0;
            for (int x = 0; x < r.Width; x++)
            for (int y = 0; y < r.Height; y++)
                if (r.TileMap[x, y] == id) n++;
            return n;
        }
    }
}
