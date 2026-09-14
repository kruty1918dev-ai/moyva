from __future__ import annotations

import contextlib
import datetime as dt
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
from typing import Callable, Iterable

from .config import ControlError, atomic_json, read_json

BEHAVIOR = "MoyvaStrategy"
SEED_SET_VERSION = "heldout-v1"
EVALUATION_STATES = {"EVALUATING", "INTERRUPTED", "COMPLETED"}
EVALUATION_RESULTS = {"PASSED", "FAILED", "INTERRUPTED"}


def _utc() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def _signed32(value: int) -> int:
    value &= 0xFFFFFFFF
    return value - 0x100000000 if value & 0x80000000 else value


def _fnv_step(value: int, part: int) -> int:
    return ((value ^ (part & 0xFFFFFFFF)) * 16777619) & 0xFFFFFFFF


def derive_episode_seed(base_seed: int, environment_id: int, episode_id: int) -> int:
    """Bit-for-bit mirror of TrainingResetContext.DeriveSeed."""
    value = 2166136261
    value = _fnv_step(value, base_seed)
    value = _fnv_step(value, environment_id)
    value = _fnv_step(value, episode_id)
    value = _fnv_step(value, episode_id >> 32)
    return _signed32(value)


def evaluation_seed_base(contract_hash: str, checkpoint_step: int, scenario_id: str, generation: int) -> int:
    payload = "\0".join((SEED_SET_VERSION, str(contract_hash), str(int(checkpoint_step)), str(scenario_id), str(int(generation))))
    digest = hashlib.sha256(payload.encode("utf-8")).digest()
    return _signed32(int.from_bytes(digest[:4], "little", signed=False))


def held_out_seeds(contract_hash: str, checkpoint_step: int, scenario_id: str, generation: int, count: int = 50) -> list[int]:
    if count < 1:
        raise ValueError("Evaluation episode count must be positive.")
    base = evaluation_seed_base(contract_hash, checkpoint_step, scenario_id, generation)
    return [derive_episode_seed(base, 0, episode) for episode in range(1, count + 1)]


def training_seeds(base_seed: int, count: int = 50) -> list[int]:
    if count < 1:
        return []
    return [derive_episode_seed(base_seed, 0, episode) for episode in range(1, count + 1)]


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with Path(path).open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def checkpoint_identity(checkpoint: Path, checkpoint_step: int, contract_hash: str) -> dict:
    checkpoint = Path(checkpoint).resolve()
    if not checkpoint.is_file():
        raise ControlError("Checkpoint does not exist: " + str(checkpoint))
    checksum = file_sha256(checkpoint)
    return {
        "source_checkpoint": str(checkpoint),
        "checkpoint_step": int(checkpoint_step),
        "contract_hash": str(contract_hash),
        "checkpoint_sha256": checksum,
        "checkpoint_id": f"{contract_hash}:{int(checkpoint_step)}:{checksum}",
    }


def _checkpoint_step(path: Path) -> int | None:
    found = re.search(r"-(\d+)$", path.stem)
    return int(found.group(1)) if found else None


def checkpoint_pair(run_dir: Path, step: int) -> tuple[Path, Path]:
    run_dir = Path(run_dir)
    behavior_dir = run_dir / BEHAVIOR
    exact_pt = behavior_dir / f"{BEHAVIOR}-{int(step)}.pt"
    exact_onnx = behavior_dir / f"{BEHAVIOR}-{int(step)}.onnx"
    if not exact_pt.is_file():
        choices = [p for p in behavior_dir.glob(f"{BEHAVIOR}-*.pt") if _checkpoint_step(p) == int(step)]
        exact_pt = choices[0] if choices else exact_pt
    if not exact_onnx.is_file():
        choices = [p for p in behavior_dir.glob(f"{BEHAVIOR}-*.onnx") if _checkpoint_step(p) == int(step)]
        if choices:
            exact_onnx = choices[0]
        else:
            # ML-Agents also keeps the last exported model at run root. It is safe to
            # snapshot only immediately after the trainer stopped at the exact step.
            root_onnx = run_dir / f"{BEHAVIOR}.onnx"
            if root_onnx.is_file():
                exact_onnx = root_onnx
    if not exact_pt.is_file() or not exact_onnx.is_file():
        raise ControlError(f"Exact checkpoint/export for training step {step} is incomplete.")
    return exact_pt.resolve(), exact_onnx.resolve()


def snapshot_frozen_checkpoint(run_dir: Path, step: int, contract_hash: str) -> dict:
    pt, onnx = checkpoint_pair(run_dir, step)
    root = Path(run_dir) / "evaluations" / "frozen" / str(int(step))
    root.mkdir(parents=True, exist_ok=True)
    frozen_pt = root / f"{BEHAVIOR}.pt"
    frozen_onnx = root / f"{BEHAVIOR}.onnx"
    for source, frozen, label in ((pt, frozen_pt, "checkpoint"), (onnx, frozen_onnx, "export")):
        if frozen.exists():
            if file_sha256(frozen) != file_sha256(source):
                raise ControlError(f"Frozen evaluation {label} already exists with different weights.")
        else:
            shutil.copy2(source, frozen)
    identity = checkpoint_identity(pt, step, contract_hash)
    identity.update(
        frozen_checkpoint=str(frozen_pt.resolve()),
        frozen_checkpoint_sha256=file_sha256(frozen_pt),
        frozen_onnx=str(frozen_onnx.resolve()),
        frozen_onnx_sha256=file_sha256(frozen_onnx),
    )
    atomic_json(root / "checkpoint.json", identity)
    return identity


def assert_checkpoint_unchanged(identity: dict) -> None:
    source = Path(identity["source_checkpoint"])
    if not source.is_file() or file_sha256(source) != identity["checkpoint_sha256"]:
        raise ControlError("Evaluation changed or replaced the source training checkpoint.")
    frozen_checkpoint = Path(identity["frozen_checkpoint"])
    if not frozen_checkpoint.is_file() or file_sha256(frozen_checkpoint) != identity["frozen_checkpoint_sha256"]:
        raise ControlError("Frozen evaluation checkpoint changed during evaluation.")
    frozen = Path(identity["frozen_onnx"])
    if not frozen.is_file() or file_sha256(frozen) != identity["frozen_onnx_sha256"]:
        raise ControlError("Frozen evaluation ONNX changed during evaluation.")


def newest_training_checkpoint(run_dir: Path) -> tuple[int, Path] | None:
    values = []
    for path in (Path(run_dir) / BEHAVIOR).glob(f"{BEHAVIOR}-*.pt"):
        step = _checkpoint_step(path)
        if step is not None:
            values.append((step, path.resolve()))
    return max(values, key=lambda item: item[0]) if values else None


def verify_resume_checkpoint(run_dir: Path, identity: dict) -> Path:
    assert_checkpoint_unchanged(identity)
    newest = newest_training_checkpoint(run_dir)
    expected = Path(identity["source_checkpoint"]).resolve()
    if newest is None or newest[0] != int(identity["checkpoint_step"]) or newest[1] != expected:
        raise ControlError("Training resume would not use the checkpoint that was evaluated.")
    return expected


def _history(path: Path) -> Iterable[dict]:
    if not path.is_file():
        return []
    rows = []
    for line in path.read_text(encoding="utf-8", errors="replace").splitlines():
        if not line.strip():
            continue
        try:
            rows.append(json.loads(line))
        except json.JSONDecodeError:
            continue
    return rows


class EvaluationStore:
    def __init__(self, run_dir: Path):
        self.run_dir = Path(run_dir)
        self.root = self.run_dir / "evaluations"
        self.history_path = self.root / "evaluation-history.jsonl"
        self.latest_path = self.root / "latest.json"
        self.root.mkdir(parents=True, exist_ok=True)

    def latest(self) -> dict:
        return read_json(self.latest_path, {})

    def next_generation(self, checkpoint_id: str, scenario_id: str) -> int:
        latest = self.latest()
        if (latest.get("state") == "INTERRUPTED"
                and latest.get("checkpoint_id") == checkpoint_id
                and latest.get("scenario_id") == scenario_id):
            return int(latest.get("evaluation_generation", 1))
        generations = [int(row.get("evaluation_generation", 0)) for row in _history(self.history_path)
                       if row.get("scenario_id") == scenario_id]
        return max(generations + [0]) + 1

    def begin(self, run_id: str, identity: dict, scenario_id: str, generation: int, episode_count: int, seed_base: int) -> dict:
        now = _utc()
        record = {
            "state": "EVALUATING",
            "result": None,
            "run_id": run_id,
            **identity,
            "scenario_id": scenario_id,
            "seed_set_version": SEED_SET_VERSION,
            "seed_base": int(seed_base),
            "seeds": held_out_seeds(identity["contract_hash"], identity["checkpoint_step"], scenario_id, generation, episode_count),
            "evaluation_generation": int(generation),
            "episode_count": int(episode_count),
            "completed_episodes": 0,
            "successes": 0,
            "failures": 0,
            "success_rate": None,
            "started_utc": now,
            "ended_utc": None,
            "mastery_changed": False,
        }
        atomic_json(self.latest_path, record)
        return record

    def publish_interrupted(self, base: dict, completed: int, successes: int, reason: str | None = None) -> dict:
        record = dict(base)
        record.update(
            state="INTERRUPTED",
            result="INTERRUPTED",
            completed_episodes=max(0, int(completed)),
            successes=max(0, int(successes)),
            failures=max(0, int(completed) - int(successes)),
            success_rate=None,
            ended_utc=_utc(),
            mastery_changed=False,
        )
        if reason:
            record["reason"] = reason
        self._publish(record)
        return record

    def publish_completed(self, base: dict, unity_result: dict, mastery_threshold: float) -> dict:
        expected = int(base["episode_count"])
        completed = int(unity_result.get("completed_episodes", -1))
        successes = int(unity_result.get("successes", -1))
        if completed != expected or not 0 <= successes <= completed:
            raise ControlError("Partial evaluation cannot be published as completed.")
        rate = successes / float(completed)
        record = dict(base)
        record.update(
            state="COMPLETED",
            result="PASSED" if rate >= float(mastery_threshold) else "FAILED",
            completed_episodes=completed,
            successes=successes,
            failures=completed - successes,
            success_rate=rate,
            ended_utc=unity_result.get("ended_utc") or _utc(),
            mastery_changed=bool(unity_result.get("mastery_changed", False)),
        )
        best = self._best_verified_candidate(record)
        record["best_verified_checkpoint"] = best
        self._publish(record)
        return record

    def _best_verified_candidate(self, candidate: dict) -> dict | None:
        completed = [row for row in _history(self.history_path) if row.get("state") == "COMPLETED"]
        completed.append(candidate)
        if not completed:
            return None
        best = max(completed, key=lambda row: (float(row.get("success_rate", -1)), int(row.get("checkpoint_step", -1))))
        return {
            "checkpoint_id": best.get("checkpoint_id"),
            "source_checkpoint": best.get("source_checkpoint"),
            "checkpoint_step": best.get("checkpoint_step"),
            "success_rate": best.get("success_rate"),
            "scenario_id": best.get("scenario_id"),
            "contract_hash": best.get("contract_hash"),
        }

    def _publish(self, record: dict) -> None:
        self.root.mkdir(parents=True, exist_ok=True)
        with self.history_path.open("a", encoding="utf-8") as stream:
            stream.write(json.dumps(record, ensure_ascii=False, separators=(",", ":")) + "\n")
        atomic_json(self.latest_path, record)


def unity_progress(path: Path) -> dict:
    return read_json(Path(path), {})


@contextlib.contextmanager
def patched_environment(values: dict[str, str]):
    old = {key: os.environ.get(key) for key in values}
    try:
        os.environ.update({key: str(value) for key, value in values.items()})
        yield
    finally:
        for key, value in old.items():
            if value is None:
                os.environ.pop(key, None)
            else:
                os.environ[key] = value


def evaluation_environment(run_id: str, identity: dict, scenario_id: str, generation: int,
                           episode_count: int, seed_base: int, state_path: Path, progress_path: Path) -> dict[str, str]:
    # This environment is deliberately inference-only. There is no trainer port,
    # PPO process, or -moyvaRequireTrainer flag in the evaluation launch.
    return {
        "MOYVA_EVALUATION": "1",
        "MOYVA_EVAL_RUN_ID": run_id,
        "MOYVA_EVAL_SCENARIO": scenario_id,
        "MOYVA_EVAL_CHECKPOINT": identity["checkpoint_id"],
        "MOYVA_EVAL_CHECKPOINT_STEP": str(identity["checkpoint_step"]),
        "MOYVA_EVAL_CONTRACT_HASH": identity["contract_hash"],
        "MOYVA_EVAL_GENERATION": str(int(generation)),
        "MOYVA_EVAL_EPISODES": str(int(episode_count)),
        "MOYVA_EVAL_SEED_BASE": str(int(seed_base)),
        "MOYVA_EVAL_SEED_SET_VERSION": SEED_SET_VERSION,
        "MOYVA_EVAL_PROGRESS_PATH": str(Path(progress_path).resolve()),
        "MOYVA_CURRICULUM_STATE_PATH": str(Path(state_path).resolve()),
    }


def evaluation_player_path(run_dir: Path, step: int, target: str) -> Path:
    suffix = {"windows": ".exe", "linux": ".x86_64", "macos": ".app"}[target]
    return Path(run_dir) / "evaluations" / "players" / str(int(step)) / ("MoyvaEvaluation" + suffix)


def build_frozen_player(root: Path, unity: str, target: str, identity: dict,
                        output: Path, run_process: Callable[..., int]) -> Path:
    output = Path(output).resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    log_path = output.parent / "build.log"
    build_target = {"windows": "StandaloneWindows64", "linux": "StandaloneLinux64", "macos": "StandaloneOSX"}[target]
    environment = {
        "MOYVA_EVAL_MODEL_SOURCE": identity["frozen_onnx"],
        "MOYVA_EVAL_BUILD_OUTPUT": str(output),
        "MOYVA_EVAL_BUILD_TARGET": build_target,
    }
    command = [str(unity), "-batchmode", "-nographics", "-quit", "-projectPath", str(Path(root).resolve()),
               "-logFile", str(log_path), "-executeMethod",
               "Kruty1918.Moyva.AI.Training.TrainingEvaluationModelBinder.BuildFrozenEvaluationPlayer"]
    with patched_environment(environment):
        code = run_process(command)
    if code or not output.exists():
        raise ControlError(f"Frozen evaluation player build failed (exit {code}); log: {log_path}")
    return output


def run_frozen_evaluation(root: Path, unity: str, target: str, run_id: str, run_dir: Path,
                          identity: dict, scenario_id: str, generation: int, episode_count: int,
                          mastery_threshold: float, state_path: Path, run_process: Callable[..., int]) -> dict:
    store = EvaluationStore(run_dir)
    seed_base = evaluation_seed_base(identity["contract_hash"], identity["checkpoint_step"], scenario_id, generation)
    base = store.begin(run_id, identity, scenario_id, generation, episode_count, seed_base)
    progress_path = store.root / "unity-progress.json"
    progress_path.unlink(missing_ok=True)
    player = evaluation_player_path(run_dir, identity["checkpoint_step"], target)
    if not player.exists():
        build_frozen_player(root, unity, target, identity, player, run_process)
    env = evaluation_environment(run_id, identity, scenario_id, generation, episode_count, seed_base, state_path, progress_path)
    command = [str(player), "-batchmode", "-nographics",
               "-logFile", str(store.root / f"evaluation-{identity['checkpoint_step']}-{generation}.log")]
    with patched_environment(env):
        code = run_process(command)
    progress = unity_progress(progress_path)
    assert_checkpoint_unchanged(identity)
    completed = int(progress.get("completed_episodes", 0) or 0)
    successes = int(progress.get("successes", 0) or 0)
    provenance_ok = (progress.get("run_id") == run_id
        and int(progress.get("checkpoint_step", -1) or -1) == int(identity["checkpoint_step"])
        and progress.get("contract_hash") == identity["contract_hash"]
        and progress.get("scenario_id") == scenario_id
        and int(progress.get("evaluation_generation", -1) or -1) == int(generation)
        and int(progress.get("episode_count", -1) or -1) == int(episode_count)
        and progress.get("seed_set_version") == SEED_SET_VERSION)
    complete_result = progress.get("state") == "COMPLETED" and completed == episode_count and provenance_ok
    if not complete_result:
        reason = progress.get("reason") or ("evaluation provenance mismatch" if not provenance_ok else f"evaluation player exit {code}")
        return store.publish_interrupted(base, completed, successes, reason)
    # A stop signal can race with Unity's final atomic progress write. Once all
    # held-out episodes and provenance are complete, the evaluation is valid even
    # if the process was reaped with a non-zero/interrupt exit code afterwards.
    return store.publish_completed(base, progress, mastery_threshold)


def update_curriculum_latest_checkpoint(state_path: Path, identity: dict, training_step: int) -> None:
    state_path = Path(state_path)
    state = read_json(state_path, {}) or {}
    # Keep a valid C# JsonUtility state even if the first exact checkpoint is
    # reached before an episode has produced the curriculum file.
    state.setdefault("version", 2)
    state.setdefault("totalDecisions", 0)
    state.setdefault("nextEvaluationStep", int(training_step))
    state.setdefault("activeScenarioId", None)
    state.setdefault("bestVerifiedCheckpoint", None)
    state.setdefault("bestVerifiedCheckpointStep", 0)
    state.setdefault("bestVerifiedRate", -1.0)
    state.setdefault("opponentPool", [])
    state.setdefault("skills", [])
    state["lastCheckpoint"] = identity["checkpoint_id"]
    state["lastCheckpointStep"] = int(training_step)
    # bestVerifiedCheckpoint is intentionally untouched here.
    atomic_json(state_path, state)


def choose_evaluation_scenario(state_path: Path) -> str:
    state = read_json(Path(state_path), {})
    scenario = state.get("activeScenarioId")
    if not scenario:
        skills = state.get("skills") or []
        scenario = next((skill.get("scenarioId") for skill in skills if not skill.get("mastered")), None)
    return scenario or "castle"
