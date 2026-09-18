using System.Collections;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapInstantiator
    {
        /// <summary>
        /// Головний метод, який приймає віртуальну карту (матрицю TypeId) і створює відповідні тайли у світі.
        /// </summary>
        void BuildWorld();

        /// <summary>
        /// Same build as <see cref="BuildWorld"/> but yields between stages so
        /// scene startup and the transport keep ticking during generation.
        /// </summary>
        Task BuildWorldAsync();
    }
}