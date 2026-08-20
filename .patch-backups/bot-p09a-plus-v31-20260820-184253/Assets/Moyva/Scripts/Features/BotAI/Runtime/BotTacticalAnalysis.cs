using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal static class BotTacticalAnalysis
    {
        internal static bool TryResolveOwnCastle(
            BotWorldSnapshot snapshot,
            IBuildingRegistry buildings,
            out BotBuildingSnapshot castle)
        {
            castle = default;
            if (snapshot == null || buildings == null || snapshot.OwnBuildings == null)
                return false;

            bool found = false;
            for (int index = 0; index < snapshot.OwnBuildings.Count; index++)
            {
                BotBuildingSnapshot candidate = snapshot.OwnBuildings[index];
                if (string.IsNullOrWhiteSpace(candidate.BuildingId)
                    || !string.Equals(candidate.OwnerId, snapshot.OwnerId, StringComparison.Ordinal))
                {
                    continue;
                }

                BuildingDefinition definition = buildings.GetById(candidate.BuildingId);
                if (!BuildingDefinitionCapabilities.IsCastle(definition))
                    continue;

                if (!found || CompareCastle(candidate, castle) < 0)
                {
                    castle = candidate;
                    found = true;
                }
            }

            return found;
        }

        internal static BotDefenseContext BuildDefenseContext(
            BotWorldSnapshot snapshot,
            IBuildingRegistry buildings,
            IUnitClassConfig configs,
            BotPlanningProfile profile)
        {
            profile ??= BotPlanningProfile.Normal();
            if (snapshot == null
                || !TryResolveOwnCastle(snapshot, buildings, out BotBuildingSnapshot castle))
            {
                return new BotDefenseContext(
                    false,
                    default,
                    0,
                    Array.Empty<BotThreatSnapshot>());
            }

            var threats = new List<BotThreatSnapshot>();
            long total = 0L;

            // P09A anti-cheat invariant: this is the only enemy discovery source.
            for (int index = 0; index < snapshot.VisibleEnemyUnits.Count; index++)
            {
                BotUnitSnapshot enemy = snapshot.VisibleEnemyUnits[index];
                if (string.IsNullOrWhiteSpace(enemy.UnitId)
                    || string.Equals(enemy.OwnerId, snapshot.OwnerId, StringComparison.Ordinal))
                {
                    continue;
                }

                UnitClassConfig config = SafeGetConfig(configs, enemy.TypeId);
                int attackRange = Math.Max(1, config?.AttackRange ?? 1);
                int movementEstimate = Mathf.Max(
                    0,
                    Mathf.CeilToInt(config?.MovementPointsPerTurn ?? 0f));
                int rawThreatDamage = EstimateRawThreatDamage(config);
                int distance = Chebyshev(enemy.Position, castle.Position);
                bool immediate = distance <= attackRange;
                bool nextTurn = !immediate && distance <= attackRange + movementEstimate;

                long score = (long)rawThreatDamage * profile.ThreatDamageWeight;
                if (immediate)
                    score += profile.ImmediateCastleThreatWeight;
                else if (nextTurn)
                    score += profile.NextTurnCastleThreatWeight;

                int distanceFactor = Math.Max(0, (profile.HomeDefenseRadius * 2) - distance);
                score += (long)distanceFactor * profile.ThreatDistanceWeight;

                if (rawThreatDamage <= 0)
                    score /= 4;

                int normalizedScore = ClampToNonNegativeInt(score);
                threats.Add(new BotThreatSnapshot(
                    enemy.UnitId,
                    enemy.Position,
                    attackRange,
                    rawThreatDamage,
                    distance,
                    immediate,
                    nextTurn,
                    normalizedScore));
                total += normalizedScore;
            }

            threats.Sort(CompareThreat);
            return new BotDefenseContext(
                true,
                castle.Position,
                ClampToNonNegativeInt(total),
                threats);
        }

        internal static UnitClassConfig SafeGetConfig(IUnitClassConfig configs, string typeId)
        {
            if (configs == null || string.IsNullOrWhiteSpace(typeId))
                return null;

            try
            {
                return configs.GetConfig(typeId);
            }
            catch
            {
                return null;
            }
        }

        internal static int EstimateRawThreatDamage(UnitClassConfig config)
        {
            if (config == null)
                return 1;

            long value =
                (long)Math.Max(0, config.CuttingDamage)
                + Math.Max(0, config.PenetratingDamage)
                + Math.Max(0, config.CrushingDamage);
            return ClampToNonNegativeInt(value);
        }

        internal static int Chebyshev(Vector2Int a, Vector2Int b)
            => Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));

        private static int CompareThreat(BotThreatSnapshot left, BotThreatSnapshot right)
        {
            int score = right.ThreatScore.CompareTo(left.ThreatScore);
            if (score != 0)
                return score;

            int distance = left.DistanceToCastle.CompareTo(right.DistanceToCastle);
            return distance != 0
                ? distance
                : string.CompareOrdinal(left.EnemyUnitId, right.EnemyUnitId);
        }

        private static int CompareCastle(BotBuildingSnapshot left, BotBuildingSnapshot right)
        {
            int x = left.Position.x.CompareTo(right.Position.x);
            if (x != 0)
                return x;

            int y = left.Position.y.CompareTo(right.Position.y);
            return y != 0
                ? y
                : string.CompareOrdinal(left.BuildingId, right.BuildingId);
        }

        private static int ClampToNonNegativeInt(long value)
        {
            if (value <= 0)
                return 0;
            return value >= int.MaxValue ? int.MaxValue : (int)value;
        }
    }
}
