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
            TrainingScenarioDefinition MakeDefault(string id, string title, TrainingCurriculumStage stage, bool castle, bool full,
                string[] prerequisites, params TrainingScenarioStepDefinition[] steps)
                => MakeWithCapabilities(id, title, stage, castle, full, prerequisites, CapabilitiesFor(full), steps);

            TrainingScenarioDefinition MakeWithCapabilities(string id, string title, TrainingCurriculumStage stage, bool castle, bool full,
                string[] prerequisites, string[] capabilities, params TrainingScenarioStepDefinition[] steps)
            {
                return new TrainingScenarioDefinition
                {
                    id = id, title = title, legacyStage = stage, learnerBuildsInitialCastle = castle, fullGame = full,
                    prerequisites = prerequisites,
                    startingConditions = new TrainingScenarioStartingConditions
                    {
                        learnerStartsWithCastle = false,
                        learnerMustPlaceCastle = castle,
                        startingUnits = Array.Empty<TrainingScenarioUnitSetup>()
                    },
                    availableCapabilities = capabilities,
                    generationConstraints = new TrainingScenarioGenerationConstraints
                    {
                        minReachableArea = 2,
                        requiredResourceTypes = new[] { "walnut-wood-materials-resources" },
                        minOpponentDistance = 2,
                        objectiveReachability = true
                    },
                    rewardRules = new TrainingScenarioRewardRules
                    {
                        rewardSetupActions = false,
                        validatedGameplayEventsOnly = true,
                        maxGameplayRewardEventsPerTurn = 16
                    },
                    steps = steps
                };
            }

            TrainingScenarioStepDefinition Castle() => new TrainingScenarioStepDefinition
                { id = "castle-operational", goal = TrainingScenarioGoalKind.CastleOperational,
                  criterion = TrainingScenarioCriterionKind.OperationalCastle, buildingTypeId = "castle-01" };
            TrainingScenarioStepDefinition Production() => new TrainingScenarioStepDefinition
                { id = "wood-production", goal = TrainingScenarioGoalKind.ProductionEstablished,
                  criterion = TrainingScenarioCriterionKind.ResourceProduction, buildingTypeId = "wood-camp",
                  resourceId = "walnut-wood-materials-resources", minProductionPerTurn = 1f };
            TrainingScenarioStepDefinition Stable() => new TrainingScenarioStepDefinition
                { id = "stable-economy", goal = TrainingScenarioGoalKind.StableEconomy,
                  criterion = TrainingScenarioCriterionKind.StableResources, requiredTurns = 3,
                  resources = new[] { new TrainingScenarioResourceCriterion
                    { resourceId = "walnut-wood-materials-resources", minStock = 1f, minProductionPerTurn = 1f } } };
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

            // NEW: S0 - Foundation legal setup (no units, no buildings, no resources, economy installed)
            TrainingScenarioStepDefinition LegalSetup() => new TrainingScenarioStepDefinition
                { id = "legal-setup-verified", goal = TrainingScenarioGoalKind.None,
                  criterion = TrainingScenarioCriterionKind.LegalInitialState };

            return new[]
            {
                // S0: Foundation - legal initial state (no scaffolding for learner)
                MakeWithCapabilities("foundation-legal-setup", "Legal initial state", TrainingCurriculumStage.BasicLifecycle, false, false,
                    Array.Empty<string>(), new[] { "end-turn" }, LegalSetup()),

                // S1: Castle foundation - learner places first castle
                MakeDefault("castle", "First castle", TrainingCurriculumStage.Building, true, false, new[] { "foundation-legal-setup" }, Castle()),

                // S2: Initial economy - castle operational -> settlement -> starter resources to settlement
                MakeDefault("production", "Resource production", TrainingCurriculumStage.Economy, true, false,
                    new[] { "castle" }, Castle(), Production()),

                // S3: Production establishment - build first production building
                MakeDefault("stable-economy", "Stable economy", TrainingCurriculumStage.Economy, true, false,
                    new[] { "production" }, Castle(), Production(), Stable()),

                // S4: Military infrastructure - build recruitment building
                MakeDefault("recruitment", "Recruitment", TrainingCurriculumStage.Recruitment, true, false,
                    new[] { "stable-economy" }, Castle(), Production(), Stable(), Recruit()),

                // S5: Movement & exploration
                MakeDefault("movement-scouting", "Movement and scouting", TrainingCurriculumStage.FogOfWar, true, false,
                    new[] { "recruitment" }, Castle(), Production(), Stable(), Recruit(), Move(), Scout()),

                // S6: Combat engagement
                MakeDefault("combat-defense", "Combat and defense", TrainingCurriculumStage.Combat, true, false,
                    new[] { "movement-scouting" }, Castle(), Production(), Stable(), Recruit(), Move(), Scout(), Combat()),

                // S7: Capture expansion
                MakeDefault("capture", "Capture", TrainingCurriculumStage.Objectives, true, false,
                    new[] { "combat-defense" }, Castle(), Production(), Stable(), Recruit(), Move(), Scout(), Combat(), Capture()),

                // S8: Combined economy + military
                MakeDefault("combo-economy-recruitment", "Economy + recruitment review", TrainingCurriculumStage.Recruitment, true, false,
                    new[] { "stable-economy", "recruitment" }, Castle(), Production(), Stable(), Recruit()),

                // S9: Combined field operations
                MakeDefault("combo-field-ops", "Movement + combat review", TrainingCurriculumStage.Combat, true, false,
                    new[] { "movement-scouting", "combat-defense" }, Castle(), Production(), Stable(), Recruit(), Move(), Scout(), Combat()),

                // S10: Full game autonomous - both sides start from S0
                MakeDefault("full-game-autonomous", "Full game autonomous", TrainingCurriculumStage.FullGame, true, true, new[] { "capture" },
                    Castle(), Production(), Stable(), Recruit(), Move(), Scout(), Combat(), Capture(), Win())
            };
        }

        private static string[] CapabilitiesFor(bool full)
            => full
                ? new[] { "construction", "economy", "recruitment", "movement", "scouting", "combat", "capture", "end-turn" }
                : new[] { "construction", "economy", "recruitment", "movement", "scouting", "combat", "capture", "end-turn" };
    }
}
