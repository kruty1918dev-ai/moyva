#!/usr/bin/env python3
"""C20 — narrow definition validator for the Moyva JSON preset tree.

Scans building/resource/unit definitions plus the wall-collection registry and
reports broken references without attempting to "fix" anything:

  * duplicate ids within each domain
  * recipe input/output and construction-cost resourceIds that no
    economy-resource defines
  * recruitment module unitTypeIds that no unit class defines
  * malformed or missing $asset / editorPath pairs (prefabs, icons)
  * identity.category / identity.role outside the runtime enums
  * wall collections referencing unknown buildings

Exit code 0 = clean, 1 = findings, 2 = infrastructure failure.
Read-only by contract.
"""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[3]
PRESETS = REPO_ROOT / "Assets" / "Moyva" / "Presets"

BUILDINGS_DIR = PRESETS / "Buildings"
RESOURCES_DIR = PRESETS / "Economy" / "economy-resource"
UNITS_DIR = PRESETS / "Units"
REGISTRY_JSON = BUILDINGS_DIR / "building-registry" / "new-building-registry-so.json"

BUILDING_CATEGORIES = {"Military", "Civilian", "Industrial", "Walls"}
BUILDING_ROLES = {
    "None", "Housing", "Production", "Storage", "SettlementCenter",
    "Defense", "Wall", "Gate", "Decoration", "Support",
}
RESOURCE_CATEGORIES = {"None", "Food", "Materials", "Money"}
# Keep in sync with EconomyResourceCategory + runtime models.
KNOWN_MODELS = {
    "building-definition", "building-registry", "economy-resource",
    "unit-class", "unit-registry",
}

ASSET_ID_RE = re.compile(r"^asset\.[a-z0-9_-]+\.[a-z0-9_.-]+\.[0-9a-f]{8}$")

findings: list[str] = []


def report(message: str) -> None:
    findings.append(message)


def load_json(path: Path):
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        report(f"PARSE  {path.relative_to(REPO_ROOT)}: {exc}")
        return None


def iter_asset_refs(node, path: Path):
    """Yield every {"$asset": ...} node in the document."""
    if isinstance(node, dict):
        if "$asset" in node:
            yield node, path
        for value in node.values():
            yield from iter_asset_refs(value, path)
    elif isinstance(node, list):
        for value in node:
            yield from iter_asset_refs(value, path)


def check_asset_refs(doc, path: Path) -> None:
    for ref, source in iter_asset_refs(doc, path):
        asset_id = ref.get("$asset")
        editor_path = ref.get("editorPath")
        # Empty optional refs (required=false, no id, no path) are legal slots.
        if ref.get("required") is not True and not asset_id and not editor_path:
            continue
        if not isinstance(asset_id, str) or not ASSET_ID_RE.match(asset_id):
            report(
                f"ASSET  {source.relative_to(REPO_ROOT)}: malformed $asset id "
                f"{asset_id!r}"
            )
        if not isinstance(editor_path, str) or not editor_path.strip():
            report(
                f"ASSET  {source.relative_to(REPO_ROOT)}: {asset_id!r} lacks "
                "editorPath"
            )
            continue
        candidate = REPO_ROOT / editor_path.strip()
        if not candidate.is_file():
            report(
                f"ASSET  {source.relative_to(REPO_ROOT)}: {asset_id} → "
                f"{editor_path} does not exist"
            )


def collect_ids(documents, key: str, label: str) -> dict[str, Path]:
    ids: dict[str, Path] = {}
    for doc, path in documents:
        value = doc.get(key)
        if not isinstance(value, str) or not value.strip():
            report(f"ID     {path.relative_to(REPO_ROOT)}: missing '{key}'")
            continue
        value = value.strip()
        if value in ids:
            report(
                f"DUP    {label} id '{value}' in "
                f"{path.relative_to(REPO_ROOT)} and "
                f"{ids[value].relative_to(REPO_ROOT)}"
            )
        else:
            ids[value] = path
    return ids


def main() -> int:
    if not PRESETS.is_dir():
        print(f"error: preset root not found: {PRESETS}", file=sys.stderr)
        return 2

    # ── Resources ────────────────────────────────────────────────────────
    resource_docs = []
    for path in sorted(RESOURCES_DIR.glob("*.json")):
        doc = load_json(path)
        if doc is not None:
            resource_docs.append((doc, path))
            check_asset_refs(doc, path)
            category = doc.get("category")
            if category is not None and category not in RESOURCE_CATEGORIES:
                report(
                    f"ENUM   {path.relative_to(REPO_ROOT)}: resource category "
                    f"{category!r} not in {sorted(RESOURCE_CATEGORIES)}"
                )
    resource_ids = collect_ids(resource_docs, "id", "resource")

    # ── Units ────────────────────────────────────────────────────────────
    unit_docs = []
    for path in sorted(UNITS_DIR.glob("*.json")):
        doc = load_json(path)
        if doc is None or doc.get("model") != "unit-class":
            continue
        unit_docs.append((doc, path))
        check_asset_refs(doc, path)
        type_id = doc.get("typeId")
        doc_id = doc.get("id")
        if type_id and doc_id and type_id != doc_id:
            report(
                f"ID     {path.relative_to(REPO_ROOT)}: typeId {type_id!r} "
                f"!= id {doc_id!r}"
            )
    unit_ids = collect_ids(unit_docs, "typeId", "unit")

    # ── Buildings ────────────────────────────────────────────────────────
    # Only building-definition documents are validated as buildings; the
    # directory also carries packs/profiles that are not definitions.
    building_docs = []
    for path in sorted(BUILDINGS_DIR.glob("*.json")):
        doc = load_json(path)
        if doc is None:
            continue
        check_asset_refs(doc, path)
        if doc.get("model") == "building-definition" or "identity" in doc:
            building_docs.append((doc, path))
    building_ids = collect_ids(building_docs, "id", "building")

    for doc, path in building_docs:
        rel = path.relative_to(REPO_ROOT)
        identity = doc.get("identity") or {}
        category = identity.get("category")
        role = identity.get("role")
        if category is not None and category not in BUILDING_CATEGORIES:
            report(f"ENUM   {rel}: building category {category!r} unknown")
        if role is not None and role not in BUILDING_ROLES:
            report(f"ENUM   {rel}: building role {role!r} unknown")

        for entry in (doc.get("construction") or {}).get("cost") or []:
            rid = entry.get("resourceId")
            if isinstance(rid, str) and rid.strip() and rid not in resource_ids:
                report(f"REF    {rel}: construction cost '{rid}' undefined")

        for module in doc.get("modules") or []:
            if not isinstance(module, dict):
                continue
            mtype = module.get("$type", "?")
            for recipe in module.get("recipes") or []:
                for leg in ("inputs", "outputs"):
                    for entry in recipe.get(leg) or []:
                        rid = entry.get("resourceId")
                        if (
                            isinstance(rid, str)
                            and rid.strip()
                            and rid not in resource_ids
                        ):
                            report(
                                f"REF    {rel}: recipe "
                                f"{recipe.get('recipeId', '?')!r} {leg} "
                                f"'{rid}' undefined"
                            )
            for recipe in module.get("recipes") or []:
                uid = recipe.get("unitTypeId")
                if (
                    isinstance(uid, str)
                    and uid.strip()
                    and uid not in unit_ids
                ):
                    report(
                        f"REF    {rel}: {mtype} recipe unitTypeId "
                        f"{uid!r} undefined"
                    )

        model = doc.get("model")
        if model and model not in KNOWN_MODELS:
            report(f"MODEL  {rel}: unrecognised model {model!r}")

    # ── Wall/gate registry ───────────────────────────────────────────────
    registry = load_json(REGISTRY_JSON) if REGISTRY_JSON.is_file() else None
    if registry is not None:
        check_asset_refs(registry, REGISTRY_JSON)
        for coll in registry.get("wallCollections") or []:
            coll_id = coll.get("collectionId", "?")
            for key in ("wallBuildingId", "gateBuildingId"):
                bid = coll.get(key)
                if (
                    isinstance(bid, str)
                    and bid.strip()
                    and bid not in building_ids
                ):
                    report(
                        f"REF    {REGISTRY_JSON.relative_to(REPO_ROOT)}: "
                        f"collection {coll_id!r} {key} '{bid}' undefined"
                    )
    else:
        report(
            f"REF    {REGISTRY_JSON.relative_to(REPO_ROOT)}: registry missing"
        )

    # ── Report ───────────────────────────────────────────────────────────
    for line in findings:
        print(line)
    print(
        f"C20 summary: {len(building_ids)} buildings, "
        f"{len(resource_ids)} resources, {len(unit_ids)} units; "
        f"{len(findings)} finding(s)."
    )
    return 1 if findings else 0


if __name__ == "__main__":
    sys.exit(main())
