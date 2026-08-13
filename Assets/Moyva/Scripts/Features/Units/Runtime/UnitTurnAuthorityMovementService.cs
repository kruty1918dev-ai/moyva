using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Authoritative command boundary for asynchronous unit movement.
    ///
    /// The underlying movement service remains responsible for pathfinding,
    /// animation, stamina and ITurnBlocker state. This decorator owns only the
    /// turn/owner lease: a command may start only for a registered unit owned by
    /// the active faction in AwaitingInput, and its linked cancellation token is
    /// cancelled as soon as that authoritative turn identity changes.
    /// </summary>
    internal sealed class UnitTurnAuthorityMovementService : IUnitMovementService
    {
        internal readonly struct UnitTurnCommandLease
        {
            public UnitTurnCommandLease(
                string unitId,
                string ownerId,
                int round,
                long globalTurn)
            {
                UnitId = unitId ?? string.Empty;
                OwnerId = ownerId ?? string.Empty;
                Round = round;
                GlobalTurn = globalTurn;
            }

            public string UnitId { get; }
            public string OwnerId { get; }
            public int Round { get; }
            public long GlobalTurn { get; }
        }

        private readonly IUnitMovementService _decorated;
        private readonly ITurnService _turns;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitService _units;

        public UnitTurnAuthorityMovementService(
            IUnitMovementService decorated,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IUnitService units = null)
        {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _turns = turns;
            _ownership = ownership;
            _units = units;
        }

        public async Task MoveUnitAsync(
            string unitId,
            Vector2Int targetPosition,
            CancellationToken token = default)
        {
            if (!TryAcquireLease(unitId, out UnitTurnCommandLease lease, out string reason))
            {
                Debug.LogWarning(
                    $"[UnitTurnAuthority] Move rejected for '{unitId ?? "<null>"}': {reason}");
                return;
            }

            using var authorityCancellation = new CancellationTokenSource();
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                token,
                authorityCancellation.Token);

            void OnTurnStateChanged()
            {
                if (!IsLeaseValid(lease, out _)
                    && !authorityCancellation.IsCancellationRequested)
                {
                    authorityCancellation.Cancel();
                }
            }

            _turns.StateChanged += OnTurnStateChanged;
            try
            {
                // Closes the subscribe/check race: if authority changed immediately
                // before the handler became visible, do not enter the decorated move.
                if (!IsLeaseValid(lease, out reason))
                {
                    Debug.LogWarning(
                        $"[UnitTurnAuthority] Move cancelled before start for '{unitId}': {reason}");
                    return;
                }

                await _decorated.MoveUnitAsync(
                    unitId,
                    targetPosition,
                    linkedCancellation.Token);
            }
            finally
            {
                _turns.StateChanged -= OnTurnStateChanged;
            }
        }

        internal bool TryAcquireLease(
            string unitId,
            out UnitTurnCommandLease lease,
            out string reason)
        {
            lease = default;

            if (string.IsNullOrWhiteSpace(unitId))
            {
                reason = "Unit id is empty.";
                return false;
            }

            string normalizedUnitId = unitId.Trim();

            if (_units == null)
            {
                reason = "Unit registry authority is unavailable.";
                return false;
            }

            if (!_units.TryGetUnitPosition(normalizedUnitId, out _))
            {
                reason = "Unit is not registered.";
                return false;
            }

            if (_ownership == null)
            {
                reason = "Unit ownership authority is unavailable.";
                return false;
            }

            string ownerId = _ownership.GetUnitOwnerId(normalizedUnitId)?.Trim();
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                reason = "Unit has no authoritative owner.";
                return false;
            }

            if (_turns == null)
            {
                reason = "Turn authority is unavailable.";
                return false;
            }

            if (!_turns.CanOwnerAct(ownerId, out reason))
                return false;

            lease = new UnitTurnCommandLease(
                normalizedUnitId,
                ownerId,
                _turns.Round,
                _turns.GlobalTurn);

            return IsLeaseValid(lease, out reason);
        }

        internal bool IsLeaseValid(
            UnitTurnCommandLease lease,
            out string reason)
        {
            if (_turns == null)
            {
                reason = "Turn authority is unavailable.";
                return false;
            }

            if (_ownership == null)
            {
                reason = "Unit ownership authority is unavailable.";
                return false;
            }

            if (_units == null
                || !_units.TryGetUnitPosition(lease.UnitId, out _))
            {
                reason = "Unit is no longer registered.";
                return false;
            }

            string currentOwner = _ownership.GetUnitOwnerId(lease.UnitId)?.Trim();
            if (!string.Equals(
                    currentOwner,
                    lease.OwnerId,
                    StringComparison.Ordinal))
            {
                reason = "Unit ownership changed while the command was active.";
                return false;
            }

            if (_turns.Round != lease.Round
                || _turns.GlobalTurn != lease.GlobalTurn)
            {
                reason = "The authoritative turn epoch changed.";
                return false;
            }

            if (_turns.Phase != TurnPhase.AwaitingInput)
            {
                reason = $"Turn phase is {_turns.Phase}, expected AwaitingInput.";
                return false;
            }

            if (!string.Equals(
                    _turns.ActiveOwnerId?.Trim(),
                    lease.OwnerId,
                    StringComparison.Ordinal))
            {
                reason = "The active faction changed.";
                return false;
            }

            return _turns.CanOwnerAct(lease.OwnerId, out reason);
        }
    }
}
