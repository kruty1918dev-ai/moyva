using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsReasoningObserver : ITickable, IInitializable
    {
        private readonly IBotReasoningTrace _trace;
        private readonly ITurnService _turns;
        private readonly IBotDiagnosticsLogger _log;
        private readonly BotDiagnosticsSettings _settings;
        private readonly Dictionary<string, long> _lastSequence =
            new(StringComparer.Ordinal);

        private bool _missingLogged;

        public BotDiagnosticsReasoningObserver(
            IBotDiagnosticsLogger log,
            BotDiagnosticsSettings settings,
            [InjectOptional] IBotReasoningTrace trace = null,
            [InjectOptional] ITurnService turns = null)
        {
            _log = log;
            _settings = settings;
            _trace = trace;
            _turns = turns;
        }

        public void Initialize()
        {
            if (_trace == null)
            {
                _missingLogged = true;
                _log.Warning(
                    BotDiagnosticCategory.Reasoning,
                    "REASONING.TRACE_MISSING",
                    "IBotReasoningTrace is not available.",
                    reason:
                        "Detailed algorithmic reasoning from the Player-vs-Bot integration patch cannot be mirrored into Unity Console.");
            }
            else
            {
                _log.Info(
                    BotDiagnosticCategory.Reasoning,
                    "REASONING.TRACE_READY",
                    "Structured BotAI reasoning trace is available and will be mirrored to Console.");
            }
        }

        public void Tick()
        {
            if (_trace == null)
            {
                if (!_missingLogged)
                    Initialize();
                return;
            }

            if (_turns?.Factions == null)
                return;

            for (int i = 0; i < _turns.Factions.Count; i++)
            {
                TurnFaction faction = _turns.Factions[i];
                if (!faction.IsBot ||
                    string.IsNullOrWhiteSpace(faction.OwnerId))
                {
                    continue;
                }

                string owner = faction.OwnerId.Trim();
                _lastSequence.TryGetValue(owner, out long last);

                IReadOnlyList<BotReasoningEntry> entries;
                try
                {
                    entries = _trace.GetEntriesSince(owner, last);
                }
                catch (Exception exception)
                {
                    _log.Exception(
                        BotDiagnosticCategory.Reasoning,
                        "REASONING.READ_EXCEPTION",
                        exception,
                        "Failed to read bot reasoning trace.",
                        owner);
                    continue;
                }

                if (entries == null || entries.Count == 0)
                    continue;

                for (int e = 0; e < entries.Count; e++)
                {
                    BotReasoningEntry entry = entries[e];
                    _lastSequence[owner] = Math.Max(
                        _lastSequence.TryGetValue(owner, out long current) ? current : 0,
                        entry.Sequence);

                    LogEntry(entry);
                }
            }
        }

        private void LogEntry(BotReasoningEntry entry)
        {
            BotDiagnosticCategory category = ResolveCategory(entry.Stage);
            string details =
                $"stage={entry.Stage}; reasoningSeq={entry.Sequence}; score={entry.Score}; " +
                $"targetCell={(entry.TargetCell.HasValue ? entry.TargetCell.Value.ToString() : "none")}; " +
                $"subject='{entry.SubjectId}'";

            if (_settings.LogReasoningFactors &&
                entry.Factors != null &&
                entry.Factors.Count > 0)
            {
                var b = new StringBuilder(details);
                b.Append("; factors=[");
                for (int i = 0; i < entry.Factors.Count; i++)
                {
                    if (i > 0)
                        b.Append(" | ");

                    BotSiteScoreFactor f = entry.Factors[i];
                    b.Append(f.Key)
                        .Append(":raw=").Append(f.RawValue.ToString("0.##"))
                        .Append(",weight=").Append(f.Weight)
                        .Append(",contribution=").Append(f.Contribution)
                        .Append(",detail=").Append(f.Detail);
                }
                b.Append(']');
                details = b.ToString();
            }

            BotDiagnosticLevel level =
                entry.Stage == BotReasoningStage.Warning
                    ? BotDiagnosticLevel.Warning
                    : BotDiagnosticLevel.Info;

            if (level == BotDiagnosticLevel.Warning)
            {
                _log.Warning(
                    category,
                    $"REASONING.{entry.Stage.ToString().ToUpperInvariant()}",
                    entry.Headline,
                    entry.Narrative,
                    details,
                    entry.OwnerId);
            }
            else
            {
                _log.Info(
                    category,
                    $"REASONING.{entry.Stage.ToString().ToUpperInvariant()}",
                    entry.Headline,
                    entry.Narrative,
                    details,
                    entry.OwnerId);
            }
        }

        private static BotDiagnosticCategory ResolveCategory(BotReasoningStage stage)
        {
            return stage switch
            {
                BotReasoningStage.TerrainScan => BotDiagnosticCategory.Perception,
                BotReasoningStage.SiteEvaluation => BotDiagnosticCategory.Construction,
                BotReasoningStage.Strategy => BotDiagnosticCategory.Strategy,
                BotReasoningStage.Goal => BotDiagnosticCategory.Goal,
                BotReasoningStage.Candidate => BotDiagnosticCategory.Candidate,
                BotReasoningStage.Selection => BotDiagnosticCategory.Candidate,
                BotReasoningStage.ActionAttempt => BotDiagnosticCategory.Executor,
                BotReasoningStage.ActionResult => BotDiagnosticCategory.Executor,
                BotReasoningStage.TurnComplete => BotDiagnosticCategory.Handoff,
                BotReasoningStage.Warning => BotDiagnosticCategory.Reasoning,
                _ => BotDiagnosticCategory.Reasoning,
            };
        }
    }
}
