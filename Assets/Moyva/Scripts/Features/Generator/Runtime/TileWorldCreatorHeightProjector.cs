using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Thin Unity lifecycle adapter for legacy post-build terrain height projection.
    /// Core logic lives in TwcHeightProjection services.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TileWorldCreatorHeightProjector : MonoBehaviour
    {
        private readonly TileWorldCreatorHeightProjectionState _state = new TileWorldCreatorHeightProjectionState();
        private ITileWorldCreatorHeightProjectionService _service;

        [Inject]
        private void Construct([InjectOptional] ITileWorldCreatorHeightProjectionService service = null)
        {
            _service = service;
        }

        public void Configure(
            Transform targetRoot,
            int[,] terrainLevelMap,
            float cellSize,
            int heightStep,
            float trackingSeconds)
        {
            ResolveService().Configure(
                _state,
                this,
                targetRoot,
                terrainLevelMap,
                cellSize,
                heightStep,
                trackingSeconds);
        }

        private void LateUpdate()
        {
            ResolveService().Tick(_state, this);
        }

        private ITileWorldCreatorHeightProjectionService ResolveService()
        {
            return _service ??= TileWorldCreatorHeightProjectionComposition.Create();
        }
    }
}
