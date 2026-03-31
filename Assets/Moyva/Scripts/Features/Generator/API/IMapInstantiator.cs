using System.Collections;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapInstantiator
    {
        /// <summary>
        /// Головний метод, який приймає віртуальну карту (матрицю TypeId) і створює відповідні тайли у світі.
        /// </summary>
        void BuildWorld();
    }
}