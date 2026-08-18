using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private void PreviewWallPathTo(Vector2Int targetTile)
        {
            var newPath = _wallPathfinder.BuildPath(_wallDragStartPosition, targetTile);

            _wallDragNewPathSet.Clear();
            for (int i = 0; i < newPath.Count; i++)
                _wallDragNewPathSet.Add(newPath[i]);

            _wallDragPositionsToRemove.Clear();
            foreach (var pos in _wallDragPendingPositions)
            {
                if (!_wallDragNewPathSet.Contains(pos))
                    _wallDragPositionsToRemove.Add(pos);
            }

            for (int i = 0; i < _wallDragPositionsToRemove.Count; i++)
            {
                var pos = _wallDragPositionsToRemove[i];
                _constructionService.RemovePendingAt(pos);
                _wallDragPendingPositions.Remove(pos);
            }

            for (int i = 0; i < newPath.Count; i++)
            {
                var tile = newPath[i];

                if (!_constructionService.HasPendingPlacementAt(tile))
                {
                    if (!_objectsMapService.TryGetOccupant(tile, out var occupantId) || !_wallTopologyService.IsWallOrGate(occupantId))
                    {
                        string selectedBuildingId = _constructionService.GetSelectedBuildingId();
                        if (!IsBuildGridPlacementAllowed(tile, selectedBuildingId))
                            break;

                        bool placed = _constructionService.TryPreviewAt(tile);
                        if (!placed)
                            break;
                    }

                    _wallDragPendingPositions.Add(tile);
                }
                else if (_constructionService.TryGetPendingBuildingIdAt(tile, out var existingPendingId)
                         && _wallTopologyService.IsWallOrGate(existingPendingId))
                {
                    _wallDragPendingPositions.Add(tile);
                }
            }

            _lastWallDragTile = targetTile;
        }
    }
}
