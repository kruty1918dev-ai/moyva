import io
import json
import os
from pathlib import Path
import re
import sys
import tempfile
import threading
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import moyva_train
from moyva_train import LaunchError, _inspect_scenario, _validate_arenas, contract, contract_signature
from moyva_cli.config import ControlError, ROOT, atomic_json, read_json
from moyva_cli.commands import _log_line_matches, follow_log_files, logs, parser
from moyva_cli.evaluation import choose_evaluation_scenario, curriculum_order

SPEC_PATH = ROOT / "Assets/Moyva/Presets/AI/Resources/MoyvaBotContract.json"
CS_PATH = ROOT / "Assets/Moyva/AI/Bot/Core/Contracts/BotDecisionContract.cs"


class ContractSpecTests(unittest.TestCase):
    def setUp(self):
        self.spec = json.loads(SPEC_PATH.read_text(encoding="utf-8-sig"))

    def test_global_slot_indices_are_unique(self):
        """Regression: tactical metrics once collided with the goal vector at 34-36."""
        covered = {}
        for feature in self.spec["global"]:
            count = feature.get("count", 1)
            for slot in range(feature["index"], feature["index"] + count):
                self.assertNotIn(slot, covered, f"slot {slot} claimed by {covered.get(slot)} and {feature['name']}")
                covered[slot] = feature["name"]

    def test_goals_and_tactical_slots_do_not_overlap(self):
        names = {f["name"]: f["index"] for f in self.spec["global"]}
        self.assertEqual(34, names["goalCastle"])
        self.assertEqual(42, names["goalWin"])
        self.assertEqual(43, names["tacticalVision"])
        self.assertEqual(45, names["tacticalHeight"])

    def test_feature_names_are_unique(self):
        names = [f["name"] for f in self.spec["global"]]
        self.assertEqual(len(names), len(set(names)))

    def test_contract_signature_format(self):
        spec = {"contractVersion": 2, "observationSchemaVersion": 3, "candidateSchemaVersion": 2,
                "actionSchemaVersion": 1, "maxCandidateSlots": 4, "globalFeatureCount": 8,
                "candidateFeatureCount": 2, "spatialSize": 2, "spatialChannels": 1,
                "global": [{"index": 0, "name": "a"}, {"index": 1, "name": "b", "count": 3}],
                "candidate": ["x"], "intents": ["None"]}
        self.assertEqual("MoyvaBot:v2:o3:c2:a1:slots4:global8:spatial2x2x1:candidate2:"
                         "global=0a,1b3:candidate=x:intents=None", contract_signature(spec))

    def test_contract_reads_shared_json_spec(self):
        result = contract(ROOT)
        self.assertEqual(self.spec["contractVersion"], result["version"])
        self.assertRegex(result["hash"], r"^[0-9a-f]{64}$")
        self.assertEqual(self.spec["maxCandidateSlots"], result["candidateSlots"])
        spatial = self.spec["spatialSize"] ** 2 * self.spec["spatialChannels"]
        expected = self.spec["globalFeatureCount"] + spatial + self.spec["maxCandidateSlots"] * self.spec["candidateFeatureCount"]
        self.assertEqual(expected, result["observations"])

    def test_contract_rejects_missing_spec(self):
        with tempfile.TemporaryDirectory() as temp:
            with self.assertRaises(LaunchError):
                contract(Path(temp))

    def test_csharp_constants_match_spec(self):
        """The C# drift guard mirrors this: constants must equal the JSON spec."""
        source = CS_PATH.read_text(encoding="utf-8-sig")
        constants = dict(re.findall(r"(?:const|public const)\s+int\s+(\w+)\s*=\s*(\d+)", source))
        constants = {k: int(v) for k, v in
                     re.findall(r"\b(\w+)\s*=\s*(\d+)", source.split("BotObservationSchema")[0])}
        mapping = {
            "ContractVersion": "contractVersion", "ObservationSchemaVersion": "observationSchemaVersion",
            "CandidateSchemaVersion": "candidateSchemaVersion", "ActionSchemaVersion": "actionSchemaVersion",
            "MaxCandidateSlots": "maxCandidateSlots", "GlobalFeatureCount": "globalFeatureCount",
            "CandidateFeatureCount": "candidateFeatureCount", "SpatialSize": "spatialSize",
            "SpatialChannels": "spatialChannels",
        }
        for csharp, field in mapping.items():
            self.assertEqual(self.spec[field], constants[csharp],
                             f"{csharp} drifted from spec field {field}")

    def test_csharp_slot_constants_match_spec(self):
        """BotObservationSchema numeric constants must equal spec slot indices."""
        source = CS_PATH.read_text(encoding="utf-8-sig")
        schema = source.split("BotObservationSchema")[1]
        constants = {name.lower(): int(value) for name, value in
                     re.findall(r"\b(\w+)\s*=\s*(\d+)", schema)}
        names = {feature["name"].lower(): feature["index"] for feature in self.spec["global"]}
        for name, index in names.items():
            self.assertIn(name, constants, f"spec slot {name} missing from BotObservationSchema")
            self.assertEqual(index, constants[name], f"slot {name} drifted")


class ArenaGuardTests(unittest.TestCase):
    def autonomous(self, enabled=True):
        return {"enabled": enabled}

    def test_single_arena_always_allowed(self):
        _validate_arenas(1, self.autonomous(enabled=True))
        _validate_arenas(1, self.autonomous(enabled=False))

    def test_multi_arena_rejected_with_autonomous(self):
        with self.assertRaises(LaunchError):
            _validate_arenas(8, self.autonomous(enabled=True))

    def test_multi_arena_allowed_without_autonomous(self):
        _validate_arenas(8, self.autonomous(enabled=False))

    def test_arena_range_enforced(self):
        for arenas in (0, -1, 17):
            with self.assertRaises(LaunchError):
                _validate_arenas(arenas, self.autonomous(enabled=False))


class ScenarioManifestTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.directory = self.root / "Assets/Moyva/Presets/AI/Scenarios"
        self.directory.mkdir(parents=True)

    def write_manifest(self, entries, version=1):
        atomic_json(self.directory / "manifest.json", {"version": version, "curriculum": entries})

    def test_curriculum_order_reads_manifest(self):
        self.write_manifest(["castle", "production", "full-game"])
        self.assertEqual(["castle", "production", "full-game"], curriculum_order(self.root))

    def test_curriculum_order_empty_without_manifest(self):
        self.assertEqual([], curriculum_order(self.root))

    def test_curriculum_order_rejects_unknown_version(self):
        self.write_manifest(["castle"], version=2)
        self.assertEqual([], curriculum_order(self.root))

    def test_curriculum_order_honors_scenario_dir_override(self):
        custom = self.root / "custom"
        custom.mkdir()
        atomic_json(custom / "manifest.json", {"version": 1, "curriculum": ["other"]})
        self.write_manifest(["castle"])
        with patch.dict(os.environ, {"MOYVA_SCENARIO_DIR": str(custom)}):
            self.assertEqual(["other"], curriculum_order(self.root))

    def test_evaluation_scenario_prefers_active(self):
        state = self.root / "state.json"
        atomic_json(state, {"activeScenarioId": "production", "skills": []})
        self.assertEqual("production", choose_evaluation_scenario(state, self.root))

    def test_evaluation_scenario_first_unmastered_in_manifest_order(self):
        self.write_manifest(["castle", "production", "full-game"])
        state = self.root / "state.json"
        atomic_json(state, {"skills": [
            {"scenarioId": "production", "mastered": False},
            {"scenarioId": "castle", "mastered": True},
        ]})
        self.assertEqual("production", choose_evaluation_scenario(state, self.root))

    def test_evaluation_scenario_capstone_when_all_mastered(self):
        self.write_manifest(["castle", "production", "full-game"])
        state = self.root / "state.json"
        atomic_json(state, {"skills": [{"scenarioId": s, "mastered": True}
                                       for s in ("castle", "production", "full-game")]})
        self.assertEqual("full-game", choose_evaluation_scenario(state, self.root))

    def test_evaluation_scenario_errors_when_nothing_known(self):
        state = self.root / "state.json"
        atomic_json(state, {"skills": []})
        with self.assertRaises(ControlError):
            choose_evaluation_scenario(state, self.root)

    def test_inspect_scenario_defaults_to_manifest_first(self):
        self.write_manifest(["castle", "production"])
        self.assertEqual("castle", _inspect_scenario(None, self.root))
        self.assertEqual("production", _inspect_scenario("production", self.root))

    def test_inspect_scenario_requires_manifest_without_choice(self):
        with self.assertRaises(LaunchError):
            _inspect_scenario(None, self.root)


class LogFollowTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.path = self.root / "run.log"

    def collect(self, files, source="all", writes=None, poll=0.05, duration=0.8):
        received = []
        self.path.write_text("initial\n", encoding="utf-8")
        def writer():
            for delay, text in (writes or []):
                threading.Event().wait(delay)
                with self.path.open("a", encoding="utf-8") as stream:
                    stream.write(text)
        thread = threading.Thread(target=writer)
        thread.start()
        def interrupt():
            threading.Event().wait(duration)
            import _thread
            _thread.interrupt_main()
        threading.Thread(target=interrupt).start()
        buffer = io.StringIO()
        try:
            with patch("sys.stdout", buffer):
                follow_log_files(files, source, poll=poll)
        except KeyboardInterrupt:
            pass
        thread.join()
        return buffer.getvalue().splitlines()

    def test_line_filter(self):
        self.assertTrue(_log_line_matches("a Warning: x", "warning"))
        self.assertFalse(_log_line_matches("a info", "warning"))
        self.assertTrue(_log_line_matches("NullReferenceException", "error"))
        self.assertTrue(_log_line_matches("an Error", "error"))
        self.assertFalse(_log_line_matches("an info", "error"))
        self.assertTrue(_log_line_matches("anything", "all"))

    def test_follow_prints_appended_lines(self):
        lines = self.collect([self.path], writes=[(0.1, "new line\n")])
        self.assertIn("[run.log] initial", lines)
        self.assertIn("[run.log] new line", lines)

    def test_follow_buffers_partial_lines(self):
        lines = self.collect([self.path], writes=[(0.1, "part"), (0.2, " whole\n")])
        self.assertIn("[run.log] part whole", lines)
        self.assertNotIn("[run.log] part", lines)

    def test_follow_resets_on_truncation(self):
        def truncating():
            threading.Event().wait(0.1)
            self.path.write_text("rotated\n", encoding="utf-8")
        received = []
        threading.Thread(target=truncating).start()
        def interrupt():
            threading.Event().wait(0.6)
            import _thread
            _thread.interrupt_main()
        threading.Thread(target=interrupt).start()
        buffer = io.StringIO()
        try:
            with patch("sys.stdout", buffer):
                follow_log_files([self.path], "all", poll=0.05)
        except KeyboardInterrupt:
            pass
        lines = buffer.getvalue().splitlines()
        self.assertIn("[run.log] rotated", lines)

    def test_follow_skips_missing_files(self):
        missing = self.root / "absent.log"
        lines = self.collect([missing, self.path], writes=[(0.1, "present\n")])
        self.assertIn("[run.log] present", lines)

    def test_follow_filters_by_source(self):
        lines = self.collect([self.path], source="error",
                             writes=[(0.1, "plain info\nreal Error happened\n")])
        self.assertIn("[run.log] real Error happened", lines)
        self.assertNotIn("[run.log] plain info", lines)

    def test_parser_accepts_follow_flag(self):
        args = parser().parse_args(["logs", "--follow"])
        self.assertTrue(args.follow)
        args = parser().parse_args(["logs", "run-1", "-f", "--source", "unity"])
        self.assertTrue(args.follow)
        self.assertEqual("unity", args.source)


if __name__ == "__main__":
    unittest.main()
