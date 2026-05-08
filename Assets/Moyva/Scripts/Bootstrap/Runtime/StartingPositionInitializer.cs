using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// На старті нової гри обирає випадкову точку на мапі,
    /// рівномірно розкриває круг туману навколо неї (імітація стартової позиції)
    /// та миттєво переміщує камеру туди.
    ///
    /// При завантаженні збереження — не втручається: туман відновлює FogOfWarSaveModule.
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
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        // ─── Основна логіка ───────────────────────────────────────────────────

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            // Якщо є збереження і автозавантаження ввімкнено —
            // туман відновить FogOfWarSaveModule, не перезаписуємо.
            int slot = GameLaunchContext.SaveSlot;
            if (GameLaunchContext.IsAutoLoadEnabled() && _saveService.HasSave(slot))
                return;

            if (!CanRunStartLogic())
                return;

            List<Vector2Int> startPositions = PickStartingPositions(signal);
            Vector2Int startPos = startPositions.Count > 0
                ? startPositions[0]
                : PickStartingPosition(signal.Width, signal.Height);

            // Зберігаємо позиції, щоб BootstrapGameInitializer міг розмістити замок на першій з них.
            if (startPositions.Count > 0)
                _startingPositionState.Set(startPositions, ResolveStartParticipantIds());
            else
                _startingPositionState.Set(startPos);

            RevealStartingArea(signal.Width, signal.Height, startPos);

            if (_settings.keepCoreFullyVisible)
            {
                // Уникаємо попередження FogOfWar «UnregisterUnit before Initialize»:
                // на першому виклику якорь ще не зареєстровано — пропускаємо unregister.
                if (_startAnchorRegistered)
                    _fogOfWarService.UnregisterUnit(StartVisionAnchorId);

                int visibleRange = _settings.coreVisibleRadiusOverride > 0
                    ? _settings.coreVisibleRadiusOverride
                    : Mathf.Max(1, _settings.revealedCircleRadius);
                _fogOfWarService.RegisterUnit(StartVisionAnchorId, startPos, visibleRange);
                _startAnchorRegistered = true;
            }

            TeleportMainCamera(startPos);

            Debug.Log($"[Bootstrap] Стартова позиція: {startPos}. Туман розкрито, камеру переміщено.");
        }

        private void TeleportMainCamera(Vector2Int startPos)
        {
            _cameraMovement.TeleportCamera(new Vector3(startPos.x, startPos.y, _settings.cameraZ));
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

            return 1;
        }

        private List<string> ResolveStartParticipantIds()
        {
            var participants = _sessionManager?.Participants;
            var ids = new List<string>();
            if (participants == null)
                return ids;

            for (int index = 0; index < participants.Count; index++)
                ids.Add(participants[index].Identity?.PlayerId);

            return ids;
        }

        private bool CanRunStartLogic()
        {
            return _sessionManager == null || _sessionManager.Participants == null || _sessionManager.Participants.Count == 0 || IsMultiplayerHost();
        }

        private bool IsMultiplayerHost()
        {
            var participants = _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
                return false;

            for (int index = 0; index < participants.Count; index++)
            {
                if (participants[index].IsHost)
                    return true;
            }

            return false;
        }

        // ─── Стартове кругле розкриття туману ─────────────────────────────────

        private void RevealStartingArea(int width, int height, Vector2Int center)
        {
            int radius = Mathf.Max(1, _settings.revealedCircleRadius);
            int radiusSqr = radius * radius;

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
                    if (deltaXSqr + deltaY * deltaY <= radiusSqr)
                        snapshot[x, y] = true;
                }
            }

            _fogOfWarService.LoadFromSnapshot(snapshot);
        }
    }
}
