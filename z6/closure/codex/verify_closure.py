"""Closure documentation/source integrity only; conclusions are reviewed by the primary/user."""
from pathlib import Path
import argparse,hashlib,json,re,subprocess
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
BASE='eb09e63f3068a98fb011709132365e81d825c23c'
COORD='9c28dcc6a97bda7d552ddf2a6cc6f12347063751'
def git(*args):return subprocess.check_output(['git','-C',str(ROOT),*args])
def sha(data):return hashlib.sha256(data).hexdigest()
def canonical(path):
    data=path.read_bytes()
    return data if 'source_archive' in path.parts else data.replace(b'\r\n',b'\n')
def tree(ref):
    return {x.split(b'\t',1)[1].decode():x.split(b'\t',1)[0].split()[2].decode() for x in git('ls-tree','-r','-z',ref).split(b'\0') if x}
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--enforce-stage-scope',action='store_true');args=parser.parse_args()
    expected={}
    for line in (HERE/'SHA256SUMS').read_text(encoding='utf-8').splitlines():
        digest,name=line.split('  ',1);p=(HERE/name).resolve();assert p.is_relative_to(HERE) and p.is_file() and name not in expected;assert sha(canonical(p))==digest,name;assert p.stat().st_size<5_000_000;expected[name]=digest
    actual={p.relative_to(HERE).as_posix() for p in HERE.rglob('*') if p.is_file() and p.name!='SHA256SUMS' and '__pycache__' not in p.parts};assert actual==set(expected),actual^set(expected)
    index=json.loads((HERE/'SOURCE_INDEX.json').read_text(encoding='utf-8'));assert index['base']==BASE and index['coord']==COORD
    source_names=set()
    for item in index['sources']:
        p=(HERE/item['path']).resolve();assert p.is_relative_to(HERE/'source_archive') and p.stat().st_size==item['bytes'] and sha(p.read_bytes())==item['sha256'];source_names.add(p.name)
    assert {p.name for p in (HERE/'source_archive').iterdir() if p.is_file()}==source_names
    assert sha((HERE/'source_archive/TECHNICAL_REVIEW_DRAFT.md').read_bytes())=='0a0ef2ce22248e797aa40eb9793f3c3efefe6435b28baf5d28c632e5493e6cab'
    archive=HERE/'source_archive'/('main-'+BASE+'-ledger.md');old=git('show',BASE+':z6/LEDGER_snapshot.md');assert archive.read_bytes()==old;assert canonical(ROOT/'z6/LEDGER_snapshot.md').startswith(old.replace(b'\r\n',b'\n'))
    binding=json.loads((HERE/'DELIVERY_BINDING.json').read_text(encoding='utf-8'))
    for name,digest in binding['canonical_file_hashes'].items():assert sha(canonical(ROOT/name))==digest,name
    auth=json.loads((HERE/'AUTHORIZATION.json').read_text(encoding='utf-8'));assert auth['main_base']==BASE and auth['accepted_technical_draft_sha256']==index['accepted_draft_sha256'];assert auth['question_item_id']=='["request_user_input_async","call_285b8127443245208d35b373a5ff2baf",0]'
    receipts=json.loads((HERE/'source_archive/SOURCE_LINK_RECEIPTS.json').read_text(encoding='utf-8'));assert receipts['main']==BASE
    for row in receipts['reports']:assert sha(git('show',BASE+':'+row['path']))==row['canonical_sha256'],row['path']
    api=json.loads((HERE/'source_archive/API_PROVENANCE.json').read_text(encoding='utf-8'));pulls={str(x['pr']) for x in api['pulls']};runs={str(x['id']) for x in api['main_post_runs']};trees={};links=0
    for doc in [ROOT/'docs/z6_closed.md',*HERE.rglob('*.md')]:
        text=doc.read_text(encoding='utf-8')
        for target in re.findall(r'\[[^\]]*\]\(([^)]+)\)',text):
            target=target.strip('<>');links+=1
            if target.startswith('https://github.com/oops11111/motion_framework/blob/'):
                ref,path=target.split('/blob/',1)[1].split('/',1);path=path.split('#',1)[0];assert re.fullmatch('[0-9a-f]{40}',ref);
                if ref not in trees:trees[ref]=tree(ref)
                assert path in trees[ref],target
            elif target.startswith('https://github.com/oops11111/motion_framework/pull/'):
                assert target.rsplit('/',1)[1] in pulls,target
            elif target.startswith('https://github.com/oops11111/motion_framework/actions/runs/'):
                assert target.rsplit('/',1)[1] in runs,target
            elif '://' not in target and not target.startswith('#'):
                local=(doc.parent/target.split('#',1)[0]).resolve();assert local.is_relative_to(ROOT) and local.exists(),target
    changed=git('diff','--name-only',BASE).decode().splitlines()+git('ls-files','--others','--exclude-standard').decode().splitlines()
    if args.enforce_stage_scope:
        for name in changed:assert name.startswith('z6/closure/codex/') or name in ['docs/z6_closed.md','z6/LEDGER_snapshot.md','.github/workflows/z6-closure.yml'],name
    assert not git('diff','--name-only',BASE,'--','code','MotionSoftDemo','pilot').strip(),'Protected source/pack changed'
    assert not any(name.endswith('_LOCKED.md') or '/APPROVED' in name for name in changed),'Unexpected lock/approval artifact'
    print(f'PASS documentation integrity: sealed={len(expected)} exact_archives={len(source_names)} source_reports={len(receipts["reports"])} checked_links={links}; no behavior/GUI tests')
if __name__=='__main__':main()
