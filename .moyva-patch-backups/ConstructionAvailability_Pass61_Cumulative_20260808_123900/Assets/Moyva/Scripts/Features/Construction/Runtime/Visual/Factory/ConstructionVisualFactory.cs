using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualFactory :
        IConstructionVisualFactory,
        IConstructionVisualInstanceRecycler
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
            bool isPreviewVisual = false,
            float visualOffsetY = 0f)
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

                return ConfigureInstance(
                    instance,
                    prefab,
                    tile,
                    parent,
                    objectName,
                    minSortingOrder,
                    forcedRotation,
                    isPreviewVisual,
                    visualOffsetY);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА при CreateInstance ({objectName}): {ex.GetType().Name} - {ex.Message}");
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
                return null;
            }
        }

        public GameObject ReuseInstance(
            GameObject instance,
            GameObject prefab,
            Vector2Int tile,
            Transform parent,
            string objectName,
            int minSortingOrder,
            Quaternion? forcedRotation = null,
            bool isPreviewVisual = false,
            float visualOffsetY = 0f)
        {
            if (instance == null)
            {
                return CreateInstance(
                    prefab,
                    tile,
                    parent,
                    objectName,
                    minSortingOrder,
                    forcedRotation,
                    isPreviewVisual,
                    visualOffsetY);
            }

            if (prefab == null || parent == null)
                return null;

            try
            {
                return ConfigureInstance(
                    instance,
                    prefab,
                    tile,
                    parent,
                    objectName,
                    minSortingOrder,
                    forcedRotation,
                    isPreviewVisual,
                    visualOffsetY);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MoyvaConstructionPerf] pooled visual reuse failed " +
                    $"name={objectName} error={ex.GetType().Name}: {ex.Message}");
                UnityEngine.Object.Destroy(instance);
                return CreateInstance(
                    prefab,
                    tile,
                    parent,
                    objectName,
                    minSortingOrder,
                    forcedRotation,
                    isPreviewVisual,
                    visualOffsetY);
            }
        }

        private GameObject ConfigureInstance(
            GameObject instance,
            GameObject prefab,
            Vector2Int tile,
            Transform parent,
            string objectName,
            int minSortingOrder,
            Quaternion? forcedRotation,
            bool isPreviewVisual,
            float visualOffsetY)
        {
            Vector3 worldPos =
                _terrainAlignmentService.ResolveWorldPosition(
                    tile,
                    0.1f);
            Quaternion rotation =
                forcedRotation ?? prefab.transform.rotation;

            Transform transform = instance.transform;
            transform.SetParent(parent, true);
            transform.position = worldPos;
            transform.rotation = rotation;

            instance.name = objectName;
            instance.SetActive(true);
            _styleService.EnsureRenderersEnabled(instance);
            _terrainAlignmentService.AlignInstanceToTerrainSurface(
                instance,
                tile,
                isPreviewVisual,
                visualOffsetY);
            _styleService.EnsureBuildingSortingOrder(
                instance,
                minSortingOrder);
            _styleService.DisableColliders(instance);
            return instance;
        }
    }
}
