#!/usr/bin/env python3
import argparse
import re
from pathlib import Path

ALLOWED_STATUS = {"Planned", "InProgress", "Blocked", "Done"}
ROW_RE = re.compile(r"^\|\s*(TD-\d{3})\s*\|")


def parse_rows(lines):
    rows = []
    in_items = False
    for line in lines:
        if line.strip() == "## Items":
            in_items = True
            continue

        if not in_items:
            continue

        if not line.startswith("|"):
            continue

        if "---" in line:
            continue

        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if cells and cells[0] == "ID":
            continue

        if len(cells) != 12:
            raise ValueError(f"Invalid table row columns count: expected 12, got {len(cells)} in row: {line.strip()}")

        rows.append(cells)

    if not rows:
        raise ValueError("No debt items found in table.")

    return rows


def as_int(value, field_name, row_id):
    try:
        return int(value)
    except ValueError as exc:
        raise ValueError(f"{row_id}: field {field_name} must be integer, got '{value}'") from exc


def validate(rows, strict_order=True):
    seen_ids = set()
    previous_priority = None

    for row in rows:
        row_id, title, area, impact_s, risk_s, effort_s, urgency_s, priority_s, status, owner, target, link = row

        if not ROW_RE.match(f"| {row_id} |"):
            raise ValueError(f"Invalid ID format: {row_id}. Expected TD-XXX.")

        if row_id in seen_ids:
            raise ValueError(f"Duplicate ID: {row_id}")
        seen_ids.add(row_id)

        if not title:
            raise ValueError(f"{row_id}: Title cannot be empty")
        if not area:
            raise ValueError(f"{row_id}: Area cannot be empty")
        if not owner:
            raise ValueError(f"{row_id}: Owner cannot be empty")
        if not target:
            raise ValueError(f"{row_id}: Target cannot be empty")
        if not link:
            raise ValueError(f"{row_id}: Link cannot be empty")

        impact = as_int(impact_s, "Impact", row_id)
        risk = as_int(risk_s, "Risk", row_id)
        effort = as_int(effort_s, "Effort", row_id)
        urgency = as_int(urgency_s, "Urgency", row_id)
        priority = as_int(priority_s, "Priority", row_id)

        if not (1 <= impact <= 5):
            raise ValueError(f"{row_id}: Impact out of range 1..5")
        if not (1 <= risk <= 5):
            raise ValueError(f"{row_id}: Risk out of range 1..5")
        if not (1 <= effort <= 5):
            raise ValueError(f"{row_id}: Effort out of range 1..5")
        if not (1 <= urgency <= 3):
            raise ValueError(f"{row_id}: Urgency out of range 1..3")

        expected_priority = impact * risk + urgency - effort
        if priority != expected_priority:
            raise ValueError(
                f"{row_id}: Priority mismatch. Expected {expected_priority} by formula Impact*Risk+Urgency-Effort, got {priority}"
            )

        if status not in ALLOWED_STATUS:
            raise ValueError(f"{row_id}: Invalid status '{status}'. Allowed: {', '.join(sorted(ALLOWED_STATUS))}")

        if strict_order and previous_priority is not None and priority > previous_priority:
            raise ValueError(f"{row_id}: Register is not sorted by descending Priority")

        previous_priority = priority


def main():
    parser = argparse.ArgumentParser(description="Validate tech debt register markdown table")
    parser.add_argument("--file", default="docs/architecture/tech-debt-register.md", help="Path to register file")
    parser.add_argument("--no-order-check", action="store_true", help="Disable descending priority order check")
    args = parser.parse_args()

    path = Path(args.file)
    if not path.exists():
        raise SystemExit(f"Register file not found: {path}")

    lines = path.read_text(encoding="utf-8").splitlines()
    rows = parse_rows(lines)
    validate(rows, strict_order=not args.no_order_check)

    print(f"Tech debt register OK: {path} ({len(rows)} items)")


if __name__ == "__main__":
    main()
