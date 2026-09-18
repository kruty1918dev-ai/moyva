#!/usr/bin/env python3
"""Measure and guard the C# context surface used by humans and coding agents."""

from __future__ import annotations

import argparse
import json
import math
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Optional


ROOT = Path(__file__).resolve().parents[2]
SCRIPTS_ROOT = ROOT / "Assets" / "Moyva" / "Scripts"
ALLOWLIST_PATH = Path(__file__).with_name("context-budget-allowlist.json")
EXCLUDED_PARTS = {"Editor", "EditorShared", "Tests", "Development"}
ENTRYPOINT_SUFFIXES = (
    "Installer",
    "Controller",
    "Coordinator",
    "Orchestrator",
    "Presenter",
)
DEFAULT_HARD_LINES = 500
DEFAULT_WARN_LINES = 350
LEAF_HARD_LINES = 800
LEAF_WARN_LINES = 500


@dataclass(frozen=True)
class Metrics:
    path: str
    module: str
    role: str
    lines: int
    effective_lines: int
    code_context_tokens: int
    documentation_tokens: int
    context_tokens: int


@dataclass(frozen=True)
class Limit:
    warn_lines: int
    hard_lines: int
    reason: str = ""


def run_git(*args: str, check: bool = True) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    if check and result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "git command failed")
    return result.stdout


def normalize_path(path: Path | str) -> str:
    candidate = Path(path)
    if candidate.is_absolute():
        candidate = candidate.relative_to(ROOT)
    return candidate.as_posix()


def is_production_file(path: Path) -> bool:
    try:
        relative = path.relative_to(SCRIPTS_ROOT)
    except ValueError:
        return False
    return path.suffix == ".cs" and not EXCLUDED_PARTS.intersection(relative.parts)


def module_for(path: Path) -> str:
    parts = path.relative_to(SCRIPTS_ROOT).parts
    if len(parts) >= 2 and parts[0] == "Features":
        return parts[1]
    return parts[0] if parts else "Unknown"


def role_for(path: Path) -> str:
    relative = path.relative_to(SCRIPTS_ROOT)
    stem = path.stem.split(".", 1)[0]
    if "API" in relative.parts:
        return "api"
    if stem.endswith(ENTRYPOINT_SUFFIXES):
        return "entrypoint"
    if "Diagnostics" in relative.parts:
        return "diagnostics"
    return "leaf"


def strip_comments(source: str) -> str:
    """Remove C# comments while preserving strings, chars and line positions."""
    output: list[str] = []
    i = 0
    state = "code"
    verbatim = False
    while i < len(source):
        char = source[i]
        nxt = source[i + 1] if i + 1 < len(source) else ""

        if state == "line_comment":
            if char == "\n":
                output.append(char)
                state = "code"
            else:
                output.append(" ")
            i += 1
            continue

        if state == "block_comment":
            if char == "*" and nxt == "/":
                output.extend((" ", " "))
                i += 2
                state = "code"
            else:
                output.append("\n" if char == "\n" else " ")
                i += 1
            continue

        if state in {"string", "char"}:
            output.append(char)
            if verbatim and state == "string" and char == '"' and nxt == '"':
                output.append(nxt)
                i += 2
                continue
            if char == "\\" and not verbatim:
                if nxt:
                    output.append(nxt)
                    i += 2
                    continue
            delimiter = '"' if state == "string" else "'"
            if char == delimiter:
                state = "code"
                verbatim = False
            i += 1
            continue

        if char == "/" and nxt == "/":
            output.extend((" ", " "))
            i += 2
            state = "line_comment"
            continue
        if char == "/" and nxt == "*":
            output.extend((" ", " "))
            i += 2
            state = "block_comment"
            continue
        if char == "@" and nxt == '"':
            output.extend((char, nxt))
            i += 2
            state = "string"
            verbatim = True
            continue
        if char == '"':
            output.append(char)
            i += 1
            state = "string"
            continue
        if char == "'":
            output.append(char)
            i += 1
            state = "char"
            continue

        output.append(char)
        i += 1

    return "".join(output)


def measure_source(path: str, source: str) -> Metrics:
    file_path = ROOT / path
    without_comments = strip_comments(source)
    effective_lines = sum(1 for line in without_comments.splitlines() if line.strip())
    source_non_whitespace = re.sub(r"\s+", "", source)
    code_non_whitespace = re.sub(r"\s+", "", without_comments)
    xml_documentation = "\n".join(
        line for line in source.splitlines() if line.lstrip().startswith("///")
    )
    documentation_non_whitespace = re.sub(r"\s+", "", xml_documentation)
    source_bytes = len(source_non_whitespace.encode("utf-8"))
    code_bytes = len(code_non_whitespace.encode("utf-8"))
    context_tokens = math.ceil(source_bytes / 4)
    code_context_tokens = math.ceil(code_bytes / 4)
    documentation_tokens = math.ceil(len(documentation_non_whitespace.encode("utf-8")) / 4)
    return Metrics(
        path=path,
        module=module_for(file_path),
        role=role_for(file_path),
        lines=len(source.splitlines()),
        effective_lines=effective_lines,
        code_context_tokens=code_context_tokens,
        documentation_tokens=documentation_tokens,
        context_tokens=context_tokens,
    )


def measure_file(path: Path) -> Metrics:
    relative = normalize_path(path)
    return measure_source(relative, path.read_text(encoding="utf-8", errors="replace"))


def load_allowlist() -> dict[str, Limit]:
    if not ALLOWLIST_PATH.exists():
        return {}
    payload = json.loads(ALLOWLIST_PATH.read_text(encoding="utf-8"))
    result: dict[str, Limit] = {}
    for path, entry in payload.get("algorithmLeaves", {}).items():
        result[path] = Limit(
            warn_lines=int(entry.get("warnEffectiveLines", LEAF_WARN_LINES)),
            hard_lines=int(entry["maxEffectiveLines"]),
            reason=str(entry.get("reason", "allowlisted algorithm leaf")),
        )
    return result


def limit_for(metrics: Metrics, allowlist: dict[str, Limit]) -> Limit:
    if metrics.path in allowlist:
        return allowlist[metrics.path]
    if metrics.role in {"api", "entrypoint"}:
        return Limit(DEFAULT_WARN_LINES, DEFAULT_HARD_LINES)
    return Limit(LEAF_WARN_LINES, LEAF_HARD_LINES)


def iter_production_files(paths: Iterable[str], feature: Optional[str]) -> list[Path]:
    selected: set[Path] = set()
    for raw in paths:
        candidate = (ROOT / raw).resolve()
        if candidate.is_file():
            if is_production_file(candidate):
                selected.add(candidate)
            continue
        if candidate.is_dir():
            selected.update(path for path in candidate.rglob("*.cs") if is_production_file(path))

    if not selected:
        selected.update(path for path in SCRIPTS_ROOT.rglob("*.cs") if is_production_file(path))

    if feature:
        selected = {path for path in selected if module_for(path).casefold() == feature.casefold()}
    return sorted(selected)


def changed_paths(base_ref: str) -> set[str]:
    outputs = (
        run_git("diff", "--name-only", f"{base_ref}...HEAD"),
        run_git("diff", "--name-only"),
        run_git("diff", "--cached", "--name-only"),
        run_git("ls-files", "--others", "--exclude-standard"),
    )
    return {
        line.strip()
        for output in outputs
        for line in output.splitlines()
        if line.strip().startswith("Assets/Moyva/Scripts/") and line.strip().endswith(".cs")
    }


def measure_base(base_ref: str, path: str) -> Optional[Metrics]:
    source = run_git("show", f"{base_ref}:{path}", check=False)
    if not source:
        return None
    return measure_source(path, source)


def print_report(metrics: list[Metrics], top: int, as_json: bool) -> None:
    totals: dict[str, dict[str, int]] = {}
    for item in metrics:
        aggregate = totals.setdefault(
            item.module,
            {
                "files": 0,
                "lines": 0,
                "effective_lines": 0,
                "code_context_tokens": 0,
                "documentation_tokens": 0,
                "context_tokens": 0,
            },
        )
        aggregate["files"] += 1
        aggregate["lines"] += item.lines
        aggregate["effective_lines"] += item.effective_lines
        aggregate["code_context_tokens"] += item.code_context_tokens
        aggregate["documentation_tokens"] += item.documentation_tokens
        aggregate["context_tokens"] += item.context_tokens

    if as_json:
        print(json.dumps({"modules": totals, "files": [item.__dict__ for item in metrics]}, indent=2))
        return

    print("MODULE                 FILES      LOC  EFFECTIVE     CODE_TOKENS     DOC_TOKENS  SOURCE_TOKENS")
    for module, values in sorted(
        totals.items(),
        key=lambda item: item[1]["code_context_tokens"],
        reverse=True,
    ):
        print(
            f"{module:<22} {values['files']:>5} {values['lines']:>8} "
            f"{values['effective_lines']:>10} {values['code_context_tokens']:>15} "
            f"{values['documentation_tokens']:>14} {values['context_tokens']:>14}"
        )

    all_lines = sum(item.lines for item in metrics)
    all_effective = sum(item.effective_lines for item in metrics)
    all_code_tokens = sum(item.code_context_tokens for item in metrics)
    all_documentation_tokens = sum(item.documentation_tokens for item in metrics)
    all_tokens = sum(item.context_tokens for item in metrics)
    print(
        f"TOTAL                  {len(metrics):>5} {all_lines:>8} {all_effective:>10} "
        f"{all_code_tokens:>15} {all_documentation_tokens:>14} {all_tokens:>14}"
    )

    if top > 0:
        print("\nLARGEST CONTEXT FILES")
        for item in sorted(metrics, key=lambda value: value.code_context_tokens, reverse=True)[:top]:
            print(
                f"{item.code_context_tokens:>8} code tokens "
                f"{item.documentation_tokens:>7} docs {item.effective_lines:>5} effective "
                f"[{item.role}] {item.path}"
            )


def run_check(metrics: list[Metrics], base_ref: Optional[str], strict: bool) -> int:
    allowlist = load_allowlist()
    warnings = 0
    failures = 0

    for item in metrics:
        limit = limit_for(item, allowlist)
        if item.effective_lines <= limit.warn_lines:
            continue

        status = "WARN"
        if item.effective_lines > limit.hard_lines:
            status = "FAIL"
            if base_ref:
                baseline = measure_base(base_ref, item.path)
                if baseline and baseline.effective_lines > limit.hard_lines:
                    status = "DEBT" if item.effective_lines <= baseline.effective_lines else "FAIL"

        if status == "FAIL":
            failures += 1
        else:
            warnings += 1

        allowlist_note = f" allowlist={limit.reason}" if limit.reason else ""
        print(
            f"{status} [{item.role}] {item.path} :: effective={item.effective_lines}, "
            f"code_tokens~={item.code_context_tokens}, docs~={item.documentation_tokens}, "
            f"source_tokens~={item.context_tokens}, warn={limit.warn_lines}, hard={limit.hard_lines}"
            f"{allowlist_note}"
        )

    print(f"\nChecked files: {len(metrics)}")
    print(f"Warnings:      {warnings}")
    print(f"Failures:      {failures}")
    return 1 if strict and failures else 0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("command", choices=("report", "check"), nargs="?", default="report")
    parser.add_argument("--feature", help="Limit the scan to one feature/module.")
    parser.add_argument("--path", action="append", default=[], help="File or directory to include; repeatable.")
    parser.add_argument("--top", type=int, default=20, help="Largest files shown by report.")
    parser.add_argument("--json", action="store_true", help="Emit report as JSON.")
    parser.add_argument("--base-ref", help="Git baseline used for no-regression checks.")
    parser.add_argument("--changed-only", action="store_true", help="Check files changed from --base-ref.")
    parser.add_argument("--strict", action="store_true", help="Return non-zero for hard-limit failures.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if args.changed_only and not args.base_ref:
        print("ERROR: --changed-only requires --base-ref", file=sys.stderr)
        return 2

    files = iter_production_files(args.path, args.feature)
    if args.changed_only:
        changed = changed_paths(args.base_ref)
        files = [path for path in files if normalize_path(path) in changed]

    metrics = [measure_file(path) for path in files]
    if args.command == "report":
        print_report(metrics, max(args.top, 0), args.json)
        return 0
    return run_check(metrics, args.base_ref, args.strict)


if __name__ == "__main__":
    raise SystemExit(main())
