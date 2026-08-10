using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Shared.UI
{
    public readonly struct UiTooltipRequest
    {
        public UiTooltipRequest(
            object owner,
            string text,
            Vector2 screenPosition,
            RectTransform anchor = null,
            bool immediate = false)
        {
            Owner = owner;
            Text = text;
            ScreenPosition = screenPosition;
            Anchor = anchor;
            Immediate = immediate;
        }

        public object Owner { get; }
        public string Text { get; }
        public Vector2 ScreenPosition { get; }
        public RectTransform Anchor { get; }
        public bool Immediate { get; }
    }

    public interface IUiTooltipService
    {
        void Request(UiTooltipRequest request);
        void Hide(object owner);
        void Clear();
    }

    public interface IUiMotionService
    {
        bool ReducedMotion { get; set; }

        void SetPanelVisible(
            CanvasGroup canvasGroup,
            RectTransform panel,
            bool visible,
            float duration = 0.14f,
            Vector2? hiddenOffset = null);

        void Cancel(object target);
    }

    internal sealed class UiMotionService : IUiMotionService, ITickable
    {
        private readonly List<Motion> _motions = new List<Motion>();

        public bool ReducedMotion { get; set; }

        public void SetPanelVisible(
            CanvasGroup canvasGroup,
            RectTransform panel,
            bool visible,
            float duration = 0.14f,
            Vector2? hiddenOffset = null)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));

            Cancel(canvasGroup);
            Vector2 offset = hiddenOffset ?? new Vector2(0f, -8f);
            Vector2 shownPosition = panel != null ? panel.anchoredPosition : Vector2.zero;
            if (!visible && canvasGroup.gameObject.activeSelf == false)
                return;

            if (visible)
                canvasGroup.gameObject.SetActive(true);

            float effectiveDuration = ReducedMotion ? 0f : Mathf.Max(0f, duration);
            if (effectiveDuration <= 0f)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
                if (panel != null)
                    panel.anchoredPosition = shownPosition;
                if (!visible)
                    canvasGroup.gameObject.SetActive(false);
                return;
            }

            _motions.Add(new Motion
            {
                CanvasGroup = canvasGroup,
                Panel = panel,
                Elapsed = 0f,
                Duration = effectiveDuration,
                StartAlpha = canvasGroup.alpha,
                EndAlpha = visible ? 1f : 0f,
                StartPosition = panel != null
                    ? (visible ? shownPosition + offset : shownPosition)
                    : Vector2.zero,
                EndPosition = panel != null
                    ? (visible ? shownPosition : shownPosition + offset)
                    : Vector2.zero,
                VisibleAtEnd = visible,
            });

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            if (panel != null && visible)
                panel.anchoredPosition = shownPosition + offset;
        }

        public void Cancel(object target)
        {
            for (int index = _motions.Count - 1; index >= 0; index--)
            {
                Motion motion = _motions[index];
                if (ReferenceEquals(motion.CanvasGroup, target) || ReferenceEquals(motion.Panel, target))
                    _motions.RemoveAt(index);
            }
        }

        public void Tick()
        {
            float deltaTime = Time.unscaledDeltaTime;
            for (int index = _motions.Count - 1; index >= 0; index--)
            {
                Motion motion = _motions[index];
                if (motion.CanvasGroup == null)
                {
                    _motions.RemoveAt(index);
                    continue;
                }

                motion.Elapsed += deltaTime;
                float normalized = Mathf.Clamp01(motion.Elapsed / motion.Duration);
                float eased = normalized * normalized * (3f - 2f * normalized);
                motion.CanvasGroup.alpha = Mathf.LerpUnclamped(motion.StartAlpha, motion.EndAlpha, eased);
                if (motion.Panel != null)
                    motion.Panel.anchoredPosition = Vector2.LerpUnclamped(motion.StartPosition, motion.EndPosition, eased);

                if (normalized < 1f)
                    continue;

                motion.CanvasGroup.interactable = motion.VisibleAtEnd;
                motion.CanvasGroup.blocksRaycasts = motion.VisibleAtEnd;
                if (!motion.VisibleAtEnd)
                    motion.CanvasGroup.gameObject.SetActive(false);
                _motions.RemoveAt(index);
            }
        }

        private sealed class Motion
        {
            public CanvasGroup CanvasGroup;
            public RectTransform Panel;
            public float Elapsed;
            public float Duration;
            public float StartAlpha;
            public float EndAlpha;
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public bool VisibleAtEnd;
        }
    }

    internal sealed class UiTooltipService : IUiTooltipService, ITickable, IDisposable
    {
        private const float DefaultDelay = 0.42f;

        private UiTooltipRequest _pending;
        private object _visibleOwner;
        private float _showAt;
        private bool _hasPending;
        private UiTooltipPresenter _presenter;

        public void Request(UiTooltipRequest request)
        {
            if (request.Owner == null || string.IsNullOrWhiteSpace(request.Text))
                return;

            bool replacingVisible = _visibleOwner != null && !ReferenceEquals(_visibleOwner, request.Owner);
            _pending = request;
            _hasPending = true;
            _showAt = Time.unscaledTime + (request.Immediate || replacingVisible ? 0f : DefaultDelay);
        }

        public void Hide(object owner)
        {
            if (owner == null)
                return;
            if (_hasPending && ReferenceEquals(_pending.Owner, owner))
                _hasPending = false;
            if (!ReferenceEquals(_visibleOwner, owner))
                return;

            _visibleOwner = null;
            _presenter?.Hide();
        }

        public void Clear()
        {
            _hasPending = false;
            _visibleOwner = null;
            _presenter?.Hide();
        }

        public void Tick()
        {
            if (!_hasPending || Time.unscaledTime < _showAt)
                return;

            _hasPending = false;
            _presenter ??= UiTooltipPresenter.Create();
            if (_presenter == null)
                return;

            _visibleOwner = _pending.Owner;
            _presenter.Show(_pending.Text, ResolveScreenPosition(_pending));
        }

        public void Dispose()
        {
            if (_presenter != null)
                UnityEngine.Object.Destroy(_presenter.gameObject);
            _presenter = null;
        }

        private static Vector2 ResolveScreenPosition(UiTooltipRequest request)
        {
            if (request.Anchor == null)
                return request.ScreenPosition;

            var corners = new Vector3[4];
            request.Anchor.GetWorldCorners(corners);
            Canvas canvas = request.Anchor.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
            return topRight + new Vector2(10f, 8f);
        }
    }

    [DisallowMultipleComponent]
    public sealed class UiTooltipTrigger : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler
    {
        [SerializeField, TextArea] private string _text;
        [SerializeField] private RectTransform _anchor;

        private IUiTooltipService _service;

        [Inject]
        public void Construct(IUiTooltipService service)
        {
            _service = service;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ResolveService()?.Request(new UiTooltipRequest(
                this,
                _text,
                eventData.position,
                _anchor != null ? _anchor : transform as RectTransform));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ResolveService()?.Hide(this);
        }

        public void OnSelect(BaseEventData eventData)
        {
            ResolveService()?.Request(new UiTooltipRequest(
                this,
                _text,
                Vector2.zero,
                _anchor != null ? _anchor : transform as RectTransform,
                immediate: true));
        }

        public void OnDeselect(BaseEventData eventData)
        {
            ResolveService()?.Hide(this);
        }

        private IUiTooltipService ResolveService()
        {
            if (_service != null)
                return _service;
            if (ProjectContext.Instance != null)
                _service = ProjectContext.Instance.Container.TryResolve<IUiTooltipService>();
            return _service;
        }
    }

    internal sealed class UiTooltipPresenter : MonoBehaviour
    {
        private const float MaxWidth = 360f;
        private RectTransform _rect;
        private TMP_Text _label;

        public static UiTooltipPresenter Create()
        {
            TMP_FontAsset font = ResolveFont();
            if (font == null)
            {
                Debug.LogError("[MoyvaUI] Cannot create runtime tooltip: no TMP default font asset is configured.");
                return null;
            }

            var canvasObject = new GameObject(
                "TooltipCanvas (Runtime)",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvasObject.GetComponent<GraphicRaycaster>().enabled = false;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("Tooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ContentSizeFitter), typeof(UiTooltipPresenter));
            panel.transform.SetParent(canvasObject.transform, false);
            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.105f, 0.09f, 0.075f, 0.97f);
            image.raycastTarget = false;
            ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(panel.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12f, 8f);
            labelRect.offsetMax = new Vector2(-12f, -8f);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = 14f;
            label.color = new Color(0.96f, 0.91f, 0.82f);
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;

            UiTooltipPresenter presenter = panel.GetComponent<UiTooltipPresenter>();
            presenter._rect = (RectTransform)panel.transform;
            presenter._label = label;
            panel.SetActive(false);
            return presenter;
        }

        public void Show(string text, Vector2 screenPosition)
        {
            gameObject.SetActive(true);
            _label.text = text;
            _label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWidth - 24f);
            Canvas.ForceUpdateCanvases();
            float width = Mathf.Min(MaxWidth, _label.preferredWidth + 24f);
            float height = _label.GetPreferredValues(text, width - 24f, 0f).y + 16f;
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            float x = Mathf.Clamp(screenPosition.x, 8f, Screen.width - width - 8f);
            float y = Mathf.Clamp(screenPosition.y, 8f, Screen.height - height - 8f);
            _rect.position = new Vector3(x, y, 0f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static TMP_FontAsset ResolveFont()
        {
            if (TMP_Settings.defaultFontAsset != null)
                return TMP_Settings.defaultFontAsset;

            TMP_FontAsset[] loaded = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            return loaded.Length > 0 ? loaded[0] : null;
        }
    }
}
