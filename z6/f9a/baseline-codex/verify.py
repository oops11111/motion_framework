"""Stage-scoped original Files command/evidence integrity, not cloud Files execution."""
from pathlib import Path
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "3078ae0e9f8792a03f45e09dd4fce5c6fda1ca7e"
PIN = "cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
PREFIX = "z6/f9a/baseline-codex/"

def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])

def digest(data):
    return hashlib.sha256(data).hexdigest()

def clean(path):
    data = (ROOT / path).read_bytes()
    return data if "/source_ledgers/" in path else data.replace(b"\r\n", b"\n")

def main():
    receipt = json.loads((HERE / "base_receipt.json").read_text(encoding="utf-8"))
    original = git("show", f"{BASE}:z6/LEDGER_snapshot.md")
    assert receipt["base"] == BASE and receipt["source_pin"] == PIN
    assert (ROOT / receipt["ledger_archive"]).read_bytes() == original
    assert receipt["ledger_sha256"] == digest(original)
    assert receipt["ledger_blob"] == git("rev-parse", f"{BASE}:z6/LEDGER_snapshot.md").decode().strip()
    assert clean("z6/LEDGER_snapshot.md").startswith(original)
    changes = set(git("diff", "--name-only", BASE, "HEAD").decode().splitlines()) | set(git("diff", "--name-only", "HEAD").decode().splitlines()) | set(git("ls-files", "--others", "--exclude-standard").decode().splitlines())
    allowed = {"z6/LEDGER_snapshot.md", ".github/workflows/z6-f9a-baseline.yml"}
    assert all(p in allowed or p.startswith(PREFIX) for p in changes), sorted(changes - allowed - {p for p in changes if p.startswith(PREFIX)})
    assert not any(p.endswith("_LOCKED.md") or p == "docs/z6_closed.md" for p in changes)
    runs = []
    for file in sorted((HERE / "raw").glob("*/result.json")):
        result = json.loads(file.read_text(encoding="utf-8"))
        assert result["source_pin"] == PIN and result["route"] in ("run1", "run2")
        env = result["child_environment"]
        assert env["DOTNET_ROOT"] == env["PATH_prefix"] == "%USERPROFILE%/.dotnet" and env["persistent_changes"] is False
        assert env["PROCESSOR_ARCHITECTURE"] == "AMD64"
        commands = result["commands"]
        for row in commands:
            if "published_sha256" in row:
                assert digest(clean((file.parent / (row["label"] + ".log")).relative_to(ROOT).as_posix())) == row["published_sha256"]
            argv = row["argv"]
            if row["label"] == "restore":
                assert argv[0].replace("\\", "/") == "%ProgramFiles%/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"
                assert argv[1:] == ["Files.slnx", "-t:Restore", "-p:Platform=x64", "-p:Configuration=Debug", "-p:PublishReadyToRun=true", "-v:quiet"]
            if row["label"] == "build":
                if result["route"] == "run1":
                    assert argv[0].replace("\\", "/") == "%ProgramFiles%/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"
                    assert argv[1:] == ["src/Files.App/Files.App.csproj", "-t:Build", "-p:Configuration=Debug", "-p:Platform=x64", "-p:AppxBundle=Never", "-p:GenerateAppxPackageOnBuild=false", "-v:quiet"]
                    assert next(r for r in commands if r["label"] == "restore")["exit_code"] == 0
                else:
                    assert argv[0].replace("\\", "/") == "%USERPROFILE%/.dotnet/dotnet.exe"
                    assert argv[1:] == ["build", "src/Files.App/Files.App.csproj", "-c", "Debug", "-p:Platform=x64"]
        build = [r for r in commands if r["label"] == "build"]
        if build:
            pre = json.loads(file.with_name("source_pre.json").read_text(encoding="utf-8"))
            post = json.loads(file.with_name("source_post.json").read_text(encoding="utf-8"))
            assert pre == post and pre["head"] == PIN and not pre["tracked_diff"] and not pre["status_porcelain"]
            assert result["source_unchanged"] and result["build_exit_code"] == build[0]["exit_code"]
            assert (result["status"] == "PASS (original build only)") == (build[0]["exit_code"] == 0)
            artifacts = json.loads(file.with_name("artifacts.json").read_text(encoding="utf-8"))
            assert artifacts["source_pin"] == PIN
            assert all(not a["executed"] and not a["registered"] and len(a["sha256"]) == 64 for a in artifacts["artifacts"])
        else:
            assert result["status"] == "STOP (precondition/provenance error)"
        runs.append({"attempt": result["attempt"], "route": result["route"], "status": result["status"]})
    assert runs
    if any(r["route"] == "run1" and r["status"] == "PASS (original build only)" for r in runs):
        assert not any(r["route"] == "run2" for r in runs), "Run2 unnecessary after run1 PASS"
    manifest = HERE / "SHA256SUMS"
    entries = [line.split("  ", 1) for line in manifest.read_text(encoding="ascii").splitlines()]
    files = {p.relative_to(ROOT).as_posix() for p in HERE.rglob("*") if p.is_file() and p != manifest}
    assert {p for _, p in entries} == files and len(entries) == len(files)
    for sha, path in entries:
        assert digest(clean(path)) == sha, path
        assert (ROOT / path).stat().st_size < 5_000_000
    print(json.dumps({"status": "PASS", "files_pin": PIN, "runs": runs, "manifest_files": len(entries), "base_ledger_prefix": "unchanged", "production_test_prior_stage_changes": 0, "scope": "Command/source/evidence integrity; actual Files build ran locally, not reproduced by this CI policy"}, indent=2))

if __name__ == "__main__":
    main()
