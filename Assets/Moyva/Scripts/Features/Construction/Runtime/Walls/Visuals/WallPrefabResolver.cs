using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallPrefabResolver : IWallPrefabResolver
    {
        private const string SlotHorizontal = "horizontal";
        private const string SlotVertical = "vertical";
        private const string SlotCornerNorthEast = "corner_ne";
        private const string SlotCornerNorthWest = "corner_nw";
        private const string SlotCornerSouthEast = "corner_se";
        private const string SlotCornerSouthWest = "corner_sw";

        private static readonly IReadOnlyDictionary<TopologyCaseType, string> WallCaseToSlot =
            new Dictionary<TopologyCaseType, string>
            {
                [TopologyCaseType.Isolated] = SlotHorizontal,
                [TopologyCaseType.CrossIntersection] = SlotHorizontal,
                [TopologyCaseType.TJunctionOpenNorth] = SlotHorizontal,
                [TopologyCaseType.TJunctionOpenSouth] = SlotHorizontal,
                [TopologyCaseType.TJunctionOpenEast] = SlotVertical,
                [TopologyCaseType.TJunctionOpenWest] = SlotVertical,
                [TopologyCaseType.Vertical] = SlotVertical,
                [TopologyCaseType.VerticalLeft] = SlotVertical,
                [TopologyCaseType.VerticalRight] = SlotVertical,
                [TopologyCaseType.Horizontal] = SlotHorizontal,
                [TopologyCaseType.HorizontalTop] = SlotHorizontal,
                [TopologyCaseType.HorizontalBottom] = SlotHorizontal,
                [TopologyCaseType.EndNorth] = SlotVertical,
                [TopologyCaseType.EndSouth] = SlotVertical,
                [TopologyCaseType.EndEast] = SlotHorizontal,
                [TopologyCaseType.EndWest] = SlotHorizontal,
                [TopologyCaseType.CornerNorthEast] = SlotCornerNorthEast,
                [TopologyCaseType.CornerNorthWest] = SlotCornerNorthWest,
                [TopologyCaseType.CornerSouthEast] = SlotCornerSouthEast,
                [TopologyCaseType.CornerSouthWest] = SlotCornerSouthWest,
            };

        private readonly IAutoTileVariantResolver _autoTileVariantResolver;
        private readonly HashSet<string> _missingPrefabWarnings = new();

        [Inject]
        public WallPrefabResolver(IAutoTileVariantResolver autoTileVariantResolver)
        {
            _autoTileVariantResolver = autoTileVariantResolver;
        }

        public GameObject ResolvePrefab(WallCollectionDefinition collection, string buildingId, TopologyNeighborMask mask)
        {
            if (collection == null)
                return null;

            GameObject prefab = TryResolvePreferredPrefab(collection, mask);
            if (prefab != null)
                return prefab;

            prefab = ResolveFallbackWallPrefab(collection);
            LogMissingPrefabWarningOnce(collection, buildingId, mask, prefab);
            return prefab;
        }

        private GameObject TryResolvePreferredPrefab(WallCollectionDefinition collection, TopologyNeighborMask mask)
        {
            if (_autoTileVariantResolver == null
                || !_autoTileVariantResolver.TryResolveId(mask, WallCaseToSlot, out var slotId, out _))
            {
                return null;
            }

            return GetPrefabBySlot(collection, slotId);
        }

        private static GameObject GetPrefabBySlot(WallCollectionDefinition collection, string slotId)
        {
            switch (slotId)
            {
                case SlotHorizontal:
                    return collection.HorizontalPrefab;
                case SlotVertical:
                    return collection.VerticalPrefab;
                case SlotCornerNorthEast:
                    return collection.CornerNorthEastPrefab;
                case SlotCornerNorthWest:
                    return collection.CornerNorthWestPrefab;
                case SlotCornerSouthEast:
                    return collection.CornerSouthEastPrefab;
                case SlotCornerSouthWest:
                    return collection.CornerSouthWestPrefab;
                default:
                    return null;
            }
        }

        private static GameObject ResolveFallbackWallPrefab(WallCollectionDefinition collection)
        {
            if (collection.HorizontalPrefab != null) return collection.HorizontalPrefab;
            if (collection.VerticalPrefab != null) return collection.VerticalPrefab;
            if (collection.CornerNorthEastPrefab != null) return collection.CornerNorthEastPrefab;
            if (collection.CornerNorthWestPrefab != null) return collection.CornerNorthWestPrefab;
            if (collection.CornerSouthEastPrefab != null) return collection.CornerSouthEastPrefab;
            if (collection.CornerSouthWestPrefab != null) return collection.CornerSouthWestPrefab;
            return null;
        }

        private void LogMissingPrefabWarningOnce(
            WallCollectionDefinition collection,
            string buildingId,
            TopologyNeighborMask mask,
            GameObject fallback)
        {
            string collectionId = string.IsNullOrWhiteSpace(collection.CollectionId)
                ? (string.IsNullOrWhiteSpace(buildingId) ? "default-wall" : buildingId.Trim())
                : collection.CollectionId.Trim();
            string warningKey = $"{collectionId}:{(mask.North ? 1 : 0)}{(mask.East ? 1 : 0)}{(mask.South ? 1 : 0)}{(mask.West ? 1 : 0)}";
            if (!_missingPrefabWarnings.Add(warningKey))
                return;

            Debug.LogWarning(
                $"[WallPlacement] Відсутній prefab для кейсу '{warningKey}'. " +
                $"Використано fallback '{(fallback != null ? fallback.name : "NULL")}'.");
        }
    }
}
