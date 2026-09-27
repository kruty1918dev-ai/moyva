using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.MapChunks.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G19 acceptance invariants for the object spawner: decor instances come
    /// only from real map data (ObjectMap/BuildingMap ids resolved through the
    /// registry mapping), land props never sit on water while water-authored
    /// props do, cells cleared by a building footprint stay clear, and
    /// instances live under per-chunk roots that Clear() empties.
    /// </summary>
    public sealed class G19AcceptanceTests
    {
        private static readonly List<UnityEngine.Object> ToDestroy = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in ToDestroy)
                if (o != null) UnityEngine.Object.DestroyImmediate(o);
            ToDestroy.Clear();
        }

        private static T Track<T>(T o) where T : UnityEngine.Object
        {
            ToDestroy.Add(o);
            return o;
        }

        private sealed class FakeEnv : ITileWorldCreatorBuildEnvironment
        {
            public TileWorldCreatorIdMappingSO Map;
            public TileWorldCreatorManager Manager => null;
            public TileWorldCreatorIdMappingSO Mapping => Map;
            public TileWorldCreatorBuildOptions Options { get; } =
                new TileWorldCreatorBuildOptions();
        }

        private sealed class FakeLayout : IMapChunkLayoutService
        {
            public bool IsConfigured => true;
            public int Width => 32;
            public int Height => 32;
            public int ChunkSize => 16;
            public float CellSize => 1f;
            public IReadOnlyList<MapChunkDescriptor> Chunks => null;
            public void Configure(int w, int h, float cs, bool hb, Bounds b) { }
            public bool TryGetChunkCoord(Vector2Int tile, out MapChunkCoord coord)
            {
                coord = new MapChunkCoord(tile.x / 16, tile.y / 16);
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

        private FakeEnv _env;
        private FakeRoots _roots;
        private ChunkFirstObjectSpawner _spawner;
        private GameObject _treePrefab;
        private GameObject _lilyPrefab;
        private GameObject _hallPrefab;

        private static void AddLayer(TileWorldCreatorIdMappingSO so,
            string field, string pattern, GameObject prefab)
        {
            var entry = new TileWorldCreatorIdMappingSO.LayerMapping();
            SetField(entry, "_idPattern", pattern);
            SetField(entry, "_registryVisualPrefab", prefab);

            var current = (TileWorldCreatorIdMappingSO.LayerMapping[])
                (GetField(so, field) ?? Array.Empty<TileWorldCreatorIdMappingSO.LayerMapping>());
            var next = new List<TileWorldCreatorIdMappingSO.LayerMapping>(current) { entry };
            SetField(so, field, next.ToArray());
        }

        private static object GetField(object target, string name)
        {
            var field = target.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field '{name}'.");
            return field.GetValue(target);
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field '{name}'.");
            field.SetValue(target, value);
        }

        [SetUp]
        public void SetUp()
        {
            _treePrefab = Track(new GameObject("tree-oak"));
            _lilyPrefab = Track(new GameObject("water-lily"));
            _hallPrefab = Track(new GameObject("town-hall"));

            var mapping = new TileWorldCreatorIdMappingSO();
            AddLayer(mapping, "_objectLayers", "tree-oak", _treePrefab);
            AddLayer(mapping, "_objectLayers", "water-lily", _lilyPrefab);
            AddLayer(mapping, "_buildingLayers", "town-hall", _hallPrefab);

            _env = new FakeEnv { Map = mapping };
            _roots = new FakeRoots();
            _spawner = new ChunkFirstObjectSpawner(
                _env, new FakeLayout(), _roots);
        }

        private static GeneratedWorldData World(
            string[,] objects, string[,] biomes, string[,] buildings = null)
        {
            return new GeneratedWorldData
            {
                Width = biomes.GetLength(0),
                Height = biomes.GetLength(1),
                Seed = 7,
                ObjectMap = objects,
                BiomeMap = biomes,
                BuildingMap = buildings,
            };
        }

        private static string[,] Fill(string id, int w, int h)
        {
            var m = new string[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                m[x, y] = id;
            return m;
        }

        private int CountProps()
        {
            int total = 0;
            foreach (var kv in _roots.Roots)
            {
                var root = kv.Value.Find("Objects");
                if (root != null)
                    total += root.childCount;
            }
            return total;
        }

        [Test]
        public void LandProp_Spawns_UnderChunkObjectsRoot()
        {
            var objects = Fill(null, 32, 32);
            objects[4, 4] = "tree-oak";
            var world = World(objects, Fill("grass", 32, 32));

            Assert.AreEqual(1, _spawner.Spawn(world));
            var objectsRoot = _roots.Roots[new MapChunkCoord(0, 0)].Find("Objects");
            Assert.IsNotNull(objectsRoot);
            Assert.AreEqual(1, objectsRoot.childCount);
            Assert.IsTrue(objectsRoot.GetChild(0).name.StartsWith("tree-oak"),
                "decor instance must carry the resolved prefab identity");
        }

        [Test]
        public void LandProp_OnWater_IsSkipped_ButWaterAuthored_Spawns()
        {
            var biomes = Fill("grass", 32, 32);
            biomes[4, 4] = "water-lake";
            biomes[6, 6] = "water-lake";
            var objects = Fill(null, 32, 32);
            objects[4, 4] = "tree-oak";    // land tree dropped on water → skip
            objects[6, 6] = "water-lily";  // authored for water → spawn

            int spawned = _spawner.Spawn(World(objects, biomes));
            Assert.AreEqual(1, spawned, "only the water-authored prop survives");
            var objectsRoot = _roots.Roots[new MapChunkCoord(0, 0)].Find("Objects");
            Assert.AreEqual(1, objectsRoot.childCount);
            Assert.IsTrue(objectsRoot.GetChild(0).name.StartsWith("water-lily"));
        }

        [Test]
        public void BuildingMap_IgnoresWaterGate()
        {
            var biomes = Fill("grass", 32, 32);
            biomes[4, 4] = "water-lake";
            var buildings = Fill(null, 32, 32);
            buildings[4, 4] = "town-hall"; // docks/mills may stand on water

            int spawned = _spawner.Spawn(World(Fill(null, 32, 32), biomes, buildings));
            Assert.AreEqual(1, spawned, "buildings are not subject to the water prop gate");
        }

        [Test]
        public void UnmappedOrEmpty_Ids_SpawnNothing()
        {
            var objects = Fill(null, 32, 32);
            objects[2, 2] = "unknown-rock";
            objects[3, 3] = "";
            Assert.AreEqual(0, _spawner.Spawn(World(objects, Fill("grass", 32, 32))));
        }

        [Test]
        public void ClearedPropCells_DestroyProps_NewWorldBuildReSeeds()
        {
            var objects = Fill(null, 32, 32);
            objects[4, 4] = "tree-oak";
            objects[5, 5] = "tree-oak";
            var world = World(objects, Fill("grass", 32, 32));

            _spawner.Spawn(world);
            Assert.AreEqual(2, CountProps());
            Assert.AreEqual(1, _spawner.ClearPropsInCells(
                new List<Vector2Int> { new Vector2Int(4, 4) }));
            Assert.AreEqual(1, CountProps(),
                "the cleared cell's prop is destroyed, the other survives");

            // A fresh world build resets the exclusion and re-seeds decor.
            _spawner.Spawn(world);
            Assert.AreEqual(2, CountProps(), "new world build re-seeds decor");
        }

        [Test]
        public void Clear_EmptiesAllChunkRoots()
        {
            var objects = Fill(null, 32, 32);
            objects[4, 4] = "tree-oak";
            objects[20, 4] = "tree-oak"; // lands in chunk (1,0)
            _spawner.Spawn(World(objects, Fill("grass", 32, 32)));
            Assert.AreEqual(2, CountProps(), "props span two chunk roots");
            _spawner.Clear();
            Assert.AreEqual(0, CountProps(), "Clear must empty every root");
        }
    }
}
