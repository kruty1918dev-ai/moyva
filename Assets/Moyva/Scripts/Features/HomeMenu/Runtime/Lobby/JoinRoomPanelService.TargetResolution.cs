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
                catch (Exception e) { Debug.LogWarning($"[JoinRoomPanelService] Leave before password retry failed: {e.Message}"); }

                probe = new ProbeResult(true, room.Name);
            }

            if (_passwordPanelService == null)
            {
                Debug.LogWarning("[JoinRoomPanelService] Кімната потребує пароль, але IPasswordPanelService не підключений.");
                _infoPanelService?.Show(new InfoMessage("Приватна кімната", "Ця кімната потребує пароль, але панель введення недоступна."));
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "Панель введення пароля недоступна.");
            }

            string error = null;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var prompt = await _passwordPanelService.RequestPasswordAsync(probe.DisplayName, error, ct);
                if (!prompt.Confirmed)
                    return Result<LobbyRoom>.Fail(DomainErrorCode.Cancelled, "Користувач скасував введення пароля.");

                var room = await JoinTargetResultAsync(target, prompt.Password, ct);
                if (room.IsSuccess)
                    return room;

                if (room.Error.Code == DomainErrorCode.WrongPassword)
                {
                    error = "Невірний пароль. Спробуйте ще раз.";
                    continue;
                }

                return room;
            }

            return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, "Невірний пароль кімнати.");
        }

        private async Task<Result<LobbyRoom>> JoinTargetResultAsync(JoinRoomTarget target, string password = null, CancellationToken ct = default)
        {
            if (!target.IsValid)
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "Ціль приєднання невалідна.");

            var room = await JoinExactTargetAsync(target, password, ct);
            if (room.IsSuccess || target.Kind != JoinRoomTargetKind.JoinCode)
                return room;

            var resolved = await ResolveJoinCodeAliasAsync(target.Value, ct);
            if (!resolved.IsValid ||
                (resolved.Kind == target.Kind && string.Equals(resolved.Value, target.Value, StringComparison.OrdinalIgnoreCase)))
            {
                return room;
            }

            Debug.LogWarning($"[JoinRoomPanelService] Join by code '{target.Value}' returned null; retrying as {resolved.Kind}='{resolved.Value}'.");
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
                        $"Кімнату '{target.Value}' не знайдено або вона недоступна.");
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
        }

        private async Task<Result<LobbyRoom>> JoinByIdWithOptionalPasswordAsync(string lobbyId, string password, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "LobbyId порожній.");

            var normalizedLobbyId = lobbyId.Trim();
            Debug.Log($"[JoinRoomPanelService] JoinByIdWithOptionalPasswordAsync: calling JoinByIdAsync('{normalizedLobbyId}')...");
            var room = await _lobbyService.JoinByIdAsync(normalizedLobbyId, GetPlayerName(), ct);
            Debug.Log($"[JoinRoomPanelService] JoinByIdWithOptionalPasswordAsync: JoinByIdAsync returned {(room == null ? "null" : $"room '{room.LobbyId}'")}.");
            if (room == null)
            {
                var fallback = await ResolveJoinCodeAliasAsync(normalizedLobbyId, ct);
                if (fallback.IsValid &&
                    !(fallback.Kind == JoinRoomTargetKind.LobbyId && string.Equals(fallback.Value, normalizedLobbyId, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.LogWarning($"[JoinRoomPanelService] JoinByIdAsync returned null for lobbyId='{normalizedLobbyId}', retrying via {fallback.Kind}='{fallback.Value}'.");
                    var fallbackResult = await JoinExactTargetAsync(fallback, password, ct);
                    if (fallbackResult.IsFailure)
                        return fallbackResult;

                    room = fallbackResult.Value;
                }
                else
                {
                    Debug.LogWarning($"[JoinRoomPanelService] JoinByIdAsync returned null for lobbyId='{normalizedLobbyId}', fallback={fallback.Kind}/'{fallback.Value}' isValid={fallback.IsValid} — giving up.");
                }

                if (room == null)
                {
                    return Result<LobbyRoom>.Fail(
                        DomainErrorCode.NotFound,
                        $"Кімнату з lobbyId '{normalizedLobbyId}' не знайдено або вона закрита.");
                }
            }

            if (!string.IsNullOrEmpty(password) && room.HasPassword && !LobbyPasswordHasher.Verify(password, room.PasswordHash))
            {
                try { await _lobbyService.LeaveAsync(ct); } catch { }
                return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, "Невірний пароль кімнати.");
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
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] ResolveJoinCodeAliasAsync failed for '{value}': {e.Message}");
            }

            return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);
        }

    }
}
