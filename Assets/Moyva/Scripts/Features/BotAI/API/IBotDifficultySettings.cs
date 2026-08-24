namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotDifficultySettings
    {
        DifficultyLevel Difficulty { get; }
        int MaxDecisionIterations { get; }
        int MaxSuccessfulMutations { get; }
        int MaxFailedMutations { get; }
        int DeterministicNoiseMagnitude { get; }
        int MinUtilityToAct { get; }
    }
}
