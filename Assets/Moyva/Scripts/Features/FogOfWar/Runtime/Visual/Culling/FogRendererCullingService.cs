using System;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Thin DI facade for fog-based renderer culling.
    /// Runtime state and scene scanning live in <see cref="FogRendererCullingEngine"/>.
    /// </summary>
    internal sealed class FogRendererCullingService : IInitializable, ITickable, IDisposable
    {
        private readonly FogRendererCullingEngine _engine;

        [Inject]
        public FogRendererCullingService(FogRendererCullingEngine engine)
        {
            _engine = engine;
        }

        public void Initialize() => _engine.Initialize();

        public void Tick() => _engine.Tick();

        public void Dispose() => _engine.Dispose();
    }
}
