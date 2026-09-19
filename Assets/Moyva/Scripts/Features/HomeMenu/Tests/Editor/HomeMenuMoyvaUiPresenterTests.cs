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
    /// Regression coverage for the empty-panel bug: a deferred route swap that
    /// lands on the regional-update path must resync the rendered-route cache
    /// and replay the enter motion. Otherwise a finished exit fade leaves the
    /// navigation region at alpha 0 and every later same-route change re-arms
    /// the exit fade, so the panel can stay invisible while mounted.
    /// </summary>
    public sealed class HomeMenuMoyvaUiPresenterTests
    {
        private const string NavRegion = HomeMenuMoyvaUiMarkup.NavRegionId;

        [UnityTest]
        public IEnumerator DeferredRouteSwap_PlaysEnterMotionAndResyncsRenderedRoute()
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
                state.Open("SettingsPanel");
                yield return null; // let Time.frameCount move past the change frame
                presenter.Tick();
                yield return new WaitForSecondsRealtime(0.3f);
                presenter.Tick();

                Assert.AreEqual(1, host.MountCalls, "Route swap should reuse the mounted document.");
                Assert.GreaterOrEqual(host.UpdateRegionsCalls, 1, "Route swap should take the regional update path.");
                Assert.AreEqual(1, CountPlays(host, "fade-out"), "Expected exactly one route exit fade.");
                Assert.AreEqual(2, CountPlays(host, "fade"),
                    "The enter motion must replay after the deferred swap (initial mount + route enter).");

                // A section switch on the same route must not re-arm the exit fade.
                state.SetSettingsSection(HomeMenuSettingsSection.Audio);
                yield return null;
                presenter.Tick();
                yield return null;
                presenter.Tick();

                Assert.AreEqual(1, CountPlays(host, "fade-out"),
                    "A same-route section switch must not trigger another route exit fade.");
                Assert.AreEqual(1, host.MountCalls, "Section switch should stay on the regional update path.");
            }
            finally
            {
                presenter.Dispose();
                Object.DestroyImmediate(anchorGo);
            }
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

            public RecordingHost() => _motion = new RecordingMotion(MotionLog);
            public IUnityHtmlMotion Motion => _motion;

            public UnityHtmlMountResult Mount(
                RectTransform root,
                UnityHtmlDocument document,
                IReadOnlyDictionary<string, object> globals = null)
            {
                MountCalls++;
                return UnityHtmlMountResult.Success();
            }

            public void Unmount() { }
            public bool UpdateRegion(string elementId, string html) => true;

            public bool UpdateRegions(
                IReadOnlyDictionary<string, string> regions,
                IReadOnlyDictionary<string, object> globals = null)
            {
                UpdateRegionsCalls++;
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
        }
    }
}
