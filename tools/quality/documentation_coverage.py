#!/usr/bin/env python3
"""Report and guard XML documentation coverage for Moyva-owned C# declarations."""

from __future__ import annotations

import argparse
import collections
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Optional


ROOT = Path(__file__).resolve().parents[2]
SCRIPTS_ROOT = ROOT / "Assets" / "Moyva" / "Scripts"
EXCLUDED_PARTS = {"Tests"}
TYPE_RE = re.compile(
    r"^\s*(?:(?:public|internal|private|protected|static|sealed|abstract|partial|readonly|ref|unsafe|new)\s+)*"
    r"(?P<kind>class|interface|struct|record(?:\s+(?:class|struct))?|enum|delegate)\s+"
    r"(?P<name>[A-Za-z_][A-Za-z0-9_]*)"
)
ACCESS_MEMBER_RE = re.compile(
    r"^\s*(?:(?:public|protected)(?:\s+internal)?|internal\s+protected)\b"
)
CONTROL_RE = re.compile(r"^\s*(?:if|for|foreach|while|switch|catch|using|lock|return|throw)\b")
IDENTIFIER_RE = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*$")
UKRAINIAN_RE = re.compile(r"[А-Яа-яІіЇїЄєҐґ]")
SUMMARY_RE = re.compile(r"<summary\b[^>]*>(?P<body>.*?)</summary>", re.DOTALL | re.IGNORECASE)


@dataclass(frozen=True)
class MissingDocumentation:
    path: str
    line: int
    category: str
    symbol: str
    reason: str


@dataclass(frozen=True)
class Coverage:
    declarations: int
    documented: int
    missing: tuple[MissingDocumentation, ...]


def run_git(*args: str, check: bool = True) -> str:
    result = subprocess.run(
        ["git", *args], cwd=ROOT, capture_output=True, text=True, check=False
    )
    if check and result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "git command failed")
    return result.stdout


def normalize(path: Path | str) -> str:
    candidate = Path(path)
    if candidate.is_absolute():
        candidate = candidate.relative_to(ROOT)
    return candidate.as_posix()


def is_candidate(path: Path) -> bool:
    try:
        relative = path.relative_to(SCRIPTS_ROOT)
    except ValueError:
        return False
    if path.suffix != ".cs" or EXCLUDED_PARTS.intersection(relative.parts):
        return False
    return not path.name.endswith((".g.cs", ".generated.cs", ".Designer.cs"))


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
        if line.strip().startswith("Assets/Moyva/Scripts/")
        and line.strip().endswith(".cs")
    }


def iter_files(paths: Iterable[str], changed_only: bool, base_ref: Optional[str]) -> list[Path]:
    selected: set[Path] = set()
    for raw in paths:
        candidate = (ROOT / raw).resolve()
        if candidate.is_file() and is_candidate(candidate):
            selected.add(candidate)
        elif candidate.is_dir():
            selected.update(path for path in candidate.rglob("*.cs") if is_candidate(path))

    if not selected:
        selected.update(path for path in SCRIPTS_ROOT.rglob("*.cs") if is_candidate(path))

    if changed_only:
        if not base_ref:
            raise ValueError("--changed-only requires --base-ref")
        changed = changed_paths(base_ref)
        selected = {path for path in selected if normalize(path) in changed}
    return sorted(selected)


def strip_inline_comments(line: str) -> str:
    result: list[str] = []
    index = 0
    in_string = False
    in_char = False
    verbatim = False
    while index < len(line):
        char = line[index]
        nxt = line[index + 1] if index + 1 < len(line) else ""
        if not in_string and not in_char and char == "/" and nxt == "/":
            break
        result.append(char)
        if in_string:
            if verbatim and char == '"' and nxt == '"':
                result.append(nxt)
                index += 2
                continue
            if char == "\\" and not verbatim and nxt:
                result.append(nxt)
                index += 2
                continue
            if char == '"':
                in_string = False
                verbatim = False
        elif in_char:
            if char == "\\" and nxt:
                result.append(nxt)
                index += 2
                continue
            if char == "'":
                in_char = False
        elif char == '@' and nxt == '"':
            result.append(nxt)
            in_string = True
            verbatim = True
            index += 2
            continue
        elif char == '"':
            in_string = True
        elif char == "'":
            in_char = True
        index += 1
    return "".join(result)


def symbol_from_member(line: str) -> str:
    code = line.strip().rstrip("{;=").strip()
    before_parameters = code.split("(", 1)[0].strip()
    tokens = re.findall(r"[A-Za-z_][A-Za-z0-9_]*", before_parameters)
    if "operator" in tokens:
        return "operator"
    return tokens[-1] if tokens else code[:80]


def looks_like_interface_member(code: str) -> bool:
    stripped = code.strip()
    if not stripped or stripped.startswith(("[", "#", "{", "}")):
        return False
    if TYPE_RE.match(stripped) or CONTROL_RE.match(stripped):
        return False
    return any(token in stripped for token in ("(", "{", "=>")) or stripped.endswith(";")


def documentation_problem(lines: list[str]) -> Optional[str]:
    if not lines:
        return "missing-summary"
    documentation = "\n".join(lines)
    if re.search(r"<inheritdoc\b", documentation, re.IGNORECASE):
        return None
    summary = SUMMARY_RE.search(documentation)
    if summary is None:
        return "missing-summary"
    if not UKRAINIAN_RE.search(summary.group("body")):
        return "summary-not-ukrainian"
    return None


def analyze(path: str, source: str) -> Coverage:
    missing: list[MissingDocumentation] = []
    declarations = 0
    documented = 0
    pending_doc_lines: list[str] = []
    pending_attributes = False
    brace_depth = 0
    scope_stack: list[tuple[str, int]] = []
    pending_type_scope: Optional[str] = None

    for line_number, raw in enumerate(source.splitlines(), start=1):
        stripped = raw.strip()
        if stripped.startswith("///"):
            pending_doc_lines.append(stripped[3:].strip())
            continue
        if stripped.startswith("[") and not stripped.startswith("[assembly:"):
            pending_attributes = bool(pending_doc_lines) or pending_attributes
            continue
        if not stripped:
            if not pending_attributes:
                pending_doc_lines.clear()
            continue
        if stripped.startswith(("//", "/*", "*", "#")):
            if not pending_attributes:
                pending_doc_lines.clear()
            continue

        code = strip_inline_comments(raw)
        type_match = TYPE_RE.match(code)
        current_scope = scope_stack[-1][0] if scope_stack else None
        is_type = type_match is not None
        is_member = False
        category = ""
        symbol = ""

        if is_type:
            category = "type"
            symbol = type_match.group("name")
            pending_type_scope = type_match.group("kind").split()[0]
        elif ACCESS_MEMBER_RE.match(code) and not CONTROL_RE.match(code):
            is_member = True
            category = "member"
            symbol = symbol_from_member(code)
        elif current_scope == "interface" and looks_like_interface_member(code):
            is_member = True
            category = "interface-member"
            symbol = symbol_from_member(code)
        elif current_scope == "enum" and brace_depth == scope_stack[-1][1]:
            candidate = stripped.rstrip(",").split("=", 1)[0].strip()
            if IDENTIFIER_RE.match(candidate):
                is_member = True
                category = "enum-member"
                symbol = candidate

        if is_type or is_member:
            declarations += 1
            problem = documentation_problem(pending_doc_lines)
            if problem is None:
                documented += 1
            else:
                missing.append(MissingDocumentation(path, line_number, category, symbol, problem))
            pending_doc_lines.clear()
            pending_attributes = False
        elif not stripped.startswith("["):
            pending_doc_lines.clear()
            pending_attributes = False

        opens = code.count("{")
        closes = code.count("}")
        if pending_type_scope and opens > 0:
            scope_stack.append((pending_type_scope, brace_depth + 1))
            pending_type_scope = None
        brace_depth += opens - closes
        while scope_stack and brace_depth < scope_stack[-1][1]:
            scope_stack.pop()

    return Coverage(declarations, documented, tuple(missing))


def source_at_base(base_ref: str, path: str) -> str:
    return run_git("show", f"{base_ref}:{path}", check=False)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--path", action="append", default=[])
    parser.add_argument("--base-ref")
    parser.add_argument("--changed-only", action="store_true")
    parser.add_argument("--strict", action="store_true")
    parser.add_argument("--summary-only", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    try:
        files = iter_files(args.path, args.changed_only, args.base_ref)
    except (RuntimeError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2

    totals = collections.Counter()
    current_missing: list[MissingDocumentation] = []
    baseline_missing_by_path: dict[str, collections.Counter[tuple[str, str]]] = {}

    for path in files:
        relative = normalize(path)
        coverage = analyze(relative, path.read_text(encoding="utf-8", errors="replace"))
        totals["declarations"] += coverage.declarations
        totals["documented"] += coverage.documented
        current_missing.extend(coverage.missing)

        if args.base_ref:
            source = source_at_base(args.base_ref, relative)
            if source:
                baseline_missing_by_path[relative] = collections.Counter(
                    (item.category, item.symbol) for item in analyze(relative, source).missing
                )

    current_missing_by_path: dict[str, collections.Counter[tuple[str, str]]] = {}
    for item in current_missing:
        current_missing_by_path.setdefault(item.path, collections.Counter())[
            (item.category, item.symbol)
        ] += 1

    failing_symbols: set[tuple[str, str, str]] = set()
    for path, current_counts in current_missing_by_path.items():
        baseline_counts = baseline_missing_by_path.get(path, collections.Counter())
        for (category, symbol), count in current_counts.items():
            if count > (baseline_counts[(category, symbol)] if args.base_ref else 0):
                failing_symbols.add((path, category, symbol))

    failing_paths = {path for path, _, _ in failing_symbols}

    if not args.summary_only:
        for item in current_missing:
            key = (item.path, item.category, item.symbol)
            status = "FAIL" if key in failing_symbols else "DEBT"
            print(
                f"{status} {item.path}:{item.line} [{item.category}/{item.reason}] {item.symbol}"
            )

    declarations = totals["declarations"]
    documented = totals["documented"]
    percentage = 100.0 if declarations == 0 else documented * 100.0 / declarations
    print(f"\nChecked files: {len(files)}")
    print(f"Declarations:  {declarations}")
    print(f"Documented:    {documented}")
    print(f"Coverage:      {percentage:.1f}%")
    print(f"Missing:       {len(current_missing)}")
    print(f"New debt files:{len(failing_paths):>5}")
    return 1 if args.strict and failing_paths else 0


if __name__ == "__main__":
    raise SystemExit(main())
