using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed partial class BotConstructionPlanner
    {
        private IReadOnlyList<BotActionCandidate>
            GenerateDevelopmentCandidates(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
        {
            BuildingDefinition[] definitions =
                _buildings.GetAll();

            if (definitions == null ||
                definitions.Length == 0 ||
                _developmentUtility == null)
            {
                return Array.Empty<BotActionCandidate>();
            }

            bool ownsWarehouse =
                HasOwnedCapability(
                    snapshot,
                    BuildingDefinitionCapabilities.IsWarehouse);

            bool ownsRecruitment =
                HasOwnedRecruitmentBuilding(snapshot);

            HashSet<string> ownedIndustry =
                CollectOwnedIndustrialResourceIds(
                    snapshot);

            Vector2Int anchor =
                ResolveSettlementAnchor(snapshot);

            var result =
                new List<BotActionCandidate>();

            var diagnostics =
                new List<string>();

            for (int definitionIndex = 0;
                 definitionIndex < definitions.Length;
                 definitionIndex++)
            {
                BuildingDefinition definition =
                    definitions[definitionIndex];

                BotDevelopmentDefinitionScore definitionScore =
                    _developmentUtility.Evaluate(
                        snapshot,
                        strategy,
                        definition,
                        ownsWarehouse,
                        ownsRecruitment,
                        ownedIndustry);

                if (definitionScore.Utility ==
                    int.MinValue)
                {
                    continue;
                }

                List<BotDevelopmentPlacementCandidate> sites =
                    FindTopDevelopmentSites(
                        snapshot,
                        definition,
                        anchor);

                if (sites.Count == 0)
                {
                    diagnostics.Add(
                        $"{definition.Id}: no commit-ready site");
                    continue;
                }

                for (int siteIndex = 0;
                     siteIndex < sites.Count;
                     siteIndex++)
                {
                    BotDevelopmentPlacementCandidate site =
                        sites[siteIndex];

                    int total =
                        definitionScore.Utility +
                        site.SiteScore.Utility;

                    string reason =
                        $"definition=[{definitionScore.Reason}]; " +
                        $"site=[{site.SiteScore.Reason}]";

                    result.Add(
                        new BotActionCandidate(
                            $"build:{definition.Id}:" +
                            $"{site.Cell.x},{site.Cell.y}",
                            BotActionKind.Build,
                            strategy.Posture,
                            new BotActionScore(
                                total,
                                reason),
                            targetCell: site.Cell,
                            definitionId: definition.Id,
                            reason:
                                "utility-based-city-development"));
                }
            }

            result.Sort(CompareDevelopmentCandidate);

            if (result.Count >
                MaxDevelopmentCandidates)
            {
                result.RemoveRange(
                    MaxDevelopmentCandidates,
                    result.Count -
                    MaxDevelopmentCandidates);
            }

            if (result.Count == 0)
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    "Development: no commit-ready build candidate",
                    diagnostics.Count == 0
                        ? "No non-Castle building definition produced " +
                          "positive development utility."
                        : string.Join(
                            " | ",
                            diagnostics));
            }
            else
            {
                BotActionCandidate top =
                    result[0];

                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Candidate,
                    $"Development utility winner: {top.DefinitionId}",
                    $"strategy={strategy.Posture}; " +
                    $"candidateCount={result.Count}; " +
                    $"topCell={top.TargetCell}; " +
                    $"topUtility={top.Score.Total}; " +
                    $"{top.Score.Explanation}",
                    top.Score.Total,
                    top.TargetCell,
                    top.DefinitionId);
            }

            return result;
        }

        private List<BotDevelopmentPlacementCandidate>
            FindTopDevelopmentSites(
                BotWorldSnapshot snapshot,
                BuildingDefinition definition,
                Vector2Int anchor)
        {
            var result =
                new List<BotDevelopmentPlacementCandidate>();

            var rejection =
                new BotPlacementRejectionAccumulator();

            List<Vector2Int> cells =
                BotDeterministicGeometry.BuildRingCandidates(
                    anchor,
                    DevelopmentSearchRadius);

            int evaluated = 0;

            for (int index = 0;
                 index < cells.Count &&
                 evaluated <
                 MaxDevelopmentCellsPerDefinition;
                 index++)
            {
                Vector2Int candidate =
                    cells[index];

                evaluated++;

                BotPlacementProbe probe =
                    _placementProbe.Evaluate(
                        snapshot.OwnerId,
                        definition.Id,
                        candidate);

                rejection.Observe(probe);

                if (!probe.CanCommit)
                    continue;

                BotDevelopmentSiteScore siteScore =
                    _developmentSite?.Evaluate(
                        snapshot,
                        anchor,
                        candidate)
                    ?? new BotDevelopmentSiteScore(
                        500 -
                        Manhattan(anchor, candidate) * 18,
                        "terrain evaluator unavailable");

                result.Add(
                    new BotDevelopmentPlacementCandidate(
                        candidate,
                        siteScore));
            }

            result.Sort(
                (left, right) =>
                {
                    int score =
                        right.SiteScore.Utility.CompareTo(
                            left.SiteScore.Utility);

                    return score != 0
                        ? score
                        : BotDeterministicGeometry
                            .ComparePosition(
                                left.Cell,
                                right.Cell);
                });

            if (result.Count >
                MaxDevelopmentSitesPerDefinition)
            {
                result.RemoveRange(
                    MaxDevelopmentSitesPerDefinition,
                    result.Count -
                    MaxDevelopmentSitesPerDefinition);
            }

            if (result.Count == 0 &&
                rejection.Legal > 0)
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    $"Development legal but blocked: {definition.Id}",
                    rejection.BuildSummary(),
                    subjectId: definition.Id);
            }

            return result;
        }

        private bool HasOwnedRecruitmentBuilding(
            BotWorldSnapshot snapshot)
        {
            for (int index = 0;
                 index < snapshot.OwnBuildings.Count;
                 index++)
            {
                BotBuildingSnapshot building =
                    snapshot.OwnBuildings[index];

                BuildingDefinition definition =
                    _buildings.GetById(
                        building.BuildingId);

                if (definition != null &&
                    BuildingDefinitionCapabilities
                        .HasEnabledModule<
                            UnitRecruitmentBuildingModule>(
                            definition))
                {
                    return true;
                }
            }

            return false;
        }

        private HashSet<string>
            CollectOwnedIndustrialResourceIds(
                BotWorldSnapshot snapshot)
        {
            var result =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                string resourceId =
                    BuildingDefinitionCapabilities
                        .GetIndustrialResourceId(
                            definition);

                if (!string.IsNullOrWhiteSpace(
                        resourceId))
                {
                    result.Add(resourceId);
                }
            }

            return result;
        }

        private Vector2Int ResolveSettlementAnchor(
            BotWorldSnapshot snapshot)
        {
            // Anchor city growth on the actual data-driven Castle, not on
            // "the first building in snapshot".
            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BotBuildingSnapshot building =
                    snapshot.OwnBuildings[i];

                BuildingDefinition definition =
                    _buildings.GetById(
                        building.BuildingId);

                if (definition != null &&
                    BuildingDefinitionCapabilities.IsCastle(
                        definition))
                {
                    return building.Position;
                }
            }

            return snapshot.StartPosition;
        }

        private static int CompareDevelopmentCandidate(
            BotActionCandidate left,
            BotActionCandidate right)
        {
            int score =
                right.Score.Total.CompareTo(
                    left.Score.Total);

            if (score != 0)
                return score;

            int definition =
                string.CompareOrdinal(
                    left.DefinitionId,
                    right.DefinitionId);

            if (definition != 0)
                return definition;

            return string.CompareOrdinal(
                left.CandidateId,
                right.CandidateId);
        }

        private static int Manhattan(
            Vector2Int left,
            Vector2Int right)
            => Mathf.Abs(left.x - right.x) +
               Mathf.Abs(left.y - right.y);

        private readonly struct
            BotDevelopmentPlacementCandidate
        {
            public BotDevelopmentPlacementCandidate(
                Vector2Int cell,
                BotDevelopmentSiteScore siteScore)
            {
                Cell = cell;
                SiteScore = siteScore;
            }

            public Vector2Int Cell { get; }
            public BotDevelopmentSiteScore SiteScore { get; }
        }
    }
}
