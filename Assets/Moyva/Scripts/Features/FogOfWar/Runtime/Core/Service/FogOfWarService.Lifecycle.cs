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
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGeneratedData);
            ReplayCachedWorldGeneratedSignalIfAvailable();
            Debug.Log($"{WorldGenDiagTag} Receiver.Fog.Initialize subscribed frame={Time.frameCount}");
        }

        /// <summary>
        /// Відписується від сигналів і очищує підписки.
        /// </summary>
        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
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

            Debug.Log($"{StartDiagTag} FogService.Initialize map={width}x{height}, wasInitialized={wasInitialized}, pendingRevealCount={_pendingRevealAreas.Count}, pendingUnits={_pendingUnits.Count}, fixedAreas={_fixedVisionShapes.Count}.");
            Debug.Log($"{DebugTag} FogService.Initialize begin requested={width}x{height}, wasInitialized={wasInitialized}, previous={_width}x{_height}, pendingReveals={_pendingRevealAreas.Count}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}.");

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
            Debug.Log($"{StartDiagTag} FogService.Initialize snapshot={(snapshot != null ? $"{snapshot.GetLength(0)}x{snapshot.GetLength(1)}" : "null")}, willApplyPendingReveals={_pendingRevealAreas.Count > 0}.");
            Debug.Log($"{DebugTag} FogService.Initialize snapshot={(snapshot != null ? $"{snapshot.GetLength(0)}x{snapshot.GetLength(1)}" : "null")}.");
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

            _visualUpdater?.RebuildFullVisual(this);
            LogStartupRevealFinalState("InitializeAfterFullVisualRebuild");
            BumpVersion();
            Debug.Log($"{DebugTag} FogService.Initialize end map={_width}x{_height}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, pendingReveals={_pendingRevealAreas.Count}, version={Version}.");
        }

        /// <summary>
        /// Змінює внутрішні розміри карти. Зберігає поточний сніпшот досліджених клітин
        /// та застосовує його після ресайзу.
        /// </summary>
        private void ResizeToWorldDimensions(int width, int height)
        {
            var exploredSnapshot = GetExploredSnapshot();
            Debug.Log($"{DebugTag} FogService.ResizeToWorldDimensions from={_width}x{_height} to={Mathf.Max(1, width)}x{Mathf.Max(1, height)}, snapshot={(exploredSnapshot != null ? $"{exploredSnapshot.GetLength(0)}x{exploredSnapshot.GetLength(1)}" : "null")}.");

            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _stateGrid.Initialize(_width, _height);
            _unitVisibleTiles.Clear();
            _visualDirtyBuffer.Clear();
            _visualContext = _visualContext.IsValid
                ? _visualContext.WithSize(_width, _height)
                : FogWorldVisualContextFactory.CreateFallback(_width, _height);
            ResetVisualHeightSampler();
            _visualUpdater?.Initialize(_width, _height, _visualContext);

            if (exploredSnapshot != null)
                LoadFromSnapshot(exploredSnapshot);
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
