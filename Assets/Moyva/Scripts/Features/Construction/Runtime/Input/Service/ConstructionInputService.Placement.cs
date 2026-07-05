using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private void HandlePointerSelection(Vector2 screenPos, int pointerId, bool allowDragStart, bool selectionOnRelease, bool skipUiCheck = false)
        {
            if (!skipUiCheck && IsPointerOverInteractiveUI(screenPos, pointerId))
            {
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Click ignored: pointer over interactive UI.");
                return;
            }

            TryResolvePointerTile(screenPos, out Vector2Int tilePos);

            if (VerboseLogs)
            {
                string inputKind = selectionOnRelease ? "ReleaseSelect" : "PressSelect";
                Debug.Log(
                    $"{LogTag} {inputKind} screen={screenPos}, tile={tilePos}, " +
                    $"state={_constructionService.State}, demolish={_constructionService.IsDemolishMode}");
            }

            if (!_gridService.TryGetTileData(tilePos, out _))
            {
                if (VerboseLogs)
                    Debug.LogWarning($"{LogTag} Tile {tilePos} is outside grid. Click ignored.");
                return;
            }

            if (_constructionService.IsDemolishMode && IsPlacementSessionInactive())
            {
                ClearTouchPlacementState();
                bool result = _constructionService.TryDemolishAt(tilePos);
                if (VerboseLogs)
                    Debug.Log($"{LogTag} TryDemolishAt({tilePos}) => {result}");
                return;
            }

            if (IsPlacementSessionActive())
            {
                HandlePlacementSelection(tilePos, allowDragStart, selectionOnRelease);
                return;
            }

            if (VerboseLogs)
                Debug.Log($"{LogTag} Click ignored: placement state is {_constructionService.State}.");
        }

        private void HandlePlacementSelection(Vector2Int tilePos, bool allowDragStart, bool selectionOnRelease)
        {
            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            bool wallMode = !string.IsNullOrWhiteSpace(selectedBuildingId) && _wallTopologyService.IsWall(selectedBuildingId);

            if (selectionOnRelease)
            {
                HandleReleaseSelectionPlacement(tilePos, selectedBuildingId, wallMode);
                return;
            }

            if (wallMode && _objectsMapService.TryGetOccupant(tilePos, out var occupantId) && _wallTopologyService.IsWallOrGate(occupantId))
            {
                _wallHandleController.Show(tilePos);
                _isDraggingWallPath = allowDragStart;
                _wallDragStartPosition = tilePos;
                _lastWallDragTile = tilePos;
                _wallDragPendingPositions.Clear();
                _wallDragPendingPositions.Add(tilePos);

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Wall drag started from existing segment at {tilePos}");

                return;
            }

            if (_constructionService.HasPendingPlacementAt(tilePos))
            {
                bool gateMode = !string.IsNullOrWhiteSpace(selectedBuildingId)
                    && _wallTopologyService.IsGate(selectedBuildingId);
                if (gateMode)
                {
                    bool placed = _constructionService.TryPreviewAt(tilePos);
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Gate placement on pending tile {tilePos} => {placed}");
                    return;
                }

                _isDraggingPendingPlacement = allowDragStart && _enableMousePendingPreviewDrag;
                _draggedPlacementPosition = tilePos;

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Drag started for preview at {tilePos}");

                return;
            }

            if (wallMode)
            {
                bool placed = _constructionService.TryPreviewAt(tilePos);
                if (placed && allowDragStart)
                {
                    _isDraggingWallPath = true;
                    _wallDragStartPosition = tilePos;
                    _lastWallDragTile = tilePos;
                    _wallDragPendingPositions.Clear();
                    _wallDragPendingPositions.Add(tilePos);

                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Wall drag started from empty tile at {tilePos}");
                }

                return;
            }

            bool result = _constructionService.TryPreviewAt(tilePos);
            if (VerboseLogs)
                Debug.Log($"{LogTag} TryPreviewAt({tilePos}) => {result}");

            if (result && allowDragStart && _enableMousePendingPreviewDrag)
            {
                _isDraggingPendingPlacement = true;
                _draggedPlacementPosition = tilePos;
            }
        }
    }
}
