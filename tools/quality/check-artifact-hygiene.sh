#!/usr/bin/env bash
# Artifact hygiene guardrail.
#
# Fails when a change adds generated artifacts to the repository:
#   - output directories: Results/, Exports/, MarketingOutput/, Build/,
#     Builds/, Library/, Temp/, Logs/, UserSettings/
#   - ML checkpoints: *.pt, *.pth, *.ckpt
#   - model/binary outputs outside Assets/: *.onnx, *.dll, *.so
#   - Unity player build payloads anywhere: *_Data/Managed/*, UnityPlayer.so,
#     resources.assets, *.resS, *.resource, globalgamemanagers*, level*
#   - training/run logs: *.log, run_logs/
#
# Vendored dependencies under Assets/ (plugin DLLs, production ONNX models in
# Presets/AI/Resources/AI/Models/) are source of truth and stay allowed.
#
# Usage:
#   tools/quality/check-artifact-hygiene.sh                    # scan all tracked files
#   tools/quality/check-artifact-hygiene.sh --base-ref REF     # scan files added since REF
set -euo pipefail

BASE_REF=""
while [[ $# -gt 0 ]]; do
    case "$1" in
        --base-ref) BASE_REF="${2:?'--base-ref requires a value'}"; shift 2 ;;
        *) echo "check-artifact-hygiene: unknown argument '$1'" >&2; exit 2 ;;
    esac
done

if [[ -n "$BASE_REF" ]]; then
    FILES="$(git diff --name-only --diff-filter=A "${BASE_REF}...HEAD")"
    SCOPE="added files since ${BASE_REF}"
else
    FILES="$(git ls-files)"
    SCOPE="tracked files"
fi

[[ -z "$FILES" ]] && { echo "artifact-hygiene: no files in scope"; exit 0; }

DENY_REGEX='^(Results|Exports|MarketingOutput|Build|Builds|Library|Temp|Logs|UserSettings)/'
DENY_REGEX+='|\.(pt|pth|ckpt|log)$'
DENY_REGEX+='|(^|/)run_logs/'
DENY_REGEX+='|(^|/)[^/]*_Data/Managed/'
DENY_REGEX+='|(^|/)(UnityPlayer\.so|resources\.assets|globalgamemanagers[^/]*|level[0-9]+[^/]*)$'
DENY_REGEX+='|\.(resS|resource)$'

VIOLATIONS="$(printf '%s\n' "$FILES" | grep -E "$DENY_REGEX" || true)"

# Binary/model extensions denied outside Assets/ (vendored plugins and the
# production ONNX under Assets/Moyva/Presets/ remain allowed).
VIOLATIONS+=$'\n'"$(printf '%s\n' "$FILES" | grep -E '\.(onnx|dll|so)$' | grep -vE '^Assets/' || true)"

VIOLATIONS="$(printf '%s\n' "$VIOLATIONS" | sed '/^$/d' | sort -u)"

if [[ -n "$VIOLATIONS" ]]; then
    echo "artifact-hygiene: generated artifacts detected in ${SCOPE}:" >&2
    printf '%s\n' "$VIOLATIONS" | sed 's/^/  /' >&2
    echo >&2
    echo "Builds, DLLs, checkpoints and training logs do not belong in Git." >&2
    echo "See docs/standards/artifact-storage.md for storage/restore flow." >&2
    exit 1
fi

echo "artifact-hygiene: OK (${SCOPE})"
