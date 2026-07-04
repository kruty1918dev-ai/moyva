using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Thin DI facade for terrain-aware fog visibility.
    /// Heavy LOS and cache logic lives in <see cref="HeightAwareVisionEngine"/>.
    /// </summary>
    internal sealed class HeightAwareVisionService : IHeightAwareVisionService
    {
        private readonly HeightAwareVisionEngine _engine;

        [Inject]
        public HeightAwareVisionService(HeightAwareVisionEngine engine)
        {
            _engine = engine;
        }

        public HeightAwareVisionService([InjectOptional] FogOfWarSettings settings = null)
            : this(new HeightAwareVisionEngine(settings))
        {
        }

        public void SetHeightMap(float[,] heightMap) => _engine.SetHeightMap(heightMap);

        public int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default)
            => _engine.GetSearchRadius(origin, baseVisionRange, maxVisionRange, observerModifiers);

        public float GetVisibilityFactor(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default)
            => _engine.GetVisibilityFactor(origin, target, baseVisionRange, maxVisionRange, observerModifiers, targetModifiers);

        public bool IsTargetVisible(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default)
            => _engine.IsTargetVisible(origin, target, baseVisionRange, maxVisionRange, observerModifiers, targetModifiers);
    }
}
