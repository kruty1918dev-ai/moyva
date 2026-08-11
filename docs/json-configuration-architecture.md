# Moyva JSON configuration architecture

Canonical authoring source: `Assets/Moyva/Presets/**/*.json`.

Runtime pipeline: JSON discovery -> deserialize -> schema/semantic validation -> Unity asset ID resolution -> normalize -> freeze -> plain C# runtime models/repositories.

`Assets/Moyva/Resources/MoyvaConfigGenerated/` and `MoyvaRuntimeAssetCatalog.prefab` are machine-generated build artifacts and must never be edited manually.

Every canonical JSON document contains `$schema`; VS Code/other JSON-Schema-aware editors therefore get completion without a custom IDE extension or hardcoded per-domain mapping.

Project-owned ScriptableObjects are migration input only and are removed after parity and reference gates pass. Third-party Unity assets may remain opaque asset references.
