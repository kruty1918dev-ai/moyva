using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Визначає, як часто volume visual updater застосовує накопичені зміни.
    /// </summary>
    public enum FogVolumeUpdateMode
    {
        /// <summary>
        /// Об'єднує зміни та перебудовує volume не частіше одного разу за кадр.
        /// </summary>
        DebouncePerFrame = 0,

        /// <summary>
        /// Перебудовує volume з інтервалом у секундах.
        /// </summary>
        Interval = 1,

        /// <summary>
        /// Застосовує visual update одразу після зміни fog state.
        /// </summary>
        Immediate = 2,
    }

    /// <summary>
    /// Вказує, до якого dual-grid slot належить конкретний TWC preset.
    /// </summary>
    public enum FogVolumeTilePresetSlot
    {
        /// <summary>
        /// Верхній сегмент об'ємної fog-колонки.
        /// </summary>
        Top = 0,

        /// <summary>
        /// Середній сегмент об'ємної fog-колонки.
        /// </summary>
        Middle = 1,

        /// <summary>
        /// Нижній сегмент об'ємної fog-колонки.
        /// </summary>
        Bottom = 2,
    }

    /// <summary>
    /// Визначає, з якого джерела береться висота для fog volume build.
    /// </summary>
    public enum FogVolumeHeightSource
    {
        /// <summary>
        /// Спершу використовується terrain level map, а за її відсутності — точна height map.
        /// </summary>
        TerrainLevelMapThenHeightMap = 0,

        /// <summary>
        /// Спершу використовується точна height map, а за її відсутності — terrain level map.
        /// </summary>
        HeightMapThenTerrainLevelMap = 1,

        /// <summary>
        /// Уся fog volume геометрія будується на пласкій висоті.
        /// </summary>
        Flat = 2,
    }

    [Serializable]
    /// <summary>
    /// Описує один варіант TWC preset-а для побудови volume fog tile.
    /// Використовується лише visual layer і не впливає на gameplay fog state.
    /// </summary>
    public sealed class FogVolumeTilePresetVariant
    {
        /// <summary>
        /// TWC preset з dual-grid prefabs для цього варіанта.
        /// </summary>
        [Required]
        [AssetsOnly]
        [ValidateInput(nameof(HasDualGridPrefab), "TilePreset має містити dual-grid prefabs.")]
        public TilePreset Preset;

        /// <summary>
        /// Слот колонки, до якого належить цей preset.
        /// </summary>
        public FogVolumeTilePresetSlot Slot = FogVolumeTilePresetSlot.Top;

        /// <summary>
        /// Вага випадкового вибору варіанта серед інших preset-ів того ж стану.
        /// </summary>
        [Range(0f, 1f)]
        public float Weight = 1f;

        /// <summary>
        /// Додаткова локальна висота для цього tile variant.
        /// </summary>
        [MinValue(0f)]
        public float TileHeight;

        /// <summary>
        /// Показує, чи preset призначено взагалі.
        /// </summary>
        public bool IsConfigured => Preset != null;

        /// <summary>
        /// Повертає вагу в діапазоні [0..1].
        /// </summary>
        public float NormalizedWeight => Mathf.Clamp01(Weight);

        private bool HasDualGridPrefab(TilePreset preset)
        {
            return preset == null || FogOfWarSettings.HasUsableDualGridPreset(preset);
        }
    }

    [Serializable]
    /// <summary>
    /// Налаштування одного fog state для TWC volume builder-а.
    /// Дозволяє окремо описати unexplored та explored presentation.
    /// </summary>
    public sealed class FogVolumeStateTileSettings
    {
        /// <summary>
        /// Чи будується цей visual state взагалі.
        /// </summary>
        [HorizontalGroup("State", Width = 80)]
        public bool Enabled = true;

        /// <summary>
        /// Назва runtime build layer-а у TWC configuration clone.
        /// </summary>
        [HorizontalGroup("State")]
        [Required]
        [ValidateInput(nameof(HasLayerName), "Layer name cannot be empty.")]
        public string LayerName = "Fog_State";

        /// <summary>
        /// Набір можливих preset-варіантів для цього fog state.
        /// </summary>
        [TableList(AlwaysExpanded = true, DrawScrollView = false)]
        [ValidateInput(nameof(HasRequiredPreset), "Enabled fog state needs at least one dual-grid TilePreset.")]
        public List<FogVolumeTilePresetVariant> TileVariants = new List<FogVolumeTilePresetVariant>
        {
            new FogVolumeTilePresetVariant()
        };

        /// <summary>
        /// Вертикальний offset build layer-а відносно базової висоти світу.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public float LayerYOffset;

        /// <summary>
        /// Додатковий scale для згенерованих fog prefabs.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public Vector3 ScaleOffset = Vector3.one;

        /// <summary>
        /// Shadow casting mode для build layer-а.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.Off;

        /// <summary>
        /// Unity layer для об'єктів цього fog state.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public LayerMask ObjectLayer = 0;

        /// <summary>
        /// Rendering layer mask для URP/light interaction.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public RenderingLayerMask RenderingLayer = 1;

        /// <summary>
        /// Тип collider-а, який TWC призначить fog tiles цього state.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public Configuration.ColliderType ColliderType = Configuration.ColliderType.none;

        /// <summary>
        /// Висота collider-а для build layer-а.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        [MinValue(0f)]
        public float TileColliderHeight;

        /// <summary>
        /// Додаткова екструзія collider-а.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        [MinValue(0f)]
        public float TileColliderExtrusionHeight;

        /// <summary>
        /// Чи потрібно інвертувати collision walls для TWC collider build.
        /// </summary>
        [FoldoutGroup("Build Layer")]
        public bool InvertCollisionWalls;

        /// <summary>
        /// Нормалізує обов'язкові значення для runtime build path.
        /// Викликається під час валідації settings або перед побудовою runtime config.
        /// </summary>
        /// <param name="fallbackLayerName">Назва layer-а за замовчуванням, якщо користувач не вказав свою.</param>
        public void EnsureDefaults(string fallbackLayerName)
        {
            if (string.IsNullOrWhiteSpace(LayerName))
                LayerName = fallbackLayerName;

            TileVariants ??= new List<FogVolumeTilePresetVariant>();
            if (TileVariants.Count == 0)
                TileVariants.Add(new FogVolumeTilePresetVariant());

            ScaleOffset = new Vector3(
                Mathf.Max(0.001f, ScaleOffset.x),
                Mathf.Max(0.001f, ScaleOffset.y),
                Mathf.Max(0.001f, ScaleOffset.z));
            TileColliderHeight = Mathf.Max(0f, TileColliderHeight);
            TileColliderExtrusionHeight = Mathf.Max(0f, TileColliderExtrusionHeight);
        }

        private bool HasLayerName(string layerName)
            => !Enabled || !string.IsNullOrWhiteSpace(layerName);

        private bool HasRequiredPreset(List<FogVolumeTilePresetVariant> variants)
        {
            if (!Enabled)
                return true;

            if (variants == null)
                return false;

            for (int i = 0; i < variants.Count; i++)
            {
                var variant = variants[i];
                if (variant != null
                    && variant.Preset != null
                    && FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }
    }

    [Serializable]
    /// <summary>
    /// Групує налаштування runtime/preview TWC volume presentation для FogOfWar.
    /// Це лише visual config і не визначає gameplay fog state напряму.
    /// </summary>
    public sealed class FogVolumeTileSettings
    {
        /// <summary>
        /// Режим оновлення volume visual updater-а.
        /// </summary>
        [BoxGroup("Runtime")]
        public FogVolumeUpdateMode UpdateMode = FogVolumeUpdateMode.DebouncePerFrame;

        /// <summary>
        /// Інтервал між rebuild-ами у режимі <see cref="FogVolumeUpdateMode.Interval"/>.
        /// </summary>
        [BoxGroup("Runtime")]
        [ShowIf(nameof(UpdateMode), FogVolumeUpdateMode.Interval)]
        [MinValue(0.02f)]
        public float RebuildIntervalSeconds = 0.1f;

        /// <summary>
        /// Додатковий простір над світом для об'ємного fog volume.
        /// </summary>
        [BoxGroup("Runtime")]
        [MinValue(0f)]
        public float TopClearance = 0.08f;

        /// <summary>
        /// Чи використовувати cell size generated світу як базовий розмір fog tiles.
        /// </summary>
        [BoxGroup("Runtime")]
        public bool UseWorldCellSize = true;

        /// <summary>
        /// Ручний cell size для fog volume, якщо world cell size не використовується.
        /// </summary>
        [BoxGroup("Runtime")]
        [HideIf(nameof(UseWorldCellSize))]
        [MinValue(0.001f)]
        public float CellSizeOverride = 1f;

        /// <summary>
        /// Джерело висоти для побудови fog volume.
        /// </summary>
        [BoxGroup("Generated World Heights")]
        public FogVolumeHeightSource HeightSource = FogVolumeHeightSource.TerrainLevelMapThenHeightMap;

        [BoxGroup("Generated World Heights")]
        [Tooltip("World units per TerrainLevelMap step when the generated world provides integer terrain levels.")]
        [MinValue(0.001f)]
        /// <summary>
        /// Скільки world units відповідає одному рівню в `TerrainLevelMap`.
        /// Використовується, коли generated світ задає висоту дискретними level-ами.
        /// </summary>
        public float TerrainLevelHeightStep = 1f;

        [BoxGroup("Generated World Heights")]
        [Tooltip("Height values closer than this are built on the same TWC layer. Keeps runtime layer count stable for noisy HeightMap data.")]
        [MinValue(0.001f)]
        /// <summary>
        /// Крок квантування height map для об'єднання близьких висот в один runtime layer.
        /// Допомагає не роздувати кількість TWC layer-ів на шумних мапах висот.
        /// </summary>
        public float HeightLayerSnap = 0.01f;

        /// <summary>
        /// Чи слід автоматично очищати editor preview перед runtime стартом.
        /// </summary>
        [BoxGroup("Preview")]
        [Tooltip("Editor preview uses the current scene TWC grid when available. It is cleared automatically at runtime and rebuilt from generated world data.")]
        public bool ClearPreviewOnRuntimeStart = true;

        /// <summary>
        /// Fallback ширина preview grid-а, якщо у сцені немає доступного world source manager-а.
        /// </summary>
        [BoxGroup("Preview")]
        [MinValue(1)]
        public int PreviewFallbackWidth = 16;

        /// <summary>
        /// Fallback висота preview grid-а, якщо у сцені немає доступного world source manager-а.
        /// </summary>
        [BoxGroup("Preview")]
        [MinValue(1)]
        public int PreviewFallbackHeight = 16;

        /// <summary>
        /// Visual settings для повністю unexplored fog state.
        /// </summary>
        [BoxGroup("Unexplored Fog")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeStateTileSettings Unexplored = new FogVolumeStateTileSettings
        {
            LayerName = "Fog_Unexplored",
            LayerYOffset = 0f,
        };

        /// <summary>
        /// Visual settings для explored, але вже не visible fog state.
        /// </summary>
        [BoxGroup("Explored Fog")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeStateTileSettings Explored = new FogVolumeStateTileSettings
        {
            LayerName = "Fog_Explored",
            LayerYOffset = 0.02f,
        };

        /// <summary>
        /// Нормалізує мінімально валідні значення для runtime та preview volume build.
        /// </summary>
        public void EnsureDefaults()
        {
            RebuildIntervalSeconds = Mathf.Max(0.02f, RebuildIntervalSeconds);
            TopClearance = Mathf.Max(0f, TopClearance);
            CellSizeOverride = Mathf.Max(0.001f, CellSizeOverride);
            TerrainLevelHeightStep = Mathf.Max(0.001f, TerrainLevelHeightStep);
            HeightLayerSnap = Mathf.Max(0.001f, HeightLayerSnap);
            PreviewFallbackWidth = Mathf.Max(1, PreviewFallbackWidth);
            PreviewFallbackHeight = Mathf.Max(1, PreviewFallbackHeight);
            Unexplored ??= new FogVolumeStateTileSettings();
            Explored ??= new FogVolumeStateTileSettings();
            Unexplored.EnsureDefaults("Fog_Unexplored");
            Explored.EnsureDefaults("Fog_Explored");
        }
    }

    [CreateAssetMenu(menuName = "Moyva/FogOfWarSettings", fileName = "FogOfWarSettings")]
    /// <summary>
    /// Головний ScriptableObject-конфіг для системи FogOfWar.
    /// Містить gameplay visibility tuning, renderer culling правила та visual settings для TWC volume.
    /// Не повинен містити runtime state; source of truth для стану туману — <see cref="IFogOfWarService"/>.
    /// </summary>
    public class FogOfWarSettings : ScriptableObject
    {
        /// <summary>
        /// Базовий vision range, який можна використовувати як стандартне значення для юнітів.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int DefaultVisionRange = 5;

        /// <summary>
        /// Мінімальна допустима дальність огляду в системі FogOfWar.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int MinVisionRange = 1;

        /// <summary>
        /// Максимальна допустима дальність огляду в системі FogOfWar.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int MaxVisionRange = 12;

        /// <summary>
        /// Висотний крок, з яким LOS-алгоритм рахує перепади рельєфу.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0.01f)]
        public float ElevationStep = 0.15f;

        /// <summary>
        /// Бонус до дальності огляду за кожен крок висоти спостерігача.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int ObserverHeightBonusPerStep = 1;

        /// <summary>
        /// Бонус до огляду при погляді вниз по схилу за кожен крок перепаду.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int DownhillVisionBonusPerStep = 1;

        /// <summary>
        /// Штраф до огляду при погляді вгору за кожен крок перепаду.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int UphillVisionPenaltyPerStep = 1;

        /// <summary>
        /// Верхня межа сумарного бонусу від висоти спостерігача.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxObserverHeightBonus = 4;

        /// <summary>
        /// Верхня межа сумарного бонусу до дальності огляду при спуску вниз по рельєфу.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxDownhillVisionBonus = 2;

        /// <summary>
        /// Верхня межа сумарного штрафу при огляді вгору.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0)]
        public int MaxUphillVisionPenalty = 6;

        /// <summary>
        /// Додатковий bias для розв'язання оклюзії на схилах.
        /// </summary>
        [TitleGroup("Terrain LOS")]
        [MinValue(0f)]
        public float OcclusionSlopeBias = 0.02f;

        /// <summary>
        /// Кількість ray samples на одну клітинку при LOS-розрахунку.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(1, 9)]
        public int TerrainRaySamplesPerTile = 5;

        /// <summary>
        /// Поріг, з якого часткова видимість уже вважається реальною видимістю.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.01f, 1f)]
        public float TerrainVisibilityThreshold = 0.5f;

        /// <summary>
        /// Множник для partial visibility detection у складних LOS-сценаріях.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0f, 1f)]
        public float PartialVisibilityDetectionMultiplier = 1f;

        /// <summary>
        /// Крок променя в клітинках під час terrain LOS raycast.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.25f, 1f)]
        public float TerrainRayStepTiles = 0.5f;

        /// <summary>
        /// Зсув точки спостерігача над поверхнею тайла.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0f)]
        public float ObserverEyeHeightOffset = 0.35f;

        /// <summary>
        /// Зсув точки семплу цілі над поверхнею тайла.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0f)]
        public float TargetSampleHeightOffset = 0.1f;

        /// <summary>
        /// Співвідношення дистанції для дальніх семплів у terrain LOS.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [Range(0.1f, 1f)]
        public float TerrainFarSampleDistanceRatio = 0.65f;

        /// <summary>
        /// Місткість кешу visibility calculations для terrain LOS.
        /// </summary>
        [TitleGroup("Terrain LOS/Raycast")]
        [MinValue(0)]
        public int TerrainVisibilityCacheCapacity = 24576;

        /// <summary>
        /// Чи дозволений окремий edge-based LOS logic для terrain transitions.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        public bool EnableTerrainEdgeLineOfSight = true;

        /// <summary>
        /// Мінімальний перепад висоти, який уже вважається edge/cliff для LOS logic.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0.001f)]
        public float TerrainEdgeHeightThreshold = 0.12f;

        /// <summary>
        /// Скільки клітин edge-peek логіка може “зазирнути” за край.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgePeekDistanceTiles = 1;

        /// <summary>
        /// Розмір blind zone за edge/cliff переходом у клітинках.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgeBlindZoneTiles = 2;

        /// <summary>
        /// Масштаб distance-based blind zone для terrain edges.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0f)]
        public float TerrainEdgeBlindZoneDistanceScale = 0.35f;

        /// <summary>
        /// Максимальна довжина blind zone для terrain edges.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [MinValue(0)]
        public int TerrainEdgeMaxBlindZoneTiles = 4;

        /// <summary>
        /// Сила часткового peek-ефекту при погляді вгору через край.
        /// </summary>
        [TitleGroup("Terrain LOS/Edges")]
        [Range(0f, 1f)]
        public float TerrainEdgeUphillPeekStrength = 0.65f;

        /// <summary>
        /// Конфігурація TWC volume presentation для fog.
        /// </summary>
        [TitleGroup("TWC Volume")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeTileSettings Volume = new FogVolumeTileSettings();

        /// <summary>
        /// Чи дозволено приховувати world renderer-и під повністю unexplored fog.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Disables world renderers that are fully covered by unexplored fog.")]
        public bool EnableRendererCulling = true;

        /// <summary>
        /// Чи потрібен майже непрозорий unexplored fog, щоб renderer culling увімкнувся.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("If enabled, renderer culling works only when UnexploredAlpha is close to opaque (>= 0.99).")]
        public bool RequireOpaqueUnexploredForCulling = true;

        /// <summary>
        /// Layer mask renderer-ів, які може ховати fog culling service.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Layers affected by fog renderer culling.")]
        public LayerMask RendererCullingLayerMask = ~0;

        /// <summary>
        /// Максимальна кількість renderer-ів, які culling service перевіряє за кадр.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Maximum tracked renderers evaluated per frame. Lower values spread work across more frames.")]
        [MinValue(1)]
        public int RendererCullingMaxRenderersPerFrame = 384;

        /// <summary>
        /// Як часто culling service шукає нові renderer-и у світі.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("How often the culling service searches for newly spawned world renderers.")]
        [MinValue(0.05f)]
        public float RendererCullingDiscoveryInterval = 0.75f;

        /// <summary>
        /// Додатковий padding bounds-ів у клітинках для безпечного culling-а на краях.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Small bounds padding in map cells to avoid edge flicker when sprites move between cells.")]
        [MinValue(0f)]
        public float RendererCullingBoundsPaddingCells = 0f;

        /// <summary>
        /// Alpha для повністю unexplored fog state.
        /// Залишається корисною для legacy notes і safety checks у culling path.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Alpha for fully unexplored fog; still used by renderer culling safety checks.")]
        [Range(0f, 1f)]
        public float UnexploredAlpha = 1f;

        /// <summary>
        /// Alpha для explored fog state.
        /// Зберігається для migration notes та tuning explored presentation.
        /// </summary>
        [TitleGroup("Renderer Culling")]
        [Tooltip("Alpha for explored fog; kept for preset migration and tuning notes.")]
        [Range(0f, 1f)]
        public float ExploredAlpha = 0.5f;

        /// <summary>
        /// Чи слід FogOfWarService відкривати fallback стартову область, якщо bootstrap reveal не надійшов.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("If no bootstrap startup reveal arrives for a fresh non-load world, FogOfWarService opens a random visible start area so the map never starts fully black.")]
        public bool EnableStartupFallbackReveal = true;

        /// <summary>
        /// Радіус fallback стартового reveal, який service застосує у крайньому випадку.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Radius used by FogOfWarService when bootstrap did not provide a startup reveal.")]
        [MinValue(1)]
        public int StartupFallbackRevealRadius = 15;

        /// <summary>
        /// Мінімальний відступ fallback стартової точки від краю карти.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Minimum margin from map edge for the fallback random start point.")]
        [MinValue(0)]
        public int StartupFallbackMinMarginFromBorder = 5;

        /// <summary>
        /// Додатковий відносний margin для випадкової fallback стартової точки.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Additional map-relative margin for the fallback random start point.")]
        [Range(0f, 0.45f)]
        public float StartupFallbackRelativeMarginFactor = 0.1667f;

        /// <summary>
        /// Форма fallback стартового reveal.
        /// </summary>
        [TitleGroup("Startup Reveal")]
        [Tooltip("Shape used by FogOfWarService fallback startup reveal.")]
        public FogRevealShape StartupFallbackRevealShape = FogRevealShape.PixelCircle;

        /// <summary>
        /// Legacy tint для повністю unexplored стану в старому shader overlay path.
        /// Залишене для migration/debug і не є source of truth для нового volume fog.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Color UnexploredColor = new Color(0f, 0f, 0f, 1f);

        /// <summary>
        /// Legacy tint для explored стану в старому shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Color ExploredColor = new Color(0f, 0f, 0f, 0.5f);

        /// <summary>
        /// Legacy sprite тайла для 2D shader/quad fog presentation.
        /// Новий runtime volume path більше не читає це поле напряму.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Sprite FogTileSprite;

        /// <summary>
        /// Legacy pixel size fog tile sprite-а для shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogTileSpritePixelSize = new Vector2Int(16, 16);

        /// <summary>
        /// Legacy розмір одного fog tile в координатах клітинок для shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2 FogTileSizeInCells = Vector2.one;

        /// <summary>
        /// Legacy overlap у пікселях між сусідніми fog tile-ами, щоб зменшити seams у shader path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogTileSeamOverlapPixels = 1f;

        /// <summary>
        /// Legacy edge padding у пікселях для fog overlay texture.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogMapEdgePaddingPixels = 2f;

        /// <summary>
        /// Legacy overhang fog overlay за межі карти в клітинках.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogMapEdgeOverhangCells = 0.5f;

        /// <summary>
        /// Legacy tiling множник для fog tile texture в shader path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogTileTiling = 1f;

        /// <summary>
        /// Legacy набір icon sprite-ів для старого shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Sprite[] FogIconSprites;

        /// <summary>
        /// Legacy pixel size icon sprite-ів у shader overlay path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogIconSpritePixelSize = new Vector2Int(16, 16);

        /// <summary>
        /// Legacy розмір сітки іконок для shader overlay presentation.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public Vector2Int FogIconGridSize = new Vector2Int(10, 10);

        /// <summary>
        /// Legacy scale іконок поверх fog overlay.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float FogIconScale = 0.5f;

        /// <summary>
        /// Legacy прапорець центрування іконки всередині клітинки shader overlay.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool CenterIcon = true;

        /// <summary>
        /// Legacy режим, у якому visible клітинки робляться повністю прозорими у shader path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool FullyTransparentWhenVisible = true;

        /// <summary>
        /// Legacy top clearance для старого 3D/shader fog presentation path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float Fog3DTopClearance = 0.08f;

        /// <summary>
        /// Legacy перемикач shader-based fog culling.
        /// Збережений для сумісності старих пресетів і migration notes.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public bool EnableShaderFogCulling = false;

        /// <summary>
        /// Legacy поріг для shader fog culling path.
        /// </summary>
        [FoldoutGroup("Legacy Shader Overlay")]
        [HideInInspector]
        public float ShaderFogCullThreshold = 0.01f;

        /// <summary>
        /// Перевіряє, чи TWC preset містить хоча б один usable dual-grid prefab для volume fog build.
        /// </summary>
        /// <param name="preset">Preset, який перевіряється.</param>
        /// <returns><see langword="true"/>, якщо preset придатний для dual-grid fog volume.</returns>
        public static bool HasUsableDualGridPreset(TilePreset preset)
        {
            return preset != null
                && (preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null);
        }

        /// <summary>
        /// Нормалізує всі поля settings після зміни в inspector.
        /// Не змінює behavior поза корекцією некоректних значень.
        /// </summary>
        private void OnValidate()
        {
            DefaultVisionRange = Mathf.Max(1, DefaultVisionRange);
            MinVisionRange = Mathf.Max(1, MinVisionRange);
            MaxVisionRange = Mathf.Max(MinVisionRange, MaxVisionRange);
            DefaultVisionRange = Mathf.Clamp(DefaultVisionRange, MinVisionRange, MaxVisionRange);
            ElevationStep = Mathf.Max(0.01f, ElevationStep);
            ObserverHeightBonusPerStep = Mathf.Max(0, ObserverHeightBonusPerStep);
            DownhillVisionBonusPerStep = Mathf.Max(0, DownhillVisionBonusPerStep);
            UphillVisionPenaltyPerStep = Mathf.Max(0, UphillVisionPenaltyPerStep);
            MaxObserverHeightBonus = Mathf.Max(0, MaxObserverHeightBonus);
            MaxDownhillVisionBonus = Mathf.Max(0, MaxDownhillVisionBonus);
            MaxUphillVisionPenalty = Mathf.Max(0, MaxUphillVisionPenalty);
            OcclusionSlopeBias = Mathf.Max(0f, OcclusionSlopeBias);
            TerrainRaySamplesPerTile = Mathf.Clamp(TerrainRaySamplesPerTile, 1, 9);
            TerrainVisibilityThreshold = Mathf.Clamp(TerrainVisibilityThreshold, 0.01f, 1f);
            PartialVisibilityDetectionMultiplier = Mathf.Clamp01(PartialVisibilityDetectionMultiplier);
            TerrainRayStepTiles = Mathf.Clamp(TerrainRayStepTiles, 0.25f, 1f);
            ObserverEyeHeightOffset = Mathf.Max(0f, ObserverEyeHeightOffset);
            TargetSampleHeightOffset = Mathf.Max(0f, TargetSampleHeightOffset);
            TerrainFarSampleDistanceRatio = Mathf.Clamp(TerrainFarSampleDistanceRatio, 0.1f, 1f);
            TerrainVisibilityCacheCapacity = Mathf.Max(0, TerrainVisibilityCacheCapacity);
            TerrainEdgeHeightThreshold = Mathf.Max(0.001f, TerrainEdgeHeightThreshold);
            TerrainEdgePeekDistanceTiles = Mathf.Max(0, TerrainEdgePeekDistanceTiles);
            TerrainEdgeBlindZoneTiles = Mathf.Max(0, TerrainEdgeBlindZoneTiles);
            TerrainEdgeBlindZoneDistanceScale = Mathf.Max(0f, TerrainEdgeBlindZoneDistanceScale);
            TerrainEdgeMaxBlindZoneTiles = Mathf.Max(TerrainEdgeBlindZoneTiles, TerrainEdgeMaxBlindZoneTiles);
            TerrainEdgeUphillPeekStrength = Mathf.Clamp01(TerrainEdgeUphillPeekStrength);
            Volume ??= new FogVolumeTileSettings();
            Volume.EnsureDefaults();
            FogTileSpritePixelSize = ClampSpritePixelSize(FogTileSpritePixelSize);
            FogTileSizeInCells = ClampTileSizeInCells(FogTileSizeInCells);
            FogTileSeamOverlapPixels = Mathf.Max(0f, FogTileSeamOverlapPixels);
            FogMapEdgePaddingPixels = Mathf.Max(0f, FogMapEdgePaddingPixels);
            FogMapEdgeOverhangCells = Mathf.Max(0f, FogMapEdgeOverhangCells);
            FogTileTiling = Mathf.Max(1f, FogTileTiling);
            FogIconSpritePixelSize = ClampSpritePixelSize(FogIconSpritePixelSize);
            FogIconScale = Mathf.Max(0.1f, FogIconScale);
            Fog3DTopClearance = Mathf.Max(0f, Fog3DTopClearance);
            StartupFallbackRevealRadius = Mathf.Max(1, StartupFallbackRevealRadius);
            StartupFallbackMinMarginFromBorder = Mathf.Max(0, StartupFallbackMinMarginFromBorder);
            StartupFallbackRelativeMarginFactor = Mathf.Clamp(StartupFallbackRelativeMarginFactor, 0f, 0.45f);
            RendererCullingMaxRenderersPerFrame = Mathf.Max(1, RendererCullingMaxRenderersPerFrame);
            RendererCullingDiscoveryInterval = Mathf.Max(0.05f, RendererCullingDiscoveryInterval);
            RendererCullingBoundsPaddingCells = Mathf.Max(0f, RendererCullingBoundsPaddingCells);
            UnexploredAlpha = Mathf.Clamp01(UnexploredAlpha);
            ExploredAlpha = Mathf.Clamp01(ExploredAlpha);
            ShaderFogCullThreshold = Mathf.Clamp(ShaderFogCullThreshold, 0f, 0.25f);
        }

        private static Vector2Int ClampSpritePixelSize(Vector2Int size)
        {
            return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }

        private static Vector2 ClampTileSizeInCells(Vector2 size)
        {
            return new Vector2(Mathf.Max(0.001f, size.x), Mathf.Max(0.001f, size.y));
        }
    }
}
