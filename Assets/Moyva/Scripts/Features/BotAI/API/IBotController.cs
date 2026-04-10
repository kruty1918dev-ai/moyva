using Kruty1918.Moyva.Faction.API;

namespace Kruty1918.Moyva.BotAI.API
{
    /// <summary>
    /// Контракт для будь-якого AI-контролера фракції.
    /// Реалізуйте цей інтерфейс щоб підключити власну логіку бота.
    ///
    /// BotTickScheduler викликає Tick() для кожного зареєстрованого контролера.
    /// </summary>
    public interface IBotController
    {
        /// <summary>Фракція, якою керує цей контролер.</summary>
        FactionId FactionId { get; }

        /// <summary>
        /// Основна точка оновлення AI.
        /// Викликається BotTickScheduler з throttle (не кожен кадр).
        /// </summary>
        void Tick();
    }
}
