using System;
using System.Collections.Generic;
using System.Text;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal static class BotAnalyzerTimelineBuilder
    {
        public static bool MatchesFilter(BotAnalyzerEvent e, BotAnalyzerTimelineFilter filter)
        {
            if (e == null) return false;
            if (filter == BotAnalyzerTimelineFilter.All) return true;

            return filter switch
            {
                BotAnalyzerTimelineFilter.Decision =>
                    e.Type == BotAnalyzerEventType.StrategyChanged ||
                    e.Type == BotAnalyzerEventType.GoalChanged ||
                    e.Type == BotAnalyzerEventType.CandidateSetChanged ||
                    e.Type == BotAnalyzerEventType.TopCandidateChanged,

                BotAnalyzerTimelineFilter.Action =>
                    e.Severity == BotAnalyzerEventSeverity.Action,

                BotAnalyzerTimelineFilter.Movement =>
                    e.Type == BotAnalyzerEventType.UnitMoved,

                BotAnalyzerTimelineFilter.Combat =>
                    e.Type == BotAnalyzerEventType.CombatObserved,

                BotAnalyzerTimelineFilter.Build =>
                    e.Type == BotAnalyzerEventType.BuildingStarted ||
                    e.Type == BotAnalyzerEventType.BuildingProgress ||
                    e.Type == BotAnalyzerEventType.BuildingPlaced ||
                    e.Type == BotAnalyzerEventType.BuildingRemoved,

                BotAnalyzerTimelineFilter.Economy =>
                    e.Type == BotAnalyzerEventType.ResourceChanged ||
                    e.Type == BotAnalyzerEventType.RecruitmentStarted ||
                    e.Type == BotAnalyzerEventType.RecruitmentProgress ||
                    e.Type == BotAnalyzerEventType.RecruitmentReady ||
                    e.Type == BotAnalyzerEventType.RecruitmentDeployed,

                BotAnalyzerTimelineFilter.Fog =>
                    e.Type == BotAnalyzerEventType.FogVisible ||
                    e.Type == BotAnalyzerEventType.FogExplored,

                BotAnalyzerTimelineFilter.Observation =>
                    e.Type == BotAnalyzerEventType.Observation ||
                    e.Type == BotAnalyzerEventType.ObservationLost ||
                    e.Type == BotAnalyzerEventType.MemoryChanged,

                BotAnalyzerTimelineFilter.Warning =>
                    e.Severity == BotAnalyzerEventSeverity.Warning ||
                    e.Severity == BotAnalyzerEventSeverity.Error ||
                    e.Type == BotAnalyzerEventType.Warning ||
                    e.Type == BotAnalyzerEventType.Error,

                _ => true,
            };
        }

        public static string FormatCompact(BotAnalyzerEvent e)
        {
            if (e == null) return string.Empty;
            string time = ParseTime(e.UtcTimestamp);
            return $"{time}  T{e.GlobalTurn}  [{e.Type}]  {e.Title}";
        }

        public static string FormatDetailed(BotAnalyzerEvent e)
        {
            if (e == null) return string.Empty;

            var b = new StringBuilder();
            b.AppendLine($"{ParseTime(e.UtcTimestamp)}  •  Turn {e.GlobalTurn}  •  {e.Type}");
            b.AppendLine(e.Title ?? string.Empty);

            if (e.HasScore)
                b.AppendLine($"Score: {e.Score}");
            if (!string.IsNullOrWhiteSpace(e.ActorId))
                b.AppendLine($"Actor: {e.ActorId}");
            if (!string.IsNullOrWhiteSpace(e.TargetId))
                b.AppendLine($"Target: {e.TargetId}");
            if (e.HasFromCell || e.HasToCell)
                b.AppendLine($"Cell: {(e.HasFromCell ? e.FromCell.ToString() : "—")} → {(e.HasToCell ? e.ToCell.ToString() : "—")}");

            if (!string.IsNullOrWhiteSpace(e.Detail))
            {
                b.AppendLine();
                b.Append(e.Detail.Trim());
            }

            return b.ToString().TrimEnd();
        }

        public static string BuildTimelineText(
            IReadOnlyList<BotAnalyzerEvent> events,
            BotAnalyzerTimelineFilter filter = BotAnalyzerTimelineFilter.All)
        {
            var b = new StringBuilder();
            if (events == null) return string.Empty;

            for (int i = 0; i < events.Count; i++)
            {
                BotAnalyzerEvent e = events[i];
                if (!MatchesFilter(e, filter)) continue;
                if (b.Length > 0) b.AppendLine().AppendLine();
                b.Append(FormatDetailed(e));
            }
            return b.ToString();
        }

        public static string BuildCurrentSummary(BotAnalyzerFrame frame)
        {
            if (frame == null)
                return "No runtime frame captured.";

            var b = new StringBuilder();
            b.AppendLine($"Bot: {frame.OwnerId}");
            b.AppendLine($"Round: {frame.Round}");
            b.AppendLine($"Global Turn: {frame.GlobalTurn}");
            b.AppendLine($"Phase: {frame.Phase}");
            b.AppendLine($"Actions This Turn: {frame.ActionsThisTurn}");
            b.AppendLine($"Active: {frame.SelectedBotActive}");

            if (frame.Strategy?.Available == true)
            {
                b.AppendLine();
                b.AppendLine($"Strategy: {frame.Strategy.Posture} ({frame.Strategy.Score})");
                b.AppendLine($"Strategy Reason: {frame.Strategy.Reason}");
            }

            if (frame.Goal?.Available == true)
            {
                b.AppendLine();
                b.AppendLine($"Goal: {frame.Goal.Kind} (priority {frame.Goal.Priority})");
                b.AppendLine($"Goal Reason: {frame.Goal.Reason}");
            }

            b.AppendLine();
            b.AppendLine($"Own Units: {frame.OwnUnits?.Count ?? 0}");
            b.AppendLine($"Own Buildings: {frame.OwnBuildings?.Count ?? 0}");
            b.AppendLine($"Visible Enemies: {(frame.VisibleEnemyUnits?.Count ?? 0) + (frame.VisibleEnemyBuildings?.Count ?? 0)}");
            b.AppendLine($"Fog Visible: {frame.Fog?.VisibleCount ?? 0}");
            b.AppendLine($"Fog Explored: {frame.Fog?.ExploredCount ?? 0}");
            return b.ToString().TrimEnd();
        }

        private static string ParseTime(string utc)
        {
            if (DateTime.TryParse(
                    utc,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out DateTime value))
            {
                return value.ToLocalTime().ToString("HH:mm:ss.fff");
            }
            return "--:--:--.---";
        }
    }
}
