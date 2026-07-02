using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Dictionary-based реалізація IFogOfWarServiceRegistry.
    /// Прив'язується як AsSingle() у FogOfWarInstaller.
    /// </summary>
    internal sealed class FogOfWarServiceRegistry : IFogOfWarServiceRegistry
    {
        private readonly Dictionary<string, IFogOfWarService> _map = new();

        /// <summary>
        /// Реєструє fog service для конкретної фракції у локальному registry.
        /// </summary>
        /// <param name="factionId">Ідентифікатор фракції.</param>
        /// <param name="service">Сервіс туману для цієї фракції.</param>
        public void Register(string factionId, IFogOfWarService service)
        {
            if (string.IsNullOrEmpty(factionId) || service == null)
                return;
            _map[factionId] = service;
        }

        /// <summary>
        /// Повертає вже зареєстрований fog service для фракції, якщо він існує.
        /// </summary>
        /// <param name="factionId">Ідентифікатор фракції.</param>
        /// <param name="service">Повернутий fog service.</param>
        /// <returns><see langword="true"/>, якщо сервіс знайдено.</returns>
        public bool TryGetFor(string factionId, out IFogOfWarService service)
        {
            if (string.IsNullOrEmpty(factionId))
            {
                service = null;
                return false;
            }
            return _map.TryGetValue(factionId, out service);
        }
    }
}
