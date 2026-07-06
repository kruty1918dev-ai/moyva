using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Окремий menu-only компонент для фону головного меню.
    /// Генерує випадковий світ із GraphAsset і показує його як menu-only фон
    /// без впливу на gameplay системи.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Moyva/Home Menu/Background Preview")]
    public sealed class HomeMenuBackgroundPreviewController : MonoBehaviour
    {
        private const string CloudMipLodShaderName = "Moyva/2D/LayerMipLod";

        [Header("Прев'ю світу")]
        [Tooltip("RawImage, на який буде встановлено згенеровану текстуру карти.")]
        [SerializeField] private RawImage _targetImage;

        [Tooltip("GraphAsset генератора, з якого будується menu preview.")]
        [SerializeField] private GraphAsset _graphAsset;

        [Tooltip("Глобальні Moyva Project Settings, які визначають flat/isometric/hex/3D projection для preview.")]
        [SerializeField] private MoyvaProjectSettingsSO _projectSettings;

        [Tooltip("Опціональний override TileRegistry. Якщо порожньо — береться з GraphAsset.")]
        [SerializeField] private TileRegistrySO _tileRegistryOverride;

        [Tooltip("Реєстр map-об'єктів для overlay шару меню-прев'ю.")]
        [SerializeField] private MapObjectRegistrySO _mapObjectRegistry;

        [Tooltip("Реєстр будівель для overlay шару меню-прев'ю.")]
        [SerializeField] private BuildingRegistrySO _buildingRegistry;

        [Tooltip("Кількість тайлів по ширині та висоті для меню-мапи.")]
        [SerializeField] private Vector2Int _mapTileCount = new Vector2Int(192, 108);

        [Tooltip("Пікселів на один тайл перед масштабуванням текстури.")]
        [SerializeField, Min(1)] private int _pixelsPerTile = 4;

        [Tooltip("Максимальна більша сторона текстури меню, щоб не перевантажувати пристрій.")]
        [SerializeField, Min(64)] private int _maxTextureEdge = 1024;

        [Header("Kingdom Placement (Preview Only)")]
        [Tooltip("Детерміноване розміщення королівств після генерації мапи прев'ю (лише Home Menu).")]
        [SerializeField] private MenuPreviewKingdomPlacementSettings _kingdomPlacement = new MenuPreviewKingdomPlacementSettings();

        [Tooltip("Розтягувати цільовий RawImage на весь батьківський RectTransform.")]
        [SerializeField] private bool _stretchTargetToParent = true;

        [Tooltip("Якщо увімкнено, компонент генерує новий seed щоразу при активації об'єкта.")]
        [SerializeField] private bool _regenerateOnEnable = true;

        [Header("Хмари")]
        [Tooltip("Кореневий RectTransform для UI-хмар. Якщо не задано, створюється автоматично поверх цільового RawImage.")]
        [SerializeField] private RectTransform _cloudsLayer;

        [Tooltip("Налаштування хмаринок з існуючої системи. Меню використовує їх як легку UI-інтерпретацію.")]
        [SerializeField] private CloudsSettings _cloudsSettings;

        [Tooltip("Жорсткий ліміт активних хмар у меню для зниження навантаження.")]
        [SerializeField, Min(0)] private int _menuMaxClouds = 4;

        [Tooltip("Скільки хмаринок створити одразу при запуску меню.")]
        [SerializeField, Min(0)] private int _menuInitialClouds = 3;

        [Tooltip("Відносна висота хмаринки до висоти контейнера.")]
        [SerializeField, Range(0.01f, 0.25f)] private float _cloudHeightRatio = 0.055f;

        [Tooltip("Множник швидкості меню-хмар поверх параметрів CloudsSettings.")]
        [SerializeField, Range(0.1f, 3f)] private float _cloudSpeedMultiplier = 1f;

        private readonly List<MenuCloudVisual> _clouds = new List<MenuCloudVisual>();

        private Texture2D _generatedTexture;
        private Material _runtimeCloudMaterial;
        private System.Random _random;
        private float _spawnTimer;
        private int _pendingInitialClouds;
        private bool _ownsCloudLayer;
        private int _currentSeed;
        private GameObject _livePreviewRoot;
        private Camera _livePreviewCamera;
        private Light _livePreviewLight;
        private IMenuWorldPreviewKingdomPlacementService _kingdomPlacementService;
        private IMenuWorldPreviewTextureBuilderService _textureBuilderService;
        private readonly List<Mesh> _livePreviewMeshes = new List<Mesh>();
        private static MoyvaProjectSettingsSO _runtimeFallbackSettings;

        [Inject]
        public void Construct(
            [InjectOptional] IMenuWorldPreviewKingdomPlacementService kingdomPlacementService = null,
            [InjectOptional] IMenuWorldPreviewTextureBuilderService textureBuilderService = null)
        {
            _kingdomPlacementService = kingdomPlacementService;
            _textureBuilderService = textureBuilderService;
        }

        private void Awake()
        {
            TryAutoAssignTargetImage();
            PrepareTargetImage();
            EnsureCloudLayer();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TryAutoAssignTargetImage();
            TryAutoAssignPreviewRegistries();
            ValidateKingdomPlacementSettings();
        }
#endif

        private void OnEnable()
        {
            TryAutoAssignTargetImage();
            PrepareTargetImage();
            EnsureCloudLayer();

            if (_regenerateOnEnable)
                RegeneratePreview();
            else
                ResetClouds();
        }

        private void Update()
        {
            if (_generatedTexture != null && _targetImage != null && _targetImage.enabled)
                ApplyCoverUv();

            TickClouds(Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            ClearClouds();
            DestroyLiveMeshPreview();
            DisposeGeneratedTexture();
        }

        private void OnDestroy()
        {
            ClearClouds();
            DestroyLiveMeshPreview();
            DisposeGeneratedTexture();
            DestroyRuntimeCloudMaterial();

            if (_ownsCloudLayer && _cloudsLayer != null)
                Destroy(_cloudsLayer.gameObject);
        }

        [ContextMenu("Regenerate Menu Preview")]
        public void RegeneratePreview()
        {
            var projectSettings = ResolveProjectSettings();
            bool useLiveMeshPreview = ShouldUseLiveMeshPreview(projectSettings);

            if (_targetImage == null && !useLiveMeshPreview)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] RawImage не призначено. Прев'ю меню не буде згенеровано.");
                return;
            }

            if (_graphAsset == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] GraphAsset не призначено. Прев'ю меню не буде згенеровано.");
                return;
            }

            var tileRegistry = ResolveTileRegistry();
            if (tileRegistry == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] TileRegistry не знайдено (ні в override, ні в GraphAsset). Прев'ю меню не буде згенеровано.");
                return;
            }

            Vector2Int mapSize = ResolveMapSize();
            int seed = Guid.NewGuid().GetHashCode();

            if (!MenuWorldPreviewGenerator.TryGenerate(_graphAsset, mapSize.x, mapSize.y, seed, out var previewData, out var errorMessage))
            {
                Debug.LogWarning($"[HomeMenuBackgroundPreview] Не вдалося згенерувати прев'ю меню: {errorMessage}");
                return;
            }

            if (_kingdomPlacement != null && _kingdomPlacement.Enabled)
            {
                var placementReport = _kingdomPlacementService != null
                    ? _kingdomPlacementService.Apply(previewData, _kingdomPlacement)
                    : MenuWorldPreviewKingdomPlacer.Apply(previewData, _kingdomPlacement);
                if (!string.IsNullOrWhiteSpace(placementReport.Warning))
                    Debug.LogWarning($"[HomeMenuBackgroundPreview] Kingdom placement warning: {placementReport.Warning}");

                Debug.Log($"[HomeMenuBackgroundPreview] Kingdom placement: {placementReport}");
            }

            if (useLiveMeshPreview && TryBuildLiveMeshPreview(previewData, tileRegistry, projectSettings))
            {
                DisposeGeneratedTexture();
                SetTexturePreviewVisible(false);
                _currentSeed = seed;
                ResetClouds();
                return;
            }

            DestroyLiveMeshPreview();
            SetTexturePreviewVisible(true);

            if (_targetImage == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] RawImage не призначено, а live mesh preview не вдалося побудувати.");
                return;
            }

            var textureRequest = new MenuWorldPreviewTextureBuildRequest(
                previewData,
                tileRegistry,
                _mapObjectRegistry,
                _buildingRegistry,
                _pixelsPerTile,
                _maxTextureEdge,
                projectSettings);
            var texture = _textureBuilderService != null
                ? _textureBuilderService.Build(textureRequest)
                : MenuWorldPreviewTextureBuilder.Build(
                    previewData,
                    tileRegistry,
                    _mapObjectRegistry,
                    _buildingRegistry,
                    _pixelsPerTile,
                    _maxTextureEdge,
                    projectSettings);

            if (texture == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] Texture builder повернув null. Прев'ю меню не оновлено.");
                return;
            }

            DisposeGeneratedTexture();
            _generatedTexture = texture;
            _currentSeed = seed;

            _targetImage.texture = _generatedTexture;
            _targetImage.color = Color.white;
            SetTexturePreviewVisible(true);
            ApplyCoverUv();

            ResetClouds();
        }

        private MoyvaProjectSettingsSO ResolveProjectSettings()
        {
            var settings = _projectSettings != null
                ? _projectSettings
                : _runtimeFallbackSettings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();

            settings.Normalize();
            return settings;
        }

        private static bool ShouldUseLiveMeshPreview(MoyvaProjectSettingsSO projectSettings)
        {
            return projectSettings != null
                && projectSettings.UseLiveHomeMenuMeshPreview
                && projectSettings.EnableMeshPrefabPreviews
                && projectSettings.Uses3DProjectMode();
        }

        private bool TryBuildLiveMeshPreview(MenuWorldPreviewData previewData, TileRegistrySO tileRegistry, MoyvaProjectSettingsSO projectSettings)
        {
            DestroyLiveMeshPreview();

            if (previewData?.BiomeMap == null || tileRegistry?.Definitions == null || projectSettings == null)
                return false;

            var tileMeshCache = BuildTileLiveMeshCache(tileRegistry);
            if (tileMeshCache.Count == 0)
                return false;

            var objectMeshCache = projectSettings.HomeMenuPreviewIncludeObjects
                ? BuildObjectLiveMeshCache(_mapObjectRegistry)
                : new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            var buildingMeshCache = projectSettings.HomeMenuPreviewIncludeBuildings
                ? BuildBuildingLiveMeshCache(_buildingRegistry)
                : new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);

            var projection = GridProjectionFactory.Create(projectSettings);
            int previewLayer = Mathf.Clamp(projectSettings.HomeMenuPreviewLayer, 0, 31);
            int tileStride = ResolveLivePreviewTileStride(previewData, projectSettings);

            _livePreviewRoot = new GameObject("HomeMenuLiveMeshWorld")
            {
                hideFlags = HideFlags.DontSave
            };
            _livePreviewRoot.layer = previewLayer;

            var terrainSurfaceY = new Dictionary<int, float>();
            var builder = new LivePreviewMeshBuilder(_livePreviewRoot.transform, previewLayer, projectSettings, _livePreviewMeshes);

            try
            {
                int terrainCount = AddLiveTerrainMeshes(previewData, tileMeshCache, projection, projectSettings, builder, terrainSurfaceY, tileStride);
                if (terrainCount == 0)
                {
                    DestroyLiveMeshPreview();
                    return false;
                }

                int objectCount = AddLiveOverlayMeshes(previewData.ObjectMap, previewData, objectMeshCache, tileMeshCache, projection,
                    projectSettings, builder, terrainSurfaceY, tileStride);
                int buildingCount = AddLiveOverlayMeshes(previewData.BuildingMap, previewData, buildingMeshCache, tileMeshCache, projection,
                    projectSettings, builder, terrainSurfaceY, tileStride);
                int meshObjectCount = builder.Flush();
                if (meshObjectCount == 0)
                {
                    DestroyLiveMeshPreview();
                    return false;
                }

                ConfigureLivePreviewCamera(builder.WorldBounds, projectSettings, previewLayer);
                ConfigureLivePreviewLight(projectSettings, previewLayer);

                Debug.Log($"[HomeMenuBackgroundPreview] Live 3D mesh preview built: terrain={terrainCount}, objects={objectCount}, buildings={buildingCount}, meshObjects={meshObjectCount}, stride={tileStride}.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[HomeMenuBackgroundPreview] Live 3D mesh preview failed. Falling back to texture preview. {exception.Message}");
                DestroyLiveMeshPreview();
                return false;
            }
        }

        private int AddLiveTerrainMeshes(
            MenuWorldPreviewData previewData,
            Dictionary<string, LivePreviewPrefabMesh> meshCache,
            IGridProjection projection,
            MoyvaProjectSettingsSO projectSettings,
            LivePreviewMeshBuilder builder,
            Dictionary<int, float> terrainSurfaceY,
            int tileStride)
        {
            int width = Mathf.Min(previewData.Width, previewData.BiomeMap.GetLength(0));
            int height = Mathf.Min(previewData.Height, previewData.BiomeMap.GetLength(1));
            int rendered = 0;

            for (int y = 0; y < height; y += tileStride)
            {
                for (int x = 0; x < width; x += tileStride)
                {
                    string id = NormalizePreviewId(previewData.BiomeMap[x, y]);
                    if (string.IsNullOrEmpty(id) || !meshCache.TryGetValue(id, out var prefabMesh))
                        continue;

                    float elevation = ResolvePreviewHeight(previewData, x, y, projectSettings);
                    Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), elevation, 0f);
                    builder.AddPrefab(prefabMesh, Matrix4x4.Translate(worldPosition));
                    terrainSurfaceY[ToPreviewIndex(x, y, previewData.Width)] = worldPosition.y + prefabMesh.Bounds.max.y;
                    rendered++;
                }
            }

            return rendered;
        }

        private int AddLiveOverlayMeshes(
            string[,] map,
            MenuWorldPreviewData previewData,
            Dictionary<string, LivePreviewPrefabMesh> meshCache,
            Dictionary<string, LivePreviewPrefabMesh> tileMeshCache,
            IGridProjection projection,
            MoyvaProjectSettingsSO projectSettings,
            LivePreviewMeshBuilder builder,
            Dictionary<int, float> terrainSurfaceY,
            int tileStride)
        {
            if (map == null || meshCache == null || meshCache.Count == 0)
                return 0;

            int width = Mathf.Min(previewData.Width, map.GetLength(0));
            int height = Mathf.Min(previewData.Height, map.GetLength(1));
            int rendered = 0;

            for (int y = 0; y < height; y += tileStride)
            {
                for (int x = 0; x < width; x += tileStride)
                {
                    string id = NormalizePreviewId(map[x, y]);
                    if (string.IsNullOrEmpty(id) || !meshCache.TryGetValue(id, out var prefabMesh))
                        continue;

                    float elevation = ResolvePreviewHeight(previewData, x, y, projectSettings);
                    Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), elevation, 0f);
                    float surfaceY = ResolveLiveTerrainSurfaceY(previewData, tileMeshCache, projection, projectSettings, terrainSurfaceY, x, y, elevation);
                    worldPosition.y = surfaceY - prefabMesh.Bounds.min.y + GridSurfacePlacementUtility.DefaultSurfaceClearance;
                    builder.AddPrefab(prefabMesh, Matrix4x4.Translate(worldPosition));
                    rendered++;
                }
            }

            return rendered;
        }

        private float ResolveLiveTerrainSurfaceY(
            MenuWorldPreviewData previewData,
            Dictionary<string, LivePreviewPrefabMesh> tileMeshCache,
            IGridProjection projection,
            MoyvaProjectSettingsSO projectSettings,
            Dictionary<int, float> terrainSurfaceY,
            int x,
            int y,
            float elevation)
        {
            int index = ToPreviewIndex(x, y, previewData.Width);
            if (terrainSurfaceY.TryGetValue(index, out float cachedSurfaceY))
                return cachedSurfaceY;

            Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), elevation, 0f);
            string tileId = previewData.BiomeMap != null
                && x >= 0 && y >= 0
                && x < previewData.BiomeMap.GetLength(0)
                && y < previewData.BiomeMap.GetLength(1)
                    ? NormalizePreviewId(previewData.BiomeMap[x, y])
                    : string.Empty;

            if (!string.IsNullOrEmpty(tileId) && tileMeshCache.TryGetValue(tileId, out var tileMesh))
                return worldPosition.y + tileMesh.Bounds.max.y;

            return worldPosition.y + GridSurfacePlacementUtility.DefaultSurfaceClearance;
        }

        private void ConfigureLivePreviewCamera(Bounds worldBounds, MoyvaProjectSettingsSO projectSettings, int previewLayer)
        {
            var cameraObject = new GameObject("HomeMenuLiveMeshPreviewCamera")
            {
                hideFlags = HideFlags.DontSave
            };
            _livePreviewCamera = cameraObject.AddComponent<Camera>();
            _livePreviewCamera.enabled = true;
            _livePreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            _livePreviewCamera.backgroundColor = projectSettings.HomeMenuPreviewBackgroundColor;
            _livePreviewCamera.cullingMask = 1 << previewLayer;
            _livePreviewCamera.depth = projectSettings.HomeMenuPreviewCameraDepth;
            _livePreviewCamera.rect = new Rect(0f, 0f, 1f, 1f);
            _livePreviewCamera.targetTexture = null;
            _livePreviewCamera.allowHDR = false;
            _livePreviewCamera.allowMSAA = false;

            var fogOverride = cameraObject.AddComponent<LivePreviewCameraFogOverride>();
            fogOverride.DisableFog = projectSettings.HomeMenuPreviewDisableFog;

            Quaternion cameraRotation = Quaternion.Euler(projectSettings.ResolvePreviewCameraEuler());
            _livePreviewCamera.transform.rotation = cameraRotation;
            bool usePerspective = projectSettings.ResolveUsePerspectivePreviewCamera();
            _livePreviewCamera.orthographic = !usePerspective;

            float aspect = ResolvePreviewAspect();
            Vector3 viewExtents = ResolveViewExtents(worldBounds, cameraRotation);
            float padding = Mathf.Max(0.01f, projectSettings.HomeMenuPreviewCameraPadding);
            float distance;
            if (usePerspective)
            {
                float fieldOfView = projectSettings.ResolvePreviewPerspectiveFieldOfView();
                _livePreviewCamera.fieldOfView = fieldOfView;
                float halfHeight = Mathf.Max(viewExtents.y, viewExtents.x / Mathf.Max(0.01f, aspect)) * padding;
                distance = halfHeight / Mathf.Tan(Mathf.Max(0.01f, fieldOfView * Mathf.Deg2Rad * 0.5f)) + viewExtents.z;
            }
            else
            {
                _livePreviewCamera.orthographicSize = Mathf.Max(viewExtents.y, viewExtents.x / Mathf.Max(0.01f, aspect)) * padding;
                distance = worldBounds.size.magnitude * 1.75f + 2f;
            }

            distance = Mathf.Max(1f, distance);
            _livePreviewCamera.transform.position = worldBounds.center - _livePreviewCamera.transform.forward * distance;
            _livePreviewCamera.nearClipPlane = 0.01f;
            _livePreviewCamera.farClipPlane = Mathf.Max(distance + worldBounds.size.magnitude * 2f + 10f, 50f);
        }

        private void ConfigureLivePreviewLight(MoyvaProjectSettingsSO projectSettings, int previewLayer)
        {
            var lightObject = new GameObject("HomeMenuLiveMeshPreviewLight")
            {
                hideFlags = HideFlags.DontSave
            };
            _livePreviewLight = lightObject.AddComponent<Light>();
            _livePreviewLight.type = LightType.Directional;
            _livePreviewLight.intensity = projectSettings.ResolvePreviewLightIntensity();
            _livePreviewLight.color = projectSettings.Project3DLightColor;
            _livePreviewLight.cullingMask = 1 << previewLayer;
            _livePreviewLight.shadows = projectSettings.HomeMenuPreviewCastShadows ? LightShadows.Soft : LightShadows.None;
            _livePreviewLight.transform.rotation = Quaternion.Euler(projectSettings.PreviewLightEuler);
        }

        private float ResolvePreviewAspect()
        {
            return Screen.width / Mathf.Max(1f, Screen.height);
        }

        private void DestroyLiveMeshPreview()
        {
            if (_livePreviewRoot != null)
                Destroy(_livePreviewRoot);
            if (_livePreviewCamera != null)
                Destroy(_livePreviewCamera.gameObject);
            if (_livePreviewLight != null)
                Destroy(_livePreviewLight.gameObject);

            for (int i = 0; i < _livePreviewMeshes.Count; i++)
            {
                if (_livePreviewMeshes[i] != null)
                    Destroy(_livePreviewMeshes[i]);
            }

            _livePreviewMeshes.Clear();
            _livePreviewRoot = null;
            _livePreviewCamera = null;
            _livePreviewLight = null;
        }

        private void SetTexturePreviewVisible(bool visible)
        {
            if (_targetImage != null)
                _targetImage.enabled = visible;
        }

        private static int ResolveLivePreviewTileStride(MenuWorldPreviewData previewData, MoyvaProjectSettingsSO projectSettings)
        {
            int stride = Mathf.Max(1, projectSettings.HomeMenuPreviewTileStride);
            int maxTerrainTiles = Mathf.Max(1, projectSettings.HomeMenuPreviewMaxTerrainTiles);
            while (CountSampledCells(previewData.Width, previewData.Height, stride) > maxTerrainTiles)
                stride++;

            return stride;
        }

        private static int CountSampledCells(int width, int height, int stride)
        {
            int sampledWidth = Mathf.CeilToInt(width / Mathf.Max(1f, stride));
            int sampledHeight = Mathf.CeilToInt(height / Mathf.Max(1f, stride));
            return Mathf.Max(1, sampledWidth) * Mathf.Max(1, sampledHeight);
        }

        private static float ResolvePreviewHeight(MenuWorldPreviewData previewData, int x, int y, MoyvaProjectSettingsSO projectSettings)
        {
            if (previewData.HeightMap == null || projectSettings == null || !projectSettings.UseHeightForPreview)
                return 0f;

            if (x < 0 || y < 0 || x >= previewData.HeightMap.GetLength(0) || y >= previewData.HeightMap.GetLength(1))
                return 0f;

            return previewData.HeightMap[x, y];
        }

        private static Dictionary<string, LivePreviewPrefabMesh> BuildTileLiveMeshCache(TileRegistrySO registry)
        {
            var cache = new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = NormalizePreviewId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectLivePreviewPrefab(definition.VisualPrefab, out var prefabMesh))
                    cache[id] = prefabMesh;
            }

            return cache;
        }

        private static Dictionary<string, LivePreviewPrefabMesh> BuildObjectLiveMeshCache(MapObjectRegistrySO registry)
        {
            var cache = new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = NormalizePreviewId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectLivePreviewPrefab(definition.VisualPrefab, out var prefabMesh))
                    cache[id] = prefabMesh;
            }

            return cache;
        }

        private static Dictionary<string, LivePreviewPrefabMesh> BuildBuildingLiveMeshCache(BuildingRegistrySO registry)
        {
            var cache = new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            var definitions = registry?.GetAll();
            if (definitions == null)
                return cache;

            foreach (var definition in definitions)
            {
                string id = NormalizePreviewId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectLivePreviewPrefab(definition.Prefab, out var prefabMesh))
                    cache[id] = prefabMesh;
            }

            return cache;
        }

        private static bool TryCollectLivePreviewPrefab(GameObject prefab, out LivePreviewPrefabMesh prefabMesh)
        {
            prefabMesh = null;
            if (prefab == null)
                return false;

            var draws = new List<LivePreviewMeshDraw>();
            Bounds bounds = default;
            bool hasBounds = false;
            Matrix4x4 rootWorldToLocal = prefab.transform.worldToLocalMatrix;

            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var renderer = meshRenderers[i];
                var filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
                Mesh mesh = filter != null ? filter.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                Bounds localBounds = TransformBounds(localMatrix, mesh.bounds);
                draws.Add(new LivePreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix, localBounds));
                EncapsulateBounds(ref bounds, ref hasBounds, localBounds);
            }

            var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                var renderer = skinnedRenderers[i];
                Mesh mesh = renderer != null ? renderer.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                Bounds localBounds = TransformBounds(localMatrix, renderer.localBounds);
                draws.Add(new LivePreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix, localBounds));
                EncapsulateBounds(ref bounds, ref hasBounds, localBounds);
            }

            if (draws.Count == 0 || !hasBounds || !IsFinite(bounds.center) || !IsFinite(bounds.size) || bounds.size.sqrMagnitude <= 0.0001f)
                return false;

            prefabMesh = new LivePreviewPrefabMesh(draws, bounds);
            return true;
        }

        private static Vector3 ResolveViewExtents(Bounds bounds, Quaternion cameraRotation)
        {
            Quaternion worldToView = Quaternion.Inverse(cameraRotation);
            Vector3 extents = bounds.extents;
            float maxX = 0.01f;
            float maxY = 0.01f;
            float maxZ = 0.01f;

            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = new Vector3(extents.x * x, extents.y * y, extents.z * z);
                Vector3 view = worldToView * corner;
                maxX = Mathf.Max(maxX, Mathf.Abs(view.x));
                maxY = Mathf.Max(maxY, Mathf.Abs(view.y));
                maxZ = Mathf.Max(maxZ, Mathf.Abs(view.z));
            }

            return new Vector3(maxX, maxY, maxZ);
        }

        private static Bounds TransformBounds(Matrix4x4 matrix, Bounds source)
        {
            Vector3 center = matrix.MultiplyPoint3x4(source.center);
            Vector3 extents = source.extents;
            Vector3 axisX = matrix.MultiplyVector(new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(new Vector3(0f, 0f, extents.z));
            extents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, extents * 2f);
        }

        private static void EncapsulateBounds(ref Bounds bounds, ref bool hasBounds, Bounds addition)
        {
            if (!hasBounds)
            {
                bounds = addition;
                hasBounds = true;
                return;
            }

            bounds.Encapsulate(addition);
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
                && !float.IsNaN(value.y) && !float.IsInfinity(value.y)
                && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }

        private static string NormalizePreviewId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static int ToPreviewIndex(int x, int y, int width)
        {
            return y * Mathf.Max(1, width) + x;
        }

        private Vector2Int ResolveMapSize()
        {
            if (_mapTileCount.x > 0 && _mapTileCount.y > 0)
                return new Vector2Int(_mapTileCount.x, _mapTileCount.y);

            if (_graphAsset != null && _graphAsset.SharedSettings != null && _graphAsset.SharedSettings.HasMapSize)
                return _graphAsset.SharedSettings.MapSize;

            return new Vector2Int(128, 72);
        }

        private TileRegistrySO ResolveTileRegistry()
        {
            if (_tileRegistryOverride != null)
                return _tileRegistryOverride;

            return _graphAsset != null ? _graphAsset.TileRegistry : null;
        }

        [ContextMenu("Validate Kingdom Placement Rules")]
        private void ValidateKingdomPlacementRules()
        {
            ValidateKingdomPlacementSettings();

            if (_kingdomPlacement == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] Kingdom placement settings is null.");
                return;
            }

            string issues = BuildKingdomPlacementIssues();
            if (string.IsNullOrEmpty(issues))
            {
                Debug.Log("[HomeMenuBackgroundPreview] Kingdom placement rules look valid.");
                return;
            }

            Debug.LogWarning($"[HomeMenuBackgroundPreview] Kingdom placement rules warnings:\n{issues}");
        }

        private void ValidateKingdomPlacementSettings()
        {
            if (_kingdomPlacement == null)
                _kingdomPlacement = new MenuPreviewKingdomPlacementSettings();

            _kingdomPlacement.ClampAndNormalize();
        }

        private string BuildKingdomPlacementIssues()
        {
            if (_kingdomPlacement == null)
                return "- Settings are missing.";

            var issues = new List<string>();

            if (_kingdomPlacement.KingdomAZone.Overlaps(_kingdomPlacement.KingdomBZone))
                issues.Add("- KingdomAZone overlaps KingdomBZone.");

            if (string.IsNullOrWhiteSpace(_kingdomPlacement.CastleBuildingId))
                issues.Add("- CastleBuildingId is empty.");

            if (string.IsNullOrWhiteSpace(_kingdomPlacement.TownHallBuildingId))
                issues.Add("- TownHallBuildingId is empty.");

            if (_kingdomPlacement.MinHeight > _kingdomPlacement.MaxHeight)
                issues.Add("- MinHeight is greater than MaxHeight.");

            return issues.Count == 0 ? string.Empty : string.Join("\n", issues);
        }

#if UNITY_EDITOR
        private void TryAutoAssignPreviewRegistries()
        {
            if (_graphAsset == null)
                return;

            string graphPath = AssetDatabase.GetAssetPath(_graphAsset);
            if (string.IsNullOrEmpty(graphPath))
                return;

            string graphDirectory = System.IO.Path.GetDirectoryName(graphPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(graphDirectory))
                return;

            bool changed = false;
            if (_projectSettings == null)
            {
                var settings = AssetDatabase.LoadAssetAtPath<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);
                if (settings != null)
                {
                    _projectSettings = settings;
                    changed = true;
                }
            }

            if (_mapObjectRegistry == null || _buildingRegistry == null)
            {
                var previewSettings = AssetDatabase.LoadAssetAtPath<ScriptableObject>($"{graphDirectory}/EditorPreviewSettings.asset");
                if (previewSettings != null)
                {
                    var serializedPreviewSettings = new SerializedObject(previewSettings);
                    if (_mapObjectRegistry == null)
                    {
                        var mapObjectRegistryProperty = serializedPreviewSettings.FindProperty("_mapObjectRegistry");
                        if (mapObjectRegistryProperty?.objectReferenceValue is MapObjectRegistrySO mapObjectRegistry)
                        {
                            _mapObjectRegistry = mapObjectRegistry;
                            changed = true;
                        }
                    }

                    if (_buildingRegistry == null)
                    {
                        var buildingRegistryProperty = serializedPreviewSettings.FindProperty("_buildingRegistry");
                        if (buildingRegistryProperty?.objectReferenceValue is BuildingRegistrySO buildingRegistry)
                        {
                            _buildingRegistry = buildingRegistry;
                            changed = true;
                        }
                    }
                }
            }

            if (_mapObjectRegistry == null)
            {
                var siblingRegistry = AssetDatabase.LoadAssetAtPath<MapObjectRegistrySO>($"{graphDirectory}/MapObjectRegistry.asset");
                if (siblingRegistry != null)
                {
                    _mapObjectRegistry = siblingRegistry;
                    changed = true;
                }
            }

            if (_buildingRegistry == null)
            {
                string[] buildingRegistryGuids = AssetDatabase.FindAssets($"t:{nameof(BuildingRegistrySO)}");
                if (buildingRegistryGuids.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(buildingRegistryGuids[0]);
                    var registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(assetPath);
                    if (registry != null)
                    {
                        _buildingRegistry = registry;
                        changed = true;
                    }
                }
            }

            if (changed)
                EditorUtility.SetDirty(this);
        }
#endif

        private void TryAutoAssignTargetImage()
        {
            if (_targetImage == null)
                _targetImage = GetComponent<RawImage>();
        }

        private void PrepareTargetImage()
        {
            if (_targetImage == null)
                return;

            if (_stretchTargetToParent)
            {
                RectTransform targetRect = _targetImage.rectTransform;
                targetRect.anchorMin = Vector2.zero;
                targetRect.anchorMax = Vector2.one;
                targetRect.offsetMin = Vector2.zero;
                targetRect.offsetMax = Vector2.zero;
                targetRect.pivot = new Vector2(0.5f, 0.5f);
            }

            _targetImage.uvRect = new Rect(0f, 0f, 1f, 1f);
        }

        private void ApplyCoverUv()
        {
            if (_targetImage == null || _generatedTexture == null)
                return;

            var rect = _targetImage.rectTransform.rect;
            float viewportWidth = rect.width > 1f ? rect.width : Screen.width;
            float viewportHeight = rect.height > 1f ? rect.height : Screen.height;
            if (viewportWidth <= 1f || viewportHeight <= 1f)
            {
                _targetImage.uvRect = new Rect(0f, 0f, 1f, 1f);
                return;
            }

            float viewportAspect = viewportWidth / viewportHeight;
            float textureAspect = _generatedTexture.width / (float)_generatedTexture.height;

            Rect uv = new Rect(0f, 0f, 1f, 1f);
            if (viewportAspect > textureAspect)
            {
                float uvHeight = Mathf.Clamp01(textureAspect / viewportAspect);
                uv.y = (1f - uvHeight) * 0.5f;
                uv.height = uvHeight;
            }
            else
            {
                float uvWidth = Mathf.Clamp01(viewportAspect / textureAspect);
                uv.x = (1f - uvWidth) * 0.5f;
                uv.width = uvWidth;
            }

            _targetImage.uvRect = uv;
        }

        private void EnsureCloudLayer()
        {
            if (_cloudsLayer != null)
                return;

            Transform parent = null;
            if (_targetImage != null && _targetImage.rectTransform.parent != null)
                parent = _targetImage.rectTransform.parent;
            else if (transform is RectTransform)
                parent = transform.parent != null ? transform.parent : transform;

            if (parent == null)
                return;

            var existing = parent.Find("MenuCloudsLayer");
            if (existing is RectTransform existingRect)
            {
                _cloudsLayer = existingRect;
                return;
            }

            var cloudLayerObject = new GameObject("MenuCloudsLayer", typeof(RectTransform));
            _cloudsLayer = cloudLayerObject.GetComponent<RectTransform>();
            _cloudsLayer.SetParent(parent, false);
            _cloudsLayer.anchorMin = Vector2.zero;
            _cloudsLayer.anchorMax = Vector2.one;
            _cloudsLayer.offsetMin = Vector2.zero;
            _cloudsLayer.offsetMax = Vector2.zero;
            _cloudsLayer.SetAsLastSibling();
            _ownsCloudLayer = true;
        }

        private void ResetClouds()
        {
            ClearClouds();

            if (_cloudsSettings == null || !_cloudsSettings.Enabled || _cloudsLayer == null)
                return;

            _random = new System.Random(_currentSeed == 0 ? Guid.NewGuid().GetHashCode() : _currentSeed ^ 0x5f3759df);
            _pendingInitialClouds = Mathf.Min(ResolveEffectiveInitialClouds(), ResolveEffectiveMaxClouds());
            ResetSpawnTimer();
            TrySpawnInitialClouds();
        }

        private void TickClouds(float deltaTime)
        {
            if (_cloudsSettings == null || !_cloudsSettings.Enabled || _cloudsLayer == null)
                return;

            if (_cloudsLayer.rect.width <= 1f || _cloudsLayer.rect.height <= 1f)
                return;

            TrySpawnInitialClouds();

            for (int i = _clouds.Count - 1; i >= 0; i--)
            {
                var cloud = _clouds[i];
                if (cloud.RootRectTransform == null || cloud.CloudImage == null)
                {
                    DestroyMenuCloud(cloud);
                    _clouds.RemoveAt(i);
                    continue;
                }

                cloud.Age += deltaTime;
                var position = cloud.RootRectTransform.anchoredPosition;
                position.x += cloud.Direction * cloud.Speed * deltaTime;
                cloud.RootRectTransform.anchoredPosition = position;

                float fadeDuration = Mathf.Max(0.01f, _cloudsSettings.FadeDuration);
                float fadeIn = Mathf.Clamp01(cloud.Age / fadeDuration);
                float fadeDistance = Mathf.Max(1f, cloud.Speed * fadeDuration);
                float distanceToEnd = Mathf.Abs(cloud.EndX - position.x);
                float fadeOut = Mathf.Clamp01(distanceToEnd / fadeDistance);
                float alpha = Mathf.Min(fadeIn, fadeOut) * Mathf.Clamp01(_cloudsSettings.CloudAlpha);

                var color = _cloudsSettings.CloudColor;
                color.a = alpha;
                cloud.CloudImage.color = color;

                if (cloud.ShadowImage != null)
                {
                    var shadowColor = _cloudsSettings.ShadowColor;
                    shadowColor.a *= alpha * ResolveMenuShadowAlphaMultiplier(cloud.VisualHeight);
                    cloud.ShadowImage.color = shadowColor;
                }

                if ((cloud.Direction > 0 && position.x >= cloud.EndX) || (cloud.Direction < 0 && position.x <= cloud.EndX))
                {
                    DestroyMenuCloud(cloud);
                    _clouds.RemoveAt(i);
                }
            }

            if (_clouds.Count >= ResolveEffectiveMaxClouds())
                return;

            _spawnTimer -= deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnCloud(startInView: false);
                ResetSpawnTimer();
            }
        }

        private void TrySpawnInitialClouds()
        {
            if (_pendingInitialClouds <= 0 || _cloudsLayer == null || _cloudsLayer.rect.width <= 1f || _cloudsLayer.rect.height <= 1f)
                return;

            while (_pendingInitialClouds > 0 && _clouds.Count < ResolveEffectiveMaxClouds())
            {
                if (!SpawnCloud(startInView: true))
                    return;

                _pendingInitialClouds--;
            }
        }

        private bool SpawnCloud(bool startInView)
        {
            if (!TryPickSprite(out var sprite) || _cloudsLayer == null)
                return false;

            var bounds = _cloudsLayer.rect;
            if (bounds.width <= 1f || bounds.height <= 1f)
                return false;

            int direction = NextFloat() <= _cloudsSettings.LeftToRightChance ? 1 : -1;

            float left = -bounds.width * 0.5f;
            float right = bounds.width * 0.5f;
            float probeCloudHeight = bounds.height * _cloudHeightRatio * Mathf.Max(0.35f, _cloudsSettings.ScaleRange.y);
            float verticalPadding = probeCloudHeight * Mathf.Max(0f, _cloudsSettings.SpawnVerticalPadding) * 0.4f;
            float x = startInView
                ? Mathf.Lerp(left, right, NextFloat())
                : direction > 0 ? left - probeCloudHeight : right + probeCloudHeight;
            float y = Mathf.Lerp(-bounds.height * 0.5f - verticalPadding, bounds.height * 0.5f + verticalPadding, NextFloat());
            float altitude01 = ResolveMenuAltitude01(bounds, y);
            float scale = _cloudsSettings.EvaluateCloudScale(altitude01, NextFloat());
            scale = Mathf.Clamp(scale, 0.35f, 1.35f);

            float cloudVisualHeight = _cloudsSettings.EvaluateCloudVisualHeight(altitude01);
            float mipBias = _cloudsSettings.EvaluateCloudMipBias(altitude01);
            float cloudHeight = bounds.height * _cloudHeightRatio * scale;
            float aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;
            float cloudWidth = cloudHeight * aspect;
            float endX = direction > 0 ? right + cloudWidth : left - cloudWidth;
            float speed = Mathf.Lerp(_cloudsSettings.SpeedRange.x, _cloudsSettings.SpeedRange.y, NextFloat())
                * Mathf.Max(8f, bounds.width / 28f)
                * Mathf.Max(0.1f, _cloudSpeedMultiplier);

            var cloudRoot = new GameObject("MenuCloud", typeof(RectTransform));
            var rectTransform = cloudRoot.GetComponent<RectTransform>();
            rectTransform.SetParent(_cloudsLayer, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(cloudWidth, cloudHeight);
            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.SetAsLastSibling();

            Material cloudMaterial = CreateCloudMaterialInstance(mipBias);
            Image shadowImage = null;
            if (_cloudsSettings.ShadowsEnabled)
            {
                shadowImage = CreateMenuCloudImage(
                    rectTransform,
                    "Shadow",
                    sprite,
                    cloudMaterial,
                    ResolveMenuShadowOffset(cloudHeight, cloudVisualHeight),
                    new Vector2(cloudWidth, cloudHeight) * ResolveMenuShadowScaleMultiplier(cloudVisualHeight),
                    new Color(
                        _cloudsSettings.ShadowColor.r,
                        _cloudsSettings.ShadowColor.g,
                        _cloudsSettings.ShadowColor.b,
                        startInView ? _cloudsSettings.CloudAlpha * ResolveMenuShadowAlphaMultiplier(cloudVisualHeight) : 0f));
            }

            var image = CreateMenuCloudImage(
                rectTransform,
                "Sprite",
                sprite,
                cloudMaterial,
                Vector2.zero,
                new Vector2(cloudWidth, cloudHeight),
                new Color(_cloudsSettings.CloudColor.r, _cloudsSettings.CloudColor.g, _cloudsSettings.CloudColor.b, startInView ? _cloudsSettings.CloudAlpha : 0f));

            _clouds.Add(new MenuCloudVisual
            {
                RootRectTransform = rectTransform,
                CloudImage = image,
                ShadowImage = shadowImage,
                MaterialInstance = cloudMaterial,
                Speed = speed,
                EndX = endX,
                Direction = direction,
                Age = startInView ? Mathf.Max(0.01f, _cloudsSettings.FadeDuration) : 0f,
                VisualHeight = cloudVisualHeight
            });

            return true;
        }

        private void ClearClouds()
        {
            for (int i = 0; i < _clouds.Count; i++)
                DestroyMenuCloud(_clouds[i]);

            _clouds.Clear();
        }

        private void DisposeGeneratedTexture()
        {
            if (_targetImage != null && ReferenceEquals(_targetImage.texture, _generatedTexture))
                _targetImage.texture = null;

            if (_generatedTexture != null)
                Destroy(_generatedTexture);

            _generatedTexture = null;
        }

        private static float ResolveMenuAltitude01(Rect bounds, float y)
        {
            float minY = -bounds.height * 0.5f;
            float maxY = bounds.height * 0.5f;
            float height = Mathf.Max(0.001f, maxY - minY);
            return Mathf.Clamp01((y - minY) / height);
        }

        private Image CreateMenuCloudImage(
            RectTransform parent,
            string name,
            Sprite sprite,
            Material material,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.SetParent(parent, false);
            imageRect.anchorMin = new Vector2(0.5f, 0.5f);
            imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = anchoredPosition;
            imageRect.sizeDelta = size;

            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.material = material;
            image.color = color;
            return image;
        }

        private Material CreateCloudMaterialInstance(float mipBias)
        {
            Material baseMaterial = ResolveCloudMaterial();
            if (baseMaterial == null)
                return null;

            var material = new Material(baseMaterial)
            {
                name = "MenuCloudRuntimeMaterialInstance",
                hideFlags = HideFlags.HideAndDontSave,
            };

            if (material.HasProperty("_MipBias"))
                material.SetFloat("_MipBias", mipBias);

            return material;
        }

        private Vector2 ResolveMenuShadowOffset(float cloudPixelHeight, float cloudVisualHeight)
        {
            Vector2 worldOffset = _cloudsSettings.ShadowOffset + _cloudsSettings.ShadowOffsetPerHeight * cloudVisualHeight;
            float pixelFactor = Mathf.Max(1f, cloudPixelHeight * 0.35f);
            return worldOffset * pixelFactor;
        }

        private float ResolveMenuShadowScaleMultiplier(float cloudVisualHeight)
        {
            return Mathf.Max(0.01f, _cloudsSettings.ShadowScaleMultiplier + _cloudsSettings.ShadowScalePerHeight * cloudVisualHeight);
        }

        private float ResolveMenuShadowAlphaMultiplier(float cloudVisualHeight)
        {
            return Mathf.Clamp01(_cloudsSettings.ShadowAlphaMultiplier / (1f + cloudVisualHeight * _cloudsSettings.ShadowAlphaHeightFade));
        }

        private void DestroyMenuCloud(MenuCloudVisual cloud)
        {
            if (cloud?.MaterialInstance != null)
                Destroy(cloud.MaterialInstance);

            if (cloud?.RootRectTransform != null)
                Destroy(cloud.RootRectTransform.gameObject);
        }

        private int ResolveEffectiveMaxClouds()
        {
            if (_cloudsSettings == null)
                return 0;

            return Mathf.Max(0, Mathf.Min(_menuMaxClouds, _cloudsSettings.MaxActiveClouds));
        }

        private int ResolveEffectiveInitialClouds()
        {
            if (_cloudsSettings == null)
                return 0;

            return Mathf.Max(0, Mathf.Min(_menuInitialClouds, _cloudsSettings.InitialClouds));
        }

        private void ResetSpawnTimer()
        {
            if (_cloudsSettings == null)
            {
                _spawnTimer = 0f;
                return;
            }

            _spawnTimer = Mathf.Lerp(_cloudsSettings.SpawnIntervalRange.x, _cloudsSettings.SpawnIntervalRange.y, NextFloat());
        }

        private bool TryPickSprite(out Sprite sprite)
        {
            sprite = null;
            if (_cloudsSettings?.CloudSprites == null || _cloudsSettings.CloudSprites.Length == 0)
                return false;

            float totalWeight = 0f;
            for (int i = 0; i < _cloudsSettings.CloudSprites.Length; i++)
            {
                var variant = _cloudsSettings.CloudSprites[i];
                if (variant?.Sprite == null || variant.Chance <= 0f)
                    continue;

                totalWeight += variant.Chance;
            }

            if (totalWeight <= 0f)
                return false;

            float roll = NextFloat() * totalWeight;
            float accumulated = 0f;
            for (int i = 0; i < _cloudsSettings.CloudSprites.Length; i++)
            {
                var variant = _cloudsSettings.CloudSprites[i];
                if (variant?.Sprite == null || variant.Chance <= 0f)
                    continue;

                accumulated += variant.Chance;
                if (roll <= accumulated)
                {
                    sprite = variant.Sprite;
                    return true;
                }
            }

            return false;
        }

        private float NextFloat()
        {
            if (_random == null)
                _random = new System.Random(Guid.NewGuid().GetHashCode());

            return (float)_random.NextDouble();
        }

        private Material ResolveCloudMaterial()
        {
            if (_cloudsSettings != null && _cloudsSettings.SpriteMaterial != null)
                return _cloudsSettings.SpriteMaterial;

            if (_runtimeCloudMaterial != null)
                return _runtimeCloudMaterial;

            Shader shader = Shader.Find(CloudMipLodShaderName);
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            if (shader == null)
                return null;

            _runtimeCloudMaterial = new Material(shader)
            {
                name = "MenuCloudsRuntimeMaterial",
                hideFlags = HideFlags.HideAndDontSave,
            };
            return _runtimeCloudMaterial;
        }

        private void DestroyRuntimeCloudMaterial()
        {
            if (_runtimeCloudMaterial == null)
                return;

            Destroy(_runtimeCloudMaterial);
            _runtimeCloudMaterial = null;
        }

        private sealed class LivePreviewMeshBuilder
        {
            private readonly Transform _parent;
            private readonly int _layer;
            private readonly bool _combineMeshes;
            private readonly bool _uploadMeshData;
            private readonly bool _castShadows;
            private readonly bool _receiveShadows;
            private readonly int _maxVerticesPerBatch;
            private readonly int _maxMaterialBatches;
            private readonly List<Mesh> _ownedMeshes;
            private readonly Dictionary<Material, List<LivePreviewMeshBatch>> _batches = new Dictionary<Material, List<LivePreviewMeshBatch>>();
            private Bounds _worldBounds;
            private bool _hasBounds;
            private int _meshObjectCount;

            public LivePreviewMeshBuilder(Transform parent, int layer, MoyvaProjectSettingsSO settings, List<Mesh> ownedMeshes)
            {
                _parent = parent;
                _layer = layer;
                _combineMeshes = settings.HomeMenuPreviewCombineMeshesByMaterial;
                _uploadMeshData = settings.HomeMenuPreviewUploadMeshData;
                _castShadows = settings.HomeMenuPreviewCastShadows;
                _receiveShadows = settings.HomeMenuPreviewReceiveShadows;
                _maxVerticesPerBatch = Mathf.Max(1024, settings.HomeMenuPreviewMaxVerticesPerBatch);
                _maxMaterialBatches = Mathf.Max(1, settings.HomeMenuPreviewMaxMaterialBatches);
                _ownedMeshes = ownedMeshes;
                _worldBounds = new Bounds(Vector3.zero, Vector3.one);
            }

            public Bounds WorldBounds => _hasBounds ? _worldBounds : new Bounds(Vector3.zero, Vector3.one);

            public void AddPrefab(LivePreviewPrefabMesh prefabMesh, Matrix4x4 rootMatrix)
            {
                if (prefabMesh == null || prefabMesh.Draws == null)
                    return;

                Bounds prefabWorldBounds = TransformBounds(rootMatrix, prefabMesh.Bounds);
                EncapsulateBounds(ref _worldBounds, ref _hasBounds, prefabWorldBounds);

                for (int i = 0; i < prefabMesh.Draws.Count; i++)
                {
                    var draw = prefabMesh.Draws[i];
                    Matrix4x4 matrix = rootMatrix * draw.LocalMatrix;
                    if (_combineMeshes)
                        AddDrawToBatches(draw, matrix);
                    else
                        CreateDrawObject(draw, matrix);
                }
            }

            public int Flush()
            {
                if (!_combineMeshes)
                    return _meshObjectCount;

                foreach (var pair in _batches)
                {
                    var materialBatches = pair.Value;
                    for (int i = 0; i < materialBatches.Count; i++)
                        CreateCombinedObject(pair.Key, materialBatches[i], i);
                }

                _batches.Clear();
                return _meshObjectCount;
            }

            private void AddDrawToBatches(LivePreviewMeshDraw draw, Matrix4x4 matrix)
            {
                if (draw.Mesh == null || draw.Materials == null || draw.Materials.Length == 0)
                    return;

                int subMeshCount = draw.Mesh.subMeshCount;
                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    Material material = draw.Materials[Mathf.Min(subMeshIndex, draw.Materials.Length - 1)];
                    if (material == null || !TryGetWritableBatch(material, draw.Mesh.vertexCount, out var batch))
                        continue;

                    batch.Instances.Add(new CombineInstance
                    {
                        mesh = draw.Mesh,
                        subMeshIndex = subMeshIndex,
                        transform = matrix
                    });
                    batch.VertexCount += draw.Mesh.vertexCount;
                }
            }

            private bool TryGetWritableBatch(Material material, int vertexCount, out LivePreviewMeshBatch batch)
            {
                if (!_batches.TryGetValue(material, out var materialBatches))
                {
                    if (_batches.Count >= _maxMaterialBatches)
                    {
                        batch = null;
                        return false;
                    }

                    materialBatches = new List<LivePreviewMeshBatch>();
                    _batches.Add(material, materialBatches);
                }

                if (materialBatches.Count == 0
                    || materialBatches[materialBatches.Count - 1].VertexCount + vertexCount > _maxVerticesPerBatch)
                {
                    materialBatches.Add(new LivePreviewMeshBatch());
                }

                batch = materialBatches[materialBatches.Count - 1];
                return true;
            }

            private void CreateCombinedObject(Material material, LivePreviewMeshBatch batch, int batchIndex)
            {
                if (batch == null || batch.Instances.Count == 0 || material == null)
                    return;

                var mesh = new Mesh
                {
                    name = $"HomeMenuPreview_{SanitizeName(material.name)}_{batchIndex}",
                    hideFlags = HideFlags.DontSave
                };
                mesh.indexFormat = batch.VertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.CombineMeshes(batch.Instances.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
                mesh.RecalculateBounds();
                if (_uploadMeshData)
                    mesh.UploadMeshData(markNoLongerReadable: true);

                _ownedMeshes.Add(mesh);

                var meshObject = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer))
                {
                    hideFlags = HideFlags.DontSave,
                    layer = _layer
                };
                meshObject.transform.SetParent(_parent, worldPositionStays: false);
                meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;

                var renderer = meshObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                ApplyRendererSettings(renderer);
                _meshObjectCount++;
            }

            private void CreateDrawObject(LivePreviewMeshDraw draw, Matrix4x4 matrix)
            {
                if (draw.Mesh == null || draw.Materials == null || draw.Materials.Length == 0)
                    return;

                var meshObject = new GameObject($"HomeMenuPreview_{draw.Mesh.name}", typeof(MeshFilter), typeof(MeshRenderer))
                {
                    hideFlags = HideFlags.DontSave,
                    layer = _layer
                };
                meshObject.transform.SetParent(_parent, worldPositionStays: true);
                ApplyMatrix(meshObject.transform, matrix);
                meshObject.GetComponent<MeshFilter>().sharedMesh = draw.Mesh;

                var renderer = meshObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterials = draw.Materials;
                ApplyRendererSettings(renderer);
                _meshObjectCount++;
            }

            private void ApplyRendererSettings(Renderer renderer)
            {
                renderer.shadowCastingMode = _castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = _receiveShadows;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }

            private static void ApplyMatrix(Transform target, Matrix4x4 matrix)
            {
                Vector3 position = matrix.GetColumn(3);
                Vector3 right = matrix.GetColumn(0);
                Vector3 up = matrix.GetColumn(1);
                Vector3 forward = matrix.GetColumn(2);

                Vector3 scale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);
                if (scale.x > 0.0001f) right /= scale.x;
                if (scale.y > 0.0001f) up /= scale.y;
                if (scale.z > 0.0001f) forward /= scale.z;

                target.position = position;
                target.rotation = Quaternion.LookRotation(forward.sqrMagnitude > 0.0001f ? forward : Vector3.forward,
                    up.sqrMagnitude > 0.0001f ? up : Vector3.up);
                target.localScale = scale;
            }

            private static string SanitizeName(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return "Material";

                return value.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
            }
        }

        private sealed class LivePreviewMeshBatch
        {
            public readonly List<CombineInstance> Instances = new List<CombineInstance>();
            public int VertexCount;
        }

        private sealed class LivePreviewPrefabMesh
        {
            public LivePreviewPrefabMesh(List<LivePreviewMeshDraw> draws, Bounds bounds)
            {
                Draws = draws;
                Bounds = bounds;
            }

            public List<LivePreviewMeshDraw> Draws { get; }
            public Bounds Bounds { get; }
        }

        private readonly struct LivePreviewMeshDraw
        {
            public LivePreviewMeshDraw(Mesh mesh, Material[] materials, Matrix4x4 localMatrix, Bounds localBounds)
            {
                Mesh = mesh;
                Materials = materials;
                LocalMatrix = localMatrix;
                LocalBounds = localBounds;
            }

            public Mesh Mesh { get; }
            public Material[] Materials { get; }
            public Matrix4x4 LocalMatrix { get; }
            public Bounds LocalBounds { get; }
        }

        private sealed class LivePreviewCameraFogOverride : MonoBehaviour
        {
            private bool _previousFog;
            private bool _hasPreviousFog;

            public bool DisableFog { get; set; }

            private void OnPreCull()
            {
                if (!DisableFog)
                    return;

                _previousFog = RenderSettings.fog;
                _hasPreviousFog = true;
                RenderSettings.fog = false;
            }

            private void OnPostRender()
            {
                RestoreFog();
            }

            private void OnDisable()
            {
                RestoreFog();
            }

            private void RestoreFog()
            {
                if (!_hasPreviousFog)
                    return;

                RenderSettings.fog = _previousFog;
                _hasPreviousFog = false;
            }
        }

        private sealed class MenuCloudVisual
        {
            public RectTransform RootRectTransform;
            public Image CloudImage;
            public Image ShadowImage;
            public Material MaterialInstance;
            public float Speed;
            public float EndX;
            public int Direction;
            public float Age;
            public float VisualHeight;
        }
    }
}
