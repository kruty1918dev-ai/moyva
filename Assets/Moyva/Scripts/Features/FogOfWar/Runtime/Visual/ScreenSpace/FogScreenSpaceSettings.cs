using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public enum FogCurtainDebugVisualMode
    {
        Off = 0,
        BoundaryDirection = 1,
        HeightBands = 2,
        SegmentParity = 3,
        HeightRelation = 4
    }

    public enum FogScreenShaderDebugMode
    {
        Off = 0,
        FogState = 1,
        GridCoordinates = 2,
        SceneDepth = 3,
        WorldGridPosition = 4,
        FogSurfaceDepth = 5,
        RawScreenState = 6,
        ClosedScreenState = 7,
        ScreenBoundary = 8,
        VirtualDepth = 9
    }

    /// <summary>
    /// Налаштування screen-space FogOfWar і world-space boundary curtain.
    /// Gameplay visibility ці параметри не змінюють.
    /// </summary>
    [Serializable]
    public sealed class FogScreenSpaceSettings
    {
        private const int CurrentCornerJoinRevision = 613;

        [SerializeField]
        [HideInInspector]
        private int _cornerJoinRevision;

        [BoxGroup("General")]
        public bool Enabled = true;

        [BoxGroup("General")]
        [Tooltip(
            "Інвертує координату Y fog texture. " +
            "Увімкни лише якщо reveal віддзеркалений по осі Z.")]
        public bool FlipTextureY;

        [BoxGroup("General")]
        [Range(-1f, 3f)]
        [Tooltip(
            "Y-offset базової world-площини для transparent shaders, " +
            "які не записують scene depth. Для поточного TerrainMesh/Water " +
            "правильне значення близьке до 0.50–0.53. Нуль зміщує fog " +
            "майже на половину клітинки при перспективній камері.")]
        public float TransparentFallbackPlaneOffsetY = 0.50f;

        [BoxGroup("Unexplored")]
        public Color UnexploredColor =
            new Color(
                0.09f,
                0.14f,
                0.17f,
                1f);

        [BoxGroup("Unexplored")]
        [Range(0f, 1f)]
        public float UnexploredOpacity = 1f;

        [BoxGroup("Unexplored")]
        [Range(0f, 1f)]
        public float UnexploredSaturation = 0.08f;

        [BoxGroup("Explored")]
        public Color ExploredColor =
            new Color(
                0.21f,
                0.27f,
                0.30f,
                1f);

        [BoxGroup("Explored")]
        [Range(0f, 1f)]
        public float ExploredOpacity = 0.42f;

        [BoxGroup("Explored")]
        [Range(0f, 1f)]
        public float ExploredSaturation = 0.45f;

        [BoxGroup("Edges")]
        [Range(0f, 1f)]
        [Tooltip(
            "М'якість верхньої fullscreen-межі " +
            "у частках клітинки.")]
        public float EdgeSoftness = 0.12f;

        [BoxGroup("Edges")]
        [Range(0f, 0.5f)]
        public float EdgeNoiseStrength;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Tooltip(
            "Темний кант на стороні дискретної Unexplored клітинки. " +
            "Він обчислюється з world-grid меж, лише на реальній поверхні, " +
            "без screen-neighbour sampling та fallback-площини.")]
        public bool DepthAwareEdgeEnabled = true;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Tooltip(
            "Тимчасово вмикає старий геометричний curtain для порівняння. " +
            "У нормальному режимі має бути вимкнено.")]
        public bool UseLegacyWorldCurtain = false;

        [BoxGroup("Depth-Aware Screen Edge")]
        public Color DepthAwareEdgeColor =
            new Color(
                0.018f,
                0.055f,
                0.075f,
                1f);

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(0f, 1f)]
        public float DepthAwareEdgeOpacity = 0.82f;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(0.1f, 5f)]
        [Tooltip(
            "Legacy-параметр. Inner Fog Bevel не використовує " +
            "world-space екструзію або напрямок Vector3.down.")]
        public float DepthAwareEdgeWorldDepth = 1.5f;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(1f, 16f)]
        [Tooltip(
            "Legacy-параметр, збережений для сумісності serialized asset. " +
            "Нова фаска використовує лише Max Pixels як ширину.")]
        public float DepthAwareEdgeMinPixels = 5f;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(2f, 10f)]
        [Tooltip(
            "Ширина surface-locked канту в screen pixels. " +
            "Логічна межа при цьому залишається у world-grid.")]
        public float DepthAwareEdgeMaxPixels = 6f;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(4, 12)]
        [Tooltip(
            "Legacy-параметр. SurfaceLockedGridEdge не виконує " +
            "радіальні screen-space вибірки.")]
        public int DepthAwareEdgeSamples = 8;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(0f, 1f)]
        [Tooltip(
            "Legacy-параметр. Нова фаска не виконує camera-depth " +
            "оклюзію, оскільки ніколи не виходить на Visible side.")]
        public float DepthAwareEdgeOcclusionBias = 0.08f;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(0.25f, 4f)]
        [Tooltip(
            "Форма згасання фаски вглиб туману. Більше значення " +
            "робить темну частину вужчою.")]
        public float DepthAwareEdgeGradientPower = 1.5f;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(0, 3)]
        [Tooltip(
            "Використовується лише для debug ClosedScreenState. " +
            "Фінальна форма fog береться з RawScreenState.")]
        public int DepthAwareStateCloseRadiusPixels = 0;

        [BoxGroup("Depth-Aware Screen Edge")]
        [Range(0f, 4f)]
        [Tooltip(
            "М'якість внутрішньої фаски. Не впливає на логічну " +
            "форму основної fog mask.")]
        public float DepthAwareBoundarySoftnessPixels = 0.65f;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Створює реальну вертикальну world-space стінку " +
            "по межі Unexplored.")]
        public bool CurtainEnabled = true;

        [BoxGroup("World Curtain")]
        [MinValue(0.1f)]
        [Tooltip(
            "Фіксована вертикальна довжина кожного curtain-сегмента. " +
            "Якщо верх fog піднімається на високий тайл, низ піднімається " +
            "разом із ним, тому товщина всюди залишається однаковою.")]
        public float CurtainWorldDepth = 1.5f;

        [BoxGroup("World Curtain")]
        [MinValue(0f)]
        [Tooltip(
            "Наскільки нижче нижньої terrain-поверхні " +
            "має доходити стінка.")]
        public float CurtainBottomPadding = 0f;

        [BoxGroup("World Curtain")]
        [Range(-0.25f, 0.5f)]
        [Tooltip(
            "Невелике вертикальне зміщення верхнього краю. " +
            "Використовуй лише для усунення z-fighting.")]
        public float CurtainTopOffset = 0.02f;

        [BoxGroup("World Curtain")]
        [Range(0f, 0.05f)]
        [Tooltip(
            "Дуже малий зсув стінки у бік Unexplored. " +
            "Не дозволяє їй заходити на відкриту поверхню.")]
        public float CurtainHiddenSideOffsetCells = 0.003f;

        [BoxGroup("World Curtain")]
        [Range(0f, 0.05f)]
        [Tooltip(
            "Невелике перекриття сусідніх вертикальних curtain edges. " +
            "Закриває мікрощілини у grid corners без підняття одного " +
            "кінця сегмента на висоту сусіднього terrain level.")]
        public float CurtainEdgeOverlapCells = 0.012f;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Верх кожного boundary edge береться з найвищої " +
            "із двох сусідніх terrain-поверхонь. Прибирає стінки, " +
            "які прорізають високий тайл.")]
        public bool CurtainUseHighestAdjacentSurface = true;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Legacy-параметр. У поточному height-aware режимі верх curtain " +
            "завжди піднімається до найвищої з двох сусідніх поверхонь. " +
            "Поле збережене лише для сумісності зі старими asset-файлами.")]
        public bool CurtainAnchorToRevealedSurface = false;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Виконує окремий downward raycast для кожного boundary edge. " +
            "Верх стінки співпадає з реальною поверхнею меша, а не лише " +
            "з logical height + глобальним median offset.")]
        public bool CurtainPerEdgeSurfaceProbe = true;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Legacy-параметр для сумісності зі старими asset-файлами. " +
            "Поточний runtime завжди використовує однакову локальну " +
            "Curtain World Depth для кожного сегмента.")]
        public bool CurtainUseSharedBottomPlane = false;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Малює обидві сторони world curtain. Для перспективної камери " +
            "це необхідно: інакше ближня або дальня половина 3D-краю " +
            "зникає через back-face culling.")]
        public bool CurtainDoubleSided = true;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Примусово використовує Cull Off у normal mode навіть для " +
            "старого serialized asset, де Curtain Double Sided ще false.")]
        public bool CurtainForceDoubleSidedInNormalMode = true;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Legacy-параметр. Height-aware curtain тепер завжди малюється " +
            "поверх world-геометрії. Перекриття високих тайлів усувається " +
            "не depth-discard, а підняттям верхньої межі fog до їхньої " +
            "реальної поверхні.")]
        public bool CurtainUseOpaqueDepthOcclusion = false;

        [BoxGroup("World Curtain")]
        [Range(0f, 0.25f)]
        [Tooltip(
            "Допуск depth-порівняння у camera-space world units. " +
            "Збільшити, якщо край мерехтить на стику з тайлом. " +
            "Зменшити, якщо curtain трохи заходить поверх близької геометрії.")]
        public float CurtainOpaqueDepthBias = 0.035f;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Автоматично вимірює різницю між logical height map " +
            "і реальною поверхнею TerrainMesh через downward raycasts.")]
        public bool CurtainAutoCalibrateSurfaceOffset = true;

        [BoxGroup("World Curtain")]
        [Range(-2f, 2f)]
        [Tooltip(
            "Fallback або ручний Y-offset поверхні. " +
            "За поточними логами Moyva реальне значення близьке до 0.51–0.53.")]
        public float CurtainSurfaceOffsetY = 0.53f;

        [BoxGroup("World Curtain")]
        [Range(4, 64)]
        public int CurtainSurfaceCalibrationSamples = 24;

        [BoxGroup("World Curtain")]
        [MinValue(0.5f)]
        public float CurtainSurfaceProbeHeight = 8f;

        [BoxGroup("World Curtain")]
        [MinValue(1f)]
        public float CurtainSurfaceProbeDistance = 24f;

        [BoxGroup("World Curtain")]
        [Range(0.1f, 3f)]
        [Tooltip(
            "Відкидає raycast hits, які надто далеко від logical height. " +
            "Це не дозволяє будівлям і юнітам псувати калібрування.")]
        public float CurtainSurfaceCalibrationMaxOffset = 1.25f;

        [BoxGroup("World Curtain")]
        public LayerMask CurtainSurfaceProbeMask = -1;

        [BoxGroup("World Curtain")]
        [Tooltip(
            "Будує вузьку горизонтальну верхню кромку у бік Unexplored. " +
            "Вона створює відчуття товщини fog-volume. Після розділення " +
            "committed/preview mask цей lip більше не утворює внутрішні кільця.")]
        public bool CurtainTopCapEnabled = true;

        [BoxGroup("World Curtain")]
        [Range(0.02f, 0.4f)]
        public float CurtainTopCapWidthCells = 0.14f;

        [BoxGroup("World Curtain")]
        [Range(0f, 0.1f)]
        public float CurtainTopCapCornerOverlapCells = 0.02f;

        [BoxGroup("World Curtain")]
        [Range(0f, 0.05f)]
        public float CurtainTopCapLift = 0.006f;

        [BoxGroup("World Curtain")]
        [Range(0f, 0.95f)]
        [Tooltip(
            "Наскільки темнішим є низ вертикальної стінки. Верхній край " +
            "збігається з Unexplored Color, тому біля води не виникає " +
            "чорного шва, схожого на пробивання water shader.")]
        public float CurtainTopDarkness = 0.30f;

        [BoxGroup("World Curtain")]
        [Range(0.01f, 0.35f)]
        [Tooltip(
            "Частка висоти, яку займає найтемніший верхній кант.")]
        public float CurtainTopBandFraction = 0.05f;

        [BoxGroup("World Curtain")]
        [Range(0.25f, 4f)]
        [Tooltip(
            "Форма вертикального градієнта. " +
            "Більше значення довше тримає нижній колір.")]
        public float CurtainGradientPower = 0.8f;

        [BoxGroup("Animation")]
        [MinValue(0f)]
        public float RevealDuration = 0.25f;

        [BoxGroup("Animation")]
        [MinValue(0f)]
        public float HideDuration = 0.45f;

        [BoxGroup("Debug")]
        public bool ApplyInSceneView = true;

        [BoxGroup("Diagnostics")]
        [Tooltip(
            "Пише summary та boundary-сегменти у Console " +
            "з префіксом [MOYVA_FOG_CURTAIN_DIAG].")]
        public bool LogCurtainDiagnostics = true;

        [BoxGroup("Diagnostics")]
        [Tooltip(
            "Пише camera/world-to-grid probes у Console " +
            "з префіксом [MOYVA_FOG_SHADER_DIAG].")]
        public bool LogShaderDiagnostics = true;

        [BoxGroup("Diagnostics")]
        [MinValue(0.1f)]
        public float DiagnosticLogIntervalSeconds = 1.5f;

        [BoxGroup("Diagnostics")]
        [Range(1, 64)]
        public int DiagnosticMaxSegmentLogs = 24;

        [BoxGroup("Diagnostics")]
        [Range(1, 30)]
        [Tooltip(
            "Радіус навколо центра відкритої області, " +
            "з якого boundary-сегменти потрапляють у детальний лог.")]
        public int DiagnosticCenterRadiusCells = 12;

        [BoxGroup("Diagnostics")]
        [Range(3, 20)]
        [Tooltip(
            "Радіус ASCII-знімка fog mask. " +
            "V=Visible, e=Explored, B=boundary, #=Unexplored.")]
        public int DiagnosticMaskRadiusCells = 10;

        [BoxGroup("Diagnostics")]
        [Range(1, 16)]
        public int DiagnosticMaxComponentLogs = 8;

        [BoxGroup("Diagnostics")]
        [Tooltip(
            "BoundaryDirection: Left=red, Right=green, " +
            "Down=blue, Up=yellow.")]
        public FogCurtainDebugVisualMode CurtainDebugVisualMode =
            FogCurtainDebugVisualMode.Off;

        [BoxGroup("Diagnostics")]
        public FogScreenShaderDebugMode ShaderDebugMode =
            FogScreenShaderDebugMode.Off;

        [BoxGroup("Diagnostics")]
        [Range(0.5f, 6f)]
        public float ShaderDebugGridLineWidthPixels = 1.5f;

        [BoxGroup("Diagnostics")]
        public LayerMask ShaderDiagnosticRaycastMask = -1;

        [BoxGroup("Diagnostics")]
        [MinValue(1f)]
        public float ShaderDiagnosticRaycastDistance = 1000f;

        public void EnsureDefaults()
        {
            /*
             * Pass 6.13 intentionally restores the exact Pass 6.11
             * presentation values. Only corner joining is new.
             */
            if (_cornerJoinRevision < CurrentCornerJoinRevision)
            {
                DepthAwareEdgeOpacity = 0.82f;
                DepthAwareEdgeMaxPixels = 6f;
                DepthAwareEdgeGradientPower = 1.5f;
                DepthAwareBoundarySoftnessPixels = 0.65f;
                DepthAwareStateCloseRadiusPixels = 0;

                _cornerJoinRevision =
                    CurrentCornerJoinRevision;
            }

            DepthAwareEdgeOpacity =
                Mathf.Clamp(
                    DepthAwareEdgeOpacity,
                    0f,
                    0.88f);

            DepthAwareEdgeWorldDepth =
                Mathf.Clamp(
                    DepthAwareEdgeWorldDepth,
                    0.1f,
                    5f);

            DepthAwareEdgeMinPixels =
                Mathf.Clamp(
                    DepthAwareEdgeMinPixels,
                    2f,
                    32f);

            DepthAwareEdgeMaxPixels =
                Mathf.Clamp(
                    DepthAwareEdgeMaxPixels,
                    2f,
                    10f);

            DepthAwareEdgeSamples =
                Mathf.Clamp(
                    DepthAwareEdgeSamples,
                    4,
                    12);

            DepthAwareEdgeOcclusionBias =
                Mathf.Clamp01(
                    DepthAwareEdgeOcclusionBias);

            DepthAwareEdgeGradientPower =
                Mathf.Clamp(
                    DepthAwareEdgeGradientPower,
                    0.25f,
                    4f);

            /*
             * Morphology is debug-only in Inner Fog Bevel mode.
             */
            DepthAwareStateCloseRadiusPixels =
                0;

            DepthAwareBoundarySoftnessPixels =
                Mathf.Clamp(
                    DepthAwareBoundarySoftnessPixels,
                    0f,
                    4f);

            TransparentFallbackPlaneOffsetY =
                Mathf.Clamp(
                    TransparentFallbackPlaneOffsetY,
                    -1f,
                    3f);

            UnexploredOpacity =
                Mathf.Clamp01(
                    UnexploredOpacity);

            ExploredOpacity =
                Mathf.Clamp01(
                    ExploredOpacity);

            UnexploredSaturation =
                Mathf.Clamp01(
                    UnexploredSaturation);

            ExploredSaturation =
                Mathf.Clamp01(
                    ExploredSaturation);

            EdgeSoftness =
                Mathf.Clamp01(
                    EdgeSoftness);

            EdgeNoiseStrength =
                Mathf.Clamp(
                    EdgeNoiseStrength,
                    0f,
                    0.5f);

            CurtainWorldDepth =
                Mathf.Max(
                    0.1f,
                    CurtainWorldDepth);

            CurtainBottomPadding =
                Mathf.Max(
                    0f,
                    CurtainBottomPadding);

            CurtainTopOffset =
                Mathf.Clamp(
                    CurtainTopOffset,
                    -0.25f,
                    0.5f);

            CurtainHiddenSideOffsetCells =
                Mathf.Clamp(
                    CurtainHiddenSideOffsetCells,
                    0f,
                    0.05f);

            CurtainEdgeOverlapCells =
                Mathf.Clamp(
                    CurtainEdgeOverlapCells,
                    0f,
                    0.05f);

            CurtainOpaqueDepthBias =
                Mathf.Clamp(
                    CurtainOpaqueDepthBias,
                    0f,
                    0.25f);

            CurtainSurfaceOffsetY =
                Mathf.Clamp(
                    CurtainSurfaceOffsetY,
                    -2f,
                    2f);

            CurtainSurfaceCalibrationSamples =
                Mathf.Clamp(
                    CurtainSurfaceCalibrationSamples,
                    4,
                    64);

            CurtainSurfaceProbeHeight =
                Mathf.Max(
                    0.5f,
                    CurtainSurfaceProbeHeight);

            CurtainSurfaceProbeDistance =
                Mathf.Max(
                    1f,
                    CurtainSurfaceProbeDistance);

            CurtainSurfaceCalibrationMaxOffset =
                Mathf.Clamp(
                    CurtainSurfaceCalibrationMaxOffset,
                    0.1f,
                    3f);

            CurtainTopCapWidthCells =
                Mathf.Clamp(
                    CurtainTopCapWidthCells,
                    0.02f,
                    0.4f);

            CurtainTopCapCornerOverlapCells =
                Mathf.Clamp(
                    CurtainTopCapCornerOverlapCells,
                    0f,
                    0.1f);

            CurtainTopCapLift =
                Mathf.Clamp(
                    CurtainTopCapLift,
                    0f,
                    0.05f);

            CurtainTopDarkness =
                Mathf.Clamp(
                    CurtainTopDarkness,
                    0f,
                    0.95f);

            CurtainTopBandFraction =
                Mathf.Clamp(
                    CurtainTopBandFraction,
                    0.01f,
                    0.35f);

            CurtainGradientPower =
                Mathf.Clamp(
                    CurtainGradientPower,
                    0.25f,
                    4f);

            RevealDuration =
                Mathf.Max(
                    0f,
                    RevealDuration);

            HideDuration =
                Mathf.Max(
                    0f,
                    HideDuration);

            DiagnosticLogIntervalSeconds =
                Mathf.Max(
                    0.1f,
                    DiagnosticLogIntervalSeconds);

            DiagnosticMaxSegmentLogs =
                Mathf.Clamp(
                    DiagnosticMaxSegmentLogs,
                    1,
                    64);

            DiagnosticCenterRadiusCells =
                Mathf.Clamp(
                    DiagnosticCenterRadiusCells,
                    1,
                    30);

            DiagnosticMaskRadiusCells =
                Mathf.Clamp(
                    DiagnosticMaskRadiusCells,
                    3,
                    20);

            DiagnosticMaxComponentLogs =
                Mathf.Clamp(
                    DiagnosticMaxComponentLogs,
                    1,
                    16);

            ShaderDebugGridLineWidthPixels =
                Mathf.Clamp(
                    ShaderDebugGridLineWidthPixels,
                    0.5f,
                    6f);

            ShaderDiagnosticRaycastDistance =
                Mathf.Max(
                    1f,
                    ShaderDiagnosticRaycastDistance);
        }
    }
}
