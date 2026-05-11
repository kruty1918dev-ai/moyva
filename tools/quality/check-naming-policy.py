#!/usr/bin/env python3
import argparse
import re
import subprocess
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Sequence, Set, Tuple

ROOT = Path(__file__).resolve().parents[2]
SCRIPTS_ROOT = ROOT / "Assets" / "Moyva" / "Scripts"

CLASS_RE = re.compile(
    r"^\s*(?:public|internal|private|protected)?\s*(?P<mods>(?:(?:sealed|abstract|static|partial)\s+)*)class\s+(?P<name>\w+)\s*(?::\s*(?P<bases>[^\{]+))?",
    re.MULTILINE,
)
INTERFACE_RE = re.compile(
    r"^\s*(?:public|internal|private|protected)?\s*interface\s+(?P<name>\w+)\s*(?::\s*(?P<bases>[^\{]+))?",
    re.MULTILINE,
)

ROLE_SUFFIXES = ("Service", "Provider", "Resolver", "Installer")
SETTINGS_SUFFIXES = ("SettingsSO", "ConfigSO")
SERVICE_CONTEXT_ALLOWED_SUFFIXES = ("Service", "Adapter", "DomainLogic", "Lifecycle", "Factory")


@dataclass(frozen=True)
class Violation:
    file_path: str
    symbol_name: str
    rule_id: str
    message: str

    @property
    def key(self) -> Tuple[str, str, str]:
        return (self.file_path, self.symbol_name, self.rule_id)


def run_git(args: Sequence[str]) -> str:
    result = subprocess.run(["git", *args], cwd=ROOT, capture_output=True, text=True)
    if result.returncode != 0:
        raise RuntimeError(result.stderr.strip() or "git command failed")
    return result.stdout


def list_candidate_files(base_ref: Optional[str], changed_only: bool) -> List[Path]:
    if changed_only:
        if not base_ref:
            raise ValueError("--changed-only requires --base-ref")
        output = run_git(["diff", "--name-only", f"{base_ref}...HEAD"])
        paths = []
        for line in output.splitlines():
            line = line.strip()
            if not line.endswith(".cs"):
                continue
            if not line.startswith("Assets/Moyva/Scripts/"):
                continue
            full = ROOT / line
            if full.exists():
                paths.append(full)
        return sorted(paths)

    return sorted(SCRIPTS_ROOT.rglob("*.cs"))


def split_bases(raw: Optional[str]) -> List[str]:
    if not raw:
        return []
    return [part.strip() for part in raw.split(",") if part.strip()]


def analyze_text(text: str, rel_path: str) -> List[Violation]:
    violations: List[Violation] = []
    is_test_file = "/Tests/" in rel_path or rel_path.endswith("Tests.cs")

    if is_test_file:
        return violations

    for m in CLASS_RE.finditer(text):
        class_name = m.group("name")
        bases = split_bases(m.group("bases"))
        modifiers = (m.group("mods") or "").split()
        del modifiers

        # Service role inferred from folder or implemented base interfaces.
        service_context = (
            "/Runtime/Services/" in rel_path
            or any(base.endswith("Service") or base.startswith("I") and base.endswith("Service") for base in bases)
        )
        if service_context and not class_name.endswith(SERVICE_CONTEXT_ALLOWED_SUFFIXES):
            violations.append(
                Violation(
                    file_path=rel_path,
                    symbol_name=class_name,
                    rule_id="class-service-suffix",
                    message=(
                        f"Service-context class '{class_name}' should end with one of: "
                        f"{', '.join(SERVICE_CONTEXT_ALLOWED_SUFFIXES)}."
                    ),
                )
            )

        # Provider role inferred from base/interface or provider folder.
        provider_base_hit = any(
            (base.endswith("Provider") and not base.endswith("PreviewProvider"))
            or (base.startswith("I") and base.endswith("Provider") and not base.endswith("PreviewProvider"))
            for base in bases
        )
        provider_context = (
            "/Runtime/" in rel_path
            and (
                "/Providers/" in rel_path
                or class_name.endswith("Provider")
                or provider_base_hit
            )
        )
        if provider_context and not class_name.endswith("Provider"):
            violations.append(
                Violation(
                    file_path=rel_path,
                    symbol_name=class_name,
                    rule_id="class-provider-suffix",
                    message=f"Provider-context class '{class_name}' should end with 'Provider'.",
                )
            )

        # Resolver role inferred from base/interface or resolver folder.
        resolver_context = (
            "/Resolvers/" in rel_path
            or any(base.endswith("Resolver") or base.startswith("I") and base.endswith("Resolver") for base in bases)
        )
        if resolver_context and not class_name.endswith("Resolver"):
            violations.append(
                Violation(
                    file_path=rel_path,
                    symbol_name=class_name,
                    rule_id="class-resolver-suffix",
                    message=f"Resolver-context class '{class_name}' should end with 'Resolver'.",
                )
            )

        # Installer role inferred from base type.
        if any(base.endswith("Installer") or base == "MonoInstaller" for base in bases):
            if not class_name.endswith("Installer"):
                violations.append(
                    Violation(
                        file_path=rel_path,
                        symbol_name=class_name,
                        rule_id="class-installer-suffix",
                        message=f"Installer class '{class_name}' should end with 'Installer'.",
                    )
                )

        # Settings role inferred from ScriptableObject naming.
        if any(base.endswith("ScriptableObject") for base in bases):
            has_settings_token = ("Settings" in class_name) or ("Config" in class_name)
            if has_settings_token and not class_name.endswith(SETTINGS_SUFFIXES):
                violations.append(
                    Violation(
                        file_path=rel_path,
                        symbol_name=class_name,
                        rule_id="class-settings-suffix",
                        message=(
                            f"Settings/config ScriptableObject '{class_name}' should end with one of: "
                            f"{', '.join(SETTINGS_SUFFIXES)}."
                        ),
                    )
                )

    for m in INTERFACE_RE.finditer(text):
        interface_name = m.group("name")
        if interface_name.endswith("Tests"):
            continue
        for role in ROLE_SUFFIXES:
            if role in interface_name:
                if not interface_name.startswith("I"):
                    violations.append(
                        Violation(
                            file_path=rel_path,
                            symbol_name=interface_name,
                            rule_id=f"interface-{role.lower()}-prefix",
                            message=f"Interface '{interface_name}' should start with 'I'.",
                        )
                    )
                if not interface_name.endswith(role):
                    violations.append(
                        Violation(
                            file_path=rel_path,
                            symbol_name=interface_name,
                            rule_id=f"interface-{role.lower()}-suffix",
                            message=f"Interface '{interface_name}' should end with '{role}'.",
                        )
                    )

    return violations


def analyze_file(path: Path) -> List[Violation]:
    rel = path.relative_to(ROOT).as_posix()
    try:
        text = path.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        return []
    return analyze_text(text, rel)


def analyze_base_file(base_ref: str, rel_path: str) -> List[Violation]:
    try:
        text = run_git(["show", f"{base_ref}:{rel_path}"])
    except Exception:
        return []
    return analyze_text(text, rel_path)


def main() -> int:
    parser = argparse.ArgumentParser(description="Check naming policy for Service/Provider/Resolver/Settings/Installer.")
    parser.add_argument("--base-ref", default="", help="Base git ref for debt comparison.")
    parser.add_argument("--changed-only", action="store_true", help="Check only changed C# files.")
    parser.add_argument("--strict", action="store_true", help="Fail on new violations.")
    args = parser.parse_args()

    base_ref = args.base_ref.strip() or None
    if base_ref:
        run_git(["rev-parse", "--verify", base_ref])

    files = list_candidate_files(base_ref, args.changed_only)
    total_files = len(files)

    current: List[Violation] = []
    for path in files:
        current.extend(analyze_file(path))

    debt_keys: Set[Tuple[str, str, str]] = set()
    if base_ref:
        rel_paths = sorted({v.file_path for v in current})
        for rel in rel_paths:
            for v in analyze_base_file(base_ref, rel):
                debt_keys.add(v.key)

    warnings = 0
    failures = 0

    for v in sorted(current, key=lambda x: (x.file_path, x.symbol_name, x.rule_id)):
        if v.key in debt_keys:
            warnings += 1
            print(f"DEBT [{v.rule_id}] {v.file_path} :: {v.message}")
        else:
            if args.strict:
                failures += 1
                print(f"FAIL [{v.rule_id}] {v.file_path} :: {v.message}")
            else:
                warnings += 1
                print(f"WARN [{v.rule_id}] {v.file_path} :: {v.message}")

    print("")
    print(f"Checked files: {total_files}")
    print(f"Violations:    {len(current)}")
    print(f"Warnings:      {warnings}")
    print(f"Failures:      {failures}")

    if failures > 0:
        print("")
        print("Naming policy check failed: new violations detected.")
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
