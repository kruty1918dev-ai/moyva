using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    [Serializable]
    public sealed class BotAnalyzerOverviewModel
    {
        [ReadOnly] public string Bot = string.Empty;
        [ReadOnly] public string RuntimeStatus = string.Empty;
        [ReadOnly] public int Round;
        [ReadOnly] public long GlobalTurn;
        [ReadOnly] public string Phase = string.Empty;
        [ReadOnly] public int ActionsThisTurn;
        [ReadOnly] public bool IsActive;
        [ReadOnly] public string Strategy = string.Empty;
        [ReadOnly] public int StrategyScore;
        [MultiLineProperty(3), ReadOnly] public string StrategyReason = string.Empty;
        [ReadOnly] public string Goal = string.Empty;
        [ReadOnly] public int GoalPriority;
        [ReadOnly] public long GoalHoldUntilTurn;
        [MultiLineProperty(3), ReadOnly] public string GoalReason = string.Empty;
    }

    [Serializable]
    public sealed class BotAnalyzerResourceRow
    {
        [TableColumnWidth(170)] public string Resource = string.Empty;
        [TableColumnWidth(100)] public float Current;
        [TableColumnWidth(100)] public float Delta;
    }

    [Serializable]
    public sealed class BotAnalyzerUnitRow
    {
        public string Unit = string.Empty;
        public string Type = string.Empty;
        public Vector2Int Cell;
        public float Stamina;
        public string Role = string.Empty;
    }

    [Serializable]
    public sealed class BotAnalyzerBuildingRow
    {
        public string Building = string.Empty;
        public Vector2Int Cell;
        public string State = string.Empty;
        public string Progress = string.Empty;
    }

    [Serializable]
    public sealed class BotAnalyzerRecruitmentRow
    {
        public long Queue;
        public string Unit = string.Empty;
        public string Building = string.Empty;
        public Vector2Int Cell;
        public string Status = string.Empty;
        public string Progress = string.Empty;
        public int RemainingTurns;
    }

    [Serializable]
    public sealed class BotAnalyzerCandidateRow
    {
        [TableColumnWidth(45)] public int Rank;
        [TableColumnWidth(90)] public string Kind = string.Empty;
        [TableColumnWidth(70)] public int Score;
        public string Candidate = string.Empty;
        [MultiLineProperty(2)] public string Explanation = string.Empty;
    }

    [Serializable]
    public sealed class BotAnalyzerReasoningRow
    {
        [TableColumnWidth(70)] public long Turn;
        [TableColumnWidth(110)] public string Stage = string.Empty;
        [TableColumnWidth(80)] public int Score;
        public string Headline = string.Empty;
        [MultiLineProperty(3)] public string Narrative = string.Empty;
        public string Target = string.Empty;
    }

    [Serializable]
    public sealed class BotAnalyzerTimelineRow
    {
        [TableColumnWidth(130)] public string Time = string.Empty;
        [TableColumnWidth(130)] public string Type = string.Empty;
        public string Title = string.Empty;
        [MultiLineProperty(3)] public string Detail = string.Empty;
    }

    [Serializable]
    public sealed class BotAnalyzerFogSummary
    {
        [ReadOnly] public int VisibleNow;
        [ReadOnly] public int ExploredMemory;
        [ReadOnly] public int KnownTotal;
        [ReadOnly] public int MapCells;
        [ReadOnly] public float KnownPercent;
        [ReadOnly] public float VisiblePercent;
    }

    [Serializable]
    public sealed class BotAnalyzerMemoryRow
    {
        public string Entity = string.Empty;
        public string Kind = string.Empty;
        public string Type = string.Empty;
        public Vector2Int LastKnownCell;
        public long LastSeenTurn;
        public int LastKnownHp;
        public bool Destroyed;
    }

    internal static class BotAnalyzerWindowModels
    {
        public static BotAnalyzerOverviewModel BuildOverview(
            BotAnalyzerFrame frame,
            string runtimeStatus)
        {
            if (frame == null)
            {
                return new BotAnalyzerOverviewModel
                {
                    RuntimeStatus = runtimeStatus ?? "No frame captured.",
                };
            }

            return new BotAnalyzerOverviewModel
            {
                Bot = frame.OwnerId,
                RuntimeStatus = runtimeStatus ?? string.Empty,
                Round = frame.Round,
                GlobalTurn = frame.GlobalTurn,
                Phase = frame.Phase,
                ActionsThisTurn = frame.ActionsThisTurn,
                IsActive = frame.SelectedBotActive,
                Strategy = frame.Strategy?.Available == true ? frame.Strategy.Posture : "Unavailable",
                StrategyScore = frame.Strategy?.Available == true ? frame.Strategy.Score : 0,
                StrategyReason = frame.Strategy?.Available == true ? frame.Strategy.Reason : string.Empty,
                Goal = frame.Goal?.Available == true ? frame.Goal.Kind : "Unavailable",
                GoalPriority = frame.Goal?.Available == true ? frame.Goal.Priority : 0,
                GoalHoldUntilTurn = frame.Goal?.Available == true ? frame.Goal.HoldUntilTurn : 0,
                GoalReason = frame.Goal?.Available == true ? frame.Goal.Reason : string.Empty,
            };
        }

        public static List<BotAnalyzerResourceRow> BuildResources(
            BotAnalyzerFrame current,
            BotAnalyzerFrame previous)
        {
            var result = new List<BotAnalyzerResourceRow>();
            if (current?.Resources == null)
                return result;

            var old = new Dictionary<string, float>(StringComparer.Ordinal);
            if (previous?.Resources != null)
            {
                foreach (BotAnalyzerResourceState item in previous.Resources)
                    if (item != null && !string.IsNullOrWhiteSpace(item.ResourceId))
                        old[item.ResourceId] = item.Amount;
            }

            foreach (BotAnalyzerResourceState item in current.Resources)
            {
                if (item == null)
                    continue;

                old.TryGetValue(item.ResourceId ?? string.Empty, out float previousAmount);
                result.Add(new BotAnalyzerResourceRow
                {
                    Resource = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ResourceId : item.DisplayName,
                    Current = item.Amount,
                    Delta = previous == null ? 0f : item.Amount - previousAmount,
                });
            }
            return result;
        }

        public static List<BotAnalyzerUnitRow> BuildUnits(IReadOnlyList<BotAnalyzerUnitState> units)
        {
            var result = new List<BotAnalyzerUnitRow>();
            if (units == null) return result;
            foreach (BotAnalyzerUnitState item in units)
            {
                if (item == null) continue;
                result.Add(new BotAnalyzerUnitRow
                {
                    Unit = item.UnitId,
                    Type = item.TypeId,
                    Cell = item.Cell,
                    Stamina = item.Stamina,
                    Role = item.TacticalRole,
                });
            }
            return result;
        }

        public static List<BotAnalyzerBuildingRow> BuildBuildings(IReadOnlyList<BotAnalyzerBuildingState> buildings)
        {
            var result = new List<BotAnalyzerBuildingRow>();
            if (buildings == null) return result;
            foreach (BotAnalyzerBuildingState item in buildings)
            {
                if (item == null) continue;
                result.Add(new BotAnalyzerBuildingRow
                {
                    Building = item.BuildingId,
                    Cell = item.Cell,
                    State = item.Operational ? "Operational" : "Under Construction",
                    Progress = item.HasProgress ? $"{item.CompletedTurns}/{item.RequiredTurns}" : "—",
                });
            }
            return result;
        }

        public static List<BotAnalyzerRecruitmentRow> BuildRecruitment(IReadOnlyList<BotAnalyzerRecruitmentState> items)
        {
            var result = new List<BotAnalyzerRecruitmentRow>();
            if (items == null) return result;
            foreach (BotAnalyzerRecruitmentState item in items)
            {
                if (item == null) continue;
                result.Add(new BotAnalyzerRecruitmentRow
                {
                    Queue = item.QueueId,
                    Unit = item.UnitTypeId,
                    Building = item.BuildingId,
                    Cell = item.BuildingCell,
                    Status = item.Status,
                    Progress = $"{item.CompletedTurns}/{item.TrainingTurns}",
                    RemainingTurns = item.RemainingTurns,
                });
            }
            return result;
        }

        public static List<BotAnalyzerCandidateRow> BuildCandidates(IReadOnlyList<BotAnalyzerCandidateState> candidates)
        {
            var result = new List<BotAnalyzerCandidateRow>();
            if (candidates == null) return result;
            foreach (BotAnalyzerCandidateState item in candidates)
            {
                if (item == null) continue;
                result.Add(new BotAnalyzerCandidateRow
                {
                    Rank = item.Rank,
                    Kind = item.Kind,
                    Score = item.Score,
                    Candidate = item.CandidateId,
                    Explanation = item.Explanation,
                });
            }
            return result;
        }

        public static List<BotAnalyzerReasoningRow> BuildReasoning(
            IReadOnlyList<BotAnalyzerReasoningState> reasoning)
        {
            var result = new List<BotAnalyzerReasoningRow>();
            if (reasoning == null)
                return result;

            for (int i = reasoning.Count - 1; i >= 0; i--)
            {
                BotAnalyzerReasoningState item = reasoning[i];
                if (item == null)
                    continue;

                result.Add(new BotAnalyzerReasoningRow
                {
                    Turn = item.GlobalTurn,
                    Stage = item.Stage,
                    Score = item.Score,
                    Headline = item.Headline,
                    Narrative = item.Narrative,
                    Target = item.HasTargetCell
                        ? item.TargetCell.ToString()
                        : item.SubjectId ?? string.Empty,
                });
            }

            return result;
        }

        public static List<BotAnalyzerTimelineRow> BuildTimeline(IReadOnlyList<BotAnalyzerEvent> events)
        {
            var result = new List<BotAnalyzerTimelineRow>();
            if (events == null) return result;
            foreach (BotAnalyzerEvent e in events)
            {
                if (e == null) continue;
                string compact = BotAnalyzerTimelineBuilder.FormatCompact(e);
                string time = compact.Length >= 12 ? compact.Substring(0, 12).Trim() : string.Empty;
                result.Add(new BotAnalyzerTimelineRow
                {
                    Time = time,
                    Type = e.Type.ToString(),
                    Title = e.Title,
                    Detail = e.Detail,
                });
            }
            return result;
        }

        public static BotAnalyzerFogSummary BuildFog(BotAnalyzerFogState fog)
        {
            fog ??= new BotAnalyzerFogState();
            return new BotAnalyzerFogSummary
            {
                VisibleNow = fog.VisibleCount,
                ExploredMemory = fog.ExploredCount,
                KnownTotal = fog.KnownCount,
                MapCells = fog.TotalCells,
                KnownPercent = fog.KnownPercent,
                VisiblePercent = fog.VisiblePercent,
            };
        }

        public static List<BotAnalyzerMemoryRow> BuildMemory(IReadOnlyList<BotAnalyzerMemoryState> memory)
        {
            var result = new List<BotAnalyzerMemoryRow>();
            if (memory == null) return result;
            foreach (BotAnalyzerMemoryState item in memory)
            {
                if (item == null) continue;
                result.Add(new BotAnalyzerMemoryRow
                {
                    Entity = item.EntityId,
                    Kind = item.Kind,
                    Type = item.TypeId,
                    LastKnownCell = item.LastKnownCell,
                    LastSeenTurn = item.LastSeenGlobalTurn,
                    LastKnownHp = item.LastKnownHp,
                    Destroyed = item.ConfirmedDestroyed,
                });
            }
            return result;
        }
    }
}
