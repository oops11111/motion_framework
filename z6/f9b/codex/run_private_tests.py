"""Extract the actual private helper from reviewed patch, hash-bind it, run real behavior tests."""
from pathlib import Path
import argparse,hashlib,json,os,shutil,subprocess,uuid
H=Path(__file__).resolve().parent;R=H.parents[2]
def main():
 ap=argparse.ArgumentParser();ap.add_argument("--prepare-only",action="store_true");args=ap.parse_args()
 patch=H/"files_f9b_pilot_v4.patch";target="src/Files.App/Views/FilesMotionEvidence.cs";lines=patch.read_text(encoding="utf-8").splitlines();start=lines.index("diff --git a/"+target+" b/"+target);end=next((i for i in range(start+1,len(lines)) if lines[i].startswith("diff --git ")),len(lines));part=lines[start:end]
 assert "new file mode 100644" in part and "--- /dev/null" in part
 body=[];hunk=False
 for line in part:
  if line.startswith("@@ "):assert not hunk;hunk=True;continue
  if hunk:assert line.startswith("+");body.append(line[1:])
 data=("\r\n".join(body)+"\r\n").encode("utf-8")
 expected=next(r["post_sha256"] for r in json.loads((H/"host_source_v4.json").read_text()) if r["path"]==target);actual=hashlib.sha256(data).hexdigest();assert actual==expected,"Extracted actual helper differs from reviewed source"
 out=R/"m0/out";out.mkdir(parents=True,exist_ok=True);temp=out/("f9b-private-tests-"+uuid.uuid4().hex);temp.mkdir()
 for n in ["Program.cs","LogicTests.csproj"]:shutil.copyfile(H/"tests"/n,temp/n)
 (temp/"FilesMotionEvidence.cs").write_bytes(data)
 print(json.dumps({"actual_helper_sha256":actual,"patch_sha256":hashlib.sha256(patch.read_bytes()).hexdigest(),"mode":"prepare only" if args.prepare_only else "actual private logic tests; not GUI"},indent=2),flush=True)
 if args.prepare_only:return 0
 env=os.environ.copy();user=Path.home()/".dotnet/dotnet.exe";dotnet=str(user) if user.exists() else "dotnet"
 if user.exists():env["DOTNET_ROOT"]=str(user.parent);env["PATH"]=str(user.parent)+os.pathsep+env["PATH"]
 if os.name=="nt" and not env.get("PROCESSOR_ARCHITECTURE"):env["PROCESSOR_ARCHITECTURE"]="AMD64"
 return subprocess.run([dotnet,"run","--project",str(temp/"LogicTests.csproj"),"-c","Release"],cwd=temp,env=env).returncode
if __name__=="__main__":raise SystemExit(main())
