"""Read-only final delivery/source proof verification; never builds or launches products."""
import argparse
import hashlib
import json
import re
import zipfile
from pathlib import Path

APP = "191f4d65b15e6fecf0118e0b120c7760ef3d8d53"
MF = "44401d3f002b427d828500331a3e9b4ada5881cd"
RUN = 45011904252
EXE = "e2f5ef48949d7af817de6d56b25eadbaca4bc6bf2331eaec68212f82d4a2a286"
PACKAGE = "b2e47d89488e51ef32d16b4775298d6171dddfc992eb187d4d7f253243354062"


def sha(path):
    value = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(block)
    return value.hexdigest()


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def safe(root, relative):
    assert not Path(relative).is_absolute() and ":" not in relative
    assert ".." not in Path(relative).parts
    result = root / relative
    assert result.resolve().is_relative_to(root.resolve())
    for item in [result, *result.parents]:
        assert not item.is_symlink() and not item.is_junction(), str(item)
        if item == root:
            break
    return result


def checksum_set(root):
    rows = {}
    for line in (root / "SHA256SUMS").read_text(encoding="utf-8").splitlines():
        match = re.fullmatch(r"([0-9a-f]{64})  (.+)", line)
        assert match, line
        digest, relative = match.groups()
        assert relative not in rows
        assert sha(safe(root, relative)) == digest, relative
        rows[relative] = digest
    # Only this root's checksum file is excluded; nested checksums remain ordinary payload.
    actual = {p.relative_to(root).as_posix() for p in root.rglob("*") if p.is_file()} - {"SHA256SUMS"}
    assert actual == set(rows), actual.symmetric_difference(rows)
    return rows


def source_docs():
    here = Path(__file__).parent
    if not (here / "P4_EVIDENCE").is_dir():
        print("Portable delivery mode: source repository documents are not this directory")
        return
    rows = checksum_set(here / "P4_EVIDENCE")
    for doc in here.glob("P4*.md"):
        text = doc.read_text(encoding="utf-8")
        for link in re.findall(r"\[[^\]]+\]\(([^)]+)\)", text):
            if not link.startswith(("https://", "http://", "#")):
                assert (doc.parent / link.split("#")[0]).is_file(), (doc.name, link)
    print(f"PASS source evidence {len(rows)} exact files and local Markdown links")


def delivery(root, allow_pending):
    rows = checksum_set(root)
    manifest = read(root / "manifest.json")
    assert manifest["application_commit"] == APP and manifest["component_source_commit"] == MF
    assert manifest["run"] == RUN
    assert sha(safe(root, manifest["installer"]["path"])) == manifest["installer"]["sha256"] == EXE
    assert sha(safe(root, manifest["package"]["path"])) == manifest["package"]["sha256"] == PACKAGE
    build = read(root / "evidence/input/build-input.json")
    state = read(root / "evidence/input/component-state.json")
    assert build["source"]["checkout_commit"] == APP == build["source"]["event_commit"]
    assert state["source_commit"] == MF and state["package_sha256"] == PACKAGE
    mapping = {item["input_path"]: item for item in manifest["input_mapping"]}
    assert len(build["files"]) == 27 and len(build["package_members"]) == 9
    for item in build["files"]:
        target = safe(root, mapping[item["path"]]["delivery_path"])
        assert sha(target) == item["sha256"] == mapping[item["path"]]["sha256"]
        assert target.stat().st_size == item["bytes"]
    with zipfile.ZipFile(safe(root, manifest["package"]["path"])) as package:
        assert set(package.namelist()) == {item["path"] for item in build["package_members"]}
        for item in build["package_members"]:
            assert hashlib.sha256(package.read(item["path"])).hexdigest() == item["sha256"]
    smoke = root / "evidence/smoke"
    exit_proof = read(smoke / "normal-exit.json")
    launch = read(smoke / "shortcut-launch.json")
    assert exit_proof["exit"] == 0 and exit_proof["retained_handle"] and exit_proof["normal_tray_exit"]
    assert exit_proof["pid"] == launch["pid"] and exit_proof["start_utc"] == launch["process_started_utc"]
    for module in read(smoke / "loaded-modules.json"):
        member = next(item for item in build["package_members"] if item["path"].endswith("/" + module["name"]))
        assert module["sha256"] == member["sha256"]
        assert module["path"].replace("\\", "/") == "C:/Program Files/EveryStage/Terminal/" + module["name"]
    calls = [json.loads(line) for line in (smoke / "installer-calls.jsonl").read_text(encoding="utf-8-sig").splitlines() if line.strip()]
    assert len(calls) == 9 and all(item["exit"] == 0 for item in calls)
    assert {item["selection"] for item in read(smoke / "result.json")["cases"]} == {"Terminal", "Caster", "Both"}
    primary = read(root / "evidence/POST_PRIMARY_ACCEPTANCE.json")
    assert sha(root / "evidence/POST_PRIMARY_ACCEPTANCE.json") == manifest["primary_post_receipt_sha256"]
    for image in primary["images"]:
        assert sha(smoke / image["file"]) == image["sha256"]
    assert sha(root / "baseline/baseline.zip") == "76614f31aa34372c15f0de26d69f818f89c3b5814967b24a0aa980b4c7eaad94"
    print(f"PASS delivery content {len(rows)} files/input27/package9/modules/normal exit/9 calls/3 cases/6 image SHA")
    final_path = root / "ROOT_ACCEPTANCE.json"
    if not final_path.exists():
        print("FINAL ACCEPTANCE PENDING")
        assert allow_pending, "Final external root acceptance is required"
        return
    final = read(final_path)
    assert final["status"] == "accepted internal delivery"
    assert final["application_commit"] == APP and final["component_source_commit"] == MF
    assert final["application_post_run"] == RUN
    assert final["manifest_sha256"] == sha(root / "manifest.json")
    assert final["installer_sha256"] == EXE and final["package_sha256"] == PACKAGE
    assert re.fullmatch("[0-9a-f]{40}", final["mf_docs_merge_sha"])
    assert final["mf_docs_post_run"] > 0 and final["mf_docs_post_conclusion"] == "success"
    assert final["primary_review"] == "accepted"
    print("PASS final external internal-delivery acceptance; product bytes unchanged")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--delivery", type=Path)
    parser.add_argument("--allow-pending", action="store_true")
    args = parser.parse_args()
    source_docs()
    if args.delivery:
        delivery(args.delivery.resolve(), args.allow_pending)
