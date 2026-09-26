# Denser object spawning — 2026-09-26

Task `08_Denser_Object_Spawning.md`: noticeably more trees/bushes/grass/
rocks on the same map, decor allowed on dry coast/highlands/mountains,
per-biome density, clusters with gaps, no land props in water, no
off-map spawns, no floating props, deterministic, chunk-owned,
mobile-conscious.

## Diagnosed causes (evidence, not screenshot inference)

New instrumentation: `DecorationPlacementStats` in
`EnvironmentDecorationGenerator.Generate` (opt-in counters) +
`WorldVisualSmoke.DumpDecorations` (scene census `decorations.csv`,
six-seed replay `decoration-stats.txt`). Same map, same code, same
camera for before/after — only config/policies changed.

| Symptom | Confirmed cause | Fix | Evidence |
|---|---|---|---|
| Large empty land areas | `biomeRules.coast=0` AND sand tile `no-spawn` → all 305 sand cells rejected in both passes | `coast=0.35`; new `no-decor` tag consulted only by `TerrainPlacementOperation.Decoration` | rejects table; census shows pebbles/grass on shore cells after |
| Dead zones across the map | `clusterStrength=0.7` binarized the smoothed density map → ~3300 `layer-zero-weight` rejects/generation | `clusterStrength=0.55` (+`globalDensity` raises effective noise floor) | zero-weight rejects vanish entirely in after-table |
| Thin overall counts | `globalDensity=1`, `maxObjectsPerTile=3`, type weights summing 0.63 → ~600 `type-select-miss` | global 1.35, maxPT 4, type weights sum ~0.93 | attempts 3364→5415, placements 2628→4685 |
| "Many stumps" | stump layer `nearTreeBoost=0.45` near every resource-tree anchor → 181 stumps vs 70 standing trees | boost→0.12 + `validateFootprint`+`shorelineExclusion` on stump layer | 80 stumps vs 154 trees after |
| Props hanging off map edge | `EnvironmentObjectPlacementResolver.CoveredCells` used `floor(v)` for cell mapping while cells are centred on integers (`mapRect −0.52..47.52`) → half-cell border overhang never hit the bounds check; stump layer lacked `validateFootprint` entirely | `floor(v+0.5)` in cell mapping + same centred lattice for the anchor-cell rewrite; stump layer opts into footprint validation | `oob.txt` 40→21 rows; `trees-b-cut` 169-vert spill eliminated; remainder ≤8-vert hairlines |
| Pebble-less slopes | `maxSlopeMeters=0.9` rejects every stepped cliff edge (215/seed) | 1.4 m — pebbles tolerate hill steps, still off walls | `layer-slope:pebble` 215→104 |
| Trees lying on slopes (risk) | tilt applied to every placement | tilt only for ground cover; structural types upright | `AlignsToSurface` type gate, spawner tests pass |

Not-causes verified: veg materials are correct alpha-clip q2490
(previous report); aquatic props on water are `waterAffinity`
by design; `no land prop on water` — water gate
(`IsWaterTile || HasWaterSheet`) ran before all land passes and the
census shows zero land-type placements on the 914 water cells
(waterCells counted separately, water-flora-density rejects confirm
only the flora roll happens there).

## Changes

- `environment-decoration-config.json` (+ generated mirror synced):
  `globalDensity 1.0→1.35`, `maxObjectsPerTile 3→4`,
  `clusterStrength .7→.55`; biome `coast 0→.35`, `grassland 1.2→1.3`,
  `rocky 1.5→1.3`; type densities tree .15→.24, bush .10→.18,
  grass .20→.32, flower 0→.05, rock .08→.14, waterplant .12→.16;
  layer weights raised ~1.3–2×; `stump.nearTreeBoost .45→.12`,
  `litter.nearTreeBoost .7→.5`, `pebble.maxSlope .9→1.4`,
  `log` lost `shorelineExclusion`, `stump` gained
  `validateFootprint`+`shorelineExclusion`.
- `TerrainPlacementPolicy.cs`: `no-decor` tag for Decoration op;
  `no-spawn` still blocks units/props/starting positions; shore-name
  fallback stays conservative for gameplay ops only.
- `EnvironmentObjectPlacementResolver.cs`: centred-lattice cell mapping.
- `EnvironmentDecorationGenerator.cs`: stats plumbing, 0.75×
  footprint retry, centred-lattice anchor rewrite.
- `EnvironmentDecorationSpawner.cs`: per-type tilt gate;
  Performance-profile light-decor thinning (~50%, hash-parity,
  deterministic) via optional `IGraphicsSettingsService`.
- `Kruty1918.Moyva.Generator.asmdef` (+ Tests asmdef): reference
  `Kruty1918.Moyva.Shared` for the graphics settings interface.
- `TerrainShoreAndPlacementTests.cs`: updated to Decoration-allows-
  dry-shore semantics + `no-decor` coverage.
- `WorldVisualSmoke.cs`: `DumpDecorations` (census + six-seed stats
  replay through the canonical generator via the bound container).

## Results (seed 6130, identical map/camera/light/quality)

| metric | before | after |
|---|---|---|
| placements | 2628 | 4685 (+78%) |
| tree | 70 | 154 |
| stump | 181 | 80 |
| bush | 169 | 422 |
| grass | 785 | 1462 |
| flower | 54 | 201 |
| rock | 149 | 288 |
| pebble | 166 | 319 |
| waterplant | 107 | 188 |
| reed | 126 | 225 |
| log | 25 | 70 |
| sapling | 37 | 63 |
| placement genMs | 72 | 79 |
| `layer-zero-weight` rejects | ~3300 | 0 |
| `type-select-miss` | 585 | 353 |
| `layer-slope:pebble` | 215 | 104 |
| OOB overhang rows | 40 | 21 (hairline only) |
| scene census objects | 2531 | 4665 |
| worldHash | 87D5247B44DA008A | same → terrain untouched, deterministic |

Six seeds replayed through `Generate` on the same map
(0/6130/777/2024/31337/42) — per-seed tables in
`evidence/density-seed6130/after/decoration-stats.txt`; totals stable
4.5–4.9k, no seed produced <4.5k.

Seeds for visual QA: 6130 (coast+plain+forest+mountains shown),
plus replay seeds 0/777/2024/31337/42 counted in the stats table.

## Verified

- Compile clean (Generator, Grid, Tests assemblies).
- EditMode filtered run (`Kruty1918.Moyva.Generator`): 158/159 —
  only `MultiplayerConstructionPlacementTests`
  `HostRequest_WhenPlacementFails_ReceivesRejected` fails on a
  `LogAssert` warning emitted by the expected rejection path
  (unrelated, uses its own fake construction service; policy
  untouched by it). `EnvironmentDecoration*Tests`,
  `TerrainShoreAndPlacementTests` all pass, including new
  `no-decor` cases and centred-lattice footprint cases.
- Visual smoke PASS ×3 runs; `worldHash` identical across all runs
  → deterministic placements + no accumulation (census stable).
- Chunk seams: per-chunk `EnvironmentDecorations` roots unchanged;
  census names show `assetId_tileX_tileY` per anchor; OOB scan shows
  no cross-chunk duplicates.
- Water safety: zero land-type placements on water cells; aquatic
  props only via `waterAffinity` rules.
- Grounding: `ResolveGroundedY` unchanged (lowest-footprint-point on
  lowest surface); OOB `minY` values sit on terrain, no floaters.

## Limits / not done

- Renderer count ~4.7k individual instances sharing ~10 decor
  materials (chunk-culled). GPU-instancing/dynamic-batching audit
  not performed; no on-device frame timing — batch editor only.
- `settlementExclusionRadius` / `suppressWaterDecorations` remain
  declared-but-unused config fields (pre-existing dead config;
  documented rather than silently wired).
- Render Graph "no matching RenderPass" error from supplied
  screenshots did not appear in these smoke logs and shows no
  density correlation — kept out of scope per task (belongs to
  renderer work).
- `sand.json` keeps `no-spawn`/`no-build` — construction and unit
  placement on beach unchanged.

Evidence: `docs/qa/evidence/density-seed6130/{before,after}/`.
