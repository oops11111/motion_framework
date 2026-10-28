// PERF-CI r2 Q4: real PresentStats instead of the EmitPresentStats(0, 16.666) placeholder.
// Source "dcomp" (Windows 11 build 22000+): a background thread wakes once per DWM
// composition (DwmFlush), walks every newly COMPLETED DirectComposition frame id and
// reads its target time with DCompositionGetStatistics (via mfprobe). An interval
// between consecutive frames longer than 1.5 x the frame period counts as one drop,
// the same rule PresentMon-side grading uses. These are system composition frames,
// not presents attributed to this process: they track the app's presents while it
// animates every frame (Steady Play / Interrupted Retarget), not while it is idle.
// On Windows 10 the DComp statistics API is not exported and DWM's cRefresh counter
// is unreliable (M0 workbuddy_soft_demo #9), so Source = "unavailable" and nothing
// is estimated; the r2 grader then skips reconciliation with a note.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MotionSoftDemo;

internal sealed class PresentStatsSampler : IDisposable
{
    public readonly record struct Result(string Source, ulong Frames, uint Dropped, double IntervalMs, double PeriodMs, int LastHr);

    const int CompletedFrameId = 2;  // COMPOSITION_FRAME_ID_COMPLETED
    [DllImport("dwmapi.dll")] static extern int DwmFlush();

    readonly object _mu = new();
    Thread? _thread;
    volatile bool _run;
    ulong _lastId, _frames;
    uint _dropped;
    ulong _prevTarget;
    double _sumIntervalTicks, _sumPeriodTicks;
    ulong _intervals;
    int _lastHr;

    /// True when DCompositionGetFrameId is exported and answers (Windows 11 22000+).
    public static bool Available => MfProbe.mf_dcomp_frame_id(CompletedFrameId, out _) == 0;

    public void Start()
    {
        if (_thread is not null) throw new InvalidOperationException("already started");
        if (!Available) return;
        MfProbe.mf_dcomp_frame_id(CompletedFrameId, out _lastId);  // count only frames after Start
        _run = true;
        _thread = new Thread(Loop) { IsBackground = true, Name = "PresentStatsSampler" };
        _thread.Start();
    }

    void Loop()
    {
        while (_run)
        {
            DwmFlush();
            int hr = MfProbe.mf_dcomp_frame_id(CompletedFrameId, out var id);
            lock (_mu)
            {
                _lastHr = hr;
                if (hr != 0) continue;
                for (ulong f = _lastId + 1; f <= id; f++)
                {
                    if (MfProbe.mf_dcomp_stats(f, out _, out var target, out var period, out _) != 0 || period == 0) continue;
                    ++_frames;
                    if (_prevTarget != 0 && target > _prevTarget)
                    {
                        ulong dt = target - _prevTarget;
                        _sumIntervalTicks += dt;
                        _sumPeriodTicks += period;
                        ++_intervals;
                        if (dt > 1.5 * period) ++_dropped;
                    }
                    _prevTarget = target;
                }
                if (id > _lastId) _lastId = id;
            }
        }
    }

    /// Stops sampling and returns the totals since Start.
    public Result Stop()
    {
        _run = false;
        _thread?.Join(1000);
        lock (_mu)
        {
            if (_thread is null) return new Result("unavailable", 0, 0, 0, 0, _lastHr);
            double tickMs = 1000.0 / Stopwatch.Frequency;  // DComp frame statistics are in QPC ticks
            double interval = _intervals == 0 ? 0 : _sumIntervalTicks / _intervals * tickMs;
            double period = _intervals == 0 ? 0 : _sumPeriodTicks / _intervals * tickMs;
            return new Result("dcomp", _frames, _dropped, interval, period, _lastHr);
        }
    }

    /// key=value summary in the format tools/perf_ci/perf_ci_grade.py reads.
    public static void WriteSummary(string path, Result r, ulong sessionId)
    {
        var inv = CultureInfo.InvariantCulture;
        File.WriteAllLines(path, new[]
        {
            $"session=0x{sessionId:X}",
            $"source={r.Source}",
            $"frames={r.Frames}",
            $"dropped={r.Dropped}",
            $"intervalMs={r.IntervalMs.ToString("F4", inv)}",
            $"periodMs={r.PeriodMs.ToString("F4", inv)}",
            $"lastHr=0x{r.LastHr:X8}",
        });
    }

    public void Dispose()
    {
        if (_run) Stop();
    }
}
