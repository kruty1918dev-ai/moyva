#!/usr/bin/env python3
"""One-shot mechanical migration: Kruty1918.Moyva.Jsonization -> Kruty1918.JsonConfig package.

Applies type renames, namespace/using rewrites, asmdef reference updates and
serialized m_EditorClassIdentifier patches. Run from repo root.
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

# Ordered list of (regex, replacement) applied to .cs files.
CS_RULES = [
    (r"\bMoyvaJsonRuntimeTypeResolver\b", "JsonConfigRuntimeTypeResolver"),
    (r"\bMoyvaJsonRuntime\b", "JsonConfigRuntime"),
    (r"\bMoyvaJsonTypeRegistry\b", "JsonConfigTypeRegistry"),
    (r"\bMoyvaJsonConfigObject\b", "JsonConfigObject"),
    (r"\bMoyvaJsonAssetCatalog\b", "JsonAssetCatalog"),
    (r"\bMoyvaJsonBindingMarker\b", "JsonBindingMarker"),
    (r"\bMoyvaJsonBindingRecord\b", "JsonBindingRecord"),
    (r"\bMoyvaJsonContractResolver\b", "JsonConfigContractResolver"),
    (r"\bMoyvaJsonDocumentMetadata\b", "JsonDocumentMetadata"),
    (r"\bMoyvaJsonObjectFactory\b", "JsonObjectFactory"),
    (r"\[MoyvaJson\]", "[JsonConfig]"),
    # qualified references: Kruty1918.Moyva.Jsonization.Something (not .Editor)
    (r"\bKruty1918\.Moyva\.Jsonization\.(?!Editor\b)", "Kruty1918.JsonConfig."),
    # bare namespace/using: Kruty1918.Moyva.Jsonization not followed by . or word char
    (r"\bKruty1918\.Moyva\.Jsonization\b(?![\.\w])", "Kruty1918.JsonConfig"),
]

# YAML serialized managed references: "assembly::namespace.type"
YAML_RULES = [
    (r"Kruty1918\.Moyva\.Jsonization::Kruty1918\.Moyva\.Jsonization\.MoyvaJsonAssetCatalog",
     "Kruty1918.JsonConfig::Kruty1918.JsonConfig.JsonAssetCatalog"),
    (r"Kruty1918\.Moyva\.Jsonization::Kruty1918\.Moyva\.Jsonization\.MoyvaJsonBindingMarker",
     "Kruty1918.JsonConfig::Kruty1918.JsonConfig.JsonBindingMarker"),
    (r"Kruty1918\.Moyva\.Jsonization::Kruty1918\.Moyva\.Jsonization\.MoyvaJsonBindingRecord",
     "Kruty1918.JsonConfig::Kruty1918.JsonConfig.JsonBindingRecord"),
]

OLD_ASM_NAME = "Kruty1918.Moyva.Jsonization"
OLD_ASM_GUID = "ba6fb4ee41f3e3638850bb445fa13ba1"
NEW_ASM_NAME = "Kruty1918.JsonConfig"
ADAPTER_ASMDEF = os.path.join("Assets", "Moyva", "Scripts", "Jsonization",
                              "Kruty1918.Moyva.Jsonization.asmdef")

CS_EXT = (".cs",)
YAML_EXT = (".prefab", ".unity", ".asset")

changed = []
skipped_self = 0

def apply_rules(path, rules):
    with open(path, "rb") as f:
        data = f.read()
    orig = data
    for pattern, repl in rules:
        data = re.sub(pattern.encode(), repl.encode(), data)
    if data != orig:
        with open(path, "wb") as f:
            f.write(data)
        changed.append(path)

for dirpath, dirs, files in os.walk(os.path.join(ROOT, "Assets")):
    for fn in files:
        p = os.path.join(dirpath, fn)
        rel = os.path.relpath(p, ROOT)
        if fn.endswith(CS_EXT):
            # game adapter file keeps its own type name
            if fn == "MoyvaJsonBootstrap.cs":
                rules = [(pat, rep) for pat, rep in CS_RULES
                         if "Bootstrap" not in rep]
                # bootstrap must keep class name MoyvaJsonBootstrap
                apply_rules(p, [(r"\bMoyvaJsonRuntime\b", "JsonConfigRuntime"),
                                (r"\[MoyvaJson\]", "[JsonConfig]")])
            else:
                apply_rules(p, CS_RULES)
        elif fn.endswith(YAML_EXT):
            apply_rules(p, YAML_RULES)
        elif fn.endswith(".asmdef"):
            if os.path.normpath(rel) == os.path.normpath(ADAPTER_ASMDEF):
                continue  # handled manually
            apply_rules(p, [
                (r'"Kruty1918\.Moyva\.Jsonization"',
                 '"Kruty1918.JsonConfig"'),
                (r"GUID:ba6fb4ee41f3e3638850bb445fa13ba1",
                 '"Kruty1918.JsonConfig"'),
            ])

# package runtime files
pkg = os.path.join(ROOT, "Packages", "com.kruty1918.json-config", "Runtime")
for fn in os.listdir(pkg):
    if fn.endswith(".cs"):
        apply_rules(os.path.join(pkg, fn), CS_RULES)

print(f"changed {len(changed)} files")
for c in changed:
    print("  " + os.path.relpath(c, ROOT))
