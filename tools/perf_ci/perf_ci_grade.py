#!/usr/bin/env python3
"""M2-PERF-CI offline grader (stdlib only; runs on the lab machines and on Linux).

Implements G2-LOCK-9..13 of docs/m2_perf_ci_gate_LOCKED.md
(GATE_BODY_SHA256 5b007dd43ba351dc0985a52d1745485954d149575feeca622791275005cd282a).
The grader only READS artefacts that were already captured; it never starts a capture,
and it refuses to emit PASS unless the gate hash matches and an APPROVED file is given.

Usage:
  perf_ci_grade.py <manifest.json> [--out-dir DIR] [--dry-run] [--frame-column NAME]

Exit codes: 0 PASS, 1 FAIL, 2 INVALID, 3 DRY-RUN / usage error.
Manifest format: tools/perf_ci/README.md.
"""
import argparse
import csv
import hashlib
import json
import math
import os
import re
import sys

GATE_SHA = "5b007dd43ba351dc0985a52d1745485954d149575feeca622791275005cd282a"
SCENARIOS = ("idle", "steady_play", "interrupted_retarget")
GPU_TIERS = ("igpu", "dgpu", "lowend-laptop")
CAPTURE_METHODS = ("logman-etl", "eventpipe")
MIN_FRAMES = 1000
MIN_ROUNDS = 2
STAGE_BUDGET = {"ui": 0.12, "render": 0.35, "gpu": 0.35}  # fraction of T (G1 P95)
MAX_REGRESSION = 0.10      # P95 relative to same-machine same-scenario baseline
DROP_FACTOR = 1.5          # dropped frame := duration > 1.5*T
MAX_DROPS_PER_1000 = 1.0   # strict <
RECONCILE_FACTOR = 2.0     # PresentStats vs PresentMon: > 2x apart = conflict
FRAME_COLUMNS = ("MsBetweenPresents", "FrameTime")  # PresentMon 1.x, 2.x


def gate_body_sha256(path):
    """SHA-256 of the file with every line starting with GATE_BODY_SHA256 removed."""
    with open(path, "rb") as f:
        lines = f.read().split(b"\n")
    body = b"\n".join(l for l in lines if not l.startswith(b"GATE_BODY_SHA256"))
    return hashlib.sha256(body).hexdigest()


def p95(values):
    """Nearest-rank P95 (the smallest sample >= 95% of samples); no interpolation."""
    s = sorted(values)
    return s[max(0, math.ceil(0.95 * len(s)) - 1)]


def read_presentmon(path, pid, column=None):
    """Valid frame durations (ms) for `pid` from a PresentMon CSV.

    Rows are kept when ProcessID matches (if the column exists) and the duration
    parses as a finite number > 0; PresentMon writes NA / 0 for the first present."""
    with open(path, newline="", encoding="utf-8-sig") as f:
        reader = csv.DictReader(f)
        fields = reader.fieldnames or []
        col = column or next((c for c in FRAME_COLUMNS if c in fields), None)
        if col is None or col not in fields:
            raise ValueError(f"no frame-duration column ({column or '/'.join(FRAME_COLUMNS)}) in {path}")
        out = []
        for row in reader:
            if pid is not None and "ProcessID" in fields and row.get("ProcessID", "").strip() != str(pid):
                continue
            try:
                v = float(row[col])
            except (TypeError, ValueError):
                continue
            if math.isfinite(v) and v > 0:
                out.append(v)
    return col, out


_PS_EVENT = re.compile(r"ETW_EVENT name=PresentStats id=4 .*?payload=([^\s]+)")


def read_present_stats(path):
    """(dropped, intervalMs) from an in-proc ledger (last PresentStats event) or a
    key=value session summary carrying dropped= and intervalMs=."""
    with open(path, encoding="utf-8-sig") as f:
        text = f.read()
    events = _PS_EVENT.findall(text)
    if events:
        parts = events[-1].split(",")
        return int(parts[1]), float(parts[2])
    kv = dict(l.split("=", 1) for l in text.splitlines() if "=" in l)
    return int(kv["dropped"]), float(kv["intervalMs"])


class Cell:
    """One machine x scenario x round."""

    def __init__(self, machine, run):
        self.host = machine.get("hostname", "?")
        self.scenario = run.get("scenario", "?")
        self.round = run.get("round", "?")
        self.invalid = []   # reasons -> INVALID
        self.fail = []      # reasons -> FAIL
        self.notes = []
        self.metrics = {}

    @property
    def verdict(self):
        return "INVALID" if self.invalid else "FAIL" if self.fail else "PASS"


def _exists(base, p):
    return bool(p) and os.path.isfile(os.path.join(base, p))


def grade_run(base, machine, run, column):
    c = Cell(machine, run)
    hz = run.get("refreshHz")
    if not isinstance(hz, (int, float)) or hz <= 0:
        c.invalid.append("refreshHz missing")
        return c
    T = 1000.0 / hz
    c.metrics["T_ms"] = T

    # G2-LOCK-12 minimum artefacts per round.
    for key in ("presentmonCsv", "trace", "presentStats"):
        if not _exists(base, run.get(key)):
            c.invalid.append(f"artefact missing: {key}")
    method = machine.get("captureMethod")
    trace = (run.get("trace") or "").lower()
    if method == "logman-etl" and trace and not trace.endswith(".etl"):
        c.invalid.append("captureMethod=logman-etl but trace is not .etl")
    if method == "eventpipe" and trace and not trace.endswith(".nettrace"):
        c.invalid.append("captureMethod=eventpipe but trace is not .nettrace")
    if c.invalid:
        return c

    try:
        col, frames = read_presentmon(os.path.join(base, run["presentmonCsv"]), run.get("pid"), column)
    except (OSError, ValueError, KeyError) as e:
        c.invalid.append(f"PresentMon unreadable: {e}")
        return c
    n = len(frames)
    drops = sum(1 for v in frames if v > DROP_FACTOR * T)
    c.metrics.update(column=col, frames=n, drops=drops)
    if n < MIN_FRAMES:
        c.invalid.append(f"frames {n} < {MIN_FRAMES}")
        return c
    per_k = drops * 1000.0 / n
    frame_p95 = p95(frames)
    c.metrics.update(drops_per_1000=per_k, frame_p95_ms=frame_p95)
    if not per_k < MAX_DROPS_PER_1000:
        c.fail.append(f"drops/1000 {per_k:.3f} >= {MAX_DROPS_PER_1000}")

    # P95 absolute + relative. Stage split (WPA) preferred; frame-interval fallback
    # only on the EventPipe path when the split is unavailable (G1 对账主路径).
    baseline = (machine.get("baselines") or {}).get(c.scenario)
    stages = run.get("stageP95Ms")
    if stages:
        missing = [s for s in STAGE_BUDGET if not isinstance(stages.get(s), (int, float))]
        if missing:
            c.invalid.append(f"stageP95Ms missing {missing}")
            return c
        base_st = (baseline or {}).get("stageP95Ms") or {}
        for s, frac in STAGE_BUDGET.items():
            v = stages[s]
            c.metrics[f"{s}_p95_ms"] = v
            if v > frac * T:
                c.fail.append(f"{s} P95 {v:.3f}ms > {frac:.0%} of T ({frac * T:.3f}ms)")
            b = base_st.get(s)
            if not isinstance(b, (int, float)) or b <= 0:
                c.invalid.append(f"baseline stageP95Ms.{s} missing")
            elif v > b * (1 + MAX_REGRESSION):
                c.fail.append(f"{s} P95 regression {v / b - 1:+.1%} > {MAX_REGRESSION:.0%}")
    else:
        if method != "eventpipe":
            c.invalid.append("stageP95Ms missing (fallback allowed only on eventpipe path)")
            return c
        c.notes.append("SUBSTITUTE: stage split unavailable -> PresentMon frame P95 <= T")
        if frame_p95 > T:
            c.fail.append(f"frame P95 {frame_p95:.3f}ms > T {T:.3f}ms")
        b = (baseline or {}).get("frameP95Ms")
        if not isinstance(b, (int, float)) or b <= 0:
            c.invalid.append("baseline frameP95Ms missing")
        elif frame_p95 > b * (1 + MAX_REGRESSION):
            c.fail.append(f"frame P95 regression {frame_p95 / b - 1:+.1%} > {MAX_REGRESSION:.0%}")

    # PresentStats reconciliation (G2-LOCK-11): own count <= system count, and the
    # two within 2x of each other; a conflict needs a written explanation.
    try:
        ps_drops, ps_interval = read_present_stats(os.path.join(base, run["presentStats"]))
    except (OSError, ValueError, KeyError, IndexError) as e:
        c.invalid.append(f"PresentStats unreadable: {e}")
        return c
    mean = sum(frames) / n
    c.metrics.update(ps_dropped=ps_drops, ps_interval_ms=ps_interval, frame_mean_ms=mean)
    conflicts = []
    if ps_drops > drops:
        conflicts.append(f"PresentStats dropped {ps_drops} > PresentMon {drops}")
    elif drops > RECONCILE_FACTOR * ps_drops:
        conflicts.append(f"PresentMon dropped {drops} > {RECONCILE_FACTOR:g}x PresentStats {ps_drops}")
    if ps_interval <= 0 or max(ps_interval, mean) > RECONCILE_FACTOR * min(ps_interval, mean):
        conflicts.append(f"intervalMs {ps_interval:.3f} vs PresentMon mean {mean:.3f} > {RECONCILE_FACTOR:g}x")
    note = (run.get("reconcileNote") or "").strip()
    for msg in conflicts:
        if note:
            c.notes.append(f"RECONCILE-CONFLICT (explained: {note}): {msg}")
        else:
            c.invalid.append(f"RECONCILE-CONFLICT unexplained: {msg}")
    return c


def grade_matrix(base, manifest):
    """G2-LOCK-9 / -10 / -12 checks that span runs; returns INVALID reasons."""
    bad = []
    machines = manifest.get("machines") or []
    if len(machines) < 2:
        bad.append(f"machines {len(machines)} < 2")
    ids = [m.get("machineId") for m in machines]
    if any(not i for i in ids):
        bad.append("machineId missing")
    if len(set(ids)) != len(ids):
        bad.append("machineId not distinct")
    tiers = {m.get("gpuTier") for m in machines}
    if not tiers <= set(GPU_TIERS):
        bad.append(f"gpuTier outside {GPU_TIERS}")
    if len(tiers & set(GPU_TIERS)) < 2:
        bad.append("fewer than 2 GPU tiers")
    for m in machines:
        h = m.get("hostname") or "?"
        if not m.get("hostname"):
            bad.append("hostname missing")
        if m.get("physical") is not True:
            bad.append(f"{h}: not declared physical")
        method = m.get("captureMethod")
        if method not in CAPTURE_METHODS:
            bad.append(f"{h}: captureMethod {method!r}")
        if method == "eventpipe" and not _exists(base, m.get("logmanDeniedEvidence")):
            bad.append(f"{h}: eventpipe without logman Access-denied evidence")
        runs = m.get("runs") or []
        extra = {r.get("scenario") for r in runs} - set(SCENARIOS)
        if extra:
            bad.append(f"{h}: scenarios outside LOCK {sorted(map(str, extra))}")
        for s in SCENARIOS:
            rs = [r for r in runs if r.get("scenario") == s]
            pids = [r.get("pid") for r in rs]
            if len(rs) < MIN_ROUNDS:
                bad.append(f"{h}/{s}: rounds {len(rs)} < {MIN_ROUNDS}")
            elif any(p is None for p in pids) or len(set(pids)) != len(pids):
                bad.append(f"{h}/{s}: rounds not independent processes (pid missing/repeated)")
    return bad


def read_approval(base, path):
    """'by@at' from an approved.md carrying APPROVED_BY=... and APPROVED_AT=..., else None."""
    if not _exists(base, path):
        return None
    with open(os.path.join(base, path), encoding="utf-8") as f:
        text = f.read()
    by = re.search(r"APPROVED_BY=(\S+)", text)
    at = re.search(r"APPROVED_AT=(\S[^\n]*)", text)
    return f"{by.group(1)}@{at.group(1).strip()}" if by and at else None


def grade(manifest_path, dry_run=False, column=None):
    base = os.path.dirname(os.path.abspath(manifest_path))
    with open(manifest_path, encoding="utf-8") as f:
        manifest = json.load(f)
    gate_path = manifest.get("gate") or ""
    gate_sha = gate_body_sha256(os.path.join(base, gate_path)) if _exists(base, gate_path) else None
    top = []
    if gate_sha != GATE_SHA:
        top.append(f"gate hash {gate_sha or 'missing'} != {GATE_SHA}")
    approved = read_approval(base, manifest.get("approvedFile"))
    if not dry_run and approved is None:
        top.append("no APPROVED file with APPROVED_BY/AT (未授权采数)")
    top += grade_matrix(base, manifest)
    cells = [grade_run(base, m, r, column)
             for m in manifest.get("machines") or [] for r in m.get("runs") or []]
    if top or any(c.verdict == "INVALID" for c in cells):
        ticket = "INVALID"
    elif any(c.verdict == "FAIL" for c in cells):
        ticket = "FAIL"
    else:
        ticket = "PASS"
    if dry_run:
        ticket = f"DRY-RUN({ticket})"
    return {"ticket": ticket, "gate_sha": gate_sha, "invalid": top,
            "machines": len(manifest.get("machines") or []),
            "approved": approved or "NONE",
            "cells": [{"host": c.host, "scenario": c.scenario, "round": c.round,
                       "verdict": c.verdict, "invalid": c.invalid, "fail": c.fail,
                       "notes": c.notes, "metrics": c.metrics} for c in cells]}


def oneline(r):
    return (f"M2-PERF-CI | gate={r['gate_sha']} | approved={r['approved']} | machines={r['machines']}"
            f" | ticket={r['ticket']} | 成功≠采用；不改composition.h；禁影子IV；≥2物理机；主判据PresentMon")


def report_md(r):
    out = ["# M2-PERF-CI grader report", "", f"TICKET: {r['ticket']}", f"GATE_BODY_SHA256: {r['gate_sha']}", ""]
    if r["invalid"]:
        out += ["## Matrix / authorisation INVALID", ""] + [f"- {x}" for x in r["invalid"]] + [""]
    out += ["## Cells", "",
            "| host | scenario | round | frames | drops/1000 | frame P95 ms | T ms | verdict | reasons |",
            "|---|---|---|---|---|---|---|---|---|"]
    for c in r["cells"]:
        m = c["metrics"]
        f = lambda k: f"{m[k]:.3f}" if isinstance(m.get(k), float) else str(m.get(k, "-"))
        reasons = "; ".join(c["invalid"] + c["fail"] + c["notes"]) or "-"
        out.append(f"| {c['host']} | {c['scenario']} | {c['round']} | {m.get('frames', '-')} | "
                   f"{f('drops_per_1000')} | {f('frame_p95_ms')} | {f('T_ms')} | {c['verdict']} | {reasons} |")
    out += ["", "## G4 ONELINE", "", "```", oneline(r), "```", ""]
    return "\n".join(out)


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    ap.add_argument("manifest")
    ap.add_argument("--out-dir")
    ap.add_argument("--dry-run", action="store_true", help="skip the APPROVED check; never yields PASS")
    ap.add_argument("--frame-column", help="PresentMon duration column (default: auto)")
    a = ap.parse_args(argv)
    r = grade(a.manifest, a.dry_run, a.frame_column)
    md = report_md(r)
    if a.out_dir:
        os.makedirs(a.out_dir, exist_ok=True)
        with open(os.path.join(a.out_dir, "perf_ci_report.md"), "w", encoding="utf-8") as f:
            f.write(md)
        with open(os.path.join(a.out_dir, "perf_ci_report.json"), "w", encoding="utf-8") as f:
            json.dump(r, f, ensure_ascii=False, indent=2)
    sys.stdout.write(md)
    return {"PASS": 0, "FAIL": 1, "INVALID": 2}.get(r["ticket"], 3)


if __name__ == "__main__":
    sys.exit(main())
