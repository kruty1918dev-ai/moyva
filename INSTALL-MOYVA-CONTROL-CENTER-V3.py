#!/usr/bin/env python3
"""Apply small launcher additions required by the simplified Control Center.

This installer is intentionally narrow and idempotent. It only patches
`tools/ai/moyva_train.py` to support:
- more frequent metric summaries selected by presets;
- explicit visual preview window size.
"""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parent
TARGET = ROOT / "tools/ai/moyva_train.py"


def replace_once(text, old, new, label):
    if old not in text:
        raise SystemExit(f"Installer could not find expected launcher block: {label}. The repository version may have changed.")
    return text.replace(old, new, 1)


def main():
    if not TARGET.is_file():
        raise SystemExit(f"Missing {TARGET}")
    text = TARGET.read_text(encoding="utf-8")
    changed = False

    if 'p.add_argument("--summary-freq", type=int)' not in text:
        old = '''            p.add_argument("--checkpoint-interval", type=int)\n            p.add_argument("--base-port", type=int, default=5005)\n'''
        new = '''            p.add_argument("--checkpoint-interval", type=int)\n            p.add_argument("--summary-freq", type=int)\n            p.add_argument("--screen-width", type=int, default=1280)\n            p.add_argument("--screen-height", type=int, default=720)\n            p.add_argument("--base-port", type=int, default=5005)\n'''
        text = replace_once(text, old, new, "parser options")
        changed = True

    if 'trainer["behaviors"][BEHAVIOR]["summary_freq"] = args.summary_freq' not in text:
        old = '''    if args.checkpoint_interval:\n        if args.checkpoint_interval < 1:\n            raise LaunchError("Checkpoint interval must be positive.")\n        trainer["behaviors"][BEHAVIOR]["checkpoint_interval"] = args.checkpoint_interval\n'''
        new = old + '''    if getattr(args, "summary_freq", None):\n        if args.summary_freq < 1:\n            raise LaunchError("Summary frequency must be positive.")\n        trainer["behaviors"][BEHAVIOR]["summary_freq"] = args.summary_freq\n'''
        text = replace_once(text, old, new, "summary frequency")
        changed = True

    if '"-screen-fullscreen", "0"' not in text:
        old = '''    if args.episode_decisions:\n        command += ["-moyvaEpisodeDecisions", str(args.episode_decisions)]\n'''
        new = '''    if mode == "Visual":\n        command += [\n            "-screen-fullscreen", "0",\n            "-screen-width", str(getattr(args, "screen_width", 1280) or 1280),\n            "-screen-height", str(getattr(args, "screen_height", 720) or 720),\n        ]\n    if args.episode_decisions:\n        command += ["-moyvaEpisodeDecisions", str(args.episode_decisions)]\n'''
        text = replace_once(text, old, new, "visual preview window size")
        changed = True

    if 'summary_freq=trainer["behaviors"][BEHAVIOR].get("summary_freq")' not in text:
        old = '''    atomic_json(run_dir / "effective-config.json", dict(metadata, max_steps=trainer["behaviors"][BEHAVIOR]["max_steps"],\n        episode_decisions=args.episode_decisions, time_scale=speed, checkpoint_interval=trainer["behaviors"][BEHAVIOR].get("checkpoint_interval")))\n'''
        new = '''    atomic_json(run_dir / "effective-config.json", dict(\n        metadata, max_steps=trainer["behaviors"][BEHAVIOR]["max_steps"],\n        episode_decisions=args.episode_decisions, time_scale=speed,\n        checkpoint_interval=trainer["behaviors"][BEHAVIOR].get("checkpoint_interval"),\n        summary_freq=trainer["behaviors"][BEHAVIOR].get("summary_freq"),\n        screen_width=getattr(args, "screen_width", None), screen_height=getattr(args, "screen_height", None)))\n'''
        text = replace_once(text, old, new, "effective config metadata")
        changed = True

    if changed:
        TARGET.write_text(text, encoding="utf-8")
        print("Moyva launcher updated for Control Center V3.")
    else:
        print("Moyva launcher already contains Control Center V3 additions.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
