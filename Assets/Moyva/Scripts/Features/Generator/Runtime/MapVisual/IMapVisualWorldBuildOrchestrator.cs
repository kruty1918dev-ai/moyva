using System.Threading.Tasks;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMapVisualWorldBuildOrchestrator
    {
        void BuildWorld();
        Task BuildWorldAsync();
    }
}
