using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.API
{
    public enum BotReasoningStage
    {
        Session = 0,
        Observation = 10,
        TerrainScan = 20,
        SiteEvaluation = 30,
        Strategy = 40,
        Goal = 50,
        Candidate = 60,
        Selection = 70,
        ActionAttempt = 80,
        ActionResult = 90,
        TurnComplete = 100,
        Warning = 110,
    }

    public readonly struct BotReasoningEntry
    {
        public BotReasoningEntry(
            long sequence,
            string ownerId,
            long globalTurn,
            BotReasoningStage stage,
            string headline,
            string narrative,
            int score = 0,
            Vector2Int? targetCell = null,
            string subjectId = null,
            IReadOnlyList<BotSiteScoreFactor> factors = null)
        {
            Sequence = sequence;
            OwnerId = Normalize(ownerId);
            GlobalTurn = globalTurn;
            Stage = stage;
            Headline = Normalize(headline);
            Narrative = Normalize(narrative);
            Score = score;
            TargetCell = targetCell;
            SubjectId = Normalize(subjectId);
            Factors = factors ?? Array.Empty<BotSiteScoreFactor>();
        }

        public long Sequence { get; }
        public string OwnerId { get; }
        public long GlobalTurn { get; }
        public BotReasoningStage Stage { get; }
        public string Headline { get; }
        public string Narrative { get; }
        public int Score { get; }
        public Vector2Int? TargetCell { get; }
        public string SubjectId { get; }
        public IReadOnlyList<BotSiteScoreFactor> Factors { get; }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public interface IBotReasoningTrace
    {
        BotReasoningEntry Record(
            string ownerId,
            long globalTurn,
            BotReasoningStage stage,
            string headline,
            string narrative,
            int score = 0,
            Vector2Int? targetCell = null,
            string subjectId = null,
            IReadOnlyList<BotSiteScoreFactor> factors = null);

        IReadOnlyList<BotReasoningEntry> GetEntries(string ownerId);
        IReadOnlyList<BotReasoningEntry> GetEntriesSince(string ownerId, long sequenceExclusive);
        void Clear(string ownerId);
    }
}
