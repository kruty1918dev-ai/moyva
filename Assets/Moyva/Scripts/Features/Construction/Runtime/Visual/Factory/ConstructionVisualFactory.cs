using System;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualFactory : IConstructionVisualInstanceRecycler {
        private readonly ConstructionVisualStyleService _styleService;
        private readonly ConstructionTerrainAlignmentService _terrainAlignmentService;

        public ConstructionVisualFactory(
            ConstructionVisualStyleService styleService,
            ConstructionTerrainAlignmentService terrainAlignmentService)
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
            float visualOffsetY = 0f,
            EntityPresentationConfig presentation = null)
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
                    visualOffsetY,
                    presentation);
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
            float visualOffsetY = 0f,
            EntityPresentationConfig presentation = null)
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
                    visualOffsetY,
                    presentation);
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
                    visualOffsetY,
                    presentation);
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
                    visualOffsetY,
                    presentation);
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
            float visualOffsetY,
            EntityPresentationConfig presentation)
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
            transform.localScale = EntityPresentationApplier.ResolveScale(
                prefab != null ? prefab.transform.localScale : Vector3.one,
                presentation);
            transform.rotation = EntityPresentationApplier.ResolveRotation(
                rotation,
                presentation);

            instance.name = objectName;
            instance.SetActive(true);
            _styleService.EnsureRenderersEnabled(instance);
            _terrainAlignmentService.AlignInstanceToTerrainSurface(
                instance,
                tile,
                isPreviewVisual,
                presentation != null
                    ? presentation.ResolveGroundOffsetY(visualOffsetY)
                    : visualOffsetY);
            EntityPresentationApplier.ApplyPositionOffset(
                instance,
                presentation,
                transform.position,
                rotation);
            _styleService.EnsureBuildingSortingOrder(
                instance,
                minSortingOrder);
            _styleService.DisableColliders(instance);
            EntityPresentationApplier.ApplyStyleAndShadows(
                instance,
                presentation);
            return instance;
        }
    }
}
