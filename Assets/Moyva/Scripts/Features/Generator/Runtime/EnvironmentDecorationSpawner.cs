using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Shared.Graphics;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Spawns environment decoration GameObjects based on placement data.
    /// </summary>
    internal sealed class EnvironmentDecorationSpawner
    {
        private const string DecorationRootName = "EnvironmentDecorations";
        private readonly IMapObjectRegistryService _objectRegistry;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly IGeneratorTerrainLevelService _terrainLevels;
        private readonly IMapVisualChunkRegistry _chunkRegistry;
        private readonly EnvironmentObjectPlacementResolver _placementResolver;
        private readonly IGraphicsSettingsService _graphicsSettings;
        private readonly bool _alignToSurface;
        private readonly float _footprintShrink;
        private readonly Dictionary<MapChunkCoord, Transform> _decorationRoots = new Dictionary<MapChunkCoord, Transform>();
        private readonly List<MapChunkCoord> _singleChunkBuffer = new List<MapChunkCoord>(1);
        private readonly HashSet<Vector2Int> _clearedDecorationCells = new HashSet<Vector2Int>();

        public EnvironmentDecorationSpawner(
            IMapObjectRegistryService objectRegistry,
            IMapChunkLayoutService layout,
            IMapVisualChunkRootService roots,
            [Zenject.InjectOptional] IGeneratorTerrainLevelService terrainLevels = null,
            [Zenject.InjectOptional] EnvironmentDecorationConfig config = null,
            [Zenject.InjectOptional] EnvironmentObjectPlacementResolver placementResolver = null,
            [Zenject.InjectOptional] IMapVisualChunkRegistry chunkRegistry = null,
            [Zenject.InjectOptional] IGraphicsSettingsService graphicsSettings = null)
        {
            _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _roots = roots ?? throw new ArgumentNullException(nameof(roots));
            _terrainLevels = terrainLevels;
            _placementResolver = placementResolver;
            _chunkRegistry = chunkRegistry;
            _graphicsSettings = graphicsSettings;
            _alignToSurface = config?.VisualVariation?.AlignToSurface ?? true;
            _footprintShrink = config?.Footprint?.FootprintShrink ?? 0.9f;
        }

        /// <summary>
        /// Spawn decorations based on placement data.
        /// </summary>
        public int Spawn(DecorationPlacementResult placementResult)
        {
            // A fresh decoration pass resets footprint-cleared cells; cleared
            // cells are re-recorded by the placement signals that follow.
            _clearedDecorationCells.Clear();
            Clear();
            if (placementResult == null || placementResult.Count == 0)
                return 0;

            int spawned = 0;

            foreach (var placement in placementResult.Placements)
            {
                if (TrySpawnDecoration(placement))
                    spawned++;
            }

            return spawned;
        }

        /// <summary>
        /// Clear all spawned decorations.
        /// </summary>
        public void Clear()
        {
            foreach (var pair in _decorationRoots)
            {
                if (pair.Value == null)
                    continue;

                for (int i = pair.Value.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(pair.Value.GetChild(i).gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(pair.Value.GetChild(i).gameObject);
                }
            }

            _decorationRoots.Clear();
        }

        /// <summary>
        /// Destroys decorations anchored to the given cells and excludes them
        /// from future spawn passes until the next world decoration build.
        /// </summary>
        public int ClearDecorationsInCells(IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
                return 0;

            var cleared = new HashSet<Vector2Int>(cells);
            _clearedDecorationCells.UnionWith(cleared);
            int removed = 0;
            foreach (var pair in _decorationRoots)
            {
                Transform root = pair.Value;
                if (root == null)
                    continue;

                for (int i = root.childCount - 1; i >= 0; i--)
                {
                    Transform child = root.GetChild(i);
                    if (!TryParseAnchorCell(child.name, out Vector2Int cell)
                        || !cleared.Contains(cell))
                    {
                        continue;
                    }

                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(child.gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                    removed++;
                }
            }

            return removed;
        }

        /// <summary>Parses the trailing _{x}_{y} anchor suffix written by
        /// TrySpawnDecoration; the asset id itself may contain underscores.</summary>
        private static bool TryParseAnchorCell(string name, out Vector2Int cell)
        {
            cell = default;
            if (string.IsNullOrEmpty(name))
                return false;

            int ySep = name.LastIndexOf('_');
            if (ySep <= 0 || ySep >= name.Length - 1)
                return false;

            int xSep = name.LastIndexOf('_', ySep - 1);
            if (xSep <= 0)
                return false;

            if (!int.TryParse(name.Substring(xSep + 1, ySep - xSep - 1), out int x)
                || !int.TryParse(name.Substring(ySep + 1), out int y))
            {
                return false;
            }

            cell = new Vector2Int(x, y);
            return true;
        }

        private bool TrySpawnDecoration(DecorationPlacement placement)
        {
            // Performance profile thins light ground cover only — trees,
            // bushes, rocks and stumps keep the biome silhouette. The skip is
            // a pure function of the placement hash, so quality switching
            // never changes which heavy props appear.
            if (_graphicsSettings != null
                && _graphicsSettings.Settings.Profile == GraphicsQualityProfile.Performance
                && IsLightweightDecor(placement.Type)
                && (placement.Position.GetHashCode() & 1) == 1)
            {
                return false;
            }

            if (_clearedDecorationCells.Contains(
                    new Vector2Int(placement.TileX, placement.TileY)))
            {
                return false;
            }

            if (!_objectRegistry.TryGetDefinition(placement.AssetId, out var definition))
                return false;

            if (definition.VisualPrefab == null)
                return false;

            var cell = new Vector2Int(placement.TileX, placement.TileY);
            if (!_layout.TryGetChunkCoord(cell, out var coord))
                return false;

            Transform root = GetDecorationRoot(coord);
            var instance = UnityEngine.Object.Instantiate(definition.VisualPrefab, root, false);
            instance.name = $"{definition.Id}_{placement.TileX}_{placement.TileY}";

            // Placement positions are authored in cell units; convert to world
            // units so offsets stay proportional at any cell size.
            float cellSize = Mathf.Max(0.0001f, _layout.CellSize);
            var localPosition = new Vector3(
                placement.Position.x * cellSize,
                placement.Position.y,
                placement.Position.z * cellSize);
            Quaternion localRotation = placement.Rotation;
            if (_terrainLevels != null
                && _terrainLevels.TryGetSurfaceHeight(cell, out float surfaceY))
            {
                // Only low ground-cover tilts with the surface — trunks,
                // boulders and logs stay upright so trees never lie
                // horizontally on slopes.
                if (_alignToSurface
                    && AlignsToSurface(placement.Type)
                    && _terrainLevels.TryGetSurfaceNormal(cell, cellSize, out Vector3 normal)
                    && normal.y < 0.9999f)
                {
                    localRotation = Quaternion.FromToRotation(Vector3.up, normal) * localRotation;
                }

                // Ground the prefab's lowest transformed point on the lowest
                // terrain under its footprint, not the pivot on the anchor
                // cell — this keeps rocks from floating over slopes.
                localPosition.y = (_placementResolver != null
                    ? _placementResolver.ResolveGroundedY(
                        definition.VisualPrefab,
                        new Vector3(localPosition.x, 0f, localPosition.z),
                        localRotation,
                        placement.Scale,
                        cellSize,
                        SurfaceHeightOrNaN,
                        surfaceY,
                        _footprintShrink)
                    : surfaceY) + placement.YOffset;
            }
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = placement.Scale;

            // Decorations keep their prefab-authored shadow casting; only ensure they receive shadows.
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.receiveShadows = true;
                if (_chunkRegistry != null)
                {
                    _singleChunkBuffer.Clear();
                    _singleChunkBuffer.Add(coord);
                    _chunkRegistry.Register(renderer, _singleChunkBuffer);
                }
            }

            var colliders = instance.GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(collider);
                else
                    UnityEngine.Object.DestroyImmediate(collider);
            }

            return true;
        }

        /// <summary>Structural props stay upright on slopes; ground cover
        /// and unclassified types tilt with the surface.</summary>
        private static bool AlignsToSurface(string type)
        {
            switch (type)
            {
                case "tree":
                case "stump":
                case "rock":
                case "bush":
                case "log":
                case "sapling":
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>Small cover props are the first thing the low-quality
        /// profile sheds; structural props always spawn.</summary>
        private static bool IsLightweightDecor(string type)
        {
            switch (type)
            {
                case "grass":
                case "tallgrass":
                case "fern":
                case "flower":
                case "litter":
                case "pebble":
                case "reed":
                    return true;
                default:
                    return false;
            }
        }

        private float SurfaceHeightOrNaN(Vector2Int cell)
            => _terrainLevels.TryGetSurfaceHeight(cell, out float h) ? h : float.NaN;

        private Transform GetDecorationRoot(MapChunkCoord coord)
        {
            if (_decorationRoots.TryGetValue(coord, out var existing) && existing != null)
                return existing;

            Transform chunkRoot = _roots.GetOrCreateRoot(coord);
            Transform root = chunkRoot.Find(DecorationRootName);
            if (root == null)
            {
                var gameObject = new GameObject(DecorationRootName);
                root = gameObject.transform;
                root.SetParent(chunkRoot, false);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;
            }

            _decorationRoots[coord] = root;
            return root;
        }
    }
}
