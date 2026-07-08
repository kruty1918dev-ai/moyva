using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TileWorldCreatorManager))]
    [HideMonoScript]
    /// <summary>
    /// Thin Unity/Odin host for volume-based FogOfWar presentation.
    /// Runtime behavior is split into partial files by responsibility.
    /// </summary>
    public sealed partial class FogOfWarVolumeController : MonoBehaviour, IFogVolumePreviewHost, IFogVolumeSceneContextHost, IFogVolumeValidationHost
    {
        private const string StartDiagTag = "[MoyvaFogStartDiag]";

        [TitleGroup("Settings")]
        [Required]
        [ValidateInput(nameof(HasSettings), "Assign FogOfWarSettings.")]
        [InlineEditor(Expanded = true)]
        [HideLabel]
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

        public FogOfWarSettings Settings => _settings;

        public TileWorldCreatorManager TileWorldCreatorManager => ResolveFogManager();

        public FogVolumeUpdateMode EffectiveUpdateMode => _overrideUpdateMode
            ? _updateMode
            : (_settings != null ? _settings.Volume.UpdateMode : FogVolumeUpdateMode.DebouncePerFrame);

        public float EffectiveRebuildIntervalSeconds => Mathf.Max(
            0.02f,
            _overrideUpdateMode
                ? _rebuildIntervalSeconds
                : (_settings != null ? _settings.Volume.RebuildIntervalSeconds : 0.1f));

        public float AdditionalTopClearance => Mathf.Max(0f, _additionalTopClearance);

        public bool LogBuildSummary => _logBuildSummary;

        public bool LogEveryVolumeUpdate => _logEveryVolumeUpdate;

        public bool LogValidationWarnings => _logValidationWarnings;

        private bool UsesIntervalUpdate => EffectiveUpdateMode == FogVolumeUpdateMode.Interval;

        private bool CanRequestRuntimeRebuild
            => Application.isPlaying
                && _runtimeUpdater != null
                && _settings != null
                && ResolveFogManager() != null;
    }
}
