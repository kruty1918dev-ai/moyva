using System.IO;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Editor.ObjectPlacement
{
    public static class GrassCardPrefabFactory
    {
        private const string DefaultFolder = "Assets/Moyva/Generated/Grass";

        [MenuItem("Moyva/Generator/Object Placement/Create Grass Card Prefab From Selection", priority = 2200)]
        public static void CreateGrassCardPrefabFromSelection()
        {
            var texture = ResolveSelectedTexture();
            if (texture == null)
            {
                EditorUtility.DisplayDialog(
                    "Create Grass Card Prefab",
                    "Select a Texture2D or a material with a grass texture first.",
                    "OK");
                return;
            }

            var settings = new GrassCardSettings
            {
                Texture = texture,
                Tint = Color.white,
                AlphaClip = 0.35f,
                CrossedPlanes = 3,
                Width = 0.7f,
                Height = 0.9f,
                DoubleSided = true
            };

            string prefabPath = CreateGrassCardPrefab(texture.name, settings, DefaultFolder);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        [MenuItem("Moyva/Generator/Object Placement/Create Grass Card Prefab From Selection", true)]
        private static bool ValidateCreateGrassCardPrefabFromSelection()
        {
            return ResolveSelectedTexture() != null;
        }

        public static string CreateGrassCardPrefab(
            string name,
            GrassCardSettings settings,
            string folder)
        {
            settings ??= new GrassCardSettings();
            string safeName = MakeSafeFileName(string.IsNullOrWhiteSpace(name) ? "GrassCard" : name);
            EnsureFolder(folder);

            string materialPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, safeName + "_Mat.mat"));
            string meshPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, safeName + "_Mesh.asset"));
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, safeName + "_Prefab.prefab"));

            var material = settings.Material != null
                ? Object.Instantiate(settings.Material)
                : CreateGrassMaterial(settings);
            material.name = safeName + "_Mat";
            ApplyMaterialSettings(material, settings);
            AssetDatabase.CreateAsset(material, materialPath);

            var mesh = BuildCrossedPlaneMesh(settings);
            mesh.name = safeName + "_Mesh";
            AssetDatabase.CreateAsset(mesh, meshPath);

            var root = new GameObject(safeName + "_GrassCard");
            try
            {
                var filter = root.AddComponent<MeshFilter>();
                var renderer = root.AddComponent<MeshRenderer>();
                filter.sharedMesh = mesh;
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return prefabPath;
        }

        private static Texture2D ResolveSelectedTexture()
        {
            if (Selection.activeObject is Texture2D texture)
                return texture;

            if (Selection.activeObject is Material material)
            {
                if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") is Texture2D baseMap)
                    return baseMap;
                if (material.HasProperty("_MainTex") && material.GetTexture("_MainTex") is Texture2D mainTex)
                    return mainTex;
            }

            return null;
        }

        private static Material CreateGrassMaterial(GrassCardSettings settings)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Unlit/Transparent Cutout")
                         ?? Shader.Find("Standard");
            return new Material(shader);
        }

        private static void ApplyMaterialSettings(Material material, GrassCardSettings settings)
        {
            if (material == null)
                return;

            if (settings.Texture != null)
            {
                SetTextureIfExists(material, "_BaseMap", settings.Texture);
                SetTextureIfExists(material, "_MainTex", settings.Texture);
            }

            SetColorIfExists(material, "_BaseColor", settings.Tint);
            SetColorIfExists(material, "_Color", settings.Tint);
            SetFloatIfExists(material, "_Cutoff", settings.AlphaClip);
            SetFloatIfExists(material, "_AlphaClip", 1f);
            SetFloatIfExists(material, "_Surface", 0f);
            SetFloatIfExists(material, "_Cull", settings.DoubleSided ? 0f : 2f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)RenderQueue.AlphaTest;
        }

        private static Mesh BuildCrossedPlaneMesh(GrassCardSettings settings)
        {
            int planes = Mathf.Clamp(settings.CrossedPlanes, 2, 4);
            float width = Mathf.Max(0.01f, settings.Width);
            float height = Mathf.Max(0.01f, settings.Height);
            float halfWidth = width * 0.5f;
            int verticesPerPlane = 4;
            int trianglesPerPlane = settings.DoubleSided ? 12 : 6;

            var vertices = new Vector3[planes * verticesPerPlane];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[planes * trianglesPerPlane];

            int tri = 0;
            for (int p = 0; p < planes; p++)
            {
                float angle = Mathf.PI * p / planes;
                var right = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * halfWidth;
                int v = p * verticesPerPlane;

                vertices[v + 0] = -right;
                vertices[v + 1] = right;
                vertices[v + 2] = -right + Vector3.up * height;
                vertices[v + 3] = right + Vector3.up * height;

                uvs[v + 0] = new Vector2(0f, 0f);
                uvs[v + 1] = new Vector2(1f, 0f);
                uvs[v + 2] = new Vector2(0f, 1f);
                uvs[v + 3] = new Vector2(1f, 1f);

                triangles[tri++] = v + 0;
                triangles[tri++] = v + 2;
                triangles[tri++] = v + 1;
                triangles[tri++] = v + 1;
                triangles[tri++] = v + 2;
                triangles[tri++] = v + 3;

                if (settings.DoubleSided)
                {
                    triangles[tri++] = v + 1;
                    triangles[tri++] = v + 2;
                    triangles[tri++] = v + 0;
                    triangles[tri++] = v + 3;
                    triangles[tri++] = v + 2;
                    triangles[tri++] = v + 1;
                }
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void SetTextureIfExists(Material material, string property, Texture texture)
        {
            if (material.HasProperty(property))
                material.SetTexture(property, texture);
        }

        private static void SetColorIfExists(Material material, string property, Color color)
        {
            if (material.HasProperty(property))
                material.SetColor(property, color);
        }

        private static void SetFloatIfExists(Material material, string property, float value)
        {
            if (material.HasProperty(property))
                material.SetFloat(property, value);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        private static string MakeSafeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');
            return value.Trim();
        }
    }
}
