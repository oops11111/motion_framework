"""F5 B/C/E package-context diagnostic. Default is read-only dry-run.

--execute requires an explicit authorization file. Do not run it merely because
this file exists. No settings writes, installers, UAC, tasks or global environment.
"""
from pathlib import Path
import argparse
import ctypes as C
from ctypes import wintypes as W
import datetime
import hashlib
import json
import os
import re
import shutil
import subprocess
import time
import uuid
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
IDENTITY = '49462fluenthub-uwp.FluentHub'
PUBLISHER = 'CN=4E3C0825-4D00-4ECC-89F8-528E9200B125'
PS = shutil.which('powershell.exe')


def literal(value):
    return "'" + str(value).replace("'", "''") + "'"


def ps(command):
    env = dict(os.environ, PROCESSOR_ARCHITECTURE='AMD64')
    run = subprocess.run([PS, '-NoProfile', '-Command', "$ErrorActionPreference='Stop'; [Console]::OutputEncoding=New-Object System.Text.UTF8Encoding($false); " + command], env=env, capture_output=True)
    if run.returncode:
        raise RuntimeError(run.stderr.decode('utf-8-sig', errors='replace'))
    return run.stdout.decode('utf-8-sig').strip()


def packages():
    text = ps("$r=@(Get-AppxPackage -Name " + literal(IDENTITY) + " | ForEach-Object { [pscustomobject]@{ Name=$_.Name; PackageFullName=$_.PackageFullName; PackageFamilyName=$_.PackageFamilyName; Publisher=$_.Publisher; Version=$_.Version.ToString(); Architecture=$_.Architecture.ToString(); InstallLocation=$_.InstallLocation } }); ConvertTo-Json -InputObject $r -Compress")
    return json.loads(text)


def snapshot():
    return json.loads(ps('& ' + literal(HERE / 'ReadAnimationSettings.ps1')))


def wait_for(predicate, description, seconds=25):
    until = time.monotonic() + seconds
    while time.monotonic() < until:
        result = predicate()
        if result:
            return result
        time.sleep(.1)
    raise RuntimeError('Observation timeout: ' + description)


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + '\n', encoding='utf-8', newline='\n')


def read_state(path):
    try:
        return json.loads(path.read_text(encoding='utf-8-sig'))
    except (OSError, json.JSONDecodeError):
        return None


class Native:
    def __init__(self):
        self.k = C.WinDLL('kernel32', use_last_error=True)
        self.u = C.WinDLL('user32', use_last_error=True)
        self.k.OpenProcess.argtypes = [W.DWORD, W.BOOL, W.DWORD]
        self.k.OpenProcess.restype = W.HANDLE
        self.k.CloseHandle.argtypes = [W.HANDLE]
        self.k.WaitForSingleObject.argtypes = [W.HANDLE, W.DWORD]
        self.k.QueryFullProcessImageNameW.argtypes = [W.HANDLE, W.DWORD, W.LPWSTR, C.POINTER(W.DWORD)]
        self.k.GetPackageFullName.argtypes = [W.HANDLE, C.POINTER(W.UINT), W.LPWSTR]
        self.k.GetExitCodeProcess.argtypes = [W.HANDLE, C.POINTER(W.DWORD)]
        self.k.OpenEventW.argtypes = [W.DWORD, W.BOOL, W.LPCWSTR]
        self.k.OpenEventW.restype = W.HANDLE
        self.k.SetEvent.argtypes = [W.HANDLE]
        self.u.GetWindowThreadProcessId.argtypes = [W.HWND, C.POINTER(W.DWORD)]
        self.u.GetAncestor.argtypes = [W.HWND, W.UINT]
        self.u.GetAncestor.restype = W.HWND
        self.u.PostMessageW.argtypes = [W.HWND, W.UINT, W.WPARAM, W.LPARAM]
        self.u.SendMessageTimeoutW.argtypes = [W.HWND, W.UINT, W.WPARAM, W.LPARAM, W.UINT, W.UINT, C.POINTER(C.c_size_t)]
        self.u.SendMessageTimeoutW.restype = C.c_ssize_t
        self.enum_proc = C.WINFUNCTYPE(W.BOOL, W.HWND, W.LPARAM)
        self.u.EnumWindows.argtypes = [self.enum_proc, W.LPARAM]

    def open_owned(self, pid, exe, full_name):
        handle = self.k.OpenProcess(0x1000 | 0x100000, False, pid)
        if not handle:
            raise C.WinError(C.get_last_error())
        try:
            path = C.create_unicode_buffer(32768)
            length = W.DWORD(len(path))
            if not self.k.QueryFullProcessImageNameW(handle, 0, path, C.byref(length)):
                raise C.WinError(C.get_last_error())
            if Path(path.value).resolve() != exe:
                raise RuntimeError('PID executable does not match the newly built host')
            if full_name is None:
                return handle # Failure cleanup still requires the exact new PID and executable.
            size = W.UINT(0)
            error = self.k.GetPackageFullName(handle, C.byref(size), None)
            if error != 122:
                raise RuntimeError('Host lacks expected package identity; error=' + str(error))
            text = C.create_unicode_buffer(size.value)
            error = self.k.GetPackageFullName(handle, C.byref(size), text)
            if error or text.value != full_name:
                raise RuntimeError('Host package identity mismatch')
            return handle
        except Exception:
            self.k.CloseHandle(handle)
            raise

    def hwnd_pid(self, hwnd):
        pid = W.DWORD()
        self.u.GetWindowThreadProcessId(hwnd, C.byref(pid))
        return pid.value

    def close_owned(self, handle, pid):
        if self.k.WaitForSingleObject(handle, 0) != 0:
            @self.enum_proc
            def callback(hwnd, unused):
                if self.hwnd_pid(hwnd) == pid:
                    self.u.PostMessageW(hwnd, 0x0010, 0, 0)
                return True
            self.u.EnumWindows(callback, 0)
            if self.k.WaitForSingleObject(handle, 5000) != 0:
                raise RuntimeError('Owned host did not exit; no kill or package removal is attempted')
        code = W.DWORD()
        if not self.k.GetExitCodeProcess(handle, C.byref(code)):
            raise C.WinError(C.get_last_error())
        return code.value

    def window_details(self, pid):
        rows = []
        self.u.GetWindowTextW.argtypes = [W.HWND, W.LPWSTR, C.c_int]
        self.u.GetClassNameW.argtypes = [W.HWND, W.LPWSTR, C.c_int]
        self.u.IsWindowVisible.argtypes = [W.HWND]
        @self.enum_proc
        def callback(hwnd, unused):
            if self.hwnd_pid(hwnd) == pid:
                title = C.create_unicode_buffer(1024)
                kind = C.create_unicode_buffer(512)
                self.u.GetWindowTextW(hwnd, title, len(title))
                self.u.GetClassNameW(hwnd, kind, len(kind))
                rows.append({'hwnd': hex(hwnd), 'title': title.value, 'class': kind.value, 'visible': bool(self.u.IsWindowVisible(hwnd))})
            return True
        self.u.EnumWindows(callback, 0)
        return rows


def lifecycle(text, real):
    assert sorted(re.findall(r'F5 subscriber attached cycle=(\d+)', text)) == ['1', '2']
    assert sorted(re.findall(r'F5 subscriber detached cycle=(\d+)', text)) == ['1', '2']
    assert len(re.findall(r'F5 source disposed ', text)) == 1
    if real:
        assert len(re.findall(r'F5 hook installed .*ok=True', text)) == 1
        assert 'F5 source disposed hookRemoved=True' in text
    else:
        assert text.count('F5 fake source enabled') == 1
        assert 'fake=2' in text


def run_host(mode, package, exe, attempt, native):
    is_fake = mode in ['Fake', 'OrdinaryFake']
    is_ordinary = mode in ['Ordinary', 'OrdinaryFake']
    managed_dll = exe.parent / 'FluentHub.dll'
    managed_dll_sha256 = hashlib.sha256(managed_dll.read_bytes()).hexdigest()
    state = attempt / (mode + '-state.json')
    candidates = {'package': Path(os.environ['LOCALAPPDATA']) / 'Packages' / package['PackageFamilyName'] / 'LocalState' / 'z6_f2_motion.log', 'TEMP': Path(os.environ['TEMP']) / 'z6_f2_motion.log'}
    prior_stamps = {name: path.stat().st_mtime_ns if path.exists() else 0 for name, path in candidates.items()}
    for name, path in candidates.items():
        if path.exists():
            shutil.copyfile(path, attempt / (mode + '-prior-' + name + '-app-log.txt'))
    log_path = None
    if is_ordinary:
        command = '& ' + literal(HERE / 'ActivateHostNormally.ps1') + ' -AppUserModelId ' + literal(package['PackageFamilyName'] + '!App') + ' -StatePath ' + literal(state)
        if is_fake:
            command += ' -ActivationArguments ' + literal('--fm-pilot-f5=1')
    else:
        args = subprocess.list2cmdline(['-NoProfile', '-File', str(HERE / 'LaunchHostInPackage.ps1'), '-ExePath', str(exe), '-Mode', mode, '-StatePath', str(state), '-StdoutPath', str(attempt / (mode + '-stdout.txt')), '-StderrPath', str(attempt / (mode + '-stderr.txt'))])
        command = 'Invoke-CommandInDesktopPackage -PackageFamilyName ' + literal(package['PackageFamilyName']) + ' -AppId App -Command ' + literal(PS) + ' -Args ' + literal(args) + ' -PreventBreakaway'
    handle = None
    pid = None
    write_json(attempt / (mode + '-launch-request.json'), {'state_file': state.name, 'mode': mode})
    try:
        ps(command)
        launched = wait_for(lambda: read_state(state), 'new host process state')
        if not is_ordinary:
            assert launched['packageFullName'] == package['PackageFullName']
        pid = launched['pid']
        handle = native.open_owned(pid, exe, package['PackageFullName'])
        def read_log():
            nonlocal log_path
            fresh = [path for name, path in candidates.items() if path.exists() and path.stat().st_mtime_ns > prior_stamps[name]]
            if len(fresh) > 1:
                raise RuntimeError('Ambiguous new app logs in both known locations')
            if not fresh:
                return ''
            log_path = fresh[0]
            text = log_path.read_text(encoding='utf-8-sig', errors='replace')
            if re.search(r'F5 hook installed .*ok=False', text):
                raise RuntimeError('Real hook installation failed; stop this path')
            return text
        wait_for(lambda: 'F5 subscriber attached cycle=2' in read_log() and 'F5 subscriber detached cycle=1' in read_log(), 'two RootViews and old subscriber disposal')
        windows = native.window_details(pid)
        write_json(attempt / (mode + '-windows.json'), windows)
        if is_ordinary:
            assert any(window['visible'] and window['class'] == 'WinUIDesktopWin32WindowClass' for window in windows), 'Ordinary app is not visibly rendering'
        if is_fake:
            event = native.k.OpenEventW(2, False, 'Local\\FM_PILOT_F5_TOGGLE')
            if not event:
                raise C.WinError(C.get_last_error())
            try:
                for count in [1, 2]:
                    if not native.k.SetEvent(event):
                        raise C.WinError(C.get_last_error())
                    wait_for(lambda: len(re.findall(r'F5 SetReducedMotion\((True|False)\) source=fake', read_log())) == count, 'fake signal reaching actual SetReducedMotion')
            finally:
                native.k.CloseHandle(event)
            calls = re.findall(r'F5 SetReducedMotion\((True|False)\) source=fake animationsEnabled=(True|False) previous=(True|False) thread=(\d+)', read_log())
            assert len(calls) == 2
            assert all(call[0] != call[1] for call in calls)
            assert calls[0][0] != calls[1][0] and calls[0][1] != calls[1][1]
            assert calls[0][1] == calls[1][2] and calls[1][1] == calls[0][2]
            threads = set(re.findall(r'F5 SetReducedMotion\([^)]*\) source=initial .*thread=(\d+)', read_log()))
            assert len(threads) == 1 and all(call[3] in threads for call in calls)
        else:
            hooks = re.findall(r'F5 hook installed hwnd=0x([0-9A-Fa-f]+).*ok=True', read_log())
            assert len(hooks) == 1
            hwnd = int(hooks[0], 16)
            assert native.hwnd_pid(hwnd) == pid and native.u.GetAncestor(hwnd, 2) == hwnd
            before = len(re.findall(r'F5 SetReducedMotion\([^)]*\) source=wm', read_log()))
            message_result = C.c_size_t()
            C.set_last_error(0)
            delivered = native.u.SendMessageTimeoutW(hwnd, 0x001A, 0x006D, 0, 0x0002, 2000, C.byref(message_result))
            last_error = C.get_last_error()
            write_json(attempt / (mode + '-message-injection.json'), {'pid': pid, 'hwnd': hex(hwnd), 'message': '0x001A', 'wParam': '0x006D', 'lParam': 0, 'api': 'SendMessageTimeoutW, SMTO_ABORTIFHUNG', 'return': delivered, 'last_error': last_error, 'message_result': message_result.value, 'watchdog_only_not_performance_gate': True})
            if not delivered:
                raise C.WinError(last_error)
            wait_for(lambda: 'F5 WM_SETTINGCHANGE wParam=0x6D ' in read_log() and re.search(r'F5 reread unchanged animationsEnabled=(True|False) source=wm', read_log()), 'arbitrary wParam injection and unchanged reread')
            assert len(re.findall(r'F5 SetReducedMotion\([^)]*\) source=wm', read_log())) == before
        exit_code = native.close_owned(handle, pid)
        wait_for(lambda: 'F5 source disposed ' in read_log() and 'F5 subscriber detached cycle=2' in read_log(), 'shutdown log flush', 5)
        text = log_path.read_text(encoding='utf-8-sig')
        (attempt / (mode + '-app-log.txt')).write_text(text, encoding='utf-8', newline='\n')
        lifecycle(text, real=not is_fake)
        assert exit_code == 0, exit_code
        launch = 'ordinary Windows.Launch activation, AO_NOERRORUI' if is_ordinary else 'package-context diagnostic, not ordinary shell activation'
        assert hashlib.sha256(managed_dll.read_bytes()).hexdigest() == managed_dll_sha256, 'Managed host changed during test'
        return {'mode': mode, 'pid': pid, 'exe_sha256': hashlib.sha256(exe.read_bytes()).hexdigest(), 'managed_dll_sha256': managed_dll_sha256, 'packageFullName': package['PackageFullName'], 'launch': launch, 'activation_arguments': '--fm-pilot-f5=1' if mode == 'OrdinaryFake' else '', 'log_path': str(log_path), 'windows': windows, 'exit_code': exit_code, 'lifecycle': 'two subscribers, one source, clean exit', 'gate': 'PASS (dev-only)'}
    finally:
        if pid is None:
            observed = read_state(state)
            pid = observed['pid'] if observed else None
        try:
            if handle is None and pid is not None:
                handle = native.open_owned(pid, exe, None)
            if handle:
                native.close_owned(handle, pid)
        except Exception as error:
            write_json(attempt / (mode + '-close-error.json'), {'pid': pid, 'error': str(error)})
        finally:
            if handle:
                native.k.CloseHandle(handle)
            # Preserve failed startup/ownership/validation logs as well as passing logs.
            for name, path in candidates.items():
                if path.exists() and path.stat().st_mtime_ns > prior_stamps[name]:
                    shutil.copyfile(path, attempt / (mode + '-' + name + '-app-log.txt'))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--manifest', type=Path, required=True)
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument('--execute', action='store_true')
    mode.add_argument('--dry-run', action='store_true')
    parser.add_argument('--authorization-file', type=Path)
    parser.add_argument('--launch-modes', choices=['Fake', 'Real', 'Ordinary', 'OrdinaryFake'], nargs='+', default=['OrdinaryFake', 'Ordinary'])
    args = parser.parse_args()
    manifest = args.manifest.resolve()
    allowed = (ROOT / 'm0/out/f5-fluenthub').resolve()
    if not manifest.is_relative_to(allowed) or manifest.name != 'AppxManifest.xml':
        raise RuntimeError('Manifest must be the isolated F5 build output')
    package_xml = ET.parse(manifest).getroot()
    identity = package_xml.find('{*}Identity').attrib
    assert identity['Name'] == IDENTITY and identity['Publisher'] == PUBLISHER
    assert identity['Version'] == '0.11.0.0' and identity['ProcessorArchitecture'] == 'x64'
    assert not packages(), 'An existing FluentHub package must not be disturbed'
    pre = snapshot()
    assert pre['AnimationsEnabled'] == pre['SPI_GETCLIENTAREAANIMATION'], 'Predicate mismatch requires review'
    exe = manifest.parent / 'FluentHub.exe'
    assert exe.is_file()
    attempt = ROOT / 'm0/out' / ('f5-gui-' + datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ-') + uuid.uuid4().hex[:8])
    attempt.mkdir()
    launch_descriptions = {'OrdinaryFake': 'ordinary Windows.Launch with exact --fm-pilot-f5=1, process-only Fake environment', 'Ordinary': 'ordinary Windows.Launch with no Fake argument, real source', 'Fake': 'legacy package-context Fake diagnostic (not retried)', 'Real': 'legacy package-context Real diagnostic (not required)'}
    plan = {'mode': 'execute' if args.execute else 'read-only dry-run', 'selected_modes': args.launch_modes, 'identity': identity, 'manifest': str(manifest), 'manifest_sha256': hashlib.sha256(manifest.read_bytes()).hexdigest(), 'exe_sha256': hashlib.sha256(exe.read_bytes()).hexdigest(), 'managed_dll_sha256': hashlib.sha256((exe.parent / 'FluentHub.dll').read_bytes()).hexdigest(), 'PRE': pre, 'register': 'Add-AppxPackage -Register ' + literal(manifest), 'launch': [launch_descriptions[mode] for mode in args.launch_modes], 'cleanup': 'Remove-AppxPackage -Package <exact newly registered FullName>; never framework/runtime packages', 'limits': 'Package-context diagnostic token differs from ordinary activation; results separate. No OS settings writes, tasks or elevation'}
    write_json(attempt / 'plan.json', plan)
    print(json.dumps(plan, ensure_ascii=True), flush=True)
    if not args.execute:
        return
    if not args.authorization_file or not args.authorization_file.is_file():
        raise RuntimeError('Explicit current-session package-operation authorization file required')
    authorization = json.loads(args.authorization_file.read_text(encoding='utf-8-sig'))
    assert authorization['conversation_id'] == '01a1fbc2-1314-7713-a701-c7b3149490ec'
    assert authorization['identity'] == IDENTITY and authorization['manifest_sha256'] == plan['manifest_sha256']
    assert authorization['scope'] == 'F5 temporary registration, package-context diagnostics, ordinary Real activation, exact cleanup'
    assert authorization['source'] == 'direct user response in current root conversation'
    assert authorization['user_text'].strip() and authorization['recorded_awst'] and authorization['question_item_id']
    # This record is evidence, not permission by itself: the operator must verify the actual current user reply.
    shutil.copyfile(args.authorization_file, attempt / 'authorization.txt')
    created = None
    result = {'status': 'not accepted', 'selected_modes': args.launch_modes, 'managed_dll_sha256': plan['managed_dll_sha256'], 'required_gate_mapping': {'B': 'OrdinaryFake', 'C': 'Ordinary', 'E': 'OrdinaryFake and Ordinary'}, 'legacy_diagnostic_modes': 'Fake/Real are not required gates and are not retried', 'runs': []}
    native = Native()
    if any(mode in ['Fake', 'OrdinaryFake'] for mode in args.launch_modes):
        existing_event = native.k.OpenEventW(0x100000, False, 'Local\\FM_PILOT_F5_TOGGLE')
        if existing_event:
            native.k.CloseHandle(existing_event)
            raise RuntimeError('Existing Fake toggle event; do not signal another test session')
        if C.get_last_error() != 2:
            raise RuntimeError('Cannot establish absence of the Fake toggle event')
    try:
        ps('Add-AppxPackage -Register ' + literal(manifest))
        matches = packages()
        assert len(matches) == 1
        package = matches[0]
        assert package['Name'] == IDENTITY and package['Publisher'] == PUBLISHER
        assert package['Version'] == identity['Version'] and package['Architecture'].lower() == 'x64'
        assert Path(package['InstallLocation']).resolve() == manifest.parent
        created = package['PackageFullName']
        write_json(attempt / 'registered-package.json', package)
        for mode in args.launch_modes:
            before = snapshot()
            result['runs'].append(run_host(mode, package, exe, attempt, native))
            after = snapshot()
            write_json(attempt / (mode + '-settings.json'), {'PRE': before, 'POST': after})
            assert before == pre == after, 'OS settings changed externally or unexpectedly'
        complete = {'OrdinaryFake', 'Ordinary'}.issubset(args.launch_modes)
        result['status'] = 'PASS (dev-only) B/C/E via ordinary activation' if complete else 'PASS (dev-only) for selected modes only; missing required gates not accepted'
    except Exception as error:
        result['error'] = type(error).__name__ + ': ' + str(error)
        raise
    finally:
        try:
            # Recover exact owned identity even if registration partially succeeded.
            matches = packages()
            owned = [p for p in matches if p['Name'] == IDENTITY and p['Publisher'] == PUBLISHER and p['Version'] == identity['Version'] and Path(p['InstallLocation']).resolve() == manifest.parent]
            if created is None and len(owned) == 1:
                created = owned[0]['PackageFullName']
            if created is not None:
                # Never remove a package while one of our wrapper states says its host may still be alive.
                live = []
                for request_path in attempt.glob('*-launch-request.json'):
                    request = json.loads(request_path.read_text(encoding='utf-8'))
                    state_path = attempt / request['state_file']
                    state = read_state(state_path)
                    if state is None:
                        live.append('unreadable child state')
                        continue
                    handle = native.k.OpenProcess(0x100000, False, state['pid'])
                    if handle:
                        try:
                            if native.k.WaitForSingleObject(handle, 0) != 0:
                                live.append(state['pid'])
                        finally:
                            native.k.CloseHandle(handle)
                    elif C.get_last_error() != 87:
                        live.append('unverifiable pid ' + str(state['pid']))
                if live:
                    result['cleanup_pending'] = live
                elif len(owned) == 1 and owned[0]['PackageFullName'] == created and len(matches) == 1:
                    ps('Remove-AppxPackage -Package ' + literal(created))
                    result['removed_exact_package'] = created
                else:
                    result['cleanup_pending'] = 'Package ownership changed; no removal attempted'
        except Exception as cleanup_error:
            result['cleanup_error'] = type(cleanup_error).__name__ + ': ' + str(cleanup_error)
        try:
            result['remaining_packages'] = packages()
            result['POST'] = snapshot()
            result['settings_unchanged'] = pre == result['POST']
        except Exception as verify_error:
            result['post_cleanup_error'] = str(verify_error)
            result['settings_unchanged'] = False
        if result.get('remaining_packages') or not result['settings_unchanged'] or result.get('cleanup_pending') or result.get('cleanup_error'):
            result['status'] = 'not accepted; cleanup or settings review required'
        write_json(attempt / 'result.json', result)
        files = sorted(p for p in attempt.iterdir() if p.is_file())
        (attempt / 'SHA256SUMS').write_text(''.join(hashlib.sha256(p.read_bytes()).hexdigest() + '  ' + p.name + '\n' for p in files), encoding='ascii', newline='\n')
        print(json.dumps(result, ensure_ascii=True), flush=True)
    if not result['status'].startswith('PASS'):
        raise RuntimeError('Acceptance or cleanup not complete; inspect the preserved attempt result')


if __name__ == '__main__':
    main()
