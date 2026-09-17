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
        MatchVictory = 9,
        LegalInitialState = 10
    }

    public enum TrainingScenarioMasteryKind
    {
        Evaluation = 0,
        BootstrapQualification = 1
    }

    [Serializable]
    public sealed class TrainingScenarioResourceAmount
    {
        public string resourceId;
        public float amount;
        // Which side receives the resources: "learner" or "opponent".
        public string owner = "learner";
    }

    [Serializable]
    public sealed class TrainingScenarioUnitSetup
    {
        public string unitTypeId;
        public int count = 1;
        public string owner = "learner";
        public bool deployed = true;
        // Placement anchor: "anchor" (own spawn), "enemy" (opposing spawn), "objective".
        public string near = "anchor";
        // Trusted setup weakening, applied through the authoritative health component.
        public float healthFraction = 1f;
    }

    [Serializable]
    public sealed class TrainingScenarioBuildingSetup
    {
        public string buildingTypeId;
        public int count = 1;
        public string owner = "learner";
        // When true, timed buildings are completed through the trusted
        // lifecycle restore seam so economy/recruitment see operational state.
        public bool operational = true;
        public float healthFraction = 1f;
    }

    [Serializable]
    public sealed class TrainingScenarioOpponentSetup
    {
        public bool enabled = true;
        public int startingUnits = 1;
        public bool startingCastle = true;
        public string unitTypeId = "warrior";
        // Scripted policy: "heuristic" (greedy reactive), "passive"/"none" (ends turns).
        public string archetype = "heuristic";
        // Adult civilian residents seeded into each opponent settlement.
        public int residents;
    }

    [Serializable]
    public sealed class TrainingScenarioObjectiveSetup
    {
        public string objectiveId;
        public string objectiveType;
        public string owner = "opponent";
        // Settlement objectives spawn this building when the owner lacks one.
        public string buildingTypeId = "castle-01";
        // Trusted setup weakening so capture/combat scenarios can start near the goal.
        public float healthFraction = 1f;
    }

    [Serializable]
    public sealed class TrainingScenarioStartingConditions
    {
        public bool learnerStartsWithCastle;
        public bool learnerMustPlaceCastle;
        public TrainingScenarioResourceAmount[] startingResources = Array.Empty<TrainingScenarioResourceAmount>();
        public TrainingScenarioUnitSetup[] startingUnits = Array.Empty<TrainingScenarioUnitSetup>();
        public TrainingScenarioBuildingSetup[] startingBuildings = Array.Empty<TrainingScenarioBuildingSetup>();
        public TrainingScenarioOpponentSetup opponent = new TrainingScenarioOpponentSetup();
        public TrainingScenarioObjectiveSetup[] objectives = Array.Empty<TrainingScenarioObjectiveSetup>();
        // Adult civilian residents seeded into each learner settlement (workforce).
        public int residents;
        // -1 keeps the default opening reveal radius; >=0 overrides it per episode.
        public int openingRevealRadius = -1;
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
    public sealed class TrainingScenarioMasteryPolicy
    {
        public TrainingScenarioMasteryKind kind = TrainingScenarioMasteryKind.Evaluation;
        public int successEpisodesRequired = 1;
        public int maxDecisionsPerSuccessfulEpisode;

        public void Validate(string scenarioId)
        {
            if (!Enum.IsDefined(typeof(TrainingScenarioMasteryKind), kind))
                throw new ArgumentException("Scenario mastery policy kind is invalid: " + scenarioId);
            if (successEpisodesRequired < 1)
                throw new ArgumentException("Scenario mastery policy requires successEpisodesRequired >= 1: " + scenarioId);
            if (maxDecisionsPerSuccessfulEpisode < 0)
                throw new ArgumentException("Scenario mastery policy maxDecisionsPerSuccessfulEpisode is invalid: " + scenarioId);
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
        public TrainingScenarioMasteryPolicy masteryPolicy = new TrainingScenarioMasteryPolicy();
        // Candidate-diversity contract: 0 disables the check.
        public int minMeaningfulCandidates;
        // Fraction of engine-forced (single-candidate) submissions tolerated before abort.
        public float maxForcedActionRatio = 1f;
        public string[] requiredIntents = Array.Empty<string>();
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
            if (masteryPolicy == null) masteryPolicy = new TrainingScenarioMasteryPolicy();
            masteryPolicy.Validate(id);
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
            if (minMeaningfulCandidates < 0)
                throw new ArgumentException("Scenario minMeaningfulCandidates is invalid: " + id);
            if (float.IsNaN(maxForcedActionRatio) || float.IsInfinity(maxForcedActionRatio)
                || maxForcedActionRatio < 0f || maxForcedActionRatio > 1f)
                throw new ArgumentException("Scenario maxForcedActionRatio must be in 0..1: " + id);
            if (requiredIntents == null)
                throw new ArgumentException("Scenario requiredIntents cannot be null: " + id);
            foreach (var intent in requiredIntents)
                if (string.IsNullOrWhiteSpace(intent)
                    || !Enum.TryParse(intent, true, out BotIntentType _))
                    throw new ArgumentException($"Scenario {id} has unknown requiredIntent '{intent}'.");
            ValidateStartingConditions();
            ValidateSetupDoesNotSatisfySteps();
        }

        private void ValidateStartingConditions()
        {
            var conditions = startingConditions;
            if (conditions.startingResources != null)
                foreach (var resource in conditions.startingResources)
                {
                    if (resource == null || string.IsNullOrWhiteSpace(resource.resourceId))
                        throw new ArgumentException("Scenario starting resource requires resourceId: " + id);
                    if (float.IsNaN(resource.amount) || float.IsInfinity(resource.amount) || resource.amount <= 0f)
                        throw new ArgumentException("Scenario starting resource amount must be positive: " + id);
                    ValidateOwner(resource.owner, "startingResources.owner");
                }
            if (conditions.startingUnits != null)
                foreach (var unit in conditions.startingUnits)
                {
                    if (unit == null || string.IsNullOrWhiteSpace(unit.unitTypeId))
                        throw new ArgumentException("Scenario starting unit requires unitTypeId: " + id);
                    if (unit.count < 1)
                        throw new ArgumentException("Scenario starting unit count must be positive: " + id);
                    ValidateOwner(unit.owner, "startingUnits.owner");
                    ValidateNear(unit.near);
                    ValidateFraction(unit.healthFraction, "startingUnits.healthFraction");
                }
            if (conditions.startingBuildings != null)
                foreach (var building in conditions.startingBuildings)
                {
                    if (building == null || string.IsNullOrWhiteSpace(building.buildingTypeId))
                        throw new ArgumentException("Scenario starting building requires buildingTypeId: " + id);
                    if (building.count < 1)
                        throw new ArgumentException("Scenario starting building count must be positive: " + id);
                    ValidateOwner(building.owner, "startingBuildings.owner");
                    ValidateFraction(building.healthFraction, "startingBuildings.healthFraction");
                }
            var opponent = conditions.opponent;
            if (opponent != null)
            {
                if (opponent.startingUnits < 0 || opponent.residents < 0)
                    throw new ArgumentException("Scenario opponent setup counts must be non-negative: " + id);
                if (string.IsNullOrWhiteSpace(opponent.archetype))
                    throw new ArgumentException("Scenario opponent archetype is required: " + id);
                if (opponent.enabled && opponent.startingUnits > 0
                    && string.IsNullOrWhiteSpace(opponent.unitTypeId))
                    throw new ArgumentException("Scenario opponent unitTypeId is required: " + id);
            }
            if (conditions.objectives != null)
                foreach (var objective in conditions.objectives)
                {
                    if (objective == null || string.IsNullOrWhiteSpace(objective.objectiveType))
                        throw new ArgumentException("Scenario objective requires objectiveType: " + id);
                    ValidateFraction(objective.healthFraction, "objectives.healthFraction");
                }
            if (conditions.residents < 0)
                throw new ArgumentException("Scenario residents must be non-negative: " + id);
            if (conditions.openingRevealRadius < -1)
                throw new ArgumentException("Scenario openingRevealRadius is invalid: " + id);
        }

        // Setup must never silently satisfy the step being trained: a scenario that
        // asks the learner to produce an operational castle cannot start with one.
        private void ValidateSetupDoesNotSatisfySteps()
        {
            foreach (var step in steps)
            {
                if (step == null) continue;
                if (step.EffectiveCriterion == TrainingScenarioCriterionKind.OperationalCastle)
                {
                    if (startingConditions.learnerStartsWithCastle)
                        throw new ArgumentException("OperationalCastle scenario cannot start with a learner castle: " + id);
                    if (startingConditions.startingBuildings != null)
                        foreach (var building in startingConditions.startingBuildings)
                            if (building != null
                                && string.Equals(building.owner, "learner", StringComparison.OrdinalIgnoreCase)
                                && string.Equals(building.buildingTypeId, step.buildingTypeId, StringComparison.Ordinal))
                                throw new ArgumentException(
                                    "OperationalCastle scenario cannot scaffold the target building for the learner: " + id);
                }
            }
        }

        private void ValidateOwner(string owner, string field)
        {
            if (!string.Equals(owner, "learner", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(owner, "opponent", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(owner, "neutral", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Scenario {id} has invalid {field} '{owner}'.");
        }

        private void ValidateNear(string near)
        {
            if (!string.Equals(near, "anchor", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(near, "enemy", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(near, "objective", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Scenario {id} has invalid starting unit near '{near}'.");
        }

        private void ValidateFraction(float value, string field)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f || value > 1f)
                throw new ArgumentException($"Scenario {id} {field} must be in (0,1].");
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
