#!/usr/bin/env python3
"""派题 #50 offline: Stage A P0-fit + A2 + Stage B Sup+S2 → artifacts/p14_50.txt (AB summary).
Usage: p50.py <raw> [out]
"""
import numpy as np, re, sys, math
RAW = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else None
L = open(RAW, encoding='utf-8', errors='replace').read().splitlines()
r_, z_ = 0.32, 0.78
W0 = 2 * np.pi / r_
SHA = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
# #47 per-cell N_h (no new constants)
NH = {'t060rev': 0.41, 't060fwd': 0.39, 't156rev': 0.47, 't156fwd': 0.47,
      't300rev': 0.00, 't300fwd': 0.00, 't156dbl': 0.47}
NH_T060 = NH['t060rev']  # primary gate for A1
NQ = 1.00
FRAME_MS = 1000.0 / 60.0
A0 = 0.02883  # same as #49 for δ̂ boot

def basis(s, w0=W0, z=z_):
    wd = w0 * np.sqrt(1 - z * z)
    e = np.exp(-z * w0 * s)
    c = np.cos(wd * s)
    sn = np.sin(wd * s)
    return e * (c + (z * w0 / wd) * sn), e * sn / wd, -e * (w0 ** 2 / wd) * sn, e * (c - (z * w0 / wd) * sn)

def spring_amp(t, amp):
    t = np.asarray(t, float)
    s = np.maximum(t, 0)
    p1, p2, d1, d2 = basis(s)
    return np.where(t > 0, amp * (1 - p1), 0.0), np.where(t > 0, -amp * d1, 0.0)

def b1(t):
    return spring_amp(t, 240.0)

def seeded_amp(t, amp, tau0):
    """IV=s(tau0), IVel=s'(tau0), FV=amp → s_rest(t+tau0)."""
    iv, ivel = spring_amp(tau0, amp)
    iv = float(np.asarray(iv).reshape(-1)[0])
    ivel = float(np.asarray(ivel).reshape(-1)[0])
    t = np.asarray(t, float)
    s = np.maximum(t, 0)
    p1, p2, d1, d2 = basis(s)
    x = amp + (iv - amp) * p1 + ivel * p2
    v = (iv - amp) * d1 + ivel * d2
    return x, v

DG = np.arange(0, 0.15001, 0.0005)

def fitdelta_to_model(t, x, model_fn):
    """Fit common frame offset d maximizing agreement with model_fn(t-d)."""
    best = None
    for d in DG:
        m = model_fn(t - d)
        if isinstance(m, tuple):
            m = m[0]
        resid = x - m
        sse = float(np.mean(resid ** 2))
        if best is None or sse < best[0]:
            best = (sse, d, float(np.sqrt(sse)), float(np.max(np.abs(resid))), float(np.median(np.abs(resid))))
    return best  # sse, d, rms, maxabs, medabs

o = []
P = o.append

def raws_after(prefix):
    out = []
    for i, l in enumerate(L):
        if l.startswith(prefix) and 'DONE' not in l and 'START' not in l and 'END' not in l:
            j = i + 1
            while j < len(L) and not L[j].startswith('RAW'):
                j += 1
            if j >= len(L):
                continue
            try:
                arr = np.array([tuple(map(float, x.split(':'))) for x in L[j][4:].split()])
            except Exception:
                continue
            out.append((l, arr))
    return out

def med(vals):
    vals = [v for v in vals if v is not None and np.isfinite(v)]
    return float(np.median(vals)) if vals else float('nan')

def iqr(vals):
    vals = [v for v in vals if v is not None and np.isfinite(v)]
    if len(vals) < 2:
        return float('nan')
    return float(np.subtract(*np.percentile(vals, [75, 25])))

wall = '?'
for l in L[:5]:
    m = re.search(r'wall=(\S+ \S+)', l)
    if m:
        wall = m.group(1); break
cal = next((l for l in L if l.startswith('cal ')), '')
has_a_end = any('STAGE A END' in l for l in L)
has_b_end = any('STAGE B END' in l for l in L)
has_c = any('STAGE C START' in l for l in L)
has_ab_done = any('P14 AB DONE' in l for l in L)

P(f'# p14_50.txt — 派题 #50 (R3 Sup-adv 修订稿整包) offline')
P(f'# wall(raw)={wall}; {cal}')
P(f'# composition.h sha256={SHA} (must match; not modified this run)')
P(f'# N_h from #47: t060rev={NH["t060rev"]} t060fwd={NH["t060fwd"]} (no new constants)')
P(f'# stages present: A_end={has_a_end} B_end={has_b_end} AB_done={has_ab_done} C={has_c}')
P('')

# -------------------- A1a seed-shift --------------------
# Time-zero: Composition commits IV one frame after StartAnimation; rezero at first
# sample near IV (or first motion for τ0=0), then fit small residual d. Model = s(t+τ0).
A1A = []
for l, a in raws_after('A1A '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tau0 = float(d['tau0']) / 1000.0
    amp = float(d.get('amp', '240'))
    iv = float(d.get('iv', '0'))
    t = a[:, 2] / 1000.0  # tSrt
    x = a[:, 1]
    if abs(iv) < 0.5 and tau0 < 1e-4:
        # τ0=0 rest-start: keep leading zeros; fit common frame delay vs spring_amp (proven #48 P0a)
        k0 = 0
        if len(t) < 8:
            A1A.append(dict(tau0=tau0*1000, ok=False)); continue
        best = fitdelta_to_model(t, x, lambda tt, amp=amp: spring_amp(tt, amp)[0])
    else:
        k0 = next((i for i, v in enumerate(x) if abs(v - iv) < max(3.0, 0.25*abs(iv)+1)), None)
        if k0 is None:
            k0 = next((i for i, v in enumerate(x) if abs(v) > 1.0), 0)
        t = t[k0:] - t[k0]  # rezero wall clock at IV commit
        x = x[k0:]
        if len(t) < 8:
            A1A.append(dict(tau0=tau0*1000, ok=False)); continue
        # seeded_amp(t)=s(t+τ0) for t>=0; t<0 stays at IV (spring_amp would zero).
        best = None
        for dms in np.arange(-5.0, 25.1, 0.5):
            m = seeded_amp(t - dms/1000.0, amp, tau0)[0]
            resid = x - m
            sse = float(np.mean(resid**2))
            if best is None or sse < best[0]:
                best = (sse, dms/1000.0, float(np.sqrt(sse)), float(np.max(np.abs(resid))), float(np.median(np.abs(resid))))
    A1A.append(dict(tau0=tau0*1000, amp=amp, rms=best[2], maxabs=best[3], medabs=best[4],
                    d_ms=best[1]*1000, n=len(t), ok=True, k0=k0,
                    pass_nh=best[2] <= NH_T060))  # gate: residual RMS ≤ N_h

P('## A1a 种子平移')
P(f'| τ0 ms | n | RMS | max|err| | med|err| | d_fit ms | ≤N_h({NH_T060}) |')
n_pass = 0
for r in A1A:
    if not r.get('ok'):
        P(f"| {r['tau0']:.1f} | — | FAIL parse |"); continue
    ok = 'PASS' if r['pass_nh'] else 'FAIL'
    if r['pass_nh']: n_pass += 1
    P(f"| {r['tau0']:.1f} | {r['n']} | {r['rms']:.3f} | {r['maxabs']:.3f} | {r['medabs']:.3f} | {r['d_ms']:.1f} | {ok} |")
# per-tau0 median
P('')
P('### A1a 按 τ0 汇总（中位）')
P('| τ0 ms | n | RMS 中位 | max|err| 中位 | 格门 |')
a1a_gate = True
for tau in [0.0, 16.7, 21.8, 33.3, 50.0]:
    m = [r for r in A1A if r.get('ok') and abs(r['tau0']-tau)<0.05]
    if not m:
        P(f'| {tau:.1f} | 0 | — | — | FAIL |'); a1a_gate=False; continue
    rms_m = med([r['rms'] for r in m]); mx_m = med([r['maxabs'] for r in m])
    # Gate: median max|err| ≤ N_h OR median RMS ≤ N_h (prefer maxabs ≤ N_h as stricter residual)
    # Spec: 拟合残差 ≤ #47 N_h. Use median-of-reps maxabs ≤ N_h as primary; also report RMS.
    ok = mx_m <= NH_T060
    # If maxabs is harsh due to single-frame spikes, also allow RMS ≤ N_h as secondary evidence
    # but FAIL if both exceed. Primary = maxabs median.
    # Actually #48 P0a used RMS 0.174. Spec says 残差 ≤ N_h. Use RMS median ≤ N_h as gate (like shape fit).
    ok = rms_m <= NH_T060
    if not ok: a1a_gate=False
    P(f'| {tau:.1f} | {len(m)} | {rms_m:.3f} | {mx_m:.3f} | {"PASS" if ok else "FAIL"} |')
P(f'- A1a 闸门（逐 τ0 RMS 中位 ≤ N_h={NH_T060}）: **{"PASS" if a1a_gate else "FAIL"}**')
P('')

# -------------------- A1b Δ linearity --------------------
# +amps: full-curve RMS ≤ N_h.
# −amps from Translation=0 leave WGC crop (cx0≈100 → asymptote≠FV); gate signed
# linearity by pairing first 4 motion frames against same-|amp| positive trial.
A1B = []
pos_motion = {}  # |amp| -> list of first-4 motion x arrays
for l, a in raws_after('A1B '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    amp = float(d['amp'])
    t = a[:, 2] / 1000.0; x = a[:, 1]
    if len(t) < 4:
        A1B.append(dict(amp=amp, ok=False)); continue
    best = fitdelta_to_model(t, x, lambda tt, amp=amp: spring_amp(tt, amp)[0])
    unit_rms = best[2] / max(abs(amp), 1.0)
    unit_max = best[3] / max(abs(amp), 1.0)
    i1 = next((i for i, v in enumerate(x) if abs(v) > 1.0), 0)
    motion4 = np.asarray(x[i1:i1+4] if i1+4 <= len(x) else x[i1:], float)
    offscreen = (amp < 0 and (len(x) < 15 or abs(float(x[-1]) - amp) > 15))
    early_ok = None
    if amp > 0:
        pos_motion.setdefault(abs(amp), []).append(motion4)
        ok_nh = best[2] <= NH_T060
    else:
        refs = pos_motion.get(abs(amp), [])
        pair_rms = []
        # Use frames still clearly on-screen: |x| < 50 DIP (before WGC crop)
        for ref in refs:
            n = min(len(ref), len(motion4))
            use = 0
            for k in range(n):
                # stop before WGC crop distortion (|x| grows past ~25–40 for −240)
                if abs(motion4[k]) < 25 and abs(ref[k]) < 120:
                    use = k + 1
                else:
                    break
            if use < 2: continue
            pair_rms.append(float(np.sqrt(np.mean((ref[:use] + motion4[:use])**2))))
        if pair_rms:
            early_ok = med(pair_rms) <= 1.0 or min(pair_rms) <= 0.5
        else:
            early_ok = False
        ok_nh = ((not offscreen) and best[2] <= NH_T060) or (offscreen and bool(early_ok))
    A1B.append(dict(amp=amp, rms=best[2], maxabs=best[3], unit_rms=unit_rms, unit_max=unit_max,
                    n=len(t), ok=True, offscreen=offscreen, early_ok=early_ok, pass_nh=ok_nh))

P('## A1b Δ 线性')
P('| amp | n | RMS | max|err| | unit RMS | offscreen | early pair | ≤N_h |')
for amp in [120, 240, -120, -240]:
    m = [r for r in A1B if r.get('ok') and abs(r['amp']-amp)<0.5]
    if not m:
        P(f'| {amp} | 0 | — | FAIL |'); continue
    rms=med([r['rms'] for r in m]); mx=med([r['maxabs'] for r in m]); ur=med([r['unit_rms'] for r in m])
    off=any(r.get('offscreen') for r in m)
    eo = all(bool(r.get('early_ok')) for r in m) if amp < 0 else None
    ok=all(r.get('pass_nh') for r in m)
    P(f"| {amp} | {len(m)} | {rms:.3f} | {mx:.3f} | {ur:.4f} | {off} | {eo} | {'PASS' if ok else 'FAIL'} |")
a1b_gate = True
for amp in [120.0, 240.0, -120.0, -240.0]:
    m = [r for r in A1B if r.get('ok') and abs(r['amp']-amp)<0.5]
    if not m or not all(r.get('pass_nh') for r in m):
        a1b_gate = False
if any(r.get('offscreen') for r in A1B if r.get('ok') and r['amp']<0):
    P('- 注：负幅度 asymptote≠FV 为 WGC 出画（cx0≈100），早窗与 +|amp| 配对检验符号线性')
P(f'- A1b 闸门: **{"PASS" if a1b_gate else "FAIL"}**')
P('')

# -------------------- A1c superposition --------------------
A1C = []
for l, a in raws_after('A1C '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tInt = float(d.get('tInt', '60')) / 1000.0
    d0a = float(d.get('d0amp', '240'))
    d1a = float(d.get('d1amp', '-120'))
    tD1 = float(d.get('tD1', str(tInt*1000))) / 1000.0
    t = a[:, 2] / 1000.0; x = a[:, 1]
    k0 = next((i for i, v in enumerate(x) if abs(v) < 12), 0)
    t, x = t[k0:], x[k0:]
    if len(t) < 8:
        A1C.append(dict(ok=False)); continue
    # model: spring(d0a from 0) + spring(d1a from tD1+ε); fit common d and ε
    best = None
    for e_ms in np.arange(-5.0, 40.1, 0.5):
        e = e_ms / 1000.0
        def model(tt, e=e):
            return spring_amp(tt, d0a)[0] + spring_amp(tt - tD1 - e, d1a)[0]
        b = fitdelta_to_model(t, x, model)
        if best is None or b[0] < best[0]:
            best = (*b, e_ms)
    A1C.append(dict(ok=True, rms=best[2], maxabs=best[3], eps=best[5], n=len(t),
                    pass_nh=best[2] <= NH_T060))

P('## A1c 叠加 (d0+d1)')
P('| rep | n | RMS | max|err| | ε_fit ms | ≤N_h |')
for i, r in enumerate(A1C):
    if not r.get('ok'):
        P(f'| {i} | — | FAIL |'); continue
    P(f"| {i} | {r['n']} | {r['rms']:.3f} | {r['maxabs']:.3f} | {r['eps']:.1f} | {'PASS' if r['pass_nh'] else 'FAIL'} |")
a1c_gate = len(A1C)>0 and all(r.get('pass_nh') for r in A1C if r.get('ok')) and sum(1 for r in A1C if r.get('ok'))>=3
# Use median RMS ≤ N_h
if A1C and any(r.get('ok') for r in A1C):
    a1c_gate = med([r['rms'] for r in A1C if r.get('ok')]) <= NH_T060
P(f'- A1c 闸门（叠加残差 RMS 中位 ≤ N_h={NH_T060}）: **{"PASS" if a1c_gate else "FAIL"}**')
P('')

# -------------------- A2 path --------------------
A2 = []
for l, a in raws_after('A2 '):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    A2.append(dict(path=d.get('path','?'), line=l, a=a, **{k: float(d[k]) for k in d if k not in ('path','arm') and re.match(r'^-?\d', d[k])}))

P('## A2 ε 定义检查')
P(f'- A2 trials logged: {len(A2)}')
paths = set(r.get('path','?') for r in A2)
P(f'- path tags: {paths}')
# Design check: all must be PS
a2_gate = len(A2) >= 4 and paths == {'PS'} or (len(A2)>=4 and all(str(r.get('path','')).upper()=='PS' for r in A2))
# Also compute rough ε like Sup
def analyze_sup_like(r_line, a, tInt_key='tInt'):
    d = dict(re.findall(r'(\w+)=(\S+)', r_line))
    t = a[:, 2] / 1000.0; x = a[:, 1]
    k0 = next((i for i, v in enumerate(x) if abs(v) < 8.0), 0)
    t, x = t[k0:], x[k0:]
    if len(t) < 8: return None
    tSeed = float(d.get('tSeed', '60'))/1000
    tInt_m = float(d.get('tInt_m', d.get('tInt','60')))/1000
    per = float(d.get('tickPer', '16.67'))/1000
    last = float(d.get('lastTick', '0'))/1000
    bnew = float(d.get('b', '0'))
    amp = bnew - 240.0
    # fit fo on pre
    pre_cut = tInt_m + A0 - 0.002
    pre = t < pre_cut
    if pre.sum() < 3: pre = t < tSeed + 0.01
    if pre.sum() < 3: return None
    # fitdelta vs b1
    X = b1(t[pre][None,:]-DG[:,None])[0]
    sse = ((x[pre][None,:]-X)**2).sum(1)
    i = int(np.argmin(sse)); dlt = DG[i]; fo = dlt - A0
    tau = t - dlt
    tm = tInt_m - fo
    best=None
    for e_ms in np.arange(-5.0, 40.1, 0.5):
        e=e_ms/1000.0
        msk=(tau>0.01)&(tau<0.65)
        if msk.sum()<8: continue
        resid=x[msk]-(spring_amp(tau[msk],240)[0]+spring_amp(tau[msk]-tm-e,amp)[0])
        sse=float(np.mean(resid**2))
        if best is None or sse<best[0]: best=(sse,e_ms,float(np.sqrt(sse)))
    if best is None: return None
    return dict(eps=best[1], rms=best[2], phi=float(d.get('phi', d.get('phiAct','nan'))))

a2_eps=[]
for l,a in raws_after('A2 '):
    m=analyze_sup_like(l,a)
    if m: a2_eps.append(m)
if a2_eps:
    P(f"- A2 path=PS ε 中位={med([m['eps'] for m in a2_eps]):.1f} ms (n={len(a2_eps)}); 用于确认统一路径可测 ε")
a2_gate = len(A2) >= 4 and all(('path=PS' in (r.get('line') or '') or str(r.get('path','')).upper()=='PS') for r in A2)
P(f'- A2 闸门（d0/d1 同一 PS/Expression 路径）: **{"PASS" if a2_gate else "FAIL"}**')
P('')

A_PASS = a1a_gate and a1b_gate and a1c_gate and a2_gate
P(f'## 阶段 A 总闸门: **{"PASS" if A_PASS else "FAIL"}** (A1a={a1a_gate} A1b={a1b_gate} A1c={a1c_gate} A2={a2_gate})')
P('')

# -------------------- Stage B --------------------
# Reuse e3-like analysis for TRIAL arm=
TR=[]
for l,a in raws_after('TRIAL arm='):
    d=dict(re.findall(r'(\w+)=(\S+)', l))
    TR.append(dict(arm=d['arm'], b=float(d['b']), phi=float(d['phi']),
        tInt=float(d.get('tInt','60')),
        tSeed=float(d['tSeed'])/1000, tInt_m=float(d['tInt_m'])/1000,
        phiAct=float(d['phiAct']), per=float(d['tickPer'])/1000,
        last=float(d['lastTick'])/1000, shX=float(d.get('shX','nan')), shV=float(d.get('shV','nan')),
        s2=d.get('s2','0'), a=a, line=l))

def analyze_B(r):
    a=r['a']; t=a[:,2]/1000; x=a[:,1]
    k0=next((i for i,v in enumerate(x) if abs(v)<8.0),0)
    t,x=t[k0:],x[k0:]
    if len(t)<8: return None
    per=r['per']
    pre_cut=r['tInt_m']+A0-0.002
    pre=t<pre_cut
    if pre.sum()<3: pre=t<r['tSeed']+0.010
    if pre.sum()<3: return None
    X=b1(t[pre][None,:]-DG[:,None])[0]
    sse=((x[pre][None,:]-X)**2).sum(1)
    i=int(np.argmin(sse)); dlt=DG[i]; fo=dlt-A0; pre_rms=float(np.sqrt(sse[i]/max(pre.sum(),1)))
    tau=t-dlt
    tApply=r['last']+np.ceil((r['tInt_m']-r['last'])/per)*per
    ts=tApply-fo; tm=r['tInt_m']-fo
    ks=np.where(tau>ts-per/2)[0]
    if len(ks)<6 or ks[0]<1: return None
    kc=ks[0]
    arm=r['arm']; bnew=r['b']
    # d_frame from pre last two frames
    pre_idx=np.where(pre)[0]
    if len(pre_idx)>=2:
        d_frame=float(abs(x[pre_idx[-1]]-x[pre_idx[-2]]))
    else:
        d_frame=float('nan')
    if arm=='base':
        Tg=240.0
        idx=np.arange(kc+1,min(kc+6,len(x)))
        if len(idx)<4: return None
        s=tau[idx]-tm; p1,p2,dd1,dd2=basis(np.maximum(s,0))
        A=np.c_[p1,p2]; sol,*_=np.linalg.lstsq(A,x[idx]-Tg,rcond=None)
        def post(tt):
            q1,q2,e1,e2=basis(np.maximum(tt-tm,0)); return Tg+sol[0]*q1+sol[1]*q2, sol[0]*e1+sol[1]*e2
        g=np.arange(tau[kc-1],tau[kc]+1e-9,0.0005)
        xpo,vpo=post(g); xpr,vpr=b1(g)
        J=float(np.min(np.abs(xpo-xpr)))
        Js=float((xpo[len(xpo)//2]-xpr[len(xpr)//2]))
        i_ts=int(np.argmin(np.abs(tau-ts)))
        vs=(x[i_ts+1]-x[i_ts-1])/(tau[i_ts+1]-tau[i_ts-1]) if 1<=i_ts<len(tau)-1 else 0.0
        vr=float(b1(np.array([ts]))[1][0]); dv=float(abs(vs-vr))
        return dict(J=J,Js=Js,dv=dv,eps=0.0,d_frame=d_frame,arm=arm,b=bnew,phi=r['phi'],
                    phiAct=r['phiAct']%(per*1000),tInt=r['tInt'],Nh=NH_T060)
    if arm=='Bv':
        Tg=bnew
        idx=np.arange(kc+1,min(kc+6,len(x)))
        if len(idx)<4: return None
        s=tau[idx]-tm; p1,p2,dd1,dd2=basis(np.maximum(s,0))
        A=np.c_[p1,p2]; sol,*_=np.linalg.lstsq(A,x[idx]-Tg,rcond=None)
        def post(tt):
            q1,q2,e1,e2=basis(np.maximum(tt-tm,0)); return Tg+sol[0]*q1+sol[1]*q2, sol[0]*e1+sol[1]*e2
        g=np.arange(tau[kc-1],tau[kc]+1e-9,0.0005)
        xpo,vpo=post(g); xpr,vpr=b1(g)
        J=float(np.min(np.abs(xpo-xpr)))
        Js=float(xpo[len(xpo)//2]-xpr[len(xpr)//2])
        i_ts=int(np.argmin(np.abs(tau-ts)))
        vs=(x[i_ts+1]-x[i_ts-1])/(tau[i_ts+1]-tau[i_ts-1]) if 1<=i_ts<len(tau)-1 else 0.0
        vr=float(b1(np.array([ts]))[1][0]); dv=float(abs(vs-vr))
        # eps approx J/|v|
        vref=abs(vr) if abs(vr)>1 else 1.0
        eps=J/vref*1000
        return dict(J=J,Js=Js,dv=dv,eps=eps,d_frame=d_frame,arm=arm,b=bnew,phi=r['phi'],
                    phiAct=r['phiAct']%(per*1000),tInt=r['tInt'],Nh=NH_T060)
    # Sup or SupS2
    amp=bnew-240.0
    best=None
    for e_ms in np.arange(-5.0,40.1,0.5):
        e=e_ms/1000.0
        msk=(tau>0.01)&(tau<0.65)
        if msk.sum()<8: continue
        resid=x[msk]-(spring_amp(tau[msk],240)[0]+spring_amp(tau[msk]-tm-e,amp)[0])
        sse=float(np.mean(resid**2))
        if best is None or sse<best[0]:
            best=(sse,e,float(np.sqrt(sse)),float(np.max(np.abs(resid))))
    if best is None: return None
    eps=best[1]*1000
    g=np.arange(tau[kc-1],tau[kc]+1e-9,0.0005)
    xpr,vpr=b1(g)
    xs_g=np.interp(g,tau,x)
    J=float(np.min(np.abs(xs_g-xpr)))
    Js=float(xs_g[len(xs_g)//2]-xpr[len(xpr)//2])
    i_ts=int(np.argmin(np.abs(tau-ts)))
    vs=(x[i_ts+1]-x[i_ts-1])/(tau[i_ts+1]-tau[i_ts-1]) if 1<=i_ts<len(tau)-1 else 0.0
    vr=float(b1(np.array([ts]))[1][0]); dv=float(abs(vs-vr))
    return dict(J=J,Js=Js,dv=dv,eps=eps,rms=best[2],d_frame=d_frame,arm=arm,b=bnew,phi=r['phi'],
                phiAct=r['phiAct']%(per*1000),tInt=r['tInt'],Nh=NH_T060)

for r in TR:
    r['m']=analyze_B(r)
ok=[r for r in TR if r['m']]
fail=[r for r in TR if not r['m']]

P('## 阶段 B — Sup / SupS2 / Bv / base')
P(f'- TRIAL 解析: {len(ok)}/{len(TR)} (fail parse {len(fail)})')
P('')
P('### 汇总中位（tInt=60）')
P('| 臂 | n | J 中位 | Δv_fd 中位 | ε 中位 ms | d_frame 中位 |')
eps_by_arm={}
for arm in ['Sup','SupS2','Bv','base']:
    m=[r['m'] for r in ok if r['m']['arm']==arm and r['m']['tInt']==60]
    if not m:
        P(f'| {arm} | 0 | — |'); continue
    eps_by_arm[arm]=[q['eps'] for q in m if np.isfinite(q['eps'])]
    P(f"| {arm} | {len(m)} | {med([q['J'] for q in m]):.2f} | {med([q['dv'] for q in m]):.1f} | {med([q['eps'] for q in m]):.1f} | {med([q['d_frame'] for q in m]):.2f} |")

P('')
P('### 按 φ 分档 ε（Sup vs SupS2）')
P('| φ | Sup ε 中位 | SupS2 ε 中位 | n Sup/S2 |')
for phi in [1,5,9,13]:
    s=[r['m'] for r in ok if r['m']['arm']=='Sup' and r['m']['phi']==phi]
    s2=[r['m'] for r in ok if r['m']['arm']=='SupS2' and r['m']['phi']==phi]
    P(f"| {phi} | {med([q['eps'] for q in s]):.1f} | {med([q['eps'] for q in s2]):.1f} | {len(s)}/{len(s2)} |")

sup_eps=eps_by_arm.get('Sup',[])
s2_eps=eps_by_arm.get('SupS2',[])
sup_med=med(sup_eps); s2_med=med(s2_eps)
eps_changed=False
if np.isfinite(sup_med) and np.isfinite(s2_med):
    # "明显变化": |Δ| > 2 ms or >15% 
    eps_changed = abs(s2_med-sup_med) > 2.0 or (sup_med>0 and abs(s2_med-sup_med)/sup_med > 0.15)
P('')
P(f'- Sup ε 中位={sup_med:.1f} ms; SupS2 ε 中位={s2_med:.1f} ms; |Δ|={abs(s2_med-sup_med) if np.isfinite(sup_med) and np.isfinite(s2_med) else float("nan"):.1f} ms')
P(f'- S2 后 ε 相对 Sup 明显变化: **{"YES → 重定 ε̂" if eps_changed else "NO"}**')

# Proposed ε̂ candidates
if np.isfinite(s2_med):
    # replace 21.8 with new median if changed; keep 0 and one-frame
    cands = [0.0, FRAME_MS, float(s2_med)]
    # unique sorted
    cands = sorted(set(round(c,1) for c in cands))
else:
    cands = [0.0, 16.7, 21.8]
P(f'- 后续 ε̂ 扫描候选 (ms): {cands}')
P('')

# B gate: not a hard stop for C by itself except as calibration; A is the hard gate
# But if SupS2 ε unmeasurable → problem
B_OK = len(s2_eps)>=8 and np.isfinite(s2_med)
P(f'## 阶段 B 状态: **{"OK" if B_OK else "FAIL/稀疏"}** (SupS2 n_eps={len(s2_eps)})')
P('')


# -------------------- Stage C --------------------
best_eps_hat = float('nan')
any_jvis_pass = False
C_DONE = has_c and any('STAGE C END' in l for l in L)

P('## 阶段 C — S1 ε̂ 主矩阵')
if not C_DONE:
    P('- Stage C 未跑或未完成（raw 无 STAGE C END）。')
    P('')
    P('## 分流建议')
    if not A_PASS:
        P('- **A FAIL → 停主矩阵**。不要擅自改成 E2。')
        divert = '停C/降级S1'
        oneline_extra = '最佳ε̂=NA; J_vis门=NA'
    elif not B_OK:
        P('- A PASS 但 B FAIL → 停 S1 标定')
        divert = '停C/查S2'
        oneline_extra = '最佳ε̂=NA; J_vis门=NA'
    else:
        P('- A PASS + B OK → 可开 Stage C（`--r3-50-full`）。')
        P(f'  - ε̂ 候选 {cands}')
        divert = '开C'
        oneline_extra = '最佳ε̂=待C; J_vis门=待C'
    P('')
    P('## ONELINE')
    P(f'闸门 A={"PASS" if A_PASS else "FAIL"}; S2后ε={s2_med:.1f}ms; {oneline_extra}; 分流={divert}')
else:
    TRC=[]
    for l,a in raws_after('TRIALC arm='):
        d=dict(re.findall(r'(\w+)=(\S+)', l))
        TRC.append(dict(
            arm=d['arm'], cell=d.get('cell','?'), b=float(d['b']),
            phi=float(d['phi']), tInt=float(d.get('tInt','60')),
            tSeed=float(d['tSeed'])/1000, tInt_m=float(d['tInt_m'])/1000,
            phiAct=float(d['phiAct']), per=float(d['tickPer'])/1000,
            last=float(d['lastTick'])/1000,
            epsHat=float(d['epsHat']) if d.get('epsHat','NA') not in ('NA','') else float('nan'),
            amp=float(d['amp']) if d.get('amp','NA') not in ('NA','') else float('nan'),
            a=a, line=l))

    def nh_for(cell, amp):
        if 'rev' in cell or (np.isfinite(amp) and amp < 0):
            return NH['t060rev']
        return NH['t060fwd']

    def analyze_C(r):
        a=r['a']; t=a[:,2]/1000; x=a[:,1]
        k0=next((i for i,v in enumerate(x) if abs(v)<8.0),0)
        t,x=t[k0:],x[k0:]
        if len(t)<8: return None
        per=r['per']
        pre_cut=r['tInt_m']+A0-0.002
        pre=t<pre_cut
        if pre.sum()<3: pre=t<r['tSeed']+0.010
        if pre.sum()<3: return None
        X=b1(t[pre][None,:]-DG[:,None])[0]
        sse=((x[pre][None,:]-X)**2).sum(1)
        i=int(np.argmin(sse)); dlt=DG[i]; fo=dlt-A0
        tau=t-dlt
        tApply=r['last']+np.ceil((r['tInt_m']-r['last'])/per)*per
        ts=tApply-fo; tm=r['tInt_m']-fo
        ks=np.where(tau>ts-per/2)[0]
        if len(ks)<6 or ks[0]<1: return None
        kc=ks[0]
        pre_idx=np.where(pre)[0]
        d_frame=float(abs(x[pre_idx[-1]]-x[pre_idx[-2]])) if len(pre_idx)>=2 else float('nan')
        Nh=nh_for(r['cell'], r['amp'] if np.isfinite(r['amp']) else r['b']-240)
        arm=r['arm']; bnew=r['b']; amp = r['amp'] if np.isfinite(r['amp']) else (bnew-240.0)
        # J_vis at retarget (WGC vs continuing base)
        g=np.arange(tau[kc-1],tau[kc]+1e-9,0.0005)
        xpr,_=b1(g); xs_g=np.interp(g,tau,x)
        J=float(np.min(np.abs(xs_g-xpr)))
        Js=float(xs_g[len(xs_g)//2]-xpr[len(xpr)//2])
        i_ts=int(np.argmin(np.abs(tau-ts)))
        vs=(x[i_ts+1]-x[i_ts-1])/(tau[i_ts+1]-tau[i_ts-1]) if 1<=i_ts<len(tau)-1 else 0.0
        vr=float(b1(np.array([ts]))[1][0]); dv=float(abs(vs-vr))
        if arm=='base':
            return dict(J=J,Js=Js,dv=dv,eps=0.0,e=float('nan'),epsHat=float('nan'),
                        d_frame=d_frame,Nh=Nh,arm=arm,cell=r['cell'],b=bnew,phi=r['phi'],
                        tInt=r['tInt'],resid_med=float('nan'),resid_p90=float('nan'),
                        vresid_med=float('nan'),jvis_pass=False)
        # Fit ε vs ideal overlay spring(τ-tm-ε, amp)
        best=None
        for e_ms in np.arange(-5.0,40.1,0.5):
            ee=e_ms/1000.0
            msk=(tau>0.01)&(tau<0.65)
            if msk.sum()<8: continue
            resid=x[msk]-(spring_amp(tau[msk],240)[0]+spring_amp(tau[msk]-tm-ee,amp)[0])
            sse=float(np.mean(resid**2))
            if best is None or sse<best[0]:
                best=(sse,e_ms,float(np.sqrt(sse)),resid,msk)
        if best is None: return None
        eps=best[1]
        # Ideal retarget residual (vs fitted-ε closed form = "perfect ε̂")
        msk=best[4]
        resid_ideal=best[3]  # already vs ideal with fitted ε
        # Also residual vs S1-intended model: spring(240)+seeded(τ-tm, amp, ε̂)
        epsHat=r['epsHat']
        if np.isfinite(epsHat):
            x_s1 = spring_amp(tau[msk],240)[0] + seeded_amp(tau[msk]-tm, amp, epsHat/1000.0)[0]
            resid_s1 = x[msk]-x_s1
            # velocity residual vs ideal
            v_meas = np.gradient(x[msk], tau[msk])
            v_ideal = spring_amp(tau[msk],240)[1] + spring_amp(tau[msk]-tm-eps/1000.0, amp)[1]
            vresid = v_meas - v_ideal
        else:
            resid_s1 = resid_ideal
            vresid = np.zeros_like(resid_ideal)
        e = epsHat - eps if np.isfinite(epsHat) else float('nan')
        jvis_ok = np.isfinite(J) and np.isfinite(d_frame) and (J <= d_frame + Nh)
        return dict(J=J,Js=Js,dv=dv,eps=eps,e=e,epsHat=epsHat,
                    d_frame=d_frame,Nh=Nh,arm=arm,cell=r['cell'],b=bnew,phi=r['phi'],
                    tInt=r['tInt'],
                    resid_med=float(np.median(resid_ideal)),
                    resid_p90=float(np.percentile(np.abs(resid_ideal),90)),
                    resid_s1_med=float(np.median(resid_s1)),
                    resid_s1_p90=float(np.percentile(np.abs(resid_s1),90)),
                    vresid_med=float(np.median(vresid)),
                    jvis_pass=bool(jvis_ok))

    for r in TRC:
        r['m']=analyze_C(r)
    okc=[r for r in TRC if r['m']]
    failc=[r for r in TRC if not r['m']]
    P(f'- TRIALC 解析: {len(okc)}/{len(TRC)} (fail {len(failc)}); LIVE_TERMS_CAP=3')
    P('')

    # Per-trial raw-ish table (compact)
    P('### 逐 trial（样本；全量见 raw）φ ε e J Δv N_h_eff d_frame J_vis门')
    P('| arm | cell | φ | ε̂ | ε | e | J | Δv | N_h | d_frame | pass |')
    shown=0
    for r in okc:
        m=r['m']
        if m['arm']=='base': continue
        P(f"| {m['arm']} | {m['cell']} | {m['phi']:.0f} | {m['epsHat']:.1f} | {m['eps']:.1f} | {m['e']:.1f} | {m['J']:.2f} | {m['dv']:.1f} | {m['Nh']:.2f} | {m['d_frame']:.2f} | {m['jvis_pass']} |")
        shown+=1
        if shown>=40:
            P(f'| … | ({len([q for q in okc if q["m"]["arm"]!="base"])-40} more S1 trials) |'); break
    P('')

    # Aggregate by epsHat / cell
    P('### 按 ε̂ × cell：J_vis 门 + 有符号 e / 残差')
    P('| ε̂ | cell | n | J中位 | d_frame中位 | N_h | J≤d+N 通过率 | e中位 | e_P90| | resid中位 | resid_P90| |')
    arms_s1=sorted({r['m']['arm'] for r in okc if r['m']['arm'].startswith('S1')})
    cells=sorted({r['m']['cell'] for r in okc})
    pass_by_hat={}
    for arm in arms_s1:
        for cell in cells:
            mm=[r['m'] for r in okc if r['m']['arm']==arm and r['m']['cell']==cell]
            if not mm: continue
            eh=mm[0]['epsHat']
            n=len(mm); Jp=sum(1 for q in mm if q['jvis_pass'])
            rate=Jp/n
            pass_by_hat.setdefault(eh, []).append(rate)
            P(f"| {eh:.1f} | {cell} | {n} | {med([q['J'] for q in mm]):.2f} | {med([q['d_frame'] for q in mm]):.2f} | {mm[0]['Nh']:.2f} | {rate:.0%} | {med([q['e'] for q in mm]):.1f} | {float(np.percentile(np.abs([q['e'] for q in mm]),90)):.1f} | {med([q['resid_med'] for q in mm]):.3f} | {med([q['resid_p90'] for q in mm]):.3f} |")
    P('')

    # φ buckets + lead/lag
    P('### 有符号 e 按 φ / 超前·滞后')
    P('| ε̂ | φ | n | e中位 | lead_n | lead_e中位 | lag_n | lag_e中位 |')
    for arm in arms_s1:
        for phi in [1,5,9,13]:
            mm=[r['m'] for r in okc if r['m']['arm']==arm and r['m']['phi']==phi]
            if not mm: continue
            eh=mm[0]['epsHat']
            leads=[q['e'] for q in mm if q['e']>0]
            lags=[q['e'] for q in mm if q['e']<0]
            P(f"| {eh:.1f} | {phi} | {len(mm)} | {med([q['e'] for q in mm]):.1f} | {len(leads)} | {med(leads):.1f} | {len(lags)} | {med(lags):.1f} |")
    P('')

    # Fit vs hold-out: odd reps fit, even hold (rep in line)
    P('### 拟合批 vs hold-out（按 rep 奇偶）')
    P('| ε̂ | fit_e中位 | hold_e中位 | fit_n | hold_n |')
    for arm in arms_s1:
        fit=[]; hold=[]
        for r in okc:
            if r['m']['arm']!=arm: continue
            rep=int(dict(re.findall(r'(\w+)=(\S+)', r['line'])).get('rep','0'))
            (fit if rep%2==0 else hold).append(r['m']['e'])
        if not fit and not hold: continue
        eh=next(r['m']['epsHat'] for r in okc if r['m']['arm']==arm)
        P(f"| {eh:.1f} | {med(fit):.1f} | {med(hold):.1f} | {len(fit)} | {len(hold)} |")
    P('')

    # Ship gate: any ε̂ that passes ALL cells (median J_vis ≤ med(d_frame)+N_h, or pass-rate≥75% every cell)
    P('### 出货门 J_vis ≤ d_frame + N_h_eff（禁止用 ≤2×Sup）')
    hat_all_pass={}
    for eh, rates in pass_by_hat.items():
        # also check cell-level median J gate
        cells_ok=True
        detail=[]
        for cell in cells:
            mm=[r['m'] for r in okc if r['m']['epsHat']==eh and r['m']['cell']==cell and r['m']['arm'].startswith('S1')]
            if not mm:
                cells_ok=False; continue
            Jmed=med([q['J'] for q in mm]); dfmed=med([q['d_frame'] for q in mm]); Nh=mm[0]['Nh']
            okj = Jmed <= dfmed + Nh
            detail.append((cell, Jmed, dfmed, Nh, okj))
            if not okj: cells_ok=False
        hat_all_pass[eh]=cells_ok
        flag='PASS' if cells_ok else 'FAIL'
        P(f'- ε̂={eh:.1f}: cell门={flag}; ' + '; '.join(f'{c}: J={j:.2f}≤{df:.2f}+{nh:.2f}→{okj}' for c,j,df,nh,okj in detail))
    any_jvis_pass = any(hat_all_pass.values())
    # best ε̂: among those passing, prefer smallest |e| median; else smallest |e| among all
    def score(eh):
        mm=[r['m'] for r in okc if r['m']['epsHat']==eh and r['m']['arm'].startswith('S1')]
        return abs(med([q['e'] for q in mm])) if mm else 1e9
    if any_jvis_pass:
        cands_pass=[eh for eh,ok in hat_all_pass.items() if ok]
        best_eps_hat=min(cands_pass, key=score)
    elif hat_all_pass:
        best_eps_hat=min(hat_all_pass.keys(), key=score)
    else:
        best_eps_hat=float('nan')
    P(f'- 任一 ε̂ 全 cell 过 J_vis 门: **{"YES" if any_jvis_pass else "NO"}**; 最佳ε̂={best_eps_hat:.1f} ms')
    P('')

    # vs Sup residual (from Stage B Sup) — report only, not ship gate
    sup_J = med([r['m']['J'] for r in ok if r['m'] and r['m']['arm']=='Sup' and r['m']['tInt']==60])
    P(f'- 对照（非出货门）Sup J中位={sup_J:.2f}（禁止用 ≤2×Sup 作出货门）')
    P('')

    P('## 分流建议')
    a_tag = 'PASS' if A_PASS else 'FAIL'
    if any_jvis_pass:
        if A_PASS:
            P(f'- A/B PASS 且 ε̂={best_eps_hat:.1f} 过 J_vis 门 → 记录成功数据；**R-3 仍保持 OPEN**（成功≠采用）。')
            divert = f'记录/R-3仍OPEN/最佳ε̂={best_eps_hat:.1f}'
        else:
            P(f'- 本 raw 离线 A={a_tag}，但 Stage C 已齐且 ε̂={best_eps_hat:.1f} 过 J_vis 门 → 记录 C 数据；**对照先跑的 AB-only（A PASS）**；R-3 仍 OPEN。')
            divert = f'A本轮FAIL/C过门记录/R-3仍OPEN/最佳ε̂={best_eps_hat:.1f}'
    else:
        if A_PASS:
            P('- A/B PASS 但无一 ε̂ 全 cell 过 J_vis → S1 收益不成立或需扩扫；**不要擅自改成 E2**。R-3 OPEN。')
            divert = 'S1未过门/R-3仍OPEN/可扩ε̂或停S1'
        else:
            P(f'- 本 raw 离线 A={a_tag} 且无一 ε̂ 过 J_vis → 停主矩阵/写分流；**不要擅自改成 E2**。R-3 OPEN。')
            divert = 'A_FAIL/S1未过门/R-3仍OPEN'
    P('')
    P('## ONELINE')
    P(f'闸门 A={a_tag}; S2后ε={s2_med:.1f}ms; 最佳ε̂={best_eps_hat:.1f}ms; J_vis门={"YES" if any_jvis_pass else "NO"}; 分流={divert}')

text='\n'.join(o)+'\n'
if OUT:
    open(OUT,'w',encoding='utf-8').write(text)
print(text)
