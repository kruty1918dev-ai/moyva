using UnityEngine;
using UnityEngine.Serialization;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>CameraControlProfile — struct: камери керування профілю.</summary>
    [System.Serializable]
    public struct CameraControlProfile
    {
        private const float DefaultRotationSpeed = 90f;

        [Min(0.01f)] public float moveSpeed;
        [Tooltip("Camera speed multiplier while the sprint binding is held.")]
        [Min(0.01f)] public float sprintMultiplier;
        [Min(0.01f)] public float smoothTime;
        [Min(0.01f)] public float zoomSpeed;
        [Min(0.01f)] public float rotationSpeed;
        [Min(0.01f)] public float rotationAcceleration;
        [Min(0.01f)] public float rotationDeceleration;
        [Min(0.001f)] public float pointerOrbitSensitivity;
        [Min(0.01f)] public float closeZoomRotationMultiplier;
        [Min(0.01f)] public float farZoomRotationMultiplier;
        [Min(0.01f)] public float closeZoomMoveMultiplier;
        [Min(0.01f)] public float farZoomMoveMultiplier;
        [Min(0.1f)] public float minZoom;
        [Min(0.2f)] public float maxZoom;

        /// <summary>Чи негайного дотику жестів — useImmediateTouchGestures.</summary>
        [Min(0.01f)] public float touchMoveSpeed;
        [Min(0.01f)] public float touchPinchZoomSensitivity;
        [Min(0f)] public float touchDragDeadZonePixels;
        [Min(0f)] public float touchPinchDeadZonePixels;
        [Min(1f)] public float maxTouchDeltaPixels;
        [Tooltip("Accumulated pinch distance (px) after which twist is suppressed for this gesture.")]
        [Min(0.5f)] public float touchPinchDominancePixels;
        [Tooltip("Accumulated twist (degrees) after which pinch is suppressed for this gesture.")]
        [Min(0.5f)] public float touchTwistDominanceDegrees;
        [Tooltip("Frames of one-finger pan ignored after a multi-touch gesture ends.")]
        [Range(0, 8)] public int touchSettleFrames;
        public bool useImmediateTouchGestures;
        /// <summary>Чи pinch фокус під пальців — keepPinchFocusUnderFingers.</summary>
        public bool keepPinchFocusUnderFingers;

        /// <summary>Нормалізує значення в допустимі межі.</summary>
        public CameraControlProfile Normalize()
        {
            float normalizedMinZoom = Mathf.Max(0.1f, minZoom);
            return new CameraControlProfile
            {
                moveSpeed = Mathf.Max(0.01f, moveSpeed),
                sprintMultiplier = sprintMultiplier > 0f ? Mathf.Max(0.01f, sprintMultiplier) : 2.5f,
                smoothTime = Mathf.Max(0.01f, smoothTime),
                zoomSpeed = Mathf.Max(0.01f, zoomSpeed),
                rotationSpeed = rotationSpeed > 0f
                    ? Mathf.Max(0.01f, rotationSpeed)
                    : DefaultRotationSpeed,
                rotationAcceleration = rotationAcceleration > 0f ? rotationAcceleration : 540f,
                rotationDeceleration = rotationDeceleration > 0f ? rotationDeceleration : 720f,
                pointerOrbitSensitivity = pointerOrbitSensitivity > 0f ? pointerOrbitSensitivity : 0.25f,
                closeZoomRotationMultiplier = closeZoomRotationMultiplier > 0f ? closeZoomRotationMultiplier : 0.65f,
                farZoomRotationMultiplier = farZoomRotationMultiplier > 0f ? farZoomRotationMultiplier : 1.15f,
                closeZoomMoveMultiplier = closeZoomMoveMultiplier > 0f ? closeZoomMoveMultiplier : 0.7f,
                farZoomMoveMultiplier = farZoomMoveMultiplier > 0f ? farZoomMoveMultiplier : 1.35f,
                minZoom = normalizedMinZoom,
                maxZoom = Mathf.Max(normalizedMinZoom + 0.1f, maxZoom),
                touchMoveSpeed = Mathf.Max(0.01f, touchMoveSpeed),
                touchPinchZoomSensitivity = Mathf.Max(0.01f, touchPinchZoomSensitivity),
                touchDragDeadZonePixels = Mathf.Max(0f, touchDragDeadZonePixels),
                touchPinchDeadZonePixels = Mathf.Max(0f, touchPinchDeadZonePixels),
                maxTouchDeltaPixels = Mathf.Max(1f, maxTouchDeltaPixels),
                touchPinchDominancePixels = touchPinchDominancePixels > 0f ? touchPinchDominancePixels : 14f,
                touchTwistDominanceDegrees = touchTwistDominanceDegrees > 0f ? touchTwistDominanceDegrees : 7f,
                touchSettleFrames = Mathf.Clamp(touchSettleFrames, 0, 8),
                useImmediateTouchGestures = useImmediateTouchGestures,
                keepPinchFocusUnderFingers = keepPinchFocusUnderFingers,
            };
        }

        /// <summary>Створює мʼякий профіль типових значень керування.</summary>
        public static CameraControlProfile CreateGentleDefaults()
        {
            return new CameraControlProfile
            {
                moveSpeed = 3.2f,
                sprintMultiplier = 2.5f,
                smoothTime = 0.42f,
                zoomSpeed = 2.4f,
                rotationSpeed = DefaultRotationSpeed,
                rotationAcceleration = 540f,
                rotationDeceleration = 720f,
                pointerOrbitSensitivity = 0.25f,
                closeZoomRotationMultiplier = 0.65f,
                farZoomRotationMultiplier = 1.15f,
                closeZoomMoveMultiplier = 0.7f,
                farZoomMoveMultiplier = 1.35f,
                minZoom = 2f,
                maxZoom = 70f,
                touchMoveSpeed = 0.9f,
                touchPinchZoomSensitivity = 0.85f,
                touchDragDeadZonePixels = 1f,
                touchPinchDeadZonePixels = 3f,
                maxTouchDeltaPixels = 80f,
                touchPinchDominancePixels = 14f,
                touchTwistDominanceDegrees = 7f,
                touchSettleFrames = 2,
                useImmediateTouchGestures = false,
                keepPinchFocusUnderFingers = true,
            };
        }
    }

    /// <summary>CameraFarViewSettings — struct: камери дальнього виду налаштування.</summary>
    [System.Serializable]
    public struct CameraFarViewSettings
    {
        [Range(0f, 1f)] public float start;
        [Range(0f, 1f)] public float full;
        [Min(0.05f)] public float smoothing;
        [Range(0.2f, 3f)] public float shape;

        /// <summary>Створює екземпляр із типовими значеннями.</summary>
        public static CameraFarViewSettings CreateDefault()
        {
            return new CameraFarViewSettings
            {
                start = 0.70f,
                full = 0.95f,
                smoothing = 4f,
                shape = 1f,
            };
        }

        /// <summary>Нормалізує значення в допустимі межі.</summary>
        public CameraFarViewSettings Normalize()
        {
            float s = Mathf.Clamp01(start);
            return new CameraFarViewSettings
            {
                start = s,
                full = Mathf.Clamp(Mathf.Max(full, s + 0.02f), 0.02f, 1f),
                smoothing = Mathf.Max(0.05f, smoothing),
                shape = Mathf.Clamp(shape <= 0f ? 1f : shape, 0.2f, 3f),
            };
        }
    }

    /// <summary>CameraFocusSettings — struct: камери фокус налаштування.</summary>
    [System.Serializable]
    public struct CameraFocusSettings
    {
        [Min(0.05f)] public float minDuration;
        [Min(0.05f)] public float maxDuration;
        [Tooltip("Navigation-plane distance at which the transition reaches maxDuration.")]
        [Min(0.1f)] public float distanceForMaxDuration;
        [Tooltip("Screen margin multiplier applied when fitting object bounds.")]
        [Min(1f)] public float boundsPadding;
        [Tooltip("Relative zoom change below which the focus transition keeps the current zoom.")]
        [Range(0f, 0.75f)] public float zoomChangeThreshold;
        [Tooltip("Duration multiplier applied when Reduce Camera Motion is enabled.")]
        [Range(0.1f, 1f)] public float reducedMotionDurationScale;

        /// <summary>Створює екземпляр із типовими значеннями.</summary>
        public static CameraFocusSettings CreateDefault()
        {
            return new CameraFocusSettings
            {
                minDuration = 0.28f,
                maxDuration = 0.8f,
                distanceForMaxDuration = 45f,
                boundsPadding = 1.35f,
                zoomChangeThreshold = 0.12f,
                reducedMotionDurationScale = 0.55f,
            };
        }

        /// <summary>Нормалізує значення в допустимі межі.</summary>
        public CameraFocusSettings Normalize()
        {
            float min = Mathf.Max(0.05f, minDuration);
            return new CameraFocusSettings
            {
                minDuration = min,
                maxDuration = Mathf.Max(min, maxDuration),
                distanceForMaxDuration = Mathf.Max(0.1f, distanceForMaxDuration),
                boundsPadding = Mathf.Max(1f, boundsPadding),
                zoomChangeThreshold = Mathf.Clamp(zoomChangeThreshold, 0f, 0.75f),
                reducedMotionDurationScale = Mathf.Clamp(reducedMotionDurationScale, 0.1f, 1f),
            };
        }
    }

    /// <summary>CameraImpulseSettings — struct: камери імпульсу налаштування.</summary>
    [System.Serializable]
    public struct CameraImpulseSettings
    {
        [Tooltip("Absolute clamp for combined positional offset in world units.")]
        [Min(0.01f)] public float maxPositionOffset;
        [Tooltip("Absolute clamp for combined rotational offset in degrees.")]
        [Min(0.01f)] public float maxRotationDegrees;
        [Tooltip("Impulse scale at far zoom (NormalizedZoom = 1).")]
        [Range(0f, 1f)] public float farZoomScale;
        [Tooltip("Extra impulse scale on mobile platforms.")]
        [Range(0f, 1f)] public float mobileScale;
        [Tooltip("Impulse scale when Reduce Camera Motion is enabled.")]
        [Range(0f, 1f)] public float reduceMotionScale;
        [Range(1, 16)] public int maxConcurrent;
        [Tooltip("Default distance falloff radius in world units for positional impulses.")]
        [Min(1f)] public float defaultFalloffRadius;

        /// <summary>Створює екземпляр із типовими значеннями.</summary>
        public static CameraImpulseSettings CreateDefault()
        {
            return new CameraImpulseSettings
            {
                maxPositionOffset = 0.55f,
                maxRotationDegrees = 1.1f,
                farZoomScale = 0.35f,
                mobileScale = 0.6f,
                reduceMotionScale = 0.12f,
                maxConcurrent = 8,
                defaultFalloffRadius = 40f,
            };
        }

        /// <summary>Нормалізує значення в допустимі межі.</summary>
        public CameraImpulseSettings Normalize()
        {
            return new CameraImpulseSettings
            {
                maxPositionOffset = Mathf.Max(0.01f, maxPositionOffset),
                maxRotationDegrees = Mathf.Max(0.01f, maxRotationDegrees),
                farZoomScale = Mathf.Clamp01(farZoomScale),
                mobileScale = Mathf.Clamp01(mobileScale),
                reduceMotionScale = Mathf.Clamp01(reduceMotionScale),
                maxConcurrent = Mathf.Clamp(maxConcurrent, 1, 16),
                defaultFalloffRadius = Mathf.Max(1f, defaultFalloffRadius),
            };
        }
    }

    /// <summary>CameraSettingsSO — class: камери налаштування SO.</summary>
    [System.Serializable]
    public class CameraSettingsSO : JsonConfigObject
    {
        /// <summary>керування профілю — CameraControlProfile.</summary>
        [Header("Control Profile")]
        [FormerlySerializedAs("desktopProfile")]
        public CameraControlProfile controlProfile = CameraControlProfile.CreateGentleDefaults();

        /// <summary>межі переповнення тайлів — Vector2.</summary>
        [Header("World Bounds")]
        [Tooltip("How many tile units the camera viewport is allowed to go outside map bounds.")]
        public Vector2 boundsOverflowTiles = Vector2.zero;
        [Tooltip("Soft-resistance zone in world units before a hard bounds stop. 0 = hard stop.")]
        [Min(0f)] public float boundsSoftMargin = 1.5f;
        [Tooltip("Elastic pull-back speed (1/s) when the target rests inside the soft margin.")]
        [Min(0.1f)] public float boundsSoftReturnSpeed = 7f;

        [Header("Touch Gestures")]
        [Tooltip("Reference DPI for touch gesture thresholds. Thresholds scale with Screen.dpi / reference.")]
        [Min(1f)] public float touchDpiReference = 160f;
        [Min(0.1f)] public float touchDpiScaleMin = 0.75f;
        [Min(0.1f)] public float touchDpiScaleMax = 2.5f;

        /// <summary>фокус — CameraFocusSettings.</summary>
        [Header("Focus Transitions")]
        public CameraFocusSettings focus = CameraFocusSettings.CreateDefault();

        /// <summary>імпульсу — CameraImpulseSettings.</summary>
        [Header("Impulse / Shake")]
        public CameraImpulseSettings impulse = CameraImpulseSettings.CreateDefault();

        /// <summary>типового камери Z — float.</summary>
        [Header("Shared")]
        [HideInInspector]
        public float defaultCameraZ = -10f;

        /// <summary>адаптації  Project3 D режим — bool.</summary>
        [Header("3D Project Adaptation")]
        public bool adaptToProject3DMode = true;
        /// <summary>Чи ортографічного камери In3 D — useOrthographicCameraIn3D.</summary>
        public bool useOrthographicCameraIn3D = false;
        /// <summary>orthographic3 D Ейлера — Vector3.</summary>
        [Min(0.1f)] public float default3DCameraDistance = 20f;
        [Min(0.1f)] public float default3DOrthographicSize = 20f;
        [Range(1f, 179f)] public float default3DFieldOfView = 30f;
        public Vector3 orthographic3DEuler = new Vector3(90f, 0f, 0f);
        /// <summary>isometric3 D Ейлера — Vector3.</summary>
        public Vector3 isometric3DEuler = new Vector3(50f, 45f, 0f);

        /// <summary>поворот півоту шарів — LayerMask.</summary>
        [Header("Rotation Pivot")]
        [Tooltip("Maximum distance for the forward ray used to find the camera rotation pivot.")]
        [Min(0.1f)] public float rotationPivotRaycastDistance = 1000f;
        [Tooltip("Physics layers that can provide a camera rotation pivot. Falls back to the grid plane when nothing is hit.")]
        public LayerMask rotationPivotLayers = Physics.DefaultRaycastLayers;

        /// <summary>краю прокручування увімкненої — bool.</summary>
        [Header("PC Edge Scroll")]
        [Tooltip("Disabled by default. Can be exposed by the gameplay controls settings UI.")]
        public bool edgeScrollEnabled;
        [Min(1f)] public float edgeScrollMarginPixels = 12f;
        [Min(0.01f)] public float edgeScrollSpeedMultiplier = 1f;

        /// <summary>Чи автоматичного mip зсув — enableAutomaticMipBias.</summary>
        [Header("Shader / Mip Bias")]
        [Tooltip("Applies global automatic mip bias for zoom. Disable to avoid tile atlas artifacts/bleeding on zoom-out.")]
        [HideInInspector]
        public bool enableAutomaticMipBias = false;
        [HideInInspector]
        [Range(0f, 3f)] public float automaticMipBiasMax = 0.75f;

        /// <summary>дальнього виду — CameraFarViewSettings.</summary>
        [Header("Far View Window")]
        [Tooltip("Shared normalized-zoom window where the world reads as distant/high-altitude. Consumed by far-view visuals and zoom-driven audio.")]
        public CameraFarViewSettings farView = CameraFarViewSettings.CreateDefault();

        /// <summary>карти рендер маски увімкненої — bool.</summary>
        [Header("Map Render Mask")]
        [HideInInspector]
        public bool mapRenderMaskEnabled = true;
        /// <summary>карти маски шарів — LayerMask.</summary>
        [HideInInspector]
        [Min(0.05f)] public float mapMaskRefreshSeconds = 0.5f;
        [HideInInspector]
        public LayerMask mapMaskLayers = ~0;
        /// <summary>карти маски сортування шару назву — string.</summary>
        [HideInInspector]
        public string mapMaskSortingLayerName = "Default";
        /// <summary>ручного карти маски центру — Vector2.</summary>
        [HideInInspector]
        [Range(-32768, 32767)] public int mapMaskBackSortingOrder = -32768;
        [HideInInspector]
        [Range(-32768, 32767)] public int mapMaskFrontSortingOrder = 32767;
        [HideInInspector]
        public Vector2 manualMapMaskCenter = new Vector2(4.5f, 4.5f);
        /// <summary>ручного карти маски розміру — Vector2.</summary>
        [HideInInspector]
        public Vector2 manualMapMaskSize = new Vector2(10f, 10f);

        /// <summary>Повертає активної профілю.</summary>
        public CameraControlProfile ResolveActiveProfile()
        {
            return controlProfile.Normalize();
        }

        /// <summary>Повертає переміщення швидкість.</summary>
        public float ResolveMoveSpeed() => ResolveActiveProfile().moveSpeed;
        /// <summary>Повертає спринт множника.</summary>
        public float ResolveSprintMultiplier() => ResolveActiveProfile().sprintMultiplier;
        /// <summary>Повертає плавного часу.</summary>
        public float ResolveSmoothTime() => ResolveActiveProfile().smoothTime;
        /// <summary>Повертає зум швидкість.</summary>
        public float ResolveZoomSpeed() => ResolveActiveProfile().zoomSpeed;
        /// <summary>Повертає поворот швидкість.</summary>
        public float ResolveRotationSpeed() => ResolveActiveProfile().rotationSpeed;
        /// <summary>Повертає поворот прискорення.</summary>
        public float ResolveRotationAcceleration() => ResolveActiveProfile().rotationAcceleration;
        /// <summary>Повертає поворот сповільнення.</summary>
        public float ResolveRotationDeceleration() => ResolveActiveProfile().rotationDeceleration;
        /// <summary>Повертає курсора орбіти чутливість.</summary>
        public float ResolvePointerOrbitSensitivity() => ResolveActiveProfile().pointerOrbitSensitivity;
        /// <summary>Повертає закриття зум поворот множника.</summary>
        public float ResolveCloseZoomRotationMultiplier() => ResolveActiveProfile().closeZoomRotationMultiplier;
        /// <summary>Повертає дальнього зум поворот множника.</summary>
        public float ResolveFarZoomRotationMultiplier() => ResolveActiveProfile().farZoomRotationMultiplier;
        /// <summary>Повертає закриття зум переміщення множника.</summary>
        public float ResolveCloseZoomMoveMultiplier() => ResolveActiveProfile().closeZoomMoveMultiplier;
        /// <summary>Повертає дальнього зум переміщення множника.</summary>
        public float ResolveFarZoomMoveMultiplier() => ResolveActiveProfile().farZoomMoveMultiplier;
        /// <summary>Повертає мінімум зум.</summary>
        public float ResolveMinZoom() => ResolveActiveProfile().minZoom;
        /// <summary>Повертає максимум зум.</summary>
        public float ResolveMaxZoom() => ResolveActiveProfile().maxZoom;
        /// <summary>Повертає дотику переміщення швидкість.</summary>
        public float ResolveTouchMoveSpeed() => ResolveActiveProfile().touchMoveSpeed;
        /// <summary>Повертає дотику pinch зум чутливість.</summary>
        public float ResolveTouchPinchZoomSensitivity() => ResolveActiveProfile().touchPinchZoomSensitivity;
        /// <summary>Повертає дотику перетягування мертвої зони у пікселях.</summary>
        public float ResolveTouchDragDeadZonePixels() => ResolveActiveProfile().touchDragDeadZonePixels;
        /// <summary>Повертає дотику pinch мертвої зони у пікселях.</summary>
        public float ResolveTouchPinchDeadZonePixels() => ResolveActiveProfile().touchPinchDeadZonePixels;
        /// <summary>Повертає максимум дотику дельти у пікселях.</summary>
        public float ResolveMaxTouchDeltaPixels() => ResolveActiveProfile().maxTouchDeltaPixels;
        /// <summary>Повертає дотику pinch домінування у пікселях.</summary>
        public float ResolveTouchPinchDominancePixels() => ResolveActiveProfile().touchPinchDominancePixels;
        /// <summary>Повертає дотику twist домінування у градусах.</summary>
        public float ResolveTouchTwistDominanceDegrees() => ResolveActiveProfile().touchTwistDominanceDegrees;
        /// <summary>Повертає дотику осідання кадрів.</summary>
        public int ResolveTouchSettleFrames() => ResolveActiveProfile().touchSettleFrames;
        /// <summary>Повертає використання негайного дотику жестів.</summary>
        public bool ResolveUseImmediateTouchGestures() => ResolveActiveProfile().useImmediateTouchGestures;
        /// <summary>Повертає збереження pinch фокус під пальців.</summary>
        public bool ResolveKeepPinchFocusUnderFingers() => ResolveActiveProfile().keepPinchFocusUnderFingers;
        /// <summary>Повертає адаптації  Project3 D режим.</summary>
        public bool ResolveAdaptToProject3DMode() => adaptToProject3DMode;
        /// <summary>Повертає використання ортографічного камери In3 D.</summary>
        public bool ResolveUseOrthographicCameraIn3D() => useOrthographicCameraIn3D;
        /// <summary>Повертає Default3 D камери відстані.</summary>
        public float ResolveDefault3DCameraDistance() => Mathf.Max(0.1f, default3DCameraDistance);
        /// <summary>Повертає Default3 D ортографічного розміру.</summary>
        public float ResolveDefault3DOrthographicSize() => Mathf.Max(ResolveMinZoom(), default3DOrthographicSize);
        /// <summary>Повертає Default3 D поля  виду.</summary>
        public float ResolveDefault3DFieldOfView() => Mathf.Clamp(default3DFieldOfView, 1f, 179f);
        /// <summary>Повертає поворот півоту рейкасту відстані.</summary>
        public float ResolveRotationPivotRaycastDistance() => Mathf.Max(0.1f, rotationPivotRaycastDistance);
        /// <summary>Повертає поворот півоту шару маски.</summary>
        public int ResolveRotationPivotLayerMask() => rotationPivotLayers.value;
        /// <summary>Повертає краю прокручування увімкненої.</summary>
        public bool ResolveEdgeScrollEnabled() => edgeScrollEnabled;
        /// <summary>Повертає краю прокручування відступу у пікселях.</summary>
        public float ResolveEdgeScrollMarginPixels()
            => Mathf.Max(1f, edgeScrollMarginPixels);
        /// <summary>Повертає краю прокручування швидкість множника.</summary>
        public float ResolveEdgeScrollSpeedMultiplier()
            => Mathf.Max(0.01f, edgeScrollSpeedMultiplier);
        /// <summary>Повертає межі переповнення світу одиниць.</summary>
        public Vector2 ResolveBoundsOverflowWorldUnits() => new Vector2(
            Mathf.Max(0f, boundsOverflowTiles.x),
            Mathf.Max(0f, boundsOverflowTiles.y));
        /// <summary>Повертає Enable автоматичного mip зсув.</summary>
        public bool ResolveEnableAutomaticMipBias() => enableAutomaticMipBias;
        /// <summary>Повертає автоматичного mip зсув максимум.</summary>
        public float ResolveAutomaticMipBiasMax() => Mathf.Clamp(automaticMipBiasMax, 0f, 3f);
        /// <summary>Повертає межі мʼякої відступу.</summary>
        public float ResolveBoundsSoftMargin() => Mathf.Max(0f, boundsSoftMargin);
        /// <summary>Повертає межі мʼякої повернення швидкість.</summary>
        public float ResolveBoundsSoftReturnSpeed() => Mathf.Max(0.1f, boundsSoftReturnSpeed);
        /// <summary>Повертає дотику DPI референсу.</summary>
        public float ResolveTouchDpiReference() => Mathf.Max(1f, touchDpiReference);
        /// <summary>Повертає дотику DPI масштаб мінімум.</summary>
        public float ResolveTouchDpiScaleMin() => Mathf.Max(0.1f, touchDpiScaleMin);
        /// <summary>Повертає дотику DPI масштаб максимум.</summary>
        public float ResolveTouchDpiScaleMax() => Mathf.Max(ResolveTouchDpiScaleMin(), touchDpiScaleMax);
        /// <summary>Повертає фокус.</summary>
        public CameraFocusSettings ResolveFocus() => focus.Normalize();
        /// <summary>Повертає імпульсу.</summary>
        public CameraImpulseSettings ResolveImpulse() => impulse.Normalize();
        /// <summary>Повертає дальнього виду початку.</summary>
        public float ResolveFarViewStart() => farView.Normalize().start;
        /// <summary>Повертає дальнього виду повного.</summary>
        public float ResolveFarViewFull() => farView.Normalize().full;
        /// <summary>Повертає дальнього виду згладжування.</summary>
        public float ResolveFarViewSmoothing() => farView.Normalize().smoothing;
        /// <summary>Повертає дальнього виду Shape.</summary>
        public float ResolveFarViewShape() => farView.Normalize().shape;

        private void OnValidate()
        {
            controlProfile = controlProfile.Normalize();
            boundsOverflowTiles = new Vector2(
                Mathf.Max(0f, boundsOverflowTiles.x),
                Mathf.Max(0f, boundsOverflowTiles.y));
            default3DCameraDistance = Mathf.Max(0.1f, default3DCameraDistance);
            default3DOrthographicSize = Mathf.Max(ResolveMinZoom(), default3DOrthographicSize);
            default3DFieldOfView = Mathf.Clamp(default3DFieldOfView, 1f, 179f);
            rotationPivotRaycastDistance = Mathf.Max(0.1f, rotationPivotRaycastDistance);
            edgeScrollMarginPixels = Mathf.Max(1f, edgeScrollMarginPixels);
            edgeScrollSpeedMultiplier = Mathf.Max(0.01f, edgeScrollSpeedMultiplier);
            automaticMipBiasMax = Mathf.Clamp(automaticMipBiasMax, 0f, 3f);
            boundsSoftMargin = Mathf.Max(0f, boundsSoftMargin);
            boundsSoftReturnSpeed = Mathf.Max(0.1f, boundsSoftReturnSpeed);
            touchDpiReference = Mathf.Max(1f, touchDpiReference);
            touchDpiScaleMin = Mathf.Max(0.1f, touchDpiScaleMin);
            touchDpiScaleMax = Mathf.Max(touchDpiScaleMin, touchDpiScaleMax);
            focus = focus.Normalize();
            impulse = impulse.Normalize();
            farView = farView.Normalize();
        }
    }
}
