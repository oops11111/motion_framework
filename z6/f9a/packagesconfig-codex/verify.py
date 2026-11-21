"""Stage-scoped one-property NON-ORIGINAL restore/build evidence integrity."""
from pathlib import Path
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "81543c86fc67801aa2de00f44d8b267c2ac09ee9"
PIN = "cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
PREFIX = "z6/f9a/packagesconfig-codex/"

def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])

def digest(data):
    return hashlib.sha256(data).hexdigest()

def clean(path):
    data = (ROOT / path).read_bytes()
    return data if "/source_ledgers/" in path else data.replace(b"\r\n", b"\n")

def main():
    auth = json.loads((HERE / "AUTHORIZATION.json").read_text(encoding="utf-8"))
    assert auth["user_verbatim"] == "批准 F9a 仅本票增加 -p:RestorePackagesConfig=true，按原版本还原并构建，标记“非原样”。"
    assert auth["recording_not_send_time"] is True
    receipt = json.loads((HERE / "base_receipt.json").read_text(encoding="utf-8"))
    original = git("show", f"{BASE}:z6/LEDGER_snapshot.md")
    assert receipt["base"] == BASE and receipt["source_pin"] == PIN
    assert (ROOT / receipt["ledger_archive"]).read_bytes() == original
    assert receipt["ledger_sha256"] == digest(original)
    assert receipt["ledger_blob"] == git("rev-parse", f"{BASE}:z6/LEDGER_snapshot.md").decode().strip()
    assert clean("z6/LEDGER_snapshot.md").startswith(original)
    changes = set(git("diff", "--name-only", BASE, "HEAD").decode().splitlines()) | set(git("diff", "--name-only", "HEAD").decode().splitlines()) | set(git("ls-files", "--others", "--exclude-standard").decode().splitlines())
    allowed = {"z6/LEDGER_snapshot.md", ".github/workflows/z6-f9a-packagesconfig.yml"}
    assert all(p in allowed or p.startswith(PREFIX) for p in changes), sorted(changes - allowed - {p for p in changes if p.startswith(PREFIX)})
    assert not any(p.endswith("_LOCKED.md") or p == "docs/z6_closed.md" for p in changes)
    runs = []
    for file in sorted((HERE / "raw").glob("*/result.json")):
        result = json.loads(file.read_text(encoding="utf-8"))
        assert result["source_pin"] == PIN and result["route"] == "packagesconfig"
        env = result["child_environment"]
        assert env["DOTNET_ROOT"] == env["PATH_prefix"] == "%USERPROFILE%/.dotnet" and env["persistent_changes"] is False
        assert env["PROCESSOR_ARCHITECTURE"] == "AMD64"
        commands = result["commands"]
        assert [r["label"] for r in commands] == ["clone", "checkout", "dotnet_version", "build"]
        for row in commands:
            assert digest(clean((file.parent / (row["label"] + ".log")).relative_to(ROOT).as_posix())) == row["published_sha256"]
        build = commands[-1]
        assert build["argv"][0].replace("\\", "/") == "%ProgramFiles%/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"
        assert build["argv"][1:] == ["-restore", "Files.slnx", "-p:Configuration=Debug", "-p:Platform=x64", "-v:quiet", "-clp:ErrorsOnly", "-p:RestorePackagesConfig=true"]
        assert build["exit_code"] == result["build_exit_code"] == 0 and result["status"] == "PASS (NON-ORIGINAL build only)"
        pre = json.loads(file.with_name("source_pre.json").read_text(encoding="utf-8"))
        post = json.loads(file.with_name("source_post.json").read_text(encoding="utf-8"))
        assert pre == post and pre["head"] == PIN and not pre["tracked_diff"] and not pre["status_porcelain"] and result["source_unchanged"]
        packages = json.loads(file.with_name("restored_package_receipts.json").read_text(encoding="utf-8"))
        assert packages["source_pin"] == PIN and packages["all_declared_versions_match"]
        declared = json.loads((HERE / "declared_packages.json").read_text(encoding="utf-8"))
        expected = {(p["id"], p["version"]) for cfg in declared for p in cfg["packages"]}
        assert {(p["declared_id"], p["declared_version"]) for p in packages["packages"]} == expected
        assert {(p["actual_nuspec_id"], p["actual_nuspec_version"]) for p in packages["packages"]} == expected
        assert all(p["packages_config_sha256_unchanged"] and p["packages_config_git_blob"] == declared[0]["git_blob"] for p in packages["packages"])
        assert len(packages["actual_import_files"]) == 3 and all(len(r["sha256"]) == 64 for r in packages["actual_import_files"])
        observations = json.loads(file.with_name("artifact_layout_receipts.json").read_text(encoding="utf-8"))
        assert observations["source_pin"] == PIN and observations["build_mode"] == "NON-ORIGINAL"
        assert all(not r["app_or_test_executed"] and not r["registered"] for r in observations["observed_outputs"])
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
