using System;

namespace Kruty1918.Moyva.BotAI.API
{
    public sealed class BotPlanningProfile
    {
        public const int DefaultMaxDecisionIterations = 24;
        public const int DefaultMaxSuccessfulMutations = 12;
        public const int DefaultMaxFailedMutations = 8;

        public const int DefaultImmediateCastleThreatWeight = 50000;
        public const int DefaultNextTurnCastleThreatWeight = 20000;
        public const int DefaultThreatDamageWeight = 10;
        public const int DefaultThreatDistanceWeight = 500;
        public const int DefaultHomeDefenseRadius = 6;
        public const int DefaultMinimumHomeDefenders = 1;
        public const int DefaultEmergencyThreatThreshold = 15000;
        public const int DefaultMaxDefendersToEvaluate = 4;

        public const int DefaultMoveToAttackWeight = 1900;
        public const int DefaultRetreatWeight = 2100;
        public const int DefaultRetreatHealthPercent = 30;
        public const int DefaultFocusFireLethalWeight = 900;
        public const int DefaultHomeReservePercent = 25;
        public const int DefaultScoutFrontierWeight = 45;
        public const int DefaultScoutThreatPenaltyPercent = 100;
        public const int DefaultGoalHysteresisTurns = 3;
        public const int DefaultMaxTacticalTilesPerUnit = 64;
        public const int DefaultMaxScoutsPerTurn = 3;

        public BotPlanningProfile(
            string profileId,
            DifficultyLevel difficulty,
            int maxDecisionIterations,
            int maxSuccessfulMutations,
            int maxFailedMutations,
            int deterministicNoiseMagnitude,
            int minUtilityToAct,
            int immediateCastleThreatWeight = DefaultImmediateCastleThreatWeight,
            int nextTurnCastleThreatWeight = DefaultNextTurnCastleThreatWeight,
            int threatDamageWeight = DefaultThreatDamageWeight,
            int threatDistanceWeight = DefaultThreatDistanceWeight,
            int homeDefenseRadius = DefaultHomeDefenseRadius,
            int minimumHomeDefenders = DefaultMinimumHomeDefenders,
            int emergencyThreatThreshold = DefaultEmergencyThreatThreshold,
            int maxDefendersToEvaluate = DefaultMaxDefendersToEvaluate,
            int moveToAttackWeight = DefaultMoveToAttackWeight,
            int retreatWeight = DefaultRetreatWeight,
            int retreatHealthPercent = DefaultRetreatHealthPercent,
            int focusFireLethalWeight = DefaultFocusFireLethalWeight,
            int homeReservePercent = DefaultHomeReservePercent,
            int scoutFrontierWeight = DefaultScoutFrontierWeight,
            int scoutThreatPenaltyPercent = DefaultScoutThreatPenaltyPercent,
            int goalHysteresisTurns = DefaultGoalHysteresisTurns,
            int maxTacticalTilesPerUnit = DefaultMaxTacticalTilesPerUnit,
            int maxScoutsPerTurn = DefaultMaxScoutsPerTurn)
        {
            ProfileId = string.IsNullOrWhiteSpace(profileId) ? "normal" : profileId.Trim();
            Difficulty = difficulty;
            MaxDecisionIterations = Math.Max(1, maxDecisionIterations);
            MaxSuccessfulMutations = Math.Max(0, maxSuccessfulMutations);
            MaxFailedMutations = Math.Max(0, maxFailedMutations);
            DeterministicNoiseMagnitude = Math.Max(0, deterministicNoiseMagnitude);
            MinUtilityToAct = minUtilityToAct;

            ImmediateCastleThreatWeight = Math.Max(0, immediateCastleThreatWeight);
            NextTurnCastleThreatWeight = Math.Max(0, nextTurnCastleThreatWeight);
            ThreatDamageWeight = Math.Max(0, threatDamageWeight);
            ThreatDistanceWeight = Math.Max(0, threatDistanceWeight);
            HomeDefenseRadius = Math.Max(1, homeDefenseRadius);
            MinimumHomeDefenders = Math.Max(0, minimumHomeDefenders);
            EmergencyThreatThreshold = Math.Max(0, emergencyThreatThreshold);
            MaxDefendersToEvaluate = Math.Max(1, maxDefendersToEvaluate);
            MoveToAttackWeight = Math.Max(0, moveToAttackWeight);
            RetreatWeight = Math.Max(0, retreatWeight);
            RetreatHealthPercent = Math.Max(1, Math.Min(100, retreatHealthPercent));
            FocusFireLethalWeight = Math.Max(0, focusFireLethalWeight);
            HomeReservePercent = Math.Max(0, Math.Min(100, homeReservePercent));
            ScoutFrontierWeight = Math.Max(0, scoutFrontierWeight);
            ScoutThreatPenaltyPercent = Math.Max(0, scoutThreatPenaltyPercent);
            GoalHysteresisTurns = Math.Max(0, goalHysteresisTurns);
            MaxTacticalTilesPerUnit = Math.Max(8, maxTacticalTilesPerUnit);
            MaxScoutsPerTurn = Math.Max(1, maxScoutsPerTurn);
        }

        public string ProfileId { get; }
        public DifficultyLevel Difficulty { get; }
        public int MaxDecisionIterations { get; }
        public int MaxSuccessfulMutations { get; }
        public int MaxFailedMutations { get; }
        public int DeterministicNoiseMagnitude { get; }
        public int MinUtilityToAct { get; }

        public int ImmediateCastleThreatWeight { get; }
        public int NextTurnCastleThreatWeight { get; }
        public int ThreatDamageWeight { get; }
        public int ThreatDistanceWeight { get; }
        public int HomeDefenseRadius { get; }
        public int MinimumHomeDefenders { get; }
        public int EmergencyThreatThreshold { get; }
        public int MaxDefendersToEvaluate { get; }
        public int MoveToAttackWeight { get; }
        public int RetreatWeight { get; }
        public int RetreatHealthPercent { get; }
        public int FocusFireLethalWeight { get; }
        public int HomeReservePercent { get; }
        public int ScoutFrontierWeight { get; }
        public int ScoutThreatPenaltyPercent { get; }
        public int GoalHysteresisTurns { get; }
        public int MaxTacticalTilesPerUnit { get; }
        public int MaxScoutsPerTurn { get; }

        public static BotPlanningProfile Normal()
            => new(
                "normal",
                DifficultyLevel.Normal,
                DefaultMaxDecisionIterations,
                DefaultMaxSuccessfulMutations,
                DefaultMaxFailedMutations,
                deterministicNoiseMagnitude: 3,
                minUtilityToAct: 1,
                immediateCastleThreatWeight: DefaultImmediateCastleThreatWeight,
                nextTurnCastleThreatWeight: DefaultNextTurnCastleThreatWeight,
                threatDamageWeight: DefaultThreatDamageWeight,
                threatDistanceWeight: DefaultThreatDistanceWeight,
                homeDefenseRadius: DefaultHomeDefenseRadius,
                minimumHomeDefenders: DefaultMinimumHomeDefenders,
                emergencyThreatThreshold: DefaultEmergencyThreatThreshold,
                maxDefendersToEvaluate: DefaultMaxDefendersToEvaluate);
    }
}
