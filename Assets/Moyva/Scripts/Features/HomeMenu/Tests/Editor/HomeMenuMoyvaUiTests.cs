using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.Shared.Graphics;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class HomeMenuMoyvaUiTests
    {
        [Test]
        public void Build_CreateRoomRoute_RendersEditableFields()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);

            try
            {
                state.Open("CreateRoomPanel");
                view.SetRoomName("Test Room");
                view.SetRoomPassword("secret");

                var html = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-wide");

                Assert.That(html, Does.Contain("<input"));
                Assert.That(html, Does.Contain("Globals.moyvaMenu.SetRoomName(event)"));
                Assert.That(html, Does.Contain("Globals.moyvaMenu.SetRoomPassword(event)"));
                Assert.That(html, Does.Contain("Test Room"));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void Build_WorldAndSettingsRoutes_RenderFullControlSet()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);

            try
            {
                state.Open("WorldSetupPanel");
                var worldHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-wide");

                Assert.That(worldHtml, Does.Contain("Globals.moyvaMenu.MapPangaea()"));
                Assert.That(worldHtml, Does.Contain("Globals.moyvaMenu.MapHighlands()"));
                Assert.That(worldHtml, Does.Contain("Globals.moyvaMenu.DifficultyHard()"));
                Assert.That(worldHtml, Does.Contain("Globals.moyvaMenu.DifficultyInsane()"));

                state.Open("SettingsPanel");
                var settingsHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-wide");

                Assert.That(settingsHtml, Does.Contain("<scroll className=\"navigation-list\""));
                Assert.That(settingsHtml, Does.Contain("Globals.moyvaMenu.ToggleDynamicRenderScale()"));
                Assert.That(settingsHtml, Does.Contain("Globals.moyvaMenu.SetMipmapValue(event)"));
                Assert.That(settingsHtml, Does.Contain("Globals.moyvaMenu.SetAntiAliasingSliderValue(event)"));
                Assert.That(settingsHtml, Does.Contain("Globals.moyvaMenu.SetLodBiasValue(event)"));
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
                var mainHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-wide");
                state.Open("SettingsPanel");
                var settingsHtml = HomeMenuMoyvaUiMarkup.Build(state, view, "vp-wide");

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
        public void Bridge_EditCommands_UpdateViewStateAndRaiseEvents()
        {
            var state = new HomeMenuMoyvaUiState();
            var view = new HomeMenuMoyvaUiViewController(state);
            var bridge = new HomeMenuMoyvaUiBridge(null, null, view);
            var settingsChanged = 0;
            var joinCodeChanged = 0;
            var dynamicScaleChanged = false;
            var antiAliasing = -1;
            var renderScale = 0f;
            var frameRate = 0;
            string playerName = null;

            try
            {
                view.OnSettingsChanged += () => settingsChanged++;
                view.OnJoinCodeChanged += () => joinCodeChanged++;
                view.OnPlayerNameChanged += value => playerName = value;
                view.OnDynamicRenderScaleChanged += value => dynamicScaleChanged = value;
                view.OnAntiAliasingChanged += value => antiAliasing = value;
                view.OnRenderScaleChanged += value => renderScale = value;
                view.OnTargetFrameRateChanged += value => frameRate = value;

                bridge.SetRoomName("Alpha");
                bridge.SetRoomPassword("pw");
                bridge.SetWorldName("Realm");
                bridge.SetSeed("12345");
                bridge.SetJoinCode("  ABCD  ");
                bridge.SetPlayerName("Oleks");
                bridge.ToggleDynamicRenderScale();
                bridge.AntiAliasing4x();
                bridge.SetRenderScaleValue(0.55f);
                bridge.SetFrameRateValue(144f);

                Assert.That(view.RoomName, Is.EqualTo("Alpha"));
                Assert.That(view.Password, Is.EqualTo("pw"));
                Assert.That(view.WorldName, Is.EqualTo("Realm"));
                Assert.That(view.Seed, Is.EqualTo(12345));
                Assert.That(view.JoinCode, Is.EqualTo("ABCD"));
                Assert.That(view.PlayerName, Is.EqualTo("Oleks"));
                Assert.That(view.DynamicRenderScale, Is.True);
                Assert.That(dynamicScaleChanged, Is.True);
                Assert.That(view.AntiAliasing, Is.EqualTo(4));
                Assert.That(antiAliasing, Is.EqualTo(4));
                Assert.That(view.RenderScale, Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(renderScale, Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(view.TargetFrameRate, Is.EqualTo(144));
                Assert.That(frameRate, Is.EqualTo(144));
                Assert.That(playerName, Is.EqualTo("Oleks"));
                Assert.That(settingsChanged, Is.EqualTo(2));
                Assert.That(joinCodeChanged, Is.EqualTo(1));
            }
            finally
            {
                view.Dispose();
            }
        }

        [Test]
        public void GraphicsSettingsData_WithDynamicRenderScale_PreservesRequestedValue()
        {
            var settings = GraphicsSettingsData.CreateDefault().WithDynamicRenderScale(true);

            Assert.That(settings.DynamicRenderScale, Is.True);
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
    }
}
