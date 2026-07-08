using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstObjectSpawner : IChunkFirstObjectSpawner
    {
        private const string ObjectsRootName = "Objects";
        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly Dictionary<MapChunkCoord, Transform> _objectRoots = new Dictionary<MapChunkCoord, Transform>();

        public ChunkFirstObjectSpawner(
            ITileWorldCreatorBuildEnvironment environment,
            IMapChunkLayoutService layout,
            IMapVisualChunkRootService roots)
        {
            _environment = environment;
            _layout = layout;
            _roots = roots;
        }

        public int Spawn(GeneratedWorldData worldData)
        {
            Clear();
            if (worldData?.LogicalTileMap != null)
                return SpawnStacks(worldData.LogicalTileMap, worldData.Seed);

            var mapping = _environment?.Mapping;
            TryResolveLayer objectResolver = null;
            TryResolveLayer buildingResolver = null;
            if (mapping != null)
            {
                objectResolver = mapping.TryResolveObjectLayer;
                buildingResolver = mapping.TryResolveBuildingLayer;
            }

            int count = 0;
            count += SpawnMap(worldData?.ObjectMap, objectResolver, worldData?.Seed ?? 1);
            count += SpawnMap(worldData?.BuildingMap, buildingResolver, worldData?.Seed ?? 1);
            return count;
        }

        public void Clear()
        {
            foreach (var pair in _objectRoots)
            {
                if (pair.Value == null)
                    continue;

                for (int i = pair.Value.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                        Object.Destroy(pair.Value.GetChild(i).gameObject);
                    else
                        Object.DestroyImmediate(pair.Value.GetChild(i).gameObject);
                }
            }
        }

        private int SpawnStacks(GraphLogicalTileMap map, int seed)
        {
            int spawned = 0;
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var stack = map.GetCellStack(x, y);
                if (stack == null || stack.IsEmpty)
                    continue;

                int candidateIndex = 0;
                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    var sample = stack.Samples[i];
                    if (!IsObjectLike(sample.LayerKind))
                        continue;

                    if (TrySpawnSample(sample, new Vector2Int(x, y), seed, candidateIndex))
                        spawned++;
                    candidateIndex++;
                }
            }

            return spawned;
        }

        private bool TrySpawnSample(GraphTileLayerSample sample, Vector2Int cell, int seed, int candidateIndex)
        {
            if (!TryResolvePrefab(sample, out string resolvedId, out var mapping))
                return false;
            if (!_layout.TryGetChunkCoord(cell, out var coord))
                return false;

            Transform root = GetObjectRoot(coord);
            uint hash = ChunkFirstStableHash.ObjectVariant(
                seed,
                cell,
                !string.IsNullOrWhiteSpace(sample.GraphLayerId) ? sample.GraphLayerId : resolvedId,
                candidateIndex,
                mapping.RegistryVisualPrefab.name);
            var instance = Object.Instantiate(mapping.RegistryVisualPrefab, root, false);
            instance.name = $"{mapping.RegistryVisualPrefab.name}_{hash:x8}";
            instance.transform.localPosition = new Vector3(
                cell.x * _layout.CellSize,
                sample.SurfaceHeight,
                cell.y * _layout.CellSize);
            return true;
        }

        private int SpawnMap(
            string[,] map,
            TryResolveLayer resolveLayer,
            int seed)
        {
            if (map == null || resolveLayer == null)
                return 0;

            int spawned = 0;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                string id = map[x, y];
                if (string.IsNullOrWhiteSpace(id) || !resolveLayer(id, out var mapping) || mapping.RegistryVisualPrefab == null)
                    continue;

                var cell = new Vector2Int(x, y);
                if (!_layout.TryGetChunkCoord(cell, out var coord))
                    continue;

                Transform root = GetObjectRoot(coord);
                uint hash = ChunkFirstStableHash.ObjectVariant(seed, cell, id, 0, mapping.RegistryVisualPrefab.name);
                var instance = Object.Instantiate(mapping.RegistryVisualPrefab, root, false);
                instance.name = $"{mapping.RegistryVisualPrefab.name}_{hash:x8}";
                instance.transform.localPosition = new Vector3(x * _layout.CellSize, 0f, y * _layout.CellSize);
                spawned++;
            }

            return spawned;
        }

        private Transform GetObjectRoot(MapChunkCoord coord)
        {
            if (_objectRoots.TryGetValue(coord, out var existing) && existing != null)
                return existing;

            Transform chunkRoot = _roots.GetOrCreateRoot(coord);
            Transform root = chunkRoot.Find(ObjectsRootName);
            if (root == null)
            {
                var gameObject = new GameObject(ObjectsRootName);
                root = gameObject.transform;
                root.SetParent(chunkRoot, false);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;
            }

            _objectRoots[coord] = root;
            return root;
        }

        private bool TryResolvePrefab(
            GraphTileLayerSample sample,
            out string resolvedId,
            out TileWorldCreatorIdMappingSO.LayerMapping mapping)
        {
            resolvedId = null;
            mapping = null;

            TryResolveLayer resolver = null;
            var mappingSource = _environment?.Mapping;
            if (mappingSource != null)
            {
                resolver = sample.LayerKind == LayerKind.Building
                    ? mappingSource.TryResolveBuildingLayer
                    : mappingSource.TryResolveObjectLayer;
            }

            if (resolver == null)
                return false;

            if (TryResolveId(resolver, sample.TileId, out resolvedId, out mapping)
                || TryResolveId(resolver, sample.PresetId, out resolvedId, out mapping)
                || TryResolveId(resolver, sample.GraphLayerId, out resolvedId, out mapping))
            {
                return mapping?.RegistryVisualPrefab != null;
            }

            return false;
        }

        private static bool TryResolveId(
            TryResolveLayer resolver,
            string id,
            out string resolvedId,
            out TileWorldCreatorIdMappingSO.LayerMapping mapping)
        {
            resolvedId = null;
            mapping = null;
            if (string.IsNullOrWhiteSpace(id) || !resolver(id, out mapping))
                return false;

            resolvedId = id;
            return true;
        }

        private static bool IsObjectLike(LayerKind kind)
            => kind == LayerKind.ObjectSpawn
               || kind == LayerKind.Building
               || kind == LayerKind.Decoration;

        private delegate bool TryResolveLayer(string id, out TileWorldCreatorIdMappingSO.LayerMapping mapping);
    }
}
