using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotCombatDefensePlannerTests
    {
        private const string BotOwner = "bot-a";
        private const string EnemyOwner = "human-a";

        [Test]
        public void Combat_NoVisibleEnemies_ReturnsNoAttack()
        {
            var combat = new TestUnitCombatService("hidden", 25, false);
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions, Is.Empty);
            Assert.That(combat.GetAttackableTargetsCalls, Is.EqualTo(0));
            Assert.That(combat.TryAttackCalls, Is.EqualTo(0));
        }

        [Test]
        public void Combat_HiddenEnemyNeverAppearsAsAttackCandidate()
        {
            var combat = new TestUnitCombatService("hidden-global-enemy", 25, false);
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: Array.Empty<BotUnitSnapshot>()),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions, Is.Empty);
            Assert.That(
                combat.GetAttackableTargetsCalls,
                Is.EqualTo(0),
                "Bot combat discovery must never enumerate the global attackable-target list.");
        }

        [Test]
        public void Combat_LegalVisibleEnemy_ReturnsAttackCandidate()
        {
            var combat = new TestUnitCombatService("enemy", 30, false);
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("enemy", EnemyOwner, Vector2Int.right),
                    }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions.Count, Is.EqualTo(1));
            Assert.That(actions[0].Kind, Is.EqualTo(BotActionKind.Attack));
            Assert.That(actions[0].ActorId, Is.EqualTo("a"));
            Assert.That(actions[0].TargetId, Is.EqualTo("enemy"));
        }

        [Test]
        public void Combat_FriendlyUnitNeverBecomesTarget()
        {
            var combat = new TestUnitCombatService("friendly", 30, false);
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("friendly", BotOwner, Vector2Int.right),
                    }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions, Is.Empty);
        }

        [Test]
        public void Combat_OutOfRangeVisibleEnemy_NoAttackCandidate()
        {
            var combat = new TestUnitCombatService();
            combat.Reject("a", "enemy", UnitAttackRejectReason.TargetOutOfRange);
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("enemy", EnemyOwner, new Vector2Int(9, 9)),
                    }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions, Is.Empty);
        }

        [Test]
        public void Combat_LethalAttackScoresAboveEquivalentNonLethal()
        {
            var combat = new TestUnitCombatService();
            combat.Allow("a", "lethal", damage: 30, hp: 20);
            combat.Allow("a", "nonlethal", damage: 30, hp: 80);
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("nonlethal", EnemyOwner, new Vector2Int(1, 1)),
                        Unit("lethal", EnemyOwner, new Vector2Int(1, 0)),
                    }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions.Count, Is.EqualTo(2));
            Assert.That(actions[0].TargetId, Is.EqualTo("lethal"));
            Assert.That(actions[0].Score.Total, Is.GreaterThan(actions[1].Score.Total));
            Assert.That(actions[0].Score.Explanation, Is.EqualTo("Legal lethal attack."));
        }

        [Test]
        public void Combat_CastleThreatGetsEmergencyBonus()
        {
            var registry = new TestBuildingRegistry(Castle("royal-keep"));
            var configs = new TestUnitConfigRegistry(
                MilitaryConfig("warrior", attackRange: 1, move: 4, damage: 10));
            var combat = new TestUnitCombatService();
            combat.Allow("a", "near", damage: 20, hp: 100);
            combat.Allow("a", "far", damage: 20, hp: 100);

            var planner = new BotCombatPlanner(
                combat,
                registry,
                configs,
                BotPlanningProfile.Normal());

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[]
                    {
                        Unit("a", BotOwner, new Vector2Int(2, 0)),
                    },
                    visibleEnemies: new[]
                    {
                        Unit("far", EnemyOwner, new Vector2Int(8, 8), "warrior"),
                        Unit("near", EnemyOwner, new Vector2Int(1, 0), "warrior"),
                    },
                    ownBuildings: new[]
                    {
                        new BotBuildingSnapshot(
                            "royal-keep",
                            BotOwner,
                            Vector2Int.zero),
                    }),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(actions.Count, Is.EqualTo(2));
            Assert.That(actions[0].TargetId, Is.EqualTo("near"));
            Assert.That(actions[0].Score.Total, Is.GreaterThan(actions[1].Score.Total));
        }

        [Test]
        public void Combat_DeterministicOrderForEqualScores()
        {
            var combat = new TestUnitCombatService();
            combat.Allow("a", "enemy-b", 20, 100);
            combat.Allow("a", "enemy-a", 20, 100);
            var planner = new BotCombatPlanner(combat);

            BotWorldSnapshot snapshot = Snapshot(
                ownUnits: new[]
                {
                    Unit("a", BotOwner, Vector2Int.zero),
                },
                visibleEnemies: new[]
                {
                    Unit("enemy-b", EnemyOwner, Vector2Int.right),
                    Unit("enemy-a", EnemyOwner, Vector2Int.up),
                });

            string[] first = planner
                .Generate(snapshot, Strategy(BotStrategicPosture.Pressure))
                .Select(x => x.CandidateId)
                .ToArray();
            string[] second = planner
                .Generate(snapshot, Strategy(BotStrategicPosture.Pressure))
                .Select(x => x.CandidateId)
                .ToArray();

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Combat_AlreadyAttackedUnitProducesNoLegalAttack()
        {
            var combat = new TestUnitCombatService();
            combat.Allow("a", "enemy", 20, 100);
            combat.BlockAttacker("a");
            var planner = new BotCombatPlanner(combat);

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("enemy", EnemyOwner, Vector2Int.right),
                    }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(actions, Is.Empty);
        }

        [Test]
        public void Combat_PlanningNeverExecutesCombat()
        {
            var combat = new TestUnitCombatService("enemy", 20, false);
            var planner = new BotCombatPlanner(combat);

            planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("enemy", EnemyOwner, Vector2Int.right),
                    }),
                Strategy(BotStrategicPosture.Pressure));

            Assert.That(combat.TryAttackCalls, Is.EqualTo(0));
        }

        [Test]
        public void Defense_NoCastle_ReturnsNoBaseDefenseMove()
        {
            var planner = DefensePlanner(
                new UnitMovementTileSnapshot(
                    Vector2Int.right,
                    true,
                    1f));

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    visibleEnemies: new[]
                    {
                        Unit("enemy", EnemyOwner, new Vector2Int(2, 0)),
                    }),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(actions, Is.Empty);
        }

        [Test]
        public void Defense_NoVisibleThreat_ReturnsNoEmergencyIntercept()
        {
            var planner = DefensePlanner(
                new UnitMovementTileSnapshot(
                    Vector2Int.right,
                    true,
                    1f));

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                Snapshot(
                    ownUnits: new[] { Unit("a", BotOwner, Vector2Int.zero) },
                    ownBuildings: new[]
                    {
                        new BotBuildingSnapshot(
                            "royal-keep",
                            BotOwner,
                            Vector2Int.zero),
                    }),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(actions, Is.Empty);
        }

        [Test]
        public void Defense_ThreatNearCastle_GeneratesInterceptMove()
        {
            var planner = DefensePlanner(
                new UnitMovementTileSnapshot(
                    new Vector2Int(1, 0),
                    true,
                    1f));

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                DefensiveSnapshot(new Vector2Int(2, 0)),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(actions.Count, Is.EqualTo(1));
            Assert.That(actions[0].Kind, Is.EqualTo(BotActionKind.Move));
            Assert.That(actions[0].Reason, Is.EqualTo("emergency-intercept"));
            Assert.That(actions[0].TargetCell, Is.EqualTo(new Vector2Int(1, 0)));
        }

        [Test]
        public void Defense_InterceptTargetComesOnlyFromCanonicalReachableTiles()
        {
            var planner = DefensePlanner(
                new UnitMovementTileSnapshot(
                    new Vector2Int(1, 0),
                    false,
                    0.1f,
                    "blocked"),
                new UnitMovementTileSnapshot(
                    new Vector2Int(0, 1),
                    true,
                    2f));

            IReadOnlyList<BotActionCandidate> actions = planner.Generate(
                DefensiveSnapshot(new Vector2Int(2, 0)),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(actions.Count, Is.EqualTo(1));
            Assert.That(
                actions[0].TargetCell,
                Is.EqualTo(new Vector2Int(0, 1)));
        }

        [Test]
        public void Defense_HiddenEnemyCannotCreateThreat()
        {
            var planner = DefensePlanner(
                new UnitMovementTileSnapshot(
                    new Vector2Int(1, 0),
                    true,
                    1f));

            BotWorldSnapshot snapshot = Snapshot(
                ownUnits: new[]
                {
                    Unit("a", BotOwner, Vector2Int.zero),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot(
                        "royal-keep",
                        BotOwner,
                        Vector2Int.zero),
                },
                visibleEnemies: Array.Empty<BotUnitSnapshot>());

            BotDefenseContext context = planner.Analyze(
                snapshot,
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(context.Threats, Is.Empty);
            Assert.That(
                planner.Generate(
                    snapshot,
                    Strategy(BotStrategicPosture.EmergencyDefense)),
                Is.Empty);
        }

        [Test]
        public void Defense_CloserCastleThreatScoresHigher()
        {
            var registry = new TestBuildingRegistry(Castle("royal-keep"));
            var configs = new TestUnitConfigRegistry(
                MilitaryConfig("warrior", 1, 4, 10));

            var planner = new BotDefensePlanner(
                registry,
                new TestMovementQuery(),
                configs,
                BotPlanningProfile.Normal());

            BotDefenseContext context = planner.Analyze(
                Snapshot(
                    ownBuildings: new[]
                    {
                        new BotBuildingSnapshot(
                            "royal-keep",
                            BotOwner,
                            Vector2Int.zero),
                    },
                    visibleEnemies: new[]
                    {
                        Unit("far", EnemyOwner, new Vector2Int(10, 0), "warrior"),
                        Unit("near", EnemyOwner, new Vector2Int(2, 0), "warrior"),
                    }),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(context.Threats.Count, Is.EqualTo(2));
            Assert.That(context.Threats[0].EnemyUnitId, Is.EqualTo("near"));
            Assert.That(
                context.Threats[0].ThreatScore,
                Is.GreaterThan(context.Threats[1].ThreatScore));
        }

        [Test]
        public void Defense_CandidateOrderIsDeterministic()
        {
            var movement = new TestMovementQuery();
            movement.Set(
                "a",
                new UnitMovementTileSnapshot(
                    new Vector2Int(1, 0),
                    true,
                    1f));
            movement.Set(
                "b",
                new UnitMovementTileSnapshot(
                    new Vector2Int(0, 1),
                    true,
                    1f));

            var planner = new BotDefensePlanner(
                new TestBuildingRegistry(Castle("royal-keep")),
                movement,
                new TestUnitConfigRegistry(
                    MilitaryConfig("warrior", 1, 4, 10)),
                BotPlanningProfile.Normal());

            BotWorldSnapshot snapshot = Snapshot(
                ownUnits: new[]
                {
                    Unit("b", BotOwner, new Vector2Int(0, 1)),
                    Unit("a", BotOwner, Vector2Int.zero),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot(
                        "royal-keep",
                        BotOwner,
                        Vector2Int.zero),
                },
                visibleEnemies: new[]
                {
                    Unit("enemy", EnemyOwner, new Vector2Int(3, 0), "warrior"),
                });

            string[] first = planner
                .Generate(
                    snapshot,
                    Strategy(BotStrategicPosture.EmergencyDefense))
                .Select(x => x.CandidateId)
                .ToArray();
            string[] second = planner
                .Generate(
                    snapshot,
                    Strategy(BotStrategicPosture.EmergencyDefense))
                .Select(x => x.CandidateId)
                .ToArray();

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Defense_CastleResolvedThroughCapabilities_NotIdHardcode()
        {
            var registry = new TestBuildingRegistry(
                Castle("definitely-not-castle-id"));

            var planner = new BotDefensePlanner(
                registry,
                new TestMovementQuery(),
                new TestUnitConfigRegistry(),
                BotPlanningProfile.Normal());

            BotDefenseContext context = planner.Analyze(
                Snapshot(
                    ownBuildings: new[]
                    {
                        new BotBuildingSnapshot(
                            "definitely-not-castle-id",
                            BotOwner,
                            new Vector2Int(3, 4)),
                    }),
                Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(context.HasCastle, Is.True);
            Assert.That(
                context.CastlePosition,
                Is.EqualTo(new Vector2Int(3, 4)));
        }

        [Test]
        public void Defense_HomeGuardSelectsClosestMilitaryUnitDeterministically()
        {
            var registry = new TestBuildingRegistry(Castle("royal-keep"));
            var configs = new TestUnitConfigRegistry(
                MilitaryConfig("warrior", 1, 4, 10));

            var planner = new BotDefensePlanner(
                registry,
                new TestMovementQuery(),
                configs,
                BotPlanningProfile.Normal());

            IReadOnlyList<string> guards =
                planner.GetProtectedHomeGuardUnitIds(
                    Snapshot(
                        ownUnits: new[]
                        {
                            Unit("far", BotOwner, new Vector2Int(8, 0), "warrior"),
                            Unit("near", BotOwner, new Vector2Int(1, 0), "warrior"),
                            Unit("mid", BotOwner, new Vector2Int(4, 0), "warrior"),
                        },
                        ownBuildings: new[]
                        {
                            new BotBuildingSnapshot(
                                "royal-keep",
                                BotOwner,
                                Vector2Int.zero),
                        }),
                    Strategy(BotStrategicPosture.Pressure));

            Assert.That(guards, Is.EqualTo(new[] { "near" }));
        }

        [Test]
        public void TurnPlanner_EmergencyAttackRanksAboveOrdinaryMove()
        {
            var attack = Candidate(
                "attack:a:enemy",
                BotActionKind.Attack,
                100,
                "a",
                "enemy");

            var ordinaryMove = Candidate(
                "move:a:1,0",
                BotActionKind.Move,
                2000,
                "a",
                null,
                Vector2Int.right);

            var planner = new BotTurnPlanner(
                combat: new StaticCombatPlanner(attack),
                movement: new StaticMovementPlanner(ordinaryMove));

            IReadOnlyList<BotActionCandidate> result =
                planner.GenerateCandidates(
                    Snapshot(),
                    Strategy(BotStrategicPosture.EmergencyDefense));

            Assert.That(result[0].Kind, Is.EqualTo(BotActionKind.Attack));
        }

        [Test]
        public void TurnPlanner_HomeGuardSuppressesOrdinaryPressureMove()
        {
            var moveGuard = Candidate(
                "move:guard:3,0",
                BotActionKind.Move,
                1000,
                "guard",
                null,
                new Vector2Int(3, 0));

            var moveArmy = Candidate(
                "move:army:3,0",
                BotActionKind.Move,
                900,
                "army",
                null,
                new Vector2Int(3, 0));

            var defense = new StaticDefensePlanner(
                protectedIds: new[] { "guard" });

            var planner = new BotTurnPlanner(
                movement: new StaticMovementPlanner(
                    moveGuard,
                    moveArmy),
                defense: defense);

            IReadOnlyList<BotActionCandidate> result =
                planner.GenerateCandidates(
                    Snapshot(),
                    Strategy(BotStrategicPosture.Pressure));

            Assert.That(
                result.Select(x => x.ActorId),
                Is.EqualTo(new[] { "army" }));
        }

        [Test]
        public void PlannerToExecutor_AttackUsesSharedCombatCommand()
        {
            var combatRead = new TestUnitCombatService("enemy", 25, false);
            var combatPlanner = new BotCombatPlanner(combatRead);
            var turnPlanner = new BotTurnPlanner(combat: combatPlanner);
            var command = new TestCombatCommandService();
            var executor = new BotActionExecutor(combat: command);

            BotWorldSnapshot snapshot = Snapshot(
                ownUnits: new[]
                {
                    Unit("a", BotOwner, Vector2Int.zero),
                },
                visibleEnemies: new[]
                {
                    Unit("enemy", EnemyOwner, Vector2Int.right),
                });

            BotActionCandidate action =
                turnPlanner.GenerateCandidates(
                    snapshot,
                    Strategy(BotStrategicPosture.Pressure))[0];

            BotActionExecutionResult result =
                executor.ExecuteAsync(
                    BotOwner,
                    action,
                    CancellationToken.None).Result;

            Assert.That(result.Succeeded, Is.True);
            Assert.That(command.ExecuteCalls, Is.EqualTo(1));
            Assert.That(command.LastAttacker, Is.EqualTo("a"));
            Assert.That(command.LastTarget, Is.EqualTo("enemy"));
            Assert.That(
                combatRead.TryAttackCalls,
                Is.EqualTo(0),
                "Planning must not execute the canonical combat service.");
        }

        [Test]
        public void StrategicPlanner_UsesActualDataDrivenCastleForEmergencyDefense()
        {
            var registry = new TestBuildingRegistry(Castle("royal-keep"));
            var configs = new TestUnitConfigRegistry(
                MilitaryConfig("warrior", attackRange: 1, move: 4, damage: 10));
            var planner = new BotStrategicPlanner(
                registry,
                configs,
                BotPlanningProfile.Normal());

            var snapshot = new BotWorldSnapshot(
                BotOwner,
                round: 1,
                globalTurn: 10,
                Kruty1918.Moyva.Turns.API.TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                startPosition: new Vector2Int(-30, -30),
                ownUnits: new[]
                {
                    Unit("guard", BotOwner, new Vector2Int(21, 20), "warrior"),
                },
                visibleEnemyUnits: new[]
                {
                    Unit("enemy", EnemyOwner, new Vector2Int(20, 21), "warrior"),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot(
                        "royal-keep",
                        BotOwner,
                        new Vector2Int(20, 20)),
                },
                readyRecruitmentItems: Array.Empty<UnitRecruitmentQueueItemSnapshot>(),
                memory: Array.Empty<BotKnownEntityMemory>());

            BotStrategicContext result = planner.Plan(snapshot);

            Assert.That(result.Posture, Is.EqualTo(BotStrategicPosture.EmergencyDefense));
            StringAssert.Contains("data-driven Castle", result.Reason);
        }

        [Test]
        public void StrategicPlanner_FarVisibleEnemyDoesNotBecomeEmergencyOnlyBecauseItExists()
        {
            var registry = new TestBuildingRegistry(Castle("royal-keep"));
            var configs = new TestUnitConfigRegistry(
                MilitaryConfig("warrior", attackRange: 1, move: 2, damage: 10));
            var planner = new BotStrategicPlanner(
                registry,
                configs,
                BotPlanningProfile.Normal());

            BotStrategicContext result = planner.Plan(
                Snapshot(
                    ownUnits: new[]
                    {
                        Unit("a", BotOwner, Vector2Int.zero, "warrior"),
                        Unit("b", BotOwner, Vector2Int.right, "warrior"),
                        Unit("c", BotOwner, Vector2Int.up, "warrior"),
                    },
                    visibleEnemies: new[]
                    {
                        Unit("enemy", EnemyOwner, new Vector2Int(30, 30), "warrior"),
                    },
                    ownBuildings: new[]
                    {
                        new BotBuildingSnapshot("royal-keep", BotOwner, Vector2Int.zero),
                    }));

            Assert.That(result.Posture, Is.Not.EqualTo(BotStrategicPosture.EmergencyDefense));
        }

        [Test]
        public void PlanningProfile_DefenseDefaultsAreBounded()
        {
            BotPlanningProfile profile = BotPlanningProfile.Normal();

            Assert.That(profile.HomeDefenseRadius, Is.GreaterThan(0));
            Assert.That(profile.MinimumHomeDefenders, Is.GreaterThanOrEqualTo(0));
            Assert.That(profile.MaxDefendersToEvaluate, Is.InRange(1, 16));
            Assert.That(
                profile.ImmediateCastleThreatWeight,
                Is.GreaterThan(profile.NextTurnCastleThreatWeight));
            Assert.That(profile.EmergencyThreatThreshold, Is.GreaterThan(0));
        }

        private static BotDefensePlanner DefensePlanner(
            params UnitMovementTileSnapshot[] reachable)
        {
            var movement = new TestMovementQuery();
            movement.Set("a", reachable);

            return new BotDefensePlanner(
                new TestBuildingRegistry(Castle("royal-keep")),
                movement,
                new TestUnitConfigRegistry(
                    MilitaryConfig("warrior", 1, 4, 10)),
                BotPlanningProfile.Normal());
        }

        private static BotWorldSnapshot DefensiveSnapshot(
            Vector2Int enemyPosition)
            => Snapshot(
                ownUnits: new[]
                {
                    Unit("a", BotOwner, Vector2Int.zero, "warrior"),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot(
                        "royal-keep",
                        BotOwner,
                        Vector2Int.zero),
                },
                visibleEnemies: new[]
                {
                    Unit("enemy", EnemyOwner, enemyPosition, "warrior"),
                });

        private static BotWorldSnapshot Snapshot(
            IReadOnlyList<BotUnitSnapshot> ownUnits = null,
            IReadOnlyList<BotUnitSnapshot> visibleEnemies = null,
            IReadOnlyList<BotBuildingSnapshot> ownBuildings = null)
            => new(
                BotOwner,
                round: 1,
                globalTurn: 10,
                Kruty1918.Moyva.Turns.API.TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                Vector2Int.zero,
                ownUnits ?? Array.Empty<BotUnitSnapshot>(),
                visibleEnemies ?? Array.Empty<BotUnitSnapshot>(),
                ownBuildings ?? Array.Empty<BotBuildingSnapshot>(),
                Array.Empty<UnitRecruitmentQueueItemSnapshot>(),
                Array.Empty<BotKnownEntityMemory>());

        private static BotUnitSnapshot Unit(
            string id,
            string owner,
            Vector2Int position,
            string typeId = "warrior")
            => new(
                id,
                owner,
                typeId,
                position,
                stamina: 5f);

        private static BotStrategicContext Strategy(
            BotStrategicPosture posture)
            => new(
                BotOwner,
                globalTurn: 10,
                posture,
                postureScore: 500,
                reason: "test");

        private static BuildingDefinition Castle(string id)
            => new()
            {
                Id = id,
                Category = BuildingCategory.Military,
                Modules = new List<BuildingModuleDefinition>
                {
                    new CastleBuildingModule
                    {
                        IsCapital = true,
                        IsEnabled = true,
                    },
                },
            };

        private static UnitClassConfig MilitaryConfig(
            string typeId,
            int attackRange,
            float move,
            int damage)
            => new()
            {
                TypeId = typeId,
                Role = UnitRole.Military,
                AttackRange = attackRange,
                MovementPointsPerTurn = move,
                HitPoints = 100,
                CuttingDamage = damage,
            };

        private static BotActionCandidate Candidate(
            string id,
            BotActionKind kind,
            int score,
            string actor = null,
            string target = null,
            Vector2Int? cell = null)
            => new(
                id,
                kind,
                BotStrategicPosture.Pressure,
                new BotActionScore(score, "test"),
                actorId: actor,
                targetId: target,
                targetCell: cell,
                reason: "test");

        private sealed class TestBuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition[] _definitions;

            public TestBuildingRegistry(
                params BuildingDefinition[] definitions)
            {
                _definitions =
                    definitions
                    ?? Array.Empty<BuildingDefinition>();
            }

            public BuildingDefinition[] GetAll()
                => _definitions;

            public BuildingDefinition GetById(string id)
                => _definitions.FirstOrDefault(
                    x =>
                        x != null
                        && string.Equals(
                            x.Id,
                            id,
                            StringComparison.Ordinal));

            public BuildingDefinition[] GetByCategory(
                BuildingCategory category)
                => _definitions
                    .Where(
                        x =>
                            x != null
                            && x.Category == category)
                    .ToArray();

            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();

            public WallCollectionDefinition
                GetWallCollectionByBuildingId(
                    string buildingId)
                => null;
        }

        private sealed class TestUnitConfigRegistry : IUnitClassConfig
        {
            private readonly Dictionary<string, UnitClassConfig>
                _configs =
                    new(StringComparer.Ordinal);

            public TestUnitConfigRegistry(
                params UnitClassConfig[] configs)
            {
                if (configs == null)
                    return;

                foreach (UnitClassConfig config in configs)
                {
                    if (config != null
                        && !string.IsNullOrWhiteSpace(
                            config.TypeId))
                    {
                        _configs[config.TypeId] = config;
                    }
                }
            }

            public UnitClassConfig GetConfig(string typeId)
                => typeId != null
                    && _configs.TryGetValue(
                        typeId,
                        out UnitClassConfig config)
                    ? config
                    : null;
        }

        private sealed class TestMovementQuery : IUnitMovementQuery
        {
            private readonly Dictionary<
                string,
                IReadOnlyList<UnitMovementTileSnapshot>> _tiles =
                    new(StringComparer.Ordinal);

            public void Set(
                string unitId,
                params UnitMovementTileSnapshot[] tiles)
                => _tiles[unitId] =
                    tiles
                    ?? Array.Empty<UnitMovementTileSnapshot>();

            public IReadOnlyList<UnitMovementTileSnapshot>
                GetMovementTiles(string unitId)
                => unitId != null
                    && _tiles.TryGetValue(
                        unitId,
                        out IReadOnlyList<
                            UnitMovementTileSnapshot> value)
                    ? value
                    : Array.Empty<UnitMovementTileSnapshot>();
        }

        private sealed class StaticCombatPlanner : IBotCombatPlanner
        {
            private readonly IReadOnlyList<BotActionCandidate> _items;

            public StaticCombatPlanner(
                params BotActionCandidate[] items)
            {
                _items = items;
            }

            public IReadOnlyList<BotActionCandidate> Generate(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
                => _items;
        }

        private sealed class StaticMovementPlanner : IBotMovementPlanner
        {
            private readonly IReadOnlyList<BotActionCandidate> _items;

            public StaticMovementPlanner(
                params BotActionCandidate[] items)
            {
                _items = items;
            }

            public IReadOnlyList<BotActionCandidate> Generate(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
                => _items;
        }

        private sealed class StaticDefensePlanner : IBotDefensePlanner
        {
            private readonly IReadOnlyList<string> _protected;
            private readonly IReadOnlyList<BotActionCandidate> _actions;

            public StaticDefensePlanner(
                IReadOnlyList<string> protectedIds = null,
                params BotActionCandidate[] actions)
            {
                _protected =
                    protectedIds
                    ?? Array.Empty<string>();
                _actions =
                    actions
                    ?? Array.Empty<BotActionCandidate>();
            }

            public BotDefenseContext Analyze(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
                => new(
                    false,
                    default,
                    0,
                    Array.Empty<BotThreatSnapshot>());

            public IReadOnlyList<BotActionCandidate> Generate(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
                => _actions;

            public IReadOnlyList<string>
                GetProtectedHomeGuardUnitIds(
                    BotWorldSnapshot snapshot,
                    BotStrategicContext strategy)
                => _protected;
        }

        private sealed class TestCombatCommandService :
            ICombatCommandService
        {
            public int ExecuteCalls { get; private set; }
            public string LastAttacker { get; private set; }
            public string LastTarget { get; private set; }

            public bool TryPreview(
                string attackerEntityId,
                string targetEntityId,
                out CombatCommandPreview preview,
                out string reason)
            {
                preview = new CombatCommandPreview(
                    attackerEntityId,
                    targetEntityId,
                    10,
                    false);
                reason = null;
                return true;
            }

            public Task<CombatCommandResult> ExecuteAsync(
                string requesterOwnerId,
                string attackerEntityId,
                string targetEntityId,
                CancellationToken token = default)
            {
                ExecuteCalls++;
                LastAttacker = attackerEntityId;
                LastTarget = targetEntityId;

                return Task.FromResult(
                    new CombatCommandResult(
                        true,
                        attackerEntityId,
                        targetEntityId,
                        10,
                        false,
                        null));
            }
        }
    }

    internal sealed class TestUnitCombatService : IUnitCombatService
    {
        private readonly Dictionary<string, Rule> _rules =
            new(StringComparer.Ordinal);
        private readonly Dictionary<
            string,
            UnitAttackRejectReason> _rejections =
                new(StringComparer.Ordinal);
        private readonly HashSet<string> _blockedAttackers =
            new(StringComparer.Ordinal);
        private readonly string _wildcardTarget;

        public TestUnitCombatService()
        {
        }

        public TestUnitCombatService(
            string legalTargetId,
            int expectedDamage,
            bool targetWouldDie)
        {
            _wildcardTarget = legalTargetId;
            int hp = targetWouldDie
                ? Math.Max(1, expectedDamage)
                : Math.Max(expectedDamage + 20, 40);

            Allow(
                "*",
                legalTargetId,
                expectedDamage,
                hp);
        }

        public event Action<string, string> AttackStarted
        {
            add { }
            remove { }
        }

        public event Action<UnitAttackResult> AttackResolved
        {
            add { }
            remove { }
        }

        public int TryAttackCalls { get; private set; }
        public int GetAttackableTargetsCalls { get; private set; }

        public void Allow(
            string attacker,
            string target,
            int damage,
            int hp)
        {
            _rules[Key(attacker, target)] =
                new Rule(
                    Math.Max(0, damage),
                    Math.Max(1, hp));
        }

        public void Reject(
            string attacker,
            string target,
            UnitAttackRejectReason reason)
        {
            _rejections[Key(attacker, target)] = reason;
        }

        public void BlockAttacker(string attacker)
        {
            if (!string.IsNullOrWhiteSpace(attacker))
                _blockedAttackers.Add(attacker);
        }

        public bool CanAttack(
            string attackerUnitId,
            string targetUnitId,
            out UnitAttackRejectReason reason)
        {
            if (_blockedAttackers.Contains(
                    attackerUnitId ?? string.Empty))
            {
                reason =
                    UnitAttackRejectReason.AttackUnavailable;
                return false;
            }

            if (_rejections.TryGetValue(
                    Key(attackerUnitId, targetUnitId),
                    out reason))
            {
                return false;
            }

            if (TryRule(
                    attackerUnitId,
                    targetUnitId,
                    out _))
            {
                reason = UnitAttackRejectReason.None;
                return true;
            }

            reason =
                UnitAttackRejectReason.TargetNotAttackable;
            return false;
        }

        public IReadOnlyList<string> GetAttackableTargets(
            string attackerUnitId)
        {
            GetAttackableTargetsCalls++;
            return string.IsNullOrWhiteSpace(_wildcardTarget)
                ? new[] { "hidden-global-enemy" }
                : new[]
                {
                    _wildcardTarget,
                    "hidden-global-enemy",
                };
        }

        public IReadOnlyList<Vector2Int> GetAttackableTiles(
            string attackerUnitId)
            => Array.Empty<Vector2Int>();

        public bool TryGetHealth(
            string unitId,
            out UnitHealthSnapshot health)
        {
            foreach (KeyValuePair<string, Rule> pair in _rules)
            {
                string target =
                    TargetFromKey(pair.Key);
                if (string.Equals(
                        target,
                        unitId,
                        StringComparison.Ordinal))
                {
                    health = new UnitHealthSnapshot(
                        unitId,
                        pair.Value.Hp,
                        pair.Value.Hp);
                    return true;
                }
            }

            health = default;
            return false;
        }

        public bool TryPreviewAttack(
            string attackerUnitId,
            string defenderUnitId,
            out UnitCombatBreakdown breakdown)
        {
            if (!TryRule(
                    attackerUnitId,
                    defenderUnitId,
                    out Rule rule))
            {
                breakdown = default;
                return false;
            }

            breakdown =
                Breakdown(
                    rule.Damage,
                    rule.Hp);
            return true;
        }

        public bool TryPreviewDuel(
            string attackerUnitId,
            string defenderUnitId,
            out UnitCombatDuel duel)
        {
            if (!TryRule(
                    attackerUnitId,
                    defenderUnitId,
                    out Rule rule))
            {
                duel = default;
                return false;
            }

            UnitCombatBreakdown outgoing =
                Breakdown(
                    rule.Damage,
                    rule.Hp);
            UnitCombatBreakdown counter =
                Breakdown(
                    Math.Max(1, rule.Damage / 2),
                    100);

            duel = new UnitCombatDuel(
                outgoing,
                counter);
            return true;
        }

        public bool TryAttack(
            string attackerUnitId,
            string targetUnitId,
            out UnitAttackResult result)
        {
            TryAttackCalls++;
            result =
                UnitAttackResult.Rejected(
                    attackerUnitId,
                    targetUnitId,
                    UnitAttackRejectReason.AttackUnavailable);
            return false;
        }

        private bool TryRule(
            string attacker,
            string target,
            out Rule rule)
            => _rules.TryGetValue(
                    Key(attacker, target),
                    out rule)
                || _rules.TryGetValue(
                    Key("*", target),
                    out rule);

        private static UnitCombatBreakdown Breakdown(
            int damage,
            int hp)
            => new(
                cuttingRaw: damage,
                penetratingRaw: 0,
                crushingRaw: 0,
                cuttingDefense: 0,
                penetratingDefense: 0,
                crushingDefense: 0,
                cuttingEffective: damage,
                penetratingEffective: 0,
                crushingEffective: 0,
                levelMultiplier: 1f,
                totalDamage: damage,
                defenderHitPoints: hp);

        private static string Key(
            string attacker,
            string target)
            => (attacker ?? string.Empty)
                + "\u001f"
                + (target ?? string.Empty);

        private static string TargetFromKey(
            string key)
        {
            int separator =
                key.IndexOf('\u001f');
            return separator < 0
                ? string.Empty
                : key.Substring(separator + 1);
        }

        private readonly struct Rule
        {
            public Rule(int damage, int hp)
            {
                Damage = damage;
                Hp = hp;
            }

            public int Damage { get; }
            public int Hp { get; }
        }
    }
}
