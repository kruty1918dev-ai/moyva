using System;
using System.Collections.Generic;
using Kruty1918.Telemetry.Canonicalization;
using Kruty1918.Telemetry.Contracts;

namespace Kruty1918.Telemetry.Fingerprinting
{
    /// <summary>
    /// Formal semantic fingerprint input model. Everything that can change the VALUES
    /// or MEANING of a dataset belongs here; incidental metadata (build timestamps,
    /// upload times, machine paths, object keys, endpoint state) must NOT be added.
    ///
    /// Sections:
    ///  - contracts: id/version/eventType + per-field (name,type,unit,semantics,required,
    ///    sampling, normalization, privacy) for fields flagged FingerprintInclude.
    ///  - semantics: product/gameplay semantic parameters (rules, formulas, units,
    ///    map/ruleset/feature definitions, AI policy/state representation ids).
    ///  - pipeline: validator/feature-extractor/transform versions that change
    ///    dataset interpretation.
    /// </summary>
    public sealed class FingerprintInput
    {
        public const int ModelVersion = 1;

        /// <summary>Contract descriptors contributing to the fingerprint.</summary>
        public readonly List<EventContract> Contracts = new List<EventContract>();

        /// <summary>Semantic parameters: key → canonical value. Sorted at serialization.</summary>
        public readonly SortedDictionary<string, CanonicalValue> Semantics =
            new SortedDictionary<string, CanonicalValue>(StringComparer.Ordinal);

        /// <summary>Pipeline/tooling versions: key → version string/int.</summary>
        public readonly SortedDictionary<string, CanonicalValue> Pipeline =
            new SortedDictionary<string, CanonicalValue>(StringComparer.Ordinal);

        public FingerprintInput WithContract(EventContract c) { Contracts.Add(c); return this; }

        public FingerprintInput WithSemantic(string key, string value)
        { Semantics[key] = CanonicalValue.Of(value); return this; }
        public FingerprintInput WithSemantic(string key, long value)
        { Semantics[key] = CanonicalValue.Of(value); return this; }
        public FingerprintInput WithSemantic(string key, double value)
        { Semantics[key] = CanonicalValue.Of(value); return this; }
        public FingerprintInput WithSemantic(string key, CanonicalValue value)
        { Semantics[key] = value; return this; }

        public FingerprintInput WithPipeline(string key, string version)
        { Pipeline[key] = CanonicalValue.Of(version); return this; }

        /// <summary>Project to the normalized canonical object graph.</summary>
        public CanonicalValue ToCanonical()
        {
            var contractList = new List<CanonicalValue>(Contracts.Count);
            foreach (var c in Contracts)
                contractList.Add(ContractToCanonical(c));
            // Contract order is not semantic: sort canonically so registration order doesn't matter.
            var contracts = CanonicalValue.SortedSet(contractList);

            var semantics = new List<KeyValuePair<string, CanonicalValue>>(Semantics.Count);
            foreach (var kv in Semantics) semantics.Add(CanonicalValue.F(kv.Key, kv.Value));

            var pipeline = new List<KeyValuePair<string, CanonicalValue>>(Pipeline.Count);
            foreach (var kv in Pipeline) pipeline.Add(CanonicalValue.F(kv.Key, kv.Value));

            return CanonicalValue.Map(
                CanonicalValue.F("contracts", contracts),
                CanonicalValue.F("model", CanonicalValue.Of(ModelVersion)),
                CanonicalValue.F("pipeline", CanonicalValue.Of(pipeline)),
                CanonicalValue.F("semantics", CanonicalValue.Of(semantics)));
        }

        private static CanonicalValue ContractToCanonical(EventContract c)
        {
            var fields = new List<CanonicalValue>(c.Fields.Count);
            foreach (var f in c.Fields)
            {
                if (!f.FingerprintInclude) continue;
                fields.Add(CanonicalValue.Map(
                    CanonicalValue.F("name", CanonicalValue.Of(f.Name)),
                    CanonicalValue.F("req", CanonicalValue.Of(f.Required)),
                    CanonicalValue.F("sem", CanonicalValue.Of(f.Semantics)),
                    CanonicalValue.F("type", CanonicalValue.Of(f.Type.ToString())),
                    CanonicalValue.F("unit", CanonicalValue.Of(f.Unit))));
            }
            return CanonicalValue.Map(
                CanonicalValue.F("fields", CanonicalValue.SortedSet(fields)),
                CanonicalValue.F("id", CanonicalValue.Of(c.ContractId)),
                CanonicalValue.F("norm", CanonicalValue.Of(c.NormalizationId)),
                CanonicalValue.F("prio", CanonicalValue.Of((int)c.Priority)),
                CanonicalValue.F("sampling", CanonicalValue.Of((double)c.SamplingRate)),
                CanonicalValue.F("type", CanonicalValue.Of(c.EventType)),
                CanonicalValue.F("ver", CanonicalValue.Of(c.Version)));
        }
    }
}
