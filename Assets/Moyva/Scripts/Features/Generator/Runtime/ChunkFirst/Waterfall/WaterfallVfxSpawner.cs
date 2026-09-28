using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Places the Stylized Water 3 waterfall prefab (or legacy lip foam,
    /// impact splashes and mist) at the fronts detected by
    /// <see cref="WaterfallChunkMeshService"/>. Instances live under a
    /// per-chunk "Waterfalls" root so chunk culling applies and a rebuild
    /// clears them with the chunk. Prefabs are scaled moderately: the edge
    /// emitter widens with the front, splash/mist get a uniform factor, and
    /// per-system particle caps stay inside the mobile budget.
    /// </summary>
    internal sealed class WaterfallVfxSpawner
    {
        private const string RootName = "Waterfalls";

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
            if (config.Prefab == null && (budget == 0
                || (config.EdgeFoamPrefab == null
                    && config.ImpactSplashPrefab == null
                    && config.MistPrefab == null)))
            {
                return 0;
            }

            float cs = Mathf.Max(0.0001f, _layout.CellSize);
            float mistDrop = falls.MinDropMeters
                * Mathf.Max(1, config.MistMinDropLevels)
                / Mathf.Max(1, config.MinDropLevels);
            float vfxScale = Mathf.Clamp(config.VfxScale, 0.1f, 4f);
            int maxParticles = Mathf.Max(1, config.MaxParticlesPerVfx);

            // Largest fronts receive particles first; every front keeps its
            // waterfall mesh when the particle budget is exhausted.
            var fronts = new List<WaterfallFieldPlanner.Front>(falls.Fronts);
            fronts.Sort((a, b) => (b.Drop * b.WidthCells).CompareTo(a.Drop * a.WidthCells));

            int spawned = 0;
            for (int i = 0; i < fronts.Count; i++)
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
                float frontWidth = front.WidthCells * cs;

                if (config.Prefab != null)
                {
                    SpawnWaterfallPrefab(config, root, id, center, rotation,
                        frontWidth, front.Drop, front.TopY, maxParticles,
                        spawned < budget);
                    spawned++;
                    continue;
                }

                if (spawned < budget)
                    spawned += SpawnOne(
                        config.EdgeFoamPrefab, root, $"wfall_edge_{id}", lipPos, rotation,
                        Vector3.one * vfxScale, frontWidth, maxParticles);
                if (spawned < budget)
                    spawned += SpawnOne(
                        config.ImpactSplashPrefab, root, $"wfall_splash_{id}", basePos, rotation,
                        Vector3.one * vfxScale, frontWidth, maxParticles);
                if (front.Drop >= mistDrop && spawned < budget)
                {
                    spawned += SpawnOne(
                        config.MistPrefab, root, $"wfall_mist_{id}", basePos, rotation,
                        Vector3.one * vfxScale,
                        Mathf.Max(2f, frontWidth), maxParticles);
                }
            }
            return spawned;
        }

        private static void SpawnWaterfallPrefab(
            RecipeWaterfallConfig config,
            Transform parent,
            string id,
            Vector3 center,
            Quaternion rotation,
            float width,
            float drop,
            float topY,
            int maxParticles,
            bool emitParticles)
        {
            MeshFilter sourceMesh = config.Prefab.GetComponent<MeshFilter>();
            if (sourceMesh == null || sourceMesh.sharedMesh == null)
                return;

            Bounds bounds = sourceMesh.sharedMesh.bounds;
            if (bounds.size.x <= 0.0001f || bounds.size.y <= 0.0001f)
                return;

            var instance = Object.Instantiate(config.Prefab, parent, false);
            instance.name = $"wfall_{id}";
            float sx = width * Mathf.Max(0.1f, config.WidthScale) / bounds.size.x;
            float sy = drop * Mathf.Max(0.1f, config.HeightScale) / bounds.size.y;
            // Keep the prefab's authored foam motion and scale the full
            // waterfall, including its lip and impact emitters, as one unit.
            // Authored splashes sit several units forward of the lip. Their
            // depth must follow the drop scale, while X follows front width.
            instance.transform.localScale = new Vector3(sx, sy, sy);
            instance.transform.localRotation = rotation;
            // Anchor the mesh's actual lip, not its whole curved volume's
            // centre, which is forward of the upper sheet on SW3 prefabs.
            Vector3 localTop = new Vector3(bounds.center.x, bounds.max.y, bounds.min.z);
            instance.transform.localPosition =
                new Vector3(center.x, topY + Mathf.Min(0.01f, drop * 0.05f), center.z)
                - rotation * Vector3.Scale(localTop, instance.transform.localScale);

            var waterfallRenderer = instance.GetComponent<MeshRenderer>();
            if (waterfallRenderer != null)
            {
                if (config.CurtainMaterial != null)
                    waterfallRenderer.sharedMaterial = config.CurtainMaterial;
                waterfallRenderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            foreach (var system in instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = system.main;
                if (main.maxParticles > maxParticles)
                    main.maxParticles = maxParticles;
                if (!emitParticles)
                    system.gameObject.SetActive(false);
            }
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

        /// <summary>
        /// Keeps the prefab transform uniformly scaled and writes the real
        /// front width into the emitter shape instead: the SW3 waterfall
        /// emitters are flat (shape.scale.x spans the pour), so transform
        /// X-scaling would squash their particle pattern into a blob while
        /// a uniform scale keeps the authored look. shape.scale is in the
        /// system's local space, so it compensates the uniform transform
        /// scale to land exactly on the requested world width.
        /// </summary>
        private static int SpawnOne(
            GameObject prefab,
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float shapeWidth,
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
                if (shapeWidth > 0f)
                {
                    var shape = systems[i].shape;
                    Vector3 s = shape.scale;
                    s.x = shapeWidth / Mathf.Max(0.0001f, scale.x);
                    shape.scale = s;
                }
            }
            return 1;
        }
    }
}
