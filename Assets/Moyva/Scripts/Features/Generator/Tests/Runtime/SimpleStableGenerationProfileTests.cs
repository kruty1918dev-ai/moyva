using System;
using System.IO;
using System.Linq;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Jsonization;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// simple-stable-v1 profile: JSON document resolves through the runtime
    /// config path, the resolved snapshot carries the visual-composition
    /// switches the chunk build consumes, the generated meshes keep their
    /// single-top/single-quad invariants, and every decoration allowlist id
    /// exists in the map-object registry.
    /// </summary>
    [TestFixture]
    public sealed class SimpleStableGenerationProfileTests
    {
        [OneTimeSetUp]
        public void EnsureConfigLoaded()
        {
            if (!JsonConfigRuntime.IsLoaded)
            {
                JsonConfigRuntime.Configure(MoyvaJsonRuntimeSettings.Create());
                JsonConfigRuntime.EnsureLoaded();
            }
        }

        [Test]
        public void GenerationProfilesDocument_LoadsAndResolvesDefault()
        {
            var config = JsonConfigRuntime.Get<GenerationProfilesConfig>(
                GenerationProfileResolver.DocumentId);
            Assert.NotNull(config, "generationprofiles.json must load at runtime");
            Assert.AreEqual("simple-stable-v1", config.DefaultProfileId);

            var profile = GenerationProfileResolver.ResolveActive();
            Assert.NotNull(profile);
            Assert.AreEqual("simple-stable-v1", profile.Id);
            Assert.IsTrue(profile.SimpleWater);
            Assert.IsTrue(profile.SingleSolidTile);
            Assert.IsFalse(profile.WaterEmitBed, "simple water is opaque; no underwater bed tiles");
            Assert.AreEqual(
                "environment-decoration-config-simple-stable-v1",
                profile.DecorationConfigId);
        }

        [Test]
        public void Resolve_MissingOrDisabledProfile_ReturnsNull()
        {
            var config = new GenerationProfilesConfig
            {
                Profiles = new[]
                {
                    new GenerationProfile { Id = "a", Enabled = false },
                    new GenerationProfile { Id = "b", Enabled = true },
                },
            };

            Assert.IsNull(GenerationProfileResolver.Resolve(config, "a"));
            Assert.IsNull(GenerationProfileResolver.Resolve(config, "missing"));
            Assert.IsNull(GenerationProfileResolver.Resolve(config, null));
            Assert.NotNull(GenerationProfileResolver.Resolve(config, "b"));
        }

        [Test]
        public void ProfileDecorationConfig_LoadsAsDecorationConfig()
        {
            var config = JsonConfigRuntime.Get<EnvironmentDecorationConfig>(
                "environment-decoration-config-simple-stable-v1");
            Assert.NotNull(config, "profile decoration document must load");
            Assert.IsTrue(config.Enabled);
            Assert.NotNull(config.AssetPools);
            Assert.Greater(config.AssetPools.Count, 0);
        }

        private const int ExpectedAllowlistIds = 13;

        // [Serializable] DTOs mirror the registry JSON fields JsonUtility reads.
        [Serializable]
        private sealed class RegistryDocument
        {
            public RegistryDefinition[] definitions;
        }

        [Serializable]
        private sealed class RegistryDefinition
        {
            public string id;
            public RegistryVisualPrefab visualPrefab;
        }

        [Serializable]
        private sealed class RegistryVisualPrefab
        {
            public string editorPath;
        }

        [Test]
        public void DecorationAllowlist_IdsExistInRegistryAndOnDisk()
        {
            string root = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));

            var registry = JsonUtility.FromJson<RegistryDocument>(File.ReadAllText(
                Path.Combine(root,
                    "Assets/Moyva/Presets/Generator/map-object-registry/mapobjectregistry.json")));
            Assert.NotNull(registry?.definitions);

            var registryPaths = registry.definitions
                .Where(d => d?.visualPrefab != null)
                .ToDictionary(d => d.id, d => d.visualPrefab.editorPath,
                    System.StringComparer.Ordinal);

            string poolsJson = File.ReadAllText(Path.Combine(
                root,
                "Assets/Moyva/Presets/Generator/environment-decoration/" +
                "environment-decoration-config-simple-stable-v1.json"));

            // Pool values are allowlisted sw-* / veg-* ids; every id must
            // resolve to a registry definition whose prefab exists on disk.
            foreach (System.Text.RegularExpressions.Match match in
                     System.Text.RegularExpressions.Regex.Matches(
                         poolsJson, "\"(veg-[a-z0-9-]+)\""))
            {
                string id = match.Groups[1].Value;
                Assert.IsTrue(
                    registryPaths.TryGetValue(id, out string vegPath),
                    $"pool id '{id}' missing from mapobjectregistry");
                Assert.IsTrue(
                    File.Exists(Path.Combine(root, vegPath)),
                    $"prefab for '{id}' missing at {vegPath}");
            }

            var checkedIds = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.Ordinal);
            foreach (System.Text.RegularExpressions.Match match in
                     System.Text.RegularExpressions.Regex.Matches(
                         poolsJson, "\"(sw-[a-z0-9-]+)\""))
            {
                string id = match.Groups[1].Value;
                Assert.IsTrue(
                    registryPaths.TryGetValue(id, out string editorPath),
                    $"allowlist id '{id}' missing from mapobjectregistry");
                Assert.IsTrue(
                    File.Exists(Path.Combine(root, editorPath)),
                    $"prefab for '{id}' missing at {editorPath}");
                checkedIds.Add(id);
            }

            Assert.AreEqual(
                ExpectedAllowlistIds,
                registry.definitions.Count(d => d.id != null && d.id.StartsWith("sw-")),
                "registry must contain the full sw-* allowlist");
            Assert.AreEqual(ExpectedAllowlistIds, checkedIds.Count,
                "decoration config pools must list the whole allowlist");
        }

        [Test]
        public void WaterfallVfxAndMaterial_ResolveThroughRuntimeRecipe()
        {
            var recipe = JsonConfigRuntime.Get<GeneratorMapRecipe>("testgeneratorrecipe");
            var falls = recipe.Hydrology.Waterfalls;
            Assert.IsTrue(falls.Enabled);
            Assert.NotNull(falls.EdgeFoamPrefab, "lip foam prefab must resolve");
            Assert.NotNull(falls.ImpactSplashPrefab, "splash prefab must resolve");
            Assert.NotNull(falls.MistPrefab, "mist prefab must resolve");
            Assert.NotNull(falls.CurtainMaterial);
            Assert.IsTrue(falls.CurtainMaterial.IsKeywordEnabled("_RIVER"));
            Assert.IsFalse(falls.CurtainMaterial.IsKeywordEnabled("_WAVES"));
            Assert.LessOrEqual(falls.MinDropMeters, 0.02f);
            var water = GenerationProfileResolver.ResolveActive().WaterMaterial;
            Assert.AreEqual(water.GetColor("_BaseColor"), falls.CurtainMaterial.GetColor("_BaseColor"));
            Assert.AreEqual(1f, water.GetFloat("_ZWrite"));
        }

        [Test]
        public void BillboardVegetation_UsesSingleGroundedPlaneWithoutRealShadows()
        {
            string folder = "Assets/Moyva/Generated/Vegetation/Prefabs";
            int count = 0;
            foreach (string guid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                var renderer = prefab.GetComponent<MeshRenderer>();
                var material = renderer.sharedMaterial;
                if (!material.HasProperty("_BillboardEnabled") || material.GetFloat("_BillboardEnabled") < 0.5f)
                    continue;
                count++;
                var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreEqual(4, mesh.vertexCount, prefab.name);
                Assert.AreEqual(0f, mesh.bounds.size.z, 0.0001f, prefab.name);
                Assert.AreEqual(0f, mesh.bounds.min.y, 0.0001f, prefab.name);
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, renderer.shadowCastingMode, prefab.name);
            }
            Assert.GreaterOrEqual(count, 20);
        }

        [Test]
        public void SimpleWaterQuad_IsSingleFlatCellQuad()
        {
            Mesh mesh = SimpleWaterQuadMeshUtility.GetOrCreate();
            Assert.NotNull(mesh);
            Assert.AreEqual(4, mesh.vertexCount, "one quad = four corners");
            Assert.AreEqual(2, mesh.triangles.Length / 3, "one quad = two triangles");

            foreach (Vector3 v in mesh.vertices)
            {
                Assert.AreEqual(0f, v.y, "water quad must be flat at y=0");
                Assert.LessOrEqual(Mathf.Abs(v.x), 0.5001f);
                Assert.LessOrEqual(Mathf.Abs(v.z), 0.5001f);
            }
            foreach (Vector3 n in mesh.normals)
                Assert.AreEqual(Vector3.up, n, "water quad faces up");
        }

        [Test]
        public void SolidBeveledTile_HasSingleTopAndBevelRing()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Assert.NotNull(mesh);
            // 5 quads (top + 4 bevel sides; the chunk pipeline extrudes the
            // bevel bottom ring into the closure skirt) * 4 verts = 20,
            // * 2 tris = 30 indices.
            Assert.AreEqual(20, mesh.vertexCount);
            Assert.AreEqual(30, mesh.triangles.Length);

            int topVerts = mesh.vertices.Count(v => Mathf.Approximately(v.y, 0f));
            Assert.AreEqual(12, topVerts,
                "one top quad contributes 4 verts and each bevel reuses 2 top-edge verts");
        }

        [Test]
        public void SolidBeveledTile_FlushSidesReachCellBorderWithoutGroove()
        {
            Mesh flat = SolidBeveledTileMeshUtility.GetOrCreate(
                TileMeshOccludedSides.North | TileMeshOccludedSides.East
                | TileMeshOccludedSides.South | TileMeshOccludedSides.West);
            Assert.AreEqual(4, flat.vertexCount, "fully surrounded tile is one flat top quad");
            foreach (Vector3 v in flat.vertices)
            {
                Assert.AreEqual(0f, v.y, 1e-5f);
                Assert.AreEqual(0.5f, Mathf.Abs(v.x), 1e-5f);
                Assert.AreEqual(0.5f, Mathf.Abs(v.z), 1e-5f);
            }

            Mesh north = SolidBeveledTileMeshUtility.GetOrCreate(TileMeshOccludedSides.North);
            Assert.AreEqual(16, north.vertexCount, "top + three open-side bevels");
            Assert.IsTrue(north.vertices.Any(v => Mathf.Approximately(v.y, 0f) && Mathf.Approximately(v.z, 0.5f)),
                "top reaches the flush north border");
            Assert.AreNotSame(north, flat);
            Assert.AreSame(north, SolidBeveledTileMeshUtility.GetOrCreate(TileMeshOccludedSides.North));
        }
    }
}
