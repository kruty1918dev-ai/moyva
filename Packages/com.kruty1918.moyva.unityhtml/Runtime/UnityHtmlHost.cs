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
using UnityEngine.UI;

namespace UnityHTML.Runtime
{
    public sealed class UnityHtmlHost : IUnityHtmlHost
    {
        private UGUIContext _context;
        private RectTransform _root;

        public UnityHtmlMountResult Mount(
            RectTransform root,
            UnityHtmlDocument document,
            IReadOnlyDictionary<string, object> globals = null)
        {
            Unmount();

            if (root == null)
                return UnityHtmlMountResult.Failure("Mount root is not assigned.");

            if (!document.HasHtml)
                return UnityHtmlMountResult.Failure($"HTML document '{document.SourceName}' is empty or missing.");

            try
            {
                RegisterMoyvaComponents();
                _root = root;
                ClearRootChildren(_root);
                var globalRecord = CreateGlobals(globals);
                var source = ScriptSource.Text(document.Html, ScriptSourceLanguage.Html);

                _context = new UGUIContext(new UGUIContext.Options
                {
                    HostElement = root,
                    Globals = globalRecord,
                    Source = source,
                    Timer = UnscaledTimer.Instance,
                    MediaProvider = DefaultMediaProvider.CreateMediaProvider(document.SourceName, "ugui", false),
                    EngineType = ResolveEngineType(),
                    Pooling = ReactContext.PoolingType.None,
                    UnknownPropertyHandling = ReactContext.UnknownPropertyHandling.Exception
                });
                DetachUnsafeEditorAssemblyReloadDispose(_context);

                if (!string.IsNullOrWhiteSpace(document.Css))
                    _context.InsertStyle(document.Css);

                _context.Start();
                _context.Host?.ResolveStyle(true);
                _context.UpdateElementsRecursively();
                _context.CalculateLayoutRecursively();
                FlushReactElementLayout(_root);
                ConfigureRenderedInputs(_root);
                _context.LateUpdateElementsRecursively();
                Canvas.ForceUpdateCanvases();
                return UnityHtmlMountResult.Success();
            }
            catch (Exception exception)
            {
                Unmount();
                return UnityHtmlMountResult.Failure($"{document.SourceName}: {exception.GetBaseException().Message}");
            }
        }

        public void Unmount()
        {
            var context = _context;
            var root = _root;
            _context = null;
            _root = null;

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

        private static GlobalRecord CreateGlobals(IReadOnlyDictionary<string, object> globals)
        {
            var record = new GlobalRecord();
            if (globals == null)
                return record;

            foreach (var pair in globals)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    continue;

                record[pair.Key] = pair.Value;
            }

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
                if (layout == null || transform == null || float.IsNaN(layout.LayoutWidth))
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
            if (!UGUIContext.ComponentCreators.ContainsKey("slider"))
                UGUIContext.ComponentCreators["slider"] = (_, _, context) => new UnityHtmlSliderComponent(context);
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

                input.customCaretColor = true;
                input.caretColor = new Color(0.98f, 0.91f, 0.56f, 1f);
                input.selectionColor = new Color(0.86f, 0.68f, 0.23f, 0.35f);
                input.lineType = TMP_InputField.LineType.SingleLine;

                if (input.textComponent != null)
                {
                    input.textComponent.color = new Color(0.98f, 0.96f, 0.89f, 1f);
                    input.textComponent.alignment = TextAlignmentOptions.MidlineLeft;
                    input.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
                    input.textComponent.overflowMode = TextOverflowModes.Masking;
                    input.textComponent.margin = new Vector4(8f, 0f, 8f, 0f);
                    input.textComponent.raycastTarget = false;
                }

                if (input.placeholder is TMP_Text placeholder)
                {
                    placeholder.color = new Color(0.72f, 0.70f, 0.64f, 0.78f);
                    placeholder.alignment = TextAlignmentOptions.MidlineLeft;
                    placeholder.textWrappingMode = TextWrappingModes.NoWrap;
                    placeholder.margin = new Vector4(8f, 0f, 8f, 0f);
                    placeholder.raycastTarget = false;
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
        private readonly Image _fillImage;
        private readonly Image _handleImage;

        public UnityHtmlSliderComponent(UGUIContext context) : base(context, "slider")
        {
            var raycastGraphic = AddComponent<Image>();
            raycastGraphic.color = new Color(0f, 0f, 0f, 0.001f);
            raycastGraphic.raycastTarget = true;

            Slider = AddComponent<Slider>();
            Slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            Slider.transition = Selectable.Transition.ColorTint;

            CreateTrack(out var fillRect, out _fillImage, out var handleRect, out _handleImage);
            Slider.fillRect = fillRect;
            Slider.handleRect = handleRect;
            Slider.targetGraphic = _handleImage;
            Slider.minValue = 0f;
            Slider.maxValue = 1f;
            Slider.value = 0f;
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
                    return;
                case "min":
                case "minValue":
                    Slider.minValue = ToSingle(value, Slider.minValue);
                    return;
                case "max":
                case "maxValue":
                    Slider.maxValue = ToSingle(value, Slider.maxValue);
                    return;
                case "wholeNumbers":
                    Slider.wholeNumbers = Convert.ToBoolean(value);
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
            _fillImage.color = new Color(0.86f, 0.68f, 0.23f, 1f);
            _handleImage.color = Disabled
                ? new Color(0.46f, 0.43f, 0.36f, 1f)
                : new Color(0.96f, 0.83f, 0.38f, 1f);
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
            out RectTransform fillRect,
            out Image fillImage,
            out RectTransform handleRect,
            out Image handleImage)
        {
            var background = CreateChild("Background", RectTransform, typeof(Image));
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = new Color(0.10f, 0.11f, 0.16f, 1f);
            backgroundImage.raycastTarget = false;
            StretchMiddle(background.GetComponent<RectTransform>(), 0f, 0f, 6f);

            var fillArea = CreateChild("Fill Area", RectTransform);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            Stretch(fillAreaRect, 8f, 8f);

            var fill = CreateChild("Fill", fillAreaRect, typeof(Image));
            fillImage = fill.GetComponent<Image>();
            fillImage.raycastTarget = false;
            fillRect = fill.GetComponent<RectTransform>();
            StretchMiddle(fillRect, 0f, 0f, 6f);

            var handleArea = CreateChild("Handle Slide Area", RectTransform);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            Stretch(handleAreaRect, 8f, 8f);

            var handle = CreateChild("Handle", handleAreaRect, typeof(Image));
            handleImage = handle.GetComponent<Image>();
            handleImage.raycastTarget = true;
            handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0.5f);
            handleRect.anchorMax = new Vector2(0f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.sizeDelta = new Vector2(18f, 18f);
            handleRect.anchoredPosition = Vector2.zero;
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
}
