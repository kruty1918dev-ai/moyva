# Startup Visual E2E — Final Report

Date: 2026-09-20 · Branch: `task/startup-visual-report` (base `fix/startup-visual-e2e` @ `e96f3e466`)

Merged stack (A+D+E on `main` @ `b77a5d8bd`):

| Part | Commit | Content |
|------|--------|---------|
| A | `adb881bf4` | Opt-in startup frame capture harness (`StartupFrameCapture.cs`) |
| D | `478253d65` | Fix: fog surface-depth prepass clear value on reversed-Z |
| E | `e96f3e466` | Regression tests `FogOfWarSurfaceDepthClearTests` + URP asmdef ref |

## Root cause

- **File:** `Assets/Moyva/Scripts/Features/FogOfWar/Runtime/Visual/ScreenSpace/FogOfWarScreenSpaceRendererFeature.cs`
- **Method:** `RecordRenderGraph` (render-graph pass data setup, ~line 637 pre-fix)
- **Defect:** `passData.DepthClearValue = SystemInfo.usesReversedZBuffer ? 0f : 1f` passed a raw reversed-Z `0`. `CommandBuffer.ClearRenderTarget` expects the *logical* depth convention (1.0 = far plane on every platform; Unity maps it to the native buffer internally). On D3D11 (reversed-Z) the depth attachment was therefore cleared to **near**, every terrain fragment failed `ZTest LEqual` in `FogSurfaceDepth.shader`, the surface eye-depth texture stayed empty, `BuildScreenState` reported `surfaceValid=0` for the whole map, and the screen-space fog composite drew the unexplored veil over the starter reveal region.
- **Fix:** `internal static float ResolveSurfaceDepthClearValue() => 1f` (lines ~95–102) used unconditionally at `passData.DepthClearValue` (line ~640).

## Captured evidence (identical conditions)

Boot → HomeMenu → Solo → `Gamplay_Scene`, seed **877147485**, map 128×128, camera `[0.91, 15.32, 0.91]`, D3D11 editor play mode. Veil coverage = fraction of blue-gray unexplored-veil pixels per frame (downscaled 220×108, classifier `B−R>12 ∧ B≥G≥R−8 ∧ 55<B<190`).

### Frames: before vs after

| | Before (defective) | After (fixed) |
|---|---|---|
| Run | `20260920-190046-8a0c5f` | `20260920-190751-6ff86e`, `20260920-191513-156d47` |
| Representative frame | `frame_01240.png` (t=90.253 s) | `frame_01033.png` (t=77.098 s), `frame_01107.png` (t=81.220 s) |
| Veil coverage | **69.9 %**, never retracts | **4.5 % / ~5 %** after reveal lands |
| Gameplay-scene mean | 69.1 % (533/540 frames >50 %) | 5.8 % / 6.3 % |

Manual capture of the defect also kept at repo-root `realcap2.png` (19:19).

### First defective frame

- Veil onset (identical to healthy runs' pre-reveal state): `frame_00977.png`, monotonic **t = 77.003 s**, coverage 0.715.
- First *unambiguously* defective frame — veil still >0.5 beyond the max normal retraction lag observed in fixed runs (0.79 s): **`frame_00992.png`, t = 77.816 s**, coverage 0.713.
- Wall clock: run started 19:00:46 UTC (run id is UTC), capture-start at monotonic t = 13.672 s → onset ≈ **19:01:49 UTC**, confirmed-defective ≈ **19:01:50 UTC**.
- In the defective run the veil stayed at ~70 % for the remaining ~28 s of capture; in fixed runs it retracted permanently below 30 % within 0.68–0.79 s of onset and settled at ~5 %.

## Run table

| # | Run dir (`Temp/MoyvaCaptures/`) | Code state | Frames | Window (start→last frame) | Achieved fps | skipped | Gameplay veil | Result |
|---|-------------------------------|------------|--------|---------------------------|--------------|---------|---------------|--------|
| 1 | `20260920-190046-8a0c5f` | pre-fix | 1510 | 13.67→103.69 s (90.0 s cap) | 16.8 | 0 | 69.1 % sustained | defect reproduced |
| 2 | `20260920-190751-6ff86e` | post-fix | 1492 | 10.69→100.71 s (90.0 s cap) | 16.6 | 0 | 5.8 % | fixed |
| 3 | `20260920-191513-156d47` | post-fix | 1515 | 11.90→101.92 s (90.0 s cap) | 16.8 | 0 | 6.3 % | fixed (repeat) |

Scene boundaries: run 1 Boot 15.9–20.4 s → HomeMenu → Gamplay 74.6 s; run 2 Boot 13.5–17.2 s → Gamplay 51.9 s; run 3 Boot 14.1–17.9 s → Gamplay 58.7 s.

## Capture frequency & gaps

- Configured cadence: **50 ms** (20 fps) real-time, `WaitForEndOfFrame` + `ScreenCapture.CaptureScreenshot` (async encode); caps: 90 s duration / 1700 frames / 380 MiB.
- **Actual mean interval: 58 ms → ~17.2 fps** (≈86 % of nominal) in all three runs; steady-state drift is encode/main-thread cost, not dropped writes.
- `skipped` (frames never written by the async pipe): **0** in all runs.
- Gaps >100 ms per run: 18–20, concentrated at scene loads — max **~5.1 s** before first `Gamplay_Scene` frame (run 1 f970; run 2 f574 4.93 s; run 3 f699 4.97 s) and **~1.7–1.8 s** at the HomeMenu load. No systematic gaps during gameplay.
- Run 1 `summary.json` numbers are malformed (`durationSec:-12,46`, `achievedFps:151000,0`) — early harness revision wrote culture-sensitive (comma-decimal) JSON; committed version uses `InvariantCulture`. Values above are recomputed from `timeline.jsonl`.

## Compile / tests

- **Compile:** final domain reloads in `Editor.log` clean (compile time 1–2 ms, 0 errors). The only CS errors logged were transient mid-session states, both resolved by the committed code:
  - `FogOfWarScreenSpaceRendererFeature.cs(630,25) CS0103` — before `ResolveSurfaceDepthClearValue` existed;
  - `FogOfWarSurfaceDepthClearTests.cs CS0012/CS0117` — before the URP asmdef reference (added in `e96f3e466`).
- **Tests** (`…/AppData/LocalLow/DefaultCompany/Moyva/TestResults.xml`, 2026-09-20 19:21 UTC): **65 run, 64 passed, 1 failed**.
  - New regression tests all pass: `SurfaceDepthClearValue_IsLogicalFarPlane`, `SurfaceDepthClearValue_DoesNotDependOnReversedZ`, `SurfaceDepthShader_KeepsLEqualDepthTest` (also a focused 3/3-pass run earlier in the session).
  - The single failure `FogOfWarServiceCharacterizationTests.SimulationBindings_ExposeWorkingOwnerScopedFog` ("unresolved local perspective sees every owner") is on the FoW service code path untouched by this change — pre-existing characterization failure, needs separate triage (not verified against `main` baseline).

## Artifacts

- Captures (primary checkout `moyva/`): `Temp/MoyvaCaptures/<runId>/` — `frame_*.png`, `timeline.jsonl`, `summary.json`.
- Analysis cache: `Temp/ai/veil-coverage.json` (per-frame coverage, all runs).
- Manual defect screenshot: `realcap2.png` (root of primary checkout `moyva/`, untracked).
- Test results: `C:/Users/yelyzaveta.khodos.TP-GE26-20/AppData/LocalLow/DefaultCompany/Moyva/TestResults.xml`.
- Editor session log: `%LOCALAPPDATA%/Unity/Editor/Editor.log` (+ `Editor-prev.log`).

## Re-run

```
set MOYVA_STARTUP_CAPTURE=1
rem optional: set MOYVA_CAPTURE_DIR=<absolute output dir>
"C:\Users\yelyzaveta.khodos.TP-GE26-20\AppData\Local\Unity\Editors\6000.3.10f1\Editor\Unity.exe" -projectPath "<repo>"
```

(or `echo on > Temp\moyva-capture.on` for the marker file), then Play: **Boot → HomeMenu → Solo, seed 877147485, 128×128 → Gameplay**. Output: `Temp/MoyvaCaptures/<yyyyMMdd-HHmmss>-<id>/`. Focused tests: `tools/ai/unity-editmode-tests-quiet.sh FogOfWar` (EditMode).

## Unverified scenarios

- Non-D3D11 graphics APIs / non-reversed-Z targets (fix is convention-based, runtime-verified on D3D11 only).
- Standalone player builds — verified in-editor Play mode only.
- Sessions beyond the 90 s capture cap; late-game FoW states (explored-vs-veil boundaries, reveal areas added later).
- Other seeds/map sizes; multiplayer perspectives; direct `Gamplay_Scene` start (captured path was menu-driven).
- `skipped`-frame accounting path (0 skips observed; disk-cap branch never hit).
- Whether `SimulationBindings_ExposeWorkingOwnerScopedFog` also fails on `main` (pre-existing, out of scope).
