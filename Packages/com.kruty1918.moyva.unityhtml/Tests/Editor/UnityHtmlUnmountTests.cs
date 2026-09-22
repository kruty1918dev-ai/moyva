using System;
using System.Reflection;
using NUnit.Framework;
using ReactUnity.UGUI;
using ReactUnity.UGUI.EventHandlers;
using UnityEngine;
using UnityHTML.Runtime;

namespace UnityHTML.Tests
{
    // P079: mount/unmount and re-render cycles must release every listener,
    // tween and helper component the document created — no handler growth, no
    // surviving tooltip layer, no MissingReferenceException.
    public sealed class UnityHtmlUnmountTests
    {
        private static readonly MethodInfo LateUpdateMethod = typeof(UnityHtmlTooltipLayer)
            .GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

        private GameObject _root;
        private UnityHtmlHost _host;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("HTML unmount", typeof(RectTransform), typeof(Canvas));
            _root.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            _host = new UnityHtmlHost();
        }

        [TearDown]
        public void TearDown()
        {
            _host.Dispose();
            UnityEngine.Object.DestroyImmediate(_root);
        }

        [Test]
        public void MountUnmountCycles_LeaveNoHandlersOrLayersBehind()
        {
            int baselineHandlers = UnityEngine.Object
                .FindObjectsByType<PointerEnterHandler>(FindObjectsSortMode.None).Length;

            for (int i = 0; i < 50; i++)
            {
                Mount($"<view id='panel' data-motion-role='panel' data-motion='exit' data-tooltip='tip{i}'>"
                    + $"<view onPointerEnter='Globals.noop()' onClick='Globals.noop()'>"
                    + $"<text>label{i}</text></view></view>");
                BindTooltipLayer();
                _host.Unmount();

                Assert.That(_root.transform.childCount, Is.EqualTo(0),
                    $"cycle {i} left live children under the root");
                Assert.That(UnityEngine.Object
                        .FindObjectsByType<UnityHtmlTooltipLayer>(FindObjectsSortMode.None).Length,
                    Is.EqualTo(0), $"cycle {i} leaked the tooltip layer");
                Assert.That(UnityEngine.Object
                        .FindObjectsByType<UnityHtmlTooltipTarget>(FindObjectsSortMode.None).Length,
                    Is.EqualTo(0), $"cycle {i} leaked tooltip targets");
            }

            Assert.That(UnityEngine.Object
                    .FindObjectsByType<PointerEnterHandler>(FindObjectsSortMode.None).Length,
                Is.EqualTo(baselineHandlers), "event handler components accumulated");
        }

        [Test]
        public void RegionChurn_KeepsListenerCountsFlat()
        {
            Mount("<view id='panel'><view id='row' data-tooltip='tip0'"
                + " onPointerEnter='Globals.noop()'><text>l0</text></view></view>");
            UnityHtmlTooltipLayer layer = BindTooltipLayer();

            int handlerCount = _root.GetComponentsInChildren<PointerEnterHandler>(true).Length;
            int listenerCount = HandlerListeners();
            int targets = _root.GetComponentsInChildren<UnityHtmlTooltipTarget>(true).Length;
            Assert.That(handlerCount, Is.EqualTo(1));
            Assert.That(listenerCount, Is.EqualTo(1));
            Assert.That(targets, Is.EqualTo(1));

            for (int i = 1; i <= 50; i++)
            {
                // Same shape, same ids, new text — the locale-change path
                // reconciles in place; handlers must swap, not stack.
                Assert.That(_host.UpdateRegion("panel",
                    $"<view id='row' data-tooltip='tip{i}'"
                    + $" onPointerEnter='Globals.noop()'><text>l{i}</text></view>"), Is.True);
                layer.RefreshTargets();
            }

            Assert.That(_root.GetComponentsInChildren<PointerEnterHandler>(true).Length,
                Is.EqualTo(handlerCount), "handler components stacked across region updates");
            Assert.That(HandlerListeners(), Is.EqualTo(listenerCount),
                "event listeners accumulated on the reconciled element");
            Assert.That(_root.GetComponentsInChildren<UnityHtmlTooltipTarget>(true).Length,
                Is.EqualTo(targets), "tooltip targets stacked across region updates");
        }

        private int HandlerListeners()
        {
            int total = 0;
            foreach (PointerEnterHandler handler in
                _root.GetComponentsInChildren<PointerEnterHandler>(true))
            {
                var field = typeof(PointerEnterHandler).GetField("OnEvent",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field?.GetValue(handler) is Delegate subscribers)
                    total += subscribers.GetInvocationList().Length;
            }
            return total;
        }

        [Test]
        public void UnmountWhileTooltipVisible_ReleasesEverythingWithoutMissingReferences()
        {
            Mount("<view id='t' data-tooltip='visible tip'><text>x</text></view>");
            UnityHtmlTooltipLayer layer = BindTooltipLayer();
            layer.ShowDelaySeconds = 0f;
            _host.SetWorldTooltip("Water Mill", new Vector2(100, 100));
            LateUpdateMethod.Invoke(layer, null);

            Assert.DoesNotThrow(() => _host.Unmount());
            Assert.That(_root.transform.childCount, Is.EqualTo(0));
            Assert.That(layer == null, Is.True, "tooltip layer survived unmount");
        }

        [Test]
        public void ManagedMemory_StaysFlatAcrossMountCycles()
        {
            const int warmup = 10, measured = 40;
            string Markup(int i) =>
                $"<view data-motion-role='panel' data-tooltip='tip{i}'>"
                + $"<view onClick='Globals.noop()'><text>label{i}</text></view></view>";

            // Warm up one-time caches (font atlas, pools, parser tables) so the
            // measured window sees steady-state retention, not cold-start cost.
            for (int i = 0; i < warmup; i++) { Mount(Markup(i)); _host.Unmount(); }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            // GetTotalMemory reports committed heap pages (Mono does not return
            // them); GetMonoUsedSizeLong reports live managed bytes — the real
            // leak signal.
            long before = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();

            for (int i = warmup; i < warmup + measured; i++)
            {
                Mount(Markup(i));
                _host.Unmount();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long delta = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() - before;
            TestContext.Out.WriteLine(
                $"live managed delta over {measured} mount cycles after warmup: {delta} bytes");
            Assert.That(delta, Is.LessThan(4L * 1024 * 1024),
                "mount cycles retained managed memory");
        }

        [Test]
        public void UnmountedContexts_AreCollected()
        {
            var references = new System.Collections.Generic.List<WeakReference>();
            for (int i = 0; i < 6; i++)
                MountAndTrack(references);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.Collect();

            int alive = 0;
            foreach (WeakReference reference in references)
                if (reference.IsAlive) alive++;
            Assert.That(alive, Is.EqualTo(0),
                "disposed UGUIContext objects stayed rooted (YogaNode GCHandle leak)");
        }

        // Kept out-of-line so the context reference dies with this frame —
        // Mono's conservative stack scan would otherwise root the last one.
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void MountAndTrack(System.Collections.Generic.List<WeakReference> references)
        {
            Mount("<view onClick='Globals.noop()'><text>w</text></view>");
            var context = typeof(UnityHtmlHost)
                .GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_host);
            references.Add(new WeakReference(context));
            _host.Unmount();
        }

        private void Mount(string html)
        {
            UnityHtmlMountResult result = _host.Mount(
                _root.GetComponent<RectTransform>(),
                new UnityHtmlDocument(html, string.Empty, "Unmount"));
            Assert.That(result.Succeeded, Is.True, result.ErrorMessage);
        }

        private UnityHtmlTooltipLayer BindTooltipLayer()
        {
            var context = (UGUIContext)typeof(UnityHtmlHost)
                .GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_host);
            var layer = _root.AddComponent<UnityHtmlTooltipLayer>();
            layer.Bind(context, _root.GetComponent<RectTransform>());
            layer.RefreshTargets();
            typeof(UnityHtmlHost)
                .GetField("_tooltips", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_host, layer);
            return layer;
        }
    }
}
