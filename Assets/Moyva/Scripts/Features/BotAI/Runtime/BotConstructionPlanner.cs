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

            List<Vector2Int> cells = BotTurnExecutor.BuildRingCandidates(
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
                    BotTurnExecutor.ComparePosition(site.Cell, best.Cell) < 0)
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

        private bool TryFindDevelopmentPlacement(
            BotWorldSnapshot snapshot,
            string buildingId,
            out Vector2Int position,
            out int score)
        {
            position = default;
            score = int.MinValue;
            int evaluated = 0;

            Vector2Int anchor = ResolveSettlementAnchor(snapshot);
            List<Vector2Int> cells = BotTurnExecutor.BuildRingCandidates(
                anchor,
                DevelopmentSearchRadius);

            for (int index = 0;
                 index < cells.Count && evaluated < MaxCandidateCells;
                 index++)
            {
                Vector2Int candidate = cells[index];
                evaluated++;

                ConstructionPlacementQueryResult result =
                    EvaluatePlacement(
                        snapshot.OwnerId,
                        buildingId,
                        candidate);

                if (!result.CanCommit)
                    continue;

                int candidateScore =
                    ScoreDevelopmentPlacement(anchor, candidate);

                if (candidateScore > score ||
                    candidateScore == score &&
                    BotTurnExecutor.ComparePosition(candidate, position) < 0)
                {
                    position = candidate;
                    score = candidateScore;
                }
            }

            return score > int.MinValue;
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

        private BuildingDefinition FindNextDevelopmentBuilding(
            BotWorldSnapshot snapshot,
            out string reason,
            out int baseScore)
        {
            reason = string.Empty;
            baseScore = 0;

            BuildingDefinition warehouse = FindPreferredWarehouse();
            if (warehouse != null && !HasOwnedCapability(snapshot, BuildingDefinitionCapabilities.IsWarehouse))
            {
                reason = "Після замку потрібне сховище, щоб місто мало стійку економічну інфраструктуру.";
                baseScore = 1100;
                return warehouse;
            }

            HashSet<string> ownedIndustrialResources =
                CollectOwnedIndustrialResourceIds(snapshot);

            if (ownedIndustrialResources.Count < 2)
            {
                BuildingDefinition production =
                    FindPreferredProductionBuilding(
                        ownedIndustrialResources);

                if (production != null)
                {
                    string resourceId =
                        BuildingDefinitionCapabilities
                            .GetIndustrialResourceId(production);

                    reason =
                        $"Розширюю базову економіку. Будівля '{production.Id}' додає ресурс '{resourceId}' " +
                        $"і збільшує різноманітність виробництва ({ownedIndustrialResources.Count}/2).";

                    baseScore =
                        980 +
                        BuildingDefinitionCapabilities
                            .GetEconomyPriority(production) * 10;

                    return production;
                }
            }

            if (!HasOwnedRecruitmentBuilding(snapshot))
            {
                BuildingDefinition recruitment =
                    FindPreferredRecruitmentBuilding();

                if (recruitment != null)
                {
                    reason =
                        "Базова економічна інфраструктура вже сформована; тепер потрібна recruitment-capable військова будівля.";
                    baseScore = 900;
                    return recruitment;
                }
            }

            reason = "Базовий ланцюжок Castle → Storage → Production → Recruitment завершено.";
            return null;
        }

        private BuildingDefinition FindPreferredWarehouse()
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
                    !BuildingDefinitionCapabilities.IsWarehouse(candidate))
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

        private BuildingDefinition FindPreferredProductionBuilding(
            HashSet<string> ownedResourceIds)
        {
            BuildingDefinition[] all = _buildings.GetAll();
            if (all == null)
                return null;

            BuildingDefinition best = null;
            int bestPriority = int.MinValue;

            for (int i = 0; i < all.Length; i++)
            {
                BuildingDefinition candidate = all[i];
                if (candidate == null ||
                    string.IsNullOrWhiteSpace(candidate.Id) ||
                    BuildingDefinitionCapabilities.IsCastle(candidate))
                {
                    continue;
                }

                string resourceId =
                    BuildingDefinitionCapabilities
                        .GetIndustrialResourceId(candidate);

                if (string.IsNullOrWhiteSpace(resourceId) ||
                    ownedResourceIds.Contains(resourceId))
                {
                    continue;
                }

                int priority =
                    BuildingDefinitionCapabilities
                        .GetEconomyPriority(candidate) * 10;

                if (best == null ||
                    priority > bestPriority ||
                    priority == bestPriority &&
                    string.CompareOrdinal(candidate.Id, best.Id) < 0)
                {
                    best = candidate;
                    bestPriority = priority;
                }
            }

            return best;
        }

        private HashSet<string> CollectOwnedIndustrialResourceIds(
            BotWorldSnapshot snapshot)
        {
            var result = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < snapshot.OwnBuildings.Count; i++)
            {
                BuildingDefinition definition =
                    _buildings.GetById(
                        snapshot.OwnBuildings[i].BuildingId);

                string resourceId =
                    BuildingDefinitionCapabilities
                        .GetIndustrialResourceId(definition);

                if (!string.IsNullOrWhiteSpace(resourceId))
                    result.Add(resourceId);
            }

            return result;
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
                    _buildings.GetById(building.BuildingId);

                if (definition != null &&
                    BuildingDefinitionCapabilities
                        .HasEnabledModule<UnitRecruitmentBuildingModule>(
                            definition))
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

                if (candidate == null ||
                    string.IsNullOrWhiteSpace(candidate.Id) ||
                    !BuildingDefinitionCapabilities
                        .HasEnabledModule<UnitRecruitmentBuildingModule>(
                            candidate))
                {
                    continue;
                }

                if (best == null ||
                    candidate.Category == BuildingCategory.Military &&
                    best.Category != BuildingCategory.Military ||
                    string.CompareOrdinal(candidate.Id, best.Id) < 0)
                {
                    best = candidate;
                }
            }

            return best;
        }

        private static Vector2Int ResolveSettlementAnchor(
            BotWorldSnapshot snapshot)
        {
            if (snapshot?.OwnBuildings != null &&
                snapshot.OwnBuildings.Count > 0)
            {
                return snapshot.OwnBuildings[0].Position;
            }

            return snapshot?.StartPosition ?? Vector2Int.zero;
        }

        private static int ScoreDevelopmentPlacement(
            Vector2Int anchor,
            Vector2Int candidate)
        {
            int distance =
                Mathf.Abs(anchor.x - candidate.x) +
                Mathf.Abs(anchor.y - candidate.y);

            return 700 - distance * 12;
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
                    : BotTurnExecutor.ComparePosition(
                        left.Cell,
                        right.Cell);
            });

            if (list.Count > cap)
                list.RemoveRange(cap, list.Count - cap);
        }
    }
}
