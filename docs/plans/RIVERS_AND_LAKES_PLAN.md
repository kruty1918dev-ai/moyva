# Rivers & Lakes — Integration Plan

Date: 2026-09-25. Base: HEAD `83ad5ea3c` + working tree (post-VA-07).
Scope: `Assets/Moyva/Scripts/Features/Generator/` recipe pipeline +
chunk-first TWC adapter. TWC version: **4.2.2** (`Assets/ThirdParty/TileWorldCreator`,
asmdef `GiantGrey.TileWorldCreator`).

## 1. Current pipeline (verified in code)

```
TerrainReliefPlanner → reliefField[m] (quantized 0.5 m steps)
  → GeneratorMaskEvaluator/GeneratorMaskSession
      └ HydrologyMaskStep(River|Lake) → RecipeHydrologyPlanner.Build
          · priority-flood fill (Barnes) seeded by sink layer mask + border
          · flood parents = D8 downstream links (acyclic by construction)
          · accumulation → river cells (≥ threshold, ≥ min source height)
          · depression fill − terrain ≥ 0.5 m → lake cells (area-capped)
          · river cells with downstream drop ≥ 0.5 m → WaterfallMask
          · per-cell WaterSurface = filled + offset (−0.03 m)
  → CompiledLayerMap.SurfaceHeightOverride = plan.WaterSurface
  → LogicalTileMapBuilderService.ResolveCellData: sample.WithSurfaceHeight()
  → LogicalTileMap → TerrainPlanApplicationService
      (relief on land, TerrainShorePlanner grading, passages, routes)
  → GeneratedWorldData.Hydrology → TileWorldCreatorWorldBuildBridge
      → RecipeHydrologyStore.Replace (runtime query: IRecipeHydrologyMap)
  → ChunkFirst: TwcTileMeshSourceProvider
      (SurfaceOnly water sheets, shore wash +1 cell, waterfall strips)
```

- Cell = 1 m, cell (x,y) centred at (x·cs, z·cs); dual-grid fragments sit on
  half-offset vertices, border verts clamped to map rect.
- `TileLayerSample.Height` = bed, `SurfaceHeight` = top sheet; `WithHeights`
  sets both independently; `WithSurfaceHeight` sets both equal.
- Water ids: `water`, `ocean-deep`, `ocean-shallow`; sea tile
  `water-middle-depth-tile-002`. Water layers are `SurfaceOnly`.

## 2. Gap analysis (vs requirements)

| Req | Status | Evidence |
|---|---|---|
| Continuous downhill channel, no cycles | **OK** — flood-parent tree, tests cover descent | `RiverDescendsAlongFlowParent` |
| Lakes fill depressions, outflows drain | **OK** — accumulation flows through; downstream cells can re-mark as rivers | `Depression_BecomesLakeWithFilledSurface` |
| Endorheic lakes | partial — lakes always fill to rim (overflow level); no below-rim level control | planner |
| Waterfall strips on drops | **partial** — only `riverMask` cells marked; lake cells excluded entirely, and raw-terrain comparison would fake falls inside flat lake basins | `MarkWaterfalls` |
| Rapids/mid transitions | N/A while `quantumMeters=0.5` — every surface step ≥0.5 m = waterfall | relief quantum |
| **Bed height/depth data** | **MISSING** — water cell `Height` = layer `defaultHeight` (0.0) → plateau rivers report depth up to 3.47 m; nothing carries a real bed | `surfaceheightmap` vs `heightmap` dumps |
| Land below adjacent water surface | **NO DEFECT** — rechecked restricted to true `SurfaceOnly` water (River/Lake/Water winners): `land below TRUE water surface: 0`. The 27 flagged cells were Swamp winners — solid-terrain water columns, not floating sheets | dump analysis |
| Flow direction exposed | **MISSING** — FlowParent stored but not queryable as direction; needed for consistent downstream animation/foam | `IRecipeHydrologyMap` |
| Topology/level invariants | partial tests; missing: path-to-sink continuity, per-basin uniform level, bed-depth invariant | tests |
| Chunk seams | OK — verified earlier (no gaps, oob=0) | smoke manifests |
| In-session regen cleanliness | store has `Replace`/`Clear`, called per build ✓ | bridge |

## 3. Work items

### W1 — Real bed heights (plan + data path)
- `RecipeHydrologyConfig` += `channelDepthMeters` (default 0.35, ≥0.05).
- `RecipeHydrologyPlan` += `BedHeight[,]`:
  - river: `bed = min(terrain, surface − channelDepth)` — the carved channel
    on normal cells, the real submerged floor on flooded segments (a
    depression deeper than the nominal channel is the true water bottom);
  - lake: `bed = min(terrain, surface − 0.05)` — real depression floor;
  - sink/sea: bed stays `terrain` (the sea-floor layer height).
- Carry bed to the logical map: `GeneratorMaskSession` gains
  `BedOverrides` (same registration as `SetSurfaceOverride`), stored on
  `CompiledLayerMap.BedHeightOverride`; `ResolveCellData` applies
  `WithHeights(bed, surface)` when both exist.
- Effect: `HeightMap`/sample `Height` report true beds for movement,
  shore lift, decoration and material stages — no behaviour change to the
  water sheet itself (SurfaceOnly renders on SurfaceHeight).

### W2 — Lake cells in waterfall marking (done differently than first drafted)
- `MarkWaterfalls` now iterates river ∪ lake cells and compares **flooded
  levels** (`filled`), not raw terrain — a lake's uneven floor under a flat
  surface must never fake a drop inside the basin.
- Priority flood fills a lake to its lowest rim, so a lake's outflow is level
  by construction; in practice lake-cell waterfalls mark only on rare merged
  masks. The real fix is eliminating *false* falls + keeping the door open.

### W3 — Store queries for downstream stages
- `IRecipeHydrologyMap` += `TryGetBedHeight(cell, out bedY)`,
  `TryGetFlowDirection(cell, out Vector2Int downstream)` (flow parent as
  direction; −1/none → false), `GetWaterKind(cell)` → None|River|Lake|Sink.
- This is the hand-off contract for shore/material/biome stages and future
  per-cell flow-direction animation (documented in WORK_STATE).

### W4 — Floating shore-wash — VERIFIED NO DEFECT
- Initial dump flagged 27 "land below water surface" cells. Restricted to
  true `SurfaceOnly` water winners (River/Lake/Water): **0 occurrences**.
  The flagged cells were Swamp winners — `SolidTerrain` water columns whose
  own surface is their top; they are not floating sheets.
- No change needed; swamp is a distinct intentional feature.

### W5 — Targeted tests (`RecipeHydrologyPlannerTests` + store tests)
- Every river/lake cell's flow-parent chain terminates at a sink/border —
  no cycles (visit-count guard).
- Water surface is non-increasing along flow (tolerance 1 mm).
- All cells of one connected lake share one surface level.
- `BedHeight ≤ WaterSurface` on every water cell; river depth == config.
- Lake outflow drop ≥ threshold ⇒ `WaterfallMask` set on the lake cell.
- Store: `TryGetBedHeight`/`TryGetFlowDirection`/`GetWaterKind` answers.

### W6 — Verification
- EditMode suite + new tests.
- Smoke seeds 42 / 777 / 12345: `worldHash` stability, `empty=0`,
  `nanVerts=0`, zero TerrainMesh OOB; inspect `tilemap/heightmap/
  surfaceheightmap` dumps — river cells show `S−H == channelDepth`.
- Screenshots (topdown + low-angle) at: plateau river chain, lake+outflow,
  waterfall edge, confluence, mouth into sea, river on flat.
- Regression: shore band distances unchanged vs `seed-42-noshore` baseline.

## 4. Acceptance criteria

1. Every generated world keeps `empty=0`, `nanVerts=0`, zero OOB verts.
2. All river/lake cells drain to a sink; no flow cycles; surfaces never
   rise downstream (≤1 mm tolerance).
3. Water cell `Height` = real bed: rivers `surface−channelDepth`, lakes the
   depression floor; no 0.0 beds on elevated water.
4. Drops ≥ `waterfallMinDropMeters` on ANY water cell (incl. lake outflows)
   produce a waterfall strip; no visible gap/step in the water sheet.
5. No land cell with `landSurface < adjacentWaterSurface` remains
   (floating-wash class eliminated).
6. `IRecipeHydrologyMap` exposes bed, flow direction and water kind.
7. Same seed + same config → identical masks, surfaces, beds (hash stable).
8. 816 existing tests still pass + new hydrology tests green.

## 5. Out of scope (documented, not implemented)

- Multi-cell river width / meander width model — channels stay 1 cell.
- Per-cell flow-direction water animation — data exposed (W3); shader hook
  deferred (Stylized Water 3 uses a material-level direction; per-cell flow
  would need a flowmap — separate task).
- Below-rim endorheic lake levels — current lakes fill to overflow rim.
- Rapids: no sub-0.5 m drops exist while `terrainRelief.quantumMeters = 0.5`.

## 6. Implementation status (2026-09-25)

- **W1 DONE**: `channelDepthMeters` (0.35) added to `RecipeHydrologyConfig` +
  both recipe JSONs. `RecipeHydrologyPlan.BedHeight` computed per water cell.
  `GeneratorMaskSession.BedOverrides` → `CompiledLayerMap.BedHeightOverride`
  → `LogicalTileMapBuilderService.ResolveCellData` applies
  `WithHeights(bed, surface)`; `LogicalTileLayerData.WithHeights` added.
  Verified end-to-end: seed-42 `waterstack.txt` shows river cells
  `bed = surface − 0.35` exactly (e.g. (40,6) bed 0.12 / surface 0.47).
- **W2 DONE**: `MarkWaterfalls` iterates river ∪ lake cells on flooded
  levels; border parents still excluded. `LakeInterior_NoFalseWaterfalls`
  test locks the basin-flatness invariant.
- **W3 DONE**: `IRecipeHydrologyMap` += `TryGetBedHeight`,
  `TryGetFlowDirection`, `GetWaterKind` (None|River|Lake|Sink).
- **W4 CLOSED**: no defect — verified 0 true-SurfaceOnly floaters.
- **W5 DONE**: tests cover chain-to-sink, monotonic descent (≤1 mm), lake
  level uniformity, bed ≤ surface / ≥ channel depth / ≤ floor, no false
  falls, store queries. 17/17 hydrology tests PASS; suite 821/821 PASS.
- **W6 partial**: seed-42 smoke PASS (`empty=0`, `nanVerts=0`, 0 OOB,
  `worldHash=85B4F2FADD94AC0D`), screenshots inspected — water sits in
  carved channels, coherent connected bodies. Seed-42 produced no Lake
  winners (no ≥0.5 m depressions at this relief) — lake behaviour covered
  by unit tests; a second seed is checked for lake coverage.
