namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMapFeatureGenerator
    {
        /// <summary>
        /// Модифікує вже створену карту біомів (наприклад, прорізає річки).
        /// </summary>
        void ApplyFeatures(string[,] biomeMap, float[,] heightMap, int width, int height);
    }
}