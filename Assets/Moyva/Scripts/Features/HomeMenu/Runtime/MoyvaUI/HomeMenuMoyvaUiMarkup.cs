using System;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class HomeMenuMoyvaUiMarkup
    {
        private const string WorldSizeOptions = "Small|Medium|Large";
        private const string MapTypeOptions = "Continents|Pangaea|Islands|Highlands|Desert|Random";
        private const string DifficultyOptions = "Easy|Normal|Hard|Insane";
        private const string NetworkOptions = "Online (Relay)|Local network (LAN)";
        private const string GraphicsProfileOptions = "Automatic|Performance|Balanced|Quality|Custom";
        private const string FrameRateOptions = "30 FPS|45 FPS|60 FPS|90 FPS|120 FPS|144 FPS|240 FPS";
        private const string TextureQualityOptions = "High|Medium|Low|Very low";
        private const string AntiAliasingOptions = "Off|2x MSAA|4x MSAA";

        public const string NavRegionId = "moyva-nav";
        public const string BrandRegionId = "moyva-brand";

        public static string Build(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view, string viewportClass)
        {
            var route = ResolveRoute(state);
            var sb = new StringBuilder(18000);
            sb.Append("<view className=\"moyva-ui-app ").Append(E(viewportClass)).Append(' ').Append(RouteClass(route));
            if (route == "SettingsPanel" && state.SettingsSection == HomeMenuSettingsSection.Controls) sb.Append(" controls-page");
            sb.Append("\">");
            sb.Append("<view className=\"background-veil\"></view><view className=\"shell-content\">");
            sb.Append("<view id=\"").Append(BrandRegionId).Append("\" className=\"brand-panel\">");
            AppendBrandContent(sb, view);
            sb.Append("</view>");
            sb.Append("<view id=\"").Append(NavRegionId).Append("\" className=\"navigation-panel\">");
            AppendRoute(sb, state, route, view, !string.Equals(route, "Main", StringComparison.Ordinal));
            sb.Append("</scroll></view></view>");
            AppendFooter(sb, view);
            AppendModals(sb, view);
            sb.Append("</view>");
            return sb.ToString();
        }

        public static string BuildRouteMarkup(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view)
        {
            var route = ResolveRoute(state);
            var sb = new StringBuilder(16000);
            AppendRoute(sb, state, route, view, !string.Equals(route, "Main", StringComparison.Ordinal));
            return sb.ToString();
        }

        public static string BuildBrandMarkup(HomeMenuMoyvaUiViewController view)
        {
            var sb = new StringBuilder(1024);
            AppendBrandContent(sb, view);
            return sb.ToString();
        }

        public static string BuildModalsMarkup(HomeMenuMoyvaUiViewController view)
        {
            var sb = new StringBuilder(1024);
            AppendModals(sb, view);
            return sb.ToString();
        }

        private static string ResolveRoute(HomeMenuMoyvaUiState state)
            => string.IsNullOrWhiteSpace(state.CurrentRoute) ? "Main" : state.CurrentRoute.Trim();

        private static void AppendRoute(
            StringBuilder sb,
            HomeMenuMoyvaUiState state,
            string route,
            HomeMenuMoyvaUiViewController view,
            bool showBack)
        {
            switch (route)
            {
                case "PlayModePanel":
                    Header(sb, view.T("PLAY"), view.T("Choose how to play"), showBack, view);
                    if (view.Slots.Count > 0)
                        Button(sb, view.T("SAVE"), view.T("CONTINUE"), view.TF("Resume one of {0} saved realms.", view.Slots.Count), "Globals.moyvaMenu.Continue()", true);
                    Button(sb, view.T("SOLO"), view.T("SOLO SANDBOX"), view.T("Build a world without an opponent."), "Globals.moyvaMenu.Solo()", view.Slots.Count == 0);
                    Button(sb, view.T("NET"), view.T("MULTIPLAYER"), view.T("Host or join an online or LAN lobby."), "Globals.moyvaMenu.Multiplayer()");
                    Button(sb, view.T("AI"), view.T("Bot"), view.T("Start a turn-based match against a computer opponent."), "Globals.moyvaMenu.PlayVsBot()");
                    break;
                case "ContinuePanel":
                    Header(sb, view.T("CONTINUE"), view.T("Saved realms"), showBack, view);
                    if (view.Slots.Count == 0)
                        Empty(sb, view.T("No saved worlds found."));
                    for (var i = 0; i < view.Slots.Count; i++)
                    {
                        var slot = view.Slots[i];
                        Button(sb, $"{slot.SlotIndex:D2}", slot.SlotName, slot.LastModified.ToString("g", CultureInfo.CurrentCulture), $"Globals.moyvaMenu.SelectSlot({i})", i == 0);
                    }
                    break;
                case "SelectMultiplayerType":
                case "MultiplayerPanel":
                    Header(sb, view.T("MULTIPLAYER"), view.T("Host or join a realm"), showBack, view);
                    SectionIntro(sb, view.T("CONNECTION"), view.T("Choose the network used for this session."));
                    Select(sb, view.T("Network"), view.TOptions(NetworkOptions), view.SelectedMode == NetworkProviderType.Lan ? 1 : 0, "Globals.moyvaMenu.SetNetworkMode(event)");
                    sb.Append("<view className=\"choice-grid\">");
                    Button(sb, view.T("HOST"), view.T("CREATE LOBBY"), view.T("Configure a room and invite players."), "Globals.moyvaMenu.CreateSelected()", true);
                    Button(sb, view.T("JOIN"), view.T("FIND LOBBY"), view.T("Browse rooms or enter an invite code."), "Globals.moyvaMenu.JoinSelected()");
                    sb.Append("</view>");
                    break;
                case "CreateRoomPanel":
                    Header(sb, view.T("LOBBY"), view.T(view.CreateRoomTitle), showBack, view);
                    sb.Append("<view className=\"form-grid\">");
                    Input(sb, view.T("Room name"), view.RoomName, view.T("Moyva Lobby"), "Globals.moyvaMenu.PreviewRoomName(event)", "Globals.moyvaMenu.CommitRoomName(event)", 48, true, "Standard", "full");
                    Toggle(sb, view.T("Private room"), view.T("Require a password to enter."), !view.IsPublic, "Globals.moyvaMenu.SetPrivate(event)");
                    Stepper(sb, view.T("Players"), view.T("Lobby capacity"), view.MaxPlayers, "Globals.moyvaMenu.MaxPlayersMinus()", "Globals.moyvaMenu.MaxPlayersPlus()");
                    if (!view.IsPublic)
                        Input(sb, view.T("Password"), view.Password, view.T("Optional password"), "Globals.moyvaMenu.PreviewRoomPassword(event)", "Globals.moyvaMenu.CommitRoomPassword(event)", 48, true, "Password", "full");
                    sb.Append("</view>");
                    ActionButton(sb, view.T(view.CreateRoomNextText), view.T("Continue to world setup"), "Globals.moyvaMenu.CreateRoom()", view.NextButton == null || view.NextButton.interactable);
                    break;
                case "JoinRoomPanel":
                    Header(sb, view.T("JOIN"), view.T("Available rooms"), showBack, view);
                    Input(sb, view.T("Invite code"), view.JoinCode, view.T("Lobby code or room id"), "Globals.moyvaMenu.PreviewJoinCode(event)", "Globals.moyvaMenu.CommitJoinCode(event)", 64, true, "Standard", "full");
                    sb.Append("<view className=\"inline-actions\">");
                    CompactButton(sb, view.T("REFRESH"), "Globals.moyvaMenu.RefreshRooms()");
                    CompactButton(sb, view.T("JOIN BY CODE"), "Globals.moyvaMenu.JoinTypedRoom()", true, view.JoinInteractable);
                    sb.Append("</view><view className=\"room-list\">");
                    if (view.Rooms.Count == 0)
                        Empty(sb, view.T("No public rooms available."));
                    for (var i = 0; i < view.Rooms.Count; i++)
                    {
                        var room = view.Rooms[i];
                        Button(sb, view.T(room.ProviderLabel), room.HostOrRoomDisplayName, $"{room.CurrentPlayers}/{room.MaxPlayers} - {view.T(room.DisplayIdentifier)}", $"Globals.moyvaMenu.SelectRoom({i})");
                    }
                    sb.Append("</view>");
                    break;
                case "WorldSetupPanel":
                    Header(sb, view.T("WORLD SETUP"), state.PlayFlow == HomeMenuPlayFlow.HumanVsBot ? view.T("Match Setup — Human vs Bot") : state.PlayFlow == HomeMenuPlayFlow.Solo ? view.T("Create a sandbox") : view.T("Shape the campaign"), showBack, view);
                    sb.Append("<view className=\"form-grid\">");
                    Input(sb, view.T("World name"), view.WorldName, view.T("New World"), "Globals.moyvaMenu.PreviewWorldName(event)", "Globals.moyvaMenu.CommitWorldName(event)", 48, true, "Standard", "full");
                    Input(sb, view.T("Seed"), view.Seed.ToString(CultureInfo.InvariantCulture), view.T("World seed"), string.Empty, "Globals.moyvaMenu.CommitSeed(event)", 12, true, "IntegerNumber");
                    CompactControlButton(sb, view.T("RANDOMIZE SEED"), "Globals.moyvaMenu.RandomSeed()", view);
                    Select(sb, view.T("World size"), view.TOptions(WorldSizeOptions), (int)view.Size, "Globals.moyvaMenu.SetWorldSizeValue(event)");
                    Select(sb, view.T("Map type"), view.TOptions(MapTypeOptions), (int)view.MapType, "Globals.moyvaMenu.SetMapTypeValue(event)");
                    if (state.PlayFlow == HomeMenuPlayFlow.HumanVsBot)
                        Select(sb, view.T("Bot difficulty"), view.TOptions(view.BotDifficultyOptions), view.SelectedBotDifficultyIndex, "Globals.moyvaMenu.SetBotDifficultyValue(event)");
                    else
                        Select(sb, view.T("Difficulty"), view.TOptions(DifficultyOptions), (int)view.Difficulty, "Globals.moyvaMenu.SetDifficultyValue(event)");
                    sb.Append("</view>");
                    ActionButton(
                        sb,
                        state.PlayFlow == HomeMenuPlayFlow.HumanVsBot ? view.T("START") : state.PlayFlow == HomeMenuPlayFlow.Solo ? view.T("START SANDBOX") : view.T("CREATE LOBBY"),
                        state.PlayFlow != HomeMenuPlayFlow.Multiplayer ? view.T("Generate the world and enter the game.") : view.T("Create the room and wait for players."),
                        "Globals.moyvaMenu.CreateWorld()",
                        view.CreateWorldButton == null || view.CreateWorldButton.interactable);
                    break;
                case "LobbyPanel":
                    Header(sb, view.T("LOBBY"), view.T(view.LobbyDisplayName), showBack, view);
                    Stat(sb, view.T("Invite"), view.T(view.InviteCodeText));
                    sb.Append("<view className=\"player-list\">");
                    if (view.LobbyUsers.Count == 0)
                        Empty(sb, view.T("Waiting for players."));
                    for (var i = 0; i < view.LobbyUsers.Count; i++)
                        Stat(sb, $"P{i + 1}", view.LobbyUsers[i].UserName);
                    sb.Append("</view>");
                    ActionButton(sb, view.T("START GAME"), view.T("Lock the lobby and launch gameplay."), "Globals.moyvaMenu.StartGame()", view.StartGameButton == null || view.StartGameButton.interactable);
                    CompactButton(sb, view.T("LEAVE LOBBY"), "Globals.moyvaMenu.LeaveLobby()");
                    break;
                case "KickPlayerPanel":
                    Header(sb, view.T("MANAGE PLAYERS"), view.T("Lobby players"), showBack, view);
                    if (!string.IsNullOrWhiteSpace(view.KickStatus))
                        Empty(sb, view.T(view.KickStatus));
                    for (var i = 0; i < view.KickPlayers.Count; i++)
                    {
                        var player = view.KickPlayers[i];
                        Button(sb, player.CanKick ? view.T("KICK") : view.T("HOST"), player.DisplayName, view.T(player.StatusLabel), $"Globals.moyvaMenu.KickPlayer({i})", false, view.KickInteractable && player.CanKick);
                    }
                    sb.Append("<view className=\"inline-actions\">");
                    CompactButton(sb, view.T("REFRESH"), "Globals.moyvaMenu.RefreshKickPlayers()");
                    CompactButton(sb, view.T("CLOSE"), "Globals.moyvaMenu.CloseKickPlayers()");
                    sb.Append("</view>");
                    break;
                case "SettingsPanel":
                    Header(sb, view.T("SETTINGS"), view.T("Game preferences"), showBack, view);
                    SettingsTabs(sb, state.SettingsSection, view);
                    AppendSettings(sb, state.SettingsSection, view);
                    break;
                default:
                    Header(sb, view.T("MAIN MENU"), view.T("A realm awaits"), false, view);
                    Button(sb, "01", view.T("PLAY"), view.T("Choose a mode and start a realm."), "Globals.moyvaMenu.Play()", true);
                    Button(sb, "02", view.T("SETTINGS"), view.T("Audio, graphics and player profile."), "Globals.moyvaMenu.Settings()");
                    Button(sb, "03", view.T("QUIT"), view.T("Close Moyva."), "Globals.moyvaMenu.Exit()");
                    break;
            }
        }

        private static void AppendSettings(StringBuilder sb, HomeMenuSettingsSection section, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"settings-content\">");
            switch (section)
            {
                case HomeMenuSettingsSection.Audio:
                    SectionIntro(sb, view.T("AUDIO"), view.T("Mix game sound without leaving the menu."));
                    sb.Append("<view className=\"settings-grid\">");
                    Toggle(sb, view.T("Mute all audio"), view.T("Disable every audio bus."), view.IsMuted, "Globals.moyvaMenu.SetMuted(event)", "full");
                    Slider(sb, view.T("Master"), view.MasterVolume, 0f, 1f, "Globals.moyvaMenu.CommitMasterValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, view.T("Music"), view.MusicVolume, 0f, 1f, "Globals.moyvaMenu.CommitMusicValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, view.T("Sound effects"), view.SfxVolume, 0f, 1f, "Globals.moyvaMenu.CommitSfxValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, view.T("Interface"), view.UiVolume, 0f, 1f, "Globals.moyvaMenu.CommitUiValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, view.T("Ambience"), view.AmbienceVolume, 0f, 1f, "Globals.moyvaMenu.CommitAmbienceValue(event)", "percent", view.SettingsInteractable);
                    sb.Append("</view>");
                    break;
                case HomeMenuSettingsSection.Graphics:
                    SectionIntro(sb, view.T("GRAPHICS"), view.T("Choose a profile, then fine-tune individual options."));
                    sb.Append("<view className=\"settings-grid\">");
                    Select(sb, view.T("Quality profile"), view.TOptions(GraphicsProfileOptions), (int)view.GraphicsProfile, "Globals.moyvaMenu.SetGraphicsProfileValue(event)", view.GraphicsSettingsInteractable);
                    Select(sb, view.T("Frame limit"), view.TOptions(FrameRateOptions), FrameRateIndex(view.TargetFrameRate), "Globals.moyvaMenu.SetFrameRateOption(event)", view.GraphicsSettingsInteractable);
                    Slider(sb, view.T("Render scale"), view.RenderScale, 0.42f, 1f, "Globals.moyvaMenu.CommitRenderScaleValue(event)", "percent", view.GraphicsSettingsInteractable);
                    Select(sb, view.T("Texture quality"), view.TOptions(TextureQualityOptions), view.TextureMipmapLimit, "Globals.moyvaMenu.SetTextureQualityOption(event)", view.GraphicsSettingsInteractable);
                    Select(sb, view.T("Anti-aliasing"), view.TOptions(AntiAliasingOptions), AntiAliasingIndex(view.AntiAliasing), "Globals.moyvaMenu.SetAntiAliasingOption(event)", view.GraphicsSettingsInteractable);
                    Slider(sb, view.T("Level of detail"), view.LodBias, 0.4f, 2f, "Globals.moyvaMenu.CommitLodBiasValue(event)", "decimal1", view.GraphicsSettingsInteractable, "x");
                    Toggle(sb, view.T("Vertical sync"), view.T("Match frames to the display refresh."), view.VSync, "Globals.moyvaMenu.SetVSync(event)", null, view.GraphicsSettingsInteractable);
                    Toggle(sb, view.T("Shadows"), view.T("Render realtime world shadows."), view.Shadows, "Globals.moyvaMenu.SetShadows(event)", null, view.GraphicsSettingsInteractable);
                    Toggle(sb, view.T("Anisotropic filtering"), view.T("Keep angled textures sharp."), view.AnisotropicFiltering, "Globals.moyvaMenu.SetAnisotropic(event)", null, view.GraphicsSettingsInteractable);
                    sb.Append("</view>");
                    CompactButton(sb, view.T("RESET GRAPHICS"), "Globals.moyvaMenu.ResetGraphics()", false, view.GraphicsSettingsInteractable);
                    break;
                case HomeMenuSettingsSection.Controls:
                    HomeMenuControlsMarkup.Append(sb, view);
                    sb.Append("<view className=\"settings-grid\">");
                    Slider(sb, view.T("Mouse sensitivity"), view.MouseSensitivity, 0.25f, 3f, "Globals.moyvaMenu.CommitMouseSensitivityValue(event)", "decimal1", view.SettingsInteractable, "x");
                    Slider(sb, view.T("Movement speed"), view.MovementSpeed, 0.25f, 3f, "Globals.moyvaMenu.CommitMovementSpeedValue(event)", "decimal1", view.SettingsInteractable, "x");
                    Slider(sb, view.T("Orbit speed"), view.OrbitSpeed, 0.25f, 3f, "Globals.moyvaMenu.CommitOrbitSpeedValue(event)", "decimal1", view.SettingsInteractable, "x");
                    Slider(sb, view.T("Zoom speed"), view.ZoomSpeed, 0.25f, 3f, "Globals.moyvaMenu.CommitZoomSpeedValue(event)", "decimal1", view.SettingsInteractable, "x");
                    sb.Append("</view>");
                    CompactButton(sb, view.T("RESET CONTROLS"), "Globals.moyvaMenu.ResetControls()");
                    break;
                default:
                    SectionIntro(sb, view.T("GENERAL"), view.T("Player identity and local data."));
                    sb.Append("<view className=\"settings-grid\">");
                    Input(sb, view.T("Player name"), view.PlayerName, view.T("Player"), "Globals.moyvaMenu.PreviewPlayerName(event)", "Globals.moyvaMenu.CommitPlayerName(event)", 32, view.SettingsInteractable, "Standard", "full");
                    Select(sb, view.T("Language"), view.LanguageOptions, view.LanguageIndex, "Globals.moyvaMenu.SetLanguageValue(event)", view.SettingsInteractable);
                    sb.Append("<view className=\"danger-zone full\"><view className=\"control-copy\"><text className=\"control-label\">").Append(view.T("LOCAL SAVES")).Append("</text><text className=\"control-help\">").Append(view.T("Permanently remove all saved realms from this device.")).Append("</text></view>");
                    CompactButton(sb, view.T("DELETE SAVES"), "Globals.moyvaMenu.DeleteSaves()", false, view.SettingsInteractable, "danger");
                    sb.Append("</view></view>");
                    break;
            }
            sb.Append("</view>");
        }

        private static void AppendBrandContent(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"kicker-row\"><view className=\"kicker-mark\"></view><text className=\"kicker\">").Append(view.T("TURN-BASED STRATEGY")).Append("</text></view>");
            sb.Append("<text className=\"brand-title\">MOYVA</text><text className=\"brand-subtitle\">").Append(view.T("Shape a realm that remembers every decision.")).Append("</text>");
            sb.Append("<view className=\"brand-rule\"><view className=\"brand-rule-accent\"></view></view><text className=\"brand-copy\">").Append(view.T("Build your kingdom, gather allies, and lead your people through a living procedural world.")).Append("</text>");
            sb.Append("<view className=\"world-status\"><view className=\"status-dot\"></view><view className=\"status-copy\"><text className=\"status-label\">").Append(view.T("WORLD PREVIEW")).Append("</text><text className=\"status-value\">")
                .Append(E(view.WorldName)).Append(" - ").Append(view.T("seed")).Append(' ').Append(view.Seed.ToString(CultureInfo.InvariantCulture)).Append("</text></view></view>");
        }

        private static void AppendModals(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            if (view.OverlayVisible)
            {
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">").Append(view.T("LOADING")).Append("</text>");
                if (!string.IsNullOrWhiteSpace(view.OverlayStatus))
                    sb.Append("<text className=\"modal-copy\">").Append(E(view.OverlayStatus)).Append("</text>");
                sb.Append("<text className=\"modal-copy\">")
                    .Append(Math.Round(view.OverlayProgress)).Append(E(view.OverlaySuffix)).Append("</text></view></view>");
            }

            if (view.InfoVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">").Append(E(view.T(view.CurrentInfo.Title))).Append("</text><text className=\"modal-copy\">")
                    .Append(E(view.T(view.CurrentInfo.Message))).Append("</text>").Append(ModalButton(view.T("OK"), "Globals.moyvaMenu.AcknowledgeInfo()"))
                    .Append("</view></view>");

            if (view.PasswordVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">").Append(view.T("PASSWORD")).Append("</text><text className=\"modal-copy\">")
                    .Append(E(view.PasswordRoomDisplayName)).Append("</text>")
                    .Append(InputMarkup(view.T("Password"), view.PasswordValue, view.T("Password"), "Globals.moyvaMenu.PreviewPasswordValue(event)", "Globals.moyvaMenu.CommitPasswordValue(event)", 48, true, "Password", "full"))
                    .Append("<text className=\"modal-copy error\">").Append(E(view.T(view.PasswordErrorText))).Append("</text><view className=\"modal-actions\">")
                    .Append(ModalButton(view.T("JOIN"), "Globals.moyvaMenu.ConfirmPassword()"))
                    .Append(ModalButton(view.T("CANCEL"), "Globals.moyvaMenu.CancelPassword()"))
                    .Append("</view></view></view>");

            if (view.ConfirmationVisible && view.CurrentConfirmation.HasValue)
            {
                var request = view.CurrentConfirmation.Value;
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">")
                    .Append(E(view.T(request.LabelText))).Append("</text><text className=\"modal-copy\">")
                    .Append(E(view.T(request.MessageText))).Append("</text><view className=\"modal-actions\">")
                    .Append(ModalButton(view.T("YES"), "Globals.moyvaMenu.Confirm()"))
                    .Append(ModalButton(view.T("NO"), "Globals.moyvaMenu.Cancel()"))
                    .Append("</view></view></view>");
            }
        }

        private static void Header(StringBuilder sb, string kicker, string title, bool showBack, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"navigation-heading\"><view className=\"navigation-heading-row\"><view className=\"navigation-heading-copy\"><text className=\"navigation-kicker\">")
                .Append(E(kicker)).Append("</text><text className=\"navigation-title\">").Append(E(title)).Append("</text></view>");
            if (showBack)
                sb.Append("<button className=\"top-back-button\" onClick=\"Globals.moyvaMenu.Back()\"><text className=\"top-back-label\">").Append(view.T("BACK")).Append("</text></button>");
            sb.Append("</view></view><scroll className=\"navigation-list\" data-key=\"scroll-")
                .Append(E(kicker)).Append('-').Append(E(title)).Append("\" sensitivity=\"24\">");
        }

        private static void Button(
            StringBuilder sb,
            string index,
            string title,
            string copy,
            string click,
            bool primary = false,
            bool enabled = true,
            string status = null)
        {
            sb.Append("<button className=\"menu-button");
            if (primary) sb.Append(" primary");
            if (!enabled) sb.Append(" disabled");
            sb.Append('"');
            if (enabled)
                sb.Append(" onClick=\"").Append(click).Append('"');
            else
                sb.Append(" disabled=\"true\"");
            sb.Append("><text className=\"button-index\">").Append(E(index)).Append("</text><view className=\"button-content\"><text className=\"button-title\">")
                .Append(E(title)).Append("</text><text className=\"button-copy\">").Append(E(copy)).Append("</text></view>");
            if (!string.IsNullOrWhiteSpace(status))
                sb.Append("<text className=\"button-status\">").Append(E(status)).Append("</text>");
            else
                sb.Append("<text className=\"button-arrow\">></text>");
            sb.Append("</button>");
        }

        private static void ActionButton(StringBuilder sb, string title, string copy, string click, bool enabled)
        {
            sb.Append("<button className=\"action-button");
            if (!enabled) sb.Append(" disabled");
            sb.Append('"');
            if (enabled) sb.Append(" onClick=\"").Append(click).Append('"');
            else sb.Append(" disabled=\"true\"");
            sb.Append("><view className=\"button-content\"><text className=\"action-title\">").Append(E(title))
                .Append("</text><text className=\"button-copy\">").Append(E(copy)).Append("</text></view><text className=\"button-arrow\">></text></button>");
        }

        private static void CompactButton(StringBuilder sb, string label, string click, bool primary = false, bool enabled = true, string extraClass = null)
        {
            sb.Append("<button className=\"compact-button");
            if (primary) sb.Append(" primary");
            if (!enabled) sb.Append(" disabled");
            if (!string.IsNullOrWhiteSpace(extraClass)) sb.Append(' ').Append(E(extraClass));
            sb.Append('"');
            if (enabled) sb.Append(" onClick=\"").Append(click).Append('"');
            else sb.Append(" disabled=\"true\"");
            sb.Append("><text className=\"compact-button-label\">").Append(E(label)).Append("</text></button>");
        }

        private static void CompactControlButton(StringBuilder sb, string label, string click, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"control-row command-control\"><text className=\"control-label\">").Append(view.T("SEED ACTION")).Append("</text>");
            CompactButton(sb, label, click);
            sb.Append("</view>");
        }

        private static void Input(
            StringBuilder sb,
            string label,
            string value,
            string placeholder,
            string preview,
            string commit,
            int limit,
            bool enabled = true,
            string contentType = "Standard",
            string extraClass = null)
        {
            sb.Append(InputMarkup(label, value, placeholder, preview, commit, limit, enabled, contentType, extraClass));
        }

        private static string InputMarkup(
            string label,
            string value,
            string placeholder,
            string preview,
            string commit,
            int limit,
            bool enabled,
            string contentType,
            string extraClass)
        {
            var disabled = enabled ? string.Empty : " disabled=\"true\"";
            var classSuffix = string.IsNullOrWhiteSpace(extraClass) ? string.Empty : $" {E(extraClass)}";
            var change = string.IsNullOrWhiteSpace(preview) ? string.Empty : $" onChange=\"{preview}\"";
            return $"<view className=\"control-row input-row{classSuffix}\"><text className=\"control-label input-label\">{E(label)}</text><input className=\"menu-input\" value=\"{E(value)}\" placeholder=\"{E(placeholder)}\" characterLimit=\"{limit.ToString(CultureInfo.InvariantCulture)}\" contentType=\"{E(contentType)}\" onFocus=\"Globals.moyvaMenu.BeginControlInteraction()\"{change} onEndEdit=\"{commit}\" onBlur=\"Globals.moyvaMenu.EndControlInteraction()\"{disabled}></input></view>";
        }

        private static void Select(StringBuilder sb, string label, string options, int value, string changed, bool enabled = true)
        {
            sb.Append("<view className=\"control-row select-row\"><text className=\"control-label\">").Append(E(label))
                .Append("</text><select className=\"menu-select\" options=\"").Append(E(options)).Append("\" value=\"")
                .Append(value.ToString(CultureInfo.InvariantCulture)).Append("\" onChange=\"").Append(changed).Append('"');
            if (!enabled) sb.Append(" disabled=\"true\"");
            sb.Append("></select></view>");
        }

        private static void Slider(
            StringBuilder sb,
            string label,
            float value,
            float min,
            float max,
            string commit,
            string format,
            bool enabled,
            string suffix = null)
        {
            sb.Append("<view className=\"control-row slider-row\"><text className=\"control-label slider-label\">").Append(E(label))
                .Append("</text><slider className=\"menu-slider\" value=\"").Append(value.ToString("0.###", CultureInfo.InvariantCulture))
                .Append("\" minValue=\"").Append(min.ToString("0.###", CultureInfo.InvariantCulture))
                .Append("\" maxValue=\"").Append(max.ToString("0.###", CultureInfo.InvariantCulture))
                .Append("\" format=\"").Append(E(format)).Append("\" onBeginChange=\"Globals.moyvaMenu.BeginControlInteraction()\" onEndChange=\"")
                .Append(commit).Append('"');
            if (!string.IsNullOrEmpty(suffix)) sb.Append(" suffix=\"").Append(E(suffix)).Append('"');
            if (!enabled) sb.Append(" disabled=\"true\"");
            sb.Append("></slider></view>");
        }

        private static void Toggle(StringBuilder sb, string label, string help, bool value, string changed, string extraClass = null, bool enabled = true)
        {
            sb.Append("<view className=\"control-row toggle-row");
            if (!string.IsNullOrWhiteSpace(extraClass)) sb.Append(' ').Append(E(extraClass));
            sb.Append("\"><view className=\"control-copy\"><text className=\"control-label\">").Append(E(label))
                .Append("</text><text className=\"control-help\">").Append(E(help))
                .Append("</text></view><toggle className=\"menu-toggle\" checked=\"").Append(value ? "true" : "false")
                .Append("\" onChange=\"").Append(changed).Append('"');
            if (!enabled) sb.Append(" disabled=\"true\"");
            sb.Append("><view className=\"toggle-knob\"></view></toggle></view>");
        }

        private static void Stepper(StringBuilder sb, string label, string help, int value, string decrease, string increase)
        {
            sb.Append("<view className=\"control-row stepper-row\"><view className=\"control-copy\"><text className=\"control-label\">").Append(E(label))
                .Append("</text><text className=\"control-help\">").Append(E(help)).Append("</text></view><view className=\"stepper\">");
            CompactButton(sb, "-", decrease);
            sb.Append("<text className=\"stepper-value\">").Append(value.ToString(CultureInfo.InvariantCulture)).Append("</text>");
            CompactButton(sb, "+", increase);
            sb.Append("</view></view>");
        }

        private static void SettingsTabs(StringBuilder sb, HomeMenuSettingsSection active, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"settings-tabs\">");
            SettingsTab(sb, view.T("GENERAL"), "Globals.moyvaMenu.ShowGeneralSettings()", active == HomeMenuSettingsSection.General);
            SettingsTab(sb, view.T("AUDIO"), "Globals.moyvaMenu.ShowAudioSettings()", active == HomeMenuSettingsSection.Audio);
            SettingsTab(sb, view.T("GRAPHICS"), "Globals.moyvaMenu.ShowGraphicsSettings()", active == HomeMenuSettingsSection.Graphics);
            SettingsTab(sb, view.T("CONTROLS"), "Globals.moyvaMenu.ShowControlsSettings()", active == HomeMenuSettingsSection.Controls);
            sb.Append("</view>");
        }

        private static void SettingsTab(StringBuilder sb, string label, string click, bool active)
        {
            sb.Append("<button className=\"settings-tab");
            if (active) sb.Append(" active");
            sb.Append("\" onClick=\"").Append(click).Append("\"><text className=\"settings-tab-label\">").Append(E(label)).Append("</text></button>");
        }

        private static void SectionIntro(StringBuilder sb, string label, string copy)
        {
            sb.Append("<view className=\"section-intro\"><text className=\"section-label\">").Append(E(label))
                .Append("</text><text className=\"section-copy\">").Append(E(copy)).Append("</text></view>");
        }

        private static void Stat(StringBuilder sb, string label, string value)
        {
            sb.Append("<view className=\"stat-row\"><text className=\"stat-label\">").Append(E(label)).Append("</text><text className=\"stat-value\">")
                .Append(E(value)).Append("</text></view>");
        }

        private static void Empty(StringBuilder sb, string value) => sb.Append("<text className=\"empty-copy\">").Append(E(value)).Append("</text>");

        private static string ModalButton(string label, string click) => $"<button className=\"modal-button\" onClick=\"{click}\"><text className=\"modal-button-label\">{E(label)}</text></button>";

        private static void AppendFooter(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"footer\"><text className=\"footer-copy\">").Append(view.T("A WORLD SHAPED BY YOUR DECISIONS")).Append("</text><view className=\"footer-line\"></view><text className=\"footer-copy\">MOYVA</text></view>");
        }

        private static int FrameRateIndex(int frameRate)
        {
            var options = new[] { 30, 45, 60, 90, 120, 144, 240 };
            var closest = 0;
            var distance = int.MaxValue;
            for (var i = 0; i < options.Length; i++)
            {
                var candidateDistance = Math.Abs(options[i] - frameRate);
                if (candidateDistance >= distance)
                    continue;
                closest = i;
                distance = candidateDistance;
            }
            return closest;
        }

        private static int AntiAliasingIndex(int value) => value >= 4 ? 2 : value >= 2 ? 1 : 0;

        private static string RouteClass(string route)
        {
            switch (route)
            {
                case "SettingsPanel": return "route-settings";
                case "WorldSetupPanel": return "route-world";
                case "CreateRoomPanel": return "route-room";
                case "JoinRoomPanel": return "route-join";
                case "SelectMultiplayerType":
                case "MultiplayerPanel": return "route-multiplayer";
                case "PlayModePanel": return "route-play";
                default: return "route-main";
            }
        }

        private static string E(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
