using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualFactory : IConstructionVisualFactory
    {
        private readonly IConstructionVisualStyleService _styleService;
        private readonly IConstructionTerrainAlignmentService _terrainAlignmentService;

        public ConstructionVisualFactory(
            IConstructionVisualStyleService styleService,
            IConstructionTerrainAlignmentService terrainAlignmentService)
        {
            _styleService = styleService;
            _terrainAlignmentService = terrainAlignmentService;
        }

        public GameObject CreateInstance(
            GameObject prefab,
            Vector2Int tile,
            Transform parent,
            string objectName,
            int minSortingOrder,
            Quaternion? forcedRotation = null,
            bool isPreviewVisual = false)
        {
            if (prefab == null)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА: prefab == null при створенні {objectName}");
                return null;
            }

            if (parent == null)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА: parent Transform == null при створенні {objectName}");
                return null;
            }

            Vector3 worldPos = _terrainAlignmentService.ResolveWorldPosition(tile, 0.1f);
            Quaternion rotation = forcedRotation ?? prefab.transform.rotation;

            GameObject instance = null;
            try
            {
                instance = UnityEngine.Object.Instantiate(prefab, worldPos, rotation, parent);
                if (instance == null)
                {
                    Debug.LogError($"[ConstructionVisual] ПОМИЛКА: Instantiate повернув null для {objectName}");
                    return null;
                }

                instance.name = objectName;
                instance.SetActive(true);
                _styleService.EnsureRenderersEnabled(instance);
                _terrainAlignmentService.AlignInstanceToTerrainSurface(instance, tile, isPreviewVisual);
                _styleService.EnsureBuildingSortingOrder(instance, minSortingOrder);
                _styleService.DisableColliders(instance);
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА при CreateInstance ({objectName}): {ex.GetType().Name} - {ex.Message}");
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
                return null;
            }
        }
    }
}
