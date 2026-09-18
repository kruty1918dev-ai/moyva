using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public IReadOnlyList<ConstructionSavedPlacement>
            GetSavedPlacements()
        {
            var result =
                new List<ConstructionSavedPlacement>(
                    _playerPlacedBuildings.Count
                    + _factionPlacedBuildings.Count);

            // Owner-indexed placements are canonical. Legacy player placements
            // are appended only when no canonical record exists at the origin.
            foreach (var pair in _factionPlacedBuildings)
            {
                result.Add(
                    new ConstructionSavedPlacement(
                        pair.Key,
                        pair.Value.BuildingId,
                        NormalizeOwnerId(pair.Value.FactionId),
                        ResolvePlacedRotation(pair.Key)));
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (_factionPlacedBuildings.ContainsKey(pair.Key))
                    continue;

                result.Add(
                    new ConstructionSavedPlacement(
                        pair.Key,
                        pair.Value,
                        NormalizeOwnerId(_activeOwnerId),
                        ResolvePlacedRotation(pair.Key)));
            }

            result.Sort(
                (left, right) =>
                {
                    int byX =
                        left.Position.x.CompareTo(
                            right.Position.x);
                    if (byX != 0)
                        return byX;

                    int byY =
                        left.Position.y.CompareTo(
                            right.Position.y);
                    if (byY != 0)
                        return byY;

                    return string.CompareOrdinal(
                        left.BuildingId,
                        right.BuildingId);
                });

            return result;
        }

        public IReadOnlyList<ConstructionSavedPlacement> GetOwnerPlacements(string ownerId)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            IReadOnlyList<ConstructionSavedPlacement> all = GetSavedPlacements();
            var result = new List<ConstructionSavedPlacement>();
            for (int index = 0; index < all.Count; index++)
            {
                ConstructionSavedPlacement placement = all[index];
                if (string.Equals(
                        NormalizeOwnerId(placement.OwnerId),
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    result.Add(placement);
                }
            }
            return result;
        }

        public void RestoreFromSave(
            Vector2Int position,
            string buildingId)
            => RestoreFromSave(
                position,
                buildingId,
                _activeOwnerId);

        public void RestoreFromSave(
            Vector2Int position,
            string buildingId,
            string ownerId,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                throw new ArgumentException("Cannot restore a building without an ID.", nameof(buildingId));

            string normalizedOwner = NormalizeOwnerId(ownerId);
            if (_factionPlacedBuildings.TryGetValue(position, out var existing)
                && existing.BuildingId == buildingId && existing.FactionId == normalizedOwner
                && ResolvePlacedRotation(position) == rotation)
                return;

            if (!_footprints.TryRegister(
                    position,
                    buildingId,
                    rotation))
            {
                throw new InvalidOperationException(
                    $"Cannot restore building '{buildingId}' for '{normalizedOwner}' at {position}: footprint is invalid, occupied, or the grid is not ready.");
            }
            _placedRotationByOrigin[position] = rotation;

            _playerPlacedBuildings.Remove(position);
            _factionPlacedBuildings[position] =
                (buildingId, normalizedOwner);

            _signalBus.Fire(
                new BuildingPlacedSignal
                {
                    BuildingId = buildingId,
                    Position = position,
                    OwnerId = normalizedOwner,
                    SourceFactionId = normalizedOwner,
                    RotationQuarterTurns = (int)rotation,
                });
            _buildingFogEffects.ApplyOnPlaced(
                buildingId,
                position,
                normalizedOwner);

        }

    }
}
