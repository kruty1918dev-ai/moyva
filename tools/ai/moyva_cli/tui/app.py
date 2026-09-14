from __future__ import annotations
import asyncio
import json
from pathlib import Path
import time
from textual import on, work
from textual.app import App, ComposeResult
from textual.containers import Container, Horizontal, Vertical, VerticalScroll
from textual.screen import ModalScreen
from textual.widgets import (Button, ContentSwitcher, DataTable, Footer, Header, Input, Label,
                             OptionList, ProgressBar, RichLog, Select, Sparkline, Static, Switch, TextArea)
from textual.widgets.option_list import Option
from rich.table import Table
from ..config import ControlError, Project
from ..commands import dispatch, logs, parser, status
from ..presets import Presets
from ..runs import RunStore
from ..processes import Supervisor
from ..metrics import METRICS

PAGES=[("dashboard","Dashboard"),("training","Training"),("runs","Runs"),("checkpoints","Checkpoints"),
       ("unity","Unity"),("tests","Tests"),("build","Build"),("environment","Environment"),("logs","Logs"),("settings","Settings")]

class Prompt(ModalScreen[str | None]):
    def __init__(self,title,value="",expected=None):
        super().__init__();self.title_text=title;self.value=value;self.expected=expected
    def compose(self):
        with Vertical(id="dialog"):
            yield Label(self.title_text,id="prompt-title")
            yield Input(self.value,id="prompt-value")
            if self.expected is not None:yield Label("Type exactly: "+self.expected)
            with Horizontal(classes="buttons"):
                yield Button("Confirm",id="prompt-ok",variant="primary")
                yield Button("Cancel",id="prompt-cancel")
    def on_mount(self):self.query_one(Input).focus()
    @on(Button.Pressed)
    def pressed(self,event):
        if event.button.id=="prompt-cancel":self.dismiss(None)
        else:
            value=self.query_one(Input).value
            if self.expected is not None and value!=self.expected:
                self.notify("Confirmation does not match.",severity="error");return
            self.dismiss(value)
    @on(Input.Submitted)
    def submitted(self):self.query_one("#prompt-ok",Button).press()

class Details(ModalScreen):
    def __init__(self,title,data):super().__init__();self.title_text=title;self.data=data
    def compose(self):
        with Vertical(id="details-dialog"):
            yield Label(self.title_text,classes="page-title")
            yield TextArea(self.data if isinstance(self.data,str) else json.dumps(self.data,indent=2,ensure_ascii=False),read_only=True,id="details-text")
            yield Button("Close",id="details-close",variant="primary")
    @on(Button.Pressed)
    def close(self):self.dismiss()

class ControlCenter(App):
    TITLE="MOYVA"
    SUB_TITLE="Development & Training Control Center"
    CSS_PATH="app.tcss"
    BINDINGS=[("q","quit","Quit UI"),("r","refresh","Refresh"),("1","page('dashboard')","Dashboard"),
              ("2","page('training')","Training"),("3","page('runs')","Runs"),("ctrl+l","page('logs')","Logs")]
    def __init__(self,project=None):
        super().__init__();self.project=project or Project();self.selected_run=None;self.selected_checkpoint=None
        self.snapshot={};self.refreshing=False;self.first_load=True;self.last_log="";self.selected_job=None
    def compose(self)->ComposeResult:
        yield Header(show_clock=True)
        with Horizontal(id="shell"):
            with Vertical(id="sidebar"):
                yield Static("MOYVA\nCONTROL CENTER",id="brand")
                yield OptionList(*(Option(title,id=name) for name,title in PAGES),id="navigation")
                yield Static("Tasks continue when the UI closes.\nUse Stop safely to end an owned task.",id="ownership-note")
            with ContentSwitcher(initial="dashboard",id="pages"):
                with VerticalScroll(id="dashboard",classes="page"):
                    yield Label("Your development workspace",classes="page-title")
                    yield Static("Inspecting project and environment…",id="dashboard-info",classes="card")
                    with Horizontal(classes="buttons"):
                        yield Button("Train",id="goto-training",variant="primary")
                        yield Button("Preflight",id="dashboard-preflight")
                        yield Button("Diagnose",id="doctor")
                    yield Label("Live reward · structured TensorBoard events",classes="section-title")
                    yield Sparkline([],id="reward-chart")
                    yield Label("Episode length · policy loss when available",classes="section-title")
                    with Horizontal(id="secondary-charts"):
                        yield Sparkline([],id="episode-chart")
                        yield Sparkline([],id="loss-chart")
                    yield Static("Metrics appear when the trainer writes event data.",id="live-metrics",classes="card")
                    yield Label("Owned processes",classes="section-title")
                    yield DataTable(id="jobs-table",cursor_type="row")
                    yield Button("Stop selected task safely",id="stop-job",variant="warning")
                with VerticalScroll(id="training",classes="page"):
                    yield Label("Start or resume real training",classes="page-title")
                    yield Select([(name+" — "+p.get("description",name),name) for name,p in Presets(self.project).list().items()],value="smoke",id="preset")
                    with Container(id="training-fields"):
                        for key,label in [("run_id","Run id (blank = generated)"),("max_steps","Maximum steps"),("episode_decisions","Episode decision limit"),
                                          ("seed","Seed"),("world_size","World size"),("time_scale","Time scale"),("checkpoint_interval","Checkpoint interval"),
                                          ("trainer","Trainer YAML"),("results","Results directory"),("base_port","Worker port")]:
                            yield Label(label);yield Input(id="field-"+key)
                        yield Label("Curriculum stage");yield Select([(name,value) for name,value in self.project.stages().items()],value=0,id="stage")
                        yield Label("Visual environment");yield Switch(False,id="visual")
                        yield Label("Preflight profile");yield Select([(n.title(),n) for n in ("fast","standard","strict")],value="standard",id="profile")
                    with Horizontal(classes="buttons"):
                        yield Button("Review settings",id="review")
                        yield Button("Preflight",id="preflight")
                        yield Button("Start training",id="start-training",variant="success")
                    with Horizontal(classes="buttons"):
                        yield Button("Save as preset",id="save-preset")
                        yield Button("Duplicate preset",id="duplicate-preset")
                        yield Button("Reset to defaults",id="reset-preset")
                    yield ProgressBar(total=100,show_eta=False,id="training-progress")
                    yield Static("Idle. Pause is unavailable: ML-Agents has no safe pause protocol. Stop preserves checkpoints; resume continues the run.",id="training-state",classes="card")
                    with Horizontal(classes="buttons"):
                        yield Button("Stop safely",id="stop-training",variant="warning")
                        yield Button("TensorBoard",id="tensorboard")
                        yield Button("Resume selected run",id="training-resume")
                with VerticalScroll(id="runs",classes="page"):
                    yield Label("Training runs",classes="page-title")
                    yield DataTable(id="runs-table",cursor_type="row")
                    yield Static("Select a run for metadata, metrics and actions.",id="run-detail",classes="card")
                    with Horizontal(classes="buttons"):
                        for label,key in [("Details","run-show"),("Resume","run-resume"),("Clone preset","run-clone"),("Compare","run-compare"),("Logs","run-logs"),("Reveal","run-reveal")]:yield Button(label,id=key)
                        yield Button("Delete",id="run-delete",variant="error")
                with VerticalScroll(id="checkpoints",classes="page"):
                    yield Label("Checkpoints & exported models",classes="page-title")
                    yield DataTable(id="checkpoints-table",cursor_type="row")
                    yield Static("Contract compatibility is required for model export and run resume.",id="checkpoint-detail",classes="card")
                    with Horizontal(classes="buttons"):
                        for label,key in [("Details","checkpoint-show"),("Favorite","checkpoint-favorite"),("Label","checkpoint-label"),("Compare","checkpoint-compare"),("Export ONNX","checkpoint-export"),("Reveal","checkpoint-reveal"),("Test model","checkpoint-test")]:yield Button(label,id=key)
                        yield Button("Delete",id="checkpoint-delete",variant="error")
                    yield Button("Resume source run (latest persisted trainer state)",id="checkpoint-resume-run")
                with VerticalScroll(id="unity",classes="page"):
                    yield Label("Unity workspace",classes="page-title")
                    yield Static("Connecting…",id="unity-state",classes="card")
                    with Horizontal(classes="buttons"):
                        for label,key in [("Open Unity","open"),("Training scene","training-scene"),("Gameplay scene","gameplay-scene"),("Play","play"),("Stop","stop"),("Save scenes","save-scenes"),("Training monitor","monitor"),("Native control center","control-center"),("Editor logs","logs")]:yield Button(label,id="unity-"+key)
                    yield Static("Commands use the project-local allow-listed Editor bridge. Unsaved scenes are refused; Save scenes is an explicit action.",classes="card")
                with VerticalScroll(id="tests",classes="page"):
                    yield Label("Validation center",classes="page-title")
                    with Horizontal(classes="buttons"):
                        for name in ("quick","editmode","playmode","ai","fullgame","readiness","build-smoke","communicator"):yield Button(name.replace("-"," ").title(),id="test-"+name)
                    yield Static("Results include exact status and report locations. FullGame preflight requires readiness for the current source.",classes="card")
                    yield RichLog(id="test-output",wrap=True,markup=False)
                with VerticalScroll(id="build",classes="page"):
                    yield Label("Dedicated training player",classes="page-title")
                    yield Static("Checking build…",id="build-state",classes="card")
                    with Horizontal(classes="buttons"):
                        yield Button("Build if stale",id="build-training",variant="primary")
                        yield Button("Rebuild",id="rebuild-training")
                        yield Button("Measure disk usage",id="disk-usage")
                        yield Button("Clear Temp/ai",id="clean-temp",variant="warning")
                        yield Button("Remove training build",id="clean-build",variant="warning")
                with VerticalScroll(id="environment",classes="page"):
                    yield Label("Environment & safe repair",classes="page-title")
                    yield Static("Diagnostics read requirements from the repository. System packages, licensing and administrator operations require your explicit action.",classes="card")
                    with Horizontal(classes="buttons"):
                        yield Button("Diagnose",id="environment-doctor",variant="primary")
                        yield Button("Setup / repair dependencies",id="setup")
                        yield Button("Repair safe findings",id="doctor-fix")
                    yield RichLog(id="environment-output",wrap=True,markup=False)
                with VerticalScroll(id="logs",classes="page"):
                    yield Label("Live logs",classes="page-title")
                    yield Select([(s.upper(),s) for s in ("all","unity","mlagents","warning","error")],value="all",id="log-source")
                    yield Static("Select a run on the Runs page. With no run selected, the latest CLI task log is shown.",classes="card")
                    yield RichLog(id="live-log",wrap=True,markup=False,max_lines=500)
                with VerticalScroll(id="settings",classes="page"):
                    yield Label("Machine-local settings",classes="page-title")
                    yield Static("Saved in ignored .moyva-local/settings.json. Source gameplay configuration is never edited here.",classes="card")
                    yield TextArea(json.dumps(self.project.settings,indent=2),id="settings-json")
                    yield Button("Save local settings",id="save-settings",variant="primary")
        yield Footer()
    def on_mount(self):
        self.query_one("#jobs-table",DataTable).add_columns("Task","Run","State","PID","Started")
        self.query_one("#runs-table",DataTable).add_columns("Run","State","Stage","Commit","Steps","Reward","Checkpoints","Final ONNX","Started")
        self.query_one("#checkpoints-table",DataTable).add_columns("File","Step","Contract","Size KiB","Final","Favorite")
        self.load_preset("smoke")
        self.refresh_data()
        self.set_interval(max(2,float(self.project.settings.get("refresh_seconds",3))),self.refresh_data)
    @on(OptionList.OptionSelected,"#navigation")
    def navigation(self,event):self.action_page(event.option.id)
    def action_page(self,page):self.query_one("#pages",ContentSwitcher).current=page
    def action_refresh(self):self.refresh_data()
    @on(Select.Changed,"#preset")
    def changed_preset(self,event):
        if event.value is not Select.BLANK:self.load_preset(str(event.value))
    def load_preset(self,name):
        preset=Presets(self.project).get(name)
        for key in ("max_steps","episode_decisions","seed","world_size","time_scale","checkpoint_interval","trainer","results"):
            self.query_one("#field-"+key,Input).value=str(preset[key])
        self.query_one("#field-base_port",Input).value=str(preset.get("base_port",5005))
        self.query_one("#stage",Select).value=preset["stage"]
        self.query_one("#visual",Switch).value=preset["visual"]
    def form(self):
        preset=Presets(self.project).get(str(self.query_one("#preset",Select).value))
        for key in ("max_steps","episode_decisions","seed","world_size","checkpoint_interval","base_port"):
            preset[key]=int(self.query_one("#field-"+key,Input).value)
        preset["time_scale"]=float(self.query_one("#field-time_scale",Input).value)
        for key in ("trainer","results"):preset[key]=self.query_one("#field-"+key,Input).value
        preset["stage"]=self.query_one("#stage",Select).value;preset["visual"]=self.query_one("#visual",Switch).value
        return Presets(self.project).validate(preset)
    @work(group="refresh")
    async def refresh_data(self):
        if self.refreshing:return
        self.refreshing=True
        try:
            data=await asyncio.to_thread(status,self.project)
            self.snapshot=data
            table=Table.grid(padding=(0,3));table.add_column(style="bold #94a3b8");table.add_column()
            env=data["environment"]
            rows=[("Repository",f"{data['branch']} · {str(data['commit'])[:10]} · {'modified' if data['dirty'] else 'clean'}"),
                  ("Unity",f"{data['unity']['version'] or 'MISSING'} · {data['editor'].get('state','OFFLINE')}"),
                  ("Python",f"{'.'.join(map(str,env.get('version',[])))} · {env.get('python')}"),
                  ("ML-Agents / PyTorch",f"{env.get('packages',{}).get('mlagents') or 'MISSING'} / {env.get('packages',{}).get('torch') or 'MISSING'}"),
                  ("CUDA",str(env.get('cuda','Unavailable'))),("Player",data['player']['state']),
                  ("Contract",f"v{data['contract']['version']} · {data['contract']['hash'][:20]}…"),
                  ("Disk free",f"{data['disk_free']/1024**3:.1f} GiB"),
                  ("Curriculum",next((k for k,v in data['stages'].items() if v==data['stage']),str(data['stage']))),
                  ("Readiness","Previously passed; preflight checks source freshness" if data['readiness'].get('passed') else "Not validated for current source")]
            for row in rows:table.add_row(*row)
            self.query_one("#dashboard-info",Static).update(table)
            self.query_one("#unity-state",Static).update(json.dumps(data["editor"],indent=2))
            self.query_one("#build-state",Static).update(f"{data['player']['state']}\n{data['player']['path']}\nContract compatible: {data['player']['compatible']}")
            self.update_table("jobs-table",[(r["token"],[r["kind"],r.get("run_id") or "—",r["state"],str(r["pid"]),r["started"][:19]]) for r in data["processes"][:30]])
            runs=data["runs"]
            self.update_table("runs-table",[(r["run_id"],[r["run_id"],r["state"],str(r.get("stage","—")),str(r.get("commit", "—"))[:8],str(r.get("metrics",{}).get("step") or "—"),str(r.get("metrics",{}).get("mean_reward") or "—"),str(r.get("checkpoints",0)),str(r.get("final_onnx",False)),r.get("started_utc", "—")[:19]]) for r in runs])
            self.update_table("checkpoints-table",[(c["relative"],[c["relative"],str(c["step"] or "—"),"READY" if c["compatible"] else "INCOMPATIBLE",str(c["bytes"]//1024),str(c["final"]),"★" if c.get("favorite") else ""]) for c in data["checkpoints"]])
            active=next((r for r in data["processes"] if r["live"] and r["kind"]=="train"),None)
            run_id=self.selected_run or (active or {}).get("run_id") or (runs[0]["run_id"] if runs else None)
            if run_id:
                metrics=(await asyncio.to_thread(RunStore(self.project).show,run_id,True))["metrics"]
                reward=metrics["series"].get("Environment/Cumulative Reward",[])
                self.query_one("#reward-chart",Sparkline).data=[p["value"] for p in reward] or [0]
                self.query_one("#episode-chart",Sparkline).data=[p["value"] for p in metrics["series"].get("Environment/Episode Length",[])] or [0]
                loss=next((v for k,v in metrics["series"].items() if "loss" in k.lower()),[])
                self.query_one("#loss-chart",Sparkline).data=[p["value"] for p in loss] or [0]
                detail=f"{run_id} · step {metrics['step'] or '—'} · reward {metrics['mean_reward']} · episode length {metrics['episode_length']} · steps/s {metrics['steps_per_second']}"
                self.query_one("#live-metrics",Static).update(detail)
                usage=(active or {}).get("usage") or {}
                resources=f"RAM {usage.get('ram_bytes',0)/1024**2:.0f} MiB · CPU {usage.get('cpu_percent') or 0:.1f}% · {usage.get('workers',0)} owned processes" if active else "No active owned training task"
                self.query_one("#training-state",Static).update(detail+"\n"+resources)
                from ..config import read_json
                effective=read_json(self.project.results/run_id/"effective-config.json",{})
                maximum=effective.get("max_steps")
                if maximum:self.query_one("#training-progress",ProgressBar).update(progress=min(100,100*(metrics['step'] or 0)/maximum))
            text=await asyncio.to_thread(logs,self.project,self.selected_run,str(self.query_one("#log-source",Select).value))
            if text!=self.last_log:
                self.last_log=text;widget=self.query_one("#live-log",RichLog);widget.clear();widget.write(text)
            self.first_load=False
        except Exception as error:
            self.query_one("#dashboard-info",Static).update("Environment needs attention: "+str(error)+"\nUse Environment → Setup / Repair.")
        finally:self.refreshing=False
    def update_table(self,identifier,rows):
        widget=self.query_one("#"+identifier,DataTable)
        index=widget.cursor_row
        widget.clear()
        for key,values in rows:widget.add_row(*values,key=key)
        if rows:widget.move_cursor(row=min(index,len(rows)-1))
    @on(DataTable.RowSelected)
    def selected(self,event):
        key=str(event.row_key.value)
        if event.data_table.id=="runs-table":
            self.selected_run=key;self.query_one("#run-detail",Static).update(key+" selected. Details show metadata, elapsed state and structured metrics.")
        elif event.data_table.id=="checkpoints-table":
            self.selected_checkpoint=key;self.query_one("#checkpoint-detail",Static).update(key)
        else:self.selected_job=key
    @work(group="operations")
    async def invoke(self,arguments,show=True):
        try:
            result=await asyncio.to_thread(dispatch,self.project,parser().parse_args(arguments))
            text=result if isinstance(result,str) else json.dumps(result,indent=2,ensure_ascii=False)
            for identifier in ("environment-output","test-output"):self.query_one("#"+identifier,RichLog).write(text)
            if show:self.push_screen(Details("Moyva · "+" ".join(arguments[:2]),result))
            self.notify("Operation returned. See status and logs for background jobs.")
            self.refresh_data()
        except Exception as error:self.push_screen(Details("Operation blocked",str(error)))
    @work(group="training-start")
    async def start(self,preset,run_id,profile):
        try:
            from ..training import start_training
            result=await asyncio.to_thread(start_training,self.project,preset,run_id,False,profile)
            self.push_screen(Details("Training preflight started",result));self.refresh_data()
        except Exception as error:self.push_screen(Details("Training blocked",str(error)))
    @on(Button.Pressed)
    def button(self,event):
        key=event.button.id or ""
        try:
            if key=="goto-training":self.action_page("training")
            elif key in ("doctor","environment-doctor"):self.invoke(["doctor"])
            elif key=="doctor-fix":self.invoke(["doctor","--fix"])
            elif key=="setup":
                self.push_screen(Prompt("Create/repair the project venv and install repository requirements? Type setup.",expected="setup"),lambda value:self.invoke(["setup"]) if value else None)
            elif key in ("review","preflight","dashboard-preflight","start-training","save-preset","duplicate-preset","reset-preset"):
                preset=self.form();name=str(self.query_one("#preset",Select).value)
                if key=="review":self.push_screen(Details("Effective training settings",preset))
                elif key in ("preflight","dashboard-preflight"):
                    self.run_preflight(preset,str(self.query_one("#profile",Select).value))
                elif key=="start-training":self.start(preset,self.query_one("#field-run_id",Input).value or time.strftime("moyva-%Y%m%d-%H%M%S"),str(self.query_one("#profile",Select).value))
                elif key in ("save-preset","duplicate-preset"):
                    self.push_screen(Prompt("Name for the local preset",name+"-custom"),lambda value:self.save_preset(value,preset) if value else None)
                else:Presets(self.project).reset(name);self.load_preset(name)
            elif key in ("stop-job","stop-training"):
                token=self.selected_job if key=="stop-job" else next((r["token"] for r in self.snapshot.get("processes",[]) if r["live"] and r["kind"]=="train"),None)
                if not token:raise ControlError("Select an owned running task first.")
                self.invoke(["stop",token])
            elif key=="tensorboard":self.invoke(["tensorboard","open"])
            elif key.startswith("unity-"):self.invoke(["unity",key[6:]])
            elif key in ("build-training","rebuild-training"):self.invoke(["build","training","--background"]+(["--rebuild"] if key=="rebuild-training" else []))
            elif key.startswith("test-"):self.invoke(["test",key[5:],"--background"])
            elif key=="disk-usage":self.invoke(["disk"])
            elif key in ("clean-temp","clean-build"):
                area="temp-ai" if key=="clean-temp" else "training-build"
                self.push_screen(Prompt("Delete the selected project output?",expected=area),lambda value:self.invoke(["clean",area,"--confirm",value]) if value else None)
            elif key=="save-settings":
                self.project.save_settings(json.loads(self.query_one("#settings-json",TextArea).text));self.project=Project(self.project.root);self.notify("Local settings saved.")
            elif key.startswith("run-") or key=="training-resume":
                if not self.selected_run:raise ControlError("Select a run first.")
                action="resume" if key=="training-resume" else key[4:];run=self.selected_run
                if action=="delete":self.push_screen(Prompt("Permanently delete run and checkpoints?",expected=run),lambda value:self.invoke(["run","delete",run,"--confirm",value]) if value else None)
                elif action in ("compare","clone"):self.push_screen(Prompt("Other run id" if action=="compare" else "New preset name"),lambda value:self.invoke(["run",action,run,value]) if value else None)
                elif action=="logs":self.action_page("logs");self.refresh_data()
                else:self.invoke(["run",action,run])
            elif key.startswith("checkpoint-"):
                if not self.selected_checkpoint:raise ControlError("Select a checkpoint first.")
                action=key[11:];path=self.selected_checkpoint
                if action=="delete":self.push_screen(Prompt("Permanently delete this checkpoint?",expected=path),lambda value:self.invoke(["checkpoint","delete",path,"--confirm",value]) if value else None)
                elif action in ("label","compare","export"):self.push_screen(Prompt({"label":"Logical label","compare":"Other checkpoint relative path","export":"Destination .onnx path (existing files are refused)"}[action]),lambda value:self.invoke(["checkpoint",action,path,value]) if value else None)
                elif action=="resume-run":self.invoke(["run","resume",RunStore(self.project).checkpoint(path)["run_id"]])
                else:self.invoke(["checkpoint",action,path])
        except Exception as error:self.push_screen(Details("Check your selection",str(error)))
    def save_preset(self,name,preset):
        try:
            Presets(self.project).save(name,preset)
            self.query_one("#preset",Select).set_options([(n,n) for n in Presets(self.project).list()]);self.query_one("#preset",Select).value=name
            self.notify("Local preset saved.")
        except Exception as error:self.push_screen(Details("Preset rejected",str(error)))
    @work(group="preflight")
    async def run_preflight(self,preset,profile):
        from ..training import preflight
        try:result=await asyncio.to_thread(preflight,self.project,preset,profile,False);self.push_screen(Details("Preflight",result))
        except Exception as error:self.push_screen(Details("Preflight blocked",str(error)))
