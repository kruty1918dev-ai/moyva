#!/usr/bin/env bash
set -euo pipefail

patch_name="${1:-p24-gameplay-pass}"
timestamp="$(date +%Y%m%d-%H%M%S)"

find_project_root() {
    local dir="$PWD"
    while [[ "$dir" != "/" ]]; do
        if [[ -f "$dir/ProjectSettings/ProjectVersion.txt" && -d "$dir/Assets/Moyva" ]]; then
            printf '%s\n' "$dir"
            return 0
        fi
        dir="$(dirname "$dir")"
    done

    return 1
}

project_root="$(find_project_root)"
audit_name="moyva-${patch_name}-audit-${timestamp}"
audit_dir="${TMPDIR:-/tmp}/${audit_name}"
audit_file="${project_root}/${audit_name}.tar.gz"
backup_path="${project_root}/.codex-backups/p24-gameplay-pass-20260818"
exclude_pathspecs=(
    ':(exclude).codex-backups/**'
    ':(exclude)moyva-*-audit-*.tar.gz'
)

mkdir -p "$audit_dir"

cd "$project_root"

{
    printf 'audit=%s\n' "$audit_name"
    printf 'created_at=%s\n' "$(date --iso-8601=seconds)"
    printf 'project_root=%s\n' "$project_root"
    printf 'unity_version='
    sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt
    printf 'branch='
    git rev-parse --abbrev-ref HEAD
    printf 'head='
    git rev-parse HEAD
    printf 'backup_path=%s\n' "$backup_path"
    printf 'unity_batch_compile_status=blocked_when_existing_editor_owns_project_lock\n'
} > "$audit_dir/summary.txt"

git status --short --untracked-files=normal -- . "${exclude_pathspecs[@]}" > "$audit_dir/git-status.txt"
git diff --binary -- . "${exclude_pathspecs[@]}" > "$audit_dir/diff.patch"

while IFS= read -r -d '' file; do
    if [[ -f "$file" ]]; then
        git diff --no-index --binary -- /dev/null "$file" >> "$audit_dir/diff.patch" || true
    fi
done < <(git ls-files --others --exclude-standard -z -- . "${exclude_pathspecs[@]}")

mapfile -d '' changed_files < <(
    {
        git diff --name-only -z -- . "${exclude_pathspecs[@]}"
        git ls-files --others --exclude-standard -z -- . "${exclude_pathspecs[@]}"
    }
)

if ((${#changed_files[@]} > 0)); then
    printf '%s\0' "${changed_files[@]}" | xargs -0 sha256sum > "$audit_dir/sha256-changed-files.txt"
    printf '%s\n' "${changed_files[@]}" > "$audit_dir/changed-files.txt"
else
    : > "$audit_dir/sha256-changed-files.txt"
    : > "$audit_dir/changed-files.txt"
fi

{
    printf '== ready indicator presenters ==\n'
    rg -n "class .*ReadyIndicatorPresenter|GetReadyItems\\(|BindInterfacesAndSelfTo<UnitRecruitmentReadyIndicatorPresenter>" Assets/Moyva/Scripts || true
    printf '\n== progress indicator presenters ==\n'
    rg -n "class .*ProgressIndicatorPresenter|BindInterfacesAndSelfTo<UnitRecruitmentProgressIndicatorPresenter>" Assets/Moyva/Scripts || true
    printf '\n== deployment controller ==\n'
    rg -n "UnitRecruitmentDeploymentController|ITurnBlocker|TryDeployReady\\(" Assets/Moyva/Scripts/Bootstrap Assets/Moyva/Scripts/Features/Units || true
    printf '\n== signal bindings ==\n'
    rg -n "UnitRecruitmentReadyIndicatorClickedSignal|UnitRecruitmentReadySignal|UnitRecruitmentDeployedSignal|BuildingOperationalSignal" Assets/Moyva/Scripts/Features/Signals Assets/Moyva/Scripts/Bootstrap Assets/Moyva/Scripts/Features/Construction || true
    printf '\n== CreateUnit call sites ==\n'
    rg -n "CreateUnit\\(|CreateUnitWithId\\(" Assets/Moyva/Scripts/Bootstrap Assets/Moyva/Scripts/Features || true
    printf '\n== legacy recruitment install scan ==\n'
    rg -n "RecruitmentBindings\\.Install|using Kruty1918\\.Moyva\\.Recruitment;|IRecruitmentService" Assets/Moyva/Scripts/Bootstrap Assets/Moyva/Scripts/Features || true
    printf '\n== deployment invariants scan ==\n'
    rg -n "previewCount|duplicatePreviews|TryDeployReady\\(|SelectedTile|InputBlock|IsTurnBlocked" Assets/Moyva/Scripts/Bootstrap/Runtime/UnitRecruitmentDeploymentController.cs || true
} > "$audit_dir/scans.txt"

if [[ -f /tmp/moyva-p24-gameplay-pass-unity.log ]]; then
    cp /tmp/moyva-p24-gameplay-pass-unity.log "$audit_dir/unity-batchmode.log"
    rg -n "error CS|warning CS|Aborting|fatal error|Unity instance|Multiple Unity instances" /tmp/moyva-p24-gameplay-pass-unity.log > "$audit_dir/unity-compile-errors.txt" || true
else
    : > "$audit_dir/unity-batchmode.log"
    : > "$audit_dir/unity-compile-errors.txt"
fi

if [[ -d "$backup_path" ]]; then
    find "$backup_path" -maxdepth 1 -type f -printf '%f\n' | sort > "$audit_dir/backups.txt"
else
    : > "$audit_dir/backups.txt"
fi

tar -czf "$audit_file" -C "$audit_dir" .
printf '%s\n' "$audit_file"
