using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.FogOfWar.API;
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

        private readonly IFogOfWarService _fogOfWarService;
        private readonly ISaveService     _saveService;
        private readonly SignalBus        _signalBus;
        private readonly StartingPositionInitializerSettings _settings;
        private readonly BootstrapStartingPositionState _startingPositionState;
        private readonly ICameraMovement _cameraMovement;

    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private IPathfinder _pathfinder;
        [InjectOptional] private IUnitService _unitService;
    #pragma warning restore CS0649

        private bool _startAnchorRegistered;

        public StartingPositionInitializer(
            IFogOfWarService fogOfWarService,
            ISaveService     saveService,
            SignalBus        signalBus,
            StartingPositionInitializerSettings settings,
            BootstrapStartingPositionState startingPositionState,
            ICameraMovement cameraMovement)
        {
            _fogOfWarService       = fogOfWarService;
            _saveService           = saveService;
            _signalBus             = signalBus;
            _settings              = settings;
            _startingPositionState = startingPositionState;
            _cameraMovement        = cameraMovement;
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
        }

        // ─── Основна логіка ───────────────────────────────────────────────────

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            // Якщо є збереження і автозавантаження ввімкнено —
            // туман відновить FogOfWarSaveModule. Якщо snapshot битий, робимо repair.
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
            {
                RepairLoadedFogIfNeeded(signal);
                TeleportMainCamera(ResolveStartupCameraTarget(signal.Width, signal.Height));
                return;
            }

            if (!CanRunStartLogic())
                return;

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

            RevealStartingArea(signal.Width, signal.Height, startPos);

            if (_settings.keepCoreFullyVisible)
            {
                // Уникаємо попередження FogOfWar «UnregisterUnit before Initialize»:
                // на першому виклику якорь ще не зареєстровано — пропускаємо unregister.
                if (_startAnchorRegistered)
                    _fogOfWarService.UnregisterUnit(StartVisionAnchorId);

                int visibleRange = _settings.coreVisibleRadiusOverride > 0
                    ? _settings.coreVisibleRadiusOverride
                    : _settings.ResolveCoreVisibleRadius(signal.Width, signal.Height);
                _fogOfWarService.RegisterFixedVisionArea(StartVisionAnchorId, startPos, visibleRange, _settings.ResolveRevealShape());
                _startAnchorRegistered = true;
            }

            TeleportMainCamera(ResolveVisibleCameraTarget(startPos, signal.Width, signal.Height));

            Debug.Log($"[Bootstrap] Стартова позиція: {startPos}. Туман розкрито, камеру переміщено.");
        }

        private void TeleportMainCamera(Vector2Int startPos)
        {
            _cameraMovement.TeleportCamera(new Vector3(startPos.x, startPos.y, _settings.cameraZ));
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

        private SpawnPositionAssignment[] BuildSpawnAssignments(IReadOnlyList<Vector2Int> positions)
        {
            var participants = _sessionManager?.Participants;
            var assignments = new SpawnPositionAssignment[positions.Count];
            int participantCount = participants?.Count ?? 0;
            int launchParticipantCount = GameLaunchContext.HasWorldSettings
                ? Mathf.Max(1, GameLaunchContext.MaxPlayers)
                : 1;

            for (int index = 0; index < positions.Count; index++)
            {
                string participantId = string.Empty;
                bool isBot = false;

                if (participants != null && index < participantCount)
                {
                    participantId = participants[index].Identity?.PlayerId ?? string.Empty;
                    isBot = participants[index].IsBot;
                }
                else if (index == 0)
                {
                    participantId = ResolveLocalPlayerId();
                }
                else if (index < launchParticipantCount)
                {
                    participantId = $"bot-{index:00}";
                    isBot = true;
                }

                assignments[index] = new SpawnPositionAssignment
                {
                    SlotIndex = index,
                    ParticipantId = participantId,
                    IsBot = isBot,
                    Position = positions[index],
                };
            }

            return assignments;
        }

        private string ResolveLocalPlayerId()
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;
            if (!string.IsNullOrEmpty(localPlayerId))
                return localPlayerId;

            return "local-player";
        }

        private static SpawnPositionAssignment[] CopySpawnAssignments(IReadOnlyList<SpawnPositionAssignment> assignments)
        {
            var copy = new SpawnPositionAssignment[assignments.Count];
            for (int index = 0; index < assignments.Count; index++)
                copy[index] = assignments[index];

            return copy;
        }

        private bool CanRunStartLogic()
        {
            return _sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0 || IsMultiplayerHost();
        }

        private bool IsMultiplayerHost()
        {
            return _sessionManager != null && _sessionManager.IsLocalPlayerHost;
        }

        // ─── Стартове кругле розкриття туману ─────────────────────────────────

        private void RevealStartingArea(int width, int height, Vector2Int center)
        {
            int radius = _settings.ResolveRevealedRadius(width, height);
            var snapshot = BuildRevealSnapshot(width, height, center, radius, _settings.ResolveRevealShape());
            _fogOfWarService.LoadFromSnapshot(snapshot);
        }

        private void RepairLoadedFogIfNeeded(WorldGeneratedDataSignal signal)
        {
            if (!CanRunStartLogic())
                return;

            var snapshot = _fogOfWarService.GetExploredSnapshot();
            if (IsFogSnapshotUsable(snapshot, signal.Width, signal.Height))
                return;

            Vector2Int center = ResolveRepairCenter(snapshot, signal.Width, signal.Height);

            int radius = _settings.ResolveRevealedRadius(signal.Width, signal.Height);
            var repaired = BuildRevealSnapshot(signal.Width, signal.Height, center, radius, _settings.ResolveRevealShape());
            _fogOfWarService.LoadFromSnapshot(repaired);

            if (_settings.keepCoreFullyVisible)
            {
                int visibleRange = _settings.ResolveCoreVisibleRadius(signal.Width, signal.Height);
                _fogOfWarService.RegisterFixedVisionArea(StartVisionAnchorId, center, visibleRange, _settings.ResolveRevealShape());
                _startAnchorRegistered = true;
            }

            Debug.LogWarning($"[Bootstrap] FogOfWar snapshot був невалідний або без видимої ділянки для мапи {signal.Width}x{signal.Height}. Стартову область відновлено біля {center}.");
        }

        private Vector2Int ResolveRepairCenter(bool[,] snapshot, int width, int height)
        {
            if (TryGetLocalSpawnPosition(out Vector2Int localSpawn))
                return ClampToMap(localSpawn, width, height);

            if (_startingPositionState.IsSet)
                return ClampToMap(_startingPositionState.StartPosition, width, height);

            Vector2Int snapshotCenter = FindRepairCenter(snapshot, width, height);
            if (TryGetClosestUnitPosition(snapshotCenter, out Vector2Int unitPosition))
                return ClampToMap(unitPosition, width, height);

            return snapshotCenter;
        }

        private bool TryGetLocalSpawnPosition(out Vector2Int position)
        {
            string localPlayerId = _sessionManager?.LocalPlayerId;
            if (!string.IsNullOrEmpty(localPlayerId) &&
                _startingPositionState.PlayerStartPositions.TryGetValue(localPlayerId, out position))
            {
                return true;
            }

            var assignments = _startingPositionState.SpawnAssignments;
            for (int index = 0; index < assignments.Count; index++)
            {
                if (!assignments[index].IsBot)
                {
                    position = assignments[index].Position;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static Vector2Int ClampToMap(Vector2Int position, int width, int height)
        {
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
            bool hasVisibleTile = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var position = new Vector2Int(x, y);
                    if (_fogOfWarService.IsVisible(position))
                        hasVisibleTile = true;

                    if (snapshot[x, y])
                    {
                        explored++;
                        if (explored >= required)
                            hasEnoughExplored = true;
                    }

                    if (hasEnoughExplored && hasVisibleTile)
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
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            radius = Mathf.Max(1, radius);
            float radiusWithCellCoverage = radius + 0.5f;
            float radiusSqr = radiusWithCellCoverage * radiusWithCellCoverage;

            var snapshot = new bool[width, height];
            int minX = Mathf.Max(0, center.x - radius);
            int maxX = Mathf.Min(width - 1, center.x + radius);
            int minY = Mathf.Max(0, center.y - radius);
            int maxY = Mathf.Min(height - 1, center.y + radius);

            for (int x = minX; x <= maxX; x++)
            {
                int deltaX = x - center.x;
                int deltaXSqr = deltaX * deltaX;

                for (int y = minY; y <= maxY; y++)
                {
                    int deltaY = y - center.y;
                    if (IsInsideRevealShape(deltaX, deltaY, radius, radiusSqr, shape))
                        snapshot[x, y] = true;
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
