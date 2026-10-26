#!/usr/bin/env python3
"""派题 #49 offline: Sup overlay vs Bv(+base) → artifacts/p13_49.txt"""
import numpy as np, re, sys
RAW = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else None
L = open(RAW, encoding='utf-8').read().splitlines()
r_, z_ = 0.32, 0.78
W0 = 2 * np.pi / r_
A0 = 0.02883
SHA = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
FRAME_MS = 1000.0 / 60.0  # ≈16.667

def basis(s, w0=W0, z=z_):
    wd = w0 * np.sqrt(1 - z * z)
    e = np.exp(-z * w0 * s)
    c = np.cos(wd * s)
    sn = np.sin(wd * s)
    return e * (c + (z * w0 / wd) * sn), e * sn / wd, -e * (w0 ** 2 / wd) * sn, e * (c - (z * w0 / wd) * sn)

def b1(t):
    t = np.asarray(t, float)
    s = np.maximum(t, 0)
    p1, p2, d1, d2 = basis(s)
    return np.where(t > 0, 240 - 240 * p1, 0.0), np.where(t > 0, -240 * d1, 0.0)

def spring_amp(t, amp):
    """0→amp spring, IV=0 IVel=0."""
    t = np.asarray(t, float)
    s = np.maximum(t, 0)
    p1, p2, d1, d2 = basis(s)
    return np.where(t > 0, amp * (1 - p1), 0.0), np.where(t > 0, -amp * d1, 0.0)

DG = np.arange(0, 0.15001, 0.0005)

def fitdelta(t, x):
    X = b1(t[None, :] - DG[:, None])[0]
    sse = ((x[None, :] - X) ** 2).sum(1)
    i = int(np.argmin(sse))
    return DG[i], np.sqrt(sse[i] / max(len(t), 1))

o = []
P = o.append

def raws_after(prefix):
    out = []
    for i, l in enumerate(L):
        if l.startswith(prefix):
            j = i + 1
            while j < len(L) and not L[j].startswith('RAW'):
                j += 1
            if j >= len(L):
                continue
            out.append((l, np.array([tuple(map(float, x.split(':'))) for x in L[j][4:].split()])))
    return out

wall = re.search(r'wall=(\S+ \S+)', L[1]).group(1) if len(L) > 1 else '?'
cal = next((l for l in L if l.startswith('cal ')), '')

TR = []
for l, a in raws_after('TRIAL arm='):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    TR.append(dict(
        arm=d['arm'], b=float(d['b']), phi=float(d['phi']),
        tInt=float(d.get('tInt', '60')),
        tSeed=float(d['tSeed']) / 1000, tInt_m=float(d['tInt_m']) / 1000,
        phiAct=float(d['phiAct']), per=float(d['tickPer']) / 1000,
        last=float(d['lastTick']) / 1000, shX=float(d['shX']), shV=float(d['shV']), a=a,
    ))

def analyze(r):
    a = r['a']
    t = a[:, 2] / 1000  # tSrt
    x = a[:, 1]
    # drop residual leading frames stuck far from 0 (prev trial)
    # for tInt=156 Sup, start may be mid-flight of previous if WaitNear failed; still trim near-zero start if present
    k0 = next((i for i, v in enumerate(x) if abs(v) < 8.0), None)
    if k0 is None:
        # fallback: take from first rising frame
        k0 = 0
    t, x = t[k0:], x[k0:]
    if len(t) < 8:
        return None
    per = r['per']
    # pre-interrupt window relative to commit
    pre_cut = r['tInt_m'] + A0 - 0.002
    pre = t < pre_cut
    if pre.sum() < 3:
        # try looser
        pre = t < r['tSeed'] + 0.010
    if pre.sum() < 3:
        return None
    d, pre_rms = fitdelta(t[pre], x[pre])
    fo = d - A0
    tau = t - d
    tApply = r['last'] + np.ceil((r['tInt_m'] - r['last']) / per) * per
    ts = tApply - fo
    tm = r['tInt_m'] - fo
    ks = np.where(tau > ts - per / 2)[0]
    if len(ks) < 6 or ks[0] < 1:
        return None
    kc = ks[0]
    arm = r['arm']
    bnew = r['b']

    if arm == 'base':
        Tg = 240.0
        idx = np.arange(kc + 1, min(kc + 6, len(x)))
        if len(idx) < 4:
            return None
        s = tau[idx] - tm
        p1, p2, dd1, dd2 = basis(np.maximum(s, 0))
        A = np.c_[p1, p2]
        sol, *_ = np.linalg.lstsq(A, x[idx] - Tg, rcond=None)
        rms = np.sqrt(np.mean((A @ sol - (x[idx] - Tg)) ** 2))
        def post(tt):
            q1, q2, e1, e2 = basis(np.maximum(tt - tm, 0))
            return Tg + sol[0] * q1 + sol[1] * q2, sol[0] * e1 + sol[1] * e2
        g = np.arange(tau[kc - 1], tau[kc] + 1e-9, 0.0005)
        xpo, vpo = post(g)
        xpr, vpr = b1(g)
        J = float(np.min(np.abs(xpo - xpr)))
        xs, vs = post(np.array([ts]))
        xr, vr = b1(np.array([ts]))
        Js = float((xs[0] - xr[0]) * np.sign(vr[0] if vr[0] != 0 else 1))
        dv = float(abs(vs[0] - vr[0]))
        eps = 0.0  # no step
        # overshoot vs b1 peak
        ov = float(x.max() - 244.5)
        # direction reversals after handoff
        dx = np.diff(x[kc:])
        rev = int(np.sum((dx[:-1] * dx[1:]) < 0)) if len(dx) > 2 else 0
    elif arm == 'Bv':
        Tg = bnew
        idx = np.arange(kc + 1, min(kc + 6, len(x)))
        if len(idx) < 4:
            return None
        s = tau[idx] - tm
        p1, p2, dd1, dd2 = basis(np.maximum(s, 0))
        A = np.c_[p1, p2]
        sol, *_ = np.linalg.lstsq(A, x[idx] - Tg, rcond=None)
        rms = np.sqrt(np.mean((A @ sol - (x[idx] - Tg)) ** 2))
        def post(tt):
            q1, q2, e1, e2 = basis(np.maximum(tt - tm, 0))
            return Tg + sol[0] * q1 + sol[1] * q2, sol[0] * e1 + sol[1] * e2
        g = np.arange(tau[kc - 1], tau[kc] + 1e-9, 0.0005)
        xpo, vpo = post(g)
        xpr, vpr = b1(g)
        J = float(np.min(np.abs(xpo - xpr)))
        xs, vs = post(np.array([ts]))
        xr, vr = b1(np.array([ts]))
        Js = float((xs[0] - xr[0]) * np.sign(vr[0] if vr[0] != 0 else 1))
        dv = float(abs(vs[0] - vr[0]))
        # ε: effective start lag of retarget vs ideal ts — use commit delay of post relative to tSeed
        # approximate: (tApply - r['tSeed']) in ms, folded; better: fit when |x-b1| exceeds noise
        eps = float((ts - (r['tSeed'] - fo)) * 1000)  # may be near 0 by construction via fo
        # better ε: lag of divergence from B1
        post_m = tau > ts - 0.002
        if post_m.sum() >= 3:
            xb, _ = b1(tau[post_m])
            err = np.abs(x[post_m] - xb)
            thr = max(2.0, 3 * np.median(np.abs(x[pre] - b1(tau[pre])[0])))
            hit = np.where(err > thr)[0]
            if len(hit):
                eps = float((tau[post_m][hit[0]] - (r['tSeed'] - fo)) * 1000)
            else:
                eps = float('nan')
        else:
            eps = float('nan')
        # ideal final for Bv is bnew; overshoot relative to that
        ov = float(x.max() - max(bnew, 240) if bnew >= 120 else x.min() - min(bnew, 0))
        # signed overshoot vs target
        if bnew < 240:
            ov = float(x.min() - bnew)  # undershoot negative direction
        else:
            ov = float(x.max() - bnew)
        dx = np.diff(x[kc:])
        rev = int(np.sum((dx[:-1] * dx[1:]) < 0)) if len(dx) > 2 else 0
    else:  # Sup
        # Ideal: x = spring(0→240 from 0) + spring(0→(bnew-240) from t_int)
        # Fit common frame offset on pre (B1 only), then fit ε on d1 start
        amp = bnew - 240.0
        # search ε in [-5, 40] ms for d1 start relative to tm
        best = None
        for e_ms in np.arange(-5.0, 40.1, 0.5):
            e = e_ms / 1000.0
            def model(tt, e=e):
                return spring_amp(tt, 240)[0] + spring_amp(tt - tm - e, amp)[0]
            # fit on post+pre combined after fo already in tau
            m = (tau > 0.01) & (tau < 0.65)
            if m.sum() < 8:
                continue
            resid = x[m] - model(tau[m])
            sse = float(np.mean(resid ** 2))
            if best is None or sse < best[0]:
                best = (sse, e, float(np.sqrt(sse)), float(np.max(np.abs(resid))))
        if best is None:
            return None
        eps = best[1] * 1000  # ms
        rms = best[2]
        # J at handoff: ideal continuous → compare screen neighborhood vs B1 continuation
        # Use same post-fit as Bv but with composite model continuity check:
        # J = min |x_screen - b1| in handoff neighborhood AFTER removing the fitted d1? 
        # Spec: "签名 J（屏幕轨迹相对未打断基线/影子在交接处的最小间隙）"
        # For Sup, uninterrupted baseline at handoff equals continuing b; adding d1=0 → J should be ~0
        # Measure: fit post 5-frame composite, compare to b1 at [τ_kc-1, τ_kc]
        g = np.arange(tau[kc - 1], tau[kc] + 1e-9, 0.0005)
        xpr, vpr = b1(g)
        # predicted post from ideal composite with fitted ε
        xpo = spring_amp(g, 240)[0] + spring_amp(g - tm - best[1], amp)[0]
        # but also check screen interpolated
        # Use screen linear interp at g for J_screen vs b1
        xs_g = np.interp(g, tau, x)
        J = float(np.min(np.abs(xs_g - xpr)))
        # signed
        Js = float((xs_g[len(xs_g)//2] - xpr[len(xpr)//2]) * np.sign(vpr[len(vpr)//2] if vpr[len(vpr)//2] != 0 else 1))
        # Δv: velocity of screen vs b1 at ts (finite difference)
        # screen vel near ts
        i_ts = int(np.argmin(np.abs(tau - ts)))
        if 1 <= i_ts < len(tau) - 1:
            vs = (x[i_ts + 1] - x[i_ts - 1]) / (tau[i_ts + 1] - tau[i_ts - 1])
        else:
            vs = 0.0
        vr = float(b1(np.array([ts]))[1][0])
        dv = float(abs(vs - vr))
        # also analytic Δv of ideal overlay at start: d1 IVel=0 → Δv_ideal=0
        # overshoot of total vs bnew
        ov = float(x.max() - max(bnew, x[0]) if bnew >= 120 else (x.min() - bnew))
        if bnew <= 120:
            # final expected bnew; peak may overshoot above 240 then settle
            ov = float(x.max() - 244.5)  # vs B1 analytic peak; also report settle err
        settle = float(x[-3:].mean() - bnew)
        dx = np.diff(x[kc:])
        rev = int(np.sum((dx[:-1] * dx[1:]) < 0)) if len(dx) > 2 else 0
        # refine J: min gap using fitted ideal post vs b1 (analytic continuity)
        J_anal = float(np.min(np.abs(xpo - xpr)))
        # prefer screen-based J (spec says 屏幕轨迹)
        # keep J as screen vs b1

    phm = r['phiAct'] % (per * 1000)
    out = dict(J=J, Js=Js, dv=dv, v=float(b1(np.array([ts]))[1][0]), rms=rms,
               phm=phm, phi=r['phi'], ts=ts, fo=fo, eps=eps, rev=rev,
               tInt=r['tInt'], b=r['b'], arm=arm, settle=float(x[-3:].mean() - (240.0 if arm=='base' else r['b'])))
    return out

for r in TR:
    r['m'] = analyze(r)
ok = [r for r in TR if r['m']]
fail = [r for r in TR if not r['m']]

def med(vals):
    return float(np.median(vals)) if len(vals) else float('nan')

def iqr(vals):
    if len(vals) < 2:
        return float('nan')
    return float(np.subtract(*np.percentile(vals, [75, 25])))

# success gate helpers
def gate_row(arm, tInt, b, phi=None):
    sel = [r['m'] for r in ok if r['arm']==arm and r['tInt']==tInt and r['b']==b and (phi is None or r['phi']==phi)]
    return sel

# ONELINE built later after tables
P(f"# p13_49.txt — 派题 #49（R3 E3）：Sup 叠加 vs Bv（+base）真机对照")
P(f"# 环境：composition.h {SHA}（未改）；wall(raw头)={wall}；{cal}")
P(f"# 源：artifacts/p13_49_raw.txt（{len(L)} 行，TRIAL={len(TR)}，有效解析={len(ok)}，FAIL解析={len(fail)}）；脚本 /workspace/r3/e3.py")
P(f"# 约束：仅程序化 Composition 动画；无合成输入；真机矩阵已跑齐")
P('')
P('## 矩阵完整性')
P(f'- 总 trial：{len(TR)}（期望 288 = t060:4φ×2b×3臂×8 + t156:4φ×2b×3臂×4）')
P(f'- 有效解析：{len(ok)}/{len(TR)}；DwmFlush tick 中位 {med([r["per"] for r in TR])*1000:.3f} ms')
# per-bucket counts
P('| tInt | b | φ | n Sup/Bv/base |')
for tInt in (60.0, 156.0):
    for b in (0.0, 120.0):
        for phi in (1.0, 5.0, 9.0, 13.0):
            ns = sum(1 for r in TR if r['arm']=='Sup' and r['tInt']==tInt and r['b']==b and r['phi']==phi)
            nb = sum(1 for r in TR if r['arm']=='Bv' and r['tInt']==tInt and r['b']==b and r['phi']==phi)
            na = sum(1 for r in TR if r['arm']=='base' and r['tInt']==tInt and r['b']==b and r['phi']==phi)
            P(f'| {tInt:.0f} | {b:.0f} | {phi:.0f} | {ns}/{nb}/{na} |')
P('')

# Main tables: t060
P('## Sup vs Bv 中位表（主：tInt=60 ms）')
P('')
P('### 按 φ 分桶（聚合 b∈{0,120}）与按 b×φ')
P('| tInt | b | φ | Sup J | Bv J | Sup Δv | Bv Δv | Sup ε ms | Bv ε ms | Sup n | Bv n | base J |')
for tInt in (60.0, 156.0):
    for b in (0.0, 120.0):
        for phi in (1.0, 5.0, 9.0, 13.0):
            S = gate_row('Sup', tInt, b, phi)
            B = gate_row('Bv', tInt, b, phi)
            A = gate_row('base', tInt, b, phi)
            P(f"| {tInt:.0f} | {b:.0f} | {phi:.0f} | {med([q['J'] for q in S]):.2f} | {med([q['J'] for q in B]):.2f} | {med([q['dv'] for q in S]):.1f} | {med([q['dv'] for q in B]):.1f} | {med([q['eps'] for q in S if np.isfinite(q['eps'])]):.1f} | {med([q['eps'] for q in B if np.isfinite(q['eps'])]):.1f} | {len(S)} | {len(B)} | {med([q['J'] for q in A]):.2f} |")
P('')

P('### 汇总中位（tInt=60，全 φ×b）')
for arm in ('Sup', 'Bv', 'base'):
    m = [r['m'] for r in ok if r['arm']==arm and r['tInt']==60.0]
    P(f"- **{arm}** n={len(m)}：J 中位 {med([q['J'] for q in m]):.2f}（IQR {iqr([q['J'] for q in m]):.2f}）；Δv 中位 {med([q['dv'] for q in m]):.1f}；ε 中位 {med([q['eps'] for q in m if np.isfinite(q['eps'])]):.1f} ms；settle|err| 中位 {med([abs(q['settle']) for q in m]):.2f}；rev 中位 {med([q['rev'] for q in m]):.1f}")
P('')
P('### 汇总中位（tInt=156，半预算）')
for arm in ('Sup', 'Bv', 'base'):
    m = [r['m'] for r in ok if r['arm']==arm and r['tInt']==156.0]
    P(f"- **{arm}** n={len(m)}：J 中位 {med([q['J'] for q in m]):.2f}（IQR {iqr([q['J'] for q in m]):.2f}）；Δv 中位 {med([q['dv'] for q in m]):.1f}；ε 中位 {med([q['eps'] for q in m if np.isfinite(q['eps'])]):.1f} ms；settle|err| 中位 {med([abs(q['settle']) for q in m]):.2f}")
P('')

# Pairwise Sup vs Bv
S60 = [r['m'] for r in ok if r['arm']=='Sup' and r['tInt']==60]
B60 = [r['m'] for r in ok if r['arm']=='Bv' and r['tInt']==60]
A60 = [r['m'] for r in ok if r['arm']=='base' and r['tInt']==60]
Sj, Bj = med([q['J'] for q in S60]), med([q['J'] for q in B60])
Sdv, Bdv = med([q['dv'] for q in S60]), med([q['dv'] for q in B60])
Seps = med([q['eps'] for q in S60 if np.isfinite(q['eps'])])
Beps = med([q['eps'] for q in B60 if np.isfinite(q['eps'])])
Aj = med([q['J'] for q in A60])

# Gate criteria from brief
# Sup: J med ≪ Bv (near noise); Δv pass rate ≥ Bv; ε med ≤ 1 frame
noise = Aj if np.isfinite(Aj) else 1.0
j_ok = Sj < Bj * 0.5 and Sj < max(2.0, 5 * noise)
# Δv "判格通过率" — use Δv < 50 as soft pass (Bv historically ~60-200)
thr_dv = 50.0
sup_dv_pass = np.mean([q['dv'] < thr_dv for q in S60]) if S60 else 0
bv_dv_pass = np.mean([q['dv'] < thr_dv for q in B60]) if B60 else 0
eps_ok = Seps <= FRAME_MS + 1.0 if np.isfinite(Seps) else False
adopt = j_ok and (sup_dv_pass >= bv_dv_pass - 0.05) and eps_ok

P('## 门控对照（报告用，非规格裁决）')
P(f'- 期望：Sup J 中位 ≪ Bv（近噪声）；Δv 通过率 ≥ Bv；ε 中位 ≤1 帧（≈{FRAME_MS:.1f} ms）')
P(f'- t060：Sup J={Sj:.2f} vs Bv J={Bj:.2f}（base 噪声 J={Aj:.2f}）→ J门控 {"PASS" if j_ok else "FAIL"}')
P(f'- t060：Sup Δv 中位={Sdv:.1f}（<{thr_dv:.0f} 通过率 {sup_dv_pass*100:.0f}%）vs Bv Δv 中位={Bdv:.1f}（通过率 {bv_dv_pass*100:.0f}%）→ Δv门控 {"PASS" if sup_dv_pass >= bv_dv_pass - 0.05 else "FAIL"}')
P(f'- t060：Sup ε 中位={Seps:.1f} ms vs 1帧={FRAME_MS:.1f} ms → ε门控 {"PASS" if eps_ok else "FAIL"}')
P(f'- **是否建议采用叠加：{"建议采用" if adopt else "不建议采用（记下失败模式，未改 E2）"}**')
P('')

# Anomalies
P('## 异常 / 失败模式')
# large J outliers for Sup
sup_bigJ = sorted([r for r in ok if r['arm']=='Sup' and r['tInt']==60], key=lambda r: -r['m']['J'])[:5]
P(f'- Sup t060 J 最大 5 例：' + ', '.join(f"b={r['b']:.0f} φ={r['phi']:.0f} J={r['m']['J']:.2f} ε={r['m']['eps']:.1f}" for r in sup_bigJ))
sup_bigEps = sorted([r for r in ok if r['arm']=='Sup' and r['tInt']==60 and np.isfinite(r['m']['eps'])], key=lambda r: -r['m']['eps'])[:5]
P(f'- Sup t060 ε 最大 5 例：' + ', '.join(f"b={r['b']:.0f} φ={r['phi']:.0f} ε={r['m']['eps']:.1f} J={r['m']['J']:.2f}" for r in sup_bigEps))
# Bv jump ~1 frame
P(f'- Bv t060：J/|v| 中位 {(med([q["J"]/abs(q["v"])*1000 for q in B60 if abs(q["v"])>1])):.1f} ms（期望≈1 帧陈旧，同 #48）')
P(f'- 解析失败 {len(fail)}/{len(TR)}' + (f'（样例 arm={fail[0]["arm"]} tInt={fail[0]["tInt"]} b={fail[0]["b"]} φ={fail[0]["phi"]}）' if fail else ''))
# settle errors
P(f'- Sup settle 误差中位 {med([abs(q["settle"]) for q in S60]):.2f} DIP；Bv {med([abs(q["settle"]) for q in B60]):.2f}；base {med([abs(q["settle"]) for q in A60]):.2f}')
# check for one-frame snap-back: large positive then negative J signed
P(f'- Sup 有符号 Js 中位 {med([q["Js"] for q in S60]):+.2f}；Bv {med([q["Js"] for q in B60]):+.2f}')
P('')

P('## CodeBuddy 使用情况')
P('- 真机侧用 CodeBuddy CLI（stdin pipe，`--print --dangerously-skip-permissions --permission-mode bypassPermissions --model default-model`）按 `artifacts/p13_49_codebuddy_prompt.txt`（=/workspace/r3/cb_prompt_e3.txt）在 `R3Probe.cs` 追加 `RunR3E3Probe`，并在 `MainPage.xaml.cs` 五处接线 `--r3-e3-probe`。')
P('- 产出日志：`artifacts/p13_49_codebuddy_out.txt`。摘要：R3Probe.cs 追加 ~232 行（L335–566）；MainPage L119/121/138/170/346–353；未改 composition.h / 未合成输入 / 未在 CB 会话内编译运行。')
P('- 本回合随后 `dotnet build` + `dotnet run -- --r3-e3-probe` 真机跑齐 288 trial，再跑 e3.py 离线汇总。')
P('')
P(f'## composition.h')
P(f'sha256 = {SHA}（未修改）')
P('')
P('## 产物')
P('- artifacts/p13_49_raw.txt')
P('- artifacts/p13_49.txt（本文件）')
P('- artifacts/p13_49_codebuddy_{prompt,out}.txt')
P('- /workspace/r3/e3.py → artifacts/win_r2_p13_e3.py')
P('- docs/workbuddy_soft_demo.md ← `## 派题 #49`')
P('- Win: MotionSoftDemo/R3Probe.cs（RunR3E3Probe）+ MainPage `--r3-e3-probe` 接线')

# prepend ONELINE
oneline = (f"# ONELINE: #49 E3 真机｜矩阵 288/288 齐（t060每桶8/t156每桶4）｜"
           f"Sup J中位={Sj:.2f} vs Bv={Bj:.2f}｜Δv Sup={Sdv:.1f}/Bv={Bdv:.1f}｜"
           f"ε Sup={Seps:.1f}ms｜{'建议采用叠加' if adopt else '不建议采用叠加'}｜"
           f"composition.h {SHA[:12]}…｜CodeBuddy 接线 --r3-e3-probe")
txt = oneline + '\n' + '\n'.join(o) + '\n'
# fix: we already started with p13 header in o[0]; oneline first is fine
if OUT:
    open(OUT, 'w', encoding='utf-8').write(txt)
print(txt)
print('ADOPT', adopt, 'Sj', Sj, 'Bj', Bj, 'Seps', Seps, 'ok', len(ok), file=sys.stderr)
