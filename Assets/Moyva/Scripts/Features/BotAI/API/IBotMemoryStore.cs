using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotMemoryStore
    {
        IReadOnlyList<BotKnownEntityMemory> GetMemory(string ownerId, long globalTurn);
        void UpdateFromObservation(BotWorldSnapshot snapshot, BotPlanningProfile profile);
        int GetConfidence(BotKnownEntityMemory memory, long globalTurn);
        IReadOnlyList<BotMemoryOwnerSnapshot> CaptureMemory();
        void RestoreMemory(IReadOnlyList<BotMemoryOwnerSnapshot> snapshots);
    }
}
