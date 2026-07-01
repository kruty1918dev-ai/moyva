using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TileWorldCreatorManager))]
    [HideMonoScript]
    public sealed class FogOfWarVolumeController : MonoBehaviour
    {
        private const string GeneratedRootName = "FogOfWar_GeneratedVolume";

        [TitleGroup("Settings")]
        [Required]
        [ValidateInput(nameof(HasSettings), "Assign FogOfWarSettings.")]
        [SerializeField] private FogOfWarSettings _settings;

        [TitleGroup("Runtime Overrides")]
        [SerializeField] private bool _overrideUpdateMode;

        [TitleGroup("Runtime Overrides")]
        [ShowIf(nameof(_overrideUpdateMode))]
        [SerializeField] private FogVolumeUpdateMode _updateMode = FogVolumeUpdateMode.DebouncePerFrame;

        [TitleGroup("Runtime Overrides")]
        [ShowIf(nameof(UsesIntervalUpdate))]
        [MinValue(0.02f)]
        [SerializeField] private float _rebuildIntervalSeconds = 0.1f;

        [TitleGroup("Runtime Overrides")]
        [SerializeField] private bool _overrideCellSize;

        [TitleGroup("Runtime Overrides")]
        [ShowIf(nameof(_overrideCellSize))]
        [MinValue(0.001f)]
        [SerializeField] private float _cellSizeOverride = 1f;

        [TitleGroup("Runtime Overrides")]
        [MinValue(0f)]
        [SerializeField] private float _additionalTopClearance;

        [TitleGroup("Debug Logging")]
        [SerializeField] private bool _logBuildSummary = true;

        [TitleGroup("Debug Logging")]
        [ShowIf(nameof(_logBuildSummary))]
        [SerializeField] private bool _logEveryVolumeUpdate;

        [TitleGroup("Debug Logging")]
        [SerializeField] private bool _logValidationWarnings = true;

        [TitleGroup("Startup Fallback")]
        [SerializeField] private bool _revealStartupFallbackArea = true;

        [TitleGroup("Startup Fallback")]
        [ShowIf(nameof(_revealStartupFallbackArea))]
        [SerializeField] private bool _teleportMainCameraToStartupFallback = true;

        [TitleGroup("Startup Fallback")]
        [ShowIf(nameof(_revealStartupFallbackArea))]
        [MinValue(1)]
        [SerializeField] private int _startupFallbackRevealRadiusOverride;

        [TitleGroup("Validation")]
        [ShowInInspector]
        [ReadOnly]
        [MultiLineProperty(7)]
        [PropertyOrder(100)]
        private string ValidationSummary => BuildValidationSummary();

        private IFogVisualUpdater _visualUpdater;
        private TileWorldCreatorManager _tileWorldCreatorManager;
        private bool _loggedAwake;
        private bool _loggedConstruct;
        private bool _loggedRegisterWithoutUpdater;
        public FogOfWarSettings Settings => _settings;
        public TileWorldCreatorManager TileWorldCreatorManager => ResolveFogManager();

        public FogVolumeUpdateMode EffectiveUpdateMode => _overrideUpdateMode
            ? _updateMode
            : (_settings != null ? _settings.Volume.UpdateMode : FogVolumeUpdateMode.DebouncePerFrame);

        public float EffectiveRebuildIntervalSeconds => Mathf.Max(
            0.02f,
            _overrideUpdateMode
                ? _rebuildIntervalSeconds
                : (_settings != null ? _settings.Volume.RebuildIntervalSeconds : 0.1f));

        public float AdditionalTopClearance => Mathf.Max(0f, _additionalTopClearance);
        public bool LogBuildSummary => _logBuildSummary;
        public bool LogEveryVolumeUpdate => _logEveryVolumeUpdate;
        public bool LogValidationWarnings => _logValidationWarnings;

        private bool UsesIntervalUpdate => EffectiveUpdateMode == FogVolumeUpdateMode.Interval;

        private bool CanRequestRuntimeRebuild
            => Application.isPlaying
                && _visualUpdater is FogOfWarVolumeUpdater
                && _settings != null
                && ResolveFogManager() != null;

        private void Awake()
        {
            LogLifecycleOnce(ref _loggedAwake, "Awake", $"settings={(_settings != null ? _settings.name : "null")}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}, clearPreview={(_settings != null && _settings.Volume.ClearPreviewOnRuntimeStart)}");

            if (_settings != null && _settings.Volume.ClearPreviewOnRuntimeStart)
                ClearGeneratedFogOutput();
        }

        [Inject]
        private void Construct([InjectOptional] IFogVisualUpdater visualUpdater)
        {
            _visualUpdater = visualUpdater;
            LogLifecycleOnce(ref _loggedConstruct, "Construct", $"visualUpdater={(visualUpdater != null ? visualUpdater.GetType().Name : "null")}, settings={(_settings != null ? _settings.name : "null")}, manager={(ResolveFogManager() != null ? ResolveFogManager().name : "null")}");
            RegisterWithUpdater();
        }

        private void OnEnable()
        {
            RegisterWithUpdater();
        }

        private void Start()
        {
            RegisterWithUpdater();
            RequestStartupBuildIfGameplayFogIsNotReady();
        }

        private void OnDisable()
        {
            if (_visualUpdater is FogOfWarVolumeUpdater volumeUpdater)
                volumeUpdater.DetachController(this);
        }

        private void OnValidate()
        {
            _rebuildIntervalSeconds = Mathf.Max(0.02f, _rebuildIntervalSeconds);
            _cellSizeOverride = Mathf.Max(0.001f, _cellSizeOverride);
            _additionalTopClearance = Mathf.Max(0f, _additionalTopClearance);
        }

        [TitleGroup("Preview")]
        [Button("Build Preview From Scene Grid")]
        [DisableInPlayMode]
        private void BuildPreviewFromSceneGrid()
        {
            if (_settings == null || ResolveFogManager() == null)
                return;

            ClearGeneratedFogOutput();
            var context = CreatePreviewContext();
            var updater = new FogOfWarVolumeUpdater(_settings);
            updater.AttachController(this);
            updater.Initialize(context.Width, context.Height, context);
            updater.RebuildFullVisual(new PreviewFogService(context.Width, context.Height));
            updater.Tick();
        }

        [TitleGroup("Preview")]
        [Button("Clear Preview")]
        private void ClearGeneratedFogOutput()
        {
            var manager = ResolveFogManager();
            if (manager == null)
                return;

            for (int i = manager.transform.childCount - 1; i >= 0; i--)
                DestroyGeneratedObject(manager.transform.GetChild(i).gameObject);

            if (manager.configuration != null
                && manager.configuration.name.StartsWith("FogOfWar_", System.StringComparison.Ordinal))
            {
                DestroyGeneratedObject(manager.configuration);
                manager.configuration = null;
            }
        }

        [TitleGroup("Runtime Actions")]
        [Button("Rebuild Fog Volume")]
        [EnableIf(nameof(CanRequestRuntimeRebuild))]
        private void RebuildFogVolume()
        {
            if (_visualUpdater is FogOfWarVolumeUpdater volumeUpdater)
                volumeUpdater.RequestFullRebuildFromController(this);
        }

        public float ResolveCellSize(float worldCellSize)
        {
            if (_overrideCellSize)
                return Mathf.Max(0.001f, _cellSizeOverride);

            if (_settings != null && !_settings.Volume.UseWorldCellSize)
                return Mathf.Max(0.001f, _settings.Volume.CellSizeOverride);

            return worldCellSize > 0.0001f ? worldCellSize : 1f;
        }

        private TileWorldCreatorManager ResolveFogManager()
        {
            if (_tileWorldCreatorManager == null)
                _tileWorldCreatorManager = GetComponent<TileWorldCreatorManager>();

            return _tileWorldCreatorManager;
        }

        private void RegisterWithUpdater()
        {
            if (!isActiveAndEnabled)
                return;

            if (_visualUpdater is not FogOfWarVolumeUpdater volumeUpdater)
            {
                LogLifecycleOnce(ref _loggedRegisterWithoutUpdater, "RegisterWithUpdater skipped", $"visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}");
                return;
            }

            volumeUpdater.AttachController(this);
        }

        private void RequestStartupBuildIfGameplayFogIsNotReady()
        {
            if (_settings == null || _visualUpdater is not FogOfWarVolumeUpdater volumeUpdater)
                return;

            var context = CreatePreviewContext();
            if (_revealStartupFallbackArea && (_logBuildSummary || _logValidationWarnings))
            {
                Debug.Log(
                    "[FogOfWarVolumeController] Runtime startup fallback reveal is deferred. " +
                    "The fog volume builds immediately, but the visible startup area is now expected to come from bootstrap/world spawn logic so gameplay fog, camera focus, and construction rules stay synchronized.",
                    this);
            }

            volumeUpdater.RequestStartupBuildFromController(this, context);
        }

        private void LogLifecycleOnce(ref bool logged, string stage, string details)
        {
            if (logged || !(_logBuildSummary || _logValidationWarnings))
                return;

            logged = true;
            Debug.Log($"[FogOfWarVolumeController] {stage}: object='{name}', active={gameObject.activeInHierarchy}, enabled={enabled}, {details}.", this);
        }

        private Vector2Int PickStartupFallbackCenter(FogWorldVisualContext context)
        {
            int width = Mathf.Max(1, context.Width);
            int height = Mathf.Max(1, context.Height);
            int radius = ResolveStartupFallbackRadius(context);
            int margin = Mathf.Clamp(
                Mathf.Max(_settings.StartupFallbackMinMarginFromBorder, radius),
                0,
                Mathf.Max(0, Mathf.Min(width, height) / 2 - 1));

            int minX = Mathf.Clamp(margin, 0, width - 1);
            int maxX = Mathf.Clamp(width - 1 - margin, minX, width - 1);
            int minY = Mathf.Clamp(margin, 0, height - 1);
            int maxY = Mathf.Clamp(height - 1 - margin, minY, height - 1);

            int seed = unchecked(width * 73856093 ^ height * 19349663 ^ Mathf.RoundToInt(context.CellSize * 1000f));
            var random = new System.Random(seed);
            var center = new Vector2Int(random.Next(minX, maxX + 1), random.Next(minY, maxY + 1));
            Debug.Log($"[FogOfWarVolumeController] Startup fallback reveal picked center={center}, radius={radius}, margin={margin}, map={width}x{height}.", this);
            return center;
        }

        private int ResolveStartupFallbackRadius(FogWorldVisualContext context)
        {
            if (_startupFallbackRevealRadiusOverride > 0)
                return Mathf.Max(1, _startupFallbackRevealRadiusOverride);

            return Mathf.Max(1, _settings != null ? _settings.StartupFallbackRevealRadius : Mathf.Max(1, Mathf.Min(context.Width, context.Height) / 5));
        }

        private void TeleportMainCameraToStartupFallback(FogWorldVisualContext context, Vector2Int center)
        {
            if (!_teleportMainCameraToStartupFallback)
                return;

            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                Debug.LogWarning("[FogOfWarVolumeController] Startup fallback camera teleport skipped: MainCamera was not found.", this);
                return;
            }

            Vector3 focus = ResolveStartupFallbackWorldPoint(context, center);
            Vector3 forward = camera.transform.forward.sqrMagnitude > 0.0001f
                ? camera.transform.forward.normalized
                : Vector3.forward;
            float distance = ResolveCameraPlaneDistance(camera, focus);
            camera.transform.position = focus - forward * distance;
            Debug.Log($"[FogOfWarVolumeController] Startup fallback camera teleported to center={center}, focus={focus}, distance={distance:0.###}.", this);
        }

        private Vector3 ResolveStartupFallbackWorldPoint(FogWorldVisualContext context, Vector2Int center)
        {
            float cellSize = ResolveCellSize(context.CellSize);
            Vector3 origin = context.HasMapWorldBounds
                ? new Vector3(context.MapWorldBounds.min.x + cellSize * 0.5f, transform.position.y, context.MapWorldBounds.min.z + cellSize * 0.5f)
                : transform.position;
            float height = ResolveStartupFallbackHeight(context, center);
            return new Vector3(origin.x + center.x * cellSize, transform.position.y + height, origin.z + center.y * cellSize);
        }

        private float ResolveStartupFallbackHeight(FogWorldVisualContext context, Vector2Int center)
        {
            if (context.HeightMap != null
                && center.x >= 0
                && center.y >= 0
                && center.x < context.HeightMap.GetLength(0)
                && center.y < context.HeightMap.GetLength(1))
            {
                return context.HeightMap[center.x, center.y];
            }

            if (context.TerrainLevelMap != null
                && center.x >= 0
                && center.y >= 0
                && center.x < context.TerrainLevelMap.GetLength(0)
                && center.y < context.TerrainLevelMap.GetLength(1))
            {
                float step = _settings != null ? Mathf.Max(0.001f, _settings.Volume.TerrainLevelHeightStep) : 1f;
                return Mathf.Max(0, context.TerrainLevelMap[center.x, center.y]) * step;
            }

            return 0f;
        }

        private static float ResolveCameraPlaneDistance(UnityEngine.Camera camera, Vector3 focus)
        {
            if (camera == null)
                return 20f;

            float distance = Vector3.Distance(camera.transform.position, focus);
            if (distance > 0.1f && !float.IsNaN(distance) && !float.IsInfinity(distance))
                return distance;

            return camera.orthographic ? Mathf.Max(10f, camera.orthographicSize * 2f) : 20f;
        }

        private FogWorldVisualContext CreatePreviewContext()
        {
            var sourceManager = FindSceneSourceTileWorldCreatorManager();
            var sourceConfiguration = sourceManager != null ? sourceManager.configuration : null;
            if (sourceConfiguration == null)
                return CreateFallbackPreviewContext();

            int width = Mathf.Max(1, sourceConfiguration.width);
            int height = Mathf.Max(1, sourceConfiguration.height);
            float cellSize = Mathf.Max(0.001f, sourceConfiguration.cellSize);
            Bounds bounds = CreateGridBounds(sourceManager.transform, width, height, cellSize);
            float[,] heightMap = BuildPreviewHeightMap(sourceConfiguration, width, height);

            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                cellSize,
                true,
                bounds,
                heightMap,
                null);
        }

        private FogWorldVisualContext CreateFallbackPreviewContext()
        {
            int width = Mathf.Max(1, _settings != null ? _settings.Volume.PreviewFallbackWidth : 16);
            int height = Mathf.Max(1, _settings != null ? _settings.Volume.PreviewFallbackHeight : 16);
            Bounds bounds = CreateGridBounds(transform, width, height, 1f);
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f,
                true,
                bounds,
                null,
                null);
        }

        private TileWorldCreatorManager FindSceneSourceTileWorldCreatorManager()
        {
            var ownManager = ResolveFogManager();
            var managers = Object.FindObjectsByType<TileWorldCreatorManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < managers.Length; i++)
            {
                var manager = managers[i];
                if (manager != null && manager != ownManager && manager.configuration != null)
                    return manager;
            }

            return null;
        }

        private static float[,] BuildPreviewHeightMap(Configuration configuration, int width, int height)
        {
            var heightMap = new float[width, height];
            if (configuration?.blueprintLayerFolders == null)
                return heightMap;

            for (int folderIndex = 0; folderIndex < configuration.blueprintLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.blueprintLayerFolders[folderIndex];
                if (folder?.blueprintLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.blueprintLayers.Count; layerIndex++)
                {
                    var layer = folder.blueprintLayers[layerIndex];
                    if (layer == null)
                        continue;

                    float layerHeight = layer.defaultLayerHeight + ResolveBuildLayerYOffset(configuration, layer.guid);
                    var positions = layer.GetAllCellPositions(new HashSet<Vector2>());
                    foreach (var position in positions)
                    {
                        int x = Mathf.RoundToInt(position.x);
                        int y = Mathf.RoundToInt(position.y);
                        if (x >= 0 && x < width && y >= 0 && y < height)
                            heightMap[x, y] = Mathf.Max(heightMap[x, y], layerHeight);
                    }
                }
            }

            return heightMap;
        }

        private static float ResolveBuildLayerYOffset(Configuration configuration, string blueprintGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrEmpty(blueprintGuid))
                return 0f;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is TilesBuildLayer buildLayer
                        && buildLayer.assignedBlueprintLayerGuid == blueprintGuid)
                    {
                        return buildLayer.layerYOffset;
                    }
                }
            }

            return 0f;
        }

        private static Bounds CreateGridBounds(Transform root, int width, int height, float cellSize)
        {
            float halfCell = cellSize * 0.5f;
            Vector3 localMin = new Vector3(-halfCell, 0f, -halfCell);
            Vector3 localMax = new Vector3(
                (width - 1) * cellSize + halfCell,
                1f,
                (height - 1) * cellSize + halfCell);

            Vector3 worldMin = root != null ? root.TransformPoint(localMin) : localMin;
            Vector3 worldMax = worldMin;
            Encapsulate(root != null ? root.TransformPoint(new Vector3(localMax.x, localMin.y, localMin.z)) : new Vector3(localMax.x, localMin.y, localMin.z), ref worldMin, ref worldMax);
            Encapsulate(root != null ? root.TransformPoint(new Vector3(localMin.x, localMax.y, localMin.z)) : new Vector3(localMin.x, localMax.y, localMin.z), ref worldMin, ref worldMax);
            Encapsulate(root != null ? root.TransformPoint(new Vector3(localMin.x, localMin.y, localMax.z)) : new Vector3(localMin.x, localMin.y, localMax.z), ref worldMin, ref worldMax);
            Encapsulate(root != null ? root.TransformPoint(localMax) : localMax, ref worldMin, ref worldMax);
            return new Bounds((worldMin + worldMax) * 0.5f, worldMax - worldMin);
        }

        private static void Encapsulate(Vector3 point, ref Vector3 min, ref Vector3 max)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        private static void DestroyGeneratedObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        private bool HasSettings(FogOfWarSettings settings)
            => settings != null;

        private string BuildValidationSummary()
        {
            if (_settings == null)
                return "Missing FogOfWarSettings.";

            if (ResolveFogManager() == null)
                return "Missing TileWorldCreatorManager on the same GameObject.";

            if (Object.FindObjectsByType<FogOfWarInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
                return "Missing FogOfWarInstaller in scene. Zenject bindings are still required for fog service/save/visibility.";

            if (!HasConfiguredFogPreset(_settings.Volume.Unexplored) && !HasConfiguredFogPreset(_settings.Volume.Explored))
                return "No enabled fog state has a valid dual-grid TilePreset.";

            return "Ready: this component follows generated world grid/height data and builds fog as a TWC dual-grid volume.";
        }

        private static bool HasConfiguredFogPreset(FogVolumeStateTileSettings state)
        {
            if (state == null || !state.Enabled || state.TileVariants == null)
                return false;

            for (int i = 0; i < state.TileVariants.Count; i++)
            {
                var variant = state.TileVariants[i];
                if (variant != null
                    && variant.Preset != null
                    && FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }

        private sealed class PreviewFogService : IFogOfWarService
        {
            private readonly int _width;
            private readonly int _height;

            public PreviewFogService(int width, int height)
            {
                _width = Mathf.Max(1, width);
                _height = Mathf.Max(1, height);
            }

            public void Initialize(int width, int height) { }
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position) => FogStateType.Unexplored;
            public bool IsVisible(Vector2Int position) => false;
            public bool IsExplored(Vector2Int position) => false;
            public bool[,] GetExploredSnapshot() => new bool[_width, _height];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }
    }
}
