# -*- coding: utf-8 -*-
"""Z6 F1C patch: sink sees enter mid-flight -> DispatcherQueue.TryEnqueue hover. No fm_* in sink/render."""
from pathlib import Path
import os, hashlib

cs = Path(os.path.expandvars(r"%USERPROFILE%\pilot\FluentHub\src\FluentHub\Views\NavMotionPilot.cs"))
raw = cs.read_bytes().replace(b"\r\n", b"\n").replace(b"\r", b"\n").decode("utf-8-sig")

def must_replace(old, new, tag):
    global raw
    if old not in raw:
        raise SystemExit(f"pattern not found: {tag}\n---\n{old[:200]}")
    n = raw.count(old)
    if n != 1:
        raise SystemExit(f"pattern count {n} != 1: {tag}")
    raw = raw.replace(old, new)
    print("ok", tag)

must_replace(
    "\t/// Left NavigationView menu items: Loaded, wait for first frame, then Play \"demo.enter\" (tokens), PointerEntered \u2192 Retarget offsetX=8,\n"
    "\t/// PointerExited/Canceled/CaptureLost \u2192 Retarget the same handle back to 0, Unloaded/Closed \u2192 Cancel+Release every handle, then Dispose.",
    "\t/// Left NavigationView menu items: Loaded, wait for first frame, then Play \"demo.enter\" (tokens).\n"
    "\t/// F1C: after the sink sees enter mid-flight, DispatcherQueue.TryEnqueue hovers (Retarget offsetX=8). No fm_* in sink/render. Pointer hover ignored.",
    "summary",
)

must_replace(
    "\t\tprivate bool _enterPlayed;\n"
    "\t\tprivate int _enterShots;\n"
    "\t\tprivate bool _enterShotBusy;\n"
    "\t\tprivate int _sinkCount;\n",
    "\t\tprivate bool _enterPlayed;\n"
    "\t\tprivate int _enterShots;\n"
    "\t\tprivate bool _enterShotBusy;\n"
    "\t\tprivate int _sinkCount;\n"
    "\t\tprivate bool _f1cEnqueued;\n"
    "\t\tprivate bool _f1cHoverPlayed;\n"
    "\t\tprivate int _f1cOverlap;\n"
    "\t\tprivate double? _f1cLastOffsetX;\n"
    "\t\tprivate double? _f1cLastOpacity;\n"
    "\t\tprivate double? _f1cMidObserved;\n"
    "\t\tprivate const ulong F1cTarget = 1;\n",
    "fields",
)

must_replace(
    '_log.Line("PILOT start (Z6-F6, wait for first frame then Play enter; T5 hover unchanged)");',
    '_log.Line("PILOT start (Z6-F1C, program-state hover after sink sees enter mid-flight; enqueue via DispatcherQueue; F6 enter wait unchanged)");',
    "start line",
)

must_replace(
    '\t\t\t_log.Line("F6 waiting for first CompositionTarget.Rendering before Play");\n\t\t\t_log.Flush();',
    '\t\t\t_log.Line("F6 waiting for first CompositionTarget.Rendering before Play");\n'
    '\t\t\t_log.Line("F1C will enqueue hover from sink mid-flight via DispatcherQueue; no fm_* in sink/render; pointer hover ignored");\n'
    '\t\t\t_log.Flush();',
    "f1c plan log",
)

must_replace(
    """\t\tprivate void OnItemPointerEntered(object sender, PointerRoutedEventArgs e)
\t\t{
\t\t\tif (_disposed || sender is not NavigationViewItem item || !_targets.TryGetValue(item, out ulong target))
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\tif (!_hover.TryGetValue(target, out AnimationHandle? hover))
\t\t\t{
\t\t\t\t// Empty animation (retarget-only) + Play, then Retarget; created once per item and reused.
\t\t\t\thover = _system.CreateAnimation(target, null);
\t\t\t\thover.Play();
\t\t\t\t_hover[target] = hover;
\t\t\t}

\t\t\tbool ok = hover.Retarget(OffsetX, HoverOffset, Snappy);
\t\t\t_log.Line($"hover-in target={target} anim={hover.Id} retarget({OffsetX}={HoverOffset}) ok={ok}");
\t\t}

\t\tprivate void OnItemPointerLeft(object sender, PointerRoutedEventArgs e)
\t\t{
\t\t\tif (_disposed || sender is not NavigationViewItem item || !_targets.TryGetValue(item, out ulong target))
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\tif (_hover.TryGetValue(target, out AnimationHandle? hover))
\t\t\t{
\t\t\t\t// Same handle back to 0 (no Cancel + new handle: Cancel would freeze the value as rest).
\t\t\t\tbool ok = hover.Retarget(OffsetX, 0, Snappy);
\t\t\t\t_log.Line($"hover-out target={target} anim={hover.Id} retarget({OffsetX}=0) ok={ok}");
\t\t\t\t_log.Flush();
\t\t\t}
\t\t}
""",
    """\t\tprivate void OnItemPointerEntered(object sender, PointerRoutedEventArgs e)
\t\t{
\t\t\t// F1C: do not rely on human hover; ignore pointer so the interrupt is program-state only.
\t\t\tif (!_disposed)
\t\t\t{
\t\t\t\t_log.Line("F1C ignore pointer-enter (program-state hover only)");
\t\t\t}
\t\t}

\t\tprivate void OnItemPointerLeft(object sender, PointerRoutedEventArgs e)
\t\t{
\t\t\tif (!_disposed)
\t\t\t{
\t\t\t\t_log.Line("F1C ignore pointer-leave (program-state hover only)");
\t\t\t}
\t\t}

\t\tprivate void PlayHoverAfterMidFlight()
\t\t{
\t\t\t// Runs on DispatcherQueue, not inside sink/render. Same hover path as T5 (empty anim + Play + Retarget offsetX=8, no from).
\t\t\tif (_disposed || _f1cHoverPlayed)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\tif (!_hover.TryGetValue(F1cTarget, out AnimationHandle? hover))
\t\t\t{
\t\t\t\thover = _system.CreateAnimation(F1cTarget, null);
\t\t\t\thover.Play();
\t\t\t\t_hover[F1cTarget] = hover;
\t\t\t}

\t\t\tbool ok = hover.Retarget(OffsetX, HoverOffset, Snappy);
\t\t\t_f1cHoverPlayed = true;
\t\t\t_log.Line($"F1C PLAY hover after mid-flight enqueue (not in sink/render) target={F1cTarget} anim={hover.Id} retarget({OffsetX}={HoverOffset}) ok={ok} mid_observed={_f1cMidObserved}");
\t\t\t_log.Flush();
\t\t}
""",
    "pointer + PlayHoverAfterMidFlight",
)

must_replace(
    """\t\tprivate void OnValueChanged(ulong target, string property, double value)
\t\t{
\t\t\t// Sink: never call back into the motion API from here.
\t\t\tif (_disposed)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\t_sinkCount++;
\t\t\tif (property == OffsetX && _transforms.TryGetValue(target, out TranslateTransform? transform))
\t\t\t{
\t\t\t\ttransform.X = value;
\t\t\t}
\t\t\telse if (property == OpacityProperty && _items.TryGetValue(target, out NavigationViewItem? item))
\t\t\t{
\t\t\t\titem.Opacity = Math.Clamp(value, 0, 1);
\t\t\t}
\t\t}
""",
    """\t\tprivate void OnValueChanged(ulong target, string property, double value)
\t\t{
\t\t\t// Sink: never call back into the motion API from here (L1 contract Table 2). Observation + TryEnqueue only.
\t\t\tif (_disposed)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\t_sinkCount++;
\t\t\tif (property == OffsetX && _transforms.TryGetValue(target, out TranslateTransform? transform))
\t\t\t{
\t\t\t\ttransform.X = value;
\t\t\t}
\t\t\telse if (property == OpacityProperty && _items.TryGetValue(target, out NavigationViewItem? item))
\t\t\t{
\t\t\t\titem.Opacity = Math.Clamp(value, 0, 1);
\t\t\t}

\t\t\tif (target != F1cTarget)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\tif (property == OffsetX)
\t\t\t{
\t\t\t\t_f1cLastOffsetX = value;
\t\t\t}
\t\t\telse if (property == OpacityProperty)
\t\t\t{
\t\t\t\t_f1cLastOpacity = value;
\t\t\t}

\t\t\t// Enter mid-flight on offsetX: token starts at -80 and ends at 0. Do not Play/Retarget here.
\t\t\tif (_enterPlayed && !_f1cEnqueued && property == OffsetX && value > -80.0 && value < 0.0)
\t\t\t{
\t\t\t\t_f1cEnqueued = true;
\t\t\t\t_f1cMidObserved = value;
\t\t\t\t_log.Line($"F1C SINK mid-flight target={target} property={property} value={value} (enqueue hover; no fm_* in this callback)");
\t\t\t\tbool queued = _dispatcher.TryEnqueue(PlayHoverAfterMidFlight);
\t\t\t\t_log.Line($"F1C SINK enqueue hover ok={queued}");
\t\t\t\t_log.Flush();
\t\t\t}

\t\t\tif (_f1cHoverPlayed && _f1cLastOffsetX.HasValue && _f1cLastOpacity.HasValue)
\t\t\t{
\t\t\t\tdouble ox = _f1cLastOffsetX.Value;
\t\t\t\tdouble op = _f1cLastOpacity.Value;
\t\t\t\tif (op > 0.0 && op < 1.0 && System.Math.Abs(ox - HoverOffset) > 1e-6)
\t\t\t\t{
\t\t\t\t\t_f1cOverlap++;
\t\t\t\t\tif (_f1cOverlap <= 12)
\t\t\t\t\t{
\t\t\t\t\t\t_log.Line($"F1C_OVERLAP n={_f1cOverlap} target={target} offsetX={ox} opacity={op}");
\t\t\t\t\t}
\t\t\t\t}
\t\t\t}
\t\t}
""",
    "OnValueChanged",
)

# log F1C_FINAL on dispose begin (before cancel)
must_replace(
    "\t\t\t_log.Line(\"Dispose begin\");",
    "\t\t\t_log.Line($\"F1C_FINAL offsetX={_f1cLastOffsetX} opacity={_f1cLastOpacity} overlapSinkNotes={_f1cOverlap} enqueued={_f1cEnqueued} hoverPlayed={_f1cHoverPlayed} mid_observed={_f1cMidObserved} expected_offsetX={HoverOffset} (later writer target)\");\n"
    "\t\t\t_log.Line(\"Dispose begin\");",
    "final log",
)

must_replace(
    '_path = Path.Combine(folder, "z6_f6_motion.log");',
    '_path = Path.Combine(folder, "z6_f1c_motion.log");',
    "log path",
)

out = raw.encode("utf-8")
# write CRLF, no BOM (original F6 current was LF then maybe converted; pre_f1c was 22b966c8 which was LF-only 15172 bytes)
# The live file before this patch was 15172 with 493 crlf according to earlier inspect... 
# Wait: backup hash 22B966C8 equals F6 LF file. But inspect said crlf 493 and size 15172.
# 15172 LF would become larger with CRLF. Get-Item said 15172 bytes and hash 22b966c8, so the file on disk is LF.
# I'll write LF to match current file, or CRLF as Windows source typically. F6 report noted LF deviation. I'll write CRLF as original T5 style? 
# Backup is the LF 22b966c8 file. Writing CRLF would make a huge diff vs pre (every line). Write LF to keep F1C patch semantic-only.

cs.write_bytes(out)
print("wrote", cs, "bytes", len(out), "sha", hashlib.sha256(out).hexdigest())
print("crlf", out.count(b"\r\n"), "lf-only-newlines", out.count(b"\n")-out.count(b"\r\n"))
