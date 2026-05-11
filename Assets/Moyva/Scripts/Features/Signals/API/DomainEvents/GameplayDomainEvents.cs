using UnityEngine;

namespace Kruty1918.Moyva.Signals.DomainEvents
{
    /// <summary>
    /// Gameplay/domain events layer. These events represent game state transitions
    /// and should not carry UI-only intent.
    /// </summary>
    public struct UnitCreatedDomainEvent
    {
        public string UnitId;
        public string UnitTypeId;
        public Vector2Int Position;
        public int VisionRange;
        public GameObject UnitObject;
        public string OwnerId;
    }

    public struct UnitMovedDomainEvent
    {
        public string UnitId;
        public Vector2Int NewPosition;
        public float Cost;
        public string SourceFactionId;
    }

    public struct UnitDestroyedDomainEvent
    {
        public string UnitId;
    }

    public struct WorldBuiltDomainEvent
    {
    }

    public struct GameModeChangedDomainEvent
    {
        public Kruty1918.Moyva.Signals.GameModeType NewMode;
    }

    public struct BuildingPlacedDomainEvent
    {
        public string BuildingId;
        public Vector2Int Position;
        public string OwnerId;
        public string SourceFactionId;
    }

    public struct BuildingDemolishedDomainEvent
    {
        public string BuildingId;
        public Vector2Int Position;
        public string OwnerId;
        public string SourceFactionId;
    }

    public struct EconomyTickCompletedDomainEvent
    {
        public string SettlementId;
        public string OwnerId;
        public int Turn;
        public int TotalPopulation;
        public int Arrivals;
        public int Deaths;
        public int ProductionCyclesCompleted;
    }

    public struct SettlementCreatedDomainEvent
    {
        public string SettlementId;
        public string OwnerId;
        public Vector2Int TownHallPosition;
    }

    public struct SettlementDeactivatedDomainEvent
    {
        public string SettlementId;
        public string OwnerId;
        public string Reason;
    }

    public struct SettlementResourceChangedDomainEvent
    {
        public string SettlementId;
        public string OwnerId;
        public string ResourceId;
        public float NewAmount;
        public float Delta;
    }

    public struct ResourceDeficitDomainEvent
    {
        public string SettlementId;
        public string OwnerId;
        public string ResourceId;
    }

    public struct GameStartedDomainEvent
    {
    }

    public struct GameEndedDomainEvent
    {
        public string WinnerId;
    }

    public struct GamePausedDomainEvent
    {
        public bool IsPaused;
    }
}
