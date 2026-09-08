using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Turns.API
{
    public enum TurnPhase
    {
        Initializing,
        Starting,
        AwaitingInput,
        Resolving,
        Ending,
        Completed,
    }

    public readonly struct TurnFaction
    {
        /// <summary>Створює фракцію учасника в порядку ходів.</summary>
        public TurnFaction(string ownerId, Vector2Int startPosition)
        {
            OwnerId = ownerId ?? string.Empty;
            StartPosition = startPosition;
        }

        public string OwnerId { get; }
        public Vector2Int StartPosition { get; }
    }

    public readonly struct TurnContext
    {
        public TurnContext(int round, long globalTurn, int factionIndex, TurnFaction faction)
        {
            Round = round;
            GlobalTurn = globalTurn;
            FactionIndex = factionIndex;
            Faction = faction;
        }

        public int Round { get; }
        public long GlobalTurn { get; }
        public int FactionIndex { get; }
        public TurnFaction Faction { get; }
    }

    public interface ITurnParticipant
    {
        int TurnOrder { get; }
        void OnTurnStarted(TurnContext context);
        void OnTurnEnding(TurnContext context);
        void OnRoundCompleted(int completedRound);
    }

    public interface ITurnBlocker
    {
        bool IsTurnBlocked(out string reason);
    }

    /// <summary>
    /// Resolves the owner controlled by this local process from the ordered turn registry.
    /// Multiplayer implementations must treat the session's local participant id as authoritative;
    /// returning an empty string is safer than silently assigning another human participant.
    /// </summary>
    public interface ITurnLocalOwnerResolver
    {
        string ResolveLocalOwnerId(IReadOnlyList<TurnFaction> factions);
    }

    public interface ITurnService
    {
        event Action StateChanged;

        TurnPhase Phase { get; }
        int Round { get; }
        long GlobalTurn { get; }
        int ActionsThisTurn { get; }
        string ActiveOwnerId { get; }
        string LocalOwnerId { get; }
        IReadOnlyList<TurnFaction> Factions { get; }

        bool IsOwnerActive(string ownerId);
        bool CanOwnerAct(string ownerId, out string reason);
        bool TryRecordAction(string ownerId, string actionId);
        bool TryEndTurn(string requesterOwnerId, out string reason);
    }

    public readonly struct TurnParticipantHistorySnapshot
    {
        public TurnParticipantHistorySnapshot(
            string ownerId,
            long completedTurns,
            bool isActive,
            bool isLocal,
            bool isEliminated = false)
        {
            OwnerId = ownerId ?? string.Empty;
            CompletedTurns = completedTurns;
            IsActive = isActive;
            IsLocal = isLocal;
            IsEliminated = isEliminated;
        }

        public string OwnerId { get; }
        public long CompletedTurns { get; }
        public bool IsActive { get; }
        public bool IsLocal { get; }
        public bool IsEliminated { get; }
    }

    public interface ITurnHistoryQuery
    {
        IReadOnlyList<TurnParticipantHistorySnapshot> GetParticipantHistory();
    }

    public interface ITurnStateRestorer
    {
        void Restore(int round, long globalTurn, string activeOwnerId, int actions);
    }

    public interface ITurnHistoryRestorer
    {
        // Null restores legacy counters from the saved global turn and faction order.
        void RestoreHistory(IReadOnlyList<TurnParticipantHistorySnapshot> history);
    }

    public interface ITurnAuthorityPolicy
    {
        bool IsAuthoritative { get; }
    }

    public interface ITurnRemoteCommandRequester
    {
        event Action<bool, string> EndTurnResolved;
        bool IsEndTurnPending { get; }
        bool TryRequestEndTurn(out string reason);
    }

    public enum GameplayProgressMode
    {
        TurnBased = 0,
        SandboxRealtime = 1,
    }

    public readonly struct GameplayProgressTick
    {
        public GameplayProgressTick(
            GameplayProgressMode mode,
            string ownerId,
            long sequence,
            float gameplaySeconds)
        {
            Mode = mode;
            OwnerId = ownerId ?? string.Empty;
            Sequence = sequence < 1 ? 1 : sequence;
            GameplaySeconds = gameplaySeconds < 0f ? 0f : gameplaySeconds;
        }

        public GameplayProgressMode Mode { get; }
        public string OwnerId { get; }
        public long Sequence { get; }
        public float GameplaySeconds { get; }
        public bool IsRealtime => Mode == GameplayProgressMode.SandboxRealtime;
    }

    /// <summary>
    /// Shared progress boundary for systems whose work advances by gameplay time.
    /// TurnService remains the authority for turn order; this clock only publishes
    /// sandbox realtime ticks and exposes their deterministic progress sequence.
    /// </summary>
    public interface IGameplayProgressClock
    {
        event Action<GameplayProgressTick> Progressed;

        GameplayProgressMode Mode { get; }
        bool IsRealtime { get; }
        float SandboxRoundSeconds { get; }
        float Speed { get; }
        long CurrentSequence { get; }
        double ElapsedGameplaySeconds { get; }
        float SecondsUntilNextProgress { get; }

        void Configure(GameplayProgressMode mode, float sandboxRoundSeconds, float speed);
        void SetSpeed(float speed);
    }
}
