namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>Типи ігрових команд, що передаються по мережі.</summary>
    public enum GameCommandType
    {
        UnitMove         = 1,
        BuildingPlace    = 2,
        BuildingDemolish = 3,
        UnitSpawn        = 4,
        GameStateChange  = 5,
        StartGame        = 6,
        EndTurn          = 7,
        StartingPositions = 8,
        QosPing          = 9,
        QosPong          = 10,
        WorldSeedHandshake = 11,
        MatchStartSync   = 12,
        WorldStateSnapshot = 13,
        CaravanCommand = 14,
        CombatCommand = 15,
        SettlementCaptureCommand = 16,
        /// <summary>
        /// Host correction that tells a peer to forget a remembered entity:
        /// the peer re-observed the entity's last-known cell and it is gone.
        /// Never sent for entities the peer cannot legitimately observe.
        /// </summary>
        UnitVanish = 17,
    }
}
