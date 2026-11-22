"""Private PID/path/package/ordinary-close helper adapted from accepted F5 harness; no kill."""
from pathlib import Path
import ctypes as C
from ctypes import wintypes as W

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
                if self.hwnd_pid(hwnd) == pid and self.u.IsWindowVisible(hwnd):
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

    def inspect_package(self, pid):
        """Known package/no-package/exited versus unknown; never treat query failure as absence."""
        handle = self.k.OpenProcess(0x1000 | 0x100000, False, pid)
        if not handle:
            return {"state": "unknown", "pid": pid, "error": C.get_last_error(), "operation": "OpenProcess"}
        try:
            if self.k.WaitForSingleObject(handle, 0) == 0:
                return {"state": "exited", "pid": pid}
            size = W.UINT(0)
            error = self.k.GetPackageFullName(handle, C.byref(size), None)
            if error == 15700:
                return {"state": "no_package", "pid": pid}
            if error != 122:
                return {"state": "unknown", "pid": pid, "error": error, "operation": "GetPackageFullName-size"}
            text = C.create_unicode_buffer(size.value)
            error = self.k.GetPackageFullName(handle, C.byref(size), text)
            if error:
                return {"state": "unknown", "pid": pid, "error": error, "operation": "GetPackageFullName"}
            return {"state": "known_package", "pid": pid, "package_fullname": text.value}
        except Exception as exc:
            return {"state": "unknown", "pid": pid, "error": str(exc)}
        finally:
            self.k.CloseHandle(handle)
