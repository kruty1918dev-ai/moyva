using System;
using System.Collections.Generic;
using Kruty1918.Calendar.Config;
using Kruty1918.Calendar.Core;
using Kruty1918.Calendar.Domain;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Turns.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests
{
    /// <summary>
    /// C12 — one authoritative turn/round boundary: participants resolve in
    /// turn order, the calendar advances exactly once per completed round,
    /// a failed resolution blocks the next round instead of double-running
    /// participants, and end-turn spam can never double-advance.
    /// </summary>
    public sealed class C12AcceptanceTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private C12Fakes.Calendar _calendar;
        private C12Fakes.WorldState _worldState;
        private List<C12Fakes.Participant> _participants;
        private TurnService _turns;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();
            _container.DeclareSignal<WorldBuiltSignal>().OptionalSubscriber();
            _container.DeclareSignal<FactionEliminatedSignal>().OptionalSubscriber();
            _container.DeclareSignal<GameEndedSignal>().OptionalSubscriber();
            _container.DeclareSignal<GameStartedSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            _calendar = new C12Fakes.Calendar();
            _worldState = new C12Fakes.WorldState();
            _participants = new List<C12Fakes.Participant>
            {
                new C12Fakes.Participant(turnOrder: 10),
                new C12Fakes.Participant(turnOrder: 20),
            };

            _turns = new TurnService(
                _signals,
                _worldState,
                _calendar,
                new List<ITurnParticipant>(_participants),
                new List<ITurnBlocker>(),
                new C12Fakes.LocalOwner("p1"));
            _turns.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _turns?.Dispose();
            _container?.UnbindAll();
        }

        private void StartTwoFactionWorld()
        {
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Source = WorldSpawnPositionsSource.GeneratedHost,
                Assignments = new[]
                {
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 0,
                        ParticipantId = "p1",
                        Position = new Vector2Int(0, 0)
                    },
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 1,
                        ParticipantId = "p2",
                        Position = new Vector2Int(4, 4)
                    },
                }
            });
            _signals.Fire(new WorldBuiltSignal());
        }

        [Test]
        public void Round_CompletesOnce_PerFullRotation()
        {
            StartTwoFactionWorld();
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual("p1", _turns.ActiveOwnerId);

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));
            Assert.AreEqual("p2", _turns.ActiveOwnerId);
            Assert.AreEqual(1, _turns.Round);
            Assert.AreEqual(0, _calendar.AdvanceCount,
                "Mid-round turn end must not advance the calendar");

            Assert.IsTrue(_turns.TryEndTurn("p2", out _));
            Assert.AreEqual(2, _turns.Round);
            Assert.AreEqual(1, _calendar.AdvanceCount,
                "Round boundary advances the calendar exactly once");
            Assert.AreEqual(
                1, _participants[0].RoundCompletedCalls);
            Assert.AreEqual(
                1, _participants[1].RoundCompletedCalls);
            Assert.AreEqual(
                1, _participants[0].LastCompletedRound);
        }

        [Test]
        public void Participants_ResolveInTurnOrder()
        {
            StartTwoFactionWorld();
            var order = new List<int>();
            _participants[0].OnRound = () => order.Add(10);
            _participants[1].OnRound = () => order.Add(20);

            _turns.TryEndTurn("p1", out _);
            _turns.TryEndTurn("p2", out _);

            Assert.AreEqual(new[] { 10, 20 }, order.ToArray());
        }

        [Test]
        public void EndTurn_DoubleSubmit_AdvancesOnce()
        {
            StartTwoFactionWorld();

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));
            long globalAfterFirst = _turns.GlobalTurn;

            Assert.IsFalse(
                _turns.TryEndTurn("p1", out _),
                "A second submit from the same owner must be rejected");
            Assert.AreEqual(
                globalAfterFirst, _turns.GlobalTurn,
                "No double advancement");
        }

        [Test]
        public void Eliminated_Faction_Skips_ItsTurn()
        {
            StartTwoFactionWorld();
            _signals.Fire(new FactionEliminatedSignal { FactionId = "p2" });

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));

            Assert.AreEqual("p1", _turns.ActiveOwnerId,
                "Turn must wrap back to the surviving faction");
            Assert.AreEqual(1, _calendar.AdvanceCount,
                "Wrapping past the eliminated faction completes one round");
        }

        [Test]
        public void FailedResolution_BlocksNextRound_WithoutDoubleWork()
        {
            StartTwoFactionWorld();
            _participants[0].ThrowOnRound = true;
            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Error,
                new System.Text.RegularExpressions.Regex(
                    @"\[RoundResolution\] failed round=1"));
            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Error,
                new System.Text.RegularExpressions.Regex(
                    @"\[Turns\] Round 1 resolution failed"));

            _turns.TryEndTurn("p1", out _);
            Assert.IsFalse(_turns.TryEndTurn("p2", out _),
                "A throwing participant fails the round resolution");

            // The lifecycle is stuck in Resolving — TryEndTurn must refuse
            // rather than re-run participants.
            Assert.AreEqual(
                0, _participants[1].RoundCompletedCalls,
                "Throwing participant[0] aborts the round before [1] runs");
            Assert.AreEqual(0, _calendar.AdvanceCount);

            Assert.IsFalse(
                _turns.TryEndTurn("p2", out _),
                "Retry is blocked while the failed round is unresolved");
            Assert.AreEqual(
                0, _participants[1].RoundCompletedCalls,
                "A refused retry must not execute pending participants");
            Assert.AreEqual(0, _calendar.AdvanceCount,
                "Calendar must not advance for a failed round");
        }

        [Test]
        public void SingleFaction_WrapCompletesRound()
        {
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Source = WorldSpawnPositionsSource.GeneratedHost,
                Assignments = new[]
                {
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 0,
                        ParticipantId = "p1",
                        Position = new Vector2Int(0, 0)
                    },
                }
            });
            _signals.Fire(new WorldBuiltSignal());

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));
            Assert.AreEqual(2, _turns.Round);
            Assert.AreEqual(1, _calendar.AdvanceCount);
        }
    }

    internal static class C12Fakes
    {
        internal sealed class Calendar : ICalendarService
        {
            public int AdvanceCount;
            public long TotalHours;
            public GameDateTime Current => default;
            public long TotalHoursSinceEpoch => TotalHours;
            public DayPhase CurrentDayPhase => default;
            public CalendarConfig Config { get; } = new CalendarConfig(
                1, 1, 1, 1, 0, 12, 30, 24, 6, 20, 1, 1, 1);
            public event Action OnHourChanged { add { } remove { } }
            public event Action OnDayChanged { add { } remove { } }
            public event Action OnMonthChanged { add { } remove { } }
            public event Action OnYearChanged { add { } remove { } }
            public event Action<DayPhase> OnDayPhaseChanged { add { } remove { } }
            public void AdvanceTurn()
            {
                AdvanceCount++;
                TotalHours += Config.HoursPerTurn;
            }
            public void SetByTotalHours(long totalHours)
                => TotalHours = totalHours;
        }

        internal sealed class WorldState : IWorldGenerationSignalState
        {
            public long BeginWorldSnapshotCycle(string sessionId) => 1;
            public void Clear() { }
            public bool TryGetCurrentWorldIdentity(
                out long startupSequence, out string sessionId)
            {
                startupSequence = 1;
                sessionId = "test";
                return true;
            }
            public WorldGeneratedDataSignal StoreWorldGeneratedData(
                WorldGeneratedDataSignal signal) => signal;
            public bool TryGetWorldGeneratedData(
                out WorldGeneratedDataSignal signal)
            {
                signal = default;
                return false;
            }
            public bool TryStoreWorldSpawnPositions(
                WorldSpawnPositionsSignal signal,
                out WorldSpawnPositionsSignal storedSignal)
            {
                storedSignal = signal;
                return true;
            }
            public bool TryGetWorldSpawnPositions(
                out WorldSpawnPositionsSignal signal)
            {
                signal = default;
                return false;
            }
        }

        internal sealed class Participant : ITurnParticipant
        {
            public int TurnOrder { get; }
            public int RoundCompletedCalls;
            public int LastCompletedRound;
            public bool ThrowOnRound;
            public Action OnRound;

            public Participant(int turnOrder) => TurnOrder = turnOrder;

            public void OnTurnStarted(TurnContext context) { }
            public void OnTurnEnding(TurnContext context) { }
            public void OnRoundCompleted(int completedRound)
            {
                if (ThrowOnRound)
                    throw new InvalidOperationException("fixture failure");
                RoundCompletedCalls++;
                LastCompletedRound = completedRound;
                OnRound?.Invoke();
            }
        }

        internal sealed class LocalOwner : ITurnLocalOwnerResolver
        {
            private readonly string _ownerId;
            public LocalOwner(string ownerId) => _ownerId = ownerId;
            public string ResolveLocalOwnerId(IReadOnlyList<TurnFaction> factions)
                => _ownerId;
        }
    }
}
