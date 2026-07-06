using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [DisallowMultipleComponent]
    public sealed class TileWorldCreatorRuntimeMeshOptimizer : MonoBehaviour
    {
        private readonly TileWorldCreatorRuntimeMeshOptimizerState _state = new TileWorldCreatorRuntimeMeshOptimizerState();
        private ITileWorldCreatorRuntimeMeshOptimizerService _service;

        [Inject]
        private void Construct([InjectOptional] ITileWorldCreatorRuntimeMeshOptimizerService service = null)
        {
            _service = service;
        }

        public void Configure(Transform targetRoot, int clustersPerFrame, bool deactivateSourceObjects)
        {
            ResolveService().Configure(_state, this, targetRoot, clustersPerFrame, deactivateSourceObjects);
        }

        public void ClearConfiguration(string reason)
        {
            ResolveService().ClearConfiguration(_state, reason);
        }

        public void RequestOptimizeAfterStable(string reason)
        {
            ResolveService().RequestOptimizeAfterStable(_state, this, reason);
        }

        private ITileWorldCreatorRuntimeMeshOptimizerService ResolveService()
        {
            return _service ??= TileWorldCreatorRuntimeMeshOptimizerComposition.Create();
        }
    }
}
