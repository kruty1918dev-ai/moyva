using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Kruty1918.JsonConfig
{
    /// <summary>
    /// Host-supplied configuration for <see cref="JsonConfigRuntime"/>.
    /// The package is game-agnostic: the host (game composition/bootstrap) provides
    /// Resources locations, model-type allow-list prefixes and legacy document filters.
    /// Must be passed to <see cref="JsonConfigRuntime.Configure"/> before the first
    /// <see cref="JsonConfigRuntime.EnsureLoaded"/> call.
    /// </summary>
    public sealed class JsonConfigRuntimeSettings
    {
        /// <summary>Default settings instance used when the host never calls Configure.</summary>
        public static readonly JsonConfigRuntimeSettings Default = new();

        /// <summary>
        /// Resources subfolder that contains the generated runtime JSON TextAssets
        /// (e.g. "MoyvaConfigGenerated" loads <c>Resources/MoyvaConfigGenerated/*.json</c>).
        /// </summary>
        public string GeneratedResourcesFolder = string.Empty;

        /// <summary>
        /// Resources path of the prefab carrying a <see cref="JsonAssetCatalog"/> component
        /// that resolves <c>$asset</c> references. Empty string disables catalog lookup.
        /// </summary>
        public string AssetCatalogResourcePath = string.Empty;

        /// <summary>
        /// Namespace prefixes allow-listed for runtime config model types
        /// (<see cref="JsonConfigObject"/> subclasses resolvable from JSON metadata).
        /// Empty list allows every namespace.
        /// </summary>
        public List<string> ModelNamespacePrefixes = new();

        /// <summary>
        /// Namespace prefixes allow-listed for polymorphic/inline Unity object types
        /// resolved by <see cref="JsonConfigTypeRegistry"/> and converters.
        /// Empty list allows every namespace.
        /// </summary>
        public List<string> RegistryNamespacePrefixes = new();

        /// <summary>
        /// Optional suffixes stripped from CLR type names when deriving stable JSON ids.
        /// </summary>
        public List<string> StableIdSuffixes = new()
        {
            "Definition",
            "Config",
            "Settings",
            "Profile",
            "Node",
            "SO",
            "Asset"
        };

        /// <summary>
        /// Optional host resolver mapping a config CLR type to its stable schema name.
        /// Return null/empty to fall back to the default <c>"&lt;prefix&gt;.&lt;stable-id&gt;"</c>
        /// convention via <see cref="SchemaPrefix"/>.
        /// </summary>
        public Func<Type, string> SchemaNameResolver;

        /// <summary>Schema prefix used by the default schema convention.</summary>
        public string SchemaPrefix = "jsonconfig";

        /// <summary>
        /// Optional host filter run on each indexed JSON document. Return true to skip
        /// the document (e.g. legacy editor-only exports that must not enter the runtime).
        /// </summary>
        public Func<JObject, string, string, bool> DocumentFilter;
    }
}
