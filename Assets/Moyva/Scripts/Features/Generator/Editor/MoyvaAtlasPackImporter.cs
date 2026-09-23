using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GiantGrey.TileWorldCreator;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Imports the AtlasV3 no-water tile pack into deterministic, production-wired
    /// Unity assets: meshes, prefabs, the shared atlas material and nine dual-grid
    /// <see cref="TilePreset"/> assets under a stable Generated/ folder.
    ///
    /// Asset GUIDs are derived from the asset path (md5), so JSON $asset keys in
    /// Assets/Moyva/Presets stay stable across machines and reimports. Running the
    /// importer twice updates content in place; it never moves or renames assets.
    /// </summary>
    public static class MoyvaAtlasPackImporter
    {
        public const string PackRoot = "Assets/Moyva/Art/World/Tiles/AtlasV3";
        public const string MeshDataPath = PackRoot + "/Unity/MoyvaMeshData.json";
        public const string OutputRoot = PackRoot + "/Generated";
        public const string MeshesFolder = OutputRoot + "/Meshes";
        public const string PrefabsFolder = OutputRoot + "/Prefabs";
        public const string PresetsFolder = OutputRoot + "/Presets";
        public const string MaterialsFolder = OutputRoot + "/Materials";
        public const string MaterialPath = MaterialsFolder + "/Moyva_Atlas.mat";

        /// <summary>Theme ids in pack order; also the semantic tile ids they render.</summary>
        public static readonly string[] Themes =
        {
            "grass", "sand", "dirt", "stone", "snow", "swamp", "rock_cliff", "road", "footpath"
        };

        private const int ExpectedMeshCount = 99;

        [Serializable]
        private sealed class MeshRecord
        {
            public string name;
            public string kind;
            public float[] vertices;
            public float[] normals;
            public float[] uv;
            public int[] triangles;
        }

        [Serializable]
        private sealed class MeshBundle
        {
            public MeshRecord[] meshes;
        }

        [MenuItem("Tools/Moyva/Atlas V3/Import Atlas Pack Assets")]
        public static void ImportMenu()
        {
            int count = Import();
            Debug.Log($"[MoyvaAtlasImporter] Imported {count} atlas meshes into {OutputRoot}.");
        }

        /// <summary>Batch entry point for -executeMethod.</summary>
        public static void ImportForBatch()
        {
            try
            {
                int count = Import();
                Debug.Log($"[MoyvaAtlasImporter] Batch import complete: {count} meshes, " +
                          $"{Themes.Length} presets under {OutputRoot}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoyvaAtlasImporter] Import failed: {ex}");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Rebuilds all atlas assets in place. Returns the number of mesh records processed.
        /// </summary>
        public static int Import()
        {
            if (!File.Exists(MeshDataPath))
                throw new FileNotFoundException("Missing atlas mesh data.", MeshDataPath);

            var data = JsonUtility.FromJson<MeshBundle>(File.ReadAllText(MeshDataPath));
            if (data?.meshes == null || data.meshes.Length != ExpectedMeshCount)
                throw new InvalidDataException(
                    $"Expected {ExpectedMeshCount} validated tile meshes in {MeshDataPath}.");

            EnsureFolder(OutputRoot);
            EnsureFolder(MeshesFolder);
            EnsureFolder(PrefabsFolder);
            EnsureFolder(PresetsFolder);
            EnsureFolder(MaterialsFolder);

            Texture2D albedo = PrepareTexture(PackRoot + "/Textures/Moyva_AlbedoAtlas.png", isNormal: false, srgb: true);
            Texture2D normal = PrepareTexture(PackRoot + "/Textures/Moyva_NormalAtlas.png", isNormal: true, srgb: false);
            Texture2D packed = PrepareTexture(PackRoot + "/Textures/Moyva_MetallicSmoothnessAtlas.png", isNormal: false, srgb: false);
            Material material = CreateOrUpdateMaterial(albedo, normal, packed);

            var prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (MeshRecord record in data.meshes)
            {
                Mesh mesh = CreateOrUpdateMesh(record);
                prefabs[record.name] = CreateOrUpdatePrefab(record.name, mesh, material);
            }

            foreach (string theme in Themes)
                CreateOrUpdatePreset(theme, prefabs);

            AssetDatabase.SaveAssets();
            NormalizeDeterministicGuids();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateAssetKeys();
            return data.meshes.Length;
        }

        /// <summary>Deterministic GUID: first 32 hex of md5(lowercase asset path).</summary>
        public static string DeterministicGuid(string assetPath)
        {
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(assetPath.ToLowerInvariant()));
            var sb = new StringBuilder(32);
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// Replicates JsonizationEditorUtil.AssetKey so the importer can verify that
        /// authored JSON $asset references resolve to the generated assets.
        /// </summary>
        public static string AssetKey(Type assetType, string assetName, string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
                guid = DeterministicGuid(assetPath);
            return $"asset.{StableId(assetType)}.{Slug(assetName)}.{guid.Substring(0, 8)}";
        }

        /// <summary>Kebab-case stable type id with Config/SO/Settings suffixes stripped.</summary>
        private static string StableId(Type type)
        {
            string name = type.Name;
            foreach (string suffix in new[] { "Config", "Settings", "SO" })
            {
                if (name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length)
                {
                    name = name.Substring(0, name.Length - suffix.Length);
                    break;
                }
            }

            var sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (char.IsUpper(c) && i > 0 &&
                    (char.IsLower(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
                {
                    sb.Append('-');
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        private static string Slug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "config";

            var sb = new StringBuilder(value.Length);
            bool dash = false;
            foreach (char c in value.Trim())
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                    dash = false;
                }
                else if (!dash && sb.Length > 0)
                {
                    sb.Append('-');
                    dash = true;
                }
            }
            while (sb.Length > 0 && sb[sb.Length - 1] == '-')
                sb.Length--;
            return sb.Length == 0 ? "config" : sb.ToString();
        }

        /// <summary>
        /// Rewrites generated asset .meta GUIDs to their deterministic values and
        /// patches every guid reference inside the generated YAML files so cross
        /// references stay intact. Runs after all assets exist; a following
        /// Refresh makes the new identities authoritative.
        /// </summary>
        private static void NormalizeDeterministicGuids()
        {
            var files = new List<string>();
            CollectGeneratedFiles(OutputRoot, files);

            var remap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in files)
            {
                string metaPath = path + ".meta";
                if (!File.Exists(metaPath))
                    continue;

                string meta = File.ReadAllText(metaPath);
                var match = System.Text.RegularExpressions.Regex.Match(meta, @"guid:\s*([0-9a-fA-F]{32})");
                if (!match.Success)
                    continue;

                string wanted = DeterministicGuid(path);
                string actual = match.Groups[1].Value;
                if (string.Equals(actual, wanted, StringComparison.OrdinalIgnoreCase))
                    continue;

                string claimed = AssetDatabase.GUIDToAssetPath(wanted);
                if (!string.IsNullOrEmpty(claimed) &&
                    !string.Equals(claimed, path, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Deterministic guid collision: {path} vs {claimed}.");
                }

                remap[actual] = wanted;
            }

            if (remap.Count == 0)
                return;

            foreach (string path in files)
            {
                string text = File.ReadAllText(path);
                string updated = text;
                foreach (var pair in remap)
                    updated = updated.Replace(pair.Key, pair.Value);
                if (!ReferenceEquals(updated, text))
                    File.WriteAllText(path, updated);

                string metaPath = path + ".meta";
                if (File.Exists(metaPath))
                {
                    string meta = File.ReadAllText(metaPath);
                    string metaUpdated = meta;
                    foreach (var pair in remap)
                        metaUpdated = metaUpdated.Replace(pair.Key, pair.Value);
                    if (!ReferenceEquals(metaUpdated, meta))
                        File.WriteAllText(metaPath, metaUpdated);
                }
            }
        }

        private static void CollectGeneratedFiles(string folder, List<string> results)
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;
                results.Add(file.Replace('\\', '/'));
            }
            foreach (string dir in Directory.GetDirectories(folder))
                CollectGeneratedFiles(dir.Replace('\\', '/'), results);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static Texture2D PrepareTexture(string path, bool isNormal, bool srgb)
        {
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                throw new FileNotFoundException("Missing atlas texture.", path);

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Material CreateOrUpdateMaterial(Texture2D albedo, Texture2D normal, Texture2D packed)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("URP Lit or Standard shader is required.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            bool created = material == null;
            if (created)
                material = new Material(shader);

            material.shader = shader;
            material.name = "Moyva_Atlas";
            material.enableInstancing = true;
            SetTexture(material, "_BaseMap", albedo);
            SetTexture(material, "_MainTex", albedo);
            SetTexture(material, "_BumpMap", normal);
            SetTexture(material, "_MetallicGlossMap", packed);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICGLOSSMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            if (material.HasProperty("_BumpScale")) material.SetFloat("_BumpScale", 0.35f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 1f);
            if (material.HasProperty("_GlossMapScale")) material.SetFloat("_GlossMapScale", 1f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);

            // CreateAsset reimports and destroys the in-memory object; only set
            // data before creation and never touch the reference afterwards.
            if (created)
                AssetDatabase.CreateAsset(material, MaterialPath);
            else
                EditorUtility.SetDirty(material);
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
                material.SetTexture(property, texture);
        }

        private static Mesh CreateOrUpdateMesh(MeshRecord record)
        {
            int count = record.vertices.Length / 3;
            if (record.normals.Length != count * 3
                || record.uv.Length != count * 2
                || record.triangles.Length % 3 != 0)
            {
                throw new InvalidDataException("Invalid array sizes: " + record.name);
            }

            var positions = new Vector3[count];
            var normals = new Vector3[count];
            var uv = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                positions[i] = new Vector3(record.vertices[3 * i], record.vertices[3 * i + 1], record.vertices[3 * i + 2]);
                normals[i] = new Vector3(record.normals[3 * i], record.normals[3 * i + 1], record.normals[3 * i + 2]);
                uv[i] = new Vector2(record.uv[2 * i], record.uv[2 * i + 1]);
            }

            string path = $"{MeshesFolder}/{record.name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool created = mesh == null;
            if (created)
                mesh = new Mesh();
            else
                mesh.Clear();

            mesh.name = record.name;
            mesh.vertices = positions;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = record.triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            if (created)
            {
                AssetDatabase.CreateAsset(mesh, path);
                // The CreateAsset import cycle may invalidate the object; reload.
                return AssetDatabase.LoadAssetAtPath<Mesh>(path);
            }

            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static GameObject CreateOrUpdatePrefab(string name, Mesh mesh, Material material)
        {
            string path = $"{PrefabsFolder}/{name}.prefab";

            var go = new GameObject(name);
            try
            {
                go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                go.transform.localScale = Vector3.one;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
                // Open surface collision is deliberate. Convex hulls would reintroduce hidden geometry.
                go.AddComponent<MeshCollider>().sharedMesh = mesh;

                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                if (prefab == null)
                    throw new IOException("Could not save prefab " + path);
                // Overwrite-import can invalidate the returned object; reload.
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static TilePreset CreateOrUpdatePreset(string theme, Dictionary<string, GameObject> prefabs)
        {
            string path = $"{PresetsFolder}/atlas-{theme}.asset";
            TilePreset preset = AssetDatabase.LoadAssetAtPath<TilePreset>(path);
            bool created = preset == null;
            if (created)
                preset = ScriptableObject.CreateInstance<TilePreset>();

            preset.tileId = theme;
            preset.gridtype = TilePreset.GridType.dual;
            preset.DUALGRD_cornerTile = RequirePrefab(prefabs, theme + "_corner", path);
            preset.DUALGRD_edgeTile = RequirePrefab(prefabs, theme + "_edge", path);
            preset.DUALGRD_invertedCornerTile = RequirePrefab(prefabs, theme + "_interior", path);
            preset.DUALGRD_doubleInteriorCornerTile = RequirePrefab(prefabs, theme + "_merged", path);
            preset.DUALGRD_fillTile = RequirePrefab(prefabs, theme + "_fill", path);

            if (created)
            {
                AssetDatabase.CreateAsset(preset, path);
                return AssetDatabase.LoadAssetAtPath<TilePreset>(path);
            }

            EditorUtility.SetDirty(preset);
            return preset;
        }

        private static GameObject RequirePrefab(Dictionary<string, GameObject> prefabs, string name, string presetPath)
        {
            if (!prefabs.TryGetValue(name, out GameObject prefab) || prefab == null)
                throw new InvalidDataException($"Missing pack mesh '{name}' required by {presetPath}.");
            return prefab;
        }

        /// <summary>
        /// Verifies every generated asset kept its deterministic GUID so the $asset
        /// keys authored in Assets/Moyva/Presets resolve correctly. Mismatches are
        /// logged with the actual key so the JSON can be corrected.
        /// </summary>
        private static void ValidateAssetKeys()
        {
            var paths = new List<string> { MaterialPath };
            foreach (string theme in Themes)
            {
                paths.Add($"{PresetsFolder}/atlas-{theme}.asset");
                foreach (string kind in new[] { "corner", "edge", "interior", "merged", "fill" })
                {
                    paths.Add($"{MeshesFolder}/{theme}_{kind}.asset");
                    paths.Add($"{MeshesFolder}/{theme}_{kind}_low.asset");
                    paths.Add($"{PrefabsFolder}/{theme}_{kind}.prefab");
                    paths.Add($"{PrefabsFolder}/{theme}_{kind}_low.prefab");
                }
                paths.Add($"{MeshesFolder}/{theme}_stair_025.asset");
                paths.Add($"{PrefabsFolder}/{theme}_stair_025.prefab");
            }

            int mismatches = 0;
            foreach (string path in paths)
            {
                string actual = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(actual))
                {
                    Debug.LogError($"[MoyvaAtlasImporter] Missing expected asset: {path}");
                    mismatches++;
                    continue;
                }

                if (!string.Equals(actual, DeterministicGuid(path), StringComparison.OrdinalIgnoreCase))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    string key = asset != null
                        ? AssetKey(asset.GetType(), asset.name, path)
                        : $"guid:{actual}";
                    Debug.LogError($"[MoyvaAtlasImporter] Non-deterministic guid at {path}; " +
                                   $"authored $asset keys must use '{key}'.");
                    mismatches++;
                }
            }

            if (mismatches > 0)
                throw new InvalidOperationException(
                    $"[MoyvaAtlasImporter] {mismatches} asset(s) lost their deterministic guid.");
        }
    }
}
