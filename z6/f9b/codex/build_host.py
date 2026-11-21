"""F9b.2 exact authorized build and actual private-helper logic tests, no app runtime."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,os,shutil,subprocess,uuid
H=Path(__file__).resolve().parent;R=H.parents[2]
SOURCE=R/"m0/out/f9b-inputs-20261121T125037Z-fab35294/source"
REFERENCE=SOURCE.parent/"reference"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 attempt=datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8];out=R/"m0/out"/("f9b-host-build-"+attempt);out.mkdir();raw=H/"raw"/attempt;raw.mkdir(parents=True)
 result={"phase":"F9b.2","attempt":attempt,"status":"STOP","build_mode":"NON-ORIGINAL RestorePackagesConfig=true and pilot EventSourceSupport=true","app_executed":False,"commands":[]}
 env=os.environ.copy();sdk=Path.home()/".dotnet";env["DOTNET_ROOT"]=str(sdk);env["PATH"]=str(sdk)+os.pathsep+env["PATH"]
 if not env.get("PROCESSOR_ARCHITECTURE"):env["PROCESSOR_ARCHITECTURE"]="AMD64"
 def clean(s):return s.replace(str(R),"%WORKSPACE%").replace(str(Path.home()),"%USERPROFILE%").replace(os.environ["ProgramFiles"],"%ProgramFiles%")
 def run(label,args,cwd):
  original=out/(label+".original.log")
  with original.open("wb") as f:
   proc=subprocess.Popen([str(x) for x in args],cwd=cwd,env=env,stdout=f,stderr=subprocess.STDOUT);print(label+" PID="+str(proc.pid),flush=True);code=proc.wait()
  text=clean(original.read_text(encoding="utf-8",errors="replace")).replace("\r\n","\n");(raw/(label+".log")).write_text(text,encoding="utf-8",newline="\n")
  result["commands"].append({"label":label,"pid":proc.pid,"argv":[clean(str(x)) for x in args],"exit":code,"original_sha256":sha(original),"published_sha256":sha(raw/(label+".log"))});return code
 def state():
  return {"head":subprocess.check_output(["git","--no-optional-locks","-C",str(SOURCE),"rev-parse","HEAD"]).decode().strip(),"status":subprocess.check_output(["git","--no-optional-locks","-C",str(SOURCE),"status","--porcelain"]).decode()}
 try:
  result["source_PRE"]=state();assert result["source_PRE"]["head"]=="cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
  tests=out/"logic-tests";tests.mkdir()
  for n in ["Program.cs","LogicTests.csproj"]:shutil.copyfile(H/"tests"/n,tests/n)
  evidence=SOURCE/"src/Files.App/Views/FilesMotionEvidence.cs";shutil.copyfile(evidence,tests/evidence.name);assert sha(evidence)==sha(tests/evidence.name);result["tested_actual_evidence_sha256"]=sha(evidence)
  assert run("private_logic_tests",[sdk/"dotnet.exe","run","--project",str(tests/"LogicTests.csproj"),"-c","Release"],tests)==0,"Private logic tests failed"
  msbuild=Path(os.environ["ProgramFiles"])/"Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe"
  argv=[msbuild,"-restore","Files.slnx","-p:Configuration=Debug","-p:Platform=x64","-v:quiet","-clp:ErrorsOnly","-p:RestorePackagesConfig=true"]
  assert run("host_build",argv,SOURCE)==0,"Host build failed; no extra properties/fallback"
  result["source_POST"]=state();assert result["source_PRE"]==result["source_POST"],"Build modified tracked/source state"
  bin=SOURCE/"src/Files.App/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64"
  result["output_pack"]={n:{"reference_sha256":sha(REFERENCE/n),"output_sha256":sha(bin/n)} for n in ["FluidMotion.L1.dll","fluidmotion.dll","demo_motion.json"]}
  assert all(x["reference_sha256"]==x["output_sha256"] for x in result["output_pack"].values())
  runtime=json.loads((bin/"Files.runtimeconfig.json").read_text());supported=runtime["runtimeOptions"]["configProperties"]["System.Diagnostics.Tracing.EventSource.IsSupported"];assert supported is True;result["EventSource_IsSupported"]=supported
  (raw/"runtimeconfig.json").write_text(json.dumps(runtime,indent=2)+"\n",encoding="utf-8",newline="\n")
  tool=SOURCE.parent/"surface_tool/bin/Release/net10.0/SurfaceDump.dll";dump=out/"surface_host_output.txt"
  assert run("output_surface_check",[sdk/"dotnet.exe",tool,bin/"FluidMotion.L1.dll",dump],out)==0
  baseline=H/"raw/20261121T125037Z-fab35294/surface_reference.txt";assert dump.read_bytes()==baseline.read_bytes(),"Output full surface bytes differ"
  shutil.copyfile(dump,raw/dump.name);shutil.copyfile(dump.with_suffix(".checksurface.txt"),raw/"surface_host_output.checksurface.txt")
  result["full_output_reference_surface_equal"]=True;result["output_surface_sha256"]=sha(dump)
  result["host_artifacts"]={n:sha(bin/n) for n in ["Files.exe","Files.dll","AppxManifest.xml","Files.App.build.appxrecipe"]}
  result["status"]="PASS F9b.2 build/private logic/output API only"
 except Exception as exc:result["error"]=clean(str(exc))
 finally:
  (raw/"result.json").write_text(json.dumps(result,indent=2)+"\n",encoding="utf-8",newline="\n");print(json.dumps(result,indent=2))
 return 0 if result["status"].startswith("PASS") else 1
if __name__=="__main__":raise SystemExit(main())
