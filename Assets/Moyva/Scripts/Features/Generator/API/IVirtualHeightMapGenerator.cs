using System;
using System.Collections;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IVirtualHeightMapGenerator
    {
        IEnumerator GenerateVirtualHeightMapRoutine(float[,] heightMap, Action<string[,]> onComplete);
    }
}