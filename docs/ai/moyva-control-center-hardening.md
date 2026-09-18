# Moyva Control Center hardening patch

Base: `game-process` at commit `255316802cee8cdccf6a48fd7a1bfa09dbe5c4a3`.

This patch is intentionally limited to Control Center infrastructure. It does not change gameplay rules, rewards, BotDecisionContract, PPO semantics, or model schemas.

## Changes

- Fixes the observed Textual crash: `NoMatches: No nodes match '#dashboard-info'`.
- Makes periodic refresh and async operations safe while the TUI is closing/unmounting.
- Removes unsafe `query_one()` use from background refresh error handling.
- Distinguishes active training metrics from last/selected run metrics.
- Disables Stop/Run/Checkpoint actions when no valid selection or owned task exists.
- Adds training ETA calculated from structured TensorBoard step rate when available.
- Adds optional NVIDIA GPU/VRAM telemetry through `nvidia-smi` without making it a dependency.
- Improves training form spacing so labels and long paths are less likely to be clipped.
- Adds lifecycle regression tests.
- Makes `fullgame-smoke` a meaningful 10,000-step FullGame run using the normal episode decision budget instead of the previous 128-step/30-decision startup-only check.

## Install

Extract the ZIP directly over the Moyva project root. The archive contains project-relative paths only.

After installation run:

```bash
chmod +x moyva moyva-train
./moyva test quick
./moyva doctor
./moyva
```

If the training player is shown as `STALE`, use **Build → Rebuild** or:

```bash
./moyva build training --rebuild
```

For FullGame training run readiness first:

```bash
./moyva test readiness
```
