using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// На старті нової гри обирає випадкову точку на мапі,
    /// рівномірно розкриває круг туману навколо неї (імітація стартової позиції)
    /// та миттєво переміщує камеру туди.
    ///
    /// При завантаженні збереження перевіряє, що туман має валідну видиму ділянку.
    /// </summary>
    internal sealed class StartingPositionInitializer : IInitializable, IDisposable
    {
        private const string StartVisionAnchorId = "bootstrap-start-vision-anchor";
        private const string StartRevealAnchorId = "bootstrap-start-vision-anchor-initial";

        private readonly IFogOfWarService _fogOfWarService;
        private readonly ISaveService _saveService;
        private readonly SignalBus _signalBus;
        private readonly StartingPositionInitializerSettings _settings;
        private readonly BootstrapStartingPositionState _startingPositionState;
        private readonly ICameraMovement _cameraMovement;

#pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private IPathfinder _pathfinder;
        [InjectOptional] private IUnitService _unitService;
        [InjectOptional] private IGridProjection _gridProjection;
        [InjectOptional] private ICameraZoom _cameraZoom;
        [InjectOptional] private MoyvaProjectSettingsSO _projectSettings;
        [InjectOptional] private UnityEngine.Camera _camera;
        [InjectOptional] private CameraSettingsSO _cameraSettings;
#pragma warning restore CS0649

        private bool _startAnchorRegistered;
        private int _registeredStartAnchorCount;
        private bool _startLogicApplied;
        private bool _startRevealApplied;
        private bool _startupCameraTeleported;
        private Vector2Int _appliedStartRevealCenter;
        private int _appliedStartRevealWidth;
        private int _appliedStartRevealHeight;
        private bool _hasPendingWorldGeneratedSignal;
        private WorldGeneratedDataSignal _pendingWorldGeneratedSignal;

        public StartingPositionInitializer(
            IFogOfWarService fogOfWarService,
            ISaveService saveService,
            SignalBus signalBus,
            StartingPositionInitializerSettings settings,
            BootstrapStartingPositionState startingPositionState,
            ICameraMovement cameraMovement)
        {
            _fogOfWarService = fogOfWarService;
            _saveService = saveService;
            _signalBus = signalBus;
            _settings = settings;
            _startingPositionState = startingPositionState;
            _cameraMovement = cameraMovement;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            if (signal.Assignments == null || signal.Assignments.Length == 0)
                return;

            _startingPositionState.Set(signal.Assignments);

            if (!_hasPendingWorldGeneratedSignal)
                return;

            if (_startLogicApplied)
            {
                ReapplyStartRevealIfNeeded(_pendingWorldGeneratedSignal);
                return;
            }

            TryApplyStartLogic();
        }

        // ─── Основна логіка ───────────────────────────────────────────────────

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            _pendingWorldGeneratedSignal = signal;
            _hasPendingWorldGeneratedSignal = true;

            TryApplyStartLogic();
        }

        private void TryApplyStartLogic()
        {
            if (_startLogicApplied || !_hasPendingWorldGeneratedSignal)
                return;

            var signal = _pendingWorldGeneratedSignal;

            // Якщо є збереження і автозавантаження ввімкнено —
            // туман відновить FogOfWarSaveModule. Якщо snapshot битий, робимо repair.
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
            {
                RepairLoadedFogIfNeeded(signal);
                TeleportMainCamera(ResolveStartupCameraTarget(signal.Width, signal.Height), signal);
                _startLogicApplied = true;
                return;
            }

            if (ShouldComputeHostStartPositions() && !_startingPositionState.IsSet)
            {
                List<Vector2Int> startPositions = PickStartingPositions(signal);
                Vector2Int startPos = startPositions.Count > 0
                    ? startPositions[0]
                    : PickStartingPosition(signal.Width, signal.Height);

                // Зберігаємо позиції, щоб BootstrapGameInitializer міг розмістити замок на першій з них.
                if (startPositions.Count > 0)
                    _startingPositionState.Set(BuildSpawnAssignments(startPositions));
                else
                    _startingPositionState.Set(startPos);

                if (_startingPositionState.SpawnAssignments.Count > 0)
                {
                    _signalBus.Fire(new WorldSpawnPositionsSignal
                    {
                        Assignments = CopySpawnAssignments(_startingPositionState.SpawnAssignments),
                    });
                }

                if (_startLogicApplied)
                    return;
            }

            if (!CanRunStartLogic() || !_startingPositionState.IsSet)
                return;

            ApplyStartReveal(signal, teleportCamera: true);
        }

        private void ApplyStartReveal(WorldGeneratedDataSignal signal, bool teleportCamera)
        {
            Vector2Int revealCenter = ResolveLocalRevealCenter(signal.Width, signal.Height);
            bool revealChanged = !_startRevealApplied
                || _appliedStartRevealWidth != signal.Width
                || _appliedStartRevealHeight != signal.Height
                || _appliedStartRevealCenter != revealCenter;

            if (revealChanged)
            {
                RevealStartingAreas(signal.Width, signal.Height, revealCenter);
                RegisterStartupCoreVisibility(signal.Width, signal.Height, revealCenter);
                _startRevealApplied = true;
                _appliedStartRevealWidth = signal.Width;
                _appliedStartRevealHeight = signal.Height;
                _appliedStartRevealCenter = revealCenter;
            }

            if (teleportCamera && !_startupCameraTeleported)
            {
                TeleportMainCamera(ResolveStartupCameraTarget(signal.Width, signal.Height), signal);
                _startupCameraTeleported = true;
            }

            _startLogicApplied = true;
            Debug.Log($"[Bootstrap] Стартова позиція: {revealCenter}. Туман розкрито, камеру переміщено.");
        }

        private void ReapplyStartRevealIfNeeded(WorldGeneratedDataSignal signal)
        {
            Vector2Int revealCenter = ResolveLocalRevealCenter(signal.Width, signal.Height);
            if (_startRevealApplied
                && _appliedStartRevealWidth == signal.Width
                && _appliedStartRevealHeight == signal.Height
                && _appliedStartRevealCenter == revealCenter)
            {
                return;
            }

            ApplyStartReveal(signal, teleportCamera: !_startupCameraTeleported);
        }

        private void RegisterStartupCoreVisibility(int width, int height, Vector2Int revealCenter)
        {
            if (!_settings.keepCoreFullyVisible)
                return;

            if (_startAnchorRegistered)
                UnregisterStartVisionAnchors();

            int visibleRange = _settings.coreVisibleRadiusOverride > 0
                ? _settings.coreVisibleRadiusOverride
                : _settings.ResolveCoreVisibleRadius(width, height);
            _fogOfWarService.RegisterFixedVisionArea(ResolveStartVisionAnchorId(0), revealCenter, visibleRange, _settings.ResolveRevealShape());
            _startAnchorRegistered = true;
            _registeredStartAnchorCount = 1;
        }

        private void TeleportMainCamera(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (TryTeleportCameraToStartupFocus(startPos, signal))
                return;

            _cameraMovement.TeleportCamera(new Vector3(startPos.x, startPos.y, _settings.cameraZ));
        }

        private bool TryTeleportCameraToStartupFocus(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (_cameraMovement == null)
                return false;

            ApplyConfiguredStartupCameraPose();
            Vector3 focusPoint = ResolveStartupFocusPoint(startPos, signal);
            float distance = ResolveStartupCameraDistance();
            _cameraMovement.TeleportCameraToFocusPoint(focusPoint, distance);
            ApplyStartupCameraZoom(startPos, focusPoint, signal);
            return true;
        }

        private void ApplyConfiguredStartupCameraPose()
        {
            if (_camera == null)
                return;

            _camera.transform.rotation = Quaternion.Euler(ResolveStartupCameraEuler());
            bool usePerspective = ResolveUsePerspectiveStartupCamera();
            _camera.orthographic = !usePerspective;

            if (_camera.orthographic)
                _camera.orthographicSize = ResolveStartupOrthographicSize();
            else
                _camera.fieldOfView = ResolveStartupFieldOfView();
        }

        private Vector3 ResolveStartupCameraEuler()
        {
            GridProjectionMode projectionMode = ResolveProjectionMode();
            if (_projectSettings != null)
                return _projectSettings.Resolve3DCameraEuler(projectionMode);

            return projectionMode == GridProjectionMode.Orthographic3D
                ? (_cameraSettings != null ? _cameraSettings.orthographic3DEuler : new Vector3(90f, 0f, 0f))
                : (_cameraSettings != null ? _cameraSettings.isometric3DEuler : new Vector3(52f, 45f, 0f));
        }

        private bool ResolveUsePerspectiveStartupCamera()
        {
            bool autoOrthographic = _cameraSettings != null && _cameraSettings.ResolveUseOrthographicCameraIn3D();
            if (_projectSettings != null)
                return _projectSettings.ResolveUsePerspectiveCamera(autoOrthographic);

            return ResolveProjectionMode() == GridProjectionMode.Isometric3DPreview || !autoOrthographic;
        }

        private GridProjectionMode ResolveProjectionMode()
        {
            if (_gridProjection != null)
                return _gridProjection.ProjectionMode;

            return _projectSettings != null
                ? _projectSettings.DefaultProjectionMode
                : GridProjectionMode.Isometric3DPreview;
        }

        private float ResolveStartupFieldOfView()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DFieldOfView();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DFieldOfView() : 40f;
        }

        private float ResolveStartupOrthographicSize()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DOrthographicSize();

            return _cameraSettings != null ? _cameraSettings.ResolveDefault3DOrthographicSize() : 20f;
        }

        private Vector3 ResolveStartupFocusPoint(Vector2Int startPos, WorldGeneratedDataSignal signal)
        {
            if (_gridProjection != null)
                return _gridProjection.GridToWorld(startPos, ResolveHeight(signal, startPos));

            return new Vector3(startPos.x, startPos.y, 0f);
        }

        private float ResolveStartupCameraDistance()
        {
            if (_projectSettings != null)
                return _projectSettings.ResolveProject3DCameraDistance();

            if (_cameraSettings != null)
                return _cameraSettings.ResolveDefault3DCameraDistance();

            if (TryResolveCurrentCameraPlaneDistance(out float currentDistance))
                return currentDistance;

            return 35f;
        }

        private bool TryResolveCurrentCameraPlaneDistance(out float distance)
        {
            distance = 0f;
            if (_camera == null)
                return false;

            Vector3 normal = _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? Vector3.up
                : Vector3.forward;
            Vector3 direction = _camera.transform.forward;
            float denominator = Vector3.Dot(normal, direction);
            if (Mathf.Abs(denominator) <= 0.0001f)
                return false;

            distance = -Vector3.Dot(normal, _camera.transform.position) / denominator;
            return distance > 0.1f && !float.IsNaN(distance) && !float.IsInfinity(distance);
        }

        private void ApplyStartupCameraZoom(Vector2Int startPos, Vector3 focusPoint, WorldGeneratedDataSignal signal)
        {
            if (!ShouldEnsureStartupCameraShowsRevealedArea() || _cameraZoom == null || _camera == null)
                return;

            float radius = ResolveStartupCameraRadius(signal.Width, signal.Height) + ResolveStartupCameraPaddingTiles();
            Vector3[] corners = BuildStartupZoneCorners(startPos, focusPoint, radius, signal);
            if (_camera.orthographic)
            {
                float zoom = ResolveOrthographicZoomToFit(focusPoint, corners);
                _camera.orthographicSize = zoom;
                _cameraZoom.ForceZoomCamera(zoom);
                return;
            }

            float fieldOfView = ResolvePerspectiveFieldOfViewToFit(focusPoint, corners);
            _camera.fieldOfView = fieldOfView;
            _cameraZoom.ForceZoomCamera(fieldOfView);
        }

        private bool ShouldEnsureStartupCameraShowsRevealedArea()
        {
            return _projectSettings != null
                ? _projectSettings.EnsureStartupCameraShowsRevealedArea
                : _settings.ensureStartupCameraShowsRevealedArea;
        }

        private float ResolveStartupCameraPaddingTiles()
        {
            return _projectSettings != null
                ? _projectSettings.ResolveStartupCameraPaddingTiles()
                : Mathf.Max(0f, _settings.startupCameraPaddingTiles);
        }

        private int ResolveStartupCameraRadius(int width, int height)
        {
            MoyvaStartupCameraRadiusSource source = _projectSettings != null
                ? _projectSettings.StartupCameraRadiusSource
                : _settings.startupCameraRadiusSource;

            return source switch
            {
                MoyvaStartupCameraRadiusSource.CoreVisibleRadius => _settings.ResolveCoreVisibleRadius(width, height),
                MoyvaStartupCameraRadiusSource.ManualRadius => _projectSettings != null
                    ? _projectSettings.ResolveManualStartupCameraRadius()
                    : Mathf.Max(1, _settings.manualStartupCameraRadius),
                _ => _settings.ResolveRevealedRadius(width, height),
            };
        }

        private Vector3[] BuildStartupZoneCorners(Vector2Int startPos, Vector3 focusPoint, float radius, WorldGeneratedDataSignal signal)
        {
            if (_gridProjection == null)
            {
                return new[]
                {
                    focusPoint + new Vector3(-radius, -radius, 0f),
                    focusPoint + new Vector3(radius, -radius, 0f),
                    focusPoint + new Vector3(-radius, radius, 0f),
                    focusPoint + new Vector3(radius, radius, 0f),
                };
            }

            int tileRadius = Mathf.Max(1, Mathf.CeilToInt(radius));
            var min = ClampToMap(new Vector2Int(startPos.x - tileRadius, startPos.y - tileRadius), signal.Width, signal.Height);
            var max = ClampToMap(new Vector2Int(startPos.x + tileRadius, startPos.y + tileRadius), signal.Width, signal.Height);
            return new[]
            {
                ProjectStartupCorner(new Vector2Int(min.x, min.y), signal),
                ProjectStartupCorner(new Vector2Int(max.x, min.y), signal),
                ProjectStartupCorner(new Vector2Int(min.x, max.y), signal),
                ProjectStartupCorner(new Vector2Int(max.x, max.y), signal),
            };
        }

        private Vector3 ProjectStartupCorner(Vector2Int gridPosition, WorldGeneratedDataSignal signal)
            => _gridProjection.GridToWorld(gridPosition, ResolveHeight(signal, gridPosition));

        private float ResolveOrthographicZoomToFit(Vector3 focusPoint, Vector3[] corners)
        {
            float aspect = _camera != null && _camera.aspect > 0.0001f ? _camera.aspect : 1f;
            Quaternion worldToView = Quaternion.Inverse(_camera.transform.rotation);
            ResolveViewHalfExtents(focusPoint, corners, worldToView, out float halfWidth, out float halfHeight);
            float required = Mathf.Max(halfHeight, halfWidth / aspect, 0.1f);
            float minZoom = _cameraSettings != null ? _cameraSettings.ResolveMinZoom() : 0.1f;
            return Mathf.Max(minZoom, required);
        }

        private float ResolvePerspectiveFieldOfViewToFit(Vector3 focusPoint, Vector3[] corners)
        {
            float aspect = _camera != null && _camera.aspect > 0.0001f ? _camera.aspect : 1f;
            Quaternion worldToView = Quaternion.Inverse(_camera.transform.rotation);
            ResolveViewHalfExtents(focusPoint, corners, worldToView, out float halfWidth, out float halfHeight);
            float requiredHalfHeight = Mathf.Max(halfHeight, halfWidth / aspect, 0.01f);
            Vector3 localFocus = worldToView * (focusPoint - _camera.transform.position);
            float distance = Mathf.Max(0.1f, Mathf.Abs(localFocus.z));
            float requiredFov = Mathf.Atan(requiredHalfHeight / distance) * 2f * Mathf.Rad2Deg;
            float configuredFov = _projectSettings != null
                ? _projectSettings.ResolveProject3DFieldOfView()
                : (_cameraSettings != null ? _cameraSettings.ResolveDefault3DFieldOfView() : 45f);
            return Mathf.Clamp(Mathf.Max(configuredFov, requiredFov), 1f, 179f);
        }

        private static void ResolveViewHalfExtents(Vector3 focusPoint, Vector3[] corners, Quaternion worldToView, out float halfWidth, out float halfHeight)
        {
            halfWidth = 0.01f;
            halfHeight = 0.01f;
            if (corners == null)
                return;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 view = worldToView * (corners[i] - focusPoint);
                halfWidth = Mathf.Max(halfWidth, Mathf.Abs(view.x));
                halfHeight = Mathf.Max(halfHeight, Mathf.Abs(view.y));
            }
        }

        private static float ResolveHeight(WorldGeneratedDataSignal signal, Vector2Int position)
        {
            if (signal.HeightMap == null)
                return 0f;

            int width = signal.HeightMap.GetLength(0);
            int height = signal.HeightMap.GetLength(1);
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
                return 0f;

            return signal.HeightMap[position.x, position.y];
        }

        private Vector2Int ResolveStartupCameraTarget(int width, int height)
        {
            Vector2Int preferred = ResolvePreferredPlayerPosition(width, height);
            return ResolveVisibleCameraTarget(preferred, width, height);
        }

        private Vector2Int ResolvePreferredPlayerPosition(int width, int height)
        {
            if (TryGetClosestUnitPosition(ResolveRepairCenter(_fogOfWarService.GetExploredSnapshot(), width, height), out Vector2Int unitPosition))
                return ClampToMap(unitPosition, width, height);

            if (TryGetLocalSpawnPosition(out Vector2Int localSpawn))
                return ClampToMap(localSpawn, width, height);

            if (_startingPositionState.IsSet)
                return ClampToMap(_startingPositionState.StartPosition, width, height);

            return FindRepairCenter(_fogOfWarService.GetExploredSnapshot(), width, height);
        }

        private bool TryGetClosestUnitPosition(Vector2Int origin, out Vector2Int position)
        {
            position = default;
            var unitIds = _unitService?.GetAllUnitIds();
            if (unitIds == null || unitIds.Count == 0)
                return false;

            int bestDistance = int.MaxValue;
            foreach (string unitId in unitIds)
            {
                if (!_unitService.TryGetUnitPosition(unitId, out Vector2Int candidate))
                    continue;

                int distance = Mathf.Abs(candidate.x - origin.x) + Mathf.Abs(candidate.y - origin.y);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                position = candidate;
            }

            return bestDistance != int.MaxValue;
        }

        private Vector2Int ResolveVisibleCameraTarget(Vector2Int preferred, int width, int height)
        {
            Vector2Int clamped = ClampToMap(preferred, width, height);
            if (_fogOfWarService.IsVisible(clamped))
                return clamped;

            if (TryFindNearestVisibleTile(clamped, width, height, out Vector2Int visiblePosition))
            {
                Debug.LogWarning($"[Bootstrap] Камера мала стартувати над чорним туманом у {clamped}. Переміщено до найближчої видимої ділянки {visiblePosition}.");
                return visiblePosition;
            }

            if (TryFindNearestExploredTile(clamped, width, height, out Vector2Int exploredPosition))
            {
                Debug.LogWarning($"[Bootstrap] Видимих тайлів для старту камери не знайдено. Використано найближчу розвідану ділянку {exploredPosition}.");
                return exploredPosition;
            }

            return clamped;
        }

        private bool TryFindNearestVisibleTile(Vector2Int origin, int width, int height, out Vector2Int position)
            => TryFindNearestFogTile(origin, width, height, _fogOfWarService.IsVisible, out position);

        private bool TryFindNearestExploredTile(Vector2Int origin, int width, int height, out Vector2Int position)
            => TryFindNearestFogTile(origin, width, height, _fogOfWarService.IsExplored, out position);

        private static bool TryFindNearestFogTile(
            Vector2Int origin,
            int width,
            int height,
            System.Func<Vector2Int, bool> predicate,
            out Vector2Int position)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            origin = ClampToMap(origin, width, height);

            int maxRadius = width + height;
            for (int radius = 0; radius <= maxRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int dy = radius - Mathf.Abs(dx);
                    if (TryMatchFogTile(origin.x + dx, origin.y + dy, width, height, predicate, out position))
                        return true;

                    if (dy != 0 && TryMatchFogTile(origin.x + dx, origin.y - dy, width, height, predicate, out position))
                        return true;
                }
            }

            position = default;
            return false;
        }

        private static bool TryMatchFogTile(
            int x,
            int y,
            int width,
            int height,
            System.Func<Vector2Int, bool> predicate,
            out Vector2Int position)
        {
            position = default;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            var candidate = new Vector2Int(x, y);
            if (!predicate(candidate))
                return false;

            position = candidate;
            return true;
        }

        // ─── Вибір стартової точки ────────────────────────────────────────────

        private List<Vector2Int> PickStartingPositions(WorldGeneratedDataSignal signal)
        {
            int positionsCount = ResolveStartPositionCount();
            var positions = new List<Vector2Int>(positionsCount);
            int attempts = Mathf.Max(1, _settings.startCandidateAttempts);

            for (int positionIndex = 0; positionIndex < positionsCount; positionIndex++)
            {
                if (TryPickStartingPosition(signal, positions, attempts, out Vector2Int position))
                    positions.Add(position);
                else
                    Debug.LogWarning($"[Bootstrap] Не вдалось знайти стартову позицію #{positionIndex + 1} із заданими обмеженнями.");
            }

            if (positions.Count > 1)
                Debug.Log($"[Bootstrap] Host зарезервував стартові позиції: {string.Join(", ", positions)}");

            return positions;
        }

        private bool TryPickStartingPosition(
            WorldGeneratedDataSignal signal,
            IReadOnlyList<Vector2Int> existingPositions,
            int attempts,
            out Vector2Int position)
        {
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                Vector2Int candidate = PickStartingPosition(signal.Width, signal.Height);
                if (!IsValidStartHeight(signal, candidate))
                    continue;

                if (!HasRequiredDistance(candidate, existingPositions))
                    continue;

                position = candidate;
                return true;
            }

            for (int x = 0; x < signal.Width; x++)
            {
                for (int y = 0; y < signal.Height; y++)
                {
                    Vector2Int candidate = new Vector2Int(x, y);
                    if (IsInsideStartBounds(candidate, signal.Width, signal.Height) &&
                        IsValidStartHeight(signal, candidate) &&
                        HasRequiredDistance(candidate, existingPositions))
                    {
                        position = candidate;
                        return true;
                    }
                }
            }

            position = Vector2Int.zero;
            return false;
        }

        private Vector2Int PickStartingPosition(int width, int height)
        {
            if (width <= 0 || height <= 0)
                return Vector2Int.zero;

            int minSide = Mathf.Min(width, height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(_settings.relativeMarginFactor));
            int margin = Mathf.Max(_settings.minMarginFromBorder, relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, width - 1);
            int xMax = Mathf.Clamp(width - margin - 1, xMin, width - 1);
            int yMin = Mathf.Clamp(margin, 0, height - 1);
            int yMax = Mathf.Clamp(height - margin - 1, yMin, height - 1);

            int x = UnityEngine.Random.Range(xMin, xMax + 1);
            int y = UnityEngine.Random.Range(yMin, yMax + 1);
            return new Vector2Int(x, y);
        }

        private bool IsInsideStartBounds(Vector2Int position, int width, int height)
        {
            if (width <= 0 || height <= 0)
                return false;

            int minSide = Mathf.Min(width, height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(_settings.relativeMarginFactor));
            int margin = Mathf.Max(_settings.minMarginFromBorder, relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, width - 1);
            int xMax = Mathf.Clamp(width - margin - 1, xMin, width - 1);
            int yMin = Mathf.Clamp(margin, 0, height - 1);
            int yMax = Mathf.Clamp(height - margin - 1, yMin, height - 1);

            return position.x >= xMin && position.x <= xMax && position.y >= yMin && position.y <= yMax;
        }

        private bool IsValidStartHeight(WorldGeneratedDataSignal signal, Vector2Int position)
        {
            if (signal.HeightMap == null)
                return !_settings.requireHeightMapForStart;

            if (position.x < 0 || position.x >= signal.HeightMap.GetLength(0) ||
                position.y < 0 || position.y >= signal.HeightMap.GetLength(1))
            {
                return false;
            }

            float minHeight = Mathf.Min(_settings.startMinHeight, _settings.startMaxHeight);
            float maxHeight = Mathf.Max(_settings.startMinHeight, _settings.startMaxHeight);
            float height = signal.HeightMap[position.x, position.y];
            return height >= minHeight && height <= maxHeight;
        }

        private bool HasRequiredDistance(Vector2Int candidate, IReadOnlyList<Vector2Int> existingPositions)
        {
            if (existingPositions == null || existingPositions.Count == 0)
                return true;

            int minDistance = Mathf.Max(1, _settings.minAStarDistanceBetweenPlayers);
            for (int index = 0; index < existingPositions.Count; index++)
            {
                int distance = ResolveStartDistance(candidate, existingPositions[index]);
                if (distance < minDistance)
                    return false;
            }

            return true;
        }

        private int ResolveStartDistance(Vector2Int first, Vector2Int second)
        {
            if (_pathfinder != null)
            {
                List<Vector2Int> path = _pathfinder.FindPath(first, second);
                if (path != null && path.Count > 0)
                    return Mathf.Max(0, path.Count - 1);
            }

            return Mathf.CeilToInt(Vector2.Distance(first, second));
        }

        private int ResolveStartPositionCount()
        {
            int participantCount = _sessionManager?.Participants?.Count ?? 1;
            if (participantCount > 1 || IsMultiplayerHost())
                return Mathf.Max(participantCount, _settings.multiplayerStartSlots);

            if (GameLaunchContext.HasWorldSettings && GameLaunchContext.MaxPlayers > 1)
                return Mathf.Max(GameLaunchContext.MaxPlayers, _settings.multiplayerStartSlots);

            return 1;
        }


        /// <summary>
        /// У багатокористувацькій грі хост відповідає за вибір стартових позицій для всіх гравців. Якщо дані про сесію недоступні, припускаємо, що це не мультиплеєр або локальна гра, і дозволяємо застосувати логіку стартової позиції.
        /// </summary>
        /// <param name="positions">Список стартових позицій для призначення.</param>
        /// <returns>Масив призначень стартових позицій.</returns>
        private SpawnPositionAssignment[] BuildSpawnAssignments(IReadOnlyList<Vector2Int> positions)
        {

            var participants = _sessionManager?.Participants;  // Отримуємо список учасників сесії, якщо доступно. Якщо дані про сесію недоступні, participants буде null.             
            var assignments = new SpawnPositionAssignment[positions.Count]; // Створюємо масив призначень стартових позицій з розміром, що відповідає кількості позицій у списку. Кожен елемент масиву буде заповнений відповідним призначенням для кожної стартової позиції.
            int participantCount = participants?.Count ?? 0;                // Визначаємо кількість учасників сесії. Якщо дані про сесію недоступні, participantCount буде 0. Це дозволяє нам коректно обробляти випадки, коли гра не є багатокористувацькою або дані про сесію недоступні.
            int launchParticipantCount = GameLaunchContext.HasWorldSettings // Визначаємо кількість учасників, яку слід враховувати при призначенні стартових позицій. Якщо у контексті запуску є налаштування світу і максимальна кількість гравців більше 1, використовуємо це значення. Інакше, якщо дані про сесію недоступні або гра не є багатокористувацькою, вважаємо, що є лише 1 учасник (локальний гравець).
                ? Mathf.Max(1, GameLaunchContext.MaxPlayers)                // Якщо у контексті запуску є налаштування світу і максимальна кількість гравців більше 1, використовуємо це значення. Це дозволяє враховувати налаштування світу при визначенні кількості стартових позицій для призначення.
                : 1;


            // Проходимо по кожній стартовій позиції і створюємо відповідне призначення для кожного учасника. Якщо дані про сесію недоступні, припускаємо, що це не мультиплеєр або локальна гра, і дозволяємо застосувати логіку стартової позиції. У цьому випадку перша позиція буде призначена локальному гравцю, а інші позиції будуть призначені ботам (якщо їх кількість перевищує 1).
            for (int index = 0; index < positions.Count; index++)
            {
                // Ініціалізуємо змінні для зберігання ідентифікатора учасника та інформації про те, чи є він ботом. За замовчуванням, якщо дані про сесію недоступні, вважаємо, що це локальна гра, і перша позиція буде призначена локальному гравцю, а інші позиції будуть призначені ботам (якщо їх кількість перевищує 1).
                string participantId = string.Empty;
                bool isBot = false;

                // Якщо дані про сесію доступні і індекс позиції менший за кількість учасників, призначаємо позицію відповідному учаснику. Інакше, якщо індекс позиції дорівнює 0, призначаємо її локальному гравцю. Якщо індекс позиції менший за кількість учасників, призначаємо її боту з унікальним ідентифікатором. Це дозволяє коректно обробляти випадки, коли гра є багатокористувацькою або локальною, і забезпечує правильне призначення стартових позицій для кожного учасника.
                if (participants != null && index < participantCount)
                {
                    // Якщо дані про сесію доступні і індекс позиції менший за кількість учасників, призначаємо позицію відповідному учаснику. Це дозволяє коректно обробляти випадки, коли гра є багатокористувацькою, і забезпечує правильне призначення стартових позицій для кожного учасника на основі їх порядку у списку учасників.
                    participantId = participants[index].Identity?.PlayerId ?? string.Empty;
                    isBot = participants[index].IsBot;
                }
                else if (index == 0) // Якщо індекс позиції дорівнює 0, призначаємо її локальному гравцю. Це дозволяє коректно обробляти випадки, коли гра є локальною або дані про сесію недоступні, і забезпечує правильне призначення стартової позиції для локального гравця.       
                {
                    // Якщо індекс позиції дорівнює 0, призначаємо її локальному гравцю. Це дозволяє коректно обробляти випадки, коли гра є локальною або дані про сесію недоступні, і забезпечує правильне призначення стартової позиції для локального гравця. Ідентифікатор локального гравця визначається за допомогою методу ResolveLocalPlayerId(), який намагається отримать його з даних про сесію, а якщо це не вдається, використовує дефолтне значення "local-player". Це гарантує, що локальний гравець отримає унікальний ідентифікатор навіть у випадках, коли дані про сесію недоступні.
                    participantId = ResolveLocalPlayerId();
                }
                else if (index < launchParticipantCount) // Якщо індекс позиції менший за кількість учасників, призначаємо її боту з унікальним ідентифікатором. Це дозволяє коректно обробляти випадки, коли гра є багатокористувацькою або локальною, і забезпечує правильне призначення стартових позицій для ботів, якщо їх кількість перевищує 1. Ідентифікатор бота формируется как "bot-XX", где XX - это порядковый номер бота, начиная с 01. Это гарантирует, что каждый бот получит уникальный идентификатор, который легко отличить от идентификатора локального игрока и других участников.
                {
                    // якщо індекс позиції менший за кількість учасників, призначаємо її боту з унікальним ідентифікатором. Це дозволяє коректно обробляти випадки, коли гра є багатокористувацькою або локальною, і забезпечує правильне призначення стартових позицій для ботів, якщо їх кількість перевищує 1. Ідентифікатор бота формируется как "bot-XX", где XX - это порядковый номер бота, начиная с 01. Это гарантирует, что каждый бот получит уникальный идентификатор, который легко отличить от идентификатора локального игрока и других участников.
                    participantId = $"bot-{index:00}";
                    isBot = true;
                }

                // Створюємо нове призначення стартової позиції з визначеними значеннями і додаємо його до масиву призначень. Це дозволяє сформувати масив призначень стартових позицій, який буде використовуватися для розміщення гравців на мапі відповідно до їх ролі (локальний гравець або бот) та порядку у списку учасників.
                assignments[index] = new SpawnPositionAssignment
                {
                    SlotIndex = index,
                    ParticipantId = participantId,
                    IsBot = isBot,
                    Position = positions[index],
                };
            }


            // Повертаємо масив призначень стартових позицій, який містить інформацію про те, яка позиція призначена якому учаснику (локальному гравцю або боту) та їх ролі. Це дозволяє коректно розмістити гравців на мапі відповідно до їх ролі та порядку у списку учасників, забезпечуючи правильну логіку стартової позиції для багатокористувацької або локальної гри.
            return assignments;
        }






        /// <summary>
        /// У багатокористувацькій грі хост відповідає за вибір стартових позицій для всіх гравців. Якщо дані про сесію недоступні, припускаємо, що це не мультиплеєр або локальна гра, і дозволяємо застосувати логіку стартової позиції.
        /// </summary>
        /// <returns>Ідентифікатор локального гравця.</returns>
        private string ResolveLocalPlayerId()
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;
            if (!string.IsNullOrEmpty(localPlayerId))
                return localPlayerId;

            return "local-player";
        }




        /// <summary>
        /// Копіює масив SpawnPositionAssignment, щоб зберегти імунітет до зовнішніх змін після передачі сигналу.
        /// </summary>
        /// <param name="assignments">Масив стартових позицій для копіювання.</param>
        /// <returns>Копія масиву стартових позицій.</returns>
        private static SpawnPositionAssignment[] CopySpawnAssignments(IReadOnlyList<SpawnPositionAssignment> assignments)
        {
            // Якщо вхідний масив null або порожній, повертаємо порожній масив.
            var copy = new SpawnPositionAssignment[assignments.Count];

            // Копіюємо кожен елемент з вхідного масиву до нового масиву.
            for (int index = 0; index < assignments.Count; index++)
                copy[index] = assignments[index];

            // Повертаємо новий масив, який є копією вхідного масиву.
            return copy;
        }



        /// <summary>
        /// У багатокористувацькій грі хост відповідає за вибір стартових позицій для всіх гравців.
        /// </summary>
        /// <returns>Повертає true, якщо локальний гравець може виконати логіку стартової позиції.</returns>
        private bool CanRunStartLogic()
        {
            if (_startingPositionState.IsSet)
                return true;

            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost)
                return true;

            if (_sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
                return !IsMultiplayerLaunchContext();

            return false;
        }

        /// <summary>
        /// У багатокористувацькій грі хост відповідає за вибір стартових позицій для всіх гравців.
        /// </summary>
        /// <returns>Повертає true, якщо локальний гравець є хостом у багатокористувацькій грі.</returns>
        private bool IsMultiplayerHost()
        {
            // Якщо немає даних про сесію, припускаємо, 
            // що це не мультиплеєр або локальна гра, 
            // і дозволяємо застосувати логіку стартової позиції.
            return _sessionManager != null && _sessionManager.IsLocalPlayerHost;
        }

        // ─── Стартове кругле розкриття туману ─────────────────────────────────

        /// <summary>
        /// Рівномірно розкриває круглу ділянку туману навколо стартової позиції.
        /// Імітує стартову позицію на мапі, якщо гравець є локальним.
        /// </summary>
        private void RevealStartingAreas(int width, int height, Vector2Int center)
        {
            // Якщо стартова позиція не визначена, розкриваємо центр мапи.
            int radius = _settings.ResolveRevealedRadius(width, height);

            // Якщо є збереження і автозавантаження ввімкнено — стартова позиція вже розкрита 
            // FogOfWarSaveModule, додатково не розкриваємо.
            var shape = _settings.ResolveRevealShape();

            // Якщо стартова позиція виявиться у чорному тумані, 
            // камера буде переміщена до найближчої видимої ділянки. 
            // Щоб уникнути неприємного сюрпризу, 
            // розкриваємо стартову область до телепортації камери.
            Debug.Log($"[Bootstrap] Стартовий туман: center={center}, radius={radius}, shape={shape}, map={width}x{height}, scaled={_settings.useMapSizeScaledFog}.");

            // Навантажуємо згенерований snapshot, 
            // щоб гарантовано розкрити стартову позицію. 
            // Якщо є збереження з валідним snapshot, 
            // він уже завантажився через FogOfWarSaveModule і додатково не перезапишеться.
            var snapshot = BuildRevealSnapshot(width, height, center, radius, shape);

            // Навантажуємо згенерований snapshot,
            // щоб гарантовано розкрити стартову позицію. 
            // Якщо є збереження з валідним snapshot, 
            //  він уже завантажився через FogOfWarSaveModule і додатково не перезапишеться.
            _fogOfWarService.LoadFromSnapshot(snapshot);

            // Гарантуємо, що стартова область буде 
            // у exploredTiles через RegisterFixedVisionArea
            _fogOfWarService.RegisterFixedVisionArea(StartRevealAnchorId, center, radius, shape);

            // Після розкриття стартової області — зберігаємо слот, щоб зміни (туман) були персистентні.
            try
            {
                int slot = GameLaunchContext.SaveSlot;
                _saveService?.Save(slot);
                Debug.Log($"[Bootstrap] Автосейв після розкриття стартового туману у слот {slot}.");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося зберегти після розкриття туману: {ex}");
            }
        }

        /// <summary>
        /// Визначає центр розкриття стартової позиції для локального гравця.
        /// </summary>
        /// <returns>Центр розкриття стартової позиції для локального гравця.</returns>
        private bool ShouldComputeHostStartPositions()
        {
            if (_sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
                return !IsMultiplayerLaunchContext();

            return _sessionManager.IsLocalPlayerHost;
        }

        private static bool IsMultiplayerLaunchContext()
        {
            return GameLaunchContext.Mode == GameLaunchMode.MenuJoinGame
                || GameLaunchContext.Mode == GameLaunchMode.MenuMultiplayerGame
                || (GameLaunchContext.HasWorldSettings && GameLaunchContext.MaxPlayers > 1);
        }


        /// <summary>
        /// Розкриває стартову позицію навколо спавну гравця (якщо він є) або навколо позиції, визначеної у WorldSpawnPositionsSignal.
        /// </summary>
        /// <param name="index">Індекс стартової позиції.</param>
        /// <returns>Ідентифікатор якоря стартової позиції.</returns>
        private static string ResolveStartVisionAnchorId(int index)
            => index <= 0 ? StartVisionAnchorId : $"{StartVisionAnchorId}-{index}";

        /// <summary>
        /// Якщо стартова позиція виявиться у чорному тумані, камера буде переміщена до найближчої видимої ділянки.
        /// </summary>
        private void UnregisterStartVisionAnchors()
        {
            int count = Mathf.Max(1, _registeredStartAnchorCount);
            for (int index = 0; index < count; index++)
                _fogOfWarService.UnregisterUnit(ResolveStartVisionAnchorId(index));

            _registeredStartAnchorCount = 0;
        }


        /// <summary>
        /// Якщо є збереження і автозавантаження ввімкнено — туман відновить FogOfWarSaveModule. Якщо snapshot битий, робимо repair:
        /// </summary>
        /// <param name="signal">Сигнал з даними згенерованого світу.</param>
        private void RepairLoadedFogIfNeeded(WorldGeneratedDataSignal signal)
        {
            // Якщо є збереження і автозавантаження ввімкнено —
            if (!CanRunStartLogic())
                return;

            // туман відновить FogOfWarSaveModule. 
            // Якщо snapshot битий, робимо repair:
            var snapshot = _fogOfWarService.GetExploredSnapshot();


            // Якщо snapshot битий або не має видимої ділянки для мапи, 
            // розкриваємо стартову область навколо спавну гравця або позиції з WorldSpawnPositionsSignal.
            if (IsFogSnapshotUsable(snapshot, signal.Width, signal.Height))
                return;


            // Якщо snapshot битий або не має видимої ділянки для мапи, 
            // розкриваємо стартову область навколо спавну гравця 
            // або позиції з WorldSpawnPositionsSignal.
            Vector2Int center = ResolveRepairCenter(snapshot, signal.Width, signal.Height);

            // Розкриваємо стартову область навколо спавну гравця або позиції з
            //  WorldSpawnPositionsSignal, щоб гарантувати видиму ділянку для старту.
            int radius = _settings.ResolveRevealedRadius(signal.Width, signal.Height);


            // Навантажуємо згенерований snapshot, 
            // щоб гарантовано розкрити стартову позицію. 
            // Якщо є збереження з валідним snapshot, він уже завантажився через 
            // FogOfWarSaveModule і додатково не перезапишеться.
            var repaired = BuildRevealSnapshot(signal.Width, signal.Height, center, radius, _settings.ResolveRevealShape());


            // Навантажуємо згенерований snapshot, 
            // щоб гарантовано розкрити стартову позицію. 
            // Якщо є збереження з валідним snapshot, 
            // оновлений snapshot завантажиться через FogOfWarSaveModule і 
            // перезапише його, інакше — завантажиться цей відремонтований snapshot.
            _fogOfWarService.LoadFromSnapshot(repaired);


            // Якщо стартова позиція виявиться у чорному тумані, 
            // камера буде переміщена до найближчої видимої ділянки. 
            // Щоб уникнути неприємного сюрпризу, розкриваємо стартову 
            // область до телепортації камери.
            if (_settings.keepCoreFullyVisible)
            {
                // Уникаємо попередження FogOfWar «UnregisterUnit before Initialize»: 
                // на першому виклику якорь ще не зареєстровано — пропускаємо unregister.
                int visibleRange = _settings.ResolveCoreVisibleRadius(signal.Width, signal.Height);

                // Розкриваємо стартову область навколо спавну гравця або позиції з 
                // WorldSpawnPositionsSignal, щоб гарантувати видиму ділянку для старту.
                _fogOfWarService.RegisterFixedVisionArea(StartVisionAnchorId, center, visibleRange, _settings.ResolveRevealShape());


                // Гарантуємо, що стартова область буде у exploredTiles через RegisterFixedVisionArea
                _startAnchorRegistered = true;
            }

            // Якщо snapshot битий або не має видимої ділянки для мапи, 
            // розкриваємо стартову область навколо спавну гравця або позиції з WorldSpawnPositionsSignal.
            Debug.LogWarning($"[Bootstrap] FogOfWar snapshot був невалідний або без видимої ділянки для мапи {signal.Width}x{signal.Height}. Стартову область відновлено біля {center}.");

        }

        /// <summary>
        /// Розкриває стартову позицію навколо спавну гравця (якщо він є) або навколо позиції, визначеної у WorldSpawnPositionsSignal.
        /// </summary>
        /// <param name="snapshot">Снімок видимості карти.</param>
        /// <param name="width">Ширина карти.</param>
        /// <param name="height">Висота карти.</param>
        /// <returns>Повертає координати стартової позиції для відновлення.</returns>
        private Vector2Int ResolveRepairCenter(bool[,] snapshot, int width, int height)
        {
            // Якщо є спавн гравця, розкриваємо навколо нього.
            if (TryGetLocalSpawnPosition(out Vector2Int localSpawn))
                // Розкриваємо навколо позиції спавну гравця,
                return ClampToMap(localSpawn, width, height);



            // Якщо є позиція з WorldSpawnPositionsSignal, розкриваємо навколо неї.
            if (_startingPositionState.IsSet)
                // Розкриваємо навколо позиції, визначеної у WorldSpawnPositionsSignal.
                return ClampToMap(_startingPositionState.StartPosition, width, height);



            // Інакше розкриваємо навколо центру карти або найближчої до нього видимої ділянки.
            Vector2Int snapshotCenter = FindRepairCenter(snapshot, width, height);



            // Якщо центр снімку не є видимою ділянкою, намагаємося знайти найближчу до нього позицію юніта,
            // щоб розкрити стартову область навколо неї замість випадкової
            if (TryGetClosestUnitPosition(snapshotCenter, out Vector2Int unitPosition))
                // Розкриваємо навколо позиції юніта, якщо вона ближче до центру снімку, 
                // ніж будь-яка інша знайдена видима ділянка.
                return ClampToMap(unitPosition, width, height);


            // Якщо центр снімку є видимою ділянкою або валідною позицією юніта не знайдено, 
            // розкриваємо навколо центру снімку.
            return snapshotCenter;
        }

        /// <summary>
        /// Пробуємо отримати стартову позицію для локального гравця. 
        /// Якщо вона визначена у WorldSpawnPositionsSignal, використовуємо її.
        /// Інакше, якщо є спавн гравця, використовуємо його.
        /// </summary>
        /// <param name="position">Позиція старту для локального гравця.</param>
        /// <returns>Повертає true, якщо стартова позиція знайдена, і false в іншому випадку.</returns>
        private bool TryGetLocalSpawnPosition(out Vector2Int position)
        {
            // Якщо є спавн гравця, використовуємо його.
            string localPlayerId = _sessionManager?.LocalPlayerId;

            // Якщо стартова позиція для локального гравця визначена у WorldSpawnPositionsSignal, 
            // використовуємо її.
            if (!string.IsNullOrEmpty(localPlayerId) &&
                _startingPositionState.PlayerStartPositions.TryGetValue(localPlayerId, out position))
            {
                return true;
            }

            // Якщо є спавн гравця, використовуємо його.
            var assignments = _startingPositionState.SpawnAssignments;

            // Якщо стартова позиція для локального гравця не визначена у WorldSpawnPositionsSignal,
            for (int index = 0; index < assignments.Count; index++)
            {
                // Якщо це не бот, вважаємо його локальним гравцем і використовуємо його стартову позицію.
                if (!assignments[index].IsBot)
                {
                    // Якщо є спавн гравця, використовуємо його.
                    position = assignments[index].Position;
                    return true;
                }
            }

            // Якщо стартова позиція для локального гравця не визначена у 
            // WorldSpawnPositionsSignal і немає спавну гравця,
            // стартову позицію для локального гравця не знайдено.
            position = default;
            return false;
        }

        /// <summary>
        /// Розкриває стартову позицію навколо спавну гравця (якщо він є) або навколо позиції, визначеної у WorldSpawnPositionsSignal.
        /// </summary>
        /// <param name="width">Ширина карти.</param>
        /// <param name="height">Висота карти.</param>
        /// <returns>Позиція для розкриття стартової області.</returns>
        private Vector2Int ResolveLocalRevealCenter(int width, int height)
        {
            // Якщо є спавн гравця, розкриваємо навколо нього.
            if (TryGetLocalSpawnPosition(out Vector2Int localSpawn))
                //  Розкриваємо навколо позиції спавну гравця, 
                // якщо вона визначена у WorldSpawnPositionsSignal.
                return ClampToMap(localSpawn, width, height);


            // Якщо є позиція з WorldSpawnPositionsSignal, розкриваємо навколо неї.
            if (_startingPositionState.IsSet)
                // Розкриваємо навколо позиції, визначеної у WorldSpawnPositionsSignal.
                return ClampToMap(_startingPositionState.StartPosition, width, height);


            // Інакше розкриваємо навколо центру карти або найближчої до нього видимої ділянки.
            return new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));
        }

        /// <summary>
        /// Обмежує позицію межами карти, щоб уникнути помилок при розкритті туману або телепортації камери.
        /// </summary>
        /// <param name="position">Позиція для обмеження.</param>
        /// <param name="width">Ширина карти.</param>
        /// <param name="height">Висота карти.</param>
        /// <returns>Обмежена позиція.</returns>
        private static Vector2Int ClampToMap(Vector2Int position, int width, int height)
        {
            // Обмежуємо позицію межами карти, 
            // щоб уникнути помилок при розкритті туману 
            // або телепортації камери.
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, Mathf.Max(0, width - 1)),
                Mathf.Clamp(position.y, 0, Mathf.Max(0, height - 1)));
        }

        private bool IsFogSnapshotUsable(bool[,] snapshot, int width, int height)
        {
            if (snapshot == null)
                return false;

            if (snapshot.GetLength(0) != width || snapshot.GetLength(1) != height)
                return false;

            int explored = 0;
            int required = Mathf.Max(1, _settings.minimumExploredTilesBeforeRepair);
            bool hasEnoughExplored = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (snapshot[x, y])
                    {
                        explored++;
                        if (explored >= required)
                        {
                            hasEnoughExplored = true;
                            break;
                        }
                    }
                }

                if (hasEnoughExplored)
                    break;
            }

            if (!hasEnoughExplored)
                return false;

            if (!_settings.keepCoreFullyVisible)
                return true;

            Vector2Int center = ResolveRepairCenter(snapshot, width, height);
            int radius = Mathf.Max(1, _settings.ResolveCoreVisibleRadius(width, height));
            return HasVisibleTileNear(center, width, height, radius, _settings.ResolveRevealShape());
        }

        private bool HasVisibleTileNear(Vector2Int center, int width, int height, int radius, FogRevealShape shape)
        {
            float radiusWithCellCoverage = radius + 0.5f;
            float radiusSqr = radiusWithCellCoverage * radiusWithCellCoverage;

            int minX = Mathf.Max(0, center.x - radius);
            int maxX = Mathf.Min(width - 1, center.x + radius);
            int minY = Mathf.Max(0, center.y - radius);
            int maxY = Mathf.Min(height - 1, center.y + radius);

            for (int x = minX; x <= maxX; x++)
            {
                int deltaX = x - center.x;
                for (int y = minY; y <= maxY; y++)
                {
                    int deltaY = y - center.y;
                    if (!IsInsideRevealShape(deltaX, deltaY, radius, radiusSqr, shape))
                        continue;

                    if (_fogOfWarService.IsVisible(new Vector2Int(x, y)))
                        return true;
                }
            }

            return false;
        }

        private static Vector2Int FindRepairCenter(bool[,] snapshot, int width, int height)
        {
            if (snapshot == null)
                return new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));

            long sumX = 0;
            long sumY = 0;
            int count = 0;
            int copyW = Mathf.Min(width, snapshot.GetLength(0));
            int copyH = Mathf.Min(height, snapshot.GetLength(1));

            for (int x = 0; x < copyW; x++)
            {
                for (int y = 0; y < copyH; y++)
                {
                    if (!snapshot[x, y])
                        continue;

                    sumX += x;
                    sumY += y;
                    count++;
                }
            }

            if (count == 0)
                return new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));

            return new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(sumX / (float)count), 0, Mathf.Max(0, width - 1)),
                Mathf.Clamp(Mathf.RoundToInt(sumY / (float)count), 0, Mathf.Max(0, height - 1)));
        }

        private static bool[,] BuildRevealSnapshot(int width, int height, Vector2Int center, int radius, FogRevealShape shape)
            => BuildRevealSnapshot(width, height, new[] { center }, radius, shape);

        private static bool[,] BuildRevealSnapshot(int width, int height, IReadOnlyList<Vector2Int> centers, int radius, FogRevealShape shape)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            radius = Mathf.Max(1, radius);
            float radiusWithCellCoverage = radius + 0.5f;
            float radiusSqr = radiusWithCellCoverage * radiusWithCellCoverage;

            var snapshot = new bool[width, height];
            if (centers == null || centers.Count == 0)
                return snapshot;

            for (int centerIndex = 0; centerIndex < centers.Count; centerIndex++)
            {
                Vector2Int center = centers[centerIndex];
                int minX = Mathf.Max(0, center.x - radius);
                int maxX = Mathf.Min(width - 1, center.x + radius);
                int minY = Mathf.Max(0, center.y - radius);
                int maxY = Mathf.Min(height - 1, center.y + radius);

                for (int x = minX; x <= maxX; x++)
                {
                    int deltaX = x - center.x;
                    for (int y = minY; y <= maxY; y++)
                    {
                        int deltaY = y - center.y;
                        if (IsInsideRevealShape(deltaX, deltaY, radius, radiusSqr, shape))
                            snapshot[x, y] = true;
                    }
                }
            }

            return snapshot;
        }

        private static bool IsInsideRevealShape(int dx, int dy, int radius, float radiusSqr, FogRevealShape shape)
        {
            return shape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= radius,
                _ => dx * dx + dy * dy <= radiusSqr,
            };
        }
    }
}
