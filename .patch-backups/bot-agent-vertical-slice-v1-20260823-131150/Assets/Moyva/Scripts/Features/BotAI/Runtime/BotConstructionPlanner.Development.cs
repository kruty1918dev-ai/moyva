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
            List<Vector2Int> cells = BotDeterministicGeometry.BuildRingCandidates(
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
                    BotDeterministicGeometry.ComparePosition(candidate, position) < 0)
                {
                    position = candidate;
                    score = candidateScore;
                }
            }

            return score > int.MinValue;
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
    }
}
