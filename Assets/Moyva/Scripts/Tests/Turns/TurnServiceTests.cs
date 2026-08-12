using System.Collections.Generic;
using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Turns.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Turns
{
    public sealed class TurnServiceTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private GameCalendarService _calendar;
        private TurnService _turns;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();
            _container.DeclareSignal<WorldBuiltSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();
            _calendar = new GameCalendarService(CalendarConfig.Default());
            _turns = new TurnService(
                _signals,
                new WorldGenerationSignalState(),
                _calendar,
                new List<ITurnParticipant>(),
                new List<ITurnBlocker>());
            _turns.Initialize();
        }

        [TearDown]
        public void TearDown() => _turns.Dispose();

        [Test]
        public void TwoFactions_CalendarAdvancesOnlyAfterFullRound()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            long before = _calendar.TotalHoursSinceEpoch;
            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));
            Assert.AreEqual(before, _calendar.TotalHoursSinceEpoch);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);

            Assert.IsTrue(_turns.TryEndTurn("bot_0", out _));
            Assert.AreEqual(before + _calendar.Config.HoursPerTurn, _calendar.TotalHoursSinceEpoch);
            Assert.AreEqual(2, _turns.Round);
        }

        [Test]
        public void Solo_EndTurnStartsNextRoundForSameOwner()
        {
            Start(new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false });
            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));
            Assert.AreEqual("player_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.Round);
            Assert.AreEqual(2, _turns.GlobalTurn);
        }

        [Test]
        public void ActionCounter_RejectsInactiveOwnerAndResetsNextTurn()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });
            Assert.IsFalse(_turns.TryRecordAction("bot_0", "move"));
            Assert.IsTrue(_turns.TryRecordAction("player_0", "move"));
            Assert.AreEqual(1, _turns.ActionsThisTurn);
            _turns.TryEndTurn("player_0", out _);
            Assert.AreEqual(0, _turns.ActionsThisTurn);
        }

        private void Start(params SpawnPositionAssignment[] assignments)
        {
            _signals.Fire(new WorldSpawnPositionsSignal { Assignments = assignments, Source = WorldSpawnPositionsSource.DirectGameplayTest });
            _signals.Fire<WorldBuiltSignal>();
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }
    }
}
