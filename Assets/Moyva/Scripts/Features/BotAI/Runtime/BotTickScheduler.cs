using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Faction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Керує всіма Bot-контролерами: створює їх на старті та тікає з throttle.
    ///
    /// Throttle: один тік AI кожні <see cref="TickIntervalSeconds"/> секунд,
    /// щоб не навантажувати CPU щокадру.
    /// </summary>
    internal sealed class BotTickScheduler : IInitializable, ITickable
    {
        private const float TickIntervalSeconds = 2f;

        private readonly IFactionRegistry _factionRegistry;
        private readonly DiContainer      _container;

        private readonly List<IBotController> _bots = new();
        private float _timer;

        public BotTickScheduler(IFactionRegistry factionRegistry, DiContainer container)
        {
            _factionRegistry = factionRegistry;
            _container       = container;
        }

        public void Initialize()
        {
            foreach (var faction in _factionRegistry.GetBotFactions())
            {
                var bot = _container.Instantiate<BotBrain>(new object[] { faction });
                _bots.Add(bot);
                Debug.Log($"[BotTickScheduler] Зареєстровано бота для фракції '{faction.FactionId}'.");
            }

            if (_bots.Count == 0)
                Debug.Log("[BotTickScheduler] Bot-фракцій не знайдено — AI не активовано.");
        }

        public void Tick()
        {
            if (_bots.Count == 0)
                return;

            _timer += Time.deltaTime;
            if (_timer < TickIntervalSeconds)
                return;

            _timer = 0f;

            foreach (var bot in _bots)
                bot.Tick();
        }
    }
}
