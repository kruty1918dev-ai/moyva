using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// C15 — the authority decorator leases a move command to (unit, owner,
    /// round, globalTurn): start requires a registered, owned unit of the
    /// active faction in AwaitingInput; a turn/owner change during await
    /// cancels the linked token; sandbox realtime leases only on ownership.
    /// </summary>
    public sealed class C15AcceptanceTests
    {
        private C15Fakes.Turns _turns;
        private C15Fakes.Clock _clock;
        private C15Fakes.Ownership _ownership;
        private C15Fakes.Units _units;
        private C15Fakes.Decorated _decorated;
        private UnitTurnAuthorityMovementService _service;

        [SetUp]
        public void SetUp()
        {
            _turns = new C15Fakes.Turns();
            _clock = new C15Fakes.Clock();
            _ownership = new C15Fakes.Ownership();
            _units = new C15Fakes.Units();
            _decorated = new C15Fakes.Decorated();
            _service = new UnitTurnAuthorityMovementService(
                _decorated,
                turns: _turns,
                progressClock: _clock,
                ownership: _ownership,
                units: _units);
        }

        private void SpawnTurnOwned()
        {
            _units.Spawn("u-1", new Vector2Int(1, 1));
            _ownership.Owners["u-1"] = "p1";
            _turns.ActiveOwnerId = "p1";
            _turns.Phase = TurnPhase.AwaitingInput;
            _clock.IsRealtime = false;
        }

        [Test]
        public async Task TurnBased_OwnerChange_DuringAwait_CancelsCommand()
        {
            SpawnTurnOwned();

            Task command = _service.MoveUnitAsync("u-1", new Vector2Int(2, 2));
            Assert.AreEqual(1, _decorated.Calls);
            Assert.IsFalse(command.IsCompleted);

            _ownership.Owners["u-1"] = "p2"; // capture mid-flight
            _turns.RaiseStateChanged();

            Assert.IsTrue(_decorated.LastToken.IsCancellationRequested);
            await AssertCanceled(command);
        }

        [Test]
        public async Task TurnBased_TurnAdvance_CancelsInFlightMove()
        {
            SpawnTurnOwned();

            Task command = _service.MoveUnitAsync("u-1", new Vector2Int(2, 2));
            _turns.GlobalTurn++;
            _turns.ActiveOwnerId = "p2";
            _turns.RaiseStateChanged();

            Assert.IsTrue(_decorated.LastToken.IsCancellationRequested);
            await AssertCanceled(command);
        }

        [Test]
        public void TurnBased_NonActiveOwner_NeverStarts()
        {
            SpawnTurnOwned();
            _turns.ActiveOwnerId = "p2";

            var task = _service.MoveUnitAsync("u-1", new Vector2Int(2, 2));

            Assert.AreEqual(
                0, _decorated.Calls,
                "A move for a non-active faction must be rejected upfront");
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public void UnregisteredUnit_NeverStarts()
        {
            SpawnTurnOwned();
            var task = _service.MoveUnitAsync("ghost", new Vector2Int(2, 2));
            Assert.AreEqual(0, _decorated.Calls);
            Assert.IsTrue(task.IsCompleted);
        }

        [Test]
        public async Task Sandbox_Realtime_LeaseSurvivesTurnlessWorld()
        {
            SpawnTurnOwned();
            _clock.IsRealtime = true;
            _turns = null; // reconstruct without turn authority
            _service = new UnitTurnAuthorityMovementService(
                _decorated,
                turns: null,
                progressClock: _clock,
                ownership: _ownership,
                units: _units);

            _decorated.AutoComplete = true;
            await _service.MoveUnitAsync("u-1", new Vector2Int(2, 2));

            Assert.AreEqual(1, _decorated.Calls);
        }

        [Test]
        public void CallerCancellation_AlwaysPropagates()
        {
            SpawnTurnOwned();
            using var caller = new CancellationTokenSource();
            var task = _service.MoveUnitAsync(
                "u-1", new Vector2Int(2, 2), caller.Token);
            caller.Cancel();

            Assert.IsTrue(_decorated.LastToken.IsCancellationRequested);
        }

        private static async Task AssertCanceled(Task task)
        {
            try
            {
                await task;
                Assert.Fail("Expected OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    internal static class C15Fakes
    {
        internal sealed class Decorated : IUnitMovementService
        {
            public int Calls;
            public bool AutoComplete;
            public CancellationToken LastToken;

            public Task MoveUnitAsync(
                string unitId,
                Vector2Int targetPosition,
                CancellationToken token = default)
            {
                Calls++;
                LastToken = token;
                if (AutoComplete)
                    return Task.CompletedTask;
                // Complete only when the linked token fires.
                return Task.Delay(Timeout.Infinite, token);
            }
        }

        internal sealed class Turns : ITurnService
        {
            public event Action StateChanged;
            public TurnPhase Phase { get; set; } = TurnPhase.AwaitingInput;
            public int Round { get; set; } = 1;
            public long GlobalTurn { get; set; } = 1;
            public int ActionsThisTurn { get; set; }
            public string ActiveOwnerId { get; set; } = "p1";
            public string LocalOwnerId { get; set; } = "p1";
            public IReadOnlyList<TurnFaction> Factions { get; }
                = new List<TurnFaction>
                {
                    new TurnFaction("p1", Vector2Int.zero),
                    new TurnFaction("p2", Vector2Int.one),
                };

            public void RaiseStateChanged() => StateChanged?.Invoke();

            public bool IsOwnerActive(string ownerId)
                => string.Equals(ownerId, ActiveOwnerId, StringComparison.Ordinal);
            public bool CanOwnerAct(string ownerId, out string reason)
            {
                if (Phase != TurnPhase.AwaitingInput || !IsOwnerActive(ownerId))
                {
                    reason = "not your turn";
                    return false;
                }
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

        internal sealed class Clock : IGameplayProgressClock
        {
            public event Action<GameplayProgressTick> Progressed
            {
                add { }
                remove { }
            }
            public GameplayProgressMode Mode => IsRealtime
                ? GameplayProgressMode.SandboxRealtime
                : GameplayProgressMode.TurnBased;
            public bool IsRealtime { get; set; }
            public float SandboxRoundSeconds => 10f;
            public float Speed => 1f;
            public long CurrentSequence => 1;
            public double ElapsedGameplaySeconds => 0;
            public float SecondsUntilNextProgress => 0f;
            public void Configure(GameplayProgressMode mode, float round, float speed) { }
            public void SetSpeed(float speed) { }
        }

        internal sealed class Ownership : IUnitOwnershipQuery
        {
            public readonly Dictionary<string, string> Owners = new();
            public string GetUnitOwnerId(string unitId)
                => Owners.TryGetValue(unitId, out var owner) ? owner : null;
        }

        internal sealed class Units : IUnitService
        {
            private readonly Dictionary<string, Vector2Int> _positions = new();
            public void Spawn(string id, Vector2Int pos) => _positions[id] = pos;
            public float GetStamina(string unitId) => 1f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
                => _positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys;
            public string GetUnitTypeId(string unitId) => "worker";
        }
    }
}
