using Kruty1918.Moyva.BotAI.API;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Налаштування складності AI-бота.
    /// Використовуйте статичні фабричні методи Easy(), Normal(), Hard() для отримання готових пресетів.
    /// </summary>
    internal sealed class BotDifficultySettings : IBotDifficultySettings
    {
        public DifficultyLevel Difficulty    { get; }
        public int             MaxDecisionIterations { get; }
        public int             MaxSuccessfulMutations { get; }
        public int             MaxFailedMutations { get; }
        public int             DeterministicNoiseMagnitude { get; }
        public int             MinUtilityToAct { get; }

        private BotDifficultySettings(
            DifficultyLevel difficulty,
            int maxDecisionIterations,
            int maxSuccessfulMutations,
            int maxFailedMutations,
            int deterministicNoiseMagnitude,
            int minUtilityToAct)
        {
            Difficulty       = difficulty;
            MaxDecisionIterations = maxDecisionIterations;
            MaxSuccessfulMutations = maxSuccessfulMutations;
            MaxFailedMutations = maxFailedMutations;
            DeterministicNoiseMagnitude = deterministicNoiseMagnitude;
            MinUtilityToAct = minUtilityToAct;
        }

        public static IBotDifficultySettings Easy()   =>
            new BotDifficultySettings(
                DifficultyLevel.Easy,
                16,
                8,
                8,
                9,
                1);

        public static IBotDifficultySettings Normal() =>
            new BotDifficultySettings(
                DifficultyLevel.Normal,
                24,
                12,
                8,
                3,
                1);

        public static IBotDifficultySettings Hard()   =>
            new BotDifficultySettings(
                DifficultyLevel.Hard,
                32,
                12,
                6,
                0,
                1);
    }
}
