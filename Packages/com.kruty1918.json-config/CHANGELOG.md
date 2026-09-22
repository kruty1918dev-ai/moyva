# Changelog

## 0.1.0 — 2026-09-22

- Initial extraction from `Kruty1918.Moyva.Jsonization` (Moyva project).
- Renamed types: `MoyvaJsonRuntime`→`JsonConfigRuntime`, `MoyvaJsonTypeRegistry`→`JsonConfigTypeRegistry`, `MoyvaJsonConfigObject`→`JsonConfigObject`, `MoyvaJsonAssetCatalog`→`JsonAssetCatalog`, `MoyvaJsonBindingMarker`/`Record`→`JsonBindingMarker`/`Record`, `MoyvaJsonContractResolver`→`JsonConfigContractResolver`, `MoyvaJsonDocumentMetadata`→`JsonDocumentMetadata`, `MoyvaJsonObjectFactory`→`JsonObjectFactory`.
- Hard-coded host specifics (Resources paths, `Kruty1918.Moyva`/`GiantGrey.*` namespace allow-lists, `moyva.*` schema map, legacy Construction document filter) moved to injected `JsonConfigRuntimeSettings`.
