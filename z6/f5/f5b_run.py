# -*- coding: utf-8 -*-
"""Z6 F5 continuation (F5b): register the loose package, launch FluentHub once via a one-shot schtasks task,
and while it runs toggle the *user-level* system animation setting off and on with
SystemParametersInfoW(SPI_SETCLIENTAREAANIMATION) + SPIF_UPDATEINIFILE|SPIF_SENDCHANGE, then restore the
recorded original value. No elevation, no HKLM. Reads back SPI and HKCU values after every step and copies
the shared pilot log. Does not change any product code.

pidan's word (2026-11-18 13:23 AWST): "F5 批准切换系统动画设置，测完恢复原值"
"""
import ctypes, os, shutil, subprocess, time, winreg
from pathlib import Path
from datetime import datetime, timezone, timedelta
from ctypes import wintypes

AWST = timezone(timedelta(hours=8))
OUT = Path(os.environ["TEMP"]) / "z6" / "f5b" / "run"
OUT.mkdir(parents=True, exist_ok=True)
FRAMES = OUT / "frames"
FRAMES.mkdir(exist_ok=True)
USER = os.path.expandvars(r"%USERPROFILE%")
MANIFEST = Path(USER) / r"pilot\FluentHub\src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppxManifest.xml"
TASK = "z6_f5b_launch"
APP = r"explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App"
LOG_SRC = Path(USER) / r"AppData\Local\Packages\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm\LocalState\z6_f2_motion.log"
WINPS = Path(os.environ["SystemRoot"]) / r"System32\WindowsPowerShell\v1.0\powershell.exe"

SPI_GETCLIENTAREAANIMATION = 0x1042
SPI_SETCLIENTAREAANIMATION = 0x1043
SPIF_UPDATEINIFILE = 0x01
SPIF_SENDCHANGE = 0x02

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


def run(args, dest=None):
    r = subprocess.run(args, capture_output=True, text=True, shell=False)
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


def read_reg():
    mi = winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Control Panel\Desktop\WindowMetrics")
    min_animate, _ = winreg.QueryValueEx(mi, "MinAnimate")
    winreg.CloseKey(mi)
    dk = winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Control Panel\Desktop")
    upm, _ = winreg.QueryValueEx(dk, "UserPreferencesMask")
    winreg.CloseKey(dk)
    return int(min_animate), bytes(upm)


def spi_get():
    v = ctypes.c_int(0)
    ok = user32.SystemParametersInfoW(SPI_GETCLIENTAREAANIMATION, 0, ctypes.byref(v), 0)
    return ok, v.value


def spi_set(value):
    """value: 0/1. Try the by-value form first, fall back to by-ref; verify with SPI_GET."""
    flags = SPIF_UPDATEINIFILE | SPIF_SENDCHANGE
    ok1 = user32.SystemParametersInfoW(SPI_SETCLIENTAREAANIMATION, 0, ctypes.c_void_p(value), flags)
    time.sleep(0.6)
    gok, got = spi_get()
    if gok and got == value:
        return ("byval", ok1, gok, got)
    v = ctypes.c_int(value)
    ok2 = user32.SystemParametersInfoW(SPI_SETCLIENTAREAANIMATION, 0, ctypes.byref(v), flags)
    time.sleep(0.6)
    gok, got = spi_get()
    return ("byref", ok2, gok, got)


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


def wait_for_event(prev_text, timeout):
    """Wait until a new 'source=event' line appears; return (text, elapsed, appeared)."""
    t0 = time.time()
    while time.time() - t0 < timeout:
        txt = read_motion()
        new = txt[len(prev_text):] if txt.startswith(prev_text) else txt
        if "source=event" in new:
            return txt, time.time() - t0, True
        time.sleep(0.15)
    return read_motion(), time.time() - t0, False


# ---------------- PRE ----------------
orig_spi_ok, orig_spi = spi_get()
orig_min, orig_upm = read_reg()
orig = orig_spi
(OUT / "settings_pre_script.txt").write_text(
    "source=SystemParametersInfoW SPI_GETCLIENTAREAANIMATION\n"
    f"spi_get ok={orig_spi_ok} value={orig_spi}\n"
    "source=HKCU\\Control Panel\\Desktop\\WindowMetrics\n"
    f"MinAnimate={orig_min}\n"
    "source=HKCU\\Control Panel\\Desktop\n"
    f"UserPreferencesMask={' '.join('%02X' % b for b in orig_upm)}\n",
    encoding="utf-8")
log(f"PRE spi ok={orig_spi_ok} value={orig_spi} MinAnimate={orig_min} UPM={' '.join('%02X' % b for b in orig_upm)}")

# 0) stale package / task
run([str(WINPS), "-NoProfile", "-Command",
     "Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Format-List Name,PackageFullName,InstallLocation"],
    OUT / "preexisting_package.txt")
run(["schtasks", "/Delete", "/TN", TASK, "/F"], OUT / "schtasks_delete_stale.txt")

# 1) register loose package
ps = ("Add-AppxPackage -Register -Path '%s' | Format-List; "
      "Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Format-List Name,Status,InstallLocation,PackageFullName") % MANIFEST
r = run([str(WINPS), "-NoProfile", "-Command", ps], OUT / "register.out.txt")
log("register exit=%s" % r.returncode)

# 2) cursor away from the window
pt = wintypes.POINT()
user32.GetCursorPos(ctypes.byref(pt))
user32.SetCursorPos(8, 8)
log("cursor saved=(%s,%s) moved=(8,8)" % (pt.x, pt.y))

# 3) one-shot task, interactive, limited (no elevation)
r = run(["schtasks", "/Create", "/TN", TASK, "/TR", APP, "/SC", "ONCE", "/ST", "00:00",
         "/IT", "/RL", "LIMITED", "/F"], OUT / "schtasks_create.txt")
log("schtasks create exit=%s" % r.returncode)
r = run(["schtasks", "/Run", "/TN", TASK], OUT / "schtasks_run.txt")
log("schtasks run exit=%s" % r.returncode)

# 4) wait for window, subscription (both F2 cycles), and the selftest path
deadline = time.time() + 40
saw_win = saw_sub2 = saw_play = False
last_shot = 0
while time.time() < deadline:
    hwnd = find_fluenthub_hwnd()
    if hwnd and not saw_win:
        saw_win = True
        log("window hwnd=%s" % hwnd)
        shot("appear")
    txt = read_motion()
    subs = txt.count("F5 subscribed UISettings.AnimationsEnabledChanged")
    if subs >= 2:
        saw_sub2 = True
    if "F6 PLAY after first frame" in txt:
        saw_play = True
    if saw_win and saw_sub2 and saw_play:
        break
    if saw_win and time.time() - last_shot >= 0.5 and shots < 4:
        shot("tick")
        last_shot = time.time()
    time.sleep(0.12)

log("startup sub_lines=%s play=%s win=%s" % (read_motion().count("F5 subscribed UISettings.AnimationsEnabledChanged"), saw_play, saw_win))
time.sleep(3.0)          # let the F2 rebuild settle so the live instance is the one that receives events
shot("settled")
base_text = read_motion()
base_lines = base_text.count("\n")
log("baseline log lines=%s bytes=%s" % (base_lines, len(base_text.encode("utf-8"))))

# ---------------- toggle OFF ----------------
t_off_req = datetime.now(AWST).isoformat()
log(f"TOGGLE OFF request t={t_off_req}")
mode, ok, gok, got = spi_set(0)
t_off_done = datetime.now(AWST).isoformat()
min_off, upm_off = read_reg()
log(f"TOGGLE OFF done t={t_off_done} mode={mode} spi_set_ret={ok} spi_read_ok={gok} spi_read={got} "
    f"MinAnimate={min_off} UPM={' '.join('%02X' % b for b in upm_off)}")
text_off, elapsed_off, seen_off = wait_for_event(base_text, 12.0)
delta_off = text_off[len(base_text):] if text_off.startswith(base_text) else text_off
(OUT / "log_after_off.txt").write_text(delta_off, encoding="utf-8")
log(f"TOGGLE OFF event_seen={seen_off} waited={elapsed_off:.2f}s new_lines={delta_off.count(chr(10))}")
shot("anim_off")

# ---------------- toggle ON ----------------
t_on_req = datetime.now(AWST).isoformat()
log(f"TOGGLE ON request t={t_on_req}")
mode, ok, gok, got = spi_set(1)
t_on_done = datetime.now(AWST).isoformat()
min_on, upm_on = read_reg()
log(f"TOGGLE ON done t={t_on_done} mode={mode} spi_set_ret={ok} spi_read_ok={gok} spi_read={got} "
    f"MinAnimate={min_on} UPM={' '.join('%02X' % b for b in upm_on)}")
text_on, elapsed_on, seen_on = wait_for_event(text_off, 12.0)
delta_on = text_on[len(text_off):] if text_on.startswith(text_off) else text_on
(OUT / "log_after_on.txt").write_text(delta_on, encoding="utf-8")
log(f"TOGGLE ON event_seen={seen_on} waited={elapsed_on:.2f}s new_lines={delta_on.count(chr(10))}")
shot("anim_on")

# ---------------- restore original ----------------
t_res_req = datetime.now(AWST).isoformat()
log(f"RESTORE request t={t_res_req} original={orig}")
mode, ok, gok, got = spi_set(orig)
time.sleep(1.0)
rest_spi_ok, rest_spi = spi_get()
rest_min, rest_upm = read_reg()
if rest_min != orig_min:
    log("MinAnimate drift after restore -> writing original value back (user-level)")
    mi = winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Control Panel\Desktop\WindowMetrics", 0, winreg.KEY_SET_VALUE)
    winreg.SetValueEx(mi, "MinAnimate", 0, winreg.REG_SZ, str(orig_min))
    winreg.CloseKey(mi)
    time.sleep(0.4)
    rest_min, rest_upm = read_reg()
if rest_upm != orig_upm:
    log("UPM drift after restore -> writing original UserPreferencesMask bytes back (user-level)")
    dk = winreg.OpenKey(winreg.HKEY_CURRENT_USER, r"Control Panel\Desktop", 0, winreg.KEY_SET_VALUE)
    winreg.SetValueEx(dk, "UserPreferencesMask", 0, winreg.REG_BINARY, orig_upm)
    winreg.CloseKey(dk)
    user32.SendMessageTimeoutW(0xFFFF, 0x001A, 0, ctypes.c_wchar_p("ImmersiveColorSet"), 0x0002, 1000, ctypes.byref(ctypes.c_ulong()))
    time.sleep(0.6)
    rest_min, rest_upm = read_reg()
rest_ok = (rest_spi == orig) and (rest_min == orig_min) and (rest_upm == orig_upm)
log(f"RESTORE done t={datetime.now(AWST).isoformat()} spi_read_ok={rest_spi_ok} spi_read={rest_spi} "
    f"MinAnimate={rest_min} UPM={' '.join('%02X' % b for b in rest_upm)} matches_original={rest_ok}")
(OUT / "settings_post_script.txt").write_text(
    f"original_spi={orig} restored_spi={rest_spi}\n"
    f"original_MinAnimate={orig_min} restored_MinAnimate={rest_min}\n"
    f"original_UserPreferencesMask={' '.join('%02X' % b for b in orig_upm)}\n"
    f"restored_UserPreferencesMask={' '.join('%02X' % b for b in rest_upm)}\n"
    f"matches_original={rest_ok}\n",
    encoding="utf-8")

# 5) close
run([str(WINPS), "-NoProfile", "-Command",
     "Get-Process | Where-Object { $_.MainWindowTitle -eq 'FluentHub' } | ForEach-Object { $_.CloseMainWindow() }"],
    OUT / "close.out.txt")
close_deadline = time.time() + 10
while time.time() < close_deadline:
    if "Dispose end" in read_motion():
        log("log: Dispose end seen after close")
        break
    time.sleep(0.2)

# 6) delete task, restore cursor
r = run(["schtasks", "/Delete", "/TN", TASK, "/F"], OUT / "schtasks_delete.txt")
log("schtasks delete exit=%s" % r.returncode)
q = run(["schtasks", "/Query", "/TN", TASK], OUT / "schtasks_query_after.txt")
log("schtasks query after exit=%s (1 = not found)" % q.returncode)
user32.SetCursorPos(pt.x, pt.y)
log("cursor restored")

# 7) copy log, unregister
if LOG_SRC.exists():
    shutil.copy2(LOG_SRC, OUT / "z6_f5b_motion.log")
    log("copied motion log bytes=%s" % LOG_SRC.stat().st_size)
else:
    log("MISSING motion log at %s" % LOG_SRC)
r = run([str(WINPS), "-NoProfile", "-Command",
         "Remove-AppxPackage -Package (Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub).PackageFullName; "
         "Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Format-List Name"],
        OUT / "unregister.out.txt")
log("unregister exit=%s" % r.returncode)
log("done")
