using System;
using System.Collections.Generic;
using ReactUnity;
using ReactUnity.Helpers;
using ReactUnity.Scheduling;
using ReactUnity.Scripting;
using ReactUnity.Styling;
using ReactUnity.Styling.Rules;
using ReactUnity.UGUI;
using ReactUnity.UGUI.Behaviours;
using UnityEngine;

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
                    EngineType = JavascriptEngineType.QuickJS,
                    Pooling = ReactContext.PoolingType.None,
                    UnknownPropertyHandling = ReactContext.UnknownPropertyHandling.Exception
                });

                if (!string.IsNullOrWhiteSpace(document.Css))
                    _context.InsertStyle(document.Css);

                _context.Start();
                _context.Host?.ResolveStyle(true);
                _context.UpdateElementsRecursively();
                _context.CalculateLayoutRecursively();
                FlushReactElementLayout(_root);
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

        private static void DisposeContext(UGUIContext context)
        {
            if (context == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= context.Dispose;

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
    }
}
