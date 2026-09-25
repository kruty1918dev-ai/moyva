# Validation Report — Post-Change Stability

Date: 2026-09-25. HEAD `83ad5ea3c` + uncommitted working tree
(~196 modified files: user's terrain/hydrology/preset work + agent fixes in
`ChunkTerrainMeshBuilder.cs`, `WorldVisualSmoke.cs`, `JsonizationBuildService.cs`,
recipe JSONs, `docs/qa/*`).

Environment: Unity 6000.6.2f1, Windows 11, scene `Gamplay_Scene`,
launch `DirectGameplayTest` via `WorldVisualSmoke` harness.

## 1. Compilation — PASS

- EditMode run compiled the full project: `Tundra build success`, script
  compilation 1.57 s, **0 errors**. Log: `Logs/Editor.log`.
- Editor.log warnings are pre-existing/environment noise: licensing access
  token, `websocket-sharp` duplicate assembly (packages), missing meta files in
  immutable `Packages/`, `SerializeReference` missing refid 7438633201058971648
  (pre-existing data), URP converter warnings. None related to the change set.

## 2. Tests — PASS

- EditMode suite (`Kruty1918.Moyva` filter, all test assemblies):
  **816 / 816 passed**, 0 failed, 0 skipped, ~140 s. Run twice; final run
  executed after the last code change in this session.
  Preserved results: `docs/qa/evidence/validation/tests-editmode-20260925_021007.xml`.
  (`Temp/ai/*.xml` is wiped by each editor launch — copy artifacts out before
  the next Unity run.)

## 3. Generation cycle + determinism — PASS

Two independent editor launches, identical config, recipe `seed: 0` → default 42:

| Run | worldHash | verts | land/water | genTimeSec | gameObjects | memMB |
|---|---|---|---|---|---|---|
| `validation/seed-42-a` | `AAFCB590DF989988` | 616775 | 1368/936 | 4.1 | 779 | 1455 |
| `validation/seed-42-b` | `AAFCB590DF989988` | 616775 | 1368/936 | 4.0 | 779 | 1443 |

`worldHash` = FNV-1a over `TileMap` + `ObjectMap` + `HeightMap` (1 mm quantised)
— identical → deterministic generation confirmed. Object counts identical;
memory delta 12 MB is within launch variance (no accumulation evidence).

Distinct seed: `validation/seed-777` → hash `C048BAE3CD9BD011`, verts 687770,
land 1573 / water 731, genTime 4.0 s — different world, sane stats.

## 4. Console / logs — PASS

- `Library/ai/visual-smoke-errors.log` captured during both smoke sessions:
  **0 lines** (was ~4700 lines of PRU noise before the `BeginStaticPreview` fix).
- No gameplay exceptions, no missing-script, no shader-error entries in
  `Logs/Editor.log` for the smoke runs.

## 5. Geometry / placement consistency — PASS

- `oob.txt` (both runs): zero `TerrainMesh` vertices outside map rect;
  only prop canopy verts ≤0.3 cell past rim (VA-06, cosmetic).
- Construction overlay (initial-castle onboarding, intended): red/green cells
  hug tile bounds exactly; green = buildable grass, red = sand/water/unrevealed
  — matches canonical `EvaluatePlacement` + `no-build` tags.
- `game_shore` on seed-777 renders uniform gray — verified **not a defect**:
  the target cell (0,1) is unrevealed → fog-of-war curtain; other shots in the
  same frame are valid. (VA-05 flake fixed: all `game_*` shots distinct.)

## 6. Player build — PASS

- `JsonizationBuildService.DevelopmentPlayerSmokeFromCli` (added parameterless
  wrapper) → `BuildPlayer` for `StandaloneWindows64`, all enabled scenes
  (Boot, HomeMenu, Gamplay_Scene), Development options.
- Result: **Success** — `Build/PlayerSmoke/MoyvaJsonSmoke.exe` (400 MB folder;
  `Build/` is gitignored).
- Re-run **after** the VA-07 gate: incremental build rewrote
  `MoyvaJsonSmoke_Data/Managed/Kruty1918.Moyva.Generator.dll` at 04:43 (contains
  the fix); `Logs/Editor.log` → "Build Finished, Result: Success", exit code 0.
  (`Temp/ai/player-build-report.json` did not materialize this run — Editor.log
  is the evidence; exe stub/DLLs unchanged by incremental build keep the
  02:07 timestamp, which is expected.)

## Harness fixes made this round

- `BeginPreview` → `BeginStaticPreview` pairing fix: eliminated all
  "Missing EndPreview" / "Matrix stack empty" error-log noise.
- Error log truncated per run; manifest gains `worldHash`, `genTimeSec`,
  object/renderer counts, `memMB`.

## Round 2 — shoreline-authority fix + path coverage (2026-09-25)

### Production fix (VA-07)

- `TileWorldCreatorShoreBandService` was a second shoreline authority: it
  retyped every water-adjacent land cell to `sand-shore-band` post-export with
  no height cap/grading — visual-only divergence from `GameplayTileMap`, sand
  at 1.5–4.0 m on plateau riverbanks, direct snow↔sand adjacency.
- Fix: gated to worlds without a `LogicalTileMap` (graph-produced worlds keep
  legacy behavior; recipe worlds use `TerrainShorePlanner` as sole authority).
- Verified: `seed-42-noshore` — `sand-shore-band` count 0 (was 182), heightmap
  **byte-identical** (geometry untouched), sand distances {1:671, 2:9},
  zero sand↔snow/rock-cliff neighbors; all 105 sand cells >1.0 m sit exactly
  at `riverSurface+0.04` (planner riverbank lifts). `seed-12345-noshore` —
  all 506 sand at distance 1, 273 elevated cells all `waterSurface+0.04`.
  `side_s` pixel-identical to pre-fix → zero geometry regression.
- EditMode after fix: **816/816 PASS**
  (`docs/qa/evidence/validation/tests-editmode-20260925_041806.xml`).

### Path coverage (previously unverified)

- `StartupBarrierSmoke` gained `menu` mode: HomeMenu → `ConfigureMenuNewGame`
  → `IHomeMenuGameStarter.StartGameAsync` → waits world+workflow+transition.
- **menu: PASS** — `world=32x32, assignments=1, errors=0`
  (`docs/qa/evidence/validation/barrier-smoke-menu.summary`; 32x32 = size
  preset 0, menu new-game presets — not the 48x48 recipe world).
- **host: PASS** — `world=128x128, assignments=1, hostReady=True,
  inputBlockedDuringLoad=True, errors=0` (barrier-smoke-host.summary) —
  startup barrier actually observed blocking input during load.
- Harness note: request file must be written to `Temp/ai/` **after** the
  editor finishes launching — Unity wipes `Temp/` at startup.

## Remaining known issues (non-blocking)

- VA-03 shoreline width: now exactly 1 cell from water on both audited seeds;
  elevated cells are riverbank lifts at the waterline. Residual question is
  pure visual-taste acceptance — data is consistent.
- VA-06 prop canopy overhang ≤0.3 cell past rim — cosmetic.
- Mesh caches (`_borderClampedMeshCache`, `_warpedMeshCache`, …) hold stale
  references to destroyed meshes across regenerations; guarded by `!= null`,
  bounded managed-wrapper leak — cosmetic housekeeping, not a functional bug.
- Not verified: camera in continuous motion, FoW transition animation,
  unit/building visuals, underwater views, in-session world regeneration
  (no gameplay regen path exists — only menu-preview `RegeneratePreview`;
  restart determinism + mesh-registry clearing cover the risk).
