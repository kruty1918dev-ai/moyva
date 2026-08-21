using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotWorldSnapshotBuilder : IBotWorldSnapshotBuilder
    {
        private readonly ITurnService _turns;
        private readonly IFactionRegistry _factions;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IConstructionService _construction;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IFogOfWarServiceRegistry _fogRegistry;
        private readonly IBotMemoryStore _memory;
        private readonly IBotPerceptionService _perception;

        [Inject]
        public BotWorldSnapshotBuilder(
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IFactionRegistry factions = null,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IConstructionService construction = null,
            [InjectOptional] IUnitRecruitmentService recruitment = null,
            [InjectOptional] IFogOfWarServiceRegistry fogRegistry = null,
            [InjectOptional] IBotMemoryStore memory = null,
            [InjectOptional] IBotPerceptionService perception = null)
        {
            _turns = turns;
            _factions = factions;
            _units = units;
            _ownership = ownership;
            _construction = construction;
            _recruitment = recruitment;
            _fogRegistry = fogRegistry;
            _memory = memory;
            _perception = perception;
        }

        public BotWorldSnapshot Build(string ownerId, long globalTurn)
        {
            string owner = Normalize(ownerId) ?? string.Empty;
            Vector2Int startPosition = ResolveStartPosition(owner);

            var ownUnits = new List<BotUnitSnapshot>();
            var enemyUnitCandidates = new List<BotUnitSnapshot>();
            CollectUnitCandidates(owner, ownUnits, enemyUnitCandidates);

            var ownBuildings = new List<BotBuildingSnapshot>();
            var enemyBuildingCandidates = new List<BotBuildingSnapshot>();
            CollectBuildingCandidates(owner, ownBuildings, enemyBuildingCandidates);

            _perception?.Refresh(
                owner,
                startPosition,
                ownUnits,
                ownBuildings);

            var visibleEnemyUnits = new List<BotUnitSnapshot>();
            for (int i = 0; i < enemyUnitCandidates.Count; i++)
            {
                BotUnitSnapshot candidate = enemyUnitCandidates[i];
                if (IsVisible(owner, candidate.Position))
                    visibleEnemyUnits.Add(candidate);
            }

            var visibleEnemyBuildings = new List<BotBuildingSnapshot>();
            for (int i = 0; i < enemyBuildingCandidates.Count; i++)
            {
                BotBuildingSnapshot candidate = enemyBuildingCandidates[i];
                if (IsVisible(owner, candidate.Position))
                    visibleEnemyBuildings.Add(candidate);
            }

            SortUnits(ownUnits);
            SortUnits(visibleEnemyUnits);
            SortBuildings(ownBuildings);
            SortBuildings(visibleEnemyBuildings);

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> ready =
                _recruitment?.GetReadyItems(owner)
                ?? Array.Empty<UnitRecruitmentQueueItemSnapshot>();

            IReadOnlyList<BotKnownEntityMemory> memory =
                _memory?.GetMemory(owner, globalTurn)
                ?? Array.Empty<BotKnownEntityMemory>();

            return new BotWorldSnapshot(
                owner,
                _turns?.Round ?? 0,
                globalTurn,
                _turns?.Phase ?? TurnPhase.Initializing,
                _turns?.ActionsThisTurn ?? 0,
                startPosition,
                ownUnits,
                visibleEnemyUnits,
                ownBuildings,
                ready,
                memory,
                visibleEnemyBuildings);
        }

        private void CollectUnitCandidates(
            string ownerId,
            List<BotUnitSnapshot> ownUnits,
            List<BotUnitSnapshot> enemyCandidates)
        {
            if (_units == null || _ownership == null)
                return;

            IReadOnlyCollection<string> ids = _units.GetAllUnitIds();
            if (ids == null)
                return;

            var sorted = new List<string>(ids);
            sorted.Sort(StringComparer.Ordinal);

            for (int index = 0; index < sorted.Count; index++)
            {
                string unitId = sorted[index];
                if (!_units.TryGetUnitPosition(unitId, out Vector2Int position))
                    continue;

                string unitOwner = Normalize(_ownership.GetUnitOwnerId(unitId));
                string typeId = Normalize(_units.GetUnitTypeId(unitId));
                float stamina = _units.GetStamina(unitId);

                var unit = new BotUnitSnapshot(
                    unitId,
                    unitOwner,
                    typeId,
                    position,
                    stamina);

                if (string.Equals(unitOwner, ownerId, StringComparison.Ordinal))
                {
                    ownUnits.Add(unit);
                    continue;
                }

                if (unitOwner != null)
                    enemyCandidates.Add(unit);
            }
        }

        private void CollectBuildingCandidates(
            string ownerId,
            List<BotBuildingSnapshot> ownBuildings,
            List<BotBuildingSnapshot> enemyCandidates)
        {
            if (_construction is not IConstructionSaveSnapshotSource source)
                return;

            IReadOnlyList<ConstructionSavedPlacement> placements =
                source.GetSavedPlacements();

            if (placements == null)
                return;

            for (int index = 0; index < placements.Count; index++)
            {
                ConstructionSavedPlacement placement = placements[index];
                string placementOwner = Normalize(placement.OwnerId);

                var building = new BotBuildingSnapshot(
                    placement.BuildingId,
                    placement.OwnerId,
                    placement.Position);

                if (string.Equals(placementOwner, ownerId, StringComparison.Ordinal))
                    ownBuildings.Add(building);
                else if (placementOwner != null)
                    enemyCandidates.Add(building);
            }
        }

        private bool IsVisible(string ownerId, Vector2Int position)
        {
            if (_perception != null)
                return _perception.IsVisible(ownerId, position);

            IFogOfWarService fog = null;
            _fogRegistry?.TryGetFor(ownerId, out fog);
            return fog != null && fog.IsVisible(position);
        }

        private Vector2Int ResolveStartPosition(string ownerId)
        {
            IReadOnlyList<FactionDefinition> factions = _factions?.GetAll();
            if (factions != null)
            {
                for (int index = 0; index < factions.Count; index++)
                {
                    FactionDefinition faction = factions[index];
                    if (faction != null &&
                        string.Equals(
                            Normalize(faction.FactionId.Value),
                            ownerId,
                            StringComparison.Ordinal))
                    {
                        return faction.StartPosition;
                    }
                }
            }

            IReadOnlyList<TurnFaction> turnFactions = _turns?.Factions;
            if (turnFactions == null)
                return Vector2Int.zero;

            for (int index = 0; index < turnFactions.Count; index++)
            {
                if (string.Equals(
                        Normalize(turnFactions[index].OwnerId),
                        ownerId,
                        StringComparison.Ordinal))
                {
                    return turnFactions[index].StartPosition;
                }
            }

            return Vector2Int.zero;
        }

        private static void SortUnits(List<BotUnitSnapshot> units)
        {
            units.Sort((left, right) =>
            {
                int x = left.Position.x.CompareTo(right.Position.x);
                if (x != 0)
                    return x;

                int y = left.Position.y.CompareTo(right.Position.y);
                return y != 0
                    ? y
                    : string.CompareOrdinal(left.UnitId, right.UnitId);
            });
        }

        private static void SortBuildings(List<BotBuildingSnapshot> buildings)
        {
            buildings.Sort((left, right) =>
            {
                int x = left.Position.x.CompareTo(right.Position.x);
                if (x != 0)
                    return x;

                int y = left.Position.y.CompareTo(right.Position.y);
                return y != 0
                    ? y
                    : string.CompareOrdinal(left.BuildingId, right.BuildingId);
            });
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
