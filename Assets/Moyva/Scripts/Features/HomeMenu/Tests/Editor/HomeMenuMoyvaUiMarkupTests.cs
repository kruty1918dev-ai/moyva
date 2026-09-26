using System.Xml.Linq;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>
    /// Smoke tests for the dynamic MoyvaUI markup: every route and settings section
    /// must produce well-formed markup regardless of the mounted scene.
    /// </summary>
    public sealed class HomeMenuMoyvaUiMarkupTests
    {
        private HomeMenuMoyvaUiState _state;
        private HomeMenuMoyvaUiViewController _view;

        [SetUp]
        public void SetUp()
        {
            _state = new HomeMenuMoyvaUiState();
            _view = new HomeMenuMoyvaUiViewController(_state);
        }

        [TearDown]
        public void TearDown()
        {
            _view.Dispose();
        }

        [TestCase("")]
        [TestCase("PlayModePanel")]
        [TestCase("ContinuePanel")]
        [TestCase("SelectMultiplayerType")]
        [TestCase("MultiplayerPanel")]
        [TestCase("CreateRoomPanel")]
        [TestCase("JoinRoomPanel")]
        [TestCase("WorldSetupPanel")]
        [TestCase("LobbyPanel")]
        [TestCase("KickPlayerPanel")]
        [TestCase("SettingsPanel")]
        public void BuildProducesWellFormedMarkupForRoute(string route)
        {
            _state.Open(route);

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.DoesNotThrow(() => XDocument.Parse(markup), $"Route '{route}' produced invalid markup.");
        }

        [TestCase((int)HomeMenuSettingsSection.General)]
        [TestCase((int)HomeMenuSettingsSection.Audio)]
        [TestCase((int)HomeMenuSettingsSection.Graphics)]
        [TestCase((int)HomeMenuSettingsSection.Controls)]
        public void BuildProducesWellFormedMarkupForSettingsSection(int sectionValue)
        {
            var section = (HomeMenuSettingsSection)sectionValue;
            _state.Open("SettingsPanel");
            _state.SetSettingsSection(section);

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-portrait");

            Assert.DoesNotThrow(() => XDocument.Parse(markup), $"Settings section '{section}' produced invalid markup.");
        }

        [Test]
        public void BuildProducesWellFormedMarkupWithEveryModal()
        {
            _state.Open("JoinRoomPanel");
            _view.ApplyOverlayPresentation(true, 10f, "%", "Loading");
            _view.Show(new InfoMessage("Title", "Message"));
            _view.Show("Private Room", "Wrong password");
            _view.Show(new ConfirmationRequest { LabelText = "Sure?", MessageText = "Really?" });

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-compact");

            Assert.DoesNotThrow(() => XDocument.Parse(markup));
        }

        [Test]
        public void ControlsSection_ContainsScrollSensitivitySlider()
        {
            _state.Open("SettingsPanel");
            _state.SetSettingsSection(HomeMenuSettingsSection.Controls);

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Contain("CommitScrollSensitivityValue"));
        }

        [Test]
        public void ControlsSection_ScrollSensitivityReflectsViewValue()
        {
            _state.Open("SettingsPanel");
            _state.SetSettingsSection(HomeMenuSettingsSection.Controls);
            _view.SetScrollSensitivity(1.25f);

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Contain("value=\"1.25\""));
        }

        [Test]
        public void SettingsToggles_PairLabelForWithToggleId()
        {
            _state.Open("SettingsPanel");
            _state.SetSettingsSection(HomeMenuSettingsSection.Graphics);

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Contain("<label className=\"control-copy\" for=\"#vsync\">"));
            Assert.That(markup, Does.Contain("<toggle id=\"vsync\""));
        }

        [Test]
        public void SettingsToggles_DisabledRowsEmitPlainViewNotLabel()
        {
            _state.Open("SettingsPanel");
            _state.SetSettingsSection(HomeMenuSettingsSection.Graphics);
            _view.SetGraphicsInteractable(false);

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Not.Contain("for=\"#vsync\""));
            Assert.That(markup, Does.Contain("<toggle id=\"vsync\" className=\"menu-toggle\" checked=\"false\" onChange=\"Globals.moyvaMenu.SetVSync(event)\" disabled=\"true\">"));
        }

        [Test]
        public void MarkupEscapesUserControlledStrings()
        {
            _view.SetInviteCode(new LobbyInviteCodePresentation("Invite Code", "<b>&\"</b>", "<room>"));
            _state.Open("LobbyPanel");

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.DoesNotThrow(() => XDocument.Parse(markup));
            Assert.That(markup, Does.Not.Contain("<b>"));
        }

        [Test]
        public void WorldSetup_Solo_HidesDifficultyAndBotFields()
        {
            _state.SetPlayFlow(HomeMenuPlayFlow.Solo);
            _state.Open("WorldSetupPanel");

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Not.Contain("SetDifficultyValue"),
                "Solo sandbox must not offer the AI/match difficulty field.");
            Assert.That(markup, Does.Not.Contain("SetBotDifficultyValue"));
        }

        [Test]
        public void WorldSetup_Multiplayer_KeepsDifficultyField()
        {
            _state.SetPlayFlow(HomeMenuPlayFlow.Multiplayer);
            _state.Open("WorldSetupPanel");

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Contain("SetDifficultyValue"));
            Assert.That(markup, Does.Not.Contain("SetBotDifficultyValue"));
        }

        [Test]
        public void WorldSetup_HumanVsBot_ShowsBotDifficultyOnly()
        {
            _state.SetPlayFlow(HomeMenuPlayFlow.HumanVsBot);
            _state.Open("WorldSetupPanel");

            var markup = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(markup, Does.Contain("SetBotDifficultyValue"));
            Assert.That(markup, Does.Not.Contain("SetDifficultyValue"));
        }

        [Test]
        public void ReducedMotionRemovesDeclarativeAnimations()
        {
            _state.Open("JoinRoomPanel");
            _view.SetRoomListStatus(RoomListStatus.Loading, "Fetching rooms...");

            var animated = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");
            Assert.That(animated, Does.Contain("data-motion"));

            _view.SetReducedMotion(true);
            var reduced = HomeMenuMoyvaUiMarkup.Build(_state, _view, "vp-wide");

            Assert.That(reduced, Does.Not.Contain("data-motion"));
        }
    }
}
