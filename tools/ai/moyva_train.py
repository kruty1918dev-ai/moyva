#!/usr/bin/env python3
"""Dedicated Moyva training launcher with checkpoint-frozen autonomous evaluation."""
import argparse
import datetime as dt
import hashlib
import importlib.metadata
import json
import os
from pathlib import Path
import re
import shutil
import signal
import subprocess
import sys
import time
from moyva_cli.telemetry import TrainingTelemetry

ROOT = Path(__file__).resolve().parents[2]
TRAINING = ROOT / "Assets/Moyva/AI/Training"
CONFIG = ROOT / "Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"
SCENE = TRAINING / "Scenes/MoyvaTraining.unity"
BEHAVIOR = "MoyvaStrategy"
RESULTS = ROOT / "Results/MoyvaTraining"


class LaunchError(Exception):
    def __init__(self, message, code=2):
        super().__init__(message)
        self.code = code


class WindowsProcessJob:
    """Closing this handle kills only this launcher's trainer and descendants."""
    def __init__(self, process):
        import ctypes
        from ctypes import wintypes
        self.api = ctypes.WinDLL("kernel32", use_last_error=True)
        self.api.CreateJobObjectW.restype = wintypes.HANDLE
        self.api.CreateJobObjectW.argtypes = [ctypes.c_void_p, wintypes.LPCWSTR]
        self.api.SetInformationJobObject.argtypes = [wintypes.HANDLE, ctypes.c_int, ctypes.c_void_p, wintypes.DWORD]
        self.api.AssignProcessToJobObject.argtypes = [wintypes.HANDLE, wintypes.HANDLE]
        self.api.CloseHandle.argtypes = [wintypes.HANDLE]
        self.handle = self.api.CreateJobObjectW(None, None)
        if ctypes.sizeof(ctypes.c_void_p) != 8:
            raise LaunchError("Training requires a 64-bit Python process.")
        limits = ctypes.create_string_buffer(144)
        ctypes.c_uint32.from_buffer(limits, 16).value = 0x2000
        if not self.handle or not self.api.SetInformationJobObject(self.handle, 9, limits, 144) \
                or not self.api.AssignProcessToJobObject(self.handle, int(process._handle)):
            error = ctypes.get_last_error()
            if self.handle:
                self.api.CloseHandle(self.handle)
                self.handle = None
            process.terminate(); process.wait()
            raise LaunchError("Cannot establish owned process cleanup (Windows error %s)." % error)

    def close(self):
        if self.handle:
            self.api.CloseHandle(self.handle)
            self.handle = None


def log(message):
    print(f"[MoyvaTrain] {message}", flush=True)


def contract(root=None):
    source = ((Path(root) if root else ROOT) / "Assets/Moyva/AI/Bot/Core/Contracts/BotDecisionContract.cs").read_text()
    signature = source.split("Encoding.UTF8.GetBytes(", 1)[1].split(").Replace", 1)[0]
    signature = signature.split('.Replace', 1)[0]
    strings = re.findall(r'"([^"\\]*(?:\\.[^"\\]*)*)"', signature)
    if not strings or not strings[0].startswith("MoyvaBot:"):
        raise LaunchError("Cannot read BotDecisionContract signature; update the launcher parser.")
    def constant(name):
        found = re.search(r"\b" + name + r"\s*=\s*(\d+)", source)
        if not found: raise LaunchError("Cannot read contract constant: " + name)
        return int(found.group(1))
    slots = constant("MaxCandidateSlots")
    return dict(behavior=BEHAVIOR, version=constant("ContractVersion"),
                hash=hashlib.sha256("".join(strings).encode()).hexdigest(), candidateSlots=slots,
                observations=constant("GlobalFeatureCount") + constant("SpatialFeatureCount") + slots * constant("CandidateFeatureCount"))


def resolve(path):
    path = Path(path).expanduser()
    return path.resolve() if path.is_absolute() else (ROOT / path).resolve()


def unity_path(explicit=None):
    candidates = [explicit, os.environ.get("MOYVA_UNITY"), os.environ.get("UNITY_EDITOR_PATH"), os.environ.get("UNITY_PATH")]
    version = re.search(r"m_EditorVersion: (.+)", (ROOT / "ProjectSettings/ProjectVersion.txt").read_text()).group(1).strip()
    if sys.platform == "win32":
        local = Path(os.environ.get("LOCALAPPDATA", ""))
        candidates += [local / f"Unity/Editors/{version}/Editor/Unity.exe", Path("C:/Program Files/Unity/Hub/Editor") / version / "Editor/Unity.exe"]
    elif sys.platform == "darwin":
        candidates += [Path("/Applications/Unity/Hub/Editor") / version / "Unity.app/Contents/MacOS/Unity"]
    else:
        candidates += [Path.home() / "Unity/Hub/Editor" / version / "Editor/Unity", Path("/opt/unity/Editor/Unity")]
    for candidate in candidates:
        if candidate and Path(candidate).is_file(): return str(Path(candidate).resolve())
    raise LaunchError("Unity Editor not found. Set MOYVA_UNITY or pass --unity /path/to/Unity.")


def player_path(args):
    if args.env: return resolve(args.env)
    suffix = {"windows": ".exe", "linux": ".x86_64", "macos": ".app"}[args.target]
    return ROOT / ("Build/Training/MoyvaTraining" + suffix)


def prerequisite(args, trainer=True):
    if not (3, 10, 1) <= sys.version_info[:3] <= (3, 10, 12):
        raise LaunchError("ML-Agents 1.1.0 requires Python 3.10.1–3.10.12. Select it with MOYVA_PYTHON.")
    unity = unity_path(args.unity)
    for path in (SCENE, CONFIG, resolve(args.trainer)):
        if not path.is_file(): raise LaunchError("Required file missing: " + str(path))
    config = json.loads(CONFIG.read_text(encoding="utf-8-sig"))
    if config.get("allowScaffoldSimulation", False):
        raise LaunchError("Training rejects allowScaffoldSimulation=true. Set it to false in MoyvaTrainingConfig.json.")
    if config.get("behaviorType", 0) != 0:
        raise LaunchError("Training requires behaviorType=0 (Default).")
    if trainer:
        try: version = importlib.metadata.version("mlagents")
        except importlib.metadata.PackageNotFoundError:
            raise LaunchError("ML-Agents missing. Install tools/ai/requirements-training.txt in this Python environment.")
        if version != "1.1.0": raise LaunchError(f"Expected mlagents==1.1.0; found {version}.")
        check = subprocess.run([sys.executable, "-m", "mlagents.trainers.learn", "--help"], capture_output=True, text=True)
        if check.returncode: raise LaunchError("mlagents-learn cannot start: " + (check.stderr or check.stdout).strip()[-1200:])
    results = resolve(args.results); results.mkdir(parents=True, exist_ok=True)
    probe = results / (".write-test-" + str(os.getpid()))
    try: probe.write_text("")
    finally: probe.unlink(missing_ok=True)
    return unity, config


def run_process(command, verbose=True, logfile=None):
    """One process group; termination never targets unrelated Unity/Python processes."""
    stream = open(logfile, "a", encoding="utf-8") if logfile else None
    kwargs = {"creationflags": subprocess.CREATE_NEW_PROCESS_GROUP} if os.name == "nt" else {"start_new_session": True}
    process = subprocess.Popen(command, cwd=ROOT, stdout=stream, stderr=subprocess.STDOUT if stream else None, **kwargs)
    job = WindowsProcessJob(process) if os.name == "nt" else None
    interrupted = False
    def interrupt(_signum, _frame):
        nonlocal interrupted
        interrupted = True
        if process.poll() is None:
            if os.name == "nt":
                try: process.send_signal(signal.CTRL_BREAK_EVENT)
                except OSError: process.terminate()
            else: os.killpg(process.pid, signal.SIGINT)
    old_handlers = {s: signal.signal(s, interrupt) for s in (signal.SIGINT, signal.SIGTERM)}
    try:
        while process.poll() is None:
            stop_file = os.environ.get("MOYVA_CLI_STOP_FILE")
            if stop_file and Path(stop_file).exists() and not interrupted: interrupt(signal.SIGINT, None)
            if interrupted:
                try: process.wait(timeout=10)
                except subprocess.TimeoutExpired:
                    if os.name == "nt": subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                    else: os.killpg(process.pid, signal.SIGKILL)
                    process.wait()
                break
            time.sleep(0.2)
        return 130 if interrupted else process.returncode
    finally:
        for s, handler in old_handlers.items(): signal.signal(s, handler)
        if os.name != "nt":
            try: os.killpg(process.pid, signal.SIGTERM)
            except ProcessLookupError: pass
        if stream: stream.close()
        if job: job.close()


def build(args, unity=None):
    unity = unity or unity_path(args.unity)
    player = player_path(args); player.parent.mkdir(parents=True, exist_ok=True)
    build_log = ROOT / ".moyva-local/logs" / ("training-build-" + dt.datetime.now(dt.timezone.utc).strftime("%Y%m%d-%H%M%S-%f") + ".log")
    build_log.parent.mkdir(parents=True, exist_ok=True)
    target = {"windows": "StandaloneWindows64", "linux": "StandaloneLinux64", "macos": "StandaloneOSX"}[args.target]
    command = [unity, "-batchmode", "-nographics", "-quit", "-projectPath", str(ROOT), "-logFile", str(build_log),
               "-executeMethod", "Kruty1918.Moyva.AI.Training.Editor.TrainingPlayerBuilder.Build",
               "-moyvaBuildTarget", target, "-moyvaBuildOutput", str(player)]
    log("Building dedicated training player; log=" + str(build_log))
    code = run_process(command)
    if code or not player.exists() or not Path(str(player) + ".contract.json").is_file():
        tail = build_log.read_text(encoding="utf-8", errors="replace").splitlines() if build_log.exists() else []
        errors = [line for line in tail if "error" in line.lower() or "Exception" in line]
        raise LaunchError("Unity build failed (exit %s). %s\nLog: %s" % (code, "\n".join(errors[-8:]), build_log), code or 2)
    latest = ROOT / "Temp/ai/training-build.log"; latest.parent.mkdir(parents=True, exist_ok=True)
    if build_log.exists(): shutil.copy2(build_log, latest)
    log("Build complete: " + str(player)); return player


def git_value(*args):
    try: return subprocess.check_output(["git", *args], cwd=ROOT, text=True, stderr=subprocess.DEVNULL).strip()
    except (OSError, subprocess.CalledProcessError): return None


def _autonomous(config):
    authored = ((config.get("curriculum") or {}).get("autonomous") or {})
    values = dict(enabled=True, evaluationEverySteps=10000, evaluationEpisodes=50,
                  masteryThreshold=0.80, regressionThreshold=0.65, masteryChecksRequired=3)
    values.update(authored)
    return values


def _write_trainer(path, trainer):
    import yaml
    path.write_text(yaml.safe_dump(trainer), encoding="utf-8")


def _trainer_command(args, effective_trainer, run_id, player, results, arenas, seed, speed, mode,
                     size, stage, initialize, resume, run_dir):
    command = [sys.executable, "-m", "mlagents.trainers.learn", str(effective_trainer), "--run-id", run_id,
               "--env", str(player), "--results-dir", str(results), "--num-envs", str(arenas), "--base-port", str(args.base_port),
               "--seed", str(seed), "--time-scale", str(speed)]
    command.append("--resume" if resume else "--force")
    if initialize and not resume: command += ["--initialize-from", initialize]
    if mode == "HeadlessFast": command.append("--no-graphics")
    if args.verbose: command.append("--debug")
    command += ["--env-args", "-moyvaRequireTrainer", "-moyvaTrainingMode", mode, "-moyvaSeed", str(seed),
                "-moyvaWorldSize", str(size), "-moyvaCurriculumStage", str(stage), "-moyvaTrainingTimeScale", str(speed),
                "-moyvaBasePort", str(args.base_port)]
    if arenas == 1: command += ["-logFile", str(run_dir / "unity.log")]
    if getattr(args, "learn_initial_castle", False): command.append("-moyvaLearnInitialCastle")
    if mode == "Visual":
        command += ["-screen-fullscreen", "0", "-screen-width", str(getattr(args, "screen_width", 1280) or 1280),
                    "-screen-height", str(getattr(args, "screen_height", 720) or 720)]
    if args.episode_decisions: command += ["-moyvaEpisodeDecisions", str(args.episode_decisions)]
    return command


def _archive_unity_log(run_dir):
    if (run_dir / "unity.log").exists():
        stamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%d-%H%M%S-%f")
        shutil.copy2(run_dir / "unity.log", run_dir / ("unity-" + stamp + ".log"))


def _validated_segment_checkpoint_step(checkpoint_step, target_step, next_evaluation_boundary=None):
    """Return the real serialized checkpoint at/after a requested segment threshold.

    ML-Agents may finish the last trajectory before serializing, so a max_steps=10000
    segment can legitimately produce e.g. MoyvaStrategy-10060.pt. Provenance must keep
    the real checkpoint step; only stopping before the threshold or skipping an entire
    evaluation boundary is invalid.
    """
    if checkpoint_step is None:
        raise ValueError("ML-Agents finished the segment without a serialized training checkpoint.")
    actual = int(checkpoint_step)
    target = int(target_step)
    if actual < target:
        raise ValueError(f"Serialized checkpoint {actual} is before requested boundary {target}.")
    if next_evaluation_boundary is not None and actual >= int(next_evaluation_boundary):
        raise ValueError(
            f"Serialized checkpoint {actual} skipped evaluation boundary {int(next_evaluation_boundary)}.")
    return actual


def _evaluation_is_overdue(current_step, latest_evaluation, eval_every):
    """True when training crossed an evaluation bucket that has no completed frozen eval."""
    current = int(current_step)
    interval = int(eval_every)
    if current < interval:
        return False
    latest = latest_evaluation or {}
    completed_step = int(latest.get("checkpoint_step", 0) or 0) if latest.get("state") == "COMPLETED" else 0
    return current // interval > completed_step // interval


def train(args):
    unity, config = prerequisite(args)
    import yaml
    from moyva_cli.config import ControlError, atomic_json, utc
    from moyva_cli.evaluation import (EvaluationStore, choose_evaluation_scenario, newest_training_checkpoint,
                                      run_frozen_evaluation, snapshot_frozen_checkpoint,
                                      update_curriculum_latest_checkpoint, verify_resume_checkpoint)

    trainer_path = resolve(args.trainer)
    trainer = yaml.safe_load(trainer_path.read_text())
    if set(trainer.get("behaviors", {})) != {BEHAVIOR}: raise LaunchError("Trainer YAML must contain exactly one behavior: MoyvaStrategy.")
    run_id = args.run_id or dt.datetime.now(dt.timezone.utc).strftime("moyva-%Y%m%d-%H%M%S")
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_.-]*", run_id): raise LaunchError("Run id must be a simple name (letters, digits, dot, dash, underscore).")
    results = resolve(args.results); run_dir = results / run_id; current = contract()
    resume = args.command == "resume" or args.resume
    if resume:
        metadata = run_dir / "run.json"
        if not metadata.is_file() or not list(run_dir.glob("MoyvaStrategy/*.pt")):
            raise LaunchError("Resume requires existing run.json and a MoyvaStrategy checkpoint: " + str(run_dir))
        previous = json.loads(metadata.read_text())
        if previous["contract"]["hash"] != current["hash"]:
            if not args.force_contract_mismatch:
                raise LaunchError("MODEL CONTRACT MISMATCH\ncheckpoint: " + previous["contract"]["hash"] + "\nruntime: " + current["hash"])
            log("WARNING: forcing incompatible checkpoint contract.")
    elif run_dir.exists() and not args.force:
        raise LaunchError("Run already exists. Use resume or choose a new --run-id.")

    player = player_path(args)
    if not player.exists():
        if args.no_build: raise LaunchError("Training player missing. Run moyva-train build or omit --no-build.")
        player = build(args, unity)
    manifest_path = Path(str(player) + ".contract.json")
    if not manifest_path.is_file() or json.loads(manifest_path.read_text()).get("hash") != current["hash"]:
        if args.no_build: raise LaunchError("Player contract is missing or stale. Rebuild with moyva-train build.")
        log("Training player contract is stale; rebuilding before launch.")
        player = build(args, unity); manifest_path = Path(str(player) + ".contract.json")
        if not manifest_path.is_file() or json.loads(manifest_path.read_text()).get("hash") != current["hash"]:
            raise LaunchError("Built player contract still does not match the runtime contract.")

    mode = "Visual" if args.command == "visual" or args.visual else "HeadlessFast"
    seed = args.seed if args.seed is not None else config.get("baseSeed", 1918)
    size = args.world_size if args.world_size is not None else config.get("worldSize", 24)
    stage = args.stage if args.stage is not None else config["curriculum"]["stage"]
    speed = args.time_scale if args.time_scale is not None else config.get("visualTimeScale" if mode == "Visual" else "headlessTimeScale", 1)
    if not 0.1 <= speed <= 20 or not 12 <= size <= 128 or not 0 <= stage <= 8:
        raise LaunchError("Allowed ranges: time-scale 0.1–20; world-size 12–128; stage 0–8.")
    arenas = getattr(args, "arenas", 1)
    if not 1 <= arenas <= 16: raise LaunchError("Arenas must be between 1 and 16.")
    autonomous = _autonomous(config)
    eval_every = int(autonomous["evaluationEverySteps"])
    eval_episodes = int(autonomous["evaluationEpisodes"])
    if eval_every < 1 or eval_episodes < 1: raise LaunchError("Autonomous evaluation interval/episode count must be positive.")

    initialize = getattr(args, "initialize_from", None)
    if initialize:
        from moyva_cli.config import simple_name
        simple_name(initialize)
        if resume or initialize == run_id: raise LaunchError("Initialize-from needs a new run, not resume or the source run.")
        source = results / initialize
        if not (source / "run.json").is_file() or not list(source.glob("MoyvaStrategy/*.pt")):
            raise LaunchError("Initialize-from requires an existing training checkpoint in this results directory.")
        if json.loads((source / "run.json").read_text())["contract"]["hash"] != current["hash"]:
            raise LaunchError("Initialize-from checkpoint contract mismatch.")
    if getattr(args, "learn_initial_castle", False) and stage < 5:
        raise LaunchError("The initial castle lesson requires Building or later curriculum.")

    run_dir.mkdir(parents=True, exist_ok=resume or args.force)
    behavior = trainer["behaviors"][BEHAVIOR]
    if args.max_steps: behavior["max_steps"] = args.max_steps
    overall_max_steps = int(behavior.get("max_steps", 0) or 0)
    if overall_max_steps < 1: raise LaunchError("Trainer max_steps must be positive.")
    if args.checkpoint_interval:
        if args.checkpoint_interval < 1: raise LaunchError("Checkpoint interval must be positive.")
        behavior["checkpoint_interval"] = args.checkpoint_interval
    if getattr(args, "summary_freq", None):
        if args.summary_freq < 1: raise LaunchError("Summary frequency must be positive.")
        behavior["summary_freq"] = args.summary_freq
    elif int(behavior.get("summary_freq", 500) or 500) > 500:
        behavior["summary_freq"] = 500

    effective_trainer = run_dir / "trainer.yaml"
    if resume and effective_trainer.exists():
        stamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%d-%H%M%S")
        shutil.copy2(effective_trainer, run_dir / ("trainer-" + stamp + ".yaml"))
    authored_effective = json.loads(json.dumps(trainer))
    _write_trainer(effective_trainer, trainer)
    metadata = dict(run_id=run_id, started_utc=dt.datetime.now(dt.timezone.utc).isoformat(), commit=git_value("rev-parse", "HEAD"),
                    branch=git_value("branch", "--show-current"), contract=current, trainer_config=str(trainer_path),
                    seed=seed, world_size=size, stage=stage, mode=mode, executable=str(player), arenas=arenas,
                    initialize_from=initialize, learn_initial_castle=getattr(args, "learn_initial_castle", False),
                    frozen_evaluation=bool(autonomous["enabled"]), evaluation_every_steps=eval_every,
                    evaluation_episodes=eval_episodes)
    if not resume: (run_dir / "run.json").write_text(json.dumps(metadata, indent=2), encoding="utf-8")
    else: (run_dir / "resume.json").write_text(json.dumps(metadata, indent=2), encoding="utf-8")

    log(f"run={run_id} behavior={BEHAVIOR} contract=v{current['version']} {current['hash']}")
    log(f"mode={mode} stage={stage} player={player}\ntrainer={trainer_path}\nresults={run_dir}")
    process_record = os.environ.get("MOYVA_CLI_PROCESS_RECORD")
    if process_record and Path(process_record).is_file(): atomic_json(run_dir / "cli-status.json", json.loads(Path(process_record).read_text()))
    atomic_json(run_dir / "effective-config.json", dict(metadata, max_steps=overall_max_steps,
        episode_decisions=args.episode_decisions, time_scale=speed,
        checkpoint_interval=behavior.get("checkpoint_interval"), summary_freq=behavior.get("summary_freq"),
        screen_width=getattr(args, "screen_width", None), screen_height=getattr(args, "screen_height", None)))
    with (run_dir / "cli.log").open("a", encoding="utf-8") as cli_log:
        cli_log.write(json.dumps(dict(time=utc(),component="training",event="start",resume=resume)) + "\n")

    curriculum_state = run_dir / "curriculum-state.json"
    telemetry_dir = run_dir / "telemetry"
    detailed_telemetry = telemetry_dir / "decisions.jsonl"
    compact_telemetry = telemetry_dir / "compact.log"
    old_state_env = os.environ.get("MOYVA_CURRICULUM_STATE_PATH")
    old_autonomous_env = os.environ.get("MOYVA_AUTONOMOUS_TRAINING")
    old_journal_env = os.environ.get("MOYVA_DECISION_JOURNAL_PATH")
    os.environ["MOYVA_CURRICULUM_STATE_PATH"] = str(curriculum_state.resolve())
    os.environ["MOYVA_DECISION_JOURNAL_PATH"] = str(detailed_telemetry.resolve())
    if autonomous["enabled"]:
        os.environ["MOYVA_AUTONOMOUS_TRAINING"] = "1"
    else:
        os.environ.pop("MOYVA_AUTONOMOUS_TRAINING", None)
    telemetry = TrainingTelemetry(run_dir, run_id, BEHAVIOR, overall_max_steps,
                                  detailed_telemetry, compact_telemetry)
    code = 0
    try:
        telemetry.start(resume=resume)
        current_step = trainer_step(run_dir) if resume else 0
        # If a previous launch stopped during evaluation, retry that exact frozen checkpoint before any more PPO updates.
        latest_eval = EvaluationStore(run_dir).latest() if autonomous["enabled"] else {}
        if resume and latest_eval.get("state") in ("INTERRUPTED", "EVALUATING"):
            identity_path = run_dir / "evaluations" / "frozen" / str(int(latest_eval["checkpoint_step"])) / "checkpoint.json"
            if not identity_path.is_file(): raise LaunchError("Interrupted evaluation lost its frozen checkpoint provenance.")
            identity = json.loads(identity_path.read_text())
            verify_resume_checkpoint(run_dir, identity)
            scenario = latest_eval["scenario_id"]; generation = int(latest_eval["evaluation_generation"])
            log(f"Retrying pending frozen evaluation: step={identity['checkpoint_step']} scenario={scenario} generation={generation}")
            evaluation = run_frozen_evaluation(ROOT, unity, args.target, run_id, run_dir, identity, scenario, generation,
                                               eval_episodes, float(autonomous["masteryThreshold"]), curriculum_state, run_process)
            if evaluation["state"] != "COMPLETED":
                code = 130
            else:
                verify_resume_checkpoint(run_dir, identity)

        # Recovery for runs produced by the old exact-boundary check: if a real checkpoint
        # already crossed an evaluation threshold but no completed frozen evaluation exists,
        # evaluate that exact checkpoint before any further PPO updates.
        latest_eval = EvaluationStore(run_dir).latest() if autonomous["enabled"] else {}
        if code == 0 and autonomous["enabled"] and _evaluation_is_overdue(current_step, latest_eval, eval_every):
            newest = newest_training_checkpoint(run_dir)
            actual_checkpoint = newest[0] if newest else None
            try:
                current_step = _validated_segment_checkpoint_step(
                    actual_checkpoint, (current_step // eval_every) * eval_every,
                    ((current_step // eval_every) + 1) * eval_every)
            except ValueError as error:
                raise ControlError(str(error)) from error
            trigger_boundary = (current_step // eval_every) * eval_every
            log(f"Recovering overdue frozen evaluation: boundary={trigger_boundary} checkpoint={current_step}")
            telemetry.note("EVAL", f"recovery boundary={trigger_boundary} checkpoint={current_step}")
            identity = snapshot_frozen_checkpoint(run_dir, current_step, current["hash"])
            update_curriculum_latest_checkpoint(curriculum_state, identity, current_step)
            scenario = choose_evaluation_scenario(curriculum_state)
            store = EvaluationStore(run_dir)
            generation = store.next_generation(identity["checkpoint_id"], scenario)
            evaluation = run_frozen_evaluation(ROOT, unity, args.target, run_id, run_dir, identity, scenario, generation,
                                               eval_episodes, float(autonomous["masteryThreshold"]), curriculum_state, run_process)
            if evaluation["state"] != "COMPLETED":
                code = 130
            else:
                verify_resume_checkpoint(run_dir, identity)
                log(f"Evaluation {evaluation['result']}: {evaluation['successes']}/{evaluation['episode_count']} ({evaluation['success_rate']:.3f})")
                telemetry.note("EVAL", f"{evaluation['result']} successes={evaluation['successes']}/{evaluation['episode_count']} rate={evaluation['success_rate']:.3f}")

        while code == 0 and current_step < overall_max_steps:
            next_due = ((current_step // eval_every) + 1) * eval_every if autonomous["enabled"] else overall_max_steps
            segment_target = min(overall_max_steps, next_due)
            evaluation_due_after_segment = bool(autonomous["enabled"] and segment_target == next_due)
            trainer = json.loads(json.dumps(authored_effective))
            trainer["behaviors"][BEHAVIOR]["max_steps"] = segment_target
            _write_trainer(effective_trainer, trainer)
            segment_resume = resume or current_step > 0
            if segment_resume: _archive_unity_log(run_dir)
            command = _trainer_command(args, effective_trainer, run_id, player, results, arenas, seed, speed, mode,
                                       size, stage, initialize, segment_resume, run_dir)
            with (run_dir / "cli.log").open("a", encoding="utf-8") as cli_log:
                cli_log.write(json.dumps(dict(time=utc(),component="training",event="segment",from_step=current_step,
                                              target_step=segment_target,resume=segment_resume,command=command)) + "\n")
            log(f"Training segment {current_step} -> {segment_target}")
            telemetry.note("SEGMENT", f"{current_step} -> {segment_target}")
            log("Live ML-Agents log: " + str(run_dir / "mlagents.log"))
            code = run_process(command, logfile=run_dir / "mlagents.log")
            segment_behavior = trainer["behaviors"][BEHAVIOR]
            early_failure = early_stop_failure(run_dir, segment_behavior)
            if code == 0 and early_failure:
                code = 3; atomic_json(run_dir / "failure.json", early_failure)
            if code: break
            newest = newest_training_checkpoint(run_dir)
            actual_checkpoint = newest[0] if newest else None
            next_boundary = ((segment_target // eval_every) + 1) * eval_every if autonomous["enabled"] else None
            try:
                current_step = _validated_segment_checkpoint_step(actual_checkpoint, segment_target, next_boundary)
            except ValueError as error:
                code = 3
                atomic_json(run_dir / "failure.json", {
                    "id":"training-segment-boundary-mismatch",
                    "component":"ML-Agents trainer",
                    "repair":str(error),
                    "step":actual_checkpoint,
                    "expected_steps":segment_target,
                    "next_evaluation_boundary":next_boundary})
                break
            if current_step != segment_target:
                log(f"ML-Agents serialized checkpoint {current_step} after requested boundary {segment_target}; preserving real checkpoint identity.")
                telemetry.note("SEGMENT", f"boundary={segment_target} serialized_checkpoint={current_step}")

            if evaluation_due_after_segment:
                identity = snapshot_frozen_checkpoint(run_dir, current_step, current["hash"])
                update_curriculum_latest_checkpoint(curriculum_state, identity, current_step)
                scenario = choose_evaluation_scenario(curriculum_state)
                store = EvaluationStore(run_dir)
                generation = store.next_generation(identity["checkpoint_id"], scenario)
                log(f"Frozen evaluation: checkpoint={identity['checkpoint_id']} scenario={scenario} episodes={eval_episodes}")
                telemetry.note("EVAL", f"checkpoint={identity['checkpoint_id']} scenario={scenario} episodes={eval_episodes}")
                evaluation = run_frozen_evaluation(ROOT, unity, args.target, run_id, run_dir, identity, scenario, generation,
                                                   eval_episodes, float(autonomous["masteryThreshold"]), curriculum_state, run_process)
                if evaluation["state"] != "COMPLETED":
                    code = 130
                    break
                # Evaluation had no trainer and must not have touched either frozen or resumable weights.
                verify_resume_checkpoint(run_dir, identity)
                log(f"Evaluation {evaluation['result']}: {evaluation['successes']}/{evaluation['episode_count']} ({evaluation['success_rate']:.3f})")
                telemetry.note("EVAL", f"{evaluation['result']} successes={evaluation['successes']}/{evaluation['episode_count']} rate={evaluation['success_rate']:.3f}")

        # Restore the authored overall max_steps in the persisted effective config after segmented execution.
        _write_trainer(effective_trainer, authored_effective)
        early_failure = None if code else early_stop_failure(run_dir, authored_effective["behaviors"][BEHAVIOR])
        if code == 0 and early_failure:
            code = 3; atomic_json(run_dir / "failure.json", early_failure)
        atomic_json(run_dir / "cli-status.json", dict(state="COMPLETED" if code == 0 else "INTERRUPTED" if code == 130 else "FAILED",
            exit_code=code,finished=utc(),run_id=run_id,result=early_failure))
        if code:
            unity_log = run_dir / "unity.log"
            if unity_log.exists():
                errors = [line for line in unity_log.read_text(errors="replace").splitlines() if "BLOCKED" in line or "Exception" in line or "failed" in line.lower()]
                for line in errors[-6:]: log(line)
        return code
    except ControlError as error:
        _write_trainer(effective_trainer, authored_effective)
        failure = {"id":"frozen-evaluation-failed","component":"Frozen evaluation",
                   "repair":str(error),"step":trainer_step(run_dir)}
        atomic_json(run_dir / "failure.json", failure)
        atomic_json(run_dir / "cli-status.json", dict(state="FAILED", exit_code=3, finished=utc(), run_id=run_id, result=failure))
        log("Frozen evaluation failed: " + str(error))
        return 3
    finally:
        telemetry.stop()
        if old_state_env is None: os.environ.pop("MOYVA_CURRICULUM_STATE_PATH", None)
        else: os.environ["MOYVA_CURRICULUM_STATE_PATH"] = old_state_env
        if old_autonomous_env is None: os.environ.pop("MOYVA_AUTONOMOUS_TRAINING", None)
        else: os.environ["MOYVA_AUTONOMOUS_TRAINING"] = old_autonomous_env
        if old_journal_env is None: os.environ.pop("MOYVA_DECISION_JOURNAL_PATH", None)
        else: os.environ["MOYVA_DECISION_JOURNAL_PATH"] = old_journal_env



def watch_model(args):
    # Launch a frozen checkpoint in the visual Model Inspector. Never starts PPO.
    from moyva_cli.config import simple_name
    from moyva_cli.evaluation import (
        build_frozen_player,
        checkpoint_pair,
        newest_training_checkpoint,
        patched_environment,
        snapshot_frozen_checkpoint,
    )

    unity, config = prerequisite(args, trainer=False)
    simple_name(args.run_id)
    results = resolve(args.results)
    run_dir = results / args.run_id
    metadata_path = run_dir / "run.json"
    if not metadata_path.is_file():
        raise LaunchError("Model Inspector requires an existing run.json: " + str(run_dir))
    metadata = json.loads(metadata_path.read_text(encoding="utf-8"))
    current = contract()
    trained_contract = ((metadata.get("contract") or {}).get("hash"))
    if trained_contract != current["hash"] and not args.force_contract_mismatch:
        raise LaunchError(
            "MODEL CONTRACT MISMATCH\n"
            + "checkpoint: " + str(trained_contract) + "\n"
            + "runtime: " + current["hash"] + "\n"
            + "Use --force-contract-mismatch only if you intentionally accept incompatible observations/actions."
        )

    if args.checkpoint == "latest":
        newest = newest_training_checkpoint(run_dir)
        if newest is None:
            raise LaunchError("No MoyvaStrategy checkpoint exists in " + str(run_dir))
        step = int(newest[0])
    else:
        try:
            step = int(args.checkpoint)
        except ValueError as error:
            raise LaunchError("--checkpoint must be an integer step or 'latest'.") from error
        if step < 0:
            raise LaunchError("--checkpoint must be non-negative.")

    checkpoint_pair(run_dir, step)
    identity = snapshot_frozen_checkpoint(run_dir, step, trained_contract or current["hash"])

    scenario = args.scenario
    seed = args.seed if args.seed is not None else int(metadata.get("seed", config.get("baseSeed", 1918)))
    world_size = args.world_size if args.world_size is not None else int(metadata.get("world_size", config.get("worldSize", 24)))
    stage = args.stage if args.stage is not None else int(metadata.get("stage", config["curriculum"]["stage"]))
    speed = args.time_scale if args.time_scale is not None else 1.0
    if not 0.1 <= speed <= 20:
        raise LaunchError("--time-scale must be between 0.1 and 20.")
    if not 12 <= world_size <= 128 or not 0 <= stage <= 8:
        raise LaunchError("Allowed ranges: world-size 12-128; stage 0-8.")

    suffix = {"windows": ".exe", "linux": ".x86_64", "macos": ".app"}[args.target]
    player = run_dir / "inspector" / "players" / str(step) / ("MoyvaInspector" + suffix)
    build_log = player.parent / "build.log"

    if not args.no_build or not player.exists():
        log("Building frozen Model Inspector player for checkpoint " + str(step))
        try:
            build_frozen_player(ROOT, unity, args.target, identity, player, run_process)
        except Exception:
            if build_log.is_file():
                lines = build_log.read_text(encoding="utf-8", errors="replace").splitlines()
                compile_errors = [
                    line for line in lines
                    if "error CS" in line or ": error " in line or "Scripts have compiler errors" in line
                ]
                if compile_errors:
                    log("Unity compiler errors:")
                    for line in compile_errors[-20:]:
                        log(line)
            raise

    if not player.exists():
        raise LaunchError("Model Inspector player was not produced: " + str(player))

    stamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%d-%H%M%S")
    session = run_dir / "inspector" / "sessions" / (str(step) + "-" + scenario + "-" + stamp)
    session.mkdir(parents=True, exist_ok=False)
    journal = session / "decisions.jsonl"
    inspector_state = session / "curriculum-state.json"
    unity_log = session / "unity.log"

    env = {
        "MOYVA_MODEL_INSPECTOR": "1",
        "MOYVA_INSPECT_RUN_ID": args.run_id,
        "MOYVA_INSPECT_SCENARIO": scenario,
        "MOYVA_INSPECT_CHECKPOINT": identity["checkpoint_id"],
        "MOYVA_INSPECT_CHECKPOINT_STEP": str(step),
        "MOYVA_INSPECT_CONTRACT_HASH": identity["contract_hash"],
        "MOYVA_INSPECT_MODEL_SHA256": identity["frozen_onnx_sha256"],
        "MOYVA_INSPECT_SEED": str(seed),
        "MOYVA_INSPECT_SESSION_PATH": str(session.resolve()),
        "MOYVA_INSPECT_JOURNAL_PATH": str(journal.resolve()),
        "MOYVA_INSPECT_STATE_PATH": str(inspector_state.resolve()),
        "MOYVA_INSPECT_AUTOPLAY": "1" if args.command == "watch" else "0",
    }

    command = [
        str(player),
        "-moyvaTrainingMode", "Visual",
        "-moyvaSeed", str(seed),
        "-moyvaWorldSize", str(world_size),
        "-moyvaCurriculumStage", str(stage),
        "-moyvaTrainingTimeScale", str(speed),
        "-screen-fullscreen", "0",
        "-screen-width", str(args.screen_width),
        "-screen-height", str(args.screen_height),
        "-logFile", str(unity_log),
    ]

    log("MODEL INSPECTOR (frozen inference; weights cannot update)")
    log("run=" + args.run_id + " checkpoint=" + str(step) + " scenario=" + scenario + " seed=" + str(seed))
    log("player=" + str(player))
    log("session=" + str(session))
    log("Controls: Space play/pause | Right step/next | Left previous | Home first | End live | R restart seed")
    with patched_environment(env):
        return run_process(command)



def trainer_step(run_dir):
    log_path = Path(run_dir) / "mlagents.log"
    steps = [int(value) for value in re.findall(r"MoyvaStrategy\. Step:\s*(\d+)", log_path.read_text(errors="replace") if log_path.is_file() else "")]
    checkpoints = []
    for path in (Path(run_dir) / "MoyvaStrategy").glob("MoyvaStrategy-*.pt"):
        found = re.search(r"-(\d+)$", path.stem)
        if found: checkpoints.append(int(found.group(1)))
    return max(steps + checkpoints + [0])


def early_stop_failure(run_dir, behavior):
    run_dir = Path(run_dir)
    log_text = (run_dir / "mlagents.log").read_text(errors="replace") if (run_dir / "mlagents.log").is_file() else ""
    unity_text = (run_dir / "unity.log").read_text(errors="replace") if (run_dir / "unity.log").is_file() else ""
    actual = trainer_step(run_dir); expected = int(behavior.get("max_steps", 0) or 0); summary = int(behavior.get("summary_freq", 0) or 0)
    if "Learning was interrupted" not in log_text and expected and actual >= min(expected, max(1, summary)): return None
    if expected and actual >= expected: return None
    if "READY_FOR_REAL_TRAINING" not in unity_text:
        evidence = [line for line in unity_text.splitlines() if "Exception" in line or "BLOCKED" in line or "ERROR" in line][-6:]
        return {"id": "unity-worker-exited", "component": "Unity worker", "evidence": evidence or unity_text.splitlines()[-6:],
                "repair": "Open unity.log; the worker exited before reporting READY_FOR_REAL_TRAINING.", "step": actual, "expected_steps": expected}
    if actual < max(1, summary):
        evidence = [line for line in log_text.splitlines() if "Learning was interrupted" in line or "SubprocessEnvManager" in line or "ERROR" in line][-6:]
        return {"id": "training-ended-before-first-metrics", "component": "ML-Agents trainer", "evidence": evidence or log_text.splitlines()[-6:],
                "repair": "The Unity worker connected but training stopped before the first metric report. Check stop requests, worker crash, and the preserved unity.log/mlagents.log.",
                "step": actual, "expected_steps": expected, "summary_freq": summary}
    return None


def parser():
    result = argparse.ArgumentParser(description="Train one MoyvaStrategy policy against real Moyva gameplay.")
    commands = result.add_subparsers(dest="command", required=True)
    for name in ("doctor", "build", "train", "resume", "visual", "watch", "inspect", "tensorboard", "info"):
        p = commands.add_parser(name); p.add_argument("--unity"); p.add_argument("--env")
        p.add_argument("--target", choices=("linux", "windows", "macos"), default="windows" if os.name == "nt" else "macos" if sys.platform == "darwin" else "linux")
        p.add_argument("--trainer", "--config", default=str(TRAINING / "Config/moyva_ppo.yaml")); p.add_argument("--results", default=str(RESULTS))
        if name in ("watch", "inspect"):
            p.add_argument("--run-id", required=True)
            p.add_argument("--checkpoint", default="latest")
            p.add_argument("--scenario", default="castle")
            p.add_argument("--time-scale", "--speed", dest="time_scale", type=float, default=1.0)
            p.add_argument("--seed", type=int)
            p.add_argument("--episode", type=int)
            p.add_argument("--world-size", type=int)
            p.add_argument("--stage", type=int)
            p.add_argument("--screen-width", type=int, default=1440)
            p.add_argument("--screen-height", type=int, default=900)
            p.add_argument("--no-build", action="store_true")
            p.add_argument("--force-contract-mismatch", action="store_true")
        if name in ("train", "resume", "visual"):
            p.add_argument("--run-id", required=name == "resume"); modes = p.add_mutually_exclusive_group()
            modes.add_argument("--visual", action="store_true"); modes.add_argument("--headless", action="store_true")
            p.add_argument("--time-scale", type=float); p.add_argument("--seed", type=int); p.add_argument("--world-size", type=int); p.add_argument("--stage", type=int)
            p.add_argument("--arenas", type=int, default=1); p.add_argument("--initialize-from"); p.add_argument("--learn-initial-castle", action="store_true")
            p.add_argument("--max-steps", type=int); p.add_argument("--checkpoint-interval", type=int); p.add_argument("--summary-freq", type=int)
            p.add_argument("--screen-width", type=int, default=1280); p.add_argument("--screen-height", type=int, default=720); p.add_argument("--base-port", type=int, default=5005)
            p.add_argument("--episode-decisions", type=int); p.add_argument("--force", action="store_true"); p.add_argument("--resume", action="store_true")
            p.add_argument("--force-contract-mismatch", action="store_true"); p.add_argument("--no-build", action="store_true"); p.add_argument("--verbose", action="store_true")
    return result


def main(argv=None):
    args = parser().parse_args(argv)
    try:
        if args.command == "info":
            config = json.loads(CONFIG.read_text(encoding="utf-8-sig"))
            print(json.dumps(dict(contract=contract(), stage=config["curriculum"]["stage"], world_size=config.get("worldSize", 24),
                graph=config.get("generatorGraphId", "generatorgraph"), mode="HeadlessFast", executable=str(player_path(args)), trainer=args.trainer, results=args.results), indent=2))
        elif args.command == "doctor":
            unity, _ = prerequisite(args)
            print("MOYVA TRAINING DOCTOR\nPython: OK\nML-Agents: OK\nUnity: OK " + unity + "\nTraining config: OK\nTrainer config: OK")
            print("Training player: " + ("OK" if player_path(args).is_file() else "MISSING (build required)")); print("Contract: v%s %s" % (contract()["version"], contract()["hash"]))
        elif args.command == "build": build(args)
        elif args.command in ("watch", "inspect"): return watch_model(args)
        elif args.command == "tensorboard":
            try: importlib.metadata.version("tensorboard")
            except importlib.metadata.PackageNotFoundError: raise LaunchError("TensorBoard missing. Install with: python -m pip install tensorboard")
            return run_process([sys.executable, "-m", "tensorboard.main", "--logdir", str(resolve(args.results))])
        else: return train(args)
        return 0
    except (LaunchError, OSError, ValueError, KeyError) as error:
        print("[MoyvaTrain] " + str(error), file=sys.stderr); return error.code if isinstance(error, LaunchError) else 2


if __name__ == "__main__":
    sys.exit(main())
