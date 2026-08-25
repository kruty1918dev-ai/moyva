using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    public readonly struct GameplayTurnHudAuthoritySnapshot
    {
        public GameplayTurnHudAuthoritySnapshot(
            TurnPhase phase,
            string activeOwnerId,
            string localOwnerId,
            bool isActiveFactionBot,
            bool isLocalOwnerTurn,
            bool canIssueLocalCommands,
            bool canEndTurn,
            IReadOnlyList<string> blockingReasons,
            string statusText)
        {
            Phase = phase;
            ActiveOwnerId = activeOwnerId ?? string.Empty;
            LocalOwnerId = localOwnerId ?? string.Empty;
            IsActiveFactionBot = isActiveFactionBot;
            IsLocalOwnerTurn = isLocalOwnerTurn;
            CanIssueLocalCommands = canIssueLocalCommands;
            CanEndTurn = canEndTurn;
            BlockingReasons = blockingReasons ?? Array.Empty<string>();
            StatusText = statusText ?? string.Empty;
        }

        public TurnPhase Phase { get; }
        public string ActiveOwnerId { get; }
        public string LocalOwnerId { get; }
        public bool IsActiveFactionBot { get; }
        public bool IsLocalOwnerTurn { get; }
        public bool CanIssueLocalCommands { get; }
        public bool CanEndTurn { get; }
        public IReadOnlyList<string> BlockingReasons { get; }
        public string StatusText { get; }
    }

    /// <summary>
    /// Pure read-only projection of authoritative turn state for local gameplay UI.
    /// It never mutates the turn service. End-turn still goes through ITurnService,
    /// so this policy is an early fail-closed UI gate rather than a second authority.
    /// </summary>
    public static class GameplayTurnHudAuthorityPolicy
    {
        public static GameplayTurnHudAuthoritySnapshot Evaluate(
            ITurnService turns,
            IReadOnlyList<ITurnBlocker> blockers = null)
        {
            if (turns == null)
            {
                return new GameplayTurnHudAuthoritySnapshot(
                    TurnPhase.Initializing,
                    string.Empty,
                    string.Empty,
                    false,
                    false,
                    false,
                    false,
                    Array.Empty<string>(),
                    "Система ходів недоступна.");
            }

            string activeOwnerId = Normalize(turns.ActiveOwnerId);
            string localOwnerId = Normalize(turns.LocalOwnerId);
            bool awaitingInput = turns.Phase == TurnPhase.AwaitingInput;
            bool sameOwner = localOwnerId.Length > 0
                && string.Equals(activeOwnerId, localOwnerId, StringComparison.Ordinal);
            bool localTurn = awaitingInput && sameOwner && !turns.IsActiveFactionBot;

            IReadOnlyList<string> blockerReasons = localTurn
                ? CollectBlockingReasons(blockers)
                : Array.Empty<string>();

            bool canEndTurn = localTurn && blockerReasons.Count == 0;
            string status = ResolveStatus(
                turns.Phase,
                activeOwnerId,
                localOwnerId,
                turns.IsActiveFactionBot,
                localTurn,
                blockerReasons);

            return new GameplayTurnHudAuthoritySnapshot(
                turns.Phase,
                activeOwnerId,
                localOwnerId,
                turns.IsActiveFactionBot,
                localTurn,
                localTurn,
                canEndTurn,
                blockerReasons,
                status);
        }

        public static string LocalizePhase(TurnPhase phase)
        {
            return phase switch
            {
                TurnPhase.Initializing => "Підготовка",
                TurnPhase.Starting => "Початок",
                TurnPhase.AwaitingInput => "Очікування дій",
                TurnPhase.Resolving => "Обробка",
                TurnPhase.Ending => "Завершення",
                _ => phase.ToString(),
            };
        }

        private static IReadOnlyList<string> CollectBlockingReasons(
            IReadOnlyList<ITurnBlocker> blockers)
        {
            if (blockers == null || blockers.Count == 0)
                return Array.Empty<string>();

            var unique = new SortedSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < blockers.Count; index++)
            {
                ITurnBlocker blocker = blockers[index];
                if (blocker == null)
                    continue;

                try
                {
                    if (!blocker.IsTurnBlocked(out string reason))
                        continue;

                    string normalized = Normalize(reason);
                    if (normalized.Length == 0)
                        normalized = $"Хід заблокований ({blocker.GetType().Name}).";
                    unique.Add(normalized);
                }
                catch
                {
                    // UI must fail closed if a blocker projection itself fails.
                    unique.Add($"Не вдалося перевірити блокер {blocker.GetType().Name}.");
                }
            }

            if (unique.Count == 0)
                return Array.Empty<string>();

            var result = new List<string>(unique.Count);
            foreach (string reason in unique)
                result.Add(reason);
            return result;
        }

        private static string ResolveStatus(
            TurnPhase phase,
            string activeOwnerId,
            string localOwnerId,
            bool isActiveFactionBot,
            bool isLocalTurn,
            IReadOnlyList<string> blockerReasons)
        {
            if (localOwnerId.Length == 0)
                return "Локального гравця не визначено.";

            if (phase != TurnPhase.AwaitingInput)
                return $"{LocalizePhase(phase)} ходу…";

            if (isActiveFactionBot)
                return activeOwnerId.Length == 0
                    ? "Хід бота."
                    : $"Хід бота: {activeOwnerId}";

            if (!isLocalTurn)
                return activeOwnerId.Length == 0
                    ? "Очікування іншого гравця."
                    : $"Хід іншого гравця: {activeOwnerId}";

            if (blockerReasons != null && blockerReasons.Count > 0)
                return "Очікування: " + string.Join(" · ", blockerReasons);

            return "Ваш хід";
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
