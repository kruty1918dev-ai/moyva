using System;
using System.Collections.Generic;
using System.Globalization;
using ReactUnity;
using ReactUnity.Helpers;
using ReactUnity.Scheduling;
using ReactUnity.Scripting;
using ReactUnity.Styling;
using ReactUnity.Styling.Rules;
using ReactUnity.UGUI;
using ReactUnity.UGUI.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityHTML.Runtime
{
    public sealed class UnityHtmlHost : IUnityHtmlHost
    {
        private UGUIContext _context;
        private RectTransform _root;
        private string _mountedCss = string.Empty;
        private readonly UnityHtmlMotionBridge _motion = new UnityHtmlMotionBridge();
        private UnityHtmlDocumentTree _tree;
        private UnityHtmlTooltipLayer _tooltips;

        public IUnityHtmlMotion Motion => _motion;

        public UnityHtmlMountResult Mount(
            RectTransform root,
            UnityHtmlDocument document,
            IReadOnlyDictionary<string, object> globals = null)
        {
            if (root == null)
                return UnityHtmlMountResult.Failure("Mount root is not assigned.");

            if (!document.HasHtml)
                return UnityHtmlMountResult.Failure($"HTML document '{document.SourceName}' is empty or missing.");

            if (CanUpdateMountedDocument(root, document))
                return UpdateMountedDocument(document, globals);

            Unmount();
            ClearDetachedEditorElements();

            try
            {
                RegisterMoyvaComponents();
                _root = root;
                _mountedCss = document.Css ?? string.Empty;
                ClearRootChildren(_root);
                var globalRecord = CreateGlobals(globals);
                var source = ScriptSource.Text(string.Empty, ScriptSourceLanguage.Html);

                _context = new UGUIContext(new UGUIContext.Options
                {
                    HostElement = root,
                    Globals = globalRecord,
                    Source = source,
                    Timer = UnscaledTimer.Instance,
                    MediaProvider = DefaultMediaProvider.CreateMediaProvider(document.SourceName, "ugui", false),
                    EngineType = ResolveEngineType(),
                    Pooling = ReactContext.PoolingType.Basic,
                    UnknownPropertyHandling = ReactContext.UnknownPropertyHandling.Exception
                });
                _motion.Attach(_root);
                DetachUnsafeEditorAssemblyReloadDispose(_context);

                if (!string.IsNullOrWhiteSpace(document.Css))
                    _context.InsertStyle(document.Css);

                _context.Start();
                _tree = new UnityHtmlDocumentTree(_context);
                _tree.Update(document.Html);
                DetachUnsafeEditorAssemblyReloadDispose(_context);
                CompleteLayoutPass();
                _motion.ApplyDeclaredMotions();
                return UnityHtmlMountResult.Success();
            }
            catch (Exception exception)
            {
                Unmount();
                ClearDetachedEditorElements();
                return UnityHtmlMountResult.Failure($"{document.SourceName}: {exception.GetBaseException().Message}");
            }
        }

        public void Unmount()
        {
            var context = _context;
            var root = _root;
            _context = null;
            _root = null;
            _mountedCss = string.Empty;
            _tree = null;
            if (_tooltips != null)
            {
                _tooltips.enabled = false;
                if (ShouldDestroyDeferred()) UnityEngine.Object.Destroy(_tooltips);
                else UnityEngine.Object.DestroyImmediate(_tooltips);
                _tooltips = null;
            }
            _motion.Detach();

            try
            {
                DisposeContext(context);
            }
            finally
            {
                ClearRootChildren(root);
            }
        }

        public void Dispose() => Unmount();
        public bool UpdateRegion(string elementId, string html)
        {
            if (_tree == null || !_tree.UpdateRegion(elementId, html)) return false;
            CompleteLayoutPass();
            _motion.ApplyDeclaredMotions();
            return true;
        }
        public bool SetValue(string elementId, string value) => _tree?.SetValue(elementId, value) == true;

        private bool CanUpdateMountedDocument(RectTransform root, UnityHtmlDocument document)
        {
            return _context != null &&
                   !_context.IsDisposed &&
                   _root == root &&
                   string.Equals(_mountedCss, document.Css ?? string.Empty, StringComparison.Ordinal);
        }

        private UnityHtmlMountResult UpdateMountedDocument(
            UnityHtmlDocument document,
            IReadOnlyDictionary<string, object> globals)
        {
            try
            {
                UpdateGlobals(globals);
                if (_tree.Update(document.Html))
                {
                    CompleteLayoutPass();
                    _motion.ApplyDeclaredMotions();
                }
                return UnityHtmlMountResult.Success();
            }
            catch (Exception exception)
            {
                Unmount();
                ClearDetachedEditorElements();
                return UnityHtmlMountResult.Failure($"{document.SourceName}: {exception.GetBaseException().Message}");
            }
        }

        private void UpdateGlobals(IReadOnlyDictionary<string, object> globals)
        {
            if (_context?.Globals == null)
                return;

            if (globals != null)
            {
                foreach (var pair in globals)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        continue;

                    if (!_context.Globals.TryGetValue(pair.Key, out object previous)
                        || !Equals(previous, pair.Value))
                        _context.Globals[pair.Key] = pair.Value;
                }
            }

            if (!_context.Globals.TryGetValue("motion", out object motion) || !ReferenceEquals(motion, _motion))
                _context.Globals["motion"] = _motion;
        }

        private void CompleteLayoutPass()
        {
            if (Application.isPlaying && _tooltips == null)
            {
                _tooltips = _root.gameObject.AddComponent<UnityHtmlTooltipLayer>();
                _tooltips.Bind(_context, _root);
            }
            _context.UpdateElementsRecursively();
            _context.CalculateLayoutRecursively();
            _context.LateUpdateElementsRecursively();
            FlushReactElementLayout(_root);
            ConfigureRenderedInputs(_root);
            Canvas.ForceUpdateCanvases();
            _tooltips?.RefreshTargets();
        }

        private GlobalRecord CreateGlobals(IReadOnlyDictionary<string, object> globals)
        {
            var record = new GlobalRecord();
            if (globals != null)
            {
                foreach (var pair in globals)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        continue;

                    record[pair.Key] = pair.Value;
                }
            }

            record["motion"] = _motion;
            return record;
        }

        private static void ClearRootChildren(RectTransform root)
        {
            if (root == null)
                return;

            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child == null)
                    continue;

                var childObject = child.gameObject;
                if (ShouldDestroyDeferred())
                    UnityEngine.Object.Destroy(childObject);
                else
                    UnityEngine.Object.DestroyImmediate(childObject);
            }
        }

        private static bool ShouldDestroyDeferred()
        {
#if UNITY_EDITOR
            return Application.isPlaying && UnityEditor.EditorApplication.isPlaying;
#else
            return Application.isPlaying;
#endif
        }

        private static void ClearDetachedEditorElements()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            var elements = UnityEngine.Object.FindObjectsByType<ReactElement>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                if (element == null || element.transform.parent != null)
                    continue;

                UnityEngine.Object.DestroyImmediate(element.gameObject);
            }
#endif
        }

        private static JavascriptEngineType ResolveEngineType()
        {
#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
#pragma warning disable CS0612
            return JavascriptEngineType.Jint;
#pragma warning restore CS0612
#else
            return JavascriptEngineType.QuickJS;
#endif
        }

        private static void FlushReactElementLayout(RectTransform root)
        {
            if (root == null)
                return;

            var elements = root.GetComponentsInChildren<ReactElement>(true);
            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var layout = element != null ? element.Layout : null;
                var transform = element != null ? element.transform as RectTransform : null;
                if (layout == null || !layout.HasNewLayout || !element.enabled || transform == null || float.IsNaN(layout.LayoutWidth))
                    continue;

                var pivotDiff = transform.pivot - Vector2.up;
                transform.anchoredPosition = new Vector2(
                    layout.LayoutLeft + pivotDiff.x * layout.LayoutWidth,
                    -layout.LayoutTop + pivotDiff.y * layout.LayoutHeight);
                transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layout.LayoutWidth);
                transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.LayoutHeight);
                layout.MarkLayoutSeen();
            }
        }

        private static void RegisterMoyvaComponents()
        {
            UGUIContext.ComponentCreators["input"] = (_, text, context) => new UnityHtmlInputComponent(text, context);
            if (!UGUIContext.ComponentCreators.ContainsKey("slider"))
                UGUIContext.ComponentCreators["slider"] = (_, _, context) => new UnityHtmlSliderComponent(context);

            if (!UGUIContext.ComponentCreators.ContainsKey("select"))
                UGUIContext.ComponentCreators["select"] = (_, _, context) => new UnityHtmlSelectComponent(context);
        }

        private static void ConfigureRenderedInputs(RectTransform root)
        {
            if (root == null)
                return;

            var inputs = root.GetComponentsInChildren<TMP_InputField>(true);
            for (var i = 0; i < inputs.Length; i++)
            {
                var input = inputs[i];
                if (input == null)
                    continue;
                var component = input.GetComponent<ReactElement>()?.Component as UnityHtmlInputComponent;
                if (component != null && component.NativeLayoutConfigured) continue;
                if (component != null) component.NativeLayoutConfigured = true;

                input.customCaretColor = true;
                input.caretColor = new Color(0.98f, 0.91f, 0.56f, 1f);
                input.selectionColor = new Color(0.86f, 0.68f, 0.23f, 0.35f);
                input.lineType = TMP_InputField.LineType.SingleLine;
                input.caretWidth = 2;
                input.resetOnDeActivation = false;

                var bubbling = input.GetComponent<ScrollEventBubbling>();
                if (bubbling != null)
                    bubbling.Bubble = false;

                ConfigureInputViewport(input);

                if (input.textComponent != null)
                {
                    input.textComponent.color = new Color(0.98f, 0.96f, 0.89f, 1f);
                    input.textComponent.alignment = TextAlignmentOptions.Center;
                    input.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
                    input.textComponent.overflowMode = TextOverflowModes.Masking;
                    input.textComponent.margin = Vector4.zero;
                    input.textComponent.raycastTarget = false;
                    input.textComponent.ForceMeshUpdate(true);
                }

                if (input.placeholder is TMP_Text placeholder)
                {
                    placeholder.color = new Color(0.72f, 0.70f, 0.64f, 0.78f);
                    placeholder.alignment = TextAlignmentOptions.Center;
                    placeholder.textWrappingMode = TextWrappingModes.NoWrap;
                    placeholder.margin = Vector4.zero;
                    placeholder.raycastTarget = false;
                    placeholder.ForceMeshUpdate(true);
                }

                var graphic = input.targetGraphic != null ? input.targetGraphic : input.GetComponent<Graphic>();
                if (graphic == null)
                {
                    var image = input.gameObject.AddComponent<Image>();
                    image.color = new Color(0f, 0f, 0f, 0.001f);
                    graphic = image;
                }

                graphic.raycastTarget = true;
                input.targetGraphic = graphic;
            }
        }

        private static void ConfigureInputViewport(TMP_InputField input)
        {
            var viewport = input.textViewport;
            if (viewport != null)
            {
                viewport.anchorMin = Vector2.zero;
                viewport.anchorMax = Vector2.one;
                viewport.pivot = new Vector2(0.5f, 0.5f);
                viewport.offsetMin = new Vector2(10f, 0f);
                viewport.offsetMax = new Vector2(-10f, 0f);
                viewport.localScale = Vector3.one;
            }

            StretchTextRect(input.textComponent != null ? input.textComponent.rectTransform : null);
            if (input.placeholder is TMP_Text placeholder)
                StretchTextRect(placeholder.rectTransform);
        }

        private static void StretchTextRect(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void DisposeContext(UGUIContext context)
        {
            if (context == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DetachUnsafeEditorAssemblyReloadDispose(context);

                try
                {
                    context.Dispatcher?.Dispose();
                }
                catch
                {
                }

                try
                {
                    context.Globals?.Dispose();
                }
                catch
                {
                }

                try
                {
                    foreach (var disposable in context.Disposables)
                        disposable?.Invoke();
                    context.Disposables.Clear();
                }
                catch
                {
                }

                try
                {
                    context.Script?.Dispose();
                }
                catch
                {
                }

                return;
            }
#endif

            context.Dispose();
        }

        private static void DetachUnsafeEditorAssemblyReloadDispose(UGUIContext context)
        {
#if UNITY_EDITOR
            if (context != null && !Application.isPlaying)
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= context.Dispose;
#endif
        }
    }

    internal sealed class UnityHtmlSliderComponent : UGUIComponent, IActivatableComponent
    {
        private readonly Image _backgroundImage;
        private readonly Image _fillImage;
        private readonly Image _handleImage;
        private readonly TMP_Text _valueText;
        private readonly SliderInteractionGuard _interactionGuard;
        private string _format = "decimal1";
        private string _suffix = string.Empty;

        public UnityHtmlSliderComponent(UGUIContext context) : base(context, "slider")
        {
            var raycastGraphic = AddComponent<Image>();
            raycastGraphic.color = new Color(0f, 0f, 0f, 0.001f);
            raycastGraphic.raycastTarget = true;

            Slider = AddComponent<Slider>();
            Slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            Slider.transition = Selectable.Transition.ColorTint;
            Slider.navigation = new Navigation { mode = Navigation.Mode.None };

            CreateTrack(context, out _backgroundImage, out var fillRect, out _fillImage, out var handleRect, out _handleImage, out _valueText);
            Slider.fillRect = fillRect;
            Slider.handleRect = handleRect;
            Slider.targetGraphic = _handleImage;
            Slider.minValue = 0f;
            Slider.maxValue = 1f;
            Slider.value = 0f;

            _interactionGuard = AddComponent<SliderInteractionGuard>();
            _interactionGuard.Initialize(Slider);
            Slider.onValueChanged.AddListener(UpdateValueText);
            UpdateValueText(Slider.value);
        }

        public Slider Slider { get; }

        public bool Disabled
        {
            get => !Slider.interactable;
            set => Slider.interactable = !value;
        }

        public void Activate()
        {
            Slider.Select();
        }

        public override Action AddEventListener(string eventName, Callback callback)
        {
            switch (eventName)
            {
                case "onChange":
                case "onValueChanged":
                    var listener = new UnityEngine.Events.UnityAction<float>(value => callback.CallWithPriority(EventPriority.Continuous, value, this));
                    Slider.onValueChanged.AddListener(listener);
                    return () => Slider.onValueChanged.RemoveListener(listener);
                case "onBeginChange":
                    Action<float> beginListener = value => callback.CallWithPriority(EventPriority.Discrete, value, this);
                    _interactionGuard.BeginChange += beginListener;
                    return () => _interactionGuard.BeginChange -= beginListener;
                case "onEndChange":
                    Action<float> endListener = value => callback.CallWithPriority(EventPriority.Discrete, value, this);
                    _interactionGuard.EndChange += endListener;
                    return () => _interactionGuard.EndChange -= endListener;
                default:
                    return base.AddEventListener(eventName, callback);
            }
        }

        public override void SetProperty(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "value":
                    Slider.SetValueWithoutNotify(ToSingle(value, Slider.value));
                    UpdateValueText(Slider.value);
                    return;
                case "min":
                case "minValue":
                    Slider.minValue = ToSingle(value, Slider.minValue);
                    UpdateValueText(Slider.value);
                    return;
                case "max":
                case "maxValue":
                    Slider.maxValue = ToSingle(value, Slider.maxValue);
                    UpdateValueText(Slider.value);
                    return;
                case "wholeNumbers":
                    Slider.wholeNumbers = Convert.ToBoolean(value);
                    UpdateValueText(Slider.value);
                    return;
                case "format":
                    _format = value?.ToString()?.Trim().ToLowerInvariant() ?? "decimal1";
                    UpdateValueText(Slider.value);
                    return;
                case "suffix":
                    _suffix = value?.ToString() ?? string.Empty;
                    UpdateValueText(Slider.value);
                    return;
                case "disabled":
                    Disabled = Convert.ToBoolean(value);
                    return;
                default:
                    base.SetProperty(propertyName, value);
                    return;
            }
        }

        protected override void ApplyStylesSelf()
        {
            base.ApplyStylesSelf();
            _backgroundImage.color = new Color(0.10f, 0.11f, 0.16f, 1f);
            _fillImage.color = new Color(0.86f, 0.68f, 0.23f, 1f);
            _handleImage.color = Disabled
                ? new Color(0.46f, 0.43f, 0.36f, 1f)
                : new Color(0.96f, 0.83f, 0.38f, 1f);
            _valueText.color = Disabled
                ? new Color(0.52f, 0.50f, 0.45f, 1f)
                : new Color(0.94f, 0.92f, 0.86f, 1f);
        }

        private void UpdateValueText(float value)
        {
            string formatted;
            switch (_format)
            {
                case "percent":
                    formatted = $"{Mathf.RoundToInt(value * 100f)}%";
                    break;
                case "integer":
                    formatted = Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
                    break;
                case "decimal2":
                    formatted = value.ToString("0.00", CultureInfo.InvariantCulture);
                    break;
                default:
                    formatted = value.ToString("0.0", CultureInfo.InvariantCulture);
                    break;
            }

            _valueText.text = formatted + _suffix;
        }

        private static float ToSingle(object value, float fallback)
        {
            if (value == null)
                return fallback;

            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantValue))
                    return invariantValue;

                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.CurrentCulture, out var currentValue))
                    return currentValue;

                return fallback;
            }

            try
            {
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return fallback;
            }
        }

        private void CreateTrack(
            UGUIContext context,
            out Image backgroundImage,
            out RectTransform fillRect,
            out Image fillImage,
            out RectTransform handleRect,
            out Image handleImage,
            out TMP_Text valueText)
        {
            var background = CreateChild("Background", RectTransform, typeof(Image));
            backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = new Color(0.10f, 0.11f, 0.16f, 1f);
            backgroundImage.raycastTarget = false;
            StretchMiddle(background.GetComponent<RectTransform>(), 10f, 72f, 8f);

            var fillArea = CreateChild("Fill Area", RectTransform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            Stretch(fillAreaRect, 10f, 72f);

            var fill = CreateChild("Fill", fillAreaRect, typeof(Image));
            fillImage = fill.GetComponent<Image>();
            fillImage.raycastTarget = false;
            fillRect = fill.GetComponent<RectTransform>();
            StretchMiddle(fillRect, 0f, 0f, 8f);

            var handleArea = CreateChild("Handle Slide Area", RectTransform);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            Stretch(handleAreaRect, 10f, 72f);

            var handle = CreateChild("Handle", handleAreaRect, typeof(Image));
            handleImage = handle.GetComponent<Image>();
            handleImage.raycastTarget = true;
            handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(20f, 20f);
            handleRect.anchoredPosition = Vector2.zero;

            var valueObject = CreateChild("Value", RectTransform, typeof(TextMeshProUGUI));
            valueText = valueObject.GetComponent<TextMeshProUGUI>();
            valueText.font = ResolveFont(context);
            valueText.fontSize = 13f;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.MidlineRight;
            valueText.textWrappingMode = TextWrappingModes.NoWrap;
            valueText.overflowMode = TextOverflowModes.Truncate;
            valueText.raycastTarget = false;
            var valueRect = valueObject.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(1f, 0f);
            valueRect.anchorMax = Vector2.one;
            valueRect.pivot = new Vector2(1f, 0.5f);
            valueRect.sizeDelta = new Vector2(58f, 0f);
            valueRect.anchoredPosition = new Vector2(-2f, 0f);
        }

        private static TMP_FontAsset ResolveFont(UGUIContext context)
        {
            if (context?.Globals != null &&
                context.Globals.TryGetValue("moyvaFont", out var font) &&
                font is TMP_FontAsset fontAsset)
            {
                return fontAsset;
            }

            return TMP_Settings.defaultFontAsset;
        }

        private static GameObject CreateChild(string name, Transform parent, params Type[] components)
        {
            var types = new Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            Array.Copy(components, 0, types, 1, components.Length);
            var child = new GameObject(name, types);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void Stretch(RectTransform rect, float left, float right)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.up;
            rect.offsetMin = new Vector2(left, 0f);
            rect.offsetMax = new Vector2(-right, 0f);
        }

        private static void StretchMiddle(RectTransform rect, float left, float right, float height)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, -height * 0.5f);
            rect.offsetMax = new Vector2(-right, height * 0.5f);
        }
    }

    internal sealed class SliderInteractionGuard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IEndDragHandler, ICancelHandler, IScrollHandler
    {
        private readonly List<ScrollState> _scrollStates = new List<ScrollState>(2);
        private Slider _slider;
        private bool _active;

        public event Action<float> BeginChange;
        public event Action<float> EndChange;

        public void Initialize(Slider slider) => _slider = slider;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_active || _slider == null || !_slider.IsInteractable())
                return;

            _active = true;
            LockParentScrolling();
            BeginChange?.Invoke(_slider.value);
        }

        public void OnPointerUp(PointerEventData eventData) => CompleteInteraction();

        public void OnEndDrag(PointerEventData eventData) => CompleteInteraction();

        public void OnCancel(BaseEventData eventData) => CompleteInteraction();

        public void OnScroll(PointerEventData eventData)
        {
            if (_active)
                eventData.Use();
        }

        private void OnDisable() => CompleteInteraction();

        private void LockParentScrolling()
        {
            _scrollStates.Clear();
            var scrollRects = GetComponentsInParent<ScrollRect>(true);
            for (var i = 0; i < scrollRects.Length; i++)
            {
                var scrollRect = scrollRects[i];
                if (scrollRect == null)
                    continue;

                _scrollStates.Add(new ScrollState(scrollRect, scrollRect.horizontal, scrollRect.vertical));
                scrollRect.horizontal = false;
                scrollRect.vertical = false;
            }
        }

        private void CompleteInteraction()
        {
            if (!_active)
                return;

            _active = false;
            for (var i = 0; i < _scrollStates.Count; i++)
                _scrollStates[i].Restore();
            _scrollStates.Clear();
            EndChange?.Invoke(_slider != null ? _slider.value : 0f);
        }

        private readonly struct ScrollState
        {
            private readonly ScrollRect _scrollRect;
            private readonly bool _horizontal;
            private readonly bool _vertical;

            public ScrollState(ScrollRect scrollRect, bool horizontal, bool vertical)
            {
                _scrollRect = scrollRect;
                _horizontal = horizontal;
                _vertical = vertical;
            }

            public void Restore()
            {
                if (_scrollRect == null)
                    return;

                _scrollRect.horizontal = _horizontal;
                _scrollRect.vertical = _vertical;
            }
        }
    }

    internal sealed class UnityHtmlSelectComponent : UGUIComponent, IActivatableComponent
    {
        private readonly Image _backgroundImage;
        private readonly Image _itemBackgroundImage;
        private readonly Image _checkmarkImage;
        private readonly TMP_Text _captionText;
        private readonly TMP_Text _itemText;
        private int _requestedValue;

        public UnityHtmlSelectComponent(UGUIContext context) : base(context, "select")
        {
            _backgroundImage = AddComponent<Image>();
            _backgroundImage.color = new Color(0.035f, 0.045f, 0.07f, 1f);
            _backgroundImage.raycastTarget = true;

            Dropdown = AddComponent<TMP_Dropdown>();
            Dropdown.targetGraphic = _backgroundImage;
            Dropdown.transition = Selectable.Transition.ColorTint;
            Dropdown.navigation = new Navigation { mode = Navigation.Mode.None };
            Dropdown.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1f, 0.93f, 0.72f, 1f),
                pressedColor = new Color(0.90f, 0.76f, 0.42f, 1f),
                selectedColor = new Color(1f, 0.93f, 0.72f, 1f),
                disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.65f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };

            BuildHierarchy(context, out _captionText, out _itemText, out _itemBackgroundImage, out _checkmarkImage, out var template);
            Dropdown.template = template;
            Dropdown.captionText = _captionText;
            Dropdown.itemText = _itemText;
            Dropdown.options.Add(new TMP_Dropdown.OptionData("Option"));
            Dropdown.SetValueWithoutNotify(0);
            Dropdown.RefreshShownValue();
            Dropdown.onValueChanged.AddListener(value => _requestedValue = value);

            var bubbling = AddComponent<ScrollEventBubbling>();
            bubbling.Bubble = false;
        }

        public TMP_Dropdown Dropdown { get; }

        public bool Disabled
        {
            get => !Dropdown.interactable;
            set => Dropdown.interactable = !value;
        }

        public void Activate()
        {
            Dropdown.Select();
            Dropdown.Show();
        }

        public override Action AddEventListener(string eventName, Callback callback)
        {
            switch (eventName)
            {
                case "onChange":
                case "onValueChanged":
                    var listener = new UnityEngine.Events.UnityAction<int>(value => callback.CallWithPriority(EventPriority.Discrete, value, this));
                    Dropdown.onValueChanged.AddListener(listener);
                    return () => Dropdown.onValueChanged.RemoveListener(listener);
                default:
                    return base.AddEventListener(eventName, callback);
            }
        }

        public override void SetProperty(string propertyName, object value)
        {
            switch (propertyName)
            {
                case "options":
                    SetOptions(value?.ToString());
                    return;
                case "value":
                    _requestedValue = ToInt(value, _requestedValue);
                    ApplyRequestedValue();
                    return;
                case "disabled":
                    Disabled = Convert.ToBoolean(value);
                    return;
                default:
                    base.SetProperty(propertyName, value);
                    return;
            }
        }

        protected override void ApplyStylesSelf()
        {
            base.ApplyStylesSelf();
            _backgroundImage.color = new Color(0.035f, 0.045f, 0.07f, 1f);
            _captionText.color = Disabled
                ? new Color(0.52f, 0.51f, 0.48f, 1f)
                : new Color(0.97f, 0.95f, 0.89f, 1f);
            _itemText.color = new Color(0.97f, 0.95f, 0.89f, 1f);
            _itemBackgroundImage.color = new Color(0.075f, 0.08f, 0.12f, 1f);
            _checkmarkImage.color = new Color(0.88f, 0.71f, 0.27f, 1f);
        }

        private void SetOptions(string value)
        {
            Dropdown.ClearOptions();
            var labels = string.IsNullOrWhiteSpace(value)
                ? new[] { "Option" }
                : value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var options = new List<TMP_Dropdown.OptionData>(labels.Length);
            for (var i = 0; i < labels.Length; i++)
                options.Add(new TMP_Dropdown.OptionData(labels[i].Trim()));
            Dropdown.AddOptions(options);
            ApplyRequestedValue();
        }

        private void ApplyRequestedValue()
        {
            var maximum = Mathf.Max(0, Dropdown.options.Count - 1);
            _requestedValue = Mathf.Clamp(_requestedValue, 0, maximum);
            Dropdown.SetValueWithoutNotify(_requestedValue);
            Dropdown.RefreshShownValue();
        }

        private void BuildHierarchy(
            UGUIContext context,
            out TMP_Text captionText,
            out TMP_Text itemText,
            out Image itemBackgroundImage,
            out Image checkmarkImage,
            out RectTransform templateRect)
        {
            var label = CreateChild("Label", RectTransform, typeof(TextMeshProUGUI));
            captionText = label.GetComponent<TextMeshProUGUI>();
            ConfigureText(captionText, context, 14f, TextAlignmentOptions.MidlineLeft);
            Stretch(label.GetComponent<RectTransform>(), 12f, 38f, 5f, 5f);

            var arrow = CreateChild("Arrow", RectTransform, typeof(TextMeshProUGUI));
            var arrowText = arrow.GetComponent<TextMeshProUGUI>();
            ConfigureText(arrowText, context, 14f, TextAlignmentOptions.Center);
            arrowText.text = "v";
            arrowText.color = new Color(0.88f, 0.71f, 0.27f, 1f);
            var arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.sizeDelta = new Vector2(28f, 28f);
            arrowRect.anchoredPosition = new Vector2(-18f, 0f);

            var template = CreateChild("Template", RectTransform, typeof(Image), typeof(ScrollRect));
            templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = Vector2.zero;
            templateRect.anchorMax = Vector2.right;
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0f, -3f);
            templateRect.sizeDelta = new Vector2(0f, 176f);
            var templateImage = template.GetComponent<Image>();
            templateImage.color = new Color(0.035f, 0.04f, 0.065f, 1f);
            templateImage.raycastTarget = true;

            var viewport = CreateChild("Viewport", templateRect, typeof(Image), typeof(RectMask2D));
            var viewportRect = viewport.GetComponent<RectTransform>();
            Stretch(viewportRect, 2f, 2f, 2f, 2f);
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;

            var content = CreateChild("Content", viewportRect);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.up;
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 36f);

            var item = CreateChild("Item", contentRect, typeof(Toggle));
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.pivot = new Vector2(0.5f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 34f);

            var itemBackground = CreateChild("Item Background", itemRect, typeof(Image));
            itemBackgroundImage = itemBackground.GetComponent<Image>();
            itemBackgroundImage.color = new Color(0.075f, 0.08f, 0.12f, 1f);
            itemBackgroundImage.raycastTarget = true;
            Stretch(itemBackground.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);

            var checkmark = CreateChild("Item Checkmark", itemRect, typeof(Image));
            checkmarkImage = checkmark.GetComponent<Image>();
            checkmarkImage.color = new Color(0.88f, 0.71f, 0.27f, 1f);
            checkmarkImage.raycastTarget = false;
            var checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0f, 0.5f);
            checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
            checkmarkRect.sizeDelta = new Vector2(4f, 18f);
            checkmarkRect.anchoredPosition = new Vector2(10f, 0f);

            var itemLabel = CreateChild("Item Label", itemRect, typeof(TextMeshProUGUI));
            itemText = itemLabel.GetComponent<TextMeshProUGUI>();
            ConfigureText(itemText, context, 13f, TextAlignmentOptions.MidlineLeft);
            Stretch(itemLabel.GetComponent<RectTransform>(), 22f, 10f, 2f, 2f);

            var itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = checkmarkImage;
            itemToggle.transition = Selectable.Transition.ColorTint;
            itemToggle.isOn = true;

            var scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 28f;
            template.SetActive(false);
        }

        private static void ConfigureText(TMP_Text text, UGUIContext context, float size, TextAlignmentOptions alignment)
        {
            if (context?.Globals != null &&
                context.Globals.TryGetValue("moyvaFont", out var font) &&
                font is TMP_FontAsset fontAsset)
            {
                text.font = fontAsset;
            }
            else
            {
                text.font = TMP_Settings.defaultFontAsset;
            }

            text.fontSize = size;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
        }

        private static int ToInt(object value, int fallback)
        {
            if (value == null)
                return fallback;
            if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                return parsed;
            try
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return fallback;
            }
        }

        private static GameObject CreateChild(string name, Transform parent, params Type[] components)
        {
            var types = new Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            Array.Copy(components, 0, types, 1, components.Length);
            var child = new GameObject(name, types);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
