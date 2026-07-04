using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Thin runtime facade for gameplay Fog of War state.
    /// Implementation details are split by responsibility into partial files.
    /// </summary>
    internal sealed partial class FogOfWarService : IFogOfWarService, IInitializable, IDisposable
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private const string StartupChainTag = "[MoyvaStartupChain]";
        private const string BuildingVisionAreaPrefix = "building:";
        private const string StartupFallbackRevealAreaId = "fog-service-startup-fallback-reveal";
        private const string DebugTag = "[MoyvaFogTrace]";
        private const string StartDiagTag = "[MoyvaFogStartDiag]";
        private const string StartupRevealDiagTag = "[MoyvaFogStartupReveal]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        private readonly IFogVisibilityResolver _resolver;
        private readonly IHeightAwareVisionService _heightVisionService;
        private readonly IFogVisualUpdater _visualUpdater;
        private readonly IFogSaveDataProvider _saveProvider;
        private readonly SignalBus _signalBus;
        private readonly IFogStartupDiagnostics _startupDiagnostics;
        private readonly IWorldGenerationSignalState _worldGenerationSignalState;
        private readonly FogOfWarSettings _settings;

        private int _defaultVisionRange = 5;
        private int _width;
        private int _height;
        private bool _initialized;

        private readonly FogStateGrid _stateGrid = new FogStateGrid();
        private bool[,] _pendingExploredSnapshot;

        private readonly Dictionary<string, IReadOnlyList<Vector2Int>> _unitVisibleTiles
            = new Dictionary<string, IReadOnlyList<Vector2Int>>();

        private readonly Dictionary<string, int> _unitVisionRange
            = new Dictionary<string, int>();

        private readonly Dictionary<string, Vector2Int> _unitPositions
            = new Dictionary<string, Vector2Int>();

        private readonly Dictionary<string, FogVisionModifiers> _unitVisionModifiers
            = new Dictionary<string, FogVisionModifiers>();

        private readonly Dictionary<string, FogRevealShape> _fixedVisionShapes
            = new Dictionary<string, FogRevealShape>();

        private readonly Dictionary<string, (Vector2Int Position, int VisionRange, FogRevealShape? Shape, FogVisionModifiers Modifiers)> _pendingUnits
            = new Dictionary<string, (Vector2Int Position, int VisionRange, FogRevealShape? Shape, FogVisionModifiers Modifiers)>();

        private readonly List<FogPendingRevealArea> _pendingRevealAreas = new List<FogPendingRevealArea>();
        private readonly FogVisualDirtyBuffer _visualDirtyBuffer = new FogVisualDirtyBuffer();

        private FogWorldVisualContext _visualContext;
        private FogVolumeHeightSampler _visualHeightSampler;
        private bool _hasVisualHeightSampler;
        private IDiagnosticFlow _startupRevealFlow;
        private int _lastHandledWorldRevision;

        /// <summary>
        /// Внутрішня версія fog state, яка збільшується після значущих змін.
        /// Використовується renderer culling та іншими runtime helpers для cheap dirty-check.
        /// </summary>
        internal int Version { get; private set; }

        /// <summary>
        /// Показує, чи fog service уже ініціалізовано під поточний розмір карти.
        /// </summary>
        internal bool IsReady => _initialized;

        /// <summary>
        /// Створює головний runtime fog service і підключає його залежності.
        /// </summary>
        public FogOfWarService(
            IFogVisibilityResolver resolver,
            IHeightAwareVisionService heightVisionService,
            IFogVisualUpdater visualUpdater,
            IFogSaveDataProvider saveProvider,
            SignalBus signalBus,
            [InjectOptional] FogOfWarSettings settings,
            [InjectOptional] IFogStartupDiagnostics startupDiagnostics = null,
            [InjectOptional] IWorldGenerationSignalState worldGenerationSignalState = null)
        {
            _resolver = resolver;
            _heightVisionService = heightVisionService;
            _visualUpdater = visualUpdater;
            _saveProvider = saveProvider;
            _signalBus = signalBus;
            _settings = settings;
            _startupDiagnostics = startupDiagnostics;
            _worldGenerationSignalState = worldGenerationSignalState;

            if (_settings != null)
                _defaultVisionRange = _settings.DefaultVisionRange;
            else
                Debug.LogWarning("[FogOfWar] FogOfWarService: FogOfWarSettings is null. Using DefaultVisionRange=5.");
        }
    }
}
