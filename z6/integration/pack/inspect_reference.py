"""Read-only F7 reference hash verification and reflection-only inspection."""
from pathlib import Path
from datetime import datetime, timezone
import hashlib
import json
import os
import re
import shutil
import subprocess
import uuid

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    attempt = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    out = ROOT / "m0/out" / ("z6-pack-reference-" + attempt)
    raw = HERE / "raw" / attempt
    out.mkdir(parents=True)
    raw.mkdir(parents=True)
    cache = Path(os.environ["USERPROFILE"]) / "mf_z6/f7/pilot/l1pack/bin/x64/Release/net8.0"
    result = {"status": "FAIL", "attempt": attempt, "cache": "%USERPROFILE%/mf_z6/f7/pilot/l1pack/bin/x64/Release/net8.0", "scope": "Existing F7 reference only; not a current-main pack rebuild or fresh F6/F11 host test", "files": [], "commands": []}
    def sanitized(text):
        for path, replacement in [(str(ROOT), "%WORKSPACE%"), (str(Path(os.environ["USERPROFILE"])), "%USERPROFILE%")]:
            text = text.replace(path, replacement).replace(path.replace("\\", "/"), replacement)
        return text
    env = os.environ.copy()
    if not env.get("PROCESSOR_ARCHITECTURE"):
        assert env.get("PROCESSOR_ARCHITEW6432", "AMD64") == "AMD64"
        env["PROCESSOR_ARCHITECTURE"] = "AMD64"
        result["child_environment"] = "Standard PROCESSOR_ARCHITECTURE=AMD64 restored only in child; no persistent changes"
    def run(label, args):
        p = subprocess.run(args, cwd=ROOT, env=env, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
        text = sanitized(p.stdout.decode("utf-8", errors="replace"))
        (raw / (label + ".log")).write_text(text, encoding="utf-8", newline="\n")
        result["commands"].append({"label": label, "argv": [sanitized(str(x)) for x in args], "exit_code": p.returncode})
        assert p.returncode == 0, label + " failed"
    try:
        sums = (ROOT / "pilot/l1pack/SHA256SUMS").read_text(encoding="utf-8")
        expected = dict((name, digest) for digest, name in re.findall(r"^([0-9a-f]{64})  (.+)$", sums, re.M))
        assert set(expected) == {"FluidMotion.L1.dll", "fluidmotion.dll", "demo_motion.json"}
        copied = out / "reference"
        copied.mkdir()
        for name, digest in expected.items():
            source = cache / name
            assert source.is_file(), "Missing reference " + name
            assert sha(source) == digest, "Reference checksum mismatch " + name
            dest = copied / name
            shutil.copyfile(source, dest)
            assert sha(dest) == digest, "Copy checksum mismatch " + name
            result["files"].append({"name": name, "reference_sha256": digest, "copied_sha256": sha(dest)})
        tool = out / "surface_tool"
        tool.mkdir()
        source = ROOT / "z5/t3_results_dev/K93N3P8"
        for src, dst in [("SurfaceDump_Program.cs", "Program.cs"), ("SurfaceDump.csproj.txt", "SurfaceDump.csproj")]:
            shutil.copyfile(source / src, tool / dst)
            assert sha(source / src) == sha(tool / dst)
        run("surface_tool_build", ["dotnet", "build", str(tool / "SurfaceDump.csproj"), "-c", "Release"])
        dump = out / "surface_reference.txt"
        run("surface_check", ["dotnet", str(tool / "bin/Release/net10.0/SurfaceDump.dll"), str(copied / "FluidMotion.L1.dll"), str(dump)])
        check = dump.with_suffix(".checksurface.txt")
        shutil.copyfile(dump, raw / dump.name)
        shutil.copyfile(check, raw / check.name)
        assert "CHECKSURFACE fails=0" in check.read_text(encoding="utf-8")
        baseline = subprocess.check_output(["git", "show", "HEAD:pilot/fluenthub/surface_l1pack.txt"], cwd=ROOT)
        assert dump.read_bytes() == baseline, "Full public surface differs from canonical baseline bytes"
        result["surface_sha256"] = sha(dump)
        result["baseline_sha256"] = hashlib.sha256(baseline).hexdigest()
        result["surface_compare"] = "Full canonical Git baseline bytes equal; no first-line exemption"
        result["status"] = "PASS (reference hash/API inspection only)"
    except Exception as exc:
        result["error"] = sanitized(str(exc))
        raise
    finally:
        (raw / "result.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8", newline="\n")
        print(json.dumps(result, indent=2))

if __name__ == "__main__":
    main()
