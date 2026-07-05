using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildingPointerTarget : MonoBehaviour
    {
        private const float MinimumColliderSize = 0.25f;
        private const float DefaultColliderSize = 1f;

        [SerializeField] private string _buildingId;
        [SerializeField] private Vector2Int _tilePosition;
        [SerializeField] private bool _isPreviewVisual;

        public string BuildingId => _buildingId;
        public Vector2Int TilePosition => _tilePosition;
        public bool IsPreviewVisual => _isPreviewVisual;

        public static ConstructionBuildingPointerTarget AttachOrUpdate(
            GameObject root,
            string buildingId,
            Vector2Int tilePosition,
            bool isPreviewVisual)
        {
            if (root == null)
                return null;

            var target = root.GetComponent<ConstructionBuildingPointerTarget>();
            if (target == null)
                target = root.AddComponent<ConstructionBuildingPointerTarget>();

            target.Sync(buildingId, tilePosition, isPreviewVisual);
            return target;
        }

        public void Sync(string buildingId, Vector2Int tilePosition, bool isPreviewVisual)
        {
            _buildingId = buildingId;
            _tilePosition = tilePosition;
            _isPreviewVisual = isPreviewVisual;
            EnsureInteractionCollider();
        }

        private void EnsureInteractionCollider()
        {
            var collider = GetComponent<BoxCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<BoxCollider>();

            collider.isTrigger = true;
            collider.enabled = true;

            if (TryBuildLocalRendererBounds(out var localBounds))
            {
                collider.center = localBounds.center;
                collider.size = EnsureMinSize(localBounds.size);
                return;
            }

            collider.center = Vector3.zero;
            collider.size = new Vector3(DefaultColliderSize, DefaultColliderSize, DefaultColliderSize);
        }

        private bool TryBuildLocalRendererBounds(out Bounds localBounds)
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                localBounds = default;
                return false;
            }

            bool hasBounds = false;
            localBounds = default;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                    continue;

                Bounds rendererBounds = renderer.bounds;
                Vector3 min = rendererBounds.min;
                Vector3 max = rendererBounds.max;
                Vector3[] worldCorners =
                {
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z),
                };

                for (int cornerIndex = 0; cornerIndex < worldCorners.Length; cornerIndex++)
                {
                    Vector3 localCorner = transform.InverseTransformPoint(worldCorners[cornerIndex]);
                    if (!hasBounds)
                    {
                        localBounds = new Bounds(localCorner, Vector3.zero);
                        hasBounds = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localCorner);
                    }
                }
            }

            return hasBounds;
        }

        private static Vector3 EnsureMinSize(Vector3 size)
        {
            return new Vector3(
                Mathf.Max(MinimumColliderSize, size.x),
                Mathf.Max(MinimumColliderSize, size.y),
                Mathf.Max(MinimumColliderSize, size.z));
        }
    }
}
