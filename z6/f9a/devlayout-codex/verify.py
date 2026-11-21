"""Recorded GateL integrity only; no copy/registration/activation."""
from pathlib import Path
import hashlib,json,subprocess,ast
H=Path(__file__).resolve().parent
R=H.parents[2]
BASE="0fc838c9624b4a23c99bbc57f3d5d1c9bf8c2510"
def git(*a):return subprocess.check_output(["git","-C",str(R),*a])
def content(p):
 b=p.read_bytes();return b if "source_ledgers" in p.parts else b.replace(b"\r\n",b"\n")
def main():
 old=git("show",BASE+":z6/LEDGER_snapshot.md");assert (H/"source_ledgers/base.md").read_bytes()==old and content(R/"z6/LEDGER_snapshot.md").startswith(old)
 changes=set(git("diff","--name-only",BASE,"HEAD").decode().splitlines())|set(git("diff","--name-only","HEAD").decode().splitlines())|set(git("ls-files","--others","--exclude-standard").decode().splitlines())
 assert all(n.startswith("z6/f9a/devlayout-codex/") or n in ["z6/LEDGER_snapshot.md",".github/workflows/z6-f9a-devlayout.yml"] for n in changes)
 a=H/"raw/f9a-devlayout-20261121T115207Z-c7612b64";result=json.loads((a/"result.json").read_text())
 assert result["status"]=="PASS L workspace byte-copy only" and not result["registration"] and not result["activation"]
 assert result["source_PRE"]==result["source_POST"] and not result["source_POST"]["status"] and result["source_POST"]["head"]=="cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
 inventory=json.loads((a/"inventory.json").read_text());dest=json.loads((a/"destination_hashes.json").read_text());assert len(inventory)==len(dest)==1328
 assert len({r["destination"].casefold() for r in inventory})==1328
 assert sum(r["kind"]=="server-output" for r in inventory)==22
 for x,y in zip(inventory,dest):assert all(x[k]==y[k] for k in ["destination","sha256","bytes"])
 for target in json.loads((a/"manifest_targets.json").read_text()):assert target["exact_file"] or (target["kind"]=="resource" and target["qualified_family"])
 runs=[]
 for name in ["f9a-gateM-20261121T120120Z-abc22b4e","f9a-gateM-20261121T120437Z-eca9f19a","f9a-gateM-20261121T121105Z-994a1615"]:
  r=json.loads((H/"raw"/name/"result.json").read_text(encoding="utf-8"));runs.append(r)
  assert r["registration_exit"]==0 and r["cleanup_complete"] and r["POST"]==[] and not r["state_changed"] and r["state_comparison_available"]
  assert r["all_packages_PRE_sha256"]==r["all_packages_POST_sha256"]
  assert not any(r["other_package_delta_after_deploy"].values()) and not any(r["other_package_delta_final"].values())
  assert r["source_PRE"]==r["source_POST"] and not r["source_POST"]["status"] and r["layout_PRE"]==r["layout_POST"]
  assert r["removed_exact_fullname"]==r["owned_package"]["PackageFullName"]=="FilesDev_4.3.0.0_x64__ykqwq8d6ps0ag"
 assert all(r["status"].startswith("STOP") and "mainpage_ready" not in r for r in runs[:2])
 passed=runs[-1];assert passed["status"]=="PASS (M manual layout main-page and exact cleanup)" and passed["normal_close_exit"]==0
 marker=passed["mainpage_ready"];assert marker["marker"]=="SettingsButton" and all(marker[k] for k in ["visible","enabled","exists","ready"])
 assert marker["pid"]==passed["identity_verified"]["pid"]
 auth=H/"AUTHORITY_CONTEXT.json";context=json.loads(auth.read_text(encoding="utf-8"));assert context["user_verbatim"]=="批准，仅本票 FilesDev 部署、主页启动及精确清理。"
 instruction=json.loads((H/"PRIMARY_GATE_M_CONTINUATION_DOTNETSHA.json").read_text(encoding="utf-8"));assert instruction["authority_sha256"]==hashlib.sha256(content(auth)).hexdigest()
 readonly=json.loads((H/"raw/f9a-gateM-20261121T121105Z-994a1615/post_readonly.json").read_text());assert readonly["residual_FilesDev_data_names"]==[]
 for n,h in instruction["executing_scripts_sha256"].items():assert hashlib.sha256(content(H/n)).hexdigest()==h and readonly["actual_script_hashes"][n]==h,n
 for f in H.glob("*.py"):ast.parse(f.read_text(encoding="utf-8"))
 manifest=H/"SHA256SUMS";entries=[l.split("  ",1) for l in manifest.read_text().splitlines()];files={f.relative_to(H).as_posix() for f in H.rglob("*") if f.is_file() and f!=manifest and "__pycache__" not in f.parts}
 assert {n for _,n in entries}==files and len(entries)==len(files)
 for sha,n in entries:assert hashlib.sha256(content(H/n)).hexdigest()==sha,n;assert (H/n).stat().st_size<5000000
 print(json.dumps({"status":"PASS","sealed_files":len(files),"source_destination_pairs":1328,"GateM":"M3 manual development layout main-page/normal close/exact cleanup PASS; M1/M2 STOP retained","scope":"Recorded L/M evidence integrity; local manual-layout runtime proof, no cloud GUI reproduction"},indent=2))
if __name__=="__main__":main()
