using System.Collections.Generic;
using ReactUnity.UGUI;
using ReactUnity.UGUI.Behaviours;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityHTML.Runtime
{
    [DefaultExecutionOrder(32000)]
    internal sealed class UnityHtmlTooltipLayer : MonoBehaviour
    {
        private readonly List<ReactElement> _elements = new();
        private readonly Vector3[] _corners = new Vector3[4];
        private RectTransform _root;
        private UGUIComponent _panel;
        private TextComponent _label;
        private UnityHtmlTooltipTarget _owner;
        private float _showAt;
        private string _text;
        private Vector2 _size;
        private float _width;
        private TMPro.TMP_FontAsset _font;

        public void Bind(UGUIContext context, RectTransform root)
        {
            _root = root;
            _panel = (UGUIComponent)context.CreateComponent("view", string.Empty);
            _panel.Id = "unityhtml-tooltip-layer";
            _panel.Style["position"] = "absolute";
            _panel.Style["backgroundColor"] = "#15171b";
            _panel.Style["borderColor"] = "#b99854";
            _panel.Style["borderWidth"] = 1;
            _panel.Style["pointerEvents"] = "none";
            _panel.SetParent(context.Host);
            _panel.Component.enabled = false;
            var canvas = _panel.RectTransform.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32000;
            _label = (TextComponent)context.CreateText("text", string.Empty);
            _label.SetParent((ReactUnity.IContainerComponent)_panel);
            if (context.Globals.TryGetValue("moyvaFont", out object font) && font is TMPro.TMP_FontAsset asset)
                _label.Text.font = asset;
            _label.Style["fontSize"] = 13;
            _label.Style["color"] = "#f6f3ea";
            _label.Style["textAlign"] = "left";
            _label.Component.enabled = false;
            _label.Text.richText = false;
            _panel.RectTransform.gameObject.SetActive(false);
        }

        public void RefreshTargets()
        {
            _root.GetComponentsInChildren(true, _elements);
            foreach (ReactElement element in _elements)
            {
                UGUIComponent component = element.Component;
                if (component?.Data == null || !component.Data.TryGetValue("tooltip", out object value)
                    || string.IsNullOrWhiteSpace(value as string)) continue;
                var target = element.GetComponent<UnityHtmlTooltipTarget>()
                    ?? element.gameObject.AddComponent<UnityHtmlTooltipTarget>();
                target.Layer = this;
                target.Source = component;
            }
            _panel.RectTransform.SetAsLastSibling();
            foreach (Graphic graphic in _panel.RectTransform.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        public void Show(UnityHtmlTooltipTarget owner)
        {
            _owner = owner;
            _showAt = Time.unscaledTime + 0.3f;
            _panel.RectTransform.gameObject.SetActive(false);
        }

        public void Hide(UnityHtmlTooltipTarget owner)
        {
            if (_owner != owner) return;
            _owner = null;
            if (_panel?.RectTransform != null) _panel.RectTransform.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_owner == null || !_owner.isActiveAndEnabled || _owner.Source.Destroyed)
            { Hide(_owner); return; }
            string text = _owner.Tooltip;
            if (string.IsNullOrWhiteSpace(text)) { Hide(_owner); return; }
            if (Time.unscaledTime < _showAt) return;
            _panel.RectTransform.gameObject.SetActive(true);
            Rect bounds = _root.rect;
            float maxWidth = Mathf.Max(40, Mathf.Min(300, bounds.width - 16));
            if (_text != text || _width != maxWidth || _font != _label.Text.font)
            {
                _text = text;
                _width = maxWidth;
                _font = _label.Text.font;
                _label.SetText(text);
                Vector2 preferred = _label.Text.GetPreferredValues(text, maxWidth - 20, Mathf.Infinity);
                _size = new Vector2(Mathf.Min(maxWidth, preferred.x + 20), preferred.y + 16);
            }
            Vector2 size = _size;
            _owner.Source.RectTransform.GetWorldCorners(_corners);
            Vector3 bottom = _root.InverseTransformPoint(_corners[0]);
            Vector3 top = _root.InverseTransformPoint(_corners[1]);
            float y = bottom.y - 8;
            if (y - size.y < bounds.yMin + 8) y = top.y + size.y + 8;
            var rect = _panel.RectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
            rect.pivot = Vector2.up;
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(
                Mathf.Clamp(bottom.x, bounds.xMin + 8, bounds.xMax - size.x - 8),
                Mathf.Min(y, bounds.yMax - 8)) - bounds.center;
            _label.RectTransform.anchorMin = Vector2.zero;
            _label.RectTransform.anchorMax = Vector2.one;
            _label.RectTransform.offsetMin = new Vector2(10, 8);
            _label.RectTransform.offsetMax = new Vector2(-10, -8);
        }
    }

    internal sealed class UnityHtmlTooltipTarget : MonoBehaviour, IPointerEnterHandler,
        IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        internal UnityHtmlTooltipLayer Layer;
        internal UGUIComponent Source;
        internal string Tooltip => Source?.Data != null && Source.Data.TryGetValue("tooltip", out object text)
            ? text as string : null;
        public void OnPointerEnter(PointerEventData data) => Layer?.Show(this);
        public void OnPointerExit(PointerEventData data) => Layer?.Hide(this);
        public void OnSelect(BaseEventData data) => Layer?.Show(this);
        public void OnDeselect(BaseEventData data) => Layer?.Hide(this);
        private void OnDisable() => Layer?.Hide(this);
    }
}
