namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Сумісний composition-контракт для gameplay FogOfWar service.
    /// Нові залежності краще брати через вузькі інтерфейси:
    /// <see cref="IFogMapInitializer"/>, <see cref="IFogVisionSourceRegistry"/>,
    /// <see cref="IFogStateReader"/>, <see cref="IFogExplorationSnapshotStore"/>
    /// та <see cref="IFogDirtyTileFeed"/>.
    /// </summary>
    public interface IFogOfWarService
        : IFogMapInitializer
        , IFogVisionSourceRegistry
        , IFogStateReader
        , IFogExplorationSnapshotStore
        , IFogDirtyTileFeed
    {
    }
}
