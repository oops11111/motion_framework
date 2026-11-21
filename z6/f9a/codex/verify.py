"""F9a stage-scoped historical/environment evidence integrity, not CI installation."""
from pathlib import Path
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "122f7b4edc7698e742658532e3e1a83010f39f17"
SOURCE = "030216fad26d2a2e0d3d3d9ef7edf057d66b07bc"
OLD = "7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a"
PREFIX = "z6/f9a/codex/"

def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])

def sha(data):
    return hashlib.sha256(data).hexdigest()

def clean(path):
    data = (ROOT / path).read_bytes()
    return data if "/source_ledgers/" in path else data.replace(b"\r\n", b"\n")

def main():
    receipts = json.loads((HERE / "source_receipts.json").read_text(encoding="utf-8"))
    assert receipts["base"] == BASE and receipts["source"] == SOURCE
    assert git("merge-base", "HEAD", SOURCE).decode().strip() == SOURCE
    expected = []
    for path in git("diff", "--name-only", OLD, SOURCE).decode().splitlines():
        if path == "z6/LEDGER_snapshot.md":
            continue
        blob = git("rev-parse", f"{SOURCE}:{path}").decode().strip()
        expected.append({"path": path, "blob": blob, "sha256": sha(git("show", f"{SOURCE}:{path}"))})
        assert git("hash-object", "--path=" + path, path).decode().strip() == blob, path
    assert len(expected) == 78 and receipts["files"] == expected
    ledgers = []
    for label, tip in [("base-main", BASE), ("F9a", SOURCE)]:
        data = git("show", f"{tip}:z6/LEDGER_snapshot.md")
        path = f"{PREFIX}source_ledgers/{label}-{tip}.md"
        assert (ROOT / path).read_bytes() == data
        ledgers.append({"source": tip, "path": path, "blob": git("rev-parse", f"{tip}:z6/LEDGER_snapshot.md").decode().strip(), "sha256": sha(data)})
    assert receipts["ledgers"] == ledgers
    assert clean("z6/LEDGER_snapshot.md").startswith(git("show", f"{BASE}:z6/LEDGER_snapshot.md"))
    allowed = {r["path"] for r in expected} | {"z6/LEDGER_snapshot.md", ".github/workflows/z6-f9a.yml"}
    changed = set(git("diff", "--name-only", BASE, "HEAD").decode().splitlines()) | set(git("diff", "--name-only", "HEAD").decode().splitlines()) | set(git("ls-files", "--others", "--exclude-standard").decode().splitlines())
    assert all(path in allowed or path.startswith(PREFIX) for path in changed), sorted(changed - allowed - {p for p in changed if p.startswith(PREFIX)})
    assert not any(p.endswith("_LOCKED.md") or p == "docs/z6_closed.md" for p in changed)
    raw = HERE / "raw/install-20261121T083326Z"
    result = json.loads((raw / "result.json").read_text(encoding="utf-8"))
    post = json.loads((raw / "postflight.json").read_text(encoding="utf-8"))
    assert result["status"] == "PASS (local workload installation only)" and result["final_exit_code"] == 0
    assert post["instance_complete"] and not post["instance_reboot_required"] and not post["remaining_installer_processes"]
    matches = post["required_workload_component_matches"]
    assert len(matches) == 1 and matches[0]["instanceId"] == "bf096482"
    assert matches[0]["isComplete"] and not matches[0]["isRebootRequired"]
    assert post["v145_props"]["exists"] and post["vcvars64_exists"] and len(post["msvc_tools"]) == 3
    assert post["user_sdk404"]["exists"] and post["user_sdk404"]["equals_during_installation"]
    index = json.loads((raw / "log-index.json").read_text(encoding="utf-8"))
    for row in index:
        if "published_sha256" in row:
            assert sha((raw / row["name"]).read_bytes()) == row["published_sha256"]
    manifest = HERE / "SHA256SUMS"
    entries = [line.split("  ", 1) for line in manifest.read_text(encoding="ascii").splitlines()]
    files = {f.relative_to(ROOT).as_posix() for f in HERE.rglob("*") if f.is_file() and f != manifest}
    assert {p for _, p in entries} == files and len(entries) == len(files)
    for digest, path in entries:
        assert sha(clean(path)) == digest, path
        assert (ROOT / path).stat().st_size < 5_000_000, path
    print(json.dumps({"status": "PASS", "original_files": len(expected), "ledger_archives": len(ledgers), "base_prefix": "unchanged", "manifest_files": len(entries), "production_test_changes": 0, "scope": "Repository provenance only; local installer result is documented evidence, not reproduced by CI"}, indent=2))

if __name__ == "__main__":
    main()
