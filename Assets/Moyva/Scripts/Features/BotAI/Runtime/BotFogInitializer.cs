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

        private IFogOfWarServiceRegistry _fogRegistry;

        private IFogOfWarService _globalFog;

        public BotFogInitializer(IFactionRegistry factionRegistry)
        {
            _factionRegistry = factionRegistry;
        }

        [Inject]
        private void ConstructOptionalDependencies(
            [InjectOptional] IFogOfWarServiceRegistry fogRegistry,
            [InjectOptional] IFogOfWarService globalFog)
        {
            _fogRegistry = fogRegistry;
            _globalFog = globalFog;
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
