"""Bound F2 observations and byte-preserving log capture; no calls on import."""
from pathlib import Path
import hashlib,json,os,re,subprocess,time
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
AUTH=HERE/'RUNTIME_AUTHORITY_CONTEXT.json'
BINDING=HERE/'RUNTIME_BINDING.json'
INSTRUCTION=HERE/'PRIMARY_RUNTIME_PREPARATION.json'
PS='powershell.exe'
IDENTITY='49462fluenthub-uwp.FluentHub'
PUBLISHER='CN=4E3C0825-4D00-4ECC-89F8-528E9200B125'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def literal(v):return "'"+str(v).replace("'","''")+"'"
def ps(code):
    env=dict(os.environ,PROCESSOR_ARCHITECTURE='AMD64')
    r=subprocess.run([PS,'-NoProfile','-Command',"$ErrorActionPreference='Stop';[Console]::OutputEncoding=New-Object System.Text.UTF8Encoding($false);"+code],env=env,capture_output=True)
    if r.returncode:raise RuntimeError(r.stderr.decode('utf-8-sig',errors='replace'))
    return r.stdout.decode('utf-8-sig').strip()
def write_json(path,value):path.write_bytes((json.dumps(value,ensure_ascii=False,indent=2)+'\n').encode())
def read_state(path):
    try:return json.loads(path.read_text(encoding='utf-8-sig'))
    except (OSError,json.JSONDecodeError):return None
def wait_for(predicate,description,seconds=30):
    until=time.monotonic()+seconds
    while time.monotonic()<until:
        value=predicate()
        if value:return value
        time.sleep(.1)
    raise RuntimeError('Observation timeout: '+description)
def packages():
    return json.loads(ps("$r=@(Get-AppxPackage -Name '"+IDENTITY+"'|ForEach-Object{[ordered]@{Name=[string]$_.Name;Publisher=[string]$_.Publisher;Version=$_.Version.ToString();Architecture=[int]$_.Architecture;Status=[int]$_.Status;PackageFullName=[string]$_.PackageFullName;PackageFamilyName=[string]$_.PackageFamilyName;InstallLocation=[string]$_.InstallLocation}});ConvertTo-Json -InputObject $r"))
def snapshot():return json.loads(ps('& '+literal(HERE/'ReadAnimationSettings.ps1')))
def capture_log(mode,path,attempt):
    data=path.read_bytes();original=attempt/(mode+'-pilot.original.log')
    if original.exists():assert original.read_bytes()==data and path.read_bytes()==data,'Original capture changed after exit'
    else:original.write_bytes(data)
    assert original.read_bytes()==data and path.read_bytes()==data
    text=data.decode('utf-8-sig');published=text.replace(str(ROOT),'%WORKSPACE%').replace(str(Path.home()),'%USERPROFILE%')
    out=attempt/(mode+'-pilot.redacted.log');out.write_bytes(published.replace('\r\n','\n').encode())
    write_json(attempt/(mode+'-capture.json'),{'original_sha256':sha(original),'original_bytes':len(data),'published_sha256':sha(out),'published_is_redacted_not_original':True,'copy_verified':True})
def lifecycle(text,real):
    assert 'F2 STOP' not in text
    assert re.findall(r'F2 listener created active=(\d+)',text)==['1','1']
    assert re.findall(r'F2 listener disposed active=(\d+)',text)==['0','0']
    markers=['F2 RootView Unloaded -> Dispose','F2 Dispose begin cycle=1','F2 Dispose end cycle=1','F2 listener disposed active=0','F2 RootView Unloaded Dispose complete','F2 old Dispose completed; queue new Create','F2 new RootView Create begin']
    positions=[text.index(x) for x in markers];assert positions==sorted(positions),'Dispose-before-Create contract'
    second=[m.start() for m in re.finditer('F2 listener created active=1',text)][1]
    loaded=[m.start() for m in re.finditer('F2 RootView Loaded -> Create NavMotionPilot',text)];assert len(loaded)==2 and positions[-1]<loaded[1]<second
    summaries=re.findall(r'ETW_SUMMARY[^\n]*lifecycle\[([^]]*)\]',text);assert len(summaries)==2
    for summary in summaries:
        states=dict((k,int(v)) for k,v in re.findall(r'(\w+)=(\d+)',summary));assert states.get('Created',0)>0 and states.get('Created')==states.get('Released'),'per-cycle Created/Released'
    assert re.findall(r'ETW_EVENT name=IdleTransition id=5 activeCount=(\d+) pumpOn=(\d+)',text)[-1]==('0','0')
    assert sorted(re.findall(r'F5 subscriber attached cycle=(\d+)',text))==['1','2']
    assert sorted(re.findall(r'F5 subscriber detached cycle=(\d+)',text))==['1','2']
    assert text.count('F5 source disposed ')==1
    if real:
        assert len(re.findall(r'F5 hook installed .*ok=True',text))==1 and 'F5 source disposed hookRemoved=True' in text
    else:assert text.count('F5 fake source enabled')==1 and 'fake=2' in text
