using Kruty1918.Telemetry.Core;

namespace Kruty1918.Moyva.Telemetry
{
    public struct MoyvaEconomyTickEvent : ITelemetryEvent
    {
        public string SettlementId;
        public string OwnerId;
        public int Turn;
        public int Population;
        public int Arrivals;
        public int Deaths;
        public int ProductionCycles;

        public string EventType => "moyva.economy.tick";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("settlementId", SettlementId);
            w.Field("ownerId", OwnerId);
            w.Field("turn", Turn);
            w.Field("population", Population);
            w.Field("arrivals", Arrivals);
            w.Field("deaths", Deaths);
            w.Field("productionCycles", ProductionCycles);
        }
    }

    public struct MoyvaSettlementEvent : ITelemetryEvent
    {
        public string Kind; // created|deactivated|captured
        public string SettlementId;
        public string OwnerId;
        public string PreviousOwnerId;
        public string NewOwnerId;
        public string Reason;

        public string EventType => "moyva.economy.settlement";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("kind", Kind);
            w.Field("settlementId", SettlementId);
            if (OwnerId != null) w.Field("ownerId", OwnerId);
            if (PreviousOwnerId != null) w.Field("previousOwnerId", PreviousOwnerId);
            if (NewOwnerId != null) w.Field("newOwnerId", NewOwnerId);
            if (Reason != null) w.Field("reason", Reason);
        }
    }

    public struct MoyvaResourceEvent : ITelemetryEvent
    {
        public string Kind; // changed|deficit
        public string SettlementId;
        public string ResourceId;
        public float Amount;
        public float Delta;

        public string EventType => "moyva.economy.resource";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("kind", Kind);
            w.Field("settlementId", SettlementId);
            w.Field("resourceId", ResourceId);
            w.Field("amount", Amount);
            w.Field("delta", Delta);
        }
    }

    public struct MoyvaFogChangedEvent : ITelemetryEvent
    {
        public int ChangedTiles;

        public string EventType => "moyva.fog.changed";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w) => w.Field("changedTiles", ChangedTiles);
    }

    public struct MoyvaWorldGeneratedEvent : ITelemetryEvent
    {
        public string Source;
        public int Width;
        public int Height;
        public float CellSize;
        public string StartupSessionId;
        public int SnapshotRevision;

        public string EventType => "moyva.world.generated";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("source", Source);
            w.Field("width", Width);
            w.Field("height", Height);
            w.Field("cellSize", CellSize);
            if (StartupSessionId != null) w.Field("startupSessionId", StartupSessionId);
            w.Field("snapshotRevision", SnapshotRevision);
        }
    }

    public struct MoyvaSaveEvent : ITelemetryEvent
    {
        public string EventType; // moyva.save.saved | moyva.save.loadRequested
        public int Slot;
        public bool Success;

        string ITelemetryEvent.EventType => EventType;
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("slot", Slot);
            if (EventType == "moyva.save.saved") w.Field("success", Success);
        }
    }

    public struct MoyvaUiPanelEvent : ITelemetryEvent
    {
        public string Kind;   // worldInfo|buildingInfo|unitInfo|mapObjectInfo|selection
        public string Action; // opened|closed|changed
        public string ObjectId;
        public string SelectionKind;

        public string EventType => "moyva.ui.panel";
        public string ContractId => EventType;
        public int ContractVersion => 1;

        public void WriteTo(ITelemetryEventWriter w)
        {
            w.Field("kind", Kind);
            w.Field("action", Action);
            if (ObjectId != null) w.Field("objectId", ObjectId);
            if (SelectionKind != null) w.Field("selectionKind", SelectionKind);
        }
    }
}
