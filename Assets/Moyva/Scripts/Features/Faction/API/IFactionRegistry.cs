using System.Collections.Generic;

namespace Kruty1918.Moyva.Faction.API
{
    /// <summary>
    /// Надає доступ до всіх фракцій поточної сесії.
    /// </summary>
    public interface IFactionRegistry
    {
        /// <summary>Усі фракції, зареєстровані в поточній сесії.</summary>
        IReadOnlyList<FactionDefinition> GetAll();

        /// <summary>Тільки Bot-фракції.</summary>
        IReadOnlyList<FactionDefinition> GetBotFactions();

        /// <summary>Фракція локального (human) гравця на цьому пристрої.</summary>
        FactionDefinition LocalPlayerFaction { get; }

        bool TryGet(FactionId id, out FactionDefinition definition);
    }
}
