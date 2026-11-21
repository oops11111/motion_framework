"""One manually assembled FilesDev package diagnostic. Default dryrun; primary M instruction required."""
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
LRECEIPT=HERE/"raw/f9a-devlayout-20261121T115207Z-c7612b64"
GRANT="F9a FilesDev selected-project deployment, ordinary startup, exact cleanup"
INVENTORY_SHA="0bfdf5bbae22fc3734ba9987be2ecd10f7e64b02f0cddccd8aa912130a7f7f43"
DESTINATION_SHA="2d910d3a01c751fa7a4bd00e4578e1455d36936c9438922b772a60463bd15fbd"
MANIFEST_SHA="ad4ee744e9e3b82875b656f3da15754003d673987b978c7c411af4d7c7c890b4"

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
    if not args.authority or not args.instruction: raise RuntimeError("Existing same-FilesDev authority context AND new primary M execution instruction required")
    a=json.loads(Path(args.authority).read_text(encoding="utf-8-sig")); i=json.loads(Path(args.instruction).read_text(encoding="utf-8-sig"))
    assert a["grant_kind"]==GRANT and a["source"]=="Direct user response in current root conversation" and a["user_verbatim"].strip() and a["recorded_utc"]
    assert a["manifest_sha256"]==MANIFEST_SHA
    assert i["instruction_kind"]=="Primary GateM execute" and i["root_verbatim"].strip() and i["authority_sha256"].lower()==sha(Path(args.authority))
    assert i["inventory_sha256"]==INVENTORY_SHA and i["destination_hashes_sha256"]==DESTINATION_SHA and i["manifest_sha256"]==MANIFEST_SHA

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

def verify_layout(layout):
    import xml.etree.ElementTree as ET
    from prepare_layout import safe_ancestors,dest_parts,source_info,SOURCE,NUGET
    assert sha(LRECEIPT/"inventory.json")==INVENTORY_SHA and sha(LRECEIPT/"destination_hashes.json")==DESTINATION_SHA
    rows=json.loads((LRECEIPT/"inventory.json").read_text(encoding="utf-8"));listed=set()
    safe_ancestors(layout)
    for row in rows:
        dst=layout.joinpath(*dest_parts(row["destination"]));safe_ancestors(dst)
        assert dst.resolve().is_relative_to(layout.resolve()) and sha(dst)==row["sha256"] and dst.stat().st_size==row["bytes"],"Layout drift"
        token=row["source"];src=(SOURCE/token[len("%BUILDROOT%/"):]) if token.startswith("%BUILDROOT%/") else NUGET/token[len("%NUGET%/"):]
        assert source_info(src)["sha256"]==row["sha256"],"Bound source drift"
        listed.add(dst.relative_to(layout).as_posix())
    assert {f.relative_to(layout).as_posix() for f in layout.rglob("*") if f.is_file()}==listed,"Unlisted layout file"
    for f in layout.rglob("*"):safe_ancestors(f)
    assert sha(layout/"AppxManifest.xml")==MANIFEST_SHA
    manifest=ET.parse(layout/"AppxManifest.xml").getroot();identity=next(e.attrib for e in manifest if e.tag.split("}")[-1]=="Identity")
    assert identity=={"Name":"FilesDev","Publisher":"CN=Files","Version":"4.3.0.0","ProcessorArchitecture":"x64"}
    return {"files":len(rows),"inventory_sha256":INVENTORY_SHA,"destination_hashes_sha256":DESTINATION_SHA,"manifest_sha256":MANIFEST_SHA}

def execute(args,binding,source,layout):
    attempt=ROOT/"m0/out"/("f9a-gateM-"+datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")+"-"+uuid.uuid4().hex[:8]);attempt.mkdir()
    result={"status":"not accepted","attempt":str(attempt),"cleanup_complete":False};owned=None;native=None;handle=None;all_pre=None;registration_started=False
    def record_all(label):
        rows=all_packages();path=attempt/("all-packages-"+label+".json");save(path,rows);result["all_packages_"+label+"_sha256"]=sha(path);return rows
    def delta(before,after):
        excluded={owned["PackageFullName"]} if owned else set()
        return {"added":sorted(set(after)-set(before)-excluded),"removed":sorted(set(before)-set(after)-excluded)}
    try:
        assert source.is_relative_to((ROOT/"m0/out").resolve()) and layout.is_relative_to((ROOT/"m0/out").resolve())
        result["layout_PRE"]=verify_layout(layout)
        pre_source=source_state(source);result["source_PRE"]=pre_source
        assert pre_source["head"]==binding["source_pin"] and not pre_source["status"] and not pre_source["tracked_diff"],"Source HEAD/pin/clean changed"
        result["PRE"]=as_list(packages());assert not result["PRE"],"Existing FilesDev package: STOP"
        residual=list((Path(os.environ["LOCALAPPDATA"])/"Packages").glob("FilesDev_*"));assert not residual,"Existing FilesDev data: STOP"
        result["runtime_PRE"]=frameworks()
        assert any(r["Architecture"]==9 and r["Status"]==0 and tuple(map(int,r["Version"].split(".")))>=(2,5,1,0) and r["Publisher"].startswith("CN=Microsoft Corporation,") for r in result["runtime_PRE"]),"Runtime prerequisite changed"
        assert not ps("@(Get-Process devenv -ErrorAction SilentlyContinue|Select Id)|ConvertTo-Json"),"Existing VS: STOP"
        from native_lifecycle import Native
        pre_native=Native()
        candidates=as_list(ps("@(Get-Process -Name Files,Files.App.Server -ErrorAction SilentlyContinue|Select Id)|ConvertTo-Json"))
        for row in candidates:
            identity=pre_native.inspect_package(row["Id"])
            assert identity["state"]!="unknown","Existing candidate process identity unknown"
            assert not (identity["state"]=="known_package" and identity["package_fullname"].startswith("FilesDev_")),"Existing FilesDev process"
        result["state_PRE"]=association_state();all_pre=record_all("PRE")
        argv=["powershell.exe","-NoProfile","-Command","$ErrorActionPreference='Stop'; Add-AppxPackage -Register "+literal(layout/"AppxManifest.xml")]
        registration_started=True
        with (attempt/"registration-stdout.log").open("wb") as out,(attempt/"registration-stderr.log").open("wb") as err:
            proc=subprocess.Popen(argv,stdout=out,stderr=err);save(attempt/"registration-process.json",{"pid":proc.pid,"argv":argv,"start_utc":datetime.now(timezone.utc).isoformat()});print("REGISTER_PID="+str(proc.pid),flush=True)
            result["registration_exit"]=proc.wait()
        post=as_list(packages());save(attempt/"package-post-deploy.json",post)
        if len(post)==1:
            pkg=post[0]
            assert pkg["Name"]=="FilesDev" and pkg["Version"]=="4.3.0.0" and pkg["Publisher"]=="CN=Files" and pkg["Architecture"]==9 and Path(pkg["InstallLocation"]).resolve()==layout.resolve()
            owned=pkg
        result["other_package_delta_after_deploy"]=delta(all_pre,record_all("AFTER_DEPLOY"))
        assert not any(result["other_package_delta_after_deploy"].values()),"Non-FilesDev package delta observed; cause not assigned, stop activation"
        assert result["registration_exit"]==0 and owned,"Registration failed; no fallback"
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
            if registration_started and owned is None:
                partial=as_list(packages())
                if partial:
                    assert len(partial)==1 and partial[0]["Name"]=="FilesDev" and partial[0]["Version"]=="4.3.0.0" and partial[0]["Publisher"]=="CN=Files" and partial[0]["Architecture"]==9 and Path(partial[0]["InstallLocation"]).resolve()==layout.resolve(),"Partial registration identity unknown"
                    owned=partial[0];result["partial_registration_owned"]=True
        except Exception as exc:result["partial_registration_capture_error"]=str(exc)
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
            result["layout_POST"]=verify_layout(layout)
            all_post=record_all("POST")
            if all_pre is not None:result["other_package_delta_final"]=delta(all_pre,all_post)
            result["cleanup_complete"]=bool(owned and result.get("removed_exact_fullname")==owned["PackageFullName"] and not result["POST"] and not result.get("cleanup_error") and not result.get("cleanup_stop"))
            before=normalized_state(result["state_PRE"]) if "state_PRE" in result else None;after=normalized_state(result["state_POST"])
            result["state_changed"]=before!=after if before is not None else None
            result["state_comparison_available"]=before is not None
            if result["state_changed"]:result["state_delta"]={"before":before,"after":after,"action":"STOP; no automatic repair"}
            source_ok=result["source_POST"]["head"]==binding["source_pin"] and not result["source_POST"]["status"] and not result["source_POST"]["tracked_diff"]
            complete=not result.get("error") and result["cleanup_complete"] and result.get("normal_close_exit")==0 and source_ok and result["state_comparison_available"] and not result["state_changed"] and not any(result.get("other_package_delta_final",{}).values())
            result["status"]="PASS (M manual layout main-page and exact cleanup)" if complete else "STOP (M incomplete; inspect actual residue/error)"
        except Exception as exc:result["post_or_cleanup_evidence_error"]=str(exc);result["status"]="STOP (cleanup/state unknown; report retained identity)"
        finally:
            result["owned_package"]=owned;save(attempt/"result.json",result);print(json.dumps(result,indent=2))

    return 0 if result["status"]=="PASS (M manual layout main-page and exact cleanup)" else 1

def main():
    ap=argparse.ArgumentParser();ap.add_argument("--execute",action="store_true");ap.add_argument("--authority");ap.add_argument("--instruction");args=ap.parse_args()
    binding=json.loads((LRECEIPT/"result.json").read_text(encoding="utf-8"));assert binding["status"]=="PASS L workspace byte-copy only"
    binding["source_pin"]=binding["source_PRE"]["head"]
    source=(ROOT/"m0/out/f9a-vsdeploy-preparation-20261121T105022Z-ff07d274-gateA/source").resolve();layout=(ROOT/binding["layout_relative"]).resolve()
    if not args.execute:
        print(json.dumps({"mode":"dryrun; no OS/package/activation/UIA calls","runtime":"NOT TESTED","layout":binding["layout_relative"],"inventory_sha256":INVENTORY_SHA,"destination_hashes_sha256":DESTINATION_SHA,"manifest_sha256":MANIFEST_SHA,"registration":"Add-AppxPackage -Register <layout/AppxManifest.xml>; no other flags","identity":binding["identity"],"startup":"ordinary PFN!App, empty args, SettingsButton visible/enabled","cleanup":"normal WM_CLOSE, then exact newly captured fullname only after no owned/unknown processes"},indent=2));return
    authority(args,binding) # Real current root M instruction bound to existing grant and L hashes BEFORE OS calls.
    raise SystemExit(execute(args,binding,source,layout))

if __name__=="__main__":main()
