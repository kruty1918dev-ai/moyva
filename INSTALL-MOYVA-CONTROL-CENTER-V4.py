#!/usr/bin/env python3
"""Install Moyva Control Center V4 preview-window support.

Idempotent, narrow patcher. It updates only:
- tools/ai/moyva_train.py: summary frequency + visual preview dimensions/windowed launch;
- Assets/Moyva/AI/Training/Editor/TrainingPlayerBuilder.cs: build the training player as
  a normal resizable window that keeps running when unfocused.

It does not change gameplay rules, rewards, BotDecisionContract, or model schemas.
"""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parent
LAUNCHER = ROOT / "tools/ai/moyva_train.py"
BUILDER = ROOT / "Assets/Moyva/AI/Training/Editor/TrainingPlayerBuilder.cs"


def replace_once(text, old, new, label):
    if old not in text:
        raise SystemExit(f"Installer could not find expected block: {label}. The repository version may have changed.")
    return text.replace(old, new, 1)


def patch_launcher():
    if not LAUNCHER.is_file():
        raise SystemExit(f"Missing {LAUNCHER}")
    text = LAUNCHER.read_text(encoding="utf-8")
    changed = False

    if 'p.add_argument("--summary-freq", type=int)' not in text:
        old = '''            p.add_argument("--checkpoint-interval", type=int)\n            p.add_argument("--base-port", type=int, default=5005)\n'''
        new = '''            p.add_argument("--checkpoint-interval", type=int)\n            p.add_argument("--summary-freq", type=int)\n            p.add_argument("--screen-width", type=int, default=1280)\n            p.add_argument("--screen-height", type=int, default=720)\n            p.add_argument("--base-port", type=int, default=5005)\n'''
        text = replace_once(text, old, new, "launcher parser options")
        changed = True

    if 'trainer["behaviors"][BEHAVIOR]["summary_freq"] = args.summary_freq' not in text:
        old = '''    if args.checkpoint_interval:\n        if args.checkpoint_interval < 1:\n            raise LaunchError("Checkpoint interval must be positive.")\n        trainer["behaviors"][BEHAVIOR]["checkpoint_interval"] = args.checkpoint_interval\n'''
        new = old + '''    if getattr(args, "summary_freq", None):\n        if args.summary_freq < 1:\n            raise LaunchError("Summary frequency must be positive.")\n        trainer["behaviors"][BEHAVIOR]["summary_freq"] = args.summary_freq\n'''
        text = replace_once(text, old, new, "summary frequency")
        changed = True

    if '"-screen-fullscreen", "0"' not in text:
        old = '''    if args.episode_decisions:\n        command += ["-moyvaEpisodeDecisions", str(args.episode_decisions)]\n'''
        new = '''    if mode == "Visual":\n        # Keep the preview as a normal decorated OS window. Control Center moves/maximizes\n        # it on the selected monitor; the user can then minimize, restore and resize it.\n        command += [\n            "-screen-fullscreen", "0",\n            "-screen-width", str(getattr(args, "screen_width", 1280) or 1280),\n            "-screen-height", str(getattr(args, "screen_height", 720) or 720),\n        ]\n    if args.episode_decisions:\n        command += ["-moyvaEpisodeDecisions", str(args.episode_decisions)]\n'''
        text = replace_once(text, old, new, "visual preview window arguments")
        changed = True

    if 'summary_freq=trainer["behaviors"][BEHAVIOR].get("summary_freq")' not in text:
        old = '''    atomic_json(run_dir / "effective-config.json", dict(metadata, max_steps=trainer["behaviors"][BEHAVIOR]["max_steps"],\n        episode_decisions=args.episode_decisions, time_scale=speed, checkpoint_interval=trainer["behaviors"][BEHAVIOR].get("checkpoint_interval")))\n'''
        new = '''    atomic_json(run_dir / "effective-config.json", dict(\n        metadata, max_steps=trainer["behaviors"][BEHAVIOR]["max_steps"],\n        episode_decisions=args.episode_decisions, time_scale=speed,\n        checkpoint_interval=trainer["behaviors"][BEHAVIOR].get("checkpoint_interval"),\n        summary_freq=trainer["behaviors"][BEHAVIOR].get("summary_freq"),\n        screen_width=getattr(args, "screen_width", None), screen_height=getattr(args, "screen_height", None)))\n'''
        text = replace_once(text, old, new, "effective config metadata")
        changed = True

    if changed:
        LAUNCHER.write_text(text, encoding="utf-8")
        print("Updated tools/ai/moyva_train.py")
    else:
        print("Launcher already contains V4-compatible preview options.")


def patch_builder():
    if not BUILDER.is_file():
        raise SystemExit(f"Missing {BUILDER}")
    text = BUILDER.read_text(encoding="utf-8")
    marker = "MOYVA_TRAINING_PREVIEW_WINDOW_SETTINGS"
    if marker in text:
        print("TrainingPlayerBuilder already contains V4 window settings.")
        return

    old = '''            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {\n                scenes = new[] { Scene }, locationPathName = output, target = target,\n                options = BuildOptions.DetailedBuildReport });\n            if (result.summary.result != BuildResult.Succeeded)\n                throw new InvalidOperationException("Training player build failed: " + result.summary.result + "; errors=" + result.summary.totalErrors);\n'''
    new = '''            // MOYVA_TRAINING_PREVIEW_WINDOW_SETTINGS\n            // Training preview must behave like a normal desktop window: resizable/minimizable\n            // and still running when the Control Center has focus. Runtime launch arguments decide\n            // the actual monitor size; Control Center moves/maximizes it after startup.\n            bool oldResizableWindow = PlayerSettings.resizableWindow;\n            bool oldRunInBackground = PlayerSettings.runInBackground;\n            bool oldAllowFullscreenSwitch = PlayerSettings.allowFullscreenSwitch;\n            var oldFullScreenMode = PlayerSettings.fullScreenMode;\n            int oldDefaultWidth = PlayerSettings.defaultScreenWidth;\n            int oldDefaultHeight = PlayerSettings.defaultScreenHeight;\n            BuildReport result;\n            try\n            {\n                PlayerSettings.resizableWindow = true;\n                PlayerSettings.runInBackground = true;\n                PlayerSettings.allowFullscreenSwitch = true;\n                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;\n                PlayerSettings.defaultScreenWidth = 1280;\n                PlayerSettings.defaultScreenHeight = 720;\n                result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {\n                    scenes = new[] { Scene }, locationPathName = output, target = target,\n                    options = BuildOptions.DetailedBuildReport });\n            }\n            finally\n            {\n                PlayerSettings.resizableWindow = oldResizableWindow;\n                PlayerSettings.runInBackground = oldRunInBackground;\n                PlayerSettings.allowFullscreenSwitch = oldAllowFullscreenSwitch;\n                PlayerSettings.fullScreenMode = oldFullScreenMode;\n                PlayerSettings.defaultScreenWidth = oldDefaultWidth;\n                PlayerSettings.defaultScreenHeight = oldDefaultHeight;\n            }\n            if (result.summary.result != BuildResult.Succeeded)\n                throw new InvalidOperationException("Training player build failed: " + result.summary.result + "; errors=" + result.summary.totalErrors);\n'''
    text = replace_once(text, old, new, "TrainingPlayerBuilder.BuildPlayer")
    BUILDER.write_text(text, encoding="utf-8")
    print("Updated TrainingPlayerBuilder.cs for resizable/background preview windows.")


def main():
    patch_launcher()
    patch_builder()
    print("Moyva Control Center V4 preview support installed. Rebuild the training player before the next visual run.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
