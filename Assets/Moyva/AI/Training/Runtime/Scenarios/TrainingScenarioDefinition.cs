using System;
using System.Collections.Generic;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingScenarioGoalKind
    {
        None = 0,
        CastleOperational = 1,
        ProductionEstablished = 2,
        StableEconomy = 3,
        UnitRecruited = 4,
        MovementOrExploration = 5,
        CombatSuccess = 6,
        ObjectiveCaptured = 7,
        MatchWon = 8
    }

    public enum TrainingScenarioCriterionKind
    {
        None = 0,
        OperationalCastle = 1,
        ResourceProduction = 2,
        StableResources = 3,
        DeployedUnit = 4,
        Movement = 5,
        Scouting = 6,
        EnemyDestroyed = 7,
        ObjectiveOwned = 8,
        MatchVictory = 9
    }

    [Serializable]
    public sealed class TrainingScenarioResourceAmount
    {
        public string resourceId;
        public float amount;
    }

    [Serializable]
    public sealed class TrainingScenarioUnitSetup
    {
        public string unitTypeId;
        public int count = 1;
        public string owner = "learner";
        public bool deployed = true;
    }

    [Serializable]
    public sealed class TrainingScenarioOpponentSetup
    {
        public bool enabled = true;
        public int startingUnits = 1;
        public bool startingCastle = true;
    }

    [Serializable]
    public sealed class TrainingScenarioObjectiveSetup
    {
        public string objectiveId;
        public string objectiveType;
        public string owner = "opponent";
    }

    [Serializable]
    public sealed class TrainingScenarioStartingConditions
    {
        public bool learnerStartsWithCastle;
        public bool learnerMustPlaceCastle;
        public TrainingScenarioResourceAmount[] startingResources = Array.Empty<TrainingScenarioResourceAmount>();
        public TrainingScenarioUnitSetup[] startingUnits = Array.Empty<TrainingScenarioUnitSetup>();
        public TrainingScenarioOpponentSetup opponent = new TrainingScenarioOpponentSetup();
        public TrainingScenarioObjectiveSetup[] objectives = Array.Empty<TrainingScenarioObjectiveSetup>();
    }

    [Serializable]
    public sealed class TrainingScenarioGenerationConstraints
    {
        public int minReachableArea = 2;
        public string[] requiredResourceTypes = Array.Empty<string>();
        public int minOpponentDistance = 1;
        public int maxOpponentDistance;
        public bool objectiveReachability = true;
        public string[] requiredTerrainCompatibility = Array.Empty<string>();

        public void Validate(string scenarioId)
        {
            if (minReachableArea < 1)
                throw new ArgumentException("Scenario minReachableArea must be positive: " + scenarioId);
            if (minOpponentDistance < 0 || maxOpponentDistance < 0
                || (maxOpponentDistance > 0 && maxOpponentDistance < minOpponentDistance))
                throw new ArgumentException("Scenario opponent distance range is invalid: " + scenarioId);
            if (requiredResourceTypes == null)
                throw new ArgumentException("Scenario requiredResourceTypes cannot be null: " + scenarioId);
            if (requiredTerrainCompatibility == null)
                throw new ArgumentException("Scenario requiredTerrainCompatibility cannot be null: " + scenarioId);
        }
    }

    [Serializable]
    public sealed class TrainingScenarioResourceCriterion
    {
        public string resourceId;
        public float minStock;
        public float minProductionPerTurn;

        public void Validate(string stepId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException("Stable-resource criterion requires resourceId: " + stepId);
            ValidateFiniteNonNegative(minStock, "minStock", stepId);
            ValidateFiniteNonNegative(minProductionPerTurn, "minProductionPerTurn", stepId);
        }

        internal static void ValidateFiniteNonNegative(float value, string name, string stepId)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentException($"Scenario step {name} is invalid: {stepId}");
        }
    }

    [Serializable]
    public sealed class TrainingScenarioRewardRules
    {
        public bool validatedGameplayEventsOnly = true;
        public bool rewardSetupActions;
        public bool requireMeaningfulEvent = true;
        public int maxGameplayRewardEventsPerTurn = 16;
        public TrainingRewardEventType[] allowedGameplayEvents = Array.Empty<TrainingRewardEventType>();

        public void Validate(string scenarioId)
        {
            if (maxGameplayRewardEventsPerTurn < 0)
                throw new ArgumentException("Scenario reward event limit is invalid: " + scenarioId);
            if (allowedGameplayEvents == null)
                throw new ArgumentException("Scenario allowedGameplayEvents cannot be null: " + scenarioId);
        }

        public bool Allows(TrainingRewardEvent e)
        {
            if (validatedGameplayEventsOnly && !e.Validated) return false;
            if (requireMeaningfulEvent && !e.Meaningful) return false;
            if (allowedGameplayEvents == null || allowedGameplayEvents.Length == 0) return true;
            for (int i = 0; i < allowedGameplayEvents.Length; i++)
                if (allowedGameplayEvents[i] == e.Type) return true;
            return false;
        }
    }

    [Serializable]
    public sealed class TrainingScenarioStepDefinition
    {
        public string id;
        public TrainingScenarioGoalKind goal;
        public TrainingScenarioCriterionKind criterion;
        // Retained only for JSON compatibility. Non-zero generic thresholds are rejected.
        public float threshold;
        public string resourceId;
        public float minStock;
        public float minProductionPerTurn;
        public int requiredTurns;
        public int requiredCount = 1;
        public string unitTypeId;
        public string buildingTypeId;
        public string objectiveId;
        public string objectiveType;
        public TrainingScenarioResourceCriterion[] resources = Array.Empty<TrainingScenarioResourceCriterion>();

        public TrainingScenarioCriterionKind EffectiveCriterion
        {
            get
            {
                if (criterion != TrainingScenarioCriterionKind.None) return criterion;
                switch (goal)
                {
                    case TrainingScenarioGoalKind.CastleOperational: return TrainingScenarioCriterionKind.OperationalCastle;
                    case TrainingScenarioGoalKind.ProductionEstablished: return TrainingScenarioCriterionKind.ResourceProduction;
                    case TrainingScenarioGoalKind.StableEconomy: return TrainingScenarioCriterionKind.StableResources;
                    case TrainingScenarioGoalKind.UnitRecruited: return TrainingScenarioCriterionKind.DeployedUnit;
                    case TrainingScenarioGoalKind.MovementOrExploration: return TrainingScenarioCriterionKind.Movement;
                    case TrainingScenarioGoalKind.CombatSuccess: return TrainingScenarioCriterionKind.EnemyDestroyed;
                    case TrainingScenarioGoalKind.ObjectiveCaptured: return TrainingScenarioCriterionKind.ObjectiveOwned;
                    case TrainingScenarioGoalKind.MatchWon: return TrainingScenarioCriterionKind.MatchVictory;
                    default: return TrainingScenarioCriterionKind.None;
                }
            }
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Scenario step id is required.");
            if (EffectiveCriterion == TrainingScenarioCriterionKind.None)
                throw new ArgumentException("Scenario step criterion is required: " + id);
            if (requiredCount < 1) throw new ArgumentException("Scenario step requiredCount must be positive: " + id);
            if (float.IsNaN(threshold) || float.IsInfinity(threshold) || threshold != 0f)
                throw new ArgumentException("Generic threshold is not a valid authoritative scenario criterion: " + id);
            TrainingScenarioResourceCriterion.ValidateFiniteNonNegative(minStock, "minStock", id);
            TrainingScenarioResourceCriterion.ValidateFiniteNonNegative(minProductionPerTurn, "minProductionPerTurn", id);
            if (requiredTurns < 0) throw new ArgumentException("Scenario step requiredTurns is invalid: " + id);
            if (resources == null) throw new ArgumentException("Scenario step resources cannot be null: " + id);

            switch (EffectiveCriterion)
            {
                case TrainingScenarioCriterionKind.OperationalCastle:
                    Require(buildingTypeId, "buildingTypeId");
                    break;
                case TrainingScenarioCriterionKind.ResourceProduction:
                    Require(resourceId, "resourceId");
                    if (minProductionPerTurn <= 0f)
                        throw new ArgumentException("ResourceProduction requires positive minProductionPerTurn: " + id);
                    break;
                case TrainingScenarioCriterionKind.StableResources:
                    if (requiredTurns < 1)
                        throw new ArgumentException("StableResources requires requiredTurns >= 1: " + id);
                    if (resources.Length == 0)
                        throw new ArgumentException("StableResources requires at least one resource criterion: " + id);
                    foreach (var resource in resources)
                    {
                        if (resource == null) throw new ArgumentException("StableResources contains null resource criterion: " + id);
                        resource.Validate(id);
                    }
                    break;
                case TrainingScenarioCriterionKind.DeployedUnit:
                    Require(unitTypeId, "unitTypeId");
                    break;
                case TrainingScenarioCriterionKind.ObjectiveOwned:
                    if (string.IsNullOrWhiteSpace(objectiveId) && string.IsNullOrWhiteSpace(objectiveType))
                        throw new ArgumentException("ObjectiveOwned requires objectiveId or objectiveType: " + id);
                    break;
            }
        }

        private void Require(string value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Scenario step {field} is required: {id}");
        }
    }

    [Serializable]
    public sealed class TrainingScenarioDefinition
    {
        public string id;
        public string title;
        public TrainingCurriculumStage legacyStage = TrainingCurriculumStage.FullGame;
        public bool learnerBuildsInitialCastle;
        public bool fullGame;
        public string[] prerequisites = Array.Empty<string>();
        public TrainingScenarioStartingConditions startingConditions = new TrainingScenarioStartingConditions();
        public string[] availableCapabilities = Array.Empty<string>();
        public TrainingScenarioGenerationConstraints generationConstraints = new TrainingScenarioGenerationConstraints();
        public TrainingScenarioRewardRules rewardRules = new TrainingScenarioRewardRules();
        public TrainingScenarioStepDefinition[] steps = Array.Empty<TrainingScenarioStepDefinition>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Scenario id is required.");
            if (!Enum.IsDefined(typeof(TrainingCurriculumStage), legacyStage))
                throw new ArgumentException("Scenario has invalid legacy stage: " + id);
            if (prerequisites == null) throw new ArgumentException("Scenario prerequisites cannot be null: " + id);
            if (startingConditions == null) throw new ArgumentException("Scenario startingConditions are required: " + id);
            if (availableCapabilities == null || availableCapabilities.Length == 0)
                throw new ArgumentException("Scenario availableCapabilities cannot be empty: " + id);
            var capabilities = new HashSet<string>(StringComparer.Ordinal);
            foreach (var capability in availableCapabilities)
            {
                if (string.IsNullOrWhiteSpace(capability) || !capabilities.Add(capability))
                    throw new ArgumentException("Scenario has invalid/duplicate capability: " + id);
            }
            if (generationConstraints == null) throw new ArgumentException("Scenario generationConstraints are required: " + id);
            generationConstraints.Validate(id);
            if (rewardRules == null) throw new ArgumentException("Scenario rewardRules are required: " + id);
            rewardRules.Validate(id);
            if (steps == null || steps.Length == 0) throw new ArgumentException("Scenario must have at least one step: " + id);

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var step in steps)
            {
                if (step == null) throw new ArgumentException("Scenario contains a null step: " + id);
                step.Validate();
                if (!seen.Add(step.id)) throw new ArgumentException("Duplicate scenario step id: " + step.id);
            }

            if (startingConditions.learnerMustPlaceCastle != learnerBuildsInitialCastle)
                throw new ArgumentException("learnerBuildsInitialCastle must match startingConditions.learnerMustPlaceCastle: " + id);
            if (startingConditions.learnerStartsWithCastle && startingConditions.learnerMustPlaceCastle)
                throw new ArgumentException("Learner cannot both start with and be required to place the castle: " + id);
        }

        public bool AllowsIntent(BotIntentType intent)
        {
            string name = intent.ToString();
            string capability = null;
            if (name.IndexOf("Build", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Construct", StringComparison.OrdinalIgnoreCase) >= 0) capability = "construction";
            else if (name.IndexOf("Recruit", StringComparison.OrdinalIgnoreCase) >= 0) capability = "recruitment";
            else if (string.Equals(name, "Move", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Reposition", StringComparison.OrdinalIgnoreCase)) capability = "movement";
            else if (name.IndexOf("Explore", StringComparison.OrdinalIgnoreCase) >= 0) capability = "scouting";
            else if (name.IndexOf("Attack", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Combat", StringComparison.OrdinalIgnoreCase) >= 0) capability = "combat";
            else if (name.IndexOf("Capture", StringComparison.OrdinalIgnoreCase) >= 0) capability = "capture";
            else if (string.Equals(name, "EndTurn", StringComparison.OrdinalIgnoreCase)) capability = "end-turn";
            if (capability == null) return true;
            for (int i = 0; i < availableCapabilities.Length; i++)
                if (string.Equals(availableCapabilities[i], capability, StringComparison.Ordinal)) return true;
            return false;
        }

        public float GoalCode => Stable01(id);

        private static float Stable01(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                if (!string.IsNullOrEmpty(value)) foreach (char c in value) hash = (hash ^ c) * 16777619;
                return (hash & 0xffff) / 65535f;
            }
        }
    }
}
