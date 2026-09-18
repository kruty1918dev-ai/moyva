# World Generation Overhaul — Plan

Goal: deterministic, visually convincing, strategically playable, multiplayer-fair
procedural worlds at every supported map size (64/128/256 and custom), without a
chaotic rewrite of the working render pipeline.

## Audit findings (branch base: game-process @ 387a07ef)

Runtime path today:

1. `GeneratorWorldStartupBuilder` → `MapVisualWorldBuildOrchestrator.BuildWorld()`
2. `MapVisualWorldDataFactory` → `IMapDataGenerator` = `GraphTwcMapDataGenerator`
   → `GraphTwcMapGenerationPipeline`: seed (`GameLaunchContext` else graph
   `ISeedProvider`), map size (launch dims → `MapChunkSizePolicy` crop to 16),
   graph semantic validation, graph compile → TWC blueprint/build layer sync,
   `manager.ExecuteBlueprintLayers()`, logical tile map export.
3. `GeneratedWorldDataIntegrityService` repairs null/blank maps.
4. `TileWorldCreatorWorldBuildBridge` → terrain policy → **chunk-first**
   (`MergedChunksWithPrecomputedHeights`): `ChunkFirstWorldBuildService` resolves
   per-cell compositions from `GraphLogicalTileMap` (`ResolvedTileCompositionResolver`
   picks main terrain by `SurfaceHeight`), autotiles via TWC `TilePreset`s, welds
   into one mesh per 16×16 chunk, emits side walls from neighbour surface deltas.
5. `MapVisualGridWriter` writes canonical gameplay tile ids to `IGridService`.
6. `IGeneratorTerrainLevelService` receives `TerrainLevelMap` + surface heights.
7. `EnvironmentDecorationGenerator`/`Spawner` place sprite prefabs under chunk roots.
8. `WorldGeneratedDataSignal` → `StartingPositionSelector` → spawn assignments.

Confirmed gaps vs. the target state:

- Active graph is `testgeneratorgraph` (not `generatorgraph` — that file is stale,
  version 1, dangling connections). It produces a trivial world: water + CA
  landmass + sand shore + one grass layer. No archetypes.
- `MapType`/`MapTypePreset` and densities are stored on world data but **never
  consumed** by generation.
- No hydrology at runtime: river config (`RiverDataConfig`, connection rules)
  exists but the old `RiverFeatureGenerator` pipeline was deleted; nothing emits
  rivers, lakes or water-depth bands in the graph path.
- `ObjectMap`/`BuildingMap` are always empty; the TWC id mapping has no
  object/building layers → no objects spawn. `mapobjectterrainconfig` rules empty.
- Decorations spawn 2D `SpriteRenderer` prefabs (`green-tree-*`, `green-forest-*`
  in `mapobjectregistry`); bushes/grass unimplemented; one GameObject per object.
- Terrain levels exist (`_applyIntegerTerrainHeights=1`, water 0 / shore 1 /
  land 1 / hill 3 / max 5, `heightStep=1`) but are derived *post-hoc* from
  biome ids by `TileWorldCreatorTerrainLevelResolver`, then normalized — they do
  not come from a real height field, so mountains cannot form chains.
- Shore band service converts water-adjacent land to `sand-shore-band` and raises
  its level to the water neighbour level — a ring, not a beach.
- No world validation, fairness measurement, retry policy, or perf telemetry.
- `GameLaunchContext` carries `Seed`, `Size`, `Width`, `Height`, `MapType`,
  `MaxPlayers`, `Difficulty`; `TryGetWorldDimensions` maps size 0/1/2 → 32/64/128.

Assets available (free, in repo): KayKit Medieval Hexagon (CC0): dual-grid TWC
presets (sand/grass/dirt/snow/autumn/spring/summer/stone/swamp), hex meshes,
`tree_single_A/B`, `trees_A/B` S/M/L (+`_cut`), `rock_single_A–E`,
`hill_single_A–C`, `hills_A–C`(+trees), `mountain_A–C`(+grass/+grass+trees),
waterlilies/waterplants, props (resource_lumber/stone, tents, crates), buildings
in 4 colours. PolyOne rocks. Stylized Water 3 materials. TWC tile presets.

Infra rules honoured: JSON under `Assets/Moyva/Presets/` is source of truth;
`MoyvaJsonRuntime.Get<T>(id)` loads config; `$asset` refs resolve via
`Resources/MoyvaRuntimeAssetCatalog.prefab` (key `asset.<type>.<slug>.<guid8>`,
resolved by `editorPath`); `JsonizationExportService.SyncGeneratedResources`
copies Presets→`Resources/MoyvaConfigGenerated`; `BuildAssetCatalog` rebuilds
the catalog prefab. Runtime ground-traversal semantics: `water`/`mountain`
impassable, `hill`/`forest`/`sand`/`snow` passable with cost.

## Target architecture

The generator produces a **complete logical world** in one deterministic engine;
existing render/grid/save infrastructure consumes it unchanged.

```
IMapDataGenerator (MoyvaWorldMapDataGenerator)
  └─ WorldGeographyEngine  (pure C#, seeded, deterministic)
       ├─ LandmassStage     archetype macro-geography (continent mask)
       ├─ ElevationStage    ridged ranges, hills, valleys, shelf → float field
       ├─ HydrologyStage    depression fill, flow accumulation, lakes,
       │                    rivers w/ tributaries, mouths, fords
       ├─ CoastStage        beaches (variable width), shallow/deep water
       ├─ BiomeStage        tile ids: water, sand, grass, lowland,
       │                    forest-sparse/dense, hill, mountain, snow
       ├─ ObjectStage       resource POIs (lumber/stone), parity-aware
       └─ FairnessStage     spawn candidates, opportunity parity,
                            connectivity, quality gates → verdict
  └─ GeographyLogicalMapFactory → GraphLogicalTileMap
       (per-cell GraphTileLayerSample: TileId, BuildLayerGuid, PresetId,
        LayerKind, SurfaceHeight = level × step)
  └─ GeographyBuildLayerProvisioner
       ensures MoyvaTerrainHeightAwareTilesBuildLayer per visual family
       (water/sand/grass/forest/hill/mountain/snow) in TWC configuration,
       presets resolved from tile-type defs (visual.variants[].preset)
```

Chunk-first then renders: autotiled dual-grid fragments between *different*
build-layer families (coastline), per-cell terrace heights + side walls.

Key invariants:

- Same `GraphTwcMapGenerationResult` contract → `GraphTwcMapDataState` →
  `GeneratedWorldData`; `TerrainLevelMap` carried through result/state and
  marked authored so `NormalizeForTileWorldCreator` does not flatten it.
- `ForceChunkFirstCompositeBuild = true` on produced data.
- Rivers are `water` gameplay cells at shore level with `sand` ford cells at
  deterministic intervals (ground-passable), guaranteeing connectivity.
- Deterministic retry: attempt i uses `seed_i = Hash(seed, i)`; report kept on
  `GeneratedWorldData` for diagnostics/tests.
- Decoration v2: 3D KayKit pools via `mapobjectregistry` ids; clustered fields
  driven by tile id + level + moisture proxies; per-chunk caps; grass as
  per-chunk combined card mesh, not GameObjects.

Config: `Assets/Moyva/Presets/Generator/world-generation/worldgenerationconfig.json`
→ `WorldGenerationConfig : MoyvaJsonConfigObject` (archetype params, fairness
thresholds, retry policy, decoration tables). No new ScriptableObject config.

Graph system stays for editor tooling/previews; runtime no longer depends on
node-graph correctness for world quality.

## Milestones

1. Engine + integration + build layers (world renders, real levels, biomes).
2. Hydrology + coasts + rivers/lakes/fords.
3. Fairness validation + retry + spawn hints.
4. 3D decoration pools + grass cards + object spawns.
5. Tests + perf measurement + docs + PR.

## Implementation status (feature/worldgen-overhaul)

Landed:

- `Runtime/Geography/` engine: `LandmassStage` → `ElevationStage` →
  `TerrainLevelStage` → `HydrologyStage` (priority-flood + D8 flow) →
  `CoastStage` (beaches, shallow bands) → `BiomeStage` (canonical tile ids,
  moisture/forest fields, POI objects on passable land) → `FairnessStage`
  (spawn scoring, opportunity parity, mutual connectivity via deterministic
  river fords) orchestrated by `WorldGeographyEngine` with seeded retries.
- `GeographyMapGenerationStep` runs inside `GraphTwcMapGenerationPipeline`
  ahead of the graph compile path; when `world-generation-config` is enabled
  it returns a `GraphTwcMapGenerationResult` with authored `TerrainLevelMap`,
  `HasAuthoredGeography`, spawn hints and the generation report. The graph
  path remains as fallback when the config is absent/disabled.
- `GeographyBuildLayerProvisioner` provisions `geography-<tileId>`
  `MoyvaTerrainHeightAwareTilesBuildLayer`s from the canonical TWC id
  mapping; `GeographyLogicalMapFactory` projects results into
  `GraphLogicalTileMap` (terrain + `ObjectSpawn` samples).
- `GraphTwcMapDataState`/`IGraphTwcMapDataDiagnostics` carry terrain levels,
  spawn hints and the geography report into `GeneratedWorldData` via
  `MapVisualWorldDataFactory`.
- `TileWorldCreatorWorldBuildBridge` skips legacy level normalisation and
  shore-band expansion for authored geography.
- `StartingPositionSelector` prefers engine-validated spawn hints, verified
  against local hard gates, with legacy selection as fallback.
- `WorldGeneratedDataSignal.SpawnHints` transports hints via the existing
  signal path.
- JSON: `Presets/Generator/world-generation/worldgenerationconfig.json` +
  schema; runtime copy `moyva-world-generation--world-generation-config.json`.
  TWC id mapping now points terrain ids at KayKit dual-grid presets and
  defines `resource-lumber`/`resource-stone` object layers; the runtime asset
  catalog gained KayKit preset/prefab entries; `mapobjectregistry` gained
  `kaykit-*` tree/rock definitions and `environment-decoration-config` pools
  now reference them (3D prefabs instead of sprite prefabs).
- Tests: `WorldGeographyEngineTests` (determinism, level/tile coherence,
  river-to-water tracing, spawn validity, object placement, archetype
  diversity, best-effort recovery) — 18/18 passing; a full multi-archetype
  generation runs in ~75 ms per candidate (EditMode measurements).
- Runtime export: `MoyvaJsonDocumentLinkReconciler.RebuildFromMenu`
  regenerates the full `Resources/MoyvaConfigGenerated` set from Presets
  (batch mode does not auto-sync; invoke the menu/`executeMethod`).

## Known limitations

- Rivers and lakes render as `water` tiles; depth/elevation difference is
  carried by surface height, not distinct visual tile ids.
- Grass decoration cards are emitted through the existing
  `EnvironmentDecorationGenerator` pools (3D KayKit prefabs), not a
  combined per-chunk card mesh.
- Beach/desert/ford cells emit canonical `sand` (`beach` resolves to it via
  alias); all emitted ids are first-class `tile-type` presets.
