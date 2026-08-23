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
            GenerateCastlePlacement(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
        {
            BuildingDefinition castle =
                FindCastleDefinition();

            if (castle == null ||
                string.IsNullOrWhiteSpace(castle.Id))
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    "Castle Search: definition missing",
                    "BuildingRegistry contains no data-driven definition " +
                    "for which BuildingDefinitionCapabilities.IsCastle is true.");

                return Array.Empty<BotActionCandidate>();
            }

            var rejection =
                new BotPlacementRejectionAccumulator();

            var strongestLegal =
                new List<BotSiteEvaluation>();

            var seen = new HashSet<Vector2Int>();

            BotSiteEvaluation bestLegal = null;
            BotSiteEvaluation bestCommitReady = null;

            BotPlacementProbe bestLegalProbe = default;
            int evaluated = 0;

            int[] searchRadii =
            {
                CastleSearchRadius,
                CastleSearchExpandedRadius,
                CastleSearchMaximumRadius,
            };

            for (int radiusIndex = 0;
                 radiusIndex < searchRadii.Length &&
                 evaluated < MaxCastleCandidateCells;
                 radiusIndex++)
            {
                int radius = searchRadii[radiusIndex];

                List<Vector2Int> cells =
                    BotDeterministicGeometry.BuildRingCandidates(
                        snapshot.StartPosition,
                        radius);

                for (int index = 0;
                     index < cells.Count &&
                     evaluated < MaxCastleCandidateCells;
                     index++)
                {
                    Vector2Int candidate =
                        cells[index];

                    if (!seen.Add(candidate))
                        continue;

                    evaluated++;

                    BotPlacementProbe probe =
                        _placementProbe.Evaluate(
                            snapshot.OwnerId,
                            castle.Id,
                            candidate);

                    rejection.Observe(probe);

                    // Legality is intentionally independent from resources.
                    // This lets diagnostics distinguish "great site, cannot
                    // afford it yet" from "site is spatially impossible".
                    if (!probe.IsLegal)
                        continue;

                    BotSiteEvaluation site =
                        _castleEvaluator?.Evaluate(
                            snapshot,
                            castle.Id,
                            candidate,
                            placementAllowed: true)
                        ?? FallbackEvaluation(
                            snapshot.StartPosition,
                            candidate,
                            placementAllowed: true);

                    InsertTop(
                        strongestLegal,
                        site,
                        MaxLoggedSiteEvaluations);

                    if (IsBetterSite(
                            site,
                            bestLegal))
                    {
                        bestLegal = site;
                        bestLegalProbe = probe;
                    }

                    if (probe.CanCommit &&
                        IsBetterSite(
                            site,
                            bestCommitReady))
                    {
                        bestCommitReady = site;
                    }
                }

                // If a commit-ready capital exists in the normal search radius
                // there is no reason to scan the whole map. Expansion is only
                // a recovery path for difficult generated terrain.
                if (bestCommitReady != null &&
                    radius == CastleSearchRadius)
                {
                    break;
                }
            }

            string rejectionSummary =
                rejection.BuildSummary();

            _reasoning?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                BotReasoningStage.TerrainScan,
                "Castle Search: legality + utility scan",
                $"start={snapshot.StartPosition}; evaluated={evaluated}; " +
                $"searchRadiusUpTo={CastleSearchMaximumRadius}. " +
                $"Legality is evaluated without resources; affordability " +
                $"and authority are evaluated separately. {rejectionSummary}");

            for (int i = 0;
                 i < strongestLegal.Count;
                 i++)
            {
                BotSiteEvaluation site =
                    strongestLegal[i];

                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.SiteEvaluation,
                    $"Castle legal site #{i + 1}: {site.Cell}",
                    BotReasoningNarrator.DescribeSite(site),
                    site.TotalScore,
                    site.Cell,
                    castle.Id,
                    site.Factors);
            }

            if (bestLegal == null)
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    "Castle Search rejected every site",
                    "No legal Castle position was found. " +
                    rejectionSummary);

                return Array.Empty<BotActionCandidate>();
            }

            if (bestCommitReady == null)
            {
                string affordabilityReason =
                    !bestLegalProbe.IsAffordable
                        ? "best legal site is currently unaffordable"
                        : !bestLegalProbe.HasAuthority
                            ? "best legal site lacks commit authority"
                            : "legal sites exist but none is commit-ready";

                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    "Castle Search found legal site but cannot commit",
                    $"{affordabilityReason}; bestLegal={bestLegal.Cell}; " +
                    $"utility={bestLegal.TotalScore}; " +
                    $"reasonCode='{bestLegalProbe.ReasonCode}'; " +
                    $"reason='{bestLegalProbe.Reason}'. " +
                    rejectionSummary,
                    bestLegal.TotalScore,
                    bestLegal.Cell,
                    castle.Id,
                    bestLegal.Factors);

                return Array.Empty<BotActionCandidate>();
            }

            int finalScore = Mathf.Clamp(
                2400 + bestCommitReady.TotalScore,
                1400,
                6000);

            _reasoning?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                BotReasoningStage.Selection,
                $"Castle Search selected {bestCommitReady.Cell}",
                "Selected the highest-utility site among legal, affordable " +
                "and authoritative Castle placements. " +
                BotReasoningNarrator.DescribeSite(
                    bestCommitReady),
                finalScore,
                bestCommitReady.Cell,
                castle.Id,
                bestCommitReady.Factors);

            return new[]
            {
                new BotActionCandidate(
                    $"build:{castle.Id}:" +
                    $"{bestCommitReady.Cell.x}," +
                    $"{bestCommitReady.Cell.y}",
                    BotActionKind.Build,
                    strategy.Posture,
                    new BotActionScore(
                        finalScore,
                        bestCommitReady.Summary),
                    targetCell: bestCommitReady.Cell,
                    definitionId: castle.Id,
                    reason:
                        "castle-search-legal-affordable-utility"),
            };
        }

        private BuildingDefinition FindCastleDefinition()
        {
            BuildingDefinition[] all =
                _buildings.GetAll();

            if (all == null)
                return null;

            BuildingDefinition best = null;

            for (int i = 0; i < all.Length; i++)
            {
                BuildingDefinition candidate =
                    all[i];

                if (candidate == null ||
                    string.IsNullOrWhiteSpace(candidate.Id) ||
                    !BuildingDefinitionCapabilities.IsCastle(
                        candidate))
                {
                    continue;
                }

                if (best == null ||
                    string.CompareOrdinal(
                        candidate.Id,
                        best.Id) < 0)
                {
                    best = candidate;
                }
            }

            return best;
        }

        private bool HasOwnedCastle(
            BotWorldSnapshot snapshot)
        {
            if (snapshot == null)
                return false;

            for (int i = 0;
                 i < snapshot.OwnBuildings.Count;
                 i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                if (definition != null &&
                    BuildingDefinitionCapabilities.IsCastle(
                        definition))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsBetterSite(
            BotSiteEvaluation candidate,
            BotSiteEvaluation current)
        {
            if (candidate == null)
                return false;

            if (current == null)
                return true;

            if (candidate.TotalScore !=
                current.TotalScore)
            {
                return candidate.TotalScore >
                       current.TotalScore;
            }

            return
                BotDeterministicGeometry.ComparePosition(
                    candidate.Cell,
                    current.Cell) < 0;
        }

        private static BotSiteEvaluation
            FallbackEvaluation(
                Vector2Int start,
                Vector2Int cell,
                bool placementAllowed)
        {
            int distance =
                Mathf.Abs(start.x - cell.x) +
                Mathf.Abs(start.y - cell.y);

            int score =
                placementAllowed
                    ? 300 - distance * 12
                    : -100000;

            return new BotSiteEvaluation(
                cell,
                placementAllowed,
                score,
                placementAllowed
                    ? $"Fallback utility uses start distance={distance}; " +
                      "terrain service unavailable."
                    : "Canonical placement legality rejected this cell.",
                Array.Empty<BotSiteScoreFactor>());
        }

        private static void InsertTop(
            List<BotSiteEvaluation> list,
            BotSiteEvaluation evaluation,
            int cap)
        {
            list.Add(evaluation);

            list.Sort((left, right) =>
            {
                int score =
                    right.TotalScore.CompareTo(
                        left.TotalScore);

                return score != 0
                    ? score
                    : BotDeterministicGeometry
                        .ComparePosition(
                            left.Cell,
                            right.Cell);
            });

            if (list.Count > cap)
            {
                list.RemoveRange(
                    cap,
                    list.Count - cap);
            }
        }
    }
}
