# Tile Migration Report — AtlasV3 (no-water) pack

Migration of the `Moyva_Integration_NoWater` tile pack (9 themes, 99 meshes)
into the Moyva runtime. Pack contract: `Assets/Moyva/Art/World/Tiles/AtlasV3/ASSET_CONTRACT_UA.md`.

## New assets (imported, deterministic GUIDs)

- `Assets/Moyva/Art/World/Tiles/AtlasV3/` — pack payload: `Textures/` (4 atlas maps +
  layout), `Unity/MoyvaMeshData.json` (native mesh data), `QA/`, contract/validation docs,
  `tile_manifest.json`.
- `Assets/Moyva/Art/World/Tiles/AtlasV3/Generated/` — 99 mesh assets, 99 prefabs,
  9 dual-grid `TilePreset` assets, shared `Moyva_Atlas.mat` (URP Lit; albedo sRGB,
  normal map, packed metallic(RGB=0)/smoothness(A)).
  Regenerate via `Tools/Moyva/Atlas V3/Import Atlas Pack Assets`
  (`MoyvaAtlasPackImporter`, idempotent, path-md5 GUIDs).

## Configuration

- `Assets/Moyva/Presets/Generator/atlas-tile-set/atlas-tile-set.json` — theme → preset +
  low/stair prefab wiring, physical contract values (quantum 0.25 m, low border 0.25 m,
  stair rise 0.25 m, overlay offset 0.003 m). Consumed by `AtlasTileSetCatalog`.
- `Assets/Moyva/Presets/Tiles/{grass,sand,dirt,stone,snow,swamp,rock-cliff,road,footpath,stair}.json`
  — tile types now point at `atlas-*` presets. `water.json` unchanged: project water is an
  independent surface system, intentionally kept.
- `Assets/Moyva/Presets/Generator/recipes/testgeneratorrecipe.json` — biome layers
  (swamp/dirt/sand/grass/stone/rock-cliff/snow) gated by `terrain-level-mask-step` over the
  relief field; `terrainRelief`, `passages`, `routes` sections enabled.
- `Assets/Moyva/Presets/Movement/*.json` — `heightLimits`: autoStepMax 0.25 m,
  stairModuleRise 0.25 m, maxStairRise 1.0 m; per-class passability/cost.

## Runtime wiring

- `Runtime/Terrain/`: `TerrainReliefPlanner` (relief field), `TerrainPassagePlanner`
  (stair flights: deltaH/0.25 modules, entrance spacing), `TerrainRoutePlanner`
  (road/footpath overlay layer), `TerrainPlanApplicationService`,
  `AtlasTileSetCatalog`, `AtlasDualGridShapes` (mask→form+rotation, low-variant rule).
- `MapGenerationPipeline` calls relief planner + terrain-plan applier; results flow into
  `GeneratedWorldData.TerrainPassages` (saved/restored via `GeneratedWorldSaveModule`).
- `TwcTileMeshSourceProvider` resolves atlas presets, emits dual-grid fragments once,
  demotes covered flat seams to fill, picks low variants for ≤0.25 m open drops, and
  emits stair modules from the passage store.
- Traversal: `Grid/API/TerrainTraversalContracts.cs` (`DirectWalk`/`Stair`/`Blocked`,
  `ITerrainPassageMap`), `UnitTraversalPolicy` enforces profile height limits.

## Removed

- `Assets/Moyva/Tiles/Cliff_*.prefab` (+meta, 15 files) — old cliff tile prefabs.
  Audit: no GUID references in scenes/prefabs/assets/JSON, no Resources or string-path
  loads. Recoverable via git (`git checkout -- Assets/Moyva/Tiles`).

## Kept intentionally

- `Assets/Moyva/Art/Models/Tiles/{sCLIFF,TilesCliff0,2}.fbx` — authoring source of the
  pack's grass geometry; unreferenced at runtime but retained for provenance.
- `Assets/Moyva/TileWater.asset`, `Presets/Tiles/water.json`, Water recipe layer —
  the project's own water surface system (pack ships no water; contract forbids
  resurrecting a water tileset but also forbids removing independent water systems).
- ThirdParty KayKit/TileWorldCreator packs — shared props/buildings still in use;
  `hill`, `mountain`, `lowland`, `forest-*` tile types still reference KayKit presets.
  They are not emitted by the active recipe (no layer uses them) and remain available
  for the dormant geography path; repointing them is deferred pending a design decision.
- `Presets/Grid/tile-registry/*` and `tile-world-creator-id-mapping` legacy entries —
  still feed menu preview and legacy id aliasing; movement-cost data, not runtime visuals.

## Verification

- `Kruty1918.Moyva.Generator(.Editor|.Tests.Runtime)` compile clean (dotnet build).
- `Tests/Runtime/TerrainPlanTests.cs` covers relief/passage planning.
- Pending (needs the Unity Editor; a batch run is blocked while the editor is open):
  EditMode suite, PlayMode world build, in-game screenshots.
- `Tools/Moyva/Atlas V3/Create Validation Scene` builds
  `Assets/Moyva/Scenes/AtlasTilesValidation.unity`: all 16 masks ×9 themes (high+low),
  assembled 6×6 platforms per theme, stair flights 1–4 modules and all 4 directions.

## Rollback

- `git checkout -- Assets/Moyva/Tiles` restores old cliff prefabs.
- Repoint `Presets/Tiles/*.json` presets to the previous TWC/KayKit assets and remove
  `terrainRelief`/`passages`/`routes` from the recipe to revert visuals.
- `AtlasV3/Generated/` is fully reproducible from `Unity/MoyvaMeshData.json` + textures
  via the importer menu item.

## Known limitations

- `autoStepMaxMeters = 0.25` is a configured design value, not a measured controller
  limit; verify against the real unit movement controller in PlayMode.
- Stair modules on open (unwalled) outer edges still need authored side closures —
  the pack does not ship them; passage planner rejects candidates without support.
- `dirt`, `stone`, `rock-cliff`, `footpath`, `stair` traversal classes currently use
  profile fallback rules (passable, cost 1.0) rather than explicit `classRules`.
- Validation scene and screenshots require an editor session; batch verification was
  not possible while the project was open in the editor.
