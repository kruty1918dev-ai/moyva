using System.Threading.Tasks;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public interface IMapDataGenerator
    {
        /// <summary>
        /// Головний метод, який запускає весь конвеєр і повертає готову віртуальну карту.
        /// </summary>
        Task<string[,]> GenerateMapDataAsync(int width, int height);
    }
}