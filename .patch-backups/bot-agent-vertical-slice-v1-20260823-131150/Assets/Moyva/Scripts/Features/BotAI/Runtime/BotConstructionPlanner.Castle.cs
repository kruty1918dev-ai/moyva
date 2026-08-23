using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed partial class BotConstructionPlanner
    {
        private IReadOnlyList<BotActionCandidate> GenerateCastlePlacement(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            BuildingDefinition castle = FindCastleDefinition();
            if (castle == null || string.IsNullOrWhiteSpace(castle.Id))
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    "Не знайдено визначення замку",
                    "У BuildingRegistry немає data-driven будівлі, для якої BuildingDefinitionCapabilities.IsCastle повертає true.");

                return Array.Empty<BotActionCandidate>();
            }

            List<Vector2Int> cells = BotDeterministicGeometry.BuildRingCandidates(
                snapshot.StartPosition,
                CastleSearchRadius);

            BotSiteEvaluation best = null;
            int evaluated = 0;
            int valid = 0;
            var strongest = new List<BotSiteEvaluation>();

            for (int index = 0;
                 index < cells.Count && evaluated < MaxCandidateCells;
                 index++)
            {
                Vector2Int candidate = cells[index];
                evaluated++;

                ConstructionPlacementQueryResult placement =
                    EvaluatePlacement(
                        snapshot.OwnerId,
                        castle.Id,
                        candidate);

                BotSiteEvaluation site =
                    _castleEvaluator?.Evaluate(
                        snapshot,
                        castle.Id,
                        candidate,
                        placement.CanCommit)
                    ?? FallbackEvaluation(
                        snapshot.StartPosition,
                        candidate,
                        placement.CanCommit);

                if (!site.PlacementAllowed)
                    continue;

                valid++;
                InsertTop(strongest, site, MaxLoggedSiteEvaluations);

                if (best == null ||
                    site.TotalScore > best.TotalScore ||
                    site.TotalScore == best.TotalScore &&
                    BotDeterministicGeometry.ComparePosition(site.Cell, best.Cell) < 0)
                {
                    best = site;
                }
            }

            _reasoning?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                BotReasoningStage.TerrainScan,
                "Пошук позиції для першого замку",
                $"Перевірено {evaluated} клітин навколо старту {snapshot.StartPosition}; " +
                $"{valid} пройшли canonical construction rules. " +
                "Кожна допустима позиція оцінюється за висотою, локальним high-ground, природними бар'єрами, " +
                "простором для міста, ризиком вищих стрілецьких позицій, прямими кавалерійськими підходами, краєм карти й видимими загрозами.");

            for (int i = 0; i < strongest.Count; i++)
            {
                BotSiteEvaluation site = strongest[i];
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.SiteEvaluation,
                    $"Кандидат замку #{i + 1}: {site.Cell}",
                    BotReasoningNarrator.DescribeSite(site),
                    site.TotalScore,
                    site.Cell,
                    castle.Id,
                    site.Factors);
            }

            if (best == null)
            {
                _reasoning?.Record(
                    snapshot.OwnerId,
                    snapshot.GlobalTurn,
                    BotReasoningStage.Warning,
                    "Замок неможливо розмістити",
                    $"Не знайдено жодної допустимої клітинки в радіусі {CastleSearchRadius}.");

                return Array.Empty<BotActionCandidate>();
            }

            int finalScore = Mathf.Clamp(
                2200 + best.TotalScore,
                1200,
                5000);

            _reasoning?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                BotReasoningStage.Selection,
                $"Обрано позицію замку {best.Cell}",
                BotReasoningNarrator.DescribeSite(best),
                finalScore,
                best.Cell,
                castle.Id,
                best.Factors);

            return new[]
            {
                new BotActionCandidate(
                    $"build:{castle.Id}:{best.Cell.x},{best.Cell.y}",
                    BotActionKind.Build,
                    strategy.Posture,
                    new BotActionScore(
                        finalScore,
                        best.Summary),
                    targetCell: best.Cell,
                    definitionId: castle.Id,
                    reason: "castle-site-weighted-evaluation"),
            };
        }

        private BuildingDefinition FindCastleDefinition()
        {
            BuildingDefinition[] all = _buildings.GetAll();
            if (all == null)
                return null;

            BuildingDefinition best = null;
            for (int i = 0; i < all.Length; i++)
            {
                BuildingDefinition candidate = all[i];
                if (candidate == null ||
                    string.IsNullOrWhiteSpace(candidate.Id) ||
                    !BuildingDefinitionCapabilities.IsCastle(candidate))
                {
                    continue;
                }

                if (best == null ||
                    string.CompareOrdinal(candidate.Id, best.Id) < 0)
                {
                    best = candidate;
                }
            }

            return best;
        }

        private bool HasOwnedCastle(BotWorldSnapshot snapshot)
        {
            for (int i = 0; i < snapshot.OwnBuildings.Count; i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(snapshot.OwnBuildings[i].BuildingId);

                if (definition != null &&
                    BuildingDefinitionCapabilities.IsCastle(definition))
                {
                    return true;
                }
            }

            return false;
        }

        private static BotSiteEvaluation FallbackEvaluation(
            Vector2Int start,
            Vector2Int cell,
            bool placementAllowed)
        {
            int distance =
                Mathf.Abs(start.x - cell.x) +
                Mathf.Abs(start.y - cell.y);

            int score = placementAllowed
                ? 300 - distance * 12
                : -100000;

            return new BotSiteEvaluation(
                cell,
                placementAllowed,
                score,
                placementAllowed
                    ? $"Fallback score based on start distance {distance}; terrain service unavailable."
                    : "Canonical placement rejected this cell.",
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
                    right.TotalScore.CompareTo(left.TotalScore);

                return score != 0
                    ? score
                    : BotDeterministicGeometry.ComparePosition(
                        left.Cell,
                        right.Cell);
            });

            if (list.Count > cap)
                list.RemoveRange(cap, list.Count - cap);
        }
    }
}
