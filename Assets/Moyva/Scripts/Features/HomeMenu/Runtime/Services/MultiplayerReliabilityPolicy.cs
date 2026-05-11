using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Common;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal enum JoinPipelineState
    {
        Idle = 0,
        Preflight = 1,
        ResolvingTarget = 2,
        JoiningLobby = 3,
        ConnectingTransport = 4,
        Ready = 5,
        Failed = 6,
    }

    internal static class MultiplayerReliabilityPolicy
    {
        private static readonly Dictionary<NetworkProviderType, TimeSpan> JoinTimeoutByProvider = new Dictionary<NetworkProviderType, TimeSpan>
        {
            { NetworkProviderType.Relay, TimeSpan.FromSeconds(15) },
            { NetworkProviderType.Lan, TimeSpan.FromSeconds(8) },
            { NetworkProviderType.Offline, TimeSpan.FromSeconds(5) },
        };

        public static TimeSpan GetJoinTimeout(NetworkProviderType providerType)
        {
            return JoinTimeoutByProvider.TryGetValue(providerType, out var timeout)
                ? timeout
                : TimeSpan.FromSeconds(12);
        }

        public static async Task<T> RetryWithBackoffAndJitterAsync<T>(
            Func<CancellationToken, Task<T>> action,
            Func<T, bool> isSuccess,
            int maxAttempts,
            TimeSpan baseDelay,
            string operationName,
            CancellationToken ct)
        {
            Exception lastException = null;
            var random = new System.Random();

            for (int attempt = 1; attempt <= Math.Max(1, maxAttempts); attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var result = await action(ct);
                    if (isSuccess == null || isSuccess(result))
                        return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                if (attempt == maxAttempts)
                    break;

                var pow = Math.Pow(2, attempt - 1);
                var delayMs = baseDelay.TotalMilliseconds * pow;
                var jitter = random.NextDouble() * 0.25d * delayMs;
                var totalDelay = TimeSpan.FromMilliseconds(delayMs + jitter);
                Debug.LogWarning($"[MultiplayerReliability] {operationName} attempt {attempt}/{maxAttempts} failed, retrying in {totalDelay.TotalMilliseconds:0} ms");
                await Task.Delay(totalDelay, ct);
            }

            if (lastException != null)
                throw lastException;

            return default;
        }
    }

    internal sealed class MultiplayerActionRateLimiter
    {
        private readonly Dictionary<string, DateTime> _lastAt = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public bool Allow(string key, TimeSpan minInterval)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            var now = DateTime.UtcNow;
            if (_lastAt.TryGetValue(key, out var last) && now - last < minInterval)
                return false;

            _lastAt[key] = now;
            return true;
        }
    }

    internal sealed class MultiplayerIdempotencyGuard
    {
        private readonly HashSet<string> _inFlight = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool TryEnter(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return _inFlight.Add(key);
        }

        public void Exit(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _inFlight.Remove(key);
        }
    }

    internal readonly struct MultiplayerUserFacingError
    {
        public readonly string ErrorCode;
        public readonly string UserMessage;
        public readonly string ActionHint;
        public readonly string TraceId;

        public MultiplayerUserFacingError(string errorCode, string userMessage, string actionHint, string traceId)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? "MP-UNKNOWN" : errorCode;
            UserMessage = userMessage ?? string.Empty;
            ActionHint = actionHint ?? string.Empty;
            TraceId = traceId ?? string.Empty;
        }

        public static MultiplayerUserFacingError FromDomainError(DomainError error, string traceId)
        {
            switch (error.Code)
            {
                case DomainErrorCode.WrongPassword:
                    return new MultiplayerUserFacingError("MP-JOIN-401", "Невірний пароль кімнати.", "Перевірте пароль і повторіть спробу.", traceId);
                case DomainErrorCode.NotFound:
                    return new MultiplayerUserFacingError("MP-JOIN-404", "Кімнату не знайдено або вона вже закрита.", "Оновіть список і оберіть іншу кімнату.", traceId);
                case DomainErrorCode.Timeout:
                    return new MultiplayerUserFacingError("MP-NET-408", "Операція перевищила ліміт часу.", "Перевірте мережу та повторіть спробу.", traceId);
                case DomainErrorCode.Network:
                    return new MultiplayerUserFacingError("MP-NET-503", "Мережева операція не виконана.", "Спробуйте ще раз за кілька секунд.", traceId);
                case DomainErrorCode.Validation:
                    return new MultiplayerUserFacingError("MP-REQ-422", string.IsNullOrWhiteSpace(error.Message) ? "Некоректний запит." : error.Message, "Перевірте вхідні параметри.", traceId);
                default:
                    return new MultiplayerUserFacingError("MP-UNKNOWN", string.IsNullOrWhiteSpace(error.Message) ? "Невідома помилка мультиплеєра." : error.Message, "Повторіть спробу або перезайдіть у меню.", traceId);
            }
        }

        public string BuildDisplayMessage()
        {
            var tracePart = string.IsNullOrWhiteSpace(TraceId) ? string.Empty : $"\nTraceId: {TraceId}";
            var hintPart = string.IsNullOrWhiteSpace(ActionHint) ? string.Empty : $"\nПідказка: {ActionHint}";
            return $"[{ErrorCode}] {UserMessage}{hintPart}{tracePart}";
        }
    }

    internal static class MultiplayerPreflightChecks
    {
        public static Result ValidateForJoin(bool hasLobbyService, bool hasNetworkProvider, bool hasModeSelector)
        {
            if (!hasLobbyService)
                return Result.Fail(DomainErrorCode.NotFound, "Lobby service недоступний.");
            if (!hasNetworkProvider)
                return Result.Fail(DomainErrorCode.NotFound, "Network provider недоступний.");
            if (!hasModeSelector)
                return Result.Fail(DomainErrorCode.Validation, "Mode selector недоступний.");

            return Result.Success();
        }

        public static Result ValidateSessionReadiness(bool hasLobbyService, bool hasGameStarter, bool hasCommandSync)
        {
            if (!hasLobbyService)
                return Result.Fail(DomainErrorCode.NotFound, "Lobby service недоступний.");
            if (!hasGameStarter)
                return Result.Fail(DomainErrorCode.NotFound, "Game starter недоступний.");
            if (!hasCommandSync)
                return Result.Fail(DomainErrorCode.NotFound, "Command sync service недоступний.");

            return Result.Success();
        }
    }
}
