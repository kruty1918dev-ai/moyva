using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.JsonConfig;
using Newtonsoft.Json;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>JSON-backed compatibility facade. Definitions are auto-discovered from JSON.</summary>
    [Serializable]
    public sealed class BuildingRegistrySO : JsonConfigObject, IBuildingRegistry
    {
        public WallCollectionDefinition[] WallCollections = Array.Empty<WallCollectionDefinition>();

        [JsonIgnore]
        public BuildingDefinitionAsset[] BuildingAssets =>
            JsonConfigRuntime.GetAll<BuildingDefinitionAsset>().ToArray();

        [JsonIgnore]
        public BuildingDefinition[] LegacyBuildings => Array.Empty<BuildingDefinition>();

        public void SetBuildingAssets(IEnumerable<BuildingDefinitionAsset> assets)
        {
            throw new InvalidOperationException(
                "Building registry is JSON-discovered. Add/remove Assets/Moyva/Presets/Buildings/*.json instead of mutating an Inspector list.");
        }

        public BuildingDefinition[] GetAll()
        {
            return JsonConfigRuntime.GetAll<BuildingDefinitionAsset>()
                .Where(asset => asset != null)
                .Select(asset => asset.ToRuntimeDefinition())
                .Where(definition => definition != null)
                .ToArray();
        }

        public BuildingDefinition GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            BuildingDefinitionAsset asset = JsonConfigRuntime.Get<BuildingDefinitionAsset>(id);
            return asset?.ToRuntimeDefinition();
        }

        public BuildingDefinitionAsset GetAssetById(string id)
        {
            return string.IsNullOrWhiteSpace(id)
                ? null
                : JsonConfigRuntime.Get<BuildingDefinitionAsset>(id);
        }

        public BuildingDefinition[] GetByCategory(BuildingCategory category)
            => GetAll().Where(x => x != null && x.Category == category).ToArray();

        public WallCollectionDefinition[] GetWallCollections()
            => WallCollections ?? Array.Empty<WallCollectionDefinition>();

        public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId)) return null;
            return GetWallCollections().FirstOrDefault(collection =>
                collection != null && collection.ContainsBuilding(buildingId));
        }
    }
}
