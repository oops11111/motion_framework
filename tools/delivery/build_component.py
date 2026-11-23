"""Build an internal candidate once, then independently consume its exact local package.

No publish, install, global tool, host GUI, or historical cache input. All outputs
are unique and preserved on failure. The JSON result is the cross-repo interface.
"""
from pathlib import Path
from datetime import datetime, timezone
import argparse
import hashlib
import json
import os
import re
import shutil
import struct
import subprocess
import uuid
import zipfile
from xml.sax.saxutils import escape

from validate_native import inspect_native

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / "src/FluidMotion.L1/FluidMotion.L1.csproj"
VERSION = "0.1.0"
# BASE is the frozen-source baseline: the protected files below must be
# byte-identical to this commit. Rotate BASE only to a reviewed head, in its
# own change, after reading the full protected diff against the old BASE and
# confirming every difference is intended. Never rotate BASE to bless an
# unreviewed change, and never make it configurable at build time (an
# environment override would silently disable the freeze check).
BASE = "11b3378fb20498675165f0b52275aa8d23ec2ccd"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def save(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8", newline="\n")


def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args]).decode().strip()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, help="New, non-existing output directory")
    args = parser.parse_args()
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    out = (args.output or ROOT / "m0/out" / ("component-" + stamp + "-" + uuid.uuid4().hex[:8])).resolve()
    if out.exists():
        raise RuntimeError("Output must be a new directory; no existing files are deleted")
    out.mkdir(parents=True)
    env = dict(os.environ)
    env.pop("FM_FLUIDMOTION_PATH", None)
    if not env.get("PROCESSOR_ARCHITECTURE"):
        env["PROCESSOR_ARCHITECTURE"] = "AMD64"
    env["DOTNET_CLI_UI_LANGUAGE"] = "en-US"
    result = {
        "status": "FAIL",
        "candidate": True,
        "sealed_release": False,
        "published": False,
        "version": VERSION,
        "rid": "win-x64",
        "source_commit": git("rev-parse", "HEAD"),
        "source_dirty": bool(git("status", "--porcelain")),
        "output": str(out),
        "commands": [],
    }
    print("OUTPUT=" + str(out), flush=True)

    def run(label, argv, expected=0, needle=None, cwd=ROOT, child_env=None):
        log = out / (label + ".log")
        with log.open("wb") as stream:
            process = subprocess.Popen([str(x) for x in argv], cwd=cwd, env=child_env or env,
                                       stdout=stream, stderr=subprocess.STDOUT)
            print(label + " PID=" + str(process.pid), flush=True)
            code = process.wait()
        result["commands"].append({"label": label, "argv": [str(x) for x in argv],
                                   "pid": process.pid, "exit": code, "log": log.name,
                                   "sha256": sha(log)})
        save(out / "result.json", result)
        if (expected == 0 and code != 0) or (expected != 0 and code == 0):
            raise RuntimeError(label + " unexpected exit " + str(code) + "; see " + log.name)
        if needle and needle not in log.read_text(encoding="utf-8", errors="replace"):
            raise RuntimeError(label + " did not report the required failure reason")
        return log

    try:
        protected = ["MotionSoftDemo/MotionSystem.cs", "MotionSoftDemo/MotionEtw.cs",
                     "code/capi/fm_capi.h", "code/capi/fm_capi.cpp", "code/composition.h"]
        if git("diff", BASE, "--", *protected):
            raise RuntimeError("Frozen source differs from accepted baseline")
        tracked = git("ls-files", "code", "MotionSoftDemo/MotionSystem.cs",
                      "MotionSoftDemo/MotionEtw.cs").splitlines()
        source_inputs = sorted(set(tracked + [str(p.relative_to(ROOT)).replace("\\", "/")
            for folder in ["src/FluidMotion.L1", "tools/delivery", "tests/delivery"]
            for p in (ROOT / folder).rglob("*")
            if p.is_file() and "__pycache__" not in p.parts and not any(x in p.parts for x in ["obj", "bin"])]))
        result["input_hashes"] = {n: sha(ROOT / n) for n in source_inputs}
        run("dotnet_info", ["dotnet", "--info"])
        vswhere = Path(os.environ["ProgramFiles(x86)"]) / "Microsoft Visual Studio/Installer/vswhere.exe"
        install = subprocess.check_output([str(vswhere), "-latest", "-products", "*", "-requires",
            "Microsoft.VisualStudio.Component.VC.Tools.x86.x64", "-property", "installationPath"], env=env).decode().strip()
        vcvars = Path(install) / "VC/Auxiliary/Build/vcvars64.bat"
        if not install or not vcvars.is_file():
            raise RuntimeError("Existing MSVC x64 tools required; no automatic installation")
        for value in [str(ROOT), str(out), str(vcvars)]:
            if any(c in value for c in '&|<>^%!\r\n'):
                raise RuntimeError("Unsupported cmd metacharacter in build path")
        native_dir = out / "native"
        native_dir.mkdir()
        native = native_dir / "fluidmotion.dll"
        compile_args = ["cl", "/nologo", "/std:c++17", "/EHsc", "/O2", "/DNDEBUG", "/MT", "/W3", "/LD", "/Bv",
            "/I", "code/capi", "/I", "code", "/I", "code/explore", "code/capi/fm_capi.cpp",
            "/Fo:" + str(native_dir) + os.sep, "/Fe:" + str(native)]
        batch = out / "native_build.cmd"
        batch.write_text('@echo off\ncall "' + str(vcvars) + '"\nif errorlevel 1 exit /b %errorlevel%\n'
            + subprocess.list2cmdline(compile_args) + '\nif errorlevel 1 exit /b %errorlevel%\n'
            + 'dumpbin /dependents "' + str(native) + '" > "' + str(out / "native_dependents.txt") + '"\n'
            + 'if errorlevel 1 exit /b %errorlevel%\n'
            + 'dumpbin /headers "' + str(native) + '" > "' + str(out / "native_headers.txt") + '"\n'
            + 'exit /b %errorlevel%\n', encoding="utf-8")
        run("native_build", ["cmd.exe", "/d", "/c", str(batch)])
        native_info = inspect_native(native, "win-x64")
        imports_text = (out / "native_dependents.txt").read_text(encoding="utf-8", errors="replace")
        dependencies = re.findall(r"^\s+([A-Za-z0-9_.-]+\.dll)\s*$", imports_text, re.M | re.I)
        if not dependencies or any(re.search(r"vcruntime|msvcp|ucrtbase|api-ms-win-crt", x, re.I) for x in dependencies):
            raise RuntimeError("Static CRT dependency check failed")
        allowed = {"kernel32.dll", "advapi32.dll", "ntdll.dll"}
        if any(x.lower() not in allowed for x in dependencies):
            raise RuntimeError("Unreviewed native dependency: " + str(dependencies))
        provenance = {
            "candidate": True, "sealed_release": False, "source_commit": result["source_commit"],
            "source_dirty": result["source_dirty"], "input_hashes": result["input_hashes"],
            "package_id": "FluidMotion.L1", "version": VERSION, "tfm": "net8.0", "rid": "win-x64",
            "native": native_info, "native_crt": "MSVC static /MT, Release /O2 /DNDEBUG",
            "native_dependencies": dependencies,
            "compiler_version_log_sha256": sha(out / "native_build.log"),
            "dotnet_info_sha256": sha(out / "dotnet_info.log"),
            "public_license_invented": False,
        }
        save(out / "provenance.json", provenance)
        feed = out / "feed"
        common = ["-p:FluidMotionNativePath=" + str(native),
                  "-p:FluidMotionProvenancePath=" + str(out / "provenance.json"),
                  "-p:SourceRevisionId=" + result["source_commit"]]
        run("pack", ["dotnet", "pack", str(PROJECT), "-c", "Release", "-o", str(feed), *common,
                     "-p:BaseIntermediateOutputPath=" + str(out / "obj") + os.sep,
                     "-p:OutputPath=" + str(out / "managed") + os.sep])
        package = feed / ("FluidMotion.L1." + VERSION + ".nupkg")
        required = ["lib/net8.0/FluidMotion.L1.dll", "runtimes/win-x64/native/fluidmotion.dll",
                    "contentFiles/any/net8.0/demo_motion.json", "README.md", "provenance.json"]
        with zipfile.ZipFile(package) as zipped:
            for name in required:
                if name not in zipped.namelist():
                    raise RuntimeError("Missing package member: " + name)
            if hashlib.sha256(zipped.read(required[1])).hexdigest() != native_info["sha256"]:
                raise RuntimeError("Packaged native changed")
            if zipped.read("provenance.json") != (out / "provenance.json").read_bytes():
                raise RuntimeError("Packaged provenance changed")
            result["package_members"] = [{"path": name, "bytes": len(zipped.read(name)),
                "sha256": hashlib.sha256(zipped.read(name)).hexdigest()} for name in zipped.namelist()]
        result["package"] = {"path": str(package), "sha256": sha(package), "candidate": True}
        # These are actual MSBuild project gates, not tests of the validator in isolation.
        wrong = out / "wrong-machine.dll"
        bad = bytearray(native.read_bytes())
        struct.pack_into("<H", bad, struct.unpack_from("<I", bad, 0x3c)[0] + 4, 0x14c)
        wrong.write_bytes(bad)
        for label, asset, rid, reason in [
            ("negative_missing_native", out / "missing.dll", "win-x64", "missing native assets are not optional"),
            ("negative_wrong_rid", native, "win-arm64", "Only win-x64 native assets are supported"),
            ("negative_wrong_machine", wrong, "win-x64", "Expected an AMD64 DLL")]:
            props = [x for x in common if not x.startswith("-p:FluidMotionNativePath=")]
            run(label, ["dotnet", "pack", str(PROJECT), "-c", "Release", "-o", str(out / label / "feed"),
                *props, "-p:FluidMotionNativePath=" + str(asset), "-p:ComponentRuntimeIdentifier=" + rid,
                "-p:BaseIntermediateOutputPath=" + str(out / label / "obj") + os.sep,
                "-p:OutputPath=" + str(out / label / "bin") + os.sep], expected=1, needle=reason)
        consumer = out / "independent-consumer"
        consumer.mkdir()
        for name in ["Consumer.csproj", "Program.cs"]:
            shutil.copyfile(ROOT / "tests/delivery/Consumer" / name, consumer / name)
        config = consumer / "NuGet.config"
        config.write_text('<?xml version="1.0" encoding="utf-8"?>\n<configuration><packageSources><clear/><add key="component" value="{feed}"/><add key="framework" value="https://api.nuget.org/v3/index.json"/></packageSources><packageSourceMapping><packageSource key="component"><package pattern="FluidMotion.L1"/></packageSource><packageSource key="framework"><package pattern="Microsoft.*"/></packageSource></packageSourceMapping></configuration>\n'.format(feed=escape(str(feed), {'"': '&quot;'})), encoding="utf-8")
        consumer_env = dict(env, NUGET_PACKAGES=str(out / "consumer-cache"))
        run("consumer_restore", ["dotnet", "restore", str(consumer / "Consumer.csproj"),
            "--configfile", str(config)], cwd=consumer, child_env=consumer_env)
        consumer_bin = out / "consumer-bin"
        run("consumer_build", ["dotnet", "build", str(consumer / "Consumer.csproj"), "-c", "Release",
            "--no-restore", "-o", str(consumer_bin)], cwd=consumer, child_env=consumer_env)
        package_members = {row["path"]: row["sha256"] for row in result["package_members"]}
        consumer_assets = []
        for member, filename in [("lib/net8.0/FluidMotion.L1.dll", "FluidMotion.L1.dll"),
                                 ("runtimes/win-x64/native/fluidmotion.dll", "fluidmotion.dll"),
                                 ("contentFiles/any/net8.0/demo_motion.json", "demo_motion.json")]:
            digest = sha(consumer_bin / filename)
            if digest != package_members[member]:
                raise RuntimeError("Consumer output differs from package member: " + filename)
            consumer_assets.append({"output": filename, "package_member": member, "sha256": digest})
        result["consumer_output_assets"] = consumer_assets
        run("consumer_run", ["dotnet", str(consumer_bin / "Consumer.dll")], cwd=consumer_bin,
            child_env=consumer_env)
        consumer_text = (out / "consumer_run.log").read_text(encoding="utf-8", errors="strict")
        summary = json.loads(consumer_text.splitlines()[-1])
        if summary["native_sha256"] != native_info["sha256"] or summary["checks"] < 14:
            raise RuntimeError("Consumer did not verify the expected actual native path/behavior")
        result["consumer"] = summary
        assets = json.loads((consumer / "obj/project.assets.json").read_text(encoding="utf-8-sig"))
        key = "FluidMotion.L1/" + VERSION
        if key not in assets["libraries"]:
            raise RuntimeError("Exact component version was not restored")
        restored = out / "consumer-cache/fluidmotion.l1" / VERSION / ("fluidmotion.l1." + VERSION + ".nupkg")
        if sha(restored) != sha(package):
            raise RuntimeError("Restored nupkg differs from the newly built local-feed package")
        surface_tool = out / "surface-tool"
        surface_tool.mkdir()
        shutil.copyfile(ROOT / "z5/t3_results_dev/K93N3P8/SurfaceDump_Program.cs", surface_tool / "Program.cs")
        # Same reflection/check code, net8 tool TFM so this entry only requires the consumer's SDK.
        template = (ROOT / "z5/t3_results_dev/K93N3P8/SurfaceDump.csproj.txt").read_text(encoding="utf-8")
        (surface_tool / "SurfaceDump.csproj").write_text(template.replace("net10.0", "net8.0"), encoding="utf-8")
        run("surface_tool_build", ["dotnet", "build", str(surface_tool / "SurfaceDump.csproj"), "-c", "Release"])
        dump = out / "surface_component.txt"
        run("surface_check", ["dotnet", str(surface_tool / "bin/Release/net8.0/SurfaceDump.dll"),
            str(consumer_bin / "FluidMotion.L1.dll"), str(dump)])
        baseline = subprocess.check_output(["git", "-C", str(ROOT), "show", "HEAD:pilot/fluenthub/surface_l1pack.txt"])
        dump_bytes = dump.read_bytes()
        if dump_bytes.replace(b"\r\n", b"\n") != baseline.replace(b"\r\n", b"\n"):
            raise RuntimeError("Complete normalized public surface differs from the frozen baseline")
        result["surface_comparison"] = {
            "raw_sha256": sha(dump),
            "baseline_raw_sha256": hashlib.sha256(baseline).hexdigest(),
            "normalized_sha256": hashlib.sha256(dump_bytes.replace(b"\r\n", b"\n")).hexdigest(),
            "baseline_normalized_sha256": hashlib.sha256(baseline.replace(b"\r\n", b"\n")).hexdigest(),
            "raw_equal": dump_bytes == baseline,
            "complete_LF_normalized_equal": True,
        }
        if "CHECKSURFACE fails=0" not in dump.with_suffix(".checksurface.txt").read_text(encoding="utf-8"):
            raise RuntimeError("CheckSurface failed")
        result["surface_sha256"] = sha(dump)
        result["native"] = provenance
        result["input_hashes_POST_equal"] = all(sha(ROOT / name) == digest
                                                 for name, digest in result["input_hashes"].items())
        result["source_commit_POST_equal"] = git("rev-parse", "HEAD") == result["source_commit"]
        if not result["input_hashes_POST_equal"] or not result["source_commit_POST_equal"]:
            raise RuntimeError("Build inputs changed during component verification")
        result["status"] = "PASS internal candidate, not sealed or published"
    except Exception as error:
        result["error"] = type(error).__name__ + ": " + str(error)
        raise
    finally:
        save(out / "result.json", result)
        print("RESULT_MANIFEST=" + str(out / "result.json"), flush=True)


if __name__ == "__main__":
    main()
