using System;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Startup
{
    public sealed class HumanVsBotTests
    {
        private object _controller;

        [SetUp]
        public void SetUp()
        {
            GameLaunchContext.ConfigureMenuNewGame(0, "Match", 1, 0, 0, 0, 2, true,
                isLocalPlayerHost: true, localPlayerId: "human");
            GameLaunchContext.ConfigureBotOpponent("bot");
        }

        [TearDown]
        public void TearDown()
        {
            (_controller as IDisposable)?.Dispose();
            GameLaunchContext.Reset();
        }

        [Test]
        public void BotMatchHasDedicatedModeAndDoesNotLoadASavedMap()
        {
            Assert.That(GameLaunchContext.Mode, Is.EqualTo(GameLaunchMode.MenuBotGame));
            Assert.That(GameLaunchContext.HasBotOpponent, Is.True);
            Assert.That(GameLaunchContext.IsAutoLoadEnabled(), Is.False);
            Assert.That(GameLaunchContext.Seed, Is.EqualTo(1));
            Assert.That(GameLaunchContext.MaxPlayers, Is.EqualTo(2));
        }

        [Test]
        public void BotMatchStartsWorldGeneration()
        {
            var type = Assembly.Load("Kruty1918.Moyva.Generator")
                .GetType("Kruty1918.Moyva.Generator.GeneratorWorldStartupBuilder", true);
            var method = type.GetMethod("ShouldBuildWorldOnStartup", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method.Invoke(null, new object[] { true, false }), Is.True);
            Assert.That(method.Invoke(null, new object[] { true, true }), Is.False);
        }

        [Test]
        public void OfflineAssignmentsIncludeHumanAndBotWithoutNetworkParticipants()
        {
            var type = RuntimeType("StartingPositionAssignmentFactory");
            var factory = Activator.CreateInstance(type, true);
            var assignments = (SpawnPositionAssignment[])type.GetMethod("BuildSpawnAssignments").Invoke(factory,
                new object[] { new[] { Vector2Int.zero, new Vector2Int(20, 20) }, null, "human", true, 2 });
            Assert.That(assignments.Length, Is.EqualTo(2));
            Assert.That(assignments[0].ParticipantId, Is.EqualTo("human"));
            Assert.That(assignments[1].ParticipantId, Is.EqualTo("bot"));
            Assert.That(assignments[1].SlotIndex, Is.EqualTo(1));
        }

        [Test]
        public void HumanBotHumanCycleUsesEndTurnAndReleasesInput()
        {
            var turns = new Turns();
            var input = CreateController(turns);
            Advance();
            Assert.That(turns.BotRequests, Is.Zero);
            turns.TryEndTurn("human", out _);
            Assert.That(input.Blocked, Is.True);
            AdvanceUntil(() => turns.ActiveOwnerId == "human");
            Assert.That(turns.BotRequests, Is.EqualTo(1));
            Assert.That(turns.ActiveOwnerId, Is.EqualTo("human"));
            Assert.That(input.Blocked, Is.False);
            Advance();
            Assert.That(turns.BotRequests, Is.EqualTo(1));
        }

        [Test]
        public void BlockedBotEndTurnRetriesThroughAuthorityAndDisposeReleasesInput()
        {
            var turns = new Turns { RejectBot = true };
            var input = CreateController(turns);
            turns.TryEndTurn("human", out _);
            AdvanceUntil(() => turns.BotRequests > 0);
            Assert.That(turns.ActiveOwnerId, Is.EqualTo("bot"));
            Assert.That(input.Blocked, Is.True);
            turns.RejectBot = false;
            AdvanceUntil(() => turns.ActiveOwnerId == "human");
            Assert.That(turns.ActiveOwnerId, Is.EqualTo("human"));
            turns.TryEndTurn("human", out _);
            ((IDisposable)_controller).Dispose();
            Assert.That(input.Blocked, Is.False);
        }

        [Test]
        public void MultiplayerAndSandboxLaunchesClearBotConfiguration()
        {
            GameLaunchContext.ConfigureMenuMultiplayerGame("Network", 1, 0, 0, 0, 2, false);
            Assert.That(GameLaunchContext.HasBotOpponent, Is.False);
            SetUp();
            GameLaunchContext.ConfigureMenuNewGame(0, "Sandbox", 1, 0, 0, 0, 1, true);
            Assert.That(GameLaunchContext.HasBotOpponent, Is.False);
        }

        private Input CreateController(Turns turns)
        {
            var input = new Input();
            var type = RuntimeType("BotController");
            var gateway = new MoyvaBotTurnAdapter(turns);
            var registry = new BotCapabilityRegistry();
            registry.Register(new EndTurnBotCapability(gateway));
            var config = new BotRuntimeConfig { visibleDelay = 0f, maxInvalidDecisions = 64 };
            var orchestrator = new BotDecisionOrchestrator(gateway, registry, new EmptyBotPerceptionSource(),
                new HeuristicBotPolicyDriver(), config, new BotTelemetryHub());
            _controller = Activator.CreateInstance(type, new object[] { turns, input, null, orchestrator, config });
            type.GetMethod("Initialize").Invoke(_controller, null);
            return input;
        }

        private void Advance() => _controller.GetType().GetMethod("Advance",
            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_controller, new object[] { 0.5f });

        private void AdvanceUntil(Func<bool> done, int maxTicks = 12)
        {
            for (int i = 0; i < maxTicks && !done(); i++) Advance();
        }

        private static Type RuntimeType(string name) => typeof(StartingPositionInitializerSettings)
            .Assembly.GetType("Kruty1918.Moyva.Bootstrap.Runtime." + name, true);

        private sealed class Input : IGameplayInputPolicy, IDisposable
        {
            public bool Blocked;
            public IDisposable AcquireBlock(GameplayInputKind mask, object owner) { Blocked = true; return this; }
            public void Dispose() => Blocked = false;
            public bool CanProcess(GameplayInputKind kind, Vector2 position, int pointerId = -1) => !Blocked;
            public bool IsPointerOverUi(Vector2 position, int pointerId = -1, bool interactiveOnly = true) => false;
            public bool TryBeginPointerCapture(GameplayInputKind kind, Vector2 position, int pointerId = -1) => !Blocked;
            public void EndPointerCapture(GameplayInputKind kind, int pointerId = -1) { }
        }

        private sealed class Turns : ITurnService, ITurnEndQuery
        {
            public event Action StateChanged;
            public TurnPhase Phase => TurnPhase.AwaitingInput;
            public int Round => 1;
            public long GlobalTurn { get; private set; } = 1;
            public int ActionsThisTurn => 0;
            public string ActiveOwnerId { get; private set; } = "human";
            public string LocalOwnerId => "human";
            public IReadOnlyList<TurnFaction> Factions => Array.Empty<TurnFaction>();
            public int BotRequests;
            public bool RejectBot;
            public bool IsOwnerActive(string ownerId) => ownerId == ActiveOwnerId;
            public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return IsOwnerActive(ownerId); }
            public bool CanEndTurn(string ownerId, out string reason) { reason = null; return IsOwnerActive(ownerId); }
            public bool TryRecordAction(string ownerId, string actionId) => IsOwnerActive(ownerId);
            public bool TryEndTurn(string ownerId, out string reason)
            {
                reason = null;
                if (ownerId == "bot") BotRequests++;
                if (!IsOwnerActive(ownerId) || ownerId == "bot" && RejectBot) return false;
                ActiveOwnerId = ownerId == "human" ? "bot" : "human";
                GlobalTurn++;
                StateChanged?.Invoke();
                return true;
            }
        }
    }
}
