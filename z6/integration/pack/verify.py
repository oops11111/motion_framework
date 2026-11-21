"""Stage-branch pack metadata provenance; no cloud reference-cache claims."""
from pathlib import Path
import argparse
import hashlib
import json
import re
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "8c5cc6f31ca2be796986afd901fc1bb1098712ca"
OLD = "7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a"
TIPS = {"F7": "6b9f4767829add919cb6e626f45208f2ecf59614", "F11": "c3fb8d926458ebb93c3bc675a13c5f25ab853719", "F6": "5e92d05ea3cf7c943dd3a79e97223db10c8d74e0"}
WORKFLOW = ".github/workflows/z6-integration-pack.yml"
PREFIX = "z6/integration/pack/"

def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])

def digest(data):
    return hashlib.sha256(data).hexdigest()

def clean_bytes(path):
    data = (ROOT / path).read_bytes()
    # All new stage text is LF; exact opaque source-ledger archives are -text.
    return data if "/source_ledgers/" in path else data.replace(b"\r\n", b"\n")

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output")
    args = parser.parse_args()
    receipts = json.loads((HERE / "source_receipts.json").read_text(encoding="utf-8"))
    assert receipts["base"] == BASE and receipts["old_base"] == OLD and receipts["sources"] == TIPS
    expected = []
    previous = OLD
    for ticket, tip in TIPS.items():
        subprocess.run(["git", "-C", str(ROOT), "merge-base", "--is-ancestor", tip, "HEAD"], check=True)
        assert git("merge-base", BASE, tip).decode().strip() == OLD
        assert git("merge-base", previous, tip).decode().strip() == previous
        for path in git("diff", "--name-only", previous, tip).decode().splitlines():
            if path == "z6/LEDGER_snapshot.md":
                continue
            data = git("show", f"{tip}:{path}")
            expected.append({"ticket": ticket, "source": tip, "path": path, "blob": git("rev-parse", f"{tip}:{path}").decode().strip(), "sha256": digest(data)})
        previous = tip
    assert receipts["files"] == expected
    assert len(expected) == 96
    for row in expected:
        actual = git("hash-object", "--path=" + row["path"], row["path"]).decode().strip()
        assert actual == row["blob"], "Import changed: " + row["path"]
    expected_ledgers = []
    for label, tip in [("base-main", BASE), *TIPS.items()]:
        data = git("show", f"{tip}:z6/LEDGER_snapshot.md")
        path = f"{PREFIX}source_ledgers/{label}-{tip}.md"
        assert (ROOT / path).read_bytes() == data, "Ledger archive changed: " + path
        expected_ledgers.append({"source": tip, "path": path, "blob": git("rev-parse", f"{tip}:z6/LEDGER_snapshot.md").decode().strip(), "sha256": digest(data)})
    assert receipts["ledgers"] == expected_ledgers
    ledger = clean_bytes("z6/LEDGER_snapshot.md")
    assert ledger.startswith(git("show", f"{BASE}:z6/LEDGER_snapshot.md")), "Base ledger prefix changed"
    assert b"44881810494" in ledger and b"8c5cc6f31ca2be796986afd901fc1bb1098712ca" in ledger
    allowed = {r["path"] for r in expected} | {"z6/LEDGER_snapshot.md", WORKFLOW}
    changes = set(git("diff", "--name-only", BASE, "HEAD").decode().splitlines())
    changes |= set(git("diff", "--name-only", "HEAD").decode().splitlines())
    changes |= set(git("ls-files", "--others", "--exclude-standard").decode().splitlines())
    assert all(f in allowed or f.startswith(PREFIX) for f in changes), "Unexpected scope: " + repr(sorted(changes - allowed - {f for f in changes if f.startswith(PREFIX)}))
    assert not any(f.endswith("_LOCKED.md") or f == "docs/z6_closed.md" for f in changes)
    previous_metadata = []
    for path in ["pilot/l1pack/README.md", "pilot/l1pack/SHA256SUMS"]:
        previous_metadata.append({"source": BASE, "path": path, "blob": git("rev-parse", f"{BASE}:{path}").decode().strip(), "sha256": digest(git("show", f"{BASE}:{path}"))})
    assert receipts["previous_metadata"] == previous_metadata
    sums = clean_bytes("pilot/l1pack/SHA256SUMS").decode("utf-8")
    assert "mf_tip (build source, main): " + OLD in sums
    binaries = dict((name, sha) for sha, name in re.findall(r"^([0-9a-f]{64})  (.+)$", sums, re.M))
    assert set(binaries) == {"FluidMotion.L1.dll", "fluidmotion.dll", "demo_motion.json"}
    source_rows = re.findall(r"^([0-9a-f]{40})  (.+)$", sums, re.M)
    assert {path for _, path in source_rows} == {"MotionSoftDemo/MotionSystem.cs", "MotionSoftDemo/MotionEtw.cs"}
    for blob, path in source_rows:
        assert git("hash-object", "--path=" + path, path).decode().strip() == blob
    assert git("rev-parse", f"{BASE}:code/capi/fm_capi.cpp") == git("rev-parse", f"{OLD}:code/capi/fm_capi.cpp")
    inspections = list((HERE / "raw").glob("*/result.json"))
    assert len(inspections) == 1
    inspection = json.loads(inspections[0].read_text(encoding="utf-8"))
    assert inspection["status"] == "PASS (reference hash/API inspection only)"
    assert {r["name"]: r["reference_sha256"] for r in inspection["files"]} == binaries
    assert {r["name"]: r["copied_sha256"] for r in inspection["files"]} == binaries
    baseline = git("show", "HEAD:pilot/fluenthub/surface_l1pack.txt")
    assert inspection["surface_sha256"] == inspection["baseline_sha256"] == digest(baseline)
    assert inspections[0].with_name("surface_reference.txt").read_bytes() == baseline
    assert "CHECKSURFACE fails=0" in inspections[0].with_name("surface_reference.checksurface.txt").read_text(encoding="utf-8")
    manifest = HERE / "SHA256SUMS"
    sealed = manifest.exists()
    assert sealed or args.output, "Missing sealed manifest"
    count = 0
    if sealed:
        rows = [line.split("  ", 1) for line in manifest.read_text(encoding="ascii").splitlines()]
        files = {f.relative_to(ROOT).as_posix() for f in HERE.rglob("*") if f.is_file() and f != manifest}
        assert {r[1] for r in rows} == files and len(rows) == len(files), "Manifest coverage"
        for sha, path in rows:
            assert digest(clean_bytes(path)) == sha, "Manifest hash: " + path
        count = len(rows)
    assert all(f.stat().st_size < 5_000_000 for f in HERE.rglob("*") if f.is_file())
    result = {"status": "PASS", "base": BASE, "imported_files": len(expected), "historical_files": 94, "metadata_files": 2, "exact_ledger_archives": len(expected_ledgers), "base_ledger_prefix": "unchanged", "source_hashes": "metadata source declarations match unchanged current sources", "reference_evidence": "Local hash/copy/API receipts intact; cloud policy does not independently access reference cache", "production_test_and_prior_stage_changes": 0, "manifest_sealed": sealed, "manifest_files": count, "limitations": "No current-main pack rebuild, GUI, relocation/corruption rerun or original verdict re-adjudication"}
    if args.output:
        dest = ROOT / args.output
        assert dest.resolve().is_relative_to(HERE.resolve())
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(json.dumps(result, indent=2))

if __name__ == "__main__":
    main()
