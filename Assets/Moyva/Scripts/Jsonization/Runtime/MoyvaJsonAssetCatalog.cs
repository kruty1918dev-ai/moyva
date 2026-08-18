using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Machine-generated build-safe catalog. It stores only Unity object references;
    /// gameplay/configuration values remain in JSON.
    /// </summary>
    public sealed class MoyvaJsonAssetCatalog : MonoBehaviour
    {
        [Serializable]
        public sealed class Entry
        {
            public string Key;
            public UnityEngine.Object Asset;
        }

        [SerializeField] private List<Entry> _entries = new();

        private Dictionary<string, UnityEngine.Object> _byKey;

        public IReadOnlyList<Entry> Entries => _entries;

        public void ReplaceEntries(IEnumerable<Entry> entries)
        {
            _entries = entries != null
                ? new List<Entry>(entries)
                : new List<Entry>();
            _byKey = null;
        }

        public UnityEngine.Object Resolve(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            EnsureIndex();
            return _byKey.TryGetValue(key, out var value) ? value : null;
        }

        private void EnsureIndex()
        {
            if (_byKey != null)
                return;

            _byKey = new Dictionary<string, UnityEngine.Object>(
                StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.Key) ||
                    entry.Asset == null)
                    continue;

                _byKey[entry.Key] = entry.Asset;
            }
        }
    }
}
