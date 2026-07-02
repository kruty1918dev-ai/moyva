using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TileWorldCreatorManager))]
    [HideMonoScript]
    /// <summary>
    /// Тонкий Unity/Odin host-компонент для volume-based FogOfWar presentation.
    /// Зберігає scene settings, підключається до runtime updater-а через Zenject
    /// і делегує preview/validation/output cleanup профільним сервісам.
    /// Не повинен містити gameplay fog logic або створювати runtime сервіси у play mode напряму.
    /// </summary>
    public sealed class FogOfWarVolumeController : MonoBehaviour, IFogVolumePreviewHost, IFogVolumeSceneContextHost, IFogVolumeValidationHost
    {
        private const string StartDiagTag = "[MoyvaFogStartDiag]";

        [TitleGroup("Settings")]
        [Required]
        [ValidateInput(nameof(HasSettings), "Assign FogOfWarSettings.")]
        [SerializeField] private FogOfWarSettings _settings;

        [TitleGroup("Runtime Overrides")]
        [SerializeField] private bool _overrideUpdateMode;

        [TitleGroup("Runtime Overrides")]
        [ShowIf(nameof(_overrideUpdateMode))]
        [SerializeField] private FogVolumeUpdateMode _updateMode = FogVolumeUpdateMode.DebouncePerFrame;

        [TitleGroup("Runtime Overrides")]
        [ShowIf(nameof(UsesIntervalUpdate))]
        [MinValue(0.02f)]
        [SerializeField] private float _rebuildIntervalSeconds = 0.1f;

        [TitleGroup("Runtime Overrides")]
        [SerializeField] private bool _overrideCellSize;

        [TitleGroup("Runtime Overrides")]
        [ShowIf(nameof(_overrideCellSize))]
        [MinValue(0.001f)]
        [SerializeField] private float _cellSizeOverride = 1f;

        [TitleGroup("Runtime Overrides")]
        [MinValue(0f)]
        [SerializeField] private float _additionalTopClearance;

        [TitleGroup("Debug Logging")]
        [SerializeField] private bool _logBuildSummary = true;

        [TitleGroup("Debug Logging")]
        [ShowIf(nameof(_logBuildSummary))]
        [SerializeField] private bool _logEveryVolumeUpdate;

        [TitleGroup("Debug Logging")]
        [SerializeField] private bool _logValidationWarnings = true;

        [TitleGroup("Startup Fallback")]
        [SerializeField] private bool _revealStartupFallbackArea = true;

        [TitleGroup("Startup Fallback")]
        [ShowIf(nameof(_revealStartupFallbackArea))]
        [MinValue(1)]
        [SerializeField] private int _startupFallbackRevealRadiusOverride;

        [TitleGroup("Validation")]
        [ShowInInspector]
        [ReadOnly]
        [MultiLineProperty(7)]
        [PropertyOrder(100)]
        private string ValidationSummary
        {
            get
            {
                var validationService = ResolveValidationService();
                return validationService != null
                    ? validationService.BuildValidationSummary(this)
                    : "Missing IFogVolumeValidationService runtime binding.";
            }
        }

        private IFogVolumeRuntimeUpdater _runtimeUpdater;
        private IFogVolumePreviewBuilder _previewBuilder;
        private IFogVolumeSceneContextBuilder _sceneContextBuilder;
        private IFogVolumeOutputCleaner _outputCleaner;
        private IFogVolumeValidationService _validationService;
        private TileWorldCreatorManager _tileWorldCreatorManager;
        private bool _loggedAwake;
        private bool _loggedConstruct;
        private bool _loggedRegisterWithoutUpdater;
        private bool _deferRuntimePreviewCleanupUntilStart;
        /// <summary>
        /// Fog settings asset, який визначає gameplay tuning і volume visual config.
        /// </summary>
        public FogOfWarSettings Settings => _settings;

        /// <summary>
        /// TWC manager на цьому ж GameObject, який тримає runtime/editor fog volume output.
        /// </summary>
        public TileWorldCreatorManager TileWorldCreatorManager => ResolveFogManager();

        /// <summary>
        /// Ефективний режим оновлення volume у runtime з урахуванням scene override.
        /// </summary>
        public FogVolumeUpdateMode EffectiveUpdateMode => _overrideUpdateMode
            ? _updateMode
            : (_settings != null ? _settings.Volume.UpdateMode : FogVolumeUpdateMode.DebouncePerFrame);

        /// <summary>
        /// Ефективний інтервал перебудови volume у runtime.
        /// </summary>
        public float EffectiveRebuildIntervalSeconds => Mathf.Max(
            0.02f,
            _overrideUpdateMode
                ? _rebuildIntervalSeconds
                : (_settings != null ? _settings.Volume.RebuildIntervalSeconds : 0.1f));

        /// <summary>
        /// Додатковий верхній clearance, який controller передає runtime volume build path.
        /// </summary>
        public float AdditionalTopClearance => Mathf.Max(0f, _additionalTopClearance);

        /// <summary>
        /// Чи дозволено підсумкове debug logging для побудови volume.
        /// </summary>
        public bool LogBuildSummary => _logBuildSummary;

        /// <summary>
        /// Чи слід логувати кожен volume update окремо.
        /// </summary>
        public bool LogEveryVolumeUpdate => _logEveryVolumeUpdate;

        /// <summary>
        /// Чи слід показувати validation warning-и для scene/setup проблем.
        /// </summary>
        public bool LogValidationWarnings => _logValidationWarnings;

        private bool UsesIntervalUpdate => EffectiveUpdateMode == FogVolumeUpdateMode.Interval;

        private bool CanRequestRuntimeRebuild
            => Application.isPlaying
                && _runtimeUpdater != null
                && _settings != null
                && ResolveFogManager() != null;

        private void Awake()
        {
            LogLifecycleOnce(ref _loggedAwake, "Awake", $"settings={(_settings != null ? _settings.name : "null")}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}, clearPreview={(_settings != null && _settings.Volume.ClearPreviewOnRuntimeStart)}");

            if (_settings != null && _settings.Volume.ClearPreviewOnRuntimeStart)
                _deferRuntimePreviewCleanupUntilStart = !TryClearGeneratedFogOutput();
        }

        [Inject]
        private void Construct(
            [InjectOptional] IFogVolumeRuntimeUpdater runtimeUpdater,
            [InjectOptional] IFogVolumePreviewBuilder previewBuilder = null,
            [InjectOptional] IFogVolumeSceneContextBuilder sceneContextBuilder = null,
            [InjectOptional] IFogVolumeOutputCleaner outputCleaner = null,
            [InjectOptional] IFogVolumeValidationService validationService = null)
        {
            _runtimeUpdater = runtimeUpdater;
            _previewBuilder = previewBuilder;
            _sceneContextBuilder = sceneContextBuilder;
            _outputCleaner = outputCleaner;
            _validationService = validationService;
            Debug.Log($"{StartDiagTag} VolumeController.Construct runtimeUpdater={(runtimeUpdater != null ? runtimeUpdater.GetType().Name : "null")}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}, settings={(_settings != null ? _settings.name : "null")}.");
            LogLifecycleOnce(ref _loggedConstruct, "Construct", $"runtimeUpdater={(runtimeUpdater != null ? runtimeUpdater.GetType().Name : "null")}, previewBuilder={(previewBuilder != null ? previewBuilder.GetType().Name : "null")}, sceneContextBuilder={(sceneContextBuilder != null ? sceneContextBuilder.GetType().Name : "null")}, outputCleaner={(outputCleaner != null ? outputCleaner.GetType().Name : "null")}, validationService={(validationService != null ? validationService.GetType().Name : "null")}, settings={(_settings != null ? _settings.name : "null")}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}");
            RegisterWithUpdater();
        }

        private void OnEnable()
        {
            RegisterWithUpdater();
        }

        private void Start()
        {
            if (_deferRuntimePreviewCleanupUntilStart)
            {
                TryClearGeneratedFogOutput();
                _deferRuntimePreviewCleanupUntilStart = false;
            }

            RegisterWithUpdater();
            RequestStartupBuildIfGameplayFogIsNotReady();
        }

        private void OnDisable()
        {
            _runtimeUpdater?.DetachController(this);
        }

        private void OnValidate()
        {
            _rebuildIntervalSeconds = Mathf.Max(0.02f, _rebuildIntervalSeconds);
            _cellSizeOverride = Mathf.Max(0.001f, _cellSizeOverride);
            _additionalTopClearance = Mathf.Max(0f, _additionalTopClearance);
        }

        [TitleGroup("Preview")]
        [Button("Build Preview From Scene Grid")]
        [DisableInPlayMode]
        private void BuildPreviewFromSceneGrid()
        {
            if (_settings == null || ResolveFogManager() == null)
                return;

            if (!TryClearGeneratedFogOutput())
                return;

            var previewBuilder = ResolvePreviewBuilder();
            var sceneContextBuilder = ResolveSceneContextBuilder();
            if (previewBuilder == null || sceneContextBuilder == null)
                return;

            previewBuilder.BuildPreview(this, sceneContextBuilder.BuildContext(this));
        }

        [TitleGroup("Preview")]
        [Button("Clear Preview")]
        private void ClearGeneratedFogOutput()
        {
            TryClearGeneratedFogOutput();
        }

        [TitleGroup("Runtime Actions")]
        [Button("Rebuild Fog Volume")]
        [EnableIf(nameof(CanRequestRuntimeRebuild))]
        private void RebuildFogVolume()
        {
            _runtimeUpdater?.RequestFullRebuildFromController(this);
        }

        /// <summary>
        /// Визначає cell size, який використовуватиме volume build path.
        /// Scene override має пріоритет над settings і world cell size.
        /// </summary>
        /// <param name="worldCellSize">Cell size, отриманий із generated світу.</param>
        /// <returns>Ефективний cell size для fog volume.</returns>
        public float ResolveCellSize(float worldCellSize)
        {
            if (_overrideCellSize)
                return Mathf.Max(0.001f, _cellSizeOverride);

            if (_settings != null && !_settings.Volume.UseWorldCellSize)
                return Mathf.Max(0.001f, _settings.Volume.CellSizeOverride);

            return worldCellSize > 0.0001f ? worldCellSize : 1f;
        }

        private TileWorldCreatorManager ResolveFogManager()
        {
            if (_tileWorldCreatorManager == null)
                _tileWorldCreatorManager = GetComponent<TileWorldCreatorManager>();

            return _tileWorldCreatorManager;
        }

        private void RegisterWithUpdater()
        {
            if (!isActiveAndEnabled)
                return;

            if (_runtimeUpdater == null)
            {
                Debug.LogWarning($"{StartDiagTag} VolumeController.RegisterWithUpdater failed runtimeUpdater=null, object={name}, active={gameObject.activeInHierarchy}, enabled={enabled}.");
                LogLifecycleOnce(ref _loggedRegisterWithoutUpdater, "RegisterWithUpdater skipped", "runtimeUpdater=null");
                return;
            }

            Debug.Log($"{StartDiagTag} VolumeController.RegisterWithUpdater success object={name}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}, settings={(_settings != null ? _settings.name : "null")}.");
            _runtimeUpdater.AttachController(this);
        }

        private void RequestStartupBuildIfGameplayFogIsNotReady()
        {
            if (_settings == null || _runtimeUpdater == null)
                return;

            var sceneContextBuilder = ResolveSceneContextBuilder();
            if (sceneContextBuilder == null)
                return;

            var context = sceneContextBuilder.BuildContext(this);
            if (_revealStartupFallbackArea && (_logBuildSummary || _logValidationWarnings))
            {
                Debug.Log(
                    "[FogOfWarVolumeController] Runtime startup fallback reveal is deferred. " +
                    "The fog volume builds immediately, but the visible startup area is now expected to come from bootstrap/world spawn logic so gameplay fog, camera focus, and construction rules stay synchronized.",
                    this);
            }

            Debug.Log($"{StartDiagTag} VolumeController.RequestStartupBuildFromController object={name}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}, context={context.Width}x{context.Height}, cell={context.CellSize:0.###}, bounds={(context.HasMapWorldBounds ? context.MapWorldBounds.ToString() : "none")}.");
            _runtimeUpdater.RequestStartupBuildFromController(this, context);
        }

        void IFogVolumePreviewHost.AttachPreviewUpdater(IFogVolumeRuntimeUpdater updater)
        {
            if (updater == null)
                return;

            Debug.Log($"{StartDiagTag} VolumeController.AttachPreviewUpdater object={name}, updater={updater.GetType().Name}.");
            updater.AttachController(this);
        }

        private bool TryClearGeneratedFogOutput()
        {
            var outputCleaner = ResolveOutputCleaner();
            if (outputCleaner == null)
                return false;

            outputCleaner.ClearGeneratedOutput(ResolveFogManager());
            return true;
        }

        private IFogVolumePreviewBuilder ResolvePreviewBuilder()
        {
            if (_previewBuilder != null)
                return _previewBuilder;

#if UNITY_EDITOR
            // Editor-only fallback so Odin preview buttons work without a Zenject runtime context.
            if (!Application.isPlaying)
                return _previewBuilder = new FogVolumePreviewBuilder();
#endif

            return null;
        }

        private IFogVolumeSceneContextBuilder ResolveSceneContextBuilder()
        {
            if (_sceneContextBuilder != null)
                return _sceneContextBuilder;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return _sceneContextBuilder = new FogVolumeSceneContextBuilder();
#endif

            return null;
        }

        private IFogVolumeOutputCleaner ResolveOutputCleaner()
        {
            if (_outputCleaner != null)
                return _outputCleaner;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return _outputCleaner = new FogVolumeOutputCleaner();
#endif

            return null;
        }

        private IFogVolumeValidationService ResolveValidationService()
        {
            if (_validationService != null)
                return _validationService;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                return _validationService = new FogVolumeValidationService();
#endif

            return null;
        }

        private void LogLifecycleOnce(ref bool logged, string stage, string details)
        {
            if (logged || !(_logBuildSummary || _logValidationWarnings))
                return;

            logged = true;
            Debug.Log($"[FogOfWarVolumeController] {stage}: object='{name}', active={gameObject.activeInHierarchy}, enabled={enabled}, {details}.", this);
        }

        private bool HasSettings(FogOfWarSettings settings)
            => settings != null;

    }
}
