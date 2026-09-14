import unittest

from moyva_cli.preview_window import Monitor, _choose_target_monitor, _parse_xrandr_monitors


class PreviewWindowTests(unittest.TestCase):
    def test_parse_xrandr_monitors(self):
        text = """Monitors: 2
 0: +*DP-1 1920/510x1080/290+0+0  DP-1
 1: +HDMI-1 2560/600x1440/340+1920+0  HDMI-1
"""
        monitors = _parse_xrandr_monitors(text)
        self.assertEqual(2, len(monitors))
        self.assertTrue(monitors[0].primary)
        self.assertEqual((1920, 0, 2560, 1440), (monitors[1].x, monitors[1].y, monitors[1].width, monitors[1].height))

    def test_choose_other_monitor_than_control_center(self):
        left = Monitor("DP-1", 0, 0, 1920, 1080, True)
        right = Monitor("HDMI-1", 1920, 0, 2560, 1440, False)
        target = _choose_target_monitor([left, right], (100, 100, 1200, 800))
        self.assertEqual("HDMI-1", target.name)

    def test_single_monitor_uses_same_monitor(self):
        only = Monitor("eDP-1", 0, 0, 1920, 1080, True)
        self.assertEqual(only, _choose_target_monitor([only], (100, 100, 800, 600)))


if __name__ == "__main__":
    unittest.main()
