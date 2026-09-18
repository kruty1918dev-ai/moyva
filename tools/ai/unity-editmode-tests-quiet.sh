#!/usr/bin/env bash
set -euo pipefail

filter=${1:-Kruty1918.Moyva}
platform=${UNITY_TEST_PLATFORM:-EditMode}
default_unity_bin=/home/oleks/Unity/Hub/Editor/6000.3.10f1/Editor/Unity
if [[ ! -x "$default_unity_bin" ]]; then
  version=$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt | head -1)
  for candidate in \
    "$HOME/Unity/Hub/Editor/$version/Editor/Unity" \
    "$HOME/Unity/Editors/$version/Editor/Unity" \
    "$HOME/AppData/Local/Unity/Editors/$version/Editor/Unity.exe"; do
    if [[ -x "$candidate" ]]; then default_unity_bin="$candidate"; break; fi
  done
fi
unity_bin=${UNITY_BIN:-$default_unity_bin}
timeout_duration=${UNITY_TEST_TIMEOUT:-900s}

mkdir -p Temp/ai
stamp=$(date +%Y%m%d_%H%M%S)
results="Temp/ai/tests-${platform,,}-${stamp}.xml"
log="Temp/ai/tests-${platform,,}-${stamp}.log"

if ! command -v python3 >/dev/null 2>&1; then
  echo "python3 is required to summarize Unity test XML output." >&2
  exit 2
fi

# NOTE: no -quit here. In batch mode -quit makes the editor exit at the first
# update loop, which can preempt -runTests and silently produce no result XML.
# -runTests exits the editor on its own when the run finishes.
set +e
timeout "$timeout_duration" "$unity_bin" \
  -batchmode \
  -automated \
  -nographics \
  -projectPath "$PWD" \
  -runTests \
  -testPlatform "$platform" \
  -testFilter "$filter" \
  -testResults "$results" \
  -logFile "$log"
code=$?
set -e

if [[ -f "$results" ]]; then
  python3 - "$results" <<'PY'
import sys
import xml.etree.ElementTree as ET

path = sys.argv[1]
root = ET.parse(path).getroot()
print(f"result: {root.get('result', 'unknown')}")
for key in ("total", "passed", "failed", "inconclusive", "skipped", "duration"):
    value = root.get(key)
    if value is not None:
        print(f"{key}: {value}")

failures = [case for case in root.iter("test-case") if case.get("result") == "Failed"]
for case in failures[:20]:
    print(f"failed: {case.get('fullname') or case.get('name')}")
    message = case.findtext("./failure/message")
    if message:
        print(message.strip().splitlines()[0])
PY
else
  echo "Unity did not produce a test result XML."
fi

echo "results: $results"
echo "log: $log"

if [[ "$code" -ne 0 ]]; then
  echo
  echo "Unity exited with code $code"
  if [[ -f "$log" ]]; then
    echo "last log lines:"
    tail -80 "$log"
  fi
fi

exit "$code"
