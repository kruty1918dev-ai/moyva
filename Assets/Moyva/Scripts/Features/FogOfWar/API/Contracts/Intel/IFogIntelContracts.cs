using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Read-only access to remembered (last-known) entity intel per fog owner.
    /// Records never expose hidden current state — only what the owner last
    /// legitimately observed.
    /// </summary>
    public interface IFogIntelReader
    {
        /// <summary>Fired with the owner id whenever that owner's intel changes.</summary>
        event Action<string> IntelChanged;

        IReadOnlyCollection<FogIntelUnitRecord> GetRememberedUnits(string ownerId);
        IReadOnlyCollection<FogIntelBuildingRecord> GetRememberedBuildings(string ownerId);
        bool TryGetRememberedUnit(string ownerId, string unitId, out FogIntelUnitRecord record);
        bool TryGetRememberedBuilding(string ownerId, Vector2Int position, out FogIntelBuildingRecord record);
    }

    /// <summary>
    /// Snapshot capture/restore for per-owner intel. Used by save/load and
    /// host-migration data carriers.
    /// </summary>
    public interface IFogIntelSnapshotStore
    {
        IReadOnlyCollection<string> GetIntelOwnerIds();
        FogIntelSnapshot CaptureSnapshot(string ownerId);
        void LoadSnapshot(string ownerId, FogIntelSnapshot snapshot);
    }

    /// <summary>
    /// Ingest point for intel replicated from the authoritative host. Records
    /// arrive already filtered to what the local owner legitimately knows.
    /// </summary>
    public interface IFogIntelReplicationSink
    {
        void ApplyReplicatedUnit(string ownerId, FogIntelUnitRecord record);
        void ApplyReplicatedBuilding(string ownerId, FogIntelBuildingRecord record);
        void RemoveReplicatedUnit(string ownerId, string unitId);
        void RemoveReplicatedBuilding(string ownerId, Vector2Int position);
    }
}
