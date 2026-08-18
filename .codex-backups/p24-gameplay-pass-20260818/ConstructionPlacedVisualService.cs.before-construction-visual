using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPlacedVisualService : IConstructionPlacedVisualService
    {
        private const float PlacedSnapSharpness = 12f;

        private readonly Dictionary<Vector2Int, GameObject> _placedByPosition = new();
        private readonly HashSet<Vector2Int> _demolitionPreviewPositions = new();
        private readonly SpriteSelectionHighlighter _selectionHighlighter = new();
        private readonly IConstructionVisualRootService _roots;
        private readonly IConstructionVisualFactory _visualFactory;
        private readonly IConstructionVisualStyleService _styleService;
        private readonly IConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;

        private Vector2Int? _selectedPosition;

        [Inject]
        public ConstructionPlacedVisualService(
            IConstructionVisualRootService roots,
            IConstructionVisualFactory visualFactory,
            IConstructionVisualStyleService styleService,
            [InjectOptional] IConstructionTerrainAlignmentService terrainAlignment = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _visualFactory = visualFactory;
            _styleService = styleService;
            _terrainAlignment = terrainAlignment;
            _settingsProvider = settingsProvider;
        }

        public void Replace(Vector2Int position, string buildingId, GameObject prefab, Quaternion rotation, float visualOffsetY = 0f, GameObject sourceVisual = null)
        {
            Remove(position);
            string objectName = $"Building_{buildingId}_{position.x}_{position.y}";
            GameObject instance = sourceVisual != null
                ? PrepareSourceVisual(sourceVisual, position, objectName, rotation, visualOffsetY)
                : _visualFactory.CreateInstance(
                    prefab,
                    position,
                    _roots.PlacedRoot,
                    objectName,
                    ResolveSortingOrder(),
                    rotation,
                    visualOffsetY: visualOffsetY);
            if (instance == null)
                return;

            ConstructionBuildingPointerTarget.AttachOrUpdate(instance, buildingId, position, isPreviewVisual: false);
            _styleService.ApplySolidStyle(instance);
            _placedByPosition[position] = instance;
            _demolitionPreviewPositions.Remove(position);

            if (_selectedPosition.HasValue && _selectedPosition.Value == position)
                _selectionHighlighter.Apply(instance);
        }

        public void Remove(Vector2Int position)
        {
            if (!_placedByPosition.TryGetValue(position, out GameObject instance))
                return;

            if (instance != null)
                Object.Destroy(instance);

            _placedByPosition.Remove(position);
            _demolitionPreviewPositions.Remove(position);
        }

        public void Select(Vector2Int position)
        {
            _selectedPosition = position;
            _selectionHighlighter.Clear();

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                _selectionHighlighter.Apply(instance);
        }

        public void ClearSelection()
        {
            _selectedPosition = null;
            _selectionHighlighter.Clear();
        }

        public bool ClearSelectionIfMatches(Vector2Int position)
        {
            if (!_selectedPosition.HasValue || _selectedPosition.Value != position)
                return false;

            ClearSelection();
            return true;
        }

        public void MarkDemolitionPreview(Vector2Int position)
        {
            if (!_placedByPosition.TryGetValue(position, out GameObject instance) || instance == null)
                return;

            _demolitionPreviewPositions.Add(position);
            _styleService.ApplyGhostStyle(instance, false);
        }

        public void RestoreDemolitionPreview(Vector2Int position)
        {
            _demolitionPreviewPositions.Remove(position);

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                _styleService.ApplySolidStyle(instance);
        }

        public void ClearDemolitionPreviewStyles()
        {
            foreach (Vector2Int position in _demolitionPreviewPositions)
            {
                if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                    _styleService.ApplySolidStyle(instance);
            }

            _demolitionPreviewPositions.Clear();
        }

        public void Clear()
        {
            foreach (KeyValuePair<Vector2Int, GameObject> pair in _placedByPosition)
            {
                if (pair.Value != null)
                    Object.Destroy(pair.Value);
            }

            _placedByPosition.Clear();
            _demolitionPreviewPositions.Clear();
            ClearSelection();
        }

        public bool TryGetPlacedVisual(Vector2Int position, out GameObject visual)
        {
            if (_placedByPosition.TryGetValue(position, out visual) && visual != null)
                return true;

            visual = null;
            return false;
        }

        private GameObject PrepareSourceVisual(GameObject sourceVisual, Vector2Int position, string objectName, Quaternion rotation, float visualOffsetY)
        {
            if (sourceVisual == null)
                return null;

            sourceVisual.name = objectName;
            sourceVisual.transform.SetParent(_roots.PlacedRoot, worldPositionStays: true);
            sourceVisual.transform.rotation = rotation;
            _styleService.EnsureRenderersEnabled(sourceVisual);
            _styleService.EnsureBuildingSortingOrder(sourceVisual, ResolveSortingOrder());
            _styleService.DisableColliders(sourceVisual);

            Vector3 targetPosition = _terrainAlignment != null
                ? _terrainAlignment.ResolveAlignedInstancePosition(sourceVisual, position, isPreviewVisual: false, visualOffsetY)
                : sourceVisual.transform.position;

            var motion = ConstructionSmoothVisualMotion.AttachOrUpdate(sourceVisual);
            if (motion != null)
                motion.MoveTo(targetPosition, PlacedSnapSharpness);
            else
                sourceVisual.transform.position = targetPosition;

            return sourceVisual;
        }

        private int ResolveSortingOrder()
            => _settingsProvider?.BuildingLayerMinSortingOrder ?? 5;
    }
}
