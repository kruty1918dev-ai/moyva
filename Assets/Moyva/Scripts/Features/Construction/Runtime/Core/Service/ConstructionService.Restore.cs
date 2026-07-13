using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void RestoreFromSave(Vector2Int position, string buildingId)
        {
            if (!TryRegisterBuildingFootprint(position, buildingId))
            {
                Debug.LogWarning($"[Construction] RestoreFromSave: footprint at {position} is occupied, skipping '{buildingId}'.");
                return;
            }

            _playerPlacedBuildings[position] = buildingId;
            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = _activeOwnerId,
            });
            ApplyBuildingFogReveal(buildingId, position);

            if (VerboseLogs)
                Debug.Log($"[Construction] RestoreFromSave: відновлено '{buildingId}' на {position}");
        }

        public bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning($"[Construction] TryDirectPlace({position}): buildingId порожній.");
                return false;
            }

            string ownerId = string.IsNullOrWhiteSpace(placedByFactionId)
                ? DefaultOwnerId
                : placedByFactionId.Trim();

            ConstructionPlacementQueryResult placement = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    includeResources: true,
                    includeDetails: true,
                    ownerId: ownerId));
            if (!placement.IsValid)
            {
                if (VerboseLogs)
                    Debug.Log($"[MoyvaBuildGridDiag] direct-placement-blocked building='{buildingId}' origin={position} reason='{placement.Reason}'");
                return false;
            }

            bool replacementRemoved = false;
            bool targetRegistered = false;
            bool modelCommitted = false;
            Vector2Int replacedOrigin = default;
            string replacedBuildingId = null;
            try
            {
                if (placement.IsGateReplacement)
                {
                    if (!TryResolveGateReplacement(
                            position,
                            buildingId,
                            out replacedOrigin,
                            out replacedBuildingId))
                    {
                        return false;
                    }

                    UnregisterBuildingFootprint(replacedOrigin, replacedBuildingId);
                    replacementRemoved = true;
                }

                if (!TryRegisterBuildingFootprint(position, buildingId))
                    return false;
                targetRegistered = true;

                if (!TryConsumeConstructionResources(position, buildingId, ownerId, out var resourceReason))
                {
                    if (VerboseLogs)
                        Debug.Log($"[MoyvaBuildGridDiag] direct-placement-blocked building='{buildingId}' origin={position} reason='{resourceReason}'");
                    return false;
                }

                if (replacementRemoved)
                    RemovePlacedRecordAt(replacedOrigin);

                _factionPlacedBuildings[position] = (buildingId, ownerId);
                modelCommitted = true;
            }
            finally
            {
                if (!modelCommitted)
                {
                    if (targetRegistered)
                        UnregisterBuildingFootprint(position, buildingId);
                    if (replacementRemoved)
                        RestoreBuildingFootprintOrLog(replacedOrigin, replacedBuildingId, "direct-gate-replacement");
                }
            }

            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = ownerId,
                SourceFactionId = ownerId,
            });
            ApplyBuildingFogReveal(buildingId, position);
            if (VerboseLogs)
                Debug.Log($"[Construction] TryDirectPlace: розміщено '{buildingId}' на {position} від '{ownerId}'.");
            return true;
        }

        public bool TryDemolishByFaction(Vector2Int position, string factionId)
        {
            Vector2Int origin = ResolvePlacedOrigin(position);
            if (!_factionPlacedBuildings.TryGetValue(origin, out var entry) || entry.FactionId != factionId)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishByFaction({position}): будівля не знайдена або не належить фракції '{factionId}'.");
                return false;
            }

            UnregisterBuildingFootprint(origin, entry.BuildingId);
            _factionPlacedBuildings.Remove(origin);
            _signalBus.Fire(new BuildingDemolishedSignal
            {
                BuildingId = entry.BuildingId,
                Position = origin,
                SourceFactionId = factionId
            });
            if (VerboseLogs)
                Debug.Log($"[Construction] TryDemolishByFaction: знесено '{entry.BuildingId}' на {origin} від '{factionId}'.");
            return true;
        }

        public bool HasPlacedBuilding(string buildingId, string ownerId = null)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            string normalizedOwner = string.IsNullOrWhiteSpace(ownerId) ? null : ownerId.Trim();

            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(pair.Value.BuildingId, buildingId, StringComparison.Ordinal))
                    continue;

                if (normalizedOwner == null || string.Equals(pair.Value.FactionId, normalizedOwner, StringComparison.Ordinal))
                    return true;
            }

            if (normalizedOwner == null || string.Equals(normalizedOwner, _activeOwnerId, StringComparison.Ordinal))
            {
                foreach (var pair in _playerPlacedBuildings)
                {
                    if (string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }
    }
}
