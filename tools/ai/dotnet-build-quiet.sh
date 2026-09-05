#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "usage: $0 <project.csproj> [dotnet-build-args...]" >&2
  exit 2
fi

project=$1
shift || true

mkdir -p Temp/ai
safe_name=$(basename "$project" | tr ' /' '__')
log="Temp/ai/${safe_name%.csproj}-build.log"

run_build() {
  if [[ "${AI_BUILD_RESTORE:-0}" == "1" ]]; then
    dotnet build "$project" --nologo "$@" >"$log" 2>&1
  else
    dotnet build "$project" --no-restore --nologo "$@" >"$log" 2>&1
  fi
}

if run_build "$@"; then
  warnings=$(rg -n "warning [A-Z]+[0-9]+:" "$log" | wc -l || true)
  errors=$(rg -n "error [A-Z]+[0-9]+:" "$log" | wc -l || true)
  echo "build ok: $project"
  echo "errors: $errors"
  echo "warnings: $warnings"
  echo "log: $log"
  exit 0
fi

code=$?

if [[ "${AI_BUILD_RESTORE:-0}" != "1" ]] && rg -q "error NETSDK1004:" "$log"; then
  echo "restore assets missing; retrying once with restore: $project"
  AI_BUILD_RESTORE=1
  if run_build "$@"; then
    warnings=$(rg -n "warning [A-Z]+[0-9]+:" "$log" | wc -l || true)
    errors=$(rg -n "error [A-Z]+[0-9]+:" "$log" | wc -l || true)
    echo "build ok: $project"
    echo "errors: $errors"
    echo "warnings: $warnings"
    echo "log: $log"
    exit 0
  fi

  code=$?
fi

echo "build failed: $project"
echo "log: $log"
echo
rg -n "error [A-Z]+[0-9]+:|Build FAILED|error:" "$log" | head -80 || tail -80 "$log"
exit "$code"
