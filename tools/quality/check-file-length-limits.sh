#!/usr/bin/env bash
set -euo pipefail

# Backward-compatible entry point. The Python implementation measures both
# effective source lines and an approximate model context size.
exec python3 "$(dirname "$0")/context_budget.py" check "$@"
