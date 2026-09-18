from __future__ import annotations
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
from .config import ControlError, Project, atomic_json, contained, read_json, simple_name, utc


def status(project):
    from .environment import find_unity, probe
    from .unity import UnityBridge, player_status, unity_processes
    from .processes import Supervisor
    from .runs import RunStore
    try:processes=Supervisor(project).records();editors=unity_processes(project)
    except ControlError:processes=[];editors=[]
    runs=RunStore(project).list(False)
    checkpoints=RunStore(project).checkpoints()
    for record in processes:
        if record.get("live"):
            record["usage"] = Supervisor(project).usage(record)
    config=project.training_config()
    result = dict(project="Moyva",branch=project.git("branch","--show-current"),commit=project.git("rev-parse","HEAD"),
        dirty=bool(project.git("status","--porcelain")),unity=find_unity(project),environment=probe(project),
        contract=project.contract(),disk_free=shutil.disk_usage(project.root).free,player=player_status(project),
        editor=UnityBridge(project).status(),unity_processes=editors,processes=processes,
        latest_run=runs[0] if runs else None,latest_checkpoint=max(checkpoints,key=lambda c:c["created"]) if checkpoints else None,
        stage=config["curriculum"]["stage"],stages=project.stages(),readiness=read_json(project.local/"readiness.json",{}),runs=runs,checkpoints=checkpoints)
    atomic_json(project.local/"dashboard.json",result)
    return result


def status_line(snapshot):
    stamp=time.strftime("%H:%M:%S")
    live=[p for p in snapshot.get("processes") or [] if p.get("live")]
    if live:
        parts=[]
        for p in live:
            label=f"{p.get('kind') or 'task'}#{p.get('pid')}"
            usage=p.get("usage") or {}
            if usage.get("cpu_percent") is not None:label+=f" cpu={usage['cpu_percent']:.0f}%"
            if usage.get("ram_bytes"):label+=f" rss={usage['ram_bytes']/1073741824:.1f}GiB"
            parts.append(label)
        activity="+".join(parts)
    else:
        activity="idle"
    run=snapshot.get("latest_run") or {}
    step=(run.get("latest_checkpoint") or {}).get("step")
    if step is None:step="-"
    rate=(run.get("best_verified_checkpoint") or {}).get("success_rate")
    evaluation=run.get("evaluation") or {}
    eval_part=f" eval={str(evaluation.get('state') or '').lower()}:{evaluation.get('progress')}" if evaluation.get("state")=="EVALUATING" else ""
    run_part=f"run={run.get('run_id','-')} state={str(run.get('state') or '-').lower()} step={step}"
    if rate is not None:run_part+=f" best={rate:.0%}"
    return f"[{stamp}] {activity} | {run_part}{eval_part} | stage={snapshot.get('stage','?')}"


def status_watch(project,interval=5.0):
    interval=max(0.5,float(interval or 5.0))
    snapshot={}
    try:
        while True:
            snapshot=status(project)
            print(status_line(snapshot),flush=True)
            time.sleep(interval)
    except KeyboardInterrupt:
        return {"stopped":True,"snapshot":snapshot}


def disk_usage(project):
    targets={"Runs":project.results,"Training build":project.root/"Build/Training","Temp/ai":project.root/"Temp/ai",
             "Training venv":project.root/".venv-training","Unity Library":project.root/"Library"}
    result={}
    for name,directory in targets.items():
        total=0
        for base,folders,files in os.walk(directory,followlinks=False):
            folders[:]=[d for d in folders if not (Path(base)/d).is_symlink()]
            for file in files:
                path=Path(base)/file
                try:
                    if not path.is_symlink():total+=path.stat().st_size
                except OSError:pass
        result[name]={"path":str(directory),"bytes":total}
    return result


def cleanup(project,area,confirmation):
    allowed={"temp-ai":"Temp/ai","training-build":"Build/Training"}
    if area not in allowed:raise ControlError("Only Temp/ai and the training build can be cleaned here; select runs/checkpoints individually.")
    if confirmation!=area:raise ControlError("Confirm cleanup by typing: "+area)
    from .processes import Supervisor
    if any(r["live"] for r in Supervisor(project).records()):raise ControlError("Stop owned jobs before cleanup.")
    from .unity import unity_processes
    if area=="training-build" and unity_processes(project):raise ControlError("Close Unity before deleting its training build.")
    directory=contained(project.root,allowed[area])
    if directory.exists():shutil.rmtree(directory)
    return {"deleted":str(directory)}


def parser():
    p=argparse.ArgumentParser(prog="moyva",description="Moyva developer control center: Unity, training, runs and diagnostics.")
    p.add_argument("--json",action="store_true",help="Print structured JSON (commands also return meaningful exit codes).")
    sub=p.add_subparsers(dest="command")
    sub.add_parser("ui",help="Open the interactive terminal control center")
    s=sub.add_parser("status",help="Environment, Unity, contract, player, processes and latest run")
    s.add_argument("--watch","-w",action="store_true",help="Print a live status line until interrupted (Ctrl+C)")
    s.add_argument("--interval",type=float,default=5.0,metavar="SECONDS",help="Refresh period for --watch (default 5)")
    sub.add_parser("setup",help="Create/repair the project venv and install repository requirements")
    d=sub.add_parser("doctor",help="Diagnose infrastructure; repairs never alter gameplay")
    d.add_argument("--fix",action="store_true");d.add_argument("--confirm");d.add_argument("--refresh",action="store_true")
    u=sub.add_parser("unity",help="Control the current project's Editor through an allow-listed file bridge")
    u.add_argument("action",choices=["open","status","logs","play","stop","training-scene","gameplay-scene","monitor","control-center","save-scenes","refresh"])
    b=sub.add_parser("build",help="Build the dedicated training player only when stale")
    b.add_argument("target",choices=["training"]);b.add_argument("--rebuild",action="store_true");b.add_argument("--background",action="store_true")
    t=sub.add_parser("test",help="Python validation, Unity suites, readiness or communicator smoke")
    t.add_argument("suite",choices=["quick","editmode","playmode","ai","fullgame","readiness","build-smoke","communicator"])
    t.add_argument("--background",action="store_true")
    for name in ("train","preflight"):
        t=sub.add_parser(name,help="Review/start a preset using the existing ML-Agents launcher" if name=="train" else "Validate a training preset")
        t.add_argument("--preset",default="smoke");t.add_argument("--run-id");t.add_argument("--resume",action="store_true")
        t.add_argument("--profile",choices=["fast","standard","strict"],default="standard")
        t.add_argument("--max-steps",type=int);t.add_argument("--stage",type=int);t.add_argument("--seed",type=int);t.add_argument("--world-size",type=int)
        t.add_argument("--arenas",type=int);t.add_argument("--initialize-from");t.add_argument("--learn-initial-castle",action="store_true",default=None)
        t.add_argument("--episode-decisions",type=int);t.add_argument("--time-scale",type=float);t.add_argument("--checkpoint-interval",type=int)
        t.add_argument("--base-port",type=int);t.add_argument("--trainer");t.add_argument("--results");t.add_argument("--visual",action="store_true",default=None)
        t.add_argument("--dry-run",action="store_true");t.add_argument("--wait",action="store_true");t.add_argument("--fix",action="store_true")
    sub.add_parser("runs",help="Discover existing Results/MoyvaTraining runs")
    r=sub.add_parser("run",help="Show, resume, clone, compare, reveal, diagnostics or explicitly delete a run")
    r.add_argument("action",choices=["show","resume","clone","compare","reveal","delete","logs","diagnostics"]);r.add_argument("run_id");r.add_argument("other",nargs="?");r.add_argument("--confirm")
    c=sub.add_parser("checkpoints",help="Discover PT and ONNX checkpoints, with contract compatibility")
    c.add_argument("--run-id")
    c=sub.add_parser("checkpoint",help="Label, compare, export, resume, evaluate or explicitly delete a checkpoint")
    c.add_argument("action",choices=["show","label","favorite","export","compare","reveal","delete","resume","test"]);c.add_argument("path");c.add_argument("value",nargs="?");c.add_argument("--confirm")
    e=sub.add_parser("evaluate",help="Report current inference support; never pretend training is evaluation")
    e.add_argument("model");e.add_argument("--seed",type=int,default=1918);e.add_argument("--episodes",type=int,default=10);e.add_argument("--visual",action="store_true")
    d=sub.add_parser("deploy",help="Attach a trained ONNX to a gameplay bot difficulty (one command model setup)")
    d.add_argument("model",nargs="?",help="run id (uses its exported MoyvaStrategy.onnx) or path to an .onnx checkpoint")
    d.add_argument("--difficulty",help="Bot difficulty id to arm (default: registry defaultId)")
    d.add_argument("--off",action="store_true",help="Detach the model and return the difficulty to the heuristic bot")
    pr=sub.add_parser("preset",help="Manage reusable local presets without changing source YAML")
    pr.add_argument("action",choices=["list","show","save","duplicate","reset"]);pr.add_argument("name",nargs="?");pr.add_argument("value",nargs="?",help="JSON object for save; destination name for duplicate")
    tb=sub.add_parser("tensorboard",help="Start/stop only CLI-owned TensorBoard on localhost")
    tb.add_argument("action",nargs="?",choices=["start","stop","open"],default="start");tb.add_argument("--port",type=int,default=6006);tb.add_argument("--token")
    s=sub.add_parser("stop",help="Cooperatively stop an owned task, checking PID, start time and executable")
    s.add_argument("token")
    sub.add_parser("processes",help="List owned tasks and recovery state")
    l=sub.add_parser("logs",help="Read current/raw logs without discarding originals")
    l.add_argument("run_id",nargs="?");l.add_argument("--source",choices=["all","unity","mlagents","warning","error"],default="all")
    l.add_argument("--follow","-f",action="store_true",help="Stream new log lines until interrupted (Ctrl+C)")
    sub.add_parser("disk",help="Measure known project output directories")
    c=sub.add_parser("clean",help="Explicitly clean selected project output; never system caches")
    c.add_argument("area",choices=["temp-ai","training-build"]);c.add_argument("--confirm",required=True)
    s=sub.add_parser("settings",help="Show or update ignored machine-local configuration")
    s.add_argument("--set",metavar="JSON")
    w=sub.add_parser("_worker",help=argparse.SUPPRESS);w.add_argument("request")
    return p


BOT_REGISTRY = "Assets/Moyva/Presets/AI/Resources/MoyvaBotDifficultyRegistry.json"
BOT_MODELS_DIR = "Assets/Moyva/Presets/AI/Resources"


def _deploy_contract(project, run_meta, source):
    contract = (run_meta or {}).get("contract") or {}
    if contract.get("hash") != project.contract()["hash"]:
        raise ControlError("Model contract mismatch for " + source + ": checkpoint "
                           + str(contract.get("hash") or "<unknown>") + " != runtime "
                           + project.contract()["hash"] + ". Train a fresh checkpoint on this contract.")
    return contract


def _deploy_model_source(project, model):
    onnx = Path(model)
    if onnx.is_file():
        if onnx.suffix.lower() != ".onnx":
            raise ControlError("Deploy expects an exported .onnx model: " + model)
        for candidate in (onnx.parent / "run.json", onnx.parent.parent / "run.json",
                          onnx.parent / "checkpoint.json"):
            meta = read_json(candidate, None)
            if meta is not None:
                return onnx, meta
        raise ControlError("Cannot verify the contract of " + str(onnx)
                           + ": no run.json/checkpoint.json next to it. Pass a run id instead.")
    from .runs import RunStore
    run_dir = RunStore(project).path(model)
    if not run_dir.is_dir():
        raise ControlError("Run not found: " + model)
    onnx = run_dir / "MoyvaStrategy.onnx"
    if not onnx.is_file():
        raise ControlError("Run " + model + " has no exported MoyvaStrategy.onnx yet.")
    return onnx, read_json(run_dir / "run.json", {})


def deploy(project, model, difficulty_id, off):
    registry_path = project.root / BOT_REGISTRY
    registry = read_json(registry_path, None)
    if registry is None or not isinstance(registry.get("difficulties"), list) or not registry["difficulties"]:
        raise ControlError("Cannot read bot difficulty registry: " + str(registry_path))
    difficulties = registry["difficulties"]
    wanted = difficulty_id or registry.get("defaultId") or "normal"
    entry = next((d for d in difficulties
                  if isinstance(d, dict) and str(d.get("id", "")).lower() == str(wanted).lower()), None)
    if entry is None:
        raise ControlError("Unknown bot difficulty '" + str(wanted) + "'. Known: "
                           + ", ".join(str(d.get("id")) for d in difficulties))
    profile = entry.setdefault("modelProfile", {})
    if off:
        entry["policyMode"] = 0
        entry["policyModeName"] = "Heuristic"
        profile["enabled"] = False
        atomic_json(registry_path, registry)
        return {"difficulty": entry["id"], "policyMode": "Heuristic",
                "note": "Model detached; the " + entry["id"] + " bot is heuristic again."}
    if not model:
        raise ControlError("deploy needs a run id or .onnx path (or --off to detach).")
    onnx, meta = _deploy_model_source(project, model)
    contract = _deploy_contract(project, meta, model)
    resource = (profile.get("modelResourcePath") or "").strip() \
        or "AI/Models/MoyvaStrategy_" + entry["id"][:1].upper() + entry["id"][1:]
    target = contained(project.root, BOT_MODELS_DIR + "/" + resource + ".onnx")
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(onnx, target)
    run_id = meta.get("run_id") or Path(model).stem
    entry["policyMode"] = 2
    entry["policyModeName"] = "MLAgentsInference"
    profile.update(enabled=True, modelName=run_id, modelResourcePath=resource,
                   trainingRunId=str(run_id), contractVersion=contract.get("version", 2),
                   contractHash=contract["hash"],
                   notes="Deployed by moyva deploy from " + str(run_id) + ".")
    atomic_json(registry_path, registry)
    return {"difficulty": entry["id"], "policyMode": "MLAgentsInference", "model": str(target),
            "resource": "Resources.Load<ModelAsset>(\"" + resource + "\")",
            "contractHash": contract["hash"], "run": run_id,
            "note": "Bot matches on difficulty '" + entry["id"] + "' now run this model; "
                    "Unity will import the .onnx as a ModelAsset on next refresh. "
                    "Detach with: moyva deploy --difficulty " + entry["id"] + " --off"}


def _log_files(project,run_id,source):
    if run_id:
        from .runs import RunStore
        directory=RunStore(project).path(run_id)
        return [directory/name for name in (["unity.log"] if source=="unity" else ["mlagents.log"] if source=="mlagents" else ["cli.log","unity.log","mlagents.log"])]
    return sorted((project.local/"logs").glob("*.log"),key=lambda p:p.stat().st_mtime,reverse=True)[:1]

def _log_line_matches(line,source):
    text=line.lower()
    if source=="warning":return "warning" in text
    if source=="error":return "error" in text or "exception" in text
    return True

def follow_log_files(files,source,poll=1.0,window=32000):
    """tail -f over run logs: prints appended lines until KeyboardInterrupt."""
    import time
    offsets={};buffers={}
    while True:
        idle=True
        for path in files:
            try:size=path.stat().st_size
            except OSError:continue
            offset=offsets.get(path)
            if offset is None:offset=offsets.setdefault(path,max(0,size-window))
            if size<offset:offset=0;buffers[path]=""
            if size==offset:continue
            with path.open("rb") as stream:
                stream.seek(offset);chunk=stream.read(size-offset)
            offsets[path]=size;idle=False
            text=buffers.get(path,"")+chunk.decode("utf-8",errors="replace")
            parts=text.split("\n")
            buffers[path]=parts.pop()  # trailing partial line ("" when chunk ended at a boundary)
            for line in parts:
                line=line.rstrip("\r")
                if _log_line_matches(line,source):print(f"[{path.name}] {line}",flush=True)
        if idle:time.sleep(poll)

def logs(project,run_id=None,source="all",follow=False):
    from .diagnostics import tail
    files=_log_files(project,run_id,source)
    if follow:
        return follow_log_files(files,source)
    lines=[]
    for path in files:
        for line in tail(path).splitlines()[-150:]:
            if not _log_line_matches(line,source):continue
            lines.append(f"[{path.name}] {line}")
    return "\n".join(lines[-250:]) or "No log data yet."


def dispatch(project,args):
    from .environment import HostPlatform
    from .presets import Presets
    from .processes import Supervisor
    from .runs import RunStore
    from .training import build_player,legacy_arguments,preflight,start_tensorboard,start_training,validate_unity
    from .unity import UnityBridge
    cmd=args.command or "ui"
    if cmd=="_worker":
        from .processes import worker_main
        return worker_main(project,args.request)
    if cmd=="ui":
        try:from .tui.app import ControlCenter
        except ImportError:
            from .environment import setup
            print("Moyva's terminal interface is not installed. Run ./moyva setup (Python 3.10.1–3.10.12).")
            if sys.stdin.isatty() and input("Set up the project environment now? [y/N] ").lower()=="y":
                setup(project);return {"message":"Environment installed. Run ./moyva again to use its selected venv."}
            raise ControlError("Interface unavailable; status, doctor and setup remain usable.")
        ControlCenter(project).run();return None
    if cmd=="status":return status_watch(project,args.interval) if getattr(args,"watch",False) else status(project)
    if cmd=="setup":
        from .environment import setup
        return setup(project)
    if cmd=="doctor":
        from .diagnostics import doctor,repair,repair_plan
        report=doctor(project,args.refresh);plan=repair_plan(report)
        if args.fix:
            repairs=[]
            for item in plan:
                if item["category"]=="SAFE" or args.confirm:
                    repairs.append(repair(project,item["action"],args.confirm))
            report=doctor(project,True);report["repairs"]=repairs
        report["repair_plan"]=plan
        return report
    if cmd=="unity" and args.action=="logs":
        from .diagnostics import tail
        import platform
        path = Path(os.environ.get("LOCALAPPDATA",str(Path.home()))) / "Unity/Editor/Editor.log" if os.name=="nt" else Path.home() / ("Library/Logs/Unity/Editor.log" if platform.system()=="Darwin" else ".config/unity3d/Editor.log")
        return tail(path)
    if cmd=="unity":return UnityBridge(project).open() if args.action=="open" else UnityBridge(project).status() if args.action=="status" else UnityBridge(project).request(args.action)
    if cmd=="build":return Supervisor(project).launch({"kind":"build","force":args.rebuild}) if args.background else build_player(project,args.rebuild)
    if cmd=="test":
        if args.suite=="quick":
            completed=subprocess.run([sys.executable,"-m","unittest","discover","-s",str(project.root/"tools/ai/tests"),"-v"],cwd=project.root)
            if completed.returncode:raise ControlError("Python CLI tests failed.")
            return {"state":"COMPLETED","suite":"Python CLI tests"}
        if args.suite=="build-smoke":return build_player(project)
        if args.suite=="communicator":return start_training(project,Presets(project).get("smoke"),"communicator-"+time.strftime("%Y%m%d-%H%M%S"))
        return Supervisor(project).launch({"kind":"test","test":args.suite}) if args.background else validate_unity(project,args.suite)
    if cmd in ("train","preflight"):
        preset=Presets(project).get(args.preset)
        for key in ("arenas","initialize_from","learn_initial_castle","max_steps","stage","seed","world_size","episode_decisions","time_scale","checkpoint_interval","base_port","trainer","results","visual"):
            value=getattr(args,key,None)
            if value is not None:preset[key]=value
        Presets(project).validate(preset)
        run_id=args.run_id or time.strftime("moyva-%Y%m%d-%H%M%S")
        if args.dry_run:return {"preset":preset,"launcher_arguments":vars(legacy_arguments(project,preset,run_id,args.resume))}
        if cmd=="preflight":return preflight(project,preset,args.profile,args.fix)
        task=start_training(project,preset,run_id,args.resume,args.profile)
        if args.wait:
            try:
                while True:
                    record=next((r for r in Supervisor(project).records() if r["token"]==task["token"]),None)
                    if record and record["state"]!="RUNNING":
                        if record["state"]!="COMPLETED":raise ControlError(str(record.get("result")))
                        return record
                    time.sleep(1)
            except KeyboardInterrupt:
                Supervisor(project).stop(task["token"]);raise
        return task
    if cmd=="runs":return RunStore(project).list()
    if cmd=="run":
        store=RunStore(project)
        if args.action=="show":return store.show(args.run_id)
        if args.action=="logs":return logs(project,args.run_id)
        if args.action=="diagnostics":return store.diagnostics_zip(args.run_id,args.other)
        if args.action=="reveal":HostPlatform().reveal(store.path(args.run_id));return {"revealed":args.run_id}
        if args.action=="delete":return store.delete(args.run_id,confirmation=args.confirm)
        if args.action=="compare":return {"left":store.show(args.run_id),"right":store.show(args.other)}
        if args.action=="clone":return Presets(project).clone_run(args.run_id,args.other or args.run_id+"-clone")
        if args.action=="resume":
            run=store.show(args.run_id,False)
            preset=Presets(project).get("smoke")
            effective=read_json(store.path(args.run_id)/"effective-config.json",run)
            for key in preset:
                if key in effective:preset[key]=effective[key]
            preset["initialize_from"]=""  # Resume this run, not its historical parent.
            preset.update(trainer=str(store.path(args.run_id)/"trainer.yaml"),visual=run.get("mode")=="Visual")
            # Preserve the prior max_steps from its effective YAML, rather than resetting to the smoke preset.
            import yaml
            trainer=yaml.safe_load(Path(preset["trainer"]).read_text())["behaviors"]["MoyvaStrategy"]
            preset["max_steps"]=trainer["max_steps"];preset["checkpoint_interval"]=trainer.get("checkpoint_interval",50000)
            return start_training(project,preset,args.run_id,True)
    if cmd=="checkpoints":return RunStore(project).checkpoints(args.run_id)
    if cmd=="checkpoint":
        store=RunStore(project);item=store.checkpoint(args.path)
        if args.action=="show":return item
        if args.action=="favorite":return store.label(args.path,favorite=args.value!="false")
        if args.action=="label":return store.label(args.path,label=args.value or "")
        if args.action=="compare":return {"left":item,"right":store.checkpoint(args.value)}
        if args.action=="export":return store.export(args.path,args.value,args.confirm)
        if args.action=="delete":return store.delete(args.path,True,args.confirm)
        if args.action=="reveal":HostPlatform().reveal(Path(item["path"]).parent);return item
        if args.action=="resume":
            raise ControlError("ML-Agents resumes a run's latest persisted trainer state, not an arbitrary selected file. Use: ./moyva run resume "+item["run_id"])
        if args.action=="test":raise ControlError("Checkpoint evaluation is unavailable: MoyvaBotPolicyBinding requires a serialized ModelAsset. No runtime checkpoint-loader/evaluation endpoint exists.")
    if cmd=="evaluate":raise ControlError("Evaluation is unavailable: production inference uses a serialized ModelAsset, with no external model/episode evaluation command. Export an ONNX and assign it through MoyvaBotPolicyBinding; no strength result was fabricated.")
    if cmd=="deploy":return deploy(project,args.model,args.difficulty,args.off)
    if cmd=="preset":
        presets=Presets(project)
        if args.action=="list":return presets.list()
        if args.action=="show":return presets.get(args.name)
        if args.action=="save":return presets.save(args.name,json.loads(args.value))
        if args.action=="duplicate":return presets.save(args.value,presets.get(args.name))
        if args.action=="reset":return presets.reset(args.name)
    if cmd=="tensorboard":
        if args.action=="stop":return Supervisor(project).stop(args.token)
        result=start_tensorboard(project,args.port)
        if args.action=="open":
            import webbrowser;webbrowser.open(result["url"])
        return result
    if cmd=="stop":return Supervisor(project).stop(args.token)
    if cmd=="processes":return Supervisor(project).records()
    if cmd=="logs":return logs(project,args.run_id,args.source,args.follow)
    if cmd=="disk":return disk_usage(project)
    if cmd=="clean":return cleanup(project,args.area,args.confirm)
    if cmd=="settings":
        if args.set:project.save_settings(json.loads(args.set))
        return project.settings
    raise ControlError("Unknown command")


def main(argv=None):
    args=parser().parse_args(argv)
    try:
        result=dispatch(Project(),args)
        if args.command=="_worker":return result
        if result is not None:print(json.dumps(result,indent=2,ensure_ascii=False) if not isinstance(result,str) else result)
        return 2 if isinstance(result,dict) and result.get("state")=="BLOCKED" else 0
    except KeyboardInterrupt:
        print("Interrupted; owned tasks are available through ./moyva processes.",file=sys.stderr);return 130
    except (ControlError,OSError,ValueError,KeyError,TypeError) as error:
        print(json.dumps({"state":"BLOCKED","message":str(error)}) if args.json else "Moyva: "+str(error),file=sys.stderr);return 2
