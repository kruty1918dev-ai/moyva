using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionVisualFactory
    {
        GameObject CreateInstance(
            GameObject prefab,
            Vector2Int tile,
            Transform parent,
            string objectName,
            int minSortingOrder,
            Quaternion? forcedRotation = null,
            bool isPreviewVisual = false,
            float visualOffsetY = 0f);
    }
}
