using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static class RecruitmentIndicatorCanvasContainer
    {
        public static bool TryResolve(
            Component hudView,
            string containerName,
            string missingCanvasWarning,
            ref bool warningLogged,
            out Canvas canvas,
            out RectTransform canvasRect,
            out RectTransform container,
            out bool createdContainer)
        {
            canvas = hudView != null
                ? hudView.GetComponentInParent<Canvas>(true)
                : null;
            canvas ??= Object.FindFirstObjectByType<Canvas>(
                FindObjectsInactive.Include);

            if (canvas == null)
            {
                canvasRect = null;
                container = null;
                createdContainer = false;

                if (!warningLogged)
                {
                    warningLogged = true;
                }

                return false;
            }

            canvasRect = canvas.transform as RectTransform;
            Transform existing = canvas.transform.Find(containerName);
            if (existing != null)
            {
                container = existing as RectTransform;
                createdContainer = false;
                return container != null;
            }

            var gameObject = new GameObject(
                containerName,
                typeof(RectTransform));
            container = gameObject.GetComponent<RectTransform>();
            container.SetParent(canvas.transform, false);
            container.anchorMin = Vector2.zero;
            container.anchorMax = Vector2.one;
            container.offsetMin = Vector2.zero;
            container.offsetMax = Vector2.zero;
            container.SetAsLastSibling();
            createdContainer = true;
            return true;
        }
    }
}
