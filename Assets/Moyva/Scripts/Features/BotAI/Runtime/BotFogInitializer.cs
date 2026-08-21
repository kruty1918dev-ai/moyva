using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Legacy compatibility bridge between the old global player fog and bot factions.
    /// Modern BotAI uses IBotPerceptionService, so this component must never be a hard
    /// dependency of the canonical bootstrap composition root.
    /// </summary>
    internal sealed class BotFogInitializer : IInitializable
    {
        private readonly IFactionRegistry _factionRegistry;
        private readonly IFogOfWarServiceRegistry _fogRegistry;
        private readonly IFogOfWarService _globalFog;

        [Inject]
        public BotFogInitializer(
            [InjectOptional] IFactionRegistry factionRegistry = null,
            [InjectOptional] IFogOfWarServiceRegistry fogRegistry = null,
            [InjectOptional] IFogOfWarService globalFog = null)
        {
            _factionRegistry = factionRegistry;
            _fogRegistry = fogRegistry;
            _globalFog = globalFog;
        }

        public void Initialize()
        {
            if (_factionRegistry == null)
            {
                Debug.LogWarning(
                    "[MOYVA-BOT][Warning][Perception][FOG.LEGACY_FACTION_REGISTRY_MISSING] " +
                    "Legacy BotFogInitializer skipped because IFactionRegistry is unavailable. " +
                    "This is non-fatal: modern BotAI uses IBotPerceptionService.");
                return;
            }

            if (_fogRegistry == null)
            {
                Debug.LogWarning(
                    "[MOYVA-BOT][Warning][Perception][FOG.LEGACY_REGISTRY_MISSING] " +
                    "Legacy BotFogInitializer skipped because IFogOfWarServiceRegistry is unavailable. " +
                    "This does not block modern per-bot perception.");
                return;
            }

            if (_globalFog == null)
            {
                Debug.LogWarning(
                    "[MOYVA-BOT][Warning][Perception][FOG.LEGACY_GLOBAL_FOG_MISSING] " +
                    "Legacy BotFogInitializer skipped because the shared IFogOfWarService is unavailable. " +
                    "This does not block modern per-bot perception.");
                return;
            }

            int registered = 0;
            var botFactions = _factionRegistry.GetBotFactions();
            if (botFactions != null)
            {
                foreach (var faction in botFactions)
                {
                    if (faction == null)
                        continue;

                    string ownerId = faction.FactionId.Value;
                    if (string.IsNullOrWhiteSpace(ownerId))
                        continue;

                    _fogRegistry.Register(ownerId, _globalFog);
                    registered++;
                }
            }

            Debug.Log(
                "[MOYVA-BOT][Info][Perception][FOG.LEGACY_INITIALIZER_COMPLETE] " +
                $"Legacy fog compatibility initialization completed; registered={registered}. " +
                "Modern BotAI perception remains authoritative.");
        }
    }
}
