using System;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Shared.Graphics;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    public sealed class GameplayStartupGraphicsWindow : EditorWindow, IMoyvaHubPreviewProvider, IMoyvaHubSettingsOpener
    {
        private const string AssetPath = "Assets/Moyva/Resources/MoyvaStartupGraphics.asset";

        private GraphicsStartupSettingsSO _settingsAsset;
        private SerializedObject _serializedObject;
        private Vector2 _scroll;
        private Texture2D _checker;
        private Texture2D _stripe;
        private float _previewZoomNormalized = 0.5f;

        public static void Open()
        {
            var window = GetWindow<GameplayStartupGraphicsWindow>("Startup Graphics");
            window.minSize = new Vector2(640f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureAssetLoaded();
            EnsurePreviewTextures();
        }

        public string HubToolMenuPath => "Moyva/Tools/Gameplay Startup Graphics";

        public string GetHubPreviewSummary()
        {
            if (_settingsAsset == null)
                return "Settings asset not found";

            var dev = _settingsAsset.DeveloperPixelOptimization;
            return dev.Enabled
                ? $"DevPixel ON, cap={dev.RenderScaleCap:0.00}, mip>={dev.MinimumTextureMipmapLimit}"
                : "DevPixel OFF";
        }

        public void DrawHubPreview(Rect rect)
        {
            EnsureAssetLoaded();
            EnsurePreviewTextures();

            EditorGUI.DrawRect(rect, new Color(0.08f, 0.11f, 0.15f));

            if (_settingsAsset == null)
            {
                GUI.Label(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, 16f), "Startup graphics asset missing", EditorStyles.miniBoldLabel);
                return;
            }

            var settings = _settingsAsset.StartupSettings;
            var dev = _settingsAsset.DeveloperPixelOptimization;
            float cap = dev.Enabled ? dev.RenderScaleCap : 1f;
            float effectiveScale = Mathf.Min(settings.RenderScale, cap);
            float pixelStrength = Mathf.InverseLerp(1f, 0.42f, effectiveScale);

            Rect top = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height * 0.46f);
            DrawVerticalGradient(top, new Color(0.16f, 0.34f, 0.56f), new Color(0.48f, 0.66f, 0.78f));

            Rect ground = new Rect(rect.x + 8f, rect.y + rect.height * 0.52f, rect.width - 16f, rect.height * 0.40f);
            EditorGUI.DrawRect(ground, new Color(0.21f, 0.36f, 0.22f));
            GUI.DrawTextureWithTexCoords(ground, _checker, new Rect(0f, 0f, Mathf.Lerp(12f, 4f, pixelStrength), Mathf.Lerp(12f, 4f, pixelStrength)));

            int columns = Mathf.RoundToInt(Mathf.Lerp(0f, 56f, pixelStrength));
            int rows = Mathf.RoundToInt(Mathf.Lerp(0f, 32f, pixelStrength));
            if (columns > 1 && rows > 1)
            {
                Color line = new Color(0f, 0f, 0f, Mathf.Lerp(0.02f, 0.14f, pixelStrength));
                for (int x = 0; x <= columns; x++)
                {
                    float px = rect.x + rect.width * (x / (float)columns);
                    EditorGUI.DrawRect(new Rect(px, rect.y, 1f, rect.height), line);
                }

                for (int y = 0; y <= rows; y++)
                {
                    float py = rect.y + rect.height * (y / (float)rows);
                    EditorGUI.DrawRect(new Rect(rect.x, py, rect.width, 1f), line);
                }
            }

            GUIStyle label = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = Color.white } };
            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 16f), "Gameplay Startup Graphics", label);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 24f, rect.width - 20f, 16f), GetHubPreviewSummary(), EditorStyles.miniLabel);
        }

        public bool OpenHubSettingsFromPreview()
        {
            Open();
            return true;
        }

        private void OnDisable()
        {
            if (_checker != null)
                DestroyImmediate(_checker);

            if (_stripe != null)
                DestroyImmediate(_stripe);
        }

        private void OnGUI()
        {
            EnsureAssetLoaded();
            EnsurePreviewTextures();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Стартова графіка Gameplay", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Ці значення використовуються як стартові для першого запуску gameplay і для кнопки скидання графіки в головному меню. " +
                "Гравець може змінити їх у налаштуваннях головного меню, після чого вони збережуться локально.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var nextAsset = (GraphicsStartupSettingsSO)EditorGUILayout.ObjectField("Settings Asset", _settingsAsset, typeof(GraphicsStartupSettingsSO), false);
                if (EditorGUI.EndChangeCheck())
                {
                    _settingsAsset = nextAsset;
                    _serializedObject = _settingsAsset != null ? new SerializedObject(_settingsAsset) : null;
                }

                if (GUILayout.Button("Create/Find", GUILayout.Width(96f)))
                    CreateOrLoadAsset();
            }

            if (_settingsAsset == null || _serializedObject == null)
            {
                EditorGUILayout.HelpBox("Asset не знайдено. Натисніть Create/Find.", MessageType.Warning);
                return;
            }

            _serializedObject.Update();
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_startupSettings"), true);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Zoom Graphics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_zoomSettings"), true);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Developer Pixel Optimization (Dev-only)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Ці параметри не віддаються гравцю в UI. Вони примусово застосовуються розробником поверх user graphics і можуть зробити картинку більш піксельною для оптимізації.",
                MessageType.Warning);
            EditorGUILayout.PropertyField(_serializedObject.FindProperty("_developerPixelOptimization"), true);
            if (_serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settingsAsset);
                AssetDatabase.SaveAssets();
                Repaint();
            }

            DrawDeveloperDiagnostics(_settingsAsset.DeveloperPixelOptimization);

            _previewZoomNormalized = EditorGUILayout.Slider(new GUIContent("Preview Zoom", "0 = далекий зум, 1 = близький зум"), _previewZoomNormalized, 0f, 1f);
            DrawPlayerPreview(_settingsAsset.StartupSettings, _settingsAsset.ZoomSettings, _settingsAsset.DeveloperPixelOptimization, _previewZoomNormalized);

            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset to Auto Defaults"))
                {
                    Undo.RecordObject(_settingsAsset, "Reset startup graphics defaults");
                    _settingsAsset.StartupSettings = GraphicsSettingsData.CreateDefault();
                    EditorUtility.SetDirty(_settingsAsset);
                    AssetDatabase.SaveAssets();
                    _serializedObject.Update();
                    Repaint();
                }

                if (GUILayout.Button("Ping Asset", GUILayout.Width(96f)))
                {
                    Selection.activeObject = _settingsAsset;
                    EditorGUIUtility.PingObject(_settingsAsset);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void EnsureAssetLoaded()
        {
            if (_settingsAsset != null && _serializedObject != null)
                return;

            _settingsAsset = AssetDatabase.LoadAssetAtPath<GraphicsStartupSettingsSO>(AssetPath);
            if (_settingsAsset != null)
                _serializedObject = new SerializedObject(_settingsAsset);
        }

        private void CreateOrLoadAsset()
        {
            EnsureFolder("Assets/Moyva");
            EnsureFolder("Assets/Moyva/Resources");

            _settingsAsset = AssetDatabase.LoadAssetAtPath<GraphicsStartupSettingsSO>(AssetPath);
            if (_settingsAsset == null)
            {
                _settingsAsset = CreateInstance<GraphicsStartupSettingsSO>();
                _settingsAsset.StartupSettings = GraphicsSettingsData.CreateDefault();
                AssetDatabase.CreateAsset(_settingsAsset, AssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            _serializedObject = new SerializedObject(_settingsAsset);
            Selection.activeObject = _settingsAsset;
            EditorGUIUtility.PingObject(_settingsAsset);
        }

        private static void EnsureFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[index]);

                current = next;
            }
        }

        private void DrawPlayerPreview(GraphicsSettingsData settings, ZoomGraphicsSettings zoomSettings, DeveloperPixelOptimizationSettings developerPixel, float zoomT)
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Прев'ю очима гравця", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Це live-симуляція того, як стартові графічні параметри вплинуть на картинку в gameplay.",
                MessageType.None);

            float width = Mathf.Min(position.width - 40f, 980f);
            float height = width * 0.56f;
            Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false));

            DrawSky(rect, settings);
            DrawTerrain(rect, settings);
            DrawDetails(rect, settings);
            float zoomPressure = zoomT;
            DrawShadows(rect, settings, zoomPressure);
            DrawFogAndScaleOverlay(rect, settings, developerPixel, zoomPressure);
            DrawHudOverlay(rect, settings, zoomSettings, developerPixel, zoomPressure);
            DrawPixelationOverlay(rect, settings, developerPixel, zoomPressure);
        }

        private void DrawSky(Rect rect, GraphicsSettingsData settings)
        {
            Color top = Color.Lerp(new Color(0.18f, 0.42f, 0.72f), new Color(0.12f, 0.25f, 0.48f), settings.TextureMipmapLimit / 3f);
            Color bottom = Color.Lerp(new Color(0.55f, 0.76f, 0.92f), new Color(0.40f, 0.58f, 0.76f), settings.TextureMipmapLimit / 3f);
            DrawVerticalGradient(rect, top, bottom);

            float cloudAlpha = settings.Shadows ? 0.18f : 0.08f;
            DrawEllipse(new Rect(rect.x + rect.width * 0.08f, rect.y + rect.height * 0.08f, rect.width * 0.24f, rect.height * 0.12f), new Color(1f, 1f, 1f, cloudAlpha));
            DrawEllipse(new Rect(rect.x + rect.width * 0.65f, rect.y + rect.height * 0.12f, rect.width * 0.20f, rect.height * 0.10f), new Color(1f, 1f, 1f, cloudAlpha));
        }

        private void DrawTerrain(Rect rect, GraphicsSettingsData settings)
        {
            Rect ground = new Rect(rect.x, rect.y + rect.height * 0.45f, rect.width, rect.height * 0.55f);
            EditorGUI.DrawRect(ground, new Color(0.23f, 0.40f, 0.24f));

            float mipT = settings.TextureMipmapLimit / 3f;
            float tiling = Mathf.Lerp(14f, 4f, mipT);
            GUI.DrawTextureWithTexCoords(ground, _checker, new Rect(0f, 0f, tiling, tiling));

            Rect road = new Rect(rect.x + rect.width * 0.08f, rect.y + rect.height * 0.63f, rect.width * 0.84f, rect.height * 0.10f);
            EditorGUI.DrawRect(road, new Color(0.47f, 0.40f, 0.28f));
            GUI.DrawTextureWithTexCoords(road, _stripe, new Rect(0f, 0f, 22f, 1f));
        }

        private void DrawDetails(Rect rect, GraphicsSettingsData settings)
        {
            int lodCount = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(4f, 14f, Mathf.InverseLerp(0.4f, 2f, settings.LodBias))), 3, 16);
            float baseY = rect.y + rect.height * 0.59f;
            for (int i = 0; i < lodCount; i++)
            {
                float t = i / Mathf.Max(1f, lodCount - 1f);
                float x = rect.x + Mathf.Lerp(rect.width * 0.12f, rect.width * 0.88f, t);
                float h = Mathf.Lerp(rect.height * 0.06f, rect.height * 0.13f, Mathf.PingPong(i * 0.37f, 1f));
                DrawTree(new Vector2(x, baseY), h, settings);
            }

            int aaLevel = settings.AntiAliasing;
            if (aaLevel == 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    float x = rect.x + (i / 29f) * rect.width;
                    EditorGUI.DrawRect(new Rect(x, rect.y + rect.height * 0.45f, 1f, 2f), new Color(0f, 0f, 0f, 0.20f));
                }
            }
        }

        private void DrawShadows(Rect rect, GraphicsSettingsData settings, float zoomPressure)
        {
            if (!settings.Shadows)
                return;

            float alpha = (settings.AntiAliasing >= 2 ? 0.22f : 0.30f) * Mathf.Lerp(0.7f, 1.2f, zoomPressure);
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                float x = rect.x + Mathf.Lerp(rect.width * 0.15f, rect.width * 0.85f, t);
                float y = rect.y + rect.height * 0.61f;
                float w = rect.width * 0.035f;
                float h = rect.height * 0.020f;
                DrawEllipse(new Rect(x, y, w, h), new Color(0f, 0f, 0f, alpha));
            }
        }

        private void DrawFogAndScaleOverlay(Rect rect, GraphicsSettingsData settings, DeveloperPixelOptimizationSettings developerPixel, float zoomPressure)
        {
            if (settings.CloseZoomOptimization)
            {
                Rect vignette = rect;
                Color edge = new Color(0f, 0f, 0f, Mathf.Lerp(0.05f, 0.26f, zoomPressure));
                EditorGUI.DrawRect(new Rect(vignette.x, vignette.y, vignette.width, vignette.height * 0.06f), edge);
                EditorGUI.DrawRect(new Rect(vignette.x, vignette.yMax - vignette.height * 0.08f, vignette.width, vignette.height * 0.08f), edge);
                EditorGUI.DrawRect(new Rect(vignette.x, vignette.y, vignette.width * 0.04f, vignette.height), edge);
                EditorGUI.DrawRect(new Rect(vignette.xMax - vignette.width * 0.04f, vignette.y, vignette.width * 0.04f, vignette.height), edge);
            }

            float zoomScaleLimit = Mathf.Lerp(1f, 0.42f, zoomPressure);
            float pixelCap = developerPixel.Enabled ? developerPixel.RenderScaleCap : 1f;
            float effectiveScale = Mathf.Min(settings.RenderScale, zoomScaleLimit, pixelCap);
            float blur = Mathf.InverseLerp(1f, 0.42f, effectiveScale);
            if (blur > 0.001f)
            {
                int strips = Mathf.RoundToInt(Mathf.Lerp(0f, 120f, blur));
                for (int i = 0; i < strips; i++)
                {
                    float x = rect.x + (i / Mathf.Max(1f, strips - 1f)) * rect.width;
                    float a = 0.008f + blur * 0.03f;
                    EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), new Color(1f, 1f, 1f, a));
                }
            }
        }

        private void DrawHudOverlay(Rect rect, GraphicsSettingsData settings, ZoomGraphicsSettings zoomSettings, DeveloperPixelOptimizationSettings developerPixel, float zoomPressure)
        {
            Rect panel = new Rect(rect.x + 10f, rect.y + 10f, 250f, 92f);
            EditorGUI.DrawRect(panel, new Color(0f, 0f, 0f, 0.45f));

            GUIStyle title = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.white } };
            GUIStyle line = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.90f, 0.93f, 0.98f, 1f) } };

            GUI.Label(new Rect(panel.x + 8f, panel.y + 6f, panel.width - 16f, 18f), "Player View Preview", title);
            GUI.Label(new Rect(panel.x + 8f, panel.y + 26f, panel.width - 16f, 16f), $"Profile: {settings.Profile}  FPS: {settings.TargetFrameRate}", line);
            float closeZoomLimit = Mathf.Lerp(1f, zoomSettings.CloseZoomRenderScaleMin, zoomPressure);
            float pixelCap = developerPixel.Enabled ? developerPixel.RenderScaleCap : 1f;
            float effectiveScale = Mathf.Min(settings.RenderScale, closeZoomLimit, pixelCap);
            GUI.Label(new Rect(panel.x + 8f, panel.y + 42f, panel.width - 16f, 16f), $"Scale: {settings.RenderScale:0.00} -> {effectiveScale:0.00}  Zoom: {zoomPressure:0.00}", line);
            GUI.Label(new Rect(panel.x + 8f, panel.y + 58f, panel.width - 16f, 16f), $"Tex: {settings.TextureMipmapLimit}  AA: {settings.AntiAliasing}x  LOD: {settings.LodBias:0.00}", line);
            string devState = developerPixel.Enabled ? $"DevPixel: ON cap={developerPixel.RenderScaleCap:0.00}" : "DevPixel: OFF";
            GUI.Label(new Rect(panel.x + 8f, panel.y + 74f, panel.width - 16f, 16f), devState, line);
        }

        private void DrawPixelationOverlay(Rect rect, GraphicsSettingsData settings, DeveloperPixelOptimizationSettings developerPixel, float zoomPressure)
        {
            if (!developerPixel.Enabled)
                return;

            float effectiveScale = Mathf.Min(settings.RenderScale, developerPixel.RenderScaleCap, Mathf.Lerp(1f, 0.42f, zoomPressure));
            float pixelStrength = Mathf.InverseLerp(1f, 0.42f, effectiveScale);
            int columns = Mathf.RoundToInt(Mathf.Lerp(0f, 120f, pixelStrength));
            int rows = Mathf.RoundToInt(Mathf.Lerp(0f, 68f, pixelStrength));

            if (columns < 2 || rows < 2)
                return;

            Color lineColor = new Color(0f, 0f, 0f, Mathf.Lerp(0.03f, 0.17f, pixelStrength));
            for (int x = 0; x <= columns; x++)
            {
                float px = rect.x + rect.width * (x / (float)columns);
                EditorGUI.DrawRect(new Rect(px, rect.y, 1f, rect.height), lineColor);
            }

            for (int y = 0; y <= rows; y++)
            {
                float py = rect.y + rect.height * (y / (float)rows);
                EditorGUI.DrawRect(new Rect(rect.x, py, rect.width, 1f), lineColor);
            }

            Rect badge = new Rect(rect.xMax - 220f, rect.y + 10f, 210f, 24f);
            EditorGUI.DrawRect(badge, new Color(0f, 0f, 0f, 0.42f));
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel) { normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter };
            GUI.Label(badge, $"Dev Pixel Preview x{Mathf.Lerp(1f, 2.8f, pixelStrength):0.0}", badgeStyle);
        }

        private static void DrawDeveloperDiagnostics(DeveloperPixelOptimizationSettings developerPixel)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Dev Diagnostics", EditorStyles.boldLabel);

            bool hasStartupAsset = AssetDatabase.LoadAssetAtPath<GraphicsStartupSettingsSO>(AssetPath) != null;
            DrawDiagLine("Resources startup asset", hasStartupAsset,
                hasStartupAsset ? "OK" : "Missing: Assets/Moyva/Resources/MoyvaStartupGraphics.asset");

            bool hasMainCamera = UnityEngine.Camera.main != null;
            DrawDiagLine("Main camera in opened scene", hasMainCamera,
                hasMainCamera ? "OK" : "MainCamera not found in opened scene.");

            bool hasCameraInstallerInScene = FindAnyComponentByTypeName("Kruty1918.Moyva.Camera.Runtime.CameraInstaller, Kruty1918.Moyva.Camera");
            DrawDiagLine("CameraInstaller on scene", hasCameraInstallerInScene,
                hasCameraInstallerInScene ? "OK" : "CameraInstaller not found on opened scene objects.");

            bool hasProjectServicesInstaller = FindAnyComponentByTypeName("Kruty1918.Moyva.Bootstrap.ProjectServicesInstaller, Kruty1918.Moyva.Bootstrap");
            DrawDiagLine("ProjectServicesInstaller present", hasProjectServicesInstaller,
                hasProjectServicesInstaller ? "OK" : "ProjectServicesInstaller instance not found (ProjectContext may be unopened in EditMode).");

            if (developerPixel.Enabled && !hasStartupAsset)
            {
                EditorGUILayout.HelpBox("Dev pixel mode enabled, але startup asset відсутній: runtime не зможе застосувати override.", MessageType.Error);
            }
        }

        private static void DrawDiagLine(string label, bool ok, string details)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(ok ? "[OK]" : "[WARN]", GUILayout.Width(52f));
                GUILayout.Label(label, EditorStyles.label, GUILayout.Width(240f));
                GUILayout.Label(details, EditorStyles.miniLabel);
            }
        }

        private static bool FindAnyComponentByTypeName(string assemblyQualifiedType)
        {
            Type type = Type.GetType(assemblyQualifiedType);
            if (type == null)
                return false;

            var components = UnityEngine.Object.FindObjectsByType(type, FindObjectsInactive.Include, FindObjectsSortMode.None);
            return components != null && components.Length > 0;
        }

        private void EnsurePreviewTextures()
        {
            if (_checker == null)
                _checker = BuildCheckerTexture(32, new Color(0.28f, 0.45f, 0.28f), new Color(0.21f, 0.36f, 0.22f));

            if (_stripe == null)
                _stripe = BuildStripeTexture(64, new Color(0.53f, 0.46f, 0.31f), new Color(0.44f, 0.38f, 0.26f));
        }

        private static Texture2D BuildCheckerTexture(int size, Color a, Color b)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };

            int block = Mathf.Max(2, size / 8);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool odd = ((x / block) + (y / block)) % 2 == 0;
                    texture.SetPixel(x, y, odd ? a : b);
                }
            }

            texture.Apply(false, true);
            return texture;
        }

        private static Texture2D BuildStripeTexture(int size, Color a, Color b)
        {
            var texture = new Texture2D(size, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.HideAndDontSave
            };

            for (int x = 0; x < size; x++)
                texture.SetPixel(x, 0, (x % 8) < 3 ? a : b);

            texture.Apply(false, true);
            return texture;
        }

        private static void DrawVerticalGradient(Rect rect, Color top, Color bottom)
        {
            Handles.BeginGUI();
            for (int i = 0; i < 64; i++)
            {
                float t0 = i / 64f;
                float y = rect.y + t0 * rect.height;
                float h = rect.height / 64f + 1f;
                EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, h), Color.Lerp(top, bottom, t0));
            }

            Handles.EndGUI();
        }

        private static void DrawEllipse(Rect rect, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Vector3 center = new Vector3(rect.center.x, rect.center.y, 0f);
            float rx = rect.width * 0.5f;
            float ry = rect.height * 0.5f;
            const int segments = 30;
            var points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = i / (float)segments * Mathf.PI * 2f;
                points[i] = new Vector3(center.x + Mathf.Cos(angle) * rx, center.y + Mathf.Sin(angle) * ry, 0f);
            }

            Handles.DrawAAConvexPolygon(points);
            Handles.EndGUI();
        }

        private static void DrawTree(Vector2 basePosition, float height, GraphicsSettingsData settings)
        {
            float trunkW = Mathf.Max(2f, height * 0.12f);
            float trunkH = height * 0.48f;
            EditorGUI.DrawRect(new Rect(basePosition.x - trunkW * 0.5f, basePosition.y - trunkH, trunkW, trunkH), new Color(0.28f, 0.18f, 0.10f));

            float canopy = height * (settings.AnisotropicFiltering ? 0.64f : 0.54f);
            DrawEllipse(new Rect(basePosition.x - canopy * 0.50f, basePosition.y - trunkH - canopy * 0.85f, canopy, canopy * 0.78f), new Color(0.14f, 0.46f, 0.19f, 0.95f));
        }
    }
}