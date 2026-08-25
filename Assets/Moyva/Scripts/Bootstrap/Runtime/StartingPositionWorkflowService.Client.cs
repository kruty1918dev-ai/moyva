using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class StartingPositionWorkflowService
    {
        private void TryApplyStartLogic()
        {
            bool hasPendingWorldSignal = _workflowState.HasPendingWorldGeneratedSignal;
            WorldGeneratedDataSignal signal = hasPendingWorldSignal
                ? _workflowState.PendingWorldGeneratedSignal
                : default;
            bool autoLoad = GameLaunchContext.IsAutoLoadEnabled();
            int slot = GameLaunchContext.SaveSlot;
            bool hasSave = _saveService != null && _saveService.HasSave(slot);
            bool canRun = _policy.CanRunStartLogic();
            if (!hasPendingWorldSignal)
            {
                return;
            }

            if (_workflowState.StartLogicApplied)
            {
                return;
            }

            if (autoLoad && hasSave)
            {
                _autoloadRecoveryService.RepairLoadedFogIfNeeded(signal);
                Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
                _revealPresentationService.TeleportMainCamera(
                    _autoloadRecoveryService.ResolveStartupCameraTarget(baseMapSize.x, baseMapSize.y, preferStartTile: false),
                    signal);
                _workflowState.StartLogicApplied = true;
                return;
            }

            if (_spawnSetupService.TryPrepareStartingPositions(signal)
                && _workflowState.StartLogicApplied)
                return;

            if (!canRun)
            {
                return;
            }

            if (!_startingPositionState.IsSet)
            {
                return;
            }

            ApplyStartReveal(signal, teleportCamera: true);
        }

        private void ApplyStartReveal(WorldGeneratedDataSignal signal, bool teleportCamera)
        {
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Vector2Int revealCenter = _revealPresentationService.ResolveRevealCenter(baseMapSize.x, baseMapSize.y);
            bool revealChanged = !_workflowState.StartRevealApplied
                || _workflowState.AppliedStartRevealWidth != baseMapSize.x
                || _workflowState.AppliedStartRevealHeight != baseMapSize.y
                || _workflowState.AppliedStartRevealCenter != revealCenter;

            if (revealChanged)
            {
                _revealPresentationService.ApplyReveal(baseMapSize.x, baseMapSize.y, revealCenter);
                _workflowState.StartRevealApplied = true;
                _workflowState.AppliedStartRevealWidth = baseMapSize.x;
                _workflowState.AppliedStartRevealHeight = baseMapSize.y;
                _workflowState.AppliedStartRevealCenter = revealCenter;
            }

            if (teleportCamera && !_workflowState.StartupCameraTeleported)
            {
                _revealPresentationService.TeleportMainCamera(revealCenter, signal);
                _workflowState.StartupCameraTeleported = true;
            }

            _workflowState.StartLogicApplied = true;
        }

        private void ReapplyStartRevealIfNeeded(WorldGeneratedDataSignal signal)
        {
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Vector2Int revealCenter = _revealPresentationService.ResolveRevealCenter(baseMapSize.x, baseMapSize.y);
            bool revealCenterChanged = _workflowState.AppliedStartRevealCenter != revealCenter;
            if (_workflowState.StartRevealApplied
                && _workflowState.AppliedStartRevealWidth == baseMapSize.x
                && _workflowState.AppliedStartRevealHeight == baseMapSize.y
                && _workflowState.AppliedStartRevealCenter == revealCenter)
            {
                return;
            }

            ApplyStartReveal(signal, teleportCamera: !_workflowState.StartupCameraTeleported || revealCenterChanged);
        }
    }
}
