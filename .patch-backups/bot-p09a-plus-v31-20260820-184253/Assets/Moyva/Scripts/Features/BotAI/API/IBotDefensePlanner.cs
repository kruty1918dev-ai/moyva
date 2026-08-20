using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotDefensePlanner
    {
        BotDefenseContext Analyze(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy);

        IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy);

        IReadOnlyList<string> GetProtectedHomeGuardUnitIds(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy);
    }
}
