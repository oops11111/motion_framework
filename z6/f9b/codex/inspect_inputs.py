"""F9b.1 only: isolated source pin, read-only pack copy, reflection-only API."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,os,re,shutil,subprocess,uuid
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
BASE="05830996127cd40d32f3718a2ae253583d092f36"
PIN="cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p,v):p.write_text(json.dumps(v,indent=2)+"\n",encoding="utf-8",newline="\n")
def main():
 attempt=datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8]
 out=ROOT/"m0/out"/("f9b-inputs-"+attempt);out.mkdir();raw=HERE/"raw"/attempt;raw.mkdir(parents=True)
 source=out/"source";reference=out/"reference";tool=out/"surface_tool";cache=Path.home()/"mf_z6/f7/pilot/l1pack/bin/x64/Release/net8.0"
 result={"phase":"F9b.1","status":"STOP","attempt":attempt,"source_pin":PIN,"source_relative":source.relative_to(ROOT).as_posix(),"reference_relative":reference.relative_to(ROOT).as_posix(),"commands":[],"host_patch_build_runtime":False,"reference_native_executed":False}
 def clean(s):return s.replace(str(ROOT),"%WORKSPACE%").replace(str(Path.home()),"%USERPROFILE%").replace(str(ROOT).replace("\\","/"),"%WORKSPACE%").replace(str(Path.home()).replace("\\","/"),"%USERPROFILE%")
 def git(*a):return subprocess.check_output(["git","--no-optional-locks",*a]).decode().strip()
 def source_state():return {"head":git("-C",str(source),"rev-parse","HEAD"),"status":git("-C",str(source),"status","--porcelain"),"diff":git("-C",str(source),"diff","HEAD","--")}
 paths=["code/composition.h","code/capi/fm_capi.h","code/capi/fm_capi.cpp","MotionSoftDemo/MotionSystem.cs","MotionSoftDemo/MotionEtw.cs","pilot/l1pack/FluidMotion.L1.csproj","pilot/l1pack/SHA256SUMS","pilot/l1pack/README.md","docs/api_surface_m2_alpha_contract_LOCKED.md","docs/z4_etw_l1_smoke_gate_r1_LOCKED.md","docs/api.md","docs/api_changelog.md"]
 def freeze():
  return {n:{"git_blob":git("-C",str(ROOT),"rev-parse","HEAD:"+n),"working_normalized_sha256":hashlib.sha256((ROOT/n).read_bytes().replace(b"\r\n",b"\n")).hexdigest()} for n in paths}
 env=os.environ.copy();sdk=Path.home()/".dotnet";env["DOTNET_ROOT"]=str(sdk);env["PATH"]=str(sdk)+os.pathsep+env["PATH"]
 if not env.get("PROCESSOR_ARCHITECTURE"):env["PROCESSOR_ARCHITECTURE"]="AMD64"
 result["child_environment"]={"DOTNET_ROOT":"%USERPROFILE%/.dotnet","PROCESSOR_ARCHITECTURE":env["PROCESSOR_ARCHITECTURE"],"persistent_changes":False}
 def run(label,args,cwd):
  original=out/(label+".original.log")
  with original.open("wb") as f:
   proc=subprocess.Popen([str(x) for x in args],cwd=cwd,env=env,stdout=f,stderr=subprocess.STDOUT);print(label+" PID="+str(proc.pid),flush=True);code=proc.wait()
  text=clean(original.read_text(encoding="utf-8",errors="replace")).replace("\r\n","\n");(raw/(label+".log")).write_text(text,encoding="utf-8",newline="\n")
  result["commands"].append({"label":label,"argv":[clean(str(x)) for x in args],"pid":proc.pid,"exit":code,"original_sha256":sha(original),"published_sha256":sha(raw/(label+".log"))});assert code==0,label+" failed"
 try:
  before=freeze();save(raw/"freeze_PRE.json",before)
  assert before["code/composition.h"]["working_normalized_sha256"]=="a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418"
  run("clone",["git","clone","--no-hardlinks","--no-checkout",str(Path.home()/"pilot/Files"),str(source)],ROOT)
  run("checkout",["git","-C",str(source),"checkout","--detach",PIN],ROOT)
  pre=source_state();assert pre["head"]==PIN and not pre["status"] and not pre["diff"];save(raw/"source_PRE.json",pre)
  hooks=["src/Files.App/Files.App.csproj","src/Files.App/Views/MainPage.xaml.cs","src/Files.App/Views/MainPage.xaml","src/Files.App/MainWindow.xaml.cs","src/Files.App/App.xaml.cs"]
  save(raw/"host_baselines.json",{n:{"git_blob":git("-C",str(source),"rev-parse","HEAD:"+n),"sha256":sha(source/n)} for n in hooks})
  sums=(ROOT/"pilot/l1pack/SHA256SUMS").read_text(encoding="utf-8");expected={n:h for h,n in re.findall(r"^([0-9a-f]{64})  (.+)$",sums,re.M)};assert set(expected)=={"FluidMotion.L1.dll","fluidmotion.dll","demo_motion.json"}
  reference.mkdir();receipts=[]
  for n,h in expected.items():
   src=cache/n;assert sha(src)==h,"Reference mismatch: "+n;dst=reference/n;shutil.copyfile(src,dst);assert sha(dst)==h
   receipts.append({"name":n,"expected_sha256":h,"cache_before_sha256":sha(src),"copied_sha256":sha(dst)})
  tool.mkdir();toolinputs=[]
  for srcname,dstname in [("SurfaceDump_Program.cs","Program.cs"),("SurfaceDump.csproj.txt","SurfaceDump.csproj")]:
   src=ROOT/"z5/t3_results_dev/K93N3P8"/srcname;dst=tool/dstname;shutil.copyfile(src,dst);assert sha(src)==sha(dst);toolinputs.append({"source":"z5/t3_results_dev/K93N3P8/"+srcname,"sha256":sha(src)})
  save(raw/"tool_inputs.json",toolinputs)
  run("dotnet_version",[sdk/"dotnet.exe","--version"],tool)
  run("surface_tool_build",[sdk/"dotnet.exe","build",str(tool/"SurfaceDump.csproj"),"-c","Release"],tool)
  dump=out/"surface_reference.txt";run("surface_check",[sdk/"dotnet.exe",str(tool/"bin/Release/net10.0/SurfaceDump.dll"),str(reference/"FluidMotion.L1.dll"),str(dump)],tool)
  check=dump.with_suffix(".checksurface.txt");assert "CHECKSURFACE fails=0" in check.read_text(encoding="utf-8")
  baselinepath="z6/integration/pack/raw/20261121T075208Z-14b65d8f/surface_reference.txt";baseline=subprocess.check_output(["git","-C",str(ROOT),"show",BASE+":"+baselinepath]);assert dump.read_bytes()==baseline,"Full stage2C baseline differs"
  shutil.copyfile(dump,raw/dump.name);shutil.copyfile(check,raw/check.name)
  result["surface"]={"sha256":sha(dump),"stage2C_baseline":baselinepath,"baseline_sha256":hashlib.sha256(baseline).hexdigest(),"baseline_git_blob":git("-C",str(ROOT),"rev-parse",BASE+":"+baselinepath),"full_bytes_equal":True,"first_line_exemption":False,"checks_fails":0,"actual_frame_method":"MotionSystem.OnFrame(); Tick is not the inspected public method"}
  for row in receipts:row["cache_after_sha256"]=sha(cache/row["name"]);row["copied_after_sha256"]=sha(reference/row["name"]);assert row["cache_after_sha256"]==row["copied_after_sha256"]==row["expected_sha256"]
  save(raw/"reference_receipts.json",receipts)
  post=source_state();save(raw/"source_POST.json",post);assert pre==post
  after=freeze();save(raw/"freeze_POST.json",after);assert before==after
  result["status"]="PASS F9b.1 pin/reference/API only"
 except Exception as exc:result["error"]=clean(str(exc))
 finally:save(raw/"result.json",result);print(json.dumps(result,indent=2))
 return 0 if result["status"].startswith("PASS") else 1
if __name__=="__main__":raise SystemExit(main())
