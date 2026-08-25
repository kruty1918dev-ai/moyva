using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    public sealed partial class FogOfWarVolumeController
    {
        // Unity can invoke OnEnable before Zenject QueueForInject reaches Construct.
        // Do not diagnose a missing runtime updater until dependency injection has completed.
        private bool _runtimeInjectionCompleted;

        private void Awake()
        {
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
            _runtimeInjectionCompleted = true;
            RegisterWithUpdater();
        }

        private void OnEnable()
        {
            if (!_runtimeInjectionCompleted)
                return;

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
                return;

            _runtimeUpdater.AttachController(this);
        }

        internal void AttachPreviewUpdater(IFogVolumeRuntimeUpdater updater)
        {
            if (updater == null)
                return;
            updater.AttachController(this);
        }

    }
}
