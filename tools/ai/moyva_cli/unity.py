from __future__ import annotations
import os
import time
import uuid
from pathlib import Path
from .config import ControlError, atomic_json, read_json, utc
from .environment import HostPlatform, find_unity

class UnityBridge:
    def __init__(self, project):
        self.project = project
        self.directory = project.local / "bridge"
    def status(self):
        status = read_json(self.directory / "editor.json", {})
        if status.get("project") != str(self.project.root): return {"state":"OFFLINE"}
        path = self.directory / "editor.json"
        if not path.exists() or time.time() - path.stat().st_mtime > 15:
            return {**status,"state":"UNRESPONSIVE"}
        if not any(p["pid"] == status.get("pid") for p in unity_processes(self.project)):
            return {**status,"state":"OFFLINE"}
        return status
    def request(self, command, *, argument="", target="", timeout=120):
        allowed = {"status","training-scene","gameplay-scene","play","stop","monitor","control-center",
                   "save-scenes","build-training","tests","readiness","refresh"}
        if command not in allowed: raise ControlError("Unsupported Unity bridge operation.")
        if self.status().get("state") in ("OFFLINE","UNRESPONSIVE"):
            raise ControlError("Moyva Editor bridge is unavailable. Open this project in Unity and allow script import to finish.")
        identifier = uuid.uuid4().hex
        request = {"id":identifier,"project":str(self.project.root),"command":command,"argument":argument,
                   "target":target,"submittedUtc":utc()}
        response = self.directory / "responses" / (identifier + ".json")
        atomic_json(self.directory / "requests" / (identifier + ".json"),request)
        until = time.monotonic() + timeout
        while time.monotonic() < until:
            stop_file = os.environ.get("MOYVA_CLI_STOP_FILE")
            if stop_file and Path(stop_file).exists():
                raise KeyboardInterrupt("Stopped observing Unity; an accepted Editor operation may still complete.")
            result = read_json(response)
            if result and result.get("state") not in ("RUNNING","QUEUED"):
                if result.get("state") != "COMPLETED": raise ControlError(result.get("message", "Unity operation failed.") + (" Report: " + result["report"] if result.get("report") else ""))
                return result
            time.sleep(0.25)
        raise ControlError(f"Unity {command} has not completed. Request {identifier} remains observable under {self.directory}; no success assumed.")
    def open(self):
        import subprocess
        found = find_unity(self.project)
        if not found["matches"]: raise ControlError("Install the exact Unity version " + found["expected"] + " using Unity Hub.")
        if unity_processes(self.project): return {"state":"RUNNING","message":"This project is already open."}
        subprocess.Popen([found["path"],"-projectPath",str(self.project.root),"-automated"], stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
        return {"state":"STARTING"}

def unity_processes(project):
    from .processes import psutil_module
    psutil = psutil_module()
    found = []
    for process in psutil.process_iter(["pid","name","cmdline","create_time"]):
        try:
            info = process.info
            if "unity" not in (info["name"] or "").lower(): continue
            args = info["cmdline"] or []
            for index, arg in enumerate(args[:-1]):
                candidate = Path(args[index+1])
                if not candidate.is_absolute(): candidate = Path(process.cwd()) / candidate
                if arg.lower() == "-projectpath" and candidate.resolve() == project.root:
                    found.append({"pid":info["pid"],"created":info["create_time"],"name":info["name"]})
                    break
        except (OSError, psutil.AccessDenied, psutil.NoSuchProcess): pass
    return found

def player_status(project):
    player = HostPlatform().player(project.root)
    manifest = read_json(Path(str(player)+".contract.json"), {})
    stamp = read_json(project.local / "build-state.json", {})
    compatible = manifest.get("hash") == project.contract()["hash"]
    exists = player.exists()
    fresh = exists and compatible and stamp.get("fingerprint") == project.fingerprint() and stamp.get("player") == str(player)
    return {"path":str(player),"exists":exists,"compatible":compatible,"fresh":fresh,
            "state":"READY" if fresh else "STALE" if exists else "MISSING", "manifest":manifest}
