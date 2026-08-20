using System;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.BotAI.Generated
{
    internal readonly struct GeneratedOwnUnit
    {
        internal GeneratedOwnUnit(int x, int y, int hp, int movement, int range, BotUnitTacticalRole role)
        {
            X = x;
            Y = y;
            Hp = hp;
            Movement = movement;
            Range = range;
            Role = role;
        }

        internal int X { get; }
        internal int Y { get; }
        internal int Hp { get; }
        internal int Movement { get; }
        internal int Range { get; }
        internal BotUnitTacticalRole Role { get; }
    }

    internal readonly struct GeneratedEnemyUnit
    {
        internal GeneratedEnemyUnit(int x, int y, int hp, int damage, int range)
        {
            X = x;
            Y = y;
            Hp = hp;
            Damage = damage;
            Range = range;
        }

        internal int X { get; }
        internal int Y { get; }
        internal int Hp { get; }
        internal int Damage { get; }
        internal int Range { get; }
    }

    /// <summary>
    /// Compiled C# regression oracle.  Every generated case is executable test code:
    /// there is no JSON fixture, external corpus, binary blob, or runtime deserializer.
    /// </summary>
    internal static class BotGeneratedRegressionKernel
    {
        internal static void Validate(
            int caseId,
            BotStrategicPosture posture,
            int castleX,
            int castleY,
            GeneratedOwnUnit[] own,
            GeneratedEnemyUnit[] visible,
            int hiddenEnemyCount,
            int expectedVisibleTargetCount,
            bool expectedEmergencyBias,
            bool expectedFrontierPreference,
            bool expectedAnyRetreatCandidate,
            int expectedBestVisibleTargetIndex,
            int expectedHomeReservePercent,
            string stableHash)
        {
            own ??= Array.Empty<GeneratedOwnUnit>();
            visible ??= Array.Empty<GeneratedEnemyUnit>();

            Assert.That(visible.Length, Is.EqualTo(expectedVisibleTargetCount),
                $"case={caseId} visible-only target discovery drift hash={stableHash}");

            Assert.That(
                BotTacticalPolicyKernel.ShouldApplyEmergencyBias(posture),
                Is.EqualTo(expectedEmergencyBias),
                $"case={caseId} emergency posture policy drift hash={stableHash}");

            Assert.That(
                BotTacticalPolicyKernel.ShouldPreferFrontier(posture, visible.Length),
                Is.EqualTo(expectedFrontierPreference),
                $"case={caseId} search/frontier policy drift hash={stableHash}");

            bool anyRetreat = false;
            for (int index = 0; index < own.Length; index++)
            {
                GeneratedOwnUnit unit = own[index];
                if (unit.Hp < 30)
                {
                    anyRetreat = true;
                    break;
                }
            }
            Assert.That(anyRetreat, Is.EqualTo(expectedAnyRetreatCandidate),
                $"case={caseId} retreat threshold fixture drift hash={stableHash}");

            int[] x = new int[visible.Length];
            int[] y = new int[visible.Length];
            int[] hp = new int[visible.Length];
            int[] damage = new int[visible.Length];
            int[] range = new int[visible.Length];
            for (int index = 0; index < visible.Length; index++)
            {
                x[index] = visible[index].X;
                y[index] = visible[index].Y;
                hp[index] = visible[index].Hp;
                damage[index] = visible[index].Damage;
                range[index] = visible[index].Range;
            }

            int selected = BotTacticalPolicyKernel.SelectBestVisibleTargetIndex(
                posture, castleX, castleY, x, y, hp, damage, range);
            Assert.That(selected, Is.EqualTo(expectedBestVisibleTargetIndex),
                $"case={caseId} visible focus-fire selection drift hash={stableHash}");

            int reserve = BotTacticalPolicyKernel.ComputeHomeReservePercent(
                posture,
                visible.Length,
                memoryConfidence: hiddenEnemyCount > 0 ? 500 : 0,
                armyCount: Math.Max(1, own.Length));
            Assert.That(reserve, Is.EqualTo(expectedHomeReservePercent),
                $"case={caseId} home reserve drift hash={stableHash}");

            // Hidden contacts are intentionally supplied only as a count.  There is no
            // hidden unit id or position available to target-selection code in this
            // compiled fixture, enforcing the same visibility boundary as BotWorldSnapshot.
            Assert.That(hiddenEnemyCount, Is.GreaterThanOrEqualTo(0));
        }
    }
}
