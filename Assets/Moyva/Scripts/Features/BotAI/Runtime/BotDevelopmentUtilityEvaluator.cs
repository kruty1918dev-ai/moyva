using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal readonly struct BotDevelopmentDefinitionScore
    {
        public BotDevelopmentDefinitionScore(
            int utility,
            string reason)
        {
            Utility = utility;
            Reason = reason ?? string.Empty;
        }

        public int Utility { get; }
        public string Reason { get; }
    }

    /// <summary>
    /// Data-driven utility for city development. This replaces the fixed
    /// Castle -> Warehouse -> Production -> Recruitment script with competing
    /// alternatives. Missing strategic capabilities receive high utility, but
    /// the winner still depends on posture, current army, existing industry,
    /// definition economy priority and site utility.
    /// </summary>
    internal sealed class BotDevelopmentUtilityEvaluator
    {
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotDevelopmentUtilityEvaluator(
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _profile =
                profile ??
                BotPlanningProfile.Normal();
        }

        public BotDevelopmentDefinitionScore Evaluate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy,
            BuildingDefinition definition,
            bool ownsWarehouse,
            bool ownsRecruitment,
            IReadOnlyCollection<string> ownedIndustrialResources)
        {
            if (snapshot == null ||
                definition == null ||
                string.IsNullOrWhiteSpace(definition.Id) ||
                BuildingDefinitionCapabilities.IsCastle(definition))
            {
                return new BotDevelopmentDefinitionScore(
                    int.MinValue,
                    "not-development-candidate");
            }

            bool warehouse =
                BuildingDefinitionCapabilities.IsWarehouse(
                    definition);

            bool recruitment =
                BuildingDefinitionCapabilities
                    .HasEnabledModule<UnitRecruitmentBuildingModule>(
                        definition);

            string industrialResource =
                BuildingDefinitionCapabilities
                    .GetIndustrialResourceId(definition);

            bool industrial =
                !string.IsNullOrWhiteSpace(
                    industrialResource);

            bool newIndustrial =
                industrial &&
                !ContainsResource(
                    ownedIndustrialResources,
                    industrialResource);

            int economyPriority =
                BuildingDefinitionCapabilities
                    .GetEconomyPriority(definition);

            int utility =
                160 +
                economyPriority * 12;

            var reasons = new List<string>();

            if (warehouse)
            {
                int value =
                    ownsWarehouse
                        ? -80
                        : 480;

                utility += value;
                reasons.Add(
                    ownsWarehouse
                        ? "warehouse already represented"
                        : "missing storage capability");
            }

            if (industrial)
            {
                int value =
                    newIndustrial
                        ? 430
                        : 110;

                utility += value;
                reasons.Add(
                    newIndustrial
                        ? $"new industrial resource '{industrialResource}'"
                        : $"existing industrial resource '{industrialResource}'");
            }

            if (recruitment)
            {
                int value =
                    ownsRecruitment
                        ? 90
                        : 500;

                if (snapshot.OwnUnits.Count < 3)
                    value += 180;

                utility += value;
                reasons.Add(
                    ownsRecruitment
                        ? "additional recruitment capacity"
                        : "missing recruitment capability");
            }

            if (definition.Category ==
                BuildingCategory.Military)
            {
                utility +=
                    strategy.Posture switch
                    {
                        BotStrategicPosture.ArmyBuildUp => 340,
                        BotStrategicPosture.EmergencyDefense => 180,
                        BotStrategicPosture.Pressure => 120,
                        _ => 0,
                    };

                reasons.Add("military category");
            }

            switch (strategy.Posture)
            {
                case BotStrategicPosture.Opening:
                    if (warehouse || industrial || recruitment)
                        utility += 260;
                    break;

                case BotStrategicPosture.Economy:
                    if (warehouse || industrial)
                        utility += 420;
                    if (recruitment)
                        utility += 80;
                    break;

                case BotStrategicPosture.ArmyBuildUp:
                    if (recruitment)
                        utility += 520;
                    if (industrial)
                        utility += 120;
                    break;

                case BotStrategicPosture.Recovery:
                    if (!ownsWarehouse && warehouse)
                        utility += 360;
                    if (!ownsRecruitment && recruitment)
                        utility += 300;
                    if (newIndustrial)
                        utility += 320;
                    break;

                case BotStrategicPosture.Search:
                    // Search should not completely freeze the economy.
                    if (industrial)
                        utility += 80;
                    break;
            }

            // A tiny deterministic preference prevents endless exact ties while
            // keeping the agent reproducible.
            utility += StableNoise(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                definition.Id,
                _profile.DeterministicNoiseMagnitude);

            if (reasons.Count == 0)
                reasons.Add("generic legal development option");

            return new BotDevelopmentDefinitionScore(
                utility,
                string.Join("; ", reasons));
        }

        private static bool ContainsResource(
            IReadOnlyCollection<string> values,
            string value)
        {
            if (values == null ||
                string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            foreach (string item in values)
            {
                if (string.Equals(
                        item,
                        value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int StableNoise(
            string ownerId,
            long globalTurn,
            string definitionId,
            int magnitude)
        {
            magnitude = Math.Max(0, magnitude);
            if (magnitude == 0)
                return 0;

            unchecked
            {
                uint hash = 2166136261u;
                Append(ref hash, ownerId);
                Append(ref hash, definitionId);

                hash ^= (uint)globalTurn;
                hash *= 16777619u;

                int range = magnitude * 2 + 1;
                return (int)(hash % (uint)range) - magnitude;
            }
        }

        private static void Append(
            ref uint hash,
            string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619u;
            }
        }
    }
}
