"""F9b manual-layout runtime driver. Default dryrun; actual runtime requires separate primary release."""
from pathlib import Path
from datetime import datetime, timezone
import argparse
import hashlib
import json
import os
import subprocess
import time
import uuid

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BINDING=HERE/"RUNTIME_BINDING_v2.json"
AUTH=HERE/"RUNTIME_AUTHORITY_CONTEXT.json"
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def literal(value): return "'" + str(value).replace("'", "''") + "'"
def ps(code):
    r = subprocess.run(["powershell.exe", "-NoProfile", "-Command", "$ErrorActionPreference='Stop';" + code], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if r.returncode: raise RuntimeError(r.stderr.decode("utf-8", errors="replace"))
    return json.loads(r.stdout.decode("utf-8-sig")) if r.stdout.strip() else None

def packages():
    return ps("@(Get-AppxPackage -Name FilesDev|ForEach-Object{[ordered]@{Name=[string]$_.Name;PackageFullName=[string]$_.PackageFullName;PackageFamilyName=[string]$_.PackageFamilyName;Version=$_.Version.ToString();Architecture=[int]$_.Architecture;Publisher=[string]$_.Publisher;InstallLocation=[string]$_.InstallLocation;Status=[int]$_.Status}})|ConvertTo-Json -Depth 4") or []
def all_packages():
    return as_list(ps("@(Get-AppxPackage|ForEach-Object{[string]$_.PackageFullName}|Sort-Object -Unique)|ConvertTo-Json"))
def frameworks():
    return as_list(ps("@(Get-AppxPackage -Name Microsoft.WindowsAppRuntime.2|ForEach-Object{[ordered]@{Name=[string]$_.Name;Version=$_.Version.ToString();Architecture=[int]$_.Architecture;Publisher=[string]$_.Publisher;Status=[int]$_.Status}})|ConvertTo-Json"))
def as_list(value): return value if isinstance(value,list) else [value] if value else []
def save(path, value): path.write_text(json.dumps(value, indent=2)+"\n", encoding="utf-8", newline="\n")
def source_state(source):
    def git(*args):return subprocess.check_output(["git","--no-optional-locks","-C",str(source),*args]).decode().strip()
    return {"head":git("rev-parse","HEAD"),"status":subprocess.check_output(["git","--no-optional-locks","-C",str(source),"status","--porcelain"]).decode().rstrip("\r\n"),"tracked_diff":git("diff","HEAD","--")}

def association_state():
    return ps("@{associations=@('.zip','.7z','.rar','.tar','.jar','.mrpack','.gz')|ForEach-Object{$p='HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\'+$_+'\\UserChoice';@{ext=$_;value=if(Test-Path -LiteralPath $p){Get-ItemProperty -LiteralPath $p|Select ProgId,Hash}else{$null}}};protocol=if(Test-Path -LiteralPath 'HKCU:\\Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\files-dev\\UserChoice'){Get-ItemProperty -LiteralPath 'HKCU:\\Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\files-dev\\UserChoice'|Select ProgId,Hash}else{$null}}|ConvertTo-Json -Depth 6")

def normalized_state(state):
    def choice(value):
        if value is None:return None
        return {"ProgId":(value.get("ProgId") or "").strip().casefold(),"Hash":(value.get("Hash") or "").strip()}
    return {"associations":sorted([{"ext":r["ext"].casefold(),"value":choice(r.get("value"))} for r in as_list(state.get("associations"))],key=lambda r:r["ext"]),"protocol":choice(state.get("protocol"))}

def guard(args,binding):
    context=json.loads(Path(args.authority).read_text(encoding="utf-8-sig"));instruction=json.loads(Path(args.instruction).read_text(encoding="utf-8-sig"))
    kinds=["Primary F9b runtime preparation only","Primary F9b runtime execute"] if args.validate_only else ["Primary F9b runtime execute"]
    assert instruction["instruction_kind"] in kinds and instruction["root_verbatim"]
    assert instruction["authority_sha256"]==sha(Path(args.authority)) and instruction["binding_sha256"]==sha(BINDING)
    assert context["original_authorization_sha256"]==sha(HERE/"AUTHORIZATION.json")
    for key in ["inventory_sha256","destination_hashes_sha256","manifest_sha256","host_patch_sha256"]:assert instruction[key]==binding[key]
    for name,h in binding["code_hashes"].items():assert sha(ROOT/name)==h,"Frozen runtime code changed"

def validate_helpers(args):
    for script,extra in [("ActivateNormally.ps1",["-AppUserModelId","FilesDev_ykqwq8d6ps0ag!App","-StatePath",str(ROOT/"m0/out/f9b-validation-unused.json")]),("RuntimeProbe.ps1",[])]:
        r=subprocess.run(["powershell.exe","-NoProfile","-File",str(HERE/script),"-ValidateOnly","-AuthorityFile",args.authority,"-InstructionFile",args.instruction,"-BindingFile",str(BINDING),*extra],stdout=subprocess.PIPE,stderr=subprocess.PIPE)
        assert r.returncode==0,r.stderr.decode("utf-8",errors="replace")
        print(r.stdout.decode("utf-8-sig"),flush=True)

def verify_layout(binding,source,layout):
    from prepare_runtime_layout import safe_ancestors,dest_parts,source_info
    receipt=ROOT/binding["layout_receipt_relative"];assert sha(receipt/"inventory.json")==binding["inventory_sha256"] and sha(receipt/"destination_hashes.json")==binding["destination_hashes_sha256"]
    rows=json.loads((receipt/"inventory.json").read_text());listed=set();safe_ancestors(layout)
    for row in rows:
        path=layout.joinpath(*dest_parts(row["destination"]));safe_ancestors(path);assert sha(path)==row["sha256"] and path.stat().st_size==row["bytes"]
        token=row["source"]
        if token.startswith("%BUILDROOT%/"):original=source/token[len("%BUILDROOT%/"):]
        elif token.startswith("%NUGET%/"):original=Path.home()/".nuget/packages"/token[len("%NUGET%/"):]
        else:assert token.startswith("%VERIFIED_REFERENCE%/");original=source.parent/"reference"/token[len("%VERIFIED_REFERENCE%/"):]
        assert source_info(original)["sha256"]==row["sha256"],"Bound source changed"
        listed.add(path.relative_to(layout).as_posix())
    assert {f.relative_to(layout).as_posix() for f in layout.rglob("*") if f.is_file()}==listed
    for path in layout.rglob("*"):safe_ancestors(path)
    assert sha(layout/"AppxManifest.xml")==binding["manifest_sha256"] and sha(layout/"Files.dll")==binding["host_managed_sha256"]
    for row in json.loads((HERE/"host_source_v4.json").read_text()):assert sha(source/row["path"])==row["post_sha256"]
    assert sha(HERE/"files_f9b_pilot_v4.patch")==binding["host_patch_sha256"]
    return {"files":len(rows),"inventory_sha256":binding["inventory_sha256"],"destination_hashes_sha256":binding["destination_hashes_sha256"]}

def execute(args,binding):
    source=ROOT/binding["source_relative"];layout=ROOT/binding["layout_relative"]
    attempt=ROOT/"m0/out"/("f9b-runtime-"+datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8]);attempt.mkdir()
    result={"status":"STOP","attempt":str(attempt),"route":"F9b manual NON-ORIGINAL","cleanup_complete":False};owned=None;native=None;handle=None;pid=None;original_pointer=None;all_pre=None;step=0;registered=False
    def record_all(label):
        values=all_packages();path=attempt/("all-packages-"+label+".json");save(path,values);result["all_packages_"+label+"_sha256"]=sha(path);return values
    def delta(before,after):
        exclude={owned["PackageFullName"]} if owned else set()
        return {"added":sorted(set(after)-set(before)-exclude),"removed":sorted(set(before)-set(after)-exclude)}
    def probe(action,**fields):
        nonlocal step
        step+=1;argv=["powershell.exe","-NoProfile","-File",str(HERE/"RuntimeProbe.ps1"),"-Execute","-Action",action,"-AuthorityFile",args.authority,"-InstructionFile",args.instruction,"-BindingFile",str(BINDING)]
        if pid and owned:argv += ["-OwnedPid",str(pid),"-OwnedExe",str(layout/"Files.exe"),"-OwnedPackage",owned["PackageFullName"],"-OwnedHwnd",result.get("hwnd","0")]
        for key,value in fields.items():argv += ["-"+key,str(value)]
        r=subprocess.run(argv,stdout=subprocess.PIPE,stderr=subprocess.PIPE);(attempt/("probe-"+str(step)+"-"+action+".stdout")).write_bytes(r.stdout);(attempt/("probe-"+str(step)+"-"+action+".stderr")).write_bytes(r.stderr)
        assert r.returncode==0,"Probe "+action+" failed: "+r.stderr.decode("utf-8",errors="replace")
        return json.loads(r.stdout.decode("utf-8-sig"))
    def log_path():return Path(os.environ["LOCALAPPDATA"])/"Packages"/owned["PackageFamilyName"]/"LocalState"/("f9b_motion_"+str(pid)+".jsonl")
    def log_text():return log_path().read_text(encoding="utf-8-sig")
    def wait_idle():
        for _ in range(80):
            text=log_text();assert "STOP" not in text,"Pilot STOP marker"
            lines=[x for x in text.splitlines() if x.startswith("idle ")]
            if lines and lines[-1]=="idle active=0 pump=0":return
            time.sleep(.05) # Orchestration only, never a timing result.
        raise RuntimeError("Pilot did not reach observed idle")
    def capture_log(validate=True):
        if not pid:return False
        path=log_path()
        try:path.stat()
        except FileNotFoundError:
            result["pilot_log_absent"]=True
            if validate:raise RuntimeError("Pilot log was never created")
            return False
        data=path.read_bytes();dest=attempt/"pilot.original.jsonl";dest.write_bytes(data);assert dest.read_bytes()==path.read_bytes()
        result["pilot_log_original_sha256"]=sha(dest);result["pilot_log_captured_before_remove"]=True
        try:
            text=data.decode("utf-8-sig");summaries=[json.loads(x[len("dispose "):]) for x in text.splitlines() if x.startswith("dispose ")]
            assert summaries,"Final Dispose summary missing";summary=summaries[-1];result["motion_summary"]=summary
            expected={"AnimationLifecycle","FrameTick","IdleTransition","Retarget","TokenLoad"};counts=summary["eventCounts"];states=summary["lifecycleCounts"]
            assert expected.issubset(counts) and counts["FrameTick"]>0
            assert states.get("Created",0)==states.get("Released",0)>0 and states.get("Played",0)>0 and states.get("Cancelled",0)>0
            assert summary["lastIdleActive"]==summary["lastIdlePump"]==0 and summary["CreatedEqualsReleased"]
            assert "tokens loaded; entry consumes demo.enter" in text and "rendering=False originalVisualRestored=true" in text and "STOP" not in text
            result["lifecycle_acceptance"]=True;return True
        except Exception as error:
            result["lifecycle_acceptance_error"]=str(error)
            if validate:raise
            return False
    try:
        result["layout_PRE"]=verify_layout(binding,source,layout);result["source_PRE"]=source_state(source)
        assert result["source_PRE"]["head"]=="cbfb4ba54a89632fbceb82ea94ada60a9bae3737"
        assert {x[3:] for x in result["source_PRE"]["status"].splitlines()}=={x["path"] for x in json.loads((HERE/"host_source_v4.json").read_text())}
        result["PRE"]=as_list(packages());assert not result["PRE"]
        assert not list((Path(os.environ["LOCALAPPDATA"])/"Packages").glob("FilesDev_*"))
        result["runtime_PRE"]=frameworks();assert any(r["Architecture"]==9 and r["Status"]==0 and tuple(map(int,r["Version"].split(".")))>=(2,5,1,0) and r["Publisher"].startswith("CN=Microsoft Corporation,") for r in result["runtime_PRE"])
        from native_lifecycle import Native
        native=Native()
        for row in as_list(ps("@(Get-Process -Name Files,Files.App.Server -ErrorAction SilentlyContinue|Select Id)|ConvertTo-Json")):
            identity=native.inspect_package(row["Id"]);assert identity["state"]!="unknown" and not(identity["state"]=="known_package" and identity["package_fullname"].startswith("FilesDev_"))
        assert not ps("@(Get-Process -Name devenv -ErrorAction SilentlyContinue|Select Id)|ConvertTo-Json"),"Existing VS process"
        result["state_PRE"]=association_state();all_pre=record_all("PRE")
        argv=["powershell.exe","-NoProfile","-Command","$ErrorActionPreference='Stop';Add-AppxPackage -Register "+literal(layout/"AppxManifest.xml")]
        registered=True
        with (attempt/"register.stdout").open("wb") as out,(attempt/"register.stderr").open("wb") as err:
            proc=subprocess.Popen(argv,stdout=out,stderr=err);save(attempt/"registration-process.json",{"pid":proc.pid,"argv":argv});print("REGISTER_PID="+str(proc.pid)+" ATTEMPT="+attempt.name,flush=True);result["registration_exit"]=proc.wait()
        post=as_list(packages());assert len(post)==1
        candidate=post[0];assert candidate["Name"]=="FilesDev" and candidate["Version"]=="4.3.0.0" and candidate["Publisher"]=="CN=Files" and candidate["Architecture"]==9 and Path(candidate["InstallLocation"]).resolve()==layout.resolve()
        owned=candidate
        result["package_delta_after_registration"]=delta(all_pre,record_all("AFTER_REGISTER"));assert not any(result["package_delta_after_registration"].values()) and result["registration_exit"]==0
        activation=["powershell.exe","-NoProfile","-File",str(HERE/"ActivateNormally.ps1"),"-Execute","-AppUserModelId",owned["PackageFamilyName"]+"!App","-StatePath",str(attempt/"activation.json"),"-AuthorityFile",args.authority,"-InstructionFile",args.instruction,"-BindingFile",str(BINDING)]
        r=subprocess.run(activation,stdout=subprocess.PIPE,stderr=subprocess.PIPE);(attempt/"activation.stdout").write_bytes(r.stdout);(attempt/"activation.stderr").write_bytes(r.stderr);assert r.returncode==0,"Ordinary activation failed"
        pid=json.loads((attempt/"activation.json").read_text(encoding="utf-8-sig"))["pid"];handle=native.open_owned(pid,layout/"Files.exe",owned["PackageFullName"]);result["owned_pid"]=pid
        for _ in range(30):
            windows=native.window_details(pid);visible=[w for w in windows if w["visible"]]
            if visible:break
            time.sleep(.1)
        assert visible;result["hwnd"]=visible[0]["hwnd"];save(attempt/"windows.json",windows)
        geometry=None
        for _ in range(30):
            try:
                geometry=probe("Geometry");break
            except AssertionError as readiness_error:
                if "MainPage SettingsButton missing" not in str(readiness_error):raise
                time.sleep(.1)
        assert geometry and geometry["Exists"] and geometry["Visible"] and geometry["Enabled"]
        modules=probe("Modules");save(attempt/"module_metadata.json",modules);assert modules["Complete"]
        expected={"FluidMotion.L1.dll","fluidmotion.dll"};paths={Path(n).name:n for n in modules["Paths"]};assert set(paths)==expected
        result["loaded_modules"]={n:{"mapped_path":paths[n],"sha256":sha(Path(paths[n]))} for n in expected}
        for n in expected:assert Path(paths[n]).resolve()==(layout/n).resolve() and sha(Path(paths[n]))==sha(layout/n)
        original_pointer=probe("Cursor");save(attempt/"original-pointer.json",original_pointer)
        probe("Move",X=geometry["Window"]["Left"]+1,Y=int((geometry["Top"]+geometry["Bottom"])/2));wait_idle();base=probe("Geometry");scale=base["Dpi"]/96
        crop={"pid":pid,"hwnd":result["hwnd"],"package":owned["PackageFullName"],"settings":base,"left":max(base["Window"]["Left"],int(base["Left"]-96*scale)),"top":max(base["Window"]["Top"],int(base["Top"]-4*scale)),"right":min(base["Window"]["Right"],int(base["Right"]+4*scale)),"bottom":min(base["Window"]["Bottom"],int(base["Bottom"]+4*scale))};save(attempt/"crop.json",crop)
        def image(action,name):return probe(action,OutputPath=attempt/name,CropFile=attempt/"crop.json",CropSha=sha(attempt/"crop.json"))
        image("Capture","rest-before.png");frames=attempt/"entry";frames.mkdir();entry=image("EntryBurst","entry");save(attempt/"entry_geometry.json",entry);result["entry_position_changed"]=any(g["Left"]<base["Left"]-2*scale for g in as_list(entry));result["entry_visible_review_required"]=True
        wait_idle();image("Capture","rest-after-entry.png")
        probe("Move",X=int((base["Left"]+base["Right"])/2),Y=int((base["Top"]+base["Bottom"])/2));time.sleep(.1);wait_idle();hover=probe("Geometry");assert hover["Left"]>base["Left"]+2*scale;image("Capture","hover.png")
        probe("Move",X=base["Window"]["Left"]+1,Y=int((base["Top"]+base["Bottom"])/2));time.sleep(.1);wait_idle();rest=probe("Geometry");assert abs(rest["Left"]-base["Left"])<=2;image("Capture","rest-after-hover.png")
        save(attempt/"motion_geometry.json",{"base":base,"hover":hover,"rest":rest});result["visible_motion_evidence"]={"entry_position_changed":result["entry_position_changed"],"entry_visible_review_required":True,"hover_position_changed":True,"rest_position_restored":True,"images":[str(f.relative_to(attempt)) for f in attempt.rglob("*.png")],"primary_visual_review":"required; no performance conclusion"}
        result["token_input_sha256"]=sha(layout/"demo_motion.json")
        result["normal_close_exit"]=native.close_owned(handle,pid);assert result["normal_close_exit"]==0;capture_log()
    except Exception as error:result["error"]=str(error)
    finally:
        try:
            if registered and owned is None:
                partial=as_list(packages())
                if partial:
                    assert len(partial)==1
                    candidate=partial[0]
                    assert candidate["Name"]=="FilesDev" and candidate["Version"]=="4.3.0.0" and candidate["Publisher"]=="CN=Files" and candidate["Architecture"]==9 and Path(candidate["InstallLocation"]).resolve()==layout.resolve(),"Partial registration identity unknown"
                    owned=candidate;result["partial_registration_owned"]=True
        except Exception as error:result["partial_registration_capture_error"]=str(error)
        try:
            if original_pointer:result["pointer_restore"]=probe("RestorePointer",OriginalPointerFile=attempt/"original-pointer.json",OriginalPointerSha=sha(attempt/"original-pointer.json"))
        except Exception as error:result["pointer_restore_error"]=str(error)
        try:
            if native and handle:
                if native.k.WaitForSingleObject(handle,0)==258:result["failure_cleanup_normal_close_exit"]=native.close_owned(handle,pid)
                try:
                    if pid and native.k.WaitForSingleObject(handle,0)==0 and not result.get("pilot_log_captured_before_remove"):capture_log(validate=False)
                finally:native.k.CloseHandle(handle);handle=None
            if owned:
                live=[]
                for _ in range(10):
                    live=[]
                    for row in as_list(ps("@(Get-Process -Name Files,Files.App.Server -ErrorAction SilentlyContinue|Select Id)|ConvertTo-Json")):
                        identity=native.inspect_package(row["Id"])
                        if identity["state"]=="unknown" or(identity["state"]=="known_package" and identity["package_fullname"]==owned["PackageFullName"]):live.append({**row,"identity":identity})
                    if not live:break
                    time.sleep(.1)
                if live:result["cleanup_stop"]={"live_or_unknown":live}
                elif pid and not result.get("pilot_log_captured_before_remove") and not result.get("pilot_log_absent"):result["cleanup_stop"]="Unique pilot log not captured; retain package"
                else:
                    now=as_list(packages());assert len(now)==1 and now[0]["PackageFullName"]==owned["PackageFullName"]
                    ps("Remove-AppxPackage -Package "+literal(owned["PackageFullName"]));result["removed_exact_fullname"]=owned["PackageFullName"]
        except Exception as error:result["cleanup_error"]=str(error)
        try:
            result["POST"]=as_list(packages());result["state_POST"]=association_state();result["source_POST"]=source_state(source);result["layout_POST"]=verify_layout(binding,source,layout)
            result["package_delta_final"]=delta(all_pre,record_all("POST")) if all_pre is not None else None
            result["state_changed"]=normalized_state(result["state_PRE"])!=normalized_state(result["state_POST"]) if "state_PRE" in result else None
            result["cleanup_complete"]=bool(owned and result.get("removed_exact_fullname")==owned["PackageFullName"] and not result["POST"] and not result.get("cleanup_error") and not result.get("cleanup_stop"))
            passed=not result.get("error") and result["cleanup_complete"] and result.get("lifecycle_acceptance") and result.get("normal_close_exit")==0 and result["state_changed"] is False and result["source_PRE"]==result["source_POST"] and not any(result["package_delta_final"].values()) and not result.get("pointer_restore_error")
            result["status"]="PASS F9b local motion evidence pending primary image review" if passed else "STOP F9b runtime incomplete"
        except Exception as error:result["post_evidence_error"]=str(error)
        finally:result["owned_package"]=owned;save(attempt/"result.json",result);print(json.dumps({"status":result["status"],"attempt":str(attempt),"error":result.get("error"),"cleanup_complete":result["cleanup_complete"]},indent=2))
    return 0 if result["status"].startswith("PASS") else 1

def main():
    ap=argparse.ArgumentParser();ap.add_argument("--execute",action="store_true");ap.add_argument("--validate-only",action="store_true");ap.add_argument("--authority",default=str(AUTH));ap.add_argument("--instruction");args=ap.parse_args();binding=json.loads(BINDING.read_text(encoding="utf-8"))
    if not args.execute and not args.validate_only:
        print(json.dumps({"mode":"dryrun only","runtime":"NOT EXECUTED","binding":binding,"register":"Add-AppxPackage -Register exact layout/AppxManifest.xml; no other flags","motion":"entry12qualitativecroppedimages/hover/rest + actualmodulemetadata/token/lifecycle; no timing payload","cleanup":"normal close, full original log capture BEFORE exactRemove, pointer finally restored"},indent=2));return
    assert args.instruction;guard(args,binding)
    if args.validate_only:validate_helpers(args);return
    raise SystemExit(execute(args,binding))
if __name__=="__main__":main()
