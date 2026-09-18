#!/usr/bin/env python3
"""Run a Moyva refactoring pass in an isolated, recoverable Git worktree."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import os
import re
import shlex
import subprocess
import sys
import tarfile
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
MAX_FAILED_ATTEMPTS = 3
UNITY_IMPORTER_CHURN_PATHS = (
    "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset",
    "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset",
    "moyva.slnx",
)


def run(
    command: Sequence[str],
    *,
    cwd: Path = ROOT,
    check: bool = True,
    capture: bool = True,
) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(
        list(command),
        cwd=cwd,
        check=False,
        text=True,
        stdout=subprocess.PIPE if capture else None,
        stderr=subprocess.STDOUT if capture else None,
    )
    if check and result.returncode != 0:
        output = (result.stdout or "").strip()
        raise RuntimeError(output or f"Command failed: {shlex.join(command)}")
    return result


def git(*args: str, cwd: Path = ROOT, check: bool = True) -> str:
    return run(("git", *args), cwd=cwd, check=check).stdout or ""


def common_git_dir() -> Path:
    value = git("rev-parse", "--git-common-dir").strip()
    path = Path(value)
    return path if path.is_absolute() else (ROOT / path).resolve()


STATE_PATH = common_git_dir() / "moyva-pass-state.json"


def artifact_root() -> Path:
    configured = os.environ.get("MOYVA_PASS_ARTIFACTS")
    if configured:
        return Path(configured).expanduser().resolve()
    return Path.home() / ".local" / "state" / "moyva-pass-runner" / ROOT.name


def load_state(required: bool = True) -> dict[str, Any]:
    if not STATE_PATH.exists():
        if required:
            raise RuntimeError("No active pass. Run 'begin' first.")
        return {}
    return json.loads(STATE_PATH.read_text(encoding="utf-8"))


def save_state(state: dict[str, Any]) -> None:
    STATE_PATH.write_text(json.dumps(state, indent=2, sort_keys=True) + "\n", encoding="utf-8")


def clear_state() -> None:
    STATE_PATH.unlink(missing_ok=True)


def slugify(value: str) -> str:
    slug = re.sub(r"[^a-z0-9]+", "-", value.casefold()).strip("-")
    return slug or "pass"


def require_clean(path: Path) -> None:
    if git("status", "--porcelain=v1", cwd=path).strip():
        raise RuntimeError(f"Worktree is not clean: {path}")


def worktree_fingerprint(path: Path) -> str:
    digest = hashlib.sha256()
    status = git("status", "--porcelain=v1", "-z", "--untracked-files=all", cwd=path)
    digest.update(status.encode("utf-8"))
    for entry in status.split("\0"):
        if len(entry) < 4:
            continue
        relative = entry[3:].split(" -> ")[-1]
        candidate = path / relative
        if candidate.is_file():
            digest.update(relative.encode("utf-8"))
            digest.update(candidate.read_bytes())
    digest.update(git("diff", "--binary", "HEAD", cwd=path).encode("utf-8"))
    digest.update(git("diff", "--binary", "--cached", "HEAD", cwd=path).encode("utf-8"))
    return digest.hexdigest()


def changed_paths(path: Path) -> set[str]:
    status = git("status", "--porcelain=v1", "-z", "--untracked-files=all", cwd=path)
    return {
        entry[3:].split(" -> ")[-1]
        for entry in status.split("\0")
        if len(entry) >= 4 and entry[:2].strip()
    }


def restore_unity_importer_churn(path: Path, dirty_before: set[str]) -> None:
    dirty_after = changed_paths(path)
    generated_only = [
        candidate
        for candidate in UNITY_IMPORTER_CHURN_PATHS
        if candidate not in dirty_before and candidate in dirty_after
    ]
    if generated_only:
        git("restore", "--worktree", "--", *generated_only, cwd=path)


def begin(args: argparse.Namespace) -> int:
    if load_state(required=False):
        raise RuntimeError("Another pass is active. Check it with 'status'.")
    require_clean(ROOT)
    base_sha = git("rev-parse", args.base_ref).strip()
    timestamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    slug = slugify(args.name)
    branch = args.branch or f"moyva-pass/{slug}-{timestamp.casefold()}"
    worktree = (
        Path(args.worktree).expanduser().resolve()
        if args.worktree
        else Path("/tmp") / "moyva-pass-worktrees" / ROOT.name / f"{slug}-{timestamp}"
    )
    worktree.parent.mkdir(parents=True, exist_ok=True)
    if worktree.exists():
        raise RuntimeError(f"Worktree path already exists: {worktree}")
    git("worktree", "add", "-b", branch, str(worktree), base_sha)
    artifacts = artifact_root() / f"{timestamp}-{slug}"
    artifacts.mkdir(parents=True, exist_ok=False)
    state = {
        "name": args.name,
        "slug": slug,
        "base_sha": base_sha,
        "branch": branch,
        "integration_worktree": str(ROOT),
        "worktree": str(worktree),
        "artifacts": str(artifacts),
        "attempts": 0,
        "created_utc": timestamp,
        "last_verified_fingerprint": None,
    }
    save_state(state)
    print(worktree)
    return 0


def verification_commands(state: dict[str, Any], args: argparse.Namespace) -> list[list[str]]:
    base = state["base_sha"]
    commands = [
        ["git", "diff", "--check", base],
        [
            "python3",
            "tools/quality/context_budget.py",
            "check",
            "--base-ref",
            base,
            "--changed-only",
            "--strict",
        ],
        [
            "python3",
            "tools/quality/no_procedural_visuals.py",
            "--base-ref",
            base,
            "--changed-only",
            "--strict",
        ],
        [
            "python3",
            "tools/quality/documentation_coverage.py",
            "--base-ref",
            base,
            "--changed-only",
            "--strict",
        ],
        [
            "python3",
            "tools/quality/feature_dependency_map.py",
            "--output",
            str(Path(state["artifacts"]) / "feature-dependency-map.md"),
            "--check-cycles",
        ],
    ]
    commands.extend(shlex.split(value) for value in args.command)
    if not args.skip_unity:
        commands.append(
            [
                "unity",
                "run",
                state["worktree"],
                "--timeout",
                str(args.unity_timeout),
                "--",
                "-nographics",
                "-executeMethod",
                "Kruty1918.Moyva.Editor.Shared.PrefabOnlyProjectValidator.ValidateBatch",
                "-logFile",
                str(Path(state["artifacts"]) / "unity-compile.log"),
            ]
        )
    return commands


def archive_failure(state: dict[str, Any], reason: str) -> Path:
    worktree = Path(state["worktree"])
    artifacts = Path(state["artifacts"])
    artifacts.mkdir(parents=True, exist_ok=True)
    (artifacts / "failure.txt").write_text(reason.rstrip() + "\n", encoding="utf-8")
    (artifacts / "state.json").write_text(
        json.dumps(state, indent=2, sort_keys=True) + "\n", encoding="utf-8"
    )
    if worktree.exists():
        (artifacts / "changes.patch").write_text(
            git("diff", "--binary", state["base_sha"], cwd=worktree, check=False),
            encoding="utf-8",
        )
        untracked = [
            line
            for line in git("ls-files", "--others", "--exclude-standard", cwd=worktree).splitlines()
            if line
        ]
        if untracked:
            with tarfile.open(artifacts / "untracked.tar.gz", "w:gz") as archive:
                for relative in untracked:
                    candidate = worktree / relative
                    if candidate.exists():
                        archive.add(candidate, arcname=relative)
    return artifacts


def discard_isolated_pass(state: dict[str, Any]) -> None:
    worktree = Path(state["worktree"])
    integration_worktree = Path(state["integration_worktree"])
    if worktree.exists():
        git("worktree", "remove", "--force", str(worktree), cwd=integration_worktree)
    git("branch", "-D", state["branch"], cwd=integration_worktree, check=False)
    clear_state()


def verify(args: argparse.Namespace) -> int:
    state = load_state()
    worktree = Path(state["worktree"])
    artifacts = Path(state["artifacts"])
    attempts = int(state.get("attempts", 0)) + 1
    state["attempts"] = attempts
    log_path = artifacts / f"verify-{attempts}.log"
    output: list[str] = []
    failed = False
    for command in verification_commands(state, args):
        dirty_before_unity = changed_paths(worktree) if command[:2] == ["unity", "run"] else set()
        output.append(f"$ {shlex.join(command)}\n")
        result = run(command, cwd=worktree, check=False)
        if dirty_before_unity or command[:2] == ["unity", "run"]:
            restore_unity_importer_churn(worktree, dirty_before_unity)
        output.append(result.stdout or "")
        output.append(f"\n[exit {result.returncode}]\n\n")
        if result.returncode != 0:
            failed = True
            break
    log_path.write_text("".join(output), encoding="utf-8")
    if failed:
        state["last_verified_fingerprint"] = None
        save_state(state)
        print(f"Verification failed ({attempts}/{MAX_FAILED_ATTEMPTS}). Log: {log_path}")
        if attempts >= MAX_FAILED_ATTEMPTS:
            archive = archive_failure(state, f"Verification failed after {attempts} attempts.")
            discard_isolated_pass(state)
            print(f"Pass discarded; recovery artifacts: {archive}")
        return 1
    state["last_verified_fingerprint"] = worktree_fingerprint(worktree)
    state["last_verified_utc"] = dt.datetime.now(dt.timezone.utc).isoformat()
    save_state(state)
    print(f"Verification passed. Log: {log_path}")
    return 0


def checkpoint(args: argparse.Namespace) -> int:
    state = load_state()
    verification_args = argparse.Namespace(
        command=args.command,
        skip_unity=args.skip_unity,
        unity_timeout=args.unity_timeout,
    )
    if verify(verification_args) != 0:
        return 1
    state = load_state()
    worktree = Path(state["worktree"])
    if state.get("last_verified_fingerprint") != worktree_fingerprint(worktree):
        raise RuntimeError("Worktree changed after verification.")
    git("add", "--all", cwd=worktree)
    git("commit", "-m", args.message, cwd=worktree)
    commit = git("rev-parse", "HEAD", cwd=worktree).strip()
    state["checkpoint_sha"] = commit
    save_state(state)
    print(commit)
    if args.integrate:
        integration_worktree = Path(state["integration_worktree"])
        require_clean(integration_worktree)
        if git("rev-parse", "HEAD", cwd=integration_worktree).strip() != state["base_sha"]:
            raise RuntimeError("Main worktree moved; integrate the checkpoint manually.")
        git("merge", "--ff-only", commit, cwd=integration_worktree)
        git("worktree", "remove", str(worktree), cwd=integration_worktree)
        git("branch", "-d", state["branch"], cwd=integration_worktree, check=False)
        clear_state()
    return 0


def abort(_: argparse.Namespace) -> int:
    state = load_state()
    archive = archive_failure(state, "Pass aborted explicitly.")
    discard_isolated_pass(state)
    print(archive)
    return 0


def status(_: argparse.Namespace) -> int:
    state = load_state(required=False)
    if not state:
        print("No active pass.")
        return 0
    print(json.dumps(state, indent=2, sort_keys=True))
    return 0


def add_verification_options(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--command", action="append", default=[], help="Additional verification command.")
    parser.add_argument("--skip-unity", action="store_true", help="Skip Unity batch compilation.")
    parser.add_argument("--unity-timeout", type=int, default=1800)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="action", required=True)
    begin_parser = commands.add_parser("begin")
    begin_parser.add_argument("name")
    begin_parser.add_argument("--base-ref", default="HEAD")
    begin_parser.add_argument("--branch")
    begin_parser.add_argument("--worktree")
    begin_parser.set_defaults(handler=begin)
    verify_parser = commands.add_parser("verify")
    add_verification_options(verify_parser)
    verify_parser.set_defaults(handler=verify)
    checkpoint_parser = commands.add_parser("checkpoint")
    checkpoint_parser.add_argument("--message", required=True)
    checkpoint_parser.add_argument("--integrate", action="store_true")
    add_verification_options(checkpoint_parser)
    checkpoint_parser.set_defaults(handler=checkpoint)
    abort_parser = commands.add_parser("abort")
    abort_parser.set_defaults(handler=abort)
    status_parser = commands.add_parser("status")
    status_parser.set_defaults(handler=status)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    try:
        return args.handler(args)
    except (OSError, RuntimeError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
