using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionPreviewVisualService
    {
        GameObject Show(BuildingPreviewChangedSignal signal, BuildingDefinition def);
        bool TryGet(Vector2Int position, out GameObject visual);
        bool Has(Vector2Int position);
        void ReplaceWallPreview(Vector2Int position, string buildingId, GameObject prefab, float visualOffsetY = 0f);
        void Remove(Vector2Int position);
        void Clear();
    }
}
