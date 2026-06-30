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
        }

        /// <summary>Отримати всі будівлі реєстру.</summary>
        public API.BuildingDefinition[] GetAll()
        {
            var result = new List<API.BuildingDefinition>();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var assets = BuildingAssets;
            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (asset == null)
                    continue;

                var definition = asset.ToRuntimeDefinition();
                result.Add(definition);
                if (!string.IsNullOrWhiteSpace(definition.Id))
                    ids.Add(definition.Id);
            }

            var legacy = LegacyBuildings;
            for (int i = 0; i < legacy.Length; i++)
            {
                var definition = legacy[i];
                if (definition == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(definition.Id) && ids.Contains(definition.Id))
                    continue;

                result.Add(definition);
            }

            return result.ToArray();
        }

        public API.WallCollectionDefinition[] GetWallCollections() => WallCollections ?? System.Array.Empty<API.WallCollectionDefinition>();

        /// <summary>Знайти будівлю за її ID. Повертає null якщо не знайдено.</summary>
        public API.BuildingDefinition GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var asset = GetAssetById(id);
            if (asset != null)
                return asset.ToRuntimeDefinition();

            return System.Array.Find(LegacyBuildings, b => b != null && b.Id == id);
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
        public API.BuildingDefinition[] GetByCategory(API.BuildingCategory category) =>
            System.Array.FindAll(GetAll(), b => b != null && b.Category == category);

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
