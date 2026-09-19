using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Canonical unit-group authority. Membership is organizational state on top
    /// of existing units; movement decomposes into canonical per-unit moves so
    /// stamina, traversal and occupancy rules remain enforced per member.
    /// </summary>
    internal sealed class UnitGroupService
        : IUnitGroupService, IUnitGroupStateStore, IInitializable, IDisposable
    {
        private sealed class GroupState
        {
            public string GroupId;
            public string OwnerId;
            public readonly List<string> UnitIds = new List<string>();
        }

        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitMovementService _movement;
        private readonly IUnitMovementQuery _movementQuery;
        private readonly IObjectsMapService _objectsMap;
        private readonly IGridService _grid;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _progressClock;
        private readonly SignalBus _signalBus;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private readonly Dictionary<string, GroupState> _groups =
            new Dictionary<string, GroupState>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _groupByUnit =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private int _nextGroupOrdinal = 1;

        public UnitGroupService(
            SignalBus signalBus,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IUnitMovementService movement = null,
            [InjectOptional] IUnitMovementQuery movementQuery = null,
            [InjectOptional] IObjectsMapService objectsMap = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IGameplayProgressClock progressClock = null)
        {
            _signalBus = signalBus;
            _units = units;
            _ownership = ownership;
            _movement = movement;
            _movementQuery = movementQuery;
            _objectsMap = objectsMap;
            _grid = grid;
            _turns = turns;
            _progressClock = progressClock;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<UnitGarrisonStateChangedSignal>(OnUnitGarrisonStateChanged);
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            _signalBus?.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus?.TryUnsubscribe<UnitGarrisonStateChangedSignal>(OnUnitGarrisonStateChanged);
        }

        // ── Membership ────────────────────────────────────────────────

        public bool TryCreateGroup(
            string ownerId,
            IReadOnlyList<string> unitIds,
            out string groupId,
            out string reason)
        {
            groupId = null;
            ownerId = Normalize(ownerId);
            if (string.IsNullOrEmpty(ownerId))
            { reason = "Group owner is empty."; return false; }

            if (unitIds == null || unitIds.Count == 0)
            { reason = "Select at least one unit."; return false; }

            var members = new List<string>(unitIds.Count);
            for (int index = 0; index < unitIds.Count; index++)
            {
                string unitId = unitIds[index];
                if (!TryValidateMember(ownerId, unitId, out reason))
                    return false;
                if (members.Contains(unitId))
                    continue;
                members.Add(unitId);
            }

            if (members.Count == 0)
            { reason = "Select at least one unit."; return false; }

            groupId = $"grp-{_nextGroupOrdinal++}";
            var state = new GroupState { GroupId = groupId, OwnerId = ownerId };
            state.UnitIds.AddRange(members);
            _groups[groupId] = state;
            for (int index = 0; index < members.Count; index++)
                _groupByUnit[members[index]] = groupId;

            FireGroupChanged(state, UnitGroupChangeKind.Formed);
            reason = null;
            return true;
        }

        public bool TryDisbandGroup(string ownerId, string groupId, out string reason)
        {
            if (!TryResolveOwnedGroup(ownerId, groupId, out GroupState state, out reason))
                return false;

            Disband(state);
            return true;
        }

        public bool TryAddUnit(string ownerId, string groupId, string unitId, out string reason)
        {
            if (!TryResolveOwnedGroup(ownerId, groupId, out GroupState state, out reason))
                return false;
            if (!TryValidateMember(state.OwnerId, unitId, out reason))
                return false;
            if (state.UnitIds.Contains(unitId))
            { reason = "Unit already belongs to this group."; return false; }

            state.UnitIds.Add(unitId);
            _groupByUnit[unitId] = groupId;
            FireGroupChanged(state, UnitGroupChangeKind.MembersChanged);
            return true;
        }

        public bool TryRemoveUnit(string ownerId, string groupId, string unitId, out string reason)
        {
            if (!TryResolveOwnedGroup(ownerId, groupId, out GroupState state, out reason))
                return false;
            if (!state.UnitIds.Remove(unitId))
            { reason = "Unit does not belong to this group."; return false; }

            _groupByUnit.Remove(unitId);
            if (state.UnitIds.Count == 0)
                Disband(state);
            else
                FireGroupChanged(state, UnitGroupChangeKind.MembersChanged);
            return true;
        }

        // ── Movement ──────────────────────────────────────────────────

        public bool TryMoveGroup(
            string ownerId,
            string groupId,
            Vector2Int targetPosition,
            out string reason)
        {
            if (!TryResolveOwnedGroup(ownerId, groupId, out GroupState state, out reason))
                return false;

            if (_movement == null)
            { reason = "Movement service is unavailable."; return false; }

            if (_turns != null
                && _progressClock?.IsRealtime != true
                && !_turns.CanOwnerAct(state.OwnerId, out reason))
            {
                return false;
            }

            if (_grid != null && !_grid.ContainsCell(targetPosition))
            { reason = "Target is outside the map."; return false; }

            // Nearest member claims the target cell first; the rest take the
            // closest reachable free cells around it.
            var ordered = new List<string>(state.UnitIds);
            ordered.Sort((a, b) =>
                DistanceTo(a, targetPosition).CompareTo(DistanceTo(b, targetPosition)));

            var reserved = new HashSet<Vector2Int>();
            var assignments = new List<(string unitId, Vector2Int destination)>();
            for (int index = 0; index < ordered.Count; index++)
            {
                string unitId = ordered[index];
                if (!_units.TryGetUnitPosition(unitId, out Vector2Int current))
                    continue;

                Vector2Int? destination =
                    ResolveMemberDestination(unitId, targetPosition, reserved);
                if (!destination.HasValue || destination.Value == current)
                    continue;

                reserved.Add(destination.Value);
                assignments.Add((unitId, destination.Value));
            }

            if (assignments.Count == 0)
            { reason = "No group member can reach the target area."; return false; }

            for (int index = 0; index < assignments.Count; index++)
            {
                var assignment = assignments[index];
                _ = _movement.MoveUnitAsync(assignment.unitId, assignment.destination, _lifetime.Token);
            }

            reason = null;
            return true;
        }

        // ── Queries ───────────────────────────────────────────────────

        public bool TryGetGroup(string groupId, out UnitGroupSnapshot snapshot)
        {
            snapshot = default;
            if (string.IsNullOrWhiteSpace(groupId)
                || !_groups.TryGetValue(groupId, out GroupState state))
                return false;

            snapshot = new UnitGroupSnapshot(
                state.GroupId,
                state.OwnerId,
                state.UnitIds.ToArray());
            return true;
        }

        public string GetGroupIdOfUnit(string unitId)
            => !string.IsNullOrWhiteSpace(unitId)
               && _groupByUnit.TryGetValue(unitId, out string groupId)
                ? groupId
                : string.Empty;

        public IReadOnlyList<UnitGroupSnapshot> GetGroupsForOwner(string ownerId)
        {
            var result = new List<UnitGroupSnapshot>();
            ownerId = Normalize(ownerId);
            foreach (var pair in _groups)
            {
                if (string.Equals(pair.Value.OwnerId, ownerId, StringComparison.Ordinal))
                    result.Add(new UnitGroupSnapshot(
                        pair.Value.GroupId,
                        pair.Value.OwnerId,
                        pair.Value.UnitIds.ToArray()));
            }
            result.Sort((a, b) => string.CompareOrdinal(a.GroupId, b.GroupId));
            return result;
        }

        // ── Persistence / replication boundary ────────────────────────

        public IReadOnlyList<UnitGroupSnapshot> CaptureState()
        {
            var result = new List<UnitGroupSnapshot>(_groups.Count);
            foreach (var pair in _groups)
            {
                result.Add(new UnitGroupSnapshot(
                    pair.Value.GroupId,
                    pair.Value.OwnerId,
                    pair.Value.UnitIds.ToArray()));
            }
            result.Sort((a, b) => string.CompareOrdinal(a.GroupId, b.GroupId));
            return result;
        }

        /// <summary>
        /// Rebuilds membership from a save snapshot. Units that no longer
        /// exist are dropped silently; empty groups are dropped. Never
        /// re-runs authority checks.
        /// </summary>
        public void RestoreState(IReadOnlyList<UnitGroupSnapshot> groups)
            => Restore(groups, validateMemberExistence: true);

        /// <summary>
        /// Applies the replicated host table verbatim. Member existence is not
        /// checked against the local unit service — unit replication may lag
        /// the group sync, and the next host snapshot corrects divergence.
        /// </summary>
        public void RestoreReplicated(IReadOnlyList<UnitGroupSnapshot> groups)
            => Restore(groups, validateMemberExistence: false);

        private void Restore(
            IReadOnlyList<UnitGroupSnapshot> groups,
            bool validateMemberExistence)
        {
            _groups.Clear();
            _groupByUnit.Clear();

            if (groups != null)
            {
                for (int index = 0; index < groups.Count; index++)
                {
                    UnitGroupSnapshot snapshot = groups[index];
                    if (string.IsNullOrWhiteSpace(snapshot.GroupId)
                        || string.IsNullOrWhiteSpace(snapshot.OwnerId)
                        || snapshot.UnitIds == null)
                        continue;

                    var state = new GroupState
                    {
                        GroupId = snapshot.GroupId,
                        OwnerId = Normalize(snapshot.OwnerId),
                    };

                    for (int memberIndex = 0; memberIndex < snapshot.UnitIds.Count; memberIndex++)
                    {
                        string unitId = snapshot.UnitIds[memberIndex];
                        if (string.IsNullOrWhiteSpace(unitId)
                            || state.UnitIds.Contains(unitId)
                            || _groupByUnit.ContainsKey(unitId))
                            continue;

                        if (validateMemberExistence
                            && _units != null
                            && !_units.TryGetUnitPosition(unitId, out _))
                            continue;

                        state.UnitIds.Add(unitId);
                    }

                    if (state.UnitIds.Count == 0)
                        continue;

                    _groups[state.GroupId] = state;
                    for (int memberIndex = 0; memberIndex < state.UnitIds.Count; memberIndex++)
                        _groupByUnit[state.UnitIds[memberIndex]] = state.GroupId;

                    AdvanceOrdinalPast(state.GroupId);
                }
            }

            // Report the restored table so listeners (HUD, replication cache)
            // rebuild from scratch.
            _signalBus?.Fire(new UnitGroupChangedSignal
            {
                GroupId = string.Empty,
                OwnerId = string.Empty,
                Kind = UnitGroupChangeKind.MembersChanged,
                MemberUnitIds = Array.Empty<string>(),
            });
        }

        // ── Internals ─────────────────────────────────────────────────

        private bool TryResolveOwnedGroup(
            string ownerId,
            string groupId,
            out GroupState state,
            out string reason)
        {
            state = null;
            if (string.IsNullOrWhiteSpace(groupId)
                || !_groups.TryGetValue(groupId, out state))
            { reason = "Group not found."; return false; }

            if (!string.Equals(state.OwnerId, Normalize(ownerId), StringComparison.Ordinal))
            { reason = "Group belongs to another owner."; return false; }

            reason = null;
            return true;
        }

        private bool TryValidateMember(string ownerId, string unitId, out string reason)
        {
            reason = null;
            if (string.IsNullOrWhiteSpace(unitId))
            { reason = "Unit id is empty."; return false; }

            if (_units == null || !_units.TryGetUnitPosition(unitId, out _))
            { reason = $"Unit '{unitId}' is not on the map."; return false; }

            if (_ownership != null
                && !string.Equals(
                    Normalize(_ownership.GetUnitOwnerId(unitId)),
                    ownerId,
                    StringComparison.Ordinal))
            { reason = $"Unit '{unitId}' belongs to another owner."; return false; }

            if (_groupByUnit.ContainsKey(unitId))
            { reason = $"Unit '{unitId}' already belongs to a group."; return false; }

            return true;
        }

        private Vector2Int? ResolveMemberDestination(
            string unitId,
            Vector2Int target,
            HashSet<Vector2Int> reserved)
        {
            IReadOnlyList<UnitMovementTileSnapshot> tiles = _movementQuery?.GetMovementTiles(unitId);
            if (tiles == null || tiles.Count == 0)
                return null;

            Vector2Int best = default;
            float bestScore = float.MaxValue;
            bool found = false;
            for (int index = 0; index < tiles.Count; index++)
            {
                UnitMovementTileSnapshot tile = tiles[index];
                if (!tile.IsReachable || reserved.Contains(tile.Position))
                    continue;
                if (IsBlockedByForeignOccupant(unitId, tile.Position))
                    continue;

                float score = Distance(tile.Position, target) * 1000f + tile.Cost;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = tile.Position;
                    found = true;
                }
            }

            return found ? best : (Vector2Int?)null;
        }

        private bool IsBlockedByForeignOccupant(string unitId, Vector2Int position)
        {
            if (_objectsMap == null
                || !_objectsMap.TryGetOccupant(position, out string occupantId)
                || string.IsNullOrWhiteSpace(occupantId))
                return false;

            // A cell currently held by a group member is a valid formation cell:
            // the member vacates it during the same order.
            if (string.Equals(occupantId, unitId, StringComparison.Ordinal)
                || (_groupByUnit.TryGetValue(occupantId, out string occupantGroup)
                    && _groupByUnit.TryGetValue(unitId, out string unitGroup)
                    && string.Equals(occupantGroup, unitGroup, StringComparison.Ordinal)))
                return false;

            return true;
        }

        private int DistanceTo(string unitId, Vector2Int target)
        {
            return _units != null && _units.TryGetUnitPosition(unitId, out Vector2Int position)
                ? Distance(position, target)
                : int.MaxValue;
        }

        private static int Distance(Vector2Int a, Vector2Int b)
            => Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));

        private void Disband(GroupState state)
        {
            for (int index = 0; index < state.UnitIds.Count; index++)
                _groupByUnit.Remove(state.UnitIds[index]);
            _groups.Remove(state.GroupId);
            FireGroupChanged(state, UnitGroupChangeKind.Disbanded);
        }

        private void RemoveMember(string unitId)
        {
            if (!_groupByUnit.TryGetValue(unitId, out string groupId))
                return;
            _groupByUnit.Remove(unitId);
            if (!_groups.TryGetValue(groupId, out GroupState state))
                return;
            state.UnitIds.Remove(unitId);
            if (state.UnitIds.Count == 0)
                Disband(state);
            else
                FireGroupChanged(state, UnitGroupChangeKind.MembersChanged);
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
            => RemoveMember(signal.UnitId);

        private void OnUnitGarrisonStateChanged(UnitGarrisonStateChangedSignal signal)
        {
            if (signal.IsGarrisoned)
                RemoveMember(signal.UnitId);
        }

        private void FireGroupChanged(GroupState state, UnitGroupChangeKind kind)
        {
            _signalBus?.Fire(new UnitGroupChangedSignal
            {
                GroupId = state.GroupId,
                OwnerId = state.OwnerId,
                Kind = kind,
                MemberUnitIds = kind == UnitGroupChangeKind.Disbanded
                    ? Array.Empty<string>()
                    : state.UnitIds.ToArray(),
            });
        }

        private void AdvanceOrdinalPast(string groupId)
        {
            const string prefix = "grp-";
            if (!groupId.StartsWith(prefix, StringComparison.Ordinal))
                return;
            if (int.TryParse(groupId.Substring(prefix.Length), out int ordinal)
                && ordinal >= _nextGroupOrdinal)
                _nextGroupOrdinal = ordinal + 1;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
