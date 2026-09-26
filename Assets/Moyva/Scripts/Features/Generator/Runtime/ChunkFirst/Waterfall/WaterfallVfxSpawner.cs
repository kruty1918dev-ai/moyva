using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Places the Stylized Water 3 waterfall VFX prefabs (lip foam, impact
    /// splashes, mist) at the fronts detected by
    /// <see cref="WaterfallChunkMeshService"/>. Instances live under a
    /// per-chunk "Waterfalls" root so chunk culling applies and a rebuild
    /// clears them with the chunk. Prefabs are scaled moderately: the edge
    /// emitter widens with the front, splash/mist get a uniform factor, and
    /// per-system particle caps stay inside the mobile budget.
    /// </summary>
    internal sealed class WaterfallVfxSpawner
    {
        private const string RootName = "Waterfalls";
        /// <summary>SW3 edge-foam emitters are authored 8 m wide.</summary>
        private const float EdgeEmitterWidthMeters = 8f;

        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly Dictionary<MapChunkCoord, Transform> _spawnRoots
            = new Dictionary<MapChunkCoord, Transform>();

        public WaterfallVfxSpawner(
            [InjectOptional] IMapChunkLayoutService layout = null,
            [InjectOptional] IMapVisualChunkRootService roots = null)
        {
            _layout = layout;
            _roots = roots;
        }

        /// <summary>
        /// Rebuilds waterfall VFX for the current front field. Called once
        /// per world build after chunk meshes exist; clears its own roots
        /// first so repeated generation cannot accumulate instances.
        /// </summary>
        public int Spawn(WaterfallChunkMeshService falls)
        {
            Clear();
            if (falls == null || !falls.IsActive || !falls.HasField
                || _layout == null || _roots == null)
            {
                return 0;
            }

            var config = falls.Config;
            int budget = Mathf.Max(0, config.MaxVfxPerMap);
            if (budget == 0
                || (config.EdgeFoamPrefab == null
                    && config.ImpactSplashPrefab == null
                    && config.MistPrefab == null))
            {
                return 0;
            }

            float cs = Mathf.Max(0.0001f, _layout.CellSize);
            float mistDrop = falls.MinDropMeters
                * Mathf.Max(1, config.MistMinDropLevels)
                / Mathf.Max(1, config.MinDropLevels);
            float vfxScale = Mathf.Clamp(config.VfxScale, 0.1f, 4f);
            int maxParticles = Mathf.Max(1, config.MaxParticlesPerVfx);

            // Largest fronts get the budget first.
            var fronts = new List<WaterfallFieldPlanner.Front>(falls.Fronts);
            fronts.Sort((a, b) => (b.Drop * b.WidthCells).CompareTo(a.Drop * a.WidthCells));

            int spawned = 0;
            for (int i = 0; i < fronts.Count && spawned < budget; i++)
            {
                var front = fronts[i];
                if (!_layout.TryGetChunkCoord(front.Anchor, out var coord))
                    continue;
                Transform root = GetSpawnRoot(coord);
                if (root == null)
                    continue;

                var dir = new Vector3(front.Dir.x, 0f, front.Dir.y).normalized;
                var rotation = Quaternion.LookRotation(dir, Vector3.up);
                Vector3 center = front.Center * cs;
                var lipPos = new Vector3(center.x, front.TopY - 0.05f, center.z);
                var basePos = new Vector3(
                    center.x + dir.x * 0.15f * cs,
                    front.BottomY + 0.05f,
                    center.z + dir.z * 0.15f * cs);
                string id = $"{front.Anchor.x}_{front.Anchor.y}_{front.Dir.x}_{front.Dir.y}";

                spawned += SpawnOne(
                    config.EdgeFoamPrefab, root, $"wfall_edge_{id}", lipPos, rotation,
                    new Vector3(
                        Mathf.Clamp(front.WidthCells * cs / EdgeEmitterWidthMeters, 0.1f, 2f) * vfxScale,
                        vfxScale, vfxScale),
                    maxParticles);
                spawned += SpawnOne(
                    config.ImpactSplashPrefab, root, $"wfall_splash_{id}", basePos, rotation,
                    Vector3.one * vfxScale
                        * Mathf.Clamp(front.WidthCells * 0.6f, 0.6f, 2.5f),
                    maxParticles);
                if (front.Drop >= mistDrop)
                {
                    spawned += SpawnOne(
                        config.MistPrefab, root, $"wfall_mist_{id}", basePos, rotation,
                        Vector3.one * vfxScale,
                        maxParticles);
                }
            }
            return spawned;
        }

        public void Clear()
        {
            foreach (var pair in _spawnRoots)
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

        private Transform GetSpawnRoot(MapChunkCoord coord)
        {
            if (_spawnRoots.TryGetValue(coord, out var existing) && existing != null)
                return existing;
            Transform chunkRoot = _roots.GetOrCreateRoot(coord);
            Transform root = chunkRoot.Find(RootName);
            if (root == null)
            {
                var go = new GameObject(RootName);
                root = go.transform;
                root.SetParent(chunkRoot, false);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;
            }
            _spawnRoots[coord] = root;
            return root;
        }

        private static int SpawnOne(
            GameObject prefab,
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            int maxParticles)
        {
            if (prefab == null)
                return 0;
            var instance = Object.Instantiate(prefab, parent, false);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = scale;
            var systems = instance.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                var main = systems[i].main;
                if (main.maxParticles > maxParticles)
                    main.maxParticles = maxParticles;
            }
            return 1;
        }
    }
}
