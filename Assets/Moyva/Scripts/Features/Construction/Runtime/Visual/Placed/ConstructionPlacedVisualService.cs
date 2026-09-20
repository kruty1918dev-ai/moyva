using System.Collections.Generic;
using Kruty1918.Moyva.Animations.API;
using Kruty1918.Moyva.Animations.Runtime.Motion;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>ConstructionPlacedVisualService — struct: будівництва Placed візуалу сервісу.</summary>
    internal sealed class ConstructionPlacedVisualService :
        IConstructionPlacedVisualLookup
    {
        private const float PlacedSnapSharpness = 12f;

        private readonly Dictionary<Vector2Int, GameObject> _placedByPosition = new();
        private readonly Dictionary<Vector2Int, EntityPresentationConfig> _presentationByPosition = new();
        private readonly Dictionary<Vector2Int, string> _buildingIdByPosition = new();
        private readonly Dictionary<Vector2Int, string> _ownerIdByPosition = new();
        private readonly Dictionary<Vector2Int, Quaternion> _baseRotationByPosition = new();
        private readonly HashSet<Vector2Int> _demolitionPreviewPositions = new();
        private readonly HashSet<Vector2Int> _underConstructionPositions = new();
        private readonly SpriteSelectionHighlighter _selectionHighlighter = new();
        private readonly ConstructionVisualRootService _roots;
        private readonly ConstructionVisualFactory _visualFactory;
        private readonly ConstructionVisualStyleService _styleService;
        private readonly ConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly IGameplayMotionSettingsProvider _motionSettings;
        private readonly SignalBus _signalBus;

        private Vector2Int? _selectedPosition;

        /// <summary>Виконує ConstructionPlacedVisualService.</summary>
        [Inject]
        public ConstructionPlacedVisualService(
            ConstructionVisualRootService roots,
            ConstructionVisualFactory visualFactory,
            ConstructionVisualStyleService styleService,
            [InjectOptional] ConstructionTerrainAlignmentService terrainAlignment = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null,
            [InjectOptional] IGameplayMotionSettingsProvider motionSettings = null,
            [InjectOptional] SignalBus signalBus = null)
        {
            _roots = roots;
            _visualFactory = visualFactory;
            _styleService = styleService;
            _terrainAlignment = terrainAlignment;
            _settingsProvider = settingsProvider;
            _motionSettings = motionSettings;
            _signalBus = signalBus;
        }

        /// <summary>Замінює або створює візуал будівлі у вказаній позиції.</summary>
        public void Replace(
            Vector2Int position,
            string buildingId,
            GameObject prefab,
            Quaternion rotation,
            float visualOffsetY = 0f,
            GameObject sourceVisual = null,
            EntityPresentationConfig presentation = null,
            string ownerId = null,
            bool instantVisual = false)
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
            SetOwner(position, ownerId);
            _baseRotationByPosition[position] = rotation;
            _demolitionPreviewPositions.Remove(position);
            _underConstructionPositions.Remove(position);

            if (_selectedPosition.HasValue && _selectedPosition.Value == position)
                _selectionHighlighter.Apply(instance);

            // Fresh placements (not relocations) emerge from the ground —
            // the building rises out of its foundation and settles into place.
            if (sourceVisual == null && !instantVisual)
                PlayPlacementEmerge(instance, buildingId, position);
        }

        /// <summary>Замінює візуал, відновлюючи збережену позу (позицію/поворот/масштаб).</summary>
        public void ReplaceWithStoredPose(
            Vector2Int position,
            GameObject prefab,
            float visualOffsetY = 0f,
            EntityPresentationConfig presentation = null,
            string ownerId = null)
        {
            if (prefab == null || !_placedByPosition.ContainsKey(position))
                return;

            string buildingId = _buildingIdByPosition.TryGetValue(position, out string storedBuildingId)
                ? storedBuildingId
                : string.Empty;
            Quaternion rotation = _baseRotationByPosition.TryGetValue(position, out Quaternion storedRotation)
                ? storedRotation
                : Quaternion.identity;
            string owner = ownerId ?? (TryGetOwner(position, out string storedOwner)
                ? storedOwner
                : null);

            Replace(
                position,
                buildingId,
                prefab,
                rotation,
                visualOffsetY,
                presentation: presentation,
                ownerId: owner);
        }

        /// <summary>Призначає власника будівлі та перемальовує під палітру фракції.</summary>
        public void SetOwner(Vector2Int position, string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
                _ownerIdByPosition.Remove(position);
            else
                _ownerIdByPosition[position] = ownerId;
        }

        /// <summary>Намагається отримати власника будівлі за позицією.</summary>
        public bool TryGetOwner(Vector2Int position, out string ownerId)
        {
            return _ownerIdByPosition.TryGetValue(position, out ownerId)
                   && !string.IsNullOrEmpty(ownerId);
        }

        /// <summary>
        /// Перехід будівництво→робочий стан: готовий префаб замінює
        /// плейсхолдер будівництва без різкого попа — новий інстанс
        /// піднімається/сідає, а плейсхолдер стискається й зникає.
        /// </summary>
        public void TransitionToOperational(
            Vector2Int position,
            GameObject prefab,
            float visualOffsetY = 0f,
            EntityPresentationConfig presentation = null,
            bool instantVisual = false)
        {
            if (prefab == null || !_placedByPosition.ContainsKey(position))
                return;

            string buildingId = _buildingIdByPosition.TryGetValue(position, out string storedId)
                ? storedId
                : string.Empty;
            Quaternion rotation = _baseRotationByPosition.TryGetValue(position, out Quaternion storedRot)
                ? storedRot
                : Quaternion.identity;
            string owner = TryGetOwner(position, out string storedOwner)
                ? storedOwner
                : null;

            GameObject outgoing = Detach(position);
            Replace(
                position,
                buildingId,
                prefab,
                rotation,
                visualOffsetY,
                presentation: presentation,
                ownerId: owner,
                instantVisual: true);

            if (outgoing == null)
                return;

            if (!instantVisual
                && _placedByPosition.TryGetValue(position, out GameObject incoming)
                && incoming != null)
            {
                PlayOperationalTransition(incoming, outgoing, position, buildingId);
            }
            else
            {
                Object.Destroy(outgoing);
            }
        }

        /// <summary>
        /// Анімоване знесення: візуал опускається/трясеться зі світу, доки
        /// gameplay-стан уже очищено. Клітинка звільняється одразу.
        /// </summary>
        public void BeginDemolition(Vector2Int position)
        {
            GameObject instance = Detach(position, out string buildingId);
            if (instance == null)
                return;

            PlayDemolition(instance, buildingId, position);
        }

        /// <summary>Видаляє візуал будівлі у позиції.</summary>
        public void Remove(Vector2Int position)
        {
            GameObject instance = Detach(position, out _);
            if (instance != null)
                Object.Destroy(instance);
        }

        /// <summary>Detaches the placed visual from all registries without destroying it.</summary>
        private GameObject Detach(Vector2Int position)
            => Detach(position, out _);

        private GameObject Detach(Vector2Int position, out string buildingId)
        {
            buildingId = string.Empty;
            if (!_placedByPosition.TryGetValue(position, out GameObject instance))
                return null;

            if (_buildingIdByPosition.TryGetValue(position, out string id))
                buildingId = id;

            _placedByPosition.Remove(position);
            _presentationByPosition.Remove(position);
            _buildingIdByPosition.Remove(position);
            _ownerIdByPosition.Remove(position);
            _baseRotationByPosition.Remove(position);
            _demolitionPreviewPositions.Remove(position);
            _underConstructionPositions.Remove(position);
            return instance;
        }

        /// <summary>Поява будівлі з фундаменту: підйом із заглиблення та розгортання за вертикаллю.</summary>
        private void PlayPlacementEmerge(GameObject instance, string buildingId, Vector2Int position)
        {
            BuildingMotionProfile profile = _motionSettings?.Building;
            if (profile == null || _motionSettings.ReducedMotion)
                return;

            float duration = _motionSettings.ScaleDuration(profile.emergeDuration);
            EntityMotion motion = duration > 0f ? EntityMotion.AttachOrUpdate(instance) : null;
            if (motion == null)
                return;

            Transform t = instance.transform;
            Vector3 finalPosition = t.position;
            Vector3 finalScale = t.localScale;

            t.position = finalPosition + Vector3.down * profile.emergeDepth;
            t.localScale = new Vector3(
                finalScale.x,
                finalScale.y * Mathf.Clamp01(profile.emergeScaleYStart),
                finalScale.z);

            motion.EaseTo(finalPosition, duration, profile.emergeEase);
            motion.ScaleTo(finalScale, duration, profile.emergeEase);
        }

        /// <summary>Перехідний рух: новий інстанс сідає на місце, а вихідний стискається й знищується.</summary>
        private void PlayOperationalTransition(
            GameObject incoming,
            GameObject outgoing,
            Vector2Int position,
            string buildingId)
        {
            BuildingMotionProfile profile = _motionSettings?.Building;
            float duration = profile != null
                ? _motionSettings.ScaleDuration(profile.operationalDuration)
                : 0f;

            if (profile == null || _motionSettings.ReducedMotion || duration <= 0f)
            {
                Object.Destroy(outgoing);
                return;
            }

            EntityMotion inMotion = EntityMotion.AttachOrUpdate(incoming);
            if (inMotion != null)
            {
                Transform inT = incoming.transform;
                Vector3 finalPosition = inT.position;
                Vector3 finalScale = inT.localScale;

                inT.position = finalPosition + Vector3.up * profile.operationalRiseHeight;
                inT.localScale = finalScale * Mathf.Clamp01(profile.operationalStartScale);

                inMotion.EaseTo(finalPosition, duration, profile.operationalEase);
                inMotion.ScaleTo(finalScale, duration, profile.operationalEase);
            }

            EntityMotion outMotion = EntityMotion.AttachOrUpdate(outgoing);
            if (outMotion == null)
            {
                Object.Destroy(outgoing);
                return;
            }

            Transform outT = outgoing.transform;
            outMotion.EaseTo(
                outT.position + Vector3.down * (profile.demolishSinkDepth * 0.5f),
                duration,
                profile.demolishEase);
            outMotion.ScaleTo(
                outT.localScale * 0.05f,
                duration,
                profile.demolishEase,
                () => Object.Destroy(outgoing));
        }

        /// <summary>Знесення: візуал занурюється з тремтінням і знищується по завершенні.</summary>
        private void PlayDemolition(GameObject instance, string buildingId, Vector2Int position)
        {
            BuildingMotionProfile profile = _motionSettings?.Building;
            float duration = profile != null
                ? _motionSettings.ScaleDuration(profile.demolishDuration)
                : 0f;

            EntityMotion motion = duration > 0f && !_motionSettings.ReducedMotion
                ? EntityMotion.AttachOrUpdate(instance)
                : null;
            if (motion == null)
            {
                Object.Destroy(instance);
                return;
            }

            Transform t = instance.transform;
            motion.EaseTo(
                t.position + Vector3.down * profile.demolishSinkDepth,
                duration,
                profile.demolishEase);
            motion.ScaleTo(t.localScale * 0.6f, duration, profile.demolishEase);

            float shake = _motionSettings.ScaleSecondaryAmplitude(profile.demolishShakeAmplitude);
            if (shake > 0f)
                motion.PunchPosition(new Vector3(shake, 0f, 0f), duration, MotionEaseKind.InOutQuad);
        }

        /// <summary>Позначає будівлю вибраною.</summary>
        public void Select(Vector2Int position)
        {
            _selectedPosition = position;
            _selectionHighlighter.Clear();

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                _selectionHighlighter.Apply(instance);
        }

        /// <summary>Знімає виділення з поточної будівлі.</summary>
        public void ClearSelection()
        {
            _selectedPosition = null;
            _selectionHighlighter.Clear();
        }

        /// <summary>Знімає виділення, якщо воно відповідає позиції.</summary>
        public bool ClearSelectionIfMatches(Vector2Int position)
        {
            if (!_selectedPosition.HasValue || _selectedPosition.Value != position)
                return false;

            ClearSelection();
            return true;
        }

        /// <summary>Позначає будівлю як кандидата на знесення.</summary>
        public void MarkDemolitionPreview(Vector2Int position)
        {
            if (!_placedByPosition.TryGetValue(position, out GameObject instance) || instance == null)
                return;

            _demolitionPreviewPositions.Add(position);
            _styleService.ApplyGhostStyle(instance, false);
        }

        /// <summary>Переводить візуал у стан будівництва.</summary>
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

        /// <summary>Переводить візуал у робочий стан.</summary>
        public void MarkOperational(Vector2Int position)
        {
            _underConstructionPositions.Remove(position);

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
            {
                _styleService.ApplySolidStyle(instance);
                ApplyStoredPresentationStyle(position, instance);
            }
        }

        /// <summary>Відновлює стиль будівлі після скасування знесення.</summary>
        public void RestoreDemolitionPreview(Vector2Int position)
        {
            _demolitionPreviewPositions.Remove(position);

            if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                ApplyPersistentStyle(position, instance);
        }

        /// <summary>Очищує знесення превʼю Styles.</summary>
        public void ClearDemolitionPreviewStyles()
        {
            foreach (Vector2Int position in _demolitionPreviewPositions)
            {
                if (_placedByPosition.TryGetValue(position, out GameObject instance) && instance != null)
                    ApplyPersistentStyle(position, instance);
            }

            _demolitionPreviewPositions.Clear();
        }

        /// <summary>Видаляє всі розміщені візуали.</summary>
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
            _ownerIdByPosition.Clear();
            _baseRotationByPosition.Clear();
            _demolitionPreviewPositions.Clear();
            _underConstructionPositions.Clear();
            ClearSelection();
        }

        /// <summary>Намагається отримати візуал будівлі за позицією.</summary>
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
