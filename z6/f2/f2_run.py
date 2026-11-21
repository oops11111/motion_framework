# -*- coding: utf-8 -*-
"""Z6 F2 GUI: register loose package, one-shot schtasks, PrintWindow current window only, wait both cycles, copy log, cleanup."""
import ctypes, os, subprocess, time, shutil
from pathlib import Path
from datetime import datetime, timezone, timedelta
from ctypes import wintypes

AWST = timezone(timedelta(hours=8))
OUT = Path(os.environ["TEMP"]) / "z6" / "f2" / "run"
OUT.mkdir(parents=True, exist_ok=True)
FRAMES = OUT / "frames"
FRAMES.mkdir(exist_ok=True)
USER = os.path.expandvars(r"%USERPROFILE%")
MANIFEST = Path(USER) / r"pilot\FluentHub\src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml"
TASK = "z6_f2_launch"
APP = r"explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App"
LOG_SRC = Path(USER) / r"AppData\Local\Packages\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm\LocalState\z6_f2_motion.log"
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

def read_motion():
    if not LOG_SRC.exists():
        return ""
    try:
        return LOG_SRC.read_text(encoding="utf-8", errors="replace")
    except Exception:
        return ""

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

shots = 0

def shot(tag):
    global shots
    hwnd = find_fluenthub_hwnd()
    if not hwnd:
        log("shot skip %s: no hwnd" % tag)
        return None
    dest = FRAMES / ("win_%02d_%s.png" % (shots, tag))
    try:
        print_window(hwnd, dest)
        log("shot %s hwnd=%s size=%s" % (dest.name, hwnd, dest.stat().st_size))
        shots += 1
        return dest
    except Exception as ex:
        log("shot fail %s: %s" % (tag, ex))
        return None

# 1) register
ps = "Add-AppxPackage -Register -Path '%s' | Format-List; Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Format-List Name,Status,InstallLocation,PackageFullName" % MANIFEST
r = run([str(WINPS), "-NoProfile", "-Command", ps], OUT / "register.out.txt")
log("register exit=%s" % r.returncode)

# 2) cursor away from the window
pt = wintypes.POINT()
user32.GetCursorPos(ctypes.byref(pt))
user32.SetCursorPos(8, 8)
log("cursor saved=(%s,%s) moved=(8,8)" % (pt.x, pt.y))

# 3) schtasks
r = run(["schtasks", "/Create", "/TN", TASK, "/TR", APP, "/SC", "ONCE", "/ST", "00:00", "/IT", "/RL", "LIMITED", "/F"], OUT / "schtasks_create.txt")
log("schtasks create exit=%s" % r.returncode)
r = run(["schtasks", "/Run", "/TN", TASK], OUT / "schtasks_run.txt")
log("schtasks run exit=%s" % r.returncode)

# 4) wait window, then poll log through cycle 2 settle
deadline = time.time() + 25
saw_win = False
saw_rebuild = False
saw_disp1 = False
saw_cyc2 = False
saw_cyc2_play = False
settle_after = None
last_shot = 0

while time.time() < deadline:
    if not saw_win:
        hwnd = find_fluenthub_hwnd()
        if hwnd:
            saw_win = True
            log("window hwnd=%s" % hwnd)
            shot("appear")
    text = read_motion()
    if (not saw_rebuild) and ("F2 REBUILD begin" in text):
        saw_rebuild = True
        log("log: F2 REBUILD begin")
        shot("rebuild")
    if (not saw_disp1) and ("F2 Dispose end cycle=1" in text):
        saw_disp1 = True
        log("log: F2 Dispose end cycle=1")
        shot("after_dispose1")
    if (not saw_cyc2) and ("cycle=2" in text and "PILOT start" in text):
        saw_cyc2 = True
        log("log: PILOT start cycle=2")
        shot("cycle2_start")
    if saw_cyc2 and (not saw_cyc2_play) and text.count("F6 PLAY after first frame") >= 2:
        saw_cyc2_play = True
        log("log: cycle2 PLAY seen")
        shot("cycle2_play")
        settle_after = time.time() + 2.5
    if settle_after and time.time() >= settle_after:
        shot("cycle2_settled")
        log("cycle2 settle wait done")
        break
    now = time.time()
    if saw_win and now - last_shot >= 0.4 and shots < 16:
        shot("tick")
        last_shot = now
    time.sleep(0.12)

text = read_motion()
nbytes = len(text.encode("utf-8")) if text else 0
log("poll end rebuild=%s disp1=%s cyc2=%s cyc2play=%s log_bytes=%s" % (saw_rebuild, saw_disp1, saw_cyc2, saw_cyc2_play, nbytes))
if LOG_SRC.exists():
    shutil.copy2(LOG_SRC, OUT / "z6_f2_motion_preclose.log")
    log("copied preclose log bytes=%s" % LOG_SRC.stat().st_size)

# 5) close
run([str(WINPS), "-NoProfile", "-Command",
     "Get-Process | Where-Object { $_.MainWindowTitle -eq 'FluentHub' } | ForEach-Object { $_.CloseMainWindow() }"])
close_deadline = time.time() + 8
saw_disp2 = False
while time.time() < close_deadline:
    text = read_motion()
    if "F2 Dispose end cycle=2" in text:
        saw_disp2 = True
        log("log: F2 Dispose end cycle=2")
        break
    time.sleep(0.2)
if not saw_disp2:
    log("WARN: no cycle-2 Dispose in log after CloseMainWindow")
    time.sleep(1.5)

# 6) delete task, restore cursor
r = run(["schtasks", "/Delete", "/TN", TASK, "/F"], OUT / "schtasks_delete.txt")
log("schtasks delete exit=%s" % r.returncode)
q = run(["schtasks", "/Query", "/TN", TASK], OUT / "schtasks_query_after.txt")
log("schtasks query after exit=%s" % q.returncode)
user32.SetCursorPos(pt.x, pt.y)
log("cursor restored")

# 7) copy log
if LOG_SRC.exists():
    shutil.copy2(LOG_SRC, OUT / "z6_f2_motion.log")
    log("copied motion log bytes=%s" % LOG_SRC.stat().st_size)
else:
    log("MISSING motion log at %s" % LOG_SRC)

log("done")
