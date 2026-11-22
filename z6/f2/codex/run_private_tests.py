"""Run unchanged actual private fixture against sources extracted from the delivered patch."""
from pathlib import Path
import hashlib,json,os,subprocess,tempfile
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
def main():
    work=ROOT/'m0/out';work.mkdir(parents=True,exist_ok=True);out=Path(tempfile.mkdtemp(prefix='f2-patch-tests-',dir=work));patch=(HERE/'fluenthub_f2_repair_v2.patch').read_text(encoding="utf-8");proof=json.loads((HERE/'raw/20261121T144839Z-f4ff7f0f/patch_roundtrip_v2.json').read_text(encoding="utf-8"));expected={x['path']:x['sha256'] for x in proof['files']}
    for name in ['RootRebuildState.cs','PilotEventCounts.cs']:
        path='src/FluentHub/Views/'+name;blocks=[b for b in patch.split('diff --git ') if ('+++ b/'+path+'\n') in b];assert len(blocks)==1;lines=blocks[0].splitlines();assert '--- /dev/null' in lines;start=next(i for i,x in enumerate(lines) if x.startswith('@@ '))+1;body=lines[start:];assert all(x.startswith('+') for x in body);data=('\r\n'.join(x[1:] for x in body)+'\r\n').encode();assert hashlib.sha256(data).hexdigest()==expected[path],name;(out/name).write_bytes(data)
    project=out/'tests';project.mkdir()
    (project/'Program.cs').write_bytes((HERE/'tests/Program.cs').read_bytes());(project/'RepairTests.csproj').write_bytes((HERE/'tests/RepairTests.csproj').read_bytes());env=os.environ.copy()
    if not env.get('PROCESSOR_ARCHITECTURE'):env['PROCESSOR_ARCHITECTURE']='AMD64'
    r=subprocess.run(['dotnet','run','--project',str(project/'RepairTests.csproj'),'-p:F2Source='+str(out)],env=env,capture_output=True);(out/'tests.stdout.log').write_bytes(r.stdout);(out/'tests.stderr.log').write_bytes(r.stderr);print(r.stdout.decode('utf-8',errors='replace'));print(r.stderr.decode('utf-8',errors='replace'));assert r.returncode==0 and b'PASS 18 checks' in r.stdout;print('PASS actual delivered patch source SHA and 18 real private checks; no GUI')
if __name__=='__main__':main()
