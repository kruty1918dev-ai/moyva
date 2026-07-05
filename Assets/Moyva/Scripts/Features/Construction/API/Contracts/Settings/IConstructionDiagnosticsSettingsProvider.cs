namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionDiagnosticsSettingsProvider
    {
        bool EnableVerboseLogs { get; }
        bool EnablePlacementDebug { get; }
        bool EnableResourceDebug { get; }
        bool EnableVisualDebug { get; }
        bool EnableWallDebug { get; }
        bool DrawSceneGizmos { get; }
        bool DrawBlockedTiles { get; }
        bool DrawInfluenceZones { get; }
    }
}
