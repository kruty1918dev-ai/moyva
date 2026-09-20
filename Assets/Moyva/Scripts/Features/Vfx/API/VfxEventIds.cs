namespace Kruty1918.Moyva.Vfx.API
{
    /// <summary>Канонічні ідентифікатори VFX-подій, на які мапляться правила ефектів.</summary>
    public static class VfxEventIds
    {
        /// <summary>Будівлю розміщено.</summary>
        public const string BuildingPlaced = "building-placed";
        /// <summary>Будівля перейшла в робочий стан.</summary>
        public const string BuildingOperational = "building-operational";
        /// <summary>Будівлю знесено.</summary>
        public const string BuildingDemolished = "building-demolished";

        /// <summary>Юніта створено.</summary>
        public const string UnitSpawned = "unit-spawned";
        /// <summary>Пил під час руху юніта.</summary>
        public const string UnitMoveDust = "unit-move-dust";
        /// <summary>Юніта знищено.</summary>
        public const string UnitDestroyed = "unit-destroyed";

        /// <summary>Атака в бою.</summary>
        public const string CombatAttack = "combat-attack";
        /// <summary>Влучання в бою.</summary>
        public const string CombatImpact = "combat-impact";

        /// <summary>Поселення створено.</summary>
        public const string SettlementCreated = "settlement-created";
        /// <summary>Поселення захоплено.</summary>
        public const string SettlementCaptured = "settlement-captured";

        /// <summary>Пінг у світі.</summary>
        public const string WorldPing = "world-ping";
    }
}
