using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// Round-1 gate: an owner that still owes the initial castle cannot finish
    /// its turn, the hold pauses the simultaneous round timer while any living
    /// faction is missing one, and eliminated factions never hold the timer.
    /// </summary>
    public sealed class InitialCastleTurnBlockerTests
    {
        private FakeBootstrap _bootstrap;
        private FakeTurns _turns;
        private FakeHistory _history;
        private InitialCastleTurnBlocker _blocker;

        [SetUp]
        public void SetUp()
        {
            _bootstrap = new FakeBootstrap();
            _turns = new FakeTurns();
            _history = new FakeHistory();
            _blocker = new InitialCastleTurnBlocker(_bootstrap, _turns, _history);
        }

        [Test]
        public void OwnerWithoutCastle_CannotEndTurn()
        {
            _bootstrap.MissingCastleOwners.Add("p2");

            Assert.IsTrue(_blocker.IsTurnBlocked("p2", out string reason));
            Assert.IsNotEmpty(reason);
        }

        [Test]
        public void OwnerWithCastle_EndsTurn()
        {
            Assert.IsFalse(_blocker.IsTurnBlocked("p2", out _));
        }

        [Test]
        public void TimerHeld_WhileAnyLivingFactionMissesCastle()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", new Vector2Int(0, 0)),
                new TurnFaction("p2", new Vector2Int(4, 4)),
            };
            _bootstrap.MissingCastleOwners.Add("p2");

            Assert.IsTrue(_blocker.ShouldHoldRoundTimer(out _));
        }

        [Test]
        public void TimerReleased_WhenAllCastlesPlaced()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", new Vector2Int(0, 0)),
            };

            Assert.IsFalse(_blocker.ShouldHoldRoundTimer(out _));
        }

        [Test]
        public void EliminatedFaction_DoesNotHoldTimer()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", new Vector2Int(0, 0)),
                new TurnFaction("p2", new Vector2Int(4, 4)),
            };
            _bootstrap.MissingCastleOwners.Add("p2");
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 0, false, false, isEliminated: true));

            Assert.IsFalse(_blocker.ShouldHoldRoundTimer(out _));
        }

        private sealed class FakeBootstrap : IConstructionBootstrapQuery
        {
            public readonly HashSet<string> MissingCastleOwners = new(StringComparer.Ordinal);

            public bool RequiresInitialCastle(string ownerId, out string castleBuildingId)
            {
                castleBuildingId = "castle";
                return MissingCastleOwners.Contains(ownerId ?? string.Empty);
            }

            public bool IsCastleBuilding(string buildingId) => buildingId == "castle";
        }

        private sealed class FakeTurns : ITurnService
        {
            public event Action StateChanged { add { } remove { } }
            public TurnPhase Phase { get; set; } = TurnPhase.AwaitingInput;
            public int Round { get; set; } = 1;
            public long GlobalTurn { get; set; } = 1;
            public int ActionsThisTurn { get; set; }
            public string ActiveOwnerId { get; set; } = "p1";
            public string LocalOwnerId { get; set; } = "p1";
            public IReadOnlyList<TurnFaction> Factions { get; set; } = new List<TurnFaction>();

            public bool IsOwnerActive(string ownerId) => ownerId == ActiveOwnerId;
            public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return true; }
            public bool TryRecordAction(string ownerId, string actionId) => true;
            public bool TryEndTurn(string requesterOwnerId, out string reason) { reason = null; return true; }
        }

        private sealed class FakeHistory : ITurnHistoryQuery
        {
            public readonly List<TurnParticipantHistorySnapshot> Rows = new();
            public IReadOnlyList<TurnParticipantHistorySnapshot> GetParticipantHistory() => Rows;
        }
    }
}
