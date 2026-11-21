"""GateA isolated preparation only; never executes VS or deployment."""
from pathlib import Path
from datetime import datetime, timezone
import argparse
import hashlib
import json
import locale
import os
import re
import subprocess
import uuid

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
PIN = "cbfb4ba54a89632fbceb82ea94ada60a9bae3737"

def utc():
    return datetime.now(timezone.utc).isoformat()

def sha(data):
    return hashlib.sha256(data).hexdigest()

def sanitize(text):
    for path, token in [(str(ROOT), "%WORKSPACE%"), (os.environ["USERPROFILE"], "%USERPROFILE%"), (os.environ["ProgramFiles(x86)"], "%ProgramFiles(x86)%"), (os.environ["ProgramFiles"], "%ProgramFiles%")]:
        for value in [path.replace("\\", "\\\\"), path, path.replace("\\", "/")]:
            text = text.replace(value, token)
    return re.sub(r"(?<![A-Za-z0-9])([A-Za-z]):[\\/]+", lambda m: "%DRIVE_" + m[1].upper() + "%/", text).replace("\r\n", "\n")

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--route", choices=["gateA"], required=True)
    args = ap.parse_args()
    attempt = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8] + "-" + args.route
    out = ROOT / "m0/out" / ("f9a-vsdeploy-preparation-" + attempt)
    source = out / "source"
    raw = HERE / "raw" / attempt
    out.mkdir(parents=True)
    raw.mkdir(parents=True)
    original = Path(os.environ["USERPROFILE"]) / "pilot/Files"
    env = os.environ.copy()
    env["DOTNET_ROOT"] = str(Path(os.environ["USERPROFILE"]) / ".dotnet")
    env["PATH"] = env["DOTNET_ROOT"] + os.pathsep + env["PATH"]
    restored_amd64 = not env.get("PROCESSOR_ARCHITECTURE")
    if restored_amd64:
        assert env.get("PROCESSOR_ARCHITEW6432", "AMD64") == "AMD64"
        env["PROCESSOR_ARCHITECTURE"] = "AMD64"
    prohibited = ["GenerateAppxPackageOnBuild", "PublishAppxPackage", "GenerateTemporaryStoreCertificate", "PackageCertificateKeyFile", "PackageCertificateThumbprint", "MSBuildSDKsPath", "CscToolPath", "SENTRY_AUTH_TOKEN"]
    assert not any(value for key, value in env.items() if key.upper() in {name.upper() for name in prohibited}), "Unexpected inherited build/certificate/upload override present; value not logged"
    result = {"status": "not accepted", "attempt": attempt, "route": args.route, "source_pin": PIN, "start_utc": utc(), "phase": "GateA preparation only; GateB not authorized", "build_mode": "NON-ORIGINAL: only RestorePackagesConfig=true authorized", "child_environment": {"DOTNET_ROOT": "%USERPROFILE%/.dotnet", "PATH_prefix": "%USERPROFILE%/.dotnet", "PROCESSOR_ARCHITECTURE": env["PROCESSOR_ARCHITECTURE"], "AMD64_restored_only_if_missing": restored_amd64, "persistent_changes": False}, "commands": []}
    def save():
        (raw / "result.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8", newline="\n")
    def git(cwd, *a):
        return subprocess.check_output(["git", "-C", str(cwd), *a], env=env)
    def run(label, argv, cwd):
        log = out / (label + ".original.log")
        started = utc()
        with log.open("wb") as stream:
            proc = subprocess.Popen([str(x) for x in argv], cwd=cwd, env=env, stdout=stream, stderr=subprocess.STDOUT)
            row = {"label": label, "argv": [sanitize(str(x)) for x in argv], "cwd": sanitize(str(cwd)), "pid": proc.pid, "start_utc": started, "exit_code": None}
            result["commands"].append(row)
            save()
            print(json.dumps({"attempt": attempt, "route": args.route, "step": label, "pid": proc.pid, "utc": started}), flush=True)
            code = proc.wait()
        data = log.read_bytes()
        encoding = "utf-16" if data.startswith((b"\xff\xfe", b"\xfe\xff")) else "utf-8-sig"
        try:
            text = data.decode(encoding)
        except UnicodeDecodeError:
            encoding = locale.getpreferredencoding(False)
            text = data.decode(encoding, errors="replace")
        text = sanitize(text)
        complete = len(text.encode("utf-8")) < 4_500_000
        if not complete:
            text = text[:150_000] + "\n[BOUNDED HEAD/TAIL EXCERPT; full original retained at indexed path]\n" + text[-150_000:]
        published = raw / (label + ".log")
        published.write_text(text, encoding="utf-8", newline="\n")
        row.update({"exit_code": code, "end_utc": utc(), "original_sha256": sha(data), "original_bytes": len(data), "original_path": sanitize(str(log)), "decode_encoding": encoding, "published_complete_sanitized": complete, "published_sha256": sha(published.read_bytes())})
        save()
        print(json.dumps({"attempt": attempt, "step": label, "exit_code": code, "utc": row["end_utc"]}), flush=True)
        return code
    def snapshot(label):
        files = git(source, "ls-files").decode().splitlines()
        selected = [f for f in files if f in ["global.json", "Files.slnx", "Directory.Build.props", "Directory.Packages.props", ".github/workflows/ci.yml", "src/Files.App/Files.App.csproj", "src/Files.App/Package.appxmanifest"] or Path(f).name.lower() in ["nuget.config", "packages.config"] or f.startswith("src/Files.App/Properties/PublishProfiles/")]
        rows = []
        for path in selected:
            data = (source / path).read_bytes()
            rows.append({"path": path, "git_blob": git(source, "hash-object", "--path=" + path, path).decode().strip(), "sha256_working_bytes": sha(data)})
        info = {"head": git(source, "rev-parse", "HEAD").decode().strip(), "tree": git(source, "rev-parse", "HEAD^{tree}").decode().strip(), "status_porcelain": git(source, "status", "--porcelain").decode(), "tracked_diff": git(source, "diff", "HEAD", "--").decode(), "key_files": rows}
        (raw / (label + ".json")).write_text(json.dumps(info, indent=2) + "\n", encoding="utf-8", newline="\n")
        assert info["head"] == PIN and not info["tracked_diff"] and not info["status_porcelain"], "Source clone changed"
        return info
    try:
        assert git(original, "rev-parse", "HEAD").decode().strip() == PIN
        assert not git(original, "status", "--porcelain"), "Original Files worktree unexpectedly dirty"
        assert run("clone", ["git", "clone", "--no-hardlinks", "--no-checkout", str(original), str(source)], ROOT) == 0
        assert run("checkout", ["git", "checkout", "--detach", PIN], source) == 0
        pre = snapshot("source_pre")
        assert run("dotnet_version", [Path(env["DOTNET_ROOT"]) / "dotnet.exe", "--version"], source) == 0
        assert (raw / "dotnet_version.log").read_text(encoding="utf-8").strip() == "10.0.404"
        versions = {"sdk": "10.0.404", "roslyn_path": "%USERPROFILE%/.dotnet/sdk/10.0.404/Roslyn/bincore/Microsoft.CodeAnalysis.dll", "msbuild_path": "%ProgramFiles%/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"}
        (raw / "toolchain.json").write_text(json.dumps(versions, indent=2) + "\n", encoding="utf-8", newline="\n")
        msbuild = Path(os.environ["ProgramFiles"]) / "Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"
        code = run("build", [msbuild, "-restore", "Files.slnx", "-p:Configuration=Debug", "-p:Platform=x64", "-v:quiet", "-clp:ErrorsOnly", "-p:RestorePackagesConfig=true"], source)
        post = snapshot("source_post")
        assert pre == post
        result["source_unchanged"] = True
        result["status"] = "PASS (NON-ORIGINAL build only)" if code == 0 else "FAIL (NON-ORIGINAL build; no further repair attempted)"
        result["build_exit_code"] = code
        result["source_tree"] = pre["tree"]
    except Exception as ex:
        result["status"] = "STOP (precondition/provenance error)"
        result["error"] = sanitize(str(ex))
        raise
    finally:
        result["end_utc"] = utc()
        save()
        print(json.dumps({"attempt": attempt, "status": result["status"], "result": sanitize(str(raw / "result.json"))}), flush=True)

if __name__ == "__main__":
    main()
