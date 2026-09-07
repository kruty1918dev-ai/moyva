using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class HomeMenuMoyvaUiTests
    {
        [Test]
        public void Build_MainAndPlayRoutes_ExposeClearGameFlow()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);

            try
            {
                var mainHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-720 vp-landscape");

                Assert.That(mainHtml, Does.Contain("Globals.moyvaMenu.Play()"));
                Assert.That(mainHtml, Does.Contain("Globals.moyvaMenu.Settings()"));
                Assert.That(mainHtml, Does.Contain("Globals.moyvaMenu.Exit()"));
                Assert.That(mainHtml, Does.Not.Contain("Globals.moyvaMenu.Multiplayer()"));
                Assert.That(mainHtml, Does.Not.Contain("Globals.moyvaMenu.Continue()"));

                state.Open("PlayModePanel");
                var playHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-720 vp-landscape");

                Assert.That(playHtml, Does.Contain("SOLO SANDBOX"));
                Assert.That(playHtml, Does.Contain("Globals.moyvaMenu.Solo()"));
                Assert.That(playHtml, Does.Contain("MULTIPLAYER"));
                Assert.That(playHtml, Does.Contain("COMING SOON"));
                Assert.That(playHtml, Does.Contain("disabled=\"true\""));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void Build_CreateRoomRoute_RendersEditableFieldsToggleAndStepper()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);

            try
            {
                state.Open("CreateRoomPanel");
                view.SetRoomName("Test Room");
                view.SetRoomPrivate(true);
                view.SetRoomPassword("secret");

                var html = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-720 vp-landscape");

                Assert.That(html, Does.Contain("Globals.moyvaMenu.PreviewRoomName(event)"));
                Assert.That(html, Does.Contain("Globals.moyvaMenu.CommitRoomName(event)"));
                Assert.That(html, Does.Contain("Globals.moyvaMenu.CommitRoomPassword(event)"));
                Assert.That(html, Does.Contain("<toggle"));
                Assert.That(html, Does.Contain("Globals.moyvaMenu.SetPrivate(event)"));
                Assert.That(html, Does.Contain("stepper-value"));
                Assert.That(html, Does.Contain("Test Room"));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void Build_WorldAndSettingsRoutes_UsePurposeBuiltControls()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);

            try
            {
                state.Open("WorldSetupPanel");
                var worldHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-720 vp-landscape");

                Assert.That(Count(worldHtml, "<select"), Is.EqualTo(3));
                Assert.That(worldHtml, Does.Contain("Small|Medium|Large"));
                Assert.That(worldHtml, Does.Contain("Continents|Pangaea|Islands|Highlands|Desert|Random"));
                Assert.That(worldHtml, Does.Not.Contain("Globals.moyvaMenu.WorldSmall()"));
                Assert.That(worldHtml, Does.Not.Contain("Globals.moyvaMenu.MapPangaea()"));

                state.Open("SettingsPanel");
                state.SetSettingsSection(HomeMenuSettingsSection.Audio);
                var audioHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-1080 vp-landscape");

                Assert.That(audioHtml, Does.Contain("settings-tabs"));
                Assert.That(Count(audioHtml, "<slider"), Is.EqualTo(4));
                Assert.That(audioHtml, Does.Contain("onBeginChange=\"Globals.moyvaMenu.BeginControlInteraction()\""));
                Assert.That(audioHtml, Does.Contain("Globals.moyvaMenu.CommitMasterValue(event)"));
                Assert.That(audioHtml, Does.Contain("Globals.moyvaMenu.SetMuted(event)"));

                state.SetSettingsSection(HomeMenuSettingsSection.Graphics);
                var graphicsHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-1080 vp-landscape");

                Assert.That(Count(graphicsHtml, "<select"), Is.EqualTo(4));
                Assert.That(Count(graphicsHtml, "<slider"), Is.EqualTo(2));
                Assert.That(Count(graphicsHtml, "<toggle"), Is.EqualTo(3));
                Assert.That(graphicsHtml, Does.Contain("Globals.moyvaMenu.SetTextureQualityOption(event)"));
                Assert.That(graphicsHtml, Does.Contain("Globals.moyvaMenu.SetAntiAliasingOption(event)"));
                Assert.That(graphicsHtml, Does.Not.Contain("Dynamic scale"));
                Assert.That(graphicsHtml, Does.Not.Contain("Close zoom"));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void Build_NestedRoute_RendersHeaderBackButton()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);

            try
            {
                var mainHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-720 vp-landscape");
                state.Open("SettingsPanel");
                var settingsHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-720 vp-landscape");

                Assert.That(mainHtml, Does.Not.Contain("top-back-button"));
                Assert.That(settingsHtml, Does.Contain("top-back-button"));
                Assert.That(settingsHtml, Does.Contain("Globals.moyvaMenu.Back()"));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void State_DuringControlInteraction_DefersDirtyRenderUntilRelease()
        {
            var state = new HomeMenuMoyvaUiState();
            var changed = 0;
            state.Changed += () => changed++;

            Assert.That(state.ConsumeDirty(), Is.True);
            state.BeginInteraction();
            state.MarkDirty();

            Assert.That(state.IsInteractionActive, Is.True);
            Assert.That(state.ConsumeDirty(), Is.False);
            Assert.That(changed, Is.Zero);

            state.EndInteraction();
            state.EndInteraction();

            Assert.That(state.IsInteractionActive, Is.False);
            Assert.That(changed, Is.EqualTo(1));
            Assert.That(state.ConsumeDirty(), Is.True);
        }

        [Test]
        public void Bridge_ControlCommands_MapIndexesAndExactToggleValues()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);
            var bridge = new HomeMenuMoyvaUiBridge(null, null, view);

            try
            {
                bridge.SetWorldSizeValue(2);
                bridge.SetMapTypeValue(5);
                bridge.SetDifficultyValue(3);
                bridge.SetPrivate(true);
                bridge.SetFrameRateOption(5);
                bridge.SetTextureQualityOption(2);
                bridge.SetAntiAliasingOption(2);
                bridge.SetMuted(true);
                bridge.SetVSync(false);
                bridge.SetShadows(false);
                bridge.SetAnisotropic(true);

                Assert.That(view.Size.ToString(), Is.EqualTo("Large"));
                Assert.That(view.MapType.ToString(), Is.EqualTo("Random"));
                Assert.That(view.Difficulty.ToString(), Is.EqualTo("Insane"));
                Assert.That(view.IsPublic, Is.False);
                Assert.That(view.TargetFrameRate, Is.EqualTo(144));
                Assert.That(view.TextureMipmapLimit, Is.EqualTo(2));
                Assert.That(view.AntiAliasing, Is.EqualTo(4));
                Assert.That(view.IsMuted, Is.True);
                Assert.That(view.VSync, Is.False);
                Assert.That(view.Shadows, Is.False);
                Assert.That(view.AnisotropicFiltering, Is.True);
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void ViewportClass_IncludesSizeAndOrientation()
        {
            Assert.That(HomeMenuViewportUtility.ResolveViewportClass(new Vector2(1920f, 1080f)), Is.EqualTo("vp-1080 vp-landscape"));
            Assert.That(HomeMenuViewportUtility.ResolveViewportClass(new Vector2(390f, 844f)), Is.EqualTo("vp-tiny vp-portrait"));
            Assert.That(HomeMenuViewportUtility.ResolveViewportClass(new Vector2(960f, 540f)), Is.EqualTo("vp-tiny vp-landscape"));
        }

        [Test]
        public void HomeMenuScene_DoesNotContainLegacyUiRoots()
        {
            var scene = System.IO.File.ReadAllText("Assets/Moyva/Scenes/HomeMenu.unity");

            Assert.That(scene, Does.Not.Contain("m_Name: SidePanel"));
            Assert.That(scene, Does.Not.Contain("m_Name: ContentArea"));
            Assert.That(scene, Does.Not.Contain("m_Name: ConfirmDialog"));
            Assert.That(scene, Does.Not.Contain("m_Name: InfoPanel"));
            Assert.That(scene, Does.Not.Contain("m_Name: Overlay"));
            Assert.That(scene, Does.Not.Contain("m_Name: Loading"));
            Assert.That(scene, Does.Contain("m_Name: UnityHTMLShellAnchor"));
        }

        [Test]
        public void CompatibilityAnchor_UsesDynamicEditorMarkup()
        {
            var anchorObject = new GameObject("HomeMenuAnchor", typeof(RectTransform));

            try
            {
                var anchor = anchorObject.AddComponent<HomeMenuHtmlShellAnchor>();
                var buildPreview = anchor.GetType().GetMethod(
                    "BuildEditorPreviewHtml",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                Assert.That(buildPreview, Is.Not.Null);
                var html = (string)buildPreview.Invoke(anchor, null);

                Assert.That(html, Does.Contain("A realm awaits"));
                Assert.That(html, Does.Contain("Globals.moyvaMenu.Play()"));
                Assert.That(html, Does.Not.Contain("Choose your path"));
                Assert.That(html, Does.Not.Contain("Globals.moyvaMenu.Multiplayer()"));
            }
            finally
            {
                Object.DestroyImmediate(anchorObject);
            }
        }

        [Test]
        public void PresenterDispose_WhenMountedAnchorWasDestroyed_DoesNotThrow()
        {
            var mountObject = new GameObject("MountRoot", typeof(RectTransform));
            var legacy = new GameObject("LegacyShell");
            var anchorObject = new GameObject("UnityHTMLShellAnchor");
            var anchor = anchorObject.AddComponent<HomeMenuMoyvaUiAnchor>();
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);
            var host = new FakeUnityHtmlHost();
            var presenter = new HomeMenuMoyvaUiPresenter(
                new HomeMenuConfigSO { useUnityHtmlShell = true },
                new FakeNavigation(),
                new FakeConfirmationService(),
                host,
                state,
                view,
                null,
                new[] { anchor });

            try
            {
                anchor.ConfigureForTests(
                    mountObject.GetComponent<RectTransform>(),
                    legacy,
                    new TextAsset("<text>Home</text>"),
                    new TextAsset(".home { width: 100%; }"));

                presenter.Initialize();
                Object.DestroyImmediate(anchorObject);

                Assert.DoesNotThrow(() => presenter.Dispose());
                Assert.That(host.DisposeCount, Is.EqualTo(1));
            }
            finally
            {
                presenter.Dispose();
                view.Dispose();
                Object.DestroyImmediate(mountObject);
                Object.DestroyImmediate(legacy);
                if (anchorObject != null)
                    Object.DestroyImmediate(anchorObject);
            }
        }

        [Test]
        public async Task WorldCreate_WhenSoloFlowHasExistingLobby_StartsLocalGame()
        {
            GameLaunchContext.Reset();
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);
            var navigation = new FakeNavigation();
            var starter = new FakeGameStarter();
            var service = new WorldCreationPanelService();

            try
            {
                state.SetPlayFlow(HomeMenuPlayFlow.Solo);
                view.WorldName = "Solo Smoke";
                view.Seed = 123456789;

                SetField(service, "_viewController", view);
                SetField(service, "_navigation", navigation);
                SetField(service, "_gameplaySession", new GameplaySession());
                SetField(service, "_lobbyService", new FakeLobbyService(CreateSingleHostLobby()));
                SetField(service, "_gameStarter", starter);
                SetField(service, "_moyvaUiState", state);
                SetField(service, "_lobbyPanelName", "LobbyPanel");

                service.Initialize();
                view.ClickCreateWorld();
                await Task.Yield();

                Assert.That(starter.StartCount, Is.EqualTo(1));
                Assert.That(navigation.OpenedMenus, Does.Not.Contain("LobbyPanel"));
                Assert.That(GameLaunchContext.Mode, Is.EqualTo(GameLaunchMode.MenuNewGame));
                Assert.That(GameLaunchContext.HasLocalPlayerRole, Is.True);
                Assert.That(GameLaunchContext.IsLocalPlayerHost, Is.True);
                Assert.That(GameLaunchContext.LocalPlayerId, Is.EqualTo("host"));
            }
            finally
            {
                service.Dispose();
                view.Dispose();
                GameLaunchContext.Reset();
            }
        }

        [Test]
        public async Task LobbyStart_WhenSingleHostAndCommandSyncMissing_StartsLocalGame()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);
            var starter = new FakeGameStarter();
            var lobby = CreateSingleHostLobby();
            var service = new LobbyPanelService();

            try
            {
                view.WorldName = "Lobby Smoke";
                view.Seed = 987654321;

                SetField(service, "_lobbyPanelViewController", view);
                SetField(service, "_lobbyService", new FakeLobbyService(lobby));
                SetField(service, "_navigation", new FakeNavigation());
                SetField(service, "_joinRoomPanelName", "JoinRoomPanel");
                SetField(service, "_lobbyPanelName", "LobbyPanel");
                SetField(service, "_gameplaySession", new GameplaySession());
                SetField(service, "_worldSetupViewController", view);
                SetField(service, "_gameStarter", starter);
                SetField(service, "_localPlayerId", "host");

                service.Initialize();
                Assert.That(view.StartGameButton.interactable, Is.True);

                view.ClickLobbyStart();
                await Task.Yield();

                Assert.That(starter.StartCount, Is.EqualTo(1));
            }
            finally
            {
                service.Dispose();
                view.Dispose();
            }
        }

        private static int Count(string source, string value)
        {
            var count = 0;
            var offset = 0;
            while ((offset = source.IndexOf(value, offset, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }
            return count;
        }

        private static LobbyRoom CreateSingleHostLobby()
        {
            return new LobbyRoom(
                "lobby",
                "CODE",
                "Smoke Lobby",
                4,
                false,
                "host",
                string.Empty,
                new List<LobbyPlayer>
                {
                    new LobbyPlayer("host", "Player", isHost: true)
                });
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private sealed class FakeUnityHtmlHost : IUnityHtmlHost
        {
            private readonly IUnityHtmlMotion _motion = new FakeUnityHtmlMotion();

            public IUnityHtmlMotion Motion => _motion;
            public bool UpdateRegion(string elementId, string html) => false;
            public bool SetValue(string elementId, string value) => false;
            public int DisposeCount { get; private set; }

            public UnityHtmlMountResult Mount(
                RectTransform root,
                UnityHtmlDocument document,
                IReadOnlyDictionary<string, object> globals = null)
            {
                return UnityHtmlMountResult.Success();
            }

            public void Unmount()
            {
            }

            public void Dispose()
            {
                DisposeCount++;
            }
        }

        private sealed class FakeUnityHtmlMotion : IUnityHtmlMotion
        {
            public void Play(string targetId, string preset, float duration, float delay)
            {
            }

            public void Stop(string targetId)
            {
            }
        }

        private sealed class FakeNavigation : INavigation
        {
            public List<string> OpenedMenus { get; } = new List<string>();
            public string CurrentMenu { get; private set; } = string.Empty;
            public event System.Action<NavigationChangeEventArgs> OnMenuChanged;

            public void Close(string menuName) => CloseLast();
            public void CloseForce(string menuName) => CloseLast();
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
            public void CloseLast()
            {
                var previous = CurrentMenu;
                CurrentMenu = string.Empty;
                OnMenuChanged?.Invoke(new NavigationChangeEventArgs
                {
                    PreviousMenu = previous,
                    CurrentMenu = CurrentMenu,
                    CurrentIsOpen = false
                });
            }

            public void CloseLastForce() => CloseLast();
        }

        private sealed class FakeConfirmationService : IConfirmationService
        {
            public void Show(ConfirmationRequest request) { }
            public void ForeceHide() { }
            public bool TryGetReqest(out ConfirmationRequest? request)
            {
                request = null;
                return false;
            }
        }

        private sealed class FakeGameStarter : IHomeMenuGameStarter
        {
            public int StartCount { get; private set; }

            public Task StartGameAsync(CancellationToken ct = default)
            {
                StartCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeLobbyService : ILobbyService
        {
            public FakeLobbyService(LobbyRoom current)
            {
                Current = current;
            }

            public LobbyRoom Current { get; private set; }
            public LobbyState State => Current?.State ?? LobbyState.Closed;
            public event System.Action<LobbyRoom> LobbyUpdated;
            public event System.Action<string> KickedFromLobby;
            public event System.Action<LobbyState> StateChanged;

            public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default) =>
                Task.FromResult(Current);

            public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default) =>
                Task.FromResult(Current);

            public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default) =>
                Task.FromResult(Current);

            public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, CancellationToken ct = default) =>
                Task.FromResult(Current);

            public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default) =>
                Task.FromResult<IReadOnlyList<LobbyRoom>>(new[] { Current });

            public Task LeaveAsync(CancellationToken ct = default)
            {
                Current = null;
                StateChanged?.Invoke(LobbyState.Closed);
                LobbyUpdated?.Invoke(null);
                return Task.CompletedTask;
            }

            public Task KickAsync(string playerId, CancellationToken ct = default) => Task.CompletedTask;
            public Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default) => Task.CompletedTask;
            public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default) => Task.CompletedTask;

            public void RaiseKicked(string reason) => KickedFromLobby?.Invoke(reason);
        }
    }
}
