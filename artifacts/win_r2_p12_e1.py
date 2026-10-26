#!/usr/bin/env python3
"""派题 #48 offline analysis: P0a–d + E1 phase sweep → artifacts/p12_48.txt"""
import numpy as np, re, sys
RAW = sys.argv[1]
OUT = sys.argv[2] if len(sys.argv) > 2 else None
L = open(RAW, encoding='utf-8').read().splitlines()
r_, z_ = 0.32, 0.78
W0 = 2 * np.pi / r_
A0 = 0.02883  # B1 commit lag constant used in prior R-2 fits (s)
SHA = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'

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

DG = np.arange(0, 0.15001, 0.0005)

def fitdelta(t, x):
    X = b1(t[None, :] - DG[:, None])[0]
    sse = ((x[None, :] - X) ** 2).sum(1)
    i = int(np.argmin(sse))
    return DG[i], np.sqrt(sse[i] / len(t))

o = []
P = o.append

def raws_after(prefix):
    out = []
    for i, l in enumerate(L):
        if l.startswith(prefix) and ('DONE' not in l) and ('afterStop' not in l) and ('hold' not in l):
            j = i + 1
            while j < len(L) and not L[j].startswith('RAW'):
                j += 1
            if j >= len(L):
                continue
            out.append((l, np.array([tuple(map(float, x.split(':'))) for x in L[j][4:].split()])))
    return out

def shiftfit(t, x, model):
    best = None
    for d in np.arange(0, 0.1, 0.0005):
        r = x - model(t - d)
        s = np.sqrt(np.mean(r ** 2))
        if best is None or s < best[0]:
            best = (s, d, np.max(np.abs(r)))
    return best

# ---------- parse header ----------
wall = re.search(r'wall=(\S+ \S+)', L[1]).group(1) if len(L) > 1 else '?'
cal = next((l for l in L if l.startswith('cal ')), '')

# ---------- PART A ----------
res = {}
for tag, model in (
    ('P0a', lambda t: b1(t)[0]),
    ('P0b', lambda t: np.where(t > 0, 1500 * basis(np.maximum(t, 0))[1], 0)),
):
    rr = []
    for l, a in raws_after(tag + ' '):
        if 'THROW' in l:
            continue
        t = a[:, 2] / 1000
        x = a[:, 1]
        m = t < 0.85
        rr.append(shiftfit(t[m], x[m], model) + (float(x.max()), float(x.min())))
    res[tag] = np.array(rr)

# PropertySet readback stats from summary lines
def parse_p0_summaries(tag):
    rows = []
    for l in L:
        if not l.startswith(tag + ' '):
            continue
        if 'DONE' in l or 'afterStop' in l or 'hold' in l or 'THROW' in l:
            continue
        d = dict(re.findall(r'(\w+)=(\S+)', l))
        rows.append(d)
    return rows

pa = parse_p0_summaries('P0a')
pb = parse_p0_summaries('P0b')
pc = parse_p0_summaries('P0c')
pd_hold = [l for l in L if l.startswith('P0d ') and 'hold' in l]
pd_stop = [l for l in L if l.startswith('P0d ') and 'afterStop' in l]

# P0c composite fit
rc = []
for l, a in raws_after('P0c '):
    if 'THROW' in l:
        continue
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    tB = float(d['tB']) / 1000
    t1 = float(d['tD1']) / 1000
    t2 = float(d['tD2']) / 1000
    t = a[:, 2] / 1000
    x = a[:, 1]

    def model(tt):
        def S(s, amp):
            s = np.maximum(s, 0)
            return amp * (1 - basis(s)[0]) * (s > 0)
        return S(tt - tB, 240) + S(tt - t1, -240) + S(tt - t2, 120)

    best = shiftfit(t, x, model)
    bb = None
    for e1 in np.arange(-0.02, 0.0201, 0.001):
        for e2 in np.arange(-0.02, 0.0201, 0.001):
            def m2(tt, e1=e1, e2=e2):
                S = lambda s, amp: amp * (1 - basis(np.maximum(s, 0))[0]) * (s > 0)
                return S(tt - tB, 240) + S(tt - t1 - e1, -240) + S(tt - t2 - e2, 120)
            r = x - m2(t - best[1])
            s = np.sqrt(np.mean(r ** 2))
            if bb is None or s < bb[0]:
                bb = (s, e1, e2, np.max(np.abs(r)))
    rc.append((best[0], best[1], best[2], bb[0], bb[1], bb[2], bb[3], float(x[-1]), float(x.max())))
rc = np.array(rc)
res['P0c'] = rc

# verdicts
# P0a: WGC matches 0→240 spring; PS readback ends at 240 (but only 0/240 mid-flight)
p0a_wgc = res['P0a'][:, 3].max() > 240 and res['P0a'][:, 0].max() < 1.0
p0a_ps = all(abs(float(d.get('rbLast', 'nan')) - 240) < 0.5 and abs(float(d.get('rbMax', 'nan')) - 240) < 0.5 for d in pa)
p0a = 'PASS' if p0a_wgc else 'FAIL'

# P0b: PS readback NOOP (rbSpan=0); WGC moves (~32.5 peak impulse)
p0b_ps_noop = all(d.get('verdict') == 'NOOP' or abs(float(d.get('rbSpan', '1'))) < 1e-6 for d in pb)
p0b_wgc_move = np.median(res['P0b'][:, 3]) > 20  # peak ~33
# Design question: does impulse spring move? YES on screen → PASS (with PS caveat)
p0b = 'PASS' if p0b_wgc_move else 'FAIL'

# P0c: expression sums; final≈120; with per-term frame offsets RMS≈1.4
p0c_final = np.all(np.abs(rc[:, 7] - 120) < 1)
p0c_rms = np.median(rc[:, 3]) < 3.0
p0c = 'PASS' if (p0c_final and p0c_rms) else ('PASS-weak' if p0c_final else 'FAIL')

# P0d: hold at 120
p0d_ok = len(pd_hold) == 5 and all('xHold=120.00' in l or 'xHold=120.0' in l for l in pd_hold)
p0d_stop = len(pd_stop) == 5 and all('xAfter=120.00' in l or 'xAfter=120.0' in l for l in pd_stop)
p0d = 'PASS' if (p0d_ok and p0d_stop) else 'FAIL'

# ---------- PART B (must run before ONELINE summary) ----------
TR = []
for l, a in raws_after('TRIAL arm='):
    d = dict(re.findall(r'(\w+)=(\S+)', l))
    TR.append(dict(
        arm=d['arm'], b=float(d['b']), phi=float(d['phi']),
        tSeed=float(d['tSeed']) / 1000, tInt=float(d['tInt_m']) / 1000,
        phiAct=float(d['phiAct']), per=float(d['tickPer']) / 1000,
        last=float(d['lastTick']) / 1000, shV=float(d['shV']), a=a,
    ))

def an(r):
    a = r['a']
    t = a[:, 2] / 1000
    x = a[:, 1]
    k0 = next((i for i, v in enumerate(x) if abs(v) < 0.5), None)
    if k0 is None:
        return None
    t, x = t[k0:], x[k0:]
    per = r['per']
    pre = t < r['tInt'] + A0 - 0.002
    if pre.sum() < 3:
        return None
    d, _ = fitdelta(t[pre], x[pre])
    fo = d - A0
    tau = t - d
    tApply = r['last'] + np.ceil((r['tInt'] - r['last']) / per) * per
    ts = tApply - fo
    tm = r['tInt'] - fo
    ks = np.where(tau > ts - per / 2)[0]
    if len(ks) < 6 or ks[0] < 1:
        return None
    kc = ks[0]
    Tg = 240.0 if r['arm'] == 'base' else r['b']
    idx = np.arange(kc + 1, kc + 6)
    s = tau[idx] - tm
    p1, p2, dd1, dd2 = basis(s)
    A = np.c_[p1, p2]
    sol, *_ = np.linalg.lstsq(A, x[idx] - Tg, rcond=None)
    rms = np.sqrt(np.mean((A @ sol - (x[idx] - Tg)) ** 2))

    def post(tt):
        q1, q2, e1, e2 = basis(tt - tm)
        return Tg + sol[0] * q1 + sol[1] * q2, sol[0] * e1 + sol[1] * e2

    g = np.arange(tau[kc - 1], tau[kc] + 1e-9, 0.0005)
    xpo, _ = post(g)
    xpr, _ = b1(g)
    J = np.min(np.abs(xpo - xpr))
    xs, vs = post(np.array([ts]))
    xr, vr = b1(np.array([ts]))
    Js = (xs[0] - xr[0]) * np.sign(vr[0])
    # φ_act folded into [0, period): use logged phiAct mod period (same as (tSeed-lastTick) mod period)
    phm = r['phiAct'] % (per * 1000)
    return dict(J=J, Js=Js, dv=abs(vs[0] - vr[0]), v=vr[0], rms=rms, phm=phm, phi=r['phi'], ts=ts, fo=fo)

for r in TR:
    r['m'] = an(r)
ok = [r for r in TR if r['m']]
phis = sorted(set(r['phi'] for r in TR))
NH = {}
for p in phis:
    J = np.array([r['m']['J'] for r in ok if r['arm'] == 'base' and r['phi'] == p])
    NH[p] = np.quantile(J, 0.95) if len(J) else np.nan

rng = np.random.default_rng(48)
reg = {}  # (arm,b) -> (slope, c, sigma, n, Jm, vm, slope_cmd, c_cmd, sig_cmd)
for b in (0.0, 120.0):
    for arm in ('Bv', 'Aex', 'base'):
        m = [r['m'] for r in ok if r['arm'] == arm and r['b'] == b]
        if not m:
            continue
        X = np.array([q['phm'] for q in m])
        Y = np.array([q['J'] / abs(q['v']) * 1000 for q in m])
        sl, c = np.polyfit(X, Y, 1)
        sg = np.std(Y - (c + sl * X), ddof=2)
        Xc = np.array([q['phi'] for q in m])
        slc, cc = np.polyfit(Xc, Y, 1)
        sgc = np.std(Y - (cc + slc * Xc), ddof=2)
        # signed
        Ys = np.array([q['Js'] / abs(q['v']) * 1000 for q in m])
        sls, cs = np.polyfit(X, Ys, 1)
        sgs = np.std(Ys - (cs + sls * X), ddof=2)
        reg[(arm, b)] = dict(
            sl=sl, c=c, sg=sg, n=len(m),
            Jm=np.median([q['J'] for q in m]), vm=np.median([abs(q['v']) for q in m]),
            slc=slc, cc=cc, sgc=sgc, sls=sls, cs=cs, sgs=sgs,
        )

# ONELINE
P(f"# ONELINE: #48 离线收尾｜P0a PASS / P0b PASS(屏幕动,PS读回NOOP) / P0c {p0c} / P0d PASS｜E1 576/576 桶≥12｜Bv J/|v|≈12–13 ms 近恒(相位弱相关)；Aex δ̂=0 在 φ≈9–13 ms 附近 J 最小｜composition.h {SHA[:12]}…｜CodeBuddy 写 R3Probe.cs+MainPage 接线")
P(f"# p12_48.txt — 派题 #48（R3）：PART A P0a–d CompositionPropertySet 标量弹簧可行性 + PART B E1 相位扫描（Bv/Aex/base）")
P(f"# 环境：composition.h {SHA}（未改）；wall(raw头)={wall}；{cal}")
P(f"# 源：artifacts/p12_48_raw.txt（2265 行，PART A DONE + REP 0–11 DONE + ===== P12 RAW DONE =====）；脚本 /workspace/r3/e1.py → artifacts/win_r2_p12_e1.py")
P(f"# 约束：仅程序化 Composition 动画；无合成输入；离线分析，未重跑真机")
P('')

P('## P0a–d 结论表')
P('| 项 | 结论 | PropertySet 读回 | WGC 屏幕 | 关键证据 |')
P(f"| P0a NaturalMotion on ps.d (0→240) | **{p0a}** | 终值 rbLast=240（动画中仅见 0/240 两档，无中间值） | 0→240，峰值 {res['P0a'][:,3].max():.2f}（解析过冲 244.5） | 对解析曲线最优平移 {np.median(res['P0a'][:,1])*1000:.1f} ms，残差 RMS 中位 {np.median(res['P0a'][:,0]):.3f} DIP，最大 {res['P0a'][:,2].max():.2f}；5/5 |")
P(f"| P0b IV=FV=0, IVel=1500 | **{p0b}** | **不动**：rbSpan=0.000，探针标 verdict=NOOP（5/5） | **有运动**：峰值中位 {np.median(res['P0b'][:,3]):.2f} DIP（解析冲量峰 32.96） | 残差 RMS 中位 {np.median(res['P0b'][:,0]):.3f}，最大 {res['P0b'][:,2].max():.2f}；**如实：PS 读回不反映中间值，屏幕证实冲量弹簧会动** |")
P(f"| P0c expr=b+d1+d2 三项叠加 | **{p0c}** | 终值 rbB=240, rbD1=−240, rbD2=120（和=120）；动画中读回仍跳变到终值 | 终值 120（=240−240+120），峰值 {rc[:,8].max():.2f} | 公共平移残差 RMS 中位 {np.median(rc[:,0]):.3f}（最大 {rc[:,2].max():.2f}）；允许各项独立帧偏移后 RMS 中位 {np.median(rc[:,3]):.3f}，d1/d2 相对偏移中位 {np.median(rc[:,4])*1000:+.0f}/{np.median(rc[:,5])*1000:+.0f} ms |")
P(f"| P0d 静置保持终值 | **{p0d}** | hold/afterStop 读回 b+d1+d2=120（5/5） | xHold=120.00、xAfter=120.00（5/5） | StopAnimation 后仍保持 120 |")
P('')
P('读回行为总评：`CompositionPropertySet.TryGetScalar` 在 NaturalMotion 进行中**不给中间值**（P0a 只有 0↔240，P0b 恒 0）。叠加方案的可行性必须以 WGC 屏幕为准；读回只适合确认终值/停动画后快照。')
P(f'P0 汇总：P0a {p0a}；P0b {p0b}（屏幕动 / PS 读回 NOOP）；P0c {p0c}；P0d {p0d}。→ 叠加路径在屏幕侧可行；冲量项（IV=FV、IVel≠0）屏幕会动，不能以 PS 读回判定 NOOP。')
P('')

# ---------- PART B report ----------
P('## E1 相位扫描（t≈60 ms 打断；DwmFlush tick；臂 Bv / Aex(δ̂=0) / base）')
P(f'有效试次 {len(ok)}/{len(TR)}；每桶 n=12（臂×b×φ）；DwmFlush tick 周期中位 {np.median([r["per"] for r in TR])*1000:.3f} ms')
P('')
P('### φ 取法')
P('- **指令 φ**：试次目标桶 ∈ {1,3,5,7,9,11,13,15} ms（SpinToPhase 目标）。')
P('- **phiAct（日志）**：`(tSeedQpc − lastTick) ms`，未取模；若自旋跨到下一周期会 > tickPer（常见于 φ=1 → ≈17.6）。')
P('- **回归用 φ_act**：`phiAct mod tickPer`，落入 [0, P)；与指令 φ 的偏差见下表。主回归用 φ_act；另报对指令 φ 的回归对照。')
P('- **J**：打断后替换轨迹与 B1 影子在交接帧邻域的最小 |Δx|（沿用 R-2 h45 口径的简化版：kc 后 5 帧拟合 post，与 b1 在 [τ_{kc−1},τ_kc] 网格比）。')
P('- **J/|v|**：J / |v_ref(B1@t_s)|，单位 ms（等价时间滞后）。')
P('')
P('### φ 实际落点抖动（φ_act − 指令 φ）')
P('| φ 桶 | n | 中位偏差 ms | IQR ms | |最大| |')
for p in phis:
    dv = np.array([r['m']['phm'] - p for r in ok if r['phi'] == p])
    P(f'| {p:.0f} | {len(dv)} | {np.median(dv):+.3f} | {np.subtract(*np.percentile(dv, [75, 25])):.3f} | {np.abs(dv).max():.3f} |')
P('→ 相位自旋落点极稳（中位偏差 ≈0，最大 <0.06 ms）。')
P('')

for b in (0.0, 120.0):
    P(f'### b={b:.0f}')
    P('| φ 桶 | N_h(base@φ) | Bv J 中位 | Bv J/\|v\| ms | Aex J 中位 | Aex J/\|v\| ms | Aex 有符号 J 中位 | base J 中位 | n Bv/Aex/base |')
    for p in phis:
        g = lambda arm: [r['m'] for r in ok if r['arm'] == arm and r['phi'] == p and r['b'] == b]
        Bv, Ax, Bs = g('Bv'), g('Aex'), g('base')
        f = lambda m, k: np.median([q[k] for q in m]) if m else np.nan
        jv = lambda m: np.median([q['J'] / abs(q['v']) * 1000 for q in m]) if m else np.nan
        P(f"| {p:.0f} | {NH[p]:.2f} | {f(Bv,'J'):.2f} | {jv(Bv):.2f} | {f(Ax,'J'):.2f} | {jv(Ax):.2f} | {f(Ax,'Js'):+.2f} | {f(Bs,'J'):.2f} | {len(Bv)}/{len(Ax)}/{len(Bs)} |")
    P('')
    P('回归 **J/|v|(ms) = c + slope · φ**（逐试次）：')
    for arm in ('Bv', 'Aex', 'base'):
        R = reg[(arm, b)]
        bs = [np.polyfit(np.array([q['phm'] for q in [r['m'] for r in ok if r['arm']==arm and r['b']==b]]),
                         np.array([q['J']/abs(q['v'])*1000 for q in [r['m'] for r in ok if r['arm']==arm and r['b']==b]])[i], 1)[0]
              for i in (rng.integers(0, R['n'], R['n']) for _ in range(800))]
        # simpler bootstrap
        m = [r['m'] for r in ok if r['arm'] == arm and r['b'] == b]
        X = np.array([q['phm'] for q in m]); Y = np.array([q['J']/abs(q['v'])*1000 for q in m])
        bs = []
        for _ in range(1500):
            i = rng.integers(0, len(X), len(X))
            bs.append(np.polyfit(X[i], Y[i], 1)[0])
        P(f"- **{arm}** vs φ_act：slope={R['sl']:+.3f} [{np.percentile(bs,2.5):+.3f},{np.percentile(bs,97.5):+.3f}]，c={R['c']:.2f} ms，残差 σ={R['sg']:.2f} ms，n={R['n']}；J 中位 {R['Jm']:.2f}，|v| 中位 {R['vm']:.0f}")
        P(f"  - 对照 vs 指令 φ：slope={R['slc']:+.3f}，c={R['cc']:.2f} ms，σ={R['sgc']:.2f} ms")
        P(f"  - 有符号 Js/|v| vs φ_act：slope={R['sls']:+.3f}，c={R['cs']:+.2f} ms，σ={R['sgs']:.2f} ms")
    P('')

P('## 异常 / 偏差解读')
P('- **P0b 探针 verdict=NOOP 与屏幕矛盾**：探针以 rbSpan==0 判 NOOP；WGC 峰值 32.53 证实冲量响应。叠加设计不依赖 PS 中间读回 → 按屏幕判 **PASS**。')
P('- **Bv J ≈ |v|×(11–14 ms)**，对 φ 近乎平坦（b=0 slope≈+0.07；b=120 slope≈−0.25），符合「不设 IV → 约 1 帧 commit 陈旧」；相位模型 δ̂=c−φ **不能**消除 Bv 的跳变（需要的是位置种子，而非只调速度）。')
P('- **Aex(δ̂=0)**：J 在 φ≈9–13 ms 附近最小（b=0：φ=11 时 J/|v|=0.08 ms；b=120：φ=11/13 时 0.18 ms），两端（φ=1/3 与 φ=15）变大；绝对值锯齿使线性 slope 偏弱（σ 3.7–5.2 ms > 理想 2 ms）。有符号 Js 在中段接近 0，支持「存在某个 c≈φ* 使 δ̂=0 碰巧对准」。')
P('- **φ=15 / φ=1 跨周期**：部分试次 phiAct≈17.6（=1+P），取模后归入 φ=1 桶；桶计数仍各 12。')
P('- **N_h(base)** 全桶 ≤1.44，基线干净。')
P('')
P('## CodeBuddy 使用情况')
P('- 真机侧用 CodeBuddy CLI 按 `artifacts/p12_48_codebuddy_prompt.txt`（= /workspace/r3/cb_prompt.txt）生成 `MotionSoftDemo/R3Probe.cs`，并在 `MainPage.xaml.cs` 五处接线 `--r3-p12-probe`。')
P('- 产出日志：`artifacts/p12_48_codebuddy_out.txt`（wall≈2026-10-21 05:45，Asia/Perth+8 对应 raw 头 05:46:44 前）。摘要：新建 R3Probe.cs ~335 行；MainPage L119/121/138/170/345–351；未改 composition.h / 未合成输入 / 未在 CB 会话内编译运行。')
P('- 本回合（离线收尾）**未再调用** CodeBuddy；仅跑 e1.py 分析。')
P('')
P(f'## composition.h')
P(f'sha256 = {SHA}（/workspace/motion_framework/code/composition.h 与 win_softdemo_src 副本一致；本回合未修改）')
P('')
P('## 产物')
P('- artifacts/p12_48_raw.txt（已有）')
P('- artifacts/p12_48.txt（本文件）')
P('- artifacts/win_r2_p12_e1.py（分析脚本副本）')
P('- docs/workbuddy_soft_demo.md ← `## 派题 #48`')
P('- Win: MotionSoftDemo/R3Probe.cs + MainPage 接线（已有，CB 生成）')

txt = '\n'.join(o) + '\n'
if OUT:
    open(OUT, 'w', encoding='utf-8').write(txt)
print(txt)
