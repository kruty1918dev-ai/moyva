using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionPreviewVisualService
    {
        GameObject Show(BuildingPreviewChangedSignal signal, BuildingDefinition def);
        bool TryGet(Vector2Int position, out GameObject visual);
        bool TryMove(Vector2Int fromPosition, Vector2Int toPosition, string buildingId, float visualOffsetY = 0f, EntityPresentationConfig presentation = null, Quaternion? baseRotation = null);
        void MoveDragVisual(Vector2Int position, string buildingId, Vector3 worldPosition, bool snapToGrid, bool hasSnapTarget, Vector2Int snapTargetPosition, bool isSnapTargetValid, float visualOffsetY = 0f, EntityPresentationConfig presentation = null, Quaternion? baseRotation = null);
        void ShowGridHover(BuildGridHoverChangedSignal signal);
        void ClearGridHover();
        bool TryRelease(Vector2Int position, out GameObject visual);
        bool Has(Vector2Int position);
        void ReplaceWallPreview(Vector2Int position, string buildingId, GameObject prefab, float visualOffsetY = 0f, EntityPresentationConfig presentation = null);
        void Remove(Vector2Int position);
        void Clear();
        void Dispose();
    }
}
