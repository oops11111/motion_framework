#!/usr/bin/env python3
"""Synthetic-data tests for perf_ci_grade.py. Run: python3 -m unittest test_perf_ci_grade -v"""
import copy
import json
import os
import random
import tempfile
import unittest

import perf_ci_grade as g

GATE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "docs", "m2_perf_ci_gate_LOCKED.md")


def write_presentmon(path, durations, pid=100, column="MsBetweenPresents", other_pid_rows=0):
    with open(path, "w", newline="") as f:
        f.write(f"Application,ProcessID,{column}\n")
        f.write(f"MotionSoftDemo.exe,{pid},NA\n")  # first present has no interval
        for d in durations:
            f.write(f"MotionSoftDemo.exe,{pid},{d:.4f}\n")
        for _ in range(other_pid_rows):
            f.write("dwm.exe,4,500.0\n")


def frames(n, T, drops=0, seed=1):
    rnd = random.Random(seed)
    out = [T * rnd.uniform(0.95, 1.05) for _ in range(n - drops)]
    return out + [T * 2.0] * drops


class Fixture:
    """A two-machine, three-scenario, two-round capture that PASSes."""

    def __init__(self, tmp):
        self.tmp = tmp
        with open(os.path.join(tmp, "approved.md"), "w") as f:
            f.write("- APPROVED_BY=pidan\n- APPROVED_AT=2026-10-27 10:00:00 Australia/Perth\n")
        with open(os.path.join(tmp, "denied.txt"), "w") as f:
            f.write("logman: Access is denied.\n")
        self.manifest = {"gate": os.path.abspath(GATE), "approvedFile": "approved.md", "machines": []}
        for host, tier, hz, method in (("LAB-IGPU", "igpu", 60, "logman-etl"),
                                       ("LAB-DGPU", "dgpu", 120, "eventpipe")):
            T = 1000.0 / hz
            m = {"hostname": host, "machineId": f"id-{host}", "gpuTier": tier, "physical": True,
                 "captureMethod": method, "runs": [],
                 "baselines": {s: {"stageP95Ms": {"ui": 0.10 * T, "render": 0.30 * T, "gpu": 0.30 * T},
                                   "frameP95Ms": 1.05 * T} for s in g.SCENARIOS}}
            if method == "eventpipe":
                m["logmanDeniedEvidence"] = "denied.txt"
            ext = ".etl" if method == "logman-etl" else ".nettrace"
            pid = 1000
            for s in g.SCENARIOS:
                for rnd in (1, 2):
                    pid += 1
                    stem = f"{host}_{s}_{rnd}"
                    write_presentmon(os.path.join(tmp, stem + ".csv"), frames(1200, T, seed=pid), pid)
                    open(os.path.join(tmp, stem + ext), "w").close()
                    with open(os.path.join(tmp, stem + "_ps.txt"), "w") as f:
                        f.write(f"dropped=0\nintervalMs={T:.3f}\n")
                    m["runs"].append({"scenario": s, "round": rnd, "pid": pid, "refreshHz": hz,
                                      "presentmonCsv": stem + ".csv", "trace": stem + ext,
                                      "presentStats": stem + "_ps.txt",
                                      "stageP95Ms": {"ui": 0.10 * T, "render": 0.30 * T, "gpu": 0.30 * T}})
            self.manifest["machines"].append(m)

    def run(self, manifest=None, **kw):
        path = os.path.join(self.tmp, "manifest.json")
        with open(path, "w") as f:
            json.dump(manifest or self.manifest, f)
        return g.grade(path, **kw)

    def m(self):
        return copy.deepcopy(self.manifest)


class GradeTests(unittest.TestCase):
    def setUp(self):
        self._td = tempfile.TemporaryDirectory()
        self.fx = Fixture(self._td.name)

    def tearDown(self):
        self._td.cleanup()

    def assertTicket(self, r, ticket, needle=None):
        self.assertEqual(r["ticket"], ticket, json.dumps(r, ensure_ascii=False, indent=1)[:3000])
        if needle:
            blob = json.dumps(r, ensure_ascii=False)
            self.assertIn(needle, blob)

    # --- helpers ------------------------------------------------------------
    def test_gate_hash_matches_locked_file(self):
        self.assertEqual(g.gate_body_sha256(GATE), g.GATE_SHA)

    def test_p95_nearest_rank(self):
        self.assertEqual(g.p95(list(range(1, 101))), 95)
        self.assertEqual(g.p95([5.0]), 5.0)
        self.assertEqual(g.p95(list(range(1, 21))), 19)

    def test_presentmon_filters_pid_na_and_reads_v2_column(self):
        p = os.path.join(self._td.name, "pm.csv")
        write_presentmon(p, [8.0, 9.0, 0.0], pid=7, column="FrameTime", other_pid_rows=3)
        col, v = g.read_presentmon(p, 7)
        self.assertEqual((col, v), ("FrameTime", [8.0, 9.0]))

    def test_present_stats_from_ledger_uses_last_event(self):
        p = os.path.join(self._td.name, "ledger.txt")
        with open(p, "w") as f:
            f.write("ETW_EVENT name=PresentStats id=4 level=Informational payload=1,0,16.666\n"
                    "ETW_EVENT name=PresentStats id=4 level=Informational payload=1,3,8.3\n")
        self.assertEqual(g.read_present_stats(p), (3, 8.3))

    # --- ticket outcomes ----------------------------------------------------
    def test_pass(self):
        r = self.fx.run()
        self.assertTicket(r, "PASS")
        self.assertEqual(len(r["cells"]), 12)
        self.assertIn("approved=pidan@2026-10-27 10:00:00 Australia/Perth", g.oneline(r))

    def test_dry_run_never_passes_and_skips_approval(self):
        m = self.fx.m()
        m["approvedFile"] = ""
        self.assertTicket(self.fx.run(m, dry_run=True), "DRY-RUN(PASS)")

    def test_unapproved_is_invalid(self):
        m = self.fx.m()
        m["approvedFile"] = "missing.md"
        self.assertTicket(self.fx.run(m), "INVALID", "未授权采数")

    def test_wrong_gate_is_invalid(self):
        m = self.fx.m()
        m["gate"] = "approved.md"
        self.assertTicket(self.fx.run(m), "INVALID", "gate hash")

    def test_single_machine_is_invalid(self):
        m = self.fx.m()
        m["machines"] = m["machines"][:1]
        self.assertTicket(self.fx.run(m), "INVALID", "machines 1 < 2")

    def test_duplicate_machine_id_is_invalid(self):
        m = self.fx.m()
        m["machines"][1]["machineId"] = m["machines"][0]["machineId"]
        self.assertTicket(self.fx.run(m), "INVALID", "machineId not distinct")

    def test_same_gpu_tier_is_invalid(self):
        m = self.fx.m()
        m["machines"][1]["gpuTier"] = "igpu"
        self.assertTicket(self.fx.run(m), "INVALID", "fewer than 2 GPU tiers")

    def test_vm_is_invalid(self):
        m = self.fx.m()
        m["machines"][0]["physical"] = False
        self.assertTicket(self.fx.run(m), "INVALID", "not declared physical")

    def test_one_round_is_invalid(self):
        m = self.fx.m()
        m["machines"][0]["runs"] = [r for r in m["machines"][0]["runs"]
                                    if not (r["scenario"] == "idle" and r["round"] == 2)]
        self.assertTicket(self.fx.run(m), "INVALID", "rounds 1 < 2")

    def test_repeated_pid_is_invalid(self):
        m = self.fx.m()
        runs = m["machines"][0]["runs"]
        runs[1]["pid"] = runs[0]["pid"]
        self.assertTicket(self.fx.run(m), "INVALID", "not independent processes")

    def test_extra_scenario_is_invalid(self):
        m = self.fx.m()
        extra = dict(m["machines"][0]["runs"][0], scenario="scroll_bonus")
        m["machines"][0]["runs"].append(extra)
        self.assertTicket(self.fx.run(m), "INVALID", "scenarios outside LOCK")

    def test_eventpipe_without_denied_evidence_is_invalid(self):
        m = self.fx.m()
        del m["machines"][1]["logmanDeniedEvidence"]
        self.assertTicket(self.fx.run(m), "INVALID", "Access-denied evidence")

    def test_missing_trace_is_invalid(self):
        m = self.fx.m()
        m["machines"][0]["runs"][0]["trace"] = "nope.etl"
        self.assertTicket(self.fx.run(m), "INVALID", "artefact missing: trace")

    def test_too_few_frames_is_invalid(self):
        m = self.fx.m()
        run = m["machines"][0]["runs"][0]
        write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]), frames(999, 16.67), run["pid"])
        self.assertTicket(self.fx.run(m), "INVALID", "frames 999 < 1000")

    def test_drops_at_threshold_fail(self):
        # 1 drop in 1000 frames = 1.0 per 1000, gate requires strictly < 1.0.
        m = self.fx.m()
        run = m["machines"][0]["runs"][0]
        write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]),
                         frames(1000, 1000 / 60, drops=1), run["pid"])
        with open(os.path.join(self._td.name, run["presentStats"]), "w") as f:
            f.write("dropped=1\nintervalMs=16.7\n")
        self.assertTicket(self.fx.run(m), "FAIL", "drops/1000 1.000")

    def test_one_drop_in_1200_passes(self):
        m = self.fx.m()
        run = m["machines"][0]["runs"][0]
        write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]),
                         frames(1200, 1000 / 60, drops=1), run["pid"])
        with open(os.path.join(self._td.name, run["presentStats"]), "w") as f:
            f.write("dropped=1\nintervalMs=16.7\n")
        self.assertTicket(self.fx.run(m), "PASS")

    def test_stage_absolute_budget_fail(self):
        m = self.fx.m()
        run = m["machines"][0]["runs"][0]
        run["stageP95Ms"]["ui"] = 0.13 * 1000 / 60
        m["machines"][0]["baselines"]["idle"]["stageP95Ms"]["ui"] = 0.13 * 1000 / 60
        self.assertTicket(self.fx.run(m), "FAIL", "ui P95")

    def test_stage_regression_fail(self):
        m = self.fx.m()
        run = m["machines"][0]["runs"][0]
        run["stageP95Ms"]["gpu"] = 0.30 * 1000 / 60 * 1.11
        self.assertTicket(self.fx.run(m), "FAIL", "gpu P95 regression")

    def test_missing_baseline_is_invalid(self):
        m = self.fx.m()
        del m["machines"][0]["baselines"]["steady_play"]
        self.assertTicket(self.fx.run(m), "INVALID", "baseline stageP95Ms.ui missing")

    def test_fallback_only_on_eventpipe(self):
        m = self.fx.m()
        m["machines"][0]["runs"][0]["stageP95Ms"] = None
        self.assertTicket(self.fx.run(m), "INVALID", "fallback allowed only on eventpipe")

    def test_eventpipe_fallback_marks_substitute(self):
        m = self.fx.m()
        # Frame-interval P95 <= T needs intervals at or under T; the +-5% jitter of the
        # fixture would put P95 above T (see open question Q2 in the tooling DRAFT).
        T = 1000 / 120
        for i, run in enumerate(m["machines"][1]["runs"]):
            run["stageP95Ms"] = None
            write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]),
                             [T * (0.9 + 0.1 * ((j * 7 + i) % 11) / 10) for j in range(1200)], run["pid"])
        r = self.fx.run(m)
        self.assertTicket(r, "PASS", "SUBSTITUTE")

    def test_eventpipe_fallback_frame_p95_over_T_fails(self):
        m = self.fx.m()
        run = m["machines"][1]["runs"][0]
        run["stageP95Ms"] = None
        T = 1000 / 120
        write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]),
                         [T * 1.2] * 1200, run["pid"])
        m["machines"][1]["baselines"]["idle"]["frameP95Ms"] = T * 1.2
        self.assertTicket(self.fx.run(m), "FAIL", "frame P95")

    def test_reconcile_conflict_unexplained_is_invalid_explained_is_noted(self):
        m = self.fx.m()
        run = m["machines"][0]["runs"][0]
        with open(os.path.join(self._td.name, run["presentStats"]), "w") as f:
            f.write("dropped=5\nintervalMs=16.7\n")   # own count > system count (0)
        self.assertTicket(self.fx.run(m), "INVALID", "RECONCILE-CONFLICT unexplained")
        run["reconcileNote"] = "ledger counts skipped ticks, not presents"
        self.assertTicket(self.fx.run(m), "PASS", "RECONCILE-CONFLICT (explained")

    def test_interval_mismatch_is_conflict(self):
        m = self.fx.m()
        run = m["machines"][1]["runs"][0]
        with open(os.path.join(self._td.name, run["presentStats"]), "w") as f:
            f.write("dropped=0\nintervalMs=33.3\n")   # 120 Hz machine, 4x the PresentMon mean
        self.assertTicket(self.fx.run(m), "INVALID", "intervalMs")

    def test_cli_exit_codes_and_report_files(self):
        path = os.path.join(self._td.name, "manifest.json")
        with open(path, "w") as f:
            json.dump(self.fx.manifest, f)
        out = os.path.join(self._td.name, "out")
        devnull = open(os.devnull, "w")
        import contextlib
        with contextlib.redirect_stdout(devnull):
            self.assertEqual(g.main([path, "--out-dir", out]), 0)
            self.assertEqual(g.main([path, "--dry-run"]), 3)
        devnull.close()
        with open(os.path.join(out, "perf_ci_report.md"), encoding="utf-8") as f:
            self.assertIn("ticket=PASS", f.read())



class GradeR2Tests(unittest.TestCase):
    """Gate r2 (rulings Q1-Q8). r2 is not LOCKED yet, so most tests run --dry-run;
    test_locked_r2_pass simulates the LOCK by setting the expected hash."""

    def setUp(self):
        self._td = tempfile.TemporaryDirectory()
        self.fx = Fixture(self._td.name)
        tmp = self._td.name
        self.gate = os.path.join(tmp, "perf_ci_gate_r2.md")
        with open(self.gate, "w") as f:
            f.write("# PERF-CI r2 (test stand-in)\nGATE_BODY_SHA256: x\n")
        m = self.fx.m()
        m["gateVersion"] = "r2"
        m["gate"] = self.gate
        for i, mach in enumerate(m["machines"]):
            mach["machineId"] = f"03FF0210-04E0-057F-E006-E9070008000{i}"
            for run in mach["runs"]:
                if run["scenario"] == "idle":
                    # An idle app should not present at all: header only. (Every
                    # PresentMon row is a present, including the first one whose
                    # interval is NA, so write_presentmon would add one.)
                    with open(os.path.join(tmp, run["presentmonCsv"]), "w") as f:
                        f.write("Application,ProcessID,MsBetweenPresents\n")
                    run.update(durationSec=60, cpuAvgPercent=0.05, gpuAvgPercent=0.0)
        self.m = m
        self._saved = g.RULES["r2"]["sha"]

    def tearDown(self):
        g.RULES["r2"]["sha"] = self._saved
        self._td.cleanup()

    def run_(self, m=None, **kw):
        return self.fx.run(m or self.m, **kw)

    def test_unlocked_r2_rejects_real_grading(self):
        r = self.run_()
        self.assertEqual(r["ticket"], "INVALID")
        self.assertIn("not LOCKED", json.dumps(r))

    def test_dry_run_pass_with_idle_by_duration(self):
        r = self.run_(dry_run=True)
        self.assertEqual(r["ticket"], "DRY-RUN(PASS)", json.dumps(r, ensure_ascii=False)[:2000])
        idle = [c for c in r["cells"] if c["scenario"] == "idle"]
        self.assertTrue(idle and all(c["metrics"]["presents"] == 0 for c in idle))

    def test_locked_r2_pass(self):
        g.RULES["r2"]["sha"] = g.gate_body_sha256(self.gate)
        self.assertEqual(self.run_()["ticket"], "PASS")

    def test_idle_presents_fail(self):
        run = next(r for r in self.m["machines"][0]["runs"] if r["scenario"] == "idle")
        write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]), [16.6, 16.7], run["pid"])
        self.assertEqual(self.run_(dry_run=True)["ticket"], "DRY-RUN(FAIL)")

    def test_idle_cpu_fail_and_short_duration_invalid(self):
        run = next(r for r in self.m["machines"][0]["runs"] if r["scenario"] == "idle")
        run["cpuAvgPercent"] = 0.2
        self.assertEqual(self.run_(dry_run=True)["ticket"], "DRY-RUN(FAIL)")
        run["cpuAvgPercent"] = 0.05
        run["durationSec"] = 30
        self.assertEqual(self.run_(dry_run=True)["ticket"], "DRY-RUN(INVALID)")

    def test_fallback_allows_vsync_jitter(self):
        # +-5% jitter puts frame P95 near 1.05*T: FAIL under r1 (<= T), PASS under r2 (<= 1.1*T).
        T = 1000 / 120
        for i, run in enumerate(self.m["machines"][1]["runs"]):
            run["stageP95Ms"] = None
            if run["scenario"] != "idle":
                write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]), frames(1200, T, seed=i), run["pid"])
        self.assertEqual(self.run_(dry_run=True)["ticket"], "DRY-RUN(PASS)")
        r1 = copy.deepcopy(self.m)
        r1["gateVersion"] = "r1"
        r1["gate"] = os.path.abspath(GATE)
        for mach in r1["machines"]:
            mach["machineId"] = "id-" + mach["hostname"]
            for run in mach["runs"]:
                if run["scenario"] == "idle":
                    write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]), frames(1200, 1000 / mach["runs"][0]["refreshHz"], seed=3), run["pid"])
        self.assertIn("FAIL", self.fx.run(r1, dry_run=True)["ticket"])

    def test_small_count_reconcile_floor(self):
        run = next(r for r in self.m["machines"][0]["runs"] if r["scenario"] == "steady_play")
        write_presentmon(os.path.join(self._td.name, run["presentmonCsv"]), frames(1500, 1000 / 60, drops=1), run["pid"])
        # PresentStats says 0 drops, PresentMon 1: a conflict under r1, the same magnitude under r2.
        self.assertEqual(self.run_(dry_run=True)["ticket"], "DRY-RUN(PASS)")

    def test_machine_id_must_be_uuid(self):
        self.m["machines"][0]["machineId"] = "DESKTOP-K93N3P8"
        r = self.run_(dry_run=True)
        self.assertEqual(r["ticket"], "DRY-RUN(INVALID)")
        self.assertIn("SMBIOS UUID", json.dumps(r))

    def test_baseline_round_then_graded_round(self):
        g.RULES["r2"]["sha"] = g.gate_body_sha256(self.gate)
        b = copy.deepcopy(self.m)
        b["mode"] = "baseline"
        for mach in b["machines"]:
            mach.pop("baselines")
            mach["runs"] = [r for r in mach["runs"] if r["round"] == 1]
        rb = self.run_(b)
        self.assertEqual(rb["ticket"], "BASELINE", json.dumps(rb)[:1500])
        mid = b["machines"][0]["machineId"]
        self.assertIn("steady_play", rb["baselines"][mid])
        self.assertNotIn("idle", rb["baselines"][mid])
        path = os.path.join(self._td.name, "baselines.json")
        with open(path, "w") as f:
            json.dump(rb["baselines"], f)
        graded = copy.deepcopy(self.m)
        graded["baselinesFile"] = "baselines.json"
        for mach in graded["machines"]:
            mach.pop("baselines")
        self.assertEqual(self.run_(graded)["ticket"], "PASS")

    def test_baseline_mode_not_defined_for_r1(self):
        m = self.fx.m()
        m["mode"] = "baseline"
        self.assertIn("INVALID", self.fx.run(m)["ticket"])


if __name__ == "__main__":
    unittest.main()
