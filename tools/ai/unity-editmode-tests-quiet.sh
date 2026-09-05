#!/usr/bin/env bash
set -euo pipefail

filter=${1:-Kruty1918.Moyva.Tests.HomeMenu}
unity_bin=${UNITY_BIN:-/home/oleks/Unity/Hub/Editor/6000.3.10f1/Editor/Unity}
timeout_duration=${UNITY_TEST_TIMEOUT:-900s}

mkdir -p Temp/ai
stamp=$(date +%Y%m%d_%H%M%S)
results="Temp/ai/editmode-${stamp}.xml"
log="Temp/ai/editmode-${stamp}.log"

if ! command -v python3 >/dev/null 2>&1; then
  echo "python3 is required to summarize Unity test XML output." >&2
  exit 2
fi

set +e
timeout "$timeout_duration" "$unity_bin" \
  -batchmode \
  -automated \
  -nographics \
  -projectPath "$PWD" \
  -runTests \
  -testPlatform EditMode \
  -testFilter "$filter" \
  -testResults "$results" \
  -logFile "$log" \
  -quit
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
