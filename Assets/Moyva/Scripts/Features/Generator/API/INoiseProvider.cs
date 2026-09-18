// Features/Generator/API/INoiseProvider.cs
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.API
{
    public interface INoiseProvider
    {
        /// <summary>
        /// Генерує карту значень від 0.0 до 1.0 на основі шуму.
        /// </summary>
        float[,] GenerateNoiseMap(DataNoiseSettings settings, int width, int height);
    }
}