#!/usr/bin/env python3
"""Scope validator for one audit task.

Verifies that every tracked change relative to --base (plus untracked,
non-ignored files in the worktree) stays inside the task's manifest whitelist.
Also reports the tree SHA of HEAD vs base so the wave can prove identical
starting content (they differ legitimately once the task commits its own files).

Default whitelist: tools/ai/moyva_parallel_audit/<TASK-ID>/**
Usage:
  python tools/ai/moyva_parallel_audit/A00/validate_scope.py \
      --base 98d86e11f1fd905d7e90b6b7a4e8c30dc6b20e38 --task-id A00

Exit codes: 0 = in scope, 1 = out-of-scope paths found, 2 = usage/git error.
"""

import argparse
import fnmatch
import os
import subprocess
import sys


def git(args, cwd):
    return subprocess.run(
        ["git"] + args, cwd=cwd, capture_output=True, text=True, check=True
    ).stdout


def changed_paths(root, base):
    """Tracked changes vs base + staged/unstaged + untracked non-ignored files."""
    paths = set()
    out = git(["diff", "--name-only", f"{base}...HEAD"], root)
    paths.update(p for p in out.splitlines() if p)
    out = git(["diff", "--name-only", base], root)  # working tree vs base
    paths.update(p for p in out.splitlines() if p)
    out = git(["ls-files", "--others", "--exclude-standard"], root)
    paths.update(p for p in out.splitlines() if p)
    out = git(["ls-files", "--modified", "--exclude-standard"], root)
    paths.update(p for p in out.splitlines() if p)
    return sorted(paths)


def allowed(path, patterns):
    norm = path.replace("\\", "/")
    for pat in patterns:
        pat = pat.rstrip("/")
        if pat.endswith("/**"):
            if norm.startswith(pat[:-3] + "/"):
                return True
        elif fnmatch.fnmatch(norm, pat) or norm == pat:
            return True
    return False


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--base", required=True, help="pinned base commit SHA")
    ap.add_argument("--task-id", required=True, help="e.g. A00")
    ap.add_argument("--allow", action="append", default=[],
                    help="extra allowed glob (repeatable); "
                         "'dir/**' covers everything under dir/")
    args = ap.parse_args(argv)

    root = git(["rev-parse", "--show-toplevel"], os.getcwd()).strip()
    head = git(["rev-parse", "HEAD"], root).strip()
    head_tree = git(["rev-parse", "HEAD^{tree}"], root).strip()
    base_tree = git(["rev-parse", f"{args.base}^{{tree}}"], root).strip()

    patterns = [f"tools/ai/moyva_parallel_audit/{args.task_id}/**"] + args.allow
    paths = changed_paths(root, args.base)
    violations = [p for p in paths if not allowed(p, patterns)]

    print(f"base commit : {args.base}")
    print(f"base tree   : {base_tree}")
    print(f"HEAD commit : {head}")
    print(f"HEAD tree   : {head_tree}")
    print(f"tree match  : {'identical' if head_tree == base_tree else 'differs (expected once task files are committed)'}")
    print(f"whitelist   : {patterns}")
    print(f"changed     : {len(paths)} file(s)")
    for p in paths:
        print(f"  {'ok ' if p not in violations else 'OUT'} {p}")

    if violations:
        print("SCOPE FAIL: files outside whitelist:", file=sys.stderr)
        for p in violations:
            print(f"  {p}", file=sys.stderr)
        print("Move the change under the task whitelist or open an "
              "integration ticket in Temp/ai/moyva-parallel/<ID>/integration_requests/.",
              file=sys.stderr)
        return 1
    print("SCOPE PASS")
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except subprocess.CalledProcessError as exc:
        sys.exit(f"git error: {exc.stderr.strip() if exc.stderr else exc}")
