using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>
    /// Focused tests for the new Home Menu status surfaces: overlay stage text,
    /// room-list status, lobby dashboard state, invite copy feedback and
    /// reduced-motion state propagation.
    /// </summary>
    public sealed class HomeMenuMoyvaUiStatusTests
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

        [Test]
        public void OverlayStatusPropagatesStageText()
        {
            var overlay = _view.LoadOverlay(0f, 100f, "%");
            var observed = string.Empty;
            void OnChanged(OverlayLoaderResult result) => observed = result.Status;
            OverlayLoaderResult.CurrentChanged += OnChanged;
            try
            {
                overlay.SetStatus("Joining the lobby...");

                Assert.That(_view.OverlayStatus, Is.EqualTo("Joining the lobby..."));
                Assert.That(observed, Is.EqualTo("Joining the lobby..."));
            }
            finally
            {
                OverlayLoaderResult.CurrentChanged -= OnChanged;
            }
        }

        [Test]
        public void RoomListStatusTransitionsAreExposed()
        {
            _view.SetRoomListStatus(RoomListStatus.Loading, "Fetching rooms...");
            Assert.That(_view.RoomListState, Is.EqualTo(RoomListStatus.Loading));
            Assert.That(_view.RoomListMessage, Is.EqualTo("Fetching rooms..."));

            _view.SetRoomListStatus(RoomListStatus.Error, "Could not load rooms.");
            Assert.That(_view.RoomListState, Is.EqualTo(RoomListStatus.Error));
            Assert.That(_view.RoomListMessage, Is.EqualTo("Could not load rooms."));

            _view.SetRoomListStatus(RoomListStatus.Ready, string.Empty);
            Assert.That(_view.RoomListState, Is.EqualTo(RoomListStatus.Ready));
        }

        [Test]
        public void LobbyStatusPopulatesDashboardFields()
        {
            _view.SetLobbyStatus(new LobbyStatusInfo
            {
                IsHost = true,
                CanManagePlayers = true,
                CanStart = false,
                StartReason = "Waiting for players.",
                RoomName = "Test Lobby",
                NetworkLabel = "LAN",
                PrivacyLabel = "Public",
                PlayerCount = 1,
                MaxPlayers = 4,
                WorldSummary = "Realm - Continents, Normal"
            });

            Assert.That(_view.LobbyStatus.IsHost, Is.True);
            Assert.That(_view.LobbyStatus.CanManagePlayers, Is.True);
            Assert.That(_view.LobbyStatus.StartReason, Is.EqualTo("Waiting for players."));
            Assert.That(_view.LobbyStatus.MaxPlayers, Is.EqualTo(4));
        }

        [Test]
        public void CopyInviteCodeSetsFeedbackOnlyWhenCodeExists()
        {
            _view.ClearLobbyInvateCode();
            _view.CopyInviteCode();
            Assert.That(_view.InviteCopied, Is.False, "No invite code -> no copied feedback.");

            _view.SetInviteCode(new LobbyInviteCodePresentation("Invite Code", "ABC123"));
            _view.CopyInviteCode();
            Assert.That(_view.InviteCopied, Is.True);

            _view.SetInviteCode(new LobbyInviteCodePresentation("Invite Code", "XYZ789"));
            Assert.That(_view.InviteCopied, Is.False, "A fresh invite resets the copied flag.");
        }

        [Test]
        public void ReducedMotionFlagReachesState()
        {
            _view.SetReducedMotion(true);
            Assert.That(_state.ReducedMotion, Is.True);
            Assert.That(_view.ReducedMotion, Is.False, "No motion service injected; state still records the choice.");
        }

        [Test]
        public void InteractionDepthSuppressesRendersUntilReleased()
        {
            _state.ConsumeDirty();
            _state.BeginInteraction();
            _state.MarkDirty();
            Assert.That(_state.ConsumeDirty(), Is.False, "Renders stay paused while a control is focused.");

            _state.EndInteraction();
            Assert.That(_state.ConsumeDirty(), Is.True, "Queued changes flush when the interaction ends.");
            Assert.That(_state.ConsumeDirty(), Is.False, "Dirty flag is consumed once.");
        }

        [Test]
        public void CreateRoomBlockReasonExplainsWhyNextIsDisabled()
        {
            _view.PreviewRoomName(string.Empty);
            Assert.That(_view.CreateRoomBlockReason, Is.Not.Empty);
            Assert.That(_view.NextButton.interactable, Is.False);

            _view.SetRoomName("Realm Lobby");
            _view.SetRoomPrivate(true);
            Assert.That(_view.CreateRoomBlockReason, Is.Not.Empty);

            _view.SetRoomPassword("secret");
            Assert.That(_view.CreateRoomBlockReason, Is.Empty);
            Assert.That(_view.NextButton.interactable, Is.True);
        }
    }
}
