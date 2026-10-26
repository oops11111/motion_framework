#!/usr/bin/env python3
"""#52 offline grader: A v2 gates (locked p16_52_a_v2_gates.txt) + P1 frame-phase vs ε.
Usage: p52.py <raw> [out] [--session TAG]
Gates written BEFORE data; do not change.
"""
import numpy as np, re, sys, math
from pathlib import Path

RAW = sys.argv[1]
OUT = None
TAG = '?'
args = sys.argv[2:]
i = 0
while i < len(args):
    if args[i] == '--session' and i+1 < len(args):
        TAG = args[i+1]; i += 2
    elif OUT is None and not args[i].startswith('-'):
        OUT = args[i]; i += 1
    else:
        i += 1

r_, z_ = 0.32, 0.78
W0 = 2 * np.pi / r_
SHA = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
NH_T060 = 0.41
# A1c wide bounds (locked)
EPS_LO, EPS_HI = -40.0, 40.0
EPS_STEP = 0.5
MU_PLUS, MU_MINUS = 6.5, -10.0
R_MODE = 4.0
SEP_LO, SEP_HI = 14.0, 19.0
GATE_LOCKED_AT = '2026-10-24 20:33:42 AWST'
GATE_BODY_SHA = '189a08209f0c3e5dd664d3ca71b5f76d6fe246dfd96b0f67d15a12d93d1dea5d'

def basis(s, w0=W0, z=z_):
    wd = w0 * np.sqrt(1 - z * z)
    e = np.exp(-z * w0 * s)
    c = np.cos(wd * s); sn = np.sin(wd * s)
    return e * (c + (z * w0 / wd) * sn), e * sn / wd, -e * (w0 ** 2 / wd) * sn, e * (c - (z * w0 / wd) * sn)

def spring_amp(t, amp):
    t = np.asarray(t, float); s = np.maximum(t, 0)
    p1, p2, d1, d2 = basis(s)
    return np.where(t > 0, amp * (1 - p1), 0.0), np.where(t > 0, -amp * d1, 0.0)

def seeded_amp(t, amp, tau0):
    iv, ivel = spring_amp(tau0, amp)
    iv = float(np.asarray(iv).reshape(-1)[0])
    ivel = float(np.asarray(ivel).reshape(-1)[0])
    t = np.asarray(t, float); s = np.maximum(t, 0)
    p1, p2, d1, d2 = basis(s)
    x = amp + (iv - amp) * p1 + ivel * p2
    return x, (iv - amp) * d1 + ivel * d2

DG = np.arange(0, 0.15001, 0.0005)

def fitdelta_to_model(t, x, model_fn):
    best = None
    for d in DG:
        m = model_fn(t - d)
        if isinstance(m, tuple): m = m[0]
        resid = x - m
        sse = float(np.mean(resid ** 2))
        if best is None or sse < best[0]:
            best = (sse, d, float(np.sqrt(sse)), float(np.max(np.abs(resid))), float(np.median(np.abs(resid))))
    return best

def med(vals):
    vals = [v for v in vals if v is not None and np.isfinite(v)]
    return float(np.median(vals)) if vals else float('nan')

L = open(RAW, encoding='utf-8', errors='replace').read().splitlines()
o = []; P = o.append

def raws_after(prefix):
    out = []
    for i, l in enumerate(L):
        if l.startswith(prefix) and 'DONE' not in l and 'START' not in l and 'END' not in l and 'THROW' not in l:
            j = i + 1
            while j < len(L) and not L[j].startswith('RAW'):
                j += 1
            if j >= len(L): continue
            try:
                arr = np.array([tuple(map(float, x.split(':'))) for x in L[j][4:].split()])
            except Exception:
                continue
            out.append((l, arr))
    return out

wall = '?'
for l in L[:8]:
    m = re.search(r'wall=(\S+ \S+)', l)
    if m: wall = m.group(1); break
cal = next((l for l in L if l.startswith('cal ')), '')
has_a_end = any('STAGE A END' in l for l in L)
has_b_end = any('STAGE B END' in l for l in L)
has_ab_done = any('P14 AB DONE' in l for l in L)

P(f'# p16_52_a{TAG}.txt — 派题 #52 A v2 + P1 (session {TAG})')
P(f'# wall(raw)={wall}; {cal}')
P(f'# composition.h sha256={SHA} (must match; not modified)')
P(f'# A v2 GATE locked_at={GATE_LOCKED_AT}; GATE_BODY_SHA256={GATE_BODY_SHA}')
P(f'# N_h t060rev={NH_T060}; A1c ε∈[{EPS_LO},{EPS_HI}] step={EPS_STEP}; bound→INVALID')
P(f'# stages: A_end={has_a_end} B_end={has_b_end} AB_done={has_ab_done}')
P('')

# ---------- A1a (unchanged) ----------
A1A = []
for l, a in raws_after('A1A '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tau0 = float(d['tau0']) / 1000.0
    amp = float(d.get('amp', '240'))
    iv = float(d.get('iv', '0'))
    t = a[:, 2] / 1000.0; x = a[:, 1]
    if abs(iv) < 0.5 and tau0 < 1e-4:
        if len(t) < 8:
            A1A.append(dict(tau0=tau0*1000, ok=False)); continue
        best = fitdelta_to_model(t, x, lambda tt, amp=amp: spring_amp(tt, amp)[0])
        k0 = 0
    else:
        k0 = next((i for i, v in enumerate(x) if abs(v - iv) < max(3.0, 0.25*abs(iv)+1)), None)
        if k0 is None:
            k0 = next((i for i, v in enumerate(x) if abs(v) > 1.0), 0)
        t = t[k0:] - t[k0]; x = x[k0:]
        if len(t) < 8:
            A1A.append(dict(tau0=tau0*1000, ok=False)); continue
        best = None
        for dms in np.arange(-5.0, 25.1, 0.5):
            m = seeded_amp(t - dms/1000.0, amp, tau0)[0]
            resid = x - m
            sse = float(np.mean(resid**2))
            if best is None or sse < best[0]:
                best = (sse, dms/1000.0, float(np.sqrt(sse)), float(np.max(np.abs(resid))), float(np.median(np.abs(resid))))
    A1A.append(dict(tau0=tau0*1000, amp=amp, rms=best[2], maxabs=best[3], medabs=best[4],
                    d_ms=best[1]*1000, n=len(t), ok=True, pass_nh=best[2] <= NH_T060))

P('## A1a 种子平移（不变）')
P('| τ0 ms | n_reps | RMS中位 | max|err|中位 | 格门 |')
a1a_gate = True
for tau in [0.0, 16.7, 21.8, 33.3, 50.0]:
    m = [r for r in A1A if r.get('ok') and abs(r['tau0']-tau)<0.05]
    if not m:
        P(f'| {tau:.1f} | 0 | — | — | FAIL |'); a1a_gate=False; continue
    rms_m = med([r['rms'] for r in m]); mx_m = med([r['maxabs'] for r in m])
    ok = rms_m <= NH_T060
    if not ok: a1a_gate=False
    P(f'| {tau:.1f} | {len(m)} | {rms_m:.3f} | {mx_m:.3f} | {"PASS" if ok else "FAIL"} |')
P(f'- A1a: **{"PASS" if a1a_gate else "FAIL"}**')
P('')

# ---------- A1b origin=220 ----------
A1B = []
byb = {}
for l, a in raws_after('A1B '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    amp = float(d['amp']); origin = float(d.get('origin', '0'))
    t = a[:, 2] / 1000.0; x = a[:, 1]
    if len(t) < 8:
        A1B.append(dict(amp=amp, origin=origin, ok=False)); continue
    b = fitdelta_to_model(t, x, lambda tt, amp=amp, origin=origin: origin + spring_amp(tt, amp)[0])
    # offscreen: origin=0 amp<0 crop; or asymptote far from origin+amp
    target = origin + amp
    off = (origin < 1 and amp < 0) or (amp < 0 and abs(float(x[-1]) - target) > 15 and origin < 1)
    # with origin=220, expect on-screen
    if origin >= 200 and amp < 0:
        off = abs(float(x[-1]) - target) > 20 or len(x) < 12
    ok_nh = (not off) and b[2] <= NH_T060
    rec = dict(amp=amp, origin=origin, rms=b[2], maxabs=b[3], n=len(t), ok=True, offscreen=off, pass_nh=ok_nh)
    A1B.append(rec)
    byb.setdefault(amp, []).append(rec)

P('## A1b Δ线性（origin=220 for −amp）')
P('| amp | origin | n | RMS中位 | max|err|中位 | offscreen | ≤N_h |')
a1b_gate = True
for amp in [120.0, 240.0, -120.0, -240.0]:
    m = byb.get(amp, [])
    if not m:
        P(f'| {amp:.0f} | ? | 0 | — | — | — | FAIL |'); a1b_gate=False; continue
    orig = med([r['origin'] for r in m])
    rms = med([r['rms'] for r in m]); mx = med([r['maxabs'] for r in m])
    off = any(r['offscreen'] for r in m)
    # locked gate: per-amp median RMS ≤ N_h and not offscreen; origin must be 220 for −amp / 0 for +amp
    if amp < 0:
        ok = (not off) and (rms <= NH_T060) and abs(orig - 220) < 1
    else:
        ok = (not off) and (rms <= NH_T060) and abs(orig) < 1
    if not ok: a1b_gate = False
    P(f'| {amp:.0f} | {orig:.0f} | {len(m)} | {rms:.3f} | {mx:.3f} | {off} | {"PASS" if ok else "FAIL"} |')
P(f'- A1b: **{"PASS" if a1b_gate else "FAIL"}**')
P('')

# ---------- A1c wide ε + bimodal + INVALID ----------
A1C = []
for l, a in raws_after('A1C '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tInt = float(d.get('tInt', '60')) / 1000.0
    d0a = float(d.get('d0amp', '240'))
    d1a = float(d.get('d1amp', '-120'))
    tD1 = float(d.get('tD1', str(tInt*1000))) / 1000.0
    tickPer = float(d.get('tickPer', 'nan'))
    lastTick = float(d.get('lastTick', 'nan'))
    phiAct = float(d.get('phiAct', 'nan'))
    tPresent0 = float(d.get('tPresent0', 'nan'))
    rep = int(float(d.get('rep', '0')))
    t = a[:, 2] / 1000.0; x = a[:, 1]
    k0 = next((i for i, v in enumerate(x) if abs(v) < 12), 0)
    t, x = t[k0:], x[k0:]
    if len(t) < 8:
        A1C.append(dict(ok=False, rep=rep)); continue
    best = None
    for e_ms in np.arange(EPS_LO, EPS_HI + 1e-9, EPS_STEP):
        e = e_ms / 1000.0
        def model(tt, e=e):
            return spring_amp(tt, d0a)[0] + spring_amp(tt - tD1 - e, d1a)[0]
        b = fitdelta_to_model(t, x, model)
        if best is None or b[0] < best[0]:
            best = (*b, e_ms)
    eps = best[5]
    hit_lo = abs(eps - EPS_LO) < 1e-6
    hit_hi = abs(eps - EPS_HI) < 1e-6
    invalid = hit_lo or hit_hi
    # mode assign
    dp = abs(eps - MU_PLUS); dm = abs(eps - MU_MINUS)
    if min(dp, dm) <= R_MODE:
        mode = '+' if dp <= dm else '-'
    else:
        mode = 'OTHER'
    A1C.append(dict(ok=True, rep=rep, rms=best[2], maxabs=best[3], eps=eps, n=len(t),
                    invalid=invalid, mode=mode, tickPer=tickPer, lastTick=lastTick,
                    phiAct=phiAct, tPresent0=tPresent0, tD1=tD1*1000,
                    pass_nh=best[2] <= NH_T060))

P('## A1c 叠加（宽ε界；撞界=INVALID；双模）')
P('| rep | n | RMS | ε_fit | mode | invalid | tickPer | phiAct | tPresent0 | tD1 |')
n_inv = 0; n_ok = 0; n_other = 0
eps_plus = []; eps_minus = []; rms_ok = []
for r in A1C:
    if not r.get('ok'):
        P(f"| {r.get('rep','?')} | — | FAIL parse |"); continue
    n_ok += 1
    if r['invalid']: n_inv += 1
    if r['mode'] == 'OTHER' and not r['invalid']: n_other += 1
    if not r['invalid']:
        rms_ok.append(r['rms'])
        if r['mode'] == '+': eps_plus.append(r['eps'])
        elif r['mode'] == '-': eps_minus.append(r['eps'])
    P(f"| {r['rep']} | {r['n']} | {r['rms']:.3f} | {r['eps']:.1f} | {r['mode']} | {r['invalid']} | {r['tickPer']:.3f} | {r['phiAct']:.3f} | {r['tPresent0']:.3f} | {r['tD1']:.3f} |")

a1c_status = 'PASS'
a1c_reason = []
if n_inv > 0:
    a1c_status = 'INVALID'
    a1c_reason.append(f'{n_inv} trials hit ε bound')
elif n_ok < 5:
    a1c_status = 'FAIL'
    a1c_reason.append(f'valid trials {n_ok}<5')
else:
    if n_other > 0:
        a1c_status = 'FAIL'; a1c_reason.append(f'OTHER={n_other}')
    rms_m = med(rms_ok)
    if not (rms_m <= NH_T060):
        a1c_status = 'FAIL'; a1c_reason.append(f'RMS_med={rms_m:.3f}>N_h')
    if eps_plus and eps_minus:
        sep = abs(med(eps_plus) - med(eps_minus))
        if not (SEP_LO <= sep <= SEP_HI):
            a1c_status = 'FAIL'; a1c_reason.append(f'sep={sep:.1f} not in [{SEP_LO},{SEP_HI}]')
    # single-mode allowed
P(f'- n_valid={n_ok-n_inv} n_invalid={n_inv} n+/n−/OTHER={len(eps_plus)}/{len(eps_minus)}/{n_other}')
P(f'- med(ε|+)= {med(eps_plus):.1f}; med(ε|−)={med(eps_minus):.1f}; RMS中位={med(rms_ok):.3f}')
P(f'- A1c: **{a1c_status}**' + (f' ({"; ".join(a1c_reason)})' if a1c_reason else ''))
P('')

# ---------- A2 ----------
A2 = []
for l, a in raws_after('A2 '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    A2.append(dict(path=d.get('path','?'), line=l))
a2_gate = len(A2) >= 4 and all(str(r.get('path','')).upper()=='PS' for r in A2)
P('## A2 ε定义（不变）')
P(f'- A2 trials={len(A2)}; paths={set(r["path"] for r in A2)}')
P(f'- A2: **{"PASS" if a2_gate else "FAIL"}**')
P('')

# Session A
if a1c_status == 'INVALID':
    A_STATUS = 'INVALID'
elif a1a_gate and a1b_gate and a1c_status == 'PASS' and a2_gate:
    A_STATUS = 'PASS'
else:
    A_STATUS = 'FAIL'
P(f'## 会话 A: **{A_STATUS}** (A1a={a1a_gate} A1b={a1b_gate} A1c={a1c_status} A2={a2_gate})')
P('')

# ---------- P1: frame phase vs ε ----------
P('## P1 帧相位 vs ε 双模')
valid_p1 = [r for r in A1C if r.get('ok') and not r.get('invalid')]
tickPers = [r['tickPer'] for r in valid_p1 if np.isfinite(r['tickPer'])]
tickPer_med = med(tickPers)
P(f'- 实测 tickPer 中位={tickPer_med:.3f} ms (n={len(tickPers)}; 假设60Hz=16.667)')
# separation criterion: Fisher-like / Mann-Whitney + mode purity by phase half
# Define phase predictor: phi_mod = phiAct % tickPer; or (tPresent0 - tD1) relative
# Hypothesis: early-in-frame → one mode, late → other
rows_p1 = []
for r in valid_p1:
    tp = r['tickPer'] if np.isfinite(r['tickPer']) and r['tickPer'] > 1 else tickPer_med
    if not np.isfinite(tp) or tp < 1: tp = 16.667
    phi = r['phiAct']
    phi_mod = phi % tp if np.isfinite(phi) else float('nan')
    # present lag after d1 command
    lag = r['tPresent0'] - r['tD1'] if np.isfinite(r['tPresent0']) else float('nan')
    rows_p1.append(dict(rep=r['rep'], eps=r['eps'], mode=r['mode'], phiAct=phi, phi_mod=phi_mod,
                        tickPer=tp, tPresent0=r['tPresent0'], lag=lag, rms=r['rms']))

P('| rep | ε | mode | phiAct | phi_mod | tickPer | tPresent0 | lag=tP0−tD1 |')
for r in rows_p1:
    P(f"| {r['rep']} | {r['eps']:.1f} | {r['mode']} | {r['phiAct']:.3f} | {r['phi_mod']:.3f} | {r['tickPer']:.3f} | {r['tPresent0']:.3f} | {r['lag']:.3f} |")

# Separation tests
p1_result = '分不开'
p1_detail = []
modes_present = set(r['mode'] for r in rows_p1 if r['mode'] in ('+','-'))
if len(rows_p1) < 5:
    p1_detail.append('n<5')
elif len(modes_present) < 2:
    p1_detail.append(f'仅单模 {modes_present} — 无法检验相位是否分开双模')
    p1_result = '分不开'  # hard-stop: cannot separate modes if only one mode
else:
    # Test 1: phi_mod median differs between modes by >= 0.25*tickPer
    plus = [r for r in rows_p1 if r['mode']=='+']
    minus = [r for r in rows_p1 if r['mode']=='-']
    med_phi_p = med([r['phi_mod'] for r in plus])
    med_phi_m = med([r['phi_mod'] for r in minus])
    dphi = abs(med_phi_p - med_phi_m)
    thr = 0.25 * (tickPer_med if np.isfinite(tickPer_med) else 16.667)
    # circular distance
    tp = tickPer_med if np.isfinite(tickPer_med) else 16.667
    dphi_circ = min(dphi, tp - dphi) if np.isfinite(dphi) else 0
    p1_detail.append(f'med_phi_mod(+)= {med_phi_p:.2f} (−)={med_phi_m:.2f} d_circ={dphi_circ:.2f} thr={thr:.2f}')

    # Test 2: classify by phi_mod half → mode purity
    # threshold at midpoint between medians (or tp/2)
    cut = (med_phi_p + med_phi_m) / 2.0 if np.isfinite(med_phi_p+med_phi_m) else tp/2
    correct = 0
    for r in rows_p1:
        if r['mode'] not in ('+','-'): continue
        pred_plus = (r['phi_mod'] < cut) if med_phi_p < med_phi_m else (r['phi_mod'] >= cut)
        # actually: assign each half to nearest mode median
        dist_p = min(abs(r['phi_mod']-med_phi_p), tp-abs(r['phi_mod']-med_phi_p))
        dist_m = min(abs(r['phi_mod']-med_phi_m), tp-abs(r['phi_mod']-med_phi_m))
        pred = '+' if dist_p <= dist_m else '-'
        if pred == r['mode']: correct += 1
    n_bi = len(plus)+len(minus)
    purity = correct / n_bi if n_bi else 0
    p1_detail.append(f'phase→mode purity={purity:.2f} ({correct}/{n_bi})')

    # Test 3: lag (tPresent0-tD1) similarly
    med_lag_p = med([r['lag'] for r in plus])
    med_lag_m = med([r['lag'] for r in minus])
    dlag = abs(med_lag_p - med_lag_m) if np.isfinite(med_lag_p) and np.isfinite(med_lag_m) else 0
    p1_detail.append(f'med_lag(+)= {med_lag_p:.2f} (−)={med_lag_m:.2f} |Δ|={dlag:.2f}')

    # SEPARATES if: dphi_circ >= thr AND purity >= 0.75
    if dphi_circ >= thr and purity >= 0.75:
        p1_result = '分开'
    else:
        p1_result = '分不开'

P(f'- 分离判据: (a) |med φ_mod(+)-med φ_mod(−)|_circ ≥ 0.25·tickPer; (b) nearest-median 纯度 ≥0.75; 双满才算分开')
for dline in p1_detail:
    P(f'- {dline}')
P(f'- P1 结论: **{p1_result}**')
P('')

# ---------- Stage B quick c / ≈30 check ----------
P('## Stage B 旁路（≈30c 硬停扫描；不与#49横比）')
# Parse TRIAL lines for Sup arm; rough ε via same method as p51 if possible
trials_b = [l for l in L if l.startswith('TRIAL arm=')]
P(f'- TRIAL lines={len(trials_b)}')
# Look for c-like from Sup ε vs phi if we can fit quickly
sup_eps = []
for l, a in raws_after('TRIAL arm=Sup'):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    # skip — full ε fit expensive; scan raw header tags only for now
    pass
# Also check any logged c= in file
c_hits = []
for l in L:
    m = re.search(r'\bc=([0-9.]+)', l)
    if m: c_hits.append(float(m.group(1)))
approx30 = any(25 <= c <= 35 for c in c_hits)
P(f'- logged c values sample={c_hits[:12]}; ≈30c_seen={approx30}')
# Fit quick ε for Sup trials (subset)
from collections import defaultdict
sup_by_phi = defaultdict(list)
for l, a in raws_after('TRIAL arm=Sup '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    try:
        phi = float(d.get('phi', 'nan'))
        tInt_m = float(d.get('tInt_m', d.get('tInt','60')))/1000
        t = a[:, 2]/1000.0; x = a[:, 1]
        k0 = next((i for i,v in enumerate(x) if abs(v)<8), 0)
        t,x = t[k0:]-t[k0], x[k0:]  # rough
        if len(t)<8: continue
        # model d0=240 + d1 amp from b
        bnew = float(d.get('b','0')); amp = bnew - 240.0
        best=None
        tD = tInt_m
        for e_ms in np.arange(-5.0, 40.1, 0.5):
            e=e_ms/1000
            def model(tt, e=e):
                return spring_amp(tt, 240)[0] + spring_amp(tt - tD - e, amp)[0]
            # common delay
            b = fitdelta_to_model(t, x, model)
            if best is None or b[0]<best[0]:
                best=(*b, e_ms)
        if best:
            sup_by_phi[phi].append(best[5])
    except Exception:
        continue
c_est = None
if len(sup_by_phi) >= 2:
    # ε ≈ c - φ  → c = median(ε+φ)
    cs = []
    for phi, es in sup_by_phi.items():
        for e in es:
            cs.append(e + phi)
    c_est = med(cs)
    approx30 = approx30 or (c_est is not None and 25 <= c_est <= 35)
    P(f'- Sup ε+φ → c_est={c_est:.2f} ms (phis={sorted(sup_by_phi.keys())})')
else:
    P(f'- Sup ε fit sparse (n_phi={len(sup_by_phi)}); c_est unavailable from B short')
P(f'- ≈30c硬停触发: **{"YES" if approx30 else "NO"}**')
P('')

P('## ONELINE_SESSION')
P(f'A={A_STATUS}; A1a={a1a_gate}; A1b={a1b_gate}; A1c={a1c_status}; A2={a2_gate}; P1={p1_result}; approx30c={"YES" if approx30 else "NO"}; tickPer_med={tickPer_med:.3f}')

text = '\n'.join(o) + '\n'
print(text)
if OUT:
    open(OUT, 'w', encoding='utf-8').write(text)
    print('WROTE', OUT, file=sys.stderr)
