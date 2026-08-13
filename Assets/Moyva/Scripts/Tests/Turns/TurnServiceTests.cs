using System;
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
        private WorldGenerationSignalState _worldState;
        private TurnService _turns;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();
            _container.DeclareSignal<WorldBuiltSignal>().OptionalSubscriber();
            _container.DeclareSignal<FactionEliminatedSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();
            _calendar = new GameCalendarService(CalendarConfig.Default());
            _worldState = new WorldGenerationSignalState();
            CreateTurnService();
        }

        [TearDown]
        public void TearDown() => _turns?.Dispose();

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

        [Test]
        public void Factions_AreOrderedDeterministically_WithoutMutatingSignalArray()
        {
            var assignments = new[]
            {
                new SpawnPositionAssignment { SlotIndex = 2, ParticipantId = "bot_2", IsBot = true, Position = new Vector2Int(9, 9) },
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false, Position = new Vector2Int(1, 1) },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_1", IsBot = true, Position = new Vector2Int(5, 5) },
            };

            Start(assignments);

            Assert.AreEqual(2, assignments[0].SlotIndex, "TurnService must not sort the publisher-owned array in-place.");
            Assert.AreEqual("player_0", _turns.Factions[0].OwnerId);
            Assert.AreEqual("bot_1", _turns.Factions[1].OwnerId);
            Assert.AreEqual("bot_2", _turns.Factions[2].OwnerId);
        }

        [Test]
        public void MissingParticipantIds_UseSlotStableFallbackOwners()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 7, ParticipantId = null, IsBot = true },
                new SpawnPositionAssignment { SlotIndex = 3, ParticipantId = " ", IsBot = false });

            Assert.AreEqual("player_3", _turns.Factions[0].OwnerId);
            Assert.AreEqual("bot_7", _turns.Factions[1].OwnerId);
        }

        [Test]
        public void LocalOwner_UsesInjectedAuthoritativeResolver()
        {
            CreateTurnService(new FixedLocalOwnerResolver("player_1"));
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "player_1", IsBot = false });

            Assert.AreEqual("player_1", _turns.LocalOwnerId);
        }

        [Test]
        public void LocalOwner_DoesNotSilentlyUseAnotherHuman_WhenResolverCannotMatch()
        {
            CreateTurnService(new FixedLocalOwnerResolver("missing-player"));
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "player_1", IsBot = false });

            Assert.AreEqual(string.Empty, _turns.LocalOwnerId);
        }

        [Test]
        public void WorldBuiltWithoutAssignments_DoesNotInventRuntimeFaction()
        {
            _signals.Fire<WorldBuiltSignal>();

            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);
            Assert.AreEqual(0, _turns.Factions.Count);
            Assert.AreEqual(string.Empty, _turns.LocalOwnerId);
        }

        [Test]
        public void DirectGameplayTestWithoutAssignments_UsesExplicitSoloFallback()
        {
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = Array.Empty<SpawnPositionAssignment>(),
                Source = WorldSpawnPositionsSource.DirectGameplayTest,
            });
            _signals.Fire<WorldBuiltSignal>();

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual(1, _turns.Factions.Count);
            Assert.AreEqual("player_0", _turns.ActiveOwnerId);
            Assert.AreEqual("player_0", _turns.LocalOwnerId);
        }

        [Test]
        public void EliminatedMiddleFaction_IsSkipped_WithoutEarlyCalendarAdvance()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true },
                new SpawnPositionAssignment { SlotIndex = 2, ParticipantId = "bot_1", IsBot = true });
            long before = _calendar.TotalHoursSinceEpoch;

            _signals.Fire(new FactionEliminatedSignal { FactionId = "bot_0" });
            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));

            Assert.AreEqual("bot_1", _turns.ActiveOwnerId);
            Assert.AreEqual(before, _calendar.TotalHoursSinceEpoch);

            Assert.IsTrue(_turns.TryEndTurn("bot_1", out _));
            Assert.AreEqual("player_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.Round);
            Assert.AreEqual(before + _calendar.Config.HoursPerTurn, _calendar.TotalHoursSinceEpoch);
        }

        [Test]
        public void EliminatedLastFaction_WrapsDirectlyToFirstEligibleFaction()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });
            long before = _calendar.TotalHoursSinceEpoch;

            _signals.Fire(new FactionEliminatedSignal { FactionId = "bot_0" });
            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));

            Assert.AreEqual("player_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.Round);
            Assert.AreEqual(2, _turns.GlobalTurn);
            Assert.AreEqual(before + _calendar.Config.HoursPerTurn, _calendar.TotalHoursSinceEpoch);
        }

        [Test]
        public void ActiveFactionElimination_AutomaticallyAdvancesToNextEligibleFaction()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            _signals.Fire(new FactionEliminatedSignal { FactionId = "player_0" });

            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.GlobalTurn);
            Assert.AreEqual(1, _turns.Round);
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }

        [Test]
        public void EliminationBeforeFactionRegistry_IsAppliedWhenMatchingFactionArrives()
        {
            _signals.Fire(new FactionEliminatedSignal { FactionId = "bot_0" });

            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));
            Assert.AreEqual("player_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.Round);
        }

        [Test]
        public void EliminationDuringTurnStart_AdvancesBeforeAwaitingInput()
        {
            CreateTurnService(participants: new List<ITurnParticipant>
            {
                new EliminateOnStartParticipant(_signals, "player_0"),
            });

            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.GlobalTurn);
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }

        [Test]
        public void AllFactionsEliminated_StopsWithoutLoopingOrAdvancingCalendar()
        {
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });
            long before = _calendar.TotalHoursSinceEpoch;

            _signals.Fire(new FactionEliminatedSignal { FactionId = "bot_0" });
            _signals.Fire(new FactionEliminatedSignal { FactionId = "player_0" });

            Assert.AreEqual(TurnPhase.Resolving, _turns.Phase);
            Assert.AreEqual(before, _calendar.TotalHoursSinceEpoch);
            Assert.AreEqual(1, _turns.Round);
            Assert.AreEqual(1, _turns.GlobalTurn);
        }

        private void CreateTurnService(
            ITurnLocalOwnerResolver localOwnerResolver = null,
            List<ITurnParticipant> participants = null)
        {
            _turns?.Dispose();
            _turns = new TurnService(
                _signals,
                _worldState,
                _calendar,
                participants ?? new List<ITurnParticipant>(),
                new List<ITurnBlocker>(),
                localOwnerResolver);
            _turns.Initialize();
        }

        private void Start(params SpawnPositionAssignment[] assignments)
        {
            _signals.Fire(new WorldSpawnPositionsSignal { Assignments = assignments, Source = WorldSpawnPositionsSource.DirectGameplayTest });
            _signals.Fire<WorldBuiltSignal>();
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }

        private sealed class FixedLocalOwnerResolver : ITurnLocalOwnerResolver
        {
            private readonly string _ownerId;

            public FixedLocalOwnerResolver(string ownerId) => _ownerId = ownerId;

            public string ResolveLocalOwnerId(IReadOnlyList<TurnFaction> factions) => _ownerId;
        }

        private sealed class EliminateOnStartParticipant : ITurnParticipant
        {
            private readonly SignalBus _signals;
            private readonly string _ownerId;

            public EliminateOnStartParticipant(SignalBus signals, string ownerId)
            {
                _signals = signals;
                _ownerId = ownerId;
            }

            public int TurnOrder => 0;

            public void OnTurnStarted(TurnContext context)
            {
                if (context.Faction.OwnerId == _ownerId)
                    _signals.Fire(new FactionEliminatedSignal { FactionId = _ownerId });
            }

            public void OnTurnEnding(TurnContext context) { }
            public void OnRoundCompleted(int completedRound) { }
        }
    }
}
