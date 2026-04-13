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
    }
}
