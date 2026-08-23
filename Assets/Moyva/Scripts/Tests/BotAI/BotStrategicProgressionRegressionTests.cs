using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotStrategicProgressionRegressionTests
    {
        [Test]
        public void FirstOwnedBuilding_EndsOpeningAndDoesNotRetainStale900Score()
        {
            var stall =
                new BotStallTracker(
                    BotPlanningProfile.Normal());

            var planner =
                new BotStrategicPlanner(
                    buildings: null,
                    unitConfigs: null,
                    profile: BotPlanningProfile.Normal(),
                    stall: stall);

            BotStrategicContext opening =
                planner.Plan(
                    Snapshot(
                        globalTurn: 1,
                        buildings:
                            new List<BotBuildingSnapshot>()));

            Assert.That(
                opening.Posture,
                Is.EqualTo(BotStrategicPosture.Opening));

            BotStrategicContext afterCastle =
                planner.Plan(
                    Snapshot(
                        globalTurn: 2,
                        buildings:
                            new List<BotBuildingSnapshot>
                            {
                                new(
                                    "castle",
                                    "bot",
                                    new Vector2Int(5, 5)),
                            }));

            Assert.That(
                afterCastle.Posture,
                Is.Not.EqualTo(BotStrategicPosture.Opening));

            Assert.That(
                afterCastle.Reason,
                Does.Not.Contain(
                    "Hysteresis retained previous posture. Hysteresis retained previous posture."));
        }

        [Test]
        public void StallInvariant_ForcesRecoveryInsteadOfRepeatingOpening()
        {
            var stall =
                new BotStallTracker(
                    BotPlanningProfile.Normal());

            for (long turn = 1; turn <= 3; turn++)
            {
                stall.BeginTurn("bot", turn);
                stall.CompleteTurn("bot", turn);
            }

            var planner =
                new BotStrategicPlanner(
                    buildings: null,
                    unitConfigs: null,
                    profile: BotPlanningProfile.Normal(),
                    stall: stall);

            BotStrategicContext result =
                planner.Plan(
                    Snapshot(
                        globalTurn: 4,
                        buildings:
                            new List<BotBuildingSnapshot>
                            {
                                new(
                                    "castle",
                                    "bot",
                                    new Vector2Int(5, 5)),
                            }));

            Assert.That(
                result.Posture,
                Is.EqualTo(BotStrategicPosture.Recovery));

            Assert.That(
                result.Reason,
                Does.Contain("Anti-stall invariant triggered"));
        }

        private static BotWorldSnapshot Snapshot(
            long globalTurn,
            IReadOnlyList<BotBuildingSnapshot> buildings)
            => new(
                "bot",
                round: 1,
                globalTurn: globalTurn,
                phase: TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                startPosition: new Vector2Int(5, 5),
                ownUnits:
                    new List<BotUnitSnapshot>(),
                visibleEnemyUnits:
                    new List<BotUnitSnapshot>(),
                ownBuildings: buildings,
                readyRecruitmentItems: null,
                memory: null,
                visibleEnemyBuildings: null);
    }
}
