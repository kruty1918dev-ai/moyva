from __future__ import annotations

import json
import re
import threading
from pathlib import Path


_STEP_RE = re.compile(r"MoyvaStrategy\. Step:\s*(\d+)")
_MEAN_RE = re.compile(r"Mean Reward:\s*([-+]?\d+(?:\.\d+)?)")


def _short(value, limit=46):
    text = "-" if value is None or value == "" else str(value)
    return text if len(text) <= limit else text[: limit - 1] + "…"


class TrainingTelemetry:
    """Tails the run-local decision journal and ML-Agents log.

    Unity's JSONL journal is the authoritative full trace. This class only
    derives a human-readable compact stream and never feeds anything back
    into training.
    """

    def __init__(self, run_dir, run_id, behavior, max_steps, detailed_path, compact_path):
        self.run_dir = Path(run_dir)
        self.run_id = str(run_id)
        self.behavior = str(behavior)
        self.max_steps = max(1, int(max_steps))
        self.detailed_path = Path(detailed_path)
        self.compact_path = Path(compact_path)
        self.mlagents_path = self.run_dir / "mlagents.log"

        self._stop = threading.Event()
        self._thread = None
        self._ml_offset = 0
        self._decision_offset = 0
        self._ml_pending = ""
        self._decision_pending = ""
        self._step = 0
        self._last_progress_step = -1
        self._decision_seen = 0
        self._episode_totals = {}
        self._last_lesson = None

    def start(self, resume=False):
        telemetry_dir = self.detailed_path.parent
        telemetry_dir.mkdir(parents=True, exist_ok=True)

        if not resume:
            self.detailed_path.unlink(missing_ok=True)
            self.compact_path.write_text("", encoding="utf-8")
        else:
            self.compact_path.touch(exist_ok=True)

        self._ml_offset = self.mlagents_path.stat().st_size if self.mlagents_path.exists() else 0
        self._decision_offset = self.detailed_path.stat().st_size if self.detailed_path.exists() else 0

        manifest = {
            "version": 1,
            "run_id": self.run_id,
            "model": self.behavior,
            "max_steps": self.max_steps,
            "detailed": str(self.detailed_path),
            "compact": str(self.compact_path),
            "console_policy": {
                "progress": "every ML-Agents summary; default launcher cap is 500 steps",
                "decisions": "all reward/penalty/rejection/lesson changes plus every 20th neutral decision",
            },
        }
        (telemetry_dir / "manifest.json").write_text(
            json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8"
        )

        self.note(
            "TELEMETRY",
            f"run={self.run_id} model={self.behavior} "
            f"detailed={self.detailed_path} compact={self.compact_path}",
        )

        self._thread = threading.Thread(
            target=self._loop, name="MoyvaTrainingTelemetry", daemon=True
        )
        self._thread.start()

    def stop(self):
        if self._thread is None:
            return
        self._stop.set()
        self._thread.join(timeout=3.0)
        self._thread = None
        self._poll()
        self.note("TELEMETRY", f"stopped run={self.run_id}")

    def note(self, kind, message):
        self._emit(f"[{kind}] {message}")

    def _emit(self, line):
        print(line, flush=True)
        try:
            self.compact_path.parent.mkdir(parents=True, exist_ok=True)
            with self.compact_path.open("a", encoding="utf-8") as stream:
                stream.write(line + "\n")
        except OSError:
            pass

    def _loop(self):
        while not self._stop.wait(0.25):
            self._poll()

    def _poll(self):
        self._poll_mlagents()
        self._poll_decisions()

    def _read_new_lines(self, path, offset_name, pending_name):
        path = Path(path)
        if not path.exists():
            return []

        try:
            size = path.stat().st_size
            offset = getattr(self, offset_name)
            if size < offset:
                offset = 0
                setattr(self, pending_name, "")

            with path.open("rb") as stream:
                stream.seek(offset)
                data = stream.read()
                setattr(self, offset_name, stream.tell())
        except OSError:
            return []

        if not data:
            return []

        pending = getattr(self, pending_name)
        text = pending + data.decode("utf-8", errors="replace")

        complete = text.endswith("\n") or text.endswith("\r")
        lines = text.splitlines()
        if not complete and lines:
            setattr(self, pending_name, lines.pop())
        elif not complete:
            setattr(self, pending_name, text)
            lines = []
        else:
            setattr(self, pending_name, "")

        return lines

    def _poll_mlagents(self):
        for line in self._read_new_lines(
            self.mlagents_path, "_ml_offset", "_ml_pending"
        ):
            match = _STEP_RE.search(line)
            if not match:
                continue

            step = int(match.group(1))
            self._step = max(self._step, step)
            if step == self._last_progress_step:
                continue

            self._last_progress_step = step
            pct = min(100.0, (step / self.max_steps) * 100.0)
            mean_match = _MEAN_RE.search(line)
            mean = f" meanReward={float(mean_match.group(1)):+.3f}" if mean_match else ""
            self._emit(
                f"[PROGRESS] {step}/{self.max_steps} ({pct:5.1f}%)"
                f"{mean} run={self.run_id}"
            )

    def _poll_decisions(self):
        for line in self._read_new_lines(
            self.detailed_path, "_decision_offset", "_decision_pending"
        ):
            try:
                event = json.loads(line)
            except (TypeError, ValueError):
                continue
            self._handle_decision(event)

    def _handle_decision(self, event):
        self._decision_seen += 1

        arena = int(event.get("arenaId", 0) or 0)
        episode = int(event.get("episodeId", 0) or 0)
        sequence = int(event.get("sequence", 0) or 0)
        lesson = f"{event.get('scenarioId') or '-'}#{event.get('scenarioStep', -1)}"
        result = str(event.get("result") or "-")
        action = _short(event.get("actionId") or "<masked-slot>")
        target = _short(event.get("targetId"))
        options = event.get("availableActions") or []
        reward = float(event.get("rewardDelta", 0.0) or 0.0)
        reason = str(event.get("rejectionReason") or "").strip()

        key = (arena, episode)
        episode_total = self._episode_totals.get(key, 0.0) + reward
        self._episode_totals[key] = episode_total

        if reward > 1e-7:
            feedback = f"PRAISE +{reward:.3f}"
        elif reward < -1e-7:
            feedback = f"PENALTY {reward:.3f}"
        else:
            feedback = "neutral"

        lesson_changed = lesson != self._last_lesson
        self._last_lesson = lesson

        result_ok = result.lower() in {"completed", "success"}
        interesting = (
            lesson_changed
            or abs(reward) > 1e-7
            or not result_ok
            or self._decision_seen % 20 == 0
        )
        if not interesting:
            return

        preview_values = [_short(v, 24) for v in options[:4] if v]
        preview = ",".join(preview_values)
        if len(options) > 4:
            preview += ",…"
        considered = f"{len(options)}[{preview}]" if preview else str(len(options))

        line = (
            f"[BOT] step~{self._step} ep={episode} seq={sequence} "
            f"lesson={_short(lesson, 34)} considered={considered} "
            f"chose={action} target={target} result={_short(result, 18)} "
            f"| {feedback} | epReward={episode_total:+.3f}"
        )

        if reason and not result_ok:
            line += f" | reason={_short(reason, 80)}"

        self._emit(line)
