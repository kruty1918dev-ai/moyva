# Work State — 2026-09-25 (Nintendo water + sandy bed verified)

## Current task

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
- Construction overlay verified live: green = buildable grass, red =
  sand/water/unrevealed; grid hugs tile bounds (initial-castle onboarding).
- Real game camera inspected from all four compass directions at low angle
  (`game_orbit_*`) plus default/iso/topdown/shore poses.
- Evidence: `docs/qa/evidence/{seed-42,seed-12345,seed-777,seed-42-v3,
  seed-12345-v2,validation/*}` — same camera poses for before/after.

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
- Harness lesson: `Temp/` is wiped at every editor launch — write
  `barrier-smoke.request` only after the editor is up; same applies to
  test-result XMLs (copy to `docs/qa/evidence/` before next launch).

## Explicitly NOT verified

Camera in continuous motion (orbit stills as proxy); fog-of-war transition
animation; spawned unit/building visuals; underwater views; in-session world
regeneration (no gameplay regen path exists — only menu-preview regen;
restart determinism + registry clearing verified instead).

## Next action

Nintendo water + bed verified; working tree committed as per-feature
commits. Remaining: full EditMode suite re-run (batch run was starved by a
concurrently open interactive editor — re-run
`tools/ai/unity-editmode-tests-quiet.sh` when the editor is free), then
optional waterfall-material polish pass.
