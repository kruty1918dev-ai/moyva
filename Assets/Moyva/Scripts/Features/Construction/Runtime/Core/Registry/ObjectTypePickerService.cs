using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class ObjectTypePickerService : IObjectTypePicker
    {
        private const bool VerboseLogs = true;

        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IAutoTileVariantResolver _autoTileVariantResolver;

        public ObjectTypePickerService(
            IBuildingRegistry buildingRegistry,
            IAutoTileVariantResolver autoTileVariantResolver)
        {
            _buildingRegistry = buildingRegistry;
            _autoTileVariantResolver = autoTileVariantResolver;
        }

        public IReadOnlyList<TopologyCaseType> SupportedCases => _autoTileVariantResolver.SupportedCases;

        public bool TryPickId(string sourceBuildingId, TopologyNeighborMask mask, out string resolvedBuildingId)
        {
            resolvedBuildingId = null;

            var collection = _buildingRegistry.GetWallCollectionByBuildingId(sourceBuildingId);
            if (collection == null)
                return false;

            var caseMap = BuildCaseMap(collection);
            if (caseMap.Count == 0)
                return false;

            if (!_autoTileVariantResolver.TryResolveId(mask, caseMap, out _, out var resolvedCase))
                return false;

            var variants = GetVariants(collection, resolvedCase);
            if (variants == null || variants.Count == 0)
                return false;

            resolvedBuildingId = ChooseRandom(variants);

            if (VerboseLogs)
            {
                Debug.Log(
                    $"[ObjectTypePicker] source='{sourceBuildingId}' mask(N={mask.North},E={mask.East},S={mask.South},W={mask.West}) " +
                    $"resolvedCase={resolvedCase} resolvedId='{resolvedBuildingId}' variants={variants.Count}");
            }

            return !string.IsNullOrWhiteSpace(resolvedBuildingId);
        }

        private static IReadOnlyDictionary<TopologyCaseType, string> BuildCaseMap(WallCollectionDefinition collection)
        {
            var map = new Dictionary<TopologyCaseType, string>();
            var bindings = collection.TopologyBindings;
            if (bindings == null)
                return map;

            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding == null || binding.VariantBuildingIds == null || binding.VariantBuildingIds.Count == 0)
                    continue;

                for (int j = 0; j < binding.VariantBuildingIds.Count; j++)
                {
                    if (!string.IsNullOrWhiteSpace(binding.VariantBuildingIds[j]))
                    {
                        map[binding.CaseType] = binding.CaseType.ToString();
                        break;
                    }
                }
            }

            return map;
        }

        private static List<string> GetVariants(WallCollectionDefinition collection, TopologyCaseType caseType)
        {
            var bindings = collection.TopologyBindings;
            if (bindings == null)
                return null;

            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding == null || binding.CaseType != caseType)
                    continue;

                var result = new List<string>();
                if (binding.VariantBuildingIds == null)
                    return result;

                for (int j = 0; j < binding.VariantBuildingIds.Count; j++)
                {
                    var id = binding.VariantBuildingIds[j];
                    if (!string.IsNullOrWhiteSpace(id))
                        result.Add(id);
                }

                return result;
            }

            return null;
        }

        private static string ChooseRandom(IReadOnlyList<string> variants)
        {
            if (variants.Count == 0)
                return null;

            if (variants.Count == 1)
                return variants[0];

            int index = UnityEngine.Random.Range(0, variants.Count);
            return variants[index];
        }
    }
}
