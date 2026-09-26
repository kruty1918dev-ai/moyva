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
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.On, r.shadowCastingMode,
                    "Child renderers keep the prefab's authored shadow mode");
            var colliders = spawnedRoot.GetComponentsInChildren<Collider>(true);
            Assert.AreEqual(0, colliders.Length,
                "Colliders on child objects must be removed for visual-only decorations");
        }

        [Test]
        public void Spawn_UsesTerrainSurfaceHeight_WhenAvailable()
        {
            // Arrange: terrain surface at y=3.2 for the placement cell
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-surface", prefab);
            var terrain = new FakeTerrainLevelService(3.2f);
            var spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots, terrain);
            var placement = new DecorationPlacement("deco-surface",
                new Vector3(2f, 0f, 3f), Quaternion.identity, Vector3.one, 2, 3);

            // Act
            int spawned = spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            Assert.AreEqual(1, spawned);
            var root = FindSpawnedDecorationRoot();
            Assert.IsNotNull(root);
            var child = root.GetChild(0);
            Assert.AreEqual(3.2f, child.localPosition.y, 0.001f,
                "Decoration Y must come from the terrain surface height map");
            Assert.AreEqual(2f, child.localPosition.x, 0.001f);
            Assert.AreEqual(3f, child.localPosition.z, 0.001f);
        }

        [Test]
        public void Spawn_MultipliesCellCoordinatesByCellSize()
        {
            // Arrange: a 2-unit cell grid; placement coords are in cell units.
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-scale", prefab);
            _layout.CellSizeValue = 2f;
            var spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots);
            var placement = new DecorationPlacement("deco-scale",
                new Vector3(2f, 0.5f, 3f), Quaternion.identity, Vector3.one, 2, 3);

            // Act
            spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            var root = FindSpawnedDecorationRoot();
            Assert.IsNotNull(root);
            var child = root.GetChild(0);
            Assert.AreEqual(4f, child.localPosition.x, 0.001f,
                "X must be scaled by the map cell size");
            Assert.AreEqual(6f, child.localPosition.z, 0.001f,
                "Z must be scaled by the map cell size");
        }

        [Test]
        public void Spawn_AlignsRotationToSurfaceNormal()
        {
            // Arrange: a constant 45-degree slope normal.
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-slope", prefab);
            var terrain = new FakeTerrainLevelService(1f)
            {
                HasNormal = true,
                Normal = new Vector3(1f, 1f, 0f).normalized
            };
            var spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots, terrain);
            var placement = new DecorationPlacement("deco-slope",
                new Vector3(2f, 0f, 3f), Quaternion.identity, Vector3.one, 2, 3);

            // Act
            spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            var child = FindSpawnedDecorationRoot().GetChild(0);
            var expected = Quaternion.FromToRotation(Vector3.up, terrain.Normal);
            Assert.Less(Quaternion.Angle(expected, child.localRotation), 0.01f,
                "Decorations must tilt onto the terrain surface normal");
        }

        [Test]
        public void Spawn_KeepsAuthoredRotation_WhenAlignToSurfaceDisabled()
        {
            // Arrange
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-flat", prefab);
            var terrain = new FakeTerrainLevelService(1f)
            {
                HasNormal = true,
                Normal = new Vector3(1f, 1f, 0f).normalized
            };
            var config = new EnvironmentDecorationConfig();
            config.VisualVariation.AlignToSurface = false;
            var spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots, terrain, config);
            var placement = new DecorationPlacement("deco-flat",
                new Vector3(2f, 0f, 3f), Quaternion.Euler(0f, 45f, 0f), Vector3.one, 2, 3);

            // Act
            spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            var child = FindSpawnedDecorationRoot().GetChild(0);
            Assert.Less(Quaternion.Angle(Quaternion.Euler(0f, 45f, 0f), child.localRotation), 0.01f,
                "With AlignToSurface off, the authored yaw must be preserved");
        }

        [Test]
        public void Spawn_AppliesYOffsetAboveSurface()
        {
            // Arrange: water flora rides a fixed offset above the water sheet.
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-lily", prefab);
            var terrain = new FakeTerrainLevelService(0.4f);
            var spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots, terrain);
            var placement = new DecorationPlacement("deco-lily",
                new Vector3(1f, 0f, 1f), Quaternion.identity, Vector3.one, 1, 1, 0.03f);

            // Act
            spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            var child = FindSpawnedDecorationRoot().GetChild(0);
            Assert.AreEqual(0.43f, child.localPosition.y, 0.001f,
                "YOffset must ride on top of the resolved surface height");
        }

        [Test]
        public void Spawn_RegistersRenderersWithChunkVisibilityRegistry()
        {
            // Arrange: chunk visibility service disables renderers when their
            // chunk leaves the camera set or is fully fog-hidden; decoration
            // renderers must be registered against their owning chunk or they
            // would keep floating over hidden terrain.
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-reg", prefab);
            var visibility = new FakeChunkVisibilityRegistry();
            var spawner = new EnvironmentDecorationSpawner(
                _registry, _layout, _roots, chunkRegistry: visibility);
            var placement = new DecorationPlacement("deco-reg",
                new Vector3(2f, 0f, 3f), Quaternion.identity, Vector3.one, 9, 3);

            // Act
            int spawned = spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            Assert.AreEqual(1, spawned);
            var child = FindSpawnedDecorationRoot().GetChild(0);
            var renderer = child.GetComponentInChildren<Renderer>();
            Assert.IsTrue(visibility.TryGetChunks(renderer, out var chunks),
                "Spawned decoration renderer must be registered for chunk visibility");
            var expected = new MapChunkCoord(1, 0); // tile 9,3 with ChunkSize=8
            CollectionAssert.AreEquivalent(new[] { expected }, chunks);
        }

        [Test]
        public void Spawn_FallsBackToPlacementHeight_WhenNoSurfaceMap()
        {
            // Arrange: terrain service present but no surface data for the cell
            var prefab = CreatePrefabWithChildVisuals();
            _registry.SetDefinition("deco-fallback", prefab);
            var terrain = new FakeTerrainLevelService(0f) { HasSurface = false };
            var spawner = new EnvironmentDecorationSpawner(_registry, _layout, _roots, terrain);
            var placement = new DecorationPlacement("deco-fallback",
                new Vector3(2f, 7.5f, 3f), Quaternion.identity, Vector3.one, 2, 3);

            // Act
            spawner.Spawn(new DecorationPlacementResult(new[] { placement }));

            // Assert
            var root = FindSpawnedDecorationRoot();
            Assert.IsNotNull(root);
            Assert.AreEqual(7.5f, root.GetChild(0).localPosition.y, 0.001f,
                "Without a surface map the generator's placement Y must be kept");
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
            public float CellSizeValue = 1f;
            public float CellSize => CellSizeValue;
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

            public bool TryGetOwnedChunk(Transform transform, out MapChunkCoord coord)
            {
                coord = default;
                for (Transform current = transform; current != null; current = current.parent)
                {
                    foreach (var pair in _roots)
                    {
                        if (pair.Value == current)
                        {
                            coord = pair.Key;
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private sealed class FakeChunkVisibilityRegistry : IMapVisualChunkRegistry
        {
            private readonly Dictionary<Renderer, List<MapChunkCoord>> _map = new();

            public int CameraVisibilityVersion => 0;
            public void Clear() => _map.Clear();
            public void ResetVisibilityState() { }

            public void Register(Renderer renderer, IReadOnlyList<MapChunkCoord> chunks)
            {
                _map[renderer] = new List<MapChunkCoord>(chunks);
            }

            public void SetCameraVisible(IReadOnlyCollection<MapChunkCoord> visibleChunks) { }
            public bool IsCameraVisible(MapChunkCoord coord) => true;
            public void SetFogFullyHidden(MapChunkCoord coord, bool hidden) { }
            public void ApplyVisibility() { }

            public bool TryGetChunks(Renderer renderer, out List<MapChunkCoord> chunks)
                => _map.TryGetValue(renderer, out chunks);
        }

        private sealed class FakeTerrainLevelService : IGeneratorTerrainLevelService
        {
            private readonly float _surfaceY;

            public FakeTerrainLevelService(float surfaceY)
            {
                _surfaceY = surfaceY;
            }

            public bool HasSurface = true;
            public bool HasNormal;
            public Vector3 Normal = Vector3.up;
            public bool HasLevelMap => true;
            public bool HasSurfaceHeightMap => HasSurface;
            public bool HasExplicitSurfaceHeightMap => HasSurface;
            public int Width => 64;
            public int Height => 64;
            public HillLevelDataMap CurrentHillLevelData => null;

            public void Clear() { }
            public void SetLevelMap(int[,] levelMap) { }
            public void SetSurfaceHeightMap(float[,] surfaceHeightMap) { }
            public void SetHillLevelData(HillLevelDataMap data) { }
            public bool TryGetLevel(Vector2Int position, out int level) { level = 0; return true; }

            public bool TryGetSurfaceHeight(Vector2Int position, out float surfaceY)
            {
                surfaceY = _surfaceY;
                return HasSurface;
            }

            public bool TryGetSurfaceNormal(Vector2Int position, float cellSize, out Vector3 normal)
            {
                normal = Normal;
                return HasNormal;
            }

            public int GetLevelOrDefault(Vector2Int position, int fallback = 0) => fallback;
            public int[,] CopyLevelMap() => null;
            public float[,] CopySurfaceHeightMap() => null;
        }
    }
}
