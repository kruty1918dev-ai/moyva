using System;

namespace Kruty1918.Moyva.BotAI.API
{
    public sealed class BotPlanningProfile
    {
        public const int DefaultMaxDecisionIterations = 24;
        public const int DefaultMaxSuccessfulMutations = 12;
        public const int DefaultMaxFailedMutations = 8;

        public BotPlanningProfile(
            string profileId,
            DifficultyLevel difficulty,
            int maxDecisionIterations,
            int maxSuccessfulMutations,
            int maxFailedMutations,
            int deterministicNoiseMagnitude,
            int minUtilityToAct)
        {
            ProfileId = string.IsNullOrWhiteSpace(profileId) ? "normal" : profileId.Trim();
            Difficulty = difficulty;
            MaxDecisionIterations = Math.Max(1, maxDecisionIterations);
            MaxSuccessfulMutations = Math.Max(0, maxSuccessfulMutations);
            MaxFailedMutations = Math.Max(0, maxFailedMutations);
            DeterministicNoiseMagnitude = Math.Max(0, deterministicNoiseMagnitude);
            MinUtilityToAct = minUtilityToAct;
        }

        public string ProfileId { get; }
        public DifficultyLevel Difficulty { get; }
        public int MaxDecisionIterations { get; }
        public int MaxSuccessfulMutations { get; }
        public int MaxFailedMutations { get; }
        public int DeterministicNoiseMagnitude { get; }
        public int MinUtilityToAct { get; }

        public static BotPlanningProfile Normal()
            => new(
                "normal",
                DifficultyLevel.Normal,
                DefaultMaxDecisionIterations,
                DefaultMaxSuccessfulMutations,
                DefaultMaxFailedMutations,
                deterministicNoiseMagnitude: 3,
                minUtilityToAct: 1);
    }
}
