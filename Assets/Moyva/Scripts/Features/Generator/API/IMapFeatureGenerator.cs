using System.Collections;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapFeatureGenerator
    {
        /// <summary>
        /// Модифікує вже створену карту біомів (наприклад, прорізає річки).
        /// </summary>
        IEnumerator ApplyFeaturesRoutine(string[,] biomeMap, float[,] heightMap, int width, int height);
    }
}