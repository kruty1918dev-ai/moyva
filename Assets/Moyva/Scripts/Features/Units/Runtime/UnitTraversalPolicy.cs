using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitTraversalPolicy :
        IUnitTraversalPolicy,
        IInitializable,
        IDisposable
    {
        private const float DiagonalFactor = 1.41421356237f;
        private readonly IGridService _grid;
        private readonly ITraversalCostResolver _traversalCosts;
        private readonly IObjectsMapService _objectsMap;
        private readonly IUnitService _units;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly IUnitPlacementValidator _placementValidator;
        private readonly SignalBus _signals;
        private readonly IGeneratedTerrainSurfaceVersionQuery _terrainVersionQuery;

        private readonly Dictionary<Vector2Int, StaticTraversalCell> _staticCache = new();
        private int _lastTerrainVersion = int.MinValue;

        private readonly struct StaticTraversalCell
        {
            public StaticTraversalCell(bool allowed, string tileTypeId, string reason)
            {
                Allowed = allowed;
                TileTypeId = tileTypeId;
                Reason = reason;
            }

            public bool Allowed { get; }
            public string TileTypeId { get; }
            public string Reason { get; }
        }

        public UnitTraversalPolicy(
            IGridService grid,
            ITraversalCostResolver traversalCosts,
            IObjectsMapService objectsMap,
            IUnitService units,
            IUnitClassConfig unitConfigs,
            IUnitPlacementValidator placementValidator,
            SignalBus signals,
            [InjectOptional] IGeneratedTerrainSurfaceVersionQuery terrainVersionQuery = null)
        {
            _grid = grid;
            _traversalCosts = traversalCosts;
            _objectsMap = objectsMap;
            _units = units;
            _unitConfigs = unitConfigs;
            _placementValidator = placementValidator;
            _signals = signals;
            _terrainVersionQuery = terrainVersionQuery;
        }

        public void Initialize()
        {
            _signals.Subscribe<GridTileChangedSignal>(OnGridTileChanged);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<GridTileChangedSignal>(OnGridTileChanged);
            _staticCache.Clear();
        }

        public bool TryEvaluateStep(
            string unitId,
            Vector2Int from,
            Vector2Int to,
            float availableMovement,
            UnitTraversalMode mode,
            out float cost,
            out string reason)
        {
            cost = 0f;
            reason = null;

            if (string.IsNullOrWhiteSpace(unitId))
            {
                reason = "Юніт не визначений.";
                return false;
            }

            int dx = Mathf.Abs(to.x - from.x);
            int dy = Mathf.Abs(to.y - from.y);
            if (dx > 1 || dy > 1 || (dx == 0 && dy == 0))
            {
                reason = $"Некоректний крок {from} -> {to}.";
                return false;
            }

            if (!TryGetStaticCell(to, out StaticTraversalCell targetCell))
            {
                reason = targetCell.Reason;
                return false;
            }

            string movementProfileId = ResolveMovementProfileId(unitId);
            if (!_traversalCosts.TryResolve(
                    movementProfileId,
                    targetCell.TileTypeId,
                    out float baseCost,
                    out reason))
            {
                return false;
            }

            if (!CanEnterDynamicCell(unitId, to, mode, out reason))
                return false;

            bool diagonal = dx == 1 && dy == 1;
            if (diagonal)
            {
                var sideA = new Vector2Int(to.x, from.y);
                var sideB = new Vector2Int(from.x, to.y);
                string sideAReason = null;
                string sideBReason = null;

                if (!CanUseDiagonalSide(unitId, movementProfileId, sideA, mode, out sideAReason)
                    || !CanUseDiagonalSide(unitId, movementProfileId, sideB, mode, out sideBReason))
                {
                    reason =
                        "Діагональний прохід заблокований: "
                        + (sideAReason ?? sideBReason ?? "кут перекритий.");
                    return false;
                }
            }

            cost = baseCost * (diagonal ? DiagonalFactor : 1f);

            if (!float.IsPositiveInfinity(availableMovement)
                && availableMovement + 0.0001f < cost)
            {
                reason = "Недостатньо очок руху.";
                return false;
            }

            return true;
        }

        public void InvalidateStaticCache()
        {
            _staticCache.Clear();
            _lastTerrainVersion =
                _terrainVersionQuery?.TerrainSurfaceVersion ?? int.MinValue;
        }

        private void OnGridTileChanged(GridTileChangedSignal signal)
        {
            _staticCache.Remove(signal.Position);
        }

        private bool TryGetStaticCell(
            Vector2Int position,
            out StaticTraversalCell cell)
        {
            EnsureTerrainVersion();

            if (_staticCache.TryGetValue(position, out cell))
                return cell.Allowed;

            string reason = null;
            if (_placementValidator != null)
            {
                if (!_placementValidator.IsTerrainAllowed(position, out reason))
                {
                    cell = new StaticTraversalCell(false, null, reason);
                    _staticCache[position] = cell;
                    return false;
                }
            }
            else if (_grid == null || !_grid.ContainsCell(position))
            {
                cell = new StaticTraversalCell(
                    false,
                    null,
                    "Тайл знаходиться за межами карти.");
                _staticCache[position] = cell;
                return false;
            }

            if (!_grid.TryGetTileData(position, out string tileTypeId)
                || string.IsNullOrWhiteSpace(tileTypeId))
            {
                cell = new StaticTraversalCell(
                    false,
                    null,
                    "На клітинці немає валідного типу тайла.");
                _staticCache[position] = cell;
                return false;
            }

            cell = new StaticTraversalCell(true, tileTypeId, null);
            _staticCache[position] = cell;
            return true;
        }

        private bool CanEnterDynamicCell(
            string unitId,
            Vector2Int position,
            UnitTraversalMode mode,
            out string reason)
        {
            reason = null;

            if (!_objectsMap.IsOccupied(position))
                return true;

            if (!_objectsMap.TryGetOccupant(position, out string occupantId)
                || string.IsNullOrWhiteSpace(occupantId)
                || string.Equals(occupantId, unitId, StringComparison.Ordinal))
            {
                return true;
            }

            if (_units is IConstructionUnitTraversalQuery constructionTraversal)
            {
                bool openGate = mode == UnitTraversalMode.Execute;
                if (constructionTraversal.CanTraverseOccupiedConstructionCell(
                        unitId,
                        position,
                        openGate,
                        out reason))
                {
                    return true;
                }
            }

            reason ??= $"Клітинка зайнята '{occupantId}'.";
            return false;
        }

        private bool CanUseDiagonalSide(
            string unitId,
            string movementProfileId,
            Vector2Int position,
            UnitTraversalMode mode,
            out string reason)
        {
            if (!TryGetStaticCell(position, out StaticTraversalCell cell))
            {
                reason = cell.Reason;
                return false;
            }

            if (!_traversalCosts.TryResolve(
                    movementProfileId,
                    cell.TileTypeId,
                    out _,
                    out reason))
            {
                return false;
            }

            return CanEnterDynamicCell(unitId, position, mode, out reason);
        }

        private string ResolveMovementProfileId(string unitId)
        {
            string unitTypeId = _units.GetUnitTypeId(unitId);
            UnitClassConfig config = string.IsNullOrWhiteSpace(unitTypeId)
                ? null
                : _unitConfigs.GetConfig(unitTypeId);
            string profileId = config?.MovementProfile?.JsonId;
            return string.IsNullOrWhiteSpace(profileId)
                ? MovementProfileIds.GroundDefault
                : profileId;
        }

        private void EnsureTerrainVersion()
        {
            if (_terrainVersionQuery == null)
                return;

            int version = _terrainVersionQuery.TerrainSurfaceVersion;
            if (version == _lastTerrainVersion)
                return;

            _lastTerrainVersion = version;
            _staticCache.Clear();
        }
    }
}
