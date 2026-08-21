using System;
using System.Collections.Generic;
using System.Text;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal static class BotAnalyzerMarkdownExporter
    {
        public static string Build(
            BotAnalyzerSession session,
            BotAnalyzerSettings settings)
        {
            settings ??= BotAnalyzerSettings.CreateDefault();
            BotAnalyzerFrame frame = session?.LatestFrame;

            var b = new StringBuilder();
            b.AppendLine("# Moyva Bot Analyzer Report");
            b.AppendLine();
            b.AppendLine($"- Schema: `{BotAnalyzerJsonExporter.Schema}`");
            b.AppendLine($"- Generated UTC: `{DateTime.UtcNow:O}`");
            b.AppendLine($"- Session started UTC: `{session?.StartedUtc ?? string.Empty}`");
            b.AppendLine($"- Bot: `{frame?.OwnerId ?? session?.LastOwnerId ?? string.Empty}`");

            AppendOverview(b, frame);
            AppendResources(b, frame);
            AppendUnits(b, "Own Units", frame?.OwnUnits);
            AppendBuildings(b, "Own Buildings", frame?.OwnBuildings);
            AppendRecruitment(b, frame);
            AppendFog(b, frame, settings);
            AppendCandidates(b, frame);
            if (settings.IncludeMemoryInExport)
                AppendMemory(b, frame);
            AppendTimeline(b, session?.Events);

            return b.ToString().TrimEnd();
        }

        private static void AppendOverview(StringBuilder b, BotAnalyzerFrame frame)
        {
            b.AppendLine();
            b.AppendLine("## Current State");
            b.AppendLine();

            if (frame == null)
            {
                b.AppendLine("No runtime frame captured.");
                return;
            }

            b.AppendLine($"- Round: **{frame.Round}**");
            b.AppendLine($"- Global turn: **{frame.GlobalTurn}**");
            b.AppendLine($"- Phase: **{Escape(frame.Phase)}**");
            b.AppendLine($"- Actions this turn: **{frame.ActionsThisTurn}**");
            b.AppendLine($"- Selected bot active: **{frame.SelectedBotActive}**");

            b.AppendLine();
            b.AppendLine("### Strategy");
            if (frame.Strategy?.Available == true)
            {
                b.AppendLine($"**{Escape(frame.Strategy.Posture)}** — score **{frame.Strategy.Score}**");
                b.AppendLine();
                b.AppendLine(EscapeMultiline(frame.Strategy.Reason));
            }
            else
            {
                b.AppendLine("Unavailable.");
            }

            b.AppendLine();
            b.AppendLine("### Goal");
            if (frame.Goal?.Available == true)
            {
                b.AppendLine($"**{Escape(frame.Goal.Kind)}** — priority **{frame.Goal.Priority}**, hold until turn **{frame.Goal.HoldUntilTurn}**");
                if (!string.IsNullOrWhiteSpace(frame.Goal.TargetId))
                    b.AppendLine($"- Target: `{Escape(frame.Goal.TargetId)}`");
                if (frame.Goal.HasTargetCell)
                    b.AppendLine($"- Target cell: `{frame.Goal.TargetCell}`");
                b.AppendLine();
                b.AppendLine(EscapeMultiline(frame.Goal.Reason));
            }
            else
            {
                b.AppendLine("Unavailable.");
            }
        }

        private static void AppendResources(StringBuilder b, BotAnalyzerFrame frame)
        {
            b.AppendLine();
            b.AppendLine("## Resources");
            b.AppendLine();
            b.AppendLine("| Resource | Amount |");
            b.AppendLine("|---|---:|");

            if (frame?.Resources == null || frame.Resources.Count == 0)
            {
                b.AppendLine("| — | — |");
                return;
            }

            foreach (BotAnalyzerResourceState item in frame.Resources)
            {
                if (item == null) continue;
                string name = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ResourceId : item.DisplayName;
                b.AppendLine($"| {EscapeCell(name)} | {item.Amount:0.##} |");
            }
        }

        private static void AppendUnits(
            StringBuilder b,
            string title,
            IReadOnlyList<BotAnalyzerUnitState> units)
        {
            b.AppendLine();
            b.AppendLine($"## {title}");
            b.AppendLine();
            b.AppendLine("| Unit | Type | Cell | Stamina | Role |");
            b.AppendLine("|---|---|---|---:|---|");

            if (units == null || units.Count == 0)
            {
                b.AppendLine("| — | — | — | — | — |");
                return;
            }

            foreach (BotAnalyzerUnitState unit in units)
            {
                if (unit == null) continue;
                b.AppendLine($"| {EscapeCell(unit.UnitId)} | {EscapeCell(unit.TypeId)} | `{unit.Cell}` | {unit.Stamina:0.##} | {EscapeCell(unit.TacticalRole)} |");
            }
        }

        private static void AppendBuildings(
            StringBuilder b,
            string title,
            IReadOnlyList<BotAnalyzerBuildingState> buildings)
        {
            b.AppendLine();
            b.AppendLine($"## {title}");
            b.AppendLine();
            b.AppendLine("| Building | Cell | State | Progress |");
            b.AppendLine("|---|---|---|---|");

            if (buildings == null || buildings.Count == 0)
            {
                b.AppendLine("| — | — | — | — |");
                return;
            }

            foreach (BotAnalyzerBuildingState item in buildings)
            {
                if (item == null) continue;
                string state = item.Operational ? "Operational" : "Under Construction";
                string progress = item.HasProgress ? $"{item.CompletedTurns}/{item.RequiredTurns}" : "—";
                b.AppendLine($"| {EscapeCell(item.BuildingId)} | `{item.Cell}` | {state} | {progress} |");
            }
        }

        private static void AppendRecruitment(StringBuilder b, BotAnalyzerFrame frame)
        {
            b.AppendLine();
            b.AppendLine("## Recruitment");
            b.AppendLine();
            b.AppendLine("| Queue | Unit | Building | Cell | Status | Progress | Remaining |");
            b.AppendLine("|---:|---|---|---|---|---|---:|");

            if (frame?.Recruitment == null || frame.Recruitment.Count == 0)
            {
                b.AppendLine("| — | — | — | — | — | — | — |");
                return;
            }

            foreach (BotAnalyzerRecruitmentState item in frame.Recruitment)
            {
                if (item == null) continue;
                b.AppendLine($"| {item.QueueId} | {EscapeCell(item.UnitTypeId)} | {EscapeCell(item.BuildingId)} | `{item.BuildingCell}` | {EscapeCell(item.Status)} | {item.CompletedTurns}/{item.TrainingTurns} | {item.RemainingTurns} |");
            }
        }

        private static void AppendFog(
            StringBuilder b,
            BotAnalyzerFrame frame,
            BotAnalyzerSettings settings)
        {
            BotAnalyzerFogState fog = frame?.Fog;
            b.AppendLine();
            b.AppendLine("## Fog Knowledge");
            b.AppendLine();

            if (fog == null)
            {
                b.AppendLine("Unavailable.");
                return;
            }

            b.AppendLine($"- Visible now: **{fog.VisibleCount}** cells");
            b.AppendLine($"- Explored memory: **{fog.ExploredCount}** cells");
            b.AppendLine($"- Known total: **{fog.KnownCount}/{fog.TotalCells}** ({fog.KnownPercent:0.##}%)");
            b.AppendLine($"- Currently visible: **{fog.VisiblePercent:0.##}%** of map");

            if (settings.IncludeFogCellsInExport)
            {
                b.AppendLine();
                b.AppendLine($"Visible cells: `{JoinCells(fog.VisibleCells, 1000)}`");
                b.AppendLine();
                b.AppendLine($"Explored cells: `{JoinCells(fog.ExploredCells, 1000)}`");
            }
        }

        private static void AppendCandidates(StringBuilder b, BotAnalyzerFrame frame)
        {
            b.AppendLine();
            b.AppendLine("## Decision Candidates");
            b.AppendLine();
            b.AppendLine("| Rank | Kind | Score | Candidate | Explanation |");
            b.AppendLine("|---:|---|---:|---|---|");

            if (frame?.Candidates == null || frame.Candidates.Count == 0)
            {
                b.AppendLine("| — | — | — | — | — |");
                return;
            }

            foreach (BotAnalyzerCandidateState item in frame.Candidates)
            {
                if (item == null) continue;
                b.AppendLine($"| {item.Rank} | {EscapeCell(item.Kind)} | {item.Score} | {EscapeCell(item.CandidateId)} | {EscapeCell(item.Explanation)} |");
            }
        }

        private static void AppendMemory(StringBuilder b, BotAnalyzerFrame frame)
        {
            b.AppendLine();
            b.AppendLine("## Bot Memory");
            b.AppendLine();
            b.AppendLine("| Entity | Kind | Type | Last Cell | Last Seen Turn | HP | Destroyed |");
            b.AppendLine("|---|---|---|---|---:|---:|---|");

            if (frame?.Memory == null || frame.Memory.Count == 0)
            {
                b.AppendLine("| — | — | — | — | — | — | — |");
                return;
            }

            foreach (BotAnalyzerMemoryState item in frame.Memory)
            {
                if (item == null) continue;
                b.AppendLine($"| {EscapeCell(item.EntityId)} | {EscapeCell(item.Kind)} | {EscapeCell(item.TypeId)} | `{item.LastKnownCell}` | {item.LastSeenGlobalTurn} | {item.LastKnownHp} | {item.ConfirmedDestroyed} |");
            }
        }

        private static void AppendTimeline(
            StringBuilder b,
            IReadOnlyList<BotAnalyzerEvent> events)
        {
            b.AppendLine();
            b.AppendLine("## Decision & Action Timeline");

            if (events == null || events.Count == 0)
            {
                b.AppendLine();
                b.AppendLine("No events captured.");
                return;
            }

            foreach (BotAnalyzerEvent e in events)
            {
                if (e == null) continue;
                b.AppendLine();
                b.AppendLine($"### Turn {e.GlobalTurn} — {e.Type}");
                b.AppendLine();
                b.AppendLine($"**{Escape(e.Title)}**");
                if (e.HasScore)
                    b.AppendLine($"- Score: {e.Score}");
                if (!string.IsNullOrWhiteSpace(e.ActorId))
                    b.AppendLine($"- Actor: `{Escape(e.ActorId)}`");
                if (!string.IsNullOrWhiteSpace(e.TargetId))
                    b.AppendLine($"- Target: `{Escape(e.TargetId)}`");
                if (e.HasFromCell || e.HasToCell)
                    b.AppendLine($"- Cell: `{(e.HasFromCell ? e.FromCell.ToString() : "—")} → {(e.HasToCell ? e.ToCell.ToString() : "—")}`");
                if (!string.IsNullOrWhiteSpace(e.Detail))
                {
                    b.AppendLine();
                    b.AppendLine(EscapeMultiline(e.Detail));
                }
            }
        }

        private static string Escape(string value)
            => (value ?? string.Empty).Replace("\r", string.Empty).Trim();

        private static string EscapeMultiline(string value)
            => Escape(value);

        private static string EscapeCell(string value)
            => Escape(value).Replace("|", "\\|").Replace("\n", "<br>");

        private static string JoinCells(IReadOnlyList<UnityEngine.Vector2Int> cells, int cap)
        {
            if (cells == null || cells.Count == 0)
                return string.Empty;

            int count = Math.Min(cap, cells.Count);
            var b = new StringBuilder(count * 10);
            for (int i = 0; i < count; i++)
            {
                if (i > 0) b.Append(", ");
                b.Append(cells[i].x).Append(':').Append(cells[i].y);
            }
            if (cells.Count > cap)
                b.Append($" … +{cells.Count - cap} more");
            return b.ToString();
        }
    }
}
