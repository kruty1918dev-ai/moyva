"""Persisted process identity and cooperative stop; never signal a PID on its own."""
from __future__ import annotations

import hashlib
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import time
import uuid

from .config import ControlError, atomic_json, read_json, simple_name, utc


def psutil_module():
    try:
        import psutil
        return psutil
    except ImportError as error:
        raise ControlError("Process supervision needs psutil. Run ./moyva setup.") from error


def identity(pid):
    psutil = psutil_module()
    try:
        process = psutil.Process(pid)
        return {
            "pid": pid,
            "created": process.create_time(),
            "executable": process.exe(),
            "command_hash": hashlib.sha256(json.dumps(process.cmdline()).encode()).hexdigest(),
        }
    except (psutil.NoSuchProcess, psutil.AccessDenied):
        return None


def same_process(record):
    if not record or not record.get("pid"):
        return False
    actual = identity(record["pid"])
    return actual is not None and all(actual.get(key) == record.get(key) for key in ("pid", "created", "executable", "command_hash"))


def _gpu_usage_for_pids(pids):
    """Best-effort NVIDIA metrics for the owned process tree.

    No dependency is required and failures are intentionally silent: non-NVIDIA hosts,
    systems without nvidia-smi, old drivers, and unsupported GPUs simply return None.
    """
    executable = shutil.which("nvidia-smi")
    if not executable or not pids:
        return None
    try:
        overall = subprocess.run(
            [
                executable,
                "--query-gpu=utilization.gpu,memory.used,memory.total",
                "--format=csv,noheader,nounits",
            ],
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
            timeout=2,
        )
        if overall.returncode:
            return None
        lines = [line.strip() for line in overall.stdout.splitlines() if line.strip()]
        if not lines:
            return None
        # On multi-GPU hosts use the busiest GPU as a compact dashboard signal.
        parsed = []
        for line in lines:
            fields = [part.strip() for part in line.split(",")]
            if len(fields) >= 3:
                parsed.append(tuple(float(value) for value in fields[:3]))
        if not parsed:
            return None
        utilization, used, total = max(parsed, key=lambda row: row[0])

        owned_memory = 0.0
        compute = subprocess.run(
            [
                executable,
                "--query-compute-apps=pid,used_memory",
                "--format=csv,noheader,nounits",
            ],
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
            timeout=2,
        )
        if compute.returncode == 0:
            owned = set(int(pid) for pid in pids)
            for line in compute.stdout.splitlines():
                fields = [part.strip() for part in line.split(",")]
                if len(fields) < 2:
                    continue
                try:
                    if int(fields[0]) in owned:
                        owned_memory += float(fields[1])
                except ValueError:
                    continue
        return {
            "utilization_percent": utilization,
            "memory_used_mib": used,
            "memory_total_mib": total,
            "owned_memory_mib": owned_memory,
        }
    except (OSError, subprocess.TimeoutExpired, ValueError):
        return None


class Supervisor:
    def __init__(self, project):
        self.project = project

    def records(self):
        records = []
        for path in (self.project.local / "processes").glob("*.json"):
            try:
                record = read_json(path)
                record["live"] = same_process(record) if record.get("state") == "RUNNING" else False
                if record.get("state") == "RUNNING" and not record["live"]:
                    record["state"] = "INTERRUPTED"
                records.append(record)
            except ControlError:
                continue
        return sorted(records, key=lambda r: r.get("started", ""), reverse=True)

    def launch(self, task, run_id=None):
        lock = self.project.local / "launch.lock"
        lock.parent.mkdir(parents=True, exist_ok=True)
        try:
            handle = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY, 0o600)
        except FileExistsError:
            owner = read_json(lock, {})
            if owner and not same_process(owner):
                lock.unlink(missing_ok=True)
                return self.launch(task, run_id)
            raise ControlError("Another task launch is in progress.")
        try:
            with os.fdopen(handle, "w") as stream:
                json.dump(identity(os.getpid()), stream)
            result = self._launch(task, run_id)
            record = self.project.local / "processes" / (result["token"] + ".json")
            until = time.monotonic() + 5
            while not record.exists() and time.monotonic() < until:
                time.sleep(0.05)
            return result
        finally:
            lock.unlink(missing_ok=True)

    def _launch(self, task, run_id=None):
        from .environment import choose_python

        kind = task["kind"]
        if kind in ("train", "build", "test") and any(
            r["live"] and r["kind"] in ("train", "build", "test") for r in self.records()
        ):
            raise ControlError("A Moyva training/build/test task is already running. Stop or wait for that owned task.")
        token = uuid.uuid4().hex
        task = {**task, "token": token, "run_id": run_id, "project": str(self.project.root)}
        request = self.project.local / "tasks" / (token + ".json")
        atomic_json(request, task)
        log = self.project.local / "logs" / (token + ".log")
        log.parent.mkdir(parents=True, exist_ok=True)
        with log.open("ab") as stream:
            command = [choose_python(self.project), str(self.project.root / "tools/ai/moyva_cli_main.py"), "_worker", str(request)]
            options = {"creationflags": subprocess.CREATE_NEW_PROCESS_GROUP} if os.name == "nt" else {"start_new_session": True}
            process = subprocess.Popen(
                command,
                cwd=self.project.root,
                stdin=subprocess.DEVNULL,
                stdout=stream,
                stderr=subprocess.STDOUT,
                **options,
            )
        return {
            "token": token,
            "pid": process.pid,
            "log": str(log),
            "state": "STARTING",
            "kind": kind,
            "run_id": run_id,
        }

    def stop(self, token):
        token = simple_name(token)
        record = read_json(self.project.local / "processes" / (token + ".json"))
        if not record or record.get("project") != str(self.project.root) or not same_process(record):
            raise ControlError("Process is absent or its identity changed; no signal was sent.")
        if os.name == "nt":
            atomic_json(self.project.local / "processes" / (token + ".stop"), {"requested": utc()})
        else:
            os.kill(record["pid"], signal.SIGINT)
        return {
            "state": "STOP_REQUESTED",
            "token": token,
            "message": "Cooperative stop requested; the worker closes only its owned children.",
        }

    def usage(self, record):
        if not same_process(record):
            return None
        psutil = psutil_module()
        try:
            process = psutil.Process(record["pid"])
            children = [process] + process.children(recursive=True)
            live_children = [p for p in children if p.is_running()]
            cpu = sum(sum(p.cpu_times()[:2]) for p in live_children)
            now = time.monotonic()
            cache_path = self.project.local / "usage" / (record["token"] + ".json")
            previous = read_json(cache_path, {})
            percent = (
                max(0, 100 * (cpu - previous["cpu"]) / (now - previous["time"]))
                if previous and now > previous["time"]
                else None
            )
            atomic_json(cache_path, {"cpu": cpu, "time": now})
            pids = [p.pid for p in live_children]
            return {
                "ram_bytes": sum(p.memory_info().rss for p in live_children),
                "cpu_seconds": cpu,
                "cpu_percent": percent,
                "workers": len(live_children),
                "children": [{"pid": p.pid, "name": p.name()} for p in live_children],
                "gpu": _gpu_usage_for_pids(pids),
            }
        except (psutil.NoSuchProcess, psutil.AccessDenied):
            return None


def worker_main(project, request_path):
    from .config import contained

    request_path = contained(project.local / "tasks", request_path)
    task = read_json(request_path)
    if task.get("project") != str(project.root):
        raise ControlError("Task belongs to another project.")
    token = simple_name(task["token"])
    if task.get("preset", {}).get("results"):
        project.results = project.path(task["preset"]["results"])
    record_path = project.local / "processes" / (token + ".json")
    record = {
        **identity(os.getpid()),
        "token": token,
        "kind": task["kind"],
        "run_id": task.get("run_id"),
        "project": str(project.root),
        "state": "RUNNING",
        "started": utc(),
        "log": str(project.local / "logs" / (token + ".log")),
    }
    atomic_json(record_path, record)
    os.environ["MOYVA_CLI_STOP_FILE"] = str(project.local / "processes" / (token + ".stop"))
    code, result = 1, None
    try:
        from .training import perform_task

        result = perform_task(project, task)
        code = 0
    except KeyboardInterrupt:
        code, result = 130, {"message": "Stopped by user"}
    except Exception as error:
        code, result = getattr(error, "code", 1) or 1, {"message": str(error)}
        import traceback

        traceback.print_exc()
    finally:
        state = "COMPLETED" if code == 0 else "INTERRUPTED" if code == 130 else "FAILED"
        record.update(state=state, finished=utc(), exit_code=code, result=result)
        atomic_json(record_path, record)
        if task["kind"] == "train" and task.get("run_id"):
            run = project.results / simple_name(task["run_id"])
            if run.is_dir():
                atomic_json(run / "cli-status.json", record)
        print(json.dumps(record, ensure_ascii=False), flush=True)
    return code
