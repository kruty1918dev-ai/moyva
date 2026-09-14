from __future__ import annotations
import os
import shutil
import socket
from pathlib import Path
from .config import ControlError, atomic_json, read_json, utc
from .environment import HostPlatform, find_unity, probe, python_info
from .unity import UnityBridge, player_status, unity_processes

FAILURES = [
    ("contract",("contract mismatch","contract is missing or stale","model contract mismatch"),"Training player / checkpoint","Rebuild the player; select a checkpoint with the current contract."),
    ("disk",("no space left","disk full","errno 28"),"Disk","Free space in selected project output directories."),
    ("lock",("another unity instance","project is locked"),"Unity editor","Use the open Editor bridge, or close the project before a batch build."),
    ("communicator",("unityenvironmentexception","timeout waiting","communicator","couldn't connect"),"Unity worker","Inspect unity.log; validate the player contract and a free worker port, then retry once."),
    ("python",("modulenotfounderror","no module named","requires-python"),"Python environment","Run ./moyva setup with Python 3.10.1–3.10.12."),
    ("port",("address already in use","winerror 10048"),"Worker port","Stop the owning run or select an unused port."),
    ("compile",("error cs","scripts have compiler errors"),"Unity compiler","Fix the reported source error, then recompile. Doctor never edits gameplay code."),
    ("build",("build failed","buildresult.failed"),"Unity build","Read the build log and install the matching platform module if missing."),
    ("tests",("tests failed","assertionexception"),"Unity tests","Read the test report; runtime/test failures are not auto-repaired."),
]

def classify(text):
    for key, needles, component, repair in FAILURES:
        lines = [line for line in text.splitlines() if any(needle in line.lower() for needle in needles)]
        if lines: return {"id":key,"component":component,"evidence":lines[-3:],"repair":repair}
    return {"id":"unknown","component":"Training process","evidence":text.splitlines()[-5:],"repair":"Inspect the preserved raw logs. No automatic repair is known."}

def tail(path, limit=32000):
    path = Path(path)
    if not path.is_file(): return ""
    with path.open("rb") as stream:
        stream.seek(max(0,path.stat().st_size-limit)); return stream.read(limit).decode("utf-8",errors="replace")

def disk_state(free, block_gib, warn_gib):
    return "BLOCKED" if free < block_gib*1024**3 else "WARNING" if free < warn_gib*1024**3 else "READY"

def doctor(project, force=False):
    data = probe(project, force)
    checks=[]
    def add(name,state,message,repair=None): checks.append(dict(name=name,state=state,message=message,repair=repair))
    version=tuple(data.get("version",()))
    add("Python","READY" if (3,10,1)<=version<=(3,10,12) else "BLOCKED",str(data.get("version",data.get("error"))),"setup")
    venv=HostPlatform().python(project.root/".venv-training")
    add("Virtual environment","READY" if python_info(venv)[1] else "BLOCKED",str(venv),"setup")
    expected={}
    for line in (project.root/"tools/ai/requirements-training.txt").read_text().splitlines():
        if "==" in line and not line.startswith("#"):
            name,version=line.split("==",1);expected[name.strip()]=version.strip()
    for name in ("mlagents","torch","tensorboard","textual","psutil"):
        installed=data.get("packages",{}).get(name)
        ok=installed is not None and (name not in expected or installed.split("+")[0]==expected[name])
        if name=="torch" and data.get("torch_error"):ok=False
        add(name,"READY" if ok else "BLOCKED",installed or "Missing","setup")
    found=find_unity(project)
    add("Unity","READY" if found["matches"] else "BLOCKED",found["path"] or "Install Unity "+found["expected"]+" through Unity Hub.")
    add("Unity Hub","READY" if shutil.which("unityhub") or Path("/Applications/Unity Hub.app").exists() or Path("C:/Program Files/Unity Hub/Unity Hub.exe").exists() else "WARNING","Unity Hub is needed for licenses and missing build modules.")
    player=player_status(project)
    add("Training player","READY" if player["fresh"] else "WARNING",player["state"]+": "+player["path"],"build")
    add("Contract","READY" if not player["exists"] or player["compatible"] else "BLOCKED",project.contract()["hash"],"build")
    free=shutil.disk_usage(project.root).free
    add("Disk",disk_state(free,float(project.settings.get("disk_block_gib",2)),float(project.settings.get("disk_warn_gib",10))),f"{free/1024**3:.2f} GiB free")
    try:
        project.results.mkdir(parents=True,exist_ok=True)
        import tempfile
        with tempfile.TemporaryFile(dir=project.results) as stream:stream.write(b"probe")
        add("Results writable","READY",str(project.results))
    except OSError as error:add("Results writable","BLOCKED",str(error))
    for name in ("moyva","moyva-train"):
        path=project.root/name
        add(name,"READY" if os.name=="nt" or os.access(path,os.X_OK) else "WARNING",str(path),"chmod")
    try:
        live=unity_processes(project)
        lock=project.root/"Temp/UnityLockfile"
        bridge=UnityBridge(project).status()
        add("Editor lock","WARNING" if lock.exists() and not live else "READY" if not live or bridge.get("state")=="READY" else "WARNING",
            f"{len(live)} matching Unity process(es); bridge {bridge.get('state')}","stale-lock" if lock.exists() and not live else None)
        from .processes import Supervisor
        for record in Supervisor(project).records()[:5]:
            if record["state"] in ("FAILED","INTERRUPTED"):
                add("Previous "+record["kind"],"WARNING",str(record.get("result"))+"; "+record["log"])
    except ControlError as error:add("Process identity","WARNING",str(error),"setup")
    with socket.socket() as sock:
        try:sock.bind(("127.0.0.1",5005));add("Worker port 5005","READY","Available")
        except OSError:add("Worker port 5005","WARNING","Occupied; use an unused --base-port for a new run.")
    from .runs import RunStore
    for run in RunStore(project).list(False):
        if run.get("state") in ("INCOMPATIBLE", "FAILED", "INTERRUPTED"):
            add("Run " + run["run_id"], "WARNING", run.get("error") or run["state"] + ": " + run.get("state_evidence", "Inspect run metadata/logs."))
    config = project.training_config()
    add("Training config", "READY" if not config.get("allowScaffoldSimulation") and config.get("environmentCount",1)==1 and config.get("behaviorType",0)==0 else "BLOCKED",
        "Real gameplay, one environment and Default behavior are required; source config is never auto-repaired.")
    readiness=read_json(project.local/"readiness.json",{})
    fresh=readiness.get("fingerprint")==project.fingerprint() and readiness.get("passed")
    add("FullGame readiness","READY" if fresh else "WARNING","Validated" if fresh else "Not validated for current source; run ./moyva test readiness.")
    result={"state":"BLOCKED" if any(c["state"]=="BLOCKED" for c in checks) else "WARNING" if any(c["state"]=="WARNING" for c in checks) else "READY","checks":checks,"environment":data,"checked":utc()}
    atomic_json(project.local/"diagnostics.json",result)
    return result

def repair_plan(report):
    result=[]
    for check in report["checks"]:
        action=check.get("repair")
        if check["state"]!="READY" and action and action not in [r["action"] for r in result]:
            result.append({"action":action,"category":"CONFIRMATION_REQUIRED" if action=="stale-lock" else "SAFE","reason":check["message"]})
    return result

def repair(project, action, confirmation=None):
    if action=="setup":
        from .environment import setup
        return setup(project,True)
    if action=="chmod":
        for name in ("moyva","moyva-train"):
            path=project.root/name
            if path.exists():path.chmod(path.stat().st_mode|0o111)
        return {"repaired":"launcher permissions"}
    if action=="build":
        from .training import build_player
        return build_player(project)
    if action=="stale-lock":
        if confirmation!="remove stale Unity lock":raise ControlError("Confirm exactly: remove stale Unity lock")
        if unity_processes(project):raise ControlError("A matching Unity process is alive; lock retained.")
        (project.root/"Temp/UnityLockfile").unlink(missing_ok=True)
        return {"repaired":"stale Unity lock"}
    raise ControlError("No safe repair registered for "+action)
