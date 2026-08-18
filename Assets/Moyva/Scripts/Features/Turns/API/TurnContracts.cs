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
    }

    public readonly struct TurnFaction
    {
        public TurnFaction(string ownerId, bool isBot, Vector2Int startPosition)
        {
            OwnerId = ownerId ?? string.Empty;
            IsBot = isBot;
            StartPosition = startPosition;
        }

        public string OwnerId { get; }
        public bool IsBot { get; }
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
        bool IsActiveFactionBot { get; }
        IReadOnlyList<TurnFaction> Factions { get; }

        bool IsOwnerActive(string ownerId);
        bool CanOwnerAct(string ownerId, out string reason);
        bool TryRecordAction(string ownerId, string actionId);
        bool TryEndTurn(string requesterOwnerId, out string reason);
    }

    public interface ITurnStateRestorer
    {
        void Restore(int round, long globalTurn, string activeOwnerId, int actions);
    }
}
