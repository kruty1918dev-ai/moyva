#!/usr/bin/env python3
import argparse
import json
import os
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Set, Tuple

ROOT = Path(__file__).resolve().parents[2]
ASMDEF_ROOT = ROOT / "Assets" / "Moyva" / "Scripts"
DEFAULT_OUTPUT = ROOT / "docs" / "architecture" / "feature-dependency-map.md"


@dataclass(frozen=True)
class AssemblyInfo:
    name: str
    guid: str
    path: Path
    module: str
    references: Tuple[str, ...]


def read_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def try_extract_module(path: Path) -> str:
    parts = path.parts
    try:
        idx = parts.index("Scripts")
    except ValueError:
        return ""

    tail = parts[idx + 1 :]
    if not tail:
        return ""

    # Feature modules: Scripts/Features/<Feature>/...
    if len(tail) >= 2 and tail[0] == "Features":
        feature = tail[1]
        return feature

    # Core modules near Scripts root.
    if tail[0] in {"Shared", "Bootstrap", "EditorShared"}:
        return tail[0]

    return ""


def is_runtime_dependency_source(path: Path, asm_name: str) -> bool:
    p = str(path).replace("\\", "/")
    if "/Tests/" in p or ".Tests." in asm_name:
        return False
    if "/Editor/" in p or asm_name.endswith(".Editor"):
        return False
    return True


def find_asmdefs() -> List[Path]:
    return sorted(ASMDEF_ROOT.rglob("*.asmdef"))


def load_assemblies() -> Tuple[List[AssemblyInfo], Dict[str, str], Dict[str, str]]:
    asm_paths = find_asmdefs()
    infos: List[AssemblyInfo] = []
    name_to_module: Dict[str, str] = {}
    guid_to_name: Dict[str, str] = {}

    for asm_path in asm_paths:
        data = read_json(asm_path)
        name = data.get("name", "").strip()
        if not name:
            continue

        module = try_extract_module(asm_path)
        if not module:
            continue

        guid = ""
        meta_path = asm_path.with_suffix(asm_path.suffix + ".meta")
        if meta_path.exists():
            for line in meta_path.read_text(encoding="utf-8", errors="ignore").splitlines():
                if line.startswith("guid:"):
                    guid = line.split(":", 1)[1].strip()
                    break

        refs = tuple(data.get("references", []))
        info = AssemblyInfo(name=name, guid=guid, path=asm_path, module=module, references=refs)
        infos.append(info)
        name_to_module[name] = module
        if guid:
            guid_to_name[guid] = name

    return infos, name_to_module, guid_to_name


def resolve_ref_name(ref: str, guid_to_name: Dict[str, str]) -> str:
    if ref.startswith("GUID:"):
        return guid_to_name.get(ref[5:], "")
    return ref


def build_module_graph(
    infos: List[AssemblyInfo],
    name_to_module: Dict[str, str],
    guid_to_name: Dict[str, str],
) -> Dict[str, Set[str]]:
    graph: Dict[str, Set[str]] = defaultdict(set)

    for info in infos:
        if not is_runtime_dependency_source(info.path, info.name):
            continue

        src = info.module
        graph.setdefault(src, set())

        for ref in info.references:
            ref_name = resolve_ref_name(ref, guid_to_name)
            if not ref_name:
                continue
            dst = name_to_module.get(ref_name, "")
            if not dst:
                continue
            if dst == src:
                continue
            graph[src].add(dst)

    # Ensure all known modules are present even without outgoing edges.
    for mod in set(name_to_module.values()):
        graph.setdefault(mod, set())

    return graph


def find_cycles(graph: Dict[str, Set[str]]) -> List[List[str]]:
    visited: Dict[str, int] = {}
    stack: List[str] = []
    stack_set: Set[str] = set()
    cycles: Set[Tuple[str, ...]] = set()

    def normalize_cycle(cycle: List[str]) -> Tuple[str, ...]:
        # cycle example: A B C A -> normalize by lexicographically smallest rotation
        base = cycle[:-1]
        n = len(base)
        rotations = [tuple(base[i:] + base[:i]) for i in range(n)]
        best = min(rotations)
        return best + (best[0],)

    def dfs(node: str) -> None:
        visited[node] = 1
        stack.append(node)
        stack_set.add(node)

        for nxt in sorted(graph.get(node, [])):
            state = visited.get(nxt, 0)
            if state == 0:
                dfs(nxt)
            elif state == 1 and nxt in stack_set:
                idx = stack.index(nxt)
                cyc = stack[idx:] + [nxt]
                cycles.add(normalize_cycle(cyc))

        stack.pop()
        stack_set.remove(node)
        visited[node] = 2

    for node in sorted(graph.keys()):
        if visited.get(node, 0) == 0:
            dfs(node)

    return [list(c) for c in sorted(cycles)]


def render_markdown(graph: Dict[str, Set[str]], cycles: List[List[str]]) -> str:
    modules = sorted(graph.keys())
    edges = sorted((src, dst) for src, dsts in graph.items() for dst in dsts)

    lines: List[str] = []
    lines.append("# Feature Dependency Map")
    lines.append("")
    lines.append("Граф залежностей збудовано з asmdef у Assets/Moyva/Scripts (runtime/API/UI модулі, без Editor/Tests).")
    lines.append("")
    lines.append(f"- Модулів: {len(modules)}")
    lines.append(f"- Ребер: {len(edges)}")
    lines.append(f"- Циклів: {len(cycles)}")
    lines.append("")

    lines.append("## Mermaid")
    lines.append("")
    lines.append("```mermaid")
    lines.append("graph TD")
    if edges:
        for src, dst in edges:
            lines.append(f"  {src} --> {dst}")
    else:
        lines.append("  NoDeps[No dependencies found]")
    lines.append("```")
    lines.append("")

    lines.append("## Adjacency List")
    lines.append("")
    for mod in modules:
        deps = sorted(graph[mod])
        if deps:
            lines.append(f"- {mod}: {', '.join(deps)}")
        else:
            lines.append(f"- {mod}: (no outgoing dependencies)")
    lines.append("")

    lines.append("## Cycle Check")
    lines.append("")
    if not cycles:
        lines.append("✅ Cycles not detected.")
    else:
        lines.append("❌ Cycles detected:")
        for cyc in cycles:
            lines.append(f"- {' -> '.join(cyc)}")

    lines.append("")
    lines.append("## Policy")
    lines.append("")
    lines.append("- Між feature-модулями цикли заборонені.")
    lines.append("- Зміни asmdef мають зберігати ациклічність графа.")

    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate feature dependency map from asmdef and check cycles.")
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT, help="Path to markdown output.")
    parser.add_argument("--check-cycles", action="store_true", help="Exit with code 1 when cycles are found.")
    args = parser.parse_args()

    infos, name_to_module, guid_to_name = load_assemblies()
    graph = build_module_graph(infos, name_to_module, guid_to_name)
    cycles = find_cycles(graph)

    markdown = render_markdown(graph, cycles)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(markdown, encoding="utf-8")

    print(f"Wrote dependency map: {args.output}")
    print(f"Modules: {len(graph)}")
    print(f"Edges: {sum(len(v) for v in graph.values())}")
    print(f"Cycles: {len(cycles)}")

    if args.check_cycles and cycles:
        print("ERROR: Feature dependency cycles detected.")
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
