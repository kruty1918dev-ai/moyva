using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public sealed partial class FogOfWarVolumeController
    {
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

        void IFogVolumePreviewHost.AttachPreviewUpdater(IFogVolumeRuntimeUpdater updater)
        {
            if (updater == null)
                return;

            Debug.Log($"{StartDiagTag} VolumeController.AttachPreviewUpdater object={name}, updater={updater.GetType().Name}.");
            updater.AttachController(this);
        }

        private void LogLifecycleOnce(ref bool logged, string stage, string details)
        {
            if (logged || !(_logBuildSummary || _logValidationWarnings))
                return;

            logged = true;
            Debug.Log($"[FogOfWarVolumeController] {stage}: object='{name}', active={gameObject.activeInHierarchy}, enabled={enabled}, {details}.", this);
        }
    }
}
