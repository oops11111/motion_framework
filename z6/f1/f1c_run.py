# -*- coding: utf-8 -*-
"""Z6 F1C GUI: register loose package, one-shot schtasks, PrintWindow current window only, copy log, cleanup."""
import ctypes, os, subprocess, time, shutil
from pathlib import Path
from datetime import datetime, timezone, timedelta
from ctypes import wintypes

AWST = timezone(timedelta(hours=8))
OUT = Path(os.environ["TEMP"]) / "z6" / "f1c" / "run"
OUT.mkdir(parents=True, exist_ok=True)
FRAMES = OUT / "frames"
FRAMES.mkdir(exist_ok=True)
USER = os.path.expandvars(r"%USERPROFILE%")
MANIFEST = Path(USER) / r"pilot\FluentHub\src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml"
TASK = "z6_f1c_launch"
APP = r"explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App"
LOG_SRC = Path(USER) / r"AppData\Local\Packages\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm\LocalState\z6_f1c_motion.log"
WINPS = Path(os.environ["SystemRoot"]) / r"System32\WindowsPowerShell\v1.0\powershell.exe"

try:
    ctypes.windll.shcore.SetProcessDpiAwareness(2)
except Exception:
    ctypes.windll.user32.SetProcessDPIAware()

user32 = ctypes.windll.user32
PW_RENDERFULLCONTENT = 2

class RECT(ctypes.Structure):
    _fields_ = [("left", ctypes.c_long), ("top", ctypes.c_long),
                ("right", ctypes.c_long), ("bottom", ctypes.c_long)]

def log(m):
    line = f"{datetime.now(AWST).isoformat()} {m}"
    print(line, flush=True)
    (OUT / "run.log").open("a", encoding="utf-8").write(line + "\n")

def run(args, dest=None, shell=False):
    r = subprocess.run(args, capture_output=True, text=True, shell=shell)
    if dest:
        Path(dest).write_text((r.stdout or "") + (r.stderr or ""), encoding="utf-8")
    return r

def find_fluenthub_hwnd():
    found = []
    EnumWindowsProc = ctypes.WINFUNCTYPE(ctypes.c_bool, wintypes.HWND, wintypes.LPARAM)

    def cb(hwnd, lparam):
        if not user32.IsWindowVisible(hwnd):
            return True
        n = user32.GetWindowTextLengthW(hwnd)
        if n <= 0:
            return True
        buf = ctypes.create_unicode_buffer(n + 1)
        user32.GetWindowTextW(hwnd, buf, n + 1)
        if buf.value == "FluentHub":
            found.append(hwnd)
        return True

    user32.EnumWindows(EnumWindowsProc(cb), 0)
    return found[0] if found else None

def print_window(hwnd, dest: Path):
    rect = RECT()
    user32.GetWindowRect(hwnd, ctypes.byref(rect))
    w, h = rect.right - rect.left, rect.bottom - rect.top
    if w <= 0 or h <= 0:
        return False
    hdc = user32.GetDC(hwnd)
    gdi = ctypes.windll.gdi32
    mem = gdi.CreateCompatibleDC(hdc)
    bmp = gdi.CreateCompatibleBitmap(hdc, w, h)
    old = gdi.SelectObject(mem, bmp)
    ok = user32.PrintWindow(hwnd, mem, PW_RENDERFULLCONTENT)
    # BITMAP to PNG via PIL
    class BITMAPINFOHEADER(ctypes.Structure):
        _fields_ = [
            ("biSize", ctypes.c_uint32), ("biWidth", ctypes.c_int32), ("biHeight", ctypes.c_int32),
            ("biPlanes", ctypes.c_uint16), ("biBitCount", ctypes.c_uint16), ("biCompression", ctypes.c_uint32),
            ("biSizeImage", ctypes.c_uint32), ("biXPelsPerMeter", ctypes.c_int32), ("biYPelsPerMeter", ctypes.c_int32),
            ("biClrUsed", ctypes.c_uint32), ("biClrImportant", ctypes.c_uint32),
        ]
    class BITMAPINFO(ctypes.Structure):
        _fields_ = [("bmiHeader", BITMAPINFOHEADER), ("bmiColors", ctypes.c_uint32 * 3)]
    bmi = BITMAPINFO()
    bmi.bmiHeader.biSize = ctypes.sizeof(BITMAPINFOHEADER)
    bmi.bmiHeader.biWidth = w
    bmi.bmiHeader.biHeight = -h
    bmi.bmiHeader.biPlanes = 1
    bmi.bmiHeader.biBitCount = 32
    buf = (ctypes.c_ubyte * (w * h * 4))()
    gdi.GetDIBits(mem, bmp, 0, h, buf, ctypes.byref(bmi), 0)
    gdi.SelectObject(mem, old)
    gdi.DeleteObject(bmp)
    gdi.DeleteDC(mem)
    user32.ReleaseDC(hwnd, hdc)
    from PIL import Image
    img = Image.frombuffer("RGBA", (w, h), bytes(buf), "raw", "BGRA", 0, 1)
    img.save(dest)
    return ok != 0

# 1) register
ps = f"Add-AppxPackage -Register -Path '{MANIFEST}' | Format-List; Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Format-List Name,Status,InstallLocation,PackageFullName"
r = run([str(WINPS), "-NoProfile", "-Command", ps], OUT / "register.out.txt")
log(f"register exit={r.returncode}")

# 2) cursor away
pt = wintypes.POINT()
user32.GetCursorPos(ctypes.byref(pt))
user32.SetCursorPos(8, 8)
log(f"cursor saved=({pt.x},{pt.y}) moved=(8,8)")

# 3) schtasks
r = run(["schtasks", "/Create", "/TN", TASK, "/TR", APP, "/SC", "ONCE", "/ST", "00:00", "/IT", "/RL", "LIMITED", "/F"], OUT / "schtasks_create.txt")
log(f"schtasks create exit={r.returncode}")
r = run(["schtasks", "/Run", "/TN", TASK], OUT / "schtasks_run.txt")
log(f"schtasks run exit={r.returncode}")

# 4) wait for window and capture a few current-window frames
hwnd = None
deadline = time.time() + 8
shots = 0
while time.time() < deadline:
    hwnd = find_fluenthub_hwnd()
    if hwnd:
        dest = FRAMES / f"win_{shots:02d}.png"
        try:
            print_window(hwnd, dest)
            log(f"shot {dest.name} hwnd={hwnd} size={dest.stat().st_size}")
            shots += 1
        except Exception as ex:
            log(f"shot fail: {ex}")
        if shots >= 8:
            break
    time.sleep(0.15)

# extra settle then one settled frame
time.sleep(2.0)
hwnd = find_fluenthub_hwnd() or hwnd
if hwnd:
    dest = FRAMES / "win_settled.png"
    try:
        print_window(hwnd, dest)
        log(f"shot settled size={dest.stat().st_size}")
    except Exception as ex:
        log(f"settled fail: {ex}")

# 5) close
run([str(WINPS), "-NoProfile", "-Command",
     "Get-Process | Where-Object { $_.MainWindowTitle -eq 'FluentHub' } | ForEach-Object { $_.CloseMainWindow() }"])
time.sleep(1.5)
# if still running, wait a bit more for Dispose/flush
time.sleep(0.5)

# 6) delete task, restore cursor
r = run(["schtasks", "/Delete", "/TN", TASK, "/F"], OUT / "schtasks_delete.txt")
log(f"schtasks delete exit={r.returncode}")
q = run(["schtasks", "/Query", "/TN", TASK], OUT / "schtasks_query_after.txt")
log(f"schtasks query after exit={q.returncode}")
user32.SetCursorPos(pt.x, pt.y)
log("cursor restored")

# 7) copy log
if LOG_SRC.exists():
    shutil.copy2(LOG_SRC, OUT / "z6_f1c_motion.log")
    log(f"copied motion log bytes={LOG_SRC.stat().st_size}")
else:
    log(f"MISSING motion log at {LOG_SRC}")

# package still registered; caller may unregister later
log("done")
