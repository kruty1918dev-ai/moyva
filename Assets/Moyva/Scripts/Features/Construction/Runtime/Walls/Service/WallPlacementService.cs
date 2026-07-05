using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallPlacementService : IWallPlacementService
    {
        private readonly IWallDragPreviewService _wallDragPreviewService;
        private readonly IWallPathfinder _wallPathfinder;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallGateReplacementValidator _wallGateReplacementValidator;
        private readonly IWallVisualResolver _wallVisualResolver;
        private readonly IWallHandleController _wallHandleController;

        [Inject]
        public WallPlacementService(
            IWallDragPreviewService wallDragPreviewService,
            IWallPathfinder wallPathfinder,
            IWallTopologyService wallTopologyService,
            IWallGateReplacementValidator wallGateReplacementValidator,
            IWallVisualResolver wallVisualResolver,
            IWallHandleController wallHandleController)
        {
            _wallDragPreviewService = wallDragPreviewService;
            _wallPathfinder = wallPathfinder;
            _wallTopologyService = wallTopologyService;
            _wallGateReplacementValidator = wallGateReplacementValidator;
            _wallVisualResolver = wallVisualResolver;
            _wallHandleController = wallHandleController;
        }

        public void ShowWallHandles(Vector2Int wallPosition) => _wallHandleController.Show(wallPosition);

        // Legacy input entry point; kept for IWallPlacementService compatibility.
        public void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition)
            => _wallDragPreviewService.PreviewDrag(startPosition, touchWorldPosition);

        public IReadOnlyList<Vector2Int> BuildPath(Vector2Int startPosition, Vector2Int endPosition)
            => _wallPathfinder.BuildPath(startPosition, endPosition);

        public bool IsWallOrGate(string buildingId) => _wallTopologyService.IsWallOrGate(buildingId);

        public bool IsWall(string buildingId) => _wallTopologyService.IsWall(buildingId);

        public bool IsGate(string buildingId) => _wallTopologyService.IsGate(buildingId);

        public bool CanReplaceWallWithGate(Vector2Int position, string gateBuildingId, out string replacedWallId)
            => _wallGateReplacementValidator.CanReplaceWallWithGate(position, gateBuildingId, out replacedWallId);

        public bool TryResolvePlacedVisual(Vector2Int position, string occupantId, out GameObject prefab, out Quaternion rotation)
            => _wallVisualResolver.TryResolvePlacedVisual(position, occupantId, out prefab, out rotation);

        public void EndDrag() => _wallHandleController.EndDrag();

        public bool TryResolvePreviewVisual(Vector2Int position, string buildingId, out GameObject prefab)
            => _wallVisualResolver.TryResolvePreviewVisual(position, buildingId, out prefab);
    }
}
