using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Owns building-replacement policy.
    ///
    /// This component decides whether a candidate requires replacement,
    /// what existing building can be replaced, and whether legacy gate/wall
    /// compatibility rules satisfy the replacement requirement.
    ///
    /// It does not mutate construction state.
    /// </summary>
    internal sealed class ConstructionReplacementPolicy
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallGateReplacementValidator
            _wallGateReplacementValidator;
        private readonly ConstructionFootprintStore _footprints;

        public ConstructionReplacementPolicy(
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            IWallTopologyService wallTopologyService,
            IWallGateReplacementValidator wallGateReplacementValidator,
            ConstructionFootprintStore footprints)
        {
            _buildingRegistry = buildingRegistry
                ?? throw new ArgumentNullException(nameof(buildingRegistry));
            _objectsMapService = objectsMapService
                ?? throw new ArgumentNullException(nameof(objectsMapService));
            _wallTopologyService = wallTopologyService;
            _wallGateReplacementValidator =
                wallGateReplacementValidator;
            _footprints = footprints
                ?? throw new ArgumentNullException(nameof(footprints));
        }

        public bool TryResolveGateReplacement(
            Vector2Int position,
            string gateBuildingId,
            out Vector2Int replacedOrigin,
            out string replacedBuildingId)
        {
            replacedOrigin = position;
            replacedBuildingId = null;

            BuildingDefinition candidate =
                _buildingRegistry.GetById(gateBuildingId);

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
                        || !CanReplace(
                            replacedBuildingId,
                            replacementModule))
                    {
                        replacedBuildingId = null;
                        return false;
                    }

                    replacedOrigin =
                        _footprints.ResolveOrigin(position);
                    return true;
                }
            }

            // Compatibility adapter for definitions not migrated to an
            // explicit replacement module yet.
            if (_wallTopologyService == null
                || _wallGateReplacementValidator == null
                || !_wallTopologyService.IsGate(gateBuildingId)
                || !_wallGateReplacementValidator
                    .CanReplaceWallWithGate(
                        position,
                        gateBuildingId,
                        out replacedBuildingId))
            {
                return false;
            }

            replacedOrigin = _footprints.ResolveOrigin(position);

            if (string.IsNullOrWhiteSpace(replacedBuildingId)
                && _objectsMapService.TryGetOccupant(
                    position,
                    out string occupantId))
            {
                replacedBuildingId = occupantId;
            }

            return !string.IsNullOrWhiteSpace(replacedBuildingId);
        }

        public bool RequiresReplacement(
            string buildingId,
            out ReplacementPlacementRuleModule module)
        {
            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);

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

        public bool RequiresSameOwner(
            ReplacementPlacementRuleModule module)
        {
            return module?.MergeMode
                    == PlacementRuleMergeMode.Override
                ? module.RequireSameOwner
                : true;
        }

        public bool IsReplacementSatisfiedByPendingMarker(
            string replacementBuildingId,
            string replacedPendingBuildingId,
            ReplacementPlacementRuleModule module)
        {
            if (string.IsNullOrWhiteSpace(
                    replacedPendingBuildingId))
            {
                return false;
            }

            if (module?.MergeMode
                == PlacementRuleMergeMode.Disabled)
            {
                return false;
            }

            if (module?.MergeMode
                == PlacementRuleMergeMode.Override)
            {
                return CanReplace(
                    replacedPendingBuildingId,
                    module);
            }

            // Compatibility adapter for unmigrated gates only.
            return _wallTopologyService != null
                && _wallTopologyService.IsGate(
                    replacementBuildingId)
                && _wallTopologyService.IsWall(
                    replacedPendingBuildingId);
        }

        public bool CanReplace(
            string replacedBuildingId,
            ReplacementPlacementRuleModule module)
        {
            if (module == null
                || string.IsNullOrWhiteSpace(
                    replacedBuildingId))
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
                _buildingRegistry.GetById(replacedBuildingId);

            if (replaced == null
                || module.ReplaceableBuildingTags == null)
            {
                return false;
            }

            for (int index = 0;
                 index < module.ReplaceableBuildingTags.Length;
                 index++)
            {
                string tag =
                    module.ReplaceableBuildingTags[index];

                if (ContainsTag(replaced.Tags, tag)
                    || ContainsTag(
                        replaced.RuntimeTags,
                        tag))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsId(
            IReadOnlyList<string> values,
            string expected)
        {
            if (values == null
                || string.IsNullOrWhiteSpace(expected))
            {
                return false;
            }

            for (int index = 0;
                 index < values.Count;
                 index++)
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

        private static bool ContainsTag(
            IReadOnlyList<string> tags,
            string value)
        {
            if (tags == null
                || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (int index = 0;
                 index < tags.Count;
                 index++)
            {
                if (string.Equals(
                        tags[index]?.Trim(),
                        value.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
