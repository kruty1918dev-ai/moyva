using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotStrategicStateStore
    {
        IReadOnlyList<BotStrategicStateSnapshot> CaptureStrategicState();
        void RestoreStrategicState(IReadOnlyList<BotStrategicStateSnapshot> states);
    }
}
