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
        public IReadOnlyCollection<UiActionId> ActionIds { get; } =
            new[]
            {
                UiActionIds.Deployment.Cancel,
                UiActionIds.Deployment.Confirm,
            };

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.Deployment.Cancel)
            {
                if (_session == null)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                CancelSession();
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Deployment.Confirm)
            {
                if (_session == null)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                if (!_session.SelectedTile.HasValue)
                    return UiActionResult.Rejected(UiActionReason.NoSelection);
                ConfirmSelectedTile();
                return UiActionResult.Performed();
            }

            return UiActionResult.Ignored(UiActionReason.ActionUnavailable);
        }

        private void RegisterUiContexts()
        {
            if (_uiContexts == null)
                return;

            _deploymentContext = _uiContexts.Push(new UiContextRegistration(
                "DeploymentMode",
                UiContextLayer.Mode,
                40,
                () => _session != null,
                UiActionIds.Deployment.Cancel,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[]
                {
                    UiActionIds.Deployment.Cancel,
                    UiActionIds.Deployment.Confirm,
                }));
        }

        private void ExecuteActionOrFallback(
            UiActionId actionId,
            UiActionSource source)
        {
            Execute(new UiActionRequest(actionId, source, "DeploymentMode"));
        }

        private bool IsPointerOverUi(Vector2 screenPosition)
        {
            if (_inputPolicy != null)
                return _inputPolicy.IsPointerOverUi(screenPosition, interactiveOnly: true);

            return EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }

        private UnityEngine.Camera ResolveCamera()
        {
            if (_camera != null && _camera.isActiveAndEnabled)
                return _camera;

            _camera = UnityEngine.Camera.main;
            if (_camera == null && !_warnedMissingCamera)
            {
                _warnedMissingCamera = true;
            }

            return _camera;
        }

    }
}
