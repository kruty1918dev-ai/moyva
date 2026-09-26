# Waterfalls — automatic stylized falls from SW3 assets (2026-09-26)

## Proven cause

Rendered "waterfalls" were per-cell vertical quads emitted by
`TwcTileMeshSourceProvider.CollectWaterfallSource`: a stretched unit strip
using the **lake water sheet material** on every lower water neighbour
(min drop 0.5 m). That is the reported symptom — turquoise vertical
plates with no foam, no lip, no downward flow.

## Fix

| Component | Role |
|---|---|
| `RecipeWaterfallConfig` (`hydrology.waterfalls`) | JSON config: enabled, `minDropLevels` (2 × `TerrainHeightStep`), `curtainMaterial` + 3 VFX prefabs via `$asset` catalog refs, VFX budgets |
| `WaterfallFieldPlanner` (pure C#) | Detects rendered water-sheet → plan-water drop edges (same gate as the old strips), suppresses corner-duplicate diagonals, merges contiguous same-direction edges into `Front`s |
| `WaterfallChunkMeshService` | Shared map field (keyed on hydrology version) → one curtain mesh per chunk on `StylizedWater3_Waterfall.mat`; merged into the combined chunk mesh — zero extra draw objects |
| `WaterfallVfxSpawner` | SW3 `Waterfall Edge` (lip), `Impact Splashes` (base), `WaterfallMist` (≥3-level falls) under per-chunk `Waterfalls` roots; width-scaled edge emitter, per-system `maxParticles` cap, `maxVfxPerMap` budget, cleared on rebuild |
| Provider/builder wiring | Legacy strips suppressed only when `IsActive && HasField`; curtain meshes added inside `ChunkTerrainMeshBuilder.Build` like the seabed |

Key SW3 detail: the waterfall material uses `_WORLDSPACEUV_ON` with
`_Direction = (0,-1,0)` — foam scrolls downward in world space on any
geometry, so the custom 4-point curtain profile (back-lip tuck, crest,
straight fall, bowed base) animates correctly without prefab scaling
hacks or UV authoring.

## Detection semantics

- Upper cell: `TileGeometryMode.SurfaceOnly` (rendered water sheet).
- Lower target: `TryGetWaterSurface` (plan water; a wet tile above a dry
  cliff emits nothing) **and** rendered drop ≥ `minDropLevels ×
  heightStep` (2 m on this recipe).
- Diagonal dedupe: a diagonal edge is dropped when the same cell pours
  over both orthogonal flanks at a similar level, or a flank cell pours
  orthogonally into the same lower cell — one D8 event, one face.
- Front merge: same direction + contiguous along the tangent + top/bottom
  within 0.3 m. Anchor = middle edge cell → deterministic chunk owner
  and stable id `wfall_<x>_<y>_<dx>_<dy>`.

## Verified (seed 6130, worldHash `87D5247B44DA008A` unchanged)

`waterfall-active.txt`: `fieldBuilt=True minDropM=2.00 fronts=5 vfxSystems=13`

```
anchor  dir      width  top    bottom  drop
16,31   W        1      3.47   0.25    3.22
17,31   E        1      3.47   0.25    3.22
16,31   N        2      3.47   0.25    3.22
2,19    SE       2      2.47   0.25    2.22
4,10    NW       1      2.47   0.25    2.22
```

- 5 fronts on the raised-lake plateau + a diagonal pour; VFX = 5 edge +
  5 splash + 3 mist (mist gated at ≥3 levels).
- Visual: glowing cyan lip + falling curtain into the teal lower pool
  (`waterfall.png`, `waterfall_closeup.png`) — no stretched lake plate.
- Old strips: removed end-to-end when the field builds; verts
  515 861 → 515 517, meshes still 9, `nanVerts=0`, `empty=0`, smoke PASS,
  zero entries in `visual-smoke-errors.log`.
- Focused EditMode: `WaterfallFieldPlannerTests` 10/10 (flat water 0,
  deep bed 0, dry cliff 0, narrow 1, wide merge, split heights, cascade
  per ledge, corner dedupe, lone diagonal, non-sheet upper).
- `TerrainPlanTests.WaterPreset_UsesStylizedWater3Material` updated to
  the Moyva `WaterMaterial` (follow-up to the water-depth change).

## Limits

- Isolated curtain top-down probe renders black: the transparent SW3
  shader samples the camera depth texture which the off-screen probe
  lacks; in-scene shots carry the evidence.
- VFX counted as ParticleSystems (`vfxSystems=13`), not renderer
  objects — per-prefab internals are package-authored.
- Mobile device check not run in this environment; particle budgets are
  config-capped (`maxParticlesPerVfx`, `maxVfxPerMap`).
- Regeneration cleanup relies on per-chunk `Waterfalls` roots being
  cleared by `WaterfallVfxSpawner.Clear()` at spawn start — verified by
  `vfxSystems=13` staying constant across two consecutive smoke runs on
  the same world.

## Files

New: `ChunkFirst/Waterfall/WaterfallFieldPlanner.cs`,
`ChunkFirst/Waterfall/WaterfallChunkMeshService.cs`,
`ChunkFirst/Waterfall/WaterfallVfxSpawner.cs`,
`Tests/Runtime/WaterfallFieldPlannerTests.cs`.

Changed: `TerrainPlanConfig.cs` (+`RecipeWaterfallConfig`),
`RecipeHydrologyPlanner.cs` (plan field + merge), `RecipeHydrologyStore.cs`
(`IRecipeHydrologyMap.Waterfalls`), `ChunkTerrainMeshBuilder.cs`,
`TwcTileMeshSourceProvider.cs`, `ChunkFirstWorldBuildService.cs`,
`ChunkFirstFeatureBindings.cs`, `WorldVisualSmoke.cs` (dump + closeups),
recipe JSON + mirror + schema + runtime asset catalog,
`TerrainPlanTests.cs` (material path).

Evidence: `docs/qa/evidence/waterfalls-seed6130/`.
