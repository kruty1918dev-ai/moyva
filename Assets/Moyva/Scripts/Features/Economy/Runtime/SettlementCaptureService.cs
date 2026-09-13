using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Combat.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class SettlementCaptureService : ISettlementCaptureService
    {
        private readonly ISettlementRegistry _settlements;
        private readonly IConstructionOwnershipTransfer _construction;
        private readonly SignalBus _signals;

        [InjectOptional] private IUnitService _units;
        [InjectOptional] private IUnitOwnershipQuery _owners;
        [InjectOptional] private ITurnService _turns;
        [InjectOptional] private ITurnAuthorityPolicy _authority;
        [InjectOptional] private IFogOwnerStateReader _fog;
        [InjectOptional] private IConstructionBuildingCombatTargetQuery _targets;
        [InjectOptional] private IBuildingRegistry _buildings;
        [InjectOptional] private IHealthRegistry _health;

        public string UnavailableReason => _units == null || _owners == null || _turns == null || _fog == null
            || _targets == null || _buildings == null || _health == null || _settlements == null || _construction == null
            || _authority == null ? "Capture gameplay services are unavailable." : null;

        public bool TryEvaluateCapture(string ownerId, string unitId, string targetEntityId,
            Vector2Int targetPosition, out ConstructionBuildingCombatTarget target, out string reason)
        {
            target = default;
            reason = UnavailableReason;
            if (reason != null) return false;
            if (!_turns.CanOwnerAct(ownerId, out reason)) return false;
            reason = "Only an existing owned unit can capture a settlement.";
            if (string.IsNullOrWhiteSpace(unitId) || _owners.GetUnitOwnerId(unitId) != ownerId
                || !_units.TryGetUnitPosition(unitId, out var position)) return false;
            // Check visibility before resolving any enemy building or settlement state.
            reason = "Target is outside your current vision.";
            if (!_fog.IsVisible(ownerId, targetPosition)) return false;
            reason = "Target building is not available for capture.";
            if (!_targets.TryGetCombatTarget(targetEntityId, out target) || target.Position != targetPosition) return false;
            reason = "You already control this settlement.";
            if (target.OwnerId == ownerId) return false;
            var definition = _buildings.GetById(target.BuildingId);
            reason = "Only castles and town halls can be captured.";
            if (!BuildingDefinitionCapabilities.IsCastle(definition) && !BuildingDefinitionCapabilities.IsTownHall(definition)) return false;
            reason = "Unit must be adjacent to the settlement center.";
            if (Math.Max(Math.Abs(position.x - targetPosition.x), Math.Abs(position.y - targetPosition.y)) > 1) return false;
            reason = "Settlement defenses are unavailable.";
            if (!_health.TryGet(target.EntityId, out var health) || health.IsDestroyed) return false;
            int threshold = Math.Max(1, Mathf.CeilToInt(health.MaxHp * 0.25f));
            reason = $"Reduce defenses to {threshold} HP or less before capture.";
            if (health.CurrentHp > threshold) return false;
            reason = "Settlement is missing, inactive, or its ownership has changed.";
            if (!_settlements.TryGetSettlementByPosition(targetPosition, out var state)
                || state == null || !state.IsActive || state.OwnerId != target.OwnerId) return false;
            reason = null;
            return true;
        }

        public SettlementCaptureResult CaptureWithUnit(string ownerId, string unitId,
            string targetEntityId, Vector2Int targetPosition)
        {
            if (_authority == null || !_authority.IsAuthoritative)
                return SettlementCaptureResult.Rejected(string.Empty, "Capture requires gameplay authority.");
            if (!TryEvaluateCapture(ownerId, unitId, targetEntityId, targetPosition, out var target, out var reason))
                return SettlementCaptureResult.Rejected(string.Empty, reason);
            return CaptureSettlementAtPosition(target.Position, target.OwnerId, ownerId, "captured-by-unit");
        }

        public SettlementCaptureService(
            ISettlementRegistry settlements,
            IConstructionOwnershipTransfer construction,
            SignalBus signals)
        {
            _settlements = settlements;
            _construction = construction;
            _signals = signals;
        }

        public SettlementCaptureResult CaptureSettlement(
            string settlementId,
            string previousOwnerId,
            string newOwnerId,
            string reason = null)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return SettlementCaptureResult.Rejected(string.Empty, "Settlement id is empty.");
            if (string.IsNullOrWhiteSpace(newOwnerId))
                return SettlementCaptureResult.Rejected(settlementId, "New owner is empty.");
            if (_settlements == null || _construction == null)
                return SettlementCaptureResult.Rejected(settlementId, "Settlement capture services are unavailable.");

            EconomySettlementState state = _settlements.GetSettlement(settlementId);
            if (state == null)
                return SettlementCaptureResult.Rejected(settlementId, "Settlement not found.");

            string previous = Normalize(previousOwnerId);
            if (string.IsNullOrWhiteSpace(previous))
                previous = Normalize(state.OwnerId);
            string next = Normalize(newOwnerId);
            var transferredPositions = new List<Vector2Int>();
            for (int index = 0; index < state.Buildings.Count; index++)
            {
                Vector2Int position = state.Buildings[index].GridPosition;
                if (!_construction.TryTransferPlacedBuildingOwner(position, previous, next, out string transferReason))
                {
                    RollBackTransferredBuildings(transferredPositions, next, previous);
                    return SettlementCaptureResult.Rejected(settlementId, transferReason);
                }

                transferredPositions.Add(position);
            }

            if (!_settlements.TryTransferSettlementOwner(
                    settlementId,
                    previous,
                    next,
                    out Vector2Int centerPosition,
                    out string settlementReason))
            {
                RollBackTransferredBuildings(transferredPositions, next, previous);
                return SettlementCaptureResult.Rejected(settlementId, settlementReason);
            }

            _signals?.Fire(new SettlementCapturedSignal
            {
                SettlementId = settlementId,
                PreviousOwnerId = previous,
                NewOwnerId = next,
                CenterPosition = centerPosition,
                Reason = string.IsNullOrWhiteSpace(reason) ? "captured" : reason.Trim(),
            });

            if (!OwnerHasActiveSettlement(previous))
            {
                _signals?.Fire(new FactionEliminatedSignal
                {
                    FactionId = previous,
                });
            }

            return new SettlementCaptureResult(true, settlementId, previous, next, null);
        }

        public SettlementCaptureResult CaptureSettlementAtPosition(
            Vector2Int centerOrBuildingPosition,
            string previousOwnerId,
            string newOwnerId,
            string reason = null)
        {
            if (_settlements == null)
                return SettlementCaptureResult.Rejected(string.Empty, "Settlement registry is unavailable.");
            if (!_settlements.TryGetSettlementByPosition(centerOrBuildingPosition, out EconomySettlementState state)
                || state == null)
            {
                return SettlementCaptureResult.Rejected(
                    string.Empty,
                    $"No settlement owns position {centerOrBuildingPosition.x},{centerOrBuildingPosition.y}.");
            }

            return CaptureSettlement(
                state.SettlementId,
                previousOwnerId,
                newOwnerId,
                reason);
        }

        private void RollBackTransferredBuildings(
            List<Vector2Int> positions,
            string currentOwner,
            string previousOwner)
        {
            for (int index = positions.Count - 1; index >= 0; index--)
                _construction.TryTransferPlacedBuildingOwner(
                    positions[index],
                    currentOwner,
                previousOwner,
                out _);
        }

        private bool OwnerHasActiveSettlement(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId)
                || _settlements?.AllSettlements == null)
            {
                return false;
            }

            foreach (var pair in _settlements.AllSettlements)
            {
                EconomySettlementState state = pair.Value;
                if (state != null
                    && state.IsActive
                    && string.Equals(Normalize(state.OwnerId), ownerId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
    }
}
