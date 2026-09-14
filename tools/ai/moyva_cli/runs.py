from __future__ import annotations
import re
import shutil
from pathlib import Path
from .config import ControlError, atomic_json, contained, read_json, simple_name, utc
from .metrics import METRICS

class RunStore:
    def __init__(self, project): self.project = project
    def path(self, name): return contained(self.project.results, simple_name(name))
    def list(self, metrics=False):
        if not self.project.results.exists(): return []
        result = []
        for path in sorted(self.project.results.iterdir(), key=lambda p:p.name, reverse=True):
            if not path.is_dir() or path.is_symlink(): continue
            try: result.append(self.show(path.name, metrics))
            except ControlError as error: result.append({"run_id":path.name,"state":"FAILED","error":str(error),"path":str(path)})
        return sorted(result, key=lambda r:r.get("started_utc", ""), reverse=True)
    def show(self, name, metrics=True):
        path = self.path(name)
        if not path.is_dir(): raise ControlError("Run not found: " + name)
        meta = read_json(path / "run.json", {})
        resume = read_json(path / "resume.json", {})
        state = read_json(path / "cli-status.json", {})
        contract = meta.get("contract", {})
        compatible = contract.get("hash") == self.project.contract()["hash"]
        checkpoints = self.checkpoints(name)
        can_resume = compatible and any(c["kind"] == "pt" for c in checkpoints)
        status = state.get("state", "RESUMABLE" if can_resume else "INTERRUPTED")
        if state.get("state") == "RUNNING":
            from .processes import same_process
            status = "RUNNING" if same_process(state) else "INTERRUPTED"
        if not compatible: status = "INCOMPATIBLE"
        failure = read_json(path / "failure.json", {})
        state_evidence = "cli-status.json" if state else "No exit record; files alone do not establish success."
        if failure:
            state_evidence = failure.get("repair") or failure.get("component") or state_evidence
        data = {**meta, "run_id":name, "resume":resume, "state":status, "compatible":compatible,
                "resumable":can_resume, "path":str(path), "checkpoints":len(checkpoints),
                "final_onnx":(path / "MoyvaStrategy.onnx").is_file(), "process":state,
                "failure":failure, "state_evidence":state_evidence}
        metric_cache = self.project.local / "run-metrics" / (simple_name(name) + ".json")
        if metrics:
            data["metrics"] = METRICS.read(path)
            atomic_json(metric_cache,{k:v for k,v in data["metrics"].items() if k != "series"})
        else: data["metrics"] = read_json(metric_cache,{})
        from datetime import datetime, timezone
        try:
            start = datetime.fromisoformat(meta["started_utc"])
            end = datetime.fromisoformat(state["finished"]) if state.get("finished") else datetime.now(timezone.utc)
            data["elapsed_seconds"] = max(0,(end-start).total_seconds())
        except (KeyError,ValueError,TypeError):data["elapsed_seconds"] = None
        data["effective_config"] = read_json(path / "effective-config.json", {})
        return data
    def checkpoints(self, name=None):
        roots = [self.path(name)] if name else [p for p in self.project.results.iterdir() if p.is_dir() and not p.is_symlink()] if self.project.results.exists() else []
        result = []
        current = self.project.contract()["hash"]
        labels = read_json(self.project.local / "checkpoint-labels.json", {})
        for root in roots:
            try: contract = read_json(root / "run.json", {}).get("contract", {})
            except ControlError: contract = {}
            for path in sorted(root.rglob("*")):
                if path.suffix not in (".pt", ".onnx") or not path.is_file() or path.is_symlink(): continue
                if not path.resolve().is_relative_to(root.resolve()): continue
                relative = str(path.relative_to(self.project.results))
                numbers = re.findall(r"(?:-|_)(\d+)(?=\D|$)", path.stem)
                stat = path.stat()
                result.append({"path":str(path), "relative":relative, "run_id":root.name, "kind":path.suffix[1:],
                    "step":int(numbers[-1]) if numbers else None, "created":getattr(stat,"st_birthtime",stat.st_mtime), "timestamp_kind":"created" if hasattr(stat,"st_birthtime") else "modified", "bytes":stat.st_size,
                    "contract":contract.get("hash"), "compatible":contract.get("hash") == current,
                    "final":path.parent == root and path.name == "MoyvaStrategy.onnx", **labels.get(relative,{})})
        return result
    def checkpoint(self, relative):
        path = contained(self.project.results, relative)
        item = next((c for c in self.checkpoints() if Path(c["path"]) == path), None)
        if item is None: raise ControlError("Checkpoint not found or outside the results directory.")
        return item
    def label(self, relative, label=None, favorite=None):
        self.checkpoint(relative)
        path = self.project.local / "checkpoint-labels.json"
        values = read_json(path, {})
        entry = values.setdefault(relative, {})
        if label is not None: entry["label"] = label[:120]
        if favorite is not None: entry["favorite"] = bool(favorite)
        atomic_json(path, values)
        return entry
    def delete(self, name, checkpoint=False, confirmation=None):
        if confirmation != name: raise ControlError("Deletion requires confirmation matching the selected name exactly.")
        from .processes import Supervisor
        run_id = self.checkpoint(name)["run_id"] if checkpoint else name
        if any(r["live"] and r.get("run_id") == run_id for r in Supervisor(self.project).records()):
            raise ControlError("Cannot delete files belonging to an active run.")
        path = contained(self.project.results, name) if checkpoint else self.path(name)
        if checkpoint: path.unlink()
        else: shutil.rmtree(path)
        return {"deleted":name}
    def export(self, relative, destination, confirmation=None):
        entry = self.checkpoint(relative)
        if not entry["compatible"]: raise ControlError("MODEL CONTRACT MISMATCH: export/selection blocked.")
        if entry["kind"] != "onnx": raise ControlError("Export an ONNX model; PT files are trainer checkpoints.")
        destination = self.project.path(destination)
        if destination.suffix.lower() != ".onnx": raise ControlError("Destination must be an .onnx file.")
        if destination.exists() and confirmation != str(destination): raise ControlError("Destination exists; confirm its absolute path to overwrite.")
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(entry["path"], destination)
        atomic_json(Path(str(destination) + ".contract.json"), self.project.contract())
        return {"exported":str(destination), "inference":"Model copied. Assign it through the existing production model binding; runtime binding was not changed."}
