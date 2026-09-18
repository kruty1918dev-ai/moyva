# Performance Findings — analysis/performance-audit

Measured dataset + cause/solution per bottleneck. Numbers measured in Unity 6000.3.10f1
Editor (batchmode, clean HEAD worktree) on Ryzen AI 5 340 / Radeon 840M / 16 GB.
Editor ≈ close to player for CPU work; GPU uploads on iGPU will be worse than batchmode.
Budgets: 60 FPS = 16.7 ms, 30 FPS = 33.3 ms, 20 FPS = 50 ms per frame.

## Raw measurements (dataset)

| # | Operation | Median ms | Range ms | Samples |
|---|---|---|---|---|
| 1 | MoyvaJsonRuntime.EnsureLoaded cold (all presets) | 4070–6664 | — | 2 cold runs |
| 2 | HomeMenu scene load+activate | 658–769 | — | 2 runs |
| 3 | HomeMenu first-10-frames settle | 221–230 | — | 2 runs |
| 4 | Gamplay_Scene load+activate | 1061–1193 | — | 2 runs |
| 5 | Gamplay first-10-frames settle | 252–268 | — | 2 runs |
| 6 | Host.Mount cold (any panel) | 7.1–8.4 | 6.9–9.7 | 4×5 |
| 7 | Host.Mount warm rebuild | 6.4–9.4 | 6.0–9.5 | 4×10 |
| 8 | Host.Mount navigation A→B→A | 13–16.9 | 12.5–16.9 | 4×10 |
| 9 | Markup.Build Settings.Controls | 2.2–2.7 | 2.1–2.7 | 2×20 |
| 10 | Markup.Build other panels | 0.004–0.010 | — | 7×20 |
| 11 | XmlDocument.LoadXml Controls (29.9 KB) | 0.48 | 0.43–0.50 | 40 |
| 12 | XmlDocument.LoadXml Main (2.3 KB) | 0.05 | 0.04–0.05 | 40 |
| 13 | ControlsEditor.LiveSignature (12.5 Hz poll) | 0.066 | 0.06–0.07 | 40 |
| 14 | ControlsEditor.Conflict getter | 0.068–0.10 | 0.06–0.10 | 200 |
| 15 | ControlsEditor BoundAction/ReservedAction | 0.003–0.007 | — | 200 |
| 16 | Controls all-keys highlight (~85 keys) | 0.65–0.92 | 0.63–0.92 | 40 |
| 17 | MenuWorldPreviewGenerator.TryGenerate 64²/128²/192×108 | 8.1 / 42.0 / 49.7 | 7.9–49.7 | 20 (real graph eval) |
| 18 | MenuWorldPreviewTextureBuilder.Build 192×108→1024px | 54–82 | 53.3–82.5 | 9 (rasterizes tile prefabs) |
| 19 | IntegrityService.EnsureReadyForBuild 128² | 38.7–47.8 | 38.2–47.8 | 40 |
| 20 | Grid SetTileData full 128² (16 384 cells+signal) | 2.5–2.8 | 2.4–2.8 | 10 |
| 21 | Grid GetTileData ×10 000 | 0.65 | 0.65–0.69 | 10 |
| 22 | Pathfinder corner→corner 128² (empty) | 79.8–80.1 | 77.9–80.1 | 40 |
| 23 | Pathfinder 40-cell path | 16.8 | 16.5–17.0 | 40 |
| 24 | Texture2D SetPixels32+Apply 1024² | 2.9 | 0.5–3.3 | 40 |
| 25 | Texture2D Apply 512² / 256² / 128² | 0.92 / 0.02 / 0.007 | — | 40 |
| 26 | Fog RequestCellsUpdate 200 cells (incremental) | 0.006 | 0.005–0.009 | 20 |
| 27 | GameObject create+destroy ×500 | 1.6–7.7 | — | 10 |
| 28 | Zenject DiContainer() / bind×100 / resolve×100 | 0.006 / 0.16 / 0.21 | — | 100/10/10 |
| 29 | Gameplay idle frame | median 16.7 | min 14.3 / p95 16.9 / max 18.8 | 120 |
| 30 | HomeMenu idle frame | median 16.7 | min 5.1 / p95 16.9 / max 37.6 | 120 |

## Findings (cause → solution direction)

### F1. Startup is dominated by synchronous JSON preset loading — 4–6.7 s
**Cause:** `MoyvaJsonRuntime.EnsureLoaded` parses, resolves and validates the entire
preset database on first access, synchronously, on the main thread. Scene load itself
is fast (~0.7–1.2 s); the perceived "game takes long to start" is this call.
**Solution direction:** make config loading lazy/partial — load only the subset the
first screen needs, stream the rest in background; optionally persist a resolved
snapshot cache so cold starts skip re-validation. Target <1 s to interactive menu.

### F2. Menu navigation rebuilds the whole HTML document per action — 13–17 ms/click
**Cause:** any state change → `RenderIfNeeded` → new `UnityHtmlDocument` + full
`Mount` (re-parse + reconcile + layout). Every click ≈ one lost 30 FPS frame;
continuous interaction feels like constant stutter.
**Solution direction:** adopt the regional-update path already used by gameplay HUD
(`UpdateRegions`) for menu panels, or skip mount entirely when the produced markup
is unchanged. Target <8 ms per navigation.
**Status: IMPLEMENTED** — markup split into `moyva-nav` / `moyva-brand` regions
(`HomeMenuMoyvaUiMarkup` region builders); presenter tries `UpdateRegions` before
full `Mount`, and skips entirely when route+brand+modals are unchanged. Modals or
viewport change still force a full remount.

### F3. Presenter applies viewport layout every Tick even when nothing changed
**Cause:** `HomeMenuMoyvaUiPresenter.Tick` → `ApplyViewportLayoutNow` every frame:
canvas scale policy + safe-area rect + root stretch, regardless of viewport change.
**Solution direction:** apply layout only when the viewport class/rect actually
changes (and once at mount). Zero-cost in steady state.
**Status: IMPLEMENTED** — per-tick `ApplyViewportLayoutNow` removed from
`HomeMenuMoyvaUiPresenter.Tick`; the anchor's own `Update()` already reapplies
layout on `LayoutSignatureChanged`, and mount still applies it once.

### F4. Menu world preview — ~110 ms total on the menu-entry path
**Cause:** two synchronous stages on the calling thread:
(a) `MenuWorldPreviewGenerator.TryGenerate` — real graph evaluation,
8 ms (64²) / 42 ms (128²) / 50 ms (192×108), superlinear in cell count;
(b) `MenuWorldPreviewTextureBuilder.Build` — 54–82 ms, because it *renders each
tile prefab through `MoyvaPrefabPreviewRenderer`* to get pixels, not just
SetPixels — a hidden miniature render pipeline inside a menu.
Combined ≈100–130 ms ≈ 6–8 lost 60 FPS frames (or ~3 lost 30 FPS frames) at once.
**Solution direction:** (a) cache the generated preview per (graph, seed, size) —
it is deterministic; (b) build the texture from tile *sprites/color data* instead
of rendering prefabs, or rasterize on a background thread into raw buffers and
only `Apply` on the main thread; (c) worst case, defer behind a fade-in so the
hitch is not perceived as a freeze. Target ≤1 frame perceived.

### F5. GeneratedWorldDataIntegrityService — 40–48 ms on world-build path
**Cause:** integrity validation iterates and re-checks all 16k cells/maps
synchronously before visual build.
**Solution direction:** treat validation as a fast sanity pass (bounds/nulls) with
deep checks amortized or parallelized; it is a guard, not the build itself.

### F6. Pathfinder worst-case — ~80 ms for corner→corner on 128²
**Cause:** A* over 16k cells runs synchronously; a single long-range unit order
can freeze multiple frames; even a 40-cell path costs ~17 ms.
**Solution direction:** budget pathfinding per frame (queue + time-slice), move to
Jobs/Burst, or hierarchical pathfinding. Target <5 ms per request amortized.

### F7. Idle-menu frame spikes up to 37.6 ms
**Cause:** PlayMode idle sampling shows periodic >33 ms frames in the menu even
without input — consistent with per-tick layout work + preview simulation/cloud
tick + sporadic full re-renders.
**Solution direction:** same root fixes as F2/F3/F4 — remove per-frame work that
does not depend on actual change; then re-sample.

### F8. (healthy) Small systems are fine
Incremental fog updates (0.006 ms/200 cells), grid writes (2.5 ms full map),
controls lookups (<0.1 ms), Zenject resolve (<0.3 ms) are all within budget —
the problem is a few large synchronous blocks, not death-by-a-thousand-cuts.

## Not yet measured

- Full world CREATION beyond the preview graph (TWC visual build, placement,
  scene population) — the graph-eval part is now measured (F4a), the visual
  build orchestration still is not.
- Save/world-load path — no working save artifact at this commit.
- Real GPU texture upload cost — batchmode understates it on iGPU.
- Allocation column is broken (all 0.0 KB) — allocation capture not wired.

## Data integrity bug found (not perf, but blocks generation)

`moyva-generator-graph--generatorgraph` preset is corrupt: layer 'Base'
contains a connection referencing deleted nodes
(`f6197751-…→c85e2fa1-…` missing). `GraphEvaluationPipeline` fails fast
(~0.17 ms) with "cannot build execution plan". `testgeneratorgraph` works and
was used for the real timings. Whichever pipeline writes graph JSON dropped a
connection on node removal — worth a validation pass at save time.
