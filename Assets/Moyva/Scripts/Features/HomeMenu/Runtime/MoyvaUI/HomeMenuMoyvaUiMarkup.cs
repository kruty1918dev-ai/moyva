using System;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class HomeMenuMoyvaUiMarkup
    {
        public static string Build(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view, string viewportClass)
        {
            var route = string.IsNullOrWhiteSpace(state.CurrentRoute) ? "Main" : state.CurrentRoute.Trim();
            var showBack = !string.Equals(route, "Main", StringComparison.Ordinal);
            var sb = new StringBuilder(12000);
            sb.Append("<view className=\"moyva-ui-app ").Append(E(viewportClass)).Append("\">");
            sb.Append("<view className=\"background-veil\"></view>");
            sb.Append("<view className=\"shell-content\">");
            AppendBrand(sb, view);
            sb.Append("<view className=\"navigation-panel\">");
            AppendRoute(sb, route, view, showBack);
            sb.Append("</scroll></view></view>");
            AppendFooter(sb);
            AppendModals(sb, view);
            sb.Append("</view>");
            return sb.ToString();
        }

        private static void AppendRoute(StringBuilder sb, string route, HomeMenuMoyvaUiViewController view, bool showBack)
        {
            switch (route)
            {
                case "PlayModePanel":
                    Header(sb, "PLAY", "Start a realm", showBack);
                    Button(sb, "01", "SET UP WORLD", "Choose seed, size and rules.", "Globals.moyvaMenu.WorldSetup()", true);
                    break;
                case "ContinuePanel":
                    Header(sb, "CONTINUE", "Saved realms", showBack);
                    if (view.Slots.Count == 0)
                        Empty(sb, "No saved worlds found.");
                    for (int i = 0; i < view.Slots.Count; i++)
                    {
                        var slot = view.Slots[i];
                        Button(sb, $"{slot.SlotIndex:D2}", slot.SlotName, slot.LastModified.ToString("g", CultureInfo.CurrentCulture), $"Globals.moyvaMenu.SelectSlot({i})", i == 0);
                    }
                    break;
                case "SelectMultiplayerType":
                case "MultiplayerPanel":
                    Header(sb, "MULTIPLAYER", "Create or join a shared realm", showBack);
                    Button(sb, "LAN", "CREATE LOCAL GAME", "Host a lobby on the local network.", "Globals.moyvaMenu.CreateLan()", true);
                    Button(sb, "LAN", "JOIN LOCAL GAME", "Find or enter a LAN lobby.", "Globals.moyvaMenu.JoinLan()");
                    Button(sb, "NET", "CREATE GLOBAL GAME", "Host with Relay services.", "Globals.moyvaMenu.CreateGlobal()");
                    Button(sb, "NET", "JOIN GLOBAL GAME", "Browse public global rooms.", "Globals.moyvaMenu.JoinGlobal()");
                    break;
                case "CreateRoomPanel":
                    Header(sb, "LOBBY", view.CreateRoomTitle, showBack);
                    Input(sb, "Room", view.RoomName, "Moyva Lobby", "Globals.moyvaMenu.SetRoomName(event)", 48);
                    Input(sb, "Password", view.Password, "Optional password", "Globals.moyvaMenu.SetRoomPassword(event)", 48, !view.IsPublic, "Password");
                    Stat(sb, "Visibility", view.IsPublic ? "Public" : "Private");
                    Stat(sb, "Max players", view.MaxPlayers.ToString(CultureInfo.InvariantCulture));
                    Button(sb, "PUB", "TOGGLE VISIBILITY", "Switch public/private lobby.", "Globals.moyvaMenu.TogglePublic()");
                    Button(sb, "-", "LESS PLAYERS", "Decrease lobby capacity.", "Globals.moyvaMenu.MaxPlayersMinus()");
                    Button(sb, "+", "MORE PLAYERS", "Increase lobby capacity.", "Globals.moyvaMenu.MaxPlayersPlus()");
                    Button(sb, "GO", view.CreateRoomNextText, view.CreateRoomSectionTitle, "Globals.moyvaMenu.CreateRoom()", true, view.NextButton == null || view.NextButton.interactable);
                    break;
                case "JoinRoomPanel":
                    Header(sb, "JOIN", "Available rooms", showBack);
                    Input(sb, "Join code", view.JoinCode, "Lobby code or room id", "Globals.moyvaMenu.SetJoinCode(event)", 64);
                    Button(sb, "R", "REFRESH ROOMS", "Ask lobby service for a fresh list.", "Globals.moyvaMenu.RefreshRooms()", false, true);
                    Button(sb, "IN", "JOIN BY CODE", "Join using the current room code.", "Globals.moyvaMenu.JoinTypedRoom()", true, view.JoinInteractable);
                    if (view.Rooms.Count == 0)
                        Empty(sb, "No rooms available yet.");
                    for (int i = 0; i < view.Rooms.Count; i++)
                    {
                        var room = view.Rooms[i];
                        Button(sb, room.ProviderLabel, room.HostOrRoomDisplayName, $"{room.CurrentPlayers}/{room.MaxPlayers} - {room.DisplayIdentifier}", $"Globals.moyvaMenu.SelectRoom({i})");
                    }
                    break;
                case "WorldSetupPanel":
                    Header(sb, "WORLD SETUP", "Shape the campaign", showBack);
                    Input(sb, "World", view.WorldName, "New World", "Globals.moyvaMenu.SetWorldName(event)", 48);
                    Input(sb, "Seed", view.Seed.ToString(CultureInfo.InvariantCulture), "Seed", "Globals.moyvaMenu.SetSeed(event)", 12, true, "IntegerNumber");
                    Stat(sb, "Size", view.Size.ToString());
                    Stat(sb, "Map", view.MapType.ToString());
                    Stat(sb, "Difficulty", view.Difficulty.ToString());
                    Button(sb, "RNG", "RANDOM SEED", "Generate another deterministic seed.", "Globals.moyvaMenu.RandomSeed()");
                    Button(sb, "S", "SMALL", "Compact world.", "Globals.moyvaMenu.WorldSmall()");
                    Button(sb, "M", "MEDIUM", "Balanced world.", "Globals.moyvaMenu.WorldMedium()", view.Size == WorldSize.Medium);
                    Button(sb, "L", "LARGE", "Longer campaign.", "Globals.moyvaMenu.WorldLarge()");
                    Button(sb, "01", "CONTINENTS", "Classic separated land masses.", "Globals.moyvaMenu.MapContinents()");
                    Button(sb, "02", "PANGAEA", "One dominant land mass.", "Globals.moyvaMenu.MapPangaea()");
                    Button(sb, "03", "ISLANDS", "More water and coasts.", "Globals.moyvaMenu.MapIslands()");
                    Button(sb, "04", "HIGHLANDS", "Mountain-heavy terrain.", "Globals.moyvaMenu.MapHighlands()");
                    Button(sb, "E", "EASY", "More forgiving campaign.", "Globals.moyvaMenu.DifficultyEasy()");
                    Button(sb, "N", "NORMAL", "Default challenge.", "Globals.moyvaMenu.DifficultyNormal()", view.Difficulty == Difficulty.Normal);
                    Button(sb, "H", "HARD", "Sharper economy and threats.", "Globals.moyvaMenu.DifficultyHard()");
                    Button(sb, "X", "INSANE", "Maximum pressure.", "Globals.moyvaMenu.DifficultyInsane()");
                    Button(sb, "GO", "CREATE WORLD", "Prepare session and continue.", "Globals.moyvaMenu.CreateWorld()", true, view.CreateWorldButton == null || view.CreateWorldButton.interactable);
                    break;
                case "LobbyPanel":
                    Header(sb, "LOBBY", "Ready room", showBack);
                    Stat(sb, "Invite", view.InviteCodeText);
                    if (view.LobbyUsers.Count == 0)
                        Empty(sb, "Waiting for players.");
                    for (int i = 0; i < view.LobbyUsers.Count; i++)
                        Stat(sb, $"P{i + 1}", view.LobbyUsers[i].UserName);
                    Button(sb, "GO", "START GAME", "Locks world settings and launches gameplay.", "Globals.moyvaMenu.StartGame()", true, view.StartGameButton == null || view.StartGameButton.interactable);
                    Button(sb, "OUT", "LEAVE LOBBY", "Return to previous menu.", "Globals.moyvaMenu.LeaveLobby()");
                    break;
                case "KickPlayerPanel":
                    Header(sb, "MANAGE PLAYERS", "Kick list", showBack);
                    if (!string.IsNullOrWhiteSpace(view.KickStatus))
                        Empty(sb, view.KickStatus);
                    for (int i = 0; i < view.KickPlayers.Count; i++)
                    {
                        var player = view.KickPlayers[i];
                        Button(sb, player.CanKick ? "KICK" : "LOCK", player.DisplayName, player.StatusLabel, $"Globals.moyvaMenu.KickPlayer({i})", false, view.KickInteractable && player.CanKick);
                    }
                    Button(sb, "R", "REFRESH", "Refresh lobby players.", "Globals.moyvaMenu.RefreshKickPlayers()");
                    Button(sb, "X", "CLOSE", "Close player manager.", "Globals.moyvaMenu.CloseKickPlayers()");
                    break;
                case "SettingsPanel":
                    Header(sb, "SETTINGS", "Audio and graphics", showBack);
                    Stat(sb, "Player", view.PlayerName);
                    Stat(sb, "Master", Percent(view.MasterVolume));
                    Stat(sb, "Music", Percent(view.MusicVolume));
                    Stat(sb, "SFX", Percent(view.SfxVolume));
                    Stat(sb, "UI", Percent(view.UiVolume));
                    Stat(sb, "Graphics", view.GraphicsProfile.ToString());
                    Stat(sb, "Render scale", Percent(view.RenderScale));
                    Stat(sb, "Dynamic scale", view.DynamicRenderScale ? "On" : "Off");
                    Stat(sb, "FPS", view.TargetFrameRate.ToString(CultureInfo.InvariantCulture));
                    Stat(sb, "VSync", view.VSync ? "On" : "Off");
                    Stat(sb, "Shadows", view.Shadows ? "On" : "Off");
                    Stat(sb, "Anisotropic", view.AnisotropicFiltering ? "On" : "Off");
                    Stat(sb, "Mip limit", view.TextureMipmapLimit.ToString(CultureInfo.InvariantCulture));
                    Stat(sb, "AA", view.AntiAliasing <= 0 ? "Off" : $"{view.AntiAliasing}x");
                    Stat(sb, "Close zoom", view.CloseZoomOptimization ? "On" : "Off");
                    Stat(sb, "LOD bias", view.LodBias.ToString("0.0", CultureInfo.InvariantCulture));
                    Input(sb, "Player", view.PlayerName, "Player name", "Globals.moyvaMenu.SetPlayerName(event)", 32, view.SettingsInteractable);
                    Slider(sb, "Master", Percent(view.MasterVolume), view.MasterVolume, 0f, 1f, "Globals.moyvaMenu.SetMasterValue(event)", false, view.SettingsInteractable);
                    Slider(sb, "Music", Percent(view.MusicVolume), view.MusicVolume, 0f, 1f, "Globals.moyvaMenu.SetMusicValue(event)", false, view.SettingsInteractable);
                    Slider(sb, "SFX", Percent(view.SfxVolume), view.SfxVolume, 0f, 1f, "Globals.moyvaMenu.SetSfxValue(event)", false, view.SettingsInteractable);
                    Slider(sb, "UI", Percent(view.UiVolume), view.UiVolume, 0f, 1f, "Globals.moyvaMenu.SetUiValue(event)", false, view.SettingsInteractable);
                    Slider(sb, "Render scale", Percent(view.RenderScale), view.RenderScale, 0.42f, 1f, "Globals.moyvaMenu.SetRenderScaleValue(event)", false, view.SettingsInteractable);
                    Slider(sb, "FPS", view.TargetFrameRate.ToString(CultureInfo.InvariantCulture), view.TargetFrameRate, 30f, 240f, "Globals.moyvaMenu.SetFrameRateValue(event)", true, view.SettingsInteractable);
                    Slider(sb, "Mip limit", view.TextureMipmapLimit.ToString(CultureInfo.InvariantCulture), view.TextureMipmapLimit, 0f, 3f, "Globals.moyvaMenu.SetMipmapValue(event)", true, view.SettingsInteractable);
                    Slider(sb, "AA", view.AntiAliasing <= 0 ? "Off" : $"{view.AntiAliasing}x", AntiAliasingToSliderValue(view.AntiAliasing), 0f, 2f, "Globals.moyvaMenu.SetAntiAliasingSliderValue(event)", true, view.SettingsInteractable);
                    Slider(sb, "LOD bias", view.LodBias.ToString("0.0", CultureInfo.InvariantCulture), view.LodBias, 0.4f, 2f, "Globals.moyvaMenu.SetLodBiasValue(event)", false, view.SettingsInteractable);
                    Button(sb, "M", view.IsMuted ? "UNMUTE" : "MUTE", "Toggle all menu audio.", "Globals.moyvaMenu.ToggleMuted()", false, view.SettingsInteractable);
                    Button(sb, "AUTO", "AUTO GRAPHICS", "Pick defaults for the device.", "Globals.moyvaMenu.GraphicsAuto()", false, view.SettingsInteractable);
                    Button(sb, "PERF", "PERFORMANCE GRAPHICS", "Favor stable frame time.", "Globals.moyvaMenu.GraphicsPerformance()", false, view.SettingsInteractable);
                    Button(sb, "BAL", "BALANCED GRAPHICS", "Apply balanced profile.", "Globals.moyvaMenu.GraphicsBalanced()", true, view.SettingsInteractable);
                    Button(sb, "QLT", "QUALITY GRAPHICS", "Favor visual clarity.", "Globals.moyvaMenu.GraphicsQuality()", false, view.SettingsInteractable);
                    Button(sb, "DRS", view.DynamicRenderScale ? "DISABLE DYNAMIC SCALE" : "ENABLE DYNAMIC SCALE", "Toggle adaptive render scale.", "Globals.moyvaMenu.ToggleDynamicRenderScale()", false, view.SettingsInteractable);
                    Button(sb, "VS", view.VSync ? "DISABLE VSYNC" : "ENABLE VSYNC", "Toggle vertical sync.", "Globals.moyvaMenu.ToggleVSync()", false, view.SettingsInteractable);
                    Button(sb, "SH", view.Shadows ? "DISABLE SHADOWS" : "ENABLE SHADOWS", "Toggle menu/game shadows.", "Globals.moyvaMenu.ToggleShadows()", false, view.SettingsInteractable);
                    Button(sb, "AN", view.AnisotropicFiltering ? "DISABLE ANISOTROPIC" : "ENABLE ANISOTROPIC", "Toggle sharper angled textures.", "Globals.moyvaMenu.ToggleAnisotropic()", false, view.SettingsInteractable);
                    Button(sb, "CZ", view.CloseZoomOptimization ? "DISABLE CLOSE ZOOM OPT" : "ENABLE CLOSE ZOOM OPT", "Adjust rendering near camera.", "Globals.moyvaMenu.ToggleCloseZoomOptimization()", false, view.SettingsInteractable);
                    Button(sb, "RST", "RESET GRAPHICS", "Return graphics defaults.", "Globals.moyvaMenu.ResetGraphics()", false, view.SettingsInteractable);
                    Button(sb, "DEL", "DELETE SAVES", "Remove all local saves after confirmation.", "Globals.moyvaMenu.DeleteSaves()", false, view.SettingsInteractable);
                    break;
                default:
                    Header(sb, "MAIN MENU", "Choose your path", false);
                    Button(sb, "01", "PLAY", "Create a local campaign.", "Globals.moyvaMenu.Play()", true);
                    Button(sb, "02", "CONTINUE", "Return to your saved realm.", "Globals.moyvaMenu.Continue()");
                    Button(sb, "03", "MULTIPLAYER", "Create or join a shared world.", "Globals.moyvaMenu.Multiplayer()");
                    Button(sb, "04", "SETTINGS", "Audio, graphics and controls.", "Globals.moyvaMenu.Settings()");
                    Button(sb, "X", "QUIT TO DESKTOP", "Close the game.", "Globals.moyvaMenu.Exit()");
                    break;
            }
        }

        private static void AppendBrand(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            sb.Append("<view className=\"brand-panel\"><view className=\"kicker-row\"><view className=\"kicker-mark\"></view><text className=\"kicker\">TURN-BASED STRATEGY</text></view>");
            sb.Append("<text className=\"brand-title\">MOYVA</text>");
            sb.Append("<text className=\"brand-subtitle\">Shape a realm that remembers every decision.</text>");
            sb.Append("<view className=\"brand-rule\"><view className=\"brand-rule-accent\"></view></view>");
            sb.Append("<text className=\"brand-copy\">Build your kingdom, gather allies, and lead your people through a living procedural world.</text>");
            sb.Append("<view className=\"world-status\"><view className=\"status-dot\"></view><view className=\"status-copy\"><text className=\"status-label\">WORLD PREVIEW</text>");
            sb.Append("<text className=\"status-value\">").Append(E(view.WorldName)).Append(" - seed ").Append(view.Seed.ToString(CultureInfo.InvariantCulture)).Append("</text></view></view></view>");
        }

        private static void AppendModals(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            if (view.OverlayVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">LOADING</text><text className=\"modal-copy\">")
                    .Append(Math.Round(view.OverlayProgress)).Append(E(view.OverlaySuffix)).Append("</text></view></view>");

            if (view.InfoVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">").Append(E(view.CurrentInfo.Title)).Append("</text><text className=\"modal-copy\">")
                    .Append(E(view.CurrentInfo.Message)).Append("</text>").Append(ModalButton("OK", "Globals.moyvaMenu.AcknowledgeInfo()")).Append("</view></view>");

            if (view.PasswordVisible)
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">PASSWORD</text><text className=\"modal-copy\">")
                    .Append(E(view.PasswordRoomDisplayName)).Append("</text>")
                    .Append(InputMarkup("Password", view.PasswordValue, "Password", "Globals.moyvaMenu.SetPasswordValue(event)", 48, true, "Password"))
                    .Append("<text className=\"modal-copy error\">").Append(E(view.PasswordErrorText)).Append("</text>")
                    .Append(ModalButton("USE EMPTY", "Globals.moyvaMenu.PasswordEmpty()"))
                    .Append(ModalButton("USE DEMO", "Globals.moyvaMenu.PasswordDemo()"))
                    .Append(ModalButton("JOIN", "Globals.moyvaMenu.ConfirmPassword()"))
                    .Append(ModalButton("CANCEL", "Globals.moyvaMenu.CancelPassword()")).Append("</view></view>");

            if (view.ConfirmationVisible && view.CurrentConfirmation.HasValue)
            {
                var request = view.CurrentConfirmation.Value;
                sb.Append("<view className=\"modal-scrim\"><view className=\"modal-card\"><text className=\"modal-title\">")
                    .Append(E(request.LabelText)).Append("</text><text className=\"modal-copy\">")
                    .Append(E(request.MessageText)).Append("</text><view className=\"modal-actions\">")
                    .Append(ModalButton("YES", "Globals.moyvaMenu.Confirm()"))
                    .Append(ModalButton("NO", "Globals.moyvaMenu.Cancel()")).Append("</view></view></view>");
            }
        }

        private static void Header(StringBuilder sb, string kicker, string title, bool showBack)
        {
            sb.Append("<view className=\"navigation-heading\"><view className=\"navigation-heading-row\"><view className=\"navigation-heading-copy\"><text className=\"navigation-kicker\">")
                .Append(E(kicker)).Append("</text><text className=\"navigation-title\">").Append(E(title)).Append("</text></view>");

            if (showBack)
                sb.Append("<button className=\"top-back-button\" onClick=\"Globals.moyvaMenu.Back()\"><text className=\"top-back-label\">BACK</text></button>");

            sb.Append("</view></view><scroll className=\"navigation-list\" direction=\"vertical\" sensitivity=\"70\" alwaysShow=\"vertical\">");
        }

        private static void Button(StringBuilder sb, string index, string title, string copy, string click, bool primary = false, bool enabled = true)
        {
            sb.Append("<button className=\"menu-button");
            if (primary) sb.Append(" primary");
            if (!enabled) sb.Append(" disabled");
            sb.Append("\" onClick=\"").Append(enabled ? click : string.Empty).Append("\"><text className=\"button-index\">").Append(E(index)).Append("</text><view className=\"button-content\"><text className=\"button-title\">")
                .Append(E(title)).Append("</text><text className=\"button-copy\">").Append(E(copy)).Append("</text></view><text className=\"button-arrow\">></text></button>");
        }

        private static void Slider(
            StringBuilder sb,
            string label,
            string valueText,
            float value,
            float min,
            float max,
            string changed,
            bool wholeNumbers,
            bool enabled)
        {
            sb.Append("<view className=\"slider-row\"><view className=\"slider-copy\"><text className=\"slider-label\">")
                .Append(E(label)).Append("</text><text className=\"slider-value\">").Append(E(valueText))
                .Append("</text></view><slider className=\"menu-slider\" value=\"")
                .Append(value.ToString("0.###", CultureInfo.InvariantCulture)).Append("\" minValue=\"")
                .Append(min.ToString("0.###", CultureInfo.InvariantCulture)).Append("\" maxValue=\"")
                .Append(max.ToString("0.###", CultureInfo.InvariantCulture)).Append("\" wholeNumbers=\"")
                .Append(wholeNumbers ? "true" : "false").Append("\" onValueChanged=\"")
                .Append(changed).Append("\"");

            if (!enabled)
                sb.Append(" disabled=\"true\"");

            sb.Append("></slider></view>");
        }

        private static void Stat(StringBuilder sb, string label, string value)
        {
            sb.Append("<view className=\"stat-row\"><text className=\"stat-label\">").Append(E(label)).Append("</text><text className=\"stat-value\">")
                .Append(E(value)).Append("</text></view>");
        }

        private static void Input(
            StringBuilder sb,
            string label,
            string value,
            string placeholder,
            string edit,
            int limit,
            bool enabled = true,
            string contentType = "Standard")
        {
            sb.Append(InputMarkup(label, value, placeholder, edit, limit, enabled, contentType));
        }

        private static string InputMarkup(
            string label,
            string value,
            string placeholder,
            string edit,
            int limit,
            bool enabled = true,
            string contentType = "Standard")
        {
            var disabled = enabled ? string.Empty : " disabled=\"true\"";
            return $"<view className=\"input-row\"><text className=\"input-label\">{E(label)}</text><input className=\"menu-input\" value=\"{E(value)}\" placeholder=\"{E(placeholder)}\" characterLimit=\"{limit.ToString(CultureInfo.InvariantCulture)}\" contentType=\"{E(contentType)}\" onEndEdit=\"{edit}\" onReturn=\"{edit}\"{disabled}></input></view>";
        }

        private static void Empty(StringBuilder sb, string text) => sb.Append("<text className=\"empty-copy\">").Append(E(text)).Append("</text>");

        private static string ModalButton(string label, string click) => $"<button className=\"modal-button\" onClick=\"{click}\"><text className=\"modal-button-label\">{E(label)}</text></button>";

        private static void AppendFooter(StringBuilder sb)
        {
            sb.Append("<view className=\"footer\"><text className=\"footer-copy\">A WORLD SHAPED BY YOUR DECISIONS</text><view className=\"footer-line\"></view><text className=\"footer-copy\">MOYVA</text></view>");
        }

        private static string Percent(float value) => $"{MathfLike.Clamp01(value) * 100f:0}%";

        private static int AntiAliasingToSliderValue(int value)
        {
            if (value >= 4)
                return 2;

            if (value >= 2)
                return 1;

            return 0;
        }

        private static string E(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static class MathfLike
        {
            public static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
        }
    }
}
