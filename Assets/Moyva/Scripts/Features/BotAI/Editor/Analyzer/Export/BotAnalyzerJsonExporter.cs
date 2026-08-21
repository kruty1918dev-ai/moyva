using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal static class BotAnalyzerJsonExporter
    {
        public const string Schema = "moyva.bot-analyzer.v1";

        [Serializable]
        private sealed class Report
        {
            public string schema = Schema;
            public string generatedUtc = string.Empty;
            public string sessionStartedUtc = string.Empty;
            public string scene = string.Empty;
            public string botId = string.Empty;
            public BotAnalyzerFrame currentFrame;
            public List<BotAnalyzerFrame> frames = new();
            public List<ExportEvent> events = new();
        }

        [Serializable]
        private sealed class ExportEvent
        {
            public long sequence;
            public double editorTime;
            public string utcTimestamp = string.Empty;
            public long globalTurn;
            public string ownerId = string.Empty;
            public string type = string.Empty;
            public string severity = string.Empty;
            public string actorId = string.Empty;
            public string targetId = string.Empty;
            public bool hasFromCell;
            public UnityEngine.Vector2Int fromCell;
            public bool hasToCell;
            public UnityEngine.Vector2Int toCell;
            public string title = string.Empty;
            public string detail = string.Empty;
            public bool hasScore;
            public int score;
        }

        public static string BuildJson(
            BotAnalyzerSession session,
            BotAnalyzerSettings settings)
        {
            settings ??= BotAnalyzerSettings.CreateDefault();
            var report = new Report
            {
                generatedUtc = DateTime.UtcNow.ToString("O"),
                sessionStartedUtc = session?.StartedUtc ?? string.Empty,
                scene = SceneManager.GetActiveScene().name ?? string.Empty,
                botId = session?.LastOwnerId ?? string.Empty,
                currentFrame = CloneFrame(session?.LatestFrame, settings),
            };

            if (session != null)
            {
                if (settings.IncludeFramesInExport)
                {
                    foreach (BotAnalyzerFrame frame in session.Frames)
                    {
                        BotAnalyzerFrame clone = CloneFrame(frame, settings);
                        if (clone != null)
                            report.frames.Add(clone);
                    }
                }

                foreach (BotAnalyzerEvent e in session.Events)
                {
                    if (e != null)
                        report.events.Add(ConvertEvent(e));
                }
            }

            return JsonUtility.ToJson(report, true);
        }

        private static BotAnalyzerFrame CloneFrame(
            BotAnalyzerFrame source,
            BotAnalyzerSettings settings)
        {
            if (source == null)
                return null;

            string raw = JsonUtility.ToJson(source);
            BotAnalyzerFrame clone = JsonUtility.FromJson<BotAnalyzerFrame>(raw);
            if (clone == null)
                return null;

            if (!settings.IncludeFogCellsInExport && clone.Fog != null)
            {
                clone.Fog.VisibleCells?.Clear();
                clone.Fog.ExploredCells?.Clear();
            }

            if (!settings.IncludeMemoryInExport)
                clone.Memory?.Clear();

            return clone;
        }

        private static ExportEvent ConvertEvent(BotAnalyzerEvent source)
        {
            return new ExportEvent
            {
                sequence = source.Sequence,
                editorTime = source.EditorTime,
                utcTimestamp = source.UtcTimestamp,
                globalTurn = source.GlobalTurn,
                ownerId = source.OwnerId,
                type = source.Type.ToString(),
                severity = source.Severity.ToString(),
                actorId = source.ActorId,
                targetId = source.TargetId,
                hasFromCell = source.HasFromCell,
                fromCell = source.FromCell,
                hasToCell = source.HasToCell,
                toCell = source.ToCell,
                title = source.Title,
                detail = source.Detail,
                hasScore = source.HasScore,
                score = source.Score,
            };
        }
    }
}
