"""Released F2 source-only baseline preparation; no build or application execution."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,os,shutil,subprocess,uuid
H=Path(__file__).resolve().parent;R=H.parents[2];PIN="eb08f0dbcbb43880ab34038b0cb6742f0c0deca5"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 stamp=datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8];out=R/"m0/out"/("f2-repair-"+stamp);out.mkdir();raw=H/"raw"/stamp;raw.mkdir(parents=True);source=out/"source";reference=R/"m0/out/f5-fluenthub"
 result={"status":"STOP","source_pin":PIN,"source_relative":source.relative_to(R).as_posix(),"app_build_runtime":False,"commands":[]}
 def clean(s):return s.replace(str(R),"%WORKSPACE%").replace(str(Path.home()),"%USERPROFILE%").replace(os.environ["ProgramFiles"],"%ProgramFiles%")
 def run(label,args,cwd,env=None):
  r=subprocess.run([str(x) for x in args],cwd=cwd,env=env,capture_output=True);original=out/(label+".original.log");original.write_bytes(r.stdout+r.stderr);(raw/(label+".log")).write_text(clean((r.stdout+r.stderr).decode("utf-8",errors="replace")),encoding="utf-8",newline="\n");result["commands"].append({"label":label,"argv":[clean(str(x)) for x in args],"exit":r.returncode,"original_sha256":sha(original)});assert r.returncode==0,label+" failed"
 try:
  run("clone",["git","clone","--no-hardlinks","--no-checkout",Path.home()/"pilot/FluentHub",source],R)
  run("checkout",["git","-C",source,"checkout","--detach",PIN],R)
  known={row["file"]:row["sha256_lf"] for row in json.loads((R/"z6/f5/codex/raw/patch_roundtrip_v3.json").read_text(encoding="utf-8"))};names=["src/FluentHub/App.xaml.cs","src/FluentHub/FluentHub.csproj","src/FluentHub/Views/MainWindow.cs","src/FluentHub/Views/RootView.xaml.cs","src/FluentHub/Views/NavMotionPilot.cs","src/FluentHub/Views/ReducedMotionState.cs","src/FluentHub/Views/FakeReducedMotionSource.cs","src/FluentHub/Views/WindowReducedMotionSource.cs"];files=[]
  for n in names:
   src=reference/n;data=src.read_bytes();normalized=hashlib.sha256(data.replace(b"\r\n",b"\n")).hexdigest()
   if n in known:assert normalized==known[n],"F5 v3 reference drift: "+n
   if n.endswith("RootView.xaml.cs"):assert sha(src)=="364e9d65a6d6508c3417a34fe98b6a16cae7314d74ccd1f28b94ebf6ab144184"
   dst=source/n;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst);assert sha(src)==sha(dst)
   files.append({"path":n,"reference_sha256":sha(src),"copied_sha256":sha(dst),"sha256_lf":normalized,"F5_v3_roundtrip_match":n in known,"provenance":"F5 v3 accepted source" if n in known else "F2 original RootView post hash" if n.endswith("RootView.xaml.cs") else "F11 configurable/default pack csproj in accepted F5 reference; copied unchanged, no path override"})
  result["baseline_source_files"]=files
  result["source_status"]=subprocess.check_output(["git","--no-optional-locks","-C",str(source),"status","--porcelain","--untracked-files=all"]).decode();result["upstream_head"]=subprocess.check_output(["git","-C",str(source),"rev-parse","HEAD"]).decode().strip();assert result["upstream_head"]==PIN
  env=os.environ.copy();result["architecture_before"]=env.get("PROCESSOR_ARCHITECTURE")
  if not env.get("PROCESSOR_ARCHITECTURE"):env["PROCESSOR_ARCHITECTURE"]="AMD64"
  result["child_only_architecture"]=env["PROCESSOR_ARCHITECTURE"];result["PATH_unchanged"]=env["PATH"]==os.environ["PATH"];result["DOTNET_ROOT_unchanged"]=env.get("DOTNET_ROOT")==os.environ.get("DOTNET_ROOT")
  run("default_dotnet_info_child_amd64",["dotnet","--info"],source,env)
  result["status"]="PASS isolated F5 v3 baseline/source-only"
 except Exception as error:result["error"]=clean(str(error))
 finally:(raw/"result.json").write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8",newline="\n");print(json.dumps({k:v for k,v in result.items() if k!="baseline_source_files"},indent=2))
 return 0 if result["status"].startswith("PASS") else 1
if __name__=="__main__":raise SystemExit(main())
