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
    public sealed class HomeMenuHtmlShellPresenterTests
    {
        [Test]
        public void Initialize_WhenFlagFalse_UsesLegacyOnly()
        {
            var fixture = CreateFixture(useUnityHtmlShell: false, mountSucceeds: true);

            try
            {
                fixture.Presenter.Initialize();

                Assert.That(fixture.Host.MountCount, Is.EqualTo(0));
                Assert.That(fixture.Legacy.activeSelf, Is.True);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void Initialize_WhenMountSucceeds_DisablesLegacyShell()
        {
            var fixture = CreateFixture(useUnityHtmlShell: true, mountSucceeds: true);

            try
            {
                fixture.Presenter.Initialize();

                Assert.That(fixture.Host.MountCount, Is.EqualTo(1));
                Assert.That(fixture.Legacy.activeSelf, Is.False);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void Initialize_WhenMountFails_KeepsLegacyShell()
        {
            var fixture = CreateFixture(useUnityHtmlShell: true, mountSucceeds: false);

            try
            {
                LogAssert.Expect(LogType.Error, "[HomeMenuHtmlShellPresenter] Falling back to legacy UGUI shell. mount failed");

                fixture.Presenter.Initialize();

                Assert.That(fixture.Host.MountCount, Is.EqualTo(1));
                Assert.That(fixture.Legacy.activeSelf, Is.True);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void Initialize_WhenCssIsMissing_KeepsLegacyShell()
        {
            var fixture = CreateFixture(useUnityHtmlShell: true, mountSucceeds: true, hasCss: false);

            try
            {
                LogAssert.Expect(LogType.Error, "[HomeMenuHtmlShellPresenter] Falling back to legacy UGUI shell. UnityHTML shell CSS asset is missing or empty.");

                fixture.Presenter.Initialize();

                Assert.That(fixture.Host.MountCount, Is.EqualTo(0));
                Assert.That(fixture.Legacy.activeSelf, Is.True);
                Assert.That(fixture.ShellRoot.activeSelf, Is.False);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void Navigation_HidesShellForPanelAndRestoresItOnReturn()
        {
            var fixture = CreateFixture(useUnityHtmlShell: true, mountSucceeds: true);

            try
            {
                fixture.Presenter.Initialize();
                Assert.That(fixture.ShellRoot.activeSelf, Is.True);
                Assert.That(fixture.Legacy.activeSelf, Is.False);

                fixture.Navigation.Open("SettingsPanel");
                Assert.That(fixture.ShellRoot.activeSelf, Is.False);
                Assert.That(fixture.Legacy.activeSelf, Is.True);

                fixture.Navigation.CloseLast();
                Assert.That(fixture.ShellRoot.activeSelf, Is.True);
                Assert.That(fixture.Legacy.activeSelf, Is.False);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void BridgeCommands_RouteToExistingServices()
        {
            var navigation = new FakeNavigation();
            var confirmation = new FakeConfirmationService();
            var bridge = new HomeMenuHtmlMenuBridge(navigation, confirmation);

            bridge.Play();
            bridge.Continue();
            bridge.Multiplayer();
            bridge.Settings();
            bridge.Exit();

            Assert.That(navigation.OpenedMenus, Is.EqualTo(new[]
            {
                "PlayModePanel",
                "ContinuePanel",
                "SelectMultiplayerType",
                "SettingsPanel"
            }));
            Assert.That(confirmation.ShowCount, Is.EqualTo(1));
            Assert.That(confirmation.LastRequest.LabelText, Is.EqualTo("Вийти з гри"));
            Assert.That(confirmation.LastRequest.OnConfirm, Is.Not.Null);
        }

        private static Fixture CreateFixture(bool useUnityHtmlShell, bool mountSucceeds, bool hasCss = true)
        {
            var rootObject = new GameObject("HomeMenuHtmlShellRoot", typeof(RectTransform));
            var legacy = new GameObject("LegacyShell");
            var anchorObject = new GameObject("HomeMenuHtmlShellAnchor");
            var anchor = anchorObject.AddComponent<HomeMenuHtmlShellAnchor>();
            anchor.ConfigureForTests(
                rootObject.GetComponent<RectTransform>(),
                legacy,
                new TextAsset("<text>Home</text>"),
                hasCss ? new TextAsset(".home { width: 100%; }") : null);

            var config = new HomeMenuConfigSO { useUnityHtmlShell = useUnityHtmlShell };
            var navigation = new FakeNavigation();
            var confirmation = new FakeConfirmationService();
            var host = new FakeUnityHtmlHost(mountSucceeds);
            var presenter = new HomeMenuHtmlShellPresenter(
                config,
                navigation,
                confirmation,
                host,
                new[] { anchor });

            return new Fixture(rootObject, legacy, anchorObject, navigation, host, presenter);
        }

        private readonly struct Fixture
        {
            private readonly GameObject _rootObject;
            private readonly GameObject _anchorObject;

            public Fixture(
                GameObject rootObject,
                GameObject legacy,
                GameObject anchorObject,
                FakeNavigation navigation,
                FakeUnityHtmlHost host,
                HomeMenuHtmlShellPresenter presenter)
            {
                _rootObject = rootObject;
                Legacy = legacy;
                _anchorObject = anchorObject;
                Navigation = navigation;
                Host = host;
                Presenter = presenter;
            }

            public GameObject ShellRoot => _rootObject;
            public GameObject Legacy { get; }
            public FakeNavigation Navigation { get; }
            public FakeUnityHtmlHost Host { get; }
            public HomeMenuHtmlShellPresenter Presenter { get; }

            public void Destroy()
            {
                Object.DestroyImmediate(_rootObject);
                Object.DestroyImmediate(Legacy);
                Object.DestroyImmediate(_anchorObject);
            }
        }

        private sealed class FakeUnityHtmlHost : IUnityHtmlHost
        {
            private readonly bool _mountSucceeds;

            public FakeUnityHtmlHost(bool mountSucceeds)
            {
                _mountSucceeds = mountSucceeds;
            }

            public int MountCount { get; private set; }

            public UnityHtmlMountResult Mount(RectTransform root, UnityHtmlDocument document, IReadOnlyDictionary<string, object> globals = null)
            {
                MountCount++;
                return _mountSucceeds ? UnityHtmlMountResult.Success() : UnityHtmlMountResult.Failure("mount failed");
            }

            public void Unmount()
            {
            }

            public void Dispose()
            {
            }
        }

        private sealed class FakeNavigation : INavigation
        {
            public List<string> OpenedMenus { get; } = new List<string>();
            public string CurrentMenu { get; private set; } = string.Empty;
            public event System.Action<NavigationChangeEventArgs> OnMenuChanged;

            public void Close(string menuName) => CloseCurrent();
            public void CloseForce(string menuName) => CloseCurrent();
            public Task CloseIf(string menuName, System.Func<Task<bool>> condition) => Task.CompletedTask;
            public void Open(string menuName)
            {
                var previous = CurrentMenu;
                OpenedMenus.Add(menuName);
                CurrentMenu = menuName;
                OnMenuChanged?.Invoke(new NavigationChangeEventArgs
                {
                    PreviousMenu = previous,
                    CurrentMenu = CurrentMenu,
                    CurrentIsOpen = true
                });
            }
            public void OpenForce(string menuName) => Open(menuName);
            public void OpenLast() { }
            public void OpenLastForce() { }
            public Task OpenIfAsync(string menuName, System.Func<Task<bool>> condition) => Task.CompletedTask;
            public void CloseLast() => CloseCurrent();
            public void CloseLastForce() => CloseCurrent();

            private void CloseCurrent()
            {
                var previous = CurrentMenu;
                CurrentMenu = string.Empty;
                OnMenuChanged?.Invoke(new NavigationChangeEventArgs
                {
                    PreviousMenu = previous,
                    CurrentMenu = string.Empty,
                    CurrentIsOpen = false
                });
            }
        }

        private sealed class FakeConfirmationService : IConfirmationService
        {
            public int ShowCount { get; private set; }
            public ConfirmationRequest LastRequest { get; private set; }

            public void Show(ConfirmationRequest request)
            {
                ShowCount++;
                LastRequest = request;
            }

            public void ForeceHide() { }

            public bool TryGetReqest(out ConfirmationRequest? request)
            {
                request = LastRequest;
                return ShowCount > 0;
            }
        }
    }
}
