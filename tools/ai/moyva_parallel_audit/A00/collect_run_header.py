#!/usr/bin/env python3
"""Emit a run header JSON conforming to run-header.schema.json.

Two modes:
  * probe (default): environment/git identity only. Runtime-owned fields
    (resolvedSeed, globalSeedCurrent, frame, gameTime, fps, ...) stay null so a
    probe can never be mistaken for gameplay evidence.
  * --runtime-json FILE: merge fields observed inside the Editor by
    capture_runtime_snapshot.cs (run via tools/unity-cli/moyva-unity eval-file).

Static resolution mirrors the verified production code path at base
98d86e11f1fd905d7e90b6b7a4e8c30dc6b20e38:
  GlobalSeed.Normalize           (seed 0 -> 1)
  GameLaunchContext.Size         (0/1/2 -> 32/64/128 tiles)
  MapSeedService.Resolve         (launch seed -> recipe.Seed -> DefaultSeed 42)
  MapSizeResolver.Resolve        (launch dims -> recipe SharedSettings -> requested)
  MapChunkSizePolicy.CropAxis    (<16 -> 16; else floor to multiple of 16)

Stdlib only. Exit codes: 0 ok, 2 usage/schema error.
"""

import argparse
import json
import os
import platform
import re
import subprocess
import sys
from datetime import datetime, timezone

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
SEEDS_FILE = os.path.join(SCRIPT_DIR, "audit-seeds.json")

# Runtime-observed keys capture_runtime_snapshot.cs may provide; anything else is
# rejected so a probe file cannot silently smuggle unexpected claims.
RUNTIME_KEYS = {
    "resolvedSeed": ("rng", "resolvedSeed"),
    "normalizedSeed": ("rng", "normalizedSeed"),
    "globalSeedCurrent": ("rng", "globalSeedCurrent"),
    "unityRandomInitialized": ("rng", "unityRandomInitialized"),
    "playMode": ("mode", "playMode"),
    "gameState": ("mode", "gameState"),
    "launchMode": ("mode", "launchMode"),
    "networkSession": ("mode", "networkSession"),
    "recipeId": ("world", "recipeId"),
    "worldWidth": ("world.dimensions", "width"),
    "worldHeight": ("world.dimensions", "height"),
    "dimensionSource": ("world.dimensions", "source"),
    "playerCount": ("world", "playerCount"),
    "timeScale": ("clockConfig", "timeScale"),
    "fixedDeltaTime": ("clockConfig", "fixedDeltaTime"),
    "maximumDeltaTime": ("clockConfig", "maximumDeltaTime"),
    "pauseOwnsTimeScale": ("clockConfig", "pauseOwnsTimeScale"),
    "fps": (None, "fps"),
    "frameTimeMs": (None, "frameTimeMs"),
    "frame": ("snapshot", "frame"),
    "gameTime": ("snapshot", "gameTime"),
    "startupSequence": ("snapshot", "startupSequence"),
    "snapshotRevision": ("snapshot", "snapshotRevision"),
    "startupSessionId": ("snapshot", "startupSessionId"),
    "unityVersion": ("environment", "unityVersion"),
    # raw int from GameLaunchContext.MapType; mapped to a name during merge
    "mapTypeRaw": (None, "__mapTypeRaw__"),
}

MAP_TYPE_NAMES = {
    0: "continents", 1: "pangaea", 2: "islands",
    3: "highlands", 4: "desert", 5: "random",
}

REQUIRED_TOP = [
    "schemaVersion", "taskId", "owner", "capturedAtUtc", "baseSha", "treeSha",
    "branch", "seed", "rng", "world", "mode", "clockConfig", "snapshot",
    "environment",
]

SIZE_SIDE = {0: 32, 1: 64, 2: 128}
SIZE_NAME = {0: "small", 1: "medium", 2: "large"}
CHUNK = 16


def normalize_seed(seed):
    """GlobalSeed.Normalize: 0 -> 1."""
    return 1 if seed == 0 else seed


def crop_axis(requested):
    """MapChunkSizePolicy.CropAxis: <16 -> 16, else floor to a multiple of 16."""
    safe = max(1, requested)
    return CHUNK if safe < CHUNK else (safe // CHUNK) * CHUNK


def resolve_seed(launch_seed, recipe_seed):
    """MapSeedService.Resolve precedence, then InitializeDeterministic input."""
    if launch_seed:
        return normalize_seed(launch_seed)
    if recipe_seed:
        return normalize_seed(recipe_seed)
    return 42  # GlobalSeed.DefaultSeed


def git(args, cwd):
    return subprocess.run(
        ["git"] + args, cwd=cwd, capture_output=True, text=True, check=True
    ).stdout.strip()


def utc_now():
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "Z"


def find_in_json(obj, names):
    """Depth-first search for the first of `names` in nested dict/list JSON."""
    if isinstance(obj, dict):
        for k, v in obj.items():
            if k.lower() in names:
                return v
        for v in obj.values():
            hit = find_in_json(v, names)
            if hit is not None:
                return hit
    elif isinstance(obj, list):
        for item in obj:
            hit = find_in_json(item, names)
            if hit is not None:
                return hit
    return None


def probe_editor(project_root):
    """Best-effort `unity status` discovery; never signals a foreign editor."""
    env = {"unityVersion": None, "pipelinePort": None, "editorPid": None}
    try:
        out = subprocess.run(
            ["unity", "status", "--format", "json"],
            capture_output=True, text=True, timeout=15,
        )
        if out.returncode == 0 and out.stdout.strip():
            data = json.loads(out.stdout)
            norm = os.path.normcase(os.path.normpath(project_root))
            candidates = data if isinstance(data, list) else [data]
            for entry in candidates:
                for item in (entry if isinstance(entry, list) else [entry]):
                    path = find_in_json(item, {"projectpath", "project", "path"})
                    if path and os.path.normcase(os.path.normpath(str(path))) == norm:
                        pid = find_in_json(item, {"pid", "processid"})
                        port = find_in_json(item, {"port", "pipelineport"})
                        env["editorPid"] = int(pid) if pid is not None else None
                        env["pipelinePort"] = int(port) if port is not None else None
    except (OSError, subprocess.SubprocessError, ValueError):
        pass  # unity CLI absent or unreadable -> probe stays environment-only
    return env


def load_fixture(seed_id):
    with open(SEEDS_FILE, encoding="utf-8") as fh:
        doc = json.load(fh)
    for entry in doc.get("seeds", []):
        if entry.get("id") == seed_id:
            return entry
    known = ", ".join(s.get("id", "?") for s in doc.get("seeds", []))
    sys.exit(f"unknown seed id '{seed_id}' (fixture has: {known})")


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--task-id", default="A00")
    ap.add_argument("--seed-id", help="id in audit-seeds.json (default: first entry)")
    ap.add_argument("--seed", type=int,
                    help="ad-hoc launch seed; bypasses the fixture (fixtureId='')")
    ap.add_argument("--sequence", type=int, default=0,
                    help="snapshot sequence within the run")
    ap.add_argument("--owner", help="owning branch (default: current git branch)")
    ap.add_argument("--runtime-json", help="merge capture_runtime_snapshot.cs output")
    ap.add_argument("--probe-editor", action="store_true",
                    help="attempt 'unity status' to fill environment.editorPid/pipelinePort")
    ap.add_argument("--unity-version", help="override environment.unityVersion")
    ap.add_argument("--notes", default="")
    ap.add_argument("--out", help="write to file instead of stdout")
    args = ap.parse_args(argv)

    if not re.fullmatch(r"[A-Z][0-9]{2}", args.task_id):
        sys.exit(f"task-id '{args.task_id}' must match ^[A-Z][0-9]{{2}}$")

    root = git(["rev-parse", "--show-toplevel"], SCRIPT_DIR)
    base_sha = git(["rev-parse", "HEAD"], root)
    tree_sha = git(["rev-parse", "HEAD^{tree}"], root)
    branch = git(["branch", "--show-current"], root) or "(detached)"

    # --- requested configuration -------------------------------------------------
    if args.seed is not None:
        fixture = None
        launch_seed = args.seed
        launch_size = 1
        launch_map_type = ""
        player_count = 2
        fixture_id = ""
        recipe_seed = 777
        recipe_id = "testgeneratorrecipe"
    else:
        with open(SEEDS_FILE, encoding="utf-8") as fh:
            doc = json.load(fh)
        fixture_id = args.seed_id or doc["seeds"][0]["id"]
        fixture = load_fixture(fixture_id)
        launch_seed = int(fixture["launchSeed"])
        launch_size = int(fixture["launchSize"])
        launch_map_type = fixture.get("launchMapType") or ""
        player_count = int(fixture.get("playerCount", 2))
        recipe_id = fixture.get("expected", {}).get("recipeId") or "testgeneratorrecipe"
        recipe_seed = 777 if recipe_id == "testgeneratorrecipe" else 0

    req_w = req_h = SIZE_SIDE.get(launch_size, 64)
    expected_seed = resolve_seed(launch_seed, recipe_seed)
    expected_w, expected_h = crop_axis(req_w), crop_axis(req_h)

    header = {
        "schemaVersion": 1,
        "taskId": args.task_id,
        "owner": args.owner or branch,
        "capturedAtUtc": utc_now(),
        "baseSha": base_sha,
        "treeSha": tree_sha,
        "branch": branch,
        "seed": launch_seed,
        "rng": {
            "resolvedSeed": None,
            "normalizedSeed": None,
            "globalSeedCurrent": None,
            "unityRandomInitialized": None,
            "fixtureId": fixture_id,
        },
        "world": {
            "requestedMapType": launch_map_type,
            "mapTypeSource": "launch-context" if launch_map_type else "none",
            "resolvedArchetype": "not-resolved",
            "recipeId": recipe_id,
            "dimensions": {
                "requestedWidth": req_w,
                "requestedHeight": req_h,
                "width": None,
                "height": None,
                "sizePreset": SIZE_NAME.get(launch_size, "unknown"),
                "source": "unknown",
            },
            "playerCount": player_count,
        },
        "mode": {
            "kind": "editor-stopped",
            "playMode": "stopped",
        },
        "clockConfig": {
            "timeScale": None,
            "fixedDeltaTime": None,
            "maximumDeltaTime": None,
            "pauseOwnsTimeScale": None,
        },
        "fps": None,
        "frameTimeMs": None,
        "snapshot": {
            "sequence": args.sequence,
            "frame": None,
            "gameTime": None,
            "wallTimeUtc": utc_now(),
        },
        "environment": {
            "unityVersion": args.unity_version,
            "projectPath": root,
            "worktree": os.path.abspath(root),
            "pipelinePort": None,
            "editorPid": None,
            "collector": "collect_run_header.py/1",
            "platform": platform.platform(),
        },
    }

    if fixture:
        exp = fixture.get("expected", {})
        header["notes"] = (
            "environment probe; no Unity session observed. "
            f"expected per {fixture_id}: resolvedSeed={exp.get('resolvedSeed')}, "
            f"dims={exp.get('width')}x{exp.get('height')}, "
            f"recipe={exp.get('recipeId')}, archetype={exp.get('resolvedArchetype')}"
        )
    else:
        header["notes"] = (
            "environment probe; ad-hoc seed. "
            f"static resolution would give resolvedSeed={expected_seed}, "
            f"dims={expected_w}x{expected_h} (post-CropAxis)"
        )
    if args.notes:
        header["notes"] = (header["notes"] + " | " + args.notes).strip(" |")

    # --- runtime merge -------------------------------------------------------------
    if args.runtime_json:
        with open(args.runtime_json, encoding="utf-8-sig") as fh:
            runtime = json.load(fh)
        unknown = set(runtime) - set(RUNTIME_KEYS)
        if unknown:
            sys.exit(f"runtime json has unknown keys: {sorted(unknown)}")
        for key, (section, field) in RUNTIME_KEYS.items():
            if key not in runtime:
                continue
            if field == "__mapTypeRaw__":
                raw = runtime[key]
                if raw is not None:
                    header["world"]["requestedMapType"] = MAP_TYPE_NAMES.get(
                        int(raw), "unknown")
                    header["world"]["mapTypeSource"] = "launch-context"
                continue
            if section is None:
                header[field] = runtime[key]
            elif section == "world.dimensions":
                header["world"]["dimensions"][field] = runtime[key]
            else:
                header[section][field] = runtime[key]
        if runtime.get("playMode") in ("playing", "paused"):
            header["mode"]["kind"] = (
                "editor-paused" if runtime["playMode"] == "paused" else "editor-playing"
            )
        header["environment"]["collector"] += " + capture_runtime_snapshot.cs/1"

    if args.probe_editor:
        env = probe_editor(root)
        for k, v in env.items():
            if v is not None:
                header["environment"][k] = v
        if env["editorPid"] and header["mode"]["kind"] == "editor-stopped":
            header["notes"] += " | editor process detected via unity status"

    # --- minimal self-check ---------------------------------------------------------
    missing = [k for k in REQUIRED_TOP if k not in header]
    if missing:
        sys.exit(f"internal error: missing required keys {missing}")
    for key in ("sequence", "frame", "gameTime", "wallTimeUtc"):
        if key not in header["snapshot"]:
            sys.exit(f"internal error: snapshot.{key} missing")
    if not re.fullmatch(r"[0-9a-f]{40}", header["baseSha"]):
        sys.exit(f"baseSha '{header['baseSha']}' is not a full SHA")

    text = json.dumps(header, indent=2, ensure_ascii=False) + "\n"
    if args.out:
        os.makedirs(os.path.dirname(os.path.abspath(args.out)), exist_ok=True)
        with open(args.out, "w", encoding="utf-8") as fh:
            fh.write(text)
        print(f"wrote {args.out} (kind={header['mode']['kind']}, "
              f"tree={header['treeSha'][:12]})", file=sys.stderr)
    else:
        sys.stdout.write(text)


if __name__ == "__main__":
    main()
