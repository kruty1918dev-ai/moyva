using UnityEngine;
using UnityEngine.Serialization;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>
    /// Профіль керування камерою: швидкості руху, зуму, обертання та touch-жестів.
    /// </summary>
    [System.Serializable]
    public struct CameraControlProfile
    {
        private const float DefaultRotationSpeed = 90f;

        [Min(0.01f)] public float moveSpeed;
        [Min(0.01f)] public float smoothTime;
        [Min(0.01f)] public float zoomSpeed;
        [Min(0.01f)] public float rotationSpeed;
        [Min(0.01f)] public float rotationAcceleration;
        [Min(0.01f)] public float rotationDeceleration;
        [Min(0.001f)] public float pointerOrbitSensitivity;
        [Min(0.01f)] public float closeZoomRotationMultiplier;
        [Min(0.01f)] public float farZoomRotationMultiplier;
        [Min(0.1f)] public float minZoom;
        [Min(0.2f)] public float maxZoom;

        [Min(0.01f)] public float touchMoveSpeed;
        [Min(0.01f)] public float touchPinchZoomSensitivity;
        [Min(0f)] public float touchDragDeadZonePixels;
        [Min(0f)] public float touchPinchDeadZonePixels;
        [Min(1f)] public float maxTouchDeltaPixels;
        /// <summary>Чи застосовувати touch-жести миттєво без інерції.</summary>
        public bool useImmediateTouchGestures;
        /// <summary>Чи тримати фокус pinch-зуму під пальцями.</summary>
        public bool keepPinchFocusUnderFingers;

        /// <summary>
        /// Повертає копію профілю з клампнутими у допустимі діапазони значеннями.
        /// </summary>
        public CameraControlProfile Normalize()
        {
            float normalizedMinZoom = Mathf.Max(0.1f, minZoom);
            return new CameraControlProfile
            {
                moveSpeed = Mathf.Max(0.01f, moveSpeed),
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
                minZoom = normalizedMinZoom,
                maxZoom = Mathf.Max(normalizedMinZoom + 0.1f, maxZoom),
                touchMoveSpeed = Mathf.Max(0.01f, touchMoveSpeed),
                touchPinchZoomSensitivity = Mathf.Max(0.01f, touchPinchZoomSensitivity),
                touchDragDeadZonePixels = Mathf.Max(0f, touchDragDeadZonePixels),
                touchPinchDeadZonePixels = Mathf.Max(0f, touchPinchDeadZonePixels),
                maxTouchDeltaPixels = Mathf.Max(1f, maxTouchDeltaPixels),
                useImmediateTouchGestures = useImmediateTouchGestures,
                keepPinchFocusUnderFingers = keepPinchFocusUnderFingers,
            };
        }

        /// <summary>
        /// Створює профіль із м'якими усталеними значеннями для комфортного керування.
        /// </summary>
        public static CameraControlProfile CreateGentleDefaults()
        {
            return new CameraControlProfile
            {
                moveSpeed = 3.2f,
                smoothTime = 0.42f,
                zoomSpeed = 2.4f,
                rotationSpeed = DefaultRotationSpeed,
                rotationAcceleration = 540f,
                rotationDeceleration = 720f,
                pointerOrbitSensitivity = 0.25f,
                closeZoomRotationMultiplier = 0.65f,
                farZoomRotationMultiplier = 1.15f,
                minZoom = 2f,
                maxZoom = 70f,
                touchMoveSpeed = 0.9f,
                touchPinchZoomSensitivity = 0.85f,
                touchDragDeadZonePixels = 1f,
                touchPinchDeadZonePixels = 3f,
                maxTouchDeltaPixels = 80f,
                useImmediateTouchGestures = false,
                keepPinchFocusUnderFingers = true,
            };
        }
    }

    /// <summary>
    /// Спільне far-view ("висотне") вікно над нормалізованим зумом.
    /// Нейтральне камерне визначення: ICameraZoomState.FarViewWeight зростає
    /// від 0 на <see cref="start"/> до 1 на <see cref="full"/>. Візуальні та
    /// аудіо системи читають ту саму вагу, тому переходи лишаються синхронними.
    /// </summary>
    [System.Serializable]
    public struct CameraFarViewSettings
    {
        [Range(0f, 1f)] public float start;
        [Range(0f, 1f)] public float full;
        [Min(0.05f)] public float smoothing;
        [Range(0.2f, 3f)] public float shape;

        /// <summary>
        /// Створює усталене far-view вікно.
        /// </summary>
        public static CameraFarViewSettings CreateDefault()
        {
            return new CameraFarViewSettings
            {
                start = 0.30f,
                full = 0.85f,
                smoothing = 4f,
                shape = 1f,
            };
        }

        /// <summary>
        /// Повертає копію вікна з клампнутими у допустимі діапазони значеннями.
        /// </summary>
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

    /// <summary>
    /// Налаштування камери гри: профіль керування, межі світу, 3D-адаптація,
    /// edge-scroll, mip-bias та far-view вікно.
    /// </summary>
    [System.Serializable]
    public class CameraSettingsSO : MoyvaJsonConfigObject
    {
        /// <summary>Активний профіль керування камерою.</summary>
        [Header("Control Profile")]
        [FormerlySerializedAs("desktopProfile")]
        public CameraControlProfile controlProfile = CameraControlProfile.CreateGentleDefaults();

        /// <summary>На скільки тайлових одиниць в'юпорт камери може виходити за межі карти.</summary>
        [Header("World Bounds")]
        [Tooltip("How many tile units the camera viewport is allowed to go outside map bounds.")]
        public Vector2 boundsOverflowTiles = Vector2.zero;

        /// <summary>Усталена Z-координата камери.</summary>
        [Header("Shared")]
        [HideInInspector]
        public float defaultCameraZ = -10f;

        /// <summary>Чи адаптувати камеру під 3D-режим проєкту.</summary>
        [Header("3D Project Adaptation")]
        public bool adaptToProject3DMode = true;
        /// <summary>Чи використовувати ортографічну камеру в 3D-режимі.</summary>
        public bool useOrthographicCameraIn3D = false;
        [Min(0.1f)] public float default3DCameraDistance = 20f;
        [Min(0.1f)] public float default3DOrthographicSize = 20f;
        /// <summary>Усталене поле зору перспективної камери в 3D-режимі.</summary>
        [Range(1f, 179f)] public float default3DFieldOfView = 30f;
        /// <summary>Кути Ейлера ортографічної камери, що дивиться вертикально вниз.</summary>
        public Vector3 orthographic3DEuler = new Vector3(90f, 0f, 0f);
        /// <summary>Кути Ейлера ізометричної камери.</summary>
        public Vector3 isometric3DEuler = new Vector3(50f, 45f, 0f);

        /// <summary>Максимальна дистанція променя для пошуку точки обертання камери.</summary>
        [Header("Rotation Pivot")]
        [Tooltip("Maximum distance for the forward ray used to find the camera rotation pivot.")]
        [Min(0.1f)] public float rotationPivotRaycastDistance = 1000f;
        /// <summary>Фізичні шари, що можуть дати точку обертання; fallback — площина сітки.</summary>
        [Tooltip("Physics layers that can provide a camera rotation pivot. Falls back to the grid plane when nothing is hit.")]
        public LayerMask rotationPivotLayers = Physics.DefaultRaycastLayers;

        /// <summary>Чи ввімкнено скрол краєм екрана на PC.</summary>
        [Header("PC Edge Scroll")]
        [Tooltip("Disabled by default. Can be exposed by the gameplay controls settings UI.")]
        public bool edgeScrollEnabled;
        [Min(1f)] public float edgeScrollMarginPixels = 12f;
        [Min(0.01f)] public float edgeScrollSpeedMultiplier = 1f;

        /// <summary>Чи вмикати глобальний автоматичний mip bias для зуму.</summary>
        [Header("Shader / Mip Bias")]
        [Tooltip("Applies global automatic mip bias for zoom. Disable to avoid tile atlas artifacts/bleeding on zoom-out.")]
        [HideInInspector]
        public bool enableAutomaticMipBias = false;
        [HideInInspector]
        [Range(0f, 3f)] public float automaticMipBiasMax = 0.75f;

        /// <summary>Спільне нормалізоване вікно зуму, де світ читається як далекий/висотний.</summary>
        [Header("Far View Window")]
        [Tooltip("Shared normalized-zoom window where the world reads as distant/high-altitude. Consumed by far-view visuals and zoom-driven audio.")]
        public CameraFarViewSettings farView = CameraFarViewSettings.CreateDefault();

        /// <summary>Чи ввімкнено маску рендера карти.</summary>
        [Header("Map Render Mask")]
        [HideInInspector]
        public bool mapRenderMaskEnabled = true;
        [HideInInspector]
        [Min(0.05f)] public float mapMaskRefreshSeconds = 0.5f;
        /// <summary>Шари, що потрапляють у маску карти.</summary>
        [HideInInspector]
        public LayerMask mapMaskLayers = ~0;
        /// <summary>Назва sorting layer для маски карти.</summary>
        [HideInInspector]
        public string mapMaskSortingLayerName = "Default";
        [HideInInspector]
        [Range(-32768, 32767)] public int mapMaskBackSortingOrder = -32768;
        [HideInInspector]
        [Range(-32768, 32767)] public int mapMaskFrontSortingOrder = 32767;
        /// <summary>Ручний центр маски карти.</summary>
        [HideInInspector]
        public Vector2 manualMapMaskCenter = new Vector2(4.5f, 4.5f);
        /// <summary>Ручний розмір маски карти.</summary>
        [HideInInspector]
        public Vector2 manualMapMaskSize = new Vector2(10f, 10f);

        /// <summary>
        /// Повертає нормалізований активний профіль керування.
        /// </summary>
        public CameraControlProfile ResolveActiveProfile()
        {
            return controlProfile.Normalize();
        }

        /// <summary>Розв'язує чинну швидкість руху камери.</summary>
        public float ResolveMoveSpeed() => ResolveActiveProfile().moveSpeed;
        /// <summary>Розв'язує чинний час згладжування руху.</summary>
        public float ResolveSmoothTime() => ResolveActiveProfile().smoothTime;
        /// <summary>Розв'язує чинну швидкість зуму.</summary>
        public float ResolveZoomSpeed() => ResolveActiveProfile().zoomSpeed;
        /// <summary>Розв'язує чинну швидкість обертання.</summary>
        public float ResolveRotationSpeed() => ResolveActiveProfile().rotationSpeed;
        /// <summary>Розв'язує чинне прискорення обертання.</summary>
        public float ResolveRotationAcceleration() => ResolveActiveProfile().rotationAcceleration;
        /// <summary>Розв'язує чинне сповільнення обертання.</summary>
        public float ResolveRotationDeceleration() => ResolveActiveProfile().rotationDeceleration;
        /// <summary>Розв'язує чинну чутливість orbit-обертання вказівником.</summary>
        public float ResolvePointerOrbitSensitivity() => ResolveActiveProfile().pointerOrbitSensitivity;
        /// <summary>Розв'язує множник швидкості обертання на близькому зумі.</summary>
        public float ResolveCloseZoomRotationMultiplier() => ResolveActiveProfile().closeZoomRotationMultiplier;
        /// <summary>Розв'язує множник швидкості обертання на далекому зумі.</summary>
        public float ResolveFarZoomRotationMultiplier() => ResolveActiveProfile().farZoomRotationMultiplier;
        /// <summary>Розв'язує мінімальний зум.</summary>
        public float ResolveMinZoom() => ResolveActiveProfile().minZoom;
        /// <summary>Розв'язує максимальний зум.</summary>
        public float ResolveMaxZoom() => ResolveActiveProfile().maxZoom;
        /// <summary>Розв'язує швидкість touch-руху.</summary>
        public float ResolveTouchMoveSpeed() => ResolveActiveProfile().touchMoveSpeed;
        /// <summary>Розв'язує чутливість pinch-зуму.</summary>
        public float ResolveTouchPinchZoomSensitivity() => ResolveActiveProfile().touchPinchZoomSensitivity;
        /// <summary>Розв'язує мертву зону drag-жесту в пікселях.</summary>
        public float ResolveTouchDragDeadZonePixels() => ResolveActiveProfile().touchDragDeadZonePixels;
        /// <summary>Розв'язує мертву зону pinch-жесту в пікселях.</summary>
        public float ResolveTouchPinchDeadZonePixels() => ResolveActiveProfile().touchPinchDeadZonePixels;
        /// <summary>Розв'язує максимальну touch-дельту в пікселях за кадр.</summary>
        public float ResolveMaxTouchDeltaPixels() => ResolveActiveProfile().maxTouchDeltaPixels;
        /// <summary>Розв'язує чи застосовувати touch-жести миттєво.</summary>
        public bool ResolveUseImmediateTouchGestures() => ResolveActiveProfile().useImmediateTouchGestures;
        /// <summary>Розв'язує чи тримати фокус pinch-зуму під пальцями.</summary>
        public bool ResolveKeepPinchFocusUnderFingers() => ResolveActiveProfile().keepPinchFocusUnderFingers;
        /// <summary>Розв'язує чи адаптувати камеру під 3D-режим проєкту.</summary>
        public bool ResolveAdaptToProject3DMode() => adaptToProject3DMode;
        /// <summary>Розв'язує чи використовувати ортографічну камеру в 3D.</summary>
        public bool ResolveUseOrthographicCameraIn3D() => useOrthographicCameraIn3D;
        /// <summary>Розв'язує усталену дистанцію 3D-камери.</summary>
        public float ResolveDefault3DCameraDistance() => Mathf.Max(0.1f, default3DCameraDistance);
        /// <summary>Розв'язує усталений ортографічний розмір 3D-камери.</summary>
        public float ResolveDefault3DOrthographicSize() => Mathf.Max(ResolveMinZoom(), default3DOrthographicSize);
        /// <summary>Розв'язує усталене поле зору 3D-камери.</summary>
        public float ResolveDefault3DFieldOfView() => Mathf.Clamp(default3DFieldOfView, 1f, 179f);
        /// <summary>Розв'язує максимальну дистанцію променя точки обертання.</summary>
        public float ResolveRotationPivotRaycastDistance() => Mathf.Max(0.1f, rotationPivotRaycastDistance);
        /// <summary>Розв'язує маску шарів для пошуку точки обертання.</summary>
        public int ResolveRotationPivotLayerMask() => rotationPivotLayers.value;
        /// <summary>Розв'язує чи ввімкнено edge-scroll.</summary>
        public bool ResolveEdgeScrollEnabled() => edgeScrollEnabled;
        /// <summary>Розв'язує поля edge-scroll у пікселях.</summary>
        public float ResolveEdgeScrollMarginPixels()
            => Mathf.Max(1f, edgeScrollMarginPixels);
        /// <summary>Розв'язує множник швидкості edge-scroll.</summary>
        public float ResolveEdgeScrollSpeedMultiplier()
            => Mathf.Max(0.01f, edgeScrollSpeedMultiplier);
        /// <summary>Розв'язує переповнення меж карти у світових одиницях.</summary>
        public Vector2 ResolveBoundsOverflowWorldUnits() => new Vector2(
            Mathf.Max(0f, boundsOverflowTiles.x),
            Mathf.Max(0f, boundsOverflowTiles.y));
        /// <summary>Розв'язує чи ввімкнено автоматичний mip bias.</summary>
        public bool ResolveEnableAutomaticMipBias() => enableAutomaticMipBias;
        /// <summary>Розв'язує максимальний автоматичний mip bias.</summary>
        public float ResolveAutomaticMipBiasMax() => Mathf.Clamp(automaticMipBiasMax, 0f, 3f);
        /// <summary>Розв'язує початок far-view вікна.</summary>
        public float ResolveFarViewStart() => farView.Normalize().start;
        /// <summary>Розв'язує межу повного far-view.</summary>
        public float ResolveFarViewFull() => farView.Normalize().full;
        /// <summary>Розв'язує згладжування far-view ваги.</summary>
        public float ResolveFarViewSmoothing() => farView.Normalize().smoothing;
        /// <summary>Розв'язує форму кривої far-view ваги.</summary>
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
            farView = farView.Normalize();
        }
    }
}
