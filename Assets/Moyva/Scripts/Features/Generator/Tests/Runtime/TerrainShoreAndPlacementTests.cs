using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class TerrainShoreAndPlacementTests
    {
        private const float WaterLevel = -0.25f;

        private static TileLayerSample Land(string tileId, float surface)
            => new TileLayerSample(
                "terrain", "Terrain", null, null, tileId, tileId,
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SolidTerrain);

        private static TileLayerSample Water(float surface)
            => new TileLayerSample(
                "water", "Water", null, null, "water", "water",
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SurfaceOnly);

        private static LogicalTileMap BuildMap(int width, int height)
            => new LogicalTileMap(width, height);

        private static TerrainShoreConfig Config(int band = 1, int blend = 0)
            => new TerrainShoreConfig
            {
                Enabled = true,
                ShoreTileId = "sand",
                BandCells = band,
                BlendCells = blend,
                ShoreLiftMeters = 0.04f,
                RisePerCellMeters = 0.2f,
                MaxDropToWaterMeters = 0.9f
            };

        [Test]
        public void AdjacentLand_ConvertsToSandAndDropsToWaterline()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(WaterLevel));
            map.AddSample(1, 0, Land("grass", 0.5f));
            map.AddSample(2, 0, Land("grass", 0.5f));

            new TerrainShorePlanner().Apply(map, Config(band: 1, blend: 0), null);
            map.ReprojectAll();

            Assert.AreEqual("sand", map.TileIds[1, 0]);
            Assert.AreEqual(WaterLevel + 0.04f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void BlendRing_CapsHeightWithoutChangingTile()
        {
            var map = BuildMap(4, 1);
            map.AddSample(0, 0, Water(WaterLevel));
            map.AddSample(1, 0, Land("grass", 0.5f));
            map.AddSample(2, 0, Land("grass", 0.5f));
            map.AddSample(3, 0, Land("grass", 5f)); // cliff: outside max drop

            new TerrainShorePlanner().Apply(map, Config(band: 1, blend: 1), null);
            map.ReprojectAll();

            // distance 2 -> blend cell: grass stays grass but is capped.
            Assert.AreEqual("grass", map.TileIds[2, 0]);
            Assert.AreEqual(WaterLevel + 0.04f + 0.2f, map.SurfaceHeights[2, 0], 0.0001f);

            // Ramp between neighbours stays within RisePerCellMeters.
            float step = map.SurfaceHeights[2, 0] - map.SurfaceHeights[1, 0];
            Assert.LessOrEqual(step, 0.2f + 0.0001f);
        }

        [Test]
        public void CliffAboveWater_StaysUntouched()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(WaterLevel));
            map.AddSample(1, 0, Land("rock_cliff", 5f));

            new TerrainShorePlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual("rock_cliff", map.TileIds[1, 0]);
            Assert.AreEqual(5f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void WaterCells_AreNeverMutated()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(WaterLevel));
            map.AddSample(1, 0, Water(WaterLevel));

            new TerrainShorePlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual("water", map.TileIds[0, 0]);
            Assert.AreEqual(WaterLevel, map.SurfaceHeights[0, 0], 0.0001f);
            Assert.AreEqual(WaterLevel, map.SurfaceHeights[1, 0], 0.0001f);
        }

        // A swamp tile renders water but is a solid non-water-like tile id:
        // only the shore-local WaterTileIds extension may count it as water,
        // otherwise water-adjacent land never lifts and the wash sheet floats.
        private static TileLayerSample Swamp(float surface)
            => new TileLayerSample(
                "swamp-layer", "Swamp", null, null, "swamp", "swamp",
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SolidTerrain);

        [Test]
        public void NonWaterLikeWinner_LiftsNeighborViaConfigIds()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Swamp(0.49f));
            map.AddSample(1, 0, Land("grass", 0f));
            map.AddSample(2, 0, Land("grass", 0.4f));

            var config = Config(band: 1, blend: 0);
            config.WaterTileIds = new[] { "swamp" };
            new TerrainShorePlanner().Apply(map, config, null);
            map.ReprojectAll();

            Assert.AreEqual(0.49f + 0.04f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void NonWaterLikeWinner_WithoutConfigIds_StaysUnlifted()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Swamp(0.49f));
            map.AddSample(1, 0, Land("grass", 0f));

            new TerrainShorePlanner().Apply(map, Config(band: 1, blend: 0), null);
            map.ReprojectAll();

            Assert.AreEqual(0f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void NoWater_NoChanges()
        {
            var map = BuildMap(2, 2);
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                map.AddSample(x, y, Land("grass", 0.7f));

            new TerrainShorePlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual("grass", map.TileIds[0, 0]);
            Assert.AreEqual(0.7f, map.SurfaceHeights[0, 0], 0.0001f);
        }

        [Test]
        public void DisabledConfig_NoChanges()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(WaterLevel));
            map.AddSample(1, 0, Land("grass", 0.5f));

            var off = Config();
            off.Enabled = false;
            new TerrainShorePlanner().Apply(map, off, null);
            map.ReprojectAll();

            Assert.AreEqual("grass", map.TileIds[1, 0]);
            Assert.AreEqual(0.5f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void Planner_IsDeterministic()
        {
            LogicalTileMap Build()
            {
                var m = BuildMap(5, 3);
                m.AddSample(0, 1, Water(WaterLevel));
                for (int x = 1; x < 5; x++)
                for (int y = 0; y < 3; y++)
                    m.AddSample(x, y, Land("grass", 0.4f + x * 0.05f));
                return m;
            }

            var a = Build();
            var b = Build();
            var planner = new TerrainShorePlanner();
            var config = Config(band: 2, blend: 1);
            planner.Apply(a, config, null);
            planner.Apply(b, config, null);
            a.ReprojectAll();
            b.ReprojectAll();

            for (int x = 0; x < 5; x++)
            for (int y = 0; y < 3; y++)
            {
                Assert.AreEqual(a.TileIds[x, y], b.TileIds[x, y]);
                Assert.AreEqual(a.SurfaceHeights[x, y], b.SurfaceHeights[x, y], 0.00001f);
            }
        }

        // ---------------- placement policy ----------------

        private sealed class FakeTileTypes : ITileTypeRepository
        {
            private readonly Dictionary<string, TileTypeSnapshot> _map =
                new Dictionary<string, TileTypeSnapshot>();

            public int Revision => 0;
            public IReadOnlyCollection<TileTypeSnapshot> All => _map.Values;

            public void Add(string id, params string[] tags)
                => _map[id] = new TileTypeSnapshot(
                    id, id, "land", null, tags, new TileVisualConfig());

            public bool TryGet(string tileTypeId, out TileTypeSnapshot tileType)
                => _map.TryGetValue(tileTypeId ?? string.Empty, out tileType);

            public bool TryResolveId(string tileTypeIdOrAlias, out string canonicalTileTypeId)
            {
                canonicalTileTypeId = tileTypeIdOrAlias;
                return _map.ContainsKey(tileTypeIdOrAlias ?? string.Empty);
            }
        }

        [Test]
        public void SandTag_BlocksSpawnAndBuild_GrassAllowsAll()
        {
            var repo = new FakeTileTypes();
            repo.Add("sand", "land", "coast", "no-spawn", "no-build");
            repo.Add("grass", "land");
            var policy = new TerrainPlacementPolicy(repo);

            foreach (var op in new[]
                     {
                         TerrainPlacementOperation.Decoration,
                         TerrainPlacementOperation.ObjectSpawn,
                         TerrainPlacementOperation.UnitDeployment,
                         TerrainPlacementOperation.StartingPosition,
                         TerrainPlacementOperation.Building
                     })
            {
                Assert.IsFalse(policy.AllowsPlacement("sand", op), op.ToString());
                Assert.IsTrue(policy.AllowsPlacement("grass", op), op.ToString());
            }
        }

        [Test]
        public void Policy_WithoutRepository_StillBlocksShoreIds()
        {
            var policy = new TerrainPlacementPolicy(null);
            Assert.IsFalse(policy.AllowsPlacement("sand", TerrainPlacementOperation.ObjectSpawn));
            Assert.IsTrue(policy.AllowsPlacement("grass", TerrainPlacementOperation.ObjectSpawn));
            Assert.IsTrue(policy.AllowsPlacement(null, TerrainPlacementOperation.ObjectSpawn));
        }

        // ---------------- placement resolver ----------------

        private static GameObject MakeProp(Vector3 meshCenter, float meshSize)
        {
            var go = new GameObject("prop");
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    meshCenter + new Vector3(-meshSize, -meshSize, -meshSize),
                    meshCenter + new Vector3(meshSize, -meshSize, -meshSize),
                    meshCenter + new Vector3(-meshSize, meshSize, -meshSize),
                    meshCenter + new Vector3(meshSize, meshSize, -meshSize),
                    meshCenter + new Vector3(-meshSize, -meshSize, meshSize),
                    meshCenter + new Vector3(meshSize, -meshSize, meshSize),
                    meshCenter + new Vector3(-meshSize, meshSize, meshSize),
                    meshCenter + new Vector3(meshSize, meshSize, meshSize)
                },
                triangles = new[]
                {
                    0, 2, 1, 1, 2, 3, 4, 5, 6, 5, 7, 6
                }
            };
            mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>();
            return go;
        }

        [Test]
        public void GroundedY_LowersElevatedPivotOntoSurface()
        {
            // Mesh occupies y in [0.5, 1.5] around the pivot — the prefab
            // floats half a unit above its origin without grounding.
            var prop = MakeProp(new Vector3(0f, 1f, 0f), 0.5f);
            try
            {
                var resolver = new EnvironmentObjectPlacementResolver();
                float y = resolver.ResolveGroundedY(
                    prop,
                    new Vector3(2f, 0f, 2f),
                    Quaternion.identity,
                    Vector3.one,
                    1f,
                    _ => 0.2f,
                    0.2f);
                Assert.AreEqual(0.2f - 0.5f, y, 0.0001f);
            }
            finally
            {
                Object.DestroyImmediate(prop);
            }
        }

        [Test]
        public void Footprint_OverlappingInvalidCell_RejectedWithoutShift()
        {
            var prop = MakeProp(Vector3.zero, 1f); // 2x2 cells footprint
            try
            {
                var resolver = new EnvironmentObjectPlacementResolver();
                var request = new EnvironmentObjectPlacementResolver.Request(
                    prop,
                    new Vector3(1.5f, 0f, 1.5f),
                    Quaternion.identity,
                    Vector3.one,
                    4, 4, 1f,
                    cell => cell.x < 2, // only left half valid
                    _ => 0f,
                    maxShiftCells: 0f,
                    maxGroundDeltaMeters: 0f,
                    footprintShrink: 1f);

                Assert.IsFalse(resolver.TryResolve(request, out _));
            }
            finally
            {
                Object.DestroyImmediate(prop);
            }
        }

        [Test]
        public void Footprint_SmallOverlap_ShiftsInsideValidArea()
        {
            var prop = MakeProp(Vector3.zero, 0.4f); // ~1 cell footprint
            try
            {
                var resolver = new EnvironmentObjectPlacementResolver();
                var request = new EnvironmentObjectPlacementResolver.Request(
                    prop,
                    new Vector3(1.9f, 0f, 1.5f), // pokes into x>=2 invalid zone
                    Quaternion.identity,
                    Vector3.one,
                    4, 4, 1f,
                    cell => cell.x < 2,
                    _ => 0f,
                    maxShiftCells: 1f,
                    maxGroundDeltaMeters: 0f,
                    footprintShrink: 0.9f);

                Assert.IsTrue(resolver.TryResolve(request, out Vector3 resolved));
                Assert.Less(resolved.x, 1.9f); // pushed back into valid cells
            }
            finally
            {
                Object.DestroyImmediate(prop);
            }
        }

        [Test]
        public void Footprint_OutsideMapBounds_Rejected()
        {
            var prop = MakeProp(Vector3.zero, 0.4f);
            try
            {
                var resolver = new EnvironmentObjectPlacementResolver();
                var request = new EnvironmentObjectPlacementResolver.Request(
                    prop,
                    new Vector3(-0.2f, 0f, 1f),
                    Quaternion.identity,
                    Vector3.one,
                    4, 4, 1f,
                    _ => true,
                    _ => 0f,
                    maxShiftCells: 0f,
                    maxGroundDeltaMeters: 0f,
                    footprintShrink: 1f);

                Assert.IsFalse(resolver.TryResolve(request, out _));
            }
            finally
            {
                Object.DestroyImmediate(prop);
            }
        }
    }
}
