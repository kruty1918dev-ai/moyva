using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        /// <summary>
        /// Підписується на ігрові сигнали необхідні для роботи служби
        /// (створення/рух/знищення одиниць, розміщення/демонтаж будівель, генерація світу).
        /// </summary>
        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<UnitGarrisonStateChangedSignal>(
                OnUnitGarrisonStateChanged);
            // Building placement fog is owned by ConstructionService via
            // ApplyBuildingFogReveal(). Keeping a second BuildingPlacedSignal
            // subscriber here caused the same building area to be registered
            // and visually flushed twice during one Confirm().
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<BuildingOwnershipTransferredSignal>(
                OnBuildingOwnershipTransferred);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
            ReplayCachedWorldGeneratedSignalIfAvailable();
        }

        /// <summary>
        /// Відписується від сигналів і очищує підписки.
        /// </summary>
        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<UnitGarrisonStateChangedSignal>(
                OnUnitGarrisonStateChanged);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<BuildingOwnershipTransferredSignal>(
                OnBuildingOwnershipTransferred);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
        }

        /// <summary>
        /// Ініціалізує службу з розмірами світу (ширина x висота).
        /// </summary>
        /// <param name="width">Ширина карти в клітинах (мінімум 1).</param>
        /// <param name="height">Висота карти в клітинах (мінімум 1).</param>
        public void Initialize(int width, int height)
        {
            bool wasInitialized = _initialized;
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            _width = width;
            _height = height;

            _stateGrid.Initialize(width, height);
            _unitVisibleTiles.Clear();
            _visualDirtyBuffer.Clear();

            _initialized = true;
            if (!_visualContext.IsValid)
                _visualContext = FogWorldVisualContextFactory.CreateFallback(width, height);
            else
                _visualContext = _visualContext.WithSize(width, height);
            ResetVisualHeightSampler();
            _visualUpdater?.Initialize(width, height, _visualContext);

            var snapshot = _pendingExploredSnapshot ?? _saveProvider?.LoadExploredData();
            bool hasLoadedSnapshot = snapshot != null;
            if (snapshot != null)
                LoadFromSnapshot(snapshot);
            _pendingExploredSnapshot = null;

            ApplyPendingRevealAreas("Initialize");
            ApplyStartupFallbackRevealIfNeeded(hasLoadedSnapshot);

            if (_pendingUnits.Count > 0)
            {
                foreach (var kvp in _pendingUnits)
                    RegisterVisionArea(kvp.Key, kvp.Value.Position, kvp.Value.VisionRange, kvp.Value.Shape, kvp.Value.Modifiers);

                _pendingUnits.Clear();
            }
            else
            {
                RecalculateAllVisibility();
            }

            InitializeOwnerStates();

            _visualUpdater?.RebuildFullVisual(this);
            LogStartupRevealFinalState("InitializeAfterFullVisualRebuild");
            BumpVersion();
        }

        /// <summary>
        /// Змінює внутрішні розміри карти. Зберігає поточний сніпшот досліджених клітин
        /// та застосовує його після ресайзу.
        /// </summary>
        private void ResizeToWorldDimensions(int width, int height)
        {
            var exploredSnapshot = GetExploredSnapshot();
            var ownerExploredSnapshots = CaptureOwnerExploredSnapshots();

            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _stateGrid.Initialize(_width, _height);
            _unitVisibleTiles.Clear();
            _visualDirtyBuffer.Clear();
            foreach (var pair in _ownerStates)
            {
                pair.Value.Grid.Initialize(_width, _height);
                pair.Value.VisibleTiles.Clear();
            }
            RestoreOwnerExploredSnapshots(ownerExploredSnapshots);
            _visualContext = _visualContext.IsValid
                ? _visualContext.WithSize(_width, _height)
                : FogWorldVisualContextFactory.CreateFallback(_width, _height);
            ResetVisualHeightSampler();
            _visualUpdater?.Initialize(_width, _height, _visualContext);

            if (exploredSnapshot != null)
                LoadFromSnapshot(exploredSnapshot);

            InitializeOwnerStates();
        }

        /// <summary>
        /// Інкрементує внутрішню версію стану туману.
        /// </summary>
        private void BumpVersion()
        {
            unchecked
            {
                Version++;
            }
        }
    }
}
