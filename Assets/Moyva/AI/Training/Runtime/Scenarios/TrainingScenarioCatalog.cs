using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    internal sealed class TrainingScenarioJsonEnvelope { public TrainingScenarioDefinition scenario; }

    public sealed class TrainingScenarioCatalog
    {
        private static readonly HashSet<string> ScenarioResourceFallback = new HashSet<string>(StringComparer.Ordinal)
        {
            "walnut-wood-materials-resources"
        };

        private readonly Dictionary<string, TrainingScenarioDefinition> _items;
        public IReadOnlyCollection<TrainingScenarioDefinition> Items => _items.Values;

        public TrainingScenarioCatalog(IEnumerable<TrainingScenarioDefinition> definitions)
        {
            var all = (definitions ?? Array.Empty<TrainingScenarioDefinition>()).ToArray();
            var ids = new HashSet<string>(all.Where(x => x != null && !string.IsNullOrWhiteSpace(x.id)).Select(x => x.id),
                StringComparer.Ordinal);
            _items = new Dictionary<string, TrainingScenarioDefinition>(StringComparer.Ordinal);
            foreach (var definition in all)
            {
                Validate(definition, ids);
                if (_items.ContainsKey(definition.id)) throw new ArgumentException("Duplicate scenario: " + definition.id);
                _items.Add(definition.id, definition);
            }
        }

        public TrainingScenarioDefinition Get(string id)
            => !string.IsNullOrWhiteSpace(id) && _items.TryGetValue(id, out var value) ? value : null;

        public static void Validate(TrainingScenarioDefinition scenario, IReadOnlyCollection<string> knownScenarioIds = null)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            scenario.Validate();

            var knownResources = KnownResourceIds();
            foreach (var resourceId in EnumerateResourceIds(scenario))
                if (!knownResources.Contains(resourceId))
                    throw new ArgumentException($"Scenario {scenario.id} references unknown resource '{resourceId}'.");

            if (knownScenarioIds != null)
                foreach (var prerequisite in scenario.prerequisites ?? Array.Empty<string>())
                    if (string.IsNullOrWhiteSpace(prerequisite) || !knownScenarioIds.Contains(prerequisite))
                        throw new ArgumentException($"Scenario {scenario.id} depends on missing {prerequisite}.");

            if (scenario.fullGame)
                ValidateFullGameSequence(scenario);
        }

        public static TrainingScenarioCatalog BuiltIn()
        {
            var presets = TryLoadPresetDefinitions();
            return presets != null
                ? new TrainingScenarioCatalog(presets)
                : new TrainingScenarioCatalog(FallbackDefinitions());
        }

        private static TrainingScenarioDefinition[] TryLoadPresetDefinitions()
        {
            string configured = Environment.GetEnvironmentVariable("MOYVA_SCENARIO_DIR");
            if (!string.IsNullOrWhiteSpace(configured))
            {
                string configuredPath = Path.GetFullPath(configured);
                if (!Directory.Exists(configuredPath))
                    throw new DirectoryNotFoundException("MOYVA_SCENARIO_DIR does not exist: " + configuredPath);
                return LoadPresetDirectory(configuredPath);
            }

            if (!string.IsNullOrWhiteSpace(Application.dataPath))
            {
                string editorPath = Path.Combine(Application.dataPath, "Moyva", "Presets", "AI", "Scenarios");
                if (Directory.Exists(editorPath))
                    return LoadPresetDirectory(editorPath);
            }
            return null;
        }

        private static TrainingScenarioDefinition[] LoadPresetDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            if (files.Length == 0)
                throw new InvalidOperationException("No training scenario JSON files found in: " + directory);

            var scenarios = files.Select(path => ParseJson(File.ReadAllText(path), validateResources: false)).ToArray();
            string[] required =
            {
                "foundation-legal-setup", "castle", "production", "stable-economy", "recruitment",
                "movement-scouting", "combat-defense", "capture", "full-game-autonomous",
                "combo-economy-recruitment", "combo-field-ops"
            };
            string[] missing = required.Where(id => scenarios.All(s => !string.Equals(s.id, id, StringComparison.Ordinal))).ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException(
                    "Training scenario preset directory is missing required scenarios: " + string.Join(", ", missing));

            // Cross-scenario validation is intentionally done after all IDs are known.
            var ids = new HashSet<string>(scenarios.Select(s => s.id), StringComparer.Ordinal);
            foreach (var scenario in scenarios) Validate(scenario, ids);

            Debug.Log("MOYVA_SCENARIOS_LOADED path=" + directory + " count=" + scenarios.Length);
            return scenarios;
        }

        public static TrainingScenarioDefinition ParseJson(string json)
            => ParseJson(json, validateResources: true);

        private static TrainingScenarioDefinition ParseJson(string json, bool validateResources)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Scenario JSON is empty.");
            var envelope = JsonUtility.FromJson<TrainingScenarioJsonEnvelope>(json);
            var result = envelope?.scenario;
            if (result == null) throw new ArgumentException("Scenario JSON must contain a 'scenario' object.");
            if (validateResources) Validate(result);
            else result.Validate();
            return result;
        }

        private static HashSet<string> KnownResourceIds()
        {
            var result = new HashSet<string>(ScenarioResourceFallback, StringComparer.Ordinal);
            try
            {
                foreach (var resource in MoyvaJsonRuntime.GetAll<EconomyResourceDefinition>() ?? Array.Empty<EconomyResourceDefinition>())
                    if (resource != null && !string.IsNullOrWhiteSpace(resource.Id)) result.Add(resource.Id);
            }
            catch
            {
                // ParseJson must remain deterministic in pure unit tests where the config runtime is not initialized.
            }
            return result;
        }

        private static IEnumerable<string> EnumerateResourceIds(TrainingScenarioDefinition scenario)
        {
            if (scenario.startingConditions?.startingResources != null)
                foreach (var resource in scenario.startingConditions.startingResources)
                    if (resource != null && !string.IsNullOrWhiteSpace(resource.resourceId)) yield return resource.resourceId;
            if (scenario.generationConstraints?.requiredResourceTypes != null)
                foreach (var resource in scenario.generationConstraints.requiredResourceTypes)
                    if (!string.IsNullOrWhiteSpace(resource)) yield return resource;
            foreach (var step in scenario.steps ?? Array.Empty<TrainingScenarioStepDefinition>())
            {
                if (!string.IsNullOrWhiteSpace(step.resourceId)) yield return step.resourceId;
                foreach (var resource in step.resources ?? Array.Empty<TrainingScenarioResourceCriterion>())
                    if (resource != null && !string.IsNullOrWhiteSpace(resource.resourceId)) yield return resource.resourceId;
            }
        }

        private static void ValidateFullGameSequence(TrainingScenarioDefinition scenario)
        {
            var required = new[]
            {
                TrainingScenarioCriterionKind.OperationalCastle,
                TrainingScenarioCriterionKind.ResourceProduction,
                TrainingScenarioCriterionKind.StableResources,
                TrainingScenarioCriterionKind.DeployedUnit,
                TrainingScenarioCriterionKind.Movement,
                TrainingScenarioCriterionKind.Scouting,
                TrainingScenarioCriterionKind.EnemyDestroyed,
                TrainingScenarioCriterionKind.ObjectiveOwned,
                TrainingScenarioCriterionKind.MatchVictory
            };
            int cursor = 0;
            foreach (var step in scenario.steps)
            {
                if (cursor < required.Length && step.EffectiveCriterion == required[cursor]) cursor++;
            }
            if (cursor != required.Length)
                throw new ArgumentException("Full-game scenario is missing or reorders the required authoritative sequence.");
        }

        private static TrainingScenarioDefinition[] FallbackDefinitions()
        {
            const string Walnut = "walnut-wood-materials-resources";
            const string Hardwood = "hardwood-materials-resources";
            const string Steak = "steak-food-resources";

            TrainingScenarioStepDefinition Castle() => new TrainingScenarioStepDefinition
                { id = "castle-operational", goal = TrainingScenarioGoalKind.CastleOperational,
                  criterion = TrainingScenarioCriterionKind.OperationalCastle, buildingTypeId = "castle-01" };
            TrainingScenarioStepDefinition Production() => new TrainingScenarioStepDefinition
                { id = "wood-production", goal = TrainingScenarioGoalKind.ProductionEstablished,
                  criterion = TrainingScenarioCriterionKind.ResourceProduction, buildingTypeId = "wood-camp",
                  resourceId = Walnut, minProductionPerTurn = 1f };
            TrainingScenarioStepDefinition Stable(int turns = 3, bool expandProduction = false) => new TrainingScenarioStepDefinition
                { id = "stable-economy", goal = TrainingScenarioGoalKind.StableEconomy,
                  criterion = TrainingScenarioCriterionKind.StableResources, requiredTurns = turns,
                  // expandProduction adds a hardwood criterion the scaffolded state cannot
                  // satisfy — the learner must build a sawmill, not just survive three turns.
                  resources = expandProduction
                    ? new[] { new TrainingScenarioResourceCriterion
                        { resourceId = Walnut, minStock = 1f, minProductionPerTurn = 1f },
                        new TrainingScenarioResourceCriterion
                        { resourceId = Hardwood, minStock = 1f, minProductionPerTurn = 1f } }
                    : new[] { new TrainingScenarioResourceCriterion
                        { resourceId = Walnut, minStock = 1f, minProductionPerTurn = 1f } } };
            TrainingScenarioStepDefinition Recruit() => new TrainingScenarioStepDefinition
                { id = "unit-recruited", goal = TrainingScenarioGoalKind.UnitRecruited,
                  criterion = TrainingScenarioCriterionKind.DeployedUnit, unitTypeId = "warrior" };
            TrainingScenarioStepDefinition Move() => new TrainingScenarioStepDefinition
                { id = "movement", goal = TrainingScenarioGoalKind.MovementOrExploration,
                  criterion = TrainingScenarioCriterionKind.Movement };
            TrainingScenarioStepDefinition Scout() => new TrainingScenarioStepDefinition
                { id = "scouting", goal = TrainingScenarioGoalKind.MovementOrExploration,
                  criterion = TrainingScenarioCriterionKind.Scouting };
            TrainingScenarioStepDefinition Combat() => new TrainingScenarioStepDefinition
                { id = "combat", goal = TrainingScenarioGoalKind.CombatSuccess,
                  criterion = TrainingScenarioCriterionKind.EnemyDestroyed };
            TrainingScenarioStepDefinition Capture() => new TrainingScenarioStepDefinition
                { id = "capture", goal = TrainingScenarioGoalKind.ObjectiveCaptured,
                  criterion = TrainingScenarioCriterionKind.ObjectiveOwned, objectiveType = "settlement" };
            TrainingScenarioStepDefinition Win() => new TrainingScenarioStepDefinition
                { id = "match-won", goal = TrainingScenarioGoalKind.MatchWon,
                  criterion = TrainingScenarioCriterionKind.MatchVictory };
            TrainingScenarioStepDefinition LegalSetup() => new TrainingScenarioStepDefinition
                { id = "legal-setup-verified", goal = TrainingScenarioGoalKind.None,
                  criterion = TrainingScenarioCriterionKind.LegalInitialState };

            TrainingScenarioResourceAmount Res(string id, float amount, string owner = "learner")
                => new TrainingScenarioResourceAmount { resourceId = id, amount = amount, owner = owner };
            TrainingScenarioUnitSetup LearnerUnits(int count, string near = "anchor")
                => new TrainingScenarioUnitSetup { unitTypeId = "warrior", count = count, owner = "learner", near = near };
            TrainingScenarioBuildingSetup LearnerBuilding(string id)
                => new TrainingScenarioBuildingSetup { buildingTypeId = id, owner = "learner", operational = true };
            TrainingScenarioOpponentSetup Opponent(bool enabled, int units = 0, bool castle = false,
                string archetype = "passive", int residents = 0)
                => new TrainingScenarioOpponentSetup
                { enabled = enabled, startingUnits = units, startingCastle = castle,
                  archetype = archetype, residents = residents };

            TrainingScenarioDefinition Make(string id, string title, TrainingCurriculumStage stage,
                string[] prerequisites, string[] capabilities,
                TrainingScenarioStartingConditions conditions,
                int minMeaningful, float maxForcedRatio, string[] requiredIntents,
                int maxDecisions, bool full = false,
                TrainingScenarioMasteryKind mastery = TrainingScenarioMasteryKind.Evaluation,
                params TrainingScenarioStepDefinition[] steps)
            {
                bool mustPlace = conditions?.learnerMustPlaceCastle ?? false;
                return new TrainingScenarioDefinition
                {
                    id = id, title = title, legacyStage = stage, learnerBuildsInitialCastle = mustPlace,
                    fullGame = full, prerequisites = prerequisites,
                    startingConditions = conditions ?? new TrainingScenarioStartingConditions(),
                    availableCapabilities = capabilities,
                    minMeaningfulCandidates = minMeaningful,
                    maxForcedActionRatio = maxForcedRatio,
                    requiredIntents = requiredIntents ?? Array.Empty<string>(),
                    generationConstraints = new TrainingScenarioGenerationConstraints
                    {
                        minReachableArea = 2,
                        requiredResourceTypes = new[] { Walnut },
                        minOpponentDistance = 2,
                        objectiveReachability = true
                    },
                    rewardRules = new TrainingScenarioRewardRules
                    {
                        rewardSetupActions = false,
                        validatedGameplayEventsOnly = true,
                        maxGameplayRewardEventsPerTurn = 16
                    },
                    masteryPolicy = new TrainingScenarioMasteryPolicy
                    {
                        kind = mastery,
                        successEpisodesRequired = 3,
                        maxDecisionsPerSuccessfulEpisode = maxDecisions
                    },
                    steps = steps
                };
            }

            TrainingScenarioStartingConditions Base(bool startsWithCastle, bool mustPlace,
                TrainingScenarioOpponentSetup opponent, int residents = 0, int reveal = -1,
                TrainingScenarioResourceAmount[] resources = null,
                TrainingScenarioUnitSetup[] units = null,
                TrainingScenarioBuildingSetup[] buildings = null,
                TrainingScenarioObjectiveSetup[] objectives = null)
                => new TrainingScenarioStartingConditions
                {
                    learnerStartsWithCastle = startsWithCastle,
                    learnerMustPlaceCastle = mustPlace,
                    startingResources = resources ?? Array.Empty<TrainingScenarioResourceAmount>(),
                    startingUnits = units ?? Array.Empty<TrainingScenarioUnitSetup>(),
                    startingBuildings = buildings ?? Array.Empty<TrainingScenarioBuildingSetup>(),
                    opponent = opponent ?? Opponent(false),
                    objectives = objectives ?? Array.Empty<TrainingScenarioObjectiveSetup>(),
                    residents = residents,
                    openingRevealRadius = reveal
                };

            // S0 — infrastructure validation. The only bootstrap-qualified
            // scenario; one clean legal-setup episode is enough to qualify.
            var foundation = Make("foundation-legal-setup", "Legal initial state", TrainingCurriculumStage.BasicLifecycle,
                Array.Empty<string>(), new[] { "end-turn" },
                Base(false, false, Opponent(false)),
                0, 1f, Array.Empty<string>(), 10, false, TrainingScenarioMasteryKind.BootstrapQualification,
                LegalSetup());
            foundation.masteryPolicy.successEpisodesRequired = 1;

            // Per-step candidate contracts: the watchdog evaluates required intents
            // against the step a submission belonged to, so combo steps declare the
            // intents that must be legal while that step is active.
            var comboStable = Stable(2, expandProduction: true);
            comboStable.requiredIntents = new[] { "Build", "EndTurn" };
            var comboRecruit = Recruit();
            comboRecruit.requiredIntents = new[] { "Recruit", "EndTurn" };
            var fieldMove = Move();
            fieldMove.requiredIntents = new[] { "Move", "EndTurn" };
            var fieldCombat = Combat();
            fieldCombat.requiredIntents = new[] { "Move", "Attack", "EndTurn" };

            return new[]
            {
                foundation,

                // S1 — castle placement: resources ready, many legal cells.
                Make("castle", "First castle", TrainingCurriculumStage.Building,
                    new[] { "foundation-legal-setup" }, new[] { "construction", "end-turn" },
                    Base(false, true, Opponent(false), resources: new[] { Res(Walnut, 40), Res(Hardwood, 20) }),
                    2, 0.9f, new[] { "Build", "EndTurn" }, 50, false, TrainingScenarioMasteryKind.Evaluation, Castle()),

                // S2 — production: castle + workforce ready, agent builds output.
                Make("production", "Resource production", TrainingCurriculumStage.Economy,
                    new[] { "castle" }, new[] { "construction", "economy", "end-turn" },
                    Base(true, false, Opponent(false), residents: 6,
                        resources: new[] { Res(Walnut, 40), Res(Hardwood, 20), Res(Steak, 40) }),
                    2, 0.9f, new[] { "Build", "EndTurn" }, 100, false, TrainingScenarioMasteryKind.Evaluation, Production()),

                // S3 — stable economy: the scaffolded wood-camp sustains walnut;
                // the learner must expand (sawmill → hardwood) then hold 3 turns.
                Make("stable-economy", "Stable economy", TrainingCurriculumStage.Economy,
                    new[] { "production" }, new[] { "construction", "economy", "end-turn" },
                    Base(true, false, Opponent(false), residents: 8,
                        resources: new[] { Res(Walnut, 30), Res(Hardwood, 20), Res(Steak, 40) },
                        buildings: new[] { LearnerBuilding("wood-camp") }),
                    2, 0.9f, new[] { "Build", "EndTurn" }, 150, false, TrainingScenarioMasteryKind.Evaluation,
                    Stable(3, expandProduction: true)),

                // S4 — recruitment: barrack + resources ready, agent recruits.
                Make("recruitment", "Recruitment", TrainingCurriculumStage.Recruitment,
                    new[] { "stable-economy" }, new[] { "recruitment", "economy", "end-turn" },
                    Base(true, false, Opponent(false), residents: 8,
                        resources: new[] { Res(Steak, 60), Res(Hardwood, 40), Res(Walnut, 30) },
                        buildings: new[] { LearnerBuilding("barrack") }),
                    2, 0.9f, new[] { "Recruit", "EndTurn" }, 100, false, TrainingScenarioMasteryKind.Evaluation, Recruit()),

                // S5/S6 — movement & scouting: deployed unit, tight reveal radius.
                Make("movement-scouting", "Movement and scouting", TrainingCurriculumStage.FogOfWar,
                    new[] { "recruitment" }, new[] { "movement", "scouting", "end-turn" },
                    Base(true, false, Opponent(false), reveal: 3,
                        units: new[] { LearnerUnits(1) }),
                    2, 0.9f, new[] { "Move", "EndTurn" }, 150, false, TrainingScenarioMasteryKind.Evaluation, Move(), Scout()),

                // S7 — combat: learner squad vs passive static target.
                Make("combat-defense", "Combat and defense", TrainingCurriculumStage.Combat,
                    new[] { "movement-scouting" }, new[] { "movement", "combat", "scouting", "end-turn" },
                    Base(true, false, Opponent(true, units: 1, castle: false, archetype: "passive"),
                        units: new[] { LearnerUnits(2) }),
                    2, 0.85f, new[] { "Attack", "Move", "EndTurn" }, 200, false, TrainingScenarioMasteryKind.Evaluation, Combat()),

                // S8 — capture: weakened opponent settlement objective.
                Make("capture", "Capture", TrainingCurriculumStage.Objectives,
                    new[] { "combat-defense" }, new[] { "movement", "combat", "capture", "scouting", "end-turn" },
                    Base(true, false, Opponent(true, units: 0, castle: true, archetype: "passive"),
                        units: new[] { LearnerUnits(2) },
                        objectives: new[] { new TrainingScenarioObjectiveSetup
                            { objectiveType = "settlement", owner = "opponent", healthFraction = 0.2f } }),
                    2, 0.85f, new[] { "Move", "Capture", "EndTurn" }, 250, false, TrainingScenarioMasteryKind.Evaluation, Capture()),

                // C1 — economy + recruitment run together from a ready base.
                Make("combo-economy-recruitment", "Economy + recruitment review", TrainingCurriculumStage.Recruitment,
                    new[] { "stable-economy", "recruitment" },
                    new[] { "construction", "economy", "recruitment", "end-turn" },
                    Base(true, false, Opponent(false), residents: 10,
                        resources: new[] { Res(Steak, 60), Res(Hardwood, 40), Res(Walnut, 40) },
                        buildings: new[] { LearnerBuilding("wood-camp"), LearnerBuilding("barrack") }),
                    2, 0.9f, new[] { "Build", "Recruit", "EndTurn" }, 150, false, TrainingScenarioMasteryKind.Evaluation,
                    comboStable, comboRecruit),

                // C2 — field operations: maneuver then destroy a passive target.
                Make("combo-field-ops", "Movement + combat review", TrainingCurriculumStage.Combat,
                    new[] { "movement-scouting", "combat-defense" },
                    new[] { "movement", "combat", "scouting", "end-turn" },
                    Base(true, false, Opponent(true, units: 1, castle: false, archetype: "passive"),
                        units: new[] { LearnerUnits(2) }),
                    2, 0.85f, new[] { "Move", "Attack", "EndTurn" }, 200, false, TrainingScenarioMasteryKind.Evaluation,
                    fieldMove, fieldCombat),

                // S10 — full game: real opening, heuristic opponent, real victory.
                Make("full-game-autonomous", "Full game autonomous", TrainingCurriculumStage.FullGame,
                    new[] { "capture" },
                    new[] { "construction", "economy", "recruitment", "movement", "scouting", "combat", "capture", "end-turn" },
                    Base(false, true, Opponent(true, units: 1, castle: true, archetype: "heuristic", residents: 10),
                        residents: 15,
                        resources: new[] { Res(Walnut, 80), Res(Hardwood, 80), Res(Steak, 100) }),
                    0, 1f, Array.Empty<string>(), 500, true, TrainingScenarioMasteryKind.Evaluation,
                    Castle(), Production(), Stable(), Recruit(), Move(), Scout(), Combat(), Capture(), Win())
            };
        }
    }
}
