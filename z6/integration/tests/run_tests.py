"""Run one real Windows configuration; no applications, collection or installs."""
from pathlib import Path
import argparse
import datetime
import hashlib
import json
import os
import re
import shutil
import subprocess
import uuid
import winreg

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--configuration', choices=['Debug', 'Release'], required=True)
    args = parser.parse_args()
    cfg = args.configuration
    stamp = datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    out = ROOT / 'm0/out' / ('z6-integration-tests-' + cfg + '-' + stamp + '-' + uuid.uuid4().hex[:8])
    out.mkdir(parents=True)
    env = dict(os.environ)
    for key in ['FM_FLUIDMOTION_PATH', 'MF_Z3A_L1_DEV', 'MF_Z4_L1_SESSION']:
        env.pop(key, None)
    arch_before = env.get('PROCESSOR_ARCHITECTURE')
    if not arch_before:
        with winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r'SYSTEM\CurrentControlSet\Control\Session Manager\Environment') as key:
            env['PROCESSOR_ARCHITECTURE'] = winreg.QueryValueEx(key, 'PROCESSOR_ARCHITECTURE')[0]
    assert env['PROCESSOR_ARCHITECTURE'].upper() == 'AMD64', 'x64 build environment required'
    env['DOTNET_CLI_UI_LANGUAGE'] = 'en-US'
    result = {'configuration': cfg, 'status': 'not accepted', 'output': str(out), 'commands': [], 'child_architecture_before': arch_before, 'child_architecture': env['PROCESSOR_ARCHITECTURE'], 'environment_persisted': False}

    def run(label, command):
        log = out / (label + '.log')
        with log.open('wb') as stream:
            completed = subprocess.run(command, cwd=ROOT, env=env, stdout=stream, stderr=subprocess.STDOUT)
        result['commands'].append({'label': label, 'command': command, 'exit_code': completed.returncode, 'log': log.name, 'sha256': sha(log)})
        print(label + ' exit=' + str(completed.returncode), flush=True)
        if completed.returncode:
            raise RuntimeError(label + ' failed; original log=' + str(log))
        return log

    try:
        vswhere = Path(os.environ['ProgramFiles(x86)']) / 'Microsoft Visual Studio/Installer/vswhere.exe'
        install = subprocess.check_output([str(vswhere), '-latest', '-products', '*', '-requires', 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64', '-property', 'installationPath'], env=env, encoding='utf-8').strip()
        assert install, 'Existing MSVC x64 tools not found; do not install automatically'
        vcvars = Path(install) / 'VC/Auxiliary/Build/vcvars64.bat'
        assert vcvars.is_file()
        capi = out / 'capi'
        native = out / 'native'
        capi.mkdir()
        native.mkdir()
        includes = ['/I', 'code/capi', '/I', 'code', '/I', 'code/explore']
        flags = ['/Od', '/Zi', '/RTC1', '/D_DEBUG', '/MTd'] if cfg == 'Debug' else ['/O2', '/DNDEBUG', '/MT']
        compile_test = ['cl', '/nologo', '/std:c++17', '/EHsc', *flags, '/W3', *includes, 'code/capi/fm_capi_test.cpp', 'code/capi/fm_capi.cpp', '/Fo:' + str(capi) + os.sep, '/Fe:' + str(capi / 'fm_capi_test.exe')]
        if cfg == 'Debug':
            compile_test += ['/Fd:' + str(capi) + os.sep]
        compile_dll = ['cl', '/nologo', '/std:c++17', '/EHsc', '/O2', '/W3', '/LD', *includes, 'code/capi/fm_capi.cpp', '/Fo:' + str(native) + os.sep, '/Fe:' + str(native / 'fluidmotion.dll')]
        batch = out / 'native_build.cmd'
        batch.write_text('@echo off\ncall "' + str(vcvars) + '"\nif errorlevel 1 exit /b %errorlevel%\n' + subprocess.list2cmdline(compile_test) + '\nif errorlevel 1 exit /b %errorlevel%\n' + subprocess.list2cmdline(compile_dll) + '\nexit /b %errorlevel%\n', encoding='utf-8')
        run('native_build', ['cmd.exe', '/d', '/c', str(batch)])
        native_log = run('native_test', [str(capi / 'fm_capi_test.exe'), str(ROOT / 'code/capi/demo_motion.json')])
        # MF-12/0008: 45 baseline checks + 4 fm_active_count checks = 49.
        assert re.search(rb'\b49 checks, 0 failures\b', native_log.read_bytes()), 'Integrated native count must be 49/0'

        managed = out / 'devsmoke'
        run('managed_build', ['dotnet', 'build', str(ROOT / 'MotionSoftDemo/z3a_l1_devsmoke/Z3aL1DevSmoke.csproj'), '-c', cfg, '--no-incremental', '-o', str(managed)])
        shutil.copyfile(native / 'fluidmotion.dll', managed / 'fluidmotion.dll')
        assert sha(native / 'fluidmotion.dll') == sha(managed / 'fluidmotion.dll')
        result['native_dll_sha256'] = sha(native / 'fluidmotion.dll')
        managed_log = run('managed_test', [str(managed / 'z3a_l1_devsmoke.exe'), str(ROOT / 'code/capi/demo_motion.json')])
        text = managed_log.read_text(encoding='utf-8-sig')
        assert 'Z3A_DEV_ONLY verdict=PASS fails=0 not_z4=1 perf_ci=HOLD' in text
        assert not re.search(r'^FAIL', text, re.M)
        assert 'ok F1B' in text and 'Z5T3_DISPOSE_IDLE emitted=1 pumpOffCallbacks=1' in text
        restricted = [line for line in text.splitlines() if line.startswith(('ETW_EVENT name=FrameTick ', 'ETW_EVENT name=PresentStats '))]
        assert restricted and all(re.fullmatch(r'ETW_EVENT name=(?:FrameTick|PresentStats) id=\d+ count=\d+', line) for line in restricted)
        for name in ['AnimationLifecycle', 'Retarget', 'FrameTick', 'PresentStats', 'IdleTransition', 'TokenLoad']:
            assert 'ok E1 event ' + name in text

        source = ROOT / 'z5/t3_results_dev/K93N3P8'
        tool = out / 'surface_tool'
        tool.mkdir()
        shutil.copyfile(source / 'SurfaceDump_Program.cs', tool / 'Program.cs')
        shutil.copyfile(source / 'SurfaceDump.csproj.txt', tool / 'SurfaceDump.csproj')
        run('surface_tool_build', ['dotnet', 'build', str(tool / 'SurfaceDump.csproj'), '-c', 'Release'])
        dump = out / ('surface_' + cfg + '.txt')
        run('surface_check', ['dotnet', str(tool / 'bin/Release/net10.0/SurfaceDump.dll'), str(managed / 'z3a_l1_devsmoke.dll'), str(dump)])
        check = dump.with_suffix('.checksurface.txt').read_text(encoding='utf-8-sig')
        assert 'CHECKSURFACE fails=0' in check
        baseline = source / ('surface_ds_' + cfg + '.txt')
        assert dump.read_bytes().replace(b'\r\n', b'\n') == baseline.read_bytes().replace(b'\r\n', b'\n'), 'Full public surface differs from frozen stored baseline'
        result['surface_sha256_lf'] = hashlib.sha256(dump.read_bytes().replace(b'\r\n', b'\n')).hexdigest()
        result['managed_assembly_sha256'] = sha(managed / 'z3a_l1_devsmoke.dll')
        result['status'] = 'PASS (dev-only)'
        print(json.dumps(result, ensure_ascii=True), flush=True)
    except Exception as error:
        result['error'] = type(error).__name__ + ': ' + str(error)
        raise
    finally:
        (out / 'result.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8', newline='\n')
        files = sorted(p for p in out.iterdir() if p.is_file())
        (out / 'SHA256SUMS').write_text(''.join(sha(p) + '  ' + p.name + '\n' for p in files), encoding='ascii', newline='\n')
        print('ARTIFACT_DIRECTORY=' + str(out), flush=True)


if __name__ == '__main__':
    main()
