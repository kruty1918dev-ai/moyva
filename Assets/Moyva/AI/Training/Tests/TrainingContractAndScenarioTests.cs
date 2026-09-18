using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.AI.Bot;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Tests
{
    // Contract spec (MoyvaBotContract.json) is the single source of truth for the
    // observation/action schema; these tests prove the C# constants and the
    // loaded spec cannot drift apart, and that scenario metadata drives the
    // curriculum instead of code literals.
    public sealed class TrainingContractAndScenarioTests
    {
        private const string Walnut = "walnut-wood-materials-resources";

        private static BotContractSpec ValidSpec() => new BotContractSpec
        {
            contractVersion = 2, observationSchemaVersion = 3, candidateSchemaVersion = 2,
            actionSchemaVersion = 1, maxCandidateSlots = 4, globalFeatureCount = 8,
            candidateFeatureCount = 2, spatialSize = 2, spatialChannels = 1,
            global = new[]
            {
                new BotContractFeature { index = 0, name = "a" },
                new BotContractFeature { index = 1, name = "b", count = 3 },
            },
            candidate = new[] { "x" },
            intents = new[] { "None" },
        };

        [Test]
        public void ContractSpecLoadsFromResources()
        {
            var spec = BotDecisionContract.Spec;
            Assert.NotNull(spec);
            Assert.AreEqual(BotDecisionContract.ContractVersion, spec.contractVersion);
            Assert.AreEqual(BotDecisionContract.ObservationSchemaVersion, spec.observationSchemaVersion);
            Assert.AreEqual(BotDecisionContract.MaxCandidateSlots, spec.maxCandidateSlots);
            Assert.AreEqual(BotDecisionContract.GlobalFeatureCount, spec.globalFeatureCount);
            Assert.AreEqual(BotDecisionContract.ObservationCount, spec.ObservationCount);
        }

        [Test]
        public void ContractHashIsStableSha256()
        {
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(
                BotDecisionContract.Hash, "^[0-9a-f]{64}$"));
            Assert.AreEqual(BotDecisionContract.Hash, BotDecisionContract.Hash);
        }

        [Test]
        public void SchemaConstantsMatchSpecSlots()
        {
            var constants = typeof(BotObservationSchema)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && !field.IsInitOnly)
                .ToDictionary(field => field.Name, field => (int)field.GetRawConstantValue());
            foreach (var feature in BotDecisionContract.Spec.global)
            {
                string name = char.ToUpperInvariant(feature.name[0]) + feature.name.Substring(1);
                Assert.IsTrue(constants.TryGetValue(name, out int index),
                    "Spec slot '" + feature.name + "' has no BotObservationSchema constant.");
                Assert.AreEqual(feature.index, index, "Slot '" + feature.name + "' drifted from the spec.");
            }
        }

        [Test]
        public void GoalAndTacticalSlotsAreDistinct()
        {
            Assert.AreEqual(34, BotObservationSchema.GoalCastle);
            Assert.AreEqual(42, BotObservationSchema.GoalWin);
            Assert.AreEqual(43, BotObservationSchema.TacticalVision);
            Assert.AreEqual(44, BotObservationSchema.TacticalAttack);
            Assert.AreEqual(45, BotObservationSchema.TacticalHeight);
        }

        [Test]
        public void SpecValidationRejectsOverlappingSlots()
        {
            var spec = ValidSpec();
            spec.global[1].index = 0; // collides with feature "a"
            Assert.Throws<ArgumentException>(() => spec.Validate());
        }

        [Test]
        public void SpecValidationRejectsDuplicateNames()
        {
            var spec = ValidSpec();
            spec.global[1].name = "a";
            Assert.Throws<ArgumentException>(() => spec.Validate());
        }

        [Test]
        public void SpecValidationRejectsOutOfRangeSlots()
        {
            var spec = ValidSpec();
            spec.global[0].index = spec.globalFeatureCount;
            Assert.Throws<ArgumentException>(() => spec.Validate());
            spec = ValidSpec();
            spec.global[1].count = spec.globalFeatureCount; // index 1 + count overflows
            Assert.Throws<ArgumentException>(() => spec.Validate());
        }

        [Test]
        public void CapabilityMapParsesCanonicalAndAliasNames()
        {
            Assert.IsTrue(BotCapabilityMap.TryParse("end-turn", out var turn));
            Assert.AreEqual(BotCapabilityId.Turn, turn);
            Assert.IsTrue(BotCapabilityMap.TryParse("scouting", out var explore));
            Assert.AreEqual(BotCapabilityId.Exploration, explore);
            Assert.IsTrue(BotCapabilityMap.TryParse("MOVEMENT", out var movement));
            Assert.AreEqual(BotCapabilityId.Movement, movement);
            Assert.IsFalse(BotCapabilityMap.TryParse("teleport", out _));
            Assert.IsFalse(BotCapabilityMap.TryParse("", out _));
            Assert.IsFalse(BotCapabilityMap.TryParse(null, out _));
        }

        [Test]
        public void CapabilityMaskAlwaysKeepsEndTurnLegal()
        {
            Assert.AreEqual(1 << (int)BotCapabilityId.Turn, BotCapabilityMap.Mask(Array.Empty<string>()));
            int mask = BotCapabilityMap.Mask(new[] { "movement", "combat" });
            Assert.AreNotEqual(0, mask & (1 << (int)BotCapabilityId.Turn));
            Assert.AreNotEqual(0, mask & (1 << (int)BotCapabilityId.Movement));
            Assert.AreNotEqual(0, mask & (1 << (int)BotCapabilityId.Combat));
            Assert.AreEqual(0, mask & (1 << (int)BotCapabilityId.Construction));
        }

        [Test]
        public void IntentCapabilityMappingIsComplete()
        {
            Assert.AreEqual(BotCapabilityId.Turn, BotCapabilityMap.ForIntent(BotIntentType.EndTurn));
            Assert.AreEqual(BotCapabilityId.Movement, BotCapabilityMap.ForIntent(BotIntentType.Move));
            Assert.AreEqual(BotCapabilityId.Combat, BotCapabilityMap.ForIntent(BotIntentType.Attack));
            Assert.AreEqual(BotCapabilityId.Construction, BotCapabilityMap.ForIntent(BotIntentType.Build));
            Assert.IsNull(BotCapabilityMap.ForIntent(BotIntentType.None));
        }

        [Test]
        public void RequiredIntentsCoverEveryCriterionStep()
        {
            Assert.Contains(BotIntentType.Build,
                TrainingScenarioContract.RequiredIntentsFor(TrainingScenarioCriterionKind.OperationalCastle));
            Assert.Contains(BotIntentType.Move,
                TrainingScenarioContract.RequiredIntentsFor(TrainingScenarioCriterionKind.Movement));
            Assert.Contains(BotIntentType.Attack,
                TrainingScenarioContract.RequiredIntentsFor(TrainingScenarioCriterionKind.EnemyDestroyed));
            Assert.IsEmpty(TrainingScenarioContract.RequiredIntentsFor(null));
        }

        private static TrainingScenarioStepDefinition Step(
            string id, TrainingScenarioCriterionKind criterion, string buildingTypeId = null)
            => new TrainingScenarioStepDefinition { id = id, criterion = criterion, buildingTypeId = buildingTypeId };

        private static TrainingScenarioDefinition Scenario(
            string id, string[] capabilities = null, string[] prerequisites = null,
            string[] requiredIntents = null, bool combination = false, bool fullGame = false,
            TrainingScenarioMasteryKind mastery = TrainingScenarioMasteryKind.Evaluation,
            params TrainingScenarioStepDefinition[] steps)
        {
            return new TrainingScenarioDefinition
            {
                id = id,
                prerequisites = prerequisites ?? Array.Empty<string>(),
                startingConditions = new TrainingScenarioStartingConditions(),
                availableCapabilities = capabilities ?? new[] { "end-turn", "movement" },
                generationConstraints = new TrainingScenarioGenerationConstraints(),
                rewardRules = new TrainingScenarioRewardRules(),
                requiredIntents = requiredIntents ?? Array.Empty<string>(),
                combination = combination,
                fullGame = fullGame,
                masteryPolicy = new TrainingScenarioMasteryPolicy { kind = mastery },
                steps = steps.Length > 0 ? steps : new[] { Step("step-1", TrainingScenarioCriterionKind.Movement) },
            };
        }

        [Test]
        public void CatalogCurriculumFollowsManifestOrder()
        {
            var a = Scenario("a");
            var b = Scenario("b");
            var catalog = new TrainingScenarioCatalog(new[] { a, b }, new[] { "b", "a" });
            Assert.AreEqual(new[] { "b", "a" }, catalog.Curriculum.Select(s => s.id).ToArray());
        }

        [Test]
        public void CatalogRejectsManifestGaps()
        {
            var a = Scenario("a");
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[] { a }, new[] { "a", "missing" }));
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[] { a }, new[] { "a", "a" }));
        }

        [Test]
        public void CatalogRejectsCombinationInCurriculumOrder()
        {
            var a = Scenario("a");
            var combo = Scenario("combo", combination: true, prerequisites: new[] { "a" });
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[] { a, combo }, new[] { "a", "combo" }));
        }

        [Test]
        public void CombinationScenariosRequirePrerequisitesAndAreNotFullGame()
        {
            var a = Scenario("a");
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[] { a, Scenario("combo", combination: true) }));
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[]
                {
                    a, Scenario("combo", combination: true, fullGame: true, prerequisites: new[] { "a" }),
                }));
        }

        [Test]
        public void CatalogExposesCombinationsSeparately()
        {
            var a = Scenario("a");
            var combo = Scenario("combo", combination: true, prerequisites: new[] { "a" });
            var catalog = new TrainingScenarioCatalog(new[] { a, combo }, new[] { "a" });
            Assert.AreEqual(new[] { "combo" }, catalog.Combinations.Select(s => s.id).ToArray());
            Assert.AreEqual(new[] { "a" }, catalog.Curriculum.Select(s => s.id).ToArray());
        }

        [Test]
        public void CatalogRejectsDuplicateAndDanglingScenarios()
        {
            var a = Scenario("a");
            Assert.Throws<ArgumentException>(() => new TrainingScenarioCatalog(new[] { a, Scenario("a") }));
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[] { Scenario("b", prerequisites: new[] { "missing" }) }));
        }

        [Test]
        public void ScenarioRejectsUnknownCapabilityNames()
        {
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[] { Scenario("a", capabilities: new[] { "teleport" }) }));
        }

        [Test]
        public void ScenarioRejectsUnproducibleRequiredIntent()
        {
            // "movement" cannot produce Attack intents (needs Combat capability).
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[]
                {
                    Scenario("a", capabilities: new[] { "end-turn", "movement" },
                        requiredIntents: new[] { "Attack" }),
                }));
        }

        [Test]
        public void ScenarioRejectsUnreachableStepCriterion()
        {
            // OperationalCastle requires construction capability, which a
            // movement-only scenario can never produce.
            Assert.Throws<ArgumentException>(() =>
                new TrainingScenarioCatalog(new[]
                {
                    Scenario("a", capabilities: new[] { "movement" },
                        steps: new[] { Step("build", TrainingScenarioCriterionKind.OperationalCastle) }),
                }));
        }

        [Test]
        public void BuiltInCatalogLoadsPresetsAndManifestOrder()
        {
            var catalog = TrainingScenarioCatalog.BuiltIn();
            string path = Path.Combine(Application.dataPath, "Moyva", "Presets", "AI", "Scenarios",
                TrainingScenarioCatalog.ManifestFileName);
            var manifest = JsonUtility.FromJson<TrainingScenarioManifest>(File.ReadAllText(path));
            Assert.AreEqual(manifest.curriculum, catalog.Curriculum.Select(s => s.id).ToArray());
            Assert.IsNotNull(catalog.FullGameScenario);
            Assert.IsTrue(catalog.FullGameScenario.fullGame);
            Assert.AreSame(catalog.Curriculum.Last(), catalog.FullGameScenario);
            Assert.IsNotNull(catalog.BootstrapScenario);
            Assert.GreaterOrEqual(catalog.Combinations.Count, 3);
            Assert.IsTrue(catalog.Combinations.All(s => (s.prerequisites?.Length ?? 0) > 0));
        }

        [Test]
        public void TrainingConfigRequiresCastleBuildingTypeId()
        {
            var config = new TrainingConfig { castleBuildingTypeId = "" };
            Assert.Throws<ArgumentException>(() => config.Validate());
            new TrainingConfig().Validate(); // default preset values stay valid
        }

        [Test]
        public void ObjectiveSetupCastleIdResolvesThroughConfig()
        {
            Assert.AreEqual("", new TrainingScenarioObjectiveSetup().buildingTypeId);
        }

        [Test]
        public void EveryIntentResolvesACapability()
        {
            foreach (BotIntentType intent in Enum.GetValues(typeof(BotIntentType)))
            {
                if (intent == BotIntentType.None) continue;
                Assert.IsNotNull(BotCapabilityMap.ForIntent(intent),
                    $"Intent {intent} has no capability mapping — new mechanics must extend BotCapabilityMap.");
            }
        }

        [Test]
        public void EveryCapabilityIsExercisedByAScenario()
        {
            var catalog = TrainingScenarioCatalog.BuiltIn();
            var covered = new HashSet<BotCapabilityId>();
            foreach (var scenario in catalog.Items)
            foreach (var name in scenario.availableCapabilities ?? Array.Empty<string>())
                if (BotCapabilityMap.TryParse(name, out var id))
                    covered.Add(id);
            foreach (BotCapabilityId capability in Enum.GetValues(typeof(BotCapabilityId)))
                Assert.IsTrue(covered.Contains(capability),
                    $"Capability {capability} has no training scenario — new mechanics need scenario coverage.");
        }

        [Test]
        public void ResourceFeaturesCarryGameAliases()
        {
            var spec = BotDecisionContract.Spec;
            foreach (int index in new[]
                     {
                         BotObservationSchema.ResourceFood, BotObservationSchema.ResourceWood,
                         BotObservationSchema.ResourceStone, BotObservationSchema.ResourceIron,
                         BotObservationSchema.ResourceGold
                     })
                Assert.IsNotEmpty(spec.Aliases(index),
                    $"Resource feature at slot {index} must declare game-side key aliases in the contract spec.");
        }

        [Test]
        public void SpecLabelsDeriveFromContract()
        {
            var spec = BotDecisionContract.Spec;
            Assert.AreEqual("active", spec.Label(BotObservationSchema.Active));
            Assert.AreEqual("food", spec.Label(BotObservationSchema.ResourceFood)); // first alias
            Assert.AreEqual("capabilities", spec.Label(BotObservationSchema.Capabilities));
            Assert.AreEqual("95", spec.Label(95)); // uncovered slot falls back to the index
        }

        [Test]
        public void CurriculumReviewNeverRepeatsActiveScenario()
        {
            string statePath = Path.Combine(Path.GetTempPath(),
                "moyva-curriculum-test-" + Guid.NewGuid().ToString("N") + ".json");
            var config = new AutonomousTrainingConfig
            {
                fullGameWeightAfterBasics = 0,
                weakSkillWeightAfterBasics = 0,
                reviewWeightAfterBasics = 1,
                statePath = statePath
            };
            try
            {
                using (var controller = new TrainingCurriculumController(config, 7))
                {
                    var curriculum = controller.State.skills;
                    Assert.GreaterOrEqual(curriculum.Count(s => s.scenarioId != null), 3,
                        "Test needs several curriculum scenarios to exercise recency.");
                    foreach (var skill in curriculum) skill.mastered = true;
                    controller.State.activeScenarioId = curriculum[0].scenarioId;

                    string previous = controller.State.activeScenarioId;
                    for (int i = 0; i < 12; i++)
                    {
                        var pick = controller.ChooseNext();
                        Assert.IsNotNull(pick);
                        Assert.AreNotEqual(previous, pick.id,
                            "Review pick must never replay the just-finished scenario.");
                        previous = pick.id;
                    }
                }
            }
            finally
            {
                if (File.Exists(statePath)) File.Delete(statePath);
            }
        }

        [Test]
        public void OpponentPoolEnrollsVerifiedCheckpointsNewestFirst()
        {
            string statePath = Path.Combine(Path.GetTempPath(),
                "moyva-curriculum-test-" + Guid.NewGuid().ToString("N") + ".json");
            var config = new AutonomousTrainingConfig { statePath = statePath };
            try
            {
                using (var controller = new TrainingCurriculumController(config, 3))
                {
                    string scenario = controller.State.skills[0].scenarioId;
                    for (int i = 0; i < 10; i++)
                        controller.RecordEvaluation(scenario, config.evaluationEpisodes,
                            config.evaluationEpisodes, i, $"checkpoint-{i}", $"eval-{i}");
                    var pool = controller.State.opponentPool;
                    Assert.AreEqual(8, pool.Length, "Opponent pool is bounded at 8 verified checkpoints.");
                    Assert.AreEqual("checkpoint-9", pool[0], "Newest verified checkpoint must lead the pool.");
                    Assert.AreEqual(pool.Distinct().Count(), pool.Length, "Pool must not contain duplicates.");
                }
            }
            finally
            {
                if (File.Exists(statePath)) File.Delete(statePath);
            }
        }
    }
}
