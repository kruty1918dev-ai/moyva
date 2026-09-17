using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    public sealed class TrainingCandidateContractTests
    {
        private const string Walnut = "walnut-wood-materials-resources";
        private const string Hardwood = "hardwood-materials-resources";

        // Scripted turn gateway + programmable capability let tests control the
        // exact number of real legal candidates each decision frame contains.

        private sealed class ScriptedTurnGateway : IBotTurnGateway
        {
            public long Turn = 1;
            public bool CanEnd = true;
            public int EndTurnCalls;
            public BotGameStamp Read(string player) => new BotGameStamp(player, Turn, 2, true);
            public bool CanEndTurn(string player, out string reason)
            { reason = CanEnd ? null : "EndTurn unavailable."; return CanEnd; }
            public bool EndTurn(string player, out string reason)
            {
                if (!CanEnd) { reason = "EndTurn unavailable."; return false; }
                reason = null; EndTurnCalls++; Turn++; return true;
            }
        }

        private sealed class ScriptedCapability : IBotCapabilityProvider
        {
            private readonly Queue<int> _counts;
            private int _last;
            private int _sequence;
            public ScriptedCapability(BotCapabilityId id, BotIntentType intent, params int[] counts)
            { Id = id; Intent = intent; _counts = new Queue<int>(counts); }
            public BotCapabilityId Id { get; }
            public BotIntentType Intent { get; }
            public string UnavailableReason(string player) => null;
            public IEnumerable<BotCandidateAction> Enumerate(string player)
            {
                int count = _counts.Count > 0 ? _last = _counts.Dequeue() : _last;
                for (int i = 0; i < count; i++)
                    yield return new BotCandidateAction("scripted:" + Id + ":" + ++_sequence + ":" + i, Id, Intent);
            }
            public bool Validate(string player, BotCandidateAction candidate, out string reason)
            { reason = null; return true; }
            public Task<BotExecutionResult> Execute(string player, BotCandidateAction candidate, CancellationToken token)
                => Task.FromResult(new BotExecutionResult(BotExecutionStatus.Completed, candidate));
        }

        private sealed class FakeSimulation : ITrainingSimulation, ITrainingBotRuntimeSource
        {
            public readonly ScriptedTurnGateway Gateway = new ScriptedTurnGateway();
            public ITurnService Turns => null;
            public string PlayerId => "player";
            public bool IsReady => true;
            public string Limitation => null;
            public IBotTurnGateway TurnGateway => Gateway;
            public BotCapabilityRegistry Capabilities { get; } = new BotCapabilityRegistry();
            public IBotPerceptionSource Perception { get; } = new EmptyBotPerceptionSource();
            public bool CanEndTurn() => Gateway.CanEnd;
            public bool Reset(TrainingResetContext context) => true;
            public void Dispose() { }
        }

        private static TrainingConfig Config()
        {
            var config = new TrainingConfig { allowScaffoldSimulation = true };
            config.curriculum.stage = TrainingCurriculumStage.FullGame;
            return config;
        }

        private static TrainingEnvironment NewEnvironment(FakeSimulation simulation, TrainingConfig config = null)
            => new TrainingEnvironment(0, config ?? Config(), simulation);

        // Test A — zero real legal candidates abort explicitly instead of a
        // synthetic "wait" forced action that ends in a timeout.
        [Test]
        public void ZeroRealCandidatesAbortWithNoLegalCandidates()
        {
            var simulation = new FakeSimulation();
            simulation.Gateway.CanEnd = false;
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            using var environment = NewEnvironment(simulation);
            environment.BeginEpisode();
            Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
            Assert.IsTrue(environment.Bridge.Frame.UsedNoLegalActionFallback);
            environment.Tick(0);
            Assert.AreEqual(TrainingEpisodeResult.InvalidState, environment.Result);
            StringAssert.Contains("no_legal_candidates", environment.Diagnostics.LastError);
            Assert.AreEqual(0, environment.Diagnostics.Decisions);
            Assert.AreEqual(0, environment.Diagnostics.ForcedActions);
        }

        // Test B — one real candidate is engine-forced: no policy decision, no
        // trainable step, no candidate accounting on the ML side.
        [Test]
        public void SingleRealCandidateIsEngineForcedWithoutTrainableDecision()
        {
            var simulation = new FakeSimulation();
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            var journal = new TrainingDecisionJournal();
            using var environment = NewEnvironment(simulation);
            environment.SetDecisionJournal(journal);
            environment.BeginEpisode();
            Assert.IsTrue(environment.CanRequestDecision);
            Assert.IsFalse(environment.HasTrainableDecision);
            environment.Tick(0);
            Assert.AreEqual(1, simulation.Gateway.EndTurnCalls);
            Assert.AreEqual(1, environment.Diagnostics.ForcedActions);
            Assert.AreEqual(0, environment.Diagnostics.Decisions);
            Assert.AreEqual(0, environment.Diagnostics.CandidateSum);
            var entry = journal.Query().LastOrDefault();
            Assert.IsNotNull(entry, "Forced action must be journaled as evidence.");
            Assert.IsTrue(entry.forcedAction);
            Assert.AreEqual(1, entry.candidateCount);
        }

        // Test B companion — a chain of single-candidate frames never produces a
        // trainable decision no matter how many ticks pass.
        [Test]
        public void ForcedSubmissionChainNeverCreatesMlDecisions()
        {
            var simulation = new FakeSimulation();
            simulation.Capabilities.Register(new ScriptedCapability(BotCapabilityId.Movement, BotIntentType.Move, 1));
            simulation.Gateway.CanEnd = false;
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            using var environment = NewEnvironment(simulation);
            environment.BeginEpisode();
            for (int i = 0; i < 3; i++)
            {
                environment.Tick(0);
                Assert.IsFalse(environment.HasTrainableDecision);
            }
            Assert.AreEqual(3, environment.Diagnostics.ForcedActions);
            Assert.AreEqual(0, environment.Diagnostics.Decisions);
            Assert.AreEqual(1f, environment.Diagnostics.ForcedActionRate);
        }

        // Test C — two or more real candidates expose a trainable decision.
        [Test]
        public void MultipleRealCandidatesExposeTrainableDecision()
        {
            var simulation = new FakeSimulation();
            simulation.Capabilities.Register(new ScriptedCapability(BotCapabilityId.Movement, BotIntentType.Move, 3));
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            using var environment = NewEnvironment(simulation);
            environment.BeginEpisode();
            environment.Tick(0);
            Assert.IsTrue(environment.CanRequestDecision);
            Assert.IsTrue(environment.HasTrainableDecision);
            Assert.AreEqual(0, environment.Diagnostics.ForcedActions);
            Assert.AreEqual(4, environment.Diagnostics.CandidateCount);
            Assert.IsTrue(environment.Step(0));
            Assert.AreEqual(1, environment.Diagnostics.Decisions);
            Assert.AreEqual(4, environment.Diagnostics.CandidateSum);
        }

        // Test D — candidate metrics aggregate over decisions; the final frame
        // alone cannot replace the per-decision mean.
        [Test]
        public void MeanCandidatesAggregatesPerDecisionNotFinalFrame()
        {
            var simulation = new FakeSimulation();
            simulation.Gateway.CanEnd = false;
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            simulation.Capabilities.Register(new ScriptedCapability(BotCapabilityId.Movement, BotIntentType.Move, 3, 5, 2));
            using var environment = NewEnvironment(simulation);
            environment.BeginEpisode();
            for (int decision = 0; decision < 3; decision++)
            {
                environment.Tick(0);
                Assert.IsTrue(environment.HasTrainableDecision, "Frame " + decision + " must be trainable.");
                Assert.IsTrue(environment.Step(0));
            }
            Assert.AreEqual(3, environment.Diagnostics.Decisions);
            Assert.AreEqual(10, environment.Diagnostics.CandidateSum);
            Assert.AreEqual(10f / 3f, environment.Diagnostics.MeanCandidates, 0.0001f);
            Assert.AreEqual(2, environment.Diagnostics.CandidateCount, "Final frame count must not rewrite the aggregate.");
        }

        // Test E — the synthetic no-legal-action fallback exposes zero real
        // candidates and is marked synthetic.
        [Test]
        public void SyntheticWaitFallbackHasZeroRealCandidates()
        {
            var turns = new ScriptedTurnGateway { CanEnd = false };
            var registry = new BotCapabilityRegistry();
            registry.Register(new EndTurnBotCapability(turns));
            var telemetry = new BotTelemetryHub();
            var builder = new BotDecisionFrameBuilder(registry, new EmptyBotPerceptionSource(), turns,
                telemetry, new BotRuntimeConfig { curriculumStage = (int)TrainingCurriculumStage.FullGame });
            var frame = builder.Build("player");
            Assert.AreEqual(1, frame.Candidates.Count);
            Assert.AreEqual(0, frame.RealCandidateCount);
            Assert.IsTrue(frame.UsedNoLegalActionFallback);
            Assert.IsFalse(frame.HasRealLegalCandidates);
            Assert.IsTrue(frame.Candidates[0].Synthetic);
            Assert.AreEqual(1, telemetry.NoLegalActionFallbackCount);
        }

        // The journal must surface the synthetic fallback without counting it as
        // a real candidate.
        [Test]
        public void SyntheticFallbackIsVisibleInDecisionJournal()
        {
            var simulation = new FakeSimulation();
            simulation.Gateway.CanEnd = false;
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            var journal = new TrainingDecisionJournal();
            using var environment = NewEnvironment(simulation);
            environment.SetDecisionJournal(journal);
            environment.BeginEpisode();
            Assert.IsTrue(environment.Step(0));
            var entry = journal.Query().LastOrDefault();
            Assert.IsNotNull(entry);
            Assert.AreEqual(0, entry.candidateCount);
            Assert.IsTrue(entry.syntheticFallback);
            Assert.IsFalse(entry.forcedAction);
        }

        // Test F — scaffolded castle/unit/resources/building must not advance
        // the corresponding scenario steps.
        [Test]
        public void ScaffoldedStateDoesNotAdvanceScenarioSteps()
        {
            var steps = new[]
            {
                new TrainingScenarioStepDefinition { id = "castle", criterion = TrainingScenarioCriterionKind.OperationalCastle, buildingTypeId = "castle-01" },
                new TrainingScenarioStepDefinition { id = "production", criterion = TrainingScenarioCriterionKind.ResourceProduction,
                    resourceId = Walnut, buildingTypeId = "wood-camp", minProductionPerTurn = 1f },
                new TrainingScenarioStepDefinition { id = "stable", criterion = TrainingScenarioCriterionKind.StableResources,
                    requiredTurns = 1, resources = new[] { new TrainingScenarioResourceCriterion
                        { resourceId = Walnut, minStock = 1f, minProductionPerTurn = 1f } } },
                new TrainingScenarioStepDefinition { id = "recruit", criterion = TrainingScenarioCriterionKind.DeployedUnit, unitTypeId = "warrior" }
            };
            var tracker = new TrainingScenarioProgressTracker(Scenario(steps));
            var scaffolded = Facts(setup: true, settlements: 1, castles: 1, units: 1,
                walnutStock: 100, walnutProduction: 6, buildings: new Dictionary<string, int>
                { ["castle-01"] = 1, ["wood-camp"] = 1 });
            tracker.Begin(1, scaffolded);
            tracker.ObserveReward(new TrainingRewardEvent(1, "setup-castle", TrainingRewardEventType.BuildingCreated,
                "building-type:castle-01", validated: true, meaningful: true), scaffolded);
            tracker.ObserveReward(new TrainingRewardEvent(1, "setup-recruit", TrainingRewardEventType.UnitCreated,
                "unit-type:warrior", validated: true, meaningful: true), scaffolded);
            tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed, Facts(setup: true, turn: 1,
                settlements: 1, castles: 1, units: 1, walnutStock: 100, walnutProduction: 6,
                buildings: new Dictionary<string, int> { ["castle-01"] = 1, ["wood-camp"] = 1 }));
            Assert.IsFalse(tracker.IsScoringActive);
            Assert.AreEqual(0, tracker.StepIndex);
            Assert.AreEqual(0f, tracker.Progress);
        }

        // Test G — a capture step cannot pass watchdog learnability while no real
        // Capture path is ever offered.
        [Test]
        public void CaptureStepRequiresRealCaptureCandidates()
        {
            var scenario = new TrainingScenarioDefinition
            {
                id = "capture", title = "capture", fullGame = false,
                startingConditions = new TrainingScenarioStartingConditions(),
                availableCapabilities = new[] { "movement", "capture", "end-turn" },
                generationConstraints = new TrainingScenarioGenerationConstraints(),
                rewardRules = new TrainingScenarioRewardRules(),
                minMeaningfulCandidates = 0,
                requiredIntents = new[] { "Move", "Capture", "EndTurn" },
                steps = new[] { new TrainingScenarioStepDefinition { id = "capture",
                    criterion = TrainingScenarioCriterionKind.ObjectiveOwned, objectiveType = "settlement" } }
            };
            var simulation = new FakeSimulation();
            simulation.Capabilities.Register(new ScriptedCapability(BotCapabilityId.Movement, BotIntentType.Move, 2));
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            var config = Config();
            config.watchdogWindow = 4;
            using var environment = new TrainingEnvironment(0, config, simulation);
            environment.SetScenario(scenario);
            environment.BeginEpisode();
            for (int i = 0; i < 8 && environment.IsReady; i++)
            {
                Assert.IsTrue(environment.Step(0));
                environment.Tick(0);
            }
            Assert.AreEqual(TrainingEpisodeResult.InvalidState, environment.Result);
            StringAssert.Contains("required_intent_missing", environment.Diagnostics.LastError);
            StringAssert.Contains("Capture", environment.Diagnostics.LastError);
        }

        // Step-aware watchdog: a later step's required intent (Attack on the
        // combat step) must not abort an earlier movement step where it is
        // legitimately unavailable.
        [Test]
        public void PerStepWatchdogDoesNotRequireFutureStepIntents()
        {
            var scenario = new TrainingScenarioDefinition
            {
                id = "combo-field-ops", title = "combo", fullGame = false,
                startingConditions = new TrainingScenarioStartingConditions(),
                availableCapabilities = new[] { "movement", "combat", "end-turn" },
                generationConstraints = new TrainingScenarioGenerationConstraints(),
                rewardRules = new TrainingScenarioRewardRules(),
                minMeaningfulCandidates = 0,
                requiredIntents = new[] { "Move", "Attack", "EndTurn" },
                steps = new[]
                {
                    new TrainingScenarioStepDefinition { id = "movement",
                        criterion = TrainingScenarioCriterionKind.Movement,
                        requiredIntents = new[] { "Move", "EndTurn" } },
                    new TrainingScenarioStepDefinition { id = "combat",
                        criterion = TrainingScenarioCriterionKind.EnemyDestroyed,
                        requiredIntents = new[] { "Move", "Attack", "EndTurn" } }
                }
            };
            var simulation = new FakeSimulation();
            simulation.Capabilities.Register(new ScriptedCapability(BotCapabilityId.Movement, BotIntentType.Move, 2));
            simulation.Capabilities.Register(new EndTurnBotCapability(simulation.Gateway));
            var config = Config();
            config.watchdogWindow = 4;
            using var environment = new TrainingEnvironment(0, config, simulation);
            environment.SetScenario(scenario);
            environment.BeginEpisode();
            // Two full watchdog windows on the movement step; Attack is never
            // offered, yet the step-level contract ([Move, EndTurn]) passes.
            for (int i = 0; i < 8 && environment.IsReady; i++)
            {
                Assert.IsTrue(environment.Step(0));
                environment.Tick(0);
            }
            Assert.IsTrue(environment.IsReady, environment.Diagnostics.LastError);
            Assert.AreEqual(TrainingEpisodeResult.None, environment.Result);
        }

        // The authored catalog encodes the required action paths: capture needs a
        // real Capture path, the combo combat step needs Attack at that step.
        [Test]
        public void CatalogContractsEncodeRequiredPaths()
        {
            var catalog = TrainingScenarioCatalog.BuiltIn();
            var capture = catalog.Get("capture");
            Assert.IsNotNull(capture);
            CollectionAssert.Contains(capture.requiredIntents, "Capture");
            var combo = catalog.Get("combo-field-ops");
            Assert.IsNotNull(combo);
            CollectionAssert.Contains(combo.requiredIntents, "Attack");
            var combatStep = combo.steps.Single(step => step.id == "combat");
            CollectionAssert.Contains(combatStep.requiredIntents, "Attack");
            var moveStep = combo.steps.Single(step => step.id == "movement");
            CollectionAssert.DoesNotContain(moveStep.requiredIntents, "Attack");
            var stable = catalog.Get("stable-economy");
            Assert.IsTrue(stable.steps[0].resources.Any(resource =>
                    resource.resourceId == Hardwood && resource.minProductionPerTurn > 0f),
                "stable-economy must require production the scaffolded state lacks.");
        }

        // Test H — repeated successful training episodes must not master
        // evaluation-only gameplay skills.
        [Test]
        public void TrainingSuccessesCannotMasterEvaluationOnlySkills()
        {
            string path = Path.Combine(Path.GetTempPath(), "moyva-mastery-" + System.Guid.NewGuid().ToString("N") + ".json");
            try
            {
                using var controller = new TrainingCurriculumController(new AutonomousTrainingConfig { statePath = path }, 1918);
                string[] evaluationSkills = { "castle", "production", "stable-economy", "recruitment",
                    "movement-scouting", "combat-defense", "capture", "combo-economy-recruitment", "combo-field-ops" };
                foreach (var id in evaluationSkills)
                    for (int i = 0; i < 5; i++)
                        controller.RecordTrainingEpisode(id, success: true, decisions: 10);
                foreach (var id in evaluationSkills)
                    Assert.IsFalse(controller.GetSkill(id).mastered, id + " mastered without frozen evaluation.");
                controller.RecordTrainingEpisode("foundation-legal-setup", success: true, decisions: 1);
                Assert.IsTrue(controller.GetSkill("foundation-legal-setup").mastered,
                    "Bootstrap-qualified scenario must still master from training.");
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        // Test I — the scaffolded economy (wood-camp already producing walnut)
        // cannot complete stable-economy; the learner must add production that
        // the starting state lacks (sawmill → hardwood) and hold it.
        [Test]
        public void StableEconomyRequiresLearnerProductionExpansion()
        {
            var scenario = TrainingScenarioCatalog.BuiltIn().Get("stable-economy");
            Assert.IsNotNull(scenario);
            var step = scenario.steps[0];
            Assert.IsTrue(step.resources.Any(resource => resource.resourceId == Hardwood
                    && resource.minProductionPerTurn > 0f),
                "stable-economy must require production the scaffolded state cannot satisfy.");

            var tracker = new TrainingScenarioProgressTracker(scenario);
            var buildings = new Dictionary<string, int> { ["castle-01"] = 1, ["wood-camp"] = 1 };
            tracker.Begin(1, Facts(settlements: 1, castles: 1, walnutStock: 30, walnutProduction: 6,
                hardwoodStock: 20, buildings: buildings));
            for (int turn = 1; turn <= 5; turn++)
                tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed,
                    Facts(turn: turn, settlements: 1, castles: 1, walnutStock: 30 + turn * 6,
                        walnutProduction: 6, hardwoodStock: 20, buildings: buildings));
            Assert.IsFalse(tracker.IsComplete, "Starting state alone must never complete stable-economy.");

            var expanded = new Dictionary<string, int> { ["castle-01"] = 1, ["wood-camp"] = 1, ["sawmill"] = 1 };
            for (int turn = 6; turn <= 6 + step.requiredTurns; turn++)
                tracker.ObserveAction(BotIntentType.EndTurn, BotExecutionStatus.Completed,
                    Facts(turn: turn, settlements: 1, castles: 1, walnutStock: 60,
                        walnutProduction: 6, hardwoodStock: 20, hardwoodProduction: 3, buildings: expanded));
            Assert.IsTrue(tracker.IsComplete, "Learner-added production must satisfy the stable-economy step.");
        }

        private static TrainingScenarioDefinition Scenario(params TrainingScenarioStepDefinition[] steps)
        {
            return new TrainingScenarioDefinition
            {
                id = "test", title = "test", learnerBuildsInitialCastle = false,
                startingConditions = new TrainingScenarioStartingConditions(),
                availableCapabilities = new[] { "construction", "economy", "recruitment", "movement",
                    "scouting", "combat", "capture", "end-turn" },
                generationConstraints = new TrainingScenarioGenerationConstraints(),
                rewardRules = new TrainingScenarioRewardRules(),
                steps = steps
            };
        }

        private static TrainingScenarioFacts Facts(bool setup = false, int settlements = 0, int castles = 0,
            int units = 0, float walnutStock = 0f, float walnutProduction = 0f, float hardwoodStock = 0f,
            float hardwoodProduction = 0f, int turn = 0, Dictionary<string, int> buildings = null)
        {
            var stock = new Dictionary<string, float> { [Walnut] = walnutStock, [Hardwood] = hardwoodStock };
            var production = new Dictionary<string, float>();
            if (walnutProduction > 0f) production[Walnut] = walnutProduction;
            if (hardwoodProduction > 0f) production[Hardwood] = hardwoodProduction;
            return new TrainingScenarioFacts(setup, settlements, castles, units, units, stock, production,
                0, null, turn, 0, null, buildings, null, null,
                recruitedUnitsByType: null);
        }
    }
}
