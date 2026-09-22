#!/usr/bin/env python3
"""Compile all project + package C# sources with Roslyn (no Unity Editor needed).
Catches syntax errors, missing usings/types/members. Does NOT validate asmdef refs."""
import glob
import os
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DOTNET = os.path.expanduser(r"~\.dotnet\dotnet.exe")
CSC = os.path.expanduser(r"~\.dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll")
EDITOR = os.path.expanduser(r"~\AppData\Local\Unity\Editors\6000.3.10f1\Editor\Data\Managed")
NETSTD = os.path.expanduser(r"~\.dotnet\packs\NETStandard.Library.Ref\2.1.0\ref\netstandard2.1")

EXCLUDE_DIRS = (
    os.sep + "Plugins" + os.sep, os.sep + "ThirdParty" + os.sep,
    os.sep + "KayKit" + os.sep, os.sep + "FlatKit" + os.sep,
    os.sep + "TextMesh Pro" + os.sep, os.sep + "Library" + os.sep,
    os.sep + "Temp" + os.sep, os.sep + "Obj" + os.sep,
)

DEFINES = [
    "UNITY_EDITOR", "UNITY_STANDALONE", "UNITY_STANDALONE_WIN", "UNITY_64",
    "UNITY_6000", "UNITY_6000_0_OR_NEWER", "UNITY_6000_1_OR_NEWER",
    "UNITY_6000_2_OR_NEWER", "UNITY_6000_3_OR_NEWER", "UNITY_6000_4_OR_NEWER",
    "UNITY_6000_5_OR_NEWER", "UNITY_6000_6_OR_NEWER",
    "UNITY_2021_1_OR_NEWER", "UNITY_2021_2_OR_NEWER", "UNITY_2021_3_OR_NEWER",
    "UNITY_2022_1_OR_NEWER", "UNITY_2022_2_OR_NEWER", "UNITY_2022_3_OR_NEWER",
    "UNITY_2023_1_OR_NEWER", "UNITY_2023_2_OR_NEWER", "UNITY_2023_3_OR_NEWER",
    "DOTWEEN", "ODIN_INSPECTOR", "ODIN_INSPECTOR_3", "APP_UI_EDITOR_ONLY",
    "SENTIS_ANALYTICS_ENABLED",
]

def collect_sources():
    srcs = []
    for base in (os.path.join(ROOT, "Assets"), os.path.join(ROOT, "Packages")):
        for p in glob.glob(os.path.join(base, "**", "*.cs"), recursive=True):
            if any(x in p for x in EXCLUDE_DIRS):
                continue
            srcs.append(p)
    return srcs

def collect_refs():
    refs = []
    refs += glob.glob(os.path.join(NETSTD, "*.dll"))
    refs += glob.glob(os.path.join(EDITOR, "UnityEngine", "*.dll"))
    refs += glob.glob(os.path.join(EDITOR, "UnityEditor", "*.dll"))
    refs += glob.glob(os.path.join(EDITOR, "*.dll"))  # UnityEngine.dll facades etc
    for dll in glob.glob(os.path.join(ROOT, "Library", "ScriptAssemblies", "*.dll")):
        name = os.path.basename(dll)
        if name.startswith("Kruty1918.") or name.startswith("Assembly-CSharp"):
            continue  # compiled from source here
        refs.append(dll)
    refs += glob.glob(os.path.join(ROOT, "Assets", "Plugins", "**", "*.dll"), recursive=True)
    for pat in ("**/nunit.framework.dll", "**/Newtonsoft.Json.dll"):
        refs += glob.glob(os.path.join(ROOT, "Library", "PackageCache", pat), recursive=True)
    return sorted(set(refs))

def main():
    srcs = collect_sources()
    refs = collect_refs()
    out_dir = os.path.join(ROOT, "Temp", "ai", "smoke")
    os.makedirs(out_dir, exist_ok=True)
    rsp = os.path.join(out_dir, "sources.rsp")
    with open(rsp, "w", encoding="utf-8") as f:
        f.write("/nologo /target:library /unsafe+ /langversion:latest /nowarn:1701,1702,0436,0067,0414,0649,0219 /nullable:disable\n")
        f.write("/define:" + ";".join(DEFINES) + "\n")
        for r in refs:
            f.write(f'/r:"{r}"\n')
        f.write(f'/out:"{os.path.join(out_dir, "MoyvaAll.dll")}"\n')
        for s in srcs:
            f.write(f'"{s}"\n')
    print(f"sources={len(srcs)} refs={len(refs)}")
    res = subprocess.run([DOTNET, CSC, f"@{rsp}"], capture_output=True, text=True,
                         encoding="utf-8", errors="replace")
    logp = os.path.join(out_dir, "smoke_compile.log")
    with open(logp, "w", encoding="utf-8") as f:
        f.write(res.stdout + res.stderr)
    all_lines = [l for l in (res.stdout + res.stderr).splitlines() if " error " in l]
    # Single-assembly compile false positive: namespace Kruty1918.Moyva.Camera
    # shadows UnityEngine.Camera in files whose real asmdef never references it.
    lines = [l for l in all_lines if not ('CS0118' in l and '"Camera"' in l)]
    print(f"errors={len(lines)} (suppressed {len(all_lines)-len(lines)} known Camera-namespace false positives) log: {logp}")
    for l in lines[:60]:
        sys.stdout.buffer.write(l[:300].encode("utf-8", errors="replace") + b"\n")
    return 1 if lines else 0

if __name__ == "__main__":
    sys.exit(main())
