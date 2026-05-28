using System.IO;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    public sealed class MoyvaProjectSettingsEditorWindow : EditorWindow
    {
        private const string ResourcesDirectory = "Assets/Moyva/Resources";
        private const string StartupGraphicsTypeName = "Kruty1918.Moyva.Shared.Graphics.GraphicsStartupSettingsSO, Kruty1918.Moyva.Shared";
        private const string AdaptivePerformanceTypeName = "Kruty1918.Moyva.Shared.Performance.AdaptivePerformanceSettingsSO, Kruty1918.Moyva.Shared";
        private static readonly string StartupGraphicsAssetPath = $"{ResourcesDirectory}/MoyvaStartupGraphics.asset";
        private static readonly string AdaptivePerformanceAssetPath = $"{ResourcesDirectory}/MoyvaAdaptivePerformance.asset";

        private MoyvaProjectSettingsSO _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scroll;

        [MenuItem("Moyva/Project/Global Settings")]
        public static void Open()
        {
            var window = GetWindow<MoyvaProjectSettingsEditorWindow>();
            window.titleContent = new GUIContent("Moyva Project Settings");
            window.minSize = new Vector2(520f, 420f);
            window.Show();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/Moyva", SettingsScope.Project)
            {
                label = "Moyva",
                guiHandler = _ => DrawSettingsProviderGui(),
                keywords = new[] { "Moyva", "Grid", "Projection", "Isometric", "Hex", "3D", "Camera", "Preview", "Optimization" },
            };
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            DrawHeader();

            if (_settings == null)
                LoadSettings();

            if (_serializedSettings == null)
            {
                EditorGUILayout.HelpBox("Moyva project settings asset is not available.", MessageType.Warning);
                if (GUILayout.Button("Create Settings Asset"))
                    LoadSettings(createIfMissing: true);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSerializedSettings(_serializedSettings);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSettingsProviderGui()
        {
            var settings = GetOrCreateSettingsAsset();
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Could not create Moyva project settings asset.", MessageType.Error);
                return;
            }

            DrawSerializedSettings(new SerializedObject(settings));
        }

        private static void DrawSerializedSettings(SerializedObject serializedSettings)
        {
            serializedSettings.Update();

            var target = serializedSettings.targetObject as MoyvaProjectSettingsSO;
            DrawVisualModeControls(serializedSettings, target);

            bool useAdvancedSettings = serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.UseAdvancedProjectSettings)).boolValue;
            if (!useAdvancedSettings)
            {
                if (serializedSettings.ApplyModifiedProperties())
                {
                    target?.Normalize();
                    EditorUtility.SetDirty(serializedSettings.targetObject);
                    serializedSettings.Update();
                }

                target?.Normalize();
                DrawResolvedAutoSettings(target);
                DrawHomeMenuMeshPreviewSettings(serializedSettings);
                if (serializedSettings.ApplyModifiedProperties())
                {
                    target?.Normalize();
                    EditorUtility.SetDirty(serializedSettings.targetObject);
                }

                DrawAssetFooter(serializedSettings.targetObject);
                DrawOptimizationSettings();
                return;
            }

            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultGridTopology)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultProjectionMode)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultRenderMode)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultNeighborhood)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Orthogonal", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.OrthogonalCellWidth)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.OrthogonalCellHeight)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.OrthogonalCellDepth)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Isometric", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.IsometricTileWidth)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.IsometricTileHeight)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Hex", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.HexOrientation)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.HexRadius)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Height Preview", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.HeightScale)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.UseHeightForPreview)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("3D Preview", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.EnableMeshPrefabPreviews)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewTextureSize)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewPadding)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.GeneratePreviewMipmaps)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewFilterMode)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewRenderTextureFormat)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewRenderTextureReadWrite)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Orthographic3DPreviewEuler)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Isometric3DPreviewEuler)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.UsePerspectivePreviewCameraIn3D)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewPerspectiveFieldOfView)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewLightEuler)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.PreviewLightIntensity)));

            DrawHomeMenuMeshPreviewSettings(serializedSettings);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("3D Lighting", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.AutoConfigure3DLighting)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.CreateDirectionalLightIn3D)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DLightEuler)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DLightColor)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DLightIntensity)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DLightShadows)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DAmbientSkyColor)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DAmbientEquatorColor)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DAmbientGroundColor)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Enable3DAtmosphericFog)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DAtmosphericFogColor)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DAtmosphericFogDensity)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Camera Defaults", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.CameraPolicy)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.CameraAnglePolicy)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Custom3DCameraEuler)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DCameraDistance)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DOrthographicSize)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.Project3DFieldOfView)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Startup Camera", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.EnsureStartupCameraShowsRevealedArea)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.StartupCameraPaddingTiles)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.StartupCameraRadiusSource)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.ManualStartupCameraRadius)));

            if (serializedSettings.ApplyModifiedProperties())
            {
                target?.Normalize();
                EditorUtility.SetDirty(serializedSettings.targetObject);
            }

            DrawAssetFooter(serializedSettings.targetObject);

            DrawOptimizationSettings();
        }

        private static void DrawVisualModeControls(SerializedObject serializedSettings, MoyvaProjectSettingsSO target)
        {
            EditorGUILayout.LabelField("Game Visual Mode", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.ProjectVisualMode)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.UseAdvancedProjectSettings)));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Full 3D Defaults"))
                {
                    serializedSettings.ApplyModifiedProperties();
                    target?.UseFull3DDefaults();
                    if (target != null)
                        EditorUtility.SetDirty(target);
                    serializedSettings.Update();
                }

                if (GUILayout.Button("Use Classic 2D Defaults"))
                {
                    serializedSettings.ApplyModifiedProperties();
                    target?.UseClassic2DDefaults();
                    if (target != null)
                        EditorUtility.SetDirty(target);
                    serializedSettings.Update();
                }
            }

            EditorGUILayout.HelpBox(
                "For normal use, keep Advanced Project Settings disabled. Full 3D automatically configures XZ grid projection, mesh rendering, perspective camera, prefab previews, and startup framing.",
                MessageType.Info);
        }

        private static void DrawResolvedAutoSettings(MoyvaProjectSettingsSO target)
        {
            if (target == null)
                return;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Resolved Defaults", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.EnumPopup("Visual Mode", target.ResolveProjectVisualMode());
                EditorGUILayout.EnumPopup("Grid Topology", target.DefaultGridTopology);
                EditorGUILayout.EnumPopup("Projection", target.DefaultProjectionMode);
                EditorGUILayout.EnumPopup("Render Mode", target.DefaultRenderMode);
                EditorGUILayout.EnumPopup("Camera", target.CameraPolicy);
                EditorGUILayout.EnumPopup("Camera Angle", target.CameraAnglePolicy);
                EditorGUILayout.Toggle("Mesh Prefab Previews", target.EnableMeshPrefabPreviews);
                EditorGUILayout.Toggle("Live Home Menu Mesh", target.UseLiveHomeMenuMeshPreview);
                EditorGUILayout.Toggle("3D Lighting", target.AutoConfigure3DLighting);
                EditorGUILayout.Toggle("Startup Framing", target.EnsureStartupCameraShowsRevealedArea);
            }
        }

        private static void DrawHomeMenuMeshPreviewSettings(SerializedObject serializedSettings)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Прев'ю головного меню (3D mesh)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Для Full 3D головне меню може показувати живу 3D-сцену з mesh-префабів замість RawImage-текстури. Це дає вигляд ближчий до гри; нижче можна обмежити деталізацію для мобільних пристроїв.",
                MessageType.Info);

            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.UseLiveHomeMenuMeshPreview),
                "Живе 3D mesh-прев'ю",
                "Якщо увімкнено, у Full 3D режимі Home Menu вимикає RawImage-текстуру і будує реальні MeshRenderer-и перед preview-камерою.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewIncludeObjects),
                "Показувати об'єкти",
                "Додає в прев'ю map-objects із відповідних prefab mesh. Вимкніть для дешевшого фону на слабких пристроях.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewIncludeBuildings),
                "Показувати будівлі",
                "Додає стартові/preview будівлі з BuildingRegistry у mesh-прев'ю головного меню.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewCombineMeshesByMaterial),
                "Об'єднувати mesh за матеріалом",
                "Зменшує кількість Renderer-ів і draw calls, групуючи однакові матеріали в батчі. Залишайте увімкненим для мобільних.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewTileStride),
                "Крок тайлів",
                "1 означає показувати кожен тайл. 2 показує кожен другий тайл по X/Y і зменшує кількість mesh-ів для дуже великих карт.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewMaxTerrainTiles),
                "Ліміт тайлів",
                "Максимальна кількість terrain-тайлів у live preview. Якщо карта більша, крок тайлів автоматично збільшується.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewMaxVerticesPerBatch),
                "Вершин у батчі",
                "Верхня межа вершин для одного комбінованого mesh. Менші значення створюють більше батчів, але зменшують розмір кожного mesh.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewMaxMaterialBatches),
                "Ліміт матеріалів",
                "Максимальна кількість унікальних матеріальних груп у live preview. Нові матеріали понад ліміт пропускаються, щоб не роздувати draw calls.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewCameraPadding),
                "Відступ камери",
                "Множник віддалення preview-камери від згенерованого світу. Більше значення показує карту здалеку.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewCameraDepth),
                "Глибина камери",
                "Render depth для preview-камери. Якщо mesh-фон перекриває UI або не видно за іншою камерою, налаштуйте це значення.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewLayer),
                "Layer прев'ю",
                "Unity layer для runtime mesh-прев'ю. Камера меню рендерить тільки цей layer, щоб не підхопити gameplay-об'єкти.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewDisableFog),
                "Вимкнути туман для камери",
                "Тимчасово вимикає RenderSettings.fog під час рендера preview-камери. Fog of War у меню не створюється.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewUploadMeshData),
                "Вивантажувати CPU mesh data",
                "Після створення комбінованого mesh звільняє CPU-копію даних. Це економить пам'ять, але mesh не можна змінювати без перебудови.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewCastShadows),
                "Тіні від mesh",
                "Дозволяє preview mesh відкидати тіні. Для мобільних зазвичай краще вимкнути.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewReceiveShadows),
                "Приймати тіні",
                "Дозволяє preview mesh приймати тіні. Вимкнення зменшує навантаження і робить фон стабільнішим.");
            DrawProperty(serializedSettings, nameof(MoyvaProjectSettingsSO.HomeMenuPreviewBackgroundColor),
                "Колір фону камери",
                "Колір очищення preview-камери за 3D світом головного меню.");
        }

        private static void DrawProperty(SerializedObject serializedSettings, string propertyName, string label, string tooltip)
        {
            var property = serializedSettings.FindProperty(propertyName);
            if (property == null)
                return;

            EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), includeChildren: true);
        }

        private static void DrawAssetFooter(Object targetObject)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Asset", GUILayout.Width(120f)))
                    EditorGUIUtility.PingObject(targetObject);
                GUILayout.Label(MoyvaProjectSettingsSO.DefaultAssetPath, EditorStyles.miniLabel);
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Moyva Project Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Central defaults for grid topology, projection, render mode, and preview height. Runtime installers can use this asset while current orthogonal 2D behavior remains the default.", MessageType.Info);
        }

        private static void DrawOptimizationSettings()
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Optimization", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These controls edit the existing startup/adaptive graphics assets. Runtime player choices still live in graphics_settings.dat.",
                MessageType.Info);

            DrawAssetSettings("Startup Graphics", GetOrCreateAsset(StartupGraphicsAssetPath, StartupGraphicsTypeName));
            DrawAssetSettings("Adaptive Performance", GetOrCreateAsset(AdaptivePerformanceAssetPath, AdaptivePerformanceTypeName));
        }

        private static void DrawAssetSettings(string title, ScriptableObject asset)
        {
            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                if (asset != null && GUILayout.Button("Ping", GUILayout.Width(56f)))
                    EditorGUIUtility.PingObject(asset);
            }

            if (asset == null)
            {
                EditorGUILayout.HelpBox($"{title} asset is not available.", MessageType.Warning);
                return;
            }

            var serializedAsset = new SerializedObject(asset);
            serializedAsset.Update();
            SerializedProperty property = serializedAsset.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
                    EditorGUILayout.PropertyField(property, includeChildren: true);
            }

            if (serializedAsset.ApplyModifiedProperties())
                EditorUtility.SetDirty(asset);
        }

        private void LoadSettings(bool createIfMissing = true)
        {
            _settings = createIfMissing ? GetOrCreateSettingsAsset() : LoadSettingsAsset();
            _serializedSettings = _settings != null ? new SerializedObject(_settings) : null;
        }

        private static MoyvaProjectSettingsSO LoadSettingsAsset()
            => AssetDatabase.LoadAssetAtPath<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);

        private static MoyvaProjectSettingsSO GetOrCreateSettingsAsset()
        {
            var settings = LoadSettingsAsset();
            if (settings != null)
                return settings;

            string directory = Path.GetDirectoryName(MoyvaProjectSettingsSO.DefaultAssetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            settings = CreateInstance<MoyvaProjectSettingsSO>();
            settings.Normalize();
            AssetDatabase.CreateAsset(settings, MoyvaProjectSettingsSO.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static ScriptableObject GetOrCreateAsset(string path, string typeName)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset != null)
                return asset;

            System.Type assetType = System.Type.GetType(typeName);
            if (assetType == null || !typeof(ScriptableObject).IsAssignableFrom(assetType))
                return null;

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            asset = CreateInstance(assetType);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}