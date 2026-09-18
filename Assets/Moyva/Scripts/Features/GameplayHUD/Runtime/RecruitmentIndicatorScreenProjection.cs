using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static class RecruitmentIndicatorScreenProjection
    {
        public static UnityEngine.Camera ResolveMainCamera(
            ref UnityEngine.Camera cached,
            ref bool warningLogged,
            string missingCameraWarning)
        {
            if (cached != null && cached.isActiveAndEnabled)
                return cached;

            cached = UnityEngine.Camera.main;
            if (cached == null && !warningLogged)
            {
                warningLogged = true;
            }

            return cached;
        }

        public static void UpdatePositions<THandle>(
            IEnumerable<THandle> handles,
            Func<THandle, Vector2Int> getGridPosition,
            Func<THandle, RectTransform> getRoot,
            IGridProjection gridProjection,
            float worldHeightOffset,
            Canvas canvas,
            RectTransform canvasRect,
            UnityEngine.Camera worldCamera)
        {
            UnityEngine.Camera uiCamera =
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera != null
                        ? canvas.worldCamera
                        : worldCamera;

            foreach (THandle handle in handles)
            {
                RectTransform root = getRoot(handle);
                Vector3 world =
                    gridProjection.GridToWorld(
                        getGridPosition(handle))
                    + Vector3.up * worldHeightOffset;
                Vector3 screen =
                    worldCamera.WorldToScreenPoint(world);
                Vector3 viewport =
                    worldCamera.WorldToViewportPoint(world);
                bool visible =
                    screen.z > 0f
                    && viewport.x >= 0f
                    && viewport.x <= 1f
                    && viewport.y >= 0f
                    && viewport.y <= 1f;

                root.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                if (RectTransformUtility
                    .ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        screen,
                        uiCamera,
                        out Vector2 local))
                {
                    root.anchoredPosition = local;
                }
            }
        }
    }
}
