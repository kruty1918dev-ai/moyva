using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Editor.ObjectPlacement
{
    public enum GrassCardSizeMode
    {
        HeightKeepsTextureAspect,
        WidthKeepsTextureAspect,
        TexturePixelsPerUnit,
        Manual
    }

    public enum GrassCardContactShadowMode
    {
        MeshFootprint = 0,
        UvCardBlob = 1
    }

    [CreateAssetMenu(fileName = "GrassCardPreset", menuName = "Moyva/Generator/Grass Card Preset", order = 2200)]
    public sealed class GrassCardPrefabPreset : ScriptableObject
    {
        public Texture2D Texture;
        public Shader MaterialShader;
        public string OutputFolder = "Assets/Moyva/Generated/Grass";

        [Range(1, 8)] public int CrossedPlanes = 3;
        public GrassCardGeometryMode GeometryMode = GrassCardGeometryMode.CrossedPlanes;
        public GrassCardSizeMode SizeMode = GrassCardSizeMode.HeightKeepsTextureAspect;
        [Min(0.01f)] public float Width = 0.7f;
        [Min(0.01f)] public float Height = 0.9f;
        [Min(1f)] public float PixelsPerUnit = 100f;
        public bool DoubleSided = true;

        public Color Tint = Color.white;
        [Range(0f, 1f)] public float AlphaClip = 0.35f;
        public Vector2 TextureFill = Vector2.one;
        public Vector2 TextureFillOffset = Vector2.zero;
        [Range(0f, 1f)] public float TextureFitClamp = 1f;

        [Range(0f, 1f)] public float TextureVolumeStrength = 0.35f;
        [Range(0f, 1f)] public float TextureVolumeRoundness = 0.62f;
        public Color TextureVolumeLightColor = new Color(1f, 0.96f, 0.82f, 1f);
        public Color TextureVolumeShadowColor = new Color(0.52f, 0.56f, 0.42f, 1f);
        public Vector2 TextureVolumeDirection = new Vector2(-0.45f, 0.75f);

        public Color OutlineColor = Color.black;
        [Range(0f, 1f)] public float OutlineStrength = 1f;
        [Range(0f, 12f)] public float OutlineScreenWidthPx = 1.5f;

        public GrassCardContactShadowMode ContactShadowMode = GrassCardContactShadowMode.UvCardBlob;
        public Color ContactColor = new Color(0.04f, 0.035f, 0.03f, 1f);
        [Range(0f, 1f)] public float ContactShadowStrength = 1f;
        public Vector2 ContactBlobAspect = new Vector2(1.25f, 0.55f);
        [Range(0f, 1f)] public float ContactDarkness = 0.09f;
        [Range(0.01f, 5f)] public float ContactRadius = 0.46f;
        [Range(0.01f, 1f)] public float ContactSoftness = 0.70f;
        [Range(-2f, 2f)] public float ContactCameraBackOffset = 0.10f;
    }

    public static class GrassCardPrefabFactory
    {
        private const string DefaultFolder = "Assets/Moyva/Generated/Grass";
        private const string DefaultPresetFolder = "Assets/Moyva/Generated/GrassCardPresets";
        private const string DecorShaderName = "Moyva/3D/Decor Shared Stylized";
        private const string PreviewShaderName = "Hidden/Moyva/Grass Card Preview";

        [MenuItem("Moyva/Generator/Object Placement/Create Grass Card Prefab From Selection", priority = 2200)]
        public static void CreateGrassCardPrefabFromSelection()
        {
            GrassCardPrefabWizardWindow.OpenFromSelection();
        }

        [MenuItem("Moyva/Generator/Object Placement/Create Grass Card Prefab From Selection", true)]
        private static bool ValidateCreateGrassCardPrefabFromSelection()
        {
            return true;
        }

        public static void OpenGrassCardPrefabWizard(
            string name,
            GrassCardSettings settings,
            string folder,
            System.Action<string> onGenerated = null)
        {
            GrassCardPrefabWizardWindow.Open(name, settings, folder, onGenerated);
        }

        public static string CreateGrassCardPrefab(
            string name,
            GrassCardSettings settings,
            string folder)
        {
            return CreateGrassCardPrefab(
                name,
                settings,
                folder,
                GrassCardMaterialSettings.CreateDefault(settings),
                null);
        }

        private static string CreateGrassCardPrefab(
            string name,
            GrassCardSettings settings,
            string folder,
            GrassCardMaterialSettings materialSettings,
            Shader materialShader)
        {
            settings ??= new GrassCardSettings();
            string safeName = MakeSafeFileName(string.IsNullOrWhiteSpace(name) ? "GrassCard" : name);
            folder = NormalizeAssetFolder(folder);
            EnsureFolder(folder);

            string materialPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, safeName + "_Mat.mat"));
            string meshPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, safeName + "_Mesh.asset"));
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(folder, safeName + "_Prefab.prefab"));

            var material = CreateGrassMaterial(materialShader);
            if (material == null)
            {
                Debug.LogError("Grass Card prefab generation failed: no compatible material or shader found.");
                return null;
            }

            material.name = safeName + "_Mat";
            ApplyMaterialSettings(material, settings, materialSettings);
            AssetDatabase.CreateAsset(material, materialPath);

            var mesh = BuildGrassCardMesh(settings);
            mesh.name = safeName + "_Mesh";
            AssetDatabase.CreateAsset(mesh, meshPath);

            var root = new GameObject(safeName + "_GrassCard");
            try
            {
                var filter = root.AddComponent<MeshFilter>();
                var renderer = root.AddComponent<MeshRenderer>();
                filter.sharedMesh = mesh;
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = IsBillboard(settings) || settings.DoubleSided
                    ? ShadowCastingMode.TwoSided
                    : ShadowCastingMode.On;
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

        private static GrassCardSettings CreateDefaultSettings(Texture2D texture)
        {
            float height = 0.9f;
            float aspect = texture != null && texture.height > 0 ? texture.width / (float)texture.height : 0.7f / height;
            return new GrassCardSettings
            {
                Texture = texture,
                Tint = Color.white,
                AlphaClip = 0.35f,
                CrossedPlanes = 3,
                GeometryMode = GrassCardGeometryMode.CrossedPlanes,
                Width = Mathf.Max(0.01f, height * aspect),
                Height = height,
                DoubleSided = true
            };
        }

        private static Texture2D ResolveSelectedTexture()
        {
            if (Selection.activeObject is Texture2D texture)
                return texture;

            return null;
        }

        private static bool IsBillboard(GrassCardSettings settings)
        {
            return settings != null && settings.GeometryMode == GrassCardGeometryMode.CameraBillboard;
        }

        private static Material CreateGrassMaterial(Shader requestedShader = null)
        {
            var shader = ResolveGrassShader(requestedShader);
            return shader != null ? new Material(shader) : null;
        }

        private static Material CreatePreviewMaterial()
        {
            Shader shader = FindUsableShader(PreviewShaderName)
                            ?? FindUsableShader("Unlit/Transparent Cutout")
                            ?? FindUsableShader("Sprites/Default")
                            ?? FindUsableShader("Universal Render Pipeline/Unlit")
                            ?? FindUsableShader("Standard");
            return shader != null ? new Material(shader) : null;
        }

        private static Shader ResolveGrassShader(Shader requestedShader)
        {
            if (IsUsableShader(requestedShader))
                return requestedShader;

            return FindUsableShader(DecorShaderName)
                   ?? FindUsableShader("Universal Render Pipeline/Unlit")
                   ?? FindUsableShader("Universal Render Pipeline/Lit")
                   ?? FindUsableShader("Unlit/Transparent Cutout")
                   ?? FindUsableShader("Standard");
        }

        private static Shader FindUsableShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            return IsUsableShader(shader) ? shader : null;
        }

        private static bool IsUsableShader(Shader shader)
        {
            return shader != null && shader.isSupported;
        }

        private static void ApplyMaterialSettings(
            Material material,
            GrassCardSettings settings,
            GrassCardMaterialSettings materialSettings)
        {
            if (material == null)
                return;

            bool billboard = IsBillboard(settings);

            if (settings.Texture != null)
            {
                SetTextureIfExists(material, "_BaseMap", settings.Texture);
                SetTextureIfExists(material, "_MainTex", settings.Texture);
            }

            SetColorIfExists(material, "_BaseColor", settings.Tint);
            SetColorIfExists(material, "_Color", settings.Tint);
            SetFloatIfExists(material, "_Alpha", settings.Tint.a);
            SetFloatIfExists(material, "_AlphaClipEnabled", 1f);
            SetFloatIfExists(material, "_AlphaClipThreshold", settings.AlphaClip);
            SetFloatIfExists(material, "_Cutoff", settings.AlphaClip);
            SetFloatIfExists(material, "_AlphaClip", 1f);
            SetFloatIfExists(material, "_Surface", 0f);
            SetFloatIfExists(material, "_BillboardEnabled", billboard ? 1f : 0f);
            SetFloatIfExists(material, "_Cull", settings.DoubleSided || billboard ? 0f : 2f);
            SetFloatIfExists(material, "_CullMode", settings.DoubleSided || billboard ? (float)CullMode.Off : (float)CullMode.Back);
            SetFloatIfExists(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfExists(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfExists(material, "_ZWrite", 1f);

            SetVectorIfExists(material, "_TextureFill", ToVector4(ClampVector2(materialSettings.TextureFill, 0.05f, 5f)));
            SetVectorIfExists(material, "_TextureFillOffset", ToVector4(materialSettings.TextureFillOffset));
            SetFloatIfExists(material, "_TextureFitClamp", Mathf.Clamp01(materialSettings.TextureFitClamp));
            SetFloatIfExists(material, "_TextureVolumeStrength", Mathf.Clamp01(materialSettings.TextureVolumeStrength));
            SetFloatIfExists(material, "_TextureVolumeRoundness", Mathf.Clamp01(materialSettings.TextureVolumeRoundness));
            SetColorIfExists(material, "_TextureVolumeLightColor", materialSettings.TextureVolumeLightColor);
            SetColorIfExists(material, "_TextureVolumeShadowColor", materialSettings.TextureVolumeShadowColor);
            SetVectorIfExists(material, "_TextureVolumeDirection", ToVector4(materialSettings.TextureVolumeDirection));

            SetColorIfExists(material, "_OutlineColor", materialSettings.OutlineColor);
            SetFloatIfExists(material, "_OutlineEnabled", Mathf.Clamp01(materialSettings.OutlineStrength));
            SetFloatIfExists(material, "_OutlineScreenWidthPx", Mathf.Clamp(materialSettings.OutlineScreenWidthPx, 0f, 12f));
            SetFloatIfExists(material, "_LeafPlaneShading", 1f);

            GrassCardContactShadowMode contactShadowMode = billboard
                ? GrassCardContactShadowMode.UvCardBlob
                : materialSettings.ContactShadowMode;
            SetFloatIfExists(material, "_ContactBlobMode", (float)contactShadowMode);
            SetColorIfExists(material, "_ContactColor", materialSettings.ContactColor);
            SetFloatIfExists(material, "_ContactShadowEnabled", Mathf.Clamp01(materialSettings.ContactShadowStrength));
            SetVectorIfExists(material, "_ContactBlobAspect", ToVector4(ClampVector2(materialSettings.ContactBlobAspect, 0.05f, 5f)));
            SetFloatIfExists(material, "_ContactDarkness", Mathf.Clamp01(materialSettings.ContactDarkness));
            SetFloatIfExists(material, "_ContactRadius", Mathf.Clamp(materialSettings.ContactRadius, 0.01f, 5f));
            SetFloatIfExists(material, "_ContactSoftness", Mathf.Clamp(materialSettings.ContactSoftness, 0.01f, 1f));
            SetFloatIfExists(material, "_ContactCameraBackOffset", Mathf.Clamp(materialSettings.ContactCameraBackOffset, -2f, 2f));

            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)RenderQueue.AlphaTest + 40;
        }

        private static void ApplyPreviewMaterialSettings(
            Material material,
            GrassCardSettings settings,
            GrassCardMaterialSettings materialSettings)
        {
            if (material == null)
                return;

            bool billboard = IsBillboard(settings);

            if (settings.Texture != null)
            {
                SetTextureIfExists(material, "_BaseMap", settings.Texture);
                SetTextureIfExists(material, "_MainTex", settings.Texture);
            }

            SetColorIfExists(material, "_BaseColor", settings.Tint);
            SetColorIfExists(material, "_Color", settings.Tint);
            SetFloatIfExists(material, "_Alpha", settings.Tint.a);
            SetFloatIfExists(material, "_AlphaClipThreshold", settings.AlphaClip);
            SetFloatIfExists(material, "_Cutoff", settings.AlphaClip);
            SetVectorIfExists(material, "_TextureFill", ToVector4(ClampVector2(materialSettings.TextureFill, 0.05f, 5f)));
            SetVectorIfExists(material, "_TextureFillOffset", ToVector4(materialSettings.TextureFillOffset));
            SetFloatIfExists(material, "_TextureFitClamp", Mathf.Clamp01(materialSettings.TextureFitClamp));
            SetFloatIfExists(material, "_BillboardEnabled", billboard ? 1f : 0f);
            SetFloatIfExists(material, "_Cull", 0f);
            SetFloatIfExists(material, "_CullMode", 0f);
            SetFloatIfExists(material, "_ZWrite", 1f);

            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.AlphaTest;
        }

        private static Mesh BuildGrassCardMesh(GrassCardSettings settings)
        {
            return IsBillboard(settings)
                ? BuildBillboardPlaneMesh(settings)
                : BuildCrossedPlaneMesh(settings);
        }

        private static Mesh BuildBillboardPlaneMesh(GrassCardSettings settings)
        {
            float width = Mathf.Max(0.01f, settings.Width);
            float height = Mathf.Max(0.01f, settings.Height);
            float halfWidth = width * 0.5f;

            var vertices = new List<Vector3>(4);
            var normals = new List<Vector3>(4);
            var uvs = new List<Vector2>(4);
            var triangles = new List<int>(6);
            AddPlaneFace(vertices, normals, uvs, triangles, Vector3.right * halfWidth, Vector3.forward, 0f, height, false);

            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildCrossedPlaneMesh(GrassCardSettings settings)
        {
            int planes = Mathf.Clamp(settings.CrossedPlanes, 1, 8);
            float width = Mathf.Max(0.01f, settings.Width);
            float height = Mathf.Max(0.01f, settings.Height);
            float halfWidth = width * 0.5f;

            int sides = settings.DoubleSided ? 2 : 1;
            var vertices = new List<Vector3>(planes * 4 * sides);
            var normals = new List<Vector3>(planes * 4 * sides);
            var uvs = new List<Vector2>(planes * 4 * sides);
            var triangles = new List<int>(planes * 6 * sides);

            for (int p = 0; p < planes; p++)
            {
                float angle = Mathf.PI * p / planes;
                var right = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * halfWidth;
                var normal = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)).normalized;

                AddPlaneFace(vertices, normals, uvs, triangles, right, normal, 0f, height, false);
                if (settings.DoubleSided)
                    AddPlaneFace(vertices, normals, uvs, triangles, right, -normal, 0f, height, true);
            }

            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddPlaneFace(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 right,
            Vector3 normal,
            float yMin,
            float yMax,
            bool backFace)
        {
            int start = vertices.Count;
            vertices.Add(-right + Vector3.up * yMin);
            vertices.Add(right + Vector3.up * yMin);
            vertices.Add(right + Vector3.up * yMax);
            vertices.Add(-right + Vector3.up * yMax);

            for (int i = 0; i < 4; i++)
                normals.Add(normal);

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));

            if (backFace)
            {
                triangles.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
                return;
            }

            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        private struct GrassCardMaterialSettings
        {
            public Vector2 TextureFill;
            public Vector2 TextureFillOffset;
            public float TextureFitClamp;
            public float TextureVolumeStrength;
            public float TextureVolumeRoundness;
            public Color TextureVolumeLightColor;
            public Color TextureVolumeShadowColor;
            public Vector2 TextureVolumeDirection;
            public Color OutlineColor;
            public float OutlineStrength;
            public float OutlineScreenWidthPx;
            public GrassCardContactShadowMode ContactShadowMode;
            public Color ContactColor;
            public float ContactShadowStrength;
            public Vector2 ContactBlobAspect;
            public float ContactDarkness;
            public float ContactRadius;
            public float ContactSoftness;
            public float ContactCameraBackOffset;

            public static GrassCardMaterialSettings CreateDefault(GrassCardSettings settings)
            {
                return new GrassCardMaterialSettings
                {
                    TextureFill = Vector2.one,
                    TextureFillOffset = Vector2.zero,
                    TextureFitClamp = 1f,
                    TextureVolumeStrength = 0.35f,
                    TextureVolumeRoundness = 0.62f,
                    TextureVolumeLightColor = new Color(1f, 0.96f, 0.82f, 1f),
                    TextureVolumeShadowColor = new Color(0.52f, 0.56f, 0.42f, 1f),
                    TextureVolumeDirection = new Vector2(-0.45f, 0.75f),
                    OutlineColor = Color.black,
                    OutlineStrength = 1f,
                    OutlineScreenWidthPx = 1.5f,
                    ContactShadowMode = GrassCardContactShadowMode.UvCardBlob,
                    ContactColor = new Color(0.04f, 0.035f, 0.03f, 1f),
                    ContactShadowStrength = 1f,
                    ContactBlobAspect = new Vector2(1.25f, 0.55f),
                    ContactDarkness = 0.09f,
                    ContactRadius = 0.46f,
                    ContactSoftness = 0.70f,
                    ContactCameraBackOffset = 0.10f
                };
            }
        }

        private sealed class GrassCardPrefabWizardWindow : EditorWindow
        {
            private GrassCardPrefabPreset _preset;
            private Texture2D _texture;
            private Shader _materialShader;
            private string _assetName = "GrassCard";
            private string _outputFolder = DefaultFolder;
            private string _presetFolder = DefaultPresetFolder;
            private int _crossedPlanes = 3;
            private GrassCardGeometryMode _geometryMode = GrassCardGeometryMode.CrossedPlanes;
            private GrassCardSizeMode _sizeMode = GrassCardSizeMode.HeightKeepsTextureAspect;
            private float _width = 0.7f;
            private float _height = 0.9f;
            private float _pixelsPerUnit = 100f;
            private bool _doubleSided = true;
            private Color _tint = Color.white;
            private float _alphaClip = 0.35f;
            private GrassCardMaterialSettings _materialSettings;
            private Vector2 _scroll;
            private float _previewYaw = -25f;
            private PreviewRenderUtility _previewUtility;
            private Mesh _previewMesh;
            private Material _previewMaterial;
            private Object _lastGenerated;
            private string _lastMessage;
            private MessageType _lastMessageType = MessageType.Info;
            private System.Action<string> _onGenerated;

            private bool IsBillboardMode => _geometryMode == GrassCardGeometryMode.CameraBillboard;

            public static void OpenFromSelection()
            {
                var window = GetWindow<GrassCardPrefabWizardWindow>("Grass Card Prefab");
                window.minSize = new Vector2(440f, 680f);
                window.InitializeFromSelection();
                window.Show();
            }

            public static void Open(
                string name,
                GrassCardSettings settings,
                string folder,
                System.Action<string> onGenerated)
            {
                var window = GetWindow<GrassCardPrefabWizardWindow>("Grass Card Prefab");
                window.minSize = new Vector2(440f, 680f);
                window.InitializeFromSettings(name, settings, folder, onGenerated);
                window.Show();
            }

            private void OnEnable()
            {
                if (_materialSettings.TextureFill == Vector2.zero)
                    _materialSettings = GrassCardMaterialSettings.CreateDefault(null);
                if (_materialShader == null)
                    _materialShader = ResolveGrassShader(null);
            }

            private void OnDisable()
            {
                DestroyPreviewAssets();
                _previewUtility?.Cleanup();
                _previewUtility = null;
            }

            private void OnGUI()
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.LabelField("Create Grass Card Prefab", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Configure crossed-plane grass/decor cards before generating mesh, material and prefab assets.",
                    MessageType.Info);

                DrawPresetSection();
                DrawSourceSection();
                DrawGeometrySection();
                DrawMaterialSection();
                DrawPreviewSection();
                DrawOutputSection();
                DrawActions();
                DrawLastResult();

                if (EditorGUI.EndChangeCheck())
                    Repaint();
                EditorGUILayout.EndScrollView();
            }

            private void InitializeFromSelection()
            {
                _onGenerated = null;
                _texture = ResolveSelectedTexture();
                _materialShader ??= ResolveGrassShader(null);
                if (_texture != null)
                {
                    _assetName = MakeSafeFileName(_texture.name);
                    ApplyTextureAspectDefault();
                }
            }

            private void InitializeFromSettings(
                string name,
                GrassCardSettings settings,
                string folder,
                System.Action<string> onGenerated)
            {
                settings ??= new GrassCardSettings();
                _onGenerated = onGenerated;
                _materialSettings = GrassCardMaterialSettings.CreateDefault(settings);
                _texture = settings.Texture;
                _materialShader = ResolveGrassShader(_materialShader);
                _outputFolder = string.IsNullOrWhiteSpace(folder) ? DefaultFolder : NormalizeAssetFolder(folder);
                _crossedPlanes = Mathf.Clamp(settings.CrossedPlanes <= 0 ? 3 : settings.CrossedPlanes, 1, 8);
                _geometryMode = settings.GeometryMode;
                _width = Mathf.Max(0.01f, settings.Width);
                _height = Mathf.Max(0.01f, settings.Height);
                _doubleSided = settings.DoubleSided;
                _tint = settings.Tint;
                _alphaClip = Mathf.Clamp01(settings.AlphaClip);

                _assetName = MakeSafeFileName(!string.IsNullOrWhiteSpace(name)
                    ? name
                    : _texture != null
                        ? _texture.name
                        : "GrassCard");
                ResolveSize();
                _lastMessage = "Grass Card settings loaded from graph node. Adjust and generate when ready.";
                _lastMessageType = MessageType.Info;
            }

            private void DrawPresetSection()
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
                _preset = (GrassCardPrefabPreset)EditorGUILayout.ObjectField(
                    new GUIContent("Preset", "Reusable Grass Card setup for quick creation of similar objects."),
                    _preset,
                    typeof(GrassCardPrefabPreset),
                    false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_preset == null))
                    {
                        if (GUILayout.Button("Load All"))
                            ApplyPreset(_preset, keepCurrentSource: false);
                        if (GUILayout.Button("Load Settings Only"))
                            ApplyPreset(_preset, keepCurrentSource: true);
                        if (GUILayout.Button("Save"))
                            SavePreset(saveAsNew: false);
                    }

                    if (GUILayout.Button("Save As New"))
                        SavePreset(saveAsNew: true);
                }

                _presetFolder = EditorGUILayout.TextField(
                    new GUIContent("Preset Folder", "Folder for newly created Grass Card presets."),
                    string.IsNullOrWhiteSpace(_presetFolder) ? DefaultPresetFolder : _presetFolder);
            }

            private void DrawSourceSection()
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                _texture = (Texture2D)EditorGUILayout.ObjectField(
                    new GUIContent("Texture", "Texture used by the generated card material."),
                    _texture,
                    typeof(Texture2D),
                    false);
                if (EditorGUI.EndChangeCheck() && _texture != null)
                {
                    _assetName = MakeSafeFileName(_texture.name);
                    ApplyTextureAspectDefault();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Load From Selection"))
                        InitializeFromSelection();
                }

                EditorGUILayout.HelpBox(
                    "Only texture and settings are inputs. The material, mesh and prefab are generated automatically.",
                    MessageType.None);

                _tint = EditorGUILayout.ColorField(
                    new GUIContent("Tint", "Base color written to the generated material."),
                    _tint);
                _alphaClip = EditorGUILayout.Slider(
                    new GUIContent("Alpha Clip", "Cutout threshold for the grass texture."),
                    _alphaClip,
                    0f,
                    1f);
            }

            private void DrawGeometrySection()
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                _geometryMode = (GrassCardGeometryMode)EditorGUILayout.EnumPopup(
                    new GUIContent("Geometry Mode", "Crossed Planes keeps the current multi-plane card. Camera Billboard generates a single quad and makes the shader face the camera."),
                    _geometryMode);
                if (EditorGUI.EndChangeCheck() && IsBillboardMode)
                    _materialSettings.ContactShadowMode = GrassCardContactShadowMode.UvCardBlob;

                using (new EditorGUI.DisabledScope(IsBillboardMode))
                {
                    _crossedPlanes = EditorGUILayout.IntSlider(
                        new GUIContent("Plane Count / Detail", "1 = single plane, more planes add rotational fullness but more geometry."),
                        Mathf.Clamp(_crossedPlanes, 1, 8),
                        1,
                        8);
                }

                _sizeMode = (GrassCardSizeMode)EditorGUILayout.EnumPopup(
                    new GUIContent("Size Mode", "How the generated mesh size is derived from the texture."),
                    _sizeMode);

                using (new EditorGUI.DisabledScope(_sizeMode == GrassCardSizeMode.HeightKeepsTextureAspect || _sizeMode == GrassCardSizeMode.TexturePixelsPerUnit))
                {
                    _width = Mathf.Max(0.01f, EditorGUILayout.FloatField("Width", _width));
                }

                using (new EditorGUI.DisabledScope(_sizeMode == GrassCardSizeMode.WidthKeepsTextureAspect || _sizeMode == GrassCardSizeMode.TexturePixelsPerUnit))
                {
                    _height = Mathf.Max(0.01f, EditorGUILayout.FloatField("Height", _height));
                }

                using (new EditorGUI.DisabledScope(_sizeMode != GrassCardSizeMode.TexturePixelsPerUnit))
                {
                    _pixelsPerUnit = Mathf.Max(1f, EditorGUILayout.FloatField("Pixels Per Unit", _pixelsPerUnit));
                }

                using (new EditorGUI.DisabledScope(IsBillboardMode))
                {
                    _doubleSided = EditorGUILayout.Toggle(
                        new GUIContent("Double Sided Geometry", "Adds real back faces with correct normals."),
                        _doubleSided);
                }

                Vector2 size = ResolveSize();
                string geometrySummary = IsBillboardMode
                    ? "camera billboard quad"
                    : $"{Mathf.Clamp(_crossedPlanes, 1, 8)} crossed plane(s)";
                EditorGUILayout.HelpBox(
                    $"Result mesh: {size.x:0.###} x {size.y:0.###} units, {geometrySummary}.",
                    MessageType.None);

                if (_texture != null && _sizeMode == GrassCardSizeMode.Manual)
                {
                    float textureLongest = Mathf.Max(_texture.width, _texture.height) / Mathf.Max(1f, _pixelsPerUnit);
                    if (Mathf.Max(size.x, size.y) > textureLongest * 2f)
                    {
                        EditorGUILayout.HelpBox(
                            "Manual mesh size is much larger than texture-pixels-per-unit size. Use Texture Pixels Per Unit or reduce Width/Height to avoid tiny visible texture content.",
                            MessageType.Warning);
                    }
                }
            }

            private void DrawMaterialSection()
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Material Look", EditorStyles.boldLabel);

                _materialShader = (Shader)EditorGUILayout.ObjectField(
                    new GUIContent("Generated Material Shader", "Shader used for the generated material. Mesh/material/prefab are still created automatically."),
                    _materialShader,
                    typeof(Shader),
                    false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Decor"))
                        _materialShader = Shader.Find(DecorShaderName);
                    if (GUILayout.Button("URP Unlit"))
                        _materialShader = Shader.Find("Universal Render Pipeline/Unlit");
                    if (GUILayout.Button("URP Lit"))
                        _materialShader = Shader.Find("Universal Render Pipeline/Lit");
                    if (GUILayout.Button("Auto"))
                        _materialShader = ResolveGrassShader(null);
                }

                Shader resolvedShader = ResolveGrassShader(_materialShader);
                if (resolvedShader == null)
                {
                    EditorGUILayout.HelpBox("No compatible shader found. Preview and generation cannot create a valid material.", MessageType.Error);
                }
                else if (_materialShader != null && resolvedShader != _materialShader)
                {
                    EditorGUILayout.HelpBox(
                        $"Selected shader is unsupported here, so preview/generation will use '{resolvedShader.name}'.",
                        MessageType.Warning);
                }

                EditorGUILayout.HelpBox(
                    "Preview uses an editor-safe preview shader to avoid magenta preview. The generated material uses the shader selected above.",
                    MessageType.None);

                DrawVector2Field(ref _materialSettings.TextureFill, "Texture Fill XY", 0.05f, 5f);
                _materialSettings.TextureFillOffset = EditorGUILayout.Vector2Field("Texture Offset XY", _materialSettings.TextureFillOffset);
                _materialSettings.TextureFitClamp = EditorGUILayout.Slider("Clamp Outside Fill", _materialSettings.TextureFitClamp, 0f, 1f);

                EditorGUILayout.Space(4f);
                _materialSettings.TextureVolumeStrength = EditorGUILayout.Slider("Volume Strength", _materialSettings.TextureVolumeStrength, 0f, 1f);
                _materialSettings.TextureVolumeRoundness = EditorGUILayout.Slider("Volume Roundness", _materialSettings.TextureVolumeRoundness, 0f, 1f);
                _materialSettings.TextureVolumeLightColor = EditorGUILayout.ColorField("Volume Light Color", _materialSettings.TextureVolumeLightColor);
                _materialSettings.TextureVolumeShadowColor = EditorGUILayout.ColorField("Volume Shadow Color", _materialSettings.TextureVolumeShadowColor);
                _materialSettings.TextureVolumeDirection = EditorGUILayout.Vector2Field("Volume Direction UV", _materialSettings.TextureVolumeDirection);

                EditorGUILayout.Space(4f);
                _materialSettings.OutlineColor = EditorGUILayout.ColorField("Outline Color", _materialSettings.OutlineColor);
                _materialSettings.OutlineStrength = EditorGUILayout.Slider("Outline Strength", _materialSettings.OutlineStrength, 0f, 1f);
                _materialSettings.OutlineScreenWidthPx = EditorGUILayout.Slider("Outline Width Px", _materialSettings.OutlineScreenWidthPx, 0f, 12f);

                EditorGUILayout.Space(4f);
                if (IsBillboardMode)
                    _materialSettings.ContactShadowMode = GrassCardContactShadowMode.UvCardBlob;
                using (new EditorGUI.DisabledScope(IsBillboardMode))
                {
                    _materialSettings.ContactShadowMode = (GrassCardContactShadowMode)EditorGUILayout.EnumPopup(
                        "Contact Shadow Mode",
                        _materialSettings.ContactShadowMode);
                }
                _materialSettings.ContactColor = EditorGUILayout.ColorField("Contact Color", _materialSettings.ContactColor);
                _materialSettings.ContactShadowStrength = EditorGUILayout.Slider("Contact Shadow Strength", _materialSettings.ContactShadowStrength, 0f, 1f);
                DrawVector2Field(ref _materialSettings.ContactBlobAspect, "Contact Blob Aspect XZ", 0.05f, 5f);
                _materialSettings.ContactDarkness = EditorGUILayout.Slider("Contact Darkness", _materialSettings.ContactDarkness, 0f, 1f);
                _materialSettings.ContactRadius = EditorGUILayout.Slider("Contact Radius", _materialSettings.ContactRadius, 0.01f, 5f);
                _materialSettings.ContactSoftness = EditorGUILayout.Slider("Contact Softness", _materialSettings.ContactSoftness, 0.01f, 1f);
                _materialSettings.ContactCameraBackOffset = EditorGUILayout.Slider("Contact Camera Back Offset", _materialSettings.ContactCameraBackOffset, -2f, 2f);
            }

            private void DrawPreviewSection()
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                _previewYaw = EditorGUILayout.Slider(
                    new GUIContent("Preview Yaw", "Orbits the preview camera around the generated mesh."),
                    _previewYaw,
                    -180f,
                    180f);

                Rect rect = GUILayoutUtility.GetRect(10f, 260f, GUILayout.ExpandWidth(true));
                if (Event.current.type == EventType.Repaint)
                    RenderPreview(rect);
            }

            private void DrawOutputSection()
            {
                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
                _assetName = EditorGUILayout.TextField("Asset Name", _assetName);
                _outputFolder = EditorGUILayout.TextField(
                    new GUIContent("Output Folder", "Mesh, material and prefab will be created here."),
                    string.IsNullOrWhiteSpace(_outputFolder) ? DefaultFolder : _outputFolder);
            }

            private void DrawActions()
            {
                EditorGUILayout.Space(8f);
                if (GUILayout.Button("Generate Mesh + Material + Prefab", GUILayout.Height(30f)))
                    GeneratePrefab();
            }

            private void DrawLastResult()
            {
                if (string.IsNullOrEmpty(_lastMessage))
                    return;

                EditorGUILayout.Space(8f);
                EditorGUILayout.HelpBox(_lastMessage, _lastMessageType);
                if (_lastGenerated != null && GUILayout.Button("Ping Last Generated Asset"))
                {
                    Selection.activeObject = _lastGenerated;
                    EditorGUIUtility.PingObject(_lastGenerated);
                }
            }

            private void GeneratePrefab()
            {
                if (_texture == null)
                {
                    _lastMessage = "Texture is required before generating a grass card prefab.";
                    _lastMessageType = MessageType.Error;
                    return;
                }

                string folder = NormalizeAssetFolder(_outputFolder);
                if (!IsAssetFolder(folder))
                {
                    _lastMessage = "Output Folder must be inside Assets.";
                    _lastMessageType = MessageType.Error;
                    return;
                }

                GrassCardSettings settings = BuildSettings();
                string prefabPath = CreateGrassCardPrefab(_assetName, settings, folder, _materialSettings, _materialShader);
                if (string.IsNullOrEmpty(prefabPath))
                {
                    _lastMessage = "Could not generate prefab because no compatible material or shader was found.";
                    _lastMessageType = MessageType.Error;
                    return;
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                _lastGenerated = prefab;
                _lastMessage = $"Generated grass card prefab: {prefabPath}";
                _lastMessageType = MessageType.Info;
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                _onGenerated?.Invoke(prefabPath);
            }

            private GrassCardSettings BuildSettings()
            {
                Vector2 size = ResolveSize();
                return new GrassCardSettings
                {
                    Texture = _texture,
                    Tint = _tint,
                    AlphaClip = _alphaClip,
                    CrossedPlanes = IsBillboardMode ? 1 : Mathf.Clamp(_crossedPlanes, 1, 8),
                    GeometryMode = _geometryMode,
                    Width = size.x,
                    Height = size.y,
                    DoubleSided = IsBillboardMode || _doubleSided
                };
            }

            private void ApplyPreset(GrassCardPrefabPreset preset, bool keepCurrentSource)
            {
                if (preset == null)
                    return;

                Texture2D currentTexture = _texture;
                Shader currentShader = _materialShader;
                string currentName = _assetName;

                _texture = preset.Texture;
                _materialShader = ResolveGrassShader(preset.MaterialShader);
                _outputFolder = string.IsNullOrWhiteSpace(preset.OutputFolder) ? DefaultFolder : preset.OutputFolder;
                _crossedPlanes = Mathf.Clamp(preset.CrossedPlanes, 1, 8);
                _geometryMode = preset.GeometryMode;
                _sizeMode = preset.SizeMode;
                _width = Mathf.Max(0.01f, preset.Width);
                _height = Mathf.Max(0.01f, preset.Height);
                _pixelsPerUnit = Mathf.Max(1f, preset.PixelsPerUnit);
                _doubleSided = preset.DoubleSided;
                _tint = preset.Tint;
                _alphaClip = preset.AlphaClip;
                _materialSettings = ReadPresetMaterialSettings(preset);

                if (keepCurrentSource)
                {
                    _texture = currentTexture;
                    _materialShader = currentShader;
                    _assetName = currentName;
                }
                else if (_texture != null)
                {
                    _assetName = MakeSafeFileName(_texture.name);
                }

                _lastMessage = keepCurrentSource
                    ? $"Preset settings loaded from '{preset.name}'."
                    : $"Preset '{preset.name}' loaded.";
                _lastMessageType = MessageType.Info;
            }

            private void SavePreset(bool saveAsNew)
            {
                GrassCardPrefabPreset target = _preset;
                if (saveAsNew || target == null)
                {
                    string folder = NormalizeAssetFolder(_presetFolder);
                    if (!IsAssetFolder(folder))
                    {
                        _lastMessage = "Preset Folder must be inside Assets.";
                        _lastMessageType = MessageType.Error;
                        return;
                    }

                    EnsureFolder(folder);
                    string safeName = MakeSafeFileName(string.IsNullOrWhiteSpace(_assetName) ? "GrassCardPreset" : _assetName);
                    string presetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, safeName + "_Preset.asset"));
                    target = CreateInstance<GrassCardPrefabPreset>();
                    AssetDatabase.CreateAsset(target, presetPath);
                    _preset = target;
                }
                else
                {
                    Undo.RecordObject(target, "Save Grass Card Preset");
                }

                WritePreset(target);
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _lastGenerated = target;
                _lastMessage = $"Grass Card preset '{target.name}' saved.";
                _lastMessageType = MessageType.Info;
                Selection.activeObject = target;
                EditorGUIUtility.PingObject(target);
            }

            private void WritePreset(GrassCardPrefabPreset target)
            {
                target.Texture = _texture;
                target.MaterialShader = _materialShader;
                target.OutputFolder = string.IsNullOrWhiteSpace(_outputFolder) ? DefaultFolder : _outputFolder;
                target.CrossedPlanes = Mathf.Clamp(_crossedPlanes, 1, 8);
                target.GeometryMode = _geometryMode;
                target.SizeMode = _sizeMode;
                target.Width = Mathf.Max(0.01f, _width);
                target.Height = Mathf.Max(0.01f, _height);
                target.PixelsPerUnit = Mathf.Max(1f, _pixelsPerUnit);
                target.DoubleSided = _doubleSided;
                target.Tint = _tint;
                target.AlphaClip = _alphaClip;

                target.TextureFill = ClampVector2(_materialSettings.TextureFill, 0.05f, 5f);
                target.TextureFillOffset = _materialSettings.TextureFillOffset;
                target.TextureFitClamp = _materialSettings.TextureFitClamp;
                target.TextureVolumeStrength = _materialSettings.TextureVolumeStrength;
                target.TextureVolumeRoundness = _materialSettings.TextureVolumeRoundness;
                target.TextureVolumeLightColor = _materialSettings.TextureVolumeLightColor;
                target.TextureVolumeShadowColor = _materialSettings.TextureVolumeShadowColor;
                target.TextureVolumeDirection = _materialSettings.TextureVolumeDirection;
                target.OutlineColor = _materialSettings.OutlineColor;
                target.OutlineStrength = _materialSettings.OutlineStrength;
                target.OutlineScreenWidthPx = _materialSettings.OutlineScreenWidthPx;
                target.ContactShadowMode = _materialSettings.ContactShadowMode;
                target.ContactColor = _materialSettings.ContactColor;
                target.ContactShadowStrength = _materialSettings.ContactShadowStrength;
                target.ContactBlobAspect = ClampVector2(_materialSettings.ContactBlobAspect, 0.05f, 5f);
                target.ContactDarkness = _materialSettings.ContactDarkness;
                target.ContactRadius = _materialSettings.ContactRadius;
                target.ContactSoftness = _materialSettings.ContactSoftness;
                target.ContactCameraBackOffset = _materialSettings.ContactCameraBackOffset;
            }

            private static GrassCardMaterialSettings ReadPresetMaterialSettings(GrassCardPrefabPreset preset)
            {
                return new GrassCardMaterialSettings
                {
                    TextureFill = preset.TextureFill,
                    TextureFillOffset = preset.TextureFillOffset,
                    TextureFitClamp = preset.TextureFitClamp,
                    TextureVolumeStrength = preset.TextureVolumeStrength,
                    TextureVolumeRoundness = preset.TextureVolumeRoundness,
                    TextureVolumeLightColor = preset.TextureVolumeLightColor,
                    TextureVolumeShadowColor = preset.TextureVolumeShadowColor,
                    TextureVolumeDirection = preset.TextureVolumeDirection,
                    OutlineColor = preset.OutlineColor,
                    OutlineStrength = preset.OutlineStrength,
                    OutlineScreenWidthPx = preset.OutlineScreenWidthPx,
                    ContactShadowMode = preset.ContactShadowMode,
                    ContactColor = preset.ContactColor,
                    ContactShadowStrength = preset.ContactShadowStrength,
                    ContactBlobAspect = preset.ContactBlobAspect,
                    ContactDarkness = preset.ContactDarkness,
                    ContactRadius = preset.ContactRadius,
                    ContactSoftness = preset.ContactSoftness,
                    ContactCameraBackOffset = preset.ContactCameraBackOffset
                };
            }

            private Vector2 ResolveSize()
            {
                float aspect = _texture != null && _texture.height > 0 ? _texture.width / (float)_texture.height : 1f;
                switch (_sizeMode)
                {
                    case GrassCardSizeMode.HeightKeepsTextureAspect:
                        _height = Mathf.Max(0.01f, _height);
                        _width = Mathf.Max(0.01f, _height * aspect);
                        break;
                    case GrassCardSizeMode.WidthKeepsTextureAspect:
                        _width = Mathf.Max(0.01f, _width);
                        _height = Mathf.Max(0.01f, _width / Mathf.Max(0.001f, aspect));
                        break;
                    case GrassCardSizeMode.TexturePixelsPerUnit:
                        float ppu = Mathf.Max(1f, _pixelsPerUnit);
                        _width = Mathf.Max(0.01f, (_texture != null ? _texture.width : 100f) / ppu);
                        _height = Mathf.Max(0.01f, (_texture != null ? _texture.height : 100f) / ppu);
                        break;
                    case GrassCardSizeMode.Manual:
                        _width = Mathf.Max(0.01f, _width);
                        _height = Mathf.Max(0.01f, _height);
                        break;
                }

                return new Vector2(_width, _height);
            }

            private void ApplyTextureAspectDefault()
            {
                if (_texture == null)
                    return;

                if (_sizeMode != GrassCardSizeMode.Manual)
                    ResolveSize();
            }

            private void RenderPreview(Rect rect)
            {
                if (rect.width <= 2f || rect.height <= 2f)
                    return;

                if (_texture == null)
                {
                    EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.13f));
                    GUI.Label(rect, "Select a texture to preview.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                EnsurePreviewUtility();
                DestroyPreviewAssets();

                GrassCardSettings settings = BuildSettings();
                _previewMesh = BuildGrassCardMesh(settings);
                _previewMesh.name = "GrassCardPreview";
                _previewMesh.hideFlags = HideFlags.HideAndDontSave;
                _previewMaterial = CreatePreviewMaterial();

                if (_previewMaterial == null)
                {
                    EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.13f));
                    GUI.Label(rect, "No compatible preview shader found.", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                _previewMaterial.hideFlags = HideFlags.HideAndDontSave;
                ApplyPreviewMaterialSettings(_previewMaterial, settings, _materialSettings);

                _previewUtility.BeginPreview(rect, GUIStyle.none);
                _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
                _previewUtility.camera.backgroundColor = new Color(0.18f, 0.20f, 0.21f, 1f);
                _previewUtility.camera.fieldOfView = 28f;
                _previewUtility.camera.nearClipPlane = 0.01f;
                _previewUtility.camera.farClipPlane = 100f;
                _previewUtility.camera.aspect = Mathf.Max(0.01f, rect.width / rect.height);

                Bounds bounds = _previewMesh.bounds;
                Vector3 center = bounds.center;
                float radius = Mathf.Max(0.35f, bounds.extents.magnitude);
                float distance = radius * 3.2f;
                Quaternion orbit = Quaternion.Euler(12f, _previewYaw, 0f);
                Vector3 cameraPosition = center + orbit * new Vector3(0f, 0f, -distance);
                _previewUtility.camera.transform.position = cameraPosition;
                _previewUtility.camera.transform.rotation = Quaternion.LookRotation(center - cameraPosition, Vector3.up);
                _previewUtility.lights[0].intensity = 1.25f;
                _previewUtility.lights[0].transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                _previewUtility.lights[1].intensity = 0.45f;
                _previewUtility.ambientColor = new Color(0.45f, 0.48f, 0.50f);
                _previewUtility.DrawMesh(_previewMesh, Matrix4x4.identity, _previewMaterial, 0);
                _previewUtility.Render();

                Texture previewTexture = _previewUtility.EndPreview();
                GUI.DrawTexture(rect, previewTexture, ScaleMode.StretchToFill, false);
            }

            private void EnsurePreviewUtility()
            {
                if (_previewUtility != null)
                    return;

                _previewUtility = new PreviewRenderUtility();
                _previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            }

            private void DestroyPreviewAssets()
            {
                if (_previewMesh != null)
                    DestroyImmediate(_previewMesh);
                if (_previewMaterial != null && _previewMaterial.hideFlags == HideFlags.HideAndDontSave)
                    DestroyImmediate(_previewMaterial);

                _previewMesh = null;
                _previewMaterial = null;
            }

            private static void DrawVector2Field(ref Vector2 value, string label, float min, float max)
            {
                value = ClampVector2(EditorGUILayout.Vector2Field(label, value), min, max);
            }
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

        private static void SetVectorIfExists(Material material, string property, Vector4 value)
        {
            if (material.HasProperty(property))
                material.SetVector(property, value);
        }

        private static Vector2 ClampVector2(Vector2 value, float min, float max)
        {
            return new Vector2(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));
        }

        private static Vector4 ToVector4(Vector2 value)
        {
            return new Vector4(value.x, value.y, 0f, 0f);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        private static string NormalizeAssetFolder(string folder)
        {
            folder = string.IsNullOrWhiteSpace(folder) ? DefaultFolder : folder.Trim();
            return folder.Replace('\\', '/').TrimEnd('/');
        }

        private static bool IsAssetFolder(string folder)
        {
            return string.Equals(folder, "Assets", System.StringComparison.Ordinal)
                   || folder.StartsWith("Assets/", System.StringComparison.Ordinal);
        }

        private static string MakeSafeFileName(string value)
        {
            value = string.IsNullOrWhiteSpace(value) ? "GrassCard" : value.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');
            return string.IsNullOrWhiteSpace(value) ? "GrassCard" : value;
        }
    }
}
