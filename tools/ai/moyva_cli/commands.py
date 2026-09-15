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
    sub.add_parser("status",help="Environment, Unity, contract, player, processes and latest run")
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
    pr=sub.add_parser("preset",help="Manage reusable local presets without changing source YAML")
    pr.add_argument("action",choices=["list","show","save","duplicate","reset"]);pr.add_argument("name",nargs="?");pr.add_argument("value",nargs="?",help="JSON object for save; destination name for duplicate")
    tb=sub.add_parser("tensorboard",help="Start/stop only CLI-owned TensorBoard on localhost")
    tb.add_argument("action",nargs="?",choices=["start","stop","open"],default="start");tb.add_argument("--port",type=int,default=6006);tb.add_argument("--token")
    s=sub.add_parser("stop",help="Cooperatively stop an owned task, checking PID, start time and executable")
    s.add_argument("token")
    sub.add_parser("processes",help="List owned tasks and recovery state")
    l=sub.add_parser("logs",help="Read current/raw logs without discarding originals")
    l.add_argument("run_id",nargs="?");l.add_argument("--source",choices=["all","unity","mlagents","warning","error"],default="all")
    sub.add_parser("disk",help="Measure known project output directories")
    c=sub.add_parser("clean",help="Explicitly clean selected project output; never system caches")
    c.add_argument("area",choices=["temp-ai","training-build"]);c.add_argument("--confirm",required=True)
    s=sub.add_parser("settings",help="Show or update ignored machine-local configuration")
    s.add_argument("--set",metavar="JSON")
    w=sub.add_parser("_worker",help=argparse.SUPPRESS);w.add_argument("request")
    return p


def logs(project,run_id=None,source="all"):
    from .diagnostics import tail
    if run_id:
        from .runs import RunStore
        directory=RunStore(project).path(run_id)
        files=[directory/name for name in (["unity.log"] if source=="unity" else ["mlagents.log"] if source=="mlagents" else ["cli.log","unity.log","mlagents.log"])]
    else:files=sorted((project.local/"logs").glob("*.log"),key=lambda p:p.stat().st_mtime,reverse=True)[:1]
    lines=[]
    for path in files:
        for line in tail(path).splitlines()[-150:]:
            if source in ("warning","error") and source not in line.lower() and (source!="error" or "exception" not in line.lower()):continue
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
    if cmd=="status":return status(project)
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
    if cmd=="logs":return logs(project,args.run_id,args.source)
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
