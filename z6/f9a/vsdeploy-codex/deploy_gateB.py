"""One FilesDev selected-project test. Default dryrun; no grant file is supplied/generated."""
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
BINDING = HERE / "raw/20261121T105022Z-ff07d274-gateA/gateA_binding.json"
GRANT = "F9a FilesDev selected-project deployment, ordinary startup, exact cleanup"
ENTRY_SHA = "47D848F16FD4CD40410B67C7E4935FD90F69736579D09F6A99C9E2273EBBD04A"

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
def authority(args, binding):
    if not args.authority or not args.instruction: raise RuntimeError("New direct B authority AND primary execution instruction are required")
    a=json.loads(Path(args.authority).read_text(encoding="utf-8-sig")); i=json.loads(Path(args.instruction).read_text(encoding="utf-8-sig"))
    assert a["grant_kind"]==GRANT and a["source"]=="Direct user response in current root conversation" and a["user_verbatim"].strip() and a["recorded_utc"]
    assert a["manifest_sha256"]==binding["outputs"]["AppxManifest.xml"]["sha256"]
    assert i["instruction_kind"]=="Primary GateB execute" and i["root_verbatim"].strip() and i["authority_sha256"].lower()==sha(Path(args.authority))

def source_state(source):
    def git(*args):return subprocess.check_output(["git","--no-optional-locks","-C",str(source),*args]).decode().strip()
    return {"head":git("rev-parse","HEAD"),"status":git("status","--porcelain"),"tracked_diff":git("diff","HEAD","--")}

def association_state():
    return ps("@{associations=@('.zip','.7z','.rar','.tar','.jar','.mrpack','.gz')|ForEach-Object{$p='HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\'+$_+'\\UserChoice';@{ext=$_;value=if(Test-Path -LiteralPath $p){Get-ItemProperty -LiteralPath $p|Select ProgId,Hash}else{$null}}};protocol=if(Test-Path -LiteralPath 'HKCU:\\Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\files-dev\\UserChoice'){Get-ItemProperty -LiteralPath 'HKCU:\\Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\files-dev\\UserChoice'|Select ProgId,Hash}else{$null}}|ConvertTo-Json -Depth 6")

def normalized_state(state):
    def choice(value):
        if value is None:return None
        return {"ProgId":(value.get("ProgId") or "").strip().casefold(),"Hash":(value.get("Hash") or "").strip()}
    return {"associations":sorted([{"ext":r["ext"].casefold(),"value":choice(r.get("value"))} for r in as_list(state.get("associations"))],key=lambda r:r["ext"]),"protocol":choice(state.get("protocol"))}

def execute(args,binding,source,entry):
    attempt=ROOT/"m0/out"/("f9a-gateB-"+datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8]);attempt.mkdir()
    result={"status":"not accepted","attempt":str(attempt),"cleanup_complete":False};owned=None;native=None;handle=None;all_pre=None
    def record_all(label):
        rows=all_packages();path=attempt/("all-packages-"+label+".json");save(path,rows);result["all_packages_"+label+"_sha256"]=sha(path);return rows
    def delta(before,after):
        excluded={owned["PackageFullName"]} if owned else set()
        return {"added":sorted(set(after)-set(before)-excluded),"removed":sorted(set(before)-set(after)-excluded)}
    try:
        assert source.is_relative_to((ROOT/"m0/out").resolve()) and sha(entry).upper()==ENTRY_SHA
        for row in binding["outputs"].values():assert sha(source/row["path_from_source"])==row["sha256"]
        pre_source=source_state(source);result["source_PRE"]=pre_source
        assert pre_source["head"]==binding["source_pin"] and not pre_source["status"] and not pre_source["tracked_diff"],"Source HEAD/pin/clean changed"
        result["PRE"]=as_list(packages());assert not result["PRE"],"Existing FilesDev package: STOP"
        residual=list((Path(os.environ["LOCALAPPDATA"])/"Packages").glob("FilesDev_*"));assert not residual,"Existing FilesDev data: STOP"
        result["runtime_PRE"]=frameworks()
        assert any(r["Architecture"]==9 and r["Status"]==0 and tuple(map(int,r["Version"].split(".")))>=(2,5,1,0) and r["Publisher"].startswith("CN=Microsoft Corporation,") for r in result["runtime_PRE"]),"Runtime prerequisite changed"
        assert not ps("@(Get-Process devenv -ErrorAction SilentlyContinue|Select Id)|ConvertTo-Json"),"Existing VS: STOP"
        signature=ps("Import-Module (Join-Path $PSHOME 'Modules/Microsoft.PowerShell.Security/Microsoft.PowerShell.Security.psd1') -ErrorAction Stop; Get-AuthenticodeSignature -LiteralPath "+literal(entry)+"|Select @{n='status';e={$_.Status.ToString()}},@{n='signer';e={$_.SignerCertificate.Subject}}|ConvertTo-Json")
        assert signature["status"]=="Valid" and "CN=Microsoft Corporation" in signature["signer"]
        result["state_PRE"]=association_state();all_pre=record_all("PRE")
        env=os.environ.copy();env["DOTNET_ROOT"]=str(Path(os.environ["USERPROFILE"])/".dotnet");env["PATH"]=env["DOTNET_ROOT"]+os.pathsep+env["PATH"]
        if not env.get("PROCESSOR_ARCHITECTURE"):env["PROCESSOR_ARCHITECTURE"]="AMD64"
        argv=[str(entry),str(source/"Files.slnx"),"/Deploy","Debug|x64","/Project","src/Files.App/Files.App.csproj","/ProjectConfig","Debug|x64","/Out",str(attempt/"deploy.log")]
        with (attempt/"stdout.log").open("wb") as out,(attempt/"stderr.log").open("wb") as err:
            proc=subprocess.Popen(argv,cwd=source,env=env,stdout=out,stderr=err);save(attempt/"deployment-process.json",{"pid":proc.pid,"argv":argv,"start_utc":datetime.now(timezone.utc).isoformat()});print("DEPLOY_PID="+str(proc.pid),flush=True)
            result["deploy_exit"]=proc.wait() # Agent monitors exact handle; never timeout-kills.
        post=as_list(packages());save(attempt/"package-post-deploy.json",post)
        if len(post)==1:
            pkg=post[0]
            assert pkg["Name"]=="FilesDev" and pkg["Version"]=="4.3.0.0" and pkg["Publisher"]=="CN=Files" and pkg["Architecture"]==9
            owned=pkg
        result["other_package_delta_after_deploy"]=delta(all_pre,record_all("AFTER_DEPLOY"))
        assert not any(result["other_package_delta_after_deploy"].values()),"Non-FilesDev package delta observed; cause not assigned, stop activation"
        assert result["deploy_exit"]==0 and owned,"Deployment failed/unsupported; no fallback"
        request=["powershell.exe","-NoProfile","-File",str(HERE/"ActivateNormally.ps1"),"-AppUserModelId",owned["PackageFamilyName"]+"!App","-StatePath",str(attempt/"activation.json"),"-Execute","-AuthorityFile",args.authority,"-InstructionFile",args.instruction]
        r=subprocess.run(request,stdout=subprocess.PIPE,stderr=subprocess.PIPE);(attempt/"activation-stdout.log").write_bytes(r.stdout);(attempt/"activation-stderr.log").write_bytes(r.stderr);assert r.returncode==0,"Normal activation failed"
        state=json.loads((attempt/"activation.json").read_text(encoding="utf-8-sig"));pid=state["pid"]
        from native_lifecycle import Native
        native=Native();exe=(Path(owned["InstallLocation"])/"Files.exe").resolve();handle=native.open_owned(pid,exe,owned["PackageFullName"])
        result["deployed_files"]={name:{"path":str(Path(owned["InstallLocation"])/name),"sha256":sha(Path(owned["InstallLocation"])/name)} for name in ["Files.exe","Files.dll","AppxManifest.xml"]}
        result["identity_verified"]={"pid":pid,"exe":str(exe),"exe_sha256":sha(exe),"package":owned["PackageFullName"],"start_time":ps("Get-Process -Id "+str(pid)+"|Select Id,StartTime|ConvertTo-Json")}
        windows=[]
        for _ in range(30):
            assert native.k.WaitForSingleObject(handle,0)==258,"Owned app exited before visible window"
            windows=native.window_details(pid)
            if any(w["visible"] for w in windows):break
            time.sleep(1)
        save(attempt/"windows.json",windows);assert any(w["visible"] for w in windows),"No ordinary visible first window"
        visible=next(w for w in windows if w["visible"])
        check=subprocess.run(["powershell.exe","-NoProfile","-File",str(HERE/"WaitMainPage.ps1"),"-OwnedPid",str(pid),"-OwnedHwnd",visible["hwnd"],"-Execute"],stdout=subprocess.PIPE,stderr=subprocess.PIPE)
        (attempt/"mainpage-uia.json").write_bytes(check.stdout);(attempt/"mainpage-uia-stderr.log").write_bytes(check.stderr)
        assert check.returncode==0,"MainPage SettingsButton readiness failed; splash alone is insufficient"
        result["mainpage_ready"]=json.loads(check.stdout.decode("utf-8-sig"))
        result["normal_close_exit"]=native.close_owned(handle,pid);assert result["normal_close_exit"]==0
    except Exception as exc:
        result["error"]=str(exc)
    finally:
        try:
            if native and handle:
                try:
                    if native.k.WaitForSingleObject(handle,0)==258:
                        result["failure_cleanup_normal_close_exit"]=native.close_owned(handle,pid)
                except Exception as close_error:result["failure_cleanup_close_error"]=str(close_error)
                finally:native.k.CloseHandle(handle)
            if owned:
                from native_lifecycle import Native
                check=Native();owned_root=Path(owned["InstallLocation"]).resolve();live=[]
                for _ in range(10):
                    live=[]
                    candidates=as_list(ps("@(Get-Process -Name Files,Files.App.Server -ErrorAction SilentlyContinue|Select Id,Path,StartTime)|ConvertTo-Json"))
                    for row in candidates:
                        identity=check.inspect_package(row["Id"])
                        if identity["state"]=="unknown":live.append({**row,"identity":identity});continue
                        if identity["state"] in ["exited","no_package"]:continue
                        if identity["package_fullname"]!=owned["PackageFullName"]:continue # Positively known foreign package.
                        live.append({**row,"identity":identity})
                    if not live:break
                    time.sleep(1)
                if live:result["cleanup_stop"]={"reason":"Owned/uncertain process remains; report, no kill/removal","processes":live}
                else:
                    now=as_list(packages());assert len(now)==1 and now[0]["PackageFullName"]==owned["PackageFullName"],"Cleanup identity changed"
                    ps("Remove-AppxPackage -Package "+literal(owned["PackageFullName"]));result["removed_exact_fullname"]=owned["PackageFullName"]
        except Exception as exc:result["cleanup_error"]=str(exc)
        try:
            result["POST"]=as_list(packages());result["state_POST"]=association_state();result["runtime_POST"]=frameworks();result["source_POST"]=source_state(source)
            all_post=record_all("POST")
            if all_pre is not None:result["other_package_delta_final"]=delta(all_pre,all_post)
            result["cleanup_complete"]=bool(owned and result.get("removed_exact_fullname")==owned["PackageFullName"] and not result["POST"] and not result.get("cleanup_error") and not result.get("cleanup_stop"))
            before=normalized_state(result["state_PRE"]) if "state_PRE" in result else None;after=normalized_state(result["state_POST"])
            result["state_changed"]=before!=after if before is not None else None
            result["state_comparison_available"]=before is not None
            if result["state_changed"]:result["state_delta"]={"before":before,"after":after,"action":"STOP; no automatic repair"}
            source_ok=result["source_POST"]["head"]==binding["source_pin"] and not result["source_POST"]["status"] and not result["source_POST"]["tracked_diff"]
            complete=not result.get("error") and result["cleanup_complete"] and result.get("normal_close_exit")==0 and source_ok and result["state_comparison_available"] and not result["state_changed"] and not any(result.get("other_package_delta_final",{}).values())
            result["status"]="PASS (B ordinary first-window and exact cleanup)" if complete else "STOP (B incomplete; inspect actual residue/error)"
        except Exception as exc:result["post_or_cleanup_evidence_error"]=str(exc);result["status"]="STOP (cleanup/state unknown; report retained identity)"
        finally:
            result["owned_package"]=owned;save(attempt/"result.json",result);print(json.dumps(result,indent=2))

    return 0 if result["status"]=="PASS (B ordinary first-window and exact cleanup)" else 1

def main():
    ap=argparse.ArgumentParser();ap.add_argument("--execute",action="store_true");ap.add_argument("--authority");ap.add_argument("--instruction");args=ap.parse_args()
    binding=json.loads(BINDING.read_text(encoding="utf-8"));gateA=json.loads(BINDING.with_name("result.json").read_text(encoding="utf-8"));assert gateA["status"]=="PASS (NON-ORIGINAL build only)" and gateA["source_unchanged"]
    source=(ROOT/binding["source_dir"]).resolve();entry=Path(os.environ["ProgramFiles"])/"Microsoft Visual Studio/18/Community/Common7/IDE/devenv.exe"
    if not args.execute:
        print(json.dumps({"mode":"dryrun only; no OS/process/package/activation calls","runtime_status":"NOT TESTED","entry":"%ProgramFiles%/Microsoft Visual Studio/18/Community/Common7/IDE/devenv.exe","entry_sha256":ENTRY_SHA,"source_dir":binding["source_dir"],"argv":["<signed entry>","<fresh Files.slnx>","/Deploy","Debug|x64","/Project","src/Files.App/Files.App.csproj","/ProjectConfig","Debug|x64","/Out","<fresh deploy.log>"],"identity":binding["identity"],"cleanup":"Only exact newly captured fullname after ownership/exit checks; never force kill"},indent=2));return
    authority(args,binding) # Both new direct B grant AND bound primary instruction BEFORE any OS calls.
    raise SystemExit(execute(args,binding,source,entry))

if __name__=="__main__":main()
