import contextlib
import io
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import time
import unittest
from unittest.mock import patch

sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
import moyva_train
from moyva_cli.config import ROOT,ControlError,Project,atomic_json,contained,simple_name
from moyva_cli.environment import HostPlatform,choose_python,python_info
from moyva_cli.diagnostics import classify,disk_state,repair_plan
from moyva_cli.presets import Presets
from moyva_cli.runs import RunStore
from moyva_cli.processes import identity,same_process,Supervisor
from moyva_cli.training import legacy_arguments
from moyva_cli.commands import parser

class ProjectFixture(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup)
        self.root=Path(self.temp.name)
        for relative in ["tools/ai/moyva-cli.json","Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json",
                "Assets/Moyva/AI/Bot/Core/Contracts/BotDecisionContract.cs",
                "Assets/Moyva/AI/Training/Runtime/Curriculum/TrainingCurriculumConfig.cs",
                "Assets/Moyva/AI/Training/Config/moyva_ppo.yaml"]:
            target=self.root/relative;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/relative,target)
        self.project=Project(self.root);self.store=RunStore(self.project)
    def run_fixture(self,name="old-run",compatible=True):
        path=self.store.path(name);path.mkdir(parents=True)
        contract=self.project.contract()
        if not compatible:contract["hash"]="wrong"
        atomic_json(path/"run.json",dict(run_id=name,contract=contract,stage=8,seed=5,world_size=24))
        (path/"MoyvaStrategy").mkdir();(path/"MoyvaStrategy/MoyvaStrategy-100.pt").write_bytes(b"checkpoint")
        (path/"MoyvaStrategy.onnx").write_bytes(b"model")
        shutil.copy2(ROOT/"Assets/Moyva/AI/Training/Config/moyva_ppo.yaml",path/"trainer.yaml")
        return path
    def test_existing_run_is_resumable_not_assumed_completed(self):
        self.run_fixture();run=self.store.show("old-run",False)
        self.assertEqual("RESUMABLE",run["state"]);self.assertTrue(run["resumable"]);self.assertTrue(run["final_onnx"])
    def test_exit_record_is_evidence_of_completion(self):
        path=self.run_fixture();atomic_json(path/"cli-status.json",{"state":"COMPLETED","exit_code":0})
        self.assertEqual("COMPLETED",self.store.show("old-run",False)["state"])
    def test_run_failure_details_are_visible(self):
        path=self.run_fixture();atomic_json(path/"cli-status.json",{"state":"FAILED","exit_code":3})
        atomic_json(path/"failure.json",{"id":"training-ended-before-first-metrics","component":"ML-Agents trainer","repair":"Trainer stopped before first metrics."})
        run=self.store.show("old-run",False)
        self.assertEqual("FAILED",run["state"])
        self.assertEqual("training-ended-before-first-metrics",run["failure"]["id"])
        self.assertEqual("Trainer stopped before first metrics.",run["state_evidence"])
    def test_incompatible_checkpoint_is_prominent_and_export_blocked(self):
        self.run_fixture(compatible=False)
        self.assertEqual("INCOMPATIBLE",self.store.show("old-run",False)["state"])
        with self.assertRaises(ControlError):self.store.export("old-run/MoyvaStrategy.onnx","model.onnx")
    def test_checkpoint_metadata_and_final_detection(self):
        self.run_fixture();items=self.store.checkpoints()
        self.assertEqual(2,len(items));self.assertEqual(100,next(c["step"] for c in items if c["kind"]=="pt"))
        self.assertTrue(next(c["final"] for c in items if c["kind"]=="onnx"))
    def test_corrupt_run_metadata_is_not_silently_successful(self):
        path=self.run_fixture();(path/"run.json").write_text("not json")
        self.assertEqual("FAILED",self.store.list(False)[0]["state"])
    def test_presets_roundtrip_and_reset(self):
        presets=Presets(self.project);preset=presets.get("fullgame");preset["seed"]=22
        presets.save("mine",preset);self.assertEqual(22,presets.get("mine")["seed"])
        self.assertEqual(8,preset["stage"]);presets.reset("mine");self.assertNotIn("mine",presets.list())
    def test_presets_reject_unknown_stage(self):
        presets=Presets(self.project);preset=presets.get("smoke");preset["stage"]=90
        with self.assertRaises(ControlError):presets.save("bad",preset)
    def test_command_reuses_launcher_parser_and_keeps_contract(self):
        preset=Presets(self.project).get("fullgame");before=self.project.contract()
        args=legacy_arguments(self.project,preset,"run-1")
        self.assertEqual(8,args.stage);self.assertEqual("train",args.command);self.assertFalse(args.force)
        self.assertEqual(preset["checkpoint_interval"],args.checkpoint_interval)
        self.assertEqual(before,self.project.contract())
    def test_clone_preserves_original(self):
        path=self.run_fixture();original=(path/"run.json").read_bytes()
        Presets(self.project).clone_run("old-run","clone")
        self.assertEqual(original,(path/"run.json").read_bytes());self.assertEqual(8,Presets(self.project).get("clone")["stage"])
    def test_delete_requires_exact_confirmation(self):
        path=self.run_fixture()
        with self.assertRaises(ControlError):self.store.delete("old-run",confirmation="yes")
        self.assertTrue(path.exists())
        self.store.delete("old-run",confirmation="old-run");self.assertFalse(path.exists())
    def test_export_does_not_overwrite(self):
        self.run_fixture();destination=self.root/"model.onnx";destination.write_bytes(b"original")
        with self.assertRaises(ControlError):self.store.export("old-run/MoyvaStrategy.onnx",str(destination))
        self.assertEqual(b"original",destination.read_bytes())
    def test_favorites_are_logical_not_file_renames(self):
        path=self.run_fixture();self.store.label("old-run/MoyvaStrategy.onnx",label="candidate",favorite=True)
        self.assertTrue((path/"MoyvaStrategy.onnx").exists());self.assertTrue(self.store.checkpoint("old-run/MoyvaStrategy.onnx")["favorite"])
    def test_path_traversal_and_symlinks(self):
        for name in ("../outside","..","/tmp/escape",""):
            with self.assertRaises(ControlError):self.store.path(name)
        outside=self.root/"outside";outside.mkdir();self.project.results.mkdir(parents=True)
        try:(self.project.results/"link").symlink_to(outside,target_is_directory=True)
        except OSError:self.skipTest("Symlink creation is unavailable")
        with self.assertRaises(ControlError):self.store.path("link")
    def test_atomic_json_handles_unicode_and_replacement(self):
        target=self.root/"дані.json";atomic_json(target,{"x":1});atomic_json(target,{"x":"тест"})
        self.assertEqual("тест",json.loads(target.read_text())["x"])
    def test_platform_executable_layouts(self):
        self.assertEqual("MoyvaTraining.exe",HostPlatform("Windows").player(self.root).name)
        self.assertEqual("MoyvaTraining.app",HostPlatform("Darwin").player(self.root).name)
        self.assertEqual("bin/python",str(HostPlatform("Linux").python(Path("."))))
    def test_missing_or_broken_venv_falls_back_cleanly(self):
        with patch.dict(os.environ,{},clear=False):
            self.project.settings.pop("python",None)
            self.assertTrue(choose_python(self.project))
        self.assertEqual(((),False),python_info(self.root/"absent-python"))
    def test_disk_thresholds(self):
        self.assertEqual("BLOCKED",disk_state(1024**3,2,10));self.assertEqual("WARNING",disk_state(5*1024**3,2,10));self.assertEqual("READY",disk_state(20*1024**3,2,10))
    def test_failure_classification_preserves_evidence(self):
        result=classify("Earlier output\nPlayer contract is missing or stale. Rebuild.")
        self.assertEqual("contract",result["id"]);self.assertIn("stale",result["evidence"][0])
    def test_early_stop_before_first_metrics_is_reported_as_failure(self):
        run=self.root/"early";(run/"MoyvaStrategy").mkdir(parents=True)
        (run/"mlagents.log").write_text("[INFO] Learning was interrupted. Please wait while the graph is generated.\\n[ERROR] SubprocessEnvManager had workers that didn't signal shutdown\\n")
        (run/"unity.log").write_text("READY_FOR_REAL_TRAINING\\n")
        failure=moyva_train.early_stop_failure(run,{"max_steps":10000,"summary_freq":500})
        self.assertEqual("training-ended-before-first-metrics",failure["id"])
        self.assertEqual(0,failure["step"])
    def test_metric_step_is_not_reported_as_early_stop(self):
        run=self.root/"healthy";run.mkdir()
        (run/"mlagents.log").write_text("MoyvaStrategy. Step: 500. Time Elapsed: 1. Mean Reward: 0.1\\n")
        (run/"unity.log").write_text("READY_FOR_REAL_TRAINING\\n")
        self.assertIsNone(moyva_train.early_stop_failure(run,{"max_steps":10000,"summary_freq":500}))
    def test_repair_plan_never_auto_deletes_lock(self):
        report={"checks":[{"state":"WARNING","repair":"stale-lock","message":"stale"},{"state":"BLOCKED","repair":"setup","message":"missing"}]}
        self.assertEqual(["CONFIRMATION_REQUIRED","SAFE"],[r["category"] for r in repair_plan(report)])
    def test_stop_rejects_forged_identity_and_leaves_unrelated_child_alive(self):
        child=subprocess.Popen([sys.executable,"-c","import time;time.sleep(30)"])
        self.addCleanup(lambda: child.poll() is None and child.terminate())
        self.addCleanup(lambda: None)
        record=identity(child.pid);self.assertTrue(same_process(record))
        record.update(created=record["created"]-1,project=str(self.project.root),token="fake")
        atomic_json(self.project.local/"processes/fake.json",record)
        with self.assertRaises(ControlError):Supervisor(self.project).stop("fake")
        self.assertIsNone(child.poll());child.terminate();child.wait(timeout=5)
    def test_cli_command_coverage(self):
        for command in ["status","setup","doctor --fix","unity play","build training","test fullgame","train --preset fullgame --dry-run","run resume run-1","checkpoint favorite run-1/model.onnx","tensorboard stop --token abc","settings"]:
            self.assertIsNotNone(parser().parse_args(command.split()).command)

try:
    import textual
except ImportError:textual=None

@unittest.skipIf(textual is None,"Install requirements-cli.txt for TUI tests")
class TerminalTests(unittest.IsolatedAsyncioTestCase):
    async def test_app_starts_navigation_and_preset_selection(self):
        from moyva_cli.tui.app import ControlCenter
        from textual.widgets import ContentSwitcher,Select,Input
        from unittest.mock import AsyncMock
        app=ControlCenter(Project())
        # Deterministic presentation test; external probing has independent service tests.
        with patch.object(ControlCenter,"refresh_data",lambda self:None):
            async with app.run_test(size=(140,46)) as pilot:
                await pilot.press("2");await pilot.pause()
                self.assertEqual("training",app.query_one("#pages",ContentSwitcher).current)
                app.query_one("#preset",Select).value="fullgame"
                await pilot.pause();self.assertEqual(8,app.form()["stage"])
                app.query_one("#field-run_id",Input).value="review-only"
                app.query_one("#review").press()
                await pilot.pause()
                self.assertGreater(len(app.screen_stack),1)
                await pilot.click("#details-close");await pilot.press("3");await pilot.pause()
                self.assertEqual("runs",app.query_one("#pages",ContentSwitcher).current)

class CurriculumPreviewTests(unittest.TestCase):
    setUp = ProjectFixture.setUp
    def test_castle_and_parallel_presets_reach_canonical_launcher(self):
        presets=Presets(self.project)
        preset=presets.get("castle-first")
        preset.update(arenas=4,initialize_from="castle-lesson")
        args=legacy_arguments(self.project,preset,"next-lesson")
        self.assertEqual(4,args.arenas)
        self.assertEqual("castle-lesson",args.initialize_from)
        self.assertTrue(args.learn_initial_castle)
        self.assertEqual(5,args.stage)
        preset["stage"]=1
        with self.assertRaises(ControlError):presets.validate(preset)

    def test_mosaic_has_non_overlapping_cells(self):
        from moyva_cli.preview_window import arena_rectangles
        rects=arena_rectangles(dict(x=100,y=20,width=1920,height=1080),4)
        self.assertEqual(4,len(rects))
        self.assertEqual(4,len({(r["x"],r["y"]) for r in rects}))
        self.assertTrue(all(r["width"]==960 and r["height"]==540 for r in rects))
