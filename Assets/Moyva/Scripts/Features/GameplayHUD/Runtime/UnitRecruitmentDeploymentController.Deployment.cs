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
        private void RefreshDeploymentOverlay()
        {
            if (_session == null)
                return;

            _overlayCells.Clear();
            for (int index = 0; index < _session.Tiles.Count; index++)
            {
                UnitRecruitmentDeploymentTileSnapshot tile = _session.Tiles[index];
                if (!tile.IsValid && _grid != null && !_grid.ContainsCell(tile.Position))
                    continue;

                GridActionOverlayVisualState state =
                    _session.SelectedTile.HasValue
                    && _session.SelectedTile.Value == tile.Position
                        ? GridActionOverlayVisualState.Selected
                        : tile.IsValid
                            ? GridActionOverlayVisualState.Valid
                            : GridActionOverlayVisualState.Invalid;
                _overlayCells.Add(new GridActionOverlayCell(
                    tile.Position,
                    state,
                    tile.Reason));
            }

            if (_session.OverlayAcquired)
                _gridOverlay?.Show(GridActionOverlayOwner.Deployment, _overlayCells);
        }

        private void UpdateConfirmInteractable()
        {
            if (_confirmButton != null)
            {
                _confirmButton.interactable =
                    _session != null
                    && _session.SelectedTile.HasValue
                    && !_confirmInProgress;
            }

            if (_cancelButton != null)
                _cancelButton.interactable = _session != null;
        }

        private void OnRecruitmentDeployed(
            UnitRecruitmentDeployedSignal signal)
        {
            if (_session == null
                || !_session.Matches(signal.OwnerId, signal.QueueId))
            {
                return;
            }

            EndSession(destroyPreview: true);
        }
    }
}
