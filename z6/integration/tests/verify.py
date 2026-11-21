"""Frozen-source/evidence policy; stage path scope is an explicit opt-in."""
from pathlib import Path
import argparse
import hashlib
import json
import subprocess

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
BASE = '7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a'
TIPS = {'f0': '45cbe21b96ac917c28635065b01206210aace738', 'f3': '5a3cef559e826a8450a252add7efd8659125a0e4', 'f1': '5ec13f459a7926012407b10cf263127e5a74ab73'}


def git(*args):
    return subprocess.check_output(['git', '-C', str(ROOT), *args])


def tree(ref, prefix=None):
    args = ['ls-tree', '-r', ref]
    if prefix:
        args += ['--', prefix]
    result = {}
    for line in git(*args).decode('utf-8').splitlines():
        meta, path = line.split('\t', 1)
        mode, kind, blob = meta.split()
        if kind == 'blob':
            result[path] = blob
    return result


def working_blobs(paths):
    result = subprocess.check_output(['git', '-C', str(ROOT), 'hash-object', '--stdin-paths'], input=('\n'.join(paths) + '\n').encode('utf-8'))
    return dict(zip(paths, result.decode().splitlines(), strict=True))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--enforce-stage-scope', action='store_true')
    args = parser.parse_args()
    base = tree(BASE)
    frozen = {name: blob for name, blob in base.items() if name.endswith('_LOCKED.md') or name in ['code/composition.h', 'code/capi/fm_capi.h', 'code/capi/fm_capi.cpp', 'MotionSoftDemo/MotionSystem.cs', 'MotionSoftDemo/MotionEtw.cs', 'docs/api_v0_frozen.md', 'pilot/l1pack/FluidMotion.L1.csproj']}
    current = working_blobs(sorted(frozen))
    for name, blob in frozen.items():
        assert current[name] == blob, 'Frozen file changed: ' + name
    assert current['code/composition.h'] == 'fd0603c17136c067e89c71e56aef310f2f7e63f4'
    normalized = (ROOT / 'code/composition.h').read_bytes().replace(b'\r\n', b'\n')
    assert hashlib.sha256(normalized).hexdigest() == 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'

    imports = {}
    for ticket, tip in TIPS.items():
        imports.update(tree(tip, 'z6/' + ticket))
    imported_now = working_blobs(sorted(imports))
    for name, blob in imports.items():
        if imported_now[name] != blob:
            # Future append-only report updates are allowed; original raw/scripts remain immutable.
            assert name.endswith('/REPORT.md'), 'Imported evidence changed: ' + name
            before = git('cat-file', 'blob', blob).replace(b'\r\n', b'\n')
            after = (ROOT / name).read_bytes().replace(b'\r\n', b'\n')
            assert after.startswith(before), 'Original report content changed: ' + name

    archives = json.loads((HERE / 'source_ledgers.json').read_text(encoding='utf-8'))
    archive_paths = ['z6/integration/tests/' + row['archive'] for row in archives]
    archive_blobs = working_blobs(archive_paths)
    for row, name in zip(archives, archive_paths, strict=True):
        path = ROOT / name
        assert archive_blobs[name] == row['source_blob'], name
        assert hashlib.sha256(path.read_bytes()).hexdigest() == row['sha256'], name
    f0_ledger = git('show', TIPS['f0'] + ':z6/LEDGER_snapshot.md')
    ledger = (ROOT / 'z6/LEDGER_snapshot.md').read_bytes().replace(b'\r\n', b'\n')
    assert ledger.startswith(f0_ledger.replace(b'\r\n', b'\n')), 'Aggregate ledger lost its original prefix'
    for ticket in ['f1', 'f3']:
        assert TIPS[ticket].encode() in ledger

    expected = {}
    for line in (HERE / 'SHA256SUMS').read_text(encoding='utf-8').splitlines():
        digest, name = line.split('  ', 1)
        assert name not in expected
        path = (HERE / name).resolve()
        assert path.is_relative_to(HERE) and path.is_file(), name
        assert path.stat().st_size < 5_000_000, name
        assert hashlib.sha256(path.read_bytes()).hexdigest() == digest, name
        expected[name] = digest
    actual = {p.relative_to(HERE).as_posix() for p in HERE.rglob('*') if p.is_file() and p.name != 'SHA256SUMS'}
    assert actual == set(expected), 'Manifest does not exactly cover the stage artifacts'

    if args.enforce_stage_scope:
        for ticket, tip in TIPS.items():
            assert subprocess.run(['git', '-C', str(ROOT), 'merge-base', '--is-ancestor', tip, 'HEAD']).returncode == 0, ticket
        for excluded in ['81b3f15cdf44b8207fa843bb819bf01416973316', 'e4cd696c10ad3d9a948c5650295be7fbbbc301cc']:
            assert subprocess.run(['git', '-C', str(ROOT), 'merge-base', '--is-ancestor', excluded, 'HEAD']).returncode == 1, 'Excluded F2/F5 ancestry'
        changed = git('diff', '--name-only', BASE).decode().splitlines()
        changed += git('ls-files', '--others', '--exclude-standard').decode().splitlines()
        allowed = {'code/capi/fm_capi_test.cpp', 'MotionSoftDemo/Z3aL1DevSmoke.cs', 'z6/LEDGER_snapshot.md', '.github/workflows/z6-integration-tests.yml'} | set(imports)
        for name in changed:
            assert name in allowed or name.startswith('z6/integration/tests/'), 'Stage scope exceeded: ' + name
    print(json.dumps({'status': 'PASS (dev-only)', 'frozen_files': len(frozen), 'imported_evidence_files': len(imports), 'source_ledger_archives': len(archives), 'manifest_entries': len(expected), 'stage_scope_enforced': args.enforce_stage_scope, 'no_timing_gate': True}))


if __name__ == '__main__':
    main()
