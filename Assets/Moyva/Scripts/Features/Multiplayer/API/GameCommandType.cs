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
    }
}
