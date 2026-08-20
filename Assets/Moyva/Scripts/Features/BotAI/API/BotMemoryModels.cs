using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public enum BotKnownEntityKind
    {
        Unit = 0,
        Building = 1,
        Objective = 2,
    }

    public readonly struct BotKnownEntityMemory
    {
        public BotKnownEntityMemory(
            string entityId,
            BotKnownEntityKind kind,
            string ownerId,
            string typeId,
            Vector2Int lastKnownPosition,
            long lastSeenGlobalTurn,
            int lastKnownHp = 0,
            bool wasConfirmedDestroyed = false)
        {
            EntityId = Normalize(entityId);
            Kind = kind;
            OwnerId = Normalize(ownerId);
            TypeId = Normalize(typeId);
            LastKnownPosition = lastKnownPosition;
            LastSeenGlobalTurn = lastSeenGlobalTurn < 1 ? 1 : lastSeenGlobalTurn;
            LastKnownHp = lastKnownHp < 0 ? 0 : lastKnownHp;
            WasConfirmedDestroyed = wasConfirmedDestroyed;
        }

        public string EntityId { get; }
        public BotKnownEntityKind Kind { get; }
        public string OwnerId { get; }
        public string TypeId { get; }
        public Vector2Int LastKnownPosition { get; }
        public long LastSeenGlobalTurn { get; }
        public int LastKnownHp { get; }
        public bool WasConfirmedDestroyed { get; }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
