using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Last-known snapshot of a unit for one fog owner. Only the state the
    /// owner legitimately observed — never the unit's hidden current state.
    /// </summary>
    [Serializable]
    public sealed class FogIntelUnitRecord
    {
        public string UnitId;
        public string TypeId;
        public string OwnerId;
        public Vector2Int LastKnownPosition;
        public long LastSeenSequence;

        public FogIntelUnitRecord Clone() => (FogIntelUnitRecord)MemberwiseClone();
    }

    /// <summary>
    /// Last-known snapshot of a placed building for one fog owner. The record
    /// persists until the owner re-observes the cell and sees it changed or
    /// gone.
    /// </summary>
    [Serializable]
    public sealed class FogIntelBuildingRecord
    {
        public string BuildingId;
        public string OwnerId;
        public Vector2Int Position;
        public int RotationQuarterTurns;
        public long LastSeenSequence;

        public FogIntelBuildingRecord Clone() => (FogIntelBuildingRecord)MemberwiseClone();
    }

    /// <summary>
    /// Serializable per-owner intel payload for save/load and replication.
    /// </summary>
    [Serializable]
    public sealed class FogIntelSnapshot
    {
        public List<FogIntelUnitRecord> Units = new List<FogIntelUnitRecord>();
        public List<FogIntelBuildingRecord> Buildings = new List<FogIntelBuildingRecord>();
    }
}
