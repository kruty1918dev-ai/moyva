using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapObjectVisualRegistryService
    {
        void Clear();
        void Register(string objectId, Vector2Int position, GameObject visual);
        bool TryGetVisual(string objectId, Vector2Int position, out GameObject visual);
    }
}
