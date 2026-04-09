using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualService : IInitializable, IDisposable, ITickable
    {
        private const bool VerboseLogs = true;
        private const int BuildingLayerMinSortingOrder = 5;
        private const float GhostAlpha = 0.55f;
        private const float BlockedFlashDuration = 0.35f;

        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IWallPlacementService _wallPlacementService;
        private readonly DiContainer _container;

        private readonly Dictionary<Vector2Int, GameObject> _previewByPosition = new();
        private readonly Dictionary<Vector2Int, GameObject> _placedByPosition = new();

        private struct FlashRestore
        {
            public GameObject Target;
            public bool IsGhostPreview; // true → відновити як зелений привид; false → відновити як solid
            public float RestoreAt;
        }

        private readonly List<FlashRestore> _flashRestores = new();

        private Transform _previewRoot;
        private Transform _placedRoot;

        [Inject]
        public ConstructionVisualService(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            IWallPlacementService wallPlacementService,
            DiContainer container)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
            _wallPlacementService = wallPlacementService;
            _container = container;
        }

        public void Initialize()
        {
            EnsureRoots();
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
            _signalBus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

            if (VerboseLogs)
                Debug.Log("[ConstructionVisual] Initialized.");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
            _signalBus.TryUnsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);

            _flashRestores.Clear();
            ClearDictionary(_previewByPosition);
            ClearDictionary(_placedByPosition);
        }

        public void Tick()
        {
            if (_flashRestores.Count == 0) return;
            float now = Time.time;
            for (int i = _flashRestores.Count - 1; i >= 0; i--)
            {
                var entry = _flashRestores[i];
                if (now >= entry.RestoreAt)
                {
                    if (entry.Target != null)
                    {
                        if (entry.IsGhostPreview)
                            ApplyGhostStyle(entry.Target, true);
                        else
                            ApplySolidStyle(entry.Target);
                    }
                    _flashRestores.RemoveAt(i);
                }
            }
        }

        private void OnBuildingPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            if (signal.PreviewState == BuildingPreviewState.None)
            {
                RemovePreview(signal.Position);
                return;
            }

            // Якщо позиція заблокована — блимаємо існуючим об'єктом, НЕ створюємо нового привида
            if (signal.PreviewState == BuildingPreviewState.Blocked)
            {
                HandleBlockedFlash(signal.Position);
                return;
            }

            if (string.IsNullOrWhiteSpace(signal.BuildingId))
            {
                Debug.LogWarning($"[ConstructionVisual] Preview at {signal.Position} has empty BuildingId.");
                return;
            }

            var def = _buildingRegistry.GetById(signal.BuildingId);
            if (def == null)
            {
                Debug.LogWarning($"[ConstructionVisual] BuildingDefinition not found for id '{signal.BuildingId}'.");
                return;
            }

            if (def.Prefab == null)
            {
                Debug.LogWarning($"[ConstructionVisual] Building '{signal.BuildingId}' has no prefab. Preview skipped.");
                return;
            }

            if (!_previewByPosition.TryGetValue(signal.Position, out var instance) || instance == null || !instance.name.Contains(signal.BuildingId))
            {
                RemovePreview(signal.Position);
                instance = CreateInstance(def.Prefab, signal.Position, _previewRoot, $"Preview_{signal.BuildingId}_{signal.Position.x}_{signal.Position.y}");
                _previewByPosition[signal.Position] = instance;
            }

            ApplyGhostStyle(instance, true);

            if (VerboseLogs)
                Debug.Log($"[ConstructionVisual] Preview Valid for '{signal.BuildingId}' at {signal.Position}");
        }

        private void HandleBlockedFlash(Vector2Int position)
        {
            // Спочатку перевіряємо чи є привид на цій позиції
            if (_previewByPosition.TryGetValue(position, out var preview) && preview != null)
            {
                FlashObject(preview, isGhostPreview: true);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Blocked flash on existing preview at {position}");
                return;
            }

            // Потім перевіряємо чи є розміщена будівля на цій позиції
            if (_placedByPosition.TryGetValue(position, out var placed) && placed != null)
            {
                FlashObject(placed, isGhostPreview: false);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Blocked flash on placed building at {position}");
                return;
            }

            if (VerboseLogs)
                Debug.Log($"[ConstructionVisual] Blocked at {position}, no tracked visual to flash (pre-loaded object or spacing violation).");
        }

        private void FlashObject(GameObject target, bool isGhostPreview)
        {
            ApplyGhostStyle(target, false); // червоний
            _flashRestores.Add(new FlashRestore
            {
                Target = target,
                IsGhostPreview = isGhostPreview,
                RestoreAt = Time.time + BlockedFlashDuration,
            });
        }

        private void OnBuildingCancelled(BuildingCancelledSignal _)
        {
            ClearDictionary(_previewByPosition);

            if (VerboseLogs)
                Debug.Log("[ConstructionVisual] Cancel received -> all previews cleared.");
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            RemovePreview(signal.Position);

            if (_wallPlacementService.TryResolvePlacedVisual(signal.Position, signal.BuildingId, out _, out _))
            {
                RefreshWallNeighborhood(signal.Position);

                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Spawned/updated wall collection element '{signal.BuildingId}' at {signal.Position}");

                return;
            }

            var def = _buildingRegistry.GetById(signal.BuildingId);
            if (def == null || def.Prefab == null)
            {
                Debug.LogWarning($"[ConstructionVisual] Cannot spawn placed building '{signal.BuildingId}' at {signal.Position}: missing definition or prefab.");
                return;
            }

            if (_placedByPosition.TryGetValue(signal.Position, out var existing) && existing != null)
                UnityEngine.Object.Destroy(existing);

            var instance = CreateInstance(def.Prefab, signal.Position, _placedRoot, $"Building_{signal.BuildingId}_{signal.Position.x}_{signal.Position.y}");
            ApplySolidStyle(instance);
            _placedByPosition[signal.Position] = instance;

            if (VerboseLogs)
                Debug.Log($"[ConstructionVisual] Spawned placed building '{signal.BuildingId}' at {signal.Position}");
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            if (_placedByPosition.TryGetValue(signal.Position, out var instance))
            {
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
                _placedByPosition.Remove(signal.Position);

                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Removed building visual '{signal.BuildingId}' at {signal.Position}");
            }
            else if (VerboseLogs)
            {
                Debug.LogWarning($"[ConstructionVisual] Demolish signal received for {signal.Position}, but no placed visual was tracked.");
            }

            if (_wallPlacementService.IsWallOrGate(signal.BuildingId))
                RefreshWallNeighborhood(signal.Position);
        }

        private void RefreshWallNeighborhood(Vector2Int center)
        {
            RefreshWallVisualAt(center);
            RefreshWallVisualAt(center + Vector2Int.up);
            RefreshWallVisualAt(center + Vector2Int.right);
            RefreshWallVisualAt(center + Vector2Int.down);
            RefreshWallVisualAt(center + Vector2Int.left);
        }

        private void RefreshWallVisualAt(Vector2Int position)
        {
            if (!_objectsMapService.TryGetOccupant(position, out var occupantId))
            {
                if (_placedByPosition.TryGetValue(position, out var existing) && existing != null)
                {
                    UnityEngine.Object.Destroy(existing);
                    _placedByPosition.Remove(position);
                }
                return;
            }

            if (!_wallPlacementService.TryResolvePlacedVisual(position, occupantId, out var prefab, out var rotation))
                return;

            if (_placedByPosition.TryGetValue(position, out var current) && current != null)
                UnityEngine.Object.Destroy(current);

            var instance = CreateInstance(prefab, position, _placedRoot, $"Building_{occupantId}_{position.x}_{position.y}", rotation);
            ApplySolidStyle(instance);
            _placedByPosition[position] = instance;
        }

        private void EnsureRoots()
        {
            _previewRoot = EnsureRoot("ConstructionPreviewRoot");
            _placedRoot = EnsureRoot("PlayerBuildingsRoot");
        }

        private static Transform EnsureRoot(string rootName)
        {
            var existing = GameObject.Find(rootName);
            if (existing != null)
                return existing.transform;

            return new GameObject(rootName).transform;
        }

        private GameObject CreateInstance(GameObject prefab, Vector2Int tile, Transform parent, string objectName, Quaternion? forcedRotation = null)
        {
            Vector3 worldPos = new Vector3(tile.x, tile.y, 0.1f);
            Quaternion rotation = forcedRotation ?? prefab.transform.rotation;
            var instance = _container.InstantiatePrefab(prefab, worldPos, rotation, parent);
            instance.name = objectName;
            EnsureBuildingSortingOrder(instance, BuildingLayerMinSortingOrder);
            DisableColliders(instance);
            return instance;
        }

        private static void DisableColliders(GameObject rootObject)
        {
            var colliders3D = rootObject.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders3D.Length; i++)
                colliders3D[i].enabled = false;

            var colliders2D = rootObject.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < colliders2D.Length; i++)
                colliders2D[i].enabled = false;
        }

        private static void ApplyGhostStyle(GameObject rootObject, bool isValid)
        {
            var tint = isValid
                ? new Color(0.55f, 1f, 0.55f, GhostAlpha)
                : new Color(1f, 0.45f, 0.45f, GhostAlpha);

            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].color = tint;
            }
        }

        private static void ApplySolidStyle(GameObject rootObject)
        {
            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                var color = spriteRenderers[i].color;
                color.a = 1f;
                color.r = Mathf.Clamp01(color.r);
                color.g = Mathf.Clamp01(color.g);
                color.b = Mathf.Clamp01(color.b);
                spriteRenderers[i].color = color;
            }
        }

        private void RemovePreview(Vector2Int position)
        {
            if (_previewByPosition.TryGetValue(position, out var instance))
            {
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
                _previewByPosition.Remove(position);
            }
        }

        private static void ClearDictionary(Dictionary<Vector2Int, GameObject> map)
        {
            foreach (var pair in map)
            {
                if (pair.Value != null)
                    UnityEngine.Object.Destroy(pair.Value);
            }

            map.Clear();
        }

        private static void EnsureBuildingSortingOrder(GameObject rootObject, int minOrder)
        {
            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr.sortingOrder < minOrder)
                    sr.sortingOrder = minOrder;
            }

            var sortingGroups = rootObject.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true);
            foreach (var sg in sortingGroups)
            {
                if (sg.sortingOrder < minOrder)
                    sg.sortingOrder = minOrder;
            }
        }
    }
}
