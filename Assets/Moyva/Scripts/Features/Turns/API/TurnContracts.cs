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

    /// <summary>
    /// How factions take their turns inside a round. Sequential is the legacy
    /// one-faction-at-a-time order; Simultaneous lets every living faction act
    /// during the same AwaitingInput window and resolves the round once all of
    /// them finish or the round timer expires (Civilization-style).
    /// </summary>
    public enum TurnFlowMode
    {
        Sequential = 0,
        Simultaneous = 1,
    }

    /// <summary>
    /// Immutable session-level turn rules resolved once per gameplay session
    /// from launch context and the JSON turn-rules preset.
    /// </summary>
    public readonly struct TurnRules
    {
        public TurnRules(
            TurnFlowMode mode,
            int roundTimerSeconds,
            bool holdTimerUntilInitialCastlesPlaced)
        {
            Mode = mode;
            RoundTimerSeconds = roundTimerSeconds < 0 ? 0 : roundTimerSeconds;
            HoldTimerUntilInitialCastlesPlaced = holdTimerUntilInitialCastlesPlaced;
        }

        public TurnFlowMode Mode { get; }
        /// <summary>Seconds each simultaneous round lasts; 0 disables the timer.</summary>
        public int RoundTimerSeconds { get; }
        /// <summary>Pause the round timer while any faction still owes its initial castle.</summary>
        public bool HoldTimerUntilInitialCastlesPlaced { get; }

        public static TurnRules SequentialDefault
            => new(TurnFlowMode.Sequential, 0, true);
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
            : this(round, globalTurn, factionIndex, faction, TurnFlowMode.Sequential, false)
        {
        }

        public TurnContext(
            int round,
            long globalTurn,
            int factionIndex,
            TurnFaction faction,
            TurnFlowMode mode,
            bool isLocalOwner)
            : this(round, globalTurn, factionIndex, faction, mode, isLocalOwner, false)
        {
        }

        public TurnContext(
            int round,
            long globalTurn,
            int factionIndex,
            TurnFaction faction,
            TurnFlowMode mode,
            bool isLocalOwner,
            bool isReplicated)
        {
            Round = round;
            GlobalTurn = globalTurn;
            FactionIndex = factionIndex;
            Faction = faction;
            Mode = mode;
            IsLocalOwner = isLocalOwner;
            IsReplicated = isReplicated;
        }

        public int Round { get; }
        public long GlobalTurn { get; }
        public int FactionIndex { get; }
        public TurnFaction Faction { get; }
        /// <summary>Flow mode the turn service is running.</summary>
        public TurnFlowMode Mode { get; }
        /// <summary>
        /// True when <see cref="Faction"/> is the local player's faction. In
        /// simultaneous mode every faction receives turn callbacks at round
        /// start, so local-only side effects must check this flag.
        /// </summary>
        public bool IsLocalOwner { get; }
        /// <summary>
        /// True when this callback replays a transition the authoritative peer
        /// already executed (multiplayer client reconciliation). Effects that
        /// must stay single-sourced on the host (analytics, remote calls)
        /// should skip replicated contexts.
        /// </summary>
        public bool IsReplicated { get; }
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
    /// Owner-scoped turn blocker. Evaluated per requesting owner instead of
    /// implicitly applying to whoever holds the turn, which matters in
    /// simultaneous mode where the host validates remote players' end-turn
    /// requests while local-only blockers (in-flight movement animations,
    /// deployment sessions) must not stall a remote owner's round.
    /// </summary>
    public interface ITurnOwnerBlocker
    {
        bool IsTurnBlocked(string ownerId, out string reason);
    }

    /// <summary>
    /// Requests the simultaneous-round timer to pause. Evaluated by the
    /// authoritative timer each frame; while any hold is active the deadline
    /// does not advance. Used so round 1 cannot expire before every player had
    /// a fair chance to place the initial castle.
    /// </summary>
    public interface ITurnTimerHold
    {
        bool ShouldHoldRoundTimer(out string reason);
    }

    /// <summary>
    /// Simultaneous-round state query exposed by the turn service. Kept
    /// separate from <see cref="ITurnService"/> so sequential-only fakes in
    /// tests and offline callers do not have to know about round readiness.
    /// </summary>
    public interface ITurnRoundState
    {
        TurnFlowMode FlowMode { get; }
        /// <summary>Configured round timer in seconds; 0 disables the timer.</summary>
        int RoundTimerSeconds { get; }
        /// <summary>Seconds left in the current round; 0 when no timer is running.</summary>
        float RoundSecondsRemaining { get; }
        /// <summary>True while a registered <see cref="ITurnTimerHold"/> pauses the timer.</summary>
        bool RoundTimerHeld { get; }
        /// <summary>Owners that already finished the current round (simultaneous mode).</summary>
        IReadOnlyCollection<string> EndedTurnOwnerIds { get; }
        bool HasOwnerEndedTurn(string ownerId);
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

    public interface ITurnEndQuery
    {
        bool CanEndTurn(string ownerId, out string reason);
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

        /// <summary>
        /// Restores the full synchronized round projection: authoritative flow
        /// mode and timer, which owners already finished the round and how
        /// much round time is left. Sequential-mode callers may pass
        /// null/default values.
        /// </summary>
        void Restore(
            int round,
            long globalTurn,
            string activeOwnerId,
            int actions,
            TurnFlowMode mode,
            int roundTimerSeconds,
            IReadOnlyCollection<string> endedOwners,
            float roundSecondsRemaining);

        /// <summary>
        /// Applies an authoritative replicated projection on a non-authoritative
        /// peer. The first call after a (re)configured faction set is a silent
        /// baseline; subsequent calls replay the deterministic turn lifecycle
        /// the host already executed for the covered delta — round resolution
        /// (with the calendar advance that drives economy) plus per-owner turn
        /// starts — so client-side construction, recruitment, stamina and
        /// economy stay consistent without accepting commands locally.
        /// </summary>
        void RestoreAuthoritative(
            int round,
            long globalTurn,
            string activeOwnerId,
            int actions,
            TurnFlowMode mode,
            int roundTimerSeconds,
            IReadOnlyCollection<string> endedOwners,
            float roundSecondsRemaining,
            long calendarTotalHours);
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
