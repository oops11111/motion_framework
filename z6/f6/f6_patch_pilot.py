# Z6 F6: defer enter Play until after the first host frame callback.
import sys
from pathlib import Path
p = Path(sys.argv[1])
raw = p.read_bytes()
bom = raw.startswith(b"\xef\xbb\xbf")
t = raw.decode("utf-8-sig")
NL = "\r\n" if "\r\n" in t else "\n"

def must(old, new, tag):
    global t
    if old not in t:
        raise SystemExit(f"pattern not found: {tag}")
    t = t.replace(old, new, 1)

must("using Microsoft.UI.Xaml;\n", "using Microsoft.UI.Dispatching;\nusing Microsoft.UI.Xaml;\n", "using")
must(
    "\t\tprivate readonly PointerEventHandler _leftHandler;\n\t\tprivate bool _renderingHooked;",
    "\t\tprivate readonly PointerEventHandler _leftHandler;\n\t\tprivate readonly DispatcherQueue _dispatcher;\n\t\tprivate bool _renderingHooked;",
    "fields-start",
)
must(
    "\t\tprivate bool _disposed;\n\t\tprivate int _sinkCount;",
    "\t\tprivate bool _disposed;\n\t\tprivate bool _firstFrameSeen;\n\t\tprivate bool _enterPlayed;\n\t\tprivate int _sinkCount;",
    "flags",
)
must(
    '\t\t\t_log = new PilotLog();\n\t\t\t_log.Line("PILOT start (Z5-T5, pilot-only)");',
    '\t\t\t_log = new PilotLog();\n\t\t\t_log.Line("PILOT start (Z6-F6, wait for first frame then Play enter; T5 hover unchanged)");',
    "start-line",
)
must(
    "\t\t\t_enteredHandler = OnItemPointerEntered;\n\t\t\t_leftHandler = OnItemPointerLeft;",
    "\t\t\t_dispatcher = navigationView.DispatcherQueue;\n\t\t\t_enteredHandler = OnItemPointerEntered;\n\t\t\t_leftHandler = OnItemPointerLeft;",
    "dispatcher",
)
must(
    """\t\t\tforeach (var (id, _) in _items)
\t\t\t{
\t\t\t\tAnimationHandle enter = _system.CreateAnimation(id, EnterTransition);
\t\t\t\t_enter[id] = enter;
\t\t\t\tenter.Play();
\t\t\t\t_log.Line($"enter target={id} anim={enter.Id}");
\t\t\t}

\t\t\t_log.Flush();
""",
    """\t\t\tforeach (var (id, _) in _items)
\t\t\t{
\t\t\t\t// Z6-F6: create enter handles now, but Play only after the first host frame callback.
\t\t\t\tAnimationHandle enter = _system.CreateAnimation(id, EnterTransition);
\t\t\t\t_enter[id] = enter;
\t\t\t\t_log.Line($"F6 enter created (Play deferred) target={id} anim={enter.Id}");
\t\t\t}

\t\t\t// Hook the host frame source immediately so the first CompositionTarget.Rendering can be observed before Play.
\t\t\tif (!_renderingHooked)
\t\t\t{
\t\t\t\tCompositionTarget.Rendering += OnRendering;
\t\t\t\t_renderingHooked = true;
\t\t\t}
\t\t\t_log.Line("F6 waiting for first CompositionTarget.Rendering before Play");
\t\t\t_log.Flush();
""",
    "play-loop",
)
must(
    """\t\tprivate void OnRendering(object? sender, object e)
\t\t{
\t\t\tif (_disposed)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\t_system.OnFrame();
\t\t}
""",
    """\t\tprivate void OnRendering(object? sender, object e)
\t\t{
\t\t\tif (_disposed)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\tif (!_firstFrameSeen)
\t\t\t{
\t\t\t\t_firstFrameSeen = true;
\t\t\t\t_log.Line("F6 FIRST_FRAME callback (CompositionTarget.Rendering) before Play");
\t\t\t\tbool queued = _dispatcher.TryEnqueue(PlayEnterAfterFirstFrame);
\t\t\t\t_log.Line($"F6 FIRST_FRAME enqueue Play ok={queued}");
\t\t\t\t_log.Flush();
\t\t\t}

\t\t\t_system.OnFrame();
\t\t}

\t\tprivate void PlayEnterAfterFirstFrame()
\t\t{
\t\t\tif (_disposed || _enterPlayed)
\t\t\t{
\t\t\t\treturn;
\t\t\t}

\t\t\t_enterPlayed = true;
\t\t\tforeach (var (id, enter) in _enter)
\t\t\t{
\t\t\t\tenter.Play();
\t\t\t\t_log.Line($"F6 PLAY after first frame target={id} anim={enter.Id}");
\t\t\t}
\t\t\t_log.Flush();
\t\t}
""",
    "onrendering",
)
must(
    '\t\t\t_path = Path.Combine(folder, "z5_t5_motion.log");',
    '\t\t\t_path = Path.Combine(folder, "z6_f6_motion.log");',
    "log-name",
)
# normalize newlines to the file's original
if NL == "\r\n":
    t = t.replace("\r\n", "\n").replace("\n", "\r\n")
out = t.encode("utf-8")
if bom:
    out = b"\xef\xbb\xbf" + out
p.write_bytes(out)
print("patched", p, "bytes", len(out))
