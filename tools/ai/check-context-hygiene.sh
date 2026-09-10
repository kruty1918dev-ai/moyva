#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.."

python3 - <<'PY'
import os
import re
import subprocess
import sys
from pathlib import Path, PurePosixPath

max_text_artifact_bytes = int(os.environ.get("AI_CONTEXT_MAX_TEXT_ARTIFACT_BYTES", str(1024 * 1024)))

tracked = subprocess.check_output(["git", "ls-files", "-z"])
paths = [p.decode("utf-8", "replace") for p in tracked.split(b"\0") if p]

forbidden_patterns = [
    re.compile(r"^text\.txt$"),
    re.compile(r"^tests/TestResults_.*\.xml$"),
    re.compile(r"^tests/.*\.tsv$"),
    re.compile(r"^Assets/__ChatGPT"),
    re.compile(r"^Assets/_Recovery(?:/|\.meta$)"),
]

large_text_extensions = {
    ".log",
    ".txt",
    ".xml",
    ".tsv",
}

violations = []

for document, limit in {"AGENTS.md": 6 * 1024, "CODEMAP.md": 16 * 1024}.items():
    path = Path(document)
    if not path.is_file():
        violations.append(("missing context document", document))
    elif path.stat().st_size > limit:
        violations.append((f"context budget exceeded ({path.stat().st_size} > {limit} bytes)", document))

source_root = Path("Assets/Moyva/Scripts").resolve()

def check_reference(value, base, location, directory=False):
    relative = PurePosixPath(value)
    if relative.is_absolute() or ".." in relative.parts or "\\" in value:
        violations.append((f"invalid CODEMAP path: {value}", location))
        return
    target = (base / value).resolve()
    if not target.is_relative_to(source_root):
        violations.append((f"CODEMAP path leaves production root: {value}", location))
    elif not (target.is_dir() if directory else target.is_file()):
        violations.append((f"missing CODEMAP {'directory' if directory else 'file'}: {value}", location))

codemap = Path("CODEMAP.md")
if codemap.is_file():
    base = None
    source_extensions = {".cs", ".asmdef", ".asmref", ".json", ".unity", ".prefab",
                         ".asset", ".meta", ".shader", ".hlsl", ".html", ".uxml", ".uss"}
    for number, line in enumerate(codemap.read_text(encoding="utf-8").splitlines(), 1):
        location = f"CODEMAP.md:{number}"
        if line.startswith("## "):
            base = None
        if line.startswith("Base:"):
            match = re.fullmatch(r"Base: `([^`]+)`", line)
            if not match:
                violations.append(("expected Base: `production-relative-directory/`", location))
                base = None
                continue
            value = match.group(1)
            before = len(violations)
            check_reference(value, source_root, location, directory=True)
            base = source_root / value if len(violations) == before else None
            continue
        for value in re.findall(r"`([^`]+)`", line):
            if not (line.startswith("|") or value.endswith("/") or Path(value).suffix in source_extensions):
                continue
            if base is None:
                violations.append((f"CODEMAP path has no valid Base: {value}", location))
                continue
            check_reference(value, base, location, directory=value.endswith("/"))

for path in paths:
    if not os.path.exists(path):
        continue

    if any(pattern.search(path) for pattern in forbidden_patterns):
        violations.append(("forbidden generated artifact", path))
        continue

    _, ext = os.path.splitext(path)
    if ext.lower() in large_text_extensions:
        try:
            size = os.path.getsize(path)
        except OSError:
            continue

        if size > max_text_artifact_bytes:
            violations.append((f"large text artifact ({size} bytes)", path))

if violations:
    print("AI context hygiene violations:")
    for reason, path in violations[:20]:
        print(f"- {reason}: {path}")
    if len(violations) > 20:
        print(f"- {len(violations) - 20} more violations; fix these and rerun")
    sys.exit(1)

print("AI context hygiene ok")
PY
