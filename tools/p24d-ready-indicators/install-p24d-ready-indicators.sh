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
PATCH="p24d-tech"
STAMP="$(date +%Y%m%d-%H%M%S)"
REPORT_DIR="$PROJECT/.codex-audit/$PATCH-$STAMP"
AUDIT_FILE="$PROJECT/moyva-$PATCH-audit-$STAMP.tar.gz"
BACKUP_DIR="$PROJECT/.codex-backups/p24d-tech"

mkdir -p "$REPORT_DIR"

cd "$PROJECT"

TARGET_FILES=(
  "Assets/Moyva/Scripts/Features/Signals/API/OnUnitRecruitmentSignals.cs"
  "Assets/Moyva/Scripts/Features/Signals/Runtime/SignalBusInstaller.cs"
  "Assets/Moyva/Scripts/Features/Units/Runtime/UnitRecruitmentService.cs"
  "Assets/Moyva/Scripts/Bootstrap/Runtime/BootstrapInstaller.cs"
  "Assets/Moyva/Scripts/Bootstrap/Runtime/UnitRecruitmentReadyIndicatorPresenter.cs"
  "Assets/Moyva/Scripts/Bootstrap/Runtime/UnitRecruitmentReadyIndicatorPresenter.cs.meta"
  "tools/p24d-ready-indicators/install-p24d-ready-indicators.sh"
  "tools/p24d-ready-indicators/CAPTURE_RUNTIME_AUDIT.sh"
)

count_rg() {
  local pattern="$1"
  shift
  { rg -n "$pattern" "$@" 2>/dev/null || true; } | wc -l | tr -d ' '
}

contains_rg() {
  local pattern="$1"
  shift
  if rg -q "$pattern" "$@" 2>/dev/null; then
    printf 'True'
  else
    printf 'False'
  fi
}

branch="$(git branch --show-current 2>/dev/null || true)"
head="$(git rev-parse HEAD 2>/dev/null || true)"
presenter_file="Assets/Moyva/Scripts/Bootstrap/Runtime/UnitRecruitmentReadyIndicatorPresenter.cs"

{
  printf 'patch=%s\n' "$PATCH"
  printf 'timestamp=%s\n' "$STAMP"
  printf 'project=%s\n' "$PROJECT"
  printf 'branch=%s\n' "$branch"
  printf 'HEAD=%s\n' "$head"
  printf 'backupPath=%s\n' "$BACKUP_DIR"
} > "$REPORT_DIR/summary.txt"

git status --short > "$REPORT_DIR/git-status.txt" 2>&1 || true
git diff -- "${TARGET_FILES[@]}" > "$REPORT_DIR/p24d-diff.patch" 2>&1 || true
for target in "${TARGET_FILES[@]}"; do
  if git ls-files --error-unmatch "$target" >/dev/null 2>&1; then
    continue
  fi

  if [[ -f "$target" ]]; then
    {
      printf '\n--- NEW FILE: %s ---\n' "$target"
      git diff --no-index /dev/null "$target" || true
    } >> "$REPORT_DIR/p24d-diff.patch"
  fi
done
sha256sum "${TARGET_FILES[@]}" > "$REPORT_DIR/changed-file-sha256.txt"
printf '%s\n' "${TARGET_FILES[@]}" > "$REPORT_DIR/declared-scope-files.txt"
git status --short -- "${TARGET_FILES[@]}" > "$REPORT_DIR/changed-scope-status.txt" 2>&1 || true
{
  git diff --name-only -- "${TARGET_FILES[@]}" 2>/dev/null || true
  for target in "${TARGET_FILES[@]}"; do
    if ! git ls-files --error-unmatch "$target" >/dev/null 2>&1 && [[ -f "$target" ]]; then
      printf '%s\n' "$target"
    fi
  done
} | sort -u > "$REPORT_DIR/changed-gameplay-files.txt"

presenter_count="$({ rg --files Assets/Moyva/Scripts | rg '(^|/)(UnitRecruitmentReadyIndicatorPresenter|RecruitmentReadyIndicatorPresenter|ReadyUnitIndicator|RecruitmentReadyIcon).*\.cs$' || true; } | wc -l | tr -d ' ')"
binding_count="$(count_rg 'BindInterfacesAndSelfTo<UnitRecruitmentReadyIndicatorPresenter>' Assets/Moyva/Scripts/Bootstrap/Runtime/BootstrapInstaller.cs)"
click_signal_declarations="$(count_rg 'DeclareSignal<UnitRecruitmentReadyIndicatorClickedSignal>' Assets/Moyva/Scripts/Features/Signals/Runtime/SignalBusInstaller.cs)"
click_signal_structs="$(count_rg 'struct UnitRecruitmentReadyIndicatorClickedSignal' Assets/Moyva/Scripts/Features/Signals/API/OnUnitRecruitmentSignals.cs)"

duplicate_presenters=0
if [[ "$presenter_count" -gt 1 ]]; then
  duplicate_presenters=$((presenter_count - 1))
fi

duplicate_bindings=0
if [[ "$binding_count" -gt 1 ]]; then
  duplicate_bindings=$((binding_count - 1))
fi

try_deploy_in_presenter="$(count_rg 'TryDeployReady' "$presenter_file")"
create_unit_in_presenter="$(count_rg 'CreateUnit|CreateUnitWithId' "$presenter_file")"
deployment_tiles_in_presenter="$(count_rg 'GetDeploymentTiles' "$presenter_file")"

{
  printf 'branchIsOptimaze=%s\n' "$([[ "$branch" == "optimaze" ]] && printf True || printf False)"
  printf 'presenterFileCount=%s\n' "$presenter_count"
  printf 'presenterBindings=%s\n' "$binding_count"
  printf 'clickSignalDeclarations=%s\n' "$click_signal_declarations"
  printf 'clickSignalStructs=%s\n' "$click_signal_structs"
  printf 'duplicateReadyIndicatorPresenters=%s\n' "$duplicate_presenters"
  printf 'duplicateBindings=%s\n' "$duplicate_bindings"
  printf 'TryDeployReadyOccurrencesInPresenter=%s\n' "$try_deploy_in_presenter"
  printf 'CreateUnitOccurrencesInPresenter=%s\n' "$create_unit_in_presenter"
  printf 'GetDeploymentTilesOccurrencesInPresenter=%s\n' "$deployment_tiles_in_presenter"
  printf 'GetReadyItemsUsage=%s\n' "$(contains_rg 'GetReadyItems' "$presenter_file")"
  printf 'IGridProjectionUsage=%s\n' "$(contains_rg 'IGridProjection' "$presenter_file")"
  printf 'CustomSpriteUsage=%s\n' "$(contains_rg 'CustomSprite' "$presenter_file")"
  printf 'RestoreStateNotificationUsage=%s\n' "$(contains_rg 'FireRestoreQueueNotifications' Assets/Moyva/Scripts/Features/Units/Runtime/UnitRecruitmentService.cs)"
} > "$REPORT_DIR/static-invariants.txt"

{
  rg -n 'UnitRecruitmentReadyIndicatorPresenter|UnitRecruitmentReadyIndicatorClickedSignal|GetReadyItems' Assets/Moyva/Scripts/Bootstrap Assets/Moyva/Scripts/Features || true
} > "$REPORT_DIR/type-and-binding-scan.txt"

{
  rg -n 'TryDeployReady|CreateUnitWithId|CreateUnit\(' Assets/Moyva/Scripts/Bootstrap Assets/Moyva/Scripts/Features/Units || true
} > "$REPORT_DIR/create-unit-try-deploy-call-sites.txt"

{
  rg -n 'Unit creation happens only through TryDeployReady|AutoDeploy|auto deploy|TryDeployReady' Assets/Moyva/Scripts/Features/Units Assets/Moyva/Scripts/Bootstrap || true
} > "$REPORT_DIR/forbidden-legacy-auto-deploy-scan.txt"

installed_type_reflection=unknown
compile_errors_fresh=unknown
if command -v unity >/dev/null 2>&1; then
  if unity command eval --project-path="$PROJECT" --format json --code 'var presenter = System.Type.GetType("Kruty1918.Moyva.Bootstrap.Runtime.UnitRecruitmentReadyIndicatorPresenter, Kruty1918.Moyva.Bootstrap") != null; var signal = System.Type.GetType("Kruty1918.Moyva.Signals.UnitRecruitmentReadyIndicatorClickedSignal, Kruty1918.Moyva.Signals") != null; return presenter && signal ? "installedTypeReflection=True" : "installedTypeReflection=False";' > "$REPORT_DIR/installed-type-reflection.json" 2>&1; then
    if rg -q '"result"[[:space:]]*:[[:space:]]*"installedTypeReflection=True"' "$REPORT_DIR/installed-type-reflection.json"; then
      installed_type_reflection=True
    else
      installed_type_reflection=False
    fi
  else
    installed_type_reflection=unity-command-failed
  fi

  if unity command get_console_logs --project-path="$PROJECT" --limit 500 --format json > "$REPORT_DIR/unity-console-logs.json" 2>&1; then
    compile_errors_fresh="$(rg -c 'error CS[0-9]+' "$REPORT_DIR/unity-console-logs.json" 2>/dev/null || true)"
    if [[ -z "$compile_errors_fresh" ]]; then
      compile_errors_fresh=0
    fi
  else
    compile_errors_fresh=unity-command-failed
  fi
fi
printf 'installedTypeReflection=%s\n' "$installed_type_reflection" >> "$REPORT_DIR/static-invariants.txt"
printf 'compileErrorsFresh=%s\n' "$compile_errors_fresh" >> "$REPORT_DIR/static-invariants.txt"

if [[ "$branch" != "optimaze" ]]; then
  printf '[P24D] ERROR branch=%s expected=optimaze\n' "$branch" | tee "$REPORT_DIR/error.txt"
  tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
  printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
  exit 20
fi

if [[ "$duplicate_presenters" != "0" || "$duplicate_bindings" != "0" ]]; then
  printf '[P24D] ERROR duplicate presenter/binding detected\n' | tee "$REPORT_DIR/error.txt"
  tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
  printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
  exit 21
fi

if [[ "$binding_count" != "1" || "$click_signal_declarations" != "1" || "$click_signal_structs" != "1" ]]; then
  printf '[P24D] ERROR signal or binding count mismatch\n' | tee "$REPORT_DIR/error.txt"
  tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
  printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
  exit 22
fi

if [[ "$try_deploy_in_presenter" != "0" || "$create_unit_in_presenter" != "0" || "$deployment_tiles_in_presenter" != "0" ]]; then
  printf '[P24D] ERROR presenter contains deployment/spawn calls\n' | tee "$REPORT_DIR/error.txt"
  tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
  printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
  exit 23
fi

if [[ "$compile_errors_fresh" != "unknown" && "$compile_errors_fresh" != "0" ]]; then
  printf '[P24D] ERROR compileErrorsFresh=%s\n' "$compile_errors_fresh" | tee "$REPORT_DIR/error.txt"
  tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
  printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
  exit 24
fi

if [[ "$installed_type_reflection" != "unknown" && "$installed_type_reflection" != "True" ]]; then
  printf '[P24D] ERROR installedTypeReflection=%s\n' "$installed_type_reflection" | tee "$REPORT_DIR/error.txt"
  tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
  printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
  exit 25
fi

printf '[P24D] STATIC_AUDIT_PASS\n' > "$REPORT_DIR/result.txt"
tar -czf "$AUDIT_FILE" -C "$REPORT_DIR" .
printf 'AUDIT_FILE=%s\n' "$AUDIT_FILE"
printf '[P24D] PASS\n'
