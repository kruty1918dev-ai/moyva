# Visual Audit Plan — Terrain/Hydrology Rework

Run: 2026-09-25, HEAD `83ad5ea3c` + uncommitted terrain/hydrology changes.
Unity 6000.6.2f1, scene `Gamplay_Scene`, launch `DirectGameplayTest`.
Seeds: 42 (recipe default), 12345, 777 — via recipe `"seed"` field.
Evidence: `docs/qa/evidence/seed-{42,12345,777}/` (PRU mesh renders + real game-camera RT renders).

## Verified OK

| Requirement | Evidence |
|---|---|
| Water only in sea/lakes/rivers; no world-filling plane | all `topdown`/`low_sw`/`game_iso`, stats `water=` 425–936 of 2304 |
| Rivers present, connected, drain to borders | `low_sw`, `edge_s` (all seeds); hydrology tests green |
| Shore band ~1 cell + foam wash | `detail_shore`, `shore`, `game_shore` |
| Terraced slopes connected; no interior holes | `empty=0`, `nanVerts=0` (all seeds); `side_*`, `low_sw` |
| No floating props; trees/rocks grounded | `shore`, `transition`, `low_sw`, `game_default` |
| Construction grid aligns with visible tiles | `game_default` seed-42/12345 (red/green outlines hug tile bounds) |
| Snow only at elevation; no lowland snow | `peak`, `topdown` (snow 8–16 cells) |
| Biome coverage without gaps | `empty=0`; biome histograms in `manifest.txt` |
| EditMode suite | 102/102 PASS (`Temp/ai/tests-editmode-20260925_001527.xml`) |

## Findings

### VA-01 — Border geometry slivers outside map edge (FIXED ✓)
- **Where:** south border (y≈0) and SW corner; likely all borders.
- **Seeds:** 42, 12345, 777 — reproducible on every run.
- **Expected:** terrain ends cleanly at the map boundary.
- **Actual (before):** thin dark plates protrude ~0.5 cell below/past the terrain rim; read as detached floating slabs from outside low angles.
- **Evidence (before):** `seed-42/edge_s.png`, `seed-42/corner_sw.png`, `seed-12345/edge_s.png`, `seed-777/edge_s.png`.
- **Confirmed root cause:** `oob.txt` scan showed TerrainMesh verts at exactly `x=-1.00`/`x=48.00`/`z=-1.00`/`z=48.00` on every border chunk (0.5 cell past the rect, materials `Moyva_Atlas` + `StylizedWater3_Toon`, y from -0.25 water level up to 1.45). Border-vertex dual fragments carry authored drop aprons covering the out-of-map quadrant.
- **Fix:** `ChunkTerrainMeshBuilder.ResolveBorderClampedMesh` — after warp/fill, sources whose footprint straddles the map rect get vertices XZ-clamped onto `[-0.5·cs, (W-0.5)·cs]`; the apron folds into a flush rim wall. Per-(mesh,center) cached, registered in `_meshRegistry`.
- **Verified:** `seed-42-v3/oob.txt` — zero TerrainMesh OOB verts (was ~7000); `edge_s`/`corner_sw`/`game_shore`/`game_iso` v3 show flush rim, no slivers; EditMode 102/102 PASS (`tests-editmode-20260925_012003.xml`).
- **Residual:** tree/grass props still overhang the rim by a few cm (canopy extent, `oob.txt` prop entries) — cosmetic, separate scope.

### VA-02 — Build-grid overlay visible on all cells in test launch (RESOLVED — intended onboarding)
- **Where:** whole map, real game view.
- **Seeds:** 42, 12345.
- **Expected:** grid only while construction mode is active.
- **Actual:** red/green cell outlines visible at startup in `DirectGameplayTest`.
- **Evidence:** `seed-42/game_default.png`, `seed-42/game_iso.png`, `seed-12345/game_default.png`.
- **Confirmed facts:** `GameModeService` defaults to `Normal`; `GameplayHtmlPresenter.EnsureInitialCastlePlacement` programmatically fires `UiActionIds.Construction.Open` when `IConstructionBootstrapQuery.RequiresInitialCastle(localOwner)` is true — a fresh world auto-enters Construction for the first-castle onboarding (`InitialCastleTransitionPolicy` blocks leaving). Overlay is `ConstructionBuildGridOverlayService`, correctly gated by `_isConstructionModeActive`.
- **Verdict:** NOT a defect — intended flow. The shots additionally verify the construction-grid requirement: cell outlines hug tile bounds exactly and colors match canonical `EvaluatePlacement` (green on grass, red on sand/water/unbuildable).
- **Re-verify (done):** green/red split matches `no-build` tags on sand/water in `game_default`/`game_iso`.

### VA-03 — Sand aprons wider than 1 tile on flat lowlands (RECLASSIFIED → VA-07)
- **Where:** low flat regions adjacent to water.
- **Seeds:** all (`sand-tile-003`+`sand-shore-band` = 834–960 cells; water 425–936).
- **Expected:** shoreline sand ≈ 0.5–1 tile band.
- **Actual:** locally wider sand flats where terrain ≤1.0 m is flat.
- **Evidence:** `seed-12345/low_sw.png` tan flats, `manifest.txt` tile counts.
- **Re-diagnosis (2026-09-25):** original attribution to the recipe Sand mask was
  **wrong**. Cell-stack dumps (`seed-42-e/sandstack.txt`) proved the excess came
  from two non-mask sources: `TerrainShorePlanner` lifts (banks raised to
  elevated river waterlines) and the legacy `TileWorldCreatorShoreBandService`
  expander (VA-07). Post-fix measurement: sand distance-to-water {1: 671, 2: 9}
  — the ring is effectively 1 cell; remaining >1 m sand is graded riverbank at
  the local waterline.
- **Re-verify:** `seed-42-noshore`/`seed-12345-noshore` `topdown`/`detail_shore`.

### VA-04 — Harness artifact: PRU first-shot missing water; late-shot translucent wash (FIXED ✓)
- First ortho `topdown` lacked water (gray); `topdown_persp`/`topdown_late` showed a pale wash.
- Real game camera rendered water correctly → was never a product defect.
- **Cause:** shared `PreviewRenderUtility` state across sequential `BeginPreview`/`EndStaticPreview` pairs.
- **Fix:** fresh `PreviewRenderUtility` per shot with `try/finally` cleanup in `WorldVisualSmoke`.
- **Verified:** `seed-42-v3/topdown.png` + `topdown_late.png` render water + foam identically clean.

### VA-05 — Harness artifact: `game_*` RT shots intermittently flat gray (FIXED ✓)
- seed-777 produced four identical 28 KB gray frames (reveal overlay still covering camera).
- **Fix:** `game_*` captures deferred ~2.5 s after mesh stability.
- **Verified:** `seed-42-v3` + `seed-12345-v2` — all four `game_*` shots valid, distinct sizes (88 KB–2.8 MB).

### VA-07 — Legacy shore-band expander paints sand on elevated banks (FIXED ✓)
- **Where:** every water-adjacent land cell — most visible on plateau
  riverbanks at 1.5–4.0 m.
- **Seeds:** all (seed-42: 182 cells; seed-12345 similar).
- **Expected:** sand only at the graded shoreline (~1 cell, at/below waterline
  +lift); cliff shores and elevated banks keep their biome.
- **Actual:** `sand-shore-band` tile id stamped on Dirt/Grass/Stone/RockCliff
  winners at full height — visual sand at 1.5–4.0 m, direct sand↔snow borders
  on the plateau, and a second ungraded sand ring around rivers.
- **Evidence:** `seed-42-e/sandstack.txt` (winner layers = Dirt/Grass/Stone/
  RockCliff at 1.5–4.0 m), `seed-42-e/tilemap.csv` dist-to-water {2:218, 3:2},
  `seed-12345` snow↔sand adjacency pairs.
- **Confirmed root cause:** `TileWorldCreatorShoreBandService.Expand` rewrites
  `GeneratedWorldData.BiomeMap` after export — 1-cell ring around ALL water, no
  height cap, no grading — gated by `worldData.HasAuthoredGeography`, a flag
  **nothing ever sets**, so it ran on recipe worlds too, duplicating/
  contradicting `TerrainShorePlanner`.
- **Fix:** `TileWorldCreatorWorldBuildBridge.PrepareTerrainData` — expander now
  runs only when `worldData.LogicalTileMap == null` (graph-produced worlds,
  where it is the only shoreline mechanism). One authoritative shore path for
  recipe worlds: `TerrainShorePlanner`.
- **Verified:** `seed-42-noshore` — `sand-shore-band` count 0 (was 182);
  heightmap byte-identical (0 diffs); sand dist {1:671, 2:9}; zero sand↔snow
  and zero sand↔rock-cliff land neighbors; `sandstack.txt` high cells are all
  planner lifts at river waterline (x.x1 signature). `seed-12345-noshore` —
  all 506 sand cells at dist 1; all 273 cells >1.05 m match `waterSurface+0.04`
  exactly; zero snow↔sand; `side_s` pixel-identical to pre-fix (zero geometry
  regression). EditMode post-fix: **816/816 PASS**.
- **Gameplay note:** BiomeMap is visual-only; `GameplayTileMap` (used by
  build/spawn rules) was never mutated — the fix removes a look↔logic
  inconsistency (cells that *looked* sand but *played* as grass/cliff).

### VA-06 — Prop canopy verts overhang map rim ≤0.3 cell (LOW — cosmetic residual)
- **Where:** border cells (x=0, x=47, z=0, z=47).
- **Seeds:** 42, 12345 (post-VA-01 `oob.txt` scans).
- **Expected:** prop geometry stays inside the world rect.
- **Actual:** canopy/billboard verts extend 0.05–0.3 units past the rim (e.g. `kaykit-tree-single-b_47_46` x→47.82, `grass-kreuz_0_25` x→−0.53). Trunks/footprints are inside valid land cells; only leaf/canopy polygons overhang the void.
- **Confirmed facts:** `seed-42-v3/oob.txt`, `seed-12345-v2/oob.txt` — 10 entries, all props, max overhang 0.30; zero TerrainMesh entries.
- **Assessment:** sub-cell overhang of organic models; invisible from gameplay angles (`low_sw` v3 shows rim trees look fine). Fixing would require shrinking or barring edge props — disproportionate.
- **Action:** none required; revisit only if a camera angle exposes it.

## Audit round 2 — 2026-09-25 (post-VA-01 code + final harness)

HEAD `83ad5ea3c`, recipe `seed:0`. Evidence: `docs/qa/evidence/validation/`
(seed-42-a/b/c, seed-777, seed-12345 — 20–23 shots each incl. `game_orbit_*`
real-camera orbit at low gameplay angle ≈ motion inspection).

**Coverage exercised:** topdown ×3 variants, side N/E/S/W, low SW, edge S,
corner SW, shore ×2 + ortho detail, transition + ortho detail, peak,
game camera default/iso/topdown/shore + 4-point orbit. All shots opened and
inspected for seeds 42-c, 12345; key shots for 777.

**New findings:** none at product level. Confirmed again: flush rim on all
borders all seeds; water only in bodies/channels; forests grounded; green
buildable overlay cluster = revealed spawn pocket, red = unbuildable/unrevealed.
VA-03 visual evidence strengthened: `seed-12345/game_orbit_s` + `game_default`
show sand aprons 3–6 cells wide on flat lowlands adjacent to water.

**Harness incident (fixed):** an earlier run was blocked ~13 min by Unity's
"Scene Backup Detected" modal (`Temp/__Backupscenes` left by
`EditorApplication.Exit`). Harness now deletes `Temp/__Backupscenes` before
exiting; subsequent runs start clean.

## Audit round 3 — rivers/lakes feature audit (2026-09-25)

Post-hydrology-contract code (bed heights, lake falls, `channelDepthMeters`).
Evidence: `docs/qa/evidence/audit-rl-seed{42,777,12345}/` — extended harness
with river/lake/waterfall/confluence-targeted shots + `waterstack.txt`
(per-cell layer stacks: `Height`=bed, `SurfaceHeight`=surface).
worldHash seed-42 `85B4F2FADD94AC0D40` stable across 3 launches; seed-777
`67BCBCA10268458038` stable; seed-12345 `239B47D25C36C327`.

**Verified on data, not just visuals:**
- River cells: `bed = surface − 0.35` exactly on every river (48 cells s-42,
  53 s-777) — `waterstack.txt`; `HeightMap` now carries real beds.
- Lake (s-12345): 50 cells, **uniform surface 2.47**, uniform bed 2.0
  (real floor); rim = 59 sand cells at 2.51 = `surface+0.04` graded shore.
- Rivers descend in 0.5 m terrace steps; drains to sea/border; acyclic
  (flood-parent tree); no floating `SurfaceOnly` sheets (0 cells where
  land < adjacent water surface — swamp = SolidTerrain, excluded).
- Waterfall strips emit on parent-edge drops ≥0.5 (`TryGetWaterfall` →
  `CollectWaterfallSource`); visible as thin cascades in `low_sw`.

### VA-08 — Open water-level seams on non-parent edges (FIXED — verified)
- **Where:** every water cell adjacent to lower water on an edge that is not
  its flow-parent edge — river mouths into sea, terrace steps with multiple
  lower neighbours, lake-to-sea adjacency.
- **Seeds:** 42 (13 gap edges ≥0.45 m), 12345 (26 edges; max 2.72 m:
  `River(27,19) S=2.97` ↔ `Water(27,20) S=0.25`; lake↔sea 2.22 m at
  `Lake(35,22)`↔`Water(35,23)` and `Lake(44,11)`↔`Water(45,12)`).
- **Expected:** every edge where water sheets meet at different levels is
  closed by a strip/wall — no open vertical gap in the water surface.
- **Actual:** `MarkWaterfalls`+`CollectWaterfallSource` emit a strip only on
  the flow-parent edge; e.g. `River(20,10)` (s-42) has **4** lower water
  neighbours (0.72 m gaps) — at most one is covered. Non-parent edges leave
  the upper sheet ending in air above the lower sheet.
- **Evidence:** `waterfall.png` crop — thin cyan ribbons hanging off bank
  edges at the river mouth (visible at close range); `low_sw` crop — thin
  vertical cyan slivers at channel steps; CSV gap scan above.
- **Severity:** medium-low — reads as small glitch slivers at close/low
  angles; nearly invisible at gameplay iso/topdown pitch.
- **Likely fix:** emit the closure strip on **every** edge where an adjacent
  water/sink cell's surface is ≥ threshold lower (driven by neighbour
  comparison, not only `FlowParent`); diagonal drops get a corner quad.
  Planner's `WaterfallMask` remains the *marker*; coverage becomes
  edge-complete in `CollectWaterfallSource`.
- **Re-verify:** gap-edge count from CSVs must equal strip-covered edges;
  close-up of a river mouth + lake rim.
- **FIX (implemented):** `TwcTileMeshSourceProvider.CollectWaterfallSource`
  rewritten edge-driven — for each water cell it iterates all 8 neighbours,
  queries the neighbour water surface via `IRecipeHydrologyMap
  .TryGetWaterSurface`, and emits a closure strip on every edge where
  `upperY - lowerY >= WaterfallMinDropMeters`. The upper cell owns the strip
  → exactly-once coverage, no duplicates. Non-water/border drains fail the
  surface query → no strips into void. Planner/store now expose
  `WaterfallMinDropMeters` (`RecipeHydrologyPlan.WaterfallMinDropMeters`,
  `IRecipeHydrologyMap.WaterfallMinDropMeters`).
- **Verified 2026-09-25:**
  - Seed-42 re-run (`audit-rl-seed42-va08/`): PASS, `nanVerts=0`,
    `empty=0`, `worldHash=85B4F2FADD94AC0D` — identical to pre-fix
    (closure strips are render-only, world data unchanged).
  - `low_sw` + `waterfall` crops: channel steps now show solid cyan water
    walls/cascades instead of dangling slivers; river mouth closed.
  - Seed-12345 re-run (`audit-rl-seed12345-va08/`): PASS, same
    `worldHash=239B47D25C36C327`; `lake` crop shows closure walls on the
    2.22 m lake↔sea rim edges; `waterfall` crop shows cascade at (4,30).
  - Hydrology suite: 17/17 PASS after the change.

### VA-09 — Diagonal waterfall strips (VERIFIED — acceptable)
- 6 cells in seed-42 drop ≥0.45 only toward a **diagonal** neighbour; the
  strip is a corner quad rotated 45°. Under the edge-driven emitter the
  same quads appear at diagonal lower-water edges; `detail_river` +
  `lake`/`waterfall` crops show the diagonal links read as thin water
  connections without visible clipping artifacts at audit camera angles.
- **Status:** accepted — no dedicated fix needed; revisit only if a
  gameplay camera angle exposes corner clipping.

## Work order

1. ~~VA-01~~ — **done** (flush rim verified on seeds 42 + 12345).
2. ~~VA-04 + VA-05~~ — **done** (v3 harness output clean).
3. ~~VA-02~~ — **resolved**: intended initial-castle onboarding, not a defect.
4. ~~VA-03~~ — **reclassified**: excess sand was VA-07's expander, not the mask.
5. ~~VA-07~~ — **fixed**: legacy `TileWorldCreatorShoreBandService` gated to
   graph worlds (`LogicalTileMap == null`); verified on seed-42.
6. ~~**VA-08**~~ — **fixed + verified**: `CollectWaterfallSource` is now
   edge-driven; all lower-water edges ≥ threshold get closure strips
   (seeds 42 + 12345 re-verified, world hashes unchanged).
7. ~~**VA-09**~~ — **verified**: diagonal corner quads read acceptably in
   post-fix evidence; no dedicated fix needed.
8. **VA-06** — prop canopy overhang: cosmetic, no action unless a camera angle exposes it.

## Audit-hygiene note

Recipe `seed` was temporarily set to 12345/777 for alternate-seed runs and has
been restored to `0` in both `Presets/.../testgeneratorrecipe.json` and the
generated `Resources/MoyvaConfigGenerated/` copy (verified 2026-09-25).

## Not verified this round

- True continuous camera motion (orbit stills are the proxy used).
- Fog-of-war transition animation (revealed/unrevealed states verified as
  static overlays; transitions between them not exercised).
- Spawned units/buildings visuals (no placements exercised).
- Menu → Gameplay and multiplayer launch paths.
- Underwater/underside views.
