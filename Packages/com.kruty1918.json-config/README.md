# Kruty1918 JSON Config

Schema-driven JSON configuration runtime for Unity: `Load -> Validate -> Resolve -> Freeze -> Consume`.

The package is game-agnostic. The host configures it once at startup via
`JsonConfigRuntime.Configure(settings)` before the first `EnsureLoaded()`:

```csharp
JsonConfigRuntime.Configure(new JsonConfigRuntimeSettings
{
    GeneratedResourcesFolder = "MyGameConfigGenerated",   // Resources subfolder with JSON TextAssets
    AssetCatalogResourcePath = "MyRuntimeAssetCatalog",   // Resources prefab carrying JsonAssetCatalog
    ModelNamespacePrefixes  = new() { "MyGame" },          // allow-list for config model types
    RegistryNamespacePrefixes = new() { "MyGame" },        // allow-list for polymorphic/inline types
    SchemaPrefix = "mygame",                               // default "schema" convention prefix
    SchemaNameResolver = t => null,                        // optional per-type schema override
    DocumentFilter = (root, model, schema) => false,       // optional skip predicate
});
JsonConfigRuntime.EnsureLoaded();
```

Config models derive from `JsonConfigObject` (plain C#, not ScriptableObject).
JSON documents carry `schema`/`id`/`model` metadata; polymorphic type ids are
stable allow-listed ids, never CLR type names. `$asset` references resolve through
the `JsonAssetCatalog` prefab; `$config` references resolve between documents.

No dependency on any host-game sources, scenes or assets.
