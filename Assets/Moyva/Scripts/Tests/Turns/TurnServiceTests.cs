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
        public void Zenject_LocalOwnerResolverBackReference_DoesNotCreateTurnServiceCycle()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();
            container.DeclareSignal<WorldBuiltSignal>().OptionalSubscriber();
            container.DeclareSignal<FactionEliminatedSignal>().OptionalSubscriber();

            var calendar = new GameCalendarService(CalendarConfig.Default());
            var worldState = new WorldGenerationSignalState();
            container.Bind<Kruty1918.Moyva.Calendar.Core.ICalendarService>().FromInstance(calendar);
            container.Bind<IWorldGenerationSignalState>().FromInstance(worldState);
            container.Bind<ITurnLocalOwnerResolver>()
                .To<TurnBackReferenceLocalOwnerResolver>()
                .AsSingle();
            container.BindInterfacesAndSelfTo<TurnService>().AsSingle();

            TurnService turns = null;
            Assert.DoesNotThrow(() => turns = container.Resolve<TurnService>());
            Assert.IsNotNull(turns);

            turns.Initialize();
            try
            {
                SignalBus signals = container.Resolve<SignalBus>();
                Assert.DoesNotThrow(() => signals.Fire(new WorldSpawnPositionsSignal
                {
                    Assignments = new[]
                    {
                        new SpawnPositionAssignment
                        {
                            SlotIndex = 0,
                            ParticipantId = "player_0",
                            IsBot = false,
                        },
                    },
                    Source = WorldSpawnPositionsSource.DirectGameplayTest,
                }));

                Assert.AreEqual("player_0", turns.LocalOwnerId);
            }
            finally
            {
                turns.Dispose();
            }
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

        [Test]
        public void RestoreBeforeRegistry_ResumesSavedTurnWithoutTurnStartSideEffects()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });

            _turns.Restore(4, 17, "bot_0", 3);
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.AreEqual(4, _turns.Round);
            Assert.AreEqual(17, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(3, _turns.ActionsThisTurn);
            Assert.AreEqual(0, participant.StartCount, "A restored in-progress turn must not replay OnTurnStarted.");
        }

        [Test]
        public void RestoreAfterRegistryBeforeWorldBuilt_WaitsThenResumesSavedOwner()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            var assignments = new[]
            {
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true },
            };

            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = assignments,
                Source = WorldSpawnPositionsSource.SavedGame,
            });
            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);

            _turns.Restore(8, 31, "bot_0", 5);
            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);
            _signals.Fire<WorldBuiltSignal>();

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(8, _turns.Round);
            Assert.AreEqual(31, _turns.GlobalTurn);
            Assert.AreEqual(5, _turns.ActionsThisTurn);
            Assert.AreEqual(0, participant.StartCount);
        }

        [Test]
        public void RestoreWhileTurnIsRunning_ReplacesTurnStateWithoutSecondStartCallback()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });
            Assert.AreEqual(1, participant.StartCount);

            _turns.Restore(6, 22, "bot_0", 4);

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(6, _turns.Round);
            Assert.AreEqual(22, _turns.GlobalTurn);
            Assert.AreEqual(4, _turns.ActionsThisTurn);
            Assert.AreEqual(1, participant.StartCount, "Restore must resume, not start the saved turn again.");
        }

        [Test]
        public void RestoreWithUnknownOwner_FailsClosedInsteadOfSelectingDifferentFaction()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            _turns.Restore(3, 9, "missing_owner", 2);

            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = new[]
                {
                    new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                    new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true },
                },
                Source = WorldSpawnPositionsSource.SavedGame,
            });
            _signals.Fire<WorldBuiltSignal>();

            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);
            Assert.AreEqual(0, participant.StartCount);
            Assert.IsFalse(_turns.CanOwnerAct("player_0", out _));
            Assert.IsFalse(_turns.CanOwnerAct("bot_0", out _));
        }

        [Test]
        public void RestoredActionsRemainUntilEndTurn_ThenFreshTurnResetsThem()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            long calendarBefore = _calendar.TotalHoursSinceEpoch;

            _turns.Restore(2, 10, "player_0", 7);
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.AreEqual(7, _turns.ActionsThisTurn);
            Assert.AreEqual(calendarBefore, _calendar.TotalHoursSinceEpoch, "Restore itself must not advance calendar time.");
            Assert.AreEqual(0, participant.StartCount);

            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(0, _turns.ActionsThisTurn);
            Assert.AreEqual(1, participant.StartCount, "The next genuinely new turn must still execute OnTurnStarted once.");
        }


        [Test]
        public void RestoreWithInvalidCounters_FailsClosedWithoutNormalizingSnapshot()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });

            _turns.Restore(0, 0, "player_0", -1);
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = new[]
                {
                    new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                },
                Source = WorldSpawnPositionsSource.SavedGame,
            });
            _signals.Fire<WorldBuiltSignal>();

            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);
            Assert.AreEqual(1, _turns.Round, "Invalid persisted round must not be silently clamped and applied.");
            Assert.AreEqual(1, _turns.GlobalTurn, "Invalid persisted global turn must not be silently clamped and applied.");
            Assert.AreEqual(0, _turns.ActionsThisTurn);
            Assert.AreEqual(0, participant.StartCount);
            Assert.IsFalse(_turns.CanOwnerAct("player_0", out _));
        }

        [Test]
        public void RestoreWithEliminatedOwner_FailsClosed()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });

            _turns.Restore(3, 12, "bot_0", 2);
            _signals.Fire(new FactionEliminatedSignal { FactionId = "bot_0" });
            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = new[]
                {
                    new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                    new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true },
                },
                Source = WorldSpawnPositionsSource.SavedGame,
            });
            _signals.Fire<WorldBuiltSignal>();

            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);
            Assert.AreEqual(0, participant.StartCount);
            Assert.IsFalse(_turns.CanOwnerAct("player_0", out _));
            Assert.IsFalse(_turns.CanOwnerAct("bot_0", out _));
        }

        [Test]
        public void LaterValidRestore_ReplacesPreviouslyBlockedSnapshot()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });
            Assert.AreEqual(1, participant.StartCount);

            _turns.Restore(4, 20, "missing_owner", 6);
            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);

            _turns.Restore(5, 21, "bot_0", 7);

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual(5, _turns.Round);
            Assert.AreEqual(21, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(7, _turns.ActionsThisTurn);
            Assert.AreEqual(1, participant.StartCount, "Replacing a blocked restore must not replay turn start.");
        }

        [Test]
        public void Restore_WorldBuiltBeforeSavedSpawns_WaitsForRegistryThenResumes()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });

            _turns.Restore(9, 44, "bot_0", 8);
            _signals.Fire<WorldBuiltSignal>();
            Assert.AreEqual(TurnPhase.Initializing, _turns.Phase);
            Assert.AreEqual(0, _turns.Factions.Count);

            _signals.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = new[]
                {
                    new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                    new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true },
                },
                Source = WorldSpawnPositionsSource.SavedGame,
            });

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual(9, _turns.Round);
            Assert.AreEqual(44, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(8, _turns.ActionsThisTurn);
            Assert.AreEqual(0, participant.StartCount);
        }

        [Test]
        public void RestoreWhileTurnIsRunning_DoesNotInvokeTurnEnding()
        {
            var participant = new CountingParticipant();
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.AreEqual(1, participant.StartCount);
            Assert.AreEqual(0, participant.EndCount);

            _turns.Restore(7, 30, "bot_0", 2);

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(1, participant.StartCount);
            Assert.AreEqual(0, participant.EndCount, "Loading replaces live state; it must not execute gameplay turn-ending side effects.");
        }

        [Test]
        public void TryEndTurn_ReentrantFromBlocker_IsRejectedAndOuterEndsExactlyOnce()
        {
            bool nestedResult = true;
            string nestedReason = null;
            int calls = 0;
            var blockers = new List<ITurnBlocker>
            {
                new DelegateBlocker(() =>
                {
                    calls++;
                    nestedResult = _turns.TryEndTurn("player_0", out nestedReason);
                    return false;
                }),
            };
            CreateTurnService(blockers: blockers);
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.IsTrue(_turns.TryEndTurn("player_0", out string outerReason));
            Assert.IsNull(outerReason);
            Assert.IsFalse(nestedResult);
            Assert.AreEqual("Turn end is already being evaluated.", nestedReason);
            Assert.AreEqual(1, calls);
            Assert.AreEqual(2, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
        }

        [Test]
        public void TryEndTurn_BlankBlockReason_UsesStableFallback()
        {
            CreateTurnService(blockers: new List<ITurnBlocker>
            {
                new DelegateBlocker(() => true, () => "   "),
            });
            Start(new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false });

            Assert.IsFalse(_turns.TryEndTurn("player_0", out string reason));
            Assert.AreEqual("Turn is blocked.", reason);
            Assert.AreEqual(1, _turns.GlobalTurn);
        }

        [Test]
        public void TryEndTurn_BlockersRunInInjectedOrderUntilFirstBlock()
        {
            var order = new List<int>();
            CreateTurnService(blockers: new List<ITurnBlocker>
            {
                new DelegateBlocker(() => { order.Add(1); return false; }),
                new DelegateBlocker(() => { order.Add(2); return true; }, () => "second blocked"),
                new DelegateBlocker(() => { order.Add(3); return false; }),
            });
            Start(new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false });

            Assert.IsFalse(_turns.TryEndTurn("player_0", out string reason));
            CollectionAssert.AreEqual(new[] { 1, 2 }, order);
            Assert.AreEqual("second blocked", reason);
        }

        [Test]
        public void TryEndTurn_BlockerRecordsAction_FailsClosedWithoutAdvancingTurn()
        {
            CreateTurnService(blockers: new List<ITurnBlocker>
            {
                new DelegateBlocker(() =>
                {
                    Assert.IsTrue(_turns.TryRecordAction("player_0", "blocker-side-effect"));
                    return false;
                }),
            });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.IsFalse(_turns.TryEndTurn("player_0", out string reason));
            Assert.AreEqual("Turn state changed while evaluating blockers.", reason);
            Assert.AreEqual(1, _turns.ActionsThisTurn);
            Assert.AreEqual(1, _turns.GlobalTurn);
            Assert.AreEqual("player_0", _turns.ActiveOwnerId);
        }

        [Test]
        public void TryEndTurn_BlockerEliminatesActiveFaction_FailsClosedWithoutSecondAdvance()
        {
            CreateTurnService(blockers: new List<ITurnBlocker>
            {
                new DelegateBlocker(() =>
                {
                    _signals.Fire(new FactionEliminatedSignal { FactionId = "player_0" });
                    return false;
                }),
            });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.IsFalse(_turns.TryEndTurn("player_0", out string reason));
            Assert.AreEqual("Turn state changed while evaluating blockers.", reason);
            Assert.AreEqual(2, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
        }

        [Test]
        public void TurnStartedCallback_CannotEndTurnOrRecordAction()
        {
            bool endResult = true;
            bool actionResult = true;
            string endReason = null;
            var participant = new LifecycleProbeParticipant(onStart: _ =>
            {
                endResult = _turns.TryEndTurn("player_0", out endReason);
                actionResult = _turns.TryRecordAction("player_0", "start-callback");
            });
            CreateTurnService(participants: new List<ITurnParticipant> { participant });

            Start(new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false });

            Assert.IsFalse(endResult);
            StringAssert.Contains("Starting", endReason);
            Assert.IsFalse(actionResult);
            Assert.AreEqual(0, _turns.ActionsThisTurn);
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }

        [Test]
        public void TurnEndingCallback_CannotEndTurnOrRecordAction()
        {
            bool nestedEnd = true;
            bool nestedAction = true;
            string nestedReason = null;
            var participant = new LifecycleProbeParticipant(onEnd: _ =>
            {
                nestedEnd = _turns.TryEndTurn("player_0", out nestedReason);
                nestedAction = _turns.TryRecordAction("player_0", "end-callback");
            });
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.IsTrue(_turns.TryEndTurn("player_0", out _));
            Assert.IsFalse(nestedEnd);
            StringAssert.Contains("Ending", nestedReason);
            Assert.IsFalse(nestedAction);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(0, _turns.ActionsThisTurn);
        }

        [Test]
        public void RestoreDuringTurnEndingCallback_StopsStaleLifecycleBeforeSecondAdvance()
        {
            bool restored = false;
            var participant = new LifecycleProbeParticipant(onEnd: _ =>
            {
                if (restored)
                    return;
                restored = true;
                _turns.Restore(5, 20, "bot_0", 3);
            });
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.IsFalse(_turns.TryEndTurn("player_0", out string reason));
            Assert.AreEqual("Turn state changed during lifecycle transition.", reason);
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual(5, _turns.Round);
            Assert.AreEqual(20, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(3, _turns.ActionsThisTurn);
        }

        [Test]
        public void RestoreDuringTurnStartedCallback_StopsStaleStartLifecycle()
        {
            bool restored = false;
            var participant = new LifecycleProbeParticipant(onStart: context =>
            {
                if (restored || context.Faction.OwnerId != "player_0")
                    return;
                restored = true;
                _turns.Restore(7, 30, "bot_0", 2);
            });
            CreateTurnService(participants: new List<ITurnParticipant> { participant });

            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual(7, _turns.Round);
            Assert.AreEqual(30, _turns.GlobalTurn);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.ActionsThisTurn);
        }

        [Test]
        public void RestoreDuringRoundCompletedCallback_DoesNotAdvanceCalendarAfterStateReplacement()
        {
            bool restored = false;
            var participant = new LifecycleProbeParticipant(onRound: _ =>
            {
                if (restored)
                    return;
                restored = true;
                _turns.Restore(8, 40, "player_0", 4);
            });
            CreateTurnService(participants: new List<ITurnParticipant> { participant });
            Start(new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false });
            long calendarBefore = _calendar.TotalHoursSinceEpoch;

            Assert.IsFalse(_turns.TryEndTurn("player_0", out string reason));
            Assert.AreEqual("Turn state changed during lifecycle transition.", reason);
            Assert.AreEqual(calendarBefore, _calendar.TotalHoursSinceEpoch);
            Assert.AreEqual(8, _turns.Round);
            Assert.AreEqual(40, _turns.GlobalTurn);
            Assert.AreEqual(4, _turns.ActionsThisTurn);
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }

        [Test]
        public void ActiveEliminationDuringTurnStart_RemainsSupportedByLifecycleGuard()
        {
            CreateTurnService(participants: new List<ITurnParticipant>
            {
                new EliminateOnStartParticipant(_signals, "player_0"),
            });

            Start(
                new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false },
                new SpawnPositionAssignment { SlotIndex = 1, ParticipantId = "bot_0", IsBot = true });

            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
            Assert.AreEqual("bot_0", _turns.ActiveOwnerId);
            Assert.AreEqual(2, _turns.GlobalTurn);
        }

        private sealed class CountingParticipant : ITurnParticipant
        {
            public int StartCount { get; private set; }
            public int EndCount { get; private set; }
            public int TurnOrder => 0;
            public void OnTurnStarted(TurnContext context) => StartCount++;
            public void OnTurnEnding(TurnContext context) => EndCount++;
            public void OnRoundCompleted(int completedRound) { }
        }

        private void CreateTurnService(
            ITurnLocalOwnerResolver localOwnerResolver = null,
            List<ITurnParticipant> participants = null,
            List<ITurnBlocker> blockers = null)
        {
            _turns?.Dispose();
            _turns = new TurnService(
                _signals,
                _worldState,
                _calendar,
                participants ?? new List<ITurnParticipant>(),
                blockers ?? new List<ITurnBlocker>(),
                localOwnerResolver);
            _turns.Initialize();
        }

        private void Start(params SpawnPositionAssignment[] assignments)
        {
            _signals.Fire(new WorldSpawnPositionsSignal { Assignments = assignments, Source = WorldSpawnPositionsSource.DirectGameplayTest });
            _signals.Fire<WorldBuiltSignal>();
            Assert.AreEqual(TurnPhase.AwaitingInput, _turns.Phase);
        }

        private sealed class DelegateBlocker : ITurnBlocker
        {
            private readonly Func<bool> _callback;
            private readonly Func<string> _reason;

            public DelegateBlocker(Func<bool> callback, Func<string> reason = null)
            {
                _callback = callback;
                _reason = reason;
            }

            public bool IsTurnBlocked(out string reason)
            {
                bool blocked = _callback?.Invoke() ?? false;
                reason = _reason?.Invoke();
                return blocked;
            }
        }

        private sealed class LifecycleProbeParticipant : ITurnParticipant
        {
            private readonly Action<TurnContext> _onStart;
            private readonly Action<TurnContext> _onEnd;
            private readonly Action<int> _onRound;

            public LifecycleProbeParticipant(
                Action<TurnContext> onStart = null,
                Action<TurnContext> onEnd = null,
                Action<int> onRound = null)
            {
                _onStart = onStart;
                _onEnd = onEnd;
                _onRound = onRound;
            }

            public int TurnOrder => 0;
            public void OnTurnStarted(TurnContext context) => _onStart?.Invoke(context);
            public void OnTurnEnding(TurnContext context) => _onEnd?.Invoke(context);
            public void OnRoundCompleted(int completedRound) => _onRound?.Invoke(completedRound);
        }

        private sealed class FixedLocalOwnerResolver : ITurnLocalOwnerResolver
        {
            private readonly string _ownerId;

            public FixedLocalOwnerResolver(string ownerId) => _ownerId = ownerId;

            public string ResolveLocalOwnerId(IReadOnlyList<TurnFaction> factions) => _ownerId;
        }

        private sealed class TurnBackReferenceLocalOwnerResolver : ITurnLocalOwnerResolver
        {
            private readonly ITurnService _turns;

            public TurnBackReferenceLocalOwnerResolver(ITurnService turns)
            {
                _turns = turns;
            }

            public string ResolveLocalOwnerId(IReadOnlyList<TurnFaction> factions)
            {
                Assert.IsNotNull(_turns);
                if (factions == null || factions.Count == 0)
                    return string.Empty;

                return factions[0].OwnerId;
            }
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
