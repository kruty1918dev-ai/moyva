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
        private void OnReadyIndicatorClicked(
            UnitRecruitmentReadyIndicatorClickedSignal signal)
        {
            string owner = NormalizeId(signal.OwnerId);
            if (owner == null || signal.QueueId < 1)
            {
                Debug.LogWarning($"{LogTag} Ignored ready indicator with invalid identity.");
                return;
            }

            string localOwner = NormalizeId(_turns.LocalOwnerId);
            if (localOwner != null
                && !string.Equals(owner, localOwner, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"{LogTag} Ignored ready indicator for non-local owner '{owner}'.");
                return;
            }

            if (!_turns.CanOwnerAct(owner, out string reason))
            {
                Debug.LogWarning($"{LogTag} Cannot enter deployment: {reason}");
                return;
            }

            if (_gameModeService != null
                && _gameModeService.CurrentMode != GameModeType.Normal)
            {
                Debug.LogWarning(
                    $"{LogTag} Cannot enter deployment while game mode is {_gameModeService.CurrentMode}.");
                return;
            }

            if (!_recruitment.TryPeekReady(
                    owner,
                    signal.RecruitingBuildingPosition,
                    out UnitRecruitmentQueueItemSnapshot ready)
                || ready.QueueId != signal.QueueId)
            {
                Debug.LogWarning(
                    $"{LogTag} Ready job {signal.QueueId} is no longer the queue head.");
                return;
            }

            if (_session != null)
            {
                if (_session.Matches(owner, signal.QueueId))
                    return;

                CancelSession();
            }

            BeginSession(
                owner,
                ready.QueueId,
                ready.UnitTypeId,
                ready.RecruitingBuildingPosition);
        }

        private void BeginSession(
            string ownerId,
            long queueId,
            string unitTypeId,
            Vector2Int recruitingBuildingPosition)
        {
            var session = new DeploymentSession(
                ownerId,
                queueId,
                unitTypeId,
                recruitingBuildingPosition);

            _session = session;
            _session.InputBlock = _inputPolicy?.AcquireBlock(
                DeploymentInputMask,
                this);
            _session.OverlayAcquired =
                _gridOverlay?.Acquire(GridActionOverlayOwner.Deployment) == true;
            if (!_session.OverlayAcquired)
            {
                Debug.LogWarning(
                    $"{LogTag} Shared grid overlay is unavailable or owned by another mode; deployment remains functional without tile highlights.");
            }

            EnsureWorldRoots();
            EnsureControls();
            RefreshDeploymentTiles();
            SetControlsVisible(true);
            UpdateConfirmInteractable();

            Debug.Log(
                $"{LogTag} Deployment session started queue={queueId} unit={unitTypeId} building={recruitingBuildingPosition}.");
        }



        private void HandleKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
        }

        private void HandleMouse()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || _session == null)
                return;

            if (mouse.rightButton.wasPressedThisFrame)
            {
                ExecuteActionOrFallback(UiActionIds.Deployment.Cancel, UiActionSource.Programmatic);
                return;
            }

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            Vector2 screenPosition = mouse.position.ReadValue();
            if (IsPointerOverUi(screenPosition))
                return;

            if (!TryResolveTile(screenPosition, out Vector2Int tile))
                return;

            SelectTile(tile);
        }



        private bool TryRevalidateTarget(
            Vector2Int target,
            out string reason)
        {
            reason = null;
            if (_session == null)
                return false;

            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles =
                _recruitment.GetDeploymentTiles(
                    _session.OwnerId,
                    _session.RecruitingBuildingPosition,
                    _session.QueueId);

            for (int index = 0; index < tiles.Count; index++)
            {
                UnitRecruitmentDeploymentTileSnapshot tile = tiles[index];
                if (tile.Position != target)
                    continue;

                reason = tile.Reason;
                return tile.IsValid;
            }

            reason = "Selected tile is no longer a deployment candidate.";
            return false;
        }

        private void CancelSession()
        {
            if (_session == null)
                return;

            Debug.Log(
                $"{LogTag} Deployment session cancelled queue={_session.QueueId}.");
            EndSession(destroyPreview: true);
        }

        private void EndSession(bool destroyPreview)
        {
            DeploymentSession session = _session;
            session?.InputBlock?.Dispose();
            if (session?.OverlayAcquired == true)
                _gridOverlay?.Release(GridActionOverlayOwner.Deployment);
            _session = null;
            _confirmInProgress = false;
            ClearSelectedTile(destroyPreview);
            SetControlsVisible(false);
        }


    }
}
