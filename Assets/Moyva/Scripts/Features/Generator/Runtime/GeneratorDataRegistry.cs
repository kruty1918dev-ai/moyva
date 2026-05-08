using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class GeneratorDataRegistry : IGeneratorDataRegistry
    {
        private readonly Dictionary<string, object> _data = new(StringComparer.Ordinal);

        public IReadOnlyCollection<string> Keys => _data.Keys;

        public void Set(string key, object data)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Generator data key cannot be empty.", nameof(key));

            _data[key.Trim()] = data;
        }

        public bool TryGet(string key, out object data)
        {
            data = null;
            return !string.IsNullOrWhiteSpace(key) && _data.TryGetValue(key.Trim(), out data);
        }

        public bool TryGet<T>(string key, out T data)
        {
            if (TryGet(key, out var raw) && raw is T typed)
            {
                data = typed;
                return true;
            }

            data = default;
            return false;
        }

        public bool TryGetHillLevelData(string key, out HillLevelDataMap data)
            => TryGet(key, out data);

        public bool Remove(string key)
            => !string.IsNullOrWhiteSpace(key) && _data.Remove(key.Trim());

        public void Clear() => _data.Clear();
    }
}