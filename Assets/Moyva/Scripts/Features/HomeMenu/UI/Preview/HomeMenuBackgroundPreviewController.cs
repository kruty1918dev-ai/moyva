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

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Окремий menu-only компонент для фону головного меню.
    /// Генерує випадковий світ із GraphAsset і показує його як menu-only фон
    /// без впливу на gameplay системи.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Moyva/Home Menu/Background Preview")]
    public sealed partial class HomeMenuBackgroundPreviewController : MonoBehaviour
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

    }
}
