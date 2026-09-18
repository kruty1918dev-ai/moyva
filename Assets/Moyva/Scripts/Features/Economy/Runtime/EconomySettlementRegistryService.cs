using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomySettlementRegistryService : ISettlementRegistry
    {
        private readonly Dictionary<string, EconomySettlementState> _settlements =
            new Dictionary<string, EconomySettlementState>(StringComparer.Ordinal);

        private readonly Dictionary<Vector2Int, string> _positionToSettlement =
            new Dictionary<Vector2Int, string>();

        private readonly Dictionary<Vector2Int, string> _positionToBuildingId =
            new Dictionary<Vector2Int, string>();

        private readonly Dictionary<Vector2Int, string> _positionToOwnerId =
            new Dictionary<Vector2Int, string>();

        public IReadOnlyDictionary<string, EconomySettlementState> AllSettlements => _settlements;

        public EconomySettlementState GetSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return null;

            _settlements.TryGetValue(settlementId, out var state);
            return state;
        }

        public bool TryGetSettlementByPosition(Vector2Int position, out EconomySettlementState state)
        {
            state = null;
            if (!_positionToSettlement.TryGetValue(position, out var settlementId))
                return false;

            if (!_settlements.TryGetValue(settlementId, out state))
                return false;

            return state != null;
        }

        public bool TryFindNearestSettlement(Vector2Int position, string ownerId, out EconomySettlementState state)
        {
            state = null;
            var settlementId = FindNearestSettlement(position, ownerId);
            if (string.IsNullOrWhiteSpace(settlementId))
                return false;

            if (!_settlements.TryGetValue(settlementId, out state) || state == null || !state.IsActive)
                return false;

            return string.Equals(NormalizeOwnerId(state.OwnerId), NormalizeOwnerId(ownerId), StringComparison.Ordinal);
        }

        public void RegisterSettlement(EconomySettlementState state, Vector2Int townHallPosition)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.SettlementId))
                return;

            _settlements[state.SettlementId] = state;
            _positionToSettlement[townHallPosition] = state.SettlementId;
        }

        public void UnregisterSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return;

            _settlements.Remove(settlementId);

            var positionsToRemove = new List<Vector2Int>();
            foreach (var kvp in _positionToSettlement)
            {
                if (kvp.Value == settlementId)
                    positionsToRemove.Add(kvp.Key);
            }

            foreach (var pos in positionsToRemove)
            {
                _positionToSettlement.Remove(pos);
                _positionToBuildingId.Remove(pos);
                _positionToOwnerId.Remove(pos);
            }
        }

        public void RegisterBuildingPosition(Vector2Int position, string settlementId, string buildingId, string ownerId)
        {
            _positionToSettlement[position] = settlementId;
            _positionToBuildingId[position] = buildingId;
            _positionToOwnerId[position] = NormalizeOwnerId(ownerId);
        }

        public void UnregisterBuildingPosition(Vector2Int position)
        {
            _positionToSettlement.Remove(position);
            _positionToBuildingId.Remove(position);
            _positionToOwnerId.Remove(position);
        }

        public bool TryTransferSettlementOwner(
            string settlementId,
            string previousOwnerId,
            string newOwnerId,
            out Vector2Int centerPosition,
            out string reason)
        {
            centerPosition = default;
            reason = null;
            if (string.IsNullOrWhiteSpace(settlementId)
                || !_settlements.TryGetValue(settlementId, out var state)
                || state == null)
            {
                reason = "Settlement not found.";
                return false;
            }

            string normalizedPrevious = NormalizeOwnerId(previousOwnerId);
            string normalizedNext = NormalizeOwnerId(newOwnerId);
            if (!state.IsActive)
            {
                reason = "Settlement is inactive.";
                return false;
            }

            if (!string.Equals(
                    NormalizeOwnerId(state.OwnerId),
                    normalizedPrevious,
                    StringComparison.Ordinal))
            {
                reason = $"Settlement belongs to '{state.OwnerId}', not '{normalizedPrevious}'.";
                return false;
            }

            if (string.Equals(normalizedPrevious, normalizedNext, StringComparison.Ordinal))
            {
                reason = "Settlement already belongs to this owner.";
                return false;
            }

            bool centerResolved = false;
            var mappedPositions = new List<Vector2Int>();
            foreach (var pair in _positionToSettlement)
            {
                if (!string.Equals(pair.Value, settlementId, StringComparison.Ordinal))
                    continue;
                mappedPositions.Add(pair.Key);
                if (!centerResolved)
                {
                    centerPosition = pair.Key;
                    centerResolved = true;
                }
            }

            state.OwnerId = normalizedNext;
            foreach (Vector2Int position in mappedPositions)
                _positionToOwnerId[position] = normalizedNext;
            return true;
        }

        public bool TryGetBuildingAtPosition(Vector2Int position, out string buildingId, out string ownerId)
        {
            buildingId = null;
            ownerId = null;

            if (!_positionToBuildingId.TryGetValue(position, out buildingId) || string.IsNullOrWhiteSpace(buildingId))
                return false;

            _positionToOwnerId.TryGetValue(position, out ownerId);
            ownerId = NormalizeOwnerId(ownerId);
            return true;
        }

        public string GetSettlementNameOrFallback(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return "Без поселення";

            if (!_settlements.TryGetValue(settlementId, out var state) || state == null)
                return settlementId;

            if (!string.IsNullOrWhiteSpace(state.SettlementName))
                return state.SettlementName;

            return state.SettlementId;
        }

        private string FindNearestSettlement(Vector2Int position, string ownerId)
        {
            string closest = null;
            float minDist = float.MaxValue;

            foreach (var kvp in _positionToSettlement)
            {
                if (!_settlements.TryGetValue(kvp.Value, out var state) || !state.IsActive)
                    continue;

                if (!string.Equals(NormalizeOwnerId(state.OwnerId), NormalizeOwnerId(ownerId), StringComparison.Ordinal))
                    continue;

                float dist = Vector2Int.Distance(kvp.Key, position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = kvp.Value;
                }
            }

            return closest;
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? EconomyManager.DefaultOwnerId : ownerId.Trim();
        }
    }
}
