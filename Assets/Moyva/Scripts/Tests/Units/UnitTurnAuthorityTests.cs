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
    [TestFixture]
    public sealed class UnitTurnAuthorityTests
    {
        private sealed class FakeMovementService : IUnitMovementService
        {
            public int Calls { get; private set; }
            public CancellationToken LastToken { get; private set; }
            public Func<CancellationToken, Task> Handler { get; set; }

            public Task MoveUnitAsync(
                string unitId,
                Vector2Int targetPosition,
                CancellationToken token = default)
            {
                Calls++;
                LastToken = token;
                return Handler?.Invoke(token) ?? Task.CompletedTask;
            }
        }

        private sealed class FakeUnitService : IUnitService
        {
            private readonly Dictionary<string, Vector2Int> _positions = new();

            public void Add(string unitId, Vector2Int position)
                => _positions[unitId] = position;

            public void Remove(string unitId)
                => _positions.Remove(unitId);

            public float GetStamina(string unitId) => 0f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
                => _positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys;
            public string GetUnitTypeId(string unitId) => null;
        }

        private sealed class FakeOwnershipQuery : IUnitOwnershipQuery
        {
            private readonly Dictionary<string, string> _owners = new();

            public void Set(string unitId, string ownerId)
                => _owners[unitId] = ownerId;

            public string GetUnitOwnerId(string unitId)
                => _owners.TryGetValue(unitId, out string ownerId)
                    ? ownerId
                    : null;
        }

        private sealed class FakeTurnService : ITurnService
        {
            public event Action StateChanged;

            public TurnPhase Phase { get; private set; } = TurnPhase.AwaitingInput;
            public int Round { get; private set; } = 3;
            public long GlobalTurn { get; private set; } = 17;
            public int ActionsThisTurn { get; private set; }
            public string ActiveOwnerId { get; private set; } = "alpha";
            public string LocalOwnerId => "alpha";
            public bool IsActiveFactionBot => false;
            public IReadOnlyList<TurnFaction> Factions { get; } = Array.Empty<TurnFaction>();

            public bool IsOwnerActive(string ownerId)
                => string.Equals(
                    ActiveOwnerId,
                    ownerId?.Trim(),
                    StringComparison.Ordinal);

            public bool CanOwnerAct(string ownerId, out string reason)
            {
                if (Phase != TurnPhase.AwaitingInput)
                {
                    reason = "wrong phase";
                    return false;
                }

                if (!IsOwnerActive(ownerId))
                {
                    reason = "wrong owner";
                    return false;
                }

                reason = null;
                return true;
            }

            public bool TryRecordAction(string ownerId, string actionId)
            {
                if (!CanOwnerAct(ownerId, out _))
                    return false;

                ActionsThisTurn++;
                StateChanged?.Invoke();
                return true;
            }

            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                reason = "not used";
                return false;
            }

            public void SetPhase(TurnPhase phase)
            {
                Phase = phase;
                StateChanged?.Invoke();
            }

            public void SetEpoch(int round, long globalTurn)
            {
                Round = round;
                GlobalTurn = globalTurn;
                StateChanged?.Invoke();
            }

            public void SetOwner(string ownerId)
            {
                ActiveOwnerId = ownerId;
                StateChanged?.Invoke();
            }

            public void IncrementActions()
            {
                ActionsThisTurn++;
                StateChanged?.Invoke();
            }
        }

        private static UnitTurnAuthorityMovementService CreateSubject(
            FakeMovementService decorated,
            FakeTurnService turns,
            FakeOwnershipQuery ownership,
            FakeUnitService units)
            => new UnitTurnAuthorityMovementService(
                decorated,
                turns,
                ownership,
                units);

        private static void Register(
            FakeUnitService units,
            FakeOwnershipQuery ownership,
            string unitId = "u1",
            string ownerId = "alpha")
        {
            units.Add(unitId, Vector2Int.zero);
            ownership.Set(unitId, ownerId);
        }

        [Test]
        public async Task ActiveOwnedRegisteredUnit_InvokesDecoratedMovement()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);

            await CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("u1", Vector2Int.one);

            Assert.AreEqual(1, decorated.Calls);
        }

        [Test]
        public async Task ForeignOwnedUnit_IsRejected()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership, ownerId: "beta");

            await CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("u1", Vector2Int.one);

            Assert.AreEqual(0, decorated.Calls);
        }

        [Test]
        public async Task WrongPhase_IsRejected()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            turns.SetPhase(TurnPhase.Ending);
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);

            await CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("u1", Vector2Int.one);

            Assert.AreEqual(0, decorated.Calls);
        }

        [Test]
        public async Task UnknownUnit_IsRejectedBeforeOwnershipFallbackCanAuthorize()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            ownership.Set("ghost", "alpha");
            var units = new FakeUnitService();

            await CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("ghost", Vector2Int.one);

            Assert.AreEqual(0, decorated.Calls);
        }

        [Test]
        public async Task MissingTurnAuthority_FailsClosed()
        {
            var decorated = new FakeMovementService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = new UnitTurnAuthorityMovementService(
                decorated,
                null,
                ownership,
                units);

            await subject.MoveUnitAsync("u1", Vector2Int.one);

            Assert.AreEqual(0, decorated.Calls);
        }

        [Test]
        public async Task MissingOwnershipAuthority_FailsClosed()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var units = new FakeUnitService();
            units.Add("u1", Vector2Int.zero);
            var subject = new UnitTurnAuthorityMovementService(
                decorated,
                turns,
                null,
                units);

            await subject.MoveUnitAsync("u1", Vector2Int.one);

            Assert.AreEqual(0, decorated.Calls);
        }

        [Test]
        public void Lease_InvalidatesWhenPhaseLeavesAwaitingInput()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = CreateSubject(decorated, turns, ownership, units);

            Assert.IsTrue(subject.TryAcquireLease("u1", out var lease, out _));
            turns.SetPhase(TurnPhase.Resolving);

            Assert.IsFalse(subject.IsLeaseValid(lease, out _));
        }

        [Test]
        public void Lease_InvalidatesWhenGlobalTurnChanges()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = CreateSubject(decorated, turns, ownership, units);

            Assert.IsTrue(subject.TryAcquireLease("u1", out var lease, out _));
            turns.SetEpoch(turns.Round, turns.GlobalTurn + 1);

            Assert.IsFalse(subject.IsLeaseValid(lease, out _));
        }

        [Test]
        public void Lease_InvalidatesWhenRoundChanges()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = CreateSubject(decorated, turns, ownership, units);

            Assert.IsTrue(subject.TryAcquireLease("u1", out var lease, out _));
            turns.SetEpoch(turns.Round + 1, turns.GlobalTurn);

            Assert.IsFalse(subject.IsLeaseValid(lease, out _));
        }

        [Test]
        public void Lease_InvalidatesWhenActiveOwnerChanges()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = CreateSubject(decorated, turns, ownership, units);

            Assert.IsTrue(subject.TryAcquireLease("u1", out var lease, out _));
            turns.SetOwner("beta");

            Assert.IsFalse(subject.IsLeaseValid(lease, out _));
        }

        [Test]
        public void Lease_InvalidatesWhenUnitOwnershipChanges()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = CreateSubject(decorated, turns, ownership, units);

            Assert.IsTrue(subject.TryAcquireLease("u1", out var lease, out _));
            ownership.Set("u1", "beta");

            Assert.IsFalse(subject.IsLeaseValid(lease, out _));
        }

        [Test]
        public void SameTurnActionCounterChange_DoesNotInvalidateLease()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);
            var subject = CreateSubject(decorated, turns, ownership, units);

            Assert.IsTrue(subject.TryAcquireLease("u1", out var lease, out _));
            turns.IncrementActions();

            Assert.IsTrue(subject.IsLeaseValid(lease, out _));
        }

        [Test]
        public async Task StateChangedToInvalidTurn_CancelsDecoratedToken()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);

            var entered = new TaskCompletionSource<bool>();
            bool cancelled = false;
            decorated.Handler = async token =>
            {
                entered.TrySetResult(true);
                try
                {
                    await Task.Delay(Timeout.Infinite, token);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }
            };

            Task move = CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("u1", Vector2Int.one);
            await entered.Task;

            turns.SetOwner("beta");
            await move;

            Assert.IsTrue(cancelled);
            Assert.IsTrue(decorated.LastToken.IsCancellationRequested);
        }

        [Test]
        public async Task ExternalCancellation_IsLinkedToDecoratedToken()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);

            var entered = new TaskCompletionSource<bool>();
            bool cancelled = false;
            decorated.Handler = async token =>
            {
                entered.TrySetResult(true);
                try
                {
                    await Task.Delay(Timeout.Infinite, token);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }
            };

            using var external = new CancellationTokenSource();
            Task move = CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("u1", Vector2Int.one, external.Token);
            await entered.Task;

            external.Cancel();
            await move;

            Assert.IsTrue(cancelled);
        }

        [Test]
        public async Task RemovedUnitDuringCommand_CancelsOnStateChange()
        {
            var decorated = new FakeMovementService();
            var turns = new FakeTurnService();
            var ownership = new FakeOwnershipQuery();
            var units = new FakeUnitService();
            Register(units, ownership);

            var entered = new TaskCompletionSource<bool>();
            bool cancelled = false;
            decorated.Handler = async token =>
            {
                entered.TrySetResult(true);
                try
                {
                    await Task.Delay(Timeout.Infinite, token);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }
            };

            Task move = CreateSubject(decorated, turns, ownership, units)
                .MoveUnitAsync("u1", Vector2Int.one);
            await entered.Task;

            units.Remove("u1");
            turns.IncrementActions();
            await move;

            Assert.IsTrue(cancelled);
        }
    }
}
