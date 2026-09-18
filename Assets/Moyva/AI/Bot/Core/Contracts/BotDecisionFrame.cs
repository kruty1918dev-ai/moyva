using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class BotCandidateAction
    {
        public string Id { get; }
        public BotCapabilityId Capability { get; }
        public BotIntentType Intent { get; }
        public string ActorKey { get; }
        public string TargetKey { get; }
        public int X { get; }
        public int Y { get; }
        public bool Critical { get; }
        // Engine-generated placeholder (e.g. "wait" when no real legal action exists); never a legal move.
        public bool Synthetic { get; }
        public IReadOnlyList<float> Features { get; }
        public BotCandidateAction(string id, BotCapabilityId capability, BotIntentType intent,
            string actor = "", string target = "", int x = 0, int y = 0, float[] features = null, bool critical = false,
            bool synthetic = false)
        {
            if (string.IsNullOrEmpty(id) || !Enum.IsDefined(typeof(BotIntentType), intent)
                || !Enum.IsDefined(typeof(BotCapabilityId), capability))
                throw new ArgumentException("Invalid semantic candidate.");
            Id = id; Capability = capability; Intent = intent; ActorKey = actor; TargetKey = target;
            X = x; Y = y; Critical = critical; Synthetic = synthetic;
            var data = new float[BotDecisionContract.CandidateFeatureCount];
            if (features != null) Array.Copy(features, data, Math.Min(data.Length, features.Length));
            data[0] = 1;
            data[1 + (int)intent] = 1;
            Features = Array.AsReadOnly(data);
        }
    }

    public sealed class BotCandidateSet
    {
        private readonly BotCandidateAction[] _slots = new BotCandidateAction[BotDecisionContract.MaxCandidateSlots];
        public int Count { get; }
        // Candidates produced by real gameplay capabilities; excludes synthetic fallbacks such as "wait".
        public int RealCount { get; }
        public BotCandidateSet(IReadOnlyList<BotCandidateAction> candidates)
        {
            if (candidates.Count > _slots.Length) throw new ArgumentException("Candidate overflow.");
            Count = candidates.Count;
            for (int i = 0; i < Count; i++)
            {
                _slots[i] = candidates[i];
                if (candidates[i] == null || !candidates[i].Synthetic) RealCount++;
            }
        }
        public bool IsLegal(int slot) => slot >= 0 && slot < Count && _slots[slot] != null;
        public BotCandidateAction this[int slot] => IsLegal(slot) ? _slots[slot] : null;
    }

    public readonly struct BotGameStamp
    {
        public readonly string Owner;
        public readonly long Turn;
        public readonly int Phase;
        public readonly bool CanAct;
        public BotGameStamp(string owner, long turn, int phase, bool canAct)
        { Owner = owner; Turn = turn; Phase = phase; CanAct = canAct; }
        public bool Matches(BotGameStamp other) => Owner == other.Owner && Turn == other.Turn && Phase == other.Phase && CanAct == other.CanAct;
    }

    public sealed class BotDecisionFrame
    {
        public string PlayerId { get; }
        public long Sequence { get; }
        public BotGameStamp Stamp { get; }
        public string ContractHash { get; } = BotDecisionContract.Hash;
        public BotCandidateSet Candidates { get; }
        public int RealCandidateCount => Candidates?.RealCount ?? 0;
        public bool HasRealLegalCandidates => RealCandidateCount > 0;
        public bool UsedNoLegalActionFallback => Candidates != null && Candidates.Count > 0 && RealCandidateCount == 0;
        public IReadOnlyList<float> Observations { get; }
        public IReadOnlyDictionary<BotCapabilityId, string> Unavailable { get; }
        public DateTime CreatedUtc { get; } = DateTime.UtcNow;
        public BotDecisionFrame(string player, long sequence, BotGameStamp stamp, BotCandidateSet candidates,
            float[] observations, Dictionary<BotCapabilityId, string> unavailable)
        {
            PlayerId = player; Sequence = sequence; Stamp = stamp; Candidates = candidates;
            Observations = Array.AsReadOnly((float[])observations.Clone());
            Unavailable = new ReadOnlyDictionary<BotCapabilityId, string>(new Dictionary<BotCapabilityId, string>(unavailable));
        }
    }
}
