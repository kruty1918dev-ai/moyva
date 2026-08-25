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
        private void EnsureControls()
        {
            if (_controlsRoot != null)
                return;

            _canvas = ResolveCanvas();
            if (_canvas == null)
            {
                return;
            }

            var root = new GameObject(
                ControlsRootName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(HorizontalLayoutGroup));
            _controlsRoot = root.GetComponent<RectTransform>();
            _controlsRoot.SetParent(_canvas.transform, false);
            _controlsRoot.anchorMin = new Vector2(0.5f, 0f);
            _controlsRoot.anchorMax = new Vector2(0.5f, 0f);
            _controlsRoot.pivot = new Vector2(0.5f, 0f);
            _controlsRoot.anchoredPosition = new Vector2(0f, 26f);
            _controlsRoot.sizeDelta = new Vector2(300f, 48f);

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.08f, 0.10f, 0.12f, 0.90f);
            background.raycastTarget = true;

            HorizontalLayoutGroup layout = root.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 7, 7);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            _confirmButton = CreateControlButton(
                _controlsRoot,
                "Confirm",
                "Підтвердити",
                () => ExecuteActionOrFallback(UiActionIds.Deployment.Confirm, UiActionSource.Button));
            _cancelButton = CreateControlButton(
                _controlsRoot,
                "Cancel",
                "Скасувати",
                () => ExecuteActionOrFallback(UiActionIds.Deployment.Cancel, UiActionSource.Button));

            _controlsRoot.SetAsLastSibling();
            SetControlsVisible(false);
        }

        private Button CreateControlButton(
            Transform parent,
            string name,
            string label,
            UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.22f, 0.96f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 138f;
            layoutElement.preferredHeight = 34f;

            var labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.SetParent(buttonObject.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            return button;
        }

        private Canvas ResolveCanvas()
        {
            Canvas canvas = _hudView != null
                ? _hudView.GetComponentInParent<Canvas>(true)
                : null;
            if (canvas != null)
                return canvas;

            return Object.FindFirstObjectByType<Canvas>(
                FindObjectsInactive.Include);
        }

        private void SetControlsVisible(bool visible)
        {
            if (_controlsRoot != null)
                _controlsRoot.gameObject.SetActive(visible);
        }

    }
}
