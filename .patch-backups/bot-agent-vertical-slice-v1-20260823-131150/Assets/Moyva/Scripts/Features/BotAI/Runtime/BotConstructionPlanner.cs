using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed partial class BotConstructionPlanner : IBotConstructionPlanner
    {
        internal const int MaxCandidateCells = 256;
        internal const int CastleSearchRadius = 10;
        internal const int DevelopmentSearchRadius = 6;
        internal const int MaxLoggedSiteEvaluations = 12;

        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly IBotCastleSiteEvaluator _castleEvaluator;
        private readonly IBotReasoningTrace _reasoning;

        [Inject]
        public BotConstructionPlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IConstructionPlacementQuery placementQuery = null,
            [InjectOptional] IBotCastleSiteEvaluator castleEvaluator = null,
            [InjectOptional] IBotReasoningTrace reasoning = null)
        {
            _buildings = buildings;
            _placementQuery = placementQuery;
            _castleEvaluator = castleEvaluator;
            _reasoning = reasoning;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null ||
                _buildings == null ||
                _placementQuery == null)
            {
                return Array.Empty<BotActionCandidate>();
            }

            if (!HasOwnedCastle(snapshot))
                return GenerateCastlePlacement(snapshot, strategy);

            BuildingDefinition development =
                FindNextDevelopmentBuilding(
                    snapshot,
                    out string developmentReason,
                    out int developmentBaseScore);

            if (development == null ||
                string.IsNullOrWhiteSpace(development.Id))
            {
                return Array.Empty<BotActionCandidate>();
            }

            if (!TryFindDevelopmentPlacement(
                    snapshot,
                    development.Id,
                    out Vector2Int position,
                    out int placementScore))
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    $"Не можу розмістити {development.Id}",
                    $"Development stage '{developmentReason}' обрано, але canonical placement query не знайшов допустимої клітинки.");
                return Array.Empty<BotActionCandidate>();
            }

            int score = developmentBaseScore + placementScore;
            _reasoning?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                BotReasoningStage.Selection,
                $"Наступний крок розвитку: {development.Id}",
                $"{developmentReason} Обрана клітинка {position}; placement score={placementScore}, final score={score}.",
                score,
                position,
                development.Id);

            return new[]
            {
                new BotActionCandidate(
                    $"build:{development.Id}:{position.x},{position.y}",
                    BotActionKind.Build,
                    strategy.Posture,
                    new BotActionScore(
                        score,
                        developmentReason),
                    targetCell: position,
                    definitionId: development.Id,
                    reason: "data-driven-settlement-development"),
            };
        }



        private ConstructionPlacementQueryResult EvaluatePlacement(
            string ownerId,
            string buildingId,
            Vector2Int candidate)
        {
            var request = new ConstructionPlacementQueryRequest(
                buildingId,
                candidate,
                includeResources: true,
                includeDetails: false,
                ownerId: ownerId,
                attemptSource: ConstructionPlacementAttemptSource.DirectPlace);

            return _placementQuery.EvaluatePlacement(request);
        }







        private bool HasOwnedCapability(
            BotWorldSnapshot snapshot,
            Func<BuildingDefinition, bool> predicate)
        {
            if (predicate == null)
                return false;

            for (int i = 0; i < snapshot.OwnBuildings.Count; i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                if (definition != null && predicate(definition))
                    return true;
            }

            return false;
        }






    }
}
