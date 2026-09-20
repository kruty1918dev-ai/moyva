#!/usr/bin/env python3
"""Interface & Resolve audit for Moyva production code.

Generates Temp/ai/interface-audit/report.json + report.md.
Read-only analysis; does not modify sources.
"""
import os, re, json, sys
from collections import defaultdict

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS = os.path.join(ROOT, "Assets", "Moyva", "Scripts")
MOYVA = os.path.join(ROOT, "Assets", "Moyva")
OUT_DIR = os.path.join(ROOT, "Temp", "ai", "interface-audit")

IFACE_RE = re.compile(r'^\s*(?:public|internal|private|protected)?\s*(?:partial\s+)?interface\s+(\w+)')
CLASS_RE = re.compile(r'^\s*(?:public|internal|private|protected)?\s*(?:sealed\s+|abstract\s+|static\s+|partial\s+|readonly\s+)*class\s+(\w+)\s*(?::\s*([^\n{]+))?')
STRUCT_RE = re.compile(r'^\s*(?:public|internal|private|protected)?\s*(?:sealed\s+|readonly\s+|partial\s+)*struct\s+(\w+)\s*(?::\s*([^\n{]+))?')
BIND_RE = re.compile(r'\bBind(?:InterfacesAndSelfTo|InterfacesTo|To)?<([^>]+)>')
RESOLVE_RE = re.compile(r'([A-Za-z_][A-Za-z0-9_.]*)\s*\.\s*(Try)?Resolve(?:All)?\s*(?:<([^>]*)>|\()',)
TOKEN_RE = re.compile(r'\bI[A-Z]\w*\b')


def is_container_receiver(recv):
    last = recv.split('.')[-1]
    if last in ("Container", "DiContainer"):
        return True
    if '.' not in recv and (recv[0].islower() or recv[0] == '_') and (
            last.endswith('Container') or last.endswith('container')):
        return True
    return False


def classify_path(path):
    norm = path.replace("\\", "/")
    segs = set(norm.split('/'))
    if "Tests" in segs:
        return 'test'
    if segs & {"Editor", "EditorShared", "Development"}:
        return 'editor'
    return 'prod'


def feature_of(path):
    norm = path.replace("\\", "/")
    m = re.search(r'Assets/Moyva/Scripts/Features/([^/]+)/', norm)
    if m:
        return "Features/" + m.group(1)
    m = re.search(r'Assets/Moyva/Scripts/([^/]+)/', norm)
    if m:
        return m.group(1)
    return "Other"


def all_cs(root):
    for dp, dn, fn in os.walk(root):
        dn[:] = [d for d in dn if d not in ("Library", "Temp", "Obj", "obj", "bin")]
        for f in fn:
            if f.endswith(".cs"):
                yield os.path.join(dp, f)


def strip_comments(src):
    src = re.sub(r'/\*.*?\*/', '', src, flags=re.S)
    src = re.sub(r'//[^\n]*', '', src)
    return src


def split_base_list(base):
    # split "IFoo, Bar<Baz>, Quux" respecting generics
    parts, depth, cur = [], 0, []
    for ch in base:
        if ch == '<':
            depth += 1
        elif ch == '>':
            depth -= 1
        if ch == ',' and depth == 0:
            parts.append(''.join(cur).strip())
            cur = []
        else:
            cur.append(ch)
    if cur:
        parts.append(''.join(cur).strip())
    return [p.split('<')[0].strip() for p in parts if p]


def main():
    prod_files = []
    test_files = []
    editor_files = []
    for f in all_cs(MOYVA):
        rel = os.path.relpath(f, ROOT)
        cls = classify_path(rel)
        if cls == 'test':
            test_files.append(f)
        elif cls == 'editor':
            editor_files.append(f)
        else:
            prod_files.append(f)

    interfaces = {}  # name -> {file, feature, in_api}
    impls = defaultdict(list)  # iface name -> list of class names
    impl_features = defaultdict(set)
    per_file_src = {}

    for f in prod_files:
        try:
            src = open(f, encoding="utf-8", errors="replace").read()
        except OSError:
            continue
        rel = os.path.relpath(f, ROOT).replace("\\", "/")
        per_file_src[f] = src
        clean = strip_comments(src)
        feat = feature_of(rel)
        in_api = "/API/" in rel.replace("\\", "/")
        for line in clean.splitlines():
            m = IFACE_RE.match(line)
            if m:
                name = m.group(1)
                interfaces.setdefault(name, {"file": rel, "feature": feat, "in_api": in_api})
            for cre in (CLASS_RE, STRUCT_RE):
                cm = cre.match(line)
                if cm and cm.group(2):
                    cname = cm.group(1)
                    for base in split_base_list(cm.group(2)):
                        if base and base[0].isupper():
                            impls[base].append(cname)
                            impl_features[base].add(feat)

    # usages: count files referencing each interface name
    usage = defaultdict(lambda: {"same_feature": set(), "cross_feature": set(), "tests": set(), "editor": set()})
    bindings = defaultdict(list)  # iface -> list of (file, line)
    resolve_sites = []
    iface_names = set(interfaces)

    def scan_file(f, bucket):
        src = per_file_src.get(f)
        if src is None:
            try:
                src = open(f, encoding="utf-8", errors="replace").read()
            except OSError:
                return
        rel = os.path.relpath(f, ROOT).replace("\\", "/")
        feat = feature_of(rel)
        clean = strip_comments(src)
        for i, line in enumerate(clean.splitlines(), 1):
            for tok in TOKEN_RE.findall(line):
                if tok in iface_names:
                    name = tok
                    if bucket == 'test':
                        usage[name]["tests"].add(rel)
                    elif bucket == 'editor':
                        usage[name]["editor"].add(rel)
                    elif feat == interfaces[name]["feature"]:
                        usage[name]["same_feature"].add(rel)
                    else:
                        usage[name]["cross_feature"].add(rel)
            for bm in BIND_RE.finditer(line):
                for t in bm.group(1).split(","):
                    t = t.strip()
                    if t in interfaces:
                        bindings[t].append((rel, i))
            for rm in RESOLVE_RE.finditer(line):
                if not is_container_receiver(rm.group(1)):
                    continue
                resolve_sites.append({
                    "file": rel, "line": i,
                    "kind": "TryResolve" if rm.group(2) else "Resolve",
                    "target": rm.group(3) or "",
                    "text": line.strip()[:160],
                    "feature": feat, "bucket": bucket,
                })

    for f in prod_files:
        scan_file(f, 'prod')
    for f in test_files:
        scan_file(f, 'test')
    for f in editor_files:
        scan_file(f, 'editor')

    report_ifaces = []
    for name, meta in interfaces.items():
        u = usage.get(name, {"same_feature": set(), "cross_feature": set(), "tests": set()})
        impl_list = impls.get(name, [])
        report_ifaces.append({
            "name": name,
            "file": meta["file"],
            "feature": meta["feature"],
            "in_api": meta["in_api"],
            "impl_count": len(set(impl_list)),
            "impls": sorted(set(impl_list)),
            "impl_features": sorted(impl_features.get(name, set())),
            "bindings": bindings.get(name, []),
            "same_feature_users": sorted(u["same_feature"]),
            "cross_feature_users": sorted(u["cross_feature"]),
            "test_users": sorted(u["tests"]),
            "editor_users": sorted(u["editor"]),
        })

    os.makedirs(OUT_DIR, exist_ok=True)
    with open(os.path.join(OUT_DIR, "report.json"), "w", encoding="utf-8") as fh:
        json.dump({"interfaces": report_ifaces, "resolve_sites": resolve_sites}, fh, indent=1)

    # summary md
    single = [i for i in report_ifaces if i["impl_count"] <= 1]
    cross = [i for i in report_ifaces if i["cross_feature_users"]]
    seams = [i for i in report_ifaces if i["test_users"]]
    lines = [
        "# Interface & Resolve audit",
        "",
        f"Production interfaces: {len(report_ifaces)}",
        f"Interfaces with 0-1 implementations: {len(single)}",
        f"Interfaces used cross-feature: {len(cross)}",
        f"Interfaces used in tests: {len(seams)}",
        f"Resolve/TryResolve call sites (prod): {sum(1 for s in resolve_sites if s['bucket'] == 'prod')}",
        f"Resolve/TryResolve call sites (tests): {sum(1 for s in resolve_sites if s['bucket'] == 'test')}",
        f"Resolve/TryResolve call sites (editor): {sum(1 for s in resolve_sites if s['bucket'] == 'editor')}",
        "",
        "## Single-impl interfaces WITHOUT cross-feature or test usage (mechanical candidates)",
        "",
    ]
    mech = [i for i in single if not i["cross_feature_users"] and not i["test_users"] and not i["editor_users"]]
    for i in sorted(mech, key=lambda x: (x["feature"], x["name"])):
        bound = "bound" if i["bindings"] else "unbound"
        api = "API" if i["in_api"] else "runtime"
        lines.append(f"- `{i['name']}` ({i['feature']}, {api}, {bound}) impl={','.join(i['impls']) or 'none'} sameUsers={len(i['same_feature_users'])}")
    lines += ["", "## Resolve/TryResolve call sites (production)", ""]
    for s in sorted(resolve_sites, key=lambda x: x["file"]):
        if s["bucket"] != 'prod':
            continue
        lines.append(f"- {s['file']}:{s['line']} [{s['kind']}] `{s['text']}`")
    lines += ["", "## Resolve/TryResolve call sites (editor)", ""]
    for s in sorted(resolve_sites, key=lambda x: x["file"]):
        if s["bucket"] != 'editor':
            continue
        lines.append(f"- {s['file']}:{s['line']} [{s['kind']}] `{s['text']}`")

    with open(os.path.join(OUT_DIR, "report.md"), "w", encoding="utf-8") as fh:
        fh.write("\n".join(lines))

    print(f"interfaces={len(report_ifaces)} single_or_zero={len(single)} cross={len(cross)} testseams={len(seams)} mechanical={len(mech)} "
          f"resolve_prod={sum(1 for s in resolve_sites if s['bucket'] == 'prod')} "
          f"resolve_test={sum(1 for s in resolve_sites if s['bucket'] == 'test')} "
          f"resolve_editor={sum(1 for s in resolve_sites if s['bucket'] == 'editor')}")
    print("out:", OUT_DIR)


if __name__ == "__main__":
    main()
