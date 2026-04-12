using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Interactions.Runtime
{
    internal sealed class WorldInfoSelectionCoordinator : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IUnitService _unitService;

        private WorldInfoSelectionKind _selectedKind;
        private string _selectedObjectId;
        private Vector2Int _selectedPosition;

        public WorldInfoSelectionCoordinator(SignalBus signalBus, IUnitService unitService)
        {
            _signalBus = signalBus;
            _unitService = unitService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingInfoPanelRequestedSignal>(OnBuildingInfoRequested);
            _signalBus.Subscribe<UnitInfoPanelRequestedSignal>(OnUnitInfoRequested);
            _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnWorldInfoPanelClosed);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingInfoPanelRequestedSignal>(OnBuildingInfoRequested);
            _signalBus.TryUnsubscribe<UnitInfoPanelRequestedSignal>(OnUnitInfoRequested);
            _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnWorldInfoPanelClosed);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
        }

        private void OnBuildingInfoRequested(BuildingInfoPanelRequestedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.BuildingId))
                return;

            UpdateSelection(WorldInfoSelectionKind.Building, signal.BuildingId, signal.Position, emitWhenPositionChanged: true);
        }

        private void OnUnitInfoRequested(UnitInfoPanelRequestedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            UpdateSelection(WorldInfoSelectionKind.Unit, signal.UnitId, signal.Position, emitWhenPositionChanged: false);
        }

        private void OnWorldInfoPanelClosed(WorldInfoPanelClosedSignal _)
        {
            if (_selectedKind == WorldInfoSelectionKind.None && string.IsNullOrWhiteSpace(_selectedObjectId))
                return;

            _selectedKind = WorldInfoSelectionKind.None;
            _selectedObjectId = null;
            _selectedPosition = default;

            _signalBus.Fire(new WorldInfoSelectionChangedSignal
            {
                Kind = WorldInfoSelectionKind.None,
                ObjectId = null,
                Position = default,
            });
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (_selectedKind != WorldInfoSelectionKind.Unit)
                return;

            if (!string.Equals(_selectedObjectId, signal.UnitId, StringComparison.Ordinal))
                return;

            if (_unitService.TryGetUnitPosition(signal.UnitId, out var position))
            {
                _selectedPosition = position;
                _signalBus.Fire(new UnitInfoPanelRequestedSignal
                {
                    UnitId = signal.UnitId,
                    Position = position,
                });
            }
        }

        private void OnEconomyTickCompleted(EconomyTickCompletedSignal _)
        {
            RefreshSelection();
        }

        private void OnSettlementResourceChanged(SettlementResourceChangedSignal _)
        {
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            switch (_selectedKind)
            {
                case WorldInfoSelectionKind.Building when !string.IsNullOrWhiteSpace(_selectedObjectId):
                    _signalBus.Fire(new BuildingInfoPanelRequestedSignal
                    {
                        BuildingId = _selectedObjectId,
                        Position = _selectedPosition,
                    });
                    break;

                case WorldInfoSelectionKind.Unit when !string.IsNullOrWhiteSpace(_selectedObjectId):
                    if (_unitService.TryGetUnitPosition(_selectedObjectId, out var position))
                        _selectedPosition = position;

                    _signalBus.Fire(new UnitInfoPanelRequestedSignal
                    {
                        UnitId = _selectedObjectId,
                        Position = _selectedPosition,
                    });
                    break;
            }
        }

        private void UpdateSelection(
            WorldInfoSelectionKind kind,
            string objectId,
            Vector2Int position,
            bool emitWhenPositionChanged)
        {
            bool kindChanged = _selectedKind != kind;
            bool objectChanged = !string.Equals(_selectedObjectId, objectId, StringComparison.Ordinal);
            bool positionChanged = _selectedPosition != position;

            _selectedKind = kind;
            _selectedObjectId = objectId;
            _selectedPosition = position;

            if (!kindChanged && !objectChanged && (!emitWhenPositionChanged || !positionChanged))
                return;

            _signalBus.Fire(new WorldInfoSelectionChangedSignal
            {
                Kind = kind,
                ObjectId = objectId,
                Position = position,
            });
        }
    }
}