namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Сумісний composition-контракт для visual FogOfWar updater-а.
    /// Нові залежності краще брати через вузькі інтерфейси:
    /// <see cref="IFogVisualWorldBootstrapper"/>, <see cref="IFogVisualPreviewRenderer"/>,
    /// <see cref="IFogVisualDeltaUpdater"/> та <see cref="IFogVisualFullRebuilder"/>.
    /// </summary>
    public interface IFogVisualUpdater
        : IFogVisualWorldBootstrapper
        , IFogVisualPreviewRenderer
        , IFogVisualDeltaUpdater
        , IFogVisualFullRebuilder
    {
    }
}
