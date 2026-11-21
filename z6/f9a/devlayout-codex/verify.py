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
 for f in H.glob("*.py"):ast.parse(f.read_text(encoding="utf-8"))
 manifest=H/"SHA256SUMS";entries=[l.split("  ",1) for l in manifest.read_text().splitlines()];files={f.relative_to(H).as_posix() for f in H.rglob("*") if f.is_file() and f!=manifest and "__pycache__" not in f.parts}
 assert {n for _,n in entries}==files and len(entries)==len(files)
 for sha,n in entries:assert hashlib.sha256(content(H/n)).hexdigest()==sha,n;assert (H/n).stat().st_size<5000000
 print(json.dumps({"status":"PASS","sealed_files":len(files),"source_destination_pairs":1328,"GateM":"First attempt STOP before activation; exact cleanup completed","scope":"Recorded GateL evidence integrity, no runtime GUI claim"},indent=2))
if __name__=="__main__":main()
