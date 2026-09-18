using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class UnitRecruitmentDeploymentController
    {
        private void RefreshDeploymentTiles()
        {
            if (_session == null)
                return;

            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles =
                _recruitment.GetDeploymentTiles(
                    _session.OwnerId,
                    _session.RecruitingBuildingPosition,
                    _session.QueueId);

            _session.SetTiles(tiles);
            RefreshDeploymentOverlay();

            if (_session.SelectedTile.HasValue
                && !_session.ValidTiles.Contains(_session.SelectedTile.Value))
            {
                ClearSelectedTile();
            }
        }

        private void SelectTile(Vector2Int tile)
        {
            if (_session == null)
                return;

            if (!_session.ValidTiles.Contains(tile))
            {
                if (_session.InvalidReasons.TryGetValue(tile, out string reason)
                    && !string.IsNullOrWhiteSpace(reason))
                {
                }

                return;
            }

            _session.SelectedTile = tile;
            MovePreviewTo(tile);
            RefreshDeploymentOverlay();
            UpdateConfirmInteractable();
        }

        private void ConfirmSelectedTile()
        {
            if (_session == null
                || !_session.SelectedTile.HasValue
                || _confirmInProgress)
            {
                return;
            }

            DeploymentSession session = _session;
            Vector2Int target = _session.SelectedTile.Value;
            if (!TryRevalidateTarget(target, out string reason))
            {
                RefreshDeploymentTiles();
                return;
            }

            _confirmInProgress = true;
            UpdateConfirmInteractable();
            try
            {
                bool deployed = _recruitment.TryDeployReady(
                    session.OwnerId,
                    session.RecruitingBuildingPosition,
                    session.QueueId,
                    target,
                    out string unitId,
                    out reason);

                if (!deployed)
                {
                    RefreshDeploymentTiles();
                    return;
                }
                if (_session != null)
                    EndSession(destroyPreview: true);
            }
            finally
            {
                _confirmInProgress = false;
                UpdateConfirmInteractable();
            }
        }

        private void ClearSelectedTile(bool destroyPreview = true)
        {
            if (_session != null)
                _session.SelectedTile = null;

            if (destroyPreview && _previewObject != null)
            {
                Object.Destroy(_previewObject);
                _previewObject = null;
            }

            if (_session != null)
                RefreshDeploymentOverlay();

            UpdateConfirmInteractable();
        }

        private bool TryResolveTile(
            Vector2 screenPosition,
            out Vector2Int tile)
        {
            tile = default;

            if (_screenToGrid != null)
            {
                tile = _screenToGrid.ScreenToGrid(screenPosition);
                return true;
            }

            if (_pointerGridResolver != null
                && _pointerGridResolver.TryScreenToGrid(screenPosition, out tile))
            {
                return true;
            }

            UnityEngine.Camera camera = ResolveCamera();
            if (camera == null || _gridProjection == null)
                return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, ResolveWorldPosition(
                    _session?.RecruitingBuildingPosition ?? Vector2Int.zero,
                    layerOffset: 0f))
                : new Plane(Vector3.forward, Vector3.zero);

            if (!plane.Raycast(ray, out float distance) || distance < 0f)
                return false;

            Vector3 worldPoint = ray.GetPoint(distance);
            if (_pointerGridResolver != null
                && _pointerGridResolver.TryWorldToGrid(worldPoint, out tile))
            {
                return true;
            }

            tile = _gridProjection.WorldToGrid(worldPoint);
            return true;
        }
    }
}
