using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.MapChunks.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Tests for environment decoration spawning lifecycle and visual-only contract.
    /// </summary>
    [TestFixture]
    public sealed class EnvironmentDecorationSpawnerTests
    {
        private readonly List<GameObject> _created = new();
        private FakeChunkLayoutService _layout;
        private FakeChunkRootService _roots;
        private EnvironmentDecorationTests.MockMapObjectRegistryService _registry;
        private EnvironmentDecorationSpawner _spawner;

        [SetUp]
        public void Setup()
        {
            _layout = new FakeChunkLayoutService();
            _roots = new FakeChunkRootService();
            _registry = new EnvironmentDecorationTests.MockMapObjectRegistryService();
            _spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots);
        }

        [TearDown]
        public void TearDown()
        {
            if (_roots != null)
                for (int i = _roots.CreatedRoots.Count - 1; i >= 0; i--)
                    if (_roots.CreatedRoots[i] != null)
                        Object.DestroyImmediate(_roots.CreatedRoots[i].gameObject);
            for (int i = _created.Count - 1; i >= 0; i--)
                if (_created[i] != null)
                    Object.DestroyImmediate(_created[i]);
            _created.Clear();
        }

        [Test]
        public void Spawn_EmptyResult_ClearsPreviouslySpawnedDecorations()
        {
            // Arrange
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-a", prefab);
            var first = new DecorationPlacementResult(new[]
            {
                new DecorationPlacement("deco-a", Vector3.zero, Quaternion.identity, Vector3.one, 0, 0)
            });

            // Act
            int spawned = _spawner.Spawn(first);
            int cleared = _spawner.Spawn(DecorationPlacementResult.Empty);

            // Assert
            Assert.AreEqual(1, spawned);
            Assert.AreEqual(0, cleared);
            Assert.AreEqual(0, CountDecorationChildren(),
                "Spawning an empty result must clear decorations from the previous set");
        }

        [Test]
        public void Spawn_NullResult_ClearsPreviouslySpawnedDecorations()
        {
            // Arrange
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-a", prefab);
            _spawner.Spawn(new DecorationPlacementResult(new[]
            {
                new DecorationPlacement("deco-a", Vector3.zero, Quaternion.identity, Vector3.one, 0, 0)
            }));

            // Act
            _spawner.Spawn(null);

            // Assert
            Assert.AreEqual(0, CountDecorationChildren(),
                "Spawning null must clear decorations from the previous set");
        }

        [Test]
        public void Spawn_VisualOnlyContract_AppliesToChildRenderersAndColliders()
        {
            // Arrange: prefab whose renderer and collider live on a CHILD, not the root.
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-child", prefab);

            // Act
            int spawned = _spawner.Spawn(new DecorationPlacementResult(new[]
            {
                new DecorationPlacement("deco-child", Vector3.zero, Quaternion.identity, Vector3.one, 2, 3)
            }));

            // Assert
            Assert.AreEqual(1, spawned);
            var spawnedRoot = FindSpawnedDecorationRoot();
            Assert.IsNotNull(spawnedRoot, "Decoration root must exist under the chunk root");
            var renderers = spawnedRoot.GetComponentsInChildren<Renderer>(true);
            Assert.Greater(renderers.Length, 0);
            foreach (var r in renderers)
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, r.shadowCastingMode,
                    "Every child renderer must have shadows disabled");
            var colliders = spawnedRoot.GetComponentsInChildren<Collider>(true);
            Assert.AreEqual(0, colliders.Length,
                "Colliders on child objects must be removed for visual-only decorations");
        }

        private int CountDecorationChildren()
        {
            int total = 0;
            foreach (var root in _roots.CreatedRoots)
            {
                var deco = root.Find("EnvironmentDecorations");
                if (deco != null)
                    total += deco.childCount;
            }
            return total;
        }

        private Transform FindSpawnedDecorationRoot()
        {
            foreach (var root in _roots.CreatedRoots)
            {
                var deco = root.Find("EnvironmentDecorations");
                if (deco != null && deco.childCount > 0)
                    return deco;
            }
            return null;
        }

        private GameObject CreatePrefabWithChildVisuals()
        {
            var prefab = new GameObject("deco-prefab");
            _created.Add(prefab);
            var child = new GameObject("visual");
            _created.Add(child);
            child.transform.SetParent(prefab.transform, false);
            child.AddComponent<MeshFilter>();
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            child.AddComponent<BoxCollider>();
            return prefab;
        }

        private sealed class FakeChunkLayoutService : IMapChunkLayoutService
        {
            public bool IsConfigured => true;
            public int Width => 64;
            public int Height => 64;
            public int ChunkSize => 8;
            public float CellSize => 1f;
            public IReadOnlyList<MapChunkDescriptor> Chunks => System.Array.Empty<MapChunkDescriptor>();

            public void Configure(int width, int height, float cellSize, bool hasWorldBounds, Bounds worldBounds) { }

            public bool TryGetChunkCoord(Vector2Int tile, out MapChunkCoord coord)
            {
                coord = new MapChunkCoord(tile.x / ChunkSize, tile.y / ChunkSize);
                return true;
            }

            public bool TryGetDescriptor(MapChunkCoord coord, out MapChunkDescriptor descriptor)
            {
                descriptor = default;
                return true;
            }

            public int GetChunksOverlapping(Bounds worldBounds, List<MapChunkCoord> results) => 0;
        }

        private sealed class FakeChunkRootService : IMapVisualChunkRootService
        {
            private readonly Dictionary<MapChunkCoord, Transform> _roots = new();
            public readonly List<Transform> CreatedRoots = new();

            public Transform GetOrCreateRoot(MapChunkCoord coord)
            {
                if (_roots.TryGetValue(coord, out var existing) && existing != null)
                    return existing;
                var go = new GameObject($"MapChunk_{coord.X}_{coord.Y}");
                var t = go.transform;
                _roots[coord] = t;
                CreatedRoots.Add(t);
                return t;
            }

            public bool IsChunkRoot(Transform transform)
            {
                return transform != null && CreatedRoots.Contains(transform);
            }
        }
    }
}
