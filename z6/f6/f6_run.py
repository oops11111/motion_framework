# Z6 F6: register loose package, launch via one-shot schtasks, capture current-window frames, close, delete task.
import ctypes, json, os, shutil, subprocess, sys, time
from ctypes import wintypes
from datetime import datetime, timezone, timedelta
from pathlib import Path

AWST = timezone(timedelta(hours=8))
TEMP = Path(os.environ["TEMP"]) / "z6" / "f6" / "run"
TEMP.mkdir(parents=True, exist_ok=True)
MANIFEST = Path(os.environ["USERPROFILE"]) / r"pilot\FluentHub\src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml"
APP_USER = r"explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App"
TASK = "z6_f6_launch"
PKG_NAME = "49462fluenthub-uwp.FluentHub"
LOG_NAME = "z6_f6_motion.log"

user32 = ctypes.windll.user32
user32.GetWindowRect.argtypes = [wintypes.HWND, ctypes.POINTER(wintypes.RECT)]
user32.GetWindowRect.restype = wintypes.BOOL
user32.IsWindowVisible.argtypes = [wintypes.HWND]
user32.IsWindowVisible.restype = wintypes.BOOL
user32.SetCursorPos.argtypes = [ctypes.c_int, ctypes.c_int]
user32.SetCursorPos.restype = wintypes.BOOL
user32.GetCursorPos.argtypes = [ctypes.POINTER(wintypes.POINT)]
user32.GetCursorPos.restype = wintypes.BOOL

def now():
    return datetime.now(AWST).isoformat()

def log(msg):
    line = f"{now()} {msg}"
    print(line, flush=True)
    with (TEMP / "run.log").open("a", encoding="utf-8") as f:
        f.write(line + "\n")

def run(args, **kw):
    r = subprocess.run(args, capture_output=True, text=True, encoding="utf-8", errors="replace", **kw)
    return r

def find_window():
    # MainWindowHandle of a process whose title is FluentHub
    r = run([
        "powershell", "-NoProfile", "-Command",
        "Get-Process | Where-Object { $_.MainWindowTitle -eq 'FluentHub' } | ForEach-Object { '{0} {1}' -f $_.Id, $_.MainWindowHandle }"
    ])
    for line in r.stdout.splitlines():
        parts = line.strip().split()
        if len(parts) == 2 and parts[0].isdigit() and parts[1].isdigit() and int(parts[1]) != 0:
            return int(parts[0]), int(parts[1])
    return None, None

def window_rect(hwnd):
    rc = wintypes.RECT()
    if not user32.GetWindowRect(hwnd, ctypes.byref(rc)):
        return None
    w, h = rc.right - rc.left, rc.bottom - rc.top
    if w <= 0 or h <= 0:
        return None
    return rc.left, rc.top, rc.right, rc.bottom


def _set_dpi_aware():
    try:
        ctypes.windll.shcore.SetProcessDpiAwareness(2)
    except Exception:
        try:
            ctypes.windll.user32.SetProcessDPIAware()
        except Exception:
            pass

_set_dpi_aware()
PW_RENDERFULLCONTENT = 2
user32.PrintWindow.argtypes = [wintypes.HWND, wintypes.HDC, wintypes.UINT]
user32.PrintWindow.restype = wintypes.BOOL
gdi32 = ctypes.windll.gdi32

def capture(hwnd, dest: Path):
    from PIL import Image
    box = window_rect(hwnd)
    if not box:
        return None
    w, h = box[2] - box[0], box[3] - box[1]
    hwndDC = user32.GetWindowDC(hwnd)
    memDC = gdi32.CreateCompatibleDC(hwndDC)
    bmp = gdi32.CreateCompatibleBitmap(hwndDC, w, h)
    prev = gdi32.SelectObject(memDC, bmp)
    ok = user32.PrintWindow(hwnd, memDC, PW_RENDERFULLCONTENT)
    if not ok:
        ok = user32.PrintWindow(hwnd, memDC, 0)
    class BITMAPINFOHEADER(ctypes.Structure):
        _fields_ = [
            ("biSize", wintypes.DWORD), ("biWidth", ctypes.c_long), ("biHeight", ctypes.c_long),
            ("biPlanes", wintypes.WORD), ("biBitCount", wintypes.WORD), ("biCompression", wintypes.DWORD),
            ("biSizeImage", wintypes.DWORD), ("biXPelsPerMeter", ctypes.c_long), ("biYPelsPerMeter", ctypes.c_long),
            ("biClrUsed", wintypes.DWORD), ("biClrImportant", wintypes.DWORD),
        ]
    class BITMAPINFO(ctypes.Structure):
        _fields_ = [("bmiHeader", BITMAPINFOHEADER)]
    bmi = BITMAPINFO()
    bmi.bmiHeader.biSize = ctypes.sizeof(BITMAPINFOHEADER)
    bmi.bmiHeader.biWidth = w
    bmi.bmiHeader.biHeight = -h
    bmi.bmiHeader.biPlanes = 1
    bmi.bmiHeader.biBitCount = 32
    buf = ctypes.create_string_buffer(w * h * 4)
    gdi32.GetDIBits(memDC, bmp, 0, h, buf, ctypes.byref(bmi), 0)
    gdi32.SelectObject(memDC, prev)
    gdi32.DeleteObject(bmp)
    gdi32.DeleteDC(memDC)
    user32.ReleaseDC(hwnd, hwndDC)
    img = Image.frombuffer("RGBA", (w, h), buf, "raw", "BGRA", 0, 1).convert("RGB")
    img.save(dest)
    return box, img.size, bool(ok)

def main():
    log(f"manifest={MANIFEST} exists={MANIFEST.exists()}")
    rcj = list(MANIFEST.parent.glob("*.runtimeconfig.json"))
    for p in rcj:
        txt = p.read_text(encoding="utf-8", errors="replace")
        (TEMP / p.name).write_text(txt, encoding="utf-8")
        log(f"runtimeconfig {p.name} EventSource.IsSupported={'IsSupported' in txt} snippet_has_true={'true' in txt.lower()}")
        if "EventSource" in txt or "IsSupported" in txt:
            for line in txt.splitlines():
                if "EventSource" in line or "IsSupported" in line:
                    log("runtimeconfig line: " + line.strip())

    # Register (user-level loose package; no cert, no elevation)
    cmd = ["powershell", "-NoProfile", "-Command",
           f"Add-AppxPackage -Register '{MANIFEST}' ; $p=Get-AppxPackage -Name {PKG_NAME}; $p | Format-List Name,PackageFullName,InstallLocation,IsDevelopmentMode,SignatureKind,Status; if($p){{ 'REGISTER_OK ' + $p.PackageFullName }} else {{ 'REGISTER_FAIL' }}"]
    log("REGISTER cmd: Add-AppxPackage -Register <AppxManifest.xml>")
    r = run(cmd)
    (TEMP / "register.out.txt").write_text((r.stdout or "") + "\n---stderr---\n" + (r.stderr or "") + f"\nexit={r.returncode}\n", encoding="utf-8")
    log(f"register exit={r.returncode}")
    if "REGISTER_OK" not in (r.stdout or ""):
        log("STOP: package register did not report REGISTER_OK")
        sys.exit(2)

    # Move cursor away from the left nav before launch
    pt = wintypes.POINT()
    user32.GetCursorPos(ctypes.byref(pt))
    log(f"cursor saved x={pt.x} y={pt.y}")
    user32.SetCursorPos(8, 8)

    # One-shot scheduled task, interactive, limited, then delete after
    create = ["schtasks", "/Create", "/TN", TASK, "/TR", APP_USER, "/SC", "ONCE", "/ST", "00:00", "/IT", "/RL", "LIMITED", "/F"]
    log("SCHTASKS create: " + " ".join(create))
    r = run(create)
    (TEMP / "schtasks_create.txt").write_text((r.stdout or "") + (r.stderr or "") + f"\nexit={r.returncode}\n", encoding="utf-8")
    if r.returncode != 0:
        log("STOP: schtasks create failed")
        sys.exit(3)
    r = run(["schtasks", "/Run", "/TN", TASK])
    (TEMP / "schtasks_run.txt").write_text((r.stdout or "") + (r.stderr or "") + f"\nexit={r.returncode}\n", encoding="utf-8")
    log(f"schtasks run exit={r.returncode}")

    frames = TEMP / "frames"
    frames.mkdir(exist_ok=True)
    hwnd = pid = None
    # Poll for the window, then grab as many frames as possible.
    deadline = time.monotonic() + 30
    while time.monotonic() < deadline and hwnd is None:
        pid, hwnd = find_window()
        if hwnd is None:
            time.sleep(0.05)
    if hwnd is None:
        log("STOP: FluentHub window not found")
        run(["schtasks", "/Delete", "/TN", TASK, "/F"])
        sys.exit(4)
    log(f"window pid={pid} hwnd={hwnd} rect={window_rect(hwnd)}")

    # Capture burst (orchestration waits only; durations are not results)
    burst_end = time.monotonic() + 3.0
    n = 0
    while time.monotonic() < burst_end:
        dest = frames / f"frame_{n:04d}.png"
        info = capture(hwnd, dest)
        if info:
            n += 1
        time.sleep(0.02)
    log(f"captured_frames={n}")

    # One settled frame after the burst
    time.sleep(1.0)
    capture(hwnd, frames / "frame_settled.png")
    log("captured settled")

    # Close
    r = run(["powershell", "-NoProfile", "-Command",
             f"$p=Get-Process -Id {pid} -ErrorAction SilentlyContinue; if($p){{ $p.CloseMainWindow() | Out-Null; 'CLOSE_SENT' }} else {{ 'ALREADY_GONE' }}"])
    log("close: " + (r.stdout or "").strip())
    # Wait for exit without treating the wait as a measured result
    gone = False
    for _ in range(50):
        if run(["powershell", "-NoProfile", "-Command", f"if(Get-Process -Id {pid} -ErrorAction SilentlyContinue){{'LIVE'}}else{{'GONE'}}"]).stdout.strip() == "GONE":
            gone = True
            break
        time.sleep(0.2)
    log(f"process_exited={gone}")

    user32.SetCursorPos(pt.x, pt.y)
    r = run(["schtasks", "/Delete", "/TN", TASK, "/F"])
    log(f"schtasks delete exit={r.returncode}")
    r = run(["schtasks", "/Query", "/TN", TASK])
    log("schtasks query after delete exit=" + str(r.returncode))

    # Copy motion log
    local = Path(os.environ["LOCALAPPDATA"]) / "Packages"
    found = list(local.glob(f"{PKG_NAME}*/LocalState/{LOG_NAME}"))
    log("motion logs found: " + ", ".join(str(x) for x in found))
    for x in found:
        shutil.copy2(x, TEMP / "z6_f6_motion.log")
    pkg = run(["powershell", "-NoProfile", "-Command",
               f"Get-AppxPackage -Name {PKG_NAME} | Format-List Name,PackageFullName,IsDevelopmentMode,SignatureKind,Status"])
    (TEMP / "package_after.txt").write_text(pkg.stdout or "", encoding="utf-8")
    log("done")

if __name__ == "__main__":
    main()
