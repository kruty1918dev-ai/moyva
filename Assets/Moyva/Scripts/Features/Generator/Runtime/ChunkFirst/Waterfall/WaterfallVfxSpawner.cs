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
            float mistDrop = Mathf.Max(falls.MinDropMeters, cs * 0.18f)
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
            // Spread impacts across fronts before spending budget on a
            // second or third effect at the same waterfall.
            for (int effect = 0; effect < 3 && spawned < budget; effect++)
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
                    front.TopY + cs * 0.025f,
                    center.z + dir.z * 0.05f * cs);
                var basePos = new Vector3(
                    center.x + dir.x * 0.12f * cs * bow,
                    front.BottomY + cs * 0.04f,
                    center.z + dir.z * 0.12f * cs * bow);
                string id = $"{front.Anchor.x}_{front.Anchor.y}_{front.Dir.x}_{front.Dir.y}";
                float frontWidth = front.WidthCells * cs;
                float speedScale = Mathf.Clamp(
                    config.VfxSpeedScale * Mathf.Sqrt(front.Drop / speedReference),
                    0.25f, 6f);

                if (effect == 1 && spawned < budget)
                    spawned += SpawnOne(
                        config.EdgeFoamPrefab, root, $"wfall_edge_{id}", lipPos, rotation,
                        Vector3.one * vfxScale, frontWidth, maxParticles,
                        speedScale, front, effect);
                if (effect == 0 && spawned < budget)
                    spawned += SpawnOne(
                        config.ImpactSplashPrefab, root, $"wfall_splash_{id}", basePos, rotation,
                        Vector3.one * vfxScale, frontWidth, maxParticles,
                        speedScale, front, effect);
                if (effect == 2 && front.Drop >= mistDrop && spawned < budget)
                {
                    spawned += SpawnOne(
                        config.MistPrefab, root, $"wfall_mist_{id}", basePos, rotation,
                        Vector3.one * vfxScale,
                        frontWidth, maxParticles,
                        speedScale, front, effect);
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
            WaterfallFieldPlanner.Front front, int effect)
        {
            if (prefab == null)
                return 0;
            var instance = Object.Instantiate(prefab, parent, false);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = Vector3.one;
            var systems = instance.GetComponentsInChildren<ParticleSystem>(true);
            // Authored SW3 splashes can be eight metres wide. Fit their
            // largest particle to the cell/drop, not to the merged front
            // width (which only controls the emitter distribution).
            float authoredSize = 0.001f;
            foreach (var system in systems)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = system.main;
                float size = CurvePeak(main.startSize);
                if (main.startSize3D)
                    size = Mathf.Max(CurvePeak(main.startSizeX),
                        Mathf.Max(CurvePeak(main.startSizeY), CurvePeak(main.startSizeZ)));
                var overLife = system.sizeOverLifetime;
                if (overLife.enabled)
                    size *= Mathf.Max(1f, CurvePeak(overLife.size));
                authoredSize = Mathf.Max(authoredSize, size * Mathf.Max(
                    system.transform.lossyScale.x, system.transform.lossyScale.y,
                    system.transform.lossyScale.z));
            }
            float cs = Mathf.Max(0.001f, _layout.CellSize);
            float targetSize = Mathf.Min(cs * 0.4f, Mathf.Max(cs * 0.24f, front.Drop * 0.5f));
            scale *= targetSize / authoredSize;
            instance.transform.localScale = scale;
            for (int i = 0; i < systems.Length; i++)
            {
                var main = systems[i].main;
                // SW3 emitters author in Local scaling: Hierarchy keeps
                // particle size and velocity consistent with VfxScale on
                // drops much smaller than the prefab's 20m source.
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.startLifetime = effect == 1
                    ? Mathf.Clamp(Mathf.Sqrt(front.Drop / 1.2f), 0.18f, 0.65f)
                    : effect == 0 ? 0.5f : 1f;
                main.gravityModifier = effect == 2 ? 0f : 0.15f;
                main.startSpeed = 0f;
                var velocity = systems[i].velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.Local;
                float unit = cs / Mathf.Max(0.0001f, systems[i].transform.lossyScale.x);
                velocity.x = new ParticleSystem.MinMaxCurve(-0.12f * unit, 0.12f * unit);
                velocity.y = effect == 1 ? new ParticleSystem.MinMaxCurve(-0.3f * unit, -0.2f * unit)
                    : effect == 0 ? new ParticleSystem.MinMaxCurve(0.3f * unit, 0.65f * unit)
                    : new ParticleSystem.MinMaxCurve(0.08f * unit, 0.16f * unit);
                velocity.z = new ParticleSystem.MinMaxCurve(0.05f * unit, 0.2f * unit);
                var force = systems[i].forceOverLifetime;
                force.enabled = false;
                var noise = systems[i].noise;
                noise.enabled = false;
                main.loop = true;
                main.prewarm = true;
                var emission = systems[i].emission;
                emission.enabled = true;
                float lifetime = Mathf.Max(0.1f, CurvePeak(main.startLifetime));
                // Size and emission density are independent of prefab scale.
                emission.rateOverTime = Mathf.Min(maxParticles / lifetime,
                    Mathf.Max(18f, front.WidthCells * 24f) / lifetime);
                if (main.maxParticles > maxParticles)
                    main.maxParticles = maxParticles;
                velocity.speedModifier = Mathf.Clamp(speedScale, 0.6f, 1.4f);
                if (shapeWidth > 0f)
                {
                    var shape = systems[i].shape;
                    Vector3 s = shape.scale;
                    s.x = shapeWidth / Mathf.Max(0.0001f, systems[i].transform.lossyScale.x);
                    s.y = cs * 0.1f / Mathf.Max(0.0001f, systems[i].transform.lossyScale.y);
                    s.z = cs * 0.1f / Mathf.Max(0.0001f, systems[i].transform.lossyScale.z);
                    shape.scale = s;
                }
            }
            foreach (var system in systems)
            {
                system.Simulate(Mathf.Min(1.5f, CurvePeak(system.main.startLifetime)), false, true);
                system.Play(false);
            }
            RegisterFront(instance, front);
            return 1;
        }

        private static float CurvePeak(ParticleSystem.MinMaxCurve curve)
        {
            if (curve.mode == ParticleSystemCurveMode.Constant)
                return Mathf.Abs(curve.constant);
            if (curve.mode == ParticleSystemCurveMode.TwoConstants)
                return Mathf.Max(Mathf.Abs(curve.constantMin), Mathf.Abs(curve.constantMax));
            float peak = 0f;
            for (int i = 0; i <= 32; i++)
            {
                float time = i / 32f;
                peak = Mathf.Max(peak, Mathf.Abs(curve.Evaluate(time, 0f)),
                    Mathf.Abs(curve.Evaluate(time, 1f)));
            }
            return peak;
        }

    }
}
