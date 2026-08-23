using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed partial class BotConstructionPlanner :
        IBotConstructionPlanner
    {
        internal const int CastleSearchRadius = 10;
        internal const int CastleSearchExpandedRadius = 18;
        internal const int CastleSearchMaximumRadius = 28;
        internal const int MaxCastleCandidateCells = 2048;

        internal const int DevelopmentSearchRadius = 7;
        internal const int MaxDevelopmentCellsPerDefinition = 196;
        internal const int MaxDevelopmentCandidates = 12;
        internal const int MaxDevelopmentSitesPerDefinition = 2;

        internal const int MaxLoggedSiteEvaluations = 12;

        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly BotConstructionPlacementProbe _placementProbe;
        private readonly IBotCastleSiteEvaluator _castleEvaluator;
        private readonly BotDevelopmentSiteEvaluator _developmentSite;
        private readonly BotDevelopmentUtilityEvaluator _developmentUtility;
        private readonly IBotReasoningTrace _reasoning;

        [Inject]
        public BotConstructionPlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IConstructionPlacementQuery placementQuery = null,
            [InjectOptional] IBotCastleSiteEvaluator castleEvaluator = null,
            [InjectOptional] BotDevelopmentSiteEvaluator developmentSite = null,
            [InjectOptional] BotDevelopmentUtilityEvaluator developmentUtility = null,
            [InjectOptional] IBotReasoningTrace reasoning = null)
        {
            _buildings = buildings;
            _placementQuery = placementQuery;
            _placementProbe =
                placementQuery != null
                    ? new BotConstructionPlacementProbe(placementQuery)
                    : null;

            _castleEvaluator = castleEvaluator;
            _developmentSite = developmentSite;
            _developmentUtility = developmentUtility;
            _reasoning = reasoning;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null ||
                _buildings == null ||
                _placementQuery == null ||
                _placementProbe == null)
            {
                return Array.Empty<BotActionCandidate>();
            }

            // The first owned Castle is the vertical-slice boundary. Before it
            // exists, no secondary city-building script is allowed to distract
            // the agent from establishing a legal capital.
            if (!HasOwnedCastle(snapshot))
                return GenerateCastlePlacement(snapshot, strategy);

            // After the Castle, development is a utility competition, not a
            // hard-coded Warehouse -> Production -> Recruitment sequence.
            return GenerateDevelopmentCandidates(
                snapshot,
                strategy);
        }

        private bool HasOwnedCapability(
            BotWorldSnapshot snapshot,
            Func<BuildingDefinition, bool> predicate)
        {
            if (snapshot == null ||
                predicate == null)
            {
                return false;
            }

            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                if (definition != null &&
                    predicate(definition))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
