import json
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from moyva_cli.config import ControlError
from moyva_cli.evaluation import (
    EvaluationStore,
    checkpoint_identity,
    evaluation_environment,
    evaluation_player_path,
    evaluation_seed_base,
    held_out_seeds,
    run_frozen_evaluation,
    snapshot_frozen_checkpoint,
    training_seeds,
    update_curriculum_latest_checkpoint,
    verify_resume_checkpoint,
)


class FrozenEvaluationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.run = self.root / "run-1"
        behavior = self.run / "MoyvaStrategy"
        behavior.mkdir(parents=True)
        self.pt = behavior / "MoyvaStrategy-10000.pt"
        self.onnx = behavior / "MoyvaStrategy-10000.onnx"
        self.pt.write_bytes(b"exact-pytorch-checkpoint")
        self.onnx.write_bytes(b"official-mlagents-onnx-export")
        self.contract = "a" * 64
        self.identity = snapshot_frozen_checkpoint(self.run, 10000, self.contract)

    def test_50_held_out_seeds_are_deterministic(self):
        first = held_out_seeds(self.contract, 10000, "castle", 3, 50)
        second = held_out_seeds(self.contract, 10000, "castle", 3, 50)
        self.assertEqual(50, len(first))
        self.assertEqual(first, second)
        self.assertEqual(50, len(set(first)))

    def test_training_and_evaluation_seed_spaces_are_different(self):
        evaluation = set(held_out_seeds(self.contract, 10000, "castle", 1, 50))
        training = set(training_seeds(1918, 50))
        self.assertFalse(evaluation.intersection(training))

    def test_checkpoint_identity_is_preserved(self):
        direct = checkpoint_identity(self.pt, 10000, self.contract)
        self.assertEqual(direct["checkpoint_id"], self.identity["checkpoint_id"])
        self.assertEqual(str(self.pt.resolve()), self.identity["source_checkpoint"])
        self.assertEqual(10000, self.identity["checkpoint_step"])
        self.assertEqual(self.contract, self.identity["contract_hash"])

    def test_partial_evaluation_is_not_published_as_completed(self):
        store = EvaluationStore(self.run)
        generation = store.next_generation(self.identity["checkpoint_id"], "castle")
        seed_base = evaluation_seed_base(self.contract, 10000, "castle", generation)
        base = store.begin("run-1", self.identity, "castle", generation, 50, seed_base)
        self.assertEqual(50, len(base["seeds"]))
        self.assertEqual(held_out_seeds(self.contract, 10000, "castle", generation, 50), base["seeds"])
        with self.assertRaises(ControlError):
            store.publish_completed(base, {"completed_episodes": 17, "successes": 15}, 0.80)
        result = store.publish_interrupted(base, 17, 15, "worker stopped")
        self.assertEqual("INTERRUPTED", result["state"])
        self.assertEqual("INTERRUPTED", result["result"])
        self.assertIsNone(result["success_rate"])
        self.assertEqual(17, result["completed_episodes"])

    def test_evaluation_environment_is_inference_not_training_mode(self):
        env = evaluation_environment(
            "run-1", self.identity, "castle", 1, 50,
            evaluation_seed_base(self.contract, 10000, "castle", 1),
            self.run / "curriculum-state.json", self.run / "evaluations/progress.json")
        self.assertEqual("1", env["MOYVA_EVALUATION"])
        self.assertNotIn("MOYVA_TRAINING_MODE", env)
        self.assertNotIn("MOYVA_REQUIRE_TRAINER", env)
        self.assertNotIn("MLAGENTS_PORT", env)

        player = evaluation_player_path(self.run, 10000, "linux")
        player.parent.mkdir(parents=True, exist_ok=True)
        player.write_bytes(b"frozen-evaluation-player")
        commands = []
        def runner(command, **_kwargs):
            commands.append([str(value) for value in command])
            progress = self.run / "evaluations/unity-progress.json"
            progress.write_text(json.dumps({
                "state": "COMPLETED", "run_id": "run-1", "checkpoint_step": 10000,
                "contract_hash": self.contract, "scenario_id": "castle",
                "seed_set_version": "heldout-v1", "evaluation_generation": 1,
                "episode_count": 50, "completed_episodes": 50, "successes": 40,
                "mastery_changed": False
            }), encoding="utf-8")
            return 0
        result = run_frozen_evaluation(
            self.root, "/unused/unity", "linux", "run-1", self.run, self.identity,
            "castle", 1, 50, 0.80, self.run / "curriculum-state.json", runner)
        self.assertEqual("COMPLETED", result["state"])
        launched = commands[-1]
        self.assertFalse(any("mlagents" in value.lower() for value in launched))
        self.assertNotIn("--mlagents-port", launched)
        self.assertNotIn("-moyvaRequireTrainer", launched)
        self.assertNotIn("-moyvaTrainingMode", launched)

    def test_best_verified_changes_only_after_completed_evaluation(self):
        store = EvaluationStore(self.run)
        generation = 1
        seed_base = evaluation_seed_base(self.contract, 10000, "castle", generation)
        base = store.begin("run-1", self.identity, "castle", generation, 50, seed_base)
        interrupted = store.publish_interrupted(base, 17, 14, "stop")
        self.assertNotIn("best_verified_checkpoint", interrupted)
        base = store.begin("run-1", self.identity, "castle", generation, 50, seed_base)
        completed = store.publish_completed(base, {"completed_episodes": 50, "successes": 41, "mastery_changed": False}, 0.80)
        self.assertEqual(self.identity["checkpoint_id"], completed["best_verified_checkpoint"]["checkpoint_id"])

    def test_resume_training_uses_evaluated_checkpoint(self):
        state = self.run / "curriculum-state.json"
        state.write_text(json.dumps({"bestVerifiedCheckpoint": "older"}), encoding="utf-8")
        update_curriculum_latest_checkpoint(state, self.identity, 10000)
        stored = json.loads(state.read_text(encoding="utf-8"))
        self.assertEqual(self.identity["checkpoint_id"], stored["lastCheckpoint"])
        self.assertEqual("older", stored["bestVerifiedCheckpoint"])
        self.assertEqual(self.pt.resolve(), verify_resume_checkpoint(self.run, self.identity))


if __name__ == "__main__":
    unittest.main()
