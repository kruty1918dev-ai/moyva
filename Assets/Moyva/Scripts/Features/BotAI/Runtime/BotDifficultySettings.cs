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
        public float           TickInterval  { get; }
        public int             AttackThreshold { get; }
        public int             DefendThreshold { get; }

        private BotDifficultySettings(
            DifficultyLevel difficulty,
            float tickInterval,
            int attackThreshold,
            int defendThreshold)
        {
            Difficulty       = difficulty;
            TickInterval     = tickInterval;
            AttackThreshold  = attackThreshold;
            DefendThreshold  = defendThreshold;
        }

        public static IBotDifficultySettings Easy()   =>
            new BotDifficultySettings(DifficultyLevel.Easy,   tickInterval: 4f, attackThreshold: 5, defendThreshold: 2);

        public static IBotDifficultySettings Normal() =>
            new BotDifficultySettings(DifficultyLevel.Normal, tickInterval: 2f, attackThreshold: 3, defendThreshold: 1);

        public static IBotDifficultySettings Hard()   =>
            new BotDifficultySettings(DifficultyLevel.Hard,   tickInterval: 1f, attackThreshold: 2, defendThreshold: 1);
    }
}
