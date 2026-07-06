using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPreviewVisualService : IConstructionPreviewVisualService
    {
        private readonly Dictionary<Vector2Int, GameObject> _previewByPosition = new();
        private readonly IConstructionVisualRootService _roots;
        private readonly IConstructionVisualFactory _visualFactory;
        private readonly IConstructionVisualStyleService _styleService;
        private readonly IWallVisualResolver _wallVisualResolver;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;

        [Inject]
        public ConstructionPreviewVisualService(
            IConstructionVisualRootService roots,
            IConstructionVisualFactory visualFactory,
            IConstructionVisualStyleService styleService,
            IWallVisualResolver wallVisualResolver,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _visualFactory = visualFactory;
            _styleService = styleService;
            _wallVisualResolver = wallVisualResolver;
            _settingsProvider = settingsProvider;
        }

        public GameObject Show(BuildingPreviewChangedSignal signal, BuildingDefinition def)
        {
            if (TryGetReusablePreview(signal, out GameObject existing))
            {
                _styleService.ApplyGhostStyle(existing, true);
                return existing;
            }

            Remove(signal.Position);
            GameObject prefab = ResolvePrefab(signal.Position, signal.BuildingId, def.Prefab);
            GameObject instance = CreatePreview(prefab, signal.Position, signal.BuildingId, def.VisualYOffset);
            if (instance == null)
                return null;

            _previewByPosition[signal.Position] = instance;
            _styleService.ApplyGhostStyle(instance, true);
            return instance;
        }

        public bool TryGet(Vector2Int position, out GameObject visual)
        {
            if (_previewByPosition.TryGetValue(position, out visual) && visual != null)
                return true;

            visual = null;
            return false;
        }

        public bool Has(Vector2Int position) => TryGet(position, out _);

        public void ReplaceWallPreview(Vector2Int position, string buildingId, GameObject prefab, float visualOffsetY = 0f)
        {
            if (!Has(position))
                return;

            Remove(position);
            GameObject instance = CreatePreview(prefab, position, buildingId, visualOffsetY);
            if (instance == null)
                return;

            _previewByPosition[position] = instance;
            _styleService.ApplyGhostStyle(instance, true);
        }

        public void Remove(Vector2Int position)
        {
            if (!_previewByPosition.TryGetValue(position, out GameObject instance))
                return;

            if (instance != null)
                Object.Destroy(instance);

            _previewByPosition.Remove(position);
        }

        public void Clear()
        {
            foreach (KeyValuePair<Vector2Int, GameObject> pair in _previewByPosition)
            {
                if (pair.Value != null)
                    Object.Destroy(pair.Value);
            }

            _previewByPosition.Clear();
        }

        private bool TryGetReusablePreview(BuildingPreviewChangedSignal signal, out GameObject existing)
        {
            return _previewByPosition.TryGetValue(signal.Position, out existing)
                && existing != null
                && existing.name.Contains(signal.BuildingId);
        }

        private GameObject ResolvePrefab(Vector2Int position, string buildingId, GameObject defaultPrefab)
        {
            return _wallVisualResolver.TryResolvePreviewVisual(position, buildingId, out GameObject wallPrefab)
                ? wallPrefab
                : defaultPrefab;
        }

        private GameObject CreatePreview(GameObject prefab, Vector2Int position, string buildingId, float visualOffsetY)
        {
            string prefabTag = prefab != null ? prefab.name : "NULL";
            GameObject instance = _visualFactory.CreateInstance(
                prefab,
                position,
                _roots.PreviewRoot,
                $"Preview_{buildingId}_{prefabTag}_{position.x}_{position.y}",
                ResolveSortingOrder(),
                isPreviewVisual: true,
                visualOffsetY: visualOffsetY);
            if (instance != null)
                ConstructionBuildingPointerTarget.AttachOrUpdate(instance, buildingId, position, isPreviewVisual: true);

            return instance;
        }

        private int ResolveSortingOrder()
            => _settingsProvider?.BuildingLayerMinSortingOrder ?? 5;
    }
}
