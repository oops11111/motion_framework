"""F2 ordinary Fake/Real runner. Default dryrun; separate direct package grant required."""
from pathlib import Path
import argparse,hashlib,json,os,subprocess,time,uuid,xml.etree.ElementTree as ET
import run_host
from runtime_support import *
from native_lifecycle import Native

def all_packages():return json.loads(ps('$r=@(Get-AppxPackage|ForEach-Object{[string]$_.PackageFullName}|Sort-Object -Unique);ConvertTo-Json -InputObject $r'))
def residual():return json.loads(ps("$r=@(Get-ChildItem -LiteralPath ($env:LOCALAPPDATA+'\\Packages') -Directory -Filter '49462fluenthub-uwp.FluentHub_*'|Select-Object -ExpandProperty Name);ConvertTo-Json -InputObject $r"))
def candidates():return json.loads(ps("$r=@(Get-CimInstance Win32_Process -Filter \"Name='FluentHub.exe'\"|Select-Object -ExpandProperty ProcessId);ConvertTo-Json -InputObject $r"))
def assert_package(p,app):
    assert p['Name']==IDENTITY and p['Publisher']==PUBLISHER and p['Version']=='0.11.0.0' and p['Architecture']==9 and p['Status']==0
    assert Path(p['InstallLocation']).resolve()==app

def verify_bound(binding):
    for n,h in binding['code_hashes'].items():assert sha(ROOT/n)==h,n
    assert sha(HERE/'RUNTIME_LAYOUT.json')==binding['layout_sha256']
    layout=json.loads((HERE/'RUNTIME_LAYOUT.json').read_text());app=(ROOT/layout['root_relative']).resolve()
    raw_app=ROOT/layout['root_relative'];assert raw_app.absolute().is_relative_to((ROOT/'m0/out').absolute()) and app.is_relative_to((ROOT/'m0/out').resolve())
    def no_reparse(path):
        for part in [path,*path.parents]:
            assert not (os.lstat(part).st_file_attributes & 0x400),str(part)
    no_reparse(raw_app)
    actual={f.relative_to(app).as_posix() for f in app.rglob('*') if f.is_file()};assert actual=={r['path'] for r in layout['files']}
    for r in layout['files']:
        q=app/r['path'];no_reparse(q);assert not q.is_symlink() and not q.is_junction() and q.stat().st_size==r['bytes'] and sha(q)==r['sha256'],r['path']
    source=ROOT/binding['source_relative'];assert subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD']).decode().strip()==binding['source_pin']
    for r in binding['source_files']:assert sha(source/r['path'])==r['sha256'],r['path']
    allowed={r['path'] for r in binding['source_files']};status=subprocess.check_output(['git','-C',str(source),'status','--porcelain']).decode().splitlines()
    assert all(line[3:].replace('\\','/') in allowed for line in status),status
    manifest=ET.parse(app/'AppxManifest.xml').getroot();identity=manifest.find('{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Identity').attrib
    assert identity==binding['identity'] and sha(app/'AppxManifest.xml')==binding['manifest_sha256']
    return app

def guard(binding,instruction,authority,validate):
    assert instruction['authority_sha256']==sha(AUTH) and instruction['binding_sha256']==sha(BINDING)
    assert instruction['layout_sha256']==binding['layout_sha256'] and instruction['manifest_sha256']==binding['manifest_sha256']
    assert instruction['root_verbatim']
    assert instruction['instruction_kind'] in (['Primary F2 runtime preparation only','Primary F2 runtime execute'] if validate else ['Primary F2 runtime execute'])
    if not validate:assert authority['grant_status']=='granted' and authority['grant_kind']=='F2 temporary package runtime' and authority['source']=='direct user response in current root conversation' and authority['user_verbatim']

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--execute',action='store_true');parser.add_argument('--validate-only',action='store_true');parser.add_argument('--instruction',type=Path,default=HERE/'PRIMARY_RUNTIME_PREPARATION.json');args=parser.parse_args();assert not(args.execute and args.validate_only)
    binding=json.loads(BINDING.read_text());print(json.dumps({'mode':'execute' if args.execute else 'ValidateOnly' if args.validate_only else 'dryrun','identity':binding['identity'],'layout_sha256':binding['layout_sha256'],'host_dll_sha256':binding['host_dll_sha256'],'registration':'NOT_EXECUTED until direct F2 grant and bound primary instruction','modes':['Fake (--fm-pilot-f5=1)','Real (empty arguments)']},indent=2),flush=True)
    if not(args.execute or args.validate_only):return
    authority=json.loads(AUTH.read_text(encoding='utf-8-sig'));instruction=json.loads(args.instruction.read_text(encoding='utf-8-sig'));guard(binding,instruction,authority,args.validate_only)
    if args.validate_only:
        command='& '+literal(HERE/'ActivateNormally.ps1')+' -ValidateOnly -AppUserModelId '+literal(IDENTITY+'!App')+' -StatePath '+literal(ROOT/'m0/out/f2-validate-only-must-not-exist.json')+' -AuthorityFile '+literal(AUTH)+' -InstructionFile '+literal(args.instruction)+' -BindingFile '+literal(BINDING)
        print(ps(command));return
    app=verify_bound(binding);run_host.INSTRUCTION=args.instruction;native=Native();attempt=ROOT/'m0/out'/('f2-runtime-'+time.strftime('%Y%m%dT%H%M%S')+'-'+uuid.uuid4().hex[:8]);attempt.mkdir();result={'status':'STOP','attempt':attempt.name,'modes':[],'cleanup_complete':False};owned=None;registered=False;before=None;settings=None;copy_safe=True
    try:
        assert not packages(),'Existing F2 identity';assert not residual(),'Existing package data';assert not candidates(),'Existing FluentHub process';assert not (Path(os.environ['TEMP'])/'z6_f2_motion.log').exists(),'Existing TEMP log must not be overwritten'
        dep=json.loads(ps("$r=@(Get-AppxPackage -Name Microsoft.WindowsAppRuntime.2|ForEach-Object{[ordered]@{Version=$_.Version.ToString();Publisher=[string]$_.Publisher;Architecture=[int]$_.Architecture;Status=[int]$_.Status}});ConvertTo-Json -InputObject $r"));assert any(tuple(map(int,x['Version'].split('.'))) >=(2,0,1,0) and x['Architecture']==9 and x['Status']==0 and x['Publisher']=='CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US' for x in dep),'Framework missing/mismatch'
        before=all_packages();settings=snapshot();write_json(attempt/'PRE.json',{'all_package_fullnames':before,'settings':settings,'frameworks':dep})
        with (attempt/'register.stdout.log').open('wb') as out,(attempt/'register.stderr.log').open('wb') as err:
            proc=subprocess.Popen([PS,'-NoProfile','-Command',"$ErrorActionPreference='Stop';Add-AppxPackage -Register "+literal(app/'AppxManifest.xml')],stdout=out,stderr=err);result['register_pid']=proc.pid;print('REGISTER_PID='+str(proc.pid)+' attempt='+attempt.name,flush=True);result['register_exit']=proc.wait();registered=True
        assert result['register_exit']==0,'Registration failed'
        observed=packages();assert len(observed)==1;assert_package(observed[0],app);owned=observed[0];result['owned_package']=owned
        after=set(all_packages());added=after-set(before);removed=set(before)-after;result['package_delta_after_register']={'added':sorted(added),'removed':sorted(removed)};assert added=={owned['PackageFullName']} and not removed,'Unexpected package change before activation'
        for mode in ['Fake','Real']:
            result['modes'].append(run_host.run_host(mode,owned,app/'FluentHub.exe',attempt,native))
            # run_host returns only after normal exit0 and verified full raw capture.
            receipt=json.loads((attempt/(mode+'-capture.json')).read_text());assert receipt['copy_verified'] and sha(attempt/(mode+'-pilot.original.log'))==receipt['original_sha256']
        result['status']='PASS dev-only; ordered lifecycle and F5 B/C/E'
    except Exception as exc:result['error']=type(exc).__name__+': '+str(exc)
    finally:
        try:
            if registered and owned is None:
                partial=packages();result['partial_registration_capture']=partial
                if len(partial)==1:assert_package(partial[0],app);owned=partial[0];result['owned_package']=owned
                elif partial:raise RuntimeError('Ambiguous partial registration; retain')
            live=[]
            for pid in candidates():
                check=native.inspect_package(pid);live.append(check)
                if check['state']=='unknown' or (owned and check.get('package_fullname')==owned['PackageFullName']):raise RuntimeError('Live owned or unknown process; retain')
            result['cleanup_candidate_checks']=live
            # Existing fresh logs must already be preserved byte-for-byte before removal.
            if owned:
                for path in [Path(os.environ['LOCALAPPDATA'])/'Packages'/owned['PackageFamilyName']/'LocalState/z6_f2_motion.log',Path(os.environ['TEMP'])/'z6_f2_motion.log']:
                    if path.exists():
                        data=path.read_bytes();copies=list(attempt.glob('*-pilot.original.log'));assert any(q.read_bytes()==data for q in copies),'Existing log not safely captured; retain'
                current=packages();assert len(current)==1 and current[0]['PackageFullName']==owned['PackageFullName'],'Package changed before removal';assert_package(current[0],app)
                ps('Remove-AppxPackage -Package '+literal(owned['PackageFullName']))
            post=packages();data=residual();result['POST']=post;result['residual_POST']=data;result['TEMP_log_retained_POST']=(Path(os.environ['TEMP'])/'z6_f2_motion.log').exists();result['cleanup_complete']=not post and not data
            if before is not None:
                result['package_delta_final']=sorted(set(all_packages())^set(before));assert not result['package_delta_final'],'Unexpected final package delta'
            if settings is not None:
                result['settings_POST']=snapshot();result['settings_unchanged']=settings==result['settings_POST'];assert result['settings_unchanged'],'Animation state changed'
            verify_bound(binding);assert result['cleanup_complete'],'Cleanup incomplete'
        except Exception as exc:result['cleanup_error']=type(exc).__name__+': '+str(exc);result['status']='STOP'
        finally:write_json(attempt/'result.json',result);print(json.dumps(result,indent=2),flush=True)
    if not result['status'].startswith('PASS'):raise SystemExit(1)
if __name__=='__main__':main()
