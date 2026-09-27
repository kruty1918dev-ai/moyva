# A00 — Evidence standards for the parallel audit

Acceptance conventions every wave applies. An item that cannot meet these is
`BLOCKED_RUNTIME` (or the equivalent honest status), never `PASS`.

## 1. Snapshot identity

Every snapshot in a run carries its own `{sequence, frame, gameTime,
wallTimeUtc}` (run-header.schema.json). Reused sequence numbers or identical
`wallTimeUtc` across "different" snapshots invalidate the set. For
world-generation evidence, pin to `WorldGeneratedDataSignal.StartupSequence` +
`SnapshotRevision` + `PublishedFrame` + `PublishedAtUtcTicks`.

## 2. Pause vs audit-script stop — these are different states

| State | Detectors |
|---|---|
| Game paused (real) | `IGameStateService.CurrentState == Paused`; `GamePausedSignal{IsPaused=true}` in trace; `Time.timeScale == 0` **only in single-player** (`GameStateService` skips the timescale write when `IGamePauseModePolicy.IsMultiplayerSessionActive`); `gameTime` frozen, `wallTimeUtc` advancing; editor `playMode=playing` |
| Audit harness stopped | No `GamePausedSignal`; `playMode` may still be `playing`; capture pipeline paused by the tool, not the game |
| Play mode exited | `playMode=stopped`; no frames at all |

A screenshot taken while the *script* is paused is not proof of a paused game;
a stopped editor is not a paused session.

## 3. Real gameplay evidence vs staged content

- Counted: `moyva-gameplay-ui-capture-window --view game` or
  `moyva-gameplay-ui-capture-runtime` output (actual GameView bitmap) of a live
  play session, correlated with `kind=signal, provenance=live-game` trace rows
  and a state transition in canonical queries (e.g. `UnitMovedSignal` + new
  position in `IUnitService`).
- Not counted: Marketing studio captures/directors
  (`Features/Marketing/Runtime/MarketingStudioController.cs`,
  `MarketingStillCapture.cs`), staged shots, editor SceneView-only captures
  without a running session.
- A screenshot named "warrior_moved" or "load" proves nothing without the
  corresponding state transition in the trace/queries.

## 4. Input provenance for UI claims

UI acceptance needs the normal user path: real keyboard/mouse through the
input stack (`Packages/com.kruty1918.input-context/`, `UiActions`). Bridge or
injected events (`eval`-driven signal fires, direct method calls) are valid
diagnostics but alone do not prove keyboard/mouse work — mark such rows
`provenance=bridge-injected`.

## 5. Leaf vs integrated results

A leaf's claim covers only its own worktree at its own base SHA. Integrated-game
claims belong to Q01's staging runs after merge — do not report leaf numbers as
integrated results or vice versa.

## 6. Concurrency

- Never kill/quit a foreign `Unity.exe`. Detect via `unity status`; record
  `environment.editorPid`/`pipelinePort` in the run header. The PID owning
  *this* worktree's Library is the only one a task may interact with.
- GameView/window captures are serialized: one capture at a time per editor;
  concurrent captures are resource-limited — schedule through Q01.
- Each worktree owns only its own `Library/`/`Temp/`; never cross-write.
- Each worktree runs on its own branch at the coordinator-pinned base SHA;
  never start from a moving remote HEAD.
