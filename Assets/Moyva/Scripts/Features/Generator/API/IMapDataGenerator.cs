using System;
using System.Collections;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapDataGenerator
    {
        /// <summary>
        /// Головний метод, який запускає весь конвеєр і повертає готову віртуальну карту.
        /// </summary>
        void GenerateMapData(int width, int height, Action<string[,], string[,], float[,], string[,]> onComplete);
    }
}