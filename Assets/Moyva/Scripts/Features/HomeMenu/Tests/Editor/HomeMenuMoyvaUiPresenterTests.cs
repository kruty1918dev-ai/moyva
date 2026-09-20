using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>
    /// Regression coverage for the empty-panel bug and the dead Controls tab.
    /// A deferred route swap must resync the rendered-route cache and replay the
    /// enter motion — otherwise a finished exit fade leaves the navigation region
    /// at alpha 0 (blank panel) and a stale route cache re-arms the exit fade on
    /// every later same-route change such as settings tab switches. Switching
    /// into Controls additionally changes the shell root class (controls-page),
    /// which a regional update cannot retarget, so it must take the full document
    /// mount for the scoped CSS to apply.
    /// </summary>
    public sealed class HomeMenuMoyvaUiPresenterTests
    {
        private const string NavRegion = HomeMenuMoyvaUiMarkup.NavRegionId;

        [UnityTest]
        public IEnumerator DeferredRouteSwap_ResyncsRenderedRoute_AndControlsTabMounts()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);
            var anchorGo = new GameObject("TestAnchor", typeof(RectTransform));
            var mountRoot = new GameObject("MountRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            mountRoot.SetParent(anchorGo.transform, false);
            mountRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1600f);
            mountRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 900f);
            var anchor = anchorGo.AddComponent<HomeMenuMoyvaUiAnchor>();
            anchor.EditorSetLivePreview(false);
            anchor.ConfigureForTests(mountRoot, null, null, new TextAsset("view { width: 100%; height: 100%; }"));

            var host = new RecordingHost();
            var presenter = new HomeMenuMoyvaUiPresenter(
                new HomeMenuConfigSO { useUnityHtmlShell = true },
                new StubNavigation(),
                null,
                host,
                state,
                view,
                null,
                new[] { anchor });

            try
            {
                presenter.Initialize();
                Assert.AreEqual(1, host.MountCalls, "Initial document mount did not happen.");

                // Main -> SettingsPanel: the exit fade defers the document swap.
                // The route-* shell class changes, so the swap must take the full
                // document path (regional updates cannot retarget root classes).
                state.Open("SettingsPanel");
                yield return null; // let Time.frameCount move past the change frame
                ClearStateChangeFrame(presenter); // Time.frameCount may not advance in edit mode
                presenter.Tick();

                Assert.AreEqual(1, CountPlays(host, "fade-out"), "Expected exactly one route exit fade.");

                // Pump ticks until the deferred swap lands. Time.unscaledTime may
                // not advance in edit mode, so expire the defer window directly.
                var deadline = Time.realtimeSinceStartup + 2f;
                while (host.MountCalls < 2 && Time.realtimeSinceStartup < deadline)
                {
                    ExpirePendingRender(presenter);
                    presenter.Tick();
                    yield return null;
                }

                Assert.AreEqual(2, host.MountCalls, "Route swap remounts the document for the new shell class.");
                Assert.AreEqual(1, CountPlays(host, "fade-out"), "Expected exactly one route exit fade.");
                Assert.AreEqual(2, CountPlays(host, "fade"),
                    "The enter motion must replay after the deferred swap (initial mount + route enter).");

                // A section switch on the same route must not re-arm the exit fade
                // and stays on the regional update path.
                state.SetSettingsSection(HomeMenuSettingsSection.Audio);
                yield return null;
                ClearStateChangeFrame(presenter);
                presenter.Tick();

                Assert.AreEqual(1, CountPlays(host, "fade-out"),
                    "A same-route section switch must not trigger another route exit fade.");
                Assert.AreEqual(2, host.MountCalls, "Same-route section switch must not remount.");
                Assert.AreEqual(1, host.UpdateRegionsCalls,
                    "Same-route section switch should reuse the regional update path.");

                // Controls is the only section that also changes the shell root
                // class (controls-page). The regional path cannot retarget the
                // root element, so this switch must take the full mount path.
                state.SetSettingsSection(HomeMenuSettingsSection.Controls);
                yield return null;
                ClearStateChangeFrame(presenter);
                presenter.Tick();

                Assert.AreEqual(1, CountPlays(host, "fade-out"),
                    "Selecting Controls must not re-arm the route exit fade.");
                Assert.AreEqual(3, host.MountCalls,
                    "Entering Controls must mount the document so the controls-page root class applies.");
                Assert.IsTrue(host.LastHtml.Contains("controls-page"),
                    "Mounted Controls document must carry the controls-page root class.");
                Assert.IsTrue(host.LastHtml.Contains("keyboard-board"),
                    "Mounted Controls document must contain the controls workspace markup.");

                // Leaving Controls restores the base settings root class.
                state.SetSettingsSection(HomeMenuSettingsSection.General);
                yield return null;
                ClearStateChangeFrame(presenter);
                presenter.Tick();

                Assert.AreEqual(4, host.MountCalls,
                    "Leaving Controls must remount to drop the controls-page root class.");
                Assert.IsFalse(host.LastHtml.Contains("controls-page"),
                    "Leaving Controls must drop the controls-page root class.");
            }
            finally
            {
                presenter.Dispose();
                Object.DestroyImmediate(anchorGo);
            }
        }

        private static void ExpirePendingRender(HomeMenuMoyvaUiPresenter presenter)
        {
            typeof(HomeMenuMoyvaUiPresenter)
                .GetField("_pendingRenderAt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(presenter, 0f);
        }

        private static void ClearStateChangeFrame(HomeMenuMoyvaUiPresenter presenter)
        {
            typeof(HomeMenuMoyvaUiPresenter)
                .GetField("_lastStateChangeFrame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(presenter, -1);
        }

        private static int CountPlays(RecordingHost host, string preset)
        {
            int count = 0;
            foreach (var play in host.MotionLog)
                if (play.preset == preset && play.id == NavRegion)
                    count++;
            return count;
        }

        private sealed class StubNavigation : INavigation
        {
            public string CurrentMenu { get; private set; } = string.Empty;
            public event System.Action<NavigationChangeEventArgs> OnMenuChanged
            {
                add { }
                remove { }
            }
            public void Close(string menuName) { }
            public void CloseForce(string menuName) { }
            public Task CloseIf(string menuName, System.Func<Task<bool>> condition) => Task.CompletedTask;
            public void Open(string menuName) { }
            public void OpenForce(string menuName) { }
            public void OpenLast() { }
            public void OpenLastForce() { }
            public Task OpenIfAsync(string menuName, System.Func<Task<bool>> condition) => Task.CompletedTask;
            public void CloseLast() { }
            public void CloseLastForce() { }
        }

        private sealed class RecordingHost : IUnityHtmlHost
        {
            public readonly List<(string id, string preset)> MotionLog = new();
            private readonly RecordingMotion _motion;
            public int MountCalls;
            public int UpdateRegionsCalls;
            public string LastHtml = string.Empty;

            public RecordingHost() => _motion = new RecordingMotion(MotionLog);
            public IUnityHtmlMotion Motion => _motion;

            public UnityHtmlMountResult Mount(
                RectTransform root,
                UnityHtmlDocument document,
                IReadOnlyDictionary<string, object> globals = null)
            {
                MountCalls++;
                LastHtml = document.Html ?? string.Empty;
                return UnityHtmlMountResult.Success();
            }

            public void Unmount() { }
            public bool UpdateRegion(string elementId, string html) => true;

            public bool UpdateRegions(
                IReadOnlyDictionary<string, string> regions,
                IReadOnlyDictionary<string, object> globals = null)
            {
                UpdateRegionsCalls++;
                if (regions != null && regions.TryGetValue(NavRegion, out var nav))
                    LastHtml = nav ?? string.Empty;
                return true;
            }

            public bool SetValue(string elementId, string value) => true;
            public void Dispose() { }
        }

        private sealed class RecordingMotion : IUnityHtmlMotion
        {
            private readonly List<(string id, string preset)> _log;
            public RecordingMotion(List<(string id, string preset)> log) => _log = log;
            public void Play(string targetId, string preset, float duration, float delay) => _log.Add((targetId, preset));
            public void Stop(string targetId) { }
            public void RestoreResting(string targetId) { }
        }
    }
}
