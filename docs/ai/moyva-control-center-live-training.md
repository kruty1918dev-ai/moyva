# Moyva Control Center — live training UX update

This patch focuses on making training understandable from the TUI instead of exposing internal worker JSON.

## Training workflow

The Training / Live page now defaults to **FullGame smoke** and explains each preset.

Two start actions are available:

- **Start training** — uses the selected preset/settings. Normal FullGame presets are headless for speed.
- **Start with live preview** — forces Visual mode and time scale 1 so the Unity training player opens as a visible window.

Starting a run no longer opens the raw supervisor token/PID JSON dialog. The page remains on Training / Live and shows the active run.

## Active training panel

While a run is active the page shows:

- run id
- startup/training phase
- FullGame/curriculum stage
- headless vs visible-preview mode
- current step / max steps
- reward
- episode length
- steps per second
- elapsed time
- ETA when enough metrics exist
- checkpoint count
- final ONNX status
- exact results directory
- CPU and RAM usage
- optional NVIDIA GPU/VRAM metrics when `nvidia-smi` is available
- recent training logs

Quick actions are provided for:

- Stop safely
- Open logs
- Checkpoints
- Open results folder
- TensorBoard
- Preview information

## Preview limitation

ML-Agents launches headless runs with `--no-graphics`. Graphics cannot be safely attached to an already running headless Unity environment. To watch the actual training environment, start the run with **Start with live preview** or select the **fullgame-preview** preset.

## Preflight

The TUI's Preflight button now uses safe repair. A stale/missing training player is rebuilt instead of only returning `Training player missing/stale; build required`.

## Presets

Added/updated:

- `smoke` — pipeline/communicator check
- `fullgame-smoke` — recommended first real FullGame validation (10k steps)
- `fullgame-preview` — visible FullGame smoke run
- `fullgame` — normal 500k FullGame training
- `long` — 5M FullGame run
- `visual-debug` — Movement-stage visual debug
