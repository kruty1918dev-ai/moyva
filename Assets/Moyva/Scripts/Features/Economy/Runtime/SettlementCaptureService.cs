using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class SettlementCaptureService : ISettlementCaptureService
    {
        private readonly ISettlementRegistry _settlements;
        private readonly IConstructionOwnershipTransfer _construction;
        private readonly SignalBus _signals;

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
