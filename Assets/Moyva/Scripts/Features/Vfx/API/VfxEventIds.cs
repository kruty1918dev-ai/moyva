namespace Kruty1918.Moyva.Vfx.API
{
    /// <summary>
    /// Stable event names used by the VFX catalog JSON mapping.
    /// Gameplay services translate domain signals to these ids; the JSON
    /// preset maps them to prefabs (with optional "context" suffix rules).
    /// </summary>
    public static class VfxEventIds
    {
        public const string BuildingPlaced = "building-placed";
        public const string BuildingOperational = "building-operational";
        public const string BuildingDemolished = "building-demolished";

        public const string UnitSpawned = "unit-spawned";
        public const string UnitMoveDust = "unit-move-dust";
        public const string UnitDestroyed = "unit-destroyed";

        public const string CombatAttack = "combat-attack";
        public const string CombatImpact = "combat-impact";

        public const string SettlementCreated = "settlement-created";
        public const string SettlementCaptured = "settlement-captured";

        public const string WorldPing = "world-ping";
    }
}
