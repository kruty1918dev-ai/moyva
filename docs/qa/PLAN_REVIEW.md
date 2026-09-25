# Plan Review — Terrain/Hydrology Rework vs Requirements

Status legend: **done** (implemented + evidence), **implemented** (code/config in
place, evidence partial), **partial** (works but open issues), **blocked**,
**planned**.

| # | Requirement | Where implemented | How verified | Result | Evidence | Remaining |
|---|---|---|---|---|---|---|
| 1 | No world-filling water plane; water only in regions | `testgeneratorrecipe.json` `Water` → `SeaMask` ref | smoke stats + shots | **done** — water 18–41% of map by seed, all in sea/lake/river areas | `evidence/*/manifest.txt`, `game_iso` | — |
| 2 | Rivers/lakes accumulate and drain | `RecipeHydrologyPlanner.IsSink` — sinks = border + mask | `Generate_Rivers_DrainToSinks` etc. + shots | **done** | EditMode 102/102; `low_sw` rivers visible | — |
| 3 | Shore sand ≈0.5–1 tile band | recipe `Sand` mask + `TerrainShorePlanner` (band=1, graded); legacy `TileWorldCreatorShoreBandService` gated off recipe worlds | per-cell tilemap/height dumps + shots | **done** — post-fix dist {1:671, 2:9} on 42; all 506 sand at dist 1 on 12345; every cell >1.0 m sits at `waterSurface+0.04` | `seed-42-noshore`, `seed-12345-noshore` CSVs, `sandstack.txt`, `detail_shore` | user visual-taste sign-off |
| 4 | Smooth low-poly slopes, no broken slabs inside | dual-grid vertex phase + corner-height warp | shots all seeds | **done** | `low_sw`, `side_*` | — |
| 5 | No holes/gaps/stretched tris inside map | `TryResolvePhysicalCell` border clamp; ownership | stats `empty=0`, `nanVerts=0` | **done** | manifests | — |
| 6 | Biome coverage without gaps; intermediate elevations | Grass unbounded below; RockCliff open above; Snow ≥3.75 | histograms, `peak` | **done** | `manifest.txt` tile counts | — |
| 7 | No sand spawning (units/props/decor/start) | `TerrainPlacementPolicy` no-spawn on sand; `HasWaterSheet` deco check | policy tests + code path audit | **done** (code); spawn visuals not exercised live | tests | visual spot-check optional |
| 8 | Movement over sand allowed | no movement tag on sand | policy | **done** | code | — |
| 9 | Saved entities not relocated/blocked on load | no validation hook on load path | code audit | **done** | prior session | — |
| 10 | Oversized props shift or skip, never clipped | `ChunkFirstObjectSpawner` footprint validation | tests | **done** | tests | — |
| 11 | Construction grid aligned w/ terrain + canonical validation | `ConstructionBuildGridChunkSurfaceService` → `EvaluatePlacement` | game view grid aligns with tiles | **done** | `game_default` | — |
| 12 | One authoritative placement path | all callers → `TerrainPlacementPolicy`/`EvaluatePlacement` | code audit | **done** | — | — |
| 13 | Far View shader preserved | untouched; fog visible at distance | `game_topdown` pale haze | **done** | game shots | — |
| 14 | Map edge terminates cleanly | `ChunkTerrainMeshBuilder.ResolveBorderClampedMesh` — verts of border-straddling fragments XZ-clamped to map rect | `oob.txt` zero TerrainMesh OOB; edge shots flush | **done** | `seed-42-v3`, `seed-12345-v2` edge/corner/low shots | prop canopy ≤0.3 cell overhang = VA-06 cosmetic |
| 15 | Better sand texture / natural shores | sand preset (user-tuned) | `game_shore` | **partial** — foam/ring OK; sand reads flat orange, tuning is user's call | game shots | user decision |
| 16 | Grounded decorations on slopes | grounding + footprint checks | `transition`, `shore` | **done** | shots | — |
| 17 | River/lake data contract (masks, kind, flow dir, bed, surface) | `RecipeHydrologyPlan` + `RecipeHydrologyStore` (`IRecipeHydrologyMap`) | store/planner tests + `waterstack.txt` | **done** — `bed=surface−0.35` on all rivers; lake level uniform | `audit-rl-seed42/777/12345` dumps | — |
| 18 | Channels reconciled with terrain; real bed depth | `BedHeight` → `BedHeightOverride` → `WithHeights` on water samples | per-cell stack dumps | **done** | `waterstack.txt` all rivers | — |
| 19 | Water-level drops get waterfall coverage | `MarkWaterfalls` (river∪lake, flooded levels) + edge-driven `CollectWaterfallSource` — strip on every neighbour edge where `upperY − lowerY ≥ WaterfallMinDropMeters`, upper cell owns edge | CSV gap-edge scan + post-fix crops + hydrology suite | **done** — VA-08 fixed: world hashes unchanged (render-only), seeds 42/12345 PASS, `nanVerts=0`, lake↔sea 2.22 m rim closed, channel steps show solid water walls | `audit-rl-seed42-va08/`, `audit-rl-seed12345-va08/` | — |
| 20 | Flow direction exposed for animation/foam | `TryGetFlowDirection` on store | store tests | **done** (data); shader flowmap consumer deferred | tests | downstream consumer = separate task |
| 21 | NintendoStyle water material; sand visible under water fading with depth | `materialOverride`→NintendoStyle on `TileWater.asset` + `BaseBlockPresetWater.asset`; `CollectWaterBedSource` emits atlas-`sand` fill tile as SolidTerrain bed column at hydrology `BedHeight` with per-edge bottoms/closure | smoke seeds 42/12345 + visual shots + OOB scan | **done** — Nintendo caustic surface renders; sandy bed visible through shallows fading cyan→navy; waterfall strips now actually render (`SurfaceOnly`→`SolidTerrain` — upward filter had silently dropped all vertical tris); border fringe fixed (no skirt to void, floor = `bedY−0.5`/lowest neighbour bed); hashes unchanged, `nanVerts=0`, OOB = baseline | `water-nintendo-seed42-v2`, `water-nintendo-seed12345-v2` | translucent waterfall panes = cosmetic caveat |

## Harness gaps fixed this round

- Seed control moved to canonical `recipe.seed` (launch-context seed dies at
  play-mode domain reload — `DirectGameplayLaunchModeInitializer` rebuilds it).
- PRU per shot (was: shared instance → first-shot missing water, late-shot wash
  — confirmed harness artifacts, not product defects; real camera renders water).
- `game_*` shots deferred 2.5 s after mesh stability (reveal overlay flake).

## Corrections found this round (re-audit)

- **False assumption:** "wide sand = recipe height cap" (old VA-03). Stack dumps
  disproved it: 182 `sand-shore-band` cells came from the legacy TWC expander
  (`TileWorldCreatorShoreBandService`), gated by a flag nothing sets; Sand-layer
  cells above 1.0 m are all `TerrainShorePlanner` lifts to elevated river
  waterlines (+0.01 signature), not mask leaks. Fixed as VA-07.
- **Duplicate authority:** two shoreline producers ran on recipe worlds —
  expander (unconditional ring) vs `TerrainShorePlanner` (graded, cliff-aware).
  Now one: the planner.
- **Data↔visual inconsistency removed:** expander mutated `BiomeMap` only;
  `GameplayTileMap` kept real ids — cells looked like sand but played as
  grass/cliff. Resolved by the same gate.

## Open items ordered

1. **VA-06** prop canopy overhang past rim ≤0.3 cell — cosmetic, optional.
2. In-session world regeneration — no gameplay regen path exists (only
   menu-preview `RegeneratePreview`); restart determinism + mesh-registry
   clearing verified instead. Residual risk: stale managed wrapper refs in
   mesh caches, null-guarded, bounded — housekeeping, not a defect.
3. Explicitly unverified: continuous camera motion (orbit stills used),
   FoW transition animation, unit/building visuals, underwater views.

## Resolved this round

- **VA-01** border slivers — fixed via `ResolveBorderClampedMesh`; zero terrain
  OOB verts on seeds 42/12345; flush rim in `edge_s`/`corner_sw`/`low_sw`.
- **VA-02** startup build-grid — intended: `GameplayHtmlPresenter.EnsureInitialCastlePlacement`
  auto-opens Construction when `RequiresInitialCastle`; grid colors verified
  against canonical placement (green=grass, red=sand/water).
- **VA-04/VA-05** harness artifacts — fixed (fresh PRU per shot; deferred game
  captures); v3 shots clean.
- **VA-07** duplicate shoreline authority — `TileWorldCreatorShoreBandService`
  gated to graph-produced worlds (no `LogicalTileMap`); recipe worlds now have
  a single shoreline authority (`TerrainShorePlanner`). Verified on seeds
  42/12345: zero `sand-shore-band` cells, sand dist-1 only, zero snow↔sand,
  byte-identical heightmaps, 816/816 tests.
- **menu→Gameplay** — `StartupBarrierSmoke` menu mode: PASS, world 32x32,
  assignments=1, errors=0 (`barrier-smoke-menu.summary`).
- **host/multiplayer barrier** — `StartupBarrierSmoke` host mode: PASS, world
  128x128, hostReady=true, input actually blocked during load, errors=0
  (`barrier-smoke-host.summary`).
