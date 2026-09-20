#!/usr/bin/env bash
# usage: ueval.sh <script.cs>  — ensures edit mode, runs eval_file with retries
set -u
PROJECT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
FILE="$1"
for i in 1 2 3 4 5 6; do
  PLAY=$(unity command eval --code 'return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode.ToString();' --project-path "$PROJECT" --format json 2>/dev/null | python -c "import sys,json;d=json.load(sys.stdin);print((d.get('data') or {}).get('result',{}).get('result') or 'ERR')" 2>/dev/null)
  if [ "$PLAY" = "True" ]; then
    unity command eval --code 'UnityEditor.EditorApplication.isPlaying=false; return "x";' --project-path "$PROJECT" --format json >/dev/null 2>&1
    sleep 3
    continue
  fi
  OUT=$(unity command eval_file --file "$FILE" --project-path "$PROJECT" --format json 2>&1)
  RES=$(echo "$OUT" | python -c "
import sys,json
try:
    d=json.load(sys.stdin)
    r=(d.get('data') or {}).get('result',{})
    print(r.get('result') if r.get('result') is not None else 'ERR:'+str(r.get('error') or d.get('errors')))
except Exception as e: print('ERR:parse')
" 2>/dev/null)
  case "$RES" in
    ERR:*) sleep 2; continue;;
    *) echo "$RES"; exit 0;;
  esac
done
echo "FAILED after retries"
exit 1
