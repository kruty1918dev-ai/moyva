from __future__ import annotations
import importlib.metadata
import json
import os
from pathlib import Path
import platform
import re
import shutil
import subprocess
import sys
from .config import ControlError, atomic_json, read_json, utc

class HostPlatform:
    """Platform differences are confined to executable layout, discovery and reveal."""
    def __init__(self, system=None):
        self.system = system or platform.system()
        if self.system not in ("Linux", "Windows", "Darwin"):
            raise ControlError("Unsupported host: " + self.system)
    @property
    def target(self): return {"Linux":"linux", "Windows":"windows", "Darwin":"macos"}[self.system]
    @property
    def build_target(self): return {"Linux":"StandaloneLinux64", "Windows":"StandaloneWindows64", "Darwin":"StandaloneOSX"}[self.system]
    def python(self, venv): return Path(venv) / ("Scripts/python.exe" if self.system == "Windows" else "bin/python")
    def player(self, root):
        suffix = {"Linux":".x86_64", "Windows":".exe", "Darwin":".app"}[self.system]
        return Path(root) / ("Build/Training/MoyvaTraining" + suffix)
    def unity_candidates(self, version):
        home = Path.home()
        if self.system == "Windows":
            return [Path(os.environ.get("PROGRAMFILES", "C:/Program Files")) / f"Unity/Hub/Editor/{version}/Editor/Unity.exe",
                    Path(os.environ.get("LOCALAPPDATA", str(home))) / f"Unity/Editors/{version}/Editor/Unity.exe"]
        if self.system == "Darwin": return [Path(f"/Applications/Unity/Hub/Editor/{version}/Unity.app/Contents/MacOS/Unity")]
        return [home / f"Unity/Hub/Editor/{version}/Editor/Unity", home / f"Unity/Editors/{version}/Editor/Unity",
                Path(f"/opt/unity/Editor/{version}/Editor/Unity")]
    def reveal(self, path):
        path = Path(path).resolve()
        if not path.exists(): raise ControlError("Path does not exist: " + str(path))
        if self.system == "Windows": os.startfile(str(path))
        else: subprocess.Popen(["open" if self.system == "Darwin" else "xdg-open", str(path)], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

def unity_version(project):
    text = (project.root / "ProjectSettings/ProjectVersion.txt").read_text()
    return re.search(r"m_EditorVersion:\s*(\S+)", text).group(1)

def find_unity(project):
    expected = unity_version(project)
    explicit = project.settings.get("unity") or os.environ.get("UNITY_EDITOR_PATH") or os.environ.get("UNITY_PATH")
    candidates = [Path(explicit)] if explicit else HostPlatform().unity_candidates(expected)
    for path in candidates:
        if path.is_file():
            # Installation path is normally versioned. For custom layouts ask the executable itself.
            version = expected if expected in path.parts else None
            if version is None:
                try:
                    r = subprocess.run([str(path), "-version"], capture_output=True, text=True, timeout=20)
                    found = re.search(r"\b\d+\.\d+\.\d+[abfp]\d+\b", r.stdout + r.stderr)
                    version = found.group(0) if found else None
                except (OSError, subprocess.TimeoutExpired): pass
            return {"path":str(path), "expected":expected, "version":version, "matches":version == expected}
    return {"path":None, "expected":expected, "version":None, "matches":False}

def python_info(executable):
    try:
        result = subprocess.run([str(executable), "-c", "import sys,json;print(json.dumps(list(sys.version_info[:3])))"],
                                capture_output=True, text=True, timeout=10)
        version = tuple(json.loads(result.stdout)) if result.returncode == 0 else ()
        return version, (3,10,1) <= version <= (3,10,12)
    except (OSError, ValueError, subprocess.TimeoutExpired): return (), False

def choose_python(project, supported=False):
    explicit = project.settings.get("python")
    if explicit:
        path = shutil.which(str(explicit)) or str(explicit)
        if python_info(path)[1] or not supported: return str(path)
        raise ControlError("MOYVA_PYTHON must be Python 3.10.1–3.10.12.")
    candidates = [HostPlatform().python(project.root / ".venv-training"), sys.executable, shutil.which("python3.10")]
    if os.name == "nt" and shutil.which("py"):
        try:
            selected = subprocess.run(["py", "-3.10", "-c", "import sys;print(sys.executable)"],capture_output=True,text=True,timeout=10)
            if selected.returncode == 0: candidates.append(selected.stdout.strip())
        except (OSError,subprocess.TimeoutExpired): pass
    candidates += sorted((Path.home()/".pyenv/versions").glob("3.10.*/bin/python"),reverse=True)
    for path in candidates:
        if path and python_info(path)[1]: return str(path)
    if supported: raise ControlError("Install Python 3.10.1–3.10.12 and set MOYVA_PYTHON to its executable. No system install was attempted.")
    return sys.executable

def probe(project, force=False):
    cache = project.local / "environment.json"
    import time
    old = read_json(cache, {})
    executable = choose_python(project)
    if not force and old.get("python") == executable and cache.exists() and time.time() - cache.stat().st_mtime < 300:
        return old
    script = '''import sys,json,importlib.metadata as m
r={"version":list(sys.version_info[:3]),"packages":{}}
for p in ["mlagents","torch","tensorboard","textual","psutil","PyYAML"]:
 try:r["packages"][p]=m.version(p)
 except m.PackageNotFoundError:r["packages"][p]=None
try:
 import torch
 r["cuda"]=torch.cuda.is_available()
except Exception as e:r["torch_error"]=str(e)
print(json.dumps(r))'''
    try:
        run = subprocess.run([executable, "-c", script], capture_output=True, text=True, timeout=45)
        data = json.loads(run.stdout.splitlines()[-1]) if run.returncode == 0 else {"error":run.stderr[-1000:]}
    except (OSError, ValueError, IndexError, subprocess.TimeoutExpired) as error: data = {"error":str(error)}
    data.update(python=executable, checked=utc())
    atomic_json(cache, data)
    return data

def setup(project, repair=False):
    from .processes import Supervisor
    if any(r["live"] and r["kind"] == "train" for r in Supervisor(project).records()):
        raise ControlError("Stop active training before repairing its Python environment.")
    venv = project.root / ".venv-training"
    python = HostPlatform().python(venv)
    if not python_info(python)[1]:
        base = choose_python(project, supported=True)
        if venv.exists():
            # Preserve the broken environment for explicit user cleanup.
            backup = project.local / ("venv-backup-" + utc().replace(":", "-"))
            backup.parent.mkdir(parents=True, exist_ok=True)
            venv.rename(backup)
        run = subprocess.run([base, "-m", "venv", str(venv)], cwd=project.root)
        if run.returncode:
            raise ControlError("venv creation failed. Install the venv module for your Python (Ubuntu: sudo apt install python3.10-venv), then run ./moyva setup. No privileged command was run.")
    commands = [[str(python), "-m", "pip", "install", "--upgrade", "pip"],
                [str(python), "-m", "pip", "install", "-r", str(project.root / "tools/ai/requirements-training.txt"),
                 "-r", str(project.root / "tools/ai/requirements-cli.txt")]]
    log_path = project.local / "logs/setup.log"
    log_path.parent.mkdir(parents=True, exist_ok=True)
    with log_path.open("a", encoding="utf-8") as log:
        for command in commands:
            log.write(utc() + " " + repr(command) + "\n"); log.flush()
            import moyva_train
            if moyva_train.run_process(command, logfile=log_path):
                raise ControlError("Dependency installation failed; see " + str(log_path))
    if os.name != "nt":
        for name in ("moyva", "moyva-train"):
            path = project.root / name
            if path.exists(): path.chmod(path.stat().st_mode | 0o111)
    return probe(project, force=True)
