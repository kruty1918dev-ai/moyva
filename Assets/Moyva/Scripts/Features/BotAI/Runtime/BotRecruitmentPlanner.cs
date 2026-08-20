using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotRecruitmentPlanner : IBotRecruitmentPlanner
    {
        private readonly IBuildingRegistry _buildings;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitClassConfig _configs;
        private readonly IEconomyInfoMediator _economy;
        private readonly IBotUnitRoleResolver _roles;

        [Inject]
        public BotRecruitmentPlanner(
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IUnitRecruitmentService recruitment = null,
            [InjectOptional] IUnitClassConfig configs = null,
            [InjectOptional] IEconomyInfoMediator economy = null,
            [InjectOptional] IBotUnitRoleResolver roles = null)
        {
            _buildings = buildings;
            _recruitment = recruitment;
            _configs = configs;
            _economy = economy;
            _roles = roles;
        }

        public IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            if (snapshot == null || _buildings == null || _recruitment == null || _configs == null)
                return Array.Empty<BotActionCandidate>();

            IReadOnlyDictionary<string, float> resources = _economy?.GetOwnerResourceTotals(snapshot.OwnerId);
            Dictionary<BotUnitTacticalRole, int> current = CountRoles(snapshot);
            int armyCount = Math.Max(1, CountMilitary(current));
            var result = new List<BotActionCandidate>();

            for (int buildingIndex = 0; buildingIndex < snapshot.OwnBuildings.Count; buildingIndex++)
            {
                BotBuildingSnapshot building = snapshot.OwnBuildings[buildingIndex];
                BuildingDefinition definition = _buildings.GetById(building.BuildingId);
                if (definition == null || !BuildingDefinitionCapabilities.TryGetEnabledModule(definition, out UnitRecruitmentBuildingModule module) || module?.Recipes == null)
                    continue;

                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue = _recruitment.GetQueue(snapshot.OwnerId, building.Position);
                int queueCount = queue?.Count ?? 0;
                if (queueCount >= Math.Max(1, module.QueueCapacity))
                    continue;

                for (int recipeIndex = 0; recipeIndex < module.Recipes.Count; recipeIndex++)
                {
                    UnitRecruitmentRecipeDefinition recipe = module.Recipes[recipeIndex];
                    if (recipe == null || string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                        continue;

                    UnitClassConfig config = BotTacticalAnalysis.SafeGetConfig(_configs, recipe.UnitTypeId);
                    if (config == null)
                        continue;

                    BotUnitTacticalRole role = _roles?.Resolve(config) ?? BotAdvancedHeuristics.InferRole(config);
                    if (role == BotUnitTacticalRole.Worker || role == BotUnitTacticalRole.Unknown)
                        continue;

                    bool affordable = HasResources(resources, recipe.Costs);
                    if (!affordable)
                        continue;

                    int existing = current.TryGetValue(role, out int count) ? count : 0;
                    int desiredPercent = BotAdvancedHeuristics.DesiredRolePercent(role, strategy.Posture);
                    int currentPercent = armyCount <= 0 ? 0 : existing * 100 / armyCount;
                    int deficit = Math.Max(0, desiredPercent - currentPercent);
                    int counter = ScoreCounters(config, snapshot.VisibleEnemyUnits);
                    int score = 700 + deficit * 25 + counter - Math.Max(1, recipe.TrainingTurns) * 25;
                    if (strategy.Posture == BotStrategicPosture.ArmyBuildUp) score += 500;
                    if (strategy.Posture == BotStrategicPosture.EmergencyDefense) score += 300;
                    if (role == BotUnitTacticalRole.Siege && strategy.Posture != BotStrategicPosture.Siege) score -= 300;

                    result.Add(new BotActionCandidate(
                        $"recruit:{building.Position.x},{building.Position.y}:{recipe.UnitTypeId}",
                        BotActionKind.Recruit,
                        strategy.Posture,
                        new BotActionScore(score, $"Role={role}; deficit={deficit}; counter={counter}; affordable recipe."),
                        targetCell: building.Position,
                        definitionId: recipe.UnitTypeId,
                        reason: "data-driven-recruitment-composition"));
                }
            }

            result.Sort(CompareCandidate);
            return result;
        }

        private Dictionary<BotUnitTacticalRole, int> CountRoles(BotWorldSnapshot snapshot)
        {
            var result = new Dictionary<BotUnitTacticalRole, int>();
            for (int i = 0; i < snapshot.OwnUnits.Count; i++)
            {
                UnitClassConfig config = BotTacticalAnalysis.SafeGetConfig(_configs, snapshot.OwnUnits[i].TypeId);
                BotUnitTacticalRole role = _roles?.Resolve(config) ?? BotAdvancedHeuristics.InferRole(config);
                result.TryGetValue(role, out int current);
                result[role] = current + 1;
            }
            return result;
        }

        private static int CountMilitary(Dictionary<BotUnitTacticalRole, int> roles)
        {
            int total = 0;
            foreach (KeyValuePair<BotUnitTacticalRole, int> pair in roles)
                if (pair.Key != BotUnitTacticalRole.Worker && pair.Key != BotUnitTacticalRole.Unknown)
                    total += pair.Value;
            return total;
        }

        private static bool HasResources(IReadOnlyDictionary<string, float> totals, IReadOnlyList<BuildingResourceAmount> costs)
        {
            if (costs == null || costs.Count == 0)
                return true;
            if (totals == null)
                return false;
            for (int i = 0; i < costs.Count; i++)
            {
                BuildingResourceAmount cost = costs[i];
                if (cost == null || string.IsNullOrWhiteSpace(cost.ResourceId) || cost.Amount <= 0f)
                    continue;
                if (!totals.TryGetValue(cost.ResourceId, out float available) || available + 0.0001f < cost.Amount)
                    return false;
            }
            return true;
        }

        private int ScoreCounters(UnitClassConfig candidate, IReadOnlyList<BotUnitSnapshot> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return 0;
            long total = 0;
            int counted = 0;
            for (int i = 0; i < enemies.Count && counted < 8; i++)
            {
                UnitClassConfig enemy = BotTacticalAnalysis.SafeGetConfig(_configs, enemies[i].TypeId);
                if (enemy == null) continue;
                UnitCombatBreakdown attack = UnitCombatCalculator.CalculateAttack(candidate, enemy);
                UnitCombatBreakdown counter = UnitCombatCalculator.CalculateAttack(enemy, candidate);
                total += (attack.TotalDamage - counter.TotalDamage) * 8L;
                counted++;
            }
            if (counted == 0) return 0;
            long value = total / counted;
            return value > 600 ? 600 : value < -600 ? -600 : (int)value;
        }

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            return score != 0 ? score : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
