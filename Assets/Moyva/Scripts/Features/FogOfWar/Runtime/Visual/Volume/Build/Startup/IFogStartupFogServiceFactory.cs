using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogStartupFogServiceFactory
    {
        IFogOfWarService Create(int width, int height);
        IFogOfWarService Create(int width, int height, Vector2Int visibleCenter, int visibleRadius, FogRevealShape visibleShape, bool keepVisible);
    }
}
