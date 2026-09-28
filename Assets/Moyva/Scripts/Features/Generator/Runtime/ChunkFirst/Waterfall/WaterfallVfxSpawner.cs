using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Places the Stylized Water 3 particle prefabs (lip foam, impact
    /// splashes, mist) on the fronts detected by
    /// <see cref="WaterfallChunkMeshService"/>, matching each instance to
    /// that front's generated ribbon: the edge emitter sits on the tucked
    /// lip, splashes and mist sit on the ribbon's impact curl above the
    /// lower sheet, and start speeds scale with sqrt(drop) like the
    /// curtain's scroll. Instances live under a per-chunk "Waterfalls"
    /// root so chunk culling applies; every renderer registers with the
    /// visual chunk registry across all chunks the front touches.
    /// </summary>
    internal sealed class WaterfallVfxSpawner
    {
        private const string RootName = "Waterfalls";

        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly IMapVisualChunkRegistry _registry;
        private readonly List<MapChunkCoord> _frontChunks = new();
        private readonly Dictionary<MapChunkCoord, Transform> _spawnRoots
            = new Dictionary<MapChunkCoord, Transform>();

        public WaterfallVfxSpawner(
            [InjectOptional] IMapChunkLayoutService layout = null,
            [InjectOptional] IMapVisualChunkRootService roots = null,
            [InjectOptional] IMapVisualChunkRegistry registry = null)
        {
            _layout = layout;
            _roots = roots;
            _registry = registry;
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
            float speedReference = Mathf.Max(0.01f, config.SpeedReferenceDropMeters);

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
                float bow = WaterfallChunkMeshService.BowFactor(front.Drop);

                // Lip foam rides the ribbon's crest nose; splashes and mist
                // sit on the impact curl where the tail dives under the
                // lower sheet — both offsets mirror the ribbon profile.
                var lipPos = new Vector3(
                    center.x + dir.x * 0.05f * cs,
                    front.TopY - 0.06f,
                    center.z + dir.z * 0.05f * cs);
                var basePos = new Vector3(
                    center.x + dir.x * 0.22f * cs * bow,
                    front.BottomY + 0.03f,
                    center.z + dir.z * 0.22f * cs * bow);
                string id = $"{front.Anchor.x}_{front.Anchor.y}_{front.Dir.x}_{front.Dir.y}";
                float frontWidth = front.WidthCells * cs;
                float speedScale = Mathf.Clamp(
                    config.VfxSpeedScale * Mathf.Sqrt(front.Drop / speedReference),
                    0.25f, 6f);

                if (spawned < budget)
                    spawned += SpawnOne(
                        config.EdgeFoamPrefab, root, $"wfall_edge_{id}", lipPos, rotation,
                        Vector3.one * vfxScale, frontWidth, maxParticles,
                        speedScale, front);
                if (spawned < budget)
                    spawned += SpawnOne(
                        config.ImpactSplashPrefab, root, $"wfall_splash_{id}", basePos, rotation,
                        Vector3.one * vfxScale, frontWidth, maxParticles,
                        speedScale, front);
                if (front.Drop >= mistDrop && spawned < budget)
                {
                    spawned += SpawnOne(
                        config.MistPrefab, root, $"wfall_mist_{id}", basePos, rotation,
                        Vector3.one * vfxScale,
                        Mathf.Max(2f, frontWidth), maxParticles,
                        speedScale, front);
                }
            }
            return spawned;
        }

        private void RegisterFront(GameObject instance, WaterfallFieldPlanner.Front front)
        {
            if (_registry == null)
                return;
            _frontChunks.Clear();
            foreach (var edge in front.Edges)
            {
                AddChunk(edge.Cell);
                AddChunk(edge.Lower);
            }
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                _registry.Register(renderer, _frontChunks);
        }

        private void AddChunk(Vector2Int cell)
        {
            if (_layout.TryGetChunkCoord(cell, out var coord) && !_frontChunks.Contains(coord))
                _frontChunks.Add(coord);
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
        private int SpawnOne(
            GameObject prefab,
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float shapeWidth,
            int maxParticles,
            float speedScale,
            WaterfallFieldPlanner.Front front)
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
                // SW3 emitters author in Local scaling: Hierarchy keeps
                // particle size and velocity consistent with VfxScale on
                // drops much smaller than the prefab's 20m source.
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                if (main.maxParticles > maxParticles)
                    main.maxParticles = maxParticles;
                if (Mathf.Abs(speedScale - 1f) > 0.01f)
                    ScaleStartSpeed(systems[i], speedScale);
                if (shapeWidth > 0f)
                {
                    var shape = systems[i].shape;
                    Vector3 s = shape.scale;
                    s.x = shapeWidth / Mathf.Max(0.0001f, scale.x);
                    shape.scale = s;
                }
            }
            RegisterFront(instance, front);
            return 1;
        }

        /*
         * Scales startSpeed across every curve mode so splash and mist
         * launch speed follows the same sqrt(drop) physics as the curtain
         * scroll — taller pours visibly hit harder and faster.
         */
        private static void ScaleStartSpeed(ParticleSystem system, float factor)
        {
            var main = system.main;
            var speed = main.startSpeed;
            if (speed.mode == ParticleSystemCurveMode.Constant
                || speed.mode == ParticleSystemCurveMode.TwoConstants)
            {
                speed.constantMin *= factor;
                speed.constantMax *= factor;
            }
            else
            {
                speed.curveMultiplier *= factor;
            }
            main.startSpeed = speed;
        }
    }
}
