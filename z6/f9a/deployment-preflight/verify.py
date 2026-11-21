"""Branch-scoped stored preflight evidence integrity; no workstation target execution."""
from pathlib import Path
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "425165e112c271dc76706e5ff2d5354b1dfc9120"
PIN = "cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
PREFIX = "z6/f9a/deployment-preflight/"

def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])

def sha(data):
    return hashlib.sha256(data).hexdigest()

def clean(path):
    data = (ROOT / path).read_bytes()
    return data if "/source_ledgers/" in path else data.replace(b"\r\n", b"\n")

def main():
    base = json.loads((HERE / "base_receipt.json").read_text(encoding="utf-8"))
    ledger = git("show", f"{BASE}:z6/LEDGER_snapshot.md")
    assert base["base"] == BASE and sha(ledger) == base["ledger_sha256"]
    assert (ROOT / base["ledger_archive"]).read_bytes() == ledger
    assert base["ledger_blob"] == git("rev-parse", f"{BASE}:z6/LEDGER_snapshot.md").decode().strip()
    assert clean("z6/LEDGER_snapshot.md").startswith(ledger)
    allowed = {"z6/LEDGER_snapshot.md", ".github/workflows/z6-f9a-deployment-preflight.yml"}
    changes = set(git("diff", "--name-only", BASE, "HEAD").decode().splitlines()) | set(git("diff", "--name-only", "HEAD").decode().splitlines()) | set(git("ls-files", "--others", "--exclude-standard").decode().splitlines())
    assert all(p in allowed or p.startswith(PREFIX) for p in changes), sorted(changes - allowed - {p for p in changes if p.startswith(PREFIX)})
    raw = HERE / "raw/20261121-readonly"
    receipt = json.loads((raw / "receipt.json").read_text(encoding="utf-8"))
    obs = json.loads((raw / "observations.json").read_text(encoding="utf-8"))
    assert sha(clean((raw / "observations.json").relative_to(ROOT).as_posix())) == receipt["published_sha256"]
    assert obs["source_pin"] == PIN and obs["source_pre"] == obs["source_post"]
    assert obs["source_pre"]["head"] == PIN and not obs["source_pre"]["status_porcelain"] and not obs["source_pre"]["tracked_diff"]
    assert obs["manifest"]["sha256"] == "ad4ee744e9e3b82875b656f3da15754003d673987b978c7c411af4d7c7c890b4"
    assert obs["recipe"]["sha256"] == "12e12ed3d1f60a365e9abca14fa8f45293422ada0e05d324a170ec33b8691e5c"
    assert obs["recipe"]["mapped_file_count"] == 1300 and not obs["recipe"]["missing_mapping_source_targets"]
    assert obs["server"]["compiled_output"]["exists"] and not obs["recipe"]["server_subtree_mappings"]
    assert obs["interpretation"]["startup"].startswith("NOT READY/NOT TESTED")
    assert obs["sdk_references"]["sha256"] == "ad6a90582d0bf3f452bc057eecd7ddf7368b4337199dfce240e6382fc8992458"
    native = json.loads((raw / "manual_native_helper_references.json").read_text(encoding="utf-8"))
    assert native["source_pin"] == PIN and len(native["references"]) == 3
    assert all(r["line_references"]["PostBuildEvent"] and r["line_references"]["xcopy"] for r in native["references"])
    manifest = HERE / "SHA256SUMS"
    rows = [line.split("  ", 1) for line in manifest.read_text(encoding="ascii").splitlines()]
    files = {f.relative_to(ROOT).as_posix() for f in HERE.rglob("*") if f.is_file() and f != manifest}
    assert len(rows) == len(files) and {p for _, p in rows} == files
    for digest, path in rows:
        assert sha(clean(path)) == digest, path
        assert (ROOT / path).stat().st_size < 5_000_000
    print(json.dumps({"status": "PASS (stored read-only evidence/scope)", "manifest_files": len(rows), "source_pin": PIN, "base_prefix": "unchanged", "prior_stage_production_changes": 0, "startup": "NOT READY/NOT TESTED", "scope": "Cloud validates stored evidence only; no Files build, target, layout or activation executed"}, indent=2))

if __name__ == "__main__":
    main()
