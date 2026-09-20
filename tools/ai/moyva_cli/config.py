from __future__ import annotations
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile
from datetime import datetime, timezone

ROOT = Path(__file__).resolve().parents[3]

class ControlError(Exception):
    pass

def utc():
    return datetime.now(timezone.utc).isoformat()

def read_json(path, default=None):
    try:
        return json.loads(Path(path).read_text(encoding="utf-8-sig"))
    except FileNotFoundError:
        return default
    except (ValueError, OSError) as error:
        raise ControlError(f"Cannot read {path}: {error}") from error

def atomic_json(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, temporary = tempfile.mkstemp(prefix="." + path.name, dir=path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as stream:
            json.dump(value, stream, indent=2, ensure_ascii=False)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
    finally:
        Path(temporary).unlink(missing_ok=True)

def simple_name(value):
    if not re.fullmatch(r"[A-Za-z0-9][A-Za-z0-9_.-]{0,119}", value or "") or value in (".", ".."):
        raise ControlError("Use a simple name: letters, numbers, dot, dash, underscore (maximum 120).")
    return value

def contained(base, value, *, allow_root=False):
    base = Path(base).resolve()
    path = (base / value).resolve()
    if (path == base and not allow_root) or not path.is_relative_to(base):
        raise ControlError(f"Path must stay inside {base}: {value}")
    return path

class Project:
    def __init__(self, root=ROOT):
        self.root = Path(root).resolve()
        self.local = self.root / ".moyva-local"
        defaults = read_json(self.root / "tools/ai/moyva-cli.json", {})
        self.settings = {**defaults, **read_json(self.local / "settings.json", {})}
        for key, env in (("results", "MOYVA_RESULTS"), ("unity", "MOYVA_UNITY"), ("python", "MOYVA_PYTHON")):
            if os.environ.get(env): self.settings[key] = os.environ[env]
        self.results = self.path(self.settings.get("results", "Results/MoyvaTraining"))

    def path(self, value):
        return (self.root / Path(value).expanduser()).resolve()

    def git(self, *args):
        result = subprocess.run(["git", *args], cwd=self.root, capture_output=True, text=True,
                                encoding="utf-8", errors="replace", timeout=15)
        return result.stdout.strip() if result.returncode == 0 else None

    def fingerprint(self):
        digest = hashlib.sha256()
        for args in (("ls-files", "-s", "Assets/Moyva", "Packages", "ProjectSettings"),
                     ("diff", "HEAD", "--binary", "--", "Assets/Moyva", "Packages", "ProjectSettings")):
            digest.update((self.git(*args) or "").encode())
        for name in (self.git("ls-files", "--others", "--exclude-standard", "Assets/Moyva", "Packages", "ProjectSettings") or "").splitlines():
            path = self.path(name)
            if path.is_file(): digest.update(name.encode() + path.read_bytes())
        return digest.hexdigest()

    def training_config(self):
        return read_json(self.root / "Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json")

    def stages(self):
        source = (self.root / "Assets/Moyva/AI/Training/Runtime/Curriculum/TrainingCurriculumConfig.cs").read_text()
        body = re.search(r"enum\s+TrainingCurriculumStage\s*\{([^}]+)\}", source).group(1)
        stages, number = {}, 0
        for item in body.split(","):
            item = item.strip()
            if not item: continue
            name, *assigned = item.split("=")
            if assigned: number = int(assigned[0].strip())
            stages[name.strip()] = number
            number += 1
        return stages

    def contract(self):
        # Shared canonical parser; no second neural schema.
        import moyva_train
        if self.root != moyva_train.ROOT:
            return parse_contract(self.root)
        return moyva_train.contract()

    def save_settings(self, changes):
        allowed = {"results", "trainer", "disk_block_gib", "disk_warn_gib", "refresh_seconds", "preflight", "unity", "python"}
        if set(changes) - allowed: raise ControlError("Unknown CLI setting.")
        for field in ("disk_block_gib", "disk_warn_gib", "refresh_seconds"):
            if field in changes and float(changes[field]) <= 0: raise ControlError(field + " must be positive.")
        atomic_json(self.local / "settings.json", {**read_json(self.local / "settings.json", {}), **changes})

def parse_contract(root):
    # Used for isolated fixture projects; the production parser accepts an explicit root too.
    import moyva_train
    return moyva_train.contract(root)
