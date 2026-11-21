"""Read-only Files deployment metadata scan. Never invokes MSBuild or deployment."""
from pathlib import Path
from datetime import datetime, timezone
import argparse
import hashlib
import json
import os
import subprocess
import xml.etree.ElementTree as E

ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / "m0/out/f9a-baseline-20261121T090938Z-a08ae53c-run1/source"
PIN = "cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
OUT = SOURCE / "src/Files.App/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64"
NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}

def local(tag):
    return tag.split("}")[-1]

def digest(data):
    return hashlib.sha256(data).hexdigest()

def file_info(path):
    info = {"path": str(path), "exists": path.is_file()}
    if path.is_file():
        info.update(bytes=path.stat().st_size, sha256=digest(path.read_bytes()))
    return info

def git(*args):
    return subprocess.check_output(["git", "--no-optional-locks", "-C", str(SOURCE), *args]).decode().strip()

def source_state():
    return {"head": git("rev-parse", "HEAD"), "tree": git("rev-parse", "HEAD^{tree}"), "status_porcelain": git("status", "--porcelain"), "tracked_diff": git("diff", "HEAD", "--")}

def source_ref(relative, tokens):
    path = SOURCE / relative
    lines = path.read_text(encoding="utf-8-sig").splitlines()
    return {**file_info(path), "git_blob": git("rev-parse", "HEAD:" + relative), "line_references": {token: [i for i, line in enumerate(lines, 1) if token in line] for token in tokens}}

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--output", required=True)
    args = ap.parse_args()
    dest = (ROOT / args.output).resolve()
    assert dest.is_relative_to((ROOT / "m0/out").resolve())
    dest.mkdir(parents=True, exist_ok=False)
    before = source_state()
    assert before["head"] == PIN and not before["tracked_diff"] and not before["status_porcelain"]
    manifest_path = OUT / "AppxManifest.xml"
    recipe_path = OUT / "Files.App.build.appxrecipe"
    manifest = E.parse(manifest_path).getroot()
    recipe = E.parse(recipe_path).getroot()
    identity = next(n for n in manifest if local(n.tag) == "Identity")
    applications = [dict(n.attrib) for n in manifest.iter() if local(n.tag) == "Application"]
    dependencies = [{"kind": local(n.tag), **n.attrib} for parent in manifest if local(parent.tag) == "Dependencies" for n in parent]
    capabilities = [n.attrib["Name"] for parent in manifest if local(parent.tag) == "Capabilities" for n in parent]
    extensions = [{"category": n.attrib.get("Category"), "entry_point": n.attrib.get("EntryPoint"), "executable": n.attrib.get("Executable"), "declared_paths": [x.text for x in n.iter() if local(x.tag) == "Path"]} for n in manifest.iter() if local(n.tag) == "Extension"]
    paths = {n.text for n in manifest.iter() if local(n.tag) == "Path" and n.text}
    paths |= {n.attrib["Executable"] for n in manifest.iter() if "Executable" in n.attrib}
    path_checks = [{"manifest_relative_path": path, **file_info(OUT / path.replace("\\", "/"))} for path in sorted(paths)]
    properties = {local(n.tag): n.text for n in recipe.find("m:PropertyGroup", NS)}
    mapped = []
    for n in recipe.findall(".//m:AppxPackagedFile", NS):
        src = Path(n.attrib["Include"])
        mapped.append({"target": n.findtext("m:PackagePath", default="", namespaces=NS), "source_exists": src.is_file()})
    reference_names = {"AdditionalPackageReference", "ProjectReference", "OptionalProjectRecipeFiles", "BundleMappingRecipes"}
    references = [{"kind": local(n.tag), "attributes": n.attrib, "text": n.text} for n in recipe.iter() if local(n.tag) in reference_names]
    server_dir = SOURCE / "src/Files.App.Server/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64"
    imports = {}
    for project in ["Files.App", "Files.App.Server"]:
        f = SOURCE / f"src/{project}/obj/{project}.csproj.nuget.g.targets"
        tree = E.parse(f)
        imports[project] = {**file_info(f), "imports": [n.attrib["Project"] for n in tree.iter() if local(n.tag) == "Import"]}
    sdk = Path(os.environ["USERPROFILE"]) / ".nuget/packages/microsoft.windows.sdk.buildtools.msix/1.7.251221100/build"
    target_path = sdk / "Microsoft.Windows.SDK.BuildTools.MSIX.Packaging.targets"
    lines = target_path.read_text(encoding="utf-8-sig").splitlines()
    target_xml = E.parse(target_path).getroot()
    names = ["_CreatePackageLayout_CalculateInputsAndOutputs", "_CreatePackageLayout", "_CreatePackageLayout_AddFileWrites", "_CreateAppxBundlePlatformSpecificArtifacts", "PrepareMsixPackage", "_GenerateAppxPackage", "_GenerateAppxPackageRecipe", "_GenerateAppxPackageRecipeFile"]
    targets = []
    for n in target_xml:
        if local(n.tag) == "Target" and n.attrib.get("Name") in names:
            name = n.attrib["Name"]
            targets.append({"name": name, "attributes": n.attrib, "line": next(i for i, line in enumerate(lines, 1) if f'<Target Name="{name}"' in line), "direct_task_names": [local(x.tag) for x in n if local(x.tag) not in ["ItemGroup", "PropertyGroup"]]})
    query_tokens = ["<LayoutDir ", "<AppxLayoutDir ", "<_GenerateAppxPackageBaseDependsOn>", "<_CreateAppxBundlePlatformSpecificArtifactsDependsOn>", "_CreatePackageLayout;", "$(AppxLayoutDir)", "UseHardlinksIfPossible=", "OptionalProjectRecipeFiles=", "F5 deploy only creates", "GenerateAppxPackageOnBuild=true", "EnableIncludePublishItemsOutputGroup"]
    sdk_refs = {**file_info(target_path), "line_references": {token: [i for i, line in enumerate(lines, 1) if token in line] for token in query_tokens}, "targets": targets}
    files = [source_ref("src/Files.App/Files.App.csproj", ["BuildFilesAppServer", "Files.App.Server.winmd", "ProjectReference"]), source_ref("src/Files.App.Server/Files.App.Server.csproj", ["CsWinRTComponent", "ProjectReference", "Appx", "Deploy"]), source_ref("Files.slnx", ["BuildDependency", "Files.App.Server", "<Deploy"]), source_ref(".github/README.md", ["Building from source", "building-from-source"]), source_ref(".github/CONTRIBUTING.md", ["Files.Package", "debug version"]), source_ref("AGENTS.md", ["msbuild -restore Files.slnx"])]
    after = source_state()
    assert before == after
    result = {"status": "PASS (read-only evidence collection only)", "utc": datetime.now(timezone.utc).isoformat(), "source_pin": PIN, "source_pre": before, "source_post": after, "manifest": {**file_info(manifest_path), "identity": identity.attrib, "applications": applications, "dependencies": dependencies, "capabilities": capabilities, "extensions": extensions, "declared_path_checks": path_checks}, "recipe": {**file_info(recipe_path), "properties": properties, "mapped_file_count": len(mapped), "missing_mapping_source_targets": [r["target"] for r in mapped if not r["source_exists"]], "server_subtree_mappings": [r["target"] for r in mapped if r["target"].replace("\\", "/").startswith("Files.App.Server/")], "cross_recipe_references": references, "item_names": sorted({local(n.tag) for n in recipe.iter()}), "declared_AppX_layout_exists": (OUT / "AppX").exists()}, "server": {"compiled_output": file_info(server_dir / "Files.App.Server.exe"), "appxrecipes_in_server_output": [f.name for f in server_dir.glob("*.appxrecipe")]}, "active_nuget_import_receipts": imports, "source_references": files, "sdk_references": sdk_refs, "sdk_supporting_files": [file_info(sdk / f) for f in ["Microsoft.Windows.SDK.BuildTools.MSIX.Common.targets", "Microsoft.Windows.SDK.BuildTools.MSIX.targets"]], "interpretation": {"startup": "NOT READY/NOT TESTED: existing appbin is not a verified deployment layout", "confirmed": "Server compiled but not in current recipe subtree; recipe AppX LayoutDir differs from SDK intermediate AppxLayoutDir", "unknown": "Normal VS deploy automatic server merge is not proven or disproven; no safe standalone complete layout-only target was established", "not_claimed": "No upstream bug judgment or complete F9a/startup acceptance", "operations_not_performed": ["MSBuild/target invocation", "layout copying/removal", "package registration", "activation", "new installation", "source changes"]}}
    (dest / "observations.original.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(json.dumps({"status": result["status"], "source_clean": True, "mapped_files": len(mapped), "output": str(dest)}, indent=2))

if __name__ == "__main__":
    main()
