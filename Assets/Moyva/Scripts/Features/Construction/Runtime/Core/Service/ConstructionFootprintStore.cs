using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Owns placed-building footprint occupancy state.
    ///
    /// ConstructionService decides when a building is committed or removed.
    /// This store is the single authority for mapping occupied cells back to
    /// their building origin and for registering/unregistering footprint cells
    /// in ObjectsMap.
    /// </summary>
    internal sealed class ConstructionFootprintStore
    {
        private readonly IObjectsMapService _objectsMapService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IGridService _gridService;
        private readonly Func<Vector2Int, ConstructionRotation> _rotationResolver;

        private readonly Dictionary<Vector2Int, Vector2Int>
            _originByOccupiedTile = new();
        private readonly Dictionary<Vector2Int, RegisteredFootprint>
            _footprintsByOrigin = new();

        private readonly struct RegisteredFootprint
        {
            public RegisteredFootprint(
                string buildingId,
                Vector2Int[] cells,
                ConstructionRotation rotation)
            {
                BuildingId = buildingId;
                Cells = cells;
                Rotation = rotation;
            }

            public string BuildingId { get; }
            public Vector2Int[] Cells { get; }
            public ConstructionRotation Rotation { get; }
        }

        public ConstructionFootprintStore(
            IObjectsMapService objectsMapService,
            IBuildingRegistry buildingRegistry,
            IGridService gridService,
            Func<Vector2Int, ConstructionRotation> rotationResolver)
        {
            _objectsMapService = objectsMapService
                ?? throw new ArgumentNullException(nameof(objectsMapService));
            _buildingRegistry = buildingRegistry
                ?? throw new ArgumentNullException(nameof(buildingRegistry));
            _gridService = gridService;
            _rotationResolver = rotationResolver
                ?? throw new ArgumentNullException(nameof(rotationResolver));
        }

        public bool TryRegister(
            Vector2Int origin,
            string buildingId,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);

            if (!BuildingFootprintUtility.TryValidate(
                    definition,
                    out string configurationReason))
            {
                Debug.LogError(
                    $"[MoyvaBuildGridDiag] footprint-register-failed " +
                    $"building='{buildingId}' origin={origin} " +
                    $"error='{configurationReason}'");
                return false;
            }

            int cellCount =
                BuildingFootprintUtility.GetOccupiedCellCount(definition);
            var cells = new Vector2Int[cellCount];

            for (int index = 0; index < cellCount; index++)
            {
                Vector2Int cell =
                    BuildingFootprintUtility.GetOccupiedCell(
                        definition,
                        origin,
                        index,
                        rotation);
                cells[index] = cell;

                if (_gridService != null
                    && !_gridService.TryGetTileData(cell, out _))
                {
                    return false;
                }

                if (_objectsMapService.IsOccupied(cell))
                    return false;
            }

            int registeredCount = 0;
            _footprintsByOrigin[origin] =
                new RegisteredFootprint(buildingId, cells, rotation);

            for (int index = 0; index < cells.Length; index++)
                _originByOccupiedTile[cells[index]] = origin;

            try
            {
                for (int index = 0; index < cellCount; index++)
                {
                    registeredCount++;
                    _objectsMapService.Register(
                        cells[index],
                        buildingId);
                }

                return true;
            }
            catch (Exception ex)
            {
                for (int index = registeredCount - 1;
                     index >= 0;
                     index--)
                {
                    Vector2Int cell = cells[index];

                    if (_objectsMapService.TryGetOccupant(
                            cell,
                            out string occupantId)
                        && string.Equals(
                            occupantId,
                            buildingId,
                            StringComparison.Ordinal))
                    {
                        TryUnregisterCell(
                            cell,
                            buildingId,
                            origin,
                            "registration-rollback");
                    }

                    _originByOccupiedTile.Remove(cell);
                }

                _footprintsByOrigin.Remove(origin);

                Debug.LogError(
                    $"[MoyvaBuildGridDiag] footprint-register-failed " +
                    $"building='{buildingId}' origin={origin} " +
                    $"error='{ex.Message}'");
                return false;
            }
        }

        public void Unregister(
            Vector2Int origin,
            string buildingId)
        {
            Vector2Int[] cells;

            if (_footprintsByOrigin.TryGetValue(
                    origin,
                    out RegisteredFootprint registered)
                && (string.IsNullOrWhiteSpace(buildingId)
                    || string.Equals(
                        registered.BuildingId,
                        buildingId,
                        StringComparison.Ordinal)))
            {
                buildingId = registered.BuildingId;
                cells = registered.Cells;
                _footprintsByOrigin.Remove(origin);
            }
            else
            {
                BuildingDefinition definition =
                    _buildingRegistry.GetById(buildingId);
                int cellCount =
                    BuildingFootprintUtility.GetOccupiedCellCount(
                        definition);
                cells = new Vector2Int[cellCount];
                ConstructionRotation rotation =
                    _rotationResolver(origin);

                for (int index = 0; index < cellCount; index++)
                {
                    cells[index] =
                        BuildingFootprintUtility.GetOccupiedCell(
                            definition,
                            origin,
                            index,
                            rotation);
                }
            }

            for (int index = 0; index < cells.Length; index++)
            {
                Vector2Int cell = cells[index];

                if (_originByOccupiedTile.TryGetValue(
                        cell,
                        out Vector2Int registeredOrigin)
                    && registeredOrigin != origin)
                {
                    continue;
                }

                _originByOccupiedTile.Remove(cell);

                if (_objectsMapService.TryGetOccupant(
                        cell,
                        out string occupantId)
                    && string.Equals(
                        occupantId,
                        buildingId,
                        StringComparison.Ordinal))
                {
                    TryUnregisterCell(
                        cell,
                        buildingId,
                        origin,
                        "unregister");
                }
            }
        }

        public Vector2Int ResolveOrigin(Vector2Int occupiedCell)
        {
            return _originByOccupiedTile.TryGetValue(
                    occupiedCell,
                    out Vector2Int origin)
                ? origin
                : occupiedCell;
        }

        private void TryUnregisterCell(
            Vector2Int cell,
            string buildingId,
            Vector2Int origin,
            string context)
        {
            try
            {
                _objectsMapService.Unregister(cell);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[MoyvaBuildGridDiag] footprint-unregister-signal-failed " +
                    $"context='{context}' building='{buildingId}' " +
                    $"origin={origin} cell={cell} error='{ex.Message}'");
            }
        }
    }
}
