using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.AI.Bot;
using UnityEngine;
using NUnit.Framework;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public sealed class TrainingLogicTests
    {
        [Test]
        public void EndTurnUsesCanonicalApiAndRechecksLegality()
        {
            using var simulation = new TurnAdapter();
            var mask = new TrainingActionMaskProvider(simulation);
            var executor = new TrainingActionExecutor(simulation, mask);
            Assert.IsTrue(mask.IsLegal((int)TrainingActionType.EndTurn));
            Assert.IsTrue(executor.TryExecute((int)TrainingActionType.EndTurn, out _));
            Assert.AreEqual("player", simulation.Service.Requester);
            Assert.AreEqual(1, simulation.Service.Calls);
            simulation.AllowEndTurn = false;
            Assert.IsFalse(executor.TryExecute((int)TrainingActionType.EndTurn, out _));
            Assert.AreEqual(1, simulation.Service.Calls);
            Assert.IsTrue(mask.IsLegal(TrainingActionMaskProvider.SafeActionIndex));
        }

        [Test]
        public void TerminalEventEndsEnvironmentAndResetClearsActiveEpisode()
        {
            using var environment = new TrainingEnvironment(0, new TrainingConfig { allowScaffoldSimulation = true },
                new ScaffoldSimulationFactory().Create(0));
            environment.BeginEpisode();
            environment.Step(TrainingActionMaskProvider.SafeActionIndex);
            environment.ResetEnvironment();
            environment.BeginEpisode();
            Assert.AreEqual(2, environment.EpisodeId);
            Assert.AreEqual(0, environment.Diagnostics.Decisions);
            Assert.AreEqual(0, environment.Rewards.TotalReward);
            environment.Rewards.Record(new TrainingRewardEvent(environment.EpisodeId, "won",
                TrainingRewardEventType.EpisodeWon, validated: true));
            Assert.AreEqual(TrainingEpisodeResult.Victory, environment.Result);
            Assert.IsFalse(environment.IsReady);
            Assert.AreEqual(1, environment.Diagnostics.TotalReward);
        }

        private sealed class TurnAdapter : ITrainingSimulation
        {
            public readonly RecordingTurns Service = new RecordingTurns();
            public bool AllowEndTurn = true;
            public ITurnService Turns => Service;
            public string PlayerId => "player";
            public bool IsReady => true;
            public string Limitation => null;
            public bool CanEndTurn() => AllowEndTurn;
            public bool Reset(TrainingResetContext context) => true;
            public void Dispose() { }
        }

        private sealed class RecordingTurns : ITurnService
        {
            public int Calls;
            public string Requester;
            public event Action StateChanged { add { } remove { } }
            public TurnPhase Phase => TurnPhase.AwaitingInput;
            public int Round => 1;
            public long GlobalTurn => 1;
            public int ActionsThisTurn => 0;
            public string ActiveOwnerId => "player";
            public string LocalOwnerId => "player";
            public IReadOnlyList<TurnFaction> Factions => Array.Empty<TurnFaction>();
            public bool IsOwnerActive(string ownerId) => ownerId == "player";
            public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return IsOwnerActive(ownerId); }
            public bool TryRecordAction(string ownerId, string actionId) => throw new NotSupportedException();
            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                Requester = requesterOwnerId;
                Calls++;
                reason = null;
                return true;
            }
        }

        private static TrainingRewardEvent Capture(long episode, string id, string subject)
            => new TrainingRewardEvent(episode, id, TrainingRewardEventType.ObjectiveCaptured,
                subject, "player", "enemy", true, true);

        [Test]
        public void SeedsAreReproducibleAndVaryByEnvironmentAndEpisode()
        {
            int seed = TrainingResetContext.DeriveSeed(1918, 0, 1);
            Assert.AreEqual(seed, TrainingResetContext.DeriveSeed(1918, 0, 1));
            Assert.AreNotEqual(seed, TrainingResetContext.DeriveSeed(1918, 1, 1));
            Assert.AreNotEqual(seed, TrainingResetContext.DeriveSeed(1918, 0, 2));
            Assert.AreNotEqual(seed, TrainingResetContext.DeriveSeed(1918, 0, 1L << 32));
        }

        [Test]
        public void AbsoluteShapingCannotExceedCap()
        {
            var tracker = new TrainingRewardTracker(new TrainingRewardConfig(), "player");
            tracker.Reset(1);
            for (int i = 0; i < 1000; i++)
            {
                tracker.Record(Capture(1, "capture" + i, "objective" + i));
                tracker.Record(new TrainingRewardEvent(1, "loss" + i, TrainingRewardEventType.ObjectiveLost,
                    "objective" + i, "enemy", "player", true, true));
            }
            Assert.That(tracker.AbsoluteShaping, Is.EqualTo(0.25f).Within(0.00001f));
            Assert.That(Math.Abs(tracker.TotalReward), Is.LessThanOrEqualTo(0.25001f));
        }

        [TestCase(TrainingEpisodeResult.Victory, 1f)]
        [TestCase(TrainingEpisodeResult.Defeat, -1f)]
        [TestCase(TrainingEpisodeResult.Draw, -0.1f)]
        [TestCase(TrainingEpisodeResult.Timeout, -0.15f)]
        public void TerminalRewardAppliedOnce(TrainingEpisodeResult result, float expected)
        {
            var tracker = new TrainingRewardTracker(new TrainingRewardConfig(), "player");
            tracker.Reset(1);
            Assert.AreEqual(expected, tracker.Complete(result));
            Assert.AreEqual(0, tracker.Complete(result));
            Assert.AreEqual(0, tracker.Record(Capture(1, "late", "late")));
            Assert.AreEqual(expected, tracker.TotalReward);
        }

        [Test]
        public void DuplicatesOscillationAndStaleEventsCannotFarmRewards()
        {
            var tracker = new TrainingRewardTracker(new TrainingRewardConfig(), "player");
            tracker.Reset(1);
            Assert.AreEqual(0.02f, tracker.Record(Capture(1, "a", "objective")));
            Assert.AreEqual(0, tracker.Record(Capture(1, "a", "different")));
            Assert.AreEqual(0, tracker.Record(Capture(1, "b", "objective")));
            Assert.AreEqual(0, tracker.Record(Capture(0, "c", "other")));
        }

        [Test]
        public void FriendlyKillsAndUnvalidatedEventsGiveNoReward()
        {
            var tracker = new TrainingRewardTracker(new TrainingRewardConfig(), "player");
            tracker.Reset(1);
            Assert.AreEqual(0, tracker.Record(new TrainingRewardEvent(1, "friendly",
                TrainingRewardEventType.EnemyUnitDestroyed, "unit", "player", "player", true, true)));
            Assert.AreEqual(0, tracker.Record(new TrainingRewardEvent(1, "unvalidated",
                TrainingRewardEventType.UnitCreated, "unit", "player", "", false, true)));
            Assert.AreEqual(0, tracker.Record(new TrainingRewardEvent(1, "generic",
                TrainingRewardEventType.ValidAction, "unit", "player", "", true, true)));
        }

        [Test]
        public void InvalidEpisodeClearsAccumulatedReward()
        {
            var tracker = new TrainingRewardTracker(new TrainingRewardConfig(), "player");
            tracker.Reset(1);
            tracker.Record(Capture(1, "a", "objective"));
            tracker.Complete(TrainingEpisodeResult.InvalidState);
            Assert.AreEqual(0, tracker.TotalReward);
        }

        [Test]
        public void SafeFallbackIsAlwaysLegalAndUnsupportedActionsAreMasked()
        {
            using var simulation = new ScaffoldSimulationFactory().Create(0);
            var mask = new TrainingActionMaskProvider(simulation);
            var executor = new TrainingActionExecutor(simulation, mask);
            Assert.AreEqual(TrainingActionMaskProvider.SafeActionIndex, mask.FirstLegalAction());
            Assert.IsTrue(executor.TryExecute(mask.FirstLegalAction(), out _));
            for (int i = 0; i < TrainingActionMaskProvider.SafeActionIndex; i++) Assert.IsFalse(mask.IsLegal(i));
            Assert.IsFalse(executor.TryExecute(-1, out _));
            Assert.IsFalse(executor.TryExecute(99, out _));
        }

        [Test]
        public void IndependentEnvironmentsResetAllEpisodeState()
        {
            var config = new TrainingConfig { maxDecisionsPerEpisode = 2, allowScaffoldSimulation = true };
            var factory = new ScaffoldSimulationFactory();
            using var a = new TrainingEnvironment(0, config, factory.Create(0));
            using var b = new TrainingEnvironment(1, config, factory.Create(1));
            a.BeginEpisode();
            b.BeginEpisode();
            Assert.AreNotEqual(a.EnvironmentId, b.EnvironmentId);
            Assert.AreNotEqual(a.Diagnostics.LastSeed, b.Diagnostics.LastSeed);
            a.Step(TrainingActionMaskProvider.SafeActionIndex);
            a.Step(TrainingActionMaskProvider.SafeActionIndex);
            Assert.AreEqual(TrainingEpisodeResult.Timeout, a.Result);
            Assert.AreEqual(0, b.Diagnostics.Decisions);
            Assert.AreEqual(0, b.Rewards.TotalReward);
            a.BeginEpisode();
            Assert.AreEqual(2, a.EpisodeId);
            Assert.AreEqual(1, b.EpisodeId);
            Assert.AreEqual(0, a.Diagnostics.Decisions);
            Assert.AreEqual(0, a.Rewards.TotalReward);
            Assert.IsNull(a.PreviousAction);
            Assert.AreEqual(2, a.Diagnostics.ResetCount);
            Assert.IsTrue(a.IsReady);
        }

        [Test]
        public void InvalidActionsTerminateAtConfiguredLimit()
        {
            var config = new TrainingConfig { allowScaffoldSimulation = true };
            config.rewards.invalidActionLimit = 2;
            using var environment = new TrainingEnvironment(0, config, new ScaffoldSimulationFactory().Create(0));
            environment.BeginEpisode();
            environment.Step((int)TrainingActionType.Attack);
            environment.Step((int)TrainingActionType.Attack);
            Assert.AreEqual(TrainingEpisodeResult.InvalidState, environment.Result);
            Assert.AreEqual(0, environment.Rewards.TotalReward);
            Assert.IsFalse(environment.IsReady);
        }

        private static TrainingScenarioDefinition AuthoritativeScenario(params TrainingScenarioStepDefinition[] steps)
        {
            return new TrainingScenarioDefinition
            {
                id = "test",
                title = "test",
                learnerBuildsInitialCastle = false,
                startingConditions = new TrainingScenarioStartingConditions(),
                availableCapabilities = new[] { "construction", "economy", "recruitment", "movement", "scouting", "combat", "capture", "end-turn" },
                generationConstraints = new TrainingScenarioGenerationConstraints(),
                rewardRules = new TrainingScenarioRewardRules(),
                steps = steps
            };
        }

        private static TrainingScenarioFacts AuthoritativeFacts(
            bool setup = false, int settlements = 0, int castles = 0, int ownedUnits = 0, int deployed = 0,
            float woodStock = 0f, float woodProduction = 0f, int turn = 0, int explored = 0,
            int warriors = 0, int castleBuildings = 0, string objective = null,
            string unitId = null, Vector2Int? cell = null)
        {
            var stock = new Dictionary<string, float> { ["walnut-wood-materials-resources"] = woodStock };
            var production = new Dictionary<string, float> { ["walnut-wood-materials-resources"] = woodProduction };
            var units = new Dictionary<string, int> { ["warrior"] = warriors };
            var buildings = new Dictionary<string, int> { ["castle-01"] = castleBuildings, ["wood-camp"] = woodProduction > 0 ? 1 : 0 };
            var cells = new Dictionary<string, Vector2Int>();
            if (unitId != null && cell.HasValue) cells[unitId] = cell.Value;
            var objectives = objective == null ? Array.Empty<string>() : new[] { objective };
            return new TrainingScenarioFacts(setup, settlements, castles, ownedUnits, deployed, stock, production, 0,
                objectives, turn, explored, units, buildings, cells, objectives);
        }

        [Test]
        public void SetupCastleDoesNotCompleteCastleLesson()
        {
            var step = new TrainingScenarioStepDefinition { id = "castle", criterion = TrainingScenarioCriterionKind.OperationalCastle, buildingTypeId = "castle-01" };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(7, AuthoritativeFacts(setup: true, settlements: 1, castles: 1, castleBuildings: 1));
            tracker.ObserveReward(new TrainingRewardEvent(7, "setup", TrainingRewardEventType.BuildingCreated,
                "building-type:castle-01", validated: true, meaningful: true), AuthoritativeFacts(setup: true, settlements: 1, castles: 1, castleBuildings: 1));
            Assert.IsFalse(tracker.IsComplete);
            Assert.AreEqual(0f, tracker.Progress);
        }

        [Test]
        public void LearnerCastleRequiresOperationalState()
        {
            var step = new TrainingScenarioStepDefinition { id = "castle", criterion = TrainingScenarioCriterionKind.OperationalCastle, buildingTypeId = "castle-01" };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(8, AuthoritativeFacts());
            tracker.ObserveReward(new TrainingRewardEvent(8, "placed", TrainingRewardEventType.BuildingCreated,
                "building-type:castle-01", validated: true, meaningful: true), AuthoritativeFacts());
            Assert.IsFalse(tracker.IsComplete);
            tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed,
                AuthoritativeFacts(settlements: 1, castles: 1, castleBuildings: 1));
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void GoldGainDoesNotSatisfyWoodProduction()
        {
            var step = new TrainingScenarioStepDefinition { id = "wood", criterion = TrainingScenarioCriterionKind.ResourceProduction,
                resourceId = "walnut-wood-materials-resources", buildingTypeId = "wood-camp", minProductionPerTurn = 1f };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(9, AuthoritativeFacts(woodStock: 5));
            tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed, AuthoritativeFacts(woodStock: 5, woodProduction: 0, turn: 1));
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void OneTimeStockInjectionDoesNotSatisfyStableEconomy()
        {
            var step = new TrainingScenarioStepDefinition { id = "stable", criterion = TrainingScenarioCriterionKind.StableResources,
                requiredTurns = 2, resources = new[] { new TrainingScenarioResourceCriterion {
                    resourceId = "walnut-wood-materials-resources", minStock = 10, minProductionPerTurn = 1 } } };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(10, AuthoritativeFacts());
            tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed, AuthoritativeFacts(woodStock: 100, woodProduction: 0, turn: 1));
            tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed, AuthoritativeFacts(woodStock: 100, woodProduction: 0, turn: 2));
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void DeployedLearnerUnitCompletesRecruitment()
        {
            var step = new TrainingScenarioStepDefinition { id = "recruit", criterion = TrainingScenarioCriterionKind.DeployedUnit, unitTypeId = "warrior" };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(11, AuthoritativeFacts(ownedUnits: 1, deployed: 1, warriors: 1));
            tracker.ObserveReward(new TrainingRewardEvent(11, "recruit", TrainingRewardEventType.UnitCreated,
                "unit-type:warrior", validated: true, meaningful: true), AuthoritativeFacts(ownedUnits: 2, deployed: 2, warriors: 2));
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void OpponentOrSetupUnitDoesNotCompleteRecruitment()
        {
            var step = new TrainingScenarioStepDefinition { id = "recruit", criterion = TrainingScenarioCriterionKind.DeployedUnit, unitTypeId = "warrior" };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(12, AuthoritativeFacts(ownedUnits: 1, deployed: 1, warriors: 1));
            tracker.ObserveReward(new TrainingRewardEvent(12, "opponent", TrainingRewardEventType.UnitCreated,
                "unit-type:warrior", validated: true, meaningful: true), AuthoritativeFacts(ownedUnits: 1, deployed: 1, warriors: 1));
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void RejectedMoveDoesNotIncreaseMovementProgress()
        {
            var step = new TrainingScenarioStepDefinition { id = "move", criterion = TrainingScenarioCriterionKind.Movement };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(13, AuthoritativeFacts(unitId: "u", cell: Vector2Int.zero));
            tracker.ObserveAction(BotIntentType.Move, BotExecutionStatus.Rejected, AuthoritativeFacts(unitId: "u", cell: Vector2Int.right));
            Assert.AreEqual(0f, tracker.Progress);
        }

        [Test]
        public void SuccessfulMoveWithoutScoutingOnlyCompletesMovementCriterion()
        {
            var move = new TrainingScenarioStepDefinition { id = "move", criterion = TrainingScenarioCriterionKind.Movement };
            var scout = new TrainingScenarioStepDefinition { id = "scout", criterion = TrainingScenarioCriterionKind.Scouting };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(move, scout));
            tracker.Begin(14, AuthoritativeFacts(explored: 5, unitId: "u", cell: Vector2Int.zero));
            tracker.ObserveAction(BotIntentType.Move, BotExecutionStatus.Completed,
                AuthoritativeFacts(explored: 5, unitId: "u", cell: Vector2Int.right));
            Assert.AreEqual(1, tracker.StepIndex);
            Assert.IsFalse(tracker.IsComplete);
        }

        [Test]
        public void CaptureRewardRequiresAuthoritativeOwnership()
        {
            var step = new TrainingScenarioStepDefinition { id = "capture", criterion = TrainingScenarioCriterionKind.ObjectiveOwned, objectiveType = "settlement" };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(15, AuthoritativeFacts());
            tracker.ObserveReward(new TrainingRewardEvent(15, "capture", TrainingRewardEventType.ObjectiveCaptured,
                "settlement-x", validated: true, meaningful: true), AuthoritativeFacts());
            Assert.IsFalse(tracker.IsComplete);
            tracker.ObserveReward(new TrainingRewardEvent(15, "capture2", TrainingRewardEventType.ObjectiveCaptured,
                "settlement-x", validated: true, meaningful: true), AuthoritativeFacts(objective: "settlement-x"));
            Assert.IsTrue(tracker.IsComplete);
        }

        [Test]
        public void SetupActionsHaveZeroScenarioProgress()
        {
            var step = new TrainingScenarioStepDefinition { id = "move", criterion = TrainingScenarioCriterionKind.Movement };
            var tracker = new TrainingScenarioProgressTracker(AuthoritativeScenario(step));
            tracker.Begin(16, AuthoritativeFacts(setup: true, unitId: "u", cell: Vector2Int.zero));
            tracker.ObserveAction(BotIntentType.Move, BotExecutionStatus.Completed,
                AuthoritativeFacts(setup: true, unitId: "u", cell: Vector2Int.right));
            Assert.AreEqual(0f, tracker.Progress);
            Assert.IsFalse(tracker.IsScoringActive);
        }
    }
}
