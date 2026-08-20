using System;
using Kruty1918.Moyva.BotAI.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Pure deterministic tactical policy primitives used by the advanced bot planners
    /// and by the generated regression matrix.  The class deliberately contains no
    /// mutation-capable gameplay service and therefore cannot move, attack, recruit,
    /// build, reveal fog, or alter health.
    /// </summary>
    internal static class BotTacticalPolicyKernel
    {
        internal static int Chebyshev(int ax, int ay, int bx, int by)
            => Math.Max(Math.Abs(ax - bx), Math.Abs(ay - by));

        internal static bool ShouldPreferFrontier(
            BotStrategicPosture posture,
            int visibleEnemyCount)
            => posture == BotStrategicPosture.Search && visibleEnemyCount <= 0;

        internal static bool ShouldApplyEmergencyBias(BotStrategicPosture posture)
            => posture == BotStrategicPosture.EmergencyDefense;

        internal static bool ShouldRetreat(
            int currentHealthPercent,
            int configuredThreshold,
            int localThreat,
            int friendlySupport,
            bool castleIsUnderImmediateThreat)
        {
            int hp = Math.Max(0, Math.Min(100, currentHealthPercent));
            int threshold = Math.Max(1, Math.Min(100, configuredThreshold));

            // A defender protecting an immediately threatened capital is allowed to
            // remain in place at slightly lower health; this is a scoring policy,
            // not a mutation or a hidden-information rule.
            int effectiveThreshold = castleIsUnderImmediateThreat
                ? Math.Max(1, threshold - 8)
                : threshold;

            if (hp >= effectiveThreshold)
                return false;

            int threat = Math.Max(0, localThreat);
            int support = Math.Max(0, friendlySupport);
            return threat > support || hp <= Math.Max(1, effectiveThreshold / 2);
        }

        internal static int ScoreVisibleTarget(
            BotStrategicPosture posture,
            int castleX,
            int castleY,
            int enemyX,
            int enemyY,
            int enemyHealthPercent,
            int enemyDamage,
            int enemyRange)
        {
            int distance = Chebyshev(castleX, castleY, enemyX, enemyY);
            int hp = Math.Max(0, Math.Min(100, enemyHealthPercent));
            int damage = Math.Max(0, enemyDamage);
            int range = Math.Max(1, enemyRange);

            long score = 0;
            score += damage * 10L;
            score += (100 - hp) * 3L;
            score += Math.Max(0, 12 - distance) * 20L;
            score += range * 15L;

            if (posture == BotStrategicPosture.EmergencyDefense
                && distance <= Math.Max(6, range + 2))
            {
                score += 1000L;
            }
            else if (posture == BotStrategicPosture.Pressure)
            {
                score += 140L;
            }
            else if (posture == BotStrategicPosture.Siege)
            {
                score += 80L;
            }

            if (score > int.MaxValue)
                return int.MaxValue;
            if (score < int.MinValue)
                return int.MinValue;
            return (int)score;
        }

        internal static int ScoreMoveToAttack(
            BotStrategicPosture posture,
            BotUnitTacticalRole role,
            int movementCostTimes100,
            int targetDistanceAfterMove,
            int targetHealthPercent,
            int localThreat,
            int friendlySupport,
            int formation)
        {
            long score = 800;
            score -= Math.Max(0, movementCostTimes100) / 5L;
            score -= Math.Max(0, targetDistanceAfterMove) * 20L;
            score += Math.Max(0, 100 - Math.Max(0, Math.Min(100, targetHealthPercent))) * 4L;
            score -= Math.Max(0, localThreat);
            score += Math.Max(0, friendlySupport);
            score += formation;

            switch (role)
            {
                case BotUnitTacticalRole.Ranged:
                    score += targetDistanceAfterMove >= 2 ? 180 : -80;
                    break;
                case BotUnitTacticalRole.Frontline:
                    score += targetDistanceAfterMove <= 1 ? 160 : 30;
                    break;
                case BotUnitTacticalRole.FastScout:
                    score += posture == BotStrategicPosture.Search ? -200 : 40;
                    break;
                case BotUnitTacticalRole.Siege:
                    score += posture == BotStrategicPosture.Siege ? 160 : -60;
                    break;
                case BotUnitTacticalRole.Worker:
                    score -= 1000;
                    break;
            }

            if (posture == BotStrategicPosture.EmergencyDefense)
                score += 220;
            else if (posture == BotStrategicPosture.Pressure)
                score += 140;

            return Clamp(score);
        }

        internal static int ComputeHomeReservePercent(
            BotStrategicPosture posture,
            int visibleThreatCount,
            int memoryConfidence,
            int armyCount)
        {
            if (armyCount <= 1)
                return 100;

            int reserve = posture switch
            {
                BotStrategicPosture.EmergencyDefense => 70,
                BotStrategicPosture.Search => 30,
                BotStrategicPosture.Siege => 15,
                BotStrategicPosture.Pressure => 20,
                _ => 25,
            };

            reserve += Math.Min(20, Math.Max(0, visibleThreatCount) * 5);
            reserve += Math.Min(15, Math.Max(0, memoryConfidence) / 100);
            return Math.Max(10, Math.Min(90, reserve));
        }

        internal static int ComputeRequiredHomeDefenders(
            int armyCount,
            int reservePercent)
        {
            if (armyCount <= 0)
                return 0;
            int pct = Math.Max(0, Math.Min(100, reservePercent));
            int count = (armyCount * pct + 99) / 100;
            return Math.Max(1, Math.Min(armyCount, count));
        }

        internal static int ScoreRoleDeficit(
            BotUnitTacticalRole role,
            BotStrategicPosture posture,
            int currentCount,
            int totalCombatUnits)
        {
            int desiredPercent = BotAdvancedHeuristics.DesiredRolePercent(role, posture);
            if (desiredPercent <= 0)
                return 0;

            int total = Math.Max(1, totalCombatUnits);
            int current = Math.Max(0, currentCount);
            int desiredScaled = desiredPercent * total;
            int currentScaled = current * 100;
            return Math.Max(0, desiredScaled - currentScaled);
        }

        internal static int ScoreCounterValue(
            int expectedDamageAgainstEnemy,
            int expectedDamageTaken,
            int trainingTurns,
            int resourcePressurePercent)
        {
            long value = Math.Max(0, expectedDamageAgainstEnemy) * 20L;
            value -= Math.Max(0, expectedDamageTaken) * 12L;
            value -= Math.Max(1, trainingTurns) * 20L;
            value -= Math.Max(0, Math.Min(100, resourcePressurePercent)) * 4L;
            return Clamp(value);
        }

        internal static int ScoreFrontier(
            int unknownCells,
            int movementCostTimes100,
            int localThreat,
            BotUnitTacticalRole role,
            bool sectorReserved)
        {
            if (unknownCells <= 0 || sectorReserved)
                return int.MinValue;

            long score = 450L;
            score += Math.Max(0, unknownCells) * 30L;
            score -= Math.Max(0, movementCostTimes100) / 10L;
            score -= Math.Max(0, localThreat);
            if (role == BotUnitTacticalRole.FastScout)
                score += 250L;
            else if (role == BotUnitTacticalRole.Ranged)
                score += 40L;
            else if (role == BotUnitTacticalRole.Siege || role == BotUnitTacticalRole.Worker)
                score -= 500L;
            return Clamp(score);
        }

        internal static int ScoreFormation(
            BotUnitTacticalRole movingRole,
            int distanceToNearestFrontline,
            int distanceToNearestRanged,
            int distanceToNearestEnemy)
        {
            int formation = 0;
            if (movingRole == BotUnitTacticalRole.Ranged)
            {
                if (distanceToNearestFrontline <= 2)
                    formation += 180;
                if (distanceToNearestEnemy <= 1)
                    formation -= 220;
            }
            else if (movingRole == BotUnitTacticalRole.Frontline)
            {
                if (distanceToNearestRanged <= 2)
                    formation += 140;
                if (distanceToNearestEnemy <= 1)
                    formation += 120;
            }
            else if (movingRole == BotUnitTacticalRole.Support)
            {
                if (distanceToNearestFrontline <= 2)
                    formation += 100;
            }
            return formation;
        }

        internal static int ScoreCriticalInfrastructureThreat(
            bool isCastle,
            bool isRecruitment,
            bool isDefense,
            bool isStorage,
            bool isProduction,
            int turnsToThreat)
        {
            int baseValue = isCastle ? 10000
                : isRecruitment ? 7500
                : isDefense ? 6000
                : isProduction ? 5500
                : isStorage ? 4000
                : 1800;

            int eta = Math.Max(0, turnsToThreat);
            int urgency = eta switch
            {
                0 => 5000,
                1 => 3000,
                2 => 1500,
                3 => 700,
                _ => 0,
            };

            return baseValue + urgency;
        }

        internal static int SelectBestVisibleTargetIndex(
            BotStrategicPosture posture,
            int castleX,
            int castleY,
            int[] enemyX,
            int[] enemyY,
            int[] enemyHealthPercent,
            int[] enemyDamage,
            int[] enemyRange)
        {
            if (enemyX == null || enemyY == null || enemyHealthPercent == null
                || enemyDamage == null || enemyRange == null)
                return -1;

            int count = Math.Min(
                Math.Min(enemyX.Length, enemyY.Length),
                Math.Min(enemyHealthPercent.Length, Math.Min(enemyDamage.Length, enemyRange.Length)));
            if (count <= 0)
                return -1;

            int bestIndex = -1;
            int bestScore = int.MinValue;
            for (int index = 0; index < count; index++)
            {
                int score = ScoreVisibleTarget(
                    posture,
                    castleX,
                    castleY,
                    enemyX[index],
                    enemyY[index],
                    enemyHealthPercent[index],
                    enemyDamage[index],
                    enemyRange[index]);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = index;
                }
            }
            return bestIndex;
        }

        private static int Clamp(long value)
            => value > int.MaxValue ? int.MaxValue
                : value < int.MinValue ? int.MinValue
                : (int)value;
    }
}
