using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualService : IInitializable, IDisposable, ITickable
    {
        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IGridService _gridService;
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallVisualResolver _wallVisualResolver;
        private readonly int _townHallBuildRadius;
        private readonly IGridProjection _gridProjection;
        private readonly IConstructionVisualFactory _visualFactory;
        private readonly IConstructionVisualStyleService _styleService;
        private readonly IConstructionTerrainAlignmentService _terrainAlignmentService;
        private readonly IConstructionBlockedFlashService _blockedFlashService;
        private readonly IConstructionVisualSettingsProvider _visualSettingsProvider;
        private readonly IConstructionSceneSettingsProvider _sceneSettingsProvider;
        private readonly IConstructionDiagnosticsSettingsProvider _diagnosticsSettingsProvider;

        private readonly Dictionary<Vector2Int, GameObject> _previewByPosition = new();
        private readonly Dictionary<Vector2Int, GameObject> _placedByPosition = new();
        private readonly HashSet<Vector2Int> _demolitionPreviewPositions = new();
        private readonly SpriteSelectionHighlighter _buildingSelectionHighlighter = new();

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

        private sealed class InfluenceRadiusMeshOverlayState
        {
            public bool Active;
            public Vector2Int Center;
            public int Radius;
            public Bounds Bounds;
            public Material Material;
            public readonly List<MeshRenderer> Renderers = new();
        }

        private readonly InfluenceRadiusMeshOverlayState _previewRadiusOverlay = new();
        private readonly InfluenceRadiusMeshOverlayState _inspectionRadiusOverlay = new();
        private bool VerboseLogs => _diagnosticsSettingsProvider?.EnableVerboseLogs ?? (Application.isEditor && Debug.isDebugBuild);

        [Inject]
        public ConstructionVisualService(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            IGridService gridService,
            LazyInject<IConstructionService> constructionService,
            IWallTopologyService wallTopologyService,
            IWallVisualResolver wallVisualResolver,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionVisualSettingsProvider visualSettingsProvider = null,
            [InjectOptional] IConstructionSceneSettingsProvider sceneSettingsProvider = null,
            [InjectOptional] IConstructionDiagnosticsSettingsProvider diagnosticsSettingsProvider = null,
            IConstructionVisualFactory visualFactory = null,
            IConstructionVisualStyleService styleService = null,
            IConstructionTerrainAlignmentService terrainAlignmentService = null,
            IConstructionBlockedFlashService blockedFlashService = null)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
            _gridService = gridService;
            _constructionService = constructionService;
            _wallTopologyService = wallTopologyService;
            _wallVisualResolver = wallVisualResolver;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
            _gridProjection = gridProjection;
            _visualSettingsProvider = visualSettingsProvider;
            _sceneSettingsProvider = sceneSettingsProvider;
            _diagnosticsSettingsProvider = diagnosticsSettingsProvider;
            _visualFactory = visualFactory;
            _styleService = styleService;
            _terrainAlignmentService = terrainAlignmentService;
            _blockedFlashService = blockedFlashService;
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

            _blockedFlashService?.Clear();
            ClearDemolitionPreviewStyles();
            ClearDictionary(_previewByPosition);
            ClearDictionary(_placedByPosition);
            SetRadiusVisible(_previewRadiusMR, false);
            SetRadiusVisible(_inspectionRadiusMR, false);
            _buildingSelectionHighlighter.Clear();
        }

        public void Tick()
        {
            _blockedFlashService?.Tick();

            DrawInfluenceRadiusMeshOverlay(_previewRadiusOverlay);
            DrawInfluenceRadiusMeshOverlay(_inspectionRadiusOverlay);
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
                if (!string.IsNullOrWhiteSpace(signal.BuildingId) && _wallTopologyService.IsWallOrGate(signal.BuildingId))
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
                if (_wallVisualResolver.TryResolvePreviewVisual(signal.Position, signal.BuildingId, out var wallPrefab))
                    prefabForPreview = wallPrefab;

                string prefabTag = prefabForPreview != null ? prefabForPreview.name : "NULL";
                instance = _visualFactory.CreateInstance(
                    prefabForPreview,
                    signal.Position,
                    _previewRoot,
                    $"Preview_{signal.BuildingId}_{prefabTag}_{signal.Position.x}_{signal.Position.y}",
                    ResolveBuildingLayerMinSortingOrder(),
                    isPreviewVisual: true);
                if (instance == null)
                    return;

                ConfigurePointerTarget(instance, signal.BuildingId, signal.Position, isPreviewVisual: true);
                _previewByPosition[signal.Position] = instance;
            }

            _styleService.ApplyGhostStyle(instance, true);

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
            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
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
                    _styleService.ApplySolidStyle(placed);

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
                _styleService.ApplyGhostStyle(instance, false);

                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Demolish preview marked for '{signal.BuildingId}' at {signal.Position}");
            }
        }

        private void HandleBlockedFlash(Vector2Int position)
        {
            // Спочатку перевіряємо чи є привид на цій позиції
            if (_previewByPosition.TryGetValue(position, out var preview) && preview != null)
            {
                _blockedFlashService.Flash(preview, isGhostPreview: true);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Blocked flash on existing preview at {position}");
                return;
            }

            // Потім перевіряємо чи є розміщена будівля на цій позиції
            if (_placedByPosition.TryGetValue(position, out var placed) && placed != null)
            {
                _blockedFlashService.Flash(placed, isGhostPreview: false);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionVisual] Blocked flash on placed building at {position}");
                return;
            }

            if (VerboseLogs)
                Debug.Log($"[ConstructionVisual] Blocked at {position}, no tracked visual to flash (pre-loaded object or spacing violation).");
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

            if (_wallVisualResolver.TryResolvePlacedVisual(signal.Position, signal.BuildingId, out _, out _))
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

            var instance = _visualFactory.CreateInstance(def.Prefab, signal.Position, _placedRoot, $"Building_{signal.BuildingId}_{signal.Position.x}_{signal.Position.y}", ResolveBuildingLayerMinSortingOrder());
            if (instance == null)
                return;

            ConfigurePointerTarget(instance, signal.BuildingId, signal.Position, isPreviewVisual: false);
            _styleService.ApplySolidStyle(instance);
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

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
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

            if (!_wallVisualResolver.TryResolvePlacedVisual(position, occupantId, out var prefab, out var rotation))
                return;

            if (_placedByPosition.TryGetValue(position, out var current) && current != null)
                UnityEngine.Object.Destroy(current);

            var instance = _visualFactory.CreateInstance(prefab, position, _placedRoot, $"Building_{occupantId}_{position.x}_{position.y}", ResolveBuildingLayerMinSortingOrder(), rotation);
            if (instance == null)
                return;

            ConfigurePointerTarget(instance, occupantId, position, isPreviewVisual: false);
            _styleService.ApplySolidStyle(instance);
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

            if (!_wallVisualResolver.TryResolvePreviewVisual(position, sourceBuildingId, out var prefab))
                return;

            bool wasGhostGreen = true;
            UnityEngine.Object.Destroy(existing);

            string prefabTag = prefab != null ? prefab.name : "NULL";
            var instance = _visualFactory.CreateInstance(
                prefab,
                position,
                _previewRoot,
                $"Preview_{sourceBuildingId}_{prefabTag}_{position.x}_{position.y}",
                ResolveBuildingLayerMinSortingOrder(),
                isPreviewVisual: true);
            if (instance == null)
                return;

            ConfigurePointerTarget(instance, sourceBuildingId, position, isPreviewVisual: true);
            _previewByPosition[position] = instance;
            if (wasGhostGreen) _styleService.ApplyGhostStyle(instance, true);
        }

        private static void ConfigurePointerTarget(GameObject instance, string buildingId, Vector2Int position, bool isPreviewVisual)
        {
            ConstructionBuildingPointerTarget.AttachOrUpdate(instance, buildingId, position, isPreviewVisual);
        }

        private void EnsureRoots()
        {
            _sceneSettingsProvider?.EnsureSceneRoots();

            _previewRoot = _sceneSettingsProvider?.ResolvePreviewRoot() ?? EnsureRoot(ResolvePreviewRootName());
            _placedRoot  = _sceneSettingsProvider?.ResolvePlacedRoot() ?? EnsureRoot(ResolvePlacedRootName());
            _radiusRoot  = _sceneSettingsProvider?.ResolveRadiusRoot() ?? EnsureRoot(ResolveRadiusRootName());

            _radiusMesh = GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                ? null
                : BuildQuadMesh();

            _previewRadiusGo = EnsureRadiusMeshObject(
                "PreviewInfluenceRadius", _radiusRoot,
                ResolveBuildingLayerMinSortingOrder() + 30,
                out _previewRadiusMR, out _previewRadiusMat);

            _inspectionRadiusGo = EnsureRadiusMeshObject(
                "SelectedInfluenceRadius", _radiusRoot,
                ResolveBuildingLayerMinSortingOrder() + 31,
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

            mr = go.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows    = false;
            mr.sortingLayerName  = "Default";
            mr.sortingOrder      = sortingOrder;

            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
            {
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = _radiusMesh;
            }

            string shaderName = ResolveInfluenceRadiusShaderName();
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[ConstructionVisual] Shader '{shaderName}' not found. Influence radius overlay is disabled to avoid rendering a fallback quad.");
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

        private string ResolveInfluenceRadiusShaderName()
        {
            return GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                ? ResolveInfluenceRadiusShaderName3D()
                : ResolveInfluenceRadiusShaderName2D();
        }

        private string ResolveInfluenceRadiusShaderName2D()
            => _visualSettingsProvider?.InfluenceRadiusShaderName2D ?? "Moyva/2D/InfluenceRadius";

        private string ResolveInfluenceRadiusShaderName3D()
            => _visualSettingsProvider?.InfluenceRadiusShaderName3D ?? "Moyva/3D/InfluenceRadiusExistingMeshOverlay";

        private bool ResolveUseInfluenceRadiusOverlay()
            => _visualSettingsProvider?.UseInfluenceRadiusOverlay ?? true;

        private float ResolveInfluenceRadiusFillAlpha()
            => Mathf.Clamp01(_visualSettingsProvider?.InfluenceRadiusFillAlpha ?? 0.055f);

        private float ResolveInfluenceRadiusBorderWidth()
            => Mathf.Max(0f, _visualSettingsProvider?.InfluenceRadiusBorderWidth ?? 0.5f);

        private int ResolveBuildingLayerMinSortingOrder()
            => _visualSettingsProvider?.BuildingLayerMinSortingOrder ?? 5;

        private string ResolvePreviewRootName()
            => _visualSettingsProvider?.PreviewRootName ?? "ConstructionPreviewRoot";

        private string ResolvePlacedRootName()
            => _visualSettingsProvider?.PlacedRootName ?? "PlayerBuildingsRoot";

        private string ResolveRadiusRootName()
            => _visualSettingsProvider?.RadiusRootName ?? "ConstructionRadiusRoot";

        /// <summary>
        /// Позиціонує, масштабує та вмикає зону впливу.
        /// localScale = (2r+1) × (2r+1) → кожен юніт = один тайл.
        /// Ring width, dash length and gap length задаються у world-space,
        /// щоб пунктир мав однакову довжину незалежно від радіуса.
        /// </summary>
        private void DrawInfluenceRadius(
            GameObject go, MeshRenderer mr, Material mat,
            Vector2Int center, int radius)
        {
            if (mat == null || !ResolveUseInfluenceRadiusOverlay())
            {
                SetRadiusVisible(mr, false);
                return;
            }

            if (GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
            {
                DrawInfluenceRadiusMeshOverlay(mr, mat, center, radius);
                return;
            }

            if (go == null || mr == null)
                return;

            float size = radius * 2f + 1f;
            go.transform.position = _terrainAlignmentService.ResolveWorldPosition(center, 0.05f);
            go.transform.rotation = _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? Quaternion.Euler(90f, 0f, 0f)
                : Quaternion.identity;
            go.transform.localScale = new Vector3(size, size, 1f);

            var white = new Color(1f, 1f, 1f, 0.95f);

            mat.SetColor("_Color", white);
            mat.SetColor("_FillColor", new Color(1f, 1f, 1f, ResolveInfluenceRadiusFillAlpha()));
            mat.SetFloat("_BorderWidth", ResolveInfluenceRadiusBorderWidth());

            mr.enabled = true;
        }

        private void DrawInfluenceRadiusMeshOverlay(MeshRenderer radiusRenderer, Material mat, Vector2Int center, int radius)
        {
            if (radiusRenderer != null)
                radiusRenderer.enabled = false;

            var state = radiusRenderer == _inspectionRadiusMR
                ? _inspectionRadiusOverlay
                : _previewRadiusOverlay;

            Vector3 centerWorld = _gridProjection.GridToWorld(center, 0f, 0f);
            float halfExtent = Mathf.Max(0, radius) + 0.5f;

            state.Active = true;
            state.Center = center;
            state.Radius = Mathf.Max(0, radius);
            state.Material = mat;
            state.Bounds = new Bounds(
                new Vector3(centerWorld.x, 0f, centerWorld.z),
                new Vector3(halfExtent * 2f, 2048f, halfExtent * 2f));

            mat.SetVector("_CenterXZ", new Vector4(centerWorld.x, centerWorld.z, 0f, 0f));
            mat.SetFloat("_HalfExtent", halfExtent);
            mat.SetColor("_Color", new Color(1f, 1f, 1f, 0.95f));
            mat.SetColor("_FillColor", new Color(1f, 1f, 1f, ResolveInfluenceRadiusFillAlpha()));
            mat.SetFloat("_BorderWidth", ResolveInfluenceRadiusBorderWidth());

            RebuildInfluenceRadiusMeshOverlayRenderers(state);
        }

        private void RebuildInfluenceRadiusMeshOverlayRenderers(InfluenceRadiusMeshOverlayState state)
        {
            state.Renderers.Clear();
            if (!state.Active)
                return;

            var renderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (!IsInfluenceRadiusMeshOverlayCandidate(renderer, state.Bounds))
                    continue;

                state.Renderers.Add(renderer);
            }
        }

        private bool IsInfluenceRadiusMeshOverlayCandidate(MeshRenderer renderer, Bounds influenceBounds)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                return false;

            if (_radiusRoot != null && renderer.transform.IsChildOf(_radiusRoot))
                return false;

            if (!renderer.bounds.Intersects(influenceBounds))
                return false;

            var meshFilter = renderer.GetComponent<MeshFilter>();
            return meshFilter != null && meshFilter.sharedMesh != null;
        }

        private void DrawInfluenceRadiusMeshOverlay(InfluenceRadiusMeshOverlayState state)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                || state == null
                || !state.Active
                || state.Material == null)
            {
                return;
            }

            for (int i = state.Renderers.Count - 1; i >= 0; i--)
            {
                var renderer = state.Renderers[i];
                if (!IsInfluenceRadiusMeshOverlayCandidate(renderer, state.Bounds))
                {
                    state.Renderers.RemoveAt(i);
                    continue;
                }

                var meshFilter = renderer.GetComponent<MeshFilter>();
                Mesh mesh = meshFilter.sharedMesh;
                int subMeshCount = Mathf.Max(1, mesh.subMeshCount);
                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    Graphics.DrawMesh(
                        mesh,
                        meshFilter.transform.localToWorldMatrix,
                        state.Material,
                        renderer.gameObject.layer,
                        null,
                        subMeshIndex);
                }
            }
        }

        private void SetRadiusVisible(MeshRenderer mr, bool visible)
        {
            if (GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
            {
                if (mr == _previewRadiusMR)
                    SetInfluenceRadiusMeshOverlayVisible(_previewRadiusOverlay, visible);
                else if (mr == _inspectionRadiusMR)
                    SetInfluenceRadiusMeshOverlayVisible(_inspectionRadiusOverlay, visible);

                if (mr != null)
                    mr.enabled = false;
                return;
            }

            if (mr != null)
                mr.enabled = visible;
        }

        private static void SetInfluenceRadiusMeshOverlayVisible(InfluenceRadiusMeshOverlayState state, bool visible)
        {
            if (state == null)
                return;

            if (visible)
                return;

            state.Active = false;
            state.Renderers.Clear();
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
                    _styleService.ApplySolidStyle(placed);
            }

            _demolitionPreviewPositions.Clear();
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
