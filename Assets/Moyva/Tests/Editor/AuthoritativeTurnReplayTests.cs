using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Turns.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests
{
    /// <summary>
    /// Multiplayer client reconciliation: <see cref="ITurnStateRestorer.RestoreAuthoritative"/>
    /// applies the host snapshot and replays the covered lifecycle delta so
    /// per-owner progress (construction, recruitment, stamina) and the
    /// calendar-driven economy tick advance on clients exactly once.
    /// </summary>
    public sealed class AuthoritativeTurnReplayTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private C12Fakes.Calendar _calendar;
        private C12Fakes.WorldState _worldState;
        private List<Recorder> _recorders;
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
            _recorders = new List<Recorder> { new Recorder(10), new Recorder(20) };
        }

        [TearDown]
        public void TearDown()
        {
            _turns?.Dispose();
            _container?.UnbindAll();
        }

        private void CreateClientTurnService(TurnRules rules)
        {
            // The client peers with owner "p2"; the host's faction order is p1, p2.
            _turns = new TurnService(
                _signals,
                _worldState,
                _calendar,
                _recorders.ConvertAll(p => (ITurnParticipant)p),
                new List<ITurnBlocker>(),
                new C12Fakes.LocalOwner("p2"),
                rules,
                timerHolds: null,
                calendarRestorer: _calendar);
            _turns.Initialize();
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
            for (int index = 0; index < _recorders.Count; index++)
                _recorders[index].Events.Clear();
        }

        [Test]
        public void FirstAuthoritativeApply_IsSilentBaseline()
        {
            CreateClientTurnService(TurnRules.SequentialDefault);
            StartTwoFactionWorld();

            ((ITurnStateRestorer)_turns).RestoreAuthoritative(
                round: 1, globalTurn: 1, activeOwnerId: "p1", actions: 0,
                mode: TurnFlowMode.Sequential, roundTimerSeconds: 0,
                endedOwners: null, roundSecondsRemaining: 0f,
                calendarTotalHours: 24);

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual("p1", _turns.ActiveOwnerId);
            Assert.IsEmpty(_recorders[0].Events, "The baseline apply must not replay lifecycle callbacks.");
            Assert.AreEqual(0, _calendar.AdvanceCount);
            Assert.AreEqual(24, _calendar.TotalHours);
        }

        [Test]
        public void SequentialDelta_ReplaysEndingAndStart_InHostOrder()
        {
            CreateClientTurnService(TurnRules.SequentialDefault);
            StartTwoFactionWorld();

            ((ITurnStateRestorer)_turns).RestoreAuthoritative(
                1, 1, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            ((ITurnStateRestorer)_turns).RestoreAuthoritative(
                1, 2, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);

            var events = _recorders[0].Events;
            Assert.AreEqual(2, events.Count);
            AssertEvent(events[0], "ending", "p1", globalTurn: 1, round: 1, local: false);
            AssertEvent(events[1], "start", "p2", globalTurn: 2, round: 1, local: true);
            Assert.AreEqual(0, _recorders[0].RoundsCompleted);
            Assert.AreEqual(0, _calendar.AdvanceCount, "Mid-round delta must not advance the calendar.");
            Assert.AreEqual("p2", _turns.ActiveOwnerId);
        }

        [Test]
        public void SequentialDelta_RoundBoundary_ResolvesRoundOnce()
        {
            CreateClientTurnService(TurnRules.SequentialDefault);
            StartTwoFactionWorld();

            ((ITurnStateRestorer)_turns).RestoreAuthoritative(
                1, 1, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            // Host ran: p1 ends → p2 starts (G2) → p2 ends → round resolves → p1 starts R2 (G3).
            ((ITurnStateRestorer)_turns).RestoreAuthoritative(
                2, 3, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f, 48);

            var events = _recorders[0].Events;
            Assert.AreEqual(4, events.Count);
            AssertEvent(events[0], "ending", "p1", 1, 1, local: false);
            AssertEvent(events[1], "start", "p2", 2, 1, local: true);
            AssertEvent(events[2], "ending", "p2", 2, 1, local: true);
            AssertEvent(events[3], "start", "p1", 3, 2, local: false);
            Assert.AreEqual(1, _recorders[0].RoundsCompleted);
            Assert.AreEqual(new[] { 1 }, _recorders[0].CompletedRounds);
            Assert.AreEqual(1, _calendar.AdvanceCount,
                "The round wrap replays the host's single calendar advance (economy tick).");
            Assert.AreEqual(48, _calendar.TotalHours,
                "The snapshot total reconciles after replay.");
            Assert.AreEqual(2, _turns.Round);
        }

        [Test]
        public void DuplicateApply_IsIdempotent()
        {
            CreateClientTurnService(TurnRules.SequentialDefault);
            StartTwoFactionWorld();

            var restorer = (ITurnStateRestorer)_turns;
            restorer.RestoreAuthoritative(1, 1, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            restorer.RestoreAuthoritative(1, 2, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            restorer.RestoreAuthoritative(1, 2, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            restorer.RestoreAuthoritative(1, 2, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);

            Assert.AreEqual(2, _recorders[0].Events.Count,
                "Re-applying an identical snapshot must not replay transitions.");
            Assert.AreEqual(0, _calendar.AdvanceCount);
        }

        [Test]
        public void SilentRestore_ResetsBaseline_NextApplyDoesNotReplay()
        {
            CreateClientTurnService(TurnRules.SequentialDefault);
            StartTwoFactionWorld();
            var restorer = (ITurnStateRestorer)_turns;

            restorer.RestoreAuthoritative(1, 1, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            restorer.RestoreAuthoritative(1, 2, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            Assert.AreEqual(2, _recorders[0].Events.Count);

            // A save/load restore rebuilds state from scratch — the covered
            // delta is already included in the loaded modules' state.
            _recorders[0].Events.Clear();
            restorer.Restore(4, 7, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f);
            restorer.RestoreAuthoritative(4, 8, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 168);

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.IsEmpty(_recorders[0].Events,
                "A post-save authoritative apply is a fresh baseline, not a delta.");
        }

        [Test]
        public void SimultaneousDelta_ReplaysCompletedRound_ForAllAliveOwners()
        {
            CreateClientTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();
            var restorer = (ITurnStateRestorer)_turns;

            restorer.RestoreAuthoritative(
                1, 1, "p1", 0, TurnFlowMode.Simultaneous, 90,
                new List<string> { "p1" }, 45f, 24);
            Assert.IsTrue(((ITurnRoundState)_turns).HasOwnerEndedTurn("p1"));

            // Host resolved round 1: both owners ended at G1, GlobalTurn += 2.
            restorer.RestoreAuthoritative(
                2, 3, "p1", 0, TurnFlowMode.Simultaneous, 90,
                null, 90f, 48);

            var events = _recorders[0].Events;
            Assert.AreEqual(4, events.Count);
            // Host ending order walks the roster in reverse.
            AssertEvent(events[0], "ending", "p2", 1, 1, local: true);
            AssertEvent(events[1], "ending", "p1", 1, 1, local: false);
            AssertEvent(events[2], "start", "p1", 3, 2, local: false);
            AssertEvent(events[3], "start", "p2", 3, 2, local: true);
            Assert.AreEqual(1, _recorders[0].RoundsCompleted);
            Assert.AreEqual(1, _calendar.AdvanceCount);
            Assert.AreEqual(48, _calendar.TotalHours);
            Assert.AreEqual(0, ((ITurnRoundState)_turns).EndedTurnOwnerIds.Count);
        }

        [Test]
        public void ReplicatedContexts_CarryReplicationFlag()
        {
            CreateClientTurnService(TurnRules.SequentialDefault);
            StartTwoFactionWorld();
            var restorer = (ITurnStateRestorer)_turns;

            restorer.RestoreAuthoritative(1, 1, "p1", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);
            restorer.RestoreAuthoritative(1, 2, "p2", 0, TurnFlowMode.Sequential, 0, null, 0f, 24);

            for (int index = 0; index < _recorders[0].Events.Count; index++)
                Assert.IsTrue(_recorders[0].Events[index].replicated,
                    "Replayed callbacks must be marked IsReplicated.");
        }

        private static void AssertEvent(
            Recorder.TurnEvent evt,
            string phase,
            string owner,
            long globalTurn,
            int round,
            bool local)
        {
            Assert.AreEqual(phase, evt.phase, "phase");
            Assert.AreEqual(owner, evt.owner, "owner");
            Assert.AreEqual(globalTurn, evt.globalTurn, "globalTurn");
            Assert.AreEqual(round, evt.round, "round");
            Assert.AreEqual(local, evt.local, "isLocalOwner");
        }

        private sealed class Recorder : ITurnParticipant
        {
            public readonly struct TurnEvent
            {
                public TurnEvent(string phase, TurnContext c)
                {
                    this.phase = phase;
                    owner = c.Faction.OwnerId;
                    globalTurn = c.GlobalTurn;
                    round = c.Round;
                    replicated = c.IsReplicated;
                    local = c.IsLocalOwner;
                }

                public readonly string phase;
                public readonly string owner;
                public readonly long globalTurn;
                public readonly int round;
                public readonly bool replicated;
                public readonly bool local;
            }

            public Recorder(int turnOrder) => TurnOrder = turnOrder;

            public int TurnOrder { get; }
            public readonly List<TurnEvent> Events = new();
            public readonly List<int> CompletedRounds = new();
            public int RoundsCompleted => CompletedRounds.Count;

            public void OnTurnStarted(TurnContext context) => Events.Add(new TurnEvent("start", context));
            public void OnTurnEnding(TurnContext context) => Events.Add(new TurnEvent("ending", context));
            public void OnRoundCompleted(int completedRound) => CompletedRounds.Add(completedRound);
        }
    }
}
