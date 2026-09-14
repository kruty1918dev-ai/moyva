from __future__ import annotations
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import time
from .config import ControlError, atomic_json, read_json, simple_name, utc
from .environment import HostPlatform, choose_python, find_unity
from .presets import Presets
from .unity import UnityBridge, player_status, unity_processes


def legacy_arguments(project, preset, run_id, resume=False):
    """One launcher owns ML-Agents arguments, validation, YAML and process cleanup."""
    import moyva_train
    arguments=["resume" if resume else "train","--run-id",simple_name(run_id),"--target",HostPlatform().target,
               "--results",str(project.path(preset["results"])),"--trainer",str(project.path(preset["trainer"])),
               "--seed",str(preset["seed"]),"--stage",str(preset["stage"]),"--world-size",str(preset["world_size"]),
               "--max-steps",str(preset["max_steps"]),"--time-scale",str(preset["time_scale"]),
               "--episode-decisions",str(preset["episode_decisions"]),"--checkpoint-interval",str(preset["checkpoint_interval"]),
               "--summary-freq",str(preset.get("summary_freq",1000)),
               "--screen-width",str(preset.get("screen_width",1280)),"--screen-height",str(preset.get("screen_height",720)),
               "--base-port",str(preset.get("base_port",5005)),"--no-build"]
    arguments += ["--arenas", str(preset.get("arenas", 1))]
    if preset.get("initialize_from"): arguments += ["--initialize-from", simple_name(preset["initialize_from"])]
    if preset.get("learn_initial_castle"): arguments.append("--learn-initial-castle")
    if preset["visual"]:arguments.append("--visual")
    if project.settings.get("unity"):arguments += ["--unity",project.settings["unity"]]
    return moyva_train.parser().parse_args(arguments)


def build_player(project, force=False):
    import moyva_train
    state=player_status(project)
    if state["fresh"] and not force:return state
    if unity_processes(project):
        UnityBridge(project).request("build-training",argument=state["path"],target=HostPlatform().build_target,timeout=1200)
    else:
        args=moyva_train.parser().parse_args(["build","--target",HostPlatform().target,"--env",state["path"]])
        args.unity=find_unity(project)["path"]
        moyva_train.build(args)
    manifest=read_json(Path(state["path"]+".contract.json"),{})
    if manifest.get("hash")!=project.contract()["hash"]:raise ControlError("Built player manifest does not match the runtime contract.")
    stamp={"fingerprint":project.fingerprint(),"contract":manifest,"player":state["path"],"built":utc()}
    atomic_json(project.local/"build-state.json",stamp)
    return stamp


def validate_unity(project, kind):
    import moyva_train
    identifier=utc().replace(":","-")
    report_path=project.local/"validation"/(kind+"-"+identifier+".json")
    started=time.monotonic()
    if unity_processes(project):
        result=UnityBridge(project).request("readiness" if kind=="readiness" else "tests",argument=kind,timeout=1200)
    else:
        unity=find_unity(project)
        if not unity["matches"]:raise ControlError("Exact Unity version is unavailable.")
        log_path=project.local/"logs"/("unity-"+kind+"-"+identifier+".log")
        log_path.parent.mkdir(parents=True,exist_ok=True)
        command=[unity["path"],"-batchmode","-nographics","-projectPath",str(project.root),"-logFile",str(log_path)]
        xml=log_path.with_suffix(".xml")
        if kind=="readiness":command += ["-quit","-executeMethod","Kruty1918.Moyva.AI.Training.Editor.TrainingPlayerBuilder.ValidateFullGameScope"]
        else:
            command += ["-runTests","-testPlatform","PlayMode" if kind=="playmode" else "EditMode","-testResults",str(xml)]
            if kind in ("ai","fullgame"):
                command += ["-testFilter","Kruty1918.Moyva.AI.Training.Tests"+(".FullGameIntegrationTests" if kind=="fullgame" else "")]
        code=moyva_train.run_process(command)
        if code:raise ControlError(f"Unity {kind} failed (exit {code}); log: {log_path}")
        if kind!="readiness":
            import xml.etree.ElementTree as ET
            if not xml.exists():raise ControlError("Unity produced no test report: "+str(log_path))
            root=ET.parse(xml).getroot()
            if root.get("result")!="Passed" or int(root.get("total","0"))==0:raise ControlError("Unity tests failed or zero tests executed: "+str(xml))
        result={"state":"COMPLETED","report":str(xml) if kind!="readiness" else str(log_path)}
    result.update(duration=time.monotonic()-started,kind=kind,checked=utc(),fingerprint=project.fingerprint(),passed=True)
    atomic_json(report_path,result)
    if kind=="readiness":atomic_json(project.local/"readiness.json",result)
    return result


def preflight(project,preset,profile="standard",repair=False):
    from .diagnostics import doctor
    report=doctor(project)
    blockers=[c for c in report["checks"] if c["state"]=="BLOCKED" and not (repair and c["name"]=="Contract")]
    port=int(preset.get("base_port",5005))
    for worker_port in range(port, port + int(preset.get("arenas", 1))):
        with socket.socket() as sock:
            try:sock.bind(("127.0.0.1",worker_port))
            except OSError:blockers.append({"name":"Worker port","message":f"Port {worker_port} is occupied."})
    if blockers:raise ControlError("PRE-FLIGHT BLOCKED: "+"; ".join(c["name"]+": "+c["message"] for c in blockers))
    Presets(project).validate(preset)
    if preset.get("initialize_from"):
        source=project.path(preset["results"])/simple_name(preset["initialize_from"])
        metadata=read_json(source/"run.json",{})
        if metadata.get("contract",{}).get("hash")!=project.contract()["hash"] or not list(source.glob("MoyvaStrategy/*.pt")):
            raise ControlError("Initialize-from requires a compatible checkpoint in the selected results directory.")
    import yaml
    trainer=yaml.safe_load(project.path(preset["trainer"]).read_text())
    if set(trainer.get("behaviors",{}))!={"MoyvaStrategy"}:raise ControlError("Trainer must contain exactly MoyvaStrategy.")
    if not player_status(project)["fresh"]:
        if repair:build_player(project)
        else:raise ControlError("Training player missing/stale; build required.")
    if preset["stage"]==project.stages()["FullGame"]:
        ready=read_json(project.local/"readiness.json",{})
        if not ready.get("passed") or ready.get("fingerprint")!=project.fingerprint():
            if repair:validate_unity(project,"readiness")
            else:raise ControlError("FullGame readiness is not validated for current source.")
    if profile in ("standard","strict"):
        check=subprocess.run([choose_python(project),"-m","mlagents.trainers.learn","--help"],capture_output=True,text=True,timeout=45)
        if check.returncode:raise ControlError("Trainer cannot start: "+(check.stderr or check.stdout)[-1500:])
    if profile=="strict":validate_unity(project,"fullgame" if preset["stage"]==project.stages()["FullGame"] else "ai")
    return {"state":"READY","profile":profile,"checks":report["checks"]}


def start_training(project,preset,run_id,resume=False,profile="standard"):
    from .runs import RunStore
    from .processes import Supervisor
    Presets(project).validate(preset)
    run_id=simple_name(run_id)
    if project.path(preset["results"])!=project.results:
        project.save_settings({"results":preset["results"]})
        project.results=project.path(preset["results"])
    run=RunStore(project)
    if resume:
        old=run.show(run_id,False)
        if not old["resumable"]:raise ControlError("Run has no compatible resumable trainer checkpoint.")
    elif run.path(run_id).exists():raise ControlError("Run exists. Resume it or choose a new id.")
    return Supervisor(project).launch({"kind":"train","preset":preset,"resume":resume,"profile":profile},run_id)


def perform_task(project,task):
    import moyva_train
    kind=task["kind"]
    if kind=="setup":
        from .environment import setup
        return setup(project)
    if kind=="build":return build_player(project,task.get("force",False))
    if kind=="test":return validate_unity(project,task["test"])
    if kind=="tensorboard":
        code=moyva_train.run_process([sys.executable,"-m","tensorboard.main","--logdir",str(project.results),"--host","127.0.0.1","--port",str(task["port"])])
        if code:raise moyva_train.LaunchError("TensorBoard stopped",code)
        return {"port":task["port"]}
    if kind=="train":
        preset=task["preset"]
        preflight(project,preset,task.get("profile","standard"),repair=True)
        args=legacy_arguments(project,preset,task["run_id"],task.get("resume",False))
        run=project.results/task["run_id"]
        os.environ["MOYVA_CLI_PROCESS_RECORD"]=str(project.local/"processes"/(task["token"]+".json"))
        code=moyva_train.train(args)
        if code:
            from .diagnostics import classify,tail
            failure=classify(tail(run/"unity.log")+"\n"+tail(run/"mlagents.log"))
            atomic_json(run/"failure.json",failure)
            raise moyva_train.LaunchError(json.dumps(failure),code)
        return {"run_id":task["run_id"],"state":"COMPLETED"}
    raise ControlError("Unknown task kind: "+kind)


def start_tensorboard(project,port=6006):
    from .processes import Supervisor
    with socket.socket() as sock:
        try:sock.bind(("127.0.0.1",port))
        except OSError:
            sock.bind(("127.0.0.1",0));port=sock.getsockname()[1]
    return {**Supervisor(project).launch({"kind":"tensorboard","port":port}),"url":f"http://127.0.0.1:{port}"}
