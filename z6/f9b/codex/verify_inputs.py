"""Phase1 receipt/freeze verification; never builds or executes reference/host."""
from pathlib import Path
import ast,hashlib,json,subprocess
H=Path(__file__).resolve().parent;R=H.parents[2];BASE="05830996127cd40d32f3718a2ae253583d092f36"
def git(*a):return subprocess.check_output(["git","-C",str(R),*a])
def data(f):
 b=f.read_bytes();return b if "source_ledgers" in f.parts else b.replace(b"\r\n",b"\n")
def digest(b):return hashlib.sha256(b).hexdigest()
def main():
 old=git("show",BASE+":z6/LEDGER_snapshot.md");assert (H/"source_ledgers/base.md").read_bytes()==old and data(R/"z6/LEDGER_snapshot.md").startswith(old)
 auth=json.loads((H/"AUTHORIZATION.json").read_text(encoding="utf-8"));assert auth["user_verbatim"]=="开票" and auth["recorded_utc"]=="2026-11-21T12:45:16Z"
 changes=set(git("diff","--name-only",BASE,"HEAD").decode().splitlines())|set(git("diff","--name-only","HEAD").decode().splitlines())|set(git("ls-files","--others","--exclude-standard").decode().splitlines());assert all(n.startswith("z6/f9b/codex/") or n in ["z6/LEDGER_snapshot.md",".github/workflows/z6-f9b-pilot.yml"] for n in changes)
 a=H/"raw/20261121T125037Z-fab35294";r=json.loads((a/"result.json").read_text())
 assert r["status"]=="PASS F9b.1 pin/reference/API only" and not r["host_patch_build_runtime"] and not r["reference_native_executed"]
 assert [x["label"] for x in r["commands"]]==["clone","checkout","dotnet_version","surface_tool_build","surface_check"]
 for x in r["commands"]:assert x["exit"]==0 and digest(data(a/(x["label"]+".log")))==x["published_sha256"]
 assert (a/"dotnet_version.log").read_text().strip()=="10.0.404"
 before=json.loads((a/"source_PRE.json").read_text());assert before==json.loads((a/"source_POST.json").read_text()) and before["head"]=="cbfb4ba54a89632fbceb82ea94ada60a9bae3737" and not before["status"] and not before["diff"]
 expected={"FluidMotion.L1.dll":"94d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2","fluidmotion.dll":"add4857dead413f8211941381c6aa8f73c546f3ecae1774aced79c464719577d","demo_motion.json":"8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e"}
 refs=json.loads((a/"reference_receipts.json").read_text());assert {x["name"] for x in refs}==set(expected)
 for x in refs:assert all(x[k]==expected[x["name"]] for k in ["expected_sha256","cache_before_sha256","copied_sha256","cache_after_sha256","copied_after_sha256"])
 surface=data(a/"surface_reference.txt");assert surface==git("show",BASE+":"+r["surface"]["stage2C_baseline"]) and digest(surface)=="7b67b77bce6b36dc58cc870a17fe1b5588df3c6e526b39ca5a3e36d5280bc5cc"
 assert "CHECKSURFACE fails=0" in (a/"surface_reference.checksurface.txt").read_text()
 frozen=json.loads((a/"freeze_PRE.json").read_text());assert frozen==json.loads((a/"freeze_POST.json").read_text())
 for n,row in frozen.items():assert digest(data(R/n))==row["working_normalized_sha256"] and git("rev-parse","HEAD:"+n).decode().strip()==row["git_blob"]
 for f in H.glob("*.py"):ast.parse(f.read_text(encoding="utf-8"))
 manifest=H/"SHA256SUMS";entries=[l.split("  ",1) for l in manifest.read_text().splitlines()];files={f.relative_to(H).as_posix() for f in H.rglob("*") if f.is_file() and f!=manifest and "__pycache__" not in f.parts};assert {n for _,n in entries}==files and len(entries)==len(files)
 for sha,n in entries:assert digest(data(H/n))==sha,n;assert (H/n).stat().st_size<5000000
 print(json.dumps({"status":"PASS F9b.1 receipts","sealed_files":len(files),"full_surface_bytes_equal":True,"reference_files":3,"freeze_files":len(frozen),"phase1_host_patch_build_runtime":"NOT EXECUTED in phase1"},indent=2))
if __name__=="__main__":main()
