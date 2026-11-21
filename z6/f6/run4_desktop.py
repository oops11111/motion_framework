import ctypes, os, threading, time, subprocess
from pathlib import Path
from datetime import datetime, timezone, timedelta
from PIL import ImageGrab, Image
from ctypes import wintypes

AWST = timezone(timedelta(hours=8))
OUT = Path(os.environ["TEMP"]) / "z6" / "f6" / "run4"
OUT.mkdir(parents=True, exist_ok=True)
FRAMES = OUT / "desktop"
FRAMES.mkdir(exist_ok=True)
TASK = "z6_f6_launch"
APP = r"explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App"

try:
    ctypes.windll.shcore.SetProcessDpiAwareness(2)
except Exception:
    ctypes.windll.user32.SetProcessDPIAware()

user32 = ctypes.windll.user32
stop = threading.Event()
nframes = [0]

def log(m):
    line = f"{datetime.now(AWST).isoformat()} {m}"
    print(line, flush=True)
    (OUT/"run.log").open("a", encoding="utf-8").write(line+"\n")

def grabber():
    while not stop.is_set():
        img = ImageGrab.grab(all_screens=True)
        dest = FRAMES / f"d_{nframes[0]:04d}.png"
        img.save(dest)
        nframes[0] += 1
        time.sleep(0.01)

# cursor away
pt = wintypes.POINT()
user32.GetCursorPos(ctypes.byref(pt))
user32.SetCursorPos(8, 8)

t = threading.Thread(target=grabber, daemon=True)
t.start()
log("grabber started")
time.sleep(0.15)

r = subprocess.run(["schtasks","/Create","/TN",TASK,"/TR",APP,"/SC","ONCE","/ST","00:00","/IT","/RL","LIMITED","/F"], capture_output=True, text=True)
log(f"schtasks create exit={r.returncode}")
r = subprocess.run(["schtasks","/Run","/TN",TASK], capture_output=True, text=True)
log(f"schtasks run exit={r.returncode}")

# let grabber run through startup + enter + settle
time.sleep(5.0)
stop.set()
t.join(timeout=3)
log(f"grabber stopped n={nframes[0]}")

# close app
subprocess.run(["powershell","-NoProfile","-Command",
    "Get-Process | Where-Object { $_.MainWindowTitle -eq 'FluentHub' } | ForEach-Object { $_.CloseMainWindow() }"], capture_output=True)
time.sleep(1)
subprocess.run(["schtasks","/Delete","/TN",TASK,"/F"], capture_output=True)
user32.SetCursorPos(pt.x, pt.y)

# analyze: file size / hash diversity, then crop later
import hashlib
files=sorted(FRAMES.glob("d_*.png"))
print("files", len(files))
prev=None
changes=[]
for f in files:
    h=hashlib.sha256(f.read_bytes()).hexdigest()
    if h!=prev:
        changes.append((f.name, f.stat().st_size, h[:12]))
        prev=h
print("unique", len(changes))
for c in changes[:40]:
    print(c)
if len(changes)>40:
    print("...")
    for c in changes[-5:]:
        print(c)
