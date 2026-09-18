# Moyva Control Center

From the repository root, run **`./moyva`** (Windows: **`moyva.cmd`**). This opens a Textual terminal application with keyboard and mouse navigation. Press `1`, `2`, `3` for Dashboard, Training, Runs; `r` refreshes; `q` closes the interface. Running tasks survive closing the interface.

## First run and environment

```sh
./moyva setup
./moyva doctor
./moyva
```

The launchers prefer `MOYVA_PYTHON`, then a working `.venv-training`, then system Python. A missing interface prints a setup instruction and offers setup in an interactive terminal. Training requires Python 3.10.1–3.10.12, matching the existing launcher. Setup creates the project venv, installs `tools/ai/requirements-training.txt` and `requirements-cli.txt`, and repairs POSIX launcher permissions. A broken venv is moved into `.moyva-local/venv-backup-*`, preserving its contents.

Setup does not install system packages, invoke sudo, authenticate Unity Hub, or accept licenses. Missing prerequisites produce actionable instructions. Install the exact editor version from `ProjectSettings/ProjectVersion.txt`, plus the host platform's build module, through Unity Hub. Linux, Windows and macOS have platform-specific executable discovery and player suffixes; only the local host can be verified by a local test run.

Shared CLI defaults: `tools/ai/moyva-cli.json`. Machine settings, presets, process records and bridge data: ignored `.moyva-local/`. Environment overrides: `MOYVA_PYTHON`, `MOYVA_UNITY`, `MOYVA_RESULTS`; existing `UNITY_EDITOR_PATH` and `UNITY_PATH` remain supported. No machine-specific absolute path is committed to shared defaults.

```sh
./moyva settings
./moyva settings --set '{"unity":"/path/to/Unity","disk_warn_gib":12}'
```

The Windows launcher automatically selects `.venv-training\Scripts\python.exe`. `MOYVA_PYTHON` can point to a specific interpreter; setup also discovers Python 3.10 through `py` where available. Paths with spaces are passed as argument arrays, without a shell.

## Training

The control center delegates to the existing `tools/ai/moyva_train.py`; it does not add another trainer, policy, neural schema or PPO preset.

```sh
./moyva train --preset smoke --run-id smoke-002 --dry-run
./moyva train --preset smoke --run-id smoke-002
./moyva train --preset fullgame --run-id fullgame-001
./moyva train --preset fullgame --max-steps 100000 --wait
```

The UI's Training page exposes preset, run id, production curriculum stage, maximum steps, decision limit, seed, world size, time scale, graphics mode, YAML, checkpoint interval, worker port and results directory. Review settings, then start. Local presets can be saved, duplicated or reset; source JSON/YAML is untouched.

Preset stage values are read from `TrainingCurriculumStage`, including BasicLifecycle, Movement, Combat, Recruitment, Economy, Building, Objectives, FogOfWar and FullGame. Additional templates include `fullgame-smoke`, `long` and `visual-debug`.

Training runs as a detached owned worker. Preflight validates dependencies, disk, contract, configuration, port and player freshness; it builds a missing/stale player once before launching. FullGame additionally requires successful `ValidateFullGameScope` for the current source fingerprint. `strict` also runs relevant integration tests. `fast` and `standard` retain mandatory safety/readiness checks; standard is the default. No profile bypasses a FullGame readiness failure.

```sh
./moyva preflight --preset fullgame --profile standard
./moyva preflight --preset fullgame --profile strict --fix
./moyva processes
./moyva stop TASK_TOKEN
```

There is no fake pause operation. Stop requests cooperative shutdown; resume uses ML-Agents' persisted trainer state. The POSIX launcher forwards interruption only to its owned process group. Windows uses the existing kill-on-close Job Object; a file stop request also works when the launching Editor has no console. PID, executable, creation time and command fingerprint must match before a recovered task can be signalled. A stale PID is never treated as permission to kill a process.

## Runs and checkpoints

```sh
./moyva runs
./moyva run show smoke-001
./moyva run resume smoke-001
./moyva run clone smoke-001 smoke-variant
./moyva run compare smoke-001 smoke-002
./moyva checkpoints --run-id smoke-001
./moyva checkpoint favorite smoke-001/MoyvaStrategy.onnx
./moyva checkpoint label smoke-001/MoyvaStrategy.onnx candidate-a
./moyva checkpoint export smoke-001/MoyvaStrategy.onnx /chosen/location/model.onnx
```

Existing `Results/MoyvaTraining/*/run.json`, `resume.json`, `trainer.yaml`, `.pt`, `.onnx` and TensorBoard files are discovered without migration. Legacy runs without an exit record are marked resumable/interrupted, not successful merely because an ONNX exists. Corrupt metadata and contract mismatch are shown explicitly. Labels and favorites are separate metadata; checkpoint filenames are not renamed.

New runs additionally retain `effective-config.json`, `cli-status.json`, `cli.log`, `mlagents.log` and failure classification where applicable. Resuming preserves the original `run.json`, archives prior effective YAML and Unity logs, and writes `resume.json`. Existing commands `./moyva-train doctor|build|train|resume|visual|tensorboard|info` remain supported; the wrapper now also selects the project venv.

ML-Agents resumes a **run's latest persisted checkpoint state**. Selecting an arbitrary older `.pt` or an ONNX is not presented as equivalent. Use `run resume`, or clone settings to a new run. Checkpoint comparison shows metadata and compatibility; it does not claim playing strength.

The current production inference binding requires a serialized `ModelAsset`. There is no external model-loading/evaluation-episode API, so `evaluate` and Test model report that boundary honestly. ONNX export writes the current contract manifest; assigning a model to `MoyvaBotPolicyBinding` remains an explicit Unity operation. No model is silently activated in production.

Deletion and overwriting require explicit exact-name/path confirmation:

```sh
./moyva run delete obsolete-run --confirm obsolete-run
./moyva checkpoint delete obsolete-run/MoyvaStrategy-100.pt --confirm obsolete-run/MoyvaStrategy-100.pt
```

Active runs cannot be deleted. Traversal and symlink escapes outside results are rejected. Checkpoints are never automatically removed by doctor.

## Unity and tests

```sh
./moyva unity open
./moyva unity training-scene
./moyva unity play
./moyva unity stop
./moyva unity gameplay-scene
./moyva unity monitor
./moyva unity control-center
./moyva unity logs
./moyva build training
./moyva test quick
./moyva test fullgame
./moyva test readiness
./moyva test communicator
```

The Editor bridge is an allow-list of project-local JSON commands under `.moyva-local/bridge`. It opens only the actual training/gameplay scenes, controls Play Mode, opens the monitor/window, builds the dedicated player and runs the configured tests/readiness method. It has no HTTP listener, arbitrary eval or shell command. It rejects another project, expired requests and unsaved scenes. `unity save-scenes` explicitly saves open scenes. Play/stop/test requests retain their identity through domain reloads using Editor SessionState. Responses and XML reports contain actual completion/failure.

The native window is **Moyva → AI → Training Control Center**. It uses the same Python commands and `dashboard.json` data for environment, runs, checkpoints, metrics and process actions; it does not implement another training launcher. The existing Training Monitor remains available.

When no Editor owns the project, build/test commands use Unity batch mode. When the Editor is open, they use the bridge. A busy or unavailable bridge reports a blocking error rather than force-closing Unity. Builds are invalidated by source/asset/config changes and contract mismatch. Long operations can be launched with `--background`; poll `processes` or the dashboard.

Communicator smoke launches the smoke training preset; inspect its task completion and `unity.log` before claiming a pass. ML-Agents may finish a policy buffer beyond the requested small `max_steps`; this is a smoke workflow, not a strength evaluation.

## Metrics, TensorBoard and diagnostics

```sh
./moyva tensorboard open
./moyva tensorboard stop --token TASK_TOKEN
./moyva logs smoke-001 --source error
./moyva doctor --fix
./moyva disk
```

Metrics come from bounded, cached TensorBoard scalar reads: step, mean reward, episode length, losses and rates where recorded. Missing data stays unavailable. CPU/RAM and owned worker state are inspected through psutil. No reward, win rate or action distribution is inferred from prose logs. Existing native telemetry remains in the Training Monitor.

TensorBoard binds to localhost; if the requested port is occupied, another local free port is selected and reported. Stop uses its task token, never a process-name kill.

Doctor classifies known dependency, contract, disk, port, lock, communicator, compile, build and test failures. Safe repairs are explicit `doctor --fix` actions. Stale Unity locks require proof of no matching live editor plus `--confirm 'remove stale Unity lock'`. Source/runtime failures are never “repaired” by changing gameplay or forcing checkpoint compatibility. No retry overwrites a run; a failed run retains its evidence and requires an explicit resume after repair.

Disk cleanup is limited to selected project outputs:

```sh
./moyva clean temp-ai --confirm temp-ai
./moyva clean training-build --confirm training-build
```

There is no automatic Library, system-cache or external-directory deletion. Free-space thresholds are configurable. CLI/Unity validation logs live under `.moyva-local`, so Unity clearing `Temp` on startup cannot erase them.

## Automated CLI validation

```sh
./moyva test quick
# equivalent
.venv-training/bin/python -m unittest discover -s tools/ai/tests -v
```

Ordinary Python tests use temporary run directories and mocked process boundaries; they do not launch Unity. Textual's headless pilot tests navigation, preset selection and settings review. Unity tests, builds, bridge commands and smoke training are separately executed workflows with explicit results; a unit-test pass does not imply those external workflows passed.

### Gameplay preview and sequential lessons

`castle-first` starts the learner without buildings, grants the normal starter
resources, and exposes production construction actions (Building stage). The
heuristic opponent keeps its starter settlement. Other stage presets keep their
existing setup; stages unlock capabilities cumulatively, rather than isolating
one action or guaranteeing mastery. Rewards and the neural contract are unchanged.

```bash
./moyva train --preset castle-first --run-id lesson-castle
./moyva train --preset recruitment --initialize-from lesson-castle --run-id lesson-recruitment --visual
./moyva train --preset fullgame --initialize-from lesson-recruitment --run-id lesson-fullgame --visual
./moyva train --preset arenas-preview --run-id four-arenas
```

Wait for a checkpoint before using a run as a source. `--initialize-from` initializes
a **new** run from compatible saved weights in the same results directory; it does
not merge independently trained models. `run resume` continues the same run.
Fine-tuning can forget previous skills: inspect performance again in FullGame.
In the TUI, Advanced settings exposes **Parallel arenas** and **Continue skills
from run ID**. Choose `castle-first` to enable the first-castle setup.

The Unity spectator overlay provides speed 0.1–20×, zoom, fit-map, arrow-key pan,
side-colored unit/building labels, current action, turn and reward. Building meshes
come from the production registry and follow authoritative placement snapshots;
no prefab behaviours or colliders are instantiated by the spectator. This is a
training spectator, not the full interactive gameplay HUD or terrain renderer.

`--arenas 1..16` uses independent Unity processes with one shared ML-Agents trainer.
Worker seeds are deterministic and distinct. Visual TUI launches tile their native
windows on Linux/X11 when wmctrl/xdotool is available; other platforms use ordinary
Unity windows. This is a window mosaic, not several worlds inside one Unity scene.
Multi-worker logs use ML-Agents' separate Player-N.log files.
