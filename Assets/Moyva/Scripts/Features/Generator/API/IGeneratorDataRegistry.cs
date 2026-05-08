using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IGeneratorDataRegistry
    {
        IReadOnlyCollection<string> Keys { get; }

        void Set(string key, object data);
        bool TryGet(string key, out object data);
        bool TryGet<T>(string key, out T data);
        bool TryGetHillLevelData(string key, out HillLevelDataMap data);
        bool Remove(string key);
        void Clear();
    }
}