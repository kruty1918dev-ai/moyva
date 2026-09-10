using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal partial class JoinRoomPanelService
    {
        /// <summary>
        /// Виконує приєднання з підтримкою кімнат із паролем. Якщо кімната захищена паролем —
        /// показує <see cref="IPasswordPanelService"/>; при невірному паролі повторює запит з повідомленням про помилку.
        /// </summary>
        private async Task<Result<LobbyRoom>> TryJoinWithPasswordLoopResultAsync(JoinRoomTarget target, CancellationToken ct)
        {
            // 1) Спочатку — швидка спроба без пароля. Для UGS це поверне room з PasswordHash != "" якщо приватна,
            //    тоді ми залишимо лобі та запитаємо пароль. Для LAN ми вже маємо PasswordHash у кеші discovered rooms.
            var probe = await TryProbeRoomForPasswordAsync(target, ct);
            if (!probe.RequiresPassword)
            {
                var joinedWithoutPassword = await JoinTargetResultAsync(target, null, ct);
                if (joinedWithoutPassword.IsFailure)
                    return joinedWithoutPassword;

                var room = joinedWithoutPassword.Value;

                if (!room.HasPassword)
                    return joinedWithoutPassword;

                try { await _lobbyService.LeaveAsync(ct); }
                catch (Exception) { }

                probe = new ProbeResult(true, room.Name);
            }

            if (_passwordPanelService == null)
            {
                _infoPanelService?.Show(new InfoMessage("Private Room", "This room requires a password, but the password panel is unavailable."));
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "Password panel is unavailable.");
            }

            string error = null;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var prompt = await _passwordPanelService.RequestPasswordAsync(probe.DisplayName, error, ct);
                if (!prompt.Confirmed)
                    return Result<LobbyRoom>.Fail(DomainErrorCode.Cancelled, "Password entry was cancelled.");

                var room = await JoinTargetResultAsync(target, prompt.Password, ct);
                if (room.IsSuccess)
                    return room;

                if (room.Error.Code == DomainErrorCode.WrongPassword)
                {
                    error = "Wrong password. Try again.";
                    continue;
                }

                return room;
            }

            return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, "Wrong room password.");
        }

        private async Task<Result<LobbyRoom>> JoinTargetResultAsync(JoinRoomTarget target, string password = null, CancellationToken ct = default)
        {
            if (!target.IsValid)
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "Join target is invalid.");

            var room = await JoinExactTargetAsync(target, password, ct);
            if (room.IsSuccess || target.Kind != JoinRoomTargetKind.JoinCode)
                return room;

            var resolved = await ResolveJoinCodeAliasAsync(target.Value, ct);
            if (!resolved.IsValid ||
                (resolved.Kind == target.Kind && string.Equals(resolved.Value, target.Value, StringComparison.OrdinalIgnoreCase)))
            {
                return room;
            }
            return await JoinExactTargetAsync(resolved, password, ct);
        }

        private async Task<Result<LobbyRoom>> JoinExactTargetAsync(JoinRoomTarget target, string password, CancellationToken ct)
        {
            try
            {
                if (target.Kind == JoinRoomTargetKind.LobbyId)
                    return await JoinByIdWithOptionalPasswordAsync(target.Value, password, ct);

                LobbyRoom room;
                if (string.IsNullOrEmpty(password))
                    room = await _lobbyService.JoinByCodeAsync(target.Value, GetPlayerName(), ct);
                else
                    room = await _lobbyService.JoinByCodeWithPasswordAsync(target.Value, GetPlayerName(), password, ct);

                if (room == null)
                {
                    return Result<LobbyRoom>.Fail(
                        DomainErrorCode.NotFound,
                        $"Room '{target.Value}' was not found or is unavailable.");
                }

                return Result<LobbyRoom>.Success(room);
            }
            catch (WrongPasswordException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, ex.Message);
            }
            catch (RoomFullException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.RoomFull, ex.Message);
            }
            catch (RoomAccessDeniedException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.PermissionDenied, ex.Message);
            }
            catch (SessionExpiredException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.SessionExpired, ex.Message);
            }
            catch (RoomConfigMismatchException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, ex.Message);
            }
        }

        private async Task<Result<LobbyRoom>> JoinByIdWithOptionalPasswordAsync(string lobbyId, string password, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "LobbyId is empty.");

            var normalizedLobbyId = lobbyId.Trim();
            var room = await _lobbyService.JoinByIdAsync(normalizedLobbyId, GetPlayerName(), ct);
            if (room == null)
            {
                var fallback = await ResolveJoinCodeAliasAsync(normalizedLobbyId, ct);
                if (fallback.IsValid &&
                    !(fallback.Kind == JoinRoomTargetKind.LobbyId && string.Equals(fallback.Value, normalizedLobbyId, StringComparison.OrdinalIgnoreCase)))
                {
                    var fallbackResult = await JoinExactTargetAsync(fallback, password, ct);
                    if (fallbackResult.IsFailure)
                        return fallbackResult;

                    room = fallbackResult.Value;
                }
                else
                {
                }

                if (room == null)
                {
                    return Result<LobbyRoom>.Fail(
                        DomainErrorCode.NotFound,
                        $"Room with lobbyId '{normalizedLobbyId}' was not found or is closed.");
                }
            }

            if (!string.IsNullOrEmpty(password) && room.HasPassword && !LobbyPasswordHasher.Verify(password, room.PasswordHash))
            {
                try { await _lobbyService.LeaveAsync(ct); } catch { }
                return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, "Wrong room password.");
            }

            return Result<LobbyRoom>.Success(room);
        }

        private async Task<JoinRoomTarget> ResolveJoinCodeAliasAsync(string value, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

            try
            {
                var rooms = await _lobbyService.QueryRoomsAsync(ct);
                if (rooms == null)
                    return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

                foreach (var room in rooms)
                {
                    if (room == null)
                        continue;

                    if (string.Equals(room.LobbyId, value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.LobbyCode.Trim());

                        if (!string.IsNullOrWhiteSpace(room.RelayJoinCode))
                            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.RelayJoinCode.Trim());

                        if (!string.IsNullOrWhiteSpace(room.LobbyId))
                            return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, room.LobbyId.Trim());
                    }

                    if (string.Equals(room.RelayJoinCode, value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(room.LobbyId))
                            return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, room.LobbyId.Trim());

                        if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.LobbyCode.Trim());
                    }
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception)
            {
            }

            return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);
        }

    }
}
