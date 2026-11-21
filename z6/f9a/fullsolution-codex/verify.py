"""Stage-scoped original full-solution failure evidence, not a dependency repair."""
from pathlib import Path
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "8e68cf81ef10bd0623651949e6a98ddef4f175ca"
PIN = "cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
PREFIX = "z6/f9a/fullsolution-codex/"

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
    allowed = {"z6/LEDGER_snapshot.md", ".github/workflows/z6-f9a-fullsolution.yml"}
    assert all(p in allowed or p.startswith(PREFIX) for p in changes), sorted(changes - allowed - {p for p in changes if p.startswith(PREFIX)})
    assert not any(p.endswith("_LOCKED.md") or p == "docs/z6_closed.md" for p in changes)
    runs = []
    for file in sorted((HERE / "raw").glob("*/result.json")):
        result = json.loads(file.read_text(encoding="utf-8"))
        assert result["source_pin"] == PIN and result["route"] == "fullsolution"
        env = result["child_environment"]
        assert env["DOTNET_ROOT"] == env["PATH_prefix"] == "%USERPROFILE%/.dotnet" and env["persistent_changes"] is False
        assert env["PROCESSOR_ARCHITECTURE"] == "AMD64"
        commands = result["commands"]
        assert [r["label"] for r in commands] == ["clone", "checkout", "dotnet_version", "build"]
        for row in commands:
            assert digest(clean((file.parent / (row["label"] + ".log")).relative_to(ROOT).as_posix())) == row["published_sha256"]
        build = commands[-1]
        assert build["argv"][0].replace("\\", "/") == "%ProgramFiles%/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"
        assert build["argv"][1:] == ["-restore", "Files.slnx", "-p:Configuration=Debug", "-p:Platform=x64", "-v:quiet", "-clp:ErrorsOnly"]
        assert build["exit_code"] == result["build_exit_code"] == 1 and result["status"] == "FAIL (original build; no repair attempted)"
        pre = json.loads(file.with_name("source_pre.json").read_text(encoding="utf-8"))
        post = json.loads(file.with_name("source_post.json").read_text(encoding="utf-8"))
        assert pre == post and pre["head"] == PIN and not pre["tracked_diff"] and not pre["status_porcelain"] and result["source_unchanged"]
        stop = json.loads(file.with_name("package_stop.json").read_text(encoding="utf-8"))
        assert stop["source_pin"] == PIN and not stop["missing_path_exists"] and stop["no_restore_or_source_repair"]
        assert stop["missing_path_from_source"] == "packages/Microsoft.Windows.ImplementationLibrary.1.0.260126.7/build/native/Microsoft.Windows.ImplementationLibrary.targets"
        observations = json.loads(file.with_name("artifact_layout_observations.json").read_text(encoding="utf-8"))
        assert observations["source_pin"] == PIN
        runs.append({"attempt": result["attempt"], "route": result["route"], "status": result["status"]})
    assert runs
    assert len(runs) == 1
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
