namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotDifficultySettings
    {
        DifficultyLevel Difficulty { get; }
        /// <summary>Obsolete compatibility value. Runtime BotAI is turn-scoped, not timer-scoped.</summary>
        [System.Obsolete("Runtime BotAI is turn-scoped. Use planning-quality settings instead.")]
        float TickInterval { get; }
        /// <summary>Legacy compatibility threshold for BotBrain only.</summary>
        int AttackThreshold { get; }
        /// <summary>Legacy compatibility threshold for BotBrain only.</summary>
        int DefendThreshold { get; }
        int MaxDecisionIterations { get; }
        int MaxSuccessfulMutations { get; }
        int MaxFailedMutations { get; }
        int DeterministicNoiseMagnitude { get; }
        int MinUtilityToAct { get; }
    }
}
