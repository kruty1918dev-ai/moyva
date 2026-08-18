#!/usr/bin/env bash
set -euo pipefail

find_project_root() {
  local dir="${1:-$PWD}"
  while [[ "$dir" != "/" ]]; do
    if [[ -f "$dir/ProjectSettings/ProjectVersion.txt" && -d "$dir/Assets/Moyva" ]]; then
      printf '%s\n' "$dir"
      return 0
    fi
    dir="$(dirname "$dir")"
  done
  return 1
}

PROJECT="$(find_project_root "$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)")"
STAMP="$(date +%Y%m%d-%H%M%S)"
REPORT_DIR="$PROJECT/.codex-audit/p24d-runtime-$STAMP"
mkdir -p "$REPORT_DIR"

cd "$PROJECT"

if ! command -v unity >/dev/null 2>&1; then
  echo "[P24D Runtime] ERROR: Unity CLI is not in PATH." | tee "$REPORT_DIR/error.txt"
  exit 10
fi

unity command editor_play --project-path="$PROJECT" --format json > "$REPORT_DIR/editor-play.json" 2>&1 || true
sleep 1

cat > "$REPORT_DIR/p24d-runtime-eval.cs" <<'CS'
using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

var context = UnityEngine.Object.FindFirstObjectByType<SceneContext>(FindObjectsInactive.Include);
if (context == null)
    return "{\"pass\":false,\"reason\":\"SceneContext not found\"}";

var container = context.Container;
var turns = container.Resolve<ITurnService>();
var recruitment = container.Resolve<IUnitRecruitmentService>();
var units = container.Resolve<IUnitService>();
var signalBus = container.Resolve<SignalBus>();

string ownerId = turns.LocalOwnerId;
var ready = recruitment.GetReadyItems(ownerId);
int authoritativeBefore = units.GetAllUnitIds().Count;

GameObject indicatorRoot = GameObject.Find("RecruitmentReadyIndicators");
var buttons = indicatorRoot != null
    ? indicatorRoot.GetComponentsInChildren<Button>(true)
    : Array.Empty<Button>();

int indicatorCount = buttons.Length;
var queueIds = new HashSet<long>();
int activeIndicators = 0;
int spriteIndicators = 0;
Button clickedButton = null;
long clickedQueueId = 0;

for (int i = 0; i < buttons.Length; i++)
{
    Button button = buttons[i];
    if (button == null)
        continue;

    if (button.gameObject.activeInHierarchy)
        activeIndicators++;

    if (button.transform.Find("Icon") is Transform iconTransform
        && iconTransform.TryGetComponent<Image>(out var icon)
        && icon.sprite != null)
    {
        spriteIndicators++;
    }

    string name = button.gameObject.name;
    if (name.StartsWith("ReadyIndicator_", StringComparison.Ordinal)
        && long.TryParse(name.Substring("ReadyIndicator_".Length), out long id))
    {
        queueIds.Add(id);
        if (clickedButton == null)
        {
            clickedButton = button;
            clickedQueueId = id;
        }
    }
}

int clickSignals = 0;
UnitRecruitmentReadyIndicatorClickedSignal lastSignal = default;
Action<UnitRecruitmentReadyIndicatorClickedSignal> handler = signal =>
{
    clickSignals++;
    lastSignal = signal;
};

signalBus.Subscribe(handler);
clickedButton?.onClick.Invoke();
signalBus.TryUnsubscribe(handler);

var readyAfter = recruitment.GetReadyItems(ownerId);
int authoritativeAfter = units.GetAllUnitIds().Count;

bool hasOneReady = ready.Count == 1;
bool identityMatches = hasOneReady
    && clickedQueueId == ready[0].QueueId
    && lastSignal.QueueId == ready[0].QueueId
    && lastSignal.RecruitingBuildingPosition == ready[0].RecruitingBuildingPosition
    && string.Equals(lastSignal.UnitTypeId, ready[0].UnitTypeId, StringComparison.Ordinal)
    && string.Equals(lastSignal.OwnerId, ready[0].OwnerId, StringComparison.Ordinal);

bool countMatch = indicatorCount == queueIds.Count && indicatorCount == ready.Count;
bool pass = hasOneReady
    && indicatorCount == 1
    && queueIds.Count == 1
    && activeIndicators == 1
    && spriteIndicators == 1
    && countMatch
    && clickSignals == 1
    && readyAfter.Count == ready.Count
    && authoritativeAfter == authoritativeBefore
    && identityMatches;

var json = new StringBuilder();
json.Append("{");
json.Append("\"pass\":").Append(pass ? "true" : "false").Append(",");
json.Append("\"readyJobs\":").Append(ready.Count).Append(",");
json.Append("\"indicatorCount\":").Append(indicatorCount).Append(",");
json.Append("\"uniqueIndicatorQueueIds\":").Append(queueIds.Count).Append(",");
json.Append("\"activeIndicators\":").Append(activeIndicators).Append(",");
json.Append("\"spriteIndicators\":").Append(spriteIndicators).Append(",");
json.Append("\"readyIndicatorCountMatch\":").Append(countMatch ? "true" : "false").Append(",");
json.Append("\"duplicateIndicators\":").Append(indicatorCount - queueIds.Count).Append(",");
json.Append("\"authoritativeUnitCountBefore\":").Append(authoritativeBefore).Append(",");
json.Append("\"clickSignalsDelta\":").Append(clickSignals).Append(",");
json.Append("\"readyJobsAfterClick\":").Append(readyAfter.Count).Append(",");
json.Append("\"authoritativeUnitCountAfter\":").Append(authoritativeAfter).Append(",");
json.Append("\"indicatorQueueId\":").Append(clickedQueueId).Append(",");
json.Append("\"readyQueueId\":").Append(hasOneReady ? ready[0].QueueId : 0).Append(",");
json.Append("\"identityMatches\":").Append(identityMatches ? "true" : "false");
json.Append("}");
return json.ToString();
CS

unity command eval_file "$REPORT_DIR/p24d-runtime-eval.cs" --project-path="$PROJECT" --format json > "$REPORT_DIR/runtime-audit.json" 2>&1

if rg -q '"pass"[: ]*true' "$REPORT_DIR/runtime-audit.json"; then
  echo "[P24D Runtime] PASS"
else
  echo "[P24D Runtime] FAIL. See $REPORT_DIR/runtime-audit.json"
  exit 30
fi
