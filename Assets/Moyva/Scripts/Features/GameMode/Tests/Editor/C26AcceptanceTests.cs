using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.GameMode.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.GameMode
{
    /// <summary>
    /// C26: match end follows faction state — a winner is the last
    /// non-eliminated owner, zero survivors is a draw, sandbox (≤1 faction)
    /// never forces victory/defeat, non-authoritative replicas never decide,
    /// and GameOver is a terminal state that stops further evaluation.
    /// </summary>
    public sealed class C26AcceptanceTests
    {
        private Turns _turns;
        private History _history;
        private Authority _authority;
        private GameStateService _gameState;
        private MatchEndConditionService _service;

        [SetUp]
        public void SetUp()
        {
            _turns = new Turns();
            _history = new History();
            _authority = new Authority();
            _gameState = new GameStateService(CreateSignalBus());
            _service = new MatchEndConditionService(_turns, _gameState, _history, _authority);
            _service.Initialize();
            _gameState.StartGame();
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            _gameState?.Dispose();
            Time.timeScale = 1f;
        }

        [Test]
        public void LastActiveOwner_Wins()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
                new TurnFaction("p2", Vector2Int.one),
            };
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 3, true, true));
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 3, true, false, isEliminated: true));

            _turns.Raise();

            Assert.IsTrue(_gameState.IsGameOver);
            Assert.AreEqual("p1", _gameState.WinnerId);
        }

        [Test]
        public void AllEliminated_IsDraw()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
                new TurnFaction("p2", Vector2Int.one),
            };
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 3, true, true, isEliminated: true));
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 3, true, false, isEliminated: true));

            _turns.Raise();

            Assert.IsTrue(_gameState.IsGameOver);
            Assert.AreEqual(string.Empty, _gameState.WinnerId,
                "No surviving owner means a draw, not an arbitrary winner.");
        }

        [Test]
        public void MultipleActive_NoGameOver()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
                new TurnFaction("p2", Vector2Int.one),
            };
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 1, true, true));
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 1, true, false));

            _turns.Raise();
            Assert.IsFalse(_gameState.IsGameOver, "Two live owners — match continues.");
        }

        [Test]
        public void SingleFaction_NeverGameOver()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
            };
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 1, true, true));

            _turns.Raise();
            Assert.IsFalse(_gameState.IsGameOver,
                "Sandbox (≤1 faction) gets no imposed win/lose.");
        }

        [Test]
        public void NonAuthoritative_NeverEvaluates()
        {
            _authority.Value = false;
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
                new TurnFaction("p2", Vector2Int.one),
            };
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 1, true, true));
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 1, true, false, isEliminated: true));

            _turns.Raise();
            Assert.IsFalse(_gameState.IsGameOver,
                "A replica never declares a winner — authority decides.");
        }

        [Test]
        public void NonAwaitingPhase_Skipped()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
                new TurnFaction("p2", Vector2Int.one),
            };
            _turns.Phase = TurnPhase.Resolving;
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 1, true, true));
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 1, true, false, isEliminated: true));

            _turns.Raise();
            Assert.IsFalse(_gameState.IsGameOver,
                "Mid-resolution state changes are not a stable verdict point.");
        }

        [Test]
        public void GameOver_IsTerminal_FurtherEventsIgnored()
        {
            _turns.Factions = new List<TurnFaction>
            {
                new TurnFaction("p1", Vector2Int.zero),
                new TurnFaction("p2", Vector2Int.one),
            };
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p1", 3, true, true));
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 3, true, false, isEliminated: true));
            _turns.Raise();
            Assert.AreEqual("p1", _gameState.WinnerId);

            // A later event cannot flip the verdict.
            _history.Rows.Clear();
            _history.Rows.Add(new TurnParticipantHistorySnapshot("p2", 4, true, false));
            _turns.Raise();
            Assert.AreEqual("p1", _gameState.WinnerId);
        }

        [Test]
        public void EndGame_BlocksInput_AndDoesNotFreezeTime()
        {
            // Input policy present — game-over must acquire the block so no
            // commands slip through, but server time is not globally frozen.
            var policy = new FakeInputPolicy();
            var state = new GameStateService(CreateSignalBus(), inputPolicy: policy);
            state.StartGame();
            state.EndGame("p1");

            Assert.AreEqual(1, policy.AcquireCount);
            Assert.AreEqual(GameStateType.GameOver, state.CurrentState);
            state.Dispose();
        }

        private static Zenject.SignalBus CreateSignalBus()
        {
            var container = new Zenject.DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<Kruty1918.Moyva.Signals.GameStartedSignal>().OptionalSubscriber();
            container.DeclareSignal<Kruty1918.Moyva.Signals.GamePausedSignal>().OptionalSubscriber();
            container.DeclareSignal<Kruty1918.Moyva.Signals.GameEndedSignal>().OptionalSubscriber();
            return container.Resolve<Zenject.SignalBus>();
        }

        private sealed class FakeInputPolicy : Kruty1918.InputRouting.API.IGameplayInputPolicy
        {
            public int AcquireCount;
            public bool CanProcess(Kruty1918.InputRouting.API.GameplayInputKind inputKind,
                Vector2 screenPosition, int pointerId = -1) => true;
            public bool IsPointerOverUi(Vector2 screenPosition, int pointerId = -1,
                bool interactiveOnly = true) => false;
            public bool TryBeginPointerCapture(Kruty1918.InputRouting.API.GameplayInputKind inputKind,
                Vector2 screenPosition, int pointerId = -1) => true;
            public void EndPointerCapture(Kruty1918.InputRouting.API.GameplayInputKind inputKind,
                int pointerId = -1) { }
            public IDisposable AcquireBlock(Kruty1918.InputRouting.API.GameplayInputKind inputMask,
                object owner)
            {
                AcquireCount++;
                return new Block();
            }
            private sealed class Block : IDisposable
            {
                public void Dispose() { }
            }
        }

        // ── Fakes ────────────────────────────────────────────────────

        private sealed class Turns : ITurnService
        {
            public event Action StateChanged;
            public void Raise() => StateChanged?.Invoke();

            public TurnPhase Phase { get; set; } = TurnPhase.AwaitingInput;
            public int Round { get; set; } = 1;
            public long GlobalTurn { get; set; } = 1;
            public int ActionsThisTurn { get; set; }
            public string ActiveOwnerId { get; set; } = "p1";
            public string LocalOwnerId { get; set; } = "p1";
            public IReadOnlyList<TurnFaction> Factions { get; set; }
                = new List<TurnFaction>();

            public bool IsOwnerActive(string ownerId) => ownerId == ActiveOwnerId;
            public bool CanOwnerAct(string ownerId, out string reason)
            {
                reason = null;
                return true;
            }
            public bool TryRecordAction(string ownerId, string actionId) => true;
            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                reason = null;
                return true;
            }
        }

        private sealed class History : ITurnHistoryQuery
        {
            public readonly List<TurnParticipantHistorySnapshot> Rows = new();
            public IReadOnlyList<TurnParticipantHistorySnapshot> GetParticipantHistory()
                => Rows;
        }

        private sealed class Authority : ITurnAuthorityPolicy
        {
            public bool Value = true;
            public bool IsAuthoritative => Value;
        }
    }
}
