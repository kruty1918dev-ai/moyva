#!/usr/bin/env bash
set -euo pipefail

RUNTIME_WARN=300
RUNTIME_LIMIT=500
EDITOR_WARN=700
EDITOR_LIMIT=900

BASE_REF=""
CHANGED_ONLY=false
STRICT=false

usage() {
  cat <<'EOF'
Usage: check-file-length-limits.sh [options]

Options:
  --base-ref <git-ref>   Compare against base ref and ignore pre-existing hard-limit debt.
  --changed-only         Check only changed C# files under Assets/Moyva/Scripts.
  --strict               Exit with non-zero when new hard-limit violations are found.
  --help                 Show this help.

Rules:
  - Runtime classes (path contains /Runtime/): warn >300, fail >500.
  - Editor windows (class derives from EditorWindow): warn >700, fail >900.

Behavior with --base-ref:
  - If a file was already above hard limit in base ref, it is reported as DEBT (warning), not failure.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --base-ref)
      if [[ $# -lt 2 ]]; then
        echo "ERROR: --base-ref requires a value" >&2
        exit 2
      fi
      BASE_REF="$2"
      shift 2
      ;;
    --changed-only)
      CHANGED_ONLY=true
      shift
      ;;
    --help)
      usage
      exit 0
      ;;
    --strict)
      STRICT=true
      shift
      ;;
    *)
      echo "ERROR: Unknown argument: $1" >&2
      usage
      exit 2
      ;;
  esac
done

if [[ -n "$BASE_REF" ]]; then
  git rev-parse --verify "$BASE_REF" >/dev/null 2>&1 || {
    echo "ERROR: base ref '$BASE_REF' is not valid" >&2
    exit 2
  }
fi

get_candidate_files() {
  if [[ "$CHANGED_ONLY" == true ]]; then
    if [[ -z "$BASE_REF" ]]; then
      echo "ERROR: --changed-only requires --base-ref" >&2
      exit 2
    fi

    git diff --name-only "$BASE_REF"...HEAD \
      | grep -E '^Assets/Moyva/Scripts/.+\.cs$' \
      || true
  else
    find Assets/Moyva/Scripts -type f -name '*.cs' | sort
  fi
}

is_editor_window_file() {
  local file="$1"
  grep -Eq ':[[:space:]]*[^\n{]*\bEditorWindow\b|,[[:space:]]*EditorWindow\b' "$file"
}

get_base_line_count() {
  local file="$1"
  if [[ -z "$BASE_REF" ]]; then
    echo 0
    return
  fi

  if git cat-file -e "$BASE_REF:$file" 2>/dev/null; then
    git show "$BASE_REF:$file" | wc -l | tr -d ' '
  else
    echo 0
  fi
}

files_checked=0
warn_count=0
fail_count=0

while IFS= read -r file; do
  [[ -z "$file" ]] && continue
  [[ ! -f "$file" ]] && continue

  category=""
  warn_limit=0
  hard_limit=0

  if is_editor_window_file "$file"; then
    category="editor-window"
    warn_limit=$EDITOR_WARN
    hard_limit=$EDITOR_LIMIT
  elif [[ "$file" == *"/Runtime/"* ]]; then
    category="runtime"
    warn_limit=$RUNTIME_WARN
    hard_limit=$RUNTIME_LIMIT
  else
    continue
  fi

  files_checked=$((files_checked + 1))
  lines=$(wc -l < "$file" | tr -d ' ')

  if (( lines > hard_limit )); then
    base_lines=$(get_base_line_count "$file")
    if (( base_lines > hard_limit )); then
      warn_count=$((warn_count + 1))
      echo "DEBT [$category] $file :: $lines lines (base=$base_lines, hard=$hard_limit)"
    else
      fail_count=$((fail_count + 1))
      echo "FAIL [$category] $file :: $lines lines (warn=$warn_limit, hard=$hard_limit)"
    fi
  elif (( lines > warn_limit )); then
    warn_count=$((warn_count + 1))
    echo "WARN [$category] $file :: $lines lines (soft=$warn_limit..$hard_limit)"
  fi
done < <(get_candidate_files)

echo ""
echo "Checked files: $files_checked"
echo "Warnings:      $warn_count"
echo "Failures:      $fail_count"

if (( fail_count > 0 )); then
  echo ""
  if [[ "$STRICT" == true ]]; then
    echo "File length guardrail failed: new files exceed hard limits."
    exit 1
  fi

  echo "Soft mode: hard-limit violations detected, but build is not blocked."
fi

exit 0
