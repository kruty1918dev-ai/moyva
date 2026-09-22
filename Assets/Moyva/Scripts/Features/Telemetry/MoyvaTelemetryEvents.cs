using Kruty1918.Telemetry.Core;

namespace Kruty1918.Moyva.Telemetry
{
    // Typed projections of SignalBus signals → telemetry events.
    // ContractId == EventType: one versioned contract per event type.

    public struct MoyvaMatchLifecycleEvent : ITelemetryEvent
    {
        public string EventType;      // moyva.match.started | .ended | .paused | .modeChanged
        public string Mode;
        public int PlayerCount;
        public string Source;
        public string WinnerId;
        public long DurationMs;
        public bool IsPaused;
        public string NewMode;

        string ITelemetryEvent.EventType => EventType;
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            if (Mode != null) w.Field("mode", Mode);
            if (PlayerCount > 0) w.Field("playerCount", PlayerCount);
            if (Source != null) w.Field("source", Source);
            if (WinnerId != null) w.Field("winnerId", WinnerId);
            if (DurationMs > 0) w.Field("durationMs", DurationMs);
            if (EventType == "moyva.match.paused") w.Field("isPaused", IsPaused);
            if (NewMode != null) w.Field("newMode", NewMode);
        }
    }

    public struct MoyvaTurnLifecycleEvent : ITelemetryEvent
    {
        public string Phase; // started|ending|roundCompleted
        public int Round;
        public long GlobalTurn;
        public int FactionIndex;
        public string OwnerId;

        public string EventType => "moyva.turn.lifecycle";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("phase", Phase);
            w.Field("round", Round);
            w.Field("globalTurn", GlobalTurn);
            w.Field("factionIndex", FactionIndex);
            if (OwnerId != null) w.Field("ownerId", OwnerId);
        }
    }

    public struct MoyvaConstructionEvent : ITelemetryEvent
    {
        public string EventType; // placed|cancelled|rejected|demolished|transferred
        public string BuildingId;
        public int X, Y;
        public string OwnerId;
        public int Rotation;
        public bool Relocated;
        public string Reason;
        public string PreviousOwnerId;
        public string NewOwnerId;

        string ITelemetryEvent.EventType => EventType;
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            if (BuildingId != null) w.Field("buildingId", BuildingId);
            if (EventType != "moyva.construction.cancelled")
            {
                w.Field("x", X);
                w.Field("y", Y);
            }
            if (OwnerId != null) w.Field("ownerId", OwnerId);
            if (EventType == "moyva.construction.placed")
            {
                w.Field("rotation", Rotation);
                w.Field("relocated", Relocated);
            }
            if (Reason != null) w.Field("reason", Reason);
            if (PreviousOwnerId != null) w.Field("previousOwnerId", PreviousOwnerId);
            if (NewOwnerId != null) w.Field("newOwnerId", NewOwnerId);
        }
    }

    public struct MoyvaUnitEvent : ITelemetryEvent
    {
        public string EventType; // created|moved|destroyed|moveRejected
        public string UnitId;
        public string UnitTypeId;
        public int X, Y;
        public float Cost;
        public string OwnerId;
        public string Reason;

        string ITelemetryEvent.EventType => EventType;
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("unitId", UnitId);
            if (UnitTypeId != null) w.Field("unitTypeId", UnitTypeId);
            if (EventType != "moyva.units.destroyed")
            {
                w.Field("x", X);
                w.Field("y", Y);
            }
            if (EventType == "moyva.units.moved") w.Field("cost", Cost);
            if (OwnerId != null) w.Field("ownerId", OwnerId);
            if (Reason != null) w.Field("reason", Reason);
        }
    }

    public struct MoyvaRecruitmentEvent : ITelemetryEvent
    {
        public string EventType; // moyva.recruitment.queueChanged|ready|deployed|rejected
        public string OwnerId;
        public string UnitTypeId;
        public long QueueId;
        public int BuildingX, BuildingY;
        public string UnitId;
        public string Reason;

        string ITelemetryEvent.EventType => EventType;
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            if (OwnerId != null) w.Field("ownerId", OwnerId);
            if (UnitTypeId != null) w.Field("unitTypeId", UnitTypeId);
            if (QueueId != 0) w.Field("queueId", QueueId);
            if (EventType != "moyva.recruitment.rejected")
            {
                w.Field("buildingX", BuildingX);
                w.Field("buildingY", BuildingY);
            }
            if (UnitId != null) w.Field("unitId", UnitId);
            if (Reason != null) w.Field("reason", Reason);
        }
    }

    public struct MoyvaNetPeerEvent : ITelemetryEvent
    {
        public string Kind; // connected|disconnected
        public string PeerIdHash;

        public string EventType => "moyva.net.peer";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("kind", Kind);
            w.Field("peerIdHash", PeerIdHash);
        }
    }
}
