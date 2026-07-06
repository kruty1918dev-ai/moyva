using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionPlacedVisualService : IConstructionPlacedVisualLookup
    {
        void Replace(Vector2Int position, string buildingId, GameObject prefab, Quaternion rotation, float visualOffsetY = 0f);
        void Remove(Vector2Int position);
        void Select(Vector2Int position);
        void ClearSelection();
        bool ClearSelectionIfMatches(Vector2Int position);
        void MarkDemolitionPreview(Vector2Int position);
        void RestoreDemolitionPreview(Vector2Int position);
        void ClearDemolitionPreviewStyles();
        void Clear();
    }
}
