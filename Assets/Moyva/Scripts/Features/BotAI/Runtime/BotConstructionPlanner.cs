using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotConstructionPlanner : IBotConstructionPlanner
    {
        internal const int MaxCandidateCells = 128;
        internal const int SearchRadius = 6;

        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionPlacementQuery _placementQuery;

        [Inject]
        public BotConstructionPlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IConstructionPlacementQuery placementQuery = null)
        {
            _buildings = buildings;
            _placementQuery = placementQuery;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null || _buildings == null || _placementQuery == null)
                return Array.Empty<BotActionCandidate>();

            if (HasOwnedRecruitmentBuilding(snapshot))
                return Array.Empty<BotActionCandidate>();

            BuildingDefinition recruitment = FindPreferredRecruitmentBuilding();
            if (recruitment == null || string.IsNullOrWhiteSpace(recruitment.Id))
                return Array.Empty<BotActionCandidate>();

            if (!TryFindPlacement(snapshot, recruitment.Id, out Vector2Int position, out int score))
                return Array.Empty<BotActionCandidate>();

            return new[]
            {
                new BotActionCandidate(
                    $"build:{recruitment.Id}:{position.x},{position.y}",
                    BotActionKind.Build,
                    strategy.Posture,
                    new BotActionScore(score, "Missing recruitment capability."),
                    targetCell: position,
                    definitionId: recruitment.Id,
                    reason: "Build first available recruitment building through canonical placement query."),
            };
        }

        private bool TryFindPlacement(
            BotWorldSnapshot snapshot,
            string buildingId,
            out Vector2Int position,
            out int score)
        {
            position = default;
            score = int.MinValue;
            int evaluated = 0;
            List<Vector2Int> cells = BotTurnExecutor.BuildRingCandidates(snapshot.StartPosition, SearchRadius);
            for (int index = 0; index < cells.Count && evaluated < MaxCandidateCells; index++)
            {
                Vector2Int candidate = cells[index];
                evaluated++;

                var request = new ConstructionPlacementQueryRequest(
                    buildingId,
                    candidate,
                    includeResources: true,
                    includeDetails: false,
                    ownerId: snapshot.OwnerId,
                    attemptSource: ConstructionPlacementAttemptSource.DirectPlace);
                ConstructionPlacementQueryResult result = _placementQuery.EvaluatePlacement(request);
                if (!result.CanCommit)
                    continue;

                int candidateScore = ScorePlacement(snapshot.StartPosition, candidate);
                if (candidateScore > score
                    || (candidateScore == score && BotTurnExecutor.ComparePosition(candidate, position) < 0))
                {
                    position = candidate;
                    score = candidateScore;
                }
            }

            return score > int.MinValue;
        }

        private static int ScorePlacement(Vector2Int anchor, Vector2Int candidate)
        {
            int distance = Mathf.Abs(anchor.x - candidate.x) + Mathf.Abs(anchor.y - candidate.y);
            return 600 - (distance * 10);
        }

        private bool HasOwnedRecruitmentBuilding(BotWorldSnapshot snapshot)
        {
            for (int index = 0; index < snapshot.OwnBuildings.Count; index++)
            {
                BotBuildingSnapshot building = snapshot.OwnBuildings[index];
                BuildingDefinition definition = _buildings.GetById(building.BuildingId);
                if (definition != null
                    && BuildingDefinitionCapabilities.HasEnabledModule<UnitRecruitmentBuildingModule>(definition))
                {
                    return true;
                }
            }

            return false;
        }

        private BuildingDefinition FindPreferredRecruitmentBuilding()
        {
            BuildingDefinition[] all = _buildings.GetAll();
            if (all == null || all.Length == 0)
                return null;

            BuildingDefinition best = null;
            for (int index = 0; index < all.Length; index++)
            {
                BuildingDefinition candidate = all[index];
                if (candidate == null
                    || string.IsNullOrWhiteSpace(candidate.Id)
                    || !BuildingDefinitionCapabilities.HasEnabledModule<UnitRecruitmentBuildingModule>(candidate))
                {
                    continue;
                }

                if (best == null
                    || candidate.Category == BuildingCategory.Military && best.Category != BuildingCategory.Military
                    || string.CompareOrdinal(candidate.Id, best.Id) < 0)
                {
                    best = candidate;
                }
            }

            return best;
        }
    }
}
