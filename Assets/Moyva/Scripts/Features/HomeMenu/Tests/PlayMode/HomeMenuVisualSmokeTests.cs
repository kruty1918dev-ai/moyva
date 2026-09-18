using System.Collections;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Kruty1918.Moyva.Tests.HomeMenu.PlayMode
{
    /// <summary>
    /// Візуальний smoke: рендерить кожен маршрут меню на всіх класах в'юпорта
    /// у PNG під Temp/ai/smoke для подальшої людської/агентської перевірки.
    /// </summary>
    public sealed class HomeMenuVisualSmokeTests : HomeMenuSmokeFixture
    {
        private static readonly (int w, int h, string tag)[] Viewports =
        {
            (1920, 1080, "w1920"),
            (1366, 768, "w1366"),
            (960, 640, "compact960"),
            (560, 420, "tiny560"),
            (480, 860, "portrait480")
        };

        private static readonly string[] Routes =
        {
            "PlayModePanel", "ContinuePanel", "SelectMultiplayerType", "MultiplayerPanel",
            "CreateRoomPanel", "JoinRoomPanel", "WorldSetupPanel", "LobbyPanel",
            "KickPlayerPanel", "SettingsPanel"
        };

        [UnityTest]
        public IEnumerator RouteMatrixRendersAcrossViewports()
        {
            yield return LoadMenu();

            foreach (var (w, h, tag) in Viewports)
            {
                yield return SetViewport(w, h);
                yield return Capture($"{tag}-main");

                foreach (var route in Routes)
                {
                    yield return OpenRoute(route);
                    yield return Capture($"{tag}-{route}");
                }

                Nav.CloseLast();
                yield return WaitRealtime(0.4f);
            }

            ReportLogSummary();
            AssertNoFatalLogs();
            Assert.That(UnexpectedErrorCount(), Is.EqualTo(0), "Unexpected console errors during route matrix render.");
        }

        [UnityTest]
        public IEnumerator SettingsSectionsRenderPerSection()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            yield return OpenRoute("SettingsPanel");
            foreach (HomeMenuSettingsSection section in System.Enum.GetValues(typeof(HomeMenuSettingsSection)))
            {
                State.SetSettingsSection(section);
                yield return WaitRealtime(0.35f);
                yield return Capture($"settings-{section}");
            }

            // Controls section also on compact + portrait to check the workspace wrap.
            State.SetSettingsSection(HomeMenuSettingsSection.Controls);
            yield return SetViewport(960, 640);
            yield return WaitRealtime(0.45f);
            yield return Capture("compact960-settings-controls");
            yield return SetViewport(480, 860);
            yield return WaitRealtime(0.45f);
            yield return Capture("portrait480-settings-controls");

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator SeededLobbyDashboardAndRoomListRender()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            // --- Join Room: populated list incl. long names + private room ---
            yield return OpenRoute("JoinRoomPanel", 0.7f);
            View.ClearRoomList();
            View.AddRoomToList(new RoomInfo
            {
                RoomName = "Short Realm",
                HostDisplayName = "Oleks",
                ProviderType = NetworkProviderType.Lan,
                CurrentPlayers = 2,
                MaxPlayers = 4,
                JoinCode = "LANC0DE"
            });
            View.AddRoomToList(new RoomInfo
            {
                RoomName = "The Extremely Long Lobby Room Name That Should Wrap Or Truncate Gracefully In The List",
                HostDisplayName = "AVeryLongPlayerNicknameWithoutSpaces12345",
                ProviderType = NetworkProviderType.Relay,
                CurrentPlayers = 3,
                MaxPlayers = 6,
                JoinCode = "RELAY42",
                HasPassword = true
            });
            View.AddRoomToList(new RoomInfo
            {
                HostDisplayName = "HostOnlyName",
                ProviderType = NetworkProviderType.Relay,
                CurrentPlayers = 1,
                MaxPlayers = 4,
                LobbyId = "lobby-id-without-code-9f8e7d6c"
            });
            View.SetRoomListStatus(RoomListStatus.Ready, string.Empty);
            yield return WaitRealtime(0.5f);
            yield return Capture("joinroom-populated");

            // --- Lobby: seeded members, badges, meta, disabled-start reason ---
            yield return OpenRoute("LobbyPanel", 0.6f);
            View.ClearUsers();
            View.AddNewUser(new LobbyUserInfo { UserName = "HostPlayer", UserId = 0, IsHost = true });
            View.AddNewUser(new LobbyUserInfo { UserName = "YouHaveAnExtremelyLongNicknameForTesting", UserId = 1, IsLocal = true });
            View.AddNewUser(new LobbyUserInfo { UserName = "Guest_ua", UserId = 2 });
            View.RefreshUserList();
            View.SetLobbyStatus(new LobbyStatusInfo
            {
                IsHost = true,
                CanManagePlayers = true,
                CanStart = false,
                StartReason = "Waiting for players — at least 2 required to start.",
                RoomName = "Smoke Lobby",
                NetworkLabel = "Local network (LAN)",
                PrivacyLabel = "Public",
                PlayerCount = 3,
                MaxPlayers = 4,
                WorldSummary = "Smoke Realm — Continents, Normal"
            });
            View.SetInviteCode(new LobbyInviteCodePresentation("Invite Code", "MXV-4821", "Smoke Lobby"));
            yield return WaitRealtime(0.5f);
            yield return Capture("lobby-dashboard");

            yield return Capture("portrait480-lobby-precheck", 0f);
            yield return SetViewport(480, 860);
            yield return WaitRealtime(0.45f);
            yield return Capture("portrait480-lobby-dashboard");

            // --- Kick panel: seeded players ---
            yield return SetViewport(1366, 768);
            yield return OpenRoute("KickPlayerPanel", 0.6f);
            View.SetPlayers(new[]
            {
                new KickPlayerInfo { PlayerId = "p1", DisplayName = "HostPlayer", IsHost = true, StatusLabel = "Host" },
                new KickPlayerInfo { PlayerId = "p2", DisplayName = "YouHaveAnExtremelyLongNicknameForTesting", IsLocalPlayer = true, StatusLabel = "You" },
                new KickPlayerInfo { PlayerId = "p3", DisplayName = "Guest_ua", CanKick = true, StatusLabel = "Connected" }
            });
            yield return WaitRealtime(0.45f);
            yield return Capture("kickplayers-populated");

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator RoomListStatusStatesRender()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);
            yield return OpenRoute("JoinRoomPanel", 0.7f);

            View.ClearRoomList();
            View.SetRoomListStatus(RoomListStatus.Loading, "Fetching rooms...");
            yield return WaitRealtime(0.4f);
            yield return Capture("roomlist-loading");

            View.SetRoomListStatus(RoomListStatus.Empty, "No public rooms found. Try Refresh or join by invite code.");
            yield return WaitRealtime(0.4f);
            yield return Capture("roomlist-empty");

            View.SetRoomListStatus(RoomListStatus.Error, "Could not load rooms. Check the connection and try Refresh.");
            yield return WaitRealtime(0.4f);
            yield return Capture("roomlist-error");

            View.SetRoomListStatus(RoomListStatus.Joining, "Joining room...");
            yield return WaitRealtime(0.4f);
            yield return Capture("roomlist-joining");

            View.SetRoomListStatus(RoomListStatus.Empty, string.Empty);

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator ModalsAndOverlayRender()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);
            yield return OpenRoute("JoinRoomPanel", 0.6f);

            View.Show("Private Room", string.Empty);
            yield return WaitRealtime(0.35f);
            yield return Capture("modal-password");
            Bridge.CancelPassword();
            yield return WaitRealtime(0.2f);

            View.Show(new InfoMessage("Connection", "Lost connection to the host. You were returned to the menu."));
            yield return WaitRealtime(0.35f);
            yield return Capture("modal-info");
            Bridge.AcknowledgeInfo();
            yield return WaitRealtime(0.2f);

            View.Show(new ConfirmationRequest
            {
                LabelText = "Delete all saves?",
                MessageText = "This permanently removes every saved realm from this device."
            });
            yield return WaitRealtime(0.35f);
            yield return Capture("modal-confirm");
            Bridge.Cancel();
            yield return WaitRealtime(0.2f);

            var overlay = View.LoadOverlay(35f, 100f, "%");
            overlay?.SetStatus("Joining the lobby...");
            yield return WaitRealtime(0.35f);
            yield return Capture("overlay-status");
            View.StopOverlay(true);
            yield return WaitRealtime(0.3f);

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator ReducedMotionRendersWithoutAnimationAttributes()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            View.SetReducedMotion(true);
            yield return OpenRoute("PlayModePanel", 0.4f);
            yield return Capture("reduced-playmode", 0.1f);
            Nav.CloseLast();
            yield return WaitRealtime(0.3f);
            yield return Capture("reduced-main", 0.1f);
            View.SetReducedMotion(false);

            ReportLogSummary();
            AssertNoFatalLogs();
        }
    }
}
