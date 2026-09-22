using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Vfx.Runtime;
using Kruty1918.Vfx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Vfx.EditorTools
{
    /// <summary>Editor-утиліта authoring'у VFX-префабів: генерує префаби ефектів із каталогу.</summary>
    public static class VfxPrefabAuthoring
    {
        private const string TextureRoot = "Assets/Moyva/Art/VFX/Textures";
        private const string MaterialRoot = "Assets/Moyva/Art/VFX/Materials";
        private const string MeshRoot = "Assets/Moyva/Art/VFX/Meshes";
        private const string PrefabRoot = "Assets/Moyva/Prefabs/VFX";

        private static readonly Color DustWarm = new Color(0.66f, 0.57f, 0.45f, 1f);
        private static readonly Color DustDark = new Color(0.45f, 0.39f, 0.32f, 1f);
        private static readonly Color SparkAmber = new Color(1.00f, 0.72f, 0.30f, 1f);
        private static readonly Color Tintable = new Color(1f, 1f, 1f, 1f);

        /// <summary>Menu-item: генерує всі VFX-префаби та логує результат.</summary>
        [MenuItem("Moyva/VFX/Rebuild VFX Assets")]
        public static void BuildAllMenu() => Debug.Log(BuildAll());

        /// <summary>Генерує всі VFX-префаби з каталогу; повертає звіт.</summary>
        public static string BuildAll()
        {
            TintMarks.Clear();
            EnsureFolder(TextureRoot);
            EnsureFolder(MaterialRoot);
            EnsureFolder(MeshRoot);
            EnsureFolder(PrefabRoot);
            EnsureFolder(PrefabRoot + "/Construction");
            EnsureFolder(PrefabRoot + "/Units");
            EnsureFolder(PrefabRoot + "/Combat");
            EnsureFolder(PrefabRoot + "/World");

            Texture2D softCircle = WriteTexture(
                TextureRoot + "/vfx-soft-circle.png", SoftCircle);
            Texture2D ring = WriteTexture(
                TextureRoot + "/vfx-ring.png", Ring);
            Texture2D spark = WriteTexture(
                TextureRoot + "/vfx-spark.png", Spark);
            Texture2D streak = WriteTexture(
                TextureRoot + "/vfx-streak.png", Streak);

            Material dust = WriteParticleMaterial(
                MaterialRoot + "/vfx-dust.mat", softCircle, BlendMode.Alpha);
            Material sparkMat = WriteParticleMaterial(
                MaterialRoot + "/vfx-spark.mat", spark, BlendMode.Additive);
            Material ringMat = WriteParticleMaterial(
                MaterialRoot + "/vfx-ring.mat", ring, BlendMode.Alpha);
            Material streakMat = WriteParticleMaterial(
                MaterialRoot + "/vfx-streak.mat", streak, BlendMode.Alpha);
            Material debrisMat = WriteParticleMaterial(
                MaterialRoot + "/vfx-debris.mat", null, BlendMode.Alpha);

            Mesh shardA = WriteShardMesh(MeshRoot + "/vfx-shard-a.asset", 0);
            Mesh shardB = WriteShardMesh(MeshRoot + "/vfx-shard-b.asset", 1);
            var shards = new[] { shardA, shardB };

            var report = new StringBuilder();
            BuildBuildingPlaced(dust, ringMat, debrisMat, shards, report);
            BuildBuildingOperational(dust, ringMat, report);
            BuildBuildingDemolished(dust, ringMat, debrisMat, shards, report);
            BuildUnitSpawned(dust, ringMat, report);
            BuildUnitMoveDust(dust, report);
            BuildCombatAttack(streakMat, dust, report);
            BuildCombatImpact(sparkMat, dust, report);
            BuildUnitDestroyed(dust, ringMat, debrisMat, shards, report);
            BuildSettlementCreated(dust, ringMat, report);
            BuildSettlementCaptured(dust, ringMat, sparkMat, report);
            BuildWorldPing(ringMat, sparkMat, report);

            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        // ------------------------------------------------------------------
        // Prefab definitions
        // ------------------------------------------------------------------

        private static void BuildBuildingPlaced(
            Material dust, Material ring, Material debris, Mesh[] shards, StringBuilder report)
        {
            // "Placement confirmed": soft ground dust + outward ring + few chips.
            GameObject root = NewEffectRoot();
            DustPuff(root, "DustRing", dust,
                countMin: 10, countMax: 14,
                radius: 0.75f, speedMin: 0.9f, speedMax: 1.6f,
                life: 0.55f, sizeMin: 0.30f, sizeMax: 0.55f,
                color: DustWarm, up: 0.25f);
            GroundRing(root, "FoundationRing", ring,
                sizeEnd: 2.1f, life: 0.45f, tintable: true);
            Debris(root, "Chips", debris, shards,
                countMin: 4, countMax: 7, speedMin: 0.8f, speedMax: 1.6f,
                upMin: 0.6f, upMax: 1.3f, life: 0.6f,
                sizeMin: 0.05f, sizeMax: 0.10f);
            SaveEffect(root, PrefabRoot + "/Construction/vfx-building-placed.prefab",
                duration: 1.0f, report);
        }

        private static void BuildBuildingOperational(
            Material dust, Material ring, StringBuilder report)
        {
            // "Now active": gentle upward release + short faction ring pulse.
            GameObject root = NewEffectRoot();
            RisingWisps(root, "Rise", dust,
                countMin: 4, countMax: 6, radius: 0.55f,
                speedMin: 0.7f, speedMax: 1.1f, life: 0.8f,
                sizeMin: 0.22f, sizeMax: 0.38f, color: DustWarm);
            GroundRing(root, "FactionRing", ring,
                sizeEnd: 1.7f, life: 0.5f, tintable: true);
            SaveEffect(root, PrefabRoot + "/Construction/vfx-building-operational.prefab",
                duration: 0.9f, report);
        }

        private static void BuildBuildingDemolished(
            Material dust, Material ring, Material debris, Mesh[] shards, StringBuilder report)
        {
            // "Collapse": denser central dust, real debris shards, shock ring.
            GameObject root = NewEffectRoot();
            DustPuff(root, "DustBurst", dust,
                countMin: 16, countMax: 24,
                radius: 0.9f, speedMin: 0.5f, speedMax: 1.9f,
                life: 0.9f, sizeMin: 0.40f, sizeMax: 0.85f,
                color: DustDark, up: 0.55f);
            GroundRing(root, "ShockRing", ring,
                sizeEnd: 2.8f, life: 0.55f, tintable: false,
                color: new Color(0.72f, 0.65f, 0.55f, 0.8f));
            Debris(root, "Debris", debris, shards,
                countMin: 6, countMax: 10, speedMin: 1.0f, speedMax: 2.4f,
                upMin: 0.9f, upMax: 1.9f, life: 0.8f,
                sizeMin: 0.06f, sizeMax: 0.14f);
            SaveEffect(root, PrefabRoot + "/Construction/vfx-building-demolished.prefab",
                duration: 1.2f, report);
        }

        private static void BuildUnitSpawned(
            Material dust, Material ring, StringBuilder report)
        {
            // "Deployed": footstep dust + quick faction accent ring.
            GameObject root = NewEffectRoot();
            DustPuff(root, "DeployDust", dust,
                countMin: 5, countMax: 8,
                radius: 0.28f, speedMin: 0.5f, speedMax: 1.0f,
                life: 0.45f, sizeMin: 0.14f, sizeMax: 0.26f,
                color: DustWarm, up: 0.15f);
            GroundRing(root, "AccentRing", ring,
                sizeEnd: 0.95f, life: 0.4f, tintable: true);
            SaveEffect(root, PrefabRoot + "/Units/vfx-unit-spawned.prefab",
                duration: 0.6f, report);
        }

        private static void BuildUnitMoveDust(Material dust, StringBuilder report)
        {
            GameObject root = NewEffectRoot();
            DustPuff(root, "StepDust", dust,
                countMin: 2, countMax: 3,
                radius: 0.18f, speedMin: 0.25f, speedMax: 0.55f,
                life: 0.35f, sizeMin: 0.10f, sizeMax: 0.18f,
                color: DustWarm * 0.9f, up: 0.10f);
            SaveEffect(root, PrefabRoot + "/Units/vfx-unit-move-dust.prefab",
                duration: 0.4f, report);
        }

        private static void BuildCombatAttack(
            Material streak, Material dust, StringBuilder report)
        {
            // Attack anticipation: 3 short directional streaks along +Z of the
            // effect root (service rotates it toward the target).
            GameObject root = NewEffectRoot();
            GameObject child = Child(root, "Streaks");
            var ps = child.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life: 0.22f, speed: 2.6f, sizeMin: 0.05f, sizeMax: 0.09f,
                color: new Color(1f, 0.85f, 0.55f, 0.9f), maxParticles: 8);
            Burst(ps, 3, 4);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 9f;
            shape.radius = 0.12f;
            FadeOut(ps, 0.22f);
            var renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.4f;
            renderer.velocityScale = 0.06f;
            renderer.sharedMaterial = streak;
            SaveEffect(root, PrefabRoot + "/Combat/vfx-combat-attack.prefab",
                duration: 0.35f, report);
        }

        private static void BuildCombatImpact(
            Material spark, Material dust, StringBuilder report)
        {
            GameObject root = NewEffectRoot();
            GameObject sparks = Child(root, "Sparks");
            var ps = sparks.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life: 0.30f, speed: 2.2f, sizeMin: 0.04f, sizeMax: 0.08f,
                color: SparkAmber, maxParticles: 16);
            Burst(ps, 6, 10);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.16f;
            sparks.transform.localRotation = Quaternion.Euler(-55f, 0f, 0f);
            sparks.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            FadeOut(ps, 0.3f);
            var renderer = sparks.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = spark;
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.0f;
            renderer.velocityScale = 0.04f;

            DustPuff(root, "HitDust", dust,
                countMin: 4, countMax: 6,
                radius: 0.22f, speedMin: 0.5f, speedMax: 1.1f,
                life: 0.4f, sizeMin: 0.16f, sizeMax: 0.28f,
                color: DustDark, up: 0.3f);
            SaveEffect(root, PrefabRoot + "/Combat/vfx-combat-impact.prefab",
                duration: 0.5f, report);
        }

        private static void BuildUnitDestroyed(
            Material dust, Material ring, Material debris, Mesh[] shards, StringBuilder report)
        {
            GameObject root = NewEffectRoot();
            DustPuff(root, "DeathDust", dust,
                countMin: 10, countMax: 14,
                radius: 0.4f, speedMin: 0.4f, speedMax: 1.4f,
                life: 0.8f, sizeMin: 0.25f, sizeMax: 0.5f,
                color: DustDark, up: 0.4f);
            Debris(root, "Fragments", debris, shards,
                countMin: 4, countMax: 6, speedMin: 0.6f, speedMax: 1.5f,
                upMin: 0.4f, upMax: 1.1f, life: 0.7f,
                sizeMin: 0.04f, sizeMax: 0.09f);
            GroundRing(root, "GroundRing", ring,
                sizeEnd: 1.4f, life: 0.6f, tintable: false,
                color: new Color(0.6f, 0.55f, 0.48f, 0.7f));
            SaveEffect(root, PrefabRoot + "/Units/vfx-unit-destroyed.prefab",
                duration: 1.0f, report);
        }

        private static void BuildSettlementCreated(
            Material dust, Material ring, StringBuilder report)
        {
            GameObject root = NewEffectRoot();
            DustPuff(root, "FoundDust", dust,
                countMin: 8, countMax: 12,
                radius: 1.1f, speedMin: 0.7f, speedMax: 1.4f,
                life: 0.7f, sizeMin: 0.3f, sizeMax: 0.6f,
                color: DustWarm, up: 0.35f);
            GroundRing(root, "FoundRing", ring,
                sizeEnd: 2.6f, life: 0.6f, tintable: true);
            SaveEffect(root, PrefabRoot + "/World/vfx-settlement-created.prefab",
                duration: 1.1f, report);
        }

        private static void BuildSettlementCaptured(
            Material dust, Material ring, Material spark, StringBuilder report)
        {
            GameObject root = NewEffectRoot();
            GroundRing(root, "CaptureRing", ring,
                sizeEnd: 3.4f, life: 0.8f, tintable: true);
            RisingWisps(root, "Rise", dust,
                countMin: 8, countMax: 12, radius: 0.9f,
                speedMin: 0.9f, speedMax: 1.6f, life: 1.0f,
                sizeMin: 0.2f, sizeMax: 0.4f, color: DustWarm, tintable: true);
            GameObject sparks = Child(root, "Accents");
            var ps = sparks.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life: 0.9f, speed: 1.4f, sizeMin: 0.05f, sizeMax: 0.10f,
                color: Tintable, maxParticles: 24);
            Burst(ps, 8, 12);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.8f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(0.9f);
            vel.space = ParticleSystemSimulationSpace.Local;
            FadeOut(ps, 0.9f);
            sparks.GetComponent<ParticleSystemRenderer>().sharedMaterial = spark;
            SaveEffect(root, PrefabRoot + "/World/vfx-settlement-captured.prefab",
                duration: 1.4f, report, extraTint: sparks.GetComponent<ParticleSystem>());
        }

        private static void BuildWorldPing(Material ring, Material spark, StringBuilder report)
        {
            GameObject root = NewEffectRoot();
            GroundRing(root, "PingRing", ring,
                sizeEnd: 2.4f, life: 0.9f, tintable: false,
                color: new Color(0.95f, 0.75f, 0.30f, 0.95f));
            GameObject beam = Child(root, "Beam");
            var ps = beam.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life: 0.8f, speed: 0f, sizeMin: 0.5f, sizeMax: 0.5f,
                color: new Color(1f, 0.85f, 0.45f, 0.55f), maxParticles: 2);
            Burst(ps, 1, 1);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.01f;
            beam.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.35f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0.9f)));
            FadeOut(ps, 0.8f);
            var renderer = beam.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = spark;
            renderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
            SaveEffect(root, PrefabRoot + "/World/vfx-world-ping.prefab",
                duration: 1.0f, report);
        }

        // ------------------------------------------------------------------
        // Subsystem builders
        // ------------------------------------------------------------------

        private static void DustPuff(
            GameObject root, string name, Material material,
            int countMin, int countMax, float radius,
            float speedMin, float speedMax, float life,
            float sizeMin, float sizeMax, Color color, float up,
            bool tintable = false)
        {
            GameObject child = Child(root, name);
            var ps = child.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life, new ParticleSystem.MinMaxCurve(speedMin, speedMax),
                sizeMin, sizeMax, color, countMax * 2);
            Burst(ps, countMin, countMax);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            child.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(up);
            vel.space = ParticleSystemSimulationSpace.Local;
            var lim = ps.limitVelocityOverLifetime;
            lim.enabled = true;
            lim.limit = new ParticleSystem.MinMaxCurve(0.45f);
            lim.dampen = 0.6f;
            FadeOut(ps, life);
            Grow(ps, 0.6f, 1.35f);
            var renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (tintable) MarkTint(child);
        }

        private static void RisingWisps(
            GameObject root, string name, Material material,
            int countMin, int countMax, float radius,
            float speedMin, float speedMax, float life,
            float sizeMin, float sizeMax, Color color, bool tintable = false)
        {
            GameObject child = Child(root, name);
            var ps = child.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life, new ParticleSystem.MinMaxCurve(speedMin, speedMax),
                sizeMin, sizeMax, color, countMax * 2);
            Burst(ps, countMin, countMax);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = radius;
            child.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(1f);
            FadeOut(ps, life);
            Grow(ps, 0.7f, 1.5f);
            var renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (tintable) MarkTint(child);
        }

        private static void GroundRing(
            GameObject root, string name, Material material,
            float sizeEnd, float life, bool tintable, Color? color = null)
        {
            GameObject child = Child(root, name);
            var ps = child.AddComponent<ParticleSystem>();
            ConfigureBase(ps, life, 0f, 1f, 1f,
                color ?? Tintable, 4);
            Burst(ps, 1, 1);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.01f;
            child.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(sizeEnd,
                new AnimationCurve(
                    new Keyframe(0f, 0.25f),
                    new Keyframe(0.35f, 0.8f),
                    new Keyframe(1f, 1f)));
            FadeOut(ps, life);
            var renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            if (tintable) MarkTint(child);
        }

        private static void Debris(
            GameObject root, string name, Material material, Mesh[] meshes,
            int countMin, int countMax, float speedMin, float speedMax,
            float upMin, float upMax, float life,
            float sizeMin, float sizeMax)
        {
            GameObject child = Child(root, name);
            var ps = child.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.8f, life);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.5f, 0.42f, 0.33f, 1f), new Color(0.62f, 0.55f, 0.45f, 1f));
            main.maxParticles = countMax * 2;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(1.6f);
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
            main.startRotationY = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(-1.5f, 1.5f);
            Burst(ps, countMin, countMax);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.35f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(upMin, upMax);
            vel.space = ParticleSystemSimulationSpace.Local;
            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.x = new ParticleSystem.MinMaxCurve(-6f, 6f);
            rot.y = new ParticleSystem.MinMaxCurve(-6f, 6f);
            var renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.sharedMaterial = material;
            renderer.SetMeshes(meshes);
        }

        // ------------------------------------------------------------------
        // ParticleSystem plumbing
        // ------------------------------------------------------------------

        private static void ConfigureBase(
            ParticleSystem ps, float life, float speed,
            float sizeMin, float sizeMax, Color color, int maxParticles)
            => ConfigureBase(ps, life, new ParticleSystem.MinMaxCurve(speed, speed),
                sizeMin, sizeMax, color, maxParticles);

        private static void ConfigureBase(
            ParticleSystem ps, float life, ParticleSystem.MinMaxCurve speed,
            float sizeMin, float sizeMax, Color color, int maxParticles)
        {
            var main = ps.main;
            main.duration = Mathf.Max(life, 0.2f);
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.7f, life);
            main.startSpeed = speed;
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = new ParticleSystem.MinMaxGradient(color);
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.stopAction = ParticleSystemStopAction.None;
        }

        private static void Burst(ParticleSystem ps, int min, int max)
        {
            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(
                    0f, new ParticleSystem.MinMaxCurve(min, max))
            });
        }

        private static void FadeOut(ParticleSystem ps, float life)
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.85f, 0.55f),
                    new GradientAlphaKey(0f, 1f),
                });
            col.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static void Grow(ParticleSystem ps, float start, float end)
        {
            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(
                    new Keyframe(0f, start), new Keyframe(1f, end)));
        }

        private static readonly Dictionary<GameObject, List<ParticleSystem>> TintMarks =
            new Dictionary<GameObject, List<ParticleSystem>>();

        private static void MarkTint(GameObject child)
        {
            GameObject root = child.transform.root.gameObject;
            if (!TintMarks.TryGetValue(root, out var list))
                TintMarks[root] = list = new List<ParticleSystem>();
            var ps = child.GetComponent<ParticleSystem>();
            if (ps != null) list.Add(ps);
        }

        // ------------------------------------------------------------------
        // Asset writers
        // ------------------------------------------------------------------

        private static GameObject NewEffectRoot() => new GameObject("vfx-effect");

        private static GameObject Child(GameObject root, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            return child;
        }

        private static void SaveEffect(
            GameObject root, string path, float duration, StringBuilder report,
            ParticleSystem extraTint = null)
        {
            var effect = root.AddComponent<VfxEffect>();
            effect.duration = duration;
            if (TintMarks.TryGetValue(root, out var tintList) || extraTint != null)
            {
                var targets = tintList != null
                    ? new List<ParticleSystem>(tintList)
                    : new List<ParticleSystem>();
                if (extraTint != null && !targets.Contains(extraTint))
                    targets.Add(extraTint);
                effect.tintTargets = targets.ToArray();
            }

            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            TintMarks.Remove(root);
            report.Append("prefab ").AppendLine(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(
                parent ?? "Assets", Path.GetFileName(path));
        }

        private static Texture2D WriteTexture(
            string path, Func<int, int, Color> pixel)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, pixel(x, y));
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 64;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>Режим блендингу ефекту.</summary>
        private enum BlendMode { Alpha, Additive }

        private static Material WriteParticleMaterial(
            string path, Texture2D texture, BlendMode blend)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            var mat = new Material(shader);
            if (texture != null)
                mat.SetTexture("_BaseMap", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", blend == BlendMode.Additive ? 2f : 0f);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_SoftParticlesEnabled", 0f);
            mat.enableInstancing = true;
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mat, path);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private static Mesh WriteShardMesh(string path, int variant)
        {
            // Two chunky low-poly shards; deterministic shape per variant.
            float s = 0.5f;
            var v = new List<Vector3>
            {
                new Vector3(-s, 0f, -s * 0.6f),
                new Vector3(s * 0.8f, 0f, -s * 0.4f),
                new Vector3(s * 0.5f, 0f, s),
                new Vector3(-s * 0.7f, 0f, s * 0.5f),
                new Vector3(variant == 0 ? -0.1f : 0.2f, s * (variant == 0 ? 0.9f : 0.6f), 0f),
            };
            var tris = new List<int>();
            for (int i = 1; i + 1 < 4; i++)
            {
                tris.Add(0); tris.Add(i); tris.Add(i + 1); // base fan
            }
            for (int i = 0; i < 4; i++)
            {
                int n = (i + 1) % 4;
                tris.Add(i); tris.Add(n); tris.Add(4); // sides to apex
            }
            var mesh = new Mesh { name = Path.GetFileNameWithoutExtension(path) };
            mesh.SetVertices(v);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh, path);
            return AssetDatabase.LoadAssetAtPath<Mesh>(path);
        }

        // ------------------------------------------------------------------
        // Texture pixel functions
        // ------------------------------------------------------------------

        private static Color SoftCircle(int x, int y)
        {
            float d = Mathf.Clamp01(1f - Dist01(x, y));
            float a = d * d * (3f - 2f * d); // smoothstep
            return new Color(1f, 1f, 1f, a * a);
        }

        private static Color Ring(int x, int y)
        {
            float d = Dist01(x, y);
            float band = Mathf.Clamp01(1f - Mathf.Abs(d - 0.78f) / 0.16f);
            return new Color(1f, 1f, 1f, band * band);
        }

        private static Color Spark(int x, int y)
        {
            float d = Dist01(x, y);
            float core = Mathf.Clamp01(1f - d / 0.22f);
            float glow = Mathf.Clamp01(1f - d / 0.6f) * 0.5f;
            return new Color(1f, 1f, 1f, Mathf.Clamp01(core + glow));
        }

        private static Color Streak(int x, int y)
        {
            float dx = Mathf.Abs(x - 31.5f) / 31.5f;
            float dy = Mathf.Abs(y - 31.5f) / 31.5f;
            float a = Mathf.Clamp01(1f - dx) * Mathf.Clamp01(1f - dy * 2.6f);
            return new Color(1f, 1f, 1f, a * a);
        }

        private static float Dist01(int x, int y)
        {
            float dx = (x - 31.5f) / 32f;
            float dy = (y - 31.5f) / 32f;
            return Mathf.Sqrt(dx * dx + dy * dy) * 2f;
        }
    }
}
