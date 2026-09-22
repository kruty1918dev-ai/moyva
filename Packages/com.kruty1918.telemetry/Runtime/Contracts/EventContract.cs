using System;
using System.Collections.Generic;

namespace Kruty1918.Telemetry.Contracts
{
    /// <summary>Batch priority / retention tier used by storage pressure policy.</summary>
    public enum TelemetryPriority
    {
        /// <summary>Replay evidence, crash markers, integrity data. Never auto-evicted silently.</summary>
        Critical = 0,
        /// <summary>High-value gameplay decisions, ML state-action-result records.</summary>
        High = 1,
        /// <summary>Normal gameplay/UI events.</summary>
        Normal = 2,
        /// <summary>Performance/debug samples — first to be dropped under pressure.</summary>
        Low = 3,
    }

    /// <summary>
    /// Versioned, machine-checkable data contract for one event type.
    /// First-class entity shared by client validation, catalog generation,
    /// fingerprint computation and backend verification.
    /// </summary>
    public sealed class EventContract
    {
        public string ContractId;
        public int Version;
        public string EventType;
        public string Description;
        /// <summary>Owning system/producer, e.g. "construction", "ai.bot".</summary>
        public string Producer;
        public TelemetryPriority Priority = TelemetryPriority.Normal;
        public PrivacyClass Privacy = PrivacyClass.Gameplay;
        /// <summary>Retention hint category, e.g. "raw-90d", "training-indefinite".</summary>
        public string Retention = "raw-90d";
        /// <summary>Sampling rate 0..1 applied before buffering (1 = keep all).</summary>
        public float SamplingRate = 1f;
        /// <summary>Free-form normalization rule id applied to values before write.</summary>
        public string NormalizationId;
        /// <summary>Optional semantic/sequence rule ids resolved at validation time.</summary>
        public string[] RuleIds = Array.Empty<string>();
        public readonly List<ContractField> Fields = new List<ContractField>();

        private Dictionary<string, ContractField> _byName;

        public EventContract() { }

        public EventContract(string contractId, int version, string eventType, string producer)
        {
            ContractId = contractId; Version = version; EventType = eventType; Producer = producer;
        }

        /// <summary>
        /// Exact match, or family wildcard: contract EventType "moyva.match.*" governs
        /// every type under the "moyva.match." prefix. Wildcards keep related lifecycle
        /// event types under one versioned contract; field rules still apply to all.
        /// </summary>
        public bool MatchesEventType(string eventType)
        {
            if (EventType == eventType) return true;
            if (EventType != null && EventType.EndsWith(".*", StringComparison.Ordinal))
                return eventType != null && eventType.StartsWith(
                    EventType.Substring(0, EventType.Length - 1), StringComparison.Ordinal);
            return false;
        }

        public EventContract Add(ContractField f) { Fields.Add(f); _byName = null; return this; }

        public ContractField Find(string name)
        {
            if (_byName == null)
            {
                _byName = new Dictionary<string, ContractField>(StringComparer.Ordinal);
                foreach (var field in Fields) _byName[field.Name] = field;
            }
            return _byName.TryGetValue(name, out var f) ? f : null;
        }

        /// <summary>All required fields present check happens during write; this lists names for catalogs.</summary>
        public IEnumerable<string> RequiredFieldNames()
        {
            foreach (var f in Fields) if (f.Required) yield return f.Name;
        }
    }
}
