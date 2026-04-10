using System.Collections.Generic;

namespace Kruty1918.Moyva.Faction.API
{
    /// <summary>
    /// Відстежує, яка фракція є власником кожного юніта.
    /// Заповнюється автоматично через підписку на UnitCreatedSignal.OwnerId.
    /// </summary>
    public interface IFactionOwnershipService
    {
        /// <summary>
        /// Повертає FactionId власника юніта або FactionId.Empty якщо не зареєстрований.
        /// </summary>
        FactionId GetOwner(string unitId);

        /// <summary>Усі unitId, що належать вказаній фракції.</summary>
        IReadOnlyList<string> GetUnitIds(FactionId factionId);

        /// <summary>Явно зареєструвати юніт під фракцією (напр. при завантаженні гри).</summary>
        void Register(string unitId, FactionId factionId);

        /// <summary>Видалити юніт з реєстру (напр. при знищенні).</summary>
        void Unregister(string unitId);
    }
}
