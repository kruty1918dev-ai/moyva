using System;
using System.Collections.Generic;
using Kruty1918.JsonConfig;
using Newtonsoft.Json.Linq;

namespace Kruty1918.Moyva.Jsonization
{
    /// <summary>
    /// Moyva-owned wiring for the com.kruty1918.json-config package: Resources locations,
    /// model type allow-list prefixes, Moyva schema names and the legacy editor-document
    /// filter that used to be hard-coded inside the runtime.
    /// </summary>
    public static class MoyvaJsonRuntimeSettings
    {
        public static JsonConfigRuntimeSettings Create()
        {
            return new JsonConfigRuntimeSettings
            {
                GeneratedResourcesFolder = "MoyvaConfigGenerated",
                AssetCatalogResourcePath = "MoyvaRuntimeAssetCatalog",
                ModelNamespacePrefixes = new List<string> { "Kruty1918.Moyva" },
                RegistryNamespacePrefixes = new List<string>
                {
                    "Kruty1918.Moyva",
                    "GiantGrey.TileWorldCreator",
                },
                StableIdSuffixes = new List<string>
                {
                    "BuildingModuleDefinition",
                    "BuildingModule",
                    "Definition",
                    "Config",
                    "Settings",
                    "Profile",
                    "Node",
                    "SO",
                    "Asset",
                },
                SchemaPrefix = "moyva",
                SchemaNameResolver = SchemaForConfigType,
                DocumentFilter = IsLegacyEditorOnlyConstructionDocument,
            };
        }

        private static string SchemaForConfigType(Type type)
        {
            if (type == null) return null;
            if (type.Name == "BuildingDefinitionAsset") return "moyva.building";
            if (type.Name == "UnitClassConfig") return "moyva.unit";
            if (type.Name == "GeneratorMapRecipe") return "moyva.generator-recipe";
            if (type.Name == "WorldCreationDefaultsSO") return "moyva.world-creation";
            return null;
        }

        /// <summary>
        /// Pass82 exported two Construction editor-authoring families into the generated
        /// Resources folder. They were never runtime JsonConfigObject models, so they must
        /// not participate in the runtime registry.
        /// </summary>
        private static bool IsLegacyEditorOnlyConstructionDocument(
            JObject root,
            string model,
            string schema)
        {
            if (root == null || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(schema))
                return false;

            var migration = root["migration"] as JObject;
            string sourceAssetPath = migration?.Value<string>("sourceAssetPath");
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
                return false;

            string normalizedPath = sourceAssetPath.Replace('\\', '/');
            const string legacyTemplateRoot =
                "Assets/Moyva/Data/ScriptableObjects/Construction/Templates/";
            if (!normalizedPath.StartsWith(legacyTemplateRoot, StringComparison.OrdinalIgnoreCase))
                return false;

            return
                (string.Equals(model, "building-archetype", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-archetype", StringComparison.OrdinalIgnoreCase)) ||
                (string.Equals(model, "building-template-library", StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(schema, "moyva.building-template-library", StringComparison.OrdinalIgnoreCase));
        }
    }
}
