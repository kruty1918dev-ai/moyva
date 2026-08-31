using System;
using System.Globalization;
using System.Text;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class HomeMenuMoyvaUiMarkup
    {
        public static string Build(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view, string viewportClass)
        {
            var route = string.IsNullOrWhiteSpace(state.CurrentRoute) ? "Main" : state.CurrentRoute.Trim();
            var sb = new StringBuilder(12000);
            sb.Append("<view className=\"moyva-ui-app ").Append(E(viewportClass)).Append("\">");
            sb.Append("<view className=\"background-veil\"></view>");
            sb.Append("<view className=\"shell-content\">");
            AppendBrand(sb, view);
            sb.Append("<view className=\"navigation-panel\">");
            AppendRoute(sb, route, view);
            sb.Append("</view></view></view>");
            AppendFooter(sb);
            AppendModals(sb, view);
            sb.Append("</view>");
            return sb.ToString();
        }

        private static void AppendRoute(StringBuilder sb, string route, HomeMenuMoyvaUiViewController view)
        {
            switch (route)
            {
                case "PlayModePanel":
                    Header(sb, "PLAY", "Start a realm");
                    Button(sb, "01", "SET UP WORLD", "Choose seed, size and rules.", "Globals.moyvaMenu.WorldSetup()", true);
                    Button(sb, "02", "BACK", "Return to main menu.", "Globals.moyvaMenu.Back()");
                    break;
                case "ContinuePanel":
                    Header(sb, "CONTINUE", "Saved realms");
                    if (view.Slots.Count == 0)
                        Empty(sb, "No saved worlds found.");
                    for (int i = 0; i < view.Slots.Count; i++)
                    {
                        var slot = view.Slots[i];
                        Button(sb, $"{slot.SlotIndex:D2}", slot.SlotName, slot.LastModified.ToString("g", CultureInfo.CurrentCulture), $"Globals.moyvaMenu.SelectSlot({i})", i == 0);
                    }
                    Back(sb);
                    break;
                case "SelectMultiplayerType":
                case "MultiplayerPanel":
                    Header(sb, "MULTIPLAYER", "Create or join a shared realm");
                    Button(sb, "LAN", "CREATE LOCAL GAME", "Host a lobby on the local network.", "Globals.moyvaMenu.CreateLan()", true);
                    Button(sb, "LAN", "JOIN LOCAL GAME", "Find or enter a LAN lobby.", "Globals.moyvaMenu.JoinLan()");
                    Button(sb, "NET", "CREATE GLOBAL GAME", "Host with Relay services.", "Globals.moyvaMenu.CreateGlobal()");
                    Button(sb, "NET", "JOIN GLOBAL GAME", "Browse public global rooms.", "Globals.moyvaMenu.JoinGlobal()");
                    Back(sb);
                    break;
                case "CreateRoomPanel":
                    Header(sb, "LOBBY", view.CreateRoomTitle);
                    Stat(sb, "Room", view.RoomName);
                    Stat(sb, "Visibility", view.IsPublic ? "Public" : "Private");
                    Stat(sb, "Max players", view.MaxPlayers.ToString(CultureInfo.InvariantCulture));
                    Button(sb, "PUB", "TOGGLE VISIBILITY", "Switch public/private lobby.", "Globals.moyvaMenu.TogglePublic()");
                    Button(sb, "-", "LESS PLAYERS", "Decrease lobby capacity.", "Globals.moyvaMenu.MaxPlayersMinus()");
                    Button(sb, "+", "MORE PLAYERS", "Increase lobby capacity.", "Globals.moyvaMenu.MaxPlayersPlus()");
                    Button(sb, "GO", view.CreateRoomNextText, view.CreateRoomSectionTitle, "Globals.moyvaMenu.CreateRoom()", true, view.NextButton == null || view.NextButton.interactable);
                    Back(sb);
                    break;
                case "JoinRoomPanel":
                    Header(sb, "JOIN", "Available rooms");
                    Stat(sb, "Typed code", string.IsNullOrWhiteSpace(view.JoinCode) ? "None" : view.JoinCode);
                    Button(sb, "R", "REFRESH ROOMS", "Ask lobby service for a fresh list.", "Globals.moyvaMenu.RefreshRooms()", false, true);
                    Button(sb, "IN", "JOIN BY CODE", "Join using the current room code.", "Globals.moyvaMenu.JoinTypedRoom()", true, view.JoinInteractable);
                    if (view.Rooms.Count == 0)
                        Empty(sb, "No rooms available yet.");
                    for (int i = 0; i < view.Rooms.Count; i++)
                    {
                        var room = view.Rooms[i];
                        Button(sb, room.ProviderLabel, room.HostOrRoomDisplayName, $"{room.CurrentPlayers}/{room.MaxPlayers} - {room.DisplayIdentifier}", $"Globals.moyvaMenu.SelectRoom({i})");
                    }
                    Back(sb);
                    break;
                case "WorldSetupPanel":
                    Header(sb, "WORLD SETUP", "Shape the campaign");
                    Stat(sb, "World", view.WorldName);
                    Stat(sb, "Seed", view.Seed.ToString(CultureInfo.InvariantCulture));
                    Stat(sb, "Size", view.Size.ToString());
                    Stat(sb, "Map", view.MapType.ToString());
                    Stat(sb, "Difficulty", view.Difficulty.ToString());
                    Button(sb, "RNG", "RANDOM SEED", "Generate another deterministic seed.", "Globals.moyvaMenu.RandomSeed()");
                    Button(sb, "S", "SMALL", "Compact world.", "Globals.moyvaMenu.WorldSmall()");
                    Button(sb, "M", "MEDIUM", "Balanced world.", "Globals.moyvaMenu.WorldMedium()", view.Size == WorldSize.Medium);
                    Button(sb, "L", "LARGE", "Longer campaign.", "Globals.moyvaMenu.WorldLarge()");
                    Button(sb, "01", "CONTINENTS", "Classic separated land masses.", "Globals.moyvaMenu.MapContinents()");
                    Button(sb, "02", "ISLANDS", "More water and coasts.", "Globals.moyvaMenu.MapIslands()");
                    Button(sb, "GO", "CREATE WORLD", "Prepare session and continue.", "Globals.moyvaMenu.CreateWorld()", true, view.CreateWorldButton == null || view.CreateWorldButton.interactable);
                    Back(sb);
                    break;
                case "LobbyPanel":
                    Header(sb, "LOBBY", "Ready room");
                    Stat(sb, "Invite", view.InviteCodeText);
                    if (view.LobbyUsers.Count == 0)
                        Empty(sb, "Waiting for players.");
                    for (int i = 0; i < view.LobbyUsers.Count; i++)
                        Stat(sb, $"P{i + 1}", view.LobbyUsers[i].UserName);
                    Button(sb, "GO", "START GAME", "Locks world settings and launches gameplay.", "Globals.moyvaMenu.StartGame()", true, view.StartGameButton == null || view.StartGameButton.interactable);
                    Button(sb, "OUT", "LEAVE LOBBY", "Return to previous menu.", "Globals.moyvaMenu.LeaveLobby()");
                    break;
                case "KickPlayerPanel":
                    Header(sb, "MANAGE PLAYERS", "Kick list");
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
                    Header(sb, "SETTINGS", "Audio and graphics");
                    Stat(sb, "Player", view.PlayerName);
                    Stat(sb, "Master", Percent(view.MasterVolume));
                    Stat(sb, "Music", Percent(view.MusicVolume));
                    Stat(sb, "SFX", Percent(view.SfxVolume));
                    Stat(sb, "UI", Percent(view.UiVolume));
                    Stat(sb, "Graphics", view.GraphicsProfile.ToString());
                    Stat(sb, "Render scale", Percent(view.RenderScale));
                    Stat(sb, "FPS", view.TargetFrameRate.ToString(CultureInfo.InvariantCulture));
                    Button(sb, "ID", "CHANGE PLAYER NAME", "Cycle visible local name.", "Globals.moyvaMenu.ChangePlayerName()", false, view.SettingsInteractable);
                    Button(sb, "M", view.IsMuted ? "UNMUTE" : "MUTE", "Toggle all menu audio.", "Globals.moyvaMenu.ToggleMuted()", false, view.SettingsInteractable);
                    Button(sb, "BAL", "BALANCED GRAPHICS", "Apply balanced profile.", "Globals.moyvaMenu.GraphicsBalanced()", true, view.SettingsInteractable);
                    Button(sb, "-", "LOWER RENDER SCALE", "Improve performance.", "Globals.moyvaMenu.RenderScaleDown()", false, view.SettingsInteractable);
                    Button(sb, "+", "RAISE RENDER SCALE", "Improve clarity.", "Globals.moyvaMenu.RenderScaleUp()", false, view.SettingsInteractable);
                    Button(sb, "RST", "RESET GRAPHICS", "Return graphics defaults.", "Globals.moyvaMenu.ResetGraphics()", false, view.SettingsInteractable);
                    Button(sb, "DEL", "DELETE SAVES", "Remove all local saves after confirmation.", "Globals.moyvaMenu.DeleteSaves()", false, view.SettingsInteractable);
                    Back(sb);
                    break;
                default:
                    Header(sb, "MAIN MENU", "Choose your path");
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
                    .Append(E(view.PasswordRoomDisplayName)).Append("</text><text className=\"modal-copy error\">").Append(E(view.PasswordErrorText)).Append("</text>")
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

        private static void Header(StringBuilder sb, string kicker, string title)
        {
            sb.Append("<view className=\"navigation-heading\"><text className=\"navigation-kicker\">").Append(E(kicker)).Append("</text><text className=\"navigation-title\">")
                .Append(E(title)).Append("</text></view><view className=\"navigation-list\">");
        }

        private static void Back(StringBuilder sb) => Button(sb, "<", "BACK", "Return to previous menu.", "Globals.moyvaMenu.Back()");

        private static void Button(StringBuilder sb, string index, string title, string copy, string click, bool primary = false, bool enabled = true)
        {
            sb.Append("<button className=\"menu-button");
            if (primary) sb.Append(" primary");
            if (!enabled) sb.Append(" disabled");
            sb.Append("\" onClick=\"").Append(enabled ? click : string.Empty).Append("\"><text className=\"button-index\">").Append(E(index)).Append("</text><view className=\"button-content\"><text className=\"button-title\">")
                .Append(E(title)).Append("</text><text className=\"button-copy\">").Append(E(copy)).Append("</text></view><text className=\"button-arrow\">></text></button>");
        }

        private static void Stat(StringBuilder sb, string label, string value)
        {
            sb.Append("<view className=\"stat-row\"><text className=\"stat-label\">").Append(E(label)).Append("</text><text className=\"stat-value\">")
                .Append(E(value)).Append("</text></view>");
        }

        private static void Empty(StringBuilder sb, string text) => sb.Append("<text className=\"empty-copy\">").Append(E(text)).Append("</text>");

        private static string ModalButton(string label, string click) => $"<button className=\"modal-button\" onClick=\"{click}\"><text className=\"modal-button-label\">{E(label)}</text></button>";

        private static void AppendFooter(StringBuilder sb)
        {
            sb.Append("<view className=\"footer\"><text className=\"footer-copy\">A WORLD SHAPED BY YOUR DECISIONS</text><view className=\"footer-line\"></view><text className=\"footer-copy\">MOYVA</text></view>");
        }

        private static string Percent(float value) => $"{MathfLike.Clamp01(value) * 100f:0}%";

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
