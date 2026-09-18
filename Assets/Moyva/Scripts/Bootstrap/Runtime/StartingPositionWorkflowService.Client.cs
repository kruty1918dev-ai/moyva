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
                Debug.Log($"{StartingPositionInitializer.DebugTag} Start logic skipped: no pending WorldGenerated signal.");
                return;
            }

            if (_workflowState.StartLogicApplied)
            {
                Debug.Log($"{StartingPositionInitializer.DebugTag} Start logic skipped: already applied.");
                return;
            }

            if (autoLoad && hasSave)
            {
                Debug.Log($"{StartingPositionInitializer.DebugTag} Applying autoload fog/camera recovery. slot={slot}.");
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
            {
                Debug.Log($"{StartingPositionInitializer.DebugTag} Start logic completed while preparing host start positions.");
                return;
            }

            if (!canRun)
            {
                Debug.LogWarning($"{StartingPositionInitializer.DebugTag} Start logic blocked by policy. " +
                                 $"mode={GameLaunchContext.Mode}, source={GameLaunchContext.Source}, " +
                                 $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                                 $"launchHasRole={GameLaunchContext.HasLocalPlayerRole}, launchHost={GameLaunchContext.IsLocalPlayerHost}, " +
                                 $"launchLocal='{GameLaunchContext.LocalPlayerId}'.");
                return;
            }

            if (!_startingPositionState.IsSet)
            {
                Debug.LogWarning($"{StartingPositionInitializer.DebugTag} Start logic waiting for start positions. " +
                                 $"mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, " +
                                 $"launchHost={GameLaunchContext.IsLocalPlayerHost}, launchLocal='{GameLaunchContext.LocalPlayerId}'.");
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
                Debug.Log($"{StartingPositionInitializer.DebugTag} Applying start reveal. " +
                          $"map={baseMapSize.x}x{baseMapSize.y}, center={revealCenter}, teleportCamera={teleportCamera}.");
                _revealPresentationService.ApplyReveal(baseMapSize.x, baseMapSize.y, revealCenter);
                _workflowState.StartRevealApplied = true;
                _workflowState.AppliedStartRevealWidth = baseMapSize.x;
                _workflowState.AppliedStartRevealHeight = baseMapSize.y;
                _workflowState.AppliedStartRevealCenter = revealCenter;
            }

            if (teleportCamera && !_workflowState.StartupCameraTeleported)
            {
                Debug.Log($"{StartingPositionInitializer.DebugTag} Teleporting startup camera to {revealCenter}.");
                _revealPresentationService.TeleportMainCamera(revealCenter, signal);
                _workflowState.StartupCameraTeleported = true;
            }

            _workflowState.StartLogicApplied = true;
            Debug.Log($"{StartingPositionInitializer.DebugTag} Start logic applied.");
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
