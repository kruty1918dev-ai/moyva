namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IBiomeResolver
    {
        /// <summary>
        /// Перетворює маски висот (та інші, якщо будуть) у матрицю TypeId тайлів.
        /// </summary>
        string[,] ResolveBiomes(float[,] heightMap, int width, int height);
    }
}