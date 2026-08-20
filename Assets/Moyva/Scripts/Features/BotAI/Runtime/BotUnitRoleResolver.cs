using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotUnitRoleResolver : IBotUnitRoleResolver
    {
        [Inject]
        public BotUnitRoleResolver() { }

        public BotUnitTacticalRole Resolve(UnitClassConfig config)
            => BotAdvancedHeuristics.InferRole(config);
    }
}
