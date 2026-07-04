using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogStartupFogServiceFactory : IFogStartupFogServiceFactory
    {
        public IFogOfWarService Create(int width, int height)
            => new StartupFogService(width, height);

        public IFogOfWarService Create(int width, int height, Vector2Int visibleCenter, int visibleRadius, FogRevealShape visibleShape, bool keepVisible)
            => new StartupFogService(width, height, visibleCenter, visibleRadius, visibleShape, keepVisible);
    }
}
