from __future__ import annotations

import asyncio
import json
from datetime import datetime, timezone
from pathlib import Path
import shutil
import subprocess
import sys
import time

from rich.table import Table
from textual import on, work
from textual.app import App, ComposeResult
from textual.containers import Container, Horizontal, Vertical, VerticalScroll
from textual.screen import ModalScreen
from textual.widgets import (
    Button,
    ContentSwitcher,
    DataTable,
    Footer,
    Header,
    Input,
    Label,
    OptionList,
    ProgressBar,
    RichLog,
    Select,
    Sparkline,
    Static,
    Switch,
    TextArea,
)
from textual.widgets.option_list import Option

from ..commands import dispatch, logs, parser, status
from ..config import ControlError, Project, read_json
from ..metrics import METRICS
from ..presets import Presets
from ..processes import Supervisor
from ..runs import RunStore

PAGES = [
    ("dashboard", "Home"),
    ("training", "Training"),
    ("runs", "History"),
    ("checkpoints", "Models"),
    ("tools", "Tools"),
    ("logs", "Logs"),
    ("settings", "Settings"),
]


def _duration(seconds):
    if seconds is None or seconds < 0:
        return "—"
    seconds = int(seconds)
    hours, seconds = divmod(seconds, 3600)
    minutes, seconds = divmod(seconds, 60)
    if hours:
        return f"{hours:d}h {minutes:02d}m"
    if minutes:
        return f"{minutes:d}m {seconds:02d}s"
    return f"{seconds:d}s"


def _elapsed_since(value):
    if not value:
        return None
    try:
        started = datetime.fromisoformat(value.replace("Z", "+00:00"))
        if started.tzinfo is None:
            started = started.replace(tzinfo=timezone.utc)
        return max(0, (datetime.now(timezone.utc) - started).total_seconds())
    except (ValueError, TypeError):
        return None


def _phase(project, run_id, active_record, metrics):
    """Return a user-facing best-effort training phase without inventing success."""
    if not active_record:
        return "IDLE"
    if metrics and metrics.get("step") not in (None, 0):
        return "TRAINING"
    try:
        from ..diagnostics import tail
        worker = tail(active_record.get("log", ""), 24000).lower()
        run_dir = project.results / run_id if run_id else None
        mlagents = tail(run_dir / "mlagents.log", 24000).lower() if run_dir else ""
    except Exception:
        worker, mlagents = "", ""
    if "building dedicated training player" in worker and "build complete" not in worker:
        return "REBUILDING TRAINING PLAYER"
    if "build complete" in worker and "live ml-agents log" not in worker:
        return "PREPARING TRAINING"
    if "live ml-agents log" in worker and not mlagents.strip():
        return "STARTING ML-AGENTS"
    if mlagents.strip():
        return "WAITING FOR FIRST METRICS"
    return "PREFLIGHT / STARTING"


def _startup_message(phase, elapsed, summary_freq, learning_confirmed):
    elapsed_text = _duration(elapsed)
    if learning_confirmed:
        return f"STATUS: TRAINING ACTIVE · elapsed {elapsed_text} · ML-Agents metrics are updating."
    if phase == "REBUILDING TRAINING PLAYER":
        return f"STATUS: PREPARING · elapsed {elapsed_text} · rebuilding the Unity training player before learning can start."
    if phase == "PREPARING TRAINING":
        return f"STATUS: PREPARING · elapsed {elapsed_text} · build is ready; launching the trainer and Unity worker."
    if phase == "STARTING ML-AGENTS":
        return f"STATUS: STARTING · elapsed {elapsed_text} · ML-Agents is launching; waiting for Unity to connect."
    if phase == "WAITING FOR FIRST METRICS":
        return f"STATUS: CONNECTED · elapsed {elapsed_text} · waiting for the first metric report (~{summary_freq:,} steps)."
    return f"STATUS: STARTING · elapsed {elapsed_text} · running preflight and startup checks."


def _preset_hint(name):
    hints = {
        "castle-first": "Learn to place the first castle. The opponent has a settlement. Use Continue skills from run ID for later lessons.",
        "arenas-preview": "Four independent Unity windows train one policy. Advanced settings changes the arena count.",
        "smoke": "Pipeline check only. Use this to verify that Unity and ML-Agents can communicate.",
        "fullgame-smoke": "Recommended first FullGame run: real gameplay with a short 10k-step validation.",
        "fullgame-preview": "FullGame training with a visible Unity window. It opens maximized on a free monitor when possible and stays resizable/minimizable.",
        "fullgame": "Normal FullGame training (500k steps). Best after FullGame smoke succeeds.",
        "long": "Long FullGame run (5M steps). Use only after shorter runs look healthy.",
        "visual-debug": "Visual Movement-stage debug run; not a complete FullGame training run.",
    }
    return hints.get(name, "Training preset using real Moyva gameplay.")


class Prompt(ModalScreen[str | None]):
    def __init__(self, title, value="", expected=None):
        super().__init__()
        self.title_text = title
        self.value = value
        self.expected = expected

    def compose(self):
        with Vertical(id="dialog"):
            yield Label(self.title_text, id="prompt-title")
            yield Input(self.value, id="prompt-value")
            if self.expected is not None:
                yield Label("Type exactly: " + self.expected)
            with Horizontal(classes="buttons compact-buttons"):
                yield Button("Confirm", id="prompt-ok", variant="primary")
                yield Button("Cancel", id="prompt-cancel")

    def on_mount(self):
        self.query_one(Input).focus()

    @on(Button.Pressed)
    def pressed(self, event):
        if event.button.id == "prompt-cancel":
            self.dismiss(None)
            return
        value = self.query_one(Input).value
        if self.expected is not None and value != self.expected:
            self.notify("Confirmation does not match.", severity="error")
            return
        self.dismiss(value)

    @on(Input.Submitted)
    def submitted(self):
        self.query_one("#prompt-ok", Button).press()


class Details(ModalScreen):
    def __init__(self, title, data):
        super().__init__()
        self.title_text = title
        self.data = data

    def compose(self):
        with Vertical(id="details-dialog"):
            yield Label(self.title_text, classes="page-title")
            yield TextArea(
                self.data if isinstance(self.data, str) else json.dumps(self.data, indent=2, ensure_ascii=False),
                read_only=True,
                id="details-text",
            )
            yield Button("Close", id="details-close", variant="primary")

    @on(Button.Pressed)
    def close(self):
        self.dismiss()


class ExitTrainingDialog(ModalScreen[str | None]):
    """Make background training explicit when the user quits the TUI."""

    def __init__(self, run_id):
        super().__init__()
        self.run_id = run_id

    def compose(self):
        with Vertical(id="exit-dialog"):
            yield Label("Training is still running", classes="page-title")
            yield Static(
                f"Run {self.run_id} is active. Closing the Control Center does not automatically stop ML-Agents.\n"
                "Choose what should happen now.",
                classes="card",
            )
            yield Button("Keep training in background and close", id="exit-keep", variant="primary")
            yield Button("Stop training safely and close", id="exit-stop", variant="warning")
            yield Button("Cancel", id="exit-cancel")

    @on(Button.Pressed)
    def pressed(self, event):
        self.dismiss({"exit-keep": "keep", "exit-stop": "stop"}.get(event.button.id))


class ControlCenter(App):
    TITLE = "MOYVA"
    SUB_TITLE = "Development & Training Control Center"
    CSS_PATH = "app.tcss"
    BINDINGS = [
        ("q", "quit", "Quit UI"),
        ("r", "refresh", "Refresh"),
        ("1", "page('dashboard')", "Dashboard"),
        ("2", "page('training')", "Training"),
        ("3", "page('runs')", "Runs"),
        ("ctrl+l", "page('logs')", "Logs"),
    ]

    def __init__(self, project=None):
        super().__init__()
        self.project = project or Project()
        self.selected_run = None
        self.selected_checkpoint = None
        self.selected_job = None
        self.snapshot = {}
        self.refreshing = False
        self.first_load = True
        self.last_log = ""
        self._closing = False
        self._keep_training_on_exit = False

    def compose(self) -> ComposeResult:
        yield Header(show_clock=True)
        with Horizontal(id="shell"):
            with Vertical(id="sidebar"):
                yield Static("MOYVA\nCONTROL CENTER", id="brand")
                yield OptionList(*(Option(title, id=name) for name, title in PAGES), id="navigation")
                yield Static(
                    "Closing the Control Center stops training by default.\nPress Q to choose whether to keep it running in background.",
                    id="ownership-note",
                )
            with ContentSwitcher(initial="dashboard", id="pages"):
                with VerticalScroll(id="dashboard", classes="page"):
                    yield Label("Moyva AI", classes="page-title")
                    yield Static("Checking whether a model is training…", id="home-training-card", classes="active-card")
                    with Horizontal(classes="home-actions"):
                        yield Button("Open training", id="goto-training", variant="primary")
                        yield Button("Open live logs", id="home-logs")
                        yield Button("Stop training", id="home-stop", variant="warning", disabled=True)
                    yield Static("Checking system…", id="home-health", classes="card")
                    yield Label("Latest learning metrics", classes="section-title")
                    yield Static("No metrics yet.", id="live-metrics", classes="card")
                    yield Sparkline([], id="reward-chart")
                    with Horizontal(id="secondary-charts"):
                        yield Sparkline([], id="episode-chart")
                        yield Sparkline([], id="loss-chart")
                    yield Static("Technical details are under Tools.", id="dashboard-info", classes="hint-card")
                    yield DataTable(id="jobs-table", cursor_type="row", classes="technical-table")
                    yield Button("Stop selected background task", id="stop-job", variant="warning", disabled=True, classes="technical-only")

                with VerticalScroll(id="training", classes="page"):
                    yield Label("Train the Moyva bot", classes="page-title")
                    yield Static(
                        "1. Choose a training type.  2. Press Start fast or Start & watch.  "
                        "Moyva performs checks and rebuilds automatically.",
                        classes="card",
                    )
                    yield Label("Training type", classes="field-title")
                    yield Select(
                        [
                            ("Quick FullGame check — 10,000 steps", "fullgame-smoke"),
                            ("Normal FullGame training — 500,000 steps", "fullgame"),
                            ("Long FullGame training — 5,000,000 steps", "long"),
                            ("Technical pipeline smoke", "smoke"),
                        ],
                        value="fullgame-smoke",
                        id="preset",
                    )
                    yield Static(_preset_hint("fullgame-smoke"), id="preset-help", classes="hint-card")
                    with Horizontal(id="simple-run-row"):
                        yield Label("Run name (optional)")
                        yield Input(placeholder="Auto-generated if blank", id="field-run_id")
                    with Horizontal(classes="simple-start-actions"):
                        yield Button("Start fast", id="start-training", variant="success")
                        yield Button("Start & watch", id="start-preview", variant="primary")
                        yield Button("Advanced settings", id="toggle-advanced")
                    with Container(id="training-advanced", classes="advanced-hidden"):
                        yield Static("Advanced settings are optional. Defaults are recommended.", classes="hint-card")
                        with Container(id="training-fields"):
                            for key, label in [
                                ("max_steps", "Maximum steps"),
                                ("episode_decisions", "Episode decision limit"),
                                ("seed", "Seed"),
                                ("world_size", "World size"),
                                ("arenas", "Parallel arenas (1–16)"),
                                ("initialize_from", "Continue skills from run ID"),
                                ("time_scale", "Time scale"),
                                ("checkpoint_interval", "Checkpoint interval"),
                                ("summary_freq", "Metric update every N steps"),
                                ("screen_width", "Preview width"),
                                ("screen_height", "Preview height"),
                                ("trainer", "Trainer YAML"),
                                ("results", "Results directory"),
                                ("base_port", "Worker port"),
                            ]:
                                yield Label(label)
                                yield Input(id="field-" + key)
                            yield Label("Curriculum stage")
                            yield Select([(name, value) for name, value in self.project.stages().items()], value=0, id="stage")
                            yield Label("Preflight profile")
                            yield Select([(n.title(), n) for n in ("fast", "standard", "strict")], value="standard", id="profile")
                            yield Label("Visual environment")
                            yield Switch(False, id="visual")
                        with Horizontal(classes="advanced-actions"):
                            yield Button("Preflight + repair", id="preflight")
                            yield Button("Review settings", id="review")
                            yield Button("Save preset", id="save-preset")
                            yield Button("Reset defaults", id="reset-preset")

                    yield Label("What is happening now", classes="section-title")
                    yield Static(
                        "NOT RUNNING\nChoose a training type above and press Start fast or Start & watch.",
                        id="active-training-card",
                        classes="active-card",
                    )
                    yield Static(
                        "Progress will appear after ML-Agents reports its first steps.",
                        id="training-progress-label",
                        classes="progress-label",
                    )
                    yield ProgressBar(total=100, show_eta=False, id="training-progress")
                    with Horizontal(classes="active-actions-simple"):
                        yield Button("Stop training", id="stop-training", variant="warning", disabled=True)
                        yield Button("Show live log", id="toggle-training-log", disabled=True)
                        yield Button("Open results folder", id="active-results", disabled=True)
                        yield Button("Models / checkpoints", id="active-checkpoints", disabled=True)
                        yield Button("TensorBoard", id="tensorboard")
                        yield Button("Focus preview", id="active-preview", disabled=True)
                    with Container(id="training-log-panel", classes="advanced-hidden"):
                        yield Label("Live training log", classes="section-title")
                        yield RichLog(id="training-log", wrap=True, markup=False, max_lines=120)
                    yield Static(
                        "Closing the Control Center stops training by default. Press Q to explicitly choose Keep in background if you want it to continue.",
                        id="training-state",
                        classes="hint-card",
                    )

                with VerticalScroll(id="runs", classes="page"):
                    yield Label("Training history", classes="page-title")
                    yield Static("Each run is saved here. Select one to inspect, resume, view logs, or open its folder.", classes="card")
                    yield DataTable(id="runs-table", cursor_type="row")
                    yield Static("Select a run.", id="run-detail", classes="card")
                    with Horizontal(classes="history-actions"):
                        yield Button("Details", id="run-show", disabled=True)
                        yield Button("Resume", id="run-resume", disabled=True)
                        yield Button("Logs", id="run-logs", disabled=True)
                        yield Button("Open folder", id="run-reveal", disabled=True)
                        yield Button("Delete", id="run-delete", variant="error", disabled=True)
                    with Horizontal(classes="advanced-history-actions"):
                        yield Button("Clone preset", id="run-clone", disabled=True)
                        yield Button("Compare", id="run-compare", disabled=True)

                with VerticalScroll(id="checkpoints", classes="page"):
                    yield Label("Models & checkpoints", classes="page-title")
                    yield Static(
                        "Checkpoints are automatic save points. Final ONNX is the exported model. Select an item for actions.",
                        classes="card",
                    )
                    yield DataTable(id="checkpoints-table", cursor_type="row")
                    yield Static("Select a model/checkpoint.", id="checkpoint-detail", classes="card")
                    with Horizontal(classes="model-actions"):
                        yield Button("Details", id="checkpoint-show", disabled=True)
                        yield Button("Open folder", id="checkpoint-reveal", disabled=True)
                        yield Button("Export ONNX", id="checkpoint-export", disabled=True)
                        yield Button("Resume source run", id="checkpoint-resume-run", disabled=True)
                    with Horizontal(classes="advanced-model-actions"):
                        yield Button("Favorite", id="checkpoint-favorite", disabled=True)
                        yield Button("Label", id="checkpoint-label", disabled=True)
                        yield Button("Compare", id="checkpoint-compare", disabled=True)
                        yield Button("Test model", id="checkpoint-test", disabled=True)
                        yield Button("Delete", id="checkpoint-delete", variant="error", disabled=True)

                with VerticalScroll(id="tools", classes="page"):
                    yield Label("Tools", classes="page-title")
                    yield Label("Unity", classes="section-title")
                    yield Static("Connecting…", id="unity-state", classes="card")
                    with Horizontal(classes="tool-actions"):
                        yield Button("Open Unity", id="unity-open", variant="primary")
                        yield Button("Play", id="unity-play")
                        yield Button("Stop", id="unity-stop")
                        yield Button("Training monitor", id="unity-monitor")
                        yield Button("Editor logs", id="unity-logs")
                    yield Label("Training player", classes="section-title")
                    yield Static("Checking build…", id="build-state", classes="card")
                    with Horizontal(classes="tool-actions"):
                        yield Button("Build if needed", id="build-training", variant="primary")
                        yield Button("Rebuild", id="rebuild-training")
                        yield Button("Disk usage", id="disk-usage")
                    yield Label("Validation", classes="section-title")
                    with Horizontal(classes="tool-actions"):
                        yield Button("Quick tests", id="test-quick")
                        yield Button("FullGame readiness", id="test-readiness")
                        yield Button("FullGame tests", id="test-fullgame")
                        yield Button("Communicator smoke", id="test-communicator")
                    yield RichLog(id="test-output", wrap=True, markup=False)
                    yield Label("Environment", classes="section-title")
                    with Horizontal(classes="tool-actions"):
                        yield Button("Diagnose", id="environment-doctor", variant="primary")
                        yield Button("Setup / repair", id="setup")
                        yield Button("Safe repair", id="doctor-fix")
                    yield RichLog(id="environment-output", wrap=True, markup=False)

                with VerticalScroll(id="logs", classes="page"):
                    yield Label("Live logs", classes="page-title")
                    yield Select(
                        [(s.upper(), s) for s in ("all", "unity", "mlagents", "warning", "error")],
                        value="all",
                        id="log-source",
                    )
                    yield Static(
                        "Select a run on the Runs page. With no run selected, the latest CLI task log is shown.",
                        classes="card",
                    )
                    yield RichLog(id="live-log", wrap=True, markup=False, max_lines=500)

                with VerticalScroll(id="settings", classes="page"):
                    yield Label("Machine-local settings", classes="page-title")
                    yield Static(
                        "Advanced machine-local settings. Most users do not need to change these.",
                        classes="card",
                    )
                    yield TextArea(json.dumps(self.project.settings, indent=2), id="settings-json")
                    yield Button("Save local settings", id="save-settings", variant="primary")
        yield Footer()

    def _widget(self, selector, expected_type=None):
        """Lifecycle-safe widget lookup for background workers.

        Textual workers can finish after the app started unmounting. Iterating query()
        returns no matches without throwing; query_one() would raise NoMatches and was
        the cause of the observed Control Center shutdown crash.
        """
        if self._closing:
            return None
        try:
            for widget in self.query(selector):
                if expected_type is None or isinstance(widget, expected_type):
                    return widget
        except Exception:
            return None
        return None

    def _safe_push(self, screen):
        if self._closing:
            return False
        try:
            self.push_screen(screen)
            return True
        except Exception:
            return False

    def _safe_notify(self, message, **kwargs):
        if self._closing:
            return
        try:
            self.notify(message, **kwargs)
        except Exception:
            pass

    def _safe_refresh(self):
        if not self._closing:
            try:
                self.refresh_data()
            except Exception:
                pass

    def _set_disabled(self, selector, disabled):
        widget = self._widget(selector, Button)
        if widget is not None:
            widget.disabled = disabled

    def on_mount(self):
        self.query_one("#jobs-table", DataTable).add_columns("Task", "Run", "State", "PID", "Started")
        self.query_one("#runs-table", DataTable).add_columns("Run", "State", "Stage", "Commit", "Steps", "Reward", "Checkpoints", "Final ONNX", "Started")
        self.query_one("#checkpoints-table", DataTable).add_columns("File", "Step", "Contract", "Size KiB", "Final", "Favorite")
        self.load_preset("fullgame-smoke")
        self.refresh_data()
        self.set_interval(max(2, float(self.project.settings.get("refresh_seconds", 3))), self.refresh_data)

    def on_unmount(self):
        if not self._keep_training_on_exit:
            active = next((r for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
            if active:
                try:
                    Supervisor(self.project).stop(active["token"])
                except Exception:
                    pass
        self._closing = True
        self.refreshing = False

    def action_quit(self):
        active = next((r for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
        if not active:
            self._closing = True
            self.exit()
            return

        def decided(choice):
            if choice is None:
                return
            if choice == "stop":
                try:
                    Supervisor(self.project).stop(active["token"])
                except Exception as error:
                    self._safe_notify("Could not request stop: " + str(error), severity="error")
                    return
            elif choice == "keep":
                self._keep_training_on_exit = True
            self._closing = True
            self.exit()

        self.push_screen(ExitTrainingDialog(active.get("run_id") or "current run"), decided)

    @on(OptionList.OptionSelected, "#navigation")
    def navigation(self, event):
        self.action_page(event.option.id)

    def action_page(self, page):
        widget = self._widget("#pages", ContentSwitcher)
        if widget is not None:
            widget.current = page

    def action_refresh(self):
        self._safe_refresh()

    @on(Select.Changed, "#preset")
    def changed_preset(self, event):
        if event.value is not Select.BLANK:
            name = str(event.value)
            self.load_preset(name)
            hint = self._widget("#preset-help", Static)
            if hint is not None:
                hint.update(_preset_hint(name))

    def load_preset(self, name):
        preset = Presets(self.project).get(name)
        for key in (
            "max_steps", "episode_decisions", "seed", "world_size", "time_scale", "checkpoint_interval",
            "summary_freq", "screen_width", "screen_height", "arenas", "initialize_from", "trainer", "results"
        ):
            widget = self._widget("#field-" + key, Input)
            if widget is not None:
                defaults = {"arenas": 1, "initialize_from": "", "summary_freq": 500, "screen_width": 1280, "screen_height": 720}
                widget.value = str(preset.get(key, defaults.get(key, "")))
        port = self._widget("#field-base_port", Input)
        if port is not None:
            port.value = str(preset.get("base_port", 5005))
        stage = self._widget("#stage", Select)
        visual = self._widget("#visual", Switch)
        if stage is not None:
            stage.value = preset["stage"]
        if visual is not None:
            visual.value = preset["visual"]
        hint = self._widget("#preset-help", Static)
        if hint is not None:
            hint.update(_preset_hint(name))

    def form(self):
        preset_selector = self.query_one("#preset", Select)
        preset = Presets(self.project).get(str(preset_selector.value))
        for key in (
            "max_steps", "episode_decisions", "seed", "world_size", "checkpoint_interval", "base_port",
            "summary_freq", "screen_width", "screen_height", "arenas"
        ):
            preset[key] = int(self.query_one("#field-" + key, Input).value)
        preset["time_scale"] = float(self.query_one("#field-time_scale", Input).value)
        for key in ("trainer", "results", "initialize_from"):
            preset[key] = self.query_one("#field-" + key, Input).value
        preset["stage"] = self.query_one("#stage", Select).value
        preset["visual"] = self.query_one("#visual", Switch).value
        return Presets(self.project).validate(preset)

    @work(group="refresh")
    async def refresh_data(self):
        if self.refreshing or self._closing:
            return
        self.refreshing = True
        try:
            data = await asyncio.to_thread(status, self.project)
            if self._closing:
                return
            dashboard = self._widget("#dashboard-info", Static)
            if dashboard is None:
                return

            self.snapshot = data
            table = Table.grid(padding=(0, 3))
            table.add_column(style="bold #94a3b8")
            table.add_column()
            env = data["environment"]
            editor_state = data["editor"].get("state", "OFFLINE")
            rows = [
                ("Repository", f"{data['branch']} · {str(data['commit'])[:10]} · {'modified' if data['dirty'] else 'clean'}"),
                ("Unity", f"{data['unity']['version'] or 'MISSING'} · {editor_state}"),
                ("Python", f"{'.'.join(map(str, env.get('version', [])))} · {env.get('python')}"),
                ("ML-Agents / PyTorch", f"{env.get('packages', {}).get('mlagents') or 'MISSING'} / {env.get('packages', {}).get('torch') or 'MISSING'}"),
                ("CUDA", str(env.get("cuda", "Unavailable"))),
                ("Player", data["player"]["state"]),
                ("Contract", f"v{data['contract']['version']} · {data['contract']['hash'][:20]}…"),
                ("Disk free", f"{data['disk_free'] / 1024**3:.1f} GiB"),
                ("Curriculum", next((k for k, v in data["stages"].items() if v == data["stage"]), str(data["stage"]))),
                ("Readiness", "Previously passed; preflight checks source freshness" if data["readiness"].get("passed") else "Not validated for current source"),
            ]
            dashboard.update(
                f"Unity {data['unity']['version'] or 'missing'} · Python {'.'.join(map(str, env.get('version', [])))} · "
                f"ML-Agents {env.get('packages', {}).get('mlagents') or 'missing'} · "
                f"Training player {data['player']['state']} · Disk {data['disk_free'] / 1024**3:.1f} GiB free"
            )

            unity_state = self._widget("#unity-state", Static)
            if unity_state is not None:
                editor = data["editor"]
                unity_table = Table.grid(padding=(0, 3))
                unity_table.add_column(style="bold #94a3b8")
                unity_table.add_column()
                for row in [
                    ("Editor bridge", editor.get("state", "OFFLINE")),
                    ("Unity version", editor.get("unityVersion") or data["unity"].get("version") or "—"),
                    ("Scene", editor.get("scene") or "No scene reported"),
                    ("Play mode", "Running" if editor.get("playing") else "Stopped"),
                    ("Compiling", "Yes" if editor.get("compiling") else "No"),
                    ("Unsaved changes", "Yes" if editor.get("dirty") else "No"),
                    ("Editor PID", str(editor.get("pid") or "—")),
                ]:
                    unity_table.add_row(*row)
                if editor.get("state") == "UNRESPONSIVE":
                    unity_table.add_row("Meaning", "Editor bridge heartbeat is stale. Training player can still run independently.")
                unity_state.update(unity_table)
            build_state = self._widget("#build-state", Static)
            if build_state is not None:
                build_state.update(
                    f"{data['player']['state']}\n{data['player']['path']}\nContract compatible: {data['player']['compatible']}"
                )

            self.update_table(
                "jobs-table",
                [
                    (r["token"], [r["kind"], r.get("run_id") or "—", r["state"], str(r["pid"]), r["started"][:19]])
                    for r in data["processes"][:30]
                ],
            )
            runs = data["runs"]
            self.update_table(
                "runs-table",
                [
                    (
                        r["run_id"],
                        [
                            r["run_id"],
                            r["state"],
                            str(r.get("stage", "—")),
                            str(r.get("commit", "—"))[:8],
                            str(r.get("metrics", {}).get("step") or "—"),
                            str(r.get("metrics", {}).get("mean_reward") if r.get("metrics", {}).get("mean_reward") is not None else "—"),
                            str(r.get("checkpoints", 0)),
                            str(r.get("final_onnx", False)),
                            r.get("started_utc", "—")[:19],
                        ],
                    )
                    for r in runs
                ],
            )
            self.update_table(
                "checkpoints-table",
                [
                    (
                        c["relative"],
                        [
                            c["relative"],
                            str(c["step"] or "—"),
                            "READY" if c["compatible"] else "INCOMPATIBLE",
                            str(c["bytes"] // 1024),
                            str(c["final"]),
                            "★" if c.get("favorite") else "",
                        ],
                    )
                    for c in data["checkpoints"]
                ],
            )

            active = next((r for r in data["processes"] if r["live"] and r["kind"] == "train"), None)
            active_run = (active or {}).get("run_id")
            home_stop = self._widget("#home-stop", Button)
            if home_stop is not None:
                home_stop.disabled = active is None
            home_health = self._widget("#home-health", Static)
            if home_health is not None:
                warnings = []
                if data["player"].get("state") != "READY":
                    warnings.append("training player " + str(data["player"].get("state")))
                if data["disk_free"] < 10 * 1024**3:
                    warnings.append("low disk space")
                if data["environment"].get("packages", {}).get("mlagents") != "1.1.0":
                    warnings.append("ML-Agents needs attention")
                home_health.update(
                    "SYSTEM READY — training dependencies are available." if not warnings
                    else "SYSTEM WARNING — " + "; ".join(warnings) + ". Open Tools for details."
                )
            displayed_run = self.selected_run or active_run or (runs[0]["run_id"] if runs else None)
            displayed_info = None
            metrics = None
            if displayed_run:
                try:
                    displayed_info = await asyncio.to_thread(RunStore(self.project).show, displayed_run, True)
                    metrics = displayed_info["metrics"]
                except Exception:
                    displayed_info = None
                    metrics = None
                if self._closing:
                    return

            if metrics:
                reward = metrics["series"].get("Environment/Cumulative Reward", [])
                widget = self._widget("#reward-chart", Sparkline)
                if widget is not None:
                    widget.data = [p["value"] for p in reward] or [0]
                widget = self._widget("#episode-chart", Sparkline)
                if widget is not None:
                    widget.data = [p["value"] for p in metrics["series"].get("Environment/Episode Length", [])] or [0]
                loss = next((v for k, v in metrics["series"].items() if "loss" in k.lower()), [])
                widget = self._widget("#loss-chart", Sparkline)
                if widget is not None:
                    widget.data = [p["value"] for p in loss] or [0]
                step = metrics.get("step")
                reward_value = metrics.get("mean_reward")
                episode_length = metrics.get("episode_length")
                sps = metrics.get("steps_per_second")
                prefix = "ACTIVE" if displayed_run == active_run else "LAST/SELECTED"
                detail = (
                    f"{prefix} · {displayed_run} · step {step if step is not None else '—'} · "
                    f"reward {reward_value if reward_value is not None else '—'} · "
                    f"episode length {episode_length if episode_length is not None else '—'} · "
                    f"steps/s {f'{sps:.1f}' if isinstance(sps, (int, float)) else '—'}"
                )
                live_metrics = self._widget("#live-metrics", Static)
                if live_metrics is not None:
                    live_metrics.update(detail)
            else:
                step = reward_value = episode_length = sps = None
                live_metrics = self._widget("#live-metrics", Static)
                if live_metrics is not None:
                    live_metrics.update(
                        f"ACTIVE · {active_run} · waiting for first TensorBoard metrics" if active_run else
                        (f"LAST/SELECTED · {displayed_run} · no metrics yet" if displayed_run else "No training runs found yet.")
                    )

            active_card = self._widget("#active-training-card", Static)
            progress = self._widget("#training-progress", ProgressBar)
            training_log = self._widget("#training-log", RichLog)
            if active and active_run:
                active_info = displayed_info if displayed_run == active_run and displayed_info else None
                if active_info is None:
                    try:
                        active_info = await asyncio.to_thread(RunStore(self.project).show, active_run, True)
                    except Exception:
                        active_info = None
                active_metrics = (active_info or {}).get("metrics", {})
                active_step = active_metrics.get("step")
                active_reward = active_metrics.get("mean_reward")
                active_sps = active_metrics.get("steps_per_second")
                active_episode = active_metrics.get("episode_length")
                effective = read_json(self.project.results / active_run / "effective-config.json", {})
                maximum = effective.get("max_steps")
                mode = (active_info or {}).get("mode") or ("Visual" if effective.get("visual") else "HeadlessFast")
                phase = _phase(self.project, active_run, active, active_metrics)
                elapsed = _elapsed_since(active.get("started"))
                eta = None
                if maximum and active_step is not None and active_sps and active_sps > 0:
                    eta = max(0, (maximum - active_step) / active_sps)
                checkpoint_count = (active_info or {}).get("checkpoints", 0)
                final_onnx = (active_info or {}).get("final_onnx", False)
                run_path = str(self.project.results / active_run)
                stage_value = (active_info or {}).get("stage", effective.get("stage", "—"))
                stage_name = next((k for k, v in data["stages"].items() if v == stage_value), str(stage_value))
                usage = active.get("usage") or {}
                resource_line = (
                    f"CPU {usage.get('cpu_percent') or 0:.1f}% · RAM {usage.get('ram_bytes', 0) / 1024**2:.0f} MiB"
                    f" · processes {usage.get('workers', 0)}"
                )
                gpu = usage.get("gpu")
                if gpu:
                    resource_line += (
                        f" · GPU {gpu.get('utilization_percent', 0):.0f}%"
                        f" · VRAM {gpu.get('memory_used_mib', 0):.0f}/{gpu.get('memory_total_mib', 0):.0f} MiB"
                    )
                progress_text = (
                    f"{active_step:,} / {maximum:,}" if isinstance(active_step, int) and isinstance(maximum, int) else
                    (f"waiting / {maximum:,}" if isinstance(maximum, int) else "waiting for metrics")
                )
                preview_text = "WATCH MODE — Unity window" if mode == "Visual" else "FAST MODE — no game window"
                preview_target_line = ""
                if mode == "Visual":
                    try:
                        from ..preview_window import describe_target, load_preview_target
                        target = load_preview_target(self.project.root)
                        if target.get("monitor"):
                            preview_target_line = "\nPreview monitor: " + describe_target(target) + " · maximized, resizable, minimizable"
                    except Exception:
                        pass
                child_names = [str(c.get("name", "")).lower() for c in usage.get("children", [])]
                unity_alive = any("unity" in name or "moyvatraining" in name for name in child_names)
                summary_freq = int(effective.get("summary_freq") or 5000)
                learning_confirmed = isinstance(active_step, int) and active_step > 0
                status_line = _startup_message(phase, elapsed, summary_freq, learning_confirmed)
                learning_line = (
                    f"Learning confirmed: yes — ML-Agents reported {active_step:,} steps."
                    if learning_confirmed else
                    f"Learning confirmed: not yet — the process is alive, but the first metric report has not arrived."
                )
                environment_line = "Unity environment: RUNNING" if unity_alive else "Unity environment: starting / not detected yet"
                process_state = active.get("state", "UNKNOWN")
                started_at = str(active.get("started", "—"))[:19]
                metric_line = (
                    f"Metrics: step {active_step:,} · reward {active_reward if active_reward is not None else '—'} "
                    f"· episode length {active_episode if active_episode is not None else '—'} "
                    f"· speed {f'{active_sps:.1f} steps/s' if isinstance(active_sps, (int, float)) else '—'}"
                    if learning_confirmed else
                    f"Metrics: waiting for first report. Until then the progress bar stays at 0%, especially in fast/headless mode."
                )
                if active_card is not None:
                    active_card.update(
                        f"{status_line}\n"
                        f"{learning_line}\n"
                        f"Run: {active_run} · started {started_at} · process {process_state}\n"
                        f"Phase: {phase} · {environment_line} · Stage: {stage_name} · {preview_text}{preview_target_line}\n"
                        f"{metric_line}\n"
                        f"Target: {progress_text} · ETA: {_duration(eta)}\n"
                        f"Saved models: {checkpoint_count} checkpoints · Final ONNX: {'yes' if final_onnx else 'not yet'}\n"
                        f"Results folder: {run_path}\n"
                        f"{resource_line}"
                    )
                progress_label = self._widget("#training-progress-label", Static)
                if progress_label is not None:
                    if learning_confirmed and maximum:
                        percent = min(100.0, 100.0 * active_step / maximum)
                        progress_label.update(f"{phase}: {percent:.1f}% — {active_step:,} / {maximum:,} steps · elapsed {_duration(elapsed)}")
                    else:
                        progress_label.update(
                            f"{phase}: process is alive for {_duration(elapsed)}. Waiting for first ML-Agents metric report "
                            f"(~{summary_freq:,} steps). The bar stays at 0% until that report arrives."
                        )
                if progress is not None:
                    progress.update(progress=min(100, 100 * (active_step or 0) / maximum) if maximum else 0)
                home_card = self._widget("#home-training-card", Static)
                if home_card is not None:
                    home_card.update(
                        ("TRAINING NOW — metrics confirmed\n" if learning_confirmed else "TRAINING STARTING — no metrics yet\n")
                        + f"{active_run} · {phase} · elapsed {_duration(elapsed)}\n"
                        + f"{stage_name} · {preview_text}\n"
                        + (f"{active_step:,} / {maximum:,} steps · reward {active_reward if active_reward is not None else '—'}" if learning_confirmed and maximum else "Waiting for first step metric…")
                    )
                if training_log is not None:
                    try:
                        recent = await asyncio.to_thread(logs, self.project, active_run, "all")
                        recent_lines = recent.splitlines()[-24:]
                        training_log.clear()
                        training_log.write("\n".join(recent_lines) if recent_lines else "Training started; waiting for log output…")
                    except Exception as error:
                        training_log.clear()
                        training_log.write("Waiting for training log: " + str(error))
                training_state = self._widget("#training-state", Static)
                if training_state is not None:
                    training_state.update(
                        f"{phase} · elapsed {_duration(elapsed)} · process {process_state}. "
                        "Open Show live log to see build/preflight/Unity connection lines. "
                        "Closing the Control Center stops this training by default; press Q to keep it running in the background."
                    )
            else:
                if active_card is not None:
                    if displayed_run:
                        state = (displayed_info or {}).get("state", "UNKNOWN")
                        active_card.update(
                            f"No active training.\nLast/selected run: {displayed_run} · {state}.\n"
                            "Choose a preset above and press Start training or Start with live preview."
                        )
                    else:
                        active_card.update(
                            "No active training.\nChoose FullGame smoke for the first real run, then press Start training. "
                            "Use Start with live preview if you want to watch the Unity game window."
                        )
                progress_label = self._widget("#training-progress-label", Static)
                if progress_label is not None:
                    progress_label.update("No training is running.")
                home_card = self._widget("#home-training-card", Static)
                if home_card is not None:
                    home_card.update("NOT TRAINING\nOpen Training and choose Quick FullGame check for the recommended first run.")
                if progress is not None:
                    progress.update(progress=0)
                if training_log is not None:
                    training_log.clear()
                    training_log.write("No active training process.")
                training_state = self._widget("#training-state", Static)
                if training_state is not None:
                    training_state.update(
                        "Idle. Headless training is faster. Visual preview is slower but opens the Unity training player so you can watch behavior."
                    )

            # Keep actions aligned with actual selection/runtime state.
            self._set_disabled("#stop-training", active is None)
            self._set_disabled("#home-stop", active is None)
            self._set_disabled("#toggle-training-log", active is None)
            self._set_disabled("#stop-job", not any(r["live"] for r in data["processes"]))
            self._set_disabled("#training-resume", self.selected_run is None or active is not None)
            self._set_disabled("#start-training", active is not None)
            self._set_disabled("#start-preview", active is not None)
            self._set_disabled("#active-logs", active is None)
            self._set_disabled("#active-checkpoints", active is None)
            self._set_disabled("#active-results", active is None)
            self._set_disabled("#active-preview", active is None)
            for key in ("show", "resume", "clone", "compare", "logs", "reveal", "delete"):
                self._set_disabled("#run-" + key, self.selected_run is None)
            for key in ("show", "favorite", "label", "compare", "export", "reveal", "test", "delete", "resume-run"):
                self._set_disabled("#checkpoint-" + key, self.selected_checkpoint is None)

            log_source = self._widget("#log-source", Select)
            if log_source is not None:
                text = await asyncio.to_thread(logs, self.project, self.selected_run, str(log_source.value))
                if self._closing:
                    return
                if text != self.last_log:
                    self.last_log = text
                    widget = self._widget("#live-log", RichLog)
                    if widget is not None:
                        widget.clear()
                        widget.write(text)
            self.first_load = False
        except asyncio.CancelledError:
            return
        except Exception as error:
            if self._closing:
                return
            dashboard = self._widget("#dashboard-info", Static)
            if dashboard is not None:
                dashboard.update("Environment needs attention: " + str(error) + "\nUse Environment → Setup / Repair.")
            else:
                # The widget tree is gone; this is a lifecycle event, not a fatal app error.
                return
        finally:
            self.refreshing = False

    def update_table(self, identifier, rows):
        widget = self._widget("#" + identifier, DataTable)
        if widget is None:
            return
        index = widget.cursor_row
        widget.clear()
        for key, values in rows:
            widget.add_row(*values, key=key)
        if rows:
            widget.move_cursor(row=min(index, len(rows) - 1))

    @on(DataTable.RowSelected)
    def selected(self, event):
        key = str(event.row_key.value)
        if event.data_table.id == "runs-table":
            self.selected_run = key
            detail = self._widget("#run-detail", Static)
            if detail is not None:
                detail.update(key + " selected. Details show metadata, elapsed state and structured metrics.")
            self._set_disabled("#training-resume", False)
            for action in ("show", "resume", "clone", "compare", "logs", "reveal", "delete"):
                self._set_disabled("#run-" + action, False)
        elif event.data_table.id == "checkpoints-table":
            self.selected_checkpoint = key
            detail = self._widget("#checkpoint-detail", Static)
            if detail is not None:
                detail.update(key)
            for action in ("show", "favorite", "label", "compare", "export", "reveal", "test", "delete", "resume-run"):
                self._set_disabled("#checkpoint-" + action, False)
        else:
            self.selected_job = key
            self._set_disabled("#stop-job", False)

    @work(group="operations")
    async def invoke(self, arguments, show=True):
        try:
            result = await asyncio.to_thread(dispatch, self.project, parser().parse_args(arguments))
            if self._closing:
                return
            text = result if isinstance(result, str) else json.dumps(result, indent=2, ensure_ascii=False)
            for identifier in ("environment-output", "test-output"):
                widget = self._widget("#" + identifier, RichLog)
                if widget is not None:
                    widget.write(text)
            if show:
                self._safe_push(Details("Moyva · " + " ".join(arguments[:2]), result))
            self._safe_notify("Operation returned. See status and logs for background jobs.")
            self._safe_refresh()
        except asyncio.CancelledError:
            return
        except Exception as error:
            if not self._closing:
                self._safe_push(Details("Operation blocked", str(error)))

    @work(group="training-start")
    async def start(self, preset, run_id, profile, preview_target=None):
        try:
            from ..training import start_training

            card = self._widget("#active-training-card", Static)
            if card is not None:
                card.update(
                    f"STARTING  {run_id}\n"
                    "Moyva is running preflight. If the training player is stale it will be rebuilt automatically, "
                    "then ML-Agents will launch."
                )
            result = await asyncio.to_thread(start_training, self.project, preset, run_id, False, profile)
            if self._closing:
                return
            self.selected_run = run_id
            self.selected_job = result.get("token") if isinstance(result, dict) else None
            if preview_target:
                try:
                    from ..preview_window import describe_target, launch_preview_watcher
                    await asyncio.to_thread(launch_preview_watcher, self.project.root, 180)
                    self._safe_notify(
                        "Visual preview is starting on " + describe_target(preview_target)
                        + ". It will be maximized there but remains a normal resizable/minimizable window."
                    )
                except Exception as preview_error:
                    self._safe_notify("Training started, but automatic preview placement failed: " + str(preview_error), severity="warning")
            else:
                self._safe_notify(
                    "Training task started. Watch Active training below; startup metrics may take a short time to appear."
                )
            self.action_page("training")
            self._safe_refresh()
        except asyncio.CancelledError:
            return
        except Exception as error:
            if not self._closing:
                self._safe_push(Details("Training blocked", str(error)))

    @on(Button.Pressed)
    def button(self, event):
        key = event.button.id or ""
        try:
            if key == "goto-training":
                self.action_page("training")
            elif key == "home-logs":
                active_run = next((r.get("run_id") for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
                if active_run:
                    self.selected_run = active_run
                self.action_page("logs")
                self._safe_refresh()
            elif key == "home-stop":
                token = next((r.get("token") for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
                if not token:
                    raise ControlError("No active training run.")
                self.invoke(["stop", token], show=False)
            elif key == "toggle-advanced":
                panel = self._widget("#training-advanced", Container)
                if panel is not None:
                    panel.display = not panel.display
                    event.button.label = "Hide advanced settings" if panel.display else "Advanced settings"
            elif key == "toggle-training-log":
                panel = self._widget("#training-log-panel", Container)
                if panel is not None:
                    panel.display = not panel.display
                    event.button.label = "Hide live log" if panel.display else "Show live log"
            elif key in ("doctor", "environment-doctor"):
                self.invoke(["doctor"])
            elif key == "doctor-fix":
                self.invoke(["doctor", "--fix"])
            elif key == "setup":
                if not self._closing:
                    self.push_screen(
                        Prompt("Create/repair the project venv and install repository requirements? Type setup.", expected="setup"),
                        lambda value: self.invoke(["setup"]) if value else None,
                    )
            elif key in ("review", "preflight", "dashboard-preflight", "start-training", "start-preview", "save-preset", "duplicate-preset", "reset-preset"):
                preset = self.form()
                name = str(self.query_one("#preset", Select).value)
                if key == "review":
                    self._safe_push(Details("Effective training settings", preset))
                elif key in ("preflight", "dashboard-preflight"):
                    self.run_preflight(preset, str(self.query_one("#profile", Select).value))
                elif key in ("start-training", "start-preview"):
                    preview_target = None
                    if key == "start-preview":
                        from ..preview_window import describe_target, prepare_preview_target
                        preview_target = prepare_preview_target(self.project.root)
                        monitor = preview_target.get("monitor") or {}
                        preset = {
                            **preset,
                            "visual": True,
                            "time_scale": 1.0,
                            "screen_width": int(monitor.get("width") or max(960, int(preset.get("screen_width", 1280)))),
                            "screen_height": int(monitor.get("height") or max(540, int(preset.get("screen_height", 720)))),
                        }
                        if preview_target.get("automatic_placement"):
                            self._safe_notify(
                                "Preview target: " + describe_target(preview_target)
                                + ". The window will open maximized on the monitor that is not showing Control Center."
                            )
                        else:
                            self._safe_notify(
                                "Preview will open as a normal resizable window. Automatic second-monitor placement is unavailable; "
                                "install wmctrl/xdotool on Linux X11 for automatic placement.",
                                severity="warning",
                            )
                    else:
                        preset = {**preset, "visual": False}
                    self.start(
                        preset,
                        self.query_one("#field-run_id", Input).value or time.strftime("moyva-%Y%m%d-%H%M%S"),
                        str(self.query_one("#profile", Select).value),
                        preview_target,
                    )
                elif key in ("save-preset", "duplicate-preset"):
                    self.push_screen(
                        Prompt("Name for the local preset", name + "-custom"),
                        lambda value: self.save_preset(value, preset) if value else None,
                    )
                else:
                    Presets(self.project).reset(name)
                    self.load_preset(name)
            elif key in ("stop-job", "stop-training"):
                token = (
                    self.selected_job
                    if key == "stop-job"
                    else next((r["token"] for r in self.snapshot.get("processes", []) if r["live"] and r["kind"] == "train"), None)
                )
                if not token:
                    raise ControlError("Select an owned running task first.")
                self.invoke(["stop", token])
            elif key == "training-refresh":
                self._safe_refresh()
            elif key == "active-logs":
                active_run = next((r.get("run_id") for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
                if not active_run:
                    raise ControlError("No active training run.")
                self.selected_run = active_run
                self.action_page("logs")
                self._safe_refresh()
            elif key == "active-checkpoints":
                active_run = next((r.get("run_id") for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
                if not active_run:
                    raise ControlError("No active training run.")
                self.selected_run = active_run
                candidates = [c for c in self.snapshot.get("checkpoints", []) if c.get("run_id") == active_run]
                if candidates:
                    latest = max(candidates, key=lambda c: c.get("created", 0))
                    self.selected_checkpoint = latest.get("relative")
                self.action_page("checkpoints")
                self._safe_refresh()
            elif key == "active-results":
                active_run = next((r.get("run_id") for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
                if not active_run:
                    raise ControlError("No active training run.")
                self.invoke(["run", "reveal", active_run], show=False)
            elif key == "active-preview":
                active_run = next((r.get("run_id") for r in self.snapshot.get("processes", []) if r.get("live") and r.get("kind") == "train"), None)
                if not active_run:
                    raise ControlError("No active training run.")
                try:
                    info = RunStore(self.project).show(active_run, False)
                except Exception:
                    info = {}
                if info.get("mode") == "Visual":
                    focused = self._focus_preview_window()
                    self._safe_notify(
                        "Preview moved to its target monitor and maximized. You can still minimize, restore and resize it." if focused else
                        "Preview is running, but automatic window placement is unavailable. Use Alt+Tab to select MoyvaTraining."
                    )
                else:
                    self._safe_push(Details("No preview for fast mode", "This run is headless. Stop it and press 'Start & watch' to start with a visible maximized Unity window."))
            elif key == "tensorboard":
                self.invoke(["tensorboard", "open"])
            elif key.startswith("unity-"):
                self.invoke(["unity", key[6:]])
            elif key in ("build-training", "rebuild-training"):
                self.invoke(["build", "training", "--background"] + (["--rebuild"] if key == "rebuild-training" else []))
            elif key.startswith("test-"):
                self.invoke(["test", key[5:], "--background"])
            elif key == "disk-usage":
                self.invoke(["disk"])
            elif key in ("clean-temp", "clean-build"):
                area = "temp-ai" if key == "clean-temp" else "training-build"
                self.push_screen(
                    Prompt("Delete the selected project output?", expected=area),
                    lambda value: self.invoke(["clean", area, "--confirm", value]) if value else None,
                )
            elif key == "save-settings":
                self.project.save_settings(json.loads(self.query_one("#settings-json", TextArea).text))
                self.project = Project(self.project.root)
                self._safe_notify("Local settings saved.")
            elif key.startswith("run-") or key == "training-resume":
                if not self.selected_run:
                    raise ControlError("Select a run first.")
                action = "resume" if key == "training-resume" else key[4:]
                run = self.selected_run
                if action == "delete":
                    self.push_screen(
                        Prompt("Permanently delete run and checkpoints?", expected=run),
                        lambda value: self.invoke(["run", "delete", run, "--confirm", value]) if value else None,
                    )
                elif action in ("compare", "clone"):
                    self.push_screen(
                        Prompt("Other run id" if action == "compare" else "New preset name"),
                        lambda value: self.invoke(["run", action, run, value]) if value else None,
                    )
                elif action == "logs":
                    self.action_page("logs")
                    self._safe_refresh()
                else:
                    self.invoke(["run", action, run])
            elif key.startswith("checkpoint-"):
                if not self.selected_checkpoint:
                    raise ControlError("Select a checkpoint first.")
                action = key[11:]
                path = self.selected_checkpoint
                if action == "delete":
                    self.push_screen(
                        Prompt("Permanently delete this checkpoint?", expected=path),
                        lambda value: self.invoke(["checkpoint", "delete", path, "--confirm", value]) if value else None,
                    )
                elif action in ("label", "compare", "export"):
                    self.push_screen(
                        Prompt(
                            {
                                "label": "Logical label",
                                "compare": "Other checkpoint relative path",
                                "export": "Destination .onnx path (existing files are refused)",
                            }[action]
                        ),
                        lambda value: self.invoke(["checkpoint", action, path, value]) if value else None,
                    )
                elif action == "resume-run":
                    self.invoke(["run", "resume", RunStore(self.project).checkpoint(path)["run_id"]])
                else:
                    self.invoke(["checkpoint", action, path])
        except Exception as error:
            self._safe_push(Details("Check your selection", str(error)))

    def _focus_preview_window(self):
        try:
            from ..preview_window import place_preview
            return bool(place_preview(self.project.root, activate=True))
        except Exception:
            return False

    def save_preset(self, name, preset):
        try:
            Presets(self.project).save(name, preset)
            selector = self._widget("#preset", Select)
            if selector is not None:
                selector.set_options([(n, n) for n in Presets(self.project).list()])
                selector.value = name
            self._safe_notify("Local preset saved.")
        except Exception as error:
            self._safe_push(Details("Preset rejected", str(error)))

    @work(group="preflight")
    async def run_preflight(self, preset, profile):
        from ..training import preflight

        try:
            card = self._widget("#active-training-card", Static)
            if card is not None and not any(r.get("live") and r.get("kind") == "train" for r in self.snapshot.get("processes", [])):
                card.update("PREFLIGHT\nChecking environment, contract, disk, training player and FullGame readiness. Safe stale-build repairs are enabled.")
            result = await asyncio.to_thread(preflight, self.project, preset, profile, True)
            if not self._closing:
                self._safe_push(Details("Preflight", result))
        except asyncio.CancelledError:
            return
        except Exception as error:
            if not self._closing:
                self._safe_push(Details("Preflight blocked", str(error)))
