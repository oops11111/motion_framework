"""Actual F2/F5 mode observations. Imported by guarded runtime driver only."""
from pathlib import Path
import ctypes as C
import hashlib,json,os,re,shutil,time
from runtime_support import *

def run_host(mode, package, exe, attempt, native):
    is_fake = mode == 'Fake'
    is_ordinary = True
    managed_dll = exe.parent / 'FluentHub.dll'
    managed_dll_sha256 = hashlib.sha256(managed_dll.read_bytes()).hexdigest()
    state = attempt / (mode + '-state.json')
    candidates = {'package': Path(os.environ['LOCALAPPDATA']) / 'Packages' / package['PackageFamilyName'] / 'LocalState' / 'z6_f2_motion.log', 'TEMP': Path(os.environ['TEMP']) / 'z6_f2_motion.log'}
    prior_stamps = {name: path.stat().st_mtime_ns if path.exists() else 0 for name, path in candidates.items()}
    for name, path in candidates.items():
        if path.exists():
            shutil.copyfile(path, attempt / (mode + '-prior-' + name + '-app-log.txt'))
    log_path = None
    command = '& ' + literal(HERE / 'ActivateNormally.ps1') + ' -Execute -AppUserModelId ' + literal(package['PackageFamilyName'] + '!App') + ' -StatePath ' + literal(state) + ' -AuthorityFile ' + literal(AUTH) + ' -InstructionFile ' + literal(INSTRUCTION) + ' -BindingFile ' + literal(BINDING)
    if is_fake: command += ' -ActivationArguments ' + literal('--fm-pilot-f5=1')
    handle = None
    pid = None
    mode_error = None
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
        capture_log(mode, log_path, attempt)
        lifecycle(text, real=not is_fake)
        assert exit_code == 0, exit_code
        launch = 'ordinary Windows.Launch activation, AO_NOERRORUI' if is_ordinary else 'package-context diagnostic, not ordinary shell activation'
        assert hashlib.sha256(managed_dll.read_bytes()).hexdigest() == managed_dll_sha256, 'Managed host changed during test'
        return {'mode': mode, 'pid': pid, 'exe_sha256': hashlib.sha256(exe.read_bytes()).hexdigest(), 'managed_dll_sha256': managed_dll_sha256, 'packageFullName': package['PackageFullName'], 'launch': launch, 'activation_arguments': '--fm-pilot-f5=1' if is_fake else '', 'log_path_kind': 'package-or-TEMP; original separately captured', 'windows': windows, 'exit_code': exit_code, 'lifecycle': 'two subscribers, one source, clean exit', 'gate': 'PASS (dev-only)'}
    except Exception as error:
        mode_error = error
        write_json(attempt / (mode + '-original-error.json'), {'error': type(error).__name__ + ': ' + str(error)})
        raise
    finally:
        cleanup_error = None
        if pid is None:
            observed = read_state(state)
            pid = observed['pid'] if observed else None
        try:
            if handle is None and pid is not None:
                handle = native.open_owned(pid, exe, package['PackageFullName'])
            if handle:
                native.close_owned(handle, pid)
        except Exception as error:
            write_json(attempt / (mode + '-close-error.json'), {'pid': pid, 'error': str(error)})
            cleanup_error = error
        finally:
            if handle:
                native.k.CloseHandle(handle)
            # Preserve failed startup/ownership/validation logs as well as passing logs.
            for name, path in candidates.items():
                if path.exists() and path.stat().st_mtime_ns > prior_stamps[name]:
                    try: capture_log(mode, path, attempt)
                    except Exception as error:
                        cleanup_error = cleanup_error or error
                        write_json(attempt / (mode + '-capture-error.json'), {'error': str(error)})
        if mode_error is None and cleanup_error is not None: raise cleanup_error

