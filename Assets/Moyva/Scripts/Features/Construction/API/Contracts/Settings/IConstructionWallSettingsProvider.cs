namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionWallSettingsProvider
    {
        bool AllowGateReplacement { get; }
        bool GateRequiresHorizontalWall { get; }
        bool AllowWallPathThroughExistingWalls { get; }
        bool AllowWallPathThroughPendingWalls { get; }
        bool AllowWallPathThroughGates { get; }
        bool ShowWallHandles { get; }
        ConstructionWallPathMode WallPathMode { get; }
    }
}
