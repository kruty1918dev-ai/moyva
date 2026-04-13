namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotDifficultySettings
    {
        DifficultyLevel Difficulty { get; }
        /// <summary>Інтервал між тіками бота (секунди).</summary>
        float TickInterval { get; }
        /// <summary>Поріг юнітів для переходу в режим атаки.</summary>
        int AttackThreshold { get; }
        /// <summary>Поріг юнітів для переходу в режим захисту.</summary>
        int DefendThreshold { get; }
    }
}
