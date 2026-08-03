using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private readonly Dictionary<Vector2Int, Vector2Int> _placedOriginByOccupiedTile = new();
        private readonly Dictionary<Vector2Int, RegisteredFootprint> _registeredFootprintsByOrigin = new();

        private readonly struct RegisteredFootprint
        {
            public RegisteredFootprint(string buildingId, Vector2Int[] cells)
            {
                BuildingId = buildingId;
                Cells = cells;
            }

            public string BuildingId { get; }
            public Vector2Int[] Cells { get; }
        }

        private bool TryRegisterBuildingFootprint(Vector2Int origin, string buildingId)
        {
            BuildingDefinition definition = _placementBuildingRegistry.GetById(buildingId);
            if (!BuildingFootprintUtility.TryValidate(definition, out string configurationReason))
            {
                Debug.LogError(
                    $"[MoyvaBuildGridDiag] footprint-register-failed building='{buildingId}' origin={origin} error='{configurationReason}'");
                return false;
            }

            int cellCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            var cells = new Vector2Int[cellCount];

            for (int index = 0; index < cellCount; index++)
            {
                Vector2Int cell = BuildingFootprintUtility.GetOccupiedCell(definition, origin, index);
                cells[index] = cell;
                if (_gridService != null && !_gridService.TryGetTileData(cell, out _))
                    return false;
                if (_objectsMapService.IsOccupied(cell))
                    return false;
            }

            int registeredCount = 0;
            _registeredFootprintsByOrigin[origin] = new RegisteredFootprint(buildingId, cells);
            for (int index = 0; index < cells.Length; index++)
                _placedOriginByOccupiedTile[cells[index]] = origin;

            try
            {
                for (int index = 0; index < cellCount; index++)
                {
                    registeredCount++;
                    _objectsMapService.Register(cells[index], buildingId);
                }

                return true;
            }
            catch (Exception ex)
            {
                for (int index = registeredCount - 1; index >= 0; index--)
                {
                    Vector2Int cell = cells[index];
                    if (_objectsMapService.TryGetOccupant(cell, out string occupantId)
                        && string.Equals(occupantId, buildingId, StringComparison.Ordinal))
                    {
                        TryUnregisterFootprintCell(cell, buildingId, origin, "registration-rollback");
                    }
                    _placedOriginByOccupiedTile.Remove(cell);
                }
                _registeredFootprintsByOrigin.Remove(origin);

                Debug.LogError($"[MoyvaBuildGridDiag] footprint-register-failed building='{buildingId}' origin={origin} error='{ex.Message}'");
                return false;
            }
        }

        private void UnregisterBuildingFootprint(Vector2Int origin, string buildingId)
        {
            Vector2Int[] cells;
            if (_registeredFootprintsByOrigin.TryGetValue(origin, out RegisteredFootprint registered)
                && (string.IsNullOrWhiteSpace(buildingId)
                    || string.Equals(registered.BuildingId, buildingId, StringComparison.Ordinal)))
            {
                buildingId = registered.BuildingId;
                cells = registered.Cells;
                _registeredFootprintsByOrigin.Remove(origin);
            }
            else
            {
                BuildingDefinition definition = _placementBuildingRegistry.GetById(buildingId);
                int cellCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
                cells = new Vector2Int[cellCount];
                for (int index = 0; index < cellCount; index++)
                    cells[index] = BuildingFootprintUtility.GetOccupiedCell(definition, origin, index);
            }

            for (int index = 0; index < cells.Length; index++)
            {
                Vector2Int cell = cells[index];
                if (_placedOriginByOccupiedTile.TryGetValue(cell, out Vector2Int registeredOrigin)
                    && registeredOrigin != origin)
                {
                    continue;
                }

                _placedOriginByOccupiedTile.Remove(cell);
                if (_objectsMapService.TryGetOccupant(cell, out string occupantId)
                    && string.Equals(occupantId, buildingId, StringComparison.Ordinal))
                {
                    TryUnregisterFootprintCell(cell, buildingId, origin, "unregister");
                }
            }
        }

        private void TryUnregisterFootprintCell(
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
                    $"[MoyvaBuildGridDiag] footprint-unregister-signal-failed context='{context}' building='{buildingId}' origin={origin} cell={cell} error='{ex.Message}'");
            }
        }

        private Vector2Int ResolvePlacedOrigin(Vector2Int occupiedCell)
            => _placedOriginByOccupiedTile.TryGetValue(occupiedCell, out Vector2Int origin)
                ? origin
                : occupiedCell;

        private bool TryResolveGateReplacement(
            Vector2Int position,
            string gateBuildingId,
            out Vector2Int replacedOrigin,
            out string replacedBuildingId)
        {
            replacedOrigin = position;
            replacedBuildingId = null;
            BuildingDefinition candidate =
                _placementBuildingRegistry?.GetById(gateBuildingId);
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    candidate,
                    out ReplacementPlacementRuleModule replacementModule))
            {
                if (replacementModule.MergeMode
                    == PlacementRuleMergeMode.Disabled)
                {
                    return false;
                }

                if (replacementModule.MergeMode
                    == PlacementRuleMergeMode.Override)
                {
                    if (!_objectsMapService.TryGetOccupant(
                            position,
                            out replacedBuildingId)
                        || !CanReplaceBuilding(
                            replacedBuildingId,
                            replacementModule))
                    {
                        replacedBuildingId = null;
                        return false;
                    }

                    replacedOrigin = ResolvePlacedOrigin(position);
                    return true;
                }
            }

            // Compatibility adapter for definitions not migrated to an explicit
            // replacement module yet.
            if (_wallTopologyService == null
                || _wallGateReplacementValidator == null
                || !_wallTopologyService.IsGate(gateBuildingId)
                || !_wallGateReplacementValidator.CanReplaceWallWithGate(
                    position,
                    gateBuildingId,
                    out replacedBuildingId))
            {
                return false;
            }

            replacedOrigin = ResolvePlacedOrigin(position);
            if (string.IsNullOrWhiteSpace(replacedBuildingId)
                && _objectsMapService.TryGetOccupant(position, out string occupantId))
            {
                replacedBuildingId = occupantId;
            }

            return !string.IsNullOrWhiteSpace(replacedBuildingId);
        }

        private bool RequiresReplacement(
            string buildingId,
            out ReplacementPlacementRuleModule module)
        {
            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(buildingId);
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out module))
            {
                return module.MergeMode
                    == PlacementRuleMergeMode.Override;
            }

            module = null;
            return _wallTopologyService != null
                && _wallTopologyService.IsGate(buildingId);
        }

        private static bool RequiresSameOwner(
            ReplacementPlacementRuleModule module)
            => module?.MergeMode == PlacementRuleMergeMode.Override
                ? module.RequireSameOwner
                : true;

        private bool IsReplacementSatisfiedByPendingMarker(
            string replacementBuildingId,
            string replacedPendingBuildingId,
            ReplacementPlacementRuleModule module)
        {
            if (string.IsNullOrWhiteSpace(replacedPendingBuildingId))
                return false;

            if (module?.MergeMode == PlacementRuleMergeMode.Disabled)
                return false;

            if (module?.MergeMode == PlacementRuleMergeMode.Override)
            {
                return CanReplaceBuilding(
                    replacedPendingBuildingId,
                    module);
            }

            // Compatibility adapter for unmigrated gates only.
            return _wallTopologyService != null
                && _wallTopologyService.IsGate(replacementBuildingId)
                && _wallTopologyService.IsWall(
                    replacedPendingBuildingId);
        }

        private bool CanReplaceBuilding(
            string replacedBuildingId,
            ReplacementPlacementRuleModule module)
        {
            if (module == null
                || string.IsNullOrWhiteSpace(replacedBuildingId))
            {
                return false;
            }

            if (ContainsId(
                    module.ReplaceableBuildingIds,
                    replacedBuildingId))
            {
                return true;
            }

            BuildingDefinition replaced =
                _placementBuildingRegistry?.GetById(replacedBuildingId);
            if (replaced == null
                || module.ReplaceableBuildingTags == null)
            {
                return false;
            }

            for (int index = 0;
                 index < module.ReplaceableBuildingTags.Length;
                 index++)
            {
                string tag = module.ReplaceableBuildingTags[index];
                if (ContainsTag(replaced.Tags, tag)
                    || ContainsTag(replaced.RuntimeTags, tag))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsId(
            System.Collections.Generic.IReadOnlyList<string> values,
            string expected)
        {
            if (values == null || string.IsNullOrWhiteSpace(expected))
                return false;

            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(
                        values[index]?.Trim(),
                        expected.Trim(),
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void RestoreBuildingFootprintOrLog(Vector2Int origin, string buildingId, string context)
        {
            if (TryRegisterBuildingFootprint(origin, buildingId))
                return;

            Debug.LogError(
                $"[MoyvaBuildGridDiag] footprint-rollback-failed context='{context}' building='{buildingId}' origin={origin}");
        }
    }
}
