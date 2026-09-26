# Rivers & Terrain Fix Plan

Date: 2026-09-26. Base: `improvement/moyva-production-polish` @ `c670cc3e`
(Nintendo water + sandy bed verified). Scope: recipe generator pipeline
(`RecipeHydrologyPlanner`), `TerrainShorePlanner`, `TwcTileMeshSourceProvider`,
`MapVisualChunkDiscoveryRebuildService` / `MapVisualChunkRootService`.

Reference seed: **42** (48×48, `worldHash=85B4F2FADD94AC0D` pre-fix).
Additional seeds: **777**, **12345**.
Diagnostic evidence: `docs/qa/evidence/fix-audit-seed42/`
(`hydromap.csv`, `tilemap.csv`, `waterstack.txt`, screenshots).

## Data path traced (verified in code)

```
TerrainReliefPlanner → reliefField (0.5 m quantization)
 → GeneratorMaskSession.GetHydrologyPlan → RecipeHydrologyPlanner.Build
     · PriorityFlood (Barnes, seeded at sink-layer cells + border)
       records flood parent per cell; filled[c] ≥ filled[parent] always
     · Accumulate — order = filled descending
     · MarkLakes  — filled−terrain ≥ LakeMinDepth, capped by fraction
     · MarkRivers — acc ≥ 10 && terrain ≥ 0.25 && !sink && !lake,
                   scan-order cap at RiverMaxFraction·N (0.03 → 69 cells)
     · MarkWaterfalls — filled drop ≥ 0.5 toward flood parent
 → HydrologyMaskStep copies RiverMask/LakeMask into layer masks
   + SetSurfaceOverride/SetBedOverride
 → LogicalTileMapBuilderService → TileStackCell samples
   (water = SurfaceOnly winner; surface = WaterSurface, bed = BedHeight)
 → TerrainPlanApplicationService → TerrainShorePlanner.Apply
   (Chebyshev distance to water winners; bandCells=1 convert→sand,
    blendCells=1 height cap; lift 0.04, rise 0.2/cell, maxDrop 0.9)
 → ChunkFirst build → ResolvedTileCompositionResolver
   (winner + WaterSurface sample for water-adjacent land)
 → TwcTileMeshSourceProvider (fill/dual terrain, SurfaceOnly water sheet,
   CollectShoreWaterSource wash, CollectWaterfallSource strips,
   CollectWaterBedSource sand columns)
 → ChunkTerrainMeshBuilder per-chunk meshes → MapChunk_X_Y roots
 → ChunkFirstObjectSpawner → MapChunk_X_Y/Objects prop roots
 → MapVisualChunkDiscoveryRebuildService bounds-overlap registration
 → MapVisualChunkRegistry OR-visibility (camera + fog)
```

## P1/P2 — Rivers render as scattered puddles (DATA defect, confirmed)

### Reproduction
Seed 42 → `fix-audit-seed42` smoke. `hydromap.csv` vs `tilemap.csv`:

- 48 river cells; **every one** renders a water tile (no winner-stage loss).
- **0/48 river cells have a 4-connected river neighbour**; 33/48 flow links
  are diagonal (corner-touching quads).
- Flow parents of river cells are overwhelmingly **non-water** land cells,
  e.g. `(40,6)→(41,7)`, `(23,25)→(22,25)` — chains die inside terrain.
- Water connectivity overall: 71 connected components, 52 of them ≤5 cells —
  the "puddles in oversized sand" symptom.
- Water cells sit at the 0.5 m terrace levels (0.47/0.97/1.47/…) → these are
  individually marked cells on one drainage path, not a channel.

### Confirmed cause — three compounding planner bugs

1. **Accumulation ordering is not topological.**
   `Accumulate` sorts by `filled` descending only. PriorityFlood produces
   huge equal-height plateaus (0.5 m quantized relief), where a cell's flood
   parent can sort *before or after* it arbitrarily (`Array.Sort` is
   unstable on ties). When a plateau child is processed after its parent,
   its contribution still lands in `acc[parent]` **but the parent has
   already forwarded a smaller sum** to the grandparent → accumulation is
   systematically under-propagated downstream.
   Proof: `acc[parent] < acc[child]` — impossible under correct ordering —
   occurs **80 times** on seed 42 (e.g. `(6,21) acc 13 → parent (6,20)
   acc 4`).

2. **Threshold-only marking.** `MarkRivers` marks single cells where
   `acc ≥ threshold`; it never traces a path. With starved acc, a channel
   keeps only its highest-acc cells → isolated puddles. Two additional
   breaks: `RiverMinSourceMeters` rejects low-terrain **path** cells
   (river dies just before reaching the sea), and `count >= cap` truncates
   in array scan order mid-river.

3. **Diagonal-only links.** Flood parents are D8; a diagonal step leaves two
   water quads touching at a single vertex → visually disconnected even when
   both cells are marked.

### Required changes — `RecipeHydrologyPlanner`
- `Accumulate`: process cells in **provable topological order** — sort by
  distance-to-sink along the parent chain (memoized `dist[c] =
  dist[parent]+1`, sinks 0), descending. Guarantees children before parents
  on every plateau; `acc` becomes monotone along any chain.
- `RefineParents`: replace raw flood parent with **steepest-descent on
  `filled`**: choose the lowest-filled neighbour; strictly-lower only —
  plateaus keep the (acyclic) flood parent; ties prefer cardinal then a
  deterministic hash. Produces mostly-cardinal, genuinely-downhill routes.
- `MarkRivers` → path carving: source candidates = `acc ≥ threshold &&
  terrain ≥ minSource && !sink && !lake`, sorted acc-desc (index
  tie-break). Each candidate traces its parent chain to a receiver
  (sink/lake/existing river/border); the whole traced path commits if it
  fits the remaining cell budget — no mid-river truncation. Diagonal hops
  additionally mark the better (lower-filled, deterministic-tie) of the two
  bracketing orthogonal cells → 4-connected channel; bracket surface
  = `min(filled[bracket], filled[hop-source])` so water never steps upward.
- Recipe `riverMaxFraction` 0.03 → 0.08 (69 → ~184 cells at 48²): a real
  river network does not fit in 69 cells.

### Post-fix confirmed defects (fix2-seed42 diagnostics)

1. **Winner-loss on unflooded channel cells (composition defect).**
   `ResolveCellData` applies `SurfaceHeightOverride` only to the water
   layer's own samples. On path cells where `filled == terrain`
   (unflooded flat), the water surface = terrain − 0.03 < land surface —
   the grass winner hides the channel (e.g. cell (33,19): mask `R`,
   renders `grass`). Fix: `TerrainPlanApplicationService.CarveWaterChannels`
   (new pass before the shore planner) lowers every terrain-like non-water
   sample's `SurfaceHeight` to the planned `BedHeight` on river/lake cells,
   so the water sheet wins the elevation vote and the channel renders.
   Also required so the shore planner's own water detection sees the true
   water set (a hidden river cell otherwise counts as land).
2. **Stranded river mouths (data defect).**
   Traces correctly terminated at sink receivers, but interior sinks not in
   the water masks and non-sea border cells render as land — the channel
   visibly stops one cell short (e.g. (39,1) → (39,0) renders `sand`).
   Fix: `MarkRivers` marks the terminal sink receiver cell as river
   (budgeted), so the mouth always ends in visible water / exits the map
   edge. 11 stale `FlowParent → non-water` bracket links were also fixed
   (brackets now drain into the hop target).

### Dependencies
`FlowParent` feeds `MarkWaterfalls` and `IRecipeHydrologyMap` queries;
re-routing keeps `filled` descent monotonic, so waterfall logic is unchanged.
`ITerrainPlanApplier.Apply` gained an optional `RecipeHydrologyPlan`
parameter (single production caller in `MapGenerationServices.GenerateSafe`).
World hash changes (data change — expected).

### Acceptance
- `acc[parent] ≥ acc[child]` for every cell (0 violations on seed 42).
- Every river cell reaches sink/lake/border through marked cells only
  (test: walk FlowParent, all nodes water-kind ≠ None-or-land).
- Every river cell has ≥1 cardinal water neighbour (4-connectivity).
- Seed-42 puddle count collapses; rivers read as continuous channels in
  topdown + game camera.

---

## P3/P4 — Sand band too wide / too uniform (data+visual, confirmed)

### Reproduction
`tilemap.csv` + `shore.png`: every direct water neighbour is converted to a
full-cell `sand` winner (`bandCells=1`) → a uniform 1-tile sand ring around
every water cell → "непропорційно великі піщані ділянки" and a hard
sand↔grass line. Requirement: visible river sand ≈ 0–0.5 tile, irregular,
grass-dominant.

### Confirmed cause
`TerrainShorePlanner` converts **every** band cell unconditionally
(`distance <= band → convert`). The sand preset is `gridMode: Dual`
(`Assets/Moyva/Presets/Tiles/sand.json`), so a *lone* sand cell amid grass
already renders as a partial (~half-cell) blob — the machinery for
"partial coverage inside one tile" exists; the planner just never leaves
grass islands in the band.

### Required changes — `TerrainShorePlanner` + `TerrainShoreConfig`
- New config `BandCoverage` (0..1, default 1 = legacy full band) +
  `SeedSalt`. Conversion becomes noise-gated:
  `HashU32(salt, x, y, …) < coverage` — deterministic, irregular shoreline.
  Recipe sets ~0.55 for sand that averages ≤ half-tile visually.
- **Grading still applies to every band cell** (converted or not) so the
  geometric descent to the waterline is continuous even under grass —
  "одного розмиття текстур недостатньо" is respected.
- Blend ring and `MaxDropToWaterMeters` cliff skip unchanged → steep shores
  stay steep.

### Acceptance
- Sand cells form an irregular partial band; grass directly touches water in
  places; no uniform 1-cell ring; cliffs untouched; sand count reduced vs
  baseline on seed 42.

---

## P5 — Water strips/fragments on dry land (rendering, partially confirmed)

### Reproduction
`fix-audit-seed42/shore.png`: bright cyan line along every bank waterline;
cyan specks visible on bank tops (bottom-left of shot).

### Hypotheses (to confirm after P1 fix re-render)
- H1 — the cyan rim is the Nintendo material's depth-fade foam at the
  waterline: **by-design look**, possibly too stark but not a leak.
- H2 — `CollectShoreWaterSource` emits a **full-cell** wash sheet on every
  land cell adjacent (incl. diagonally) to water at the neighbouring water
  surface height. Where the land fragment's dual-grid shape leaves gaps, or
  where an ungraded cell sits below the sheet, cyan peeks through/above dry
  land.
- H3 — wash sheet emitted when land surface is *below* the water surface
  (ungraded winner kinds) → literal water film over land.

### Required changes (pending H-confirmation)
- If H2/H3 confirmed: replace the full-cell wash with **per-edge strips** —
  a generated quad covering only ~0.3 cell inward from each water-adjacent
  cardinal edge (plus small corner patches for diagonal-only adjacency), at
  the neighbour's water height; skip emission entirely when
  `waterHeight > landSurface + epsilon`.
- Verify bed columns and waterfall strips don't extend past banks
  (bed columns already occlude land/border edges).

### Acceptance
- No cyan on dry surfaces at topdown/game/low angles; waterline foam line
  present but bounded to the shore; no water past bank contours.

---

## P6 — Cliff/stair joints + floating rocks (to verify)

### Reproduction
Previous audits flagged slivers at sharp drops and floating decor rocks.
`oob.txt` currently shows **0 TerrainMesh OOB**; 11 prop OOB entries are
border-crossing tree canopies (P7-related, not terrain holes).

### Hypotheses
- Terraced 0.5 m steps are intentional relief; gaps appear only at
  *diagonal* cliff corners (dual-grid fragments meet at a vertex).
- Floating rocks: `EnvironmentObjectPlacementResolver.ResolveGroundedY`
  already grounds props by lowest footprint point — suspect remaining cases
  are props whose footprint validation was waived (water-authored objects)
  or whose support changed after shore grading.

### Plan
- Re-inspect after the P1 fix on all three seeds; catalogue any
  see-through gaps with coords; fix only confirmed geometry holes
  (likely inside `ChunkTerrainMeshBuilder` corner closure or per-edge
  bottoms).
- Rocks: verify support after mesh change; no gameplay-rule changes.

### Acceptance
- No see-through slivers on cliff corners; props sit on surface
  (screenshot inspection at low angle).

---

## P7 — Decor visible where its chunk is hidden (RENDERING defect, confirmed)

### Reproduction/cause
`MapVisualChunkDiscoveryRebuildService.RegisterRenderers` maps each renderer
to **every chunk its bounds overlap**; `MapVisualChunkRegistry.
ShouldRenderAnyChunk` shows it when **any** of those chunks is visible.
A tree canopy crossing a chunk border therefore stays enabled while the
supporting chunk (and its terrain) is culled or fog-hidden → "floating
objects over empty space".

Key fact: props are already parented to per-chunk roots —
`ChunkFirstObjectSpawner` → `MapVisualChunkRootService.GetOrCreateRoot` →
`MapChunk_X_Y/Objects`. Terrain meshes likewise live under
`MapChunk_X_Y` and are additionally registered single-chunk by
`ChunkFirstWorldBuildService.RegisterChunkRenderer`. The hierarchy already
knows the owner; only the registration ignores it.

### Required changes
- `IMapVisualChunkRootService`/`MapVisualChunkRootService`: add
  `TryGetOwnedChunk(Transform, out MapChunkCoord)` — walk ancestors, find
  the `MapChunk_X_Y` node, reverse-map to coord (service keeps a
  Transform→coord map beside `_roots`).
- `MapVisualChunkDiscoveryRebuildService.RegisterRenderers`: if the
  renderer resolves an owning chunk root → register to that single chunk;
  else keep the bounds-overlap path (loose scene renderers unchanged).
- Effect: props die with their support chunk on camera-cull **and**
  fog-hide; terrain keeps its correct registration.

### Acceptance
- Prop renderer's chunk list = exactly its owner chunk (unit test on the
  service + smoke check); hiding a chunk hides its props; camera return /
  rediscovery produces no duplicates (`Rebuild` → `registry.Clear` +
  re-register is idempotent).

---

## P8 — Verification

- Seeds 42 (reference, before/after), 777, 12345: full pipeline +
  `hydromap.csv`/`tilemap.csv`/`waterstack.txt` dumps.
- Per-seed asserts: acc monotonicity = 0 violations; river cells contiguous
  to receiver; water components = lakes+sea+rivers only; `nanVerts=0`,
  `empty=0`, terrain OOB = 0.
- Cameras: topdown, game default/orbit N-E-S-W, low-angle SW, zoom in/out;
  construction grid OFF, then grid overlay vs terrain check.
- Focused EditMode: hydrology suite + new continuity tests; chunk-registry
  test for prop ownership; full EditMode suite at the end.
- Docs: update this file + `docs/qa/WORK_STATE.md`; mark unverified items.

## Status log

- 2026-09-26 — Diagnosis confirmed (accumulation ordering violation ×80,
  threshold-only marking, diagonal links, bounds-OR prop registration).
  Implementation starting.
- 2026-09-26 — P1/P2 core implemented: `RefineParents` steepest descent,
  `Accumulate` distance-to-sink order, path-traced `MarkRivers` with
  diagonal brackets + budget, `riverMaxFraction` 0.08. P7 implemented:
  `TryGetOwnedChunk` single-chunk registration. P3/P4 implemented:
  `BandCoverage` noise gate. fix2-seed42 smoke PASS (184 river cells,
  acc violations 0, 0 orphan river cells) but two residual defects found:
  winner-loss on unflooded cells and stranded mouths at non-rendered sinks.
- 2026-09-26 — Residual defects fixed: `CarveWaterChannels` pass in
  `TerrainPlanApplicationService` (hydrology plan now passed through from
  `MapGenerationServices`); `MarkRivers` marks the terminal sink receiver.
  Compile clean, 513/513 EditMode tests pass. fix3-seed42 smoke running.
- 2026-09-26 — fix4-seed42: stair flights touching water dropped, route
  cells skip water, diagonal brackets may mark sink receivers. Renders show
  connected rivers; residual: floating wash plates (146 land<water),
  pale waterfall panes.
- 2026-09-26 — fix5-seed42: shore-lift inverted early-out fixed
  (`!convert && target==original` skip), wash guard added
  (`waterSurface > landSurface` -> no sheet). land-below-water 146->52.
  Residual 52 cells all bordered Swamp-layer water.
- 2026-09-26 — Swamp root cause: swamp winner TileId="swamp" (SolidTerrain)
  is invisible to `waterLikeTileIds`, so neighbours never saw its 0.49
  surface and never lifted. Fix: `waterTileIds` extension on
  `TerrainShoreConfig` merged into detection only (relief/carve semantics
  unchanged). fix6 identical hash (NaN-poison fix was incomplete); probe
  stack dump confirmed TileId="swamp". fix7-seed42: land-below-water = 0,
  water 1149 cells, 16 components, all 184 river cells connected,
  hash 5D8E6C1024CA46E8. Tests: 59/59 focused pass (two hydrology tests
  updated to receiver-marking semantics, one stale Toon-material path
  updated to NintendoStyle).
- 2026-09-26 — P6 waterfall-strip root cause found: `CollectWaterfallSource`
  used `IRecipeHydrologyMap.TryGetWaterSurface` = plan WaterSurface grid,
  where sink cells store drainage pseudo-surfaces (bed-0.03 ~ terrain top,
  e.g. 3.47) far above the rendered sheet (0.25). Strips on sink-adjacent
  edges therefore became 3m floating panes; ~600 qualifying edges produced
  the spike/pane field seen on renders. Fix: strips now measure the drop
  against the *rendered* surfaces — upperY = winner water sample
  SurfaceHeight, lowerY = ResolvedTileComposition per-direction rendered
  neighbour height (N/E/S/W/NE/SE/SW/NW) — gated by the plan so falls only
  face water, plus a 0.1 m lip overlap so strip tops hide under the rounded
  sheet rim. Pending fix8 render verification.
- 2026-09-26 — fix8-seed42 smoke PASS (world hash unchanged
  5D8E6C1024CA46E8 — composition identical, verts 843652->841848):
  floating waterfall panes/fins gone on renders. Seeds 777 and 12345
  verified: rivers 184/184, acc viol 0, broken 0, land-below-water 0,
  nanVerts 0, empty 0; seed777 has two legit 3-cell streams ending in
  closed depressions (true local minima); seed12345 fully connected.
  ChunkOwnershipTests 4/4. Generator 112/112 + full EditMode suite 830/830 PASS.
  All P1–P8 items verified; remaining optional polish: dedicated
  waterfall material (none exists in atlas), cosmetic border-rim cyan line.
