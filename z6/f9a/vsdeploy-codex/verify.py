"""Stage-scoped evidence validation; no VS/deploy/GUI execution."""
from pathlib import Path
import hashlib,json,subprocess,ast
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
BASE="92c498fbcf8ea7216d54104886a722d34d5099fb"
PREFIX="z6/f9a/vsdeploy-codex/"
def git(*a):return subprocess.check_output(["git","-C",str(ROOT),*a])
def digest(b):return hashlib.sha256(b).hexdigest()
def data(p):
 b=p.read_bytes()
 return b if "source_ledgers" in p.parts else b.replace(b"\r\n",b"\n")
def main():
 old=git("show",BASE+":z6/LEDGER_snapshot.md")
 assert (HERE/"source_ledgers/base.md").read_bytes()==old
 assert data(ROOT/"z6/LEDGER_snapshot.md").startswith(old)
 receipt=json.loads((HERE/"base_receipt.json").read_text())
 assert receipt["ledger_sha256"]==digest(old) and receipt["ledger_blob"]==git("rev-parse",BASE+":z6/LEDGER_snapshot.md").decode().strip()
 changes=set(git("diff","--name-only",BASE,"HEAD").decode().splitlines())|set(git("diff","--name-only","HEAD").decode().splitlines())|set(git("ls-files","--others","--exclude-standard").decode().splitlines())
 assert all(n.startswith(PREFIX) or n in ["z6/LEDGER_snapshot.md",".github/workflows/z6-f9a-vsdeploy.yml"] for n in changes),changes
 auth=json.loads((HERE/"B_AUTHORIZATION.json").read_text(encoding="utf-8"));instruction=json.loads((HERE/"PRIMARY_GATE_B_INSTRUCTION.json").read_text(encoding="utf-8"))
 assert auth["user_verbatim"]=="批准，仅本票 FilesDev 部署、主页启动及精确清理。"
 assert instruction["authority_sha256"]==digest(data(HERE/"B_AUTHORIZATION.json"))
 result=json.loads((HERE/"raw/f9a-gateB-20261121T113424Z-2ea2b1b4/result.json").read_text(encoding="utf-8"))
 assert result["deploy_exit"]==0 and result["status"].startswith("STOP") and result["owned_package"] is None
 assert result["PRE"]==result["POST"]==[] and result["state_changed"] is False
 assert result["state_comparison_available"] and result["source_PRE"]==result["source_POST"]
 assert result["source_POST"]["head"]=="cbfb4ba54a89632fbceb82ea94ada60a9bae3737" and not result["source_POST"]["status"]
 assert not any(result["other_package_delta_final"].values()) and not any(result["other_package_delta_after_deploy"].values())
 assert result["all_packages_PRE_sha256"]==result["all_packages_AFTER_DEPLOY_sha256"]==result["all_packages_POST_sha256"]
 assert "mainpage_ready" not in result and "normal_close_exit" not in result
 for f in HERE.glob("*.py"):ast.parse(f.read_text(encoding="utf-8"))
 manifest=HERE/"SHA256SUMS";entries=[l.split("  ",1) for l in manifest.read_text().splitlines()]
 files={f.relative_to(HERE).as_posix() for f in HERE.rglob("*") if f.is_file() and f!=manifest and "__pycache__" not in f.parts}
 assert {n for _,n in entries}==files and len(entries)==len(files)
 for h,n in entries:
  f=HERE/n;assert digest(data(f))==h,n;assert f.stat().st_size<5000000
 print(json.dumps({"status":"PASS","manifest_files":len(files),"local_GateA":"NON-ORIGINAL build PASS","local_GateB":"STOP: exit0 without FilesDev","startup":"NOT TESTED","scope":"evidence integrity only; no cloud VS deployment claim"},indent=2))
if __name__=="__main__":main()
