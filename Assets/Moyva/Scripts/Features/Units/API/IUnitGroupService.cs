using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    /// <summary>
    /// Read-only snapshot of one unit group. Groups bind same-owner units into a
    /// formation that receives shared move orders; members keep their own
    /// authoritative stats, stamina and cells.
    /// </summary>
    public readonly struct UnitGroupSnapshot
    {
        public UnitGroupSnapshot(string groupId, string ownerId, IReadOnlyList<string> unitIds)
        {
            GroupId = groupId ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            UnitIds = unitIds ?? (IReadOnlyList<string>)System.Array.Empty<string>();
        }

        /// <summary>Stable id assigned by the authoritative side.</summary>
        public string GroupId { get; }

        public string OwnerId { get; }

        /// <summary>Ordered member ids; index 0 is the group leader.</summary>
        public IReadOnlyList<string> UnitIds { get; }

        public int Count => UnitIds?.Count ?? 0;
    }

    /// <summary>
    /// Canonical unit grouping authority. All membership mutations run here;
    /// group move orders decompose into canonical per-unit movement so stamina,
    /// occupancy and turn rules stay enforced per member.
    /// </summary>
    public interface IUnitGroupService
    {
        bool TryCreateGroup(
            string ownerId,
            IReadOnlyList<string> unitIds,
            out string groupId,
            out string reason);

        bool TryDisbandGroup(string ownerId, string groupId, out string reason);

        bool TryAddUnit(string ownerId, string groupId, string unitId, out string reason);

        bool TryRemoveUnit(string ownerId, string groupId, string unitId, out string reason);

        /// <summary>
        /// Orders every group member toward <paramref name="targetPosition"/>.
        /// The leader takes the target cell; other members take the closest
        /// reachable free cells around it. Members that cannot reach a legal
        /// cell stay behind; the order still succeeds if at least one member
        /// moves.
        /// </summary>
        bool TryMoveGroup(string ownerId, string groupId, Vector2Int targetPosition, out string reason);

        bool TryGetGroup(string groupId, out UnitGroupSnapshot snapshot);

        /// <summary>Returns the group id the unit belongs to, or empty.</summary>
        string GetGroupIdOfUnit(string unitId);

        IReadOnlyList<UnitGroupSnapshot> GetGroupsForOwner(string ownerId);
    }

    /// <summary>
    /// Save/load and replication boundary for group membership. Restore must
    /// never re-run authority checks; it rebuilds membership for units that
    /// actually exist.
    /// </summary>
    public interface IUnitGroupStateStore
    {
        IReadOnlyList<UnitGroupSnapshot> CaptureState();

        /// <summary>
        /// Save/load path: rebuilds membership for units that actually exist.
        /// </summary>
        void RestoreState(IReadOnlyList<UnitGroupSnapshot> groups);

        /// <summary>
        /// Replication path: applies the authoritative host table verbatim.
        /// Member existence is not validated — unit replication may lag the
        /// group sync, and the next host snapshot corrects any divergence.
        /// </summary>
        void RestoreReplicated(IReadOnlyList<UnitGroupSnapshot> groups);
    }

    /// <summary>
    /// Client-side boundary for group commands in multiplayer sessions. A
    /// client never mutates local group state directly; the host executes the
    /// canonical <see cref="IUnitGroupService"/> and broadcasts the group table.
    /// </summary>
    public interface IUnitGroupRemoteCommandRequester
    {
        bool TryRequestCreateGroup(string ownerId, IReadOnlyList<string> unitIds, out string reason);
        bool TryRequestDisbandGroup(string ownerId, string groupId, out string reason);
        bool TryRequestAddUnit(string ownerId, string groupId, string unitId, out string reason);
        bool TryRequestRemoveUnit(string ownerId, string groupId, string unitId, out string reason);
        bool TryRequestMoveGroup(string ownerId, string groupId, Vector2Int targetPosition, out string reason);
    }
}
