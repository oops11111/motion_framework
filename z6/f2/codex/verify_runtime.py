"""F2 stage evidence/provenance integrity; does not claim CI executes GUI."""
from pathlib import Path
import argparse,hashlib,json,subprocess
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
BASE='d2aac2dc355d0412aa5fc3ede212b79faf4459b9'
def git(*args):return subprocess.check_output(['git','-C',str(ROOT),*args])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def data(p):
    b=p.read_bytes()
    return b if 'source_ledgers' in p.parts else b.replace(b'\r\n',b'\n')
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--enforce-stage-scope',action='store_true');args=parser.parse_args()
    entries={}
    for line in (HERE/'SHA256SUMS').read_text(encoding="utf-8").splitlines():
        h,n=line.split('  ',1);q=(HERE/n).resolve();assert q.is_relative_to(HERE) and q.is_file() and n not in entries;assert hashlib.sha256(data(q)).hexdigest()==h,n;assert q.stat().st_size<5_000_000;entries[n]=h
    actual={x.relative_to(HERE).as_posix() for x in HERE.rglob('*') if x.is_file() and x.name!='SHA256SUMS' and '__pycache__' not in x.parts and not any(k in x.parts for k in ['bin','obj'])};assert set(entries)==actual,(set(entries)^actual)
    protected=git('diff','--name-only',BASE,'--','code','MotionSoftDemo','pilot/l1pack').decode().splitlines();assert not protected,protected
    imports=json.loads((HERE/'IMPORT_PROVENANCE.json').read_text(encoding="utf-8"))
    def tree(ref):
        return {x.split(b'\t',1)[1].decode():x.split(b'\t',1)[0].split()[2].decode() for x in git('ls-tree','-r','-z',ref).split(b'\0') if x}
    head_tree=tree('HEAD');source_tree=tree('15d4108fd56eb661c1a46c6552ffe74a0318ff28')
    for item in imports:
        if item['path'] in ['z6/f5/codex/verify.py','z6/f5/codex/SHA256SUMS']:continue
        assert head_tree[item['path']]==item['source_blob']==source_tree[item['path']],item['path']
    main_ledger=git('show',BASE+':z6/LEDGER_snapshot.md');assert (ROOT/'z6/LEDGER_snapshot.md').read_bytes().replace(b'\r\n',b'\n').startswith(main_ledger.replace(b'\r\n',b'\n'))
    for label,ref in [('main',BASE),('integration','15d4108fd56eb661c1a46c6552ffe74a0318ff28')]:assert (HERE/'source_ledgers'/(label+'-'+ref+'.md')).read_bytes()==git('show',ref+':z6/LEDGER_snapshot.md')
    if args.enforce_stage_scope:
        changed=git('diff','--name-only',BASE).decode().splitlines()+git('ls-files','--others','--exclude-standard').decode().splitlines()
        historical={x['path'] for x in imports}
        exceptions={'z6/LEDGER_snapshot.md','.github/workflows/z6-f5.yml','.github/workflows/z6-f2-repair.yml','z6/f5/codex/verify.py','z6/f5/codex/SHA256SUMS'}
        for name in changed:assert name.startswith('z6/f2/codex/') or name in historical or name in exceptions,name
    binding=json.loads((HERE/'RUNTIME_BINDING.json').read_text(encoding="utf-8"));instruction=json.loads((HERE/'PRIMARY_RUNTIME_EXECUTE.json').read_text(encoding="utf-8"));authority=json.loads((HERE/'RUNTIME_AUTHORITY_CONTEXT.json').read_text(encoding="utf-8"));assert authority['grant_status']=='granted' and authority['grant_kind']=='F2 temporary package runtime';assert instruction['authority_sha256']==sha(HERE/'RUNTIME_AUTHORITY_CONTEXT.json') and instruction['binding_sha256']==sha(HERE/'RUNTIME_BINDING.json')
    for n,h in binding['code_hashes'].items():assert sha(ROOT/n)==h,n
    assert sha(HERE/'RUNTIME_LAYOUT.json')==binding['layout_sha256'];layout=json.loads((HERE/'RUNTIME_LAYOUT.json').read_text(encoding="utf-8"));assert len(layout['files'])==284
    raw=HERE/'raw/runtime_20261122T104327';result=json.loads((raw/'result.json').read_text(encoding="utf-8"));assert result['status'].startswith('PASS') and result['cleanup_complete'] and result['register_exit']==0;assert result['POST']==[] and result['residual_POST']==[] and not result['TEMP_log_retained_POST'] and result['package_delta_final']==[] and result['settings_unchanged'];assert result['package_delta_after_register']['removed']==[]
    assert [x['mode'] for x in result['modes']]==['Fake','Real'] and all(x['exit_code']==0 and x['managed_dll_sha256']==binding['host_dll_sha256'] for x in result['modes'])
    from runtime_support import lifecycle
    review=json.loads((HERE/'PRIMARY_RUNTIME_ACCEPTANCE.json').read_text(encoding="utf-8"))
    for mode in ['Fake','Real']:
        lifecycle((raw/(mode+'-pilot.redacted.log')).read_text(encoding='utf-8-sig'),mode=='Real');capture=json.loads((raw/(mode+'-capture.json')).read_text(encoding="utf-8"));assert capture['copy_verified'] and capture['original_sha256']==review['original_log_sha256'][mode];assert sha(raw/(mode+'-pilot.redacted.log'))==capture['published_sha256']
    post=json.loads((raw/'POST_integrity.json').read_text(encoding="utf-8"));assert post['all_layout_POST_equal'] and post['source10_POST_equal'] and post['code7_POST_equal']
    build=json.loads((HERE/'raw/20261122T093319AWST-x64/result.json').read_text(encoding="utf-8"));assert [x['exit'] for x in build['commands']]==[0,0] and build['surface_full_equal'] and build['checksurface']=='fails=0';assert (HERE/'raw/20261122T093319AWST-x64/surface_output.txt').read_bytes()==git('show','HEAD:pilot/fluenthub/surface_l1pack.txt')
    subprocess.run(['git','apply','--numstat',str(HERE/'fluenthub_f2_repair_v2.patch')],cwd=ROOT,check=True,stdout=subprocess.DEVNULL)
    print(f'PASS F2 sealed={len(entries)} imports={len(imports)} source10/layout284 runtime order/cleanup; CI does not rerun GUI')
if __name__=='__main__':main()
