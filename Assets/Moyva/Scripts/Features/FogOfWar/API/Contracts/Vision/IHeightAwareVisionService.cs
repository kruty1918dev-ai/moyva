namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Сумісний composition-контракт для legacy споживачів terrain-aware vision.
    /// Нові залежності краще брати через вузькі інтерфейси:
    /// <see cref="IHeightMapVisionContext"/>, <see cref="IHeightAwareSearchRadiusProvider"/>
    /// та <see cref="IHeightAwareVisibilityEvaluator"/>.
    /// </summary>
    public interface IHeightAwareVisionService
        : IHeightMapVisionContext
        , IHeightAwareSearchRadiusProvider
        , IHeightAwareVisibilityEvaluator
    {
    }
}
