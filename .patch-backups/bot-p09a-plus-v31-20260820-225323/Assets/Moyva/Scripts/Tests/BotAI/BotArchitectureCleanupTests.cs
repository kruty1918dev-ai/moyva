using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotArchitectureCleanupTests
    {
        [Test]
        public void LegacyScheduler_IsNotAZenjectLifecycleService()
        {
            Type scheduler = typeof(BotTickScheduler);

            Assert.That(typeof(IInitializable).IsAssignableFrom(scheduler), Is.False);
            Assert.That(typeof(ITickable).IsAssignableFrom(scheduler), Is.False);
        }

        [Test]
        public void LegacyScheduler_CompatibilityMethodsAreInert()
        {
            var scheduler = new BotTickScheduler(null, null, null);

            Assert.That(scheduler.IsRuntimeEnabled, Is.False);
            Assert.DoesNotThrow(scheduler.Initialize);
            Assert.DoesNotThrow(scheduler.Tick);
            Assert.That(scheduler.IsRuntimeEnabled, Is.False);
        }

        [Test]
        public void TurnExecutorContract_ExposesOneTurnScopedBeginBoundary()
        {
            var methods = typeof(IBotTurnExecutor).GetMethods();
            Assert.That(methods.Select(method => method.Name), Is.EquivalentTo(new[] { "TryBeginTurn" }));

            var method = methods.Single();
            Assert.That(method.ReturnType, Is.EqualTo(typeof(bool)));
            var parameters = method.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(3));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
            Assert.That(parameters[1].ParameterType, Is.EqualTo(typeof(long)));
            Assert.That(parameters[2].IsOut, Is.True);
            Assert.That(parameters[2].ParameterType, Is.EqualTo(typeof(string).MakeByRefType()));
        }

        [Test]
        public void LegacyBrain_IsMarkedObsolete()
        {
#pragma warning disable CS0618
            Assert.That(
                typeof(BotBrain).GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
                Is.Not.Empty);
#pragma warning restore CS0618
        }

        [Test]
        public void DifficultySettings_ExposePlanningQualityBudgets()
        {
            var easy = BotDifficultySettings.Easy();
            var normal = BotDifficultySettings.Normal();
            var hard = BotDifficultySettings.Hard();

            Assert.That(normal.MaxDecisionIterations, Is.EqualTo(24));
            Assert.That(normal.MaxSuccessfulMutations, Is.EqualTo(12));
            Assert.That(normal.MaxFailedMutations, Is.EqualTo(8));
            Assert.That(easy.DeterministicNoiseMagnitude, Is.GreaterThan(normal.DeterministicNoiseMagnitude));
            Assert.That(hard.DeterministicNoiseMagnitude, Is.EqualTo(0));
        }

        [Test]
        public void DeterministicNoise_IsStableAndBounded()
        {
            int first = BotTurnExecutor.StableNoise("bot-a", 12, 3, "move:unit-1:4,5", 5);
            int second = BotTurnExecutor.StableNoise("bot-a", 12, 3, "move:unit-1:4,5", 5);

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.InRange(-5, 5));
            Assert.That(BotTurnExecutor.StableNoise("bot-a", 12, 3, "move:unit-1:4,5", 0), Is.EqualTo(0));
        }

        [Test]
        public void MemoryStore_UpdatesVisibleEnemiesAndDecaysMobileConfidence()
        {
            var store = new BotMemoryStore();
            var visibleEnemy = new BotUnitSnapshot(
                "enemy-1",
                "human-a",
                "warrior",
                new Vector2Int(4, 5),
                stamina: 1f);
            var snapshot = new BotWorldSnapshot(
                "bot-a",
                round: 1,
                globalTurn: 10,
                Kruty1918.Moyva.Turns.API.TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                Vector2Int.zero,
                Array.Empty<BotUnitSnapshot>(),
                new[] { visibleEnemy },
                Array.Empty<BotBuildingSnapshot>(),
                Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot>(),
                Array.Empty<BotKnownEntityMemory>());

            store.UpdateFromObservation(snapshot, BotPlanningProfile.Normal());
            IReadOnlyList<BotKnownEntityMemory> memory = store.GetMemory("bot-a", 10);

            Assert.That(memory.Count, Is.EqualTo(1));
            Assert.That(memory[0].EntityId, Is.EqualTo("enemy-1"));
            Assert.That(memory[0].LastKnownPosition, Is.EqualTo(new Vector2Int(4, 5)));
            Assert.That(store.GetConfidence(memory[0], 10), Is.EqualTo(1000));
            Assert.That(store.GetConfidence(memory[0], 13), Is.EqualTo(450));
            Assert.That(store.GetConfidence(memory[0], 16), Is.EqualTo(0));
        }

        [Test]
        public void MemoryStore_RecordsVisibleEnemyCastleAsObjective()
        {
            var store = new BotMemoryStore();
            var snapshot = MakeSnapshot(
                globalTurn: 14,
                visibleEnemyBuildings: new[]
                {
                    new BotBuildingSnapshot("castle-01", "human-a", new Vector2Int(8, 2)),
                });

            store.UpdateFromObservation(snapshot, BotPlanningProfile.Normal());
            IReadOnlyList<BotKnownEntityMemory> memory = store.GetMemory("bot-a", 14);

            Assert.That(memory.Count, Is.EqualTo(1));
            Assert.That(memory[0].Kind, Is.EqualTo(BotKnownEntityKind.Objective));
            Assert.That(memory[0].TypeId, Is.EqualTo("castle-01"));
            Assert.That(memory[0].LastKnownPosition, Is.EqualTo(new Vector2Int(8, 2)));
        }

        [Test]
        public void SaveModule_RoundTripsMemoryAndStrategicPosture()
        {
            var memory = new BotMemoryStore();
            memory.UpdateFromObservation(
                MakeSnapshot(
                    globalTurn: 20,
                    visibleEnemyBuildings: new[]
                    {
                        new BotBuildingSnapshot("castle-01", "human-a", new Vector2Int(7, 3)),
                    }),
                BotPlanningProfile.Normal());

            var strategic = new BotStrategicPlanner();
            strategic.Plan(MakeSnapshot(
                globalTurn: 20,
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                    new BotUnitSnapshot("bot-unit-2", "bot-a", "warrior", Vector2Int.right, 1f),
                    new BotUnitSnapshot("bot-unit-3", "bot-a", "warrior", Vector2Int.up, 1f),
                },
                visibleEnemyBuildings: new[]
                {
                    new BotBuildingSnapshot("castle-01", "human-a", new Vector2Int(7, 3)),
                }));

            byte[] payload;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                var saveContext = new BinarySaveContext(writer, null);
                new BotAISaveModule(memory, strategic, BotPlanningProfile.Normal()).OnSave(saveContext);
                payload = stream.ToArray();
            }

            var restoredMemory = new BotMemoryStore();
            var restoredStrategic = new BotStrategicPlanner();
            using (var stream = new MemoryStream(payload))
            using (var reader = new BinaryReader(stream))
            {
                var loadContext = new BinarySaveContext(null, reader);
                new BotAISaveModule(restoredMemory, restoredStrategic, BotPlanningProfile.Normal()).OnLoad(loadContext);
            }

            IReadOnlyList<BotKnownEntityMemory> records = restoredMemory.GetMemory("bot-a", 20);
            IReadOnlyList<BotStrategicStateSnapshot> states = restoredStrategic.CaptureStrategicState();

            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(records[0].Kind, Is.EqualTo(BotKnownEntityKind.Objective));
            Assert.That(records[0].LastKnownPosition, Is.EqualTo(new Vector2Int(7, 3)));
            Assert.That(states.Count, Is.EqualTo(1));
            Assert.That(states[0].Posture, Is.EqualTo(BotStrategicPosture.Siege));
            Assert.That(states[0].GlobalTurn, Is.EqualTo(20));
        }

        [Test]
        public void StrategicPlanner_ChoosesEmergencyDefenseForVisibleBaseThreat()
        {
            var planner = new BotStrategicPlanner();
            var snapshot = MakeSnapshot(
                visibleEnemies: new[]
                {
                    new BotUnitSnapshot("enemy-1", "human-a", "warrior", new Vector2Int(2, 0), 1f),
                },
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("castle", "bot-a", Vector2Int.zero),
                });

            BotStrategicContext context = planner.Plan(snapshot);

            Assert.That(context.Posture, Is.EqualTo(BotStrategicPosture.EmergencyDefense));
        }

        [Test]
        public void StrategicPlanner_SearchesWhenNoVisibleContactAndSomeArmyExists()
        {
            var planner = new BotStrategicPlanner();
            var snapshot = MakeSnapshot(
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                    new BotUnitSnapshot("bot-unit-2", "bot-a", "warrior", Vector2Int.right, 1f),
                    new BotUnitSnapshot("bot-unit-3", "bot-a", "warrior", Vector2Int.up, 1f),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("castle", "bot-a", Vector2Int.zero),
                });

            BotStrategicContext context = planner.Plan(snapshot);

            Assert.That(context.Posture, Is.EqualTo(BotStrategicPosture.Search));
        }

        [Test]
        public void StrategicPlanner_ChoosesSiegeForKnownEnemyCastle()
        {
            var planner = new BotStrategicPlanner();
            var snapshot = MakeSnapshot(
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                    new BotUnitSnapshot("bot-unit-2", "bot-a", "warrior", Vector2Int.right, 1f),
                    new BotUnitSnapshot("bot-unit-3", "bot-a", "warrior", Vector2Int.up, 1f),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("castle", "bot-a", Vector2Int.zero),
                },
                visibleEnemyBuildings: new[]
                {
                    new BotBuildingSnapshot("castle-01", "human-a", new Vector2Int(8, 0)),
                });

            BotStrategicContext context = planner.Plan(snapshot);

            Assert.That(context.Posture, Is.EqualTo(BotStrategicPosture.Siege));
        }

        [Test]
        public void StrategicPlanner_RetainsPreviousPostureInsideHysteresisMargin()
        {
            var planner = new BotStrategicPlanner();
            var first = MakeSnapshot(
                globalTurn: 20,
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                    new BotUnitSnapshot("bot-unit-2", "bot-a", "warrior", Vector2Int.right, 1f),
                    new BotUnitSnapshot("bot-unit-3", "bot-a", "warrior", Vector2Int.up, 1f),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("castle", "bot-a", Vector2Int.zero),
                });
            Assert.That(planner.Plan(first).Posture, Is.EqualTo(BotStrategicPosture.Search));

            var second = MakeSnapshot(
                globalTurn: 21,
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                    new BotUnitSnapshot("bot-unit-2", "bot-a", "warrior", Vector2Int.right, 1f),
                    new BotUnitSnapshot("bot-unit-3", "bot-a", "warrior", Vector2Int.up, 1f),
                },
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("castle", "bot-a", Vector2Int.zero),
                },
                visibleEnemies: new[]
                {
                    new BotUnitSnapshot("enemy-1", "human-a", "warrior", new Vector2Int(20, 20), 1f),
                });

            Assert.That(planner.Plan(second).Posture, Is.EqualTo(BotStrategicPosture.Search));
        }

        [Test]
        public void ConstructionPlanner_GeneratesRecruitmentBuildFromPlacementQuery()
        {
            var registry = new FakeBuildingRegistry(
                RecruitmentBuilding("barrack"),
                PlainBuilding("storage"));
            var placement = new FakePlacementQuery(new Vector2Int(1, 0));
            var planner = new BotConstructionPlanner(registry, placement);
            BotWorldSnapshot snapshot = MakeSnapshot(
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("castle", "bot-a", Vector2Int.zero),
                });
            var strategy = new BotStrategicContext("bot-a", 10, BotStrategicPosture.Opening, 500, "test");

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(snapshot, strategy);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Kind, Is.EqualTo(BotActionKind.Build));
            Assert.That(candidates[0].DefinitionId, Is.EqualTo("barrack"));
            Assert.That(candidates[0].TargetCell, Is.EqualTo(new Vector2Int(1, 0)));
            Assert.That(placement.EvaluateCalls, Is.GreaterThan(0));
        }

        [Test]
        public void ConstructionPlanner_SkipsWhenRecruitmentBuildingAlreadyOwned()
        {
            var registry = new FakeBuildingRegistry(RecruitmentBuilding("barrack"));
            var placement = new FakePlacementQuery(new Vector2Int(1, 0));
            var planner = new BotConstructionPlanner(registry, placement);
            BotWorldSnapshot snapshot = MakeSnapshot(
                ownBuildings: new[]
                {
                    new BotBuildingSnapshot("barrack", "bot-a", Vector2Int.zero),
                });
            var strategy = new BotStrategicContext("bot-a", 10, BotStrategicPosture.Opening, 500, "test");

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(snapshot, strategy);

            Assert.That(candidates.Count, Is.EqualTo(0));
            Assert.That(placement.EvaluateCalls, Is.EqualTo(0));
        }

        [Test]
        public void DeploymentPlanner_GeneratesReadyDeploymentCandidateOnBestValidTile()
        {
            var ready = new Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot(
                queueId: 7,
                ownerId: "bot-a",
                recruitingBuildingPosition: new Vector2Int(2, 2),
                recruitingBuildingId: "barrack",
                unitTypeId: "warrior",
                completedTurns: 1,
                trainingTurns: 1,
                enqueuedGlobalTurn: 8,
                lastProgressGlobalTurn: 9,
                Kruty1918.Moyva.Units.API.UnitRecruitmentQueueStatus.Ready);
            var recruitment = new FakeRecruitmentService(
                new Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot(new Vector2Int(5, 5), false, "blocked"),
                new Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot(new Vector2Int(4, 4), true, null),
                new Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot(new Vector2Int(1, 0), true, null));
            var planner = new BotDeploymentPlanner(recruitment);
            var snapshot = new BotWorldSnapshot(
                "bot-a",
                round: 1,
                globalTurn: 10,
                Kruty1918.Moyva.Turns.API.TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                Vector2Int.zero,
                Array.Empty<BotUnitSnapshot>(),
                Array.Empty<BotUnitSnapshot>(),
                Array.Empty<BotBuildingSnapshot>(),
                new[] { ready },
                Array.Empty<BotKnownEntityMemory>());
            var strategy = new BotStrategicContext("bot-a", 10, BotStrategicPosture.ArmyBuildUp, 500, "test");

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(snapshot, strategy);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Kind, Is.EqualTo(BotActionKind.DeployReadyUnit));
            Assert.That(candidates[0].TargetCell, Is.EqualTo(new Vector2Int(1, 0)));
            Assert.That(candidates[0].DefinitionId, Is.EqualTo("warrior"));
        }

        [Test]
        public void TurnPlanner_AggregatesAndSortsCandidatesDeterministically()
        {
            var low = new BotActionCandidate(
                "build:z",
                BotActionKind.Build,
                BotStrategicPosture.Opening,
                new BotActionScore(10, "low"));
            var high = new BotActionCandidate(
                "deploy:a",
                BotActionKind.DeployReadyUnit,
                BotStrategicPosture.Opening,
                new BotActionScore(20, "high"));
            var planner = new BotTurnPlanner(
                deployment: new StaticDeploymentPlanner(high),
                construction: new StaticConstructionPlanner(low));

            IReadOnlyList<BotActionCandidate> candidates = planner.GenerateCandidates(
                MakeSnapshot(),
                new BotStrategicContext("bot-a", 10, BotStrategicPosture.Opening, 500, "test"));

            Assert.That(candidates.Select(candidate => candidate.CandidateId), Is.EqualTo(new[] { "deploy:a", "build:z" }));
        }

        [Test]
        public void CombatPlanner_GeneratesOnlyLegalVisibleImmediateAttacks()
        {
            var combat = new TestUnitCombatService(
                legalTargetId: "enemy-2",
                expectedDamage: 30,
                targetWouldDie: true);
            var planner = new BotCombatPlanner(combat);
            var snapshot = MakeSnapshot(
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                },
                visibleEnemies: new[]
                {
                    new BotUnitSnapshot("enemy-1", "human-a", "warrior", new Vector2Int(1, 0), 1f),
                    new BotUnitSnapshot("enemy-2", "human-a", "warrior", new Vector2Int(2, 0), 1f),
                });

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(
                snapshot,
                new BotStrategicContext("bot-a", 10, BotStrategicPosture.EmergencyDefense, 500, "test"));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Kind, Is.EqualTo(BotActionKind.Attack));
            Assert.That(candidates[0].ActorId, Is.EqualTo("bot-unit-1"));
            Assert.That(candidates[0].TargetId, Is.EqualTo("enemy-2"));
            Assert.That(candidates[0].Score.Explanation, Is.EqualTo("Legal lethal attack."));
        }

        [Test]
        public void TurnPlanner_PrioritizesHighValueCombatCandidate()
        {
            var attack = new BotActionCandidate(
                "attack:a",
                BotActionKind.Attack,
                BotStrategicPosture.EmergencyDefense,
                new BotActionScore(1000, "attack"));
            var build = new BotActionCandidate(
                "build:z",
                BotActionKind.Build,
                BotStrategicPosture.EmergencyDefense,
                new BotActionScore(200, "build"));
            var planner = new BotTurnPlanner(
                combat: new StaticCombatPlanner(attack),
                construction: new StaticConstructionPlanner(build));

            IReadOnlyList<BotActionCandidate> candidates = planner.GenerateCandidates(
                MakeSnapshot(),
                new BotStrategicContext("bot-a", 10, BotStrategicPosture.EmergencyDefense, 500, "test"));

            Assert.That(candidates.Select(candidate => candidate.CandidateId), Is.EqualTo(new[] { "attack:a", "build:z" }));
        }

        [Test]
        public void ActionExecutor_RevalidatesAndDeploysReadyUnitThroughRecruitmentService()
        {
            var ready = new Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot(
                queueId: 7,
                ownerId: "bot-a",
                recruitingBuildingPosition: new Vector2Int(2, 2),
                recruitingBuildingId: "barrack",
                unitTypeId: "warrior",
                completedTurns: 1,
                trainingTurns: 1,
                enqueuedGlobalTurn: 8,
                lastProgressGlobalTurn: 9,
                Kruty1918.Moyva.Units.API.UnitRecruitmentQueueStatus.Ready);
            var recruitment = new FakeRecruitmentService(
                ready,
                new Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot(new Vector2Int(1, 0), true, null));
            var executor = new BotActionExecutor(recruitment: recruitment);
            var action = new BotActionCandidate(
                "deploy:7:1,0",
                BotActionKind.DeployReadyUnit,
                BotStrategicPosture.ArmyBuildUp,
                new BotActionScore(100, "test"),
                targetCell: new Vector2Int(1, 0),
                definitionId: "warrior");

            BotActionExecutionResult result = executor.ExecuteAsync("bot-a", action, System.Threading.CancellationToken.None).Result;

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Mutated, Is.True);
            Assert.That(recruitment.DeployCalls, Is.EqualTo(1));
            Assert.That(recruitment.DeployedTarget, Is.EqualTo(new Vector2Int(1, 0)));
        }

        [Test]
        public void MovementPlanner_UsesCanonicalReachableTiles()
        {
            var movementQuery = new FakeMovementQuery(
                "bot-unit-1",
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(1, 0), false, 1f),
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(2, 0), true, 1f));
            var planner = new BotMovementPlanner(movementQuery);
            var snapshot = MakeSnapshot(
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                },
                visibleEnemies: new[]
                {
                    new BotUnitSnapshot("enemy-1", "human-a", "warrior", new Vector2Int(4, 0), 1f),
                });

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(
                snapshot,
                new BotStrategicContext("bot-a", 10, BotStrategicPosture.Pressure, 500, "test"));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Kind, Is.EqualTo(BotActionKind.Move));
            Assert.That(candidates[0].TargetCell, Is.EqualTo(new Vector2Int(2, 0)));
        }

        [Test]
        public void ObjectivePlanner_MovesTowardVisibleEnemyCastle()
        {
            var movementQuery = new FakeMovementQuery(
                "bot-unit-1",
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(1, 0), true, 1f),
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(3, 0), true, 1f));
            var planner = new BotObjectivePlanner(movementQuery);
            var snapshot = MakeSnapshot(
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                },
                visibleEnemyBuildings: new[]
                {
                    new BotBuildingSnapshot("castle-01", "human-a", new Vector2Int(5, 0)),
                });

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(
                snapshot,
                new BotStrategicContext("bot-a", 10, BotStrategicPosture.Siege, 600, "test"));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Kind, Is.EqualTo(BotActionKind.Move));
            Assert.That(candidates[0].Posture, Is.EqualTo(BotStrategicPosture.Siege));
            Assert.That(candidates[0].TargetCell, Is.EqualTo(new Vector2Int(3, 0)));
        }

        [Test]
        public void ObjectivePlanner_UsesRememberedObjectiveWhenNotVisible()
        {
            var movementQuery = new FakeMovementQuery(
                "bot-unit-1",
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(0, 1), true, 1f),
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(2, 2), true, 1f));
            var planner = new BotObjectivePlanner(movementQuery);
            var snapshot = MakeSnapshot(
                ownUnits: new[]
                {
                    new BotUnitSnapshot("bot-unit-1", "bot-a", "warrior", Vector2Int.zero, 1f),
                },
                memory: new[]
                {
                    new BotKnownEntityMemory(
                        "building:human-a:castle-01:5,5",
                        BotKnownEntityKind.Objective,
                        "human-a",
                        "castle-01",
                        new Vector2Int(5, 5),
                        8),
                });

            IReadOnlyList<BotActionCandidate> candidates = planner.Generate(
                snapshot,
                new BotStrategicContext("bot-a", 10, BotStrategicPosture.Search, 500, "test"));

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Kind, Is.EqualTo(BotActionKind.Move));
            Assert.That(candidates[0].TargetCell, Is.EqualTo(new Vector2Int(2, 2)));
            Assert.That(candidates[0].Score.Explanation, Is.EqualTo("Remembered enemy objective."));
        }

        [Test]
        public void ActionExecutor_RevalidatesAndMovesThroughMovementService()
        {
            var movementQuery = new FakeMovementQuery(
                "bot-unit-1",
                new Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot(new Vector2Int(2, 0), true, 1f));
            var movement = new FakeMovementService();
            var executor = new BotActionExecutor(movement: movement, movementQuery: movementQuery);
            var action = new BotActionCandidate(
                "Move:bot-unit-1:2,0",
                BotActionKind.Move,
                BotStrategicPosture.Pressure,
                new BotActionScore(100, "test"),
                actorId: "bot-unit-1",
                targetCell: new Vector2Int(2, 0));

            BotActionExecutionResult result = executor.ExecuteAsync("bot-a", action, System.Threading.CancellationToken.None).Result;

            Assert.That(result.Succeeded, Is.True);
            Assert.That(movement.MoveCalls, Is.EqualTo(1));
            Assert.That(movement.LastTarget, Is.EqualTo(new Vector2Int(2, 0)));
        }

        private static BotWorldSnapshot MakeSnapshot(
            long globalTurn = 10,
            IReadOnlyList<BotUnitSnapshot> ownUnits = null,
            IReadOnlyList<BotUnitSnapshot> visibleEnemies = null,
            IReadOnlyList<BotBuildingSnapshot> ownBuildings = null,
            IReadOnlyList<BotBuildingSnapshot> visibleEnemyBuildings = null,
            IReadOnlyList<BotKnownEntityMemory> memory = null)
            => new(
                "bot-a",
                round: 1,
                globalTurn,
                Kruty1918.Moyva.Turns.API.TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                Vector2Int.zero,
                ownUnits ?? Array.Empty<BotUnitSnapshot>(),
                visibleEnemies ?? Array.Empty<BotUnitSnapshot>(),
                ownBuildings ?? Array.Empty<BotBuildingSnapshot>(),
                Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot>(),
                memory ?? Array.Empty<BotKnownEntityMemory>(),
                visibleEnemyBuildings ?? Array.Empty<BotBuildingSnapshot>());

        private static BuildingDefinition RecruitmentBuilding(string id)
            => new()
            {
                Id = id,
                Category = BuildingCategory.Military,
                Modules = new List<BuildingModuleDefinition>
                {
                    new UnitRecruitmentBuildingModule(),
                },
            };

        private static BuildingDefinition PlainBuilding(string id)
            => new()
            {
                Id = id,
                Category = BuildingCategory.Civilian,
            };

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition[] _definitions;

            public FakeBuildingRegistry(params BuildingDefinition[] definitions)
            {
                _definitions = definitions ?? Array.Empty<BuildingDefinition>();
            }

            public BuildingDefinition[] GetAll() => _definitions;

            public BuildingDefinition GetById(string id)
            {
                for (int index = 0; index < _definitions.Length; index++)
                {
                    if (string.Equals(_definitions[index]?.Id, id, StringComparison.Ordinal))
                        return _definitions[index];
                }

                return null;
            }

            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => _definitions.Where(definition => definition != null && definition.Category == category).ToArray();

            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();

            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
                => null;
        }

        private sealed class FakePlacementQuery : IConstructionPlacementQuery
        {
            private readonly Vector2Int _validPosition;

            public FakePlacementQuery(Vector2Int validPosition)
            {
                _validPosition = validPosition;
            }

            public int EvaluateCalls { get; private set; }

            public ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request)
            {
                EvaluateCalls++;
                bool valid = request.Position == _validPosition;
                return new ConstructionPlacementQueryResult(
                    availabilityValid: true,
                    spatialValid: valid,
                    resourcesValid: valid,
                    authorityValid: valid,
                    isGateReplacement: false,
                    reason: valid ? null : "blocked");
            }
        }

        private sealed class FakeRecruitmentService : Kruty1918.Moyva.Units.API.IUnitRecruitmentService
        {
            private readonly IReadOnlyList<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot> _ready;
            private readonly IReadOnlyList<Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot> _tiles;

            public FakeRecruitmentService(params Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot[] tiles)
            {
                _ready = Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot>();
                _tiles = tiles ?? Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot>();
            }

            public FakeRecruitmentService(
                Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot ready,
                params Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot[] tiles)
            {
                _ready = new[] { ready };
                _tiles = tiles ?? Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot>();
            }

            public int DeployCalls { get; private set; }
            public Vector2Int DeployedTarget { get; private set; }

            public bool TryEnqueue(string ownerId, Vector2Int recruitingBuildingPosition, string unitTypeId, out string reason)
            {
                reason = "not implemented";
                return false;
            }

            public IReadOnlyList<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot> GetQueue(string ownerId, Vector2Int recruitingBuildingPosition)
                => Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot>();

            public bool TryPeekReady(string ownerId, Vector2Int recruitingBuildingPosition, out Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot item)
            {
                item = default;
                return false;
            }

            public IReadOnlyList<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot> GetReadyItems(string ownerId)
                => _ready;

            public IReadOnlyList<Kruty1918.Moyva.Units.API.UnitRecruitmentDeploymentTileSnapshot> GetDeploymentTiles(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId)
                => _tiles;

            public bool TryDeployReady(
                string ownerId,
                Vector2Int recruitingBuildingPosition,
                long queueId,
                Vector2Int targetPosition,
                out string unitId,
                out string reason)
            {
                DeployCalls++;
                DeployedTarget = targetPosition;
                unitId = "deployed-unit";
                reason = null;
                return true;
            }
        }

        private sealed class StaticDeploymentPlanner : IBotDeploymentPlanner
        {
            private readonly IReadOnlyList<BotActionCandidate> _candidates;

            public StaticDeploymentPlanner(params BotActionCandidate[] candidates)
            {
                _candidates = candidates;
            }

            public IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy)
                => _candidates;
        }

        private sealed class StaticConstructionPlanner : IBotConstructionPlanner
        {
            private readonly IReadOnlyList<BotActionCandidate> _candidates;

            public StaticConstructionPlanner(params BotActionCandidate[] candidates)
            {
                _candidates = candidates;
            }

            public IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy)
                => _candidates;
        }

        private sealed class StaticCombatPlanner : IBotCombatPlanner
        {
            private readonly IReadOnlyList<BotActionCandidate> _candidates;

            public StaticCombatPlanner(params BotActionCandidate[] candidates)
            {
                _candidates = candidates;
            }

            public IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy)
                => _candidates;
        }

        private sealed class FakeCombatCommandService : ICombatCommandService
        {
            private readonly string _legalTargetId;
            private readonly int _expectedDamage;
            private readonly bool _targetWouldDie;

            public FakeCombatCommandService(string legalTargetId, int expectedDamage, bool targetWouldDie)
            {
                _legalTargetId = legalTargetId;
                _expectedDamage = expectedDamage;
                _targetWouldDie = targetWouldDie;
            }

            public bool TryPreview(
                string attackerEntityId,
                string targetEntityId,
                out CombatCommandPreview preview,
                out string reason)
            {
                if (!string.Equals(targetEntityId, _legalTargetId, StringComparison.Ordinal))
                {
                    preview = default;
                    reason = "illegal";
                    return false;
                }

                preview = new CombatCommandPreview(attackerEntityId, targetEntityId, _expectedDamage, _targetWouldDie);
                reason = null;
                return true;
            }

            public System.Threading.Tasks.Task<CombatCommandResult> ExecuteAsync(
                string requesterOwnerId,
                string attackerEntityId,
                string targetEntityId,
                System.Threading.CancellationToken token = default)
                => System.Threading.Tasks.Task.FromResult(new CombatCommandResult(
                    true,
                    attackerEntityId,
                    targetEntityId,
                    _expectedDamage,
                    _targetWouldDie,
                    null));
        }

        private sealed class FakeMovementQuery : Kruty1918.Moyva.Units.API.IUnitMovementQuery
        {
            private readonly string _unitId;
            private readonly IReadOnlyList<Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot> _tiles;

            public FakeMovementQuery(
                string unitId,
                params Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot[] tiles)
            {
                _unitId = unitId;
                _tiles = tiles ?? Array.Empty<Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot>();
            }

            public IReadOnlyList<Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot> GetMovementTiles(string unitId)
                => string.Equals(unitId, _unitId, StringComparison.Ordinal)
                    ? _tiles
                    : Array.Empty<Kruty1918.Moyva.Units.API.UnitMovementTileSnapshot>();
        }

        private sealed class FakeMovementService : Kruty1918.Moyva.Units.API.IUnitMovementService
        {
            public int MoveCalls { get; private set; }
            public Vector2Int LastTarget { get; private set; }

            public System.Threading.Tasks.Task MoveUnitAsync(
                string unitId,
                Vector2Int targetPosition,
                System.Threading.CancellationToken token = default)
            {
                MoveCalls++;
                LastTarget = targetPosition;
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

        private sealed class BinarySaveContext : ISaveContext
        {
            public BinarySaveContext(BinaryWriter writer, BinaryReader reader)
            {
                Writer = writer;
                Reader = reader;
            }

            public BinaryWriter Writer { get; }
            public BinaryReader Reader { get; }
        }
    }
}
