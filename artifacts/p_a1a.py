#!/usr/bin/env python3
"""T-A1a grader: focus-τ0 A1a + OFFSET_METHOD docs; RMS gate=#52 IV-rezero+seeded(τ0); A1b/c/A2 #52 A v2."""
import numpy as np, re, sys
RAW = sys.argv[1]
OUT = None
TAG = '?'
args = sys.argv[2:]; i = 0
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
EPS_LO, EPS_HI = -40.0, 40.0
EPS_STEP = 0.5
MU_PLUS, MU_MINUS = 6.5, -10.0
R_MODE = 4.0
SEP_LO, SEP_HI = 14.0, 19.0
GATE_LOCKED_AT = '2026-10-24 22:27:54 AWST'
GATE_BODY_SHA = '0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d'
FOCUS_TAU0 = [21.8, 3.3, 6.8, 8.3, 33.3]
DELTA0 = 39.5
MED_RES_REF = 2.70
CORR = 'CONFIRM_F5_REFUTE_2X'

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

def letter(status):
    s = str(status).upper()
    if s in ('PASS', 'P', 'TRUE'): return 'P'
    if s in ('FAIL', 'F', 'FALSE'): return 'F'
    if s in ('INVALID', 'I'): return 'I'
    return '?'

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
has_a_end = any('===== STAGE A END =====' in l for l in L)

P(f'# t_a1a_s{TAG}.txt — T-A1a session {TAG}')
P(f'# wall(raw)={wall}; {cal}')
P(f'# composition.h sha256={SHA} (must match; not modified)')
P(f'# GATE locked_at={GATE_LOCKED_AT}; GATE_BODY_SHA256={GATE_BODY_SHA}')
P(f'# corr={CORR}; OFFSET_METHOD residual=(d0-d_rest)-tau0; d0={DELTA0}; k=median(residual|unsat); ref=+{MED_RES_REF}')
P(f'# focus tau0={FOCUS_TAU0}; N_h={NH_T060}; A_end={has_a_end}')
P(f'# A1a RMS gate = #52 IV-rezero + seeded(commanded tau0); NOT 2x; NOT seed+k as gate')
P(f'# OFFSET_METHOD estimates/reports k (constant-offset口径); 偏离2x/魔术数→INVALID')
P('')

A1A = []
for l, a in raws_after('A1A '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tau0_ms = float(d['tau0']); tau0 = tau0_ms / 1000.0
    amp = float(d.get('amp', '240')); iv = float(d.get('iv', '0'))
    t_all = a[:, 2] / 1000.0; x_all = a[:, 1]
    if len(t_all) < 8:
        A1A.append(dict(tau0=tau0_ms, ok=False)); continue
    b_rest = fitdelta_to_model(t_all, x_all, lambda tt, amp=amp: spring_amp(tt, amp)[0])
    d_rest_ms = b_rest[1] * 1000.0
    residual = (DELTA0 - d_rest_ms) - tau0_ms
    unsat = (d_rest_ms > 0.5) and (tau0_ms > 0)
    if abs(iv) < 0.5 and tau0 < 1e-4:
        t = t_all; x = x_all
    else:
        k0 = next((i for i, v in enumerate(x_all) if abs(v - iv) < max(3.0, 0.25*abs(iv)+1)), None)
        if k0 is None:
            k0 = next((i for i, v in enumerate(x_all) if abs(v) > 1.0), 0)
        t = t_all[k0:] - t_all[k0]; x = x_all[k0:]
    if len(t) < 8:
        A1A.append(dict(tau0=tau0_ms, ok=False)); continue
    best0 = None
    for dms in np.arange(-5.0, 25.1, 0.5):
        m = seeded_amp(t - dms/1000.0, amp, tau0)[0]
        resid = x - m
        sse = float(np.mean(resid**2))
        if best0 is None or sse < best0[0]:
            best0 = (sse, dms, float(np.sqrt(sse)), float(np.max(np.abs(resid))), float(np.median(np.abs(resid))))
    A1A.append(dict(tau0=tau0_ms, amp=amp, ok=True, n=len(t),
                    d_rest=d_rest_ms, residual=residual, unsat=unsat,
                    rms=best0[2], maxabs=best0[3], d_ms=best0[1],
                    t=t, x=x, amp_v=amp, tau0_s=tau0))

res_unsat = [r['residual'] for r in A1A if r.get('ok') and r.get('unsat')]
used_2x = False
used_magic_single = False
if not res_unsat:
    k_est = float('nan'); method_ok = False
    method_note = 'no unsat residuals; cannot estimate k by OFFSET_METHOD'
else:
    k_est = med(res_unsat); method_ok = True
    method_note = f'median(residual|unsat)=+{k_est:.2f}; n_unsat={len(res_unsat)}; ref=+{MED_RES_REF}'

for r in A1A:
    if not r.get('ok'): continue
    if method_ok and np.isfinite(k_est):
        tau_corr = r['tau0_s'] + k_est/1000.0
        best = None
        for dms in np.arange(-5.0, 25.1, 0.5):
            mm = seeded_amp(r['t'] - dms/1000.0, r['amp_v'], tau_corr)[0]
            resid = r['x'] - mm
            sse = float(np.mean(resid**2))
            if best is None or sse < best[0]:
                best = (sse, dms, float(np.sqrt(sse)))
        r['rms_seed_k'] = best[2]
    else:
        r['rms_seed_k'] = float('nan')

P('## A1a focus grid + OFFSET_METHOD')
P(f'- corr={CORR}; OFFSET_METHOD=median(residual|unsat); d0={DELTA0}')
P(f'- k_est={k_est}; {method_note}')
P(f'- method_ok={method_ok}; used_2x={used_2x}; used_magic_single={used_magic_single}')
P('| tau0 | n | d_rest_med | residual_med | unsat | RMS_gate_med | RMS_seed+k_med | cell |')

a1a_status = 'PASS'; a1a_reasons = []
if not method_ok or used_2x or used_magic_single:
    a1a_status = 'INVALID'
    a1a_reasons.append(method_note if not method_ok else 'deviated from OFFSET_METHOD')

grid_rows = []
for tau in FOCUS_TAU0:
    m = [r for r in A1A if r.get('ok') and abs(r['tau0']-tau)<0.05]
    if len(m) < 5:
        a1a_status = 'INVALID'
        a1a_reasons.append(f'tau0={tau} n={len(m)}<5')
        grid_rows.append(dict(tau=tau, n=len(m), rms=float('nan'), rms_k=float('nan'), gate='INVALID'))
        P(f'| {tau:.1f} | {len(m)} | — | — | — | — | — | INVALID |')
        continue
    rms_m = med([r['rms'] for r in m])
    rms_k = med([r['rms_seed_k'] for r in m])
    d_rest_m = med([r['d_rest'] for r in m])
    res_m = med([r['residual'] for r in m])
    n_un = sum(1 for r in m if r['unsat'])
    ok = method_ok and (rms_m <= NH_T060)
    if method_ok and not ok and a1a_status == 'PASS':
        a1a_status = 'FAIL'
        a1a_reasons.append(f'tau0={tau} RMS={rms_m:.3f}>N_h')
    cell = 'PASS' if ok else ('FAIL' if method_ok else 'INVALID')
    grid_rows.append(dict(tau=tau, n=len(m), rms=rms_m, rms_k=rms_k, d_rest=d_rest_m, residual=res_m, gate=cell))
    P(f'| {tau:.1f} | {len(m)} | {d_rest_m:.1f} | {res_m:+.1f} | {n_un}/{len(m)} | {rms_m:.3f} | {rms_k:.3f} | {cell} |')

P(f'- A1a: **{a1a_status}**' + (f' ({"; ".join(a1a_reasons)})' if a1a_reasons else ''))
P('')

for r in A1A:
    r.pop('t', None); r.pop('x', None)

A1B = []; byb = {}
for l, a in raws_after('A1B '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    amp = float(d['amp']); origin = float(d.get('origin', '0'))
    t = a[:, 2] / 1000.0; x = a[:, 1]
    if len(t) < 8:
        A1B.append(dict(amp=amp, origin=origin, ok=False)); continue
    b = fitdelta_to_model(t, x, lambda tt, amp=amp, origin=origin: origin + spring_amp(tt, amp)[0])
    target = origin + amp
    off = (origin < 1 and amp < 0) or (amp < 0 and abs(float(x[-1]) - target) > 15 and origin < 1)
    if origin >= 200 and amp < 0:
        off = abs(float(x[-1]) - target) > 20 or len(x) < 12
    rec = dict(amp=amp, origin=origin, rms=b[2], maxabs=b[3], n=len(t), ok=True, offscreen=off)
    A1B.append(rec); byb.setdefault(amp, []).append(rec)

P('## A1b delta linearity (origin=220 for -amp)')
P('| amp | origin | n | RMS_med | maxerr_med | offscreen | cell |')
a1b_status = 'PASS'
for amp in [120.0, 240.0, -120.0, -240.0]:
    m = byb.get(amp, [])
    if len(m) < 5:
        P(f'| {amp:.0f} | ? | {len(m)} | — | — | — | INVALID |'); a1b_status='INVALID'; continue
    orig = med([r['origin'] for r in m]); rms = med([r['rms'] for r in m]); mx = med([r['maxabs'] for r in m])
    off = any(r['offscreen'] for r in m)
    if amp < 0:
        ok = (not off) and (rms <= NH_T060) and abs(orig - 220) < 1
    else:
        ok = (not off) and (rms <= NH_T060) and abs(orig) < 1
    if not ok and a1b_status == 'PASS': a1b_status = 'FAIL'
    P(f'| {amp:.0f} | {orig:.0f} | {len(m)} | {rms:.3f} | {mx:.3f} | {off} | {"PASS" if ok else "FAIL"} |')
P(f'- A1b: **{a1b_status}**')
P('')

A1C = []
for l, a in raws_after('A1C '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tInt = float(d.get('tInt', '60')) / 1000.0
    d0a = float(d.get('d0amp', '240')); d1a = float(d.get('d1amp', '-120'))
    tD1 = float(d.get('tD1', str(tInt*1000))) / 1000.0
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
    invalid = abs(eps - EPS_LO) < 1e-6 or abs(eps - EPS_HI) < 1e-6
    dp = abs(eps - MU_PLUS); dm = abs(eps - MU_MINUS)
    mode = ('+' if dp <= dm else '-') if min(dp, dm) <= R_MODE else 'OTHER'
    A1C.append(dict(ok=True, rep=rep, rms=best[2], eps=eps, n=len(t), invalid=invalid, mode=mode))

P('## A1c superposition')
P('| rep | n | RMS | eps_fit | mode | invalid |')
n_inv=0; n_ok=0; n_other=0; eps_plus=[]; eps_minus=[]; rms_ok=[]
for r in A1C:
    if not r.get('ok'):
        P(f"| {r.get('rep','?')} | — | FAIL parse |"); continue
    n_ok += 1
    if r['invalid']: n_inv += 1
    if r['mode']=='OTHER' and not r['invalid']: n_other += 1
    if not r['invalid']:
        rms_ok.append(r['rms'])
        if r['mode']=='+': eps_plus.append(r['eps'])
        elif r['mode']=='-': eps_minus.append(r['eps'])
    P(f"| {r['rep']} | {r['n']} | {r['rms']:.3f} | {r['eps']:.1f} | {r['mode']} | {r['invalid']} |")

a1c_status='PASS'; a1c_reason=[]
if n_inv>0:
    a1c_status='INVALID'; a1c_reason.append(f'{n_inv} hit eps bound')
elif n_ok<5:
    a1c_status='FAIL'; a1c_reason.append(f'n={n_ok}<5')
else:
    if n_other>0: a1c_status='FAIL'; a1c_reason.append(f'OTHER={n_other}')
    rms_m=med(rms_ok)
    if not (rms_m<=NH_T060): a1c_status='FAIL'; a1c_reason.append(f'RMS_med={rms_m:.3f}>N_h')
    if eps_plus and eps_minus:
        sep=abs(med(eps_plus)-med(eps_minus))
        if not (SEP_LO<=sep<=SEP_HI): a1c_status='FAIL'; a1c_reason.append(f'sep={sep:.1f}')
P(f'- n_valid={n_ok-n_inv} n_invalid={n_inv} n+/n-/OTHER={len(eps_plus)}/{len(eps_minus)}/{n_other}')
P(f'- med(eps|+)= {med(eps_plus):.1f}; med(eps|-)= {med(eps_minus):.1f}; RMS_med={med(rms_ok):.3f}')
P(f'- A1c: **{a1c_status}**' + (f' ({"; ".join(a1c_reason)})' if a1c_reason else ''))
P('')

A2=[]
for l,a in raws_after('A2 '):
    d=dict(re.findall(r'(\w+)=(\S+)', l)); A2.append(dict(path=d.get('path','?')))
if len(A2)<4: a2_status='INVALID'
elif all(str(r.get('path','')).upper()=='PS' for r in A2): a2_status='PASS'
else: a2_status='FAIL'
P('## A2 path=PS')
P(f'- A2 trials={len(A2)}; paths={set(r["path"] for r in A2)}')
P(f'- A2: **{a2_status}**')
P('')

statuses=[a1a_status,a1b_status,a1c_status,a2_status]
if 'INVALID' in statuses: A_STATUS='INVALID'
elif all(s=='PASS' for s in statuses): A_STATUS='PASS'
else: A_STATUS='FAIL'
P(f'## Session A: **{A_STATUS}** (A1a={a1a_status} A1b={a1b_status} A1c={a1c_status} A2={a2_status})')
P('')
P('## ONELINE_SESSION')
P(f'A={A_STATUS}; A1a={letter(a1a_status)}; A1b={letter(a1b_status)}; A1c={letter(a1c_status)}; A2={letter(a2_status)}; k_est={k_est}; method_ok={method_ok}')
P('')
P('## MACHINE')
P(f'A1A={letter(a1a_status)}')
P(f'A1B={letter(a1b_status)}')
P(f'A1C={letter(a1c_status)}')
P(f'A2={letter(a2_status)}')
P(f'A={letter(A_STATUS)}')
P(f'K_EST={k_est}')
P(f'METHOD_OK={method_ok}')
for gr in grid_rows:
    P(f"GRID tau={gr['tau']} n={gr['n']} rms={gr['rms']} rms_seed_k={gr.get('rms_k')} gate={gr['gate']}")

text='\n'.join(o)+'\n'
if OUT:
    open(OUT,'w',encoding='utf-8').write(text)
    print('WROTE', OUT, file=sys.stderr)
try:
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
except Exception:
    pass
print(text)
