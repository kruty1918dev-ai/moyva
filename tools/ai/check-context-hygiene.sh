#!/usr/bin/env bash
set -euo pipefail

python3 - <<'PY'
import os
import re
import subprocess
import sys

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
    for reason, path in violations:
        print(f"- {reason}: {path}")
    sys.exit(1)

print("AI context hygiene ok")
PY
