using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
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
        private readonly Dictionary<MapChunkCoord, Transform> _decorationRoots = new Dictionary<MapChunkCoord, Transform>();

        public EnvironmentDecorationSpawner(
            IMapObjectRegistryService objectRegistry,
            IMapChunkLayoutService layout,
            IMapVisualChunkRootService roots)
        {
            _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _roots = roots ?? throw new ArgumentNullException(nameof(roots));
        }

        /// <summary>
        /// Spawn decorations based on placement data.
        /// </summary>
        public int Spawn(DecorationPlacementResult placementResult)
        {
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

        private bool TrySpawnDecoration(DecorationPlacement placement)
        {
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
            instance.transform.localPosition = placement.Position;
            instance.transform.localRotation = placement.Rotation;
            instance.transform.localScale = placement.Scale;

            // Ensure decorations don't cast shadows or have colliders (purely visual)
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
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
