"""Stage-branch documentation import policy; not a general draft freeze."""
from pathlib import Path
import argparse
import hashlib
import json
import re
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = "4e128e735edf788da0b9fa9e11f0d5e2511e5585"
OLD = "7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a"
TIPS = {"F12": "876b73ac752338fab09b225e227cfde77c1c0c63", "F4": "5fa5f856bc54632becbc365bb2453a71af4b0c29", "F8a": "c640972a4858c6cb72dbc5060a601c4f040c0e2f", "F8b": "fe2373b523c6fe2a70598af0bf37b8a028a70d63"}
WORKFLOW = ".github/workflows/z6-integration-docs.yml"
PREFIX = "z6/integration/docs/"

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
    for ticket, tip in TIPS.items():
        subprocess.run(["git", "-C", str(ROOT), "merge-base", "--is-ancestor", tip, "HEAD"], check=True)
        assert git("merge-base", BASE, tip).decode().strip() == OLD
        for path in git("diff", "--name-only", OLD, tip).decode().splitlines():
            if path == "z6/LEDGER_snapshot.md":
                continue
            data = git("show", f"{tip}:{path}")
            expected.append({"ticket": ticket, "source": tip, "path": path, "blob": git("rev-parse", f"{tip}:{path}").decode().strip(), "sha256": digest(data)})
    assert receipts["files"] == expected
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
    assert b"44880677014" in ledger and b"4e128e735edf788da0b9fa9e11f0d5e2511e5585" in ledger
    allowed = {r["path"] for r in expected} | {"z6/LEDGER_snapshot.md", WORKFLOW}
    changes = set(git("diff", "--name-only", BASE, "HEAD").decode().splitlines())
    changes |= set(git("diff", "--name-only", "HEAD").decode().splitlines())
    changes |= set(git("ls-files", "--others", "--exclude-standard").decode().splitlines())
    assert all(f in allowed or f.startswith(PREFIX) for f in changes), "Unexpected scope: " + repr(sorted(changes - allowed - {f for f in changes if f.startswith(PREFIX)}))
    assert not any(f.endswith("_LOCKED.md") or f == "docs/z6_closed.md" for f in changes)
    links = []
    for row in expected:
        if not row["path"].startswith("docs/"):
            continue
        text = git("show", f'{row["source"]}:{row["path"]}').decode("utf-8")
        for target in re.findall(r"(?<!!)\[[^\]]*\]\(([^)]+)\)", text):
            target = target.split("#", 1)[0]
            if not target or "://" in target or target.startswith("mailto:"):
                continue
            assert (ROOT / row["path"]).parent.joinpath(target).is_file(), "Broken actual Markdown link: " + target
            links.append({"document": row["path"], "target": target})
        if row["ticket"] in ("F8a", "F8b"):
            assert re.search(r"^STATUS=DRAFT.*NOT LOCKED", text, re.M)
            for key in ("APPROVED_BY", "APPROVED_AT"):
                assert re.search(r"^" + key + r": [＿_]+$", text, re.M), key
            assert re.search(r"^LOCKED_AT: .*LOCK", text, re.M)
            assert re.search(r"^GATE_BODY_SHA256: .*LOCK", text, re.M)
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
    result = {"status": "PASS", "base": BASE, "imported_files": len(expected), "exact_ledger_archives": len(expected_ledgers), "base_ledger_prefix": "unchanged", "local_links": links, "f8": "exact DRAFT imports; approvals blank; no lock/probe issued", "production_test_and_stage2a_changes": 0, "manifest_sealed": sealed, "manifest_files": count, "limitations": "Documentation integration only; no new GUI/performance run or original ticket re-adjudication"}
    if args.output:
        dest = ROOT / args.output
        assert dest.resolve().is_relative_to(HERE.resolve())
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(json.dumps(result, indent=2))

if __name__ == "__main__":
    main()
