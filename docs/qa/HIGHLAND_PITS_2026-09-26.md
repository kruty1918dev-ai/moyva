# Highland pits — fix report (2026-09-26)

Task: "Гори й підйоми без випадкових провалів до базового рівня" —
mountains/highlands must not contain random deep shafts to base level,
while intentional valleys, gorges, rivers, lakes, coastal cliffs, sea
exits and relief variety are preserved.

## Root cause

NOT a relief-field defect and NOT `WorldGeographyEngine` (dormant,
test-only; production binds `IMapGenerationPipeline →
MapGenerationPipeline`, the TWC recipe path).

- `SeaMask = invert(_baseLayer)`; `_baseLayer` is perlin noise + cellular
  automata, fully independent of `TerrainReliefPlanner`'s height field.
- Small isolated `false` components (holes) in `_baseLayer` therefore
  invert into `SeaMask` blobs at arbitrary relief heights.
- The `Water` layer paints every `SeaMask` cell with
  `defaultHeight = -0.25` regardless of the relief there, and
  `CarveWaterChannels` digs a corresponding bed.

Seed-6130 proof: each pit cell had relief 2.5–4.5 m and ring-median 3–4,
yet published surface ≈ 0.25 m as `water-middle-depth` tiles — ~3 m
water shafts inside level-3 highland, rendering as the dark narrow
"breaks". Hydrology saw them only as brim-full sinks
(`WaterSurface ≈ BedHeight ≈ relief`); the fixed `defaultHeight`
produced the shaft.

## Fix

`TerrainHoleFillStep` (stable polymorphic id `terrain-hole-fill-step`)
in `GeneratorMaskStep.cs`, appended to `_baseLayer.steps` (after perlin
+ TWC CA) in `Presets/Generator/recipes/testgeneratorrecipe.json`, its
`Resources/MoyvaConfigGenerated/` mirror, and the schema `$type` enum.

Fills an interior `false` component into land iff:

- it never touches the map border (coast/sea exits preserved);
- `cells ≤ MaxComponentCells` (32) — large inland seas/lakes preserved;
- ring (8-neighbour land) relief median `≥ MinRingMedianMeters` (1.5 m) —
  lowland ponds preserved;
- ring median − own relief median `≤ MaxReliefDropMeters` (1.0 m) —
  genuine basins/gorges preserved.

Placed on `_baseLayer` (not `SeaMask`) so the filled cells become land:
`SeaMask`, hydrology sink mask, `Water`, `Sand` shore dilation and all
biome layers inherit the correction consistently — no orphan cells.
No-op without a relief field; input mask never mutated; deterministic.

## Verification

- Unit: `TerrainHoleFillStepTests` — fill/no-fill cases (highland vs
  border vs large vs lowland vs deep basin), determinism, input
  immutability, null field → pass.
- Real-recipe regression scan
  (`ProductionMask_NoClassifiedPits_AcrossSeedsAndProfiles`): production
  `GeneratorMaskEvaluator` on the loaded recipe + real relief field —
  40 instances (48×48 × 12 seeds, 96×96 × 6, mountain-profile 48 × 6)
  → **0 classified pits**; legitimate interior holes still present.
- Same-seed rendered A/B (seed 6130, `docs/qa/evidence/pitfix-seed6130/`
  vs `fbx-square-seed6130/`): interior pit components 10 → **0**, pit
  cells 53 → **0**; former cells now `hill`@level 3. Sea (1419), river
  network (107 cells, R=184 hydro markers), 90-cell inland lake all
  preserved; `empty=0`, `nanVerts=0`, `worldHash=87D5247B44DA008A`,
  9 meshes / 703 344 verts. Renders show a contiguous terraced massif
  instead of pillars separated by slots; south border rim unchanged.
- Recipe seed restored to 777 after the A/B run.

## Side fix (test robustness, not production)

`FullGameIntegrationTests.RealUnitsCapture...` generates a 24x24 world
from the same recipe. On the new map the scripted unit stopped at the
first BFS-adjacent cell — which height-aware fog legitimately hides from
the castle across a terrain edge — so no building attack enumerated.
`MoveToSettlement` now rings the target's adjacent cells until one is
visible (up to 8), then proceeds. Verified: single-test rerun passes,
full suite 883/883.

## Known unrelated gaps (pre-existing)

- `TerrainLevelMap` vs `round(SurfaceHeightMap)` divergence (~1k cells)
  exists before and after — independent published signals, not a pit.
- Vegetation boundary OOB vertices; barrack placement not exercised by
  the gameplay smoke harness (legal-cell setup) — both pre-date the fix.
