# Moyva Control Center V3 — simplified workflow

This revision focuses on making the training state understandable without knowing ML-Agents internals.

## Main UI

The sidebar is reduced to Home, Training, History, Models, Tools, Logs, and Settings.
Unity/build/tests/environment operations are grouped under Tools.

Training exposes only the common workflow by default:

1. choose training type;
2. optional run name;
3. Start fast, or Start & watch.

Advanced PPO/runtime fields are hidden under Advanced settings.

## Training state

The live card distinguishes:

- process is running but learning is not confirmed yet;
- ML-Agents has emitted step metrics and learning is confirmed;
- current step / maximum step;
- reward, episode length and steps/sec when available;
- elapsed time and ETA;
- checkpoints/final ONNX;
- exact results folder;
- CPU/RAM and optional NVIDIA GPU telemetry.

The progress bar is explicitly labeled. Before the first TensorBoard summary, the UI explains why it remains at 0%.
Default FullGame smoke metrics are emitted every 500 steps; watch mode every 250 steps.

## Closing the Control Center

On a normal Control Center close, training is stopped by default. Pressing Q with an active run opens a choice:

- keep training in the background and close the UI;
- stop training safely and close;
- cancel.

## Preview

Start & watch launches Visual training with a default 1280x720 window instead of relying on the project's previous tiny default player size.
Focus preview tries to focus/maximize the training window on Linux if `wmctrl` or `xdotool` is already present. Otherwise Alt+Tab remains the fallback.
