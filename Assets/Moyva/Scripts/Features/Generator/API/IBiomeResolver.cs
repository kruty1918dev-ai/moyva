using System;
using System.Collections;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IBiomeResolver
    {
        /// <summary>
        /// Перетворює маски висот (та інші, якщо будуть) у матрицю TypeId тайлів.
        /// </summary>
        IEnumerator ResolveBiomesRoutine(float[,] heightMap, string[,] currentMap, Action<string[,]> onComplete);
    }
}