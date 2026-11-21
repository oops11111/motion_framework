# -*- coding: utf-8 -*-
"""Z6 F2: one-shot RootView forceReload after first enter Play; shared log across rebuild cycles."""
from pathlib import Path
import os, hashlib

cs = Path(os.path.expandvars(r"%USERPROFILE%\pilot\FluentHub\src\FluentHub\Views\NavMotionPilot.cs"))
rv = Path(os.path.expandvars(r"%USERPROFILE%\pilot\FluentHub\src\FluentHub\Views\RootView.xaml.cs"))
raw = cs.read_bytes().replace(b"\r\n", b"\n").replace(b"\r", b"\n").decode("utf-8-sig")
rvraw = rv.read_bytes().replace(b"\r\n", b"\n").replace(b"\r", b"\n").decode("utf-8-sig")

def must_replace(buf, old, new, tag):
    if old not in buf:
        raise SystemExit(f"pattern not found: {tag}\n---\n{old[:240]}")
    n = buf.count(old)
    if n != 1:
        raise SystemExit(f"pattern count {n} != 1: {tag}")
    return buf.replace(old, new)

# --- NavMotionPilot ---
raw = must_replace(raw,
    '\t/// F1C: after the sink sees enter mid-flight, DispatcherQueue.TryEnqueue hovers (Retarget offsetX=8). No fm_* in sink/render. Pointer hover ignored.',
    '\t/// F1C hover enqueue unchanged. F2: after first enter Play, DispatcherQueue.TryEnqueue RootView forceReload (Unloaded Dispose, then Loaded Create).',
    "summary")

raw = must_replace(raw,
    '\t\tprivate const ulong F1cTarget = 1;\n',
    '\t\tprivate const ulong F1cTarget = 1;\n'
    '\t\tprivate static int s_f2Cycle;\n'
    '\t\tprivate static bool s_f2RebuildQueued;\n'
    '\t\tprivate readonly int _f2Cycle;\n',
    "f2 fields")

raw = must_replace(raw,
    '\t\t\t_log = new PilotLog();\n'
    '\t\t\t_log.Line("PILOT start (Z6-F1C, program-state hover after sink sees enter mid-flight; enqueue via DispatcherQueue; F6 enter wait unchanged)");',
    '\t\t\t_f2Cycle = System.Threading.Interlocked.Increment(ref s_f2Cycle);\n'
    '\t\t\t_log = new PilotLog();\n'
    '\t\t\t_log.Line($"PILOT start (Z6-F2, RootView rebuild cycle={_f2Cycle}; Unloaded Dispose then Loaded Create; F6 enter wait unchanged)");',
    "start line")

raw = must_replace(raw,
    '\t\t\t_enterPlayed = true;\n'
    '\t\t\tforeach (var (id, enter) in _enter)\n'
    '\t\t\t{\n'
    '\t\t\t\tenter.Play();\n'
    '\t\t\t\t_log.Line($"F6 PLAY after first frame target={id} anim={enter.Id}");\n'
    '\t\t\t}\n'
    '\t\t\t_log.Flush();\n'
    '\t\t}',
    '\t\t\t_enterPlayed = true;\n'
    '\t\t\tforeach (var (id, enter) in _enter)\n'
    '\t\t\t{\n'
    '\t\t\t\tenter.Play();\n'
    '\t\t\t\t_log.Line($"F6 PLAY after first frame target={id} anim={enter.Id}");\n'
    '\t\t\t}\n'
    '\t\t\tif (!s_f2RebuildQueued && _f2Cycle == 1)\n'
    '\t\t\t{\n'
    '\t\t\t\ts_f2RebuildQueued = true;\n'
    '\t\t\t\tbool queued = _dispatcher.TryEnqueue(RequestF2Rebuild);\n'
    '\t\t\t\t_log.Line($"F2 enqueue RootView rebuild ok={queued} (DispatcherQueue, not sink/render)");\n'
    '\t\t\t}\n'
    '\t\t\t_log.Flush();\n'
    '\t\t}\n'
    '\n'
    '\t\tprivate void RequestF2Rebuild()\n'
    '\t\t{\n'
    '\t\t\t// UI thread, not sink/render. forceReload replaces Content: old RootView Unloaded (Dispose) then new Loaded (Create).\n'
    '\t\t\t_log.Line("F2 REBUILD begin: MainWindow.InitializeApplication(forceReload=true)");\n'
    '\t\t\t_log.Flush();\n'
    '\t\t\tMainWindow.Instance.InitializeApplication(null, forceReload: true);\n'
    '\t\t}',
    "play + request rebuild")

raw = must_replace(raw,
    '\t\t\t_log.Line("Dispose begin");\n',
    '\t\t\t_log.Line($"F2 Dispose begin cycle={_f2Cycle}");\n'
    '\t\t\t_log.Line("Dispose begin");\n',
    "dispose begin cycle")

raw = must_replace(raw,
    '\t\t\t_log.Line($"Dispose end handles={_enter.Count + _hover.Count} (enter={_enter.Count}, hover={_hover.Count}) sinkCalls={_sinkCount}");\n',
    '\t\t\t_log.Line($"Dispose end handles={_enter.Count + _hover.Count} (enter={_enter.Count}, hover={_hover.Count}) sinkCalls={_sinkCount} cycle={_f2Cycle}");\n'
    '\t\t\t_log.Line($"F2 Dispose end cycle={_f2Cycle}");\n',
    "dispose end cycle")

# PilotLog -> shared buffer so cycle 2 does not overwrite cycle 1
old_log = '''	/// <summary>Bounded in-memory line buffer, written to the package LocalState folder on Flush.</summary>
	internal sealed class PilotLog
	{
		private const int Capacity = 4096;
		private readonly object _gate = new();
		private readonly List<string> _lines = [];
		private readonly string _path;
		private int _sequence;
		private int _dropped;

		public PilotLog()
		{
			string folder;
			try
			{
				folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			}
			catch (Exception)
			{
				folder = Path.GetTempPath();
			}

			_path = Path.Combine(folder, "z6_f1c_motion.log");
		}

		public void Line(string text)
		{
			lock (_gate)
			{
				_sequence++;
				if (_lines.Count >= Capacity)
				{
					_dropped++;
					return;
				}

				_lines.Add($"{_sequence:D5} {text}");
			}
		}

		public void Flush()
		{
			lock (_gate)
			{
				try
				{
					var all = new List<string>(_lines) { $"# path={_path} lines={_lines.Count} dropped={_dropped} capacity={Capacity}" };
					File.WriteAllLines(_path, all);
				}
				catch (Exception)
				{
					// Pilot logging must never take the app down.
				}
			}
		}
	}'''

new_log = '''	/// <summary>Shared in-memory line buffer (survives RootView rebuild), written to LocalState on Flush.</summary>
	internal sealed class PilotLog
	{
		private const int Capacity = 4096;
		private static readonly object SharedGate = new();
		private static readonly List<string> SharedLines = [];
		private static int SharedSeq;
		private static int SharedDropped;
		private static string? SharedPath;

		public PilotLog()
		{
			EnsurePath();
		}

		private static void EnsurePath()
		{
			if (SharedPath is not null)
			{
				return;
			}

			string folder;
			try
			{
				folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
			}
			catch (Exception)
			{
				folder = Path.GetTempPath();
			}

			SharedPath = Path.Combine(folder, "z6_f2_motion.log");
		}

		public void Line(string text) => Write(text);

		public static void Write(string text)
		{
			EnsurePath();
			lock (SharedGate)
			{
				SharedSeq++;
				if (SharedLines.Count >= Capacity)
				{
					SharedDropped++;
					return;
				}

				SharedLines.Add($"{SharedSeq:D5} {text}");
			}
		}

		public void Flush()
		{
			EnsurePath();
			lock (SharedGate)
			{
				try
				{
					var all = new List<string>(SharedLines) { $"# path={SharedPath} lines={SharedLines.Count} dropped={SharedDropped} capacity={Capacity}" };
					File.WriteAllLines(SharedPath!, all);
				}
				catch (Exception)
				{
					// Pilot logging must never take the app down.
				}
			}
		}
	}'''

raw = must_replace(raw, old_log, new_log, "PilotLog shared")

# --- RootView markers ---
rvraw = must_replace(rvraw,
    '''		private async void OnRootViewLoaded(object sender, RoutedEventArgs e)
		{
			MainWindow.Instance.SetTitleBar(MainTitleBar);
			// Z5-T5 pilot-only: Loaded re-entry guard; Unloaded disposes first, so a rebuilt RootView creates a fresh system.
			_motionPilot ??= new NavMotionPilot(NavigationView);
''',
    '''		private async void OnRootViewLoaded(object sender, RoutedEventArgs e)
		{
			MainWindow.Instance.SetTitleBar(MainTitleBar);
			// Z5-T5 / Z6-F2: Unloaded disposes first, so a rebuilt RootView creates a fresh system.
			PilotLog.Write("F2 RootView Loaded -> Create NavMotionPilot");
			_motionPilot ??= new NavMotionPilot(NavigationView);
''',
    "RootView Loaded")

rvraw = must_replace(rvraw,
    '''		private void OnRootViewUnloaded(object sender, RoutedEventArgs e)
		{
			ViewModel.CancelLoading();
			_motionPilot?.Dispose(); // Z5-T5 pilot-only
			_motionPilot = null;
		}
''',
    '''		private void OnRootViewUnloaded(object sender, RoutedEventArgs e)
		{
			ViewModel.CancelLoading();
			PilotLog.Write("F2 RootView Unloaded -> Dispose NavMotionPilot then null");
			_motionPilot?.Dispose(); // Z5-T5 / Z6-F2: Dispose before the next Loaded Create
			_motionPilot = null;
		}
''',
    "RootView Unloaded")

cs.write_bytes(raw.encode("utf-8"))
rv.write_bytes(rvraw.encode("utf-8"))
print("CS", len(raw.encode()), hashlib.sha256(raw.encode()).hexdigest())
print("RV", len(rvraw.encode()), hashlib.sha256(rvraw.encode()).hexdigest())
