#!/usr/bin/env python3
"""Rewrite a namespace across Assets and ensure owning asmdefs gain the new ref.

usage: upm_ns_rewrite.py <old_ns> <new_ns> <new_asm> [--skip-file NAME]...
                         [--skip-substring SUB]...

- Replaces <old_ns> with <new_ns> in every .cs under Assets/ (skipping files
  whose name is in --skip-file or whose path contains a --skip-substring).
- Afterwards, every .cs that mentions <new_ns> is mapped to its owning asmdef
  (nearest ancestor .asmdef); that asmdef gains "<new_asm>" in references
  when missing.
Prints a short summary only.
"""
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

def main():
    args = sys.argv[1:]
    old_ns, new_ns, new_asm = args[0], args[1], args[2]
    skip_files = set()
    skip_subs = []
    i = 3
    while i < len(args):
        if args[i] == "--skip-file":
            skip_files.add(args[i + 1]); i += 2
        elif args[i] == "--skip-substring":
            skip_subs.append(args[i + 1]); i += 2
        else:
            i += 1

    pat = re.compile(re.escape(old_ns).encode() + rb"\b")
    touched = []

    for dirpath, _dirs, files in os.walk(os.path.join(ROOT, "Assets")):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            p = os.path.join(dirpath, fn)
            rel = os.path.relpath(p, ROOT)
            if fn in skip_files or any(s in rel for s in skip_subs):
                continue
            data = open(p, "rb").read()
            new = pat.sub(new_ns.encode(), data)
            if new != data:
                open(p, "wb").write(new)
                touched.append(rel)

    # asmdef ref fix-up
    asmdefs = []
    for dirpath, _dirs, files in os.walk(os.path.join(ROOT, "Assets")):
        for fn in files:
            if fn.endswith(".asmdef"):
                asmdefs.append(os.path.join(dirpath, fn))
    asmdefs.sort(key=len, reverse=True)  # deepest first for nearest-ancestor match

    new_ns_b = new_ns.encode()
    changed_asm = []
    for dirpath, _dirs, files in os.walk(os.path.join(ROOT, "Assets")):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            p = os.path.join(dirpath, fn)
            if new_ns_b not in open(p, "rb").read():
                continue
            owner = None
            d = dirpath
            while True:
                for cand in asmdefs:
                    if os.path.dirname(cand) == d:
                        owner = cand
                        break
                if owner or d == ROOT:
                    break
                d = os.path.dirname(d)
            if not owner:
                print("NO-ASMDEF", p)
                continue
            text = open(owner, encoding="utf-8").read()
            if f'"{new_asm}"' in text:
                continue
            # asmdefs allow // comments — insert the ref textually instead of JSON round-trip
            m = re.search(r'"references"\s*:\s*\[', text)
            if not m:
                print("ASMDEF-NO-REFS", owner)
                continue
            insert_at = m.end()
            indent = re.match(r"[ \t]*", text[insert_at:]).group(0) or "    "
            text = text[:insert_at] + f'\n{indent}"{new_asm}",' + text[insert_at:]
            open(owner, "w", encoding="utf-8", newline="\n").write(text)
            changed_asm.append(os.path.relpath(owner, ROOT))

    print(f"cs-rewritten: {len(touched)}")
    print(f"asmdefs-updated: {len(changed_asm)}")
    for a in changed_asm:
        print("  " + a)

if __name__ == "__main__":
    main()
