using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal delegate bool StartingPositionTryResolvePosition(out Vector2Int position);
    internal delegate bool StartingPositionTryResolveClosestPosition(Vector2Int origin, out Vector2Int position);

    internal interface IStartingPositionLoadedFogRepairService
    {
        bool RepairLoadedFogIfNeeded(
            WorldGeneratedDataSignal signal,
            System.Func<bool> canRunStartLogic,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            bool hasStartingPosition,
            Vector2Int startingPosition,
            StartingPositionTryResolveClosestPosition tryGetClosestUnitPosition);

        Vector2Int ResolveRepairCenter(
            bool[,] snapshot,
            int width,
            int height,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            bool hasStartingPosition,
            Vector2Int startingPosition,
            StartingPositionTryResolveClosestPosition tryGetClosestUnitPosition);
    }

    internal sealed class StartingPositionLoadedFogRepairService
        : IStartingPositionLoadedFogRepairService
    {
        private const string StartDiagTag = "[MoyvaFogStartDiag]";

        private readonly IFogOfWarService _fogOfWarService;
        private readonly StartingPositionInitializerSettings _settings;
        private readonly IStartingPositionFogRevealService _fogRevealService;
        private readonly string _startRevealAnchorId;
        private readonly string _debugTag;

        public StartingPositionLoadedFogRepairService(
            IFogOfWarService fogOfWarService,
            StartingPositionInitializerSettings settings,
            IStartingPositionFogRevealService fogRevealService,
            string startRevealAnchorId,
            string debugTag)
        {
            _fogOfWarService = fogOfWarService;
            _settings = settings;
            _fogRevealService = fogRevealService;
            _startRevealAnchorId = startRevealAnchorId;
            _debugTag = debugTag;
        }

        public bool RepairLoadedFogIfNeeded(
            WorldGeneratedDataSignal signal,
            System.Func<bool> canRunStartLogic,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            bool hasStartingPosition,
            Vector2Int startingPosition,
            StartingPositionTryResolveClosestPosition tryGetClosestUnitPosition)
        {
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Debug.Log($"{StartDiagTag} RepairLoadedFogIfNeeded begin map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}, hasStartState={hasStartingPosition}, hasLocalSpawnResolver={tryGetLocalSpawnPosition != null}, hasUnitResolver={tryGetClosestUnitPosition != null}.");
            if (canRunStartLogic != null && !canRunStartLogic())
            {
                Debug.LogWarning($"{StartDiagTag} RepairLoadedFogIfNeeded blocked canRunStartLogic=false.");
                Debug.LogWarning($"{_debugTag} Bootstrap.RepairLoadedFogIfNeeded blocked by CanRunStartLogic=false.");
                return false;
            }

            var snapshot = _fogOfWarService.GetExploredSnapshot();
            string snapshotState = snapshot == null
                ? "null"
                : snapshot.GetLength(0) != baseMapSize.x || snapshot.GetLength(1) != baseMapSize.y
                    ? $"wrong-size:{snapshot.GetLength(0)}x{snapshot.GetLength(1)}"
                    : "candidate";
            Debug.Log($"{StartDiagTag} RepairLoadedFogIfNeeded snapshotState={snapshotState}, map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}.");
            Debug.Log($"{_debugTag} Bootstrap.RepairLoadedFogIfNeeded snapshot={(snapshot != null ? $"{snapshot.GetLength(0)}x{snapshot.GetLength(1)}" : "null")}, map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}.");

            if (IsFogSnapshotUsable(
                    snapshot,
                    baseMapSize.x,
                    baseMapSize.y,
                    tryGetLocalSpawnPosition,
                    hasStartingPosition,
                    startingPosition,
                    tryGetClosestUnitPosition))
            {
                Debug.Log($"{StartDiagTag} RepairLoadedFogIfNeeded snapshot usable, no repair reveal needed.");
                Debug.Log($"{_debugTag} Bootstrap.RepairLoadedFogIfNeeded snapshot usable, no reveal repair needed.");
                return false;
            }

            Vector2Int center = ResolveRepairCenter(
                snapshot,
                baseMapSize.x,
                baseMapSize.y,
                tryGetLocalSpawnPosition,
                hasStartingPosition,
                startingPosition,
                tryGetClosestUnitPosition);
            string reason = ResolveRepairCenterReason(
                snapshot,
                baseMapSize.x,
                baseMapSize.y,
                tryGetLocalSpawnPosition,
                hasStartingPosition,
                startingPosition,
                tryGetClosestUnitPosition);
            int radius = _settings.ResolveRevealedRadius(baseMapSize.x, baseMapSize.y);
            var shape = _settings.ResolveRevealShape();
            Debug.LogWarning($"{StartDiagTag} RepairLoadedFogIfNeeded applying repair center={center}, reason={reason}, radius={radius}, shape={shape}, snapshotState={snapshotState}.");
            Debug.LogWarning($"{_debugTag} Bootstrap.RepairLoadedFogIfNeeded applying repair center={center}, radius={radius}, shape={shape}, map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}.");
            _fogOfWarService.RevealArea(center, radius, shape, keepVisible: true, visibleAreaId: _startRevealAnchorId);

            if (_settings.keepCoreFullyVisible)
                _fogRevealService.RegisterStartupCoreVisibility(baseMapSize.x, baseMapSize.y, center);

            bool visibleAfter = _fogOfWarService != null && _fogOfWarService.IsVisible(center);
            bool exploredAfter = _fogOfWarService != null && _fogOfWarService.IsExplored(center);
            Debug.Log($"{StartDiagTag} RepairLoadedFogIfNeeded result center={center}, repairExecuted=true, visibleAfter={visibleAfter}, exploredAfter={exploredAfter}.");

            Debug.LogWarning($"[Bootstrap] FogOfWar snapshot був невалідний або без видимої ділянки для мапи {baseMapSize.x}x{baseMapSize.y}. Стартову область відновлено біля {center}.");
            return true;
        }

        public Vector2Int ResolveRepairCenter(
            bool[,] snapshot,
            int width,
            int height,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            bool hasStartingPosition,
            Vector2Int startingPosition,
            StartingPositionTryResolveClosestPosition tryGetClosestUnitPosition)
        {
            if (tryGetLocalSpawnPosition != null && tryGetLocalSpawnPosition(out Vector2Int localSpawn))
                return StartingPositionMapUtility.ClampToMap(localSpawn, width, height);

            if (hasStartingPosition)
                return StartingPositionMapUtility.ClampToMap(startingPosition, width, height);

            Vector2Int snapshotCenter = StartingPositionMapUtility.FindRepairCenter(snapshot, width, height);
            if (tryGetClosestUnitPosition != null && tryGetClosestUnitPosition(snapshotCenter, out Vector2Int unitPosition))
                return StartingPositionMapUtility.ClampToMap(unitPosition, width, height);

            return snapshotCenter;
        }

        public bool IsFogSnapshotUsable(
            bool[,] snapshot,
            int width,
            int height,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            bool hasStartingPosition,
            Vector2Int startingPosition,
            StartingPositionTryResolveClosestPosition tryGetClosestUnitPosition)
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

            Vector2Int center = ResolveRepairCenter(
                snapshot,
                width,
                height,
                tryGetLocalSpawnPosition,
                hasStartingPosition,
                startingPosition,
                tryGetClosestUnitPosition);
            int radius = Mathf.Max(1, _settings.ResolveCoreVisibleRadius(width, height));
            return HasVisibleTileNear(center, width, height, radius, _settings.ResolveRevealShape());
        }

        public bool HasVisibleTileNear(Vector2Int center, int width, int height, int radius, FogRevealShape shape)
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
                    if (!StartingPositionMapUtility.IsInsideRevealShape(deltaX, deltaY, radius, radiusSqr, shape))
                        continue;

                    if (_fogOfWarService.IsVisible(new Vector2Int(x, y)))
                        return true;
                }
            }

            return false;
        }

        private static string ResolveRepairCenterReason(
            bool[,] snapshot,
            int width,
            int height,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            bool hasStartingPosition,
            Vector2Int startingPosition,
            StartingPositionTryResolveClosestPosition tryGetClosestUnitPosition)
        {
            if (tryGetLocalSpawnPosition != null && tryGetLocalSpawnPosition(out _))
                return "local-spawn";

            if (hasStartingPosition)
                return "starting-position-state";

            Vector2Int snapshotCenter = StartingPositionMapUtility.FindRepairCenter(snapshot, width, height);
            if (tryGetClosestUnitPosition != null && tryGetClosestUnitPosition(snapshotCenter, out _))
                return "closest-unit";

            return "snapshot-center-fallback";
        }
    }
}
