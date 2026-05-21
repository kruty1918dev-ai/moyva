using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Окремий menu-only компонент для фону головного меню.
    /// Генерує випадковий світ із GraphAsset, рендерить його в Texture2D
    /// і показує в RawImage без впливу на gameplay системи.
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
            if (_generatedTexture != null)
                ApplyCoverUv();

            TickClouds(Time.unscaledDeltaTime);
        }

        private void OnDisable()
        {
            ClearClouds();
            DisposeGeneratedTexture();
        }

        private void OnDestroy()
        {
            ClearClouds();
            DisposeGeneratedTexture();
            DestroyRuntimeCloudMaterial();

            if (_ownsCloudLayer && _cloudsLayer != null)
                Destroy(_cloudsLayer.gameObject);
        }

        [ContextMenu("Regenerate Menu Preview")]
        public void RegeneratePreview()
        {
            if (_targetImage == null)
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
                var placementReport = MenuWorldPreviewKingdomPlacer.Apply(previewData, _kingdomPlacement);
                if (!string.IsNullOrWhiteSpace(placementReport.Warning))
                    Debug.LogWarning($"[HomeMenuBackgroundPreview] Kingdom placement warning: {placementReport.Warning}");

                Debug.Log($"[HomeMenuBackgroundPreview] Kingdom placement: {placementReport}");
            }

            var texture = MenuWorldPreviewTextureBuilder.Build(
                previewData,
                tileRegistry,
                _mapObjectRegistry,
                _buildingRegistry,
                _pixelsPerTile,
                _maxTextureEdge);

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
            ApplyCoverUv();

            ResetClouds();
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
                if (cloud.RectTransform == null || cloud.Image == null)
                {
                    _clouds.RemoveAt(i);
                    continue;
                }

                cloud.Age += deltaTime;
                var position = cloud.RectTransform.anchoredPosition;
                position.x += cloud.Direction * cloud.Speed * deltaTime;
                cloud.RectTransform.anchoredPosition = position;

                float fadeDuration = Mathf.Max(0.01f, _cloudsSettings.FadeDuration);
                float fadeIn = Mathf.Clamp01(cloud.Age / fadeDuration);
                float fadeDistance = Mathf.Max(1f, cloud.Speed * fadeDuration);
                float distanceToEnd = Mathf.Abs(cloud.EndX - position.x);
                float fadeOut = Mathf.Clamp01(distanceToEnd / fadeDistance);
                float alpha = Mathf.Min(fadeIn, fadeOut) * Mathf.Clamp01(_cloudsSettings.CloudAlpha);

                var color = _cloudsSettings.CloudColor;
                color.a = alpha;
                cloud.Image.color = color;

                if ((cloud.Direction > 0 && position.x >= cloud.EndX) || (cloud.Direction < 0 && position.x <= cloud.EndX))
                {
                    Destroy(cloud.RectTransform.gameObject);
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

            float scale = Mathf.Lerp(_cloudsSettings.ScaleRange.x, _cloudsSettings.ScaleRange.y, NextFloat());
            scale = Mathf.Clamp(scale, 0.35f, 1.35f);

            float cloudHeight = bounds.height * _cloudHeightRatio * scale;
            float aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;
            float cloudWidth = cloudHeight * aspect;
            int direction = NextFloat() <= _cloudsSettings.LeftToRightChance ? 1 : -1;

            float left = -bounds.width * 0.5f;
            float right = bounds.width * 0.5f;
            float verticalPadding = cloudHeight * Mathf.Max(0f, _cloudsSettings.SpawnVerticalPadding) * 0.4f;
            float x = startInView
                ? Mathf.Lerp(left, right, NextFloat())
                : direction > 0 ? left - cloudWidth : right + cloudWidth;
            float y = Mathf.Lerp(-bounds.height * 0.5f - verticalPadding, bounds.height * 0.5f + verticalPadding, NextFloat());
            float endX = direction > 0 ? right + cloudWidth : left - cloudWidth;
            float speed = Mathf.Lerp(_cloudsSettings.SpeedRange.x, _cloudsSettings.SpeedRange.y, NextFloat())
                * Mathf.Max(8f, bounds.width / 28f)
                * Mathf.Max(0.1f, _cloudSpeedMultiplier);

            var cloudObject = new GameObject("MenuCloud", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rectTransform = cloudObject.GetComponent<RectTransform>();
            rectTransform.SetParent(_cloudsLayer, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(cloudWidth, cloudHeight);
            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.SetAsLastSibling();

            var image = cloudObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.material = ResolveCloudMaterial();
            image.color = new Color(_cloudsSettings.CloudColor.r, _cloudsSettings.CloudColor.g, _cloudsSettings.CloudColor.b, startInView ? _cloudsSettings.CloudAlpha : 0f);

            _clouds.Add(new MenuCloudVisual
            {
                RectTransform = rectTransform,
                Image = image,
                Speed = speed,
                EndX = endX,
                Direction = direction,
                Age = startInView ? Mathf.Max(0.01f, _cloudsSettings.FadeDuration) : 0f
            });

            return true;
        }

        private void ClearClouds()
        {
            for (int i = 0; i < _clouds.Count; i++)
            {
                if (_clouds[i].RectTransform != null)
                    Destroy(_clouds[i].RectTransform.gameObject);
            }

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

        private sealed class MenuCloudVisual
        {
            public RectTransform RectTransform;
            public Image Image;
            public float Speed;
            public float EndX;
            public int Direction;
            public float Age;
        }
    }
}