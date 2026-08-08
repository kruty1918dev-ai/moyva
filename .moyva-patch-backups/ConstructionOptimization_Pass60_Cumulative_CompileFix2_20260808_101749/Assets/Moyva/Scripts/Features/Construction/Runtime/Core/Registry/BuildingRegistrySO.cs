using Kruty1918.Moyva.Construction.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    [CreateAssetMenu(menuName = "Moyva/Construction/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject, IBuildingRegistry
    {
        [SerializeField] private BuildingDefinitionAsset[] _buildingAssets = Array.Empty<BuildingDefinitionAsset>();

        [NonSerialized]
        private Dictionary<string, API.BuildingDefinition> _runtimeById;
        [NonSerialized]
        private API.BuildingDefinition[] _runtimeDefinitions;
        [NonSerialized]
        private int _runtimeCacheRevision = int.MinValue;

        [Header("Legacy Inline Definitions")]
        [Tooltip("Legacy data kept only for migration. New runtime/editor flows should use Building Definition assets above.")]
        public API.BuildingDefinition[] Buildings;

        public API.WallCollectionDefinition[] WallCollections;

        public BuildingDefinitionAsset[] BuildingAssets => _buildingAssets ?? Array.Empty<BuildingDefinitionAsset>();
        public API.BuildingDefinition[] LegacyBuildings => Buildings ?? Array.Empty<API.BuildingDefinition>();

        public void SetBuildingAssets(IEnumerable<BuildingDefinitionAsset> assets)
        {
            if (assets == null)
            {
                _buildingAssets = Array.Empty<BuildingDefinitionAsset>();
                BuildingDefinitionAsset.NotifyRuntimeRegistryChanged();
                InvalidateRuntimeCache();
                return;
            }

            var unique = new List<BuildingDefinitionAsset>();
            var seen = new HashSet<BuildingDefinitionAsset>();
            foreach (var asset in assets)
            {
                if (asset == null || !seen.Add(asset))
                    continue;

                unique.Add(asset);
            }

            _buildingAssets = unique.ToArray();
            BuildingDefinitionAsset.NotifyRuntimeRegistryChanged();
            InvalidateRuntimeCache();
        }

        /// <summary>Отримати всі будівлі реєстру.</summary>
        public API.BuildingDefinition[] GetAll()
        {
            EnsureRuntimeCache();
            return _runtimeDefinitions;
        }

        public API.WallCollectionDefinition[] GetWallCollections() => WallCollections ?? System.Array.Empty<API.WallCollectionDefinition>();

        /// <summary>Знайти будівлю за її ID. Повертає null якщо не знайдено.</summary>
        public API.BuildingDefinition GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            EnsureRuntimeCache();
            return _runtimeById.TryGetValue(
                    id,
                    out API.BuildingDefinition definition)
                ? definition
                : null;
        }

        public BuildingDefinitionAsset GetAssetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var assets = BuildingAssets;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] != null && string.Equals(assets[i].Id, id, StringComparison.OrdinalIgnoreCase))
                    return assets[i];
            }

            return null;
        }

        /// <summary>Отримати всі будівлі заданої категорії.</summary>
        public API.BuildingDefinition[] GetByCategory(
            API.BuildingCategory category)
        {
            EnsureRuntimeCache();
            return System.Array.FindAll(
                _runtimeDefinitions,
                b => b != null && b.Category == category);
        }

        private void EnsureRuntimeCache()
        {
            int revision = BuildingDefinitionAsset.RuntimeRevision;
            if (_runtimeDefinitions != null
                && _runtimeById != null
                && _runtimeCacheRevision == revision)
            {
                return;
            }

            var result = new List<API.BuildingDefinition>();
            _runtimeById ??=
                new Dictionary<string, API.BuildingDefinition>(
                    StringComparer.OrdinalIgnoreCase);
            _runtimeById.Clear();

            BuildingDefinitionAsset[] assets = BuildingAssets;
            for (int index = 0; index < assets.Length; index++)
            {
                BuildingDefinitionAsset asset = assets[index];
                if (asset == null)
                    continue;

                API.BuildingDefinition definition =
                    asset.ToRuntimeDefinition();
                if (definition == null)
                    continue;

                result.Add(definition);
                if (!string.IsNullOrWhiteSpace(definition.Id))
                    _runtimeById[definition.Id] = definition;
            }

            API.BuildingDefinition[] legacy = LegacyBuildings;
            for (int index = 0; index < legacy.Length; index++)
            {
                API.BuildingDefinition definition = legacy[index];
                if (definition == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(definition.Id)
                    && _runtimeById.ContainsKey(definition.Id))
                {
                    continue;
                }

                result.Add(definition);
                if (!string.IsNullOrWhiteSpace(definition.Id))
                    _runtimeById[definition.Id] = definition;
            }

            _runtimeDefinitions = result.ToArray();
            _runtimeCacheRevision = revision;
        }

        private void InvalidateRuntimeCache()
        {
            _runtimeDefinitions = null;
            _runtimeById?.Clear();
            _runtimeCacheRevision = int.MinValue;
        }

        private void OnValidate()
        {
            InvalidateRuntimeCache();
            BuildingDefinitionAsset.NotifyRuntimeRegistryChanged();
        }

        public API.WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return null;

            var source = WallCollections ?? System.Array.Empty<API.WallCollectionDefinition>();
            for (int i = 0; i < source.Length; i++)
            {
                var collection = source[i];
                if (collection != null && collection.ContainsBuilding(buildingId))
                    return collection;
            }

            return null;
        }
    }
}
