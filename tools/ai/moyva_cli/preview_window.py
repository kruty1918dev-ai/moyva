from __future__ import annotations

"""Desktop placement for Moyva's visible training player.

The preview intentionally remains a normal decorated OS window: it is moved to a
monitor, maximized there, and may then be minimized, restored or resized by the
user.  This is preferable to exclusive fullscreen for a developer/training tool.

Linux/X11 is the primary fully-automatic backend because the Control Center is a
terminal/TUI and X11 exposes standard window-manager tooling. Other platforms
fall back safely to Unity's normal window placement.
"""

from dataclasses import asdict, dataclass
import argparse
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys
import time


@dataclass(frozen=True)
class Monitor:
    name: str
    x: int
    y: int
    width: int
    height: int
    primary: bool = False

    @property
    def area(self):
        return self.width * self.height

    def contains(self, px, py):
        return self.x <= px < self.x + self.width and self.y <= py < self.y + self.height


def _run(command, timeout=3):
    try:
        return subprocess.run(command, capture_output=True, text=True, timeout=timeout, check=False)
    except (OSError, subprocess.SubprocessError):
        return None


def _parse_xrandr_monitors(text):
    monitors = []
    # Example: 0: +*DP-1 1920/510x1080/290+0+0  DP-1
    pattern = re.compile(
        r"^\s*\d+:\s+([+*]*)(\S+)\s+(\d+)(?:/\d+)?x(\d+)(?:/\d+)?\+(-?\d+)\+(-?\d+)(?:\s+(\S+))?"
    )
    for line in (text or "").splitlines():
        match = pattern.match(line)
        if not match:
            continue
        flags, short_name, width, height, x, y, trailing = match.groups()
        monitors.append(
            Monitor(
                name=trailing or short_name,
                x=int(x),
                y=int(y),
                width=int(width),
                height=int(height),
                primary="*" in flags,
            )
        )
    return monitors


def _linux_monitors():
    xrandr = shutil.which("xrandr")
    if not xrandr:
        return []
    result = _run([xrandr, "--listmonitors"])
    return _parse_xrandr_monitors(result.stdout if result else "")


def _active_window_rect_linux():
    xdotool = shutil.which("xdotool")
    if xdotool:
        active = _run([xdotool, "getactivewindow"])
        window = (active.stdout.strip() if active else "")
        if window:
            geometry = _run([xdotool, "getwindowgeometry", "--shell", window])
            values = {}
            if geometry:
                for line in geometry.stdout.splitlines():
                    if "=" in line:
                        key, value = line.split("=", 1)
                        values[key.strip()] = value.strip()
            try:
                return (
                    int(values["X"]), int(values["Y"]),
                    int(values["WIDTH"]), int(values["HEIGHT"]),
                )
            except (KeyError, ValueError):
                pass

    xprop = shutil.which("xprop")
    xwininfo = shutil.which("xwininfo")
    if xprop and xwininfo:
        active = _run([xprop, "-root", "_NET_ACTIVE_WINDOW"])
        match = re.search(r"0x[0-9a-fA-F]+", active.stdout if active else "")
        if match:
            info = _run([xwininfo, "-id", match.group(0)])
            text = info.stdout if info else ""
            def number(label):
                found = re.search(re.escape(label) + r":\s*(-?\d+)", text)
                return int(found.group(1)) if found else None
            x = number("Absolute upper-left X")
            y = number("Absolute upper-left Y")
            w = number("Width")
            h = number("Height")
            if None not in (x, y, w, h):
                return x, y, w, h
    return None


def _choose_target_monitor(monitors, control_rect=None):
    if not monitors:
        return None
    if len(monitors) == 1:
        return monitors[0]

    control_monitor = None
    if control_rect:
        x, y, width, height = control_rect
        center = (x + width / 2.0, y + height / 2.0)
        control_monitor = next((m for m in monitors if m.contains(*center)), None)

    if control_monitor is not None:
        others = [m for m in monitors if m != control_monitor]
        if others:
            return max(others, key=lambda m: (m.area, not m.primary))

    # If the terminal window cannot be identified, the primary display is the
    # most likely place for the Control Center, so prefer a non-primary display.
    non_primary = [m for m in monitors if not m.primary]
    return max(non_primary or monitors, key=lambda m: m.area)


def target_path(project_root):
    return Path(project_root) / ".moyva-local" / "preview-target.json"


def prepare_preview_target(project_root):
    root = Path(project_root).resolve()
    session = os.environ.get("XDG_SESSION_TYPE", "").lower()
    data = {
        "platform": sys.platform,
        "session": session,
        "automatic_placement": False,
        "created": time.time(),
    }
    if sys.platform.startswith("linux"):
        monitors = _linux_monitors()
        control_rect = _active_window_rect_linux()
        target = _choose_target_monitor(monitors, control_rect)
        data["monitors"] = [asdict(m) for m in monitors]
        data["control_window"] = control_rect
        if target:
            data["monitor"] = asdict(target)
            data["automatic_placement"] = bool(shutil.which("wmctrl") or shutil.which("xdotool"))
            data["backend"] = "wmctrl" if shutil.which("wmctrl") else "xdotool" if shutil.which("xdotool") else "none"
    path = target_path(root)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2), encoding="utf-8")
    return data


def load_preview_target(project_root):
    path = target_path(project_root)
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, ValueError, TypeError):
        return {}


def _pid_looks_like_training_player(pid, title=""):
    title_lower = (title or "").lower()
    try:
        cmd = Path(f"/proc/{pid}/cmdline").read_bytes().replace(b"\0", b" ").decode(errors="replace").lower()
        exe = os.path.basename(os.readlink(f"/proc/{pid}/exe")).lower()
    except (OSError, ValueError):
        cmd, exe = "", ""
    if "moyvatraining" in exe or "moyvatraining" in cmd or "build/training/moyvatraining" in cmd:
        return True
    # Title is a fallback only; avoid matching the Unity Editor if its command line is visible.
    return ("moyvatraining" in title_lower or "moyva training" in title_lower) and "editor/unity" not in cmd


def _windows_wmctrl():
    wmctrl = shutil.which("wmctrl")
    if not wmctrl:
        return []
    result = _run([wmctrl, "-lpG"])
    windows = []
    for line in (result.stdout if result else "").splitlines():
        parts = line.split(None, 8)
        if len(parts) < 9:
            continue
        wid, desktop, pid, x, y, width, height, host, title = parts
        try:
            pid_value = int(pid)
        except ValueError:
            continue
        if _pid_looks_like_training_player(pid_value, title):
            windows.append({"id": wid, "pid": pid_value, "title": title})
    return windows


def _windows_xdotool():
    xdotool = shutil.which("xdotool")
    if not xdotool:
        return []
    result = _run([xdotool, "search", "--onlyvisible", "--name", "Moyva"])
    windows = []
    for wid in (result.stdout if result else "").splitlines():
        wid = wid.strip()
        if not wid:
            continue
        pid_result = _run([xdotool, "getwindowpid", wid])
        title_result = _run([xdotool, "getwindowname", wid])
        try:
            pid = int(pid_result.stdout.strip()) if pid_result else 0
        except ValueError:
            pid = 0
        title = title_result.stdout.strip() if title_result else ""
        if pid and _pid_looks_like_training_player(pid, title):
            windows.append({"id": wid, "pid": pid, "title": title})
    return windows


def _find_preview_window():
    return next(iter(_windows_wmctrl() or _windows_xdotool()), None)


def _place_with_wmctrl(window_id, monitor, activate=True):
    wmctrl = shutil.which("wmctrl")
    if not wmctrl:
        return False
    # Keep normal window decorations. Move first so EWMH maximize applies to the target monitor.
    _run([wmctrl, "-ir", window_id, "-b", "remove,fullscreen,maximized_vert,maximized_horz"])
    margin = 24
    x = int(monitor["x"]) + margin
    y = int(monitor["y"]) + margin
    width = max(800, int(monitor["width"]) - margin * 2)
    height = max(450, int(monitor["height"]) - margin * 2)
    _run([wmctrl, "-ir", window_id, "-e", f"0,{x},{y},{width},{height}"])
    time.sleep(0.15)
    _run([wmctrl, "-ir", window_id, "-b", "add,maximized_vert,maximized_horz"])
    if activate:
        _run([wmctrl, "-ia", window_id])
    return True


def _place_with_xdotool(window_id, monitor, activate=True):
    xdotool = shutil.which("xdotool")
    if not xdotool:
        return False
    x = int(monitor["x"])
    y = int(monitor["y"])
    width = int(monitor["width"])
    height = int(monitor["height"])
    _run([xdotool, "windowmove", "--sync", window_id, str(x), str(y)])
    _run([xdotool, "windowsize", "--sync", window_id, str(width), str(height)])
    if activate:
        _run([xdotool, "windowactivate", "--sync", window_id])
    return True


def place_preview(project_root, target=None, activate=True):
    target = target or load_preview_target(project_root)
    monitor = (target or {}).get("monitor")
    if not monitor or not sys.platform.startswith("linux"):
        return False
    window = _find_preview_window()
    if not window:
        return False
    return _place_with_wmctrl(window["id"], monitor, activate) or _place_with_xdotool(window["id"], monitor, activate)


def arena_rectangles(monitor, count):
    """Stable mosaic geometry for independent native Unity windows."""
    import math
    columns = math.ceil(math.sqrt(count))
    rows = math.ceil(count / columns)
    width, height = int(monitor["width"]) // columns, int(monitor["height"]) // rows
    return [{"x": int(monitor["x"]) + i % columns * width,
             "y": int(monitor["y"]) + i // columns * height,
             "width": width, "height": height} for i in range(count)]


def watch_and_place(project_root, timeout=180):
    target = load_preview_target(project_root)
    monitor = target.get("monitor")
    if not monitor or not sys.platform.startswith("linux"):
        return False
    deadline = time.monotonic() + timeout
    previous = ()
    placed = False
    while time.monotonic() < deadline:
        windows = _windows_wmctrl() or _windows_xdotool()
        # Only position this project's executable, including when other projects train.
        root = str(Path(project_root).resolve()) + os.sep
        def belongs(window):
            try:
                return os.readlink(f"/proc/{window['pid']}/exe").startswith(root)
            except OSError:
                return False
        windows = sorted((w for w in windows if belongs(w)), key=lambda w: w["pid"])
        signature = tuple(w["id"] for w in windows)
        if signature and signature != previous:
            time.sleep(2)  # Let initial graphics/window sizing settle.
            if len(windows) == 1:
                window = windows[0]
                _place_with_wmctrl(window["id"], monitor, activate=not placed) or _place_with_xdotool(window["id"], monitor, activate=not placed)
            else:
                for window, rect in zip(windows, arena_rectangles(monitor, len(windows))):
                    wmctrl = shutil.which("wmctrl")
                    if wmctrl:
                        _run([wmctrl, "-ir", window["id"], "-b", "remove,fullscreen,maximized_vert,maximized_horz"])
                        _run([wmctrl, "-ir", window["id"], "-e",
                              f"0,{rect['x']},{rect['y']},{rect['width']},{max(200,rect['height']-32)}"])
                    else:
                        _place_with_xdotool(window["id"], rect, activate=False)
            previous, placed = signature, True
        if placed and not signature:
            return True
        time.sleep(0.5)
    return placed


def launch_preview_watcher(project_root, timeout=180):
    root = Path(project_root).resolve()
    log_dir = root / ".moyva-local" / "logs"
    log_dir.mkdir(parents=True, exist_ok=True)
    log_path = log_dir / "preview-window.log"
    script = Path(__file__).resolve()
    stream = log_path.open("a", encoding="utf-8")
    kwargs = {"creationflags": subprocess.CREATE_NEW_PROCESS_GROUP} if os.name == "nt" else {"start_new_session": True}
    subprocess.Popen(
        [sys.executable, str(script), "--watch", str(root), "--timeout", str(timeout)],
        cwd=root,
        stdout=stream,
        stderr=subprocess.STDOUT,
        **kwargs,
    )
    stream.close()
    return {"target": load_preview_target(root), "log": str(log_path)}


def describe_target(target):
    monitor = (target or {}).get("monitor")
    if not monitor:
        return "OS-selected monitor"
    return f"{monitor.get('name', 'monitor')} · {monitor.get('width')}×{monitor.get('height')} @ {monitor.get('x')},{monitor.get('y')}"


def main(argv=None):
    parser = argparse.ArgumentParser()
    parser.add_argument("--watch", metavar="PROJECT")
    parser.add_argument("--focus", metavar="PROJECT")
    parser.add_argument("--timeout", type=int, default=180)
    args = parser.parse_args(argv)
    if args.watch:
        ok = watch_and_place(args.watch, args.timeout)
        print("preview placed" if ok else "preview window not found/placed")
        return 0 if ok else 2
    if args.focus:
        ok = place_preview(args.focus, activate=True)
        print("preview focused" if ok else "preview window not found/placed")
        return 0 if ok else 2
    return 2


if __name__ == "__main__":
    raise SystemExit(main())
