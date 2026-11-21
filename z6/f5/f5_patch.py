# -*- coding: utf-8 -*-
"""Z6 F5: follow UISettings.AnimationsEnabled changes while running (subscribe, marshal to UI thread)."""
from pathlib import Path
import os, hashlib, shutil, subprocess, sys

cs = Path(os.path.expandvars(r"%USERPROFILE%\pilot\FluentHub\src\FluentHub\Views\NavMotionPilot.cs"))
tmp = Path(os.path.expandvars(r"%TEMP%\z6\f5"))
tmp.mkdir(parents=True, exist_ok=True)
pre = tmp / "pre_f5_NavMotionPilot.cs"
if pre.exists():
    shutil.copy2(pre, cs)   # rerun: restore the pre-F5 file before applying again
else:
    shutil.copy2(cs, pre)

raw = cs.read_bytes().replace(b"\r\n", b"\n").replace(b"\r", b"\n").decode("utf-8-sig")


def must_replace(buf, old, new, tag):
    if old not in buf:
        raise SystemExit(f"pattern not found: {tag}\n---\n{old[:240]}")
    n = buf.count(old)
    if n != 1:
        raise SystemExit(f"pattern count {n} != 1: {tag}")
    return buf.replace(old, new)


# --- class summary ---
raw = must_replace(raw,
    "\t/// F1C hover enqueue unchanged. F2: after first enter Play, DispatcherQueue.TryEnqueue RootView forceReload (Unloaded Dispose, then Loaded Create).\n",
    "\t/// F1C hover enqueue unchanged. F2: after first enter Play, DispatcherQueue.TryEnqueue RootView forceReload (Unloaded Dispose, then Loaded Create).\n"
    "\t/// F5: subscribe UISettings.AnimationsEnabledChanged and call MotionSystem.SetReducedMotion(!AnimationsEnabled) on the UI thread.\n",
    "summary")

# --- fields ---
raw = must_replace(raw,
    "\t\tprivate static int s_f2Cycle;\n",
    "\t\tprivate readonly UISettings _uiSettings;\n"
    "\t\tprivate int _f5EventCount;\n"
    "\t\tprivate bool _f5AppliedAnimationsEnabled;\n"
    "\t\tprivate static int s_f2Cycle;\n",
    "f5 fields")

# --- using for the platform guard ---
raw = must_replace(raw,
    "using System.Diagnostics.Tracing;\n",
    "using System.Diagnostics.Tracing;\n"
    "using System.Runtime.Versioning;\n",
    "using")

# --- startup read -> subscribe (moved after the DispatcherQueue is known) ---
raw = must_replace(raw,
    "\n\t\t\tbool animationsEnabled = new UISettings().AnimationsEnabled;\n"
    "\t\t\t_log.Line($\"UISettings.AnimationsEnabled={animationsEnabled} -> SetReducedMotion({!animationsEnabled})\");\n"
    "\t\t\t_system.SetReducedMotion(!animationsEnabled);\n",
    "",
    "remove one-shot read")

raw = must_replace(raw,
    "\t\t\t_dispatcher = navigationView.DispatcherQueue;\n"
    "\t\t\t_nav = navigationView;\n",
    "\t\t\t_dispatcher = navigationView.DispatcherQueue;\n"
    "\t\t\t_nav = navigationView;\n"
    "\n"
    "\t\t\t// Z6-F5: follow the system setting while running. Startup value is read once, but the subscription keeps\n"
    "\t\t\t// SetReducedMotion in sync afterwards (was: a single read at startup only).\n"
    "\t\t\t_uiSettings = new UISettings();\n"
    "\t\t\t// AnimationsEnabledChanged needs Windows 10 19041; the project declares 17763 as its minimum, so guard the\n"
    "\t\t\t// subscription explicitly (CA1416) instead of raising the app minimum.\n"
    "\t\t\tbool f5CanSubscribe = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041);\n"
    "\t\t\tif (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))\n"
    "\t\t\t{\n"
    "\t\t\t\tSubscribeAnimationsChanged();\n"
    "\t\t\t}\n"
    "\t\t\tbool animationsEnabled = _uiSettings.AnimationsEnabled;\n"
    "\t\t\t_f5AppliedAnimationsEnabled = animationsEnabled;\n"
    "\t\t\t_log.Line($\"F5 subscribed UISettings.AnimationsEnabledChanged={f5CanSubscribe}; initial AnimationsEnabled={animationsEnabled} -> SetReducedMotion({!animationsEnabled})\");\n"
    "\t\t\t_system.SetReducedMotion(!animationsEnabled);\n"
    "\t\t\t_log.Line($\"F5 initial SetReducedMotion({!animationsEnabled}) applied on UI thread thread={Environment.CurrentManagedThreadId}\");\n"
    "\n"
    "\t\t\t// Dev-only selftest: run the same enqueued path once with the current value. No state change, and not\n"
    "\t\t\t// evidence of a system-setting switch; it only proves the DispatcherQueue hand-off works.\n"
    "\t\t\tbool f5SelfQueued = _dispatcher.TryEnqueue(() => ApplyReducedMotion(animationsEnabled, \"selftest\"));\n"
    "\t\t\t_log.Line($\"F5 selftest enqueue ok={f5SelfQueued} (same value, no state change)\");\n",
    "subscribe after dispatcher")

# --- handler + applier, inserted before OnWindowClosed ---
raw = must_replace(raw,
    "\t\tprivate void OnWindowClosed(object sender, WindowEventArgs args)\n",
    "\t\t[SupportedOSPlatform(\"windows10.0.19041.0\")]\n"
    "\t\tprivate void SubscribeAnimationsChanged() => _uiSettings.AnimationsEnabledChanged += OnAnimationsEnabledChanged;\n"
    "\n"
    "\t\t[SupportedOSPlatform(\"windows10.0.19041.0\")]\n"
    "\t\tprivate void UnsubscribeAnimationsChanged() => _uiSettings.AnimationsEnabledChanged -= OnAnimationsEnabledChanged;\n"
    "\n"
    "\t\tprivate void OnAnimationsEnabledChanged(UISettings sender, UISettingsAnimationsEnabledChangedEventArgs args)\n"
    "\t\t{\n"
    "\t\t\t// WinRT raises this on a thread-pool thread. MotionSystem is thread-affine (contract Table 2), so only\n"
    "\t\t\t// read the new value here and marshal the API call to the UI thread that created the system.\n"
    "\t\t\tbool animationsEnabled = sender.AnimationsEnabled;\n"
    "\t\t\t_f5EventCount++;\n"
    "\t\t\t_log.Line($\"F5 AnimationsEnabledChanged n={_f5EventCount} enabled={animationsEnabled} thread={Environment.CurrentManagedThreadId} -> TryEnqueue SetReducedMotion({!animationsEnabled})\");\n"
    "\t\t\tbool queued = _dispatcher.TryEnqueue(() => ApplyReducedMotion(animationsEnabled, \"event\"));\n"
    "\t\t\t_log.Line($\"F5 AnimationsEnabledChanged n={_f5EventCount} enqueue ok={queued}\");\n"
    "\t\t\t_log.Flush();\n"
    "\t\t}\n"
    "\n"
    "\t\tprivate void ApplyReducedMotion(bool animationsEnabled, string source)\n"
    "\t\t{\n"
    "\t\t\t// UI thread only (DispatcherQueue). SetReducedMotion is thread-affine and may snap running animations to their end.\n"
    "\t\t\tif (_disposed)\n"
    "\t\t\t{\n"
    "\t\t\t\treturn;\n"
    "\t\t\t}\n"
    "\n"
    "\t\t\tbool reduced = !animationsEnabled;\n"
    "\t\t\t_log.Line($\"F5 SetReducedMotion({reduced}) source={source} animationsEnabled={animationsEnabled} previous={_f5AppliedAnimationsEnabled} thread={Environment.CurrentManagedThreadId}\");\n"
    "\t\t\t_system.SetReducedMotion(reduced);\n"
    "\t\t\t_f5AppliedAnimationsEnabled = animationsEnabled;\n"
    "\t\t\t_log.Line($\"F5 SetReducedMotion({reduced}) applied; last_animationsEnabled={_f5AppliedAnimationsEnabled}\");\n"
    "\t\t\t_log.Flush();\n"
    "\t\t}\n"
    "\n"
    "\t\tprivate void OnWindowClosed(object sender, WindowEventArgs args)\n",
    "handler")

# --- unsubscribe on dispose ---
raw = must_replace(raw,
    "\t\t\t_system.Dispose();\n",
    "\t\t\tif (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))\n"
    "\t\t\t{\n"
    "\t\t\t\tUnsubscribeAnimationsChanged();\n"
    "\t\t\t}\n"
    "\t\t\t_log.Line($\"F5 unsubscribed AnimationsEnabledChanged events={_f5EventCount}\");\n"
    "\t\t\t_system.Dispose();\n",
    "unsubscribe")

cs.write_bytes(raw.encode("utf-8"))
print("POST_CS", len(raw.encode()), hashlib.sha256(raw.encode()).hexdigest())
print("PRE_CS", pre.stat().st_size, hashlib.sha256(pre.read_bytes()).hexdigest())

# patch (a/b paths relative to the FluentHub repo root)
post = tmp / "post_f5_NavMotionPilot.cs"
post.write_bytes(raw.encode("utf-8"))
out = Path(os.path.expandvars(r"%USERPROFILE%\mf_z6\f5\z6\f5\fluenthub_f5_NavMotionPilot.patch"))
rel = "src/FluentHub/Views/NavMotionPilot.cs"
diffdir = tmp / "diff"
diffdir.mkdir(exist_ok=True)
shutil.copy2(pre, diffdir / "pre.cs")
shutil.copy2(post, diffdir / "post.cs")
p = subprocess.run(["git", "diff", "--no-index", "--no-color", "--src-prefix=a/", "--dst-prefix=b/",
                    "pre.cs", "post.cs"], capture_output=True, text=True, encoding="utf-8", cwd=str(diffdir))
if p.returncode != 1:
    raise SystemExit(f"git diff --no-index rc={p.returncode}\n{p.stderr}")
body = (p.stdout
        .replace("a/pre.cs", "a/" + rel)
        .replace("b/post.cs", "b/" + rel))
out.write_bytes(body.encode("utf-8"))
print("PATCH", out.name, hashlib.sha256(body.encode("utf-8")).hexdigest())
