using System;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Heavyweight runtime engine for TWC dual-grid fog volume visuals.
    /// Owns caches, runtime configuration clone and build orchestration.
    /// </summary>
    internal sealed partial class FogVolumeVisualUpdateEngine : IFogVisualUpdater, IFogVolumeRuntimeUpdater, ITickable, IDisposable
    {
        private const string LogTag = "[FogOfWarVolume]";
        private const string StartDiagTag = "[MoyvaFogStartDiag]";
        private const string StartupChainTag = "[MoyvaStartupChain]";
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private const string ConstructionPerfTag =
            "[MoyvaConstructionPerf]";
        private const int LargeDirtyRequestThreshold = 64;
        private const string FolderName = "Fog Volume";
        private const int TargetFogClusterBudget = 32;
        private const int TargetFogHeightLayerBudget = 8;
        private const int MinimumFogClusterCellSize = 8;

        private readonly FogOfWarSettings _injectedSettings;
        private readonly IFogVolumePendingWorkState _pendingWorkState;
        private readonly IFogVolumePendingWorkRequests _pendingWorkRequests;
        private readonly IFogVolumePendingWorkMaintenance _pendingWorkMaintenance;
        private readonly IFogVisualUpdateScheduleState _visualUpdateScheduleState;
        private readonly IFogVisualUpdateTickGate _visualUpdateTickGate;
        private readonly IFogVisualUpdateRequestPolicy _visualUpdateRequestPolicy;
        private readonly HashSet<Vector2> _scratchCells = new HashSet<Vector2>();
        private readonly IFogVolumeStateCache _stateCache;
        private readonly IFogStartupFogServiceFactory _startupFogServiceFactory;
        private readonly Dictionary<int, float> _heightByKey = new Dictionary<int, float>();
        private readonly List<RuntimeLayer> _runtimeLayers = new List<RuntimeLayer>();
        private readonly IFogDirtyClusterTracker _dirtyClusterTracker;
        private readonly IFogClusteredVolumeRenderer _clusteredVolumeRenderer;
        private readonly IFogVolumeOutputCleaner _outputCleaner;

        private FogOfWarVolumeController _controller;
        private TileWorldCreatorManager _manager;
        private Configuration _previousManagerConfiguration;
        private Configuration _runtimeConfiguration;
        private FogWorldVisualContext _context;
        private bool _runtimeConfigurationDirty = true;
        private bool _hasBuiltAtLeastOnce;
        private bool _worldContextChangedSinceBuild;
        private bool _loggedFirstBuild;
        private int _mapWidth = 1;
        private int _mapHeight = 1;
        private bool _loggedMissingController;
        private bool _loggedMissingManager;
        private bool _loggedMissingSettings;
        private bool _loggedMissingFogService;
        private bool _loggedNoRuntimeLayers;
        private bool _loggedUnexploredPresetProblem;
        private bool _loggedExploredPresetProblem;
        private bool _loggedAttach;
        private bool _loggedInitialize;
        private bool _loggedWorldContext;
        private bool _loggedRebuildRequest;
        private bool _loggedDirtyUpdate;
        private bool _loggedTickWaitingForInterval;
        private float _cachedEffectiveHeightLayerSnap = -1f;

        /// <summary>
        /// Створює updater для volume visual path.
        /// У runtime зазвичай отримує settings через Zenject, а у preview може бути створений локально.
        /// </summary>
        /// <param name="settings">Fog settings для tuning і побудови runtime layers.</param>
        public FogVolumeVisualUpdateEngine(
            [InjectOptional] FogOfWarSettings settings = null,
            [InjectOptional] IFogVolumePendingWorkState pendingWorkState = null,
            [InjectOptional] IFogVolumePendingWorkRequests pendingWorkRequests = null,
            [InjectOptional] IFogVolumePendingWorkMaintenance pendingWorkMaintenance = null,
            [InjectOptional] IFogVisualUpdateScheduleState visualUpdateScheduleState = null,
            [InjectOptional] IFogVisualUpdateTickGate visualUpdateTickGate = null,
            [InjectOptional] IFogVisualUpdateRequestPolicy visualUpdateRequestPolicy = null,
            [InjectOptional] IFogVisualUpdateSchedulerFactory visualUpdateSchedulerFactory = null,
            [InjectOptional] IFogVolumeStateCache stateCache = null,
            [InjectOptional] IFogStartupFogServiceFactory startupFogServiceFactory = null,
            [InjectOptional] IFogDirtyClusterTracker dirtyClusterTracker = null,
            [InjectOptional] IFogClusteredVolumeRenderer clusteredVolumeRenderer = null,
            [InjectOptional] IFogVolumeOutputCleaner outputCleaner = null)
        {
            _injectedSettings = settings;
            var fallbackQueue = ResolvePendingWorkQueueFallback(pendingWorkState, pendingWorkRequests, pendingWorkMaintenance);
            _pendingWorkState = pendingWorkState ?? fallbackQueue;
            _pendingWorkRequests = pendingWorkRequests ?? fallbackQueue;
            _pendingWorkMaintenance = pendingWorkMaintenance ?? fallbackQueue;
            var fallbackScheduler = ResolveVisualUpdateSchedulerFallback(
                visualUpdateScheduleState,
                visualUpdateTickGate,
                visualUpdateRequestPolicy,
                visualUpdateSchedulerFactory);
            _visualUpdateScheduleState = visualUpdateScheduleState ?? fallbackScheduler;
            _visualUpdateTickGate = visualUpdateTickGate ?? fallbackScheduler;
            _visualUpdateRequestPolicy = visualUpdateRequestPolicy ?? fallbackScheduler;
            _stateCache = stateCache ?? new FogVolumeStateCache();
            _startupFogServiceFactory = startupFogServiceFactory ?? new FogStartupFogServiceFactory();
            _dirtyClusterTracker = dirtyClusterTracker;
            _clusteredVolumeRenderer = clusteredVolumeRenderer;
            _outputCleaner = outputCleaner ?? new FogVolumeOutputCleaner();
        }

        private static FogVolumePendingWorkQueue ResolvePendingWorkQueueFallback(
            IFogVolumePendingWorkState pendingWorkState,
            IFogVolumePendingWorkRequests pendingWorkRequests,
            IFogVolumePendingWorkMaintenance pendingWorkMaintenance)
        {
            return pendingWorkState as FogVolumePendingWorkQueue
                ?? pendingWorkRequests as FogVolumePendingWorkQueue
                ?? pendingWorkMaintenance as FogVolumePendingWorkQueue
                ?? new FogVolumePendingWorkQueue();
        }

        private FogVisualUpdateScheduler ResolveVisualUpdateSchedulerFallback(
            IFogVisualUpdateScheduleState visualUpdateScheduleState,
            IFogVisualUpdateTickGate visualUpdateTickGate,
            IFogVisualUpdateRequestPolicy visualUpdateRequestPolicy,
            IFogVisualUpdateSchedulerFactory visualUpdateSchedulerFactory)
        {
            return visualUpdateScheduleState as FogVisualUpdateScheduler
                ?? visualUpdateTickGate as FogVisualUpdateScheduler
                ?? visualUpdateRequestPolicy as FogVisualUpdateScheduler
                ?? (visualUpdateSchedulerFactory ?? new FogVisualUpdateSchedulerFactory())
                    .Create(ResolveUpdateMode, ResolveRebuildIntervalSeconds);
        }

        /// <summary>
        /// Діагностична кількість unexplored-клітинок у поточному кеші visual state.
        /// </summary>
        internal int DebugUnexploredCellCount => _stateCache.UnexploredCellCount;

        /// <summary>
        /// Діагностична кількість explored-клітинок у поточному кеші visual state.
        /// </summary>
        internal int DebugExploredCellCount => _stateCache.ExploredCellCount;

        /// <summary>
        /// Діагностичний доступ до runtime TWC configuration clone.
        /// </summary>
        internal Configuration DebugRuntimeConfiguration => _runtimeConfiguration;

    }
}
