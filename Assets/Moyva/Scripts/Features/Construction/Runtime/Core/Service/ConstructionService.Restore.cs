using System;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void RestoreFromSave(Vector2Int position, string buildingId)
        {
            if (_objectsMapService.IsOccupied(position))
            {
                Debug.LogWarning($"[Construction] RestoreFromSave: позиція {position} вже зайнята, пропускаємо '{buildingId}'.");
                return;
            }

            _objectsMapService.Register(position, buildingId);
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

            if (_objectsMapService.IsOccupied(position))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}): тайл зайнятий.");
                return false;
            }

            if (IsBlockedByInfluenceZone(position, buildingId, ignoredPendingPosition: null))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}) -> BLOCKED. influenceZoneBlocked=True, townHallBuildRadius={_townHallBuildRadius}.");
                return false;
            }

            if (IsBlockedByTerrain(position, out var terrainReason))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}) -> BLOCKED. terrainBlocked=True, reason={terrainReason}.");
                return false;
            }

            if (!TryConsumeConstructionResources(position, buildingId, ownerId, out var resourceReason))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}) -> BLOCKED. resourcesBlocked=True, reason={resourceReason}.");
                return false;
            }

            _objectsMapService.Register(position, buildingId);
            _factionPlacedBuildings[position] = (buildingId, ownerId);
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
            if (!_factionPlacedBuildings.TryGetValue(position, out var entry) || entry.FactionId != factionId)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishByFaction({position}): будівля не знайдена або не належить фракції '{factionId}'.");
                return false;
            }

            _objectsMapService.Unregister(position);
            _factionPlacedBuildings.Remove(position);
            _signalBus.Fire(new BuildingDemolishedSignal
            {
                BuildingId = entry.BuildingId,
                Position = position,
                SourceFactionId = factionId
            });
            if (VerboseLogs)
                Debug.Log($"[Construction] TryDemolishByFaction: знесено '{entry.BuildingId}' на {position} від '{factionId}'.");
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
