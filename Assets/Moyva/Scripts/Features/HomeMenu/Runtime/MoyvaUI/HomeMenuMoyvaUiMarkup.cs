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

        public static string Build(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view, string viewportClass)
        {
            var route = string.IsNullOrWhiteSpace(state.CurrentRoute) ? "Main" : state.CurrentRoute.Trim();
            var showBack = !string.Equals(route, "Main", StringComparison.Ordinal);
            var sb = new StringBuilder(18000);
            sb.Append("<view className=\"moyva-ui-app ").Append(E(viewportClass)).Append(' ').Append(RouteClass(route));
            if (route == "SettingsPanel" && state.SettingsSection == HomeMenuSettingsSection.Controls) sb.Append(" controls-page");
            sb.Append("\">");
            sb.Append("<view className=\"background-veil\"></view><view className=\"shell-content\">");
            AppendBrand(sb, view);
            sb.Append("<view className=\"navigation-panel\">");
            AppendRoute(sb, state, route, view, showBack);
            sb.Append("</scroll></view></view>");
            AppendFooter(sb);
            AppendModals(sb, view);
            sb.Append("</view>");
            return sb.ToString();
        }

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
                    Header(sb, "PLAY", "Choose how to play", showBack);
                    if (view.Slots.Count > 0)
                        Button(sb, "SAVE", "CONTINUE", $"Resume one of {view.Slots.Count} saved realms.", "Globals.moyvaMenu.Continue()", true);
                    Button(sb, "SOLO", "SOLO SANDBOX", "Build a world without an opponent.", "Globals.moyvaMenu.Solo()", view.Slots.Count == 0);
                    Button(sb, "NET", "MULTIPLAYER", "Host or join an online or LAN lobby.", "Globals.moyvaMenu.Multiplayer()");
                    Button(sb, "AI", "PLAY AGAINST AI", "Computer opponents are still in development.", string.Empty, false, false, "COMING SOON");
                    break;
                case "ContinuePanel":
                    Header(sb, "CONTINUE", "Saved realms", showBack);
                    if (view.Slots.Count == 0)
                        Empty(sb, "No saved worlds found.");
                    for (var i = 0; i < view.Slots.Count; i++)
                    {
                        var slot = view.Slots[i];
                        Button(sb, $"{slot.SlotIndex:D2}", slot.SlotName, slot.LastModified.ToString("g", CultureInfo.CurrentCulture), $"Globals.moyvaMenu.SelectSlot({i})", i == 0);
                    }
                    break;
                case "SelectMultiplayerType":
                case "MultiplayerPanel":
                    Header(sb, "MULTIPLAYER", "Host or join a realm", showBack);
                    SectionIntro(sb, "CONNECTION", "Choose the network used for this session.");
                    Select(sb, "Network", NetworkOptions, view.SelectedMode == NetworkProviderType.Lan ? 1 : 0, "Globals.moyvaMenu.SetNetworkMode(event)");
                    sb.Append("<view className=\"choice-grid\">");
                    Button(sb, "HOST", "CREATE LOBBY", "Configure a room and invite players.", "Globals.moyvaMenu.CreateSelected()", true);
                    Button(sb, "JOIN", "FIND LOBBY", "Browse rooms or enter an invite code.", "Globals.moyvaMenu.JoinSelected()");
                    sb.Append("</view>");
                    break;
                case "CreateRoomPanel":
                    Header(sb, "LOBBY", view.CreateRoomTitle, showBack);
                    sb.Append("<view className=\"form-grid\">");
                    Input(sb, "Room name", view.RoomName, "Moyva Lobby", "Globals.moyvaMenu.PreviewRoomName(event)", "Globals.moyvaMenu.CommitRoomName(event)", 48, true, "Standard", "full");
                    Toggle(sb, "Private room", "Require a password to enter.", !view.IsPublic, "Globals.moyvaMenu.SetPrivate(event)");
                    Stepper(sb, "Players", "Lobby capacity", view.MaxPlayers, "Globals.moyvaMenu.MaxPlayersMinus()", "Globals.moyvaMenu.MaxPlayersPlus()");
                    if (!view.IsPublic)
                        Input(sb, "Password", view.Password, "Optional password", "Globals.moyvaMenu.PreviewRoomPassword(event)", "Globals.moyvaMenu.CommitRoomPassword(event)", 48, true, "Password", "full");
                    sb.Append("</view>");
                    ActionButton(sb, view.CreateRoomNextText, "Continue to world setup", "Globals.moyvaMenu.CreateRoom()", view.NextButton == null || view.NextButton.interactable);
                    break;
                case "JoinRoomPanel":
                    Header(sb, "JOIN", "Available rooms", showBack);
                    Input(sb, "Invite code", view.JoinCode, "Lobby code or room id", "Globals.moyvaMenu.PreviewJoinCode(event)", "Globals.moyvaMenu.CommitJoinCode(event)", 64, true, "Standard", "full");
                    sb.Append("<view className=\"inline-actions\">");
                    CompactButton(sb, "REFRESH", "Globals.moyvaMenu.RefreshRooms()");
                    CompactButton(sb, "JOIN BY CODE", "Globals.moyvaMenu.JoinTypedRoom()", true, view.JoinInteractable);
                    sb.Append("</view><view className=\"room-list\">");
                    if (view.Rooms.Count == 0)
                        Empty(sb, "No public rooms available.");
                    for (var i = 0; i < view.Rooms.Count; i++)
                    {
                        var room = view.Rooms[i];
                        Button(sb, room.ProviderLabel, room.HostOrRoomDisplayName, $"{room.CurrentPlayers}/{room.MaxPlayers} - {room.DisplayIdentifier}", $"Globals.moyvaMenu.SelectRoom({i})");
                    }
                    sb.Append("</view>");
                    break;
                case "WorldSetupPanel":
                    Header(sb, "WORLD SETUP", state.PlayFlow == HomeMenuPlayFlow.Solo ? "Create a sandbox" : "Shape the campaign", showBack);
                    sb.Append("<view className=\"form-grid\">");
                    Input(sb, "World name", view.WorldName, "New World", "Globals.moyvaMenu.PreviewWorldName(event)", "Globals.moyvaMenu.CommitWorldName(event)", 48, true, "Standard", "full");
                    Input(sb, "Seed", view.Seed.ToString(CultureInfo.InvariantCulture), "World seed", string.Empty, "Globals.moyvaMenu.CommitSeed(event)", 12, true, "IntegerNumber");
                    CompactControlButton(sb, "RANDOMIZE SEED", "Globals.moyvaMenu.RandomSeed()");
                    Select(sb, "World size", WorldSizeOptions, (int)view.Size, "Globals.moyvaMenu.SetWorldSizeValue(event)");
                    Select(sb, "Map type", MapTypeOptions, (int)view.MapType, "Globals.moyvaMenu.SetMapTypeValue(event)");
                    Select(sb, "Difficulty", DifficultyOptions, (int)view.Difficulty, "Globals.moyvaMenu.SetDifficultyValue(event)");
                    sb.Append("</view>");
                    ActionButton(
                        sb,
                        state.PlayFlow == HomeMenuPlayFlow.Solo ? "START SANDBOX" : "CREATE LOBBY",
                        state.PlayFlow == HomeMenuPlayFlow.Solo ? "Generate the world and enter the game." : "Create the room and wait for players.",
                        "Globals.moyvaMenu.CreateWorld()",
                        view.CreateWorldButton == null || view.CreateWorldButton.interactable);
                    break;
                case "LobbyPanel":
                    Header(sb, "LOBBY", "Players and session", showBack);
                    Stat(sb, "Invite", view.InviteCodeText);
                    sb.Append("<view className=\"player-list\">");
                    if (view.LobbyUsers.Count == 0)
                        Empty(sb, "Waiting for players.");
                    for (var i = 0; i < view.LobbyUsers.Count; i++)
                        Stat(sb, $"P{i + 1}", view.LobbyUsers[i].UserName);
                    sb.Append("</view>");
                    ActionButton(sb, "START GAME", "Lock the lobby and launch gameplay.", "Globals.moyvaMenu.StartGame()", view.StartGameButton == null || view.StartGameButton.interactable);
                    CompactButton(sb, "LEAVE LOBBY", "Globals.moyvaMenu.LeaveLobby()");
                    break;
                case "KickPlayerPanel":
                    Header(sb, "MANAGE PLAYERS", "Lobby players", showBack);
                    if (!string.IsNullOrWhiteSpace(view.KickStatus))
                        Empty(sb, view.KickStatus);
                    for (var i = 0; i < view.KickPlayers.Count; i++)
                    {
                        var player = view.KickPlayers[i];
                        Button(sb, player.CanKick ? "KICK" : "HOST", player.DisplayName, player.StatusLabel, $"Globals.moyvaMenu.KickPlayer({i})", false, view.KickInteractable && player.CanKick);
                    }
                    sb.Append("<view className=\"inline-actions\">");
                    CompactButton(sb, "REFRESH", "Globals.moyvaMenu.RefreshKickPlayers()");
                    CompactButton(sb, "CLOSE", "Globals.moyvaMenu.CloseKickPlayers()");
                    sb.Append("</view>");
                    break;
                case "SettingsPanel":
                    Header(sb, "SETTINGS", "Game preferences", showBack);
                    SettingsTabs(sb, state.SettingsSection);
                    AppendSettings(sb, state.SettingsSection, view);
                    break;
                default:
                    Header(sb, "MAIN MENU", "A realm awaits", false);
                    Button(sb, "01", "PLAY", "Choose a mode and start a realm.", "Globals.moyvaMenu.Play()", true);
                    Button(sb, "02", "SETTINGS", "Audio, graphics and player profile.", "Globals.moyvaMenu.Settings()");
                    Button(sb, "03", "QUIT", "Close Moyva.", "Globals.moyvaMenu.Exit()");
                    break;
            }
        }

        private static void AppendSettings(StringBuilder sb, HomeMenuSettingsSection section, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"settings-content\">");
            switch (section)
            {
                case HomeMenuSettingsSection.Audio:
                    SectionIntro(sb, "AUDIO", "Mix game sound without leaving the menu.");
                    sb.Append("<view className=\"settings-grid\">");
                    Toggle(sb, "Mute all audio", "Disable every audio bus.", view.IsMuted, "Globals.moyvaMenu.SetMuted(event)", "full");
                    Slider(sb, "Master", view.MasterVolume, 0f, 1f, "Globals.moyvaMenu.CommitMasterValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, "Music", view.MusicVolume, 0f, 1f, "Globals.moyvaMenu.CommitMusicValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, "Sound effects", view.SfxVolume, 0f, 1f, "Globals.moyvaMenu.CommitSfxValue(event)", "percent", view.SettingsInteractable);
                    Slider(sb, "Interface", view.UiVolume, 0f, 1f, "Globals.moyvaMenu.CommitUiValue(event)", "percent", view.SettingsInteractable);
                    sb.Append("</view>");
                    break;
                case HomeMenuSettingsSection.Graphics:
                    SectionIntro(sb, "GRAPHICS", "Choose a profile, then fine-tune individual options.");
                    sb.Append("<view className=\"settings-grid\">");
                    Select(sb, "Quality profile", GraphicsProfileOptions, (int)view.GraphicsProfile, "Globals.moyvaMenu.SetGraphicsProfileValue(event)", view.GraphicsSettingsInteractable);
                    Select(sb, "Frame limit", FrameRateOptions, FrameRateIndex(view.TargetFrameRate), "Globals.moyvaMenu.SetFrameRateOption(event)", view.GraphicsSettingsInteractable);
                    Slider(sb, "Render scale", view.RenderScale, 0.42f, 1f, "Globals.moyvaMenu.CommitRenderScaleValue(event)", "percent", view.GraphicsSettingsInteractable);
                    Select(sb, "Texture quality", TextureQualityOptions, view.TextureMipmapLimit, "Globals.moyvaMenu.SetTextureQualityOption(event)", view.GraphicsSettingsInteractable);
                    Select(sb, "Anti-aliasing", AntiAliasingOptions, AntiAliasingIndex(view.AntiAliasing), "Globals.moyvaMenu.SetAntiAliasingOption(event)", view.GraphicsSettingsInteractable);
                    Slider(sb, "Level of detail", view.LodBias, 0.4f, 2f, "Globals.moyvaMenu.CommitLodBiasValue(event)", "decimal1", view.GraphicsSettingsInteractable, "x");
                    Toggle(sb, "Vertical sync", "Match frames to the display refresh.", view.VSync, "Globals.moyvaMenu.SetVSync(event)", null, view.GraphicsSettingsInteractable);
                    Toggle(sb, "Shadows", "Render realtime world shadows.", view.Shadows, "Globals.moyvaMenu.SetShadows(event)", null, view.GraphicsSettingsInteractable);
                    Toggle(sb, "Anisotropic filtering", "Keep angled textures sharp.", view.AnisotropicFiltering, "Globals.moyvaMenu.SetAnisotropic(event)", null, view.GraphicsSettingsInteractable);
                    sb.Append("</view>");
                    CompactButton(sb, "RESET GRAPHICS", "Globals.moyvaMenu.ResetGraphics()", false, view.GraphicsSettingsInteractable);
                    break;
                case HomeMenuSettingsSection.Controls:
                    HomeMenuControlsMarkup.Append(sb, view);
                    sb.Append("<view className=\"settings-grid\">");
                    Slider(sb, "Mouse sensitivity", view.MouseSensitivity, 0.25f, 3f, "Globals.moyvaMenu.CommitMouseSensitivityValue(event)", "decimal1", view.SettingsInteractable, "x");
                    Slider(sb, "Movement speed", view.MovementSpeed, 0.25f, 3f, "Globals.moyvaMenu.CommitMovementSpeedValue(event)", "decimal1", view.SettingsInteractable, "x");
                    Slider(sb, "Orbit speed", view.OrbitSpeed, 0.25f, 3f, "Globals.moyvaMenu.CommitOrbitSpeedValue(event)", "decimal1", view.SettingsInteractable, "x");
                    Slider(sb, "Zoom speed", view.ZoomSpeed, 0.25f, 3f, "Globals.moyvaMenu.CommitZoomSpeedValue(event)", "decimal1", view.SettingsInteractable, "x");
                    sb.Append("</view>");
                    CompactButton(sb, "RESET CONTROLS", "Globals.moyvaMenu.ResetControls()");
                    break;
                default:
                    SectionIntro(sb, "GENERAL", "Player identity and local data.");
                    sb.Append("<view className=\"settings-grid\">");
                    Input(sb, "Player name", view.PlayerName, "Player", "Globals.moyvaMenu.PreviewPlayerName(event)", "Globals.moyvaMenu.CommitPlayerName(event)", 32, view.SettingsInteractable, "Standard", "full");
                    sb.Append("<view className=\"danger-zone full\"><view className=\"control-copy\"><text className=\"control-label\">LOCAL SAVES</text><text className=\"control-help\">Permanently remove all saved realms from this device.</text></view>");
                    CompactButton(sb, "DELETE SAVES", "Globals.moyvaMenu.DeleteSaves()", false, view.SettingsInteractable, "danger");
                    sb.Append("</view></view>");
                    break;
            }
            sb.Append("</view>");
        }

        private static void AppendBrand(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"brand-panel\"><view className=\"kicker-row\"><view className=\"kicker-mark\"></view><text className=\"kicker\">TURN-BASED STRATEGY</text></view>");
            sb.Append("<text className=\"brand-title\">MOYVA</text><text className=\"brand-subtitle\">Shape a realm that remembers every decision.</text>");
            sb.Append("<view className=\"brand-rule\"><view className=\"brand-rule-accent\"></view></view><text className=\"brand-copy\">Build your kingdom, gather allies, and lead your people through a living procedural world.</text>");
            sb.Append("<view className=\"world-status\"><view className=\"status-dot\"></view><view className=\"status-copy\"><text className=\"status-label\">WORLD PREVIEW</text><text className=\"status-value\">")
                .Append(E(view.WorldName)).Append(" - seed ").Append(view.Seed.ToString(CultureInfo.InvariantCulture)).Append("</text></view></view></view>");
        }

        private static void AppendModals(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            if (view.OverlayVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">LOADING</text><text className=\"modal-copy\">")
                    .Append(Math.Round(view.OverlayProgress)).Append(E(view.OverlaySuffix)).Append("</text></view></view>");

            if (view.InfoVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">").Append(E(view.CurrentInfo.Title)).Append("</text><text className=\"modal-copy\">")
                    .Append(E(view.CurrentInfo.Message)).Append("</text>").Append(ModalButton("OK", "Globals.moyvaMenu.AcknowledgeInfo()"))
                    .Append("</view></view>");

            if (view.PasswordVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">PASSWORD</text><text className=\"modal-copy\">")
                    .Append(E(view.PasswordRoomDisplayName)).Append("</text>")
                    .Append(InputMarkup("Password", view.PasswordValue, "Password", "Globals.moyvaMenu.PreviewPasswordValue(event)", "Globals.moyvaMenu.CommitPasswordValue(event)", 48, true, "Password", "full"))
                    .Append("<text className=\"modal-copy error\">").Append(E(view.PasswordErrorText)).Append("</text><view className=\"modal-actions\">")
                    .Append(ModalButton("JOIN", "Globals.moyvaMenu.ConfirmPassword()"))
                    .Append(ModalButton("CANCEL", "Globals.moyvaMenu.CancelPassword()"))
                    .Append("</view></view></view>");

            if (view.ConfirmationVisible && view.CurrentConfirmation.HasValue)
            {
                var request = view.CurrentConfirmation.Value;
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">")
                    .Append(E(request.LabelText)).Append("</text><text className=\"modal-copy\">")
                    .Append(E(request.MessageText)).Append("</text><view className=\"modal-actions\">")
                    .Append(ModalButton("YES", "Globals.moyvaMenu.Confirm()"))
                    .Append(ModalButton("NO", "Globals.moyvaMenu.Cancel()"))
                    .Append("</view></view></view>");
            }
        }

        private static void Header(StringBuilder sb, string kicker, string title, bool showBack)
        {
            sb.Append("<view className=\"navigation-heading\"><view className=\"navigation-heading-row\"><view className=\"navigation-heading-copy\"><text className=\"navigation-kicker\">")
                .Append(E(kicker)).Append("</text><text className=\"navigation-title\">").Append(E(title)).Append("</text></view>");
            if (showBack)
                sb.Append("<button className=\"top-back-button\" onClick=\"Globals.moyvaMenu.Back()\"><text className=\"top-back-label\">BACK</text></button>");
            sb.Append("</view></view><scroll className=\"navigation-list\" sensitivity=\"55\">");
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

        private static void CompactControlButton(StringBuilder sb, string label, string click)
        {
            sb.Append("<view className=\"control-row command-control\"><text className=\"control-label\">SEED ACTION</text>");
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

        private static void SettingsTabs(StringBuilder sb, HomeMenuSettingsSection active)
        {
            sb.Append("<view className=\"settings-tabs\">");
            SettingsTab(sb, "GENERAL", "Globals.moyvaMenu.ShowGeneralSettings()", active == HomeMenuSettingsSection.General);
            SettingsTab(sb, "AUDIO", "Globals.moyvaMenu.ShowAudioSettings()", active == HomeMenuSettingsSection.Audio);
            SettingsTab(sb, "GRAPHICS", "Globals.moyvaMenu.ShowGraphicsSettings()", active == HomeMenuSettingsSection.Graphics);
            SettingsTab(sb, "CONTROLS", "Globals.moyvaMenu.ShowControlsSettings()", active == HomeMenuSettingsSection.Controls);
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

        private static void AppendFooter(StringBuilder sb)
        {
            sb.Append("<view className=\"footer\"><text className=\"footer-copy\">A WORLD SHAPED BY YOUR DECISIONS</text><view className=\"footer-line\"></view><text className=\"footer-copy\">MOYVA</text></view>");
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
