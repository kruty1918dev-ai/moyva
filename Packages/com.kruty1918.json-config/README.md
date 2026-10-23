# Kruty1918 JSON Config

![UPM package](https://img.shields.io/badge/UPM-package-blue)
![version](https://img.shields.io/github/v/tag/kruty1918dev-ai/com.kruty1918.json-config?label=version&sort=semver)

Schema-driven JSON configuration runtime for Unity: `Load -> Validate -> Resolve -> Freeze -> Consume`.

## Install (Unity Package Manager)

Package Manager → **+** → **Add package from git URL**:

```
https://github.com/kruty1918dev-ai/com.kruty1918.json-config.git
```

or in `Packages/manifest.json`:

```json
"com.kruty1918.json-config": "https://github.com/kruty1918dev-ai/com.kruty1918.json-config.git#v0.1.0"
```

## Configuration

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

## Releasing / updating

`main` is wired to CI that auto-tags releases: bump `"version"` in
`package.json`, push to `main`, and the `UPM release` workflow tags
`v<version>` automatically. Consumers pinned to a tag
(`...git#v0.1.0`) upgrade by changing the tag in `manifest.json`;
consumers on `...git` (HEAD) get the latest `main` on next resolve.
