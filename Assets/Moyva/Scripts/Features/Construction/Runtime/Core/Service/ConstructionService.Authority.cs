using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService :
        IConfirmedConstructionDemolitionApplier
    {
        private bool TryAuthorizeConstructionMutation(
            string ownerId,
            string action,
            bool requireLocalOwner,
            out string normalizedOwnerId,
            out string reason)
        {
            var snapshot = new ConstructionTurnAuthoritySnapshot(
                _turns != null,
                _turns?.ActiveOwnerId,
                _turns?.LocalOwnerId,
                _turns?.Phase ?? TurnPhase.Initializing);

            if (!ConstructionTurnAuthorityPolicy.TryAuthorize(
                    ownerId,
                    snapshot,
                    requireLocalOwner,
                    out normalizedOwnerId,
                    out reason))
            {
                reason = $"{action}: {reason}";
                return false;
            }

            if (!_turns.CanOwnerAct(normalizedOwnerId, out reason))
            {
                reason = $"{action}: {reason}";
                return false;
            }

            return true;
        }

        private bool CanActiveOwnerMutate(
            string action,
            out string reason)
            => TryAuthorizeConstructionMutation(
                _activeOwnerId,
                action,
                requireLocalOwner: true,
                out _,
                out reason);

        private bool IsLocalConstructionOwner(string ownerId)
        {
            if (_turns == null || string.IsNullOrWhiteSpace(ownerId))
                return false;

            return string.Equals(
                _turns.LocalOwnerId?.Trim(),
                ownerId.Trim(),
                StringComparison.Ordinal);
        }

        private void RecordConstructionAction(
            string ownerId,
            string actionId)
        {
            if (_turns == null)
                return;

            string normalizedOwner = ownerId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedOwner)
                || string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            if (!_turns.TryRecordAction(normalizedOwner, actionId))
            {
            }
        }

        private bool TryResolveCommittedBuildingForOwner(
            Vector2Int position,
            string ownerId,
            out Vector2Int origin,
            out string buildingId,
            out string reason)
        {
            origin = _footprints.ResolveOrigin(position);
            buildingId = null;
            string normalizedOwner = ownerId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedOwner))
            {
                reason = "Building owner is empty.";
                return false;
            }

            if (_factionPlacedBuildings.TryGetValue(
                    origin,
                    out var factionEntry))
            {
                string entryOwner =
                    NormalizeOwnerId(factionEntry.FactionId);
                if (!string.Equals(
                        entryOwner,
                        normalizedOwner,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Building at {origin} belongs to '{entryOwner}', not '{normalizedOwner}'.";
                    return false;
                }

                buildingId = factionEntry.BuildingId;
                reason = null;
                return !string.IsNullOrWhiteSpace(buildingId);
            }

            // Compatibility only for sessions created before P06. New commits and
            // restores are indexed in _factionPlacedBuildings with a stable owner.
            if (_playerPlacedBuildings.TryGetValue(
                    origin,
                    out string legacyBuildingId))
            {
                string legacyOwner = NormalizeOwnerId(_activeOwnerId);
                if (!string.Equals(
                        legacyOwner,
                        normalizedOwner,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Legacy building at {origin} is not owned by '{normalizedOwner}'.";
                    return false;
                }

                buildingId = legacyBuildingId;
                reason = null;
                return !string.IsNullOrWhiteSpace(buildingId);
            }

            reason = $"No committed building exists at {origin}.";
            return false;
        }

        private bool TryApplyCommittedDemolition(
            Vector2Int position,
            string ownerId,
            bool recordTurnAction,
            bool idempotentWhenMissing,
            out string reason)
        {
            if (!TryResolveCommittedBuildingForOwner(
                    position,
                    ownerId,
                    out Vector2Int origin,
                    out string buildingId,
                    out reason))
            {
                if (idempotentWhenMissing
                    && reason != null
                    && reason.StartsWith(
                        "No committed building exists",
                        StringComparison.Ordinal))
                {
                    reason = null;
                    return true;
                }

                return false;
            }

            string normalizedOwner = ownerId.Trim();
            _footprints.Unregister(origin, buildingId);
            RemovePlacedRecordAt(origin);
            InvalidatePlacementAvailabilityCache();
            _buildingFogEffects.Remove(origin);

            _signalBus.Fire(new BuildingDemolishedSignal
            {
                BuildingId = buildingId,
                Position = origin,
                OwnerId = normalizedOwner,
                SourceFactionId = normalizedOwner,
            });

            if (recordTurnAction)
                RecordConstructionAction(normalizedOwner, "building-demolish");

            reason = null;
            return true;
        }

        public bool TryApplyConfirmedDemolition(
            Vector2Int position,
            string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return false;

            // Host-confirmed replication is a state-convergence path, not a local
            // gameplay command. It intentionally bypasses local turn authority.
            return TryApplyCommittedDemolition(
                position,
                ownerId.Trim(),
                recordTurnAction: false,
                idempotentWhenMissing: true,
                out _);
        }
    }
}
