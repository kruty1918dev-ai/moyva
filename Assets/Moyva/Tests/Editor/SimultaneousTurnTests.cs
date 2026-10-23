using System;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Turns.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests
{
    /// <summary>
    /// Simultaneous (Civilization-style) rounds: every living faction acts in
    /// the same AwaitingInput window, the round closes when all owners finish
    /// or the authoritative timer expires, and per-owner blockers evaluate for
    /// the requesting owner rather than the active cursor.
    /// </summary>
    public sealed class SimultaneousTurnTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private C12Fakes.Calendar _calendar;
        private C12Fakes.WorldState _worldState;
        private List<C12Fakes.Participant> _participants;
        private List<ITurnBlocker> _blockers;
        private List<ITurnTimerHold> _timerHolds;
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
            _blockers = new List<ITurnBlocker>();
            _timerHolds = new List<ITurnTimerHold>();
        }

        [TearDown]
        public void TearDown()
        {
            _turns?.Dispose();
            _container?.UnbindAll();
        }

        private void CreateTurnService(TurnRules rules)
        {
            _turns = new TurnService(
                _signals,
                _worldState,
                _calendar,
                _participants.ConvertAll(p => (ITurnParticipant)p),
                _blockers,
                new C12Fakes.LocalOwner("p1"),
                rules,
                _timerHolds);
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
        }

        private void ForceRoundDeadlinePassed()
        {
            typeof(TurnService)
                .GetField("_roundEndsAtUnscaled", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_turns, float.NegativeInfinity);
        }

        [Test]
        public void Simultaneous_AllOwnersCanAct_InSharedRound()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.IsTrue(_turns.CanOwnerAct("p1", out _));
            Assert.IsTrue(_turns.CanOwnerAct("p2", out _),
                "A non-cursor faction must act during the shared simultaneous round.");
            Assert.IsTrue(_turns.TryRecordAction("p2", "test-action"));
        }

        [Test]
        public void Simultaneous_OneOwnerEnds_RoundStaysOpen()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual(1, _turns.Round);
            Assert.IsTrue(((ITurnRoundState)_turns).HasOwnerEndedTurn("p1"));
            Assert.IsFalse(((ITurnRoundState)_turns).HasOwnerEndedTurn("p2"));
        }

        [Test]
        public void Simultaneous_AllOwnersEnd_RoundResolvesEarly()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));
            Assert.IsTrue(_turns.TryEndTurn("p2", out _));
            Assert.AreEqual(2, _turns.Round, "All owners ending resolves the round without waiting for the timer.");
            Assert.AreEqual(0, ((ITurnRoundState)_turns).EndedTurnOwnerIds.Count);
            Assert.AreEqual(1, _calendar.AdvanceCount);
        }

        [Test]
        public void Simultaneous_EndedOwner_CannotActOrEndAgain()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            Assert.IsTrue(_turns.TryEndTurn("p1", out _));
            Assert.IsFalse(_turns.CanOwnerAct("p1", out _));
            Assert.IsFalse(_turns.TryEndTurn("p1", out _));
            Assert.IsTrue(_turns.CanOwnerAct("p2", out _));
        }

        [Test]
        public void Simultaneous_TimerExpiry_CompletesRound()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();
            Assert.IsTrue(((ITurnRoundState)_turns).RoundTimerSeconds == 90);

            ForceRoundDeadlinePassed();
            _turns.TickRoundTimer();

            Assert.AreEqual(2, _turns.Round, "Deadline expiry on the authoritative peer resolves the round.");
        }

        [Test]
        public void Simultaneous_TimerHold_PausesDeadline()
        {
            var hold = new FakeTimerHold { Hold = true };
            _timerHolds.Add(hold);
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            ForceRoundDeadlinePassed();
            _turns.TickRoundTimer();
            Assert.AreEqual(1, _turns.Round, "An active hold must pause the round deadline.");
            Assert.IsTrue(((ITurnRoundState)_turns).RoundTimerHeld);

            hold.Hold = false;
            ForceRoundDeadlinePassed();
            _turns.TickRoundTimer();
            Assert.AreEqual(2, _turns.Round);
        }

        [Test]
        public void Simultaneous_OwnerBlocker_BlocksRequestingOwnerOnly()
        {
            _blockers.Add(new FakeOwnerBlocker { BlockedOwnerId = "p2", Reason = "Castle missing." });
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            Assert.IsTrue(_turns.TryEndTurn("p1", out _),
                "A blocker scoped to p2 must not stall p1's end-turn.");
            Assert.IsFalse(_turns.TryEndTurn("p2", out string reason));
            Assert.AreEqual("Castle missing.", reason);
            Assert.AreEqual(1, _turns.Round);
        }

        [Test]
        public void Simultaneous_Restore_PreservesEndedOwnersAndTimer()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Simultaneous, 90, true));
            StartTwoFactionWorld();

            _turns.Restore(
                round: 2,
                globalTurn: 3,
                activeOwnerId: "p2",
                actions: 0,
                mode: TurnFlowMode.Simultaneous,
                roundTimerSeconds: 90,
                endedOwners: new List<string> { "p1" },
                roundSecondsRemaining: 45f);

            var state = (ITurnRoundState)_turns;
            Assert.AreEqual(2, _turns.Round);
            Assert.IsTrue(state.HasOwnerEndedTurn("p1"));
            Assert.IsFalse(state.HasOwnerEndedTurn("p2"));
            Assert.Greater(state.RoundSecondsRemaining, 40f);
            Assert.LessOrEqual(state.RoundSecondsRemaining, 90f);
        }

        [Test]
        public void Sequential_StillRequiresActiveOwner()
        {
            CreateTurnService(new TurnRules(TurnFlowMode.Sequential, 0, true));
            StartTwoFactionWorld();

            Assert.IsTrue(_turns.CanOwnerAct("p1", out _));
            Assert.IsFalse(_turns.CanOwnerAct("p2", out _),
                "Sequential mode keeps the one-active-faction contract.");
        }

        private sealed class FakeTimerHold : ITurnTimerHold
        {
            public bool Hold;
            public bool ShouldHoldRoundTimer(out string reason)
            {
                reason = Hold ? "setup" : null;
                return Hold;
            }
        }

        private sealed class FakeOwnerBlocker : ITurnBlocker, ITurnOwnerBlocker
        {
            public string BlockedOwnerId;
            public string Reason;

            public bool IsTurnBlocked(out string reason)
            {
                reason = Reason;
                return false;
            }

            public bool IsTurnBlocked(string ownerId, out string reason)
            {
                reason = Reason;
                return string.Equals(ownerId, BlockedOwnerId, StringComparison.Ordinal);
            }
        }
    }
}
