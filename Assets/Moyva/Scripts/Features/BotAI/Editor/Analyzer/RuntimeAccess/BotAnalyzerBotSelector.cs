using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal static class BotAnalyzerBotSelector
    {
        public static List<string> GetBotIds(IReadOnlyList<TurnFaction> factions)
        {
            var result = new List<string>();
            if (factions == null)
                return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < factions.Count; i++)
            {
                TurnFaction faction = factions[i];
                string ownerId = Normalize(faction.OwnerId);
                if (!faction.IsBot || ownerId == null || !seen.Add(ownerId))
                    continue;

                result.Add(ownerId);
            }

            result.Sort(StringComparer.Ordinal);
            return result;
        }

        public static string ResolveOwner(
            ITurnService turns,
            string requestedOwnerId,
            bool followActiveBot)
        {
            if (turns == null)
                return string.Empty;

            List<string> bots = GetBotIds(turns.Factions);
            if (bots.Count == 0)
                return string.Empty;

            string active = Normalize(turns.ActiveOwnerId);
            if (followActiveBot && turns.IsActiveFactionBot && active != null && bots.Contains(active))
                return active;

            string requested = Normalize(requestedOwnerId);
            if (requested != null && bots.Contains(requested))
                return requested;

            if (turns.IsActiveFactionBot && active != null && bots.Contains(active))
                return active;

            return bots[0];
        }

        public static bool IsBotOwner(ITurnService turns, string ownerId)
        {
            string normalized = Normalize(ownerId);
            if (turns == null || normalized == null)
                return false;

            IReadOnlyList<TurnFaction> factions = turns.Factions;
            if (factions == null)
                return false;

            for (int i = 0; i < factions.Count; i++)
            {
                if (factions[i].IsBot &&
                    string.Equals(Normalize(factions[i].OwnerId), normalized, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
