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

        public void Register(string factionId, IFogOfWarService service)
        {
            if (string.IsNullOrEmpty(factionId) || service == null)
                return;
            _map[factionId] = service;
        }

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
