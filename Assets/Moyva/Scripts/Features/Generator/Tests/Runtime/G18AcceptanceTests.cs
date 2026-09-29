using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.MapChunks.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G18 acceptance invariants for waterfall VFX: instances spawn only for
    /// real fronts (active field), live under per-chunk roots so chunk culling
    /// applies, a rebuild clears before spawning (no accumulation), the map
    /// budget prefers the largest fronts, per-system particle counts are
    /// capped, and mist requires the taller-drop threshold.
    /// </summary>
    public sealed class G18AcceptanceTests
    {
        private static readonly List<Object> ToDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in ToDestroy)
                if (o != null) Object.DestroyImmediate(o);
            ToDestroy.Clear();
        }

        private static T Track<T>(T o) where T : Object
        {
            ToDestroy.Add(o);
            return o;
        }

        private sealed class FakeLayout : IMapChunkLayoutService
        {
            public bool IsConfigured => true;
            public int Width => 32;
            public int Height => 32;
            public int ChunkSize => 16;
            public float CellSize => 1f;
            public IReadOnlyList<MapChunkDescriptor> Chunks => null;
            public void Configure(int width, int height, float cellSize,
                bool hasWorldBounds, Bounds worldBounds) { }
            public bool TryGetChunkCoord(Vector2Int tile, out MapChunkCoord coord)
            {
                coord = new MapChunkCoord(
                    Mathf.Clamp(tile.x / 16, 0, 1), Mathf.Clamp(tile.y / 16, 0, 1));
                return tile.x >= 0 && tile.y >= 0 && tile.x < 32 && tile.y < 32;
            }
            public bool TryGetDescriptor(MapChunkCoord c, out MapChunkDescriptor d)
            {
                d = default; return true;
            }
            public int GetChunksOverlapping(Bounds b, List<MapChunkCoord> r) => 0;
        }

        private sealed class FakeRoots : IMapVisualChunkRootService
        {
            public readonly Dictionary<MapChunkCoord, Transform> Roots =
                new Dictionary<MapChunkCoord, Transform>();
            public Transform GetOrCreateRoot(MapChunkCoord coord)
            {
                if (Roots.TryGetValue(coord, out var t) && t != null)
                    return t;
                var go = Track(new GameObject($"MapChunk_{coord.X}_{coord.Y}"));
                Roots[coord] = go.transform;
                return go.transform;
            }
            public bool IsChunkRoot(Transform t) => Roots.ContainsValue(t);
            public bool TryGetOwnedChunk(Transform t, out MapChunkCoord c)
            {
                foreach (var kv in Roots)
                    if (t == kv.Value || t.IsChildOf(kv.Value))
                    {
                        c = kv.Key; return true;
                    }
                c = default; return false;
            }
        }

        private sealed class HydroStub : IRecipeHydrologyMap
        {
            public RecipeWaterfallConfig WaterfallConfig;
            public readonly HashSet<Vector2Int> Water = new HashSet<Vector2Int>();
            public int Version => 1;
            public bool HasHydrology => WaterfallConfig != null;
            public float WaterfallMinDropMeters => 0.5f;
            public RecipeSeabedConfig Seabed => null;
            public RecipeWaterfallConfig Waterfalls => WaterfallConfig;
            public bool IsRiverCell(Vector2Int c) => Water.Contains(c);
            public bool IsLakeCell(Vector2Int c) => Water.Contains(c);
            public bool IsWaterCell(Vector2Int c) => Water.Contains(c);
            public RecipeWaterKind GetWaterKind(Vector2Int c)
                => Water.Contains(c) ? RecipeWaterKind.River : RecipeWaterKind.None;
            public bool TryGetWaterSurface(Vector2Int c, out float s)
            {
                s = Water.Contains(c) ? 0f : float.NaN;
                return Water.Contains(c);
            }
            public bool TryGetBedHeight(Vector2Int c, out float b)
            {
                b = Water.Contains(c) ? -1f : float.NaN;
                return Water.Contains(c);
            }
            public bool TryGetFlowDirection(Vector2Int c, out Vector2Int d)
            {
                d = c; return Water.Contains(c);
            }
            public bool TryGetWaterfall(Vector2Int c, out Vector2Int d,
                out float upperY, out float lowerY)
            {
                d = c; upperY = lowerY = 0f; return false;
            }
        }

        private static TileLayerSample WaterSheet(float surface) =>
            new TileLayerSample("water", "water", null, null, "w", null,
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SurfaceOnly);

        private static GameObject FoamPrefab()
        {
            var go = Track(new GameObject("foam"));
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.maxParticles = 500; // over budget — spawner must clamp
            var shape = ps.shape;
            shape.scale = new Vector3(1f, 1f, 1f);
            return go;
        }

        private HydroStub _hydro;
        private FakeRoots _roots;
        private WaterfallVfxSpawner _spawner;
        private WaterfallChunkMeshService _falls;

        [SetUp]
        public void SetUp()
        {
            _hydro = new HydroStub
            {
                WaterfallConfig = new RecipeWaterfallConfig
                {
                    Enabled = true,
                    MinDropLevels = 1,
                    MistMinDropLevels = 3,
                    MaxVfxPerMap = 18,
                    MaxParticlesPerVfx = 60,
                    EdgeFoamPrefab = FoamPrefab(),
                    ImpactSplashPrefab = FoamPrefab(),
                    MistPrefab = FoamPrefab(),
                    CurtainMaterial = Track(
                        new Material(Shader.Find("Hidden/InternalErrorShader"))),
                },
            };
            _falls = new WaterfallChunkMeshService(_hydro, null);
            _roots = new FakeRoots();
            _spawner = new WaterfallVfxSpawner(new FakeLayout(), _roots);
        }

        private void PrepareFall()
        {
            var cells = new Dictionary<Vector2Int, ResolvedTileComposition>();
            foreach (var (c, s) in new[]
                     { (new Vector2Int(3, 3), 5f), (new Vector2Int(4, 3), 4f) })
            {
                cells[c] = new ResolvedTileComposition(
                    c, WaterSheet(s), default, true, false, "g18");
                _hydro.Water.Add(c);
            }
            _falls.Prepare(cells, 32, 32, 1f);
        }

        [Test]
        public void Spawn_EmitsEdgeAndSplash_ButNoMist_BelowMistDrop()
        {
            PrepareFall();
            int spawned = _spawner.Spawn(_falls);
            Assert.AreEqual(2, spawned, "edge foam + splash, mist gated by drop");
            var chunkRoot = _roots.Roots[new MapChunkCoord(0, 0)]
                .Find("Waterfalls");
            Assert.IsNotNull(chunkRoot, "VFX must live under a per-chunk root");
            Assert.AreEqual(2, chunkRoot.childCount);
            var names = new List<string>();
            foreach (Transform child in chunkRoot)
                names.Add(child.name);
            Assert.IsTrue(names.Exists(n => n.StartsWith("wfall_edge_")));
            Assert.IsTrue(names.Exists(n => n.StartsWith("wfall_splash_")));
            Assert.IsFalse(names.Exists(n => n.StartsWith("wfall_mist_")));
        }

        [Test]
        public void Respawn_ClearsPrevious_Instances_NoAccumulation()
        {
            PrepareFall();
            _spawner.Spawn(_falls);
            _spawner.Spawn(_falls);
            var chunkRoot = _roots.Roots[new MapChunkCoord(0, 0)]
                .Find("Waterfalls");
            Assert.AreEqual(2, chunkRoot.childCount,
                "rebuild must clear its own root before spawning");
        }

        [Test]
        public void MapBudget_LimitsTotalVfx()
        {
            _hydro.WaterfallConfig.MaxVfxPerMap = 1;
            PrepareFall();
            Assert.AreEqual(1, _spawner.Spawn(_falls),
                "budget caps spawned instances, largest front wins");
        }

        [Test]
        public void LimitedBudget_CoversBothFallsBeforeAddingSecondaryEffects()
        {
            _hydro.WaterfallConfig.MaxVfxPerMap = 2;
            var cells = new Dictionary<Vector2Int, ResolvedTileComposition>();
            foreach (int row in new[] { 3, 8 })
            foreach (int column in new[] { 3, 4 })
            {
                var cell = new Vector2Int(column, row);
                cells[cell] = new ResolvedTileComposition(cell, WaterSheet(column == 3 ? 5f : 4f),
                    default, true, false, "budget");
                _hydro.Water.Add(cell);
            }
            _falls.Prepare(cells, 32, 32, 1f);
            Assert.AreEqual(2, _spawner.Spawn(_falls));
            var root = _roots.Roots[new MapChunkCoord(0, 0)].Find("Waterfalls");
            foreach (Transform child in root)
            {
                StringAssert.StartsWith("wfall_splash_", child.name);
                var system = child.GetComponent<ParticleSystem>();
                Assert.Greater(system.particleCount, 0, "Effects must be visible immediately after generation.");
                Assert.GreaterOrEqual(system.main.startSize.constantMax * system.transform.lossyScale.x,
                    0.16f, "Tile-scale effects must remain readable from the gameplay camera.");
            }
        }

        [Test]
        public void ParticleSystems_AreCapped()
        {
            PrepareFall();
            _spawner.Spawn(_falls);
            var chunkRoot = _roots.Roots[new MapChunkCoord(0, 0)].Find("Waterfalls");
            foreach (Transform child in chunkRoot)
            {
                foreach (var ps in child.GetComponentsInChildren<ParticleSystem>(true))
                    Assert.LessOrEqual(ps.main.maxParticles, 60,
                        "per-system particle count must respect the budget");
            }
        }

        [Test]
        public void LargeAuthoredParticles_FitCellWithoutShrinkingEmitterWidth()
        {
            var prefab = _hydro.WaterfallConfig.ImpactSplashPrefab;
            var authored = prefab.GetComponent<ParticleSystem>().main;
            authored.startSize = new ParticleSystem.MinMaxCurve(4f, 8f);
            PrepareFall();
            _spawner.Spawn(_falls);
            var root = _roots.Roots[new MapChunkCoord(0, 0)].Find("Waterfalls");
            foreach (Transform child in root)
            {
                var ps = child.GetComponent<ParticleSystem>();
                Assert.LessOrEqual(ps.main.startSize.constantMax * ps.transform.lossyScale.x,
                    0.3f, "Particles must be smaller than one tile.");
                Assert.AreEqual(1f, ps.shape.scale.x * ps.transform.lossyScale.x, 0.001f,
                    "The emitter must still cover the entire front.");
                Assert.AreEqual(ParticleSystemScalingMode.Hierarchy, ps.main.scalingMode);
            }
            Assert.AreEqual(8f, prefab.GetComponent<ParticleSystem>().main.startSize.constantMax,
                "Spawning must not mutate the source prefab.");
        }

        [Test]
        public void Sw3Particles_StayNearTheirWaterfallAfterSimulation()
        {
            const string folder = "Assets/ThirdParty/Stylized Water 3/Prefabs/Particles/";
            _hydro.WaterfallConfig.EdgeFoamPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(folder + "Waterfall Edge.prefab");
            _hydro.WaterfallConfig.ImpactSplashPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(folder + "Waterfall Impact Splashes.prefab");
            _hydro.WaterfallConfig.MistPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(folder + "WaterfallMist.prefab");
            _hydro.WaterfallConfig.MistMinDropLevels = 1;
            PrepareFall();
            Assert.AreEqual(3, _spawner.Spawn(_falls));
            var root = _roots.Roots[new MapChunkCoord(0, 0)].Find("Waterfalls");
            foreach (var system in root.GetComponentsInChildren<ParticleSystem>())
            {
                system.Simulate(3f, false, true);
                var particles = new ParticleSystem.Particle[system.main.maxParticles];
                int count = system.GetParticles(particles);
                Assert.Greater(count, 0, system.name);
                for (int i = 0; i < count; i++)
                {
                    var delta = system.transform.TransformVector(particles[i].position);
                    Assert.Less(delta.magnitude, 1.1f,
                        system.name + " must not scatter particles metres away from a one-cell fall.");
                }
            }
        }

        [Test]
        public void NoPrefabs_OrInactiveField_SpawnsNothing()
        {
            _hydro.WaterfallConfig = new RecipeWaterfallConfig
            {
                Enabled = true,
                CurtainMaterial = Track(
                    new Material(Shader.Find("Hidden/InternalErrorShader"))),
            };
            PrepareFall();
            Assert.AreEqual(0, _spawner.Spawn(_falls),
                "no prefabs configured → nothing spawns");
            Assert.AreEqual(0, _spawner.Spawn(null));
        }
    }
}
