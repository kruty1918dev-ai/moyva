#!/usr/bin/env python3
"""Prevent project-owned runtime code from constructing visual hierarchies or assets."""

from __future__ import annotations

import argparse
import collections
import re
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Optional


ROOT = Path(__file__).resolve().parents[2]
SCRIPTS_ROOT = ROOT / "Assets" / "Moyva" / "Scripts"
EXCLUDED_PARTS = {"Editor", "EditorShared", "Tests", "Development"}
NON_VISUAL_GAME_OBJECT_ALLOWLIST = {
    "Assets/Moyva/Scripts/Features/HomeMenu/Runtime/MainThreadDispatcher.cs",
    "Assets/Moyva/Scripts/Shared/Audio/AudioService.cs",
    "Assets/Moyva/Scripts/Shared/Audio/MusicService.cs",
}

VISUAL_COMPONENTS = (
    "AspectRatioFitter",
    "Button",
    "Camera",
    "Canvas",
    "CanvasGroup",
    "CanvasRenderer",
    "CanvasScaler",
    "ContentSizeFitter",
    "DecalProjector",
    "Dropdown",
    "EventTrigger",
    "GraphicRaycaster",
    "GridLayoutGroup",
    "HorizontalLayoutGroup",
    "Image",
    "LayoutElement",
    "Light",
    "LineRenderer",
    "Mask",
    "MeshFilter",
    "MeshRenderer",
    "ParticleSystem",
    "Projector",
    "RawImage",
    "RectMask2D",
    "RectTransform",
    "Renderer",
    "ScrollRect",
    "Selectable",
    "SkinnedMeshRenderer",
    "Slider",
    "SpriteMask",
    "SpriteRenderer",
    "Text",
    "TextMeshPro",
    "TextMeshProUGUI",
    "TMP_Dropdown",
    "TMP_InputField",
    "Toggle",
    "TrailRenderer",
    "UniversalAdditionalCameraData",
    "VerticalLayoutGroup",
    "VisualElement",
    "Volume",
)
VISUAL_COMPONENT_PATTERN = "|".join(
    sorted((re.escape(name) for name in VISUAL_COMPONENTS), key=len, reverse=True)
)


@dataclass(frozen=True)
class Violation:
    path: str
    line: int
    rule: str
    excerpt: str


RULES = (
    (
        "game-object-construction",
        re.compile(r"\bnew\s+GameObject\s*\("),
        "Visual/runtime GameObjects must originate from serialized prefab references.",
    ),
    (
        "primitive-construction",
        re.compile(r"\bGameObject\s*\.\s*CreatePrimitive\s*\("),
        "Primitive fallback visuals must be authored as prefabs.",
    ),
    (
        "visual-component-construction",
        re.compile(
            rf"\bAddComponent\s*<\s*(?:(?:UnityEngine|TMPro|UnityEngine\.UI)\s*\.\s*)?(?:{VISUAL_COMPONENT_PATTERN})\s*>"
        ),
        "Visual and UI components must already exist on the scene object or prefab.",
    ),
    (
        "visual-component-type-construction",
        re.compile(
            rf"\bAddComponent\s*\(\s*typeof\s*\(\s*(?:(?:UnityEngine|TMPro|UnityEngine\.UI)\s*\.\s*)?(?:{VISUAL_COMPONENT_PATTERN})\s*\)"
        ),
        "Visual and UI components must already exist on the scene object or prefab.",
    ),
    (
        "material-construction",
        re.compile(r"\bnew\s+Material\s*\("),
        "Runtime presentation must use serialized material assets and MaterialPropertyBlock.",
    ),
    (
        "shader-lookup",
        re.compile(r"\bShader\s*\.\s*Find\s*\("),
        "Shaders must be referenced by serialized material or renderer assets.",
    ),
    (
        "zenject-game-object-construction",
        re.compile(r"\bFromNewComponentOnNewGameObject\s*\("),
        "Scene presenters must be bound from authored hierarchy components.",
    ),
    (
        "ui-toolkit-construction",
        re.compile(r"\bnew\s+(?:VisualElement|Button|Label|TextField|ScrollView)\s*\("),
        "Runtime UI Toolkit hierarchies must originate from authored UXML/templates.",
    ),
)


def run_git(*args: str, check: bool = True) -> str:
    result = subprocess.run(
        ["git", *args], cwd=ROOT, capture_output=True, text=True, check=False
    )
    if check and result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "git command failed")
    return result.stdout


def normalize(path: Path | str) -> str:
    candidate = Path(path)
    if candidate.is_absolute():
        candidate = candidate.relative_to(ROOT)
    return candidate.as_posix()


def is_production(path: Path) -> bool:
    try:
        relative = path.relative_to(SCRIPTS_ROOT)
    except ValueError:
        return False
    return path.suffix == ".cs" and not EXCLUDED_PARTS.intersection(relative.parts)


def changed_paths(base_ref: str) -> set[str]:
    outputs = (
        run_git("diff", "--name-only", f"{base_ref}...HEAD"),
        run_git("diff", "--name-only"),
        run_git("diff", "--cached", "--name-only"),
        run_git("ls-files", "--others", "--exclude-standard"),
    )
    return {
        line.strip()
        for output in outputs
        for line in output.splitlines()
        if line.strip().startswith("Assets/Moyva/Scripts/")
        and line.strip().endswith(".cs")
    }


def iter_files(paths: Iterable[str], changed_only: bool, base_ref: Optional[str]) -> list[Path]:
    selected: set[Path] = set()
    for raw in paths:
        candidate = (ROOT / raw).resolve()
        if candidate.is_file() and is_production(candidate):
            selected.add(candidate)
        elif candidate.is_dir():
            selected.update(path for path in candidate.rglob("*.cs") if is_production(path))

    if not selected:
        selected.update(path for path in SCRIPTS_ROOT.rglob("*.cs") if is_production(path))

    if changed_only:
        if not base_ref:
            raise ValueError("--changed-only requires --base-ref")
        changed = changed_paths(base_ref)
        selected = {path for path in selected if normalize(path) in changed}
    return sorted(selected)


def mask_non_code(source: str) -> str:
    """Mask C# comments and literals while preserving line positions."""
    output: list[str] = []
    index = 0
    state = "code"
    verbatim = False
    while index < len(source):
        char = source[index]
        nxt = source[index + 1] if index + 1 < len(source) else ""

        if state == "line-comment":
            if char == "\n":
                output.append(char)
                state = "code"
            else:
                output.append(" ")
            index += 1
            continue

        if state == "block-comment":
            if char == "*" and nxt == "/":
                output.extend((" ", " "))
                index += 2
                state = "code"
            else:
                output.append("\n" if char == "\n" else " ")
                index += 1
            continue

        if state in {"string", "char"}:
            if verbatim and state == "string" and char == '"' and nxt == '"':
                output.extend((" ", " "))
                index += 2
                continue
            if char == "\\" and not verbatim and nxt:
                output.extend((" ", "\n" if nxt == "\n" else " "))
                index += 2
                continue
            delimiter = '"' if state == "string" else "'"
            output.append("\n" if char == "\n" else " ")
            if char == delimiter:
                state = "code"
                verbatim = False
            index += 1
            continue

        if char == "/" and nxt == "/":
            output.extend((" ", " "))
            index += 2
            state = "line-comment"
            continue
        if char == "/" and nxt == "*":
            output.extend((" ", " "))
            index += 2
            state = "block-comment"
            continue
        if char == "@" and nxt == '"':
            output.extend((" ", " "))
            index += 2
            state = "string"
            verbatim = True
            continue
        if char == '"':
            output.append(" ")
            index += 1
            state = "string"
            continue
        if char == "'":
            output.append(" ")
            index += 1
            state = "char"
            continue

        output.append(char)
        index += 1

    return "".join(output)


def analyze(path: str, source: str) -> list[Violation]:
    violations: list[Violation] = []
    source_lines = source.splitlines()
    masked_lines = mask_non_code(source).splitlines()
    for line_number, line in enumerate(masked_lines, start=1):
        if not line.strip():
            continue
        original = source_lines[line_number - 1].strip()

        for rule, pattern, _ in RULES:
            if rule == "game-object-construction" and path in NON_VISUAL_GAME_OBJECT_ALLOWLIST:
                continue
            if not pattern.search(line):
                continue
            violations.append(Violation(path, line_number, rule, original[:180]))

        is_ui_owner = any(
            token in path
            for token in ("/UI/", "/Presentation/", "Presenter", "View", "HUD", "HomeMenu")
        )
        if is_ui_owner and re.search(r"\bResources\s*\.\s*Load(?:<[^>]+>)?\s*\(", line):
            violations.append(
                Violation(
                    path,
                    line_number,
                    "ui-resource-fallback",
                    original[:180],
                )
            )
    return violations


def source_at_base(base_ref: str, path: str) -> str:
    return run_git("show", f"{base_ref}:{path}", check=False)


def count_by_rule(violations: Iterable[Violation]) -> collections.Counter[tuple[str, str]]:
    return collections.Counter((item.path, item.rule) for item in violations)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--path", action="append", default=[])
    parser.add_argument("--base-ref")
    parser.add_argument("--changed-only", action="store_true")
    parser.add_argument("--strict", action="store_true")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    try:
        files = iter_files(args.path, args.changed_only, args.base_ref)
    except (RuntimeError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2

    current: list[Violation] = []
    for path in files:
        current.extend(analyze(normalize(path), path.read_text(encoding="utf-8", errors="replace")))

    baseline_counts: collections.Counter[tuple[str, str]] = collections.Counter()
    if args.base_ref:
        baseline: list[Violation] = []
        for path in files:
            relative = normalize(path)
            source = source_at_base(args.base_ref, relative)
            if source:
                baseline.extend(analyze(relative, source))
        baseline_counts = count_by_rule(baseline)

    current_counts = count_by_rule(current)
    failures: set[tuple[str, str]] = set()
    for key, count in current_counts.items():
        allowed = baseline_counts.get(key, 0) if args.base_ref else 0
        if count > allowed:
            failures.add(key)

    for item in current:
        key = (item.path, item.rule)
        status = "FAIL" if key in failures else "DEBT"
        print(f"{status} {item.path}:{item.line} [{item.rule}] {item.excerpt}")

    print(f"\nChecked files: {len(files)}")
    print(f"Violations:    {len(current)}")
    print(f"New groups:    {len(failures)}")
    return 1 if args.strict and failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
