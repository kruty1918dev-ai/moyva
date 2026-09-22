using System;
using System.Collections.Generic;

namespace Kruty1918.Telemetry.Contracts
{
    /// <summary>
    /// Immutable-after-freeze registry of event contracts. Resolution is by exact
    /// (ContractId, ContractVersion); the registry also tracks the latest version
    /// per id for catalog and compatibility tooling.
    /// </summary>
    public sealed class ContractRegistry
    {
        private readonly Dictionary<string, EventContract> _byKey =
            new Dictionary<string, EventContract>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _latest =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private bool _frozen;

        public int Count => _byKey.Count;

        public void Register(EventContract contract)
        {
            if (_frozen) throw new InvalidOperationException("registry frozen");
            if (contract == null || string.IsNullOrEmpty(contract.ContractId))
                throw new ArgumentException("contract needs ContractId");
            if (contract.Version <= 0) throw new ArgumentException("contract version must be >= 1");
            string key = Key(contract.ContractId, contract.Version);
            if (_byKey.ContainsKey(key))
                throw new InvalidOperationException("duplicate contract " + key);
            _byKey[key] = contract;
            if (!_latest.TryGetValue(contract.ContractId, out int v) || contract.Version > v)
                _latest[contract.ContractId] = contract.Version;
        }

        public void Freeze() => _frozen = true;

        public bool TryResolve(string contractId, int version, out EventContract contract)
            => _byKey.TryGetValue(Key(contractId, version), out contract);

        public EventContract Resolve(string contractId, int version)
            => TryResolve(contractId, version, out var c) ? c : null;

        public int LatestVersion(string contractId)
            => _latest.TryGetValue(contractId, out var v) ? v : 0;

        public IEnumerable<EventContract> All()
        {
            var list = new List<EventContract>(_byKey.Values);
            list.Sort((a, b) => string.CompareOrdinal(a.ContractId, b.ContractId) != 0
                ? string.CompareOrdinal(a.ContractId, b.ContractId)
                : a.Version.CompareTo(b.Version));
            return list;
        }

        /// <summary>
        /// Serialization compatibility: same id, fields added are optional, no removed
        /// or retyped required fields. Semantic compatibility is NOT implied — that is
        /// governed by the dataset fingerprint, not by this check.
        /// </summary>
        public static bool IsSerializationCompatible(EventContract older, EventContract newer)
        {
            foreach (var of in older.Fields)
            {
                var nf = newer.Find(of.Name);
                if (nf == null)
                {
                    if (of.Required) return false; // removed required field breaks old readers' expectations
                    continue;
                }
                if (nf.Type != of.Type) return false;
            }
            foreach (var nf in newer.Fields)
                if (older.Find(nf.Name) == null && nf.Required) return false; // new required field breaks old payloads
            return true;
        }

        private static string Key(string id, int version) => id + "@" + version;
    }
}
