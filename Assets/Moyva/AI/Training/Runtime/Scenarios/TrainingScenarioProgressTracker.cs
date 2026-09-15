using System;
using System.Collections.Generic;
using Kruty1918.Moyva.AI.Bot;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingScenarioFacts
    {
        private static readonly IReadOnlyDictionary<string, float> EmptyFloat =
            new Dictionary<string, float>(StringComparer.Ordinal);
        private static readonly IReadOnlyDictionary<string, int> EmptyInt =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private static readonly IReadOnlyDictionary<string, Vector2Int> EmptyCells =
            new Dictionary<string, Vector2Int>(StringComparer.Ordinal);
        private static readonly IReadOnlyCollection<string> EmptyStrings = Array.Empty<string>();

        public bool IsSetup { get; }
        public int OwnedSettlements { get; }
        public int OperationalCastles { get; }
        public int OwnedUnits { get; }
        public int DeployedUnits { get; }
        public IReadOnlyDictionary<string, float> ResourceStock { get; }
        public IReadOnlyDictionary<string, float> ProductionPerTurn { get; }
        public int VisibleEnemyUnitCount { get; }
        public IReadOnlyCollection<string> CapturedObjectiveIds { get; }
        public int CurrentTurn { get; }
        public int ExploredCells { get; }
        public IReadOnlyDictionary<string, int> DeployedUnitsByType { get; }
        public IReadOnlyDictionary<string, int> OperationalBuildingsByType { get; }
        public IReadOnlyDictionary<string, Vector2Int> UnitCells { get; }
        public IReadOnlyCollection<string> OwnedSettlementIds { get; }
        public int ReachableLandCellsFromLearner { get; }
        public int TotalLandCells { get; }
        public int LearnerResourcePotential { get; }

        // NEW: Recruitment tracking fields for invariant I1
        public IReadOnlyDictionary<string, int> RecruitedUnitsByType { get; }
        public IReadOnlyDictionary<string, int> SetupUnitsByType { get; }
        public IReadOnlyCollection<string> LegitimateRecruitmentSources { get; }

        public float LearnerReachableLandRatio => TotalLandCells <= 0
            ? 0f
            : Mathf.Clamp01((float)ReachableLandCellsFromLearner / TotalLandCells);

        public TrainingScenarioFacts(
            bool isSetup = false,
            int ownedSettlements = 0,
            int operationalCastles = 0,
            int ownedUnits = 0,
            int deployedUnits = 0,
            IReadOnlyDictionary<string, float> resourceStock = null,
            IReadOnlyDictionary<string, float> productionPerTurn = null,
            int visibleEnemyUnitCount = 0,
            IReadOnlyCollection<string> capturedObjectiveIds = null,
            int currentTurn = 0,
            int exploredCells = 0,
            IReadOnlyDictionary<string, int> deployedUnitsByType = null,
            IReadOnlyDictionary<string, int> operationalBuildingsByType = null,
            IReadOnlyDictionary<string, Vector2Int> unitCells = null,
            IReadOnlyCollection<string> ownedSettlementIds = null,
            int reachableLandCellsFromLearner = 0,
            int totalLandCells = 0,
            int learnerResourcePotential = 0,
            // NEW: Recruitment tracking parameters
            IReadOnlyDictionary<string, int> recruitedUnitsByType = null,
            IReadOnlyDictionary<string, int> setupUnitsByType = null,
            IReadOnlyCollection<string> legitimateRecruitmentSources = null)
        {
            IsSetup = isSetup;
            OwnedSettlements = ownedSettlements;
            OperationalCastles = operationalCastles;
            OwnedUnits = ownedUnits;
            DeployedUnits = deployedUnits;
            ResourceStock = resourceStock ?? EmptyFloat;
            ProductionPerTurn = productionPerTurn ?? EmptyFloat;
            VisibleEnemyUnitCount = visibleEnemyUnitCount;
            CapturedObjectiveIds = capturedObjectiveIds ?? EmptyStrings;
            CurrentTurn = currentTurn;
            ExploredCells = exploredCells;
            DeployedUnitsByType = deployedUnitsByType ?? EmptyInt;
            OperationalBuildingsByType = operationalBuildingsByType ?? EmptyInt;
            UnitCells = unitCells ?? EmptyCells;
            OwnedSettlementIds = ownedSettlementIds ?? EmptyStrings;
            ReachableLandCellsFromLearner = reachableLandCellsFromLearner;
            TotalLandCells = totalLandCells;
            LearnerResourcePotential = learnerResourcePotential;
            // NEW: Initialize recruitment tracking fields
            RecruitedUnitsByType = recruitedUnitsByType ?? EmptyInt;
            SetupUnitsByType = setupUnitsByType ?? EmptyInt;
            LegitimateRecruitmentSources = legitimateRecruitmentSources ?? EmptyStrings;
        }

        public float Stock(string resourceId)
            => resourceId != null && ResourceStock.TryGetValue(resourceId, out var value) ? value : 0f;

        public float Production(string resourceId)
            => resourceId != null && ProductionPerTurn.TryGetValue(resourceId, out var value) ? value : 0f;

        public int DeployedCount(string unitTypeId)
            => unitTypeId != null && DeployedUnitsByType.TryGetValue(unitTypeId, out var value) ? value : 0;

        public int OperationalBuildingCount(string buildingTypeId)
            => buildingTypeId != null && OperationalBuildingsByType.TryGetValue(buildingTypeId, out var value) ? value : 0;

        // NEW: Recruitment tracking accessors
        public int RecruitedCount(string unitTypeId)
            => unitTypeId != null && RecruitedUnitsByType.TryGetValue(unitTypeId, out var value) ? value : 0;

        public int SetupCount(string unitTypeId)
            => unitTypeId != null && SetupUnitsByType.TryGetValue(unitTypeId, out var value) ? value : 0;

        public bool HasLegitimateRecruitmentSource(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId)) return false;
            foreach (var source in LegitimateRecruitmentSources)
                if (string.Equals(source, buildingId, StringComparison.Ordinal)) return true;
            return false;
        }

        public bool OwnsObjective(string objectiveId)
        {
            if (string.IsNullOrWhiteSpace(objectiveId)) return false;
            foreach (var value in CapturedObjectiveIds)
                if (string.Equals(value, objectiveId, StringComparison.Ordinal)) return true;
            foreach (var value in OwnedSettlementIds)
                if (string.Equals(value, objectiveId, StringComparison.Ordinal)) return true;
            return false;
        }
    }

    public sealed class TrainingScenarioProgressTracker
    {
        private readonly TrainingScenarioDefinition _scenario;
        private long _episodeId;
        private int _stepIndex;
        private int _count;
        private int _stableTurns;
        private int _lastStableTurn = int.MinValue;
        private bool _sawLearnerCastle;
        private bool _sawLearnerRecruit;
        private TrainingScenarioFacts _stepBaseline;
        private TrainingScenarioFacts _lastFacts;

        public TrainingScenarioDefinition Scenario => _scenario;
        public long EpisodeId => _episodeId;
        public int StepIndex => _stepIndex;
        public int StepCount => _scenario?.steps?.Length ?? 0;
        public bool IsScoringActive { get; private set; }
        public float Progress => StepCount == 0 ? 0 : Math.Min(1f, (_stepIndex + CurrentFraction()) / StepCount);
        public bool IsComplete => _scenario != null && _stepIndex >= StepCount;
        public TrainingScenarioStepDefinition CurrentStep => IsComplete || _scenario == null ? null : _scenario.steps[_stepIndex];

        public TrainingScenarioProgressTracker(TrainingScenarioDefinition scenario)
        {
            _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            scenario.Validate();
        }

        public void Begin(long episodeId, TrainingScenarioFacts baseline)
        {
            _episodeId = episodeId;
            _stepIndex = _count = _stableTurns = 0;
            _lastStableTurn = int.MinValue;
            _sawLearnerCastle = _sawLearnerRecruit = false;
            _stepBaseline = _lastFacts = baseline ?? new TrainingScenarioFacts();
            IsScoringActive = !(_lastFacts?.IsSetup ?? true);
        }

        public void SetSetupPhase(bool setup, TrainingScenarioFacts facts)
        {
            IsScoringActive = !setup;
            if (setup) return;
            _stepBaseline = _lastFacts = facts ?? new TrainingScenarioFacts();
            _count = _stableTurns = 0;
            _lastStableTurn = int.MinValue;
            _sawLearnerCastle = _sawLearnerRecruit = false;
        }

        public void ObserveReward(TrainingRewardEvent e, TrainingScenarioFacts facts)
        {
            facts ??= new TrainingScenarioFacts();
            if (e.EpisodeId != _episodeId || !e.Validated || IsComplete || facts.IsSetup || !IsScoringActive) return;
            var step = CurrentStep;
            if (step == null) return;

            switch (step.EffectiveCriterion)
            {
                case TrainingScenarioCriterionKind.OperationalCastle:
                    if (e.Type != TrainingRewardEventType.BuildingCreated || !MatchesBuilding(e.SubjectId, step.buildingTypeId)) return;
                    _sawLearnerCastle = true;
                    if (CastleAuthoritative(step, facts)) Advance(facts);
                    break;

                case TrainingScenarioCriterionKind.DeployedUnit:
                    if (e.Type != TrainingRewardEventType.UnitCreated || !MatchesUnit(e.SubjectId, step.unitTypeId)) return;
                    _sawLearnerRecruit = true;
                    if (RecruitAuthoritative(step, facts)) Count(facts);
                    break;

                case TrainingScenarioCriterionKind.EnemyDestroyed:
                    if (e.Type == TrainingRewardEventType.EnemyUnitDestroyed) Count(facts);
                    break;

                case TrainingScenarioCriterionKind.ObjectiveOwned:
                    if (e.Type != TrainingRewardEventType.ObjectiveCaptured) return;
                    string objective = !string.IsNullOrWhiteSpace(step.objectiveId) ? step.objectiveId : e.SubjectId;
                    if (facts.OwnsObjective(objective) && (string.IsNullOrWhiteSpace(step.objectiveId)
                        || string.Equals(step.objectiveId, e.SubjectId, StringComparison.Ordinal))) Count(facts);
                    break;
            }
        }

        public void ObserveAction(BotIntentType intent, BotExecutionStatus result, TrainingScenarioFacts facts)
        {
            facts ??= new TrainingScenarioFacts();
            if (IsComplete || facts.IsSetup || !IsScoringActive)
            {
                _lastFacts = facts;
                return;
            }

            var step = CurrentStep;
            if (step == null)
            {
                _lastFacts = facts;
                return;
            }

            // Rejected/masked/no-op submissions can never advance scenario state.
            if (result != BotExecutionStatus.Completed)
            {
                _lastFacts = facts;
                return;
            }

            switch (step.EffectiveCriterion)
            {
                case TrainingScenarioCriterionKind.OperationalCastle:
                    if (_sawLearnerCastle && CastleAuthoritative(step, facts)) Advance(facts);
                    break;

                case TrainingScenarioCriterionKind.ResourceProduction:
                    // A producer that already existed at step entry is setup/prior-step state, not proof
                    // that this step established production. Required production must become newly true.
                    if (_stepBaseline.Production(step.resourceId) < step.minProductionPerTurn
                        && ProductionAuthoritative(step, facts)) Advance(facts);
                    break;

                case TrainingScenarioCriterionKind.StableResources:
                    if (intent == BotIntentType.EndTurn) ObserveStableTurn(step, facts);
                    break;

                case TrainingScenarioCriterionKind.DeployedUnit:
                    if (_sawLearnerRecruit && RecruitAuthoritative(step, facts)) Count(facts);
                    break;

                case TrainingScenarioCriterionKind.Movement:
                    if (IsMovementIntent(intent) && AnyLearnerUnitMoved(_lastFacts, facts)) Count(facts);
                    break;

                case TrainingScenarioCriterionKind.Scouting:
                    if (facts.ExploredCells > _lastFacts.ExploredCells) Count(facts);
                    break;
            }

            _lastFacts = facts;
        }

        public void ObserveMatch(TrainingEpisodeResult result)
        {
            if (!IsComplete && IsScoringActive && CurrentStep?.EffectiveCriterion == TrainingScenarioCriterionKind.MatchVictory
                && result == TrainingEpisodeResult.Victory)
                Advance(_lastFacts);
        }

        private bool CastleAuthoritative(TrainingScenarioStepDefinition step, TrainingScenarioFacts facts)
        {
            if (!_sawLearnerCastle || facts.OperationalCastles <= _stepBaseline.OperationalCastles
                || facts.OwnedSettlements <= _stepBaseline.OwnedSettlements) return false;
            return facts.OperationalBuildingCount(step.buildingTypeId)
                > _stepBaseline.OperationalBuildingCount(step.buildingTypeId);
        }

        private bool ProductionAuthoritative(TrainingScenarioStepDefinition step, TrainingScenarioFacts facts)
        {
            if (facts.Production(step.resourceId) + 0.00001f < step.minProductionPerTurn) return false;
            if (!string.IsNullOrWhiteSpace(step.buildingTypeId)
                && facts.OperationalBuildingCount(step.buildingTypeId) <= 0) return false;
            return true;
        }

        private void ObserveStableTurn(TrainingScenarioStepDefinition step, TrainingScenarioFacts facts)
        {
            if (facts.CurrentTurn == _lastStableTurn) return;
            _lastStableTurn = facts.CurrentTurn;
            bool valid = true;
            foreach (var resource in step.resources)
            {
                if (facts.Stock(resource.resourceId) + 0.00001f < resource.minStock
                    || facts.Production(resource.resourceId) + 0.00001f < resource.minProductionPerTurn)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                _stableTurns = 0;
                return;
            }

            _stableTurns++;
            if (_stableTurns >= step.requiredTurns) Advance(facts);
        }

        private bool RecruitAuthoritative(TrainingScenarioStepDefinition step, TrainingScenarioFacts facts)
        {
            return _sawLearnerRecruit
                && facts.OwnedUnits > _stepBaseline.OwnedUnits
                && facts.DeployedUnits > _stepBaseline.DeployedUnits
                && facts.DeployedCount(step.unitTypeId) > _stepBaseline.DeployedCount(step.unitTypeId);
        }

        private static bool AnyLearnerUnitMoved(TrainingScenarioFacts before, TrainingScenarioFacts after)
        {
            if (before == null || after == null) return false;
            foreach (var pair in after.UnitCells)
                if (before.UnitCells.TryGetValue(pair.Key, out var previous) && previous != pair.Value) return true;
            return false;
        }

        private static bool IsMovementIntent(BotIntentType intent)
        {
            string value = intent.ToString();
            return string.Equals(value, "Move", StringComparison.Ordinal)
                || string.Equals(value, "Reposition", StringComparison.Ordinal)
                || string.Equals(value, "Explore", StringComparison.Ordinal);
        }

        private static bool MatchesBuilding(string subject, string buildingTypeId)
            => string.Equals(subject, "building-type:" + buildingTypeId, StringComparison.Ordinal)
               || string.Equals(subject, buildingTypeId, StringComparison.Ordinal);

        private static bool MatchesUnit(string subject, string unitTypeId)
            => string.Equals(subject, "unit-type:" + unitTypeId, StringComparison.Ordinal)
               || string.Equals(subject, unitTypeId, StringComparison.Ordinal);

        private void Count(TrainingScenarioFacts facts)
        {
            if (++_count >= CurrentStep.requiredCount) Advance(facts);
        }

        private void Advance(TrainingScenarioFacts facts)
        {
            _stepIndex++;
            _count = _stableTurns = 0;
            _lastStableTurn = int.MinValue;
            _sawLearnerCastle = _sawLearnerRecruit = false;
            _stepBaseline = _lastFacts = facts ?? new TrainingScenarioFacts();
        }

        private float CurrentFraction()
        {
            var step = CurrentStep;
            if (step == null) return 0;
            if (step.EffectiveCriterion == TrainingScenarioCriterionKind.StableResources)
                return Math.Min(1f, _stableTurns / (float)Math.Max(1, step.requiredTurns));
            return Math.Min(1f, _count / (float)Math.Max(1, step.requiredCount));
        }
    }
}

