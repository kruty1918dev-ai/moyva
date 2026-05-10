using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
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
        private const string InfluenceRadiusShaderName = "Moyva/2D/InfluenceRadius";

        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IGridService _gridService;
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IWallPlacementService _wallPlacementService;
        private readonly DiContainer _container;
        private readonly int _townHallBuildRadius;

        private readonly Dictionary<Vector2Int, GameObject> _previewByPosition = new();
        private readonly Dictionary<Vector2Int, GameObject> _placedByPosition = new();
        private readonly HashSet<Vector2Int> _demolitionPreviewPositions = new();
        private readonly SpriteSelectionHighlighter _buildingSelectionHighlighter = new();

        private struct FlashRestore
        {
            public GameObject Target;
            public bool IsGhostPreview; // true → відновити як зелений привид; false → відновити як solid
            public float RestoreAt;
        }

        private readonly List<FlashRestore> _flashRestores = new();

        private Transform _previewRoot;
        private Transform _placedRoot;
        private Transform _radiusRoot;

        // Кешований меш (1×1 quad, локальні координати ±0.5), спільний для обох рендерів
        private Mesh _radiusMesh;

        private GameObject _previewRadiusGo;
        private MeshRenderer _previewRadiusMR;
        private Material _previewRadiusMat;

        private GameObject _inspectionRadiusGo;
        private MeshRenderer _inspectionRadiusMR;
        private Material _inspectionRadiusMat;
        private Vector2Int? _selectedBuildingPosition;

        [Inject]
        public ConstructionVisualService(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            IGridService gridService,
            LazyInject<IConstructionService> constructionService,
            IWallPlacementService wallPlacementService,
            DiContainer container,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
            _gridService = gridService;
            _constructionService = constructionService;
            _wallPlacementService = wallPlacementService;
            _container = container;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
        }

        public void Initialize()
        {
            Debug.Log("[ConstructionVisual] Initialize() почало роботу...");
            
            try
            {
                EnsureRoots();
                Debug.Log($"[ConstructionVisual] ✓ Root трансформи підготовлені: PreviewRoot={_previewRoot.name}, PlacedRoot={_placedRoot.name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА при EnsureRoots: {ex.Message}");
                return;
            }
            
            try
            {
                _signalBus.Subscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
                Debug.Log("[ConstructionVisual] ✓ Підписано на BuildingPreviewChangedSignal");
                
                _signalBus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
                Debug.Log("[ConstructionVisual] ✓ Підписано на BuildingCancelledSignal");
                
                _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                Debug.Log("[ConstructionVisual] ✓ Підписано на BuildingPlacedSignal");
                
                _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
                Debug.Log("[ConstructionVisual] ✓ Підписано на BuildingDemolishedSignal");

                _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);
                Debug.Log("[ConstructionVisual] ✓ Підписано на WorldInfoSelectionChangedSignal");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА при підписці на сигнали: {ex.Message}");
                return;
            }

            Debug.Log("[ConstructionVisual] ✅ Initialized успішно.");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
            _signalBus.TryUnsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnWorldInfoSelectionChanged);

            _flashRestores.Clear();
            ClearDemolitionPreviewStyles();
            ClearDictionary(_previewByPosition);
            ClearDictionary(_placedByPosition);
            SetRadiusVisible(_previewRadiusMR, false);
            SetRadiusVisible(_inspectionRadiusMR, false);
            _buildingSelectionHighlighter.Clear();
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
            if (_constructionService.Value.IsDemolishMode)
            {
                HandleDemolitionPreviewChanged(signal);
                return;
            }

            if (signal.PreviewState == BuildingPreviewState.None)
            {
                RemovePreview(signal.Position);
                SetRadiusVisible(_previewRadiusMR, false);

                // Якщо видалили стіну — оновити сусідні preview-спрайти
                if (!string.IsNullOrWhiteSpace(signal.BuildingId) && _wallPlacementService.IsWallOrGate(signal.BuildingId))
                    RefreshWallPreviewNeighborhood(signal.Position, signal.BuildingId);

                return;
            }

            // Якщо позиція заблокована — блимаємо існуючим об'єктом, НЕ створюємо нового привида
            if (signal.PreviewState == BuildingPreviewState.Blocked)
            {
                HandleBlockedFlash(signal.Position);

                var blockedDef = _buildingRegistry.GetById(signal.BuildingId);
                if (HasInfluenceRadius(blockedDef))
                {
                    int radius = ResolveInfluenceRadius(blockedDef);
                    DrawInfluenceRadius(_previewRadiusGo, _previewRadiusMR, _previewRadiusMat, signal.Position, radius);
                }

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

                // Для стін використовуємо prefab з урахуванням сусідів
                GameObject prefabForPreview = def.Prefab;
                if (_wallPlacementService.TryResolvePreviewVisual(signal.Position, signal.BuildingId, out var wallPrefab))
                    prefabForPreview = wallPrefab;

                string prefabTag = prefabForPreview != null ? prefabForPreview.name : "NULL";
                instance = CreateInstance(prefabForPreview, signal.Position, _previewRoot, $"Preview_{signal.BuildingId}_{prefabTag}_{signal.Position.x}_{signal.Position.y}");
                _previewByPosition[signal.Position] = instance;
            }

            ApplyGhostStyle(instance, true);

            if (HasInfluenceRadius(def))
            {
                int radius = ResolveInfluenceRadius(def);
                DrawInfluenceRadius(_previewRadiusGo, _previewRadiusMR, _previewRadiusMat, signal.Position, radius);
            }
            else
            {
                SetRadiusVisible(_previewRadiusMR, false);
            }

            // Для стін — оновити візуал сусідніх preview
            if (_wallPlacementService.IsWallOrGate(signal.BuildingId))
                RefreshWallPreviewNeighborhood(signal.Position, signal.BuildingId);

            if (VerboseLogs)
                Debug.Log($"[ConstructionVisual] Preview Valid for '{signal.BuildingId}' at {signal.Position}");
        }

        private void HandleDemolitionPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            if (signal.PreviewState == BuildingPreviewState.None)
            {
                _demolitionPreviewPositions.Remove(signal.Position);

                if (_placedByPosition.TryGetValue(signal.Position, out var placed) && placed != null)
                    ApplySolidStyle(placed);

                return;
            }

            if (signal.PreviewState == BuildingPreviewState.Blocked)
            {
                HandleBlockedFlash(signal.Position);
                return;
            }

            if (_placedByPosition.TryGetValue(signal.Position, out var instance) && instance != null)
            {
                _demolitionPreviewPositions.Add(signal.Position);
                ApplyGhostStyle(instance, false);

                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Demolish preview marked for '{signal.BuildingId}' at {signal.Position}");
            }
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
            ClearDemolitionPreviewStyles();
            SetRadiusVisible(_previewRadiusMR, false);

            if (VerboseLogs)
                Debug.Log("[ConstructionVisual] Cancel received -> all previews cleared.");
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            RemovePreview(signal.Position);
            SetRadiusVisible(_previewRadiusMR, false);

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
            _demolitionPreviewPositions.Remove(signal.Position);

            if (_selectedBuildingPosition.HasValue && _selectedBuildingPosition.Value == signal.Position)
                _buildingSelectionHighlighter.Apply(instance);

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
                _demolitionPreviewPositions.Remove(signal.Position);

                if (_selectedBuildingPosition.HasValue && _selectedBuildingPosition.Value == signal.Position)
                {
                    _selectedBuildingPosition = null;
                    _buildingSelectionHighlighter.Clear();
                    SetRadiusVisible(_inspectionRadiusMR, false);
                }

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

        private void OnWorldInfoSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind != WorldInfoSelectionKind.Building || string.IsNullOrWhiteSpace(signal.ObjectId))
            {
                _selectedBuildingPosition = null;
                _buildingSelectionHighlighter.Clear();
                SetRadiusVisible(_inspectionRadiusMR, false);
                return;
            }

            _selectedBuildingPosition = signal.Position;
            _buildingSelectionHighlighter.Clear();

            if (_placedByPosition.TryGetValue(signal.Position, out var instance) && instance != null)
                _buildingSelectionHighlighter.Apply(instance);

            var def = _buildingRegistry.GetById(signal.ObjectId);
            if (!HasInfluenceRadius(def))
            {
                SetRadiusVisible(_inspectionRadiusMR, false);
                return;
            }

            int radius = ResolveInfluenceRadius(def);
            DrawInfluenceRadius(_inspectionRadiusGo, _inspectionRadiusMR, _inspectionRadiusMat, signal.Position, radius);
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

            if (_selectedBuildingPosition.HasValue && _selectedBuildingPosition.Value == position)
                _buildingSelectionHighlighter.Apply(instance);
        }

        /// <summary>Оновити preview-спрайти сусідніх стін при додаванні/видаленні wall preview.</summary>
        private void RefreshWallPreviewNeighborhood(Vector2Int center, string buildingId)
        {
            RefreshWallPreviewAt(center + Vector2Int.up, buildingId);
            RefreshWallPreviewAt(center + Vector2Int.right, buildingId);
            RefreshWallPreviewAt(center + Vector2Int.down, buildingId);
            RefreshWallPreviewAt(center + Vector2Int.left, buildingId);
        }

        private void RefreshWallPreviewAt(Vector2Int position, string wallBuildingId)
        {
            if (!_previewByPosition.TryGetValue(position, out var existing) || existing == null)
                return;

            var sourceBuildingId = wallBuildingId;
            if (_constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingBuildingId)
                && !string.IsNullOrWhiteSpace(pendingBuildingId))
            {
                sourceBuildingId = pendingBuildingId;
            }

            if (!_wallPlacementService.TryResolvePreviewVisual(position, sourceBuildingId, out var prefab))
                return;

            bool wasGhostGreen = true;
            UnityEngine.Object.Destroy(existing);

            string prefabTag = prefab != null ? prefab.name : "NULL";
            var instance = CreateInstance(prefab, position, _previewRoot,
                $"Preview_{sourceBuildingId}_{prefabTag}_{position.x}_{position.y}");
            _previewByPosition[position] = instance;
            if (wasGhostGreen) ApplyGhostStyle(instance, true);
        }

        private void EnsureRoots()
        {
            _previewRoot = EnsureRoot("ConstructionPreviewRoot");
            _placedRoot  = EnsureRoot("PlayerBuildingsRoot");
            _radiusRoot  = EnsureRoot("ConstructionRadiusRoot");

            // Кешуємо один спільний меш — простий quad 1×1
            _radiusMesh = BuildQuadMesh();

            _previewRadiusGo = EnsureRadiusMeshObject(
                "PreviewInfluenceRadius", _radiusRoot,
                BuildingLayerMinSortingOrder + 30,
                out _previewRadiusMR, out _previewRadiusMat);

            _inspectionRadiusGo = EnsureRadiusMeshObject(
                "SelectedInfluenceRadius", _radiusRoot,
                BuildingLayerMinSortingOrder + 31,
                out _inspectionRadiusMR, out _inspectionRadiusMat);
        }

        private static Transform EnsureRoot(string rootName)
        {
            var existing = GameObject.Find(rootName);
            if (existing != null)
                return existing.transform;

            return new GameObject(rootName).transform;
        }

        /// <summary>Quad 1×1 у локальному просторі (вершини ±0.5), UV (0,0)→(1,1).</summary>
        private static Mesh BuildQuadMesh()
        {
            var mesh = new Mesh { name = "InfluenceRadiusQuad" };
            mesh.vertices = new Vector3[]
            {
                new(-0.5f, -0.5f, 0f),
                new( 0.5f, -0.5f, 0f),
                new( 0.5f,  0.5f, 0f),
                new(-0.5f,  0.5f, 0f),
            };
            mesh.uv = new Vector2[] { new(0f, 0f), new(1f, 0f), new(1f, 1f), new(0f, 1f) };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }

        private GameObject EnsureRadiusMeshObject(
            string objectName, Transform parent, int sortingOrder,
            out MeshRenderer mr, out Material mat)
        {
            Transform existing = parent.Find(objectName);
            if (existing != null)
                UnityEngine.Object.Destroy(existing.gameObject);

            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);

            // MeshFilter ОБОВЯЗКОВО до MeshRenderer (вимога Unity)
            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = _radiusMesh;

            mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;
            mr.sortingLayerName  = "Default";
            mr.sortingOrder      = sortingOrder;

            var shader = Shader.Find(InfluenceRadiusShaderName);
            if (shader == null)
            {
                Debug.LogError($"[ConstructionVisual] Shader '{InfluenceRadiusShaderName}' not found. Influence radius overlay is disabled to avoid rendering a fallback quad.");
                mat = null;
                mr.sharedMaterial = null;
                mr.enabled = false;
                return go;
            }

            mat = new Material(shader) { name = $"{objectName}_Material" };

            // Межі мапи в world-space для шейдерної маски (1 тайл = 1 юніт, центр тайла в int-координаті)
            float minX = -0.5f;
            float minY = -0.5f;
            float maxX = _gridService.GridWidth - 0.5f;
            float maxY = _gridService.GridHeight - 0.5f;
            mat.SetVector("_MapRect", new Vector4(minX, minY, maxX, maxY));

            mr.sharedMaterial = mat;
            mr.enabled = false;

            return go;
        }

        private bool HasInfluenceRadius(BuildingDefinition def)
        {
            return def != null &&
                (BuildingDefinitionCapabilities.IsTownHall(def) || BuildingDefinitionCapabilities.IsCastle(def));
        }

        private int ResolveInfluenceRadius(BuildingDefinition def)
        {
            return BuildingDefinitionCapabilities.GetInfluenceRadius(def, _townHallBuildRadius);
        }

        /// <summary>
        /// Позиціонує, масштабує та вмикає зону впливу.
        /// localScale = (2r+1) × (2r+1) → кожен юніт = один тайл.
        /// Ring width, dash length and gap length задаються у world-space,
        /// щоб пунктир мав однакову довжину незалежно від радіуса.
        /// </summary>
        private static void DrawInfluenceRadius(
            GameObject go, MeshRenderer mr, Material mat,
            Vector2Int center, int radius)
        {
            if (go == null || mr == null || mat == null) return;

            float size = radius * 2f + 1f;
            go.transform.position   = new Vector3(center.x, center.y, 0.05f);
            go.transform.localScale = new Vector3(size, size, 1f);

            const float borderWidthWorld = 0.5f;
            var white = new Color(1f, 1f, 1f, 0.95f);

            mat.SetColor("_Color", white);
            mat.SetColor("_FillColor", new Color(1f, 1f, 1f, 0.04f));
            mat.SetFloat("_BorderWidth", borderWidthWorld);

            mr.enabled = true;
        }

        private static void SetRadiusVisible(MeshRenderer mr, bool visible)
        {
            if (mr != null)
                mr.enabled = visible;
        }

        private GameObject CreateInstance(GameObject prefab, Vector2Int tile, Transform parent, string objectName, Quaternion? forcedRotation = null)
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
            
            Vector3 worldPos = new Vector3(tile.x, tile.y, 0.1f);
            Quaternion rotation = forcedRotation ?? prefab.transform.rotation;
            
            GameObject instance = null;
            try
            {
                // Використовуємо Object.Instantiate замість Container.InstantiatePrefab,
                // щоб уникнути проблем з Zenject-ін'єкцією при спавні preview/placed об'єктів
                instance = UnityEngine.Object.Instantiate(prefab, worldPos, rotation, parent);
                
                if (instance == null)
                {
                    Debug.LogError($"[ConstructionVisual] ПОМИЛКА: Instantiate повернув null для {objectName}");
                    return null;
                }
                
                instance.name = objectName;
                
                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] ✓ Created instance: {objectName} at {worldPos}, parent={parent.name}");
                
                EnsureBuildingSortingOrder(instance, BuildingLayerMinSortingOrder);
                DisableColliders(instance);
                return instance;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConstructionVisual] ПОМИЛКА при CreateInstance ({objectName}): {ex.GetType().Name} - {ex.Message}");
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
                return null;
            }
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
                spriteRenderers[i].color = Color.white;
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

        private void ClearDemolitionPreviewStyles()
        {
            foreach (var position in _demolitionPreviewPositions)
            {
                if (_placedByPosition.TryGetValue(position, out var placed) && placed != null)
                    ApplySolidStyle(placed);
            }

            _demolitionPreviewPositions.Clear();
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

        public bool TryGetPlacedVisual(Vector2Int position, out GameObject visual)
        {
            if (_placedByPosition.TryGetValue(position, out var go) && go != null)
            {
                visual = go;
                return true;
            }
            visual = null;
            return false;
        }
    }
}
