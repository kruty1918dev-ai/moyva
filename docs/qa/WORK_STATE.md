# Work State — 2026-09-26 (water artifacts + waterfalls + water depth + seabed + pit fix + FBX tiles)

## Current task

**Water / seam artifacts (2026-09-26):**
- Factor-by-factor diagnostics on seed 6130: `material-audit.csv`
  (slots==submeshes on all 9 chunk renderers; water q3001 draws before
  waterfall q3000 = correct submerged-base overdraw), `waterfall-vfx.csv`,
  `clay_iso`/`clay_wall` (all slots → opaque URP/Lit), `wall_closeup`,
  `border_water` in `WorldVisualSmoke.DumpArtifacts`.
- Confirmed defect fixed: `WaterfallVfxSpawner` transform-X-squashed the
  SW3 emitters — Edge shape authored 8 m wide in `shape.scale.x` → blob;
  Splash 16 m shape sprayed walls on narrow fronts. Now transform stays
  uniform (`vfxScale`) and `shape.scale.x = frontWidth/uniformScale`.
- Confirmed-not-defects (kept): authored cliff chamfer seams + veg-card
  silhouettes (clay pass: no voids/UV tear), SW3 intersection-foam
  waterline, elevated water sheets on dark pits, aquatic props on water
  (`waterAffinity`), TransparentCutout veg (AlphaTest+40, ZWrite, clip).
- Already-fixed symptoms verified: sandy bands → seabed (`c3a14b9a`),
  stretched water plates → curtains (`94babbd3`).
- Planner tests 10/10 after change; smoke PASS same worldHash.
- Report: `docs/qa/WATER_ARTIFACTS_2026-09-26.md`; evidence:
  `docs/qa/evidence/artifacts-seed6130/`.

## Previous task

**Automatic SW3 waterfalls (2026-09-26):**
- Cause: `CollectWaterfallSource` drew stretched unit quads on the lake
  sheet material at every lower water neighbour (min 0.5 m) — turquoise
  plates, no lip/foam/flow.
- `RecipeWaterfallConfig` under `hydrology.waterfalls`: `minDropLevels`
  (2 × `TerrainHeightStep`), `curtainMaterial` + SW3 edge/splash/mist
  prefabs via `$asset` refs (registered in `MoyvaRuntimeAssetCatalog`).
- `WaterfallFieldPlanner` — drop edges on rendered sheets pouring onto
  plan-water; corner-diagonal suppression (own ortho pour OR flank-cell
  ortho pour into same lower); contiguous same-dir merge → fronts,
  anchor = middle cell → chunk owner + stable id.
- `WaterfallChunkMeshService` — 4-point lipped curtain profile per
  front, one mesh per chunk on `StylizedWater3_Waterfall.mat`
  (`_WORLDSPACEUV_ON` + dir (0,−1,0) = downward flow on any geometry),
  merged into the combined chunk mesh. `WaterfallVfxSpawner` — SW3
  prefabs under per-chunk `Waterfalls` roots, budget + particle caps.
- Legacy strips suppressed only when `IsActive && HasField`; legacy
  stays the fallback.
- `WorldVisualSmoke` +`DumpWaterfalls` (fronts csv, active txt, closeup
  shots) + fixed `TerrainPlanTests` water-material assertion.
- Verified seed 6130 same world: `fronts=5 vfxSystems=13`, drops
  2.22–3.22 m, verts 515861→515517, smoke PASS, 0 errors; planner
  tests 10/10. Report: `docs/qa/WATERFALLS_2026-09-26.md`; evidence:
  `docs/qa/evidence/waterfalls-seed6130/`.

## Previous task

**Water depth visibility (2026-09-26) — committed `1dd75d73`:**
- Runtime water was `StylizedWater3_NintendoStyle` via
  `TileWater.asset` `materialOverride`, with `_DepthHorizontal=0.01`
  → column-depth shading effectively off. `WaterLayerMaterialApplier`
  is dormant legacy (injected, never applied).
- Fix: `TileWater.asset` now overrides to Moyva-owned
  `WaterMaterial.mat` (same SW3 Standard shader; scene +
  `PlanarReflectionRenderer` already used it); retuned
  `_DepthHorizontal` 6.12→1.0, `_DepthVertical` 9.1→4.0 so fog spans
  the real 0.05–2 m seabed column.
- SW3 stock depth path (`_FogSource=0`, `_DisableDepthTexture=0`,
  URP Depth Texture + `StylizedWaterRenderFeature` on both assets)
  computes per-pixel world-space column → chunk-independent,
  camera-stable shore→deep gradient; no new shader/map.
- `WorldVisualSmoke` +`DumpWaterMaterial` (effective runtime material
  state) + `water_transect_top/low` shore→deep strip shots.
- Verified seed 6130 same world: sand readable at shallows, smooth
  fade, deep invisible; smoke PASS, 0 errors, no perf delta.
- Report: `docs/qa/WATER_DEPTH_2026-09-26.md`. Evidence:
  `docs/qa/evidence/waterdepth-seed6130/`.
- Note: user editor session opened the project mid-task — smoke
  requests must be cleaned if a second instance holds the lock.

## Previous task

**Continuous sloping seabed (2026-09-26) — committed `c3a14b9a`:**
- Replaced per-cell underwater "bed columns"
  (`TwcTileMeshSourceProvider.CollectWaterBedSource`) with one
  shared field + one triangle-fan mesh per chunk merged into the
  existing combined chunk mesh — no new GameObjects/materials.
- New `RecipeSeabedConfig` under `hydrology.seabed` (recipe + mirror +
  regenerated schema): shelf 1 m, falloff 3 m, exponent 1.6, max depth
  sea 2 / lake 1.2 / river 0.5 m, river falloff scale 0.4, shore recess
  0.02, border skirt 0.4.
- `SeabedFieldPlanner` (pure C#): multi-source Dijkstra over water →
  distance to land per cell (cannot cross land); shared `(w+1)×(h+1)`
  corner lattice → identical chunk-border heights; shore vertices
  anchor `min(land, waterline−recess)`; border skirts close map edges.
- `SeabedChunkMeshService` bound in `ChunkFirstFeatureBindings`;
  `ChunkTerrainMeshBuilder.Build` calls `Prepare` before the source
  plan; provider drops bed columns only when `IsActive && HasField`.
- Water mask = rendered-sheet criterion (`MainTerrain.SurfaceOnly &&
  Height < SurfaceHeight`) — same gate the columns used; 95 flush
  `water` tiles correctly carry no bed.
- Ordering fix: field heights come from `resolvedCells` at Build time;
  the level service only publishes `SurfaceHeightMap` post-Build on
  this path (was why the field never built in the first run).
- Verified seed 6130 same world `87D5247B44DA008A`: `fieldBuilt=True`,
  `beddedCells=914`, `uncoveredWaterTiles=95` (flush only),
  `nanVerts=0`, verts −26.7 % (703 344 → 515 861), gen 5.0 → 4.2 s,
  meshes/GOs/renderers unchanged. A/B shows uniform sandy bed vs
  divided blue plates; skirts render at map border.
- `maxAdjacentBedStep=3.22` only at legit waterfall walls; smoke PASS,
  error log empty. Full EditMode **891/891** (8 new planner tests).
- Report: `docs/qa/SEABED_2026-09-26.md`. Evidence:
  `docs/qa/evidence/seabed-seed6130/` (isolated topdown/iso/section
  renders + `seabedmap.csv` + water-on shots).

## Previous task

**Highland pits: no random base-level shafts inside mountains

**Highland pits: no random base-level shafts inside mountains
(2026-09-26) — committed `14976b65`:**
- Root cause NOT relief/planner: `SeaMask = invert(_baseLayer)` is pure
  perlin/CA noise independent of the relief field. Small isolated `false`
  holes inside `_baseLayer` land on relief 2.5–4.5 m, invert into `SeaMask`
  blobs, and the `Water` layer stamps `defaultHeight=-0.25` → ~3 m water
  shafts inside highland ("narrow deep breaks" in renders).
- Fix: new `TerrainHoleFillStep` (`terrain-hole-fill-step`) appended to
  `_baseLayer.steps` in `testgeneratorrecipe.json` (+ generated mirror +
  schema enum). Fills interior `false` components only when
  `cells ≤ MaxComponentCells(32)` AND ring relief median
  `≥ MinRingMedianMeters(1.5)` AND own-relief median within
  `MaxReliefDropMeters(1.0)` of the ring. No relief field → no-op.
- Applies at `_baseLayer` so Sea/Water/hydrology-sink/Sand/biomes all
  inherit consistently; border-connected sea, ≥33-cell lakes, lowland
  ponds and true basins are preserved by construction.
- Evidence: same-seed 6130 A/B `docs/qa/evidence/pitfix-seed6130/`
  (before `fbx-square-seed6130`): 10 pit components/53 cells → **0**;
  sea, river net (R=184) and 90-cell inland lake preserved; `empty=0`,
  `nanVerts=0`. Production-mask scans: 40 instances over
  48×48/96×96/mountain profiles × seeds → 0 classified pits.
- Regression test: `TerrainHoleFillStepTests` (unit cases +
  `ProductionMask_NoClassifiedPits_AcrossSeedsAndProfiles` real-recipe
  scan asserting zero pits and non-empty legitimate holes).
- Side fix: `FullGameIntegrationTests` repositioned units to the first
  BFS-adjacent cell only; height-aware fog can legitimately hide an
  adjacent cell across a terrain edge, and the changed 24x24 training
  world surfaced it — `MoveToSettlement` now rings the target until a
  visible adjacent cell is found.
- Recipe seed restored to 777. Full EditMode 883/883.
- Report: `docs/qa/HIGHLAND_PITS_2026-09-26.md`.

## Previous task (completed)

**Square tiles keep authored FBX shape — removed the slope warp
(2026-09-26):**
- Root cause: `ChunkTerrainMeshBuilder.ResolveWarpedMesh` →
  `TileSurfaceHeightWarpUtility` sheared each dual-grid land fragment onto a
  bilinear field of the four neighbours' continuous `SurfaceHeight` (jitter +
  level steps) → wavy same-level seams + ramped tops. Redundant: the dual-grid
  forms (`MatchesMain` opens a side on differing identity/height) +
  `edgeBottoms` side walls + stair modules already encode and close every
  transition.
- Removed the warp end-to-end: `ResolveWarpedMesh`, caches,
  `TileMeshCornerHeights`/`CornerHeights`, provider threading; deleted
  `TileSurfaceHeightWarpUtility` + `TileHeightWarpMeshKey`.
- Kept `ExactVertexWeldMeshUtility` (full-attribute weld, no averaging) and
  `ResolveBorderClampedMesh` (folds overshoot apron → flush rim; not the
  deformer).
- Evidence `docs/qa/evidence/fbx-square-seed6130/`, report
  `docs/qa/FBX_SQUARE_TILES_2026-09-26.md`. Seed 6130, `worldHash` unchanged,
  verts 699362→701058, nanVerts=0, 0 errors; `control_fbx` shows authored
  `grass_fill` == generated flat tiles.
- `WorldVisualSmoke` extended: movement/turn/deploy probe + `control_fbx`.
- EditMode 873/873 (was 877; −6 warp tests, +2 weld-preservation tests).

## Previous task (completed)

**Autonomous QA cycle (seed 5150) — gameplay-level coverage added
(2026-09-26):**
- Cycle ran on recipe seed **5150** (restored to user value 777 after);
  evidence `docs/qa/evidence/qa-cycle-seed5150/`, report
  `docs/qa/QA_CYCLE_2026-09-26_seed5150.md`.
- World: 48×48, 9 meshes, 726 272 verts, **nanVerts=0**, 0 errors,
  `worldHash=DD09595646CF924B` (deterministic). land=1254, water=1050;
  rivers+falls+confluence, no lake (legit variation).
- **New `RunGameplayProbe` in `WorldVisualSmoke.cs`** — closes the prior
  "world-gen only" gap. Resolves canonical services via Zenject
  containers (reflection, `TypeCache`; no parallel authority):
  `ConstructionService`, `UnitRecruitmentService`,
  `ConstructionLifecycleService`, `IUnitFactory`.
- Verified canonical paths end-to-end: castle-first bootstrap rule →
  castle-01 placed (Settlement 1) → barrack placed in radius →
  water/occupied/limit rejects w/ reasons → `TryRestoreOperational` →
  `GetOptions`=3 → `TryEnqueue` warrior queued (qid=1) → `GetQueue`=1 →
  `TryDeployReady` correctly "not ready" → `IUnitFactory.CreateUnit`
  spawned warrior. Proof shots `game_building_0/1`, `game_unit_0`.
- **No production defects** — only new-harness bring-up fixes (concrete-
  type vs interface resolution, struct null-check, castle-first order,
  UAC0005-safe TypeCache). Prior water-moiré + sedge fixes hold on
  seed 5150 (clean navy water, OOB 16 rim overhangs ≤0.6u).
- Verified: full EditMode **877/877**, smoke PASS deterministic, 0 errors.
- Honest gaps: `TryDeployReady` never reaches ready in the smoke window
  (turn advance not driven); insufficient-resource popup UI not exercised
  (enqueue succeeded → no shortage surfaced).

## Previous task (completed)

**Autonomous QA cycle (seed 4242) — fixes verified (2026-09-26):**
- Cycle ran on recipe seed **4242** (restored to user value 777 after);
  evidence `qa-cycle-seed4242{,-before,-mid,-mid2,-mid3}/`.
- **fix(water)**: rainbow "soap-bubble" moiré on all water from top-down —
  root cause was `_CausticsChromance` (chromatic voronoi caustics on the
  sand bed through transparent water) + `_RefractionChromaticAberration`.
  NintendoStyle material now: caustics off, CA 0, surface foam single off,
  translucency off, distance normals off, env reflections off,
  `_SunReflectionStrength` 0. Water reads clean navy at all distances.
- **fix(vegetation)**: giant sedge blades (7–14 unit meshes spanning cells
  and past the map rim) — `BuildSedgeMesh` bend used `lean·0.01·bh·60f`;
  replaced with `tan(lean°)·bh`. Bounds now 0.2–0.4 units; oob entries
  128→39 (rest = minor tree/stump rim overhangs ≤1.2u, P3).
- **regen-safety**: `MoyvaVegetationAssetBuilder.BuildAll()` hooked into
  `MoyvaAtlasPackImporter.Import()` (was menu-only).
- Verified: same worldHash twice (deterministic), nanVerts=0, 0 errors,
  full EditMode 877/877.
- Observed not-blocking: stump/log litter density reads busy in forests
  (P3); border-sink ring is a flat pale apron (known cosmetic);
  dark albedos keep world reading dark (documented caveat); smoke
  harness covers world-gen only — buildings/units/UI not exercised
  this cycle.
- Prior work committed in logical groups: rivers/terrain, vegetation,
  guidance/economy, TWC texture migration, cliff tiles, QA tooling.

## Previous task (completed)

**KayKit Cliff tiles → dual-grid migration — IMPLEMENTED & verified
(2026-09-26):**
- Report: `docs/plans/CLIFF_TILES_DUAL_GRID_REPORT.md`. Source
  `AtlasV3/Models/TileSet-1.fbx` (19 Cliff_* models) normalized by
  `MoyvaCliffTileAssetBuilder` (Editor): baked rot/scale, quad-centered
  pivots, top→Y0, high −0.5/low −0.25 walls, auto canonical yaw from
  top-face coverage (corner 90/edge 0/interior 180/merged 90 — preset
  offsets stay 0), face-soup faceted normals, rebuilt UVs.
- 10 shared meshes under `Generated/Meshes/Cliff/`; 9 theme materials
  under `Generated/Materials/Cliff/` (T_MOYVA_* albedo, smoothness
  0.12); **all themes share the same geometry — only texture differs**.
- 90 `{theme}_{form}[_low]` prefabs rewritten **in place** —
  root fileIDs preserved, `TilePreset.DUALGRD_*` refs verified intact.
- Hooked into `MoyvaAtlasPackImporter.Import()` → regen-safe.
- Verify: compile 0; focused Generator EditMode 135/135; smoke seed-777
  recipe ×2 identical `worldHash=2F557635C172624E`, verts=749 329,
  nanVerts=0, 0 errors; land/water counts unchanged (1351/953) — world
  logic untouched. Evidence `cliff-tiles-seed777{,-rerun}/`.
- Known: chunky slab seams + dark shadow-side walls = pack aesthetic /
  dark albedos; iridescent water specular is pre-existing.

## Previous task (completed)

**TWC dual-tiles + texture migration — IMPLEMENTED & verified (2026-09-26):**
- Plan: `docs/plans/TWC_DUAL_TILES_TEXTURE_MIGRATION_PLAN.md`; report:
  `docs/plans/TWC_DUAL_TILES_TEXTURE_MIGRATION_REPORT.md`.
- 7 `T_MOYVA_*_BaseColor` textures under `AtlasV3/Textures/Sources/`;
  `Moyva_AlbedoAtlas.png` rebuilt in place (2048×4096, 4×8 cells,
  authored UVs untouched → dual-tile half-composition preserved).
  Theme→texture map verified by mean colour.
- `Moyva_Atlas.mat` is albedo-only (`_Smoothness` 0.12); Normal/
  MetallicSmoothness/Roughness atlas maps deleted after ref check;
  `MoyvaAtlasPackImporter` rebuilds material without them.
- Smoothing removed: `FacetNormalsMeshUtility` per-face normals replace
  all `RecalculateNormals` on border-clamped copies, generated skirts,
  closures and side walls; authored faceted normals preserved.
- Stale tile-type presets repointed: `hill`→atlas-stone,
  `mountain`→atlas-rock_cliff, `lowland`/`forest-*`→atlas-grass.
  Legacy `tile-registry`/`id-mapping` JSONs keep third-party refs —
  dead config (disabled MapVisual path + orphaned build layers).
- Bark: `MoyvaVegetationAssetBuilder` now emits `VegBark.mat` +
  wrap-UV bark meshes (`veg-log`, `veg-twig-a/b`) — regen-safe.
- Tests: compile 0 errors; focused Generator suite green;
  **full EditMode 877/877 PASS**.
- Smoke: seed 42 ×2 identical `worldHash=5D8E6C1024CA46E8` (841 929
  verts, nanVerts=0); seed 777 `5187D641E927D132` (nanVerts=0).
  Evidence: `docs/qa/evidence/twc-migration-seed{42,42-rerun,777}/`.
- Caveat: world reads darker — supplied textures are intrinsically
  mid-dark (measured means in the report), not a rendering defect.

## Previous task (completed)

**Vegetation & world dressing — IMPLEMENTED (2026-09-26):**
- Plan + acceptance: `docs/plans/VEGETATION_AND_WORLD_DRESSING_PLAN.md`
  (§8 = implementation status, metrics, residual limits).
- New generator path: `EnvironmentDecorationGenerator.GenerateLayers` —
  additive per-cell layer rules (`DecorationLayerRule` in JSON
  `environment-decoration-config.json`, schema extended): water
  any/prefer/avoid/require, forest any/interior/edge/avoid, nearTree
  boost, `skipObjectCells`, shoreline exclusion, slope cap, per-layer
  scale range, per-layer noise salt.
- **Grove semantics**: `MarkTreeAnchorGroves` marks cells within 2 of
  spawned tree anchors as forest for affinity purposes — required because
  the recipe pipeline never emits `forest-*` tile ids (verified across
  seeds 42/777/12345/9/12 manifests; engine-side forests exist per
  `Generate_Forests_AppearAcrossSeedSpace`: 66/96 seed×archetype worlds).
- Assets: `MoyvaVegetationAssetBuilder` (Editor, `-executeMethod
  Kruty1918.Moyva.Generator.Editor.MoyvaVegetationAssetBuilder
  .BuildFromMenu`) → 27 `veg-*` prefabs under
  `Assets/Moyva/Generated/Vegetation/` (grass clump/cross/tall cards ×6,
  bush cards ×5 from unused bush_001..005 textures, ferns ×2, flower,
  twigs ×2, leaf patches ×2, moss, log, saplings ×3, sedge) +
  `VegPalette.png` + `VegFlat` opaque palette material + card materials;
  all on `DecorSharedStylized` with instancing ON (also enabled on legacy
  grass mats).
- Registry: 28 `veg-*` defs in `mapobjectregistry.json` + catalog keys in
  `MoyvaRuntimeAssetCatalog.prefab` (deterministic md5-path GUIDs).
- Chunk lifecycle: `EnvironmentDecorationSpawner` now registers each
  spawned renderer into `IMapVisualChunkRegistry` against its owning
  chunk → decorations hide/show with terrain under camera culling and
  fog (previously deco stayed visible on hidden chunks).
- Layer rules (14): grass .6, tallgrass .14 prefer-water + .22 edge,
  bush .16, fern .22 interior + .12 edge, litter .03+nearTree .7,
  flower .06, pebble .1 (rocky ×2.5, slope cap), log .05,
  sapling .14 edge + .03 meadow, stump .02+nearTree .45,
  reed .30 require-water r1.
- Perf (48×48 smoke): gen 4.7–6.4 s, 1658–2771 renderers, ~1.5 GB —
  within budget (gen <10 s, renderers <4k). No pre-change baseline
  (older manifests lacked perf fields).
- Wind: not added — `DecorSharedStylized` has no wind inputs (documented
  limitation, plan §7/§8).
- Tests: focused decoration 50/50; **full EditMode 877/877 PASS**.
- Evidence: `docs/qa/evidence/veg-seed{42,42-tuned,42-grove,9-forest,
  12-forest,777,12345}/`.

## Earlier task (completed)

**NintendoStyle water + visible sandy bed — DONE + verified (2026-09-25):**
- Request: use `StylizedWater3_NintendoStyle` for water; sand must be
  visible under water so the surface reads as stretching from the shore and
  disappearing into depth.
- Material: `materialOverride` now points at the NintendoStyle material
  (`e641fc50a6991f94488a292b9be307c9`) on `Assets/Moyva/TileWater.asset`
  and `BaseBlockPresetWater.asset` (recipe water tiles resolve the latter;
  override wins over prefab materials `WaterMaterial`/`SW3_Mobile`).
- Bed: water cells were `SurfaceOnly` sheets over void — nothing to see
  through. `CollectWaterBedSource` emits the atlas `sand` theme's fill tile
  as a `SolidTerrain` column under each water sheet: top lands on the
  hydrology `BedHeight`, `GeneratedClosure` deforms the authored bottom
  ring to a floor just below the deepest neighbouring bed, and per-edge
  bottoms stop open skirts exactly at each lower water neighbour's bed
  (higher cell owns the edge → exactly once; land/border sides occluded).
- Found while verifying: generated waterfall strips and the first bed
  attempt were `SurfaceOnly` → the upward-triangle filter silently dropped
  every vertical triangle, so VA-08 strips had never rendered. Both are
  `SolidTerrain` now; strips actually close the seams in v2 shots.
- Border fix: skirts at map edges with no neighbour are occluded; the
  column floor is `bedY − 0.5` (or just below the lowest real neighbour
  bed) — v1's dangling rim columns are gone; only a 0.5 m floor lip
  remains under rim water (reads as the sea floor edge).
- Proof: seed-42 + seed-12345 smoke PASS, world hashes unchanged
  (`85B4F2FADD94AC0D` / `239B47D25C36C327` — render-only), `nanVerts=0`,
  `empty=0`; `oob.txt` identical to VA-08 baseline (11 prop-edge entries,
  no TerrainMesh/water). Evidence: `docs/qa/evidence/water-nintendo-seed
  {42,12345}-v2/` — Nintendo caustic surface, sandy bed visible through
  shallows, cyan→navy depth fade, waterfall strips visible at drops.
- Compile: `smoke_compile` errors=0 (1744 sources).
- Cosmetic caveat: strips render as translucent panes (Nintendo material
  on vertical geometry); a dedicated `StylizedWater3_Waterfall` pass is a
  possible polish, not a defect.

Fifth pass complete: rivers/lakes visual audit + VA-08 fix.
Evidence: `docs/qa/evidence/audit-rl-seed{42,777,12345}/` (pre-fix) and
`docs/qa/evidence/audit-rl-seed{42,12345}-va08/` (post-fix).

**VA-08 FIXED + verified (2026-09-25):**
- Root cause: `CollectWaterfallSource` emitted a waterfall strip only on the
  single `FlowParent` edge → every other lower-water adjacency stayed an
  open vertical seam (13 edges ≥0.45 m on s-42; 26 on s-12345, max 2.72 m).
- Fix: `TwcTileMeshSourceProvider.CollectWaterfallSource` is now
  edge-driven — iterates all 8 neighbours, queries `IRecipeHydrologyMap
  .TryGetWaterSurface`, emits a strip when `upperY − lowerY ≥
  WaterfallMinDropMeters`. Upper cell owns each edge → no duplicates;
  non-water/border neighbours fail the surface query → no strips into
  void. `RecipeHydrologyPlan`/`IRecipeHydrologyMap` now expose
  `WaterfallMinDropMeters`.
- Proof: seed-42 `worldHash=85B4F2FADD94AC0D` and seed-12345
  `worldHash=239B47D25C36C327` both unchanged (render-only change);
  `nanVerts=0`, `empty=0` on both; post-fix crops show solid cyan closure
  walls at channel steps and the lake↔sea rim instead of dangling
  slivers; hydrology suite 17/17 PASS.
- **VA-09 VERIFIED-acceptable:** diagonal drops render 45° corner quads
  that read as thin water links in post-fix shots; no dedicated fix.

Full EditMode suite re-run: in progress at time of writing — the batch run
is starved by a concurrently open interactive editor; focused evidence
(17/17 hydrology + 2 smoke PASSes) already green. See PLAN_REVIEW.md.

## Previous task (completed)

Fourth pass: coherent rivers/lakes system per
`docs/plans/RIVERS_AND_LAKES_PLAN.md` (architecture, gaps, stages,
acceptance — statuses in §6 of that file).

**Implemented this round:**
- `RecipeHydrologyConfig.channelDepthMeters` (0.35, ≥0.05) + both recipe
  JSONs synchronized.
- `RecipeHydrologyPlan.BedHeight[,]` — river `min(terrain, surface−depth)`,
  lake real floor, sink = terrain; merged across plans.
- `MarkWaterfalls` compares **flooded** levels on river ∪ lake cells — flat
  lake basins can no longer fake drops; border parents excluded.
- `IRecipeHydrologyMap` += `TryGetBedHeight`, `TryGetFlowDirection`,
  `GetWaterKind` (None|River|Lake|Sink).
- Bed data path: `GeneratorMaskSession.BedOverrides` →
  `CompiledLayerMap.BedHeightOverride` → `LogicalTileMapBuilderService.
  ResolveCellData` → `WithHeights(bed, surface)` on the water-layer sample.
- Tests: chain-to-sink, monotonic descent ≤1 mm, lake-level uniformity, bed
  invariants, no-false-falls, store queries — 17/17 hydrology PASS.

**Proof:** seed-42 `waterstack.txt` — every River sample carries
`bed = surface − 0.35` (e.g. (40,6) bed 0.12 / surf 0.47); sea beds −0.25.
Seed-777: 53 rivers, 0 bed anomalies. Smoke PASS `nanVerts=0`, `empty=0`.
Evidence: `docs/qa/evidence/{hydro-seed42,hydro-seed777}`.
No Lake winners on seeds 42/777 — no ≥0.5 m depressions at this relief;
lake path covered by unit tests (`Depression_BecomesLakeWithFilledSurface`,
`LakeInterior_NoFalseWaterfalls`).

## Hydrology data contract (for shore/material/biome stages)

Authoritative source: `IRecipeHydrologyMap` (`RecipeHydrologyStore`, written
by `TileWorldCreatorWorldBuildBridge` per world build; `Replace`/`Clear` per
regen). All units meters, cell coords = tile coords (1 m cells, x→+X, y→+Z).

Stored per water cell (`RecipeHydrologyPlan`):
- `RiverMask`/`LakeMask`/`WaterfallMask` — classification; sinks are the
  sea layer + map border (no mask bit — use `GetWaterKind` == Sink).
- `WaterSurface` — surface Y = flooded level + `waterSurfaceOffsetMeters`.
- `BedHeight` — water bottom: river `min(terrain, surface−channelDepth)`,
  lake `min(terrain, surface−0.05)`, sink = terrain.
- `FlowParent` — D8 downstream index (flattened), −1 at sinks; acyclic.
- `Filled` — priority-flood level (rim/fill level); `Accumulation` — cells
  drained through this cell.

Computed on demand (store): `GetWaterKind`, `TryGetFlowDirection`
(downstream cell), `TryGetWaterfall` (downstream + upper/lower surface Y).
Channel width is always 1 cell; depth = `surface − bed` (≥
`channelDepthMeters` on rivers, ≥ `lakeMinDepthMeters` on lakes).

Rules for consumers:
- Water **sheets** render at `SurfaceHeight` (`SurfaceOnly`); terrain reads
  `Height` (= bed for water cells). Never recompute masks — query the store.
- Water surface is non-increasing along `FlowParent` (≤1 mm); lake cells of
  one basin share one level.
- Swamp = `SolidTerrain` water-like winner — NOT a `SurfaceOnly` sheet; do
  not treat as floating water.

Prior state below kept for history — VA-07 fix and earlier verification
unchanged.

**VA-07 — legacy shore-band expander (FIXED):**
`TileWorldCreatorShoreBandService.Expand` rewrote `BiomeMap` — a 1-cell
unconditional ring around ALL water, no height cap, no grading — on recipe
worlds too (its `HasAuthoredGeography` gate is never set). It stamped
`sand-shore-band` onto Dirt/Grass/Stone/RockCliff winners at 1.5–4.0 m
(plateau riverbanks), created direct sand↔snow borders, and duplicated
`TerrainShorePlanner` (second shoreline authority). Fix in
`TileWorldCreatorWorldBuildBridge.PrepareTerrainData`: expander now runs only
when `worldData.LogicalTileMap == null` (graph-produced worlds).

**Proof (`seed-42-noshore`):** `sand-shore-band` 182→0; heightmap
byte-identical (0 cell diffs); tilemap diff = exactly the 182 cells →
grass/hill; sand dist-to-water {1:671, 2:9} (was {1:642, 2:218, 3:2}); zero
sand↔snow / sand↔rock-cliff neighbors (was: snow pairs at 4.0 m); all
remaining sand >1 m is `TerrainShorePlanner` lifts to river waterline
(+0.01 signature in `sandstack.txt`). `worldHash` changed (tile ids only).
EditMode **816/816 PASS** post-fix (`validation/tests-editmode-20260925_041806.xml`).

**Also corrected prior claims:** "wide sand = mask height cap" was wrong
(VA-03 reclassified — cause was the expander + planner lifts); `empty=0`
proves logical occupancy only, not rendered-geometry continuity (mesh-level
proof = `oob.txt` + stitched shots, already in evidence).

Round-3 evidence: `docs/qa/evidence/validation/seed-42-{d,e,noshore}`,
`seed-12345-noshore` (new dumps: `surfaceheightmap.csv`,
`terrainlevelmap.csv`, `layernames.csv`, `sandstack.txt`).

- Determinism: seed-42 ×3 → identical `worldHash=AAFCB590DF989988`.
- New harness fix: deletes `Temp/__Backupscenes` before exit — prevents Unity's
  "Scene Backup Detected" modal that blocked one launch ~13 min.
- VA-03 evidence strengthened: sand aprons 3–6 cells wide on flats
  (`seed-12345/game_orbit_s`, `game_default`).

- Compile: 0 errors (`Logs/Editor.log`, Tundra build success).
- EditMode: **816/816 PASS** after the last code change —
  `docs/qa/evidence/validation/tests-editmode-20260925_021007.xml`.
- Player build: **Success** — `Build/PlayerSmoke/MoyvaJsonSmoke.exe`
  (StandaloneWindows64, dev build, all enabled scenes; `Build/` is gitignored).
- Determinism: two seed-42 launches → identical `worldHash=AAFCB590DF989988`,
  identical object counts (779 GO / 470 MF), genTime 4.0–4.1 s.
- Seed 777 re-run: distinct world `C048BAE3CD9BD011`, PASS; `game_shore` gray =
  fog-of-war on an unrevealed cell (correct, not a flake).
- Smoke error log: 0 entries (`BeginStaticPreview` pairing fix killed the
  ~4700-line PRU noise).

## Code under audit

- HEAD `83ad5ea3c` + ~196 uncommitted files (user's terrain/hydrology/preset
  work preserved; agent changes: `ChunkTerrainMeshBuilder.cs`,
  `WorldVisualSmoke.cs`, `JsonizationBuildService.cs`, recipe JSONs, `docs/qa/*`).
- Recipe `testgeneratorrecipe.json` — `seed: 12345` for the VA-07 verification
  run; **restore to `0` in both copies after it** (seed-42 runs used `0`).

## Reproduction

```
printf 'out=docs/qa/evidence/<run_id>\n' > Library/ai/visual-smoke.request
"$LOCALAPPDATA/Unity/Editors/6000.6.2f1/Editor/Unity.exe" -projectPath <repo>
# exits by itself; summary in Library/ai/visual-smoke.summary
# NOTE: Temp/ai is wiped per editor launch — copy artifacts before next run.
```

Seed override: set `"seed": N` in BOTH recipe JSONs (launch-context seed does
not survive play-mode domain reload), restore to 0 after the run.
Player build: `-batchmode -executeMethod
Kruty1918.Moyva.Jsonization.Editor.JsonizationBuildService.DevelopmentPlayerSmokeFromCli -quit`.

## Verified scenarios

- Seeds 42 (×3 launches), 12345, 777 — world 48×48, `empty=0`, `nanVerts=0`,
  zero TerrainMesh OOB verts, flush map rim on all borders.
- Rivers/shores/terrain-joints pass (plan doc
  `docs/plans/RIVERS_AND_TERRAIN_FIX_PLAN.md`), seeds 42/777/12345:
  - 184 river cells each, acc-order violations 0, orphan/broken river cells 0,
    every river cell inside a connected water component (777: two legit
    3-cell streams ending in closed depressions — true local minima).
  - land-below-rendered-water = 0 on all three seeds (swamp waterTileIds fix).
  - Waterfall strips now anchored to rendered surfaces: fix8 renders show no
    floating panes/fins (previously ~600 strips lifted by sink
    pseudo-surfaces up to ~3 m above the sheet).
  - Terrain joints solid on all rendered angles; props grounded.
- Construction overlay verified live: green = buildable grass, red =
  sand/water/unrevealed; grid hugs tile bounds (initial-castle onboarding).
- Real game camera inspected from all four compass directions at low angle
  (`game_orbit_*`) plus default/iso/topdown/shore poses.
- Evidence: `docs/qa/evidence/{seed-42,seed-12345,seed-777,seed-42-v3,
  seed-12345-v2,fix5-seed42..fix8-seed42,seed777,seed12345,validation/*}` —
  same camera poses for before/after.

## Status of findings

- **VA-01 border slivers — FIXED + verified** (`ResolveBorderClampedMesh`).
- **VA-02 startup build-grid — RESOLVED**, intended initial-castle onboarding.
- **VA-03 wide sand aprons — RECLASSIFIED** (was the expander, see VA-07).
- **VA-04/VA-05 harness artifacts — FIXED + verified.**
- **VA-06 prop canopy overhang ≤0.3 cell — LOW cosmetic, no action.**
- **VA-07 legacy shore-band expander — FIXED + verified** on seeds 42 and
  12345 (sand-shore-band 182→0 / all 506 sand at dist-1; every elevated sand
  cell = riverbank lift at `waterSurface+0.04`; zero sand↔snow; `side_s`
  pixel-identical pre/post → no geometry regression). EditMode 816/816 PASS.
- **menu→Gameplay — VERIFIED**: `StartupBarrierSmoke` menu mode PASS —
  HomeMenu → ConfigureMenuNewGame → StartGameAsync → Gamplay_Scene world
  32x32 (size preset 0), assignments=1, errors=0.
  `validation/barrier-smoke-menu.summary`.
- **host/multiplayer barrier — VERIFIED**: host mode PASS — world 128x128,
  hostReady=true, input actually observed blocked during load, errors=0.
  `validation/barrier-smoke-host.summary`.
- Mesh caches hold stale destroyed-mesh references across regen — bounded
  managed-wrapper leak, guarded by `!= null`; housekeeping only.
- **Rivers/chunk pass — FIXED + verified** (2026-09-26):
  - Hydrology: topological accumulation, path-traced `MarkRivers` with
    diagonal brackets + sink receivers, water-channel carve pass in
    `TerrainPlanApplicationService`, stair flights touching water dropped,
    route cells skip water.
  - Shore: noise-gated sand band (`bandCoverage` 0.55), shore-lift early-out
    fixed, wash-sheet guard (no wash above land), `waterTileIds` extension
    so Swamp-layer water participates in shore grading.
  - Waterfall strips: plan pseudo-surfaces replaced by rendered
    neighbour surfaces + 0.1 m lip overlap — floating panes/fins removed.
  - Chunk ownership: prop renderers register to the owning
    `MapChunk_X_Y` ancestor instead of every bounds-overlapping chunk;
    `ChunkOwnershipTests` 4/4 pass.
  - EditMode: full suite 830/830 PASS (three stale tests updated to
    the new receiver-marking/Nintendo-material semantics).
- Harness lesson: `Temp/` is wiped at every editor launch — write
  `barrier-smoke.request` only after the editor is up; same applies to
  test-result XMLs (copy to `docs/qa/evidence/` before next launch).

## Explicitly NOT verified

Camera in continuous motion (orbit stills as proxy); fog-of-war transition
animation; spawned unit/building visuals; underwater views; in-session world
regeneration (no gameplay regen path exists — only menu-preview regen;
restart determinism + registry clearing verified instead).

## Economy action guidance (2026-09-26)

Shared "what blocks my action" system for construction + recruitment —
plan + economy map in `docs/plans/ECONOMY_ACTION_GUIDANCE_PLAN.md`.

- `GameplayGuidanceResolver` composes canonical queries only
  (`GetResourceProjection`, `TryGetPendingPlacementStatus`,
  `TryGetEnqueueShortages`, `ProducerFeasibilityResolver`, portfolio +
  lifecycle + production snapshot) — no parallel validation, no mutation.
- Deliberate `ConfirmPlacement` rejection -> guidance popup with every
  deficit (Need/Have/Missing) + per-blocker actions; preview/hover never
  opens it; repeated clicks reuse the single session.
- `Recruit` `InsufficientResources` -> goal-linked deduped notification +
  popup; notification click reopens guidance with live data.
- Producer suggestions are state-aware: producing -> focus; under
  construction -> progress; idle -> workers/inputs hint (staffing is
  automatic); feasible -> "Build X" opens construction, clears filters,
  jumps pages, highlights the card ~6 s; cyclic/blocked -> honest
  "unobtainable". Supply-wagon + queue-release options where applicable.
- Resume-goal re-validates: pending kept -> reopen confirm flow; cleared ->
  re-select building + `TryPreviewAt(saved tile)`; recruitment -> reopen
  building panel on Recruit tab; deleted target -> explains itself.
- Popup blocks map input (full-screen shield in `SyncInputShields`);
  Esc/X peels popup before panels (`GameplayHtmlPresenter` +
  `bridge.ClosePanel`).
- Economy fix: `tavern:cook-steak` (2 wheat -> 3 steak, workerless) removes
  the documented steak deadlock — barracks/stable units are producible
  via tavern after starter stock runs out.
- Tests: `GameplayGuidanceResolverTests` 13, `GameplayGuidanceStateTests` 12,
  guidance action-map sweep 1 — 27/27 pass; `GameplayHtmlActionMapTests`
  12/12; `ClosePanel` 22/22; feasibility 8/8; recruitment shortage 7/7;
  compile clean.
- Limits: page jump is instant (paged list, no scroll animation);
  client-role remote rejections degrade to reason text; notifications are
  in-memory only.

## Next action

Rivers, sandy banks, terrain joints, waterfall strips and chunk-owned prop
rendering verified on seeds 42/777/12345 (evidence `fix8-seed42`, `seed777`,
`seed12345`). Remaining optional polish: waterfall strips still use the
opaque Nintendo water material (no dedicated waterfall theme exists in the
atlas) — acceptable as cascade faces, revisit only if a waterfall material
is added. Bright cyan rim line along map-border sea cells is the border
sink sheet — cosmetic.
