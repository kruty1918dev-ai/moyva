import unittest

try:
    import textual
except ImportError:
    textual = None


@unittest.skipIf(textual is None, "Install tools/ai/requirements-cli.txt for TUI tests")
class ControlCenterLifecycleTests(unittest.TestCase):
    def test_widget_lookup_is_safe_after_shutdown_begins(self):
        from moyva_cli.tui.app import ControlCenter

        app = ControlCenter()
        app._closing = True
        self.assertIsNone(app._widget("#dashboard-info"))
        # These helpers must be no-ops rather than raising Textual NoMatches.
        app.update_table("runs-table", [])
        app._set_disabled("#stop-training", True)

    def test_duration_formatting(self):
        from moyva_cli.tui.app import _duration

        self.assertEqual("—", _duration(None))
        self.assertEqual("45s", _duration(45))
        self.assertEqual("2m 05s", _duration(125))
        self.assertEqual("1h 02m", _duration(3720))


@unittest.skipIf(textual is None, "Install tools/ai/requirements-cli.txt for TUI tests")
class TrainingUiHelpersTests(unittest.TestCase):
    def test_preset_hint_explains_fullgame_preview(self):
        from moyva_cli.tui.app import _preset_hint
        self.assertIn("visible Unity", _preset_hint("fullgame-preview"))

    def test_phase_reports_training_when_metrics_exist(self):
        from moyva_cli.tui.app import _phase
        class Project:
            results = None
        self.assertEqual("TRAINING", _phase(Project(), "run", {"log": ""}, {"step": 100}))

    def test_startup_message_distinguishes_preparation_from_learning(self):
        from moyva_cli.tui.app import _startup_message

        self.assertIn("rebuilding", _startup_message("REBUILDING TRAINING PLAYER", 65, 500, False))
        self.assertIn("TRAINING ACTIVE", _startup_message("TRAINING", 125, 500, True))
        self.assertIn("first metric report", _startup_message("WAITING FOR FIRST METRICS", 10, 500, False))
