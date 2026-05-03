using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Реєструє глобальний IFogOfWarService у IFogOfWarServiceRegistry для кожної бот-фракції.
    /// Використовує один загальний сервіс туману (бот бачить усі юніти).
    ///
    /// Розширення: замінити на окремі per-faction FogOfWarService з фільтрацією сигналів.
    /// </summary>
    internal sealed class BotFogInitializer : IInitializable
    {
        private readonly IFactionRegistry _factionRegistry;

        [InjectOptional]
        private IFogOfWarServiceRegistry _fogRegistry = null;

        [InjectOptional]
        private IFogOfWarService _globalFog = null;

        public BotFogInitializer(IFactionRegistry factionRegistry)
        {
            _factionRegistry = factionRegistry;
        }

        public void Initialize()
        {
            if (_fogRegistry == null || _globalFog == null)
            {
                Debug.Log("[BotFogInitializer] IFogOfWarServiceRegistry або IFogOfWarService не доступні — туман для ботів не налаштовано.");
                return;
            }

            foreach (var faction in _factionRegistry.GetBotFactions())
            {
                _fogRegistry.Register(faction.FactionId.Value, _globalFog);
                Debug.Log($"[BotFogInitializer] Зареєстровано туман для фракції '{faction.FactionId}'.");
            }
        }
    }
}
