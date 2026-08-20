using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public readonly struct BotMemoryOwnerSnapshot
    {
        public BotMemoryOwnerSnapshot(string ownerId, IReadOnlyList<BotKnownEntityMemory> records)
        {
            OwnerId = string.IsNullOrWhiteSpace(ownerId) ? string.Empty : ownerId.Trim();
            Records = records ?? System.Array.Empty<BotKnownEntityMemory>();
        }

        public string OwnerId { get; }
        public IReadOnlyList<BotKnownEntityMemory> Records { get; }
    }

    public readonly struct BotStrategicStateSnapshot
    {
        public BotStrategicStateSnapshot(
            string ownerId,
            long globalTurn,
            BotStrategicPosture posture,
            int postureScore,
            string reason)
        {
            OwnerId = string.IsNullOrWhiteSpace(ownerId) ? string.Empty : ownerId.Trim();
            GlobalTurn = globalTurn < 0 ? 0 : globalTurn;
            Posture = posture;
            PostureScore = postureScore;
            Reason = string.IsNullOrWhiteSpace(reason) ? string.Empty : reason.Trim();
        }

        public string OwnerId { get; }
        public long GlobalTurn { get; }
        public BotStrategicPosture Posture { get; }
        public int PostureScore { get; }
        public string Reason { get; }
    }
}
