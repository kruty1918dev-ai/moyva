using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    internal sealed class TrainingScenarioJsonEnvelope { public TrainingScenarioDefinition scenario; }

    public sealed class TrainingScenarioCatalog
    {
        private readonly Dictionary<string, TrainingScenarioDefinition> _items;
        public IReadOnlyCollection<TrainingScenarioDefinition> Items => _items.Values;

        public TrainingScenarioCatalog(IEnumerable<TrainingScenarioDefinition> definitions)
        {
            _items = new Dictionary<string, TrainingScenarioDefinition>(StringComparer.Ordinal);
            foreach (var definition in definitions ?? Array.Empty<TrainingScenarioDefinition>())
            {
                definition.Validate();
                if (_items.ContainsKey(definition.id)) throw new ArgumentException("Duplicate scenario: " + definition.id);
                _items.Add(definition.id, definition);
            }
            foreach (var scenario in _items.Values)
                foreach (var dependency in scenario.prerequisites ?? Array.Empty<string>())
                    if (!_items.ContainsKey(dependency)) throw new ArgumentException($"Scenario {scenario.id} depends on missing {dependency}.");
        }

        public TrainingScenarioDefinition Get(string id)
            => !string.IsNullOrWhiteSpace(id) && _items.TryGetValue(id, out var value) ? value : null;

        public static TrainingScenarioCatalog BuiltIn()
        {
            var presets = TryLoadPresetDefinitions();
            return presets != null
                ? new TrainingScenarioCatalog(presets)
                : new TrainingScenarioCatalog(FallbackDefinitions());
        }

        private static TrainingScenarioDefinition[] FallbackDefinitions() => new[]
        {
            Scenario("castle", "First castle", TrainingCurriculumStage.Building, true, false, Array.Empty<string>(),
                Step("castle-operational", TrainingScenarioGoalKind.CastleOperational)),
            Scenario("production", "Resource production", TrainingCurriculumStage.Economy, false, false, new[] { "castle" },
                Step("production-established", TrainingScenarioGoalKind.ProductionEstablished)),
            Scenario("stable-economy", "Stable economy", TrainingCurriculumStage.Economy, false, false, new[] { "production" },
                Step("stable-economy", TrainingScenarioGoalKind.StableEconomy, 2, 1f)),
            Scenario("recruitment", "Recruitment", TrainingCurriculumStage.Recruitment, false, false, new[] { "stable-economy" },
                Step("unit-recruited", TrainingScenarioGoalKind.UnitRecruited)),
            Scenario("movement-scouting", "Movement and scouting", TrainingCurriculumStage.FogOfWar, false, false, new[] { "recruitment" },
                Step("movement", TrainingScenarioGoalKind.MovementOrExploration, 2)),
            Scenario("combat-defense", "Combat and defense", TrainingCurriculumStage.Combat, false, false, new[] { "movement-scouting" },
                Step("combat", TrainingScenarioGoalKind.CombatSuccess)),
            Scenario("capture", "Capture", TrainingCurriculumStage.Objectives, false, false, new[] { "combat-defense" },
                Step("capture", TrainingScenarioGoalKind.ObjectiveCaptured)),
            Scenario("combo-foundation", "Castle + production review", TrainingCurriculumStage.Economy, true, false,
                new[] { "castle", "production" },
                Step("castle-operational", TrainingScenarioGoalKind.CastleOperational),
                Step("production-established", TrainingScenarioGoalKind.ProductionEstablished)),
            Scenario("combo-economy-recruitment", "Economy + recruitment review", TrainingCurriculumStage.Recruitment, false, false,
                new[] { "stable-economy", "recruitment" },
                Step("stable-economy", TrainingScenarioGoalKind.StableEconomy, 2, 1f),
                Step("unit-recruited", TrainingScenarioGoalKind.UnitRecruited)),
            Scenario("combo-field-ops", "Movement + combat review", TrainingCurriculumStage.Combat, false, false,
                new[] { "movement-scouting", "combat-defense" },
                Step("movement", TrainingScenarioGoalKind.MovementOrExploration, 2),
                Step("combat", TrainingScenarioGoalKind.CombatSuccess)),
            Scenario("full-game", "Full game", TrainingCurriculumStage.FullGame, true, true, new[] { "capture" },
                Step("castle-operational", TrainingScenarioGoalKind.CastleOperational),
                Step("production-established", TrainingScenarioGoalKind.ProductionEstablished),
                Step("stable-economy", TrainingScenarioGoalKind.StableEconomy, 2, 1f),
                Step("unit-recruited", TrainingScenarioGoalKind.UnitRecruited),
                Step("movement", TrainingScenarioGoalKind.MovementOrExploration, 2),
                Step("combat", TrainingScenarioGoalKind.CombatSuccess),
                Step("capture", TrainingScenarioGoalKind.ObjectiveCaptured),
                Step("match-won", TrainingScenarioGoalKind.MatchWon))
        };

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

            // Packaged players may not ship loose Assets. Keep a code fallback so training
            // remains bootable; headless/Control Center can point at JSON with MOYVA_SCENARIO_DIR.
            return null;
        }

        private static TrainingScenarioDefinition[] LoadPresetDirectory(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            if (files.Length == 0)
                throw new InvalidOperationException("No training scenario JSON files found in: " + directory);

            var scenarios = files.Select(path => ParseJson(File.ReadAllText(path))).ToArray();
            string[] required =
            {
                "castle", "production", "stable-economy", "recruitment",
                "movement-scouting", "combat-defense", "capture", "full-game"
            };
            string[] missing = required
                .Where(id => scenarios.All(s => !string.Equals(s.id, id, StringComparison.Ordinal)))
                .ToArray();
            if (missing.Length > 0)
                throw new InvalidOperationException(
                    "Training scenario preset directory is missing required scenarios: " + string.Join(", ", missing));

            Debug.Log("MOYVA_SCENARIOS_LOADED path=" + directory + " count=" + scenarios.Length);
            return scenarios;
        }

        public static TrainingScenarioDefinition ParseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Scenario JSON is empty.");
            var envelope = JsonUtility.FromJson<TrainingScenarioJsonEnvelope>(json);
            var result = envelope?.scenario;
            if (result == null) throw new ArgumentException("Scenario JSON must contain a 'scenario' object.");
            result.Validate();
            return result;
        }

        private static TrainingScenarioDefinition Scenario(string id, string title, TrainingCurriculumStage stage,
            bool learnerBuildsCastle, bool fullGame, string[] prerequisites, params TrainingScenarioStepDefinition[] steps)
            => new TrainingScenarioDefinition { id = id, title = title, legacyStage = stage,
                learnerBuildsInitialCastle = learnerBuildsCastle, fullGame = fullGame,
                prerequisites = prerequisites, steps = steps };

        private static TrainingScenarioStepDefinition Step(string id, TrainingScenarioGoalKind goal, int count = 1, float threshold = 0)
            => new TrainingScenarioStepDefinition { id = id, goal = goal, requiredCount = count, threshold = threshold };
    }
}
