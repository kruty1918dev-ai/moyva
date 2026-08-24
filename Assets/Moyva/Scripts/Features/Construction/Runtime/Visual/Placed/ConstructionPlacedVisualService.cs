using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPlacedVisualService :
        IConstructionPlacedVisualLookup
    {
        private const float PlacedSnapSharpness = 12f;

        private readonly Dictionary<Vector2Int, GameObject> _placedByPosition = new();
        private readonly Dictionary<Vector2Int, EntityPresentationConfig> _presentationByPosition = new();
        private readonly Dictionary<Vector2Int, string> _buildingIdByPosition = new();
        private readonly Dictionary<Vector2Int, Quaternion> _baseRotationByPosition = new();
        private readonly HashSet<Vector2Int> _demolitionPreviewPositions = new();
        private readonly HashSet<Vector2Int> _underConstructionPositions = new();
        private readonly SpriteSelectionHighlighter _selectionHighlighter = new();
        private readonly ConstructionVisualRootService _roots;
        private readonly ConstructionVisualFactory _visualFactory;
        private readonly ConstructionVisualStyleService _styleService;
        private readonly ConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;

        private Vector2Int? _selectedPosition;

        [Inject]
        public ConstructionPlacedVisualService(
            ConstructionVisualRootService roots,
            ConstructionVisualFactory visualFactory,
            ConstructionVisualStyleService styleService,
            [InjectOptional] ConstructionTerrainAlignmentService terrainAlignment = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _roots = roots;
            _visualFactory = visualFactory;
            _styleService = styleService;
            _terrainAlignment = terrainAlignment;
            _settingsProvider = settingsProvider;
        }

        public void Replace(
            Vector2Int position,
            string buildingId,
            GameObject prefab,
            Quaternion rotation,
            float visualOffsetY = 0f,
            GameObject sourceVisual = null,
            EntityPresentationConfig presentation = null)
        {
            Remove(position);
            string objectName = $"Building_{buildingId}_{position.x}_{position.y}";
            GameObject instance = sourceVisual != null
                ? PrepareSourceVisual(
                    sourceVisual,
                    prefab,
                    position,
                    objectName,
                    rotation,
                    visualOffsetY,
                    presentation)
                : _visualFactory.CreateInstance(
                    prefab,
                    position,
                    _roots.PlacedRoot,
                    objectName,
                    ResolveSortingOrder(),
                    rotation,
                    visualOffsetY: visualOffsetY,
                    presentation: presentation);
            if (instance == null)
                return;

            ConstructionBuildingPointerTarget.AttachOrUpdate(instance, buildingId, position, isPreviewVisual: false);
            _styleService.ApplySolidStyle(instance);
            EntityPresentationApplier.ApplyStyleAndShadows(instance, presentation);
            _placedByPosition[position] = instance;
            StorePresentation(position, presentation);
            _buildingIdByPosition[position] = buildingId;
            _baseRotationByPosition[position] = rotation;
            _demolitionPreviewPositions.Remove(position);
            _underConstructionPositions.Remove(position);

            if (_selectedPosition.HasValue && _selectedPosition.Value == position)
                _selectionHighlighter.Apply(instance);
        }

        public void ReplaceWithStoredPose(
            Vector2Int position,
            GameObject prefab,
            float visualOffsetY = 0f,
            EntityPresentationConfig presentation = null)
        {
            if (prefab == null || !_placedByPosition.ContainsKey(position))
                return;

            string buildingId = _buildingIdByPosition.TryGetValue(position, out string storedBuildingId)
                ? storedBuildingId
                : string.Empty;
            Quaternion rotation = _baseRotationByPosition.TryGetValue(position, out Quaternion storedRotation)
                ? storedRotation
                : Quaternion.identity;

            Replace(
                position,
                buildingId,
                prefab,
                rotation,
                visualOffsetY,
                presentation: presentation);
        }

        public void Remove(Vector2Int position)
        {
            if (!_placedByPosition.TryGetValue(position, out GameObject instance))
                return;

            if (instance != null)
                Object.Destroy(instance);

            _placedByPosition.Remove(position);
            _presentationByPosition.Remove(position);
            _buildingIdByPosition.Remove(position);
            _baseRotationByPosition.Remove(position);
            _demolitionPreviewPositions.Remove(position);
            _underConstructionPositions.Remove(position);
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

        public void MarkUnderConstruction(Vector2Int position)
        {
            if (!_placedByPosition.TryGetValue(position, out GameObject instance) || instance == null)
                return;

            _underConstructionPositions.Add(position);
            if (!_demolitionPreviewPositions.Contains(position))
            {
                _styleService.ApplyUnderConstructionStyle(instance);
                ApplyStoredPresentationStyle(position, instance);
            }
        }

        public void MarkOperational(Vector2Int position)
        {
            _underConstructionPositions.Remove(position);

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
            {
                _styleService.ApplySolidStyle(instance);
                ApplyStoredPresentationStyle(position, instance);
            }
        }

        public void RestoreDemolitionPreview(Vector2Int position)
        {
            _demolitionPreviewPositions.Remove(position);

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                ApplyPersistentStyle(position, instance);
        }

        public void ClearDemolitionPreviewStyles()
        {
            foreach (Vector2Int position in _demolitionPreviewPositions)
            {
                if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                    ApplyPersistentStyle(position, instance);
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
            _presentationByPosition.Clear();
            _buildingIdByPosition.Clear();
            _baseRotationByPosition.Clear();
            _demolitionPreviewPositions.Clear();
            _underConstructionPositions.Clear();
            ClearSelection();
        }

        public bool TryGetPlacedVisual(Vector2Int position, out GameObject visual)
        {
            if (_placedByPosition.TryGetValue(position, out visual) && visual != null)
                return true;

            visual = null;
            return false;
        }

        private GameObject PrepareSourceVisual(
            GameObject sourceVisual,
            GameObject prefab,
            Vector2Int position,
            string objectName,
            Quaternion rotation,
            float visualOffsetY,
            EntityPresentationConfig presentation)
        {
            if (sourceVisual == null)
                return null;

            sourceVisual.name = objectName;
            sourceVisual.transform.SetParent(_roots.PlacedRoot, worldPositionStays: true);
            sourceVisual.transform.localScale = EntityPresentationApplier.ResolveScale(
                prefab != null ? prefab.transform.localScale : sourceVisual.transform.localScale,
                presentation);
            sourceVisual.transform.rotation = EntityPresentationApplier.ResolveRotation(
                rotation,
                presentation);
            _styleService.EnsureRenderersEnabled(sourceVisual);
            _styleService.EnsureBuildingSortingOrder(sourceVisual, ResolveSortingOrder());
            _styleService.DisableColliders(sourceVisual);

            Vector3 targetPosition = _terrainAlignment != null
                ? _terrainAlignment.ResolveAlignedInstancePosition(
                    sourceVisual,
                    position,
                    isPreviewVisual: false,
                    presentation != null
                        ? presentation.ResolveGroundOffsetY(visualOffsetY)
                        : visualOffsetY)
                : sourceVisual.transform.position;
            targetPosition = EntityPresentationApplier.ResolvePositionOffset(
                targetPosition,
                rotation,
                presentation);

            var motion = ConstructionSmoothVisualMotion.AttachOrUpdate(sourceVisual);
            if (motion != null)
                motion.MoveTo(targetPosition, PlacedSnapSharpness);
            else
                sourceVisual.transform.position = targetPosition;

            return sourceVisual;
        }

        private int ResolveSortingOrder()
            => _settingsProvider?.BuildingLayerMinSortingOrder ?? 5;

        private void ApplyPersistentStyle(Vector2Int position, GameObject instance)
        {
            if (_underConstructionPositions.Contains(position))
                _styleService.ApplyUnderConstructionStyle(instance);
            else
                _styleService.ApplySolidStyle(instance);

            ApplyStoredPresentationStyle(position, instance);
        }

        private void StorePresentation(
            Vector2Int position,
            EntityPresentationConfig presentation)
        {
            if (presentation == null)
                _presentationByPosition.Remove(position);
            else
                _presentationByPosition[position] = presentation;
        }

        private void ApplyStoredPresentationStyle(
            Vector2Int position,
            GameObject instance)
        {
            if (_presentationByPosition.TryGetValue(
                    position,
                    out EntityPresentationConfig presentation))
            {
                EntityPresentationApplier.ApplyStyleAndShadows(
                    instance,
                    presentation);
            }
        }
    }
}
