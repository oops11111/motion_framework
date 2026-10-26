import sys,numpy as np,re,warnings; warnings.filterwarnings('ignore'); sys.path.insert(0,'/workspace/r2'); import lib46 as l
A='/workspace/motion_framework/artifacts/'
o=[];P=o.append
AN={'B':'B','A0':'A0','Aphi':'Aφ','Ap8':'A+8'}
def runR(ns,modes=('h45','pre','post'),drop2=False):
    R={}
    for mode in modes:
        for r in ns['tr']: R.setdefault((mode,r['arm'],r['cell']),[]).append(l.e3x(ns,r,mode,drop2))
    return R
def okl(R,mode,a,c,f=None): return [m for m in R[(mode,a,c)] if not m['NA'] and (f is None or f(m))]
def dual_table(R,cells,arms,title):
    P(f'### {title}：k_c 三口径（h45=#45 实际做法：J 窗 [τ_kc−1,τ_kc]、后拟合 kc+1…kc+5；pre=强制归前：k_c 并入前段 δ 拟合、J 窗 [τ_kc,τ_kc+1]、后拟合 kc+1…kc+5；post=强制不归前：后拟合 kc…kc+4、J 窗 [τ_kc−1,τ_kc]）')
    P('注：#45 实现中“k_c≤1.5 归前”只是计数标记，未改变拟合与 J 窗；通过与该计数的重合来自 k_c 帧本身连续（J 小 ⇒ k_c 偏差小），不是归前操作造成的。')
    P('| 臂 | 格 | h45 J[上界]/Δv/结论 | 强制归前 J[上界]/Δv/结论 | 强制不归前 J[上界]/Δv/结论 | 标记 |')
    out={}
    for a in arms:
        for c in cells:
            vs=[]
            for mode in ('h45','pre','post'):
                Nh,Nv=l.nullq(okl(R,mode,'base',c),'dv'); vs.append(l.verdict(okl(R,mode,a,c),Nh,Nv))
            pp=[v['c'].startswith('PASS') or v['c']=='J-PASS' for v in vs[1:]]
            tag='k_c 敏感' if pp[0]!=pp[1] else ('稳健通过' if pp[0] else '稳健不通过')
            out[(a,c)]=(vs,tag)
            P(f"| {AN[a]} | {c} | "+' | '.join(f"{v['Jm']:.2f}[{v['up']:.2f}]/{v['dvm']:.0f}/{v['c']}" for v in vs)+f" | {tag} |")
    return out
# ================= #46 live
ns=l.load(A+'win_r2_p10_raw.txt'); cells46=['t060rev','t060fwd']
R46=runR(ns)
lag=np.array([r['tInt_m']-r['tInt_c'] for r in ns['tr']]); q1,q3=np.percentile(lag,[25,75]); cth=q3+3*(q3-q1)
P(''); P('## #46 附加：⚠C（tInt_m−tInt_c > Q3+3·IQR）')
nC=sum(lag>cth); P(f'P1 {len(lag)} 次：中位 {np.median(lag):.2f} ms，Q3+3·IQR={cth:.2f} ms，⚠C {nC} 次（范围 {lag.min():.2f}–{lag.max():.2f}）')
P('| 臂 | 格 | 剔除前 J[上界]/Δv/结论 | 剔除 ⚠C 后 | 变化 |')
for a in ['B','A0']:
    for c in cells46:
        Nh,Nv=l.nullq(okl(R46,'h45','base',c),'dv'); v0=l.verdict(okl(R46,'h45',a,c),Nh,Nv)
        Nh2,Nv2=l.nullq(okl(R46,'h45','base',c,lambda m:m['lag']<=cth),'dv'); v1=l.verdict(okl(R46,'h45',a,c,lambda m:m['lag']<=cth),Nh2,Nv2)
        P(f"| {AN[a]} | {c} | {v0['Jm']:.2f}[{v0['up']:.2f}]/{v0['dvm']:.0f}/{v0['c']} | {v1['Jm']:.2f}[{v1['up']:.2f}]/{v1['dvm']:.0f}/{v1['c']} (n={v1['n']}) | {'不变' if v0['c']==v1['c'] else '改变'} |")
P('')
d46=dual_table(R46,cells46,['B','A0'],'#46 实测')
# ================= #45 offline
ns5=l.load(A+'win_r2_p9_raw.txt'); cells=['t060rev','t060fwd','t156rev','t156fwd','t300rev','t300fwd','t156dbl']; arms=['B','A0','Aphi','Ap8']
R5=runR(ns5)
P(''); P('## 离线（#45 raw，零新跑）')
d45=dual_table(R5,cells,arms,'#45 离线')
fl=[(a,c) for a in arms for c in cells if d45[(a,c)][1]=='k_c 敏感']
P('k_c 敏感格：'+('，'.join(f'{AN[a]} {c}' for a,c in fl) if fl else '无'))
a8=[c for c in cells if d45[('Ap8',c)][0][0]['c'].startswith('PASS')]
P(f"A+8 诊断 1：#45 通过格 {a8}；双口径下：" + '；'.join(f"{c} 归前 {d45[('Ap8',c)][0][1]['c']} / 不归前 {d45[('Ap8',c)][0][2]['c']}" for c in a8))
# drop last 2 pre frames
P(''); P('### 去掉末两帧（打断前最后两帧不进前段 δ 拟合）重算 A0/Aφ/A+8（h45 口径）')
Rd=runR(ns5,('h45',),True)
P('| 臂 | 格 | 原 J[上界]/Δv/结论 | 去末两帧 J[上界]/Δv/结论 | 变化 |'); chg=0
for a in ['A0','Aphi','Ap8']:
    for c in cells:
        Nh,Nv=l.nullq(okl(R5,'h45','base',c),'dv'); v0=l.verdict(okl(R5,'h45',a,c),Nh,Nv)
        Nh2,Nv2=l.nullq(okl(Rd,'h45','base',c),'dv'); v1=l.verdict(okl(Rd,'h45',a,c),Nh2,Nv2); chg+=v0['c']!=v1['c']
        P(f"| {AN[a]} | {c} | {v0['Jm']:.2f}[{v0['up']:.2f}]/{v0['dvm']:.0f}/{v0['c']} | {v1['Jm']:.2f}[{v1['up']:.2f}]/{v1['dvm']:.0f}/{v1['c']} | {'不变' if v0['c']==v1['c'] else '改变'} |")
P(f'→ 结论改变格数 {chg}/21')
# E3r/E3q
P(''); P('### t156 Δv 估计器离线试算（#45 base，h45 口径；Nv_h=0.95 分位，⚠E 规则同前）')
P('E3r：以“打断前解析轨迹（ω0、ζ）＋在 t_s 处位置/速度连续地切到新目标的解析续接”为参考，残差在 k_c 前 3 帧与 k_c 起 3 帧各做线性拟合，斜率差为 Δv（base 时参考即原轨迹）；E3q：k_c 前 5 帧与 k_c 起 5 帧各做局部二次拟合，t_s 处导数差。')
P('| 格 | Nv_h E3 | Nv_h E3r | 增幅 | Nv_h E3q | 增幅 | 阈中位 |')
G={}
for c in cells:
    b=okl(R5,'h45','base',c); n3=l.nullq(b,'dv')[1]; nr=l.nullq(b,'dvr')[1]; nq=l.nullq(b,'dvq')[1]; G[c]=(n3,nr,nq)
    P(f"| {c} | {n3:.1f} | {nr:.1f} | {100*(nr/n3-1):+.0f}% | {nq:.1f} | {100*(nq/n3-1):+.0f}% | 26/20/98 |")
for k,name in ((1,'E3r'),(2,'E3q')):
    t156=all(G[c][k]<=13 for c in ['t156rev','t156fwd','t156dbl']); oth=all(G[c][k]<=1.2*G[c][0] for c in ['t060rev','t060fwd','t300rev','t300fwd'])
    P(f"{name} 门：t156 三格 Nv_h≤13 {'✓' if t156 else '✗'}；t060/t300 增幅≤20% {'✓' if oth else '✗'} → {'过门' if t156 and oth else '不过门'}")
    G[name]=t156 and oth
P('各臂 t156 Δv 中位（E3 / E3r / E3q；阈 26）：')
for a in arms:
    P(f"- {AN[a]}："+'；'.join(f"{c} {np.median([m['dv'] for m in okl(R5,'h45',a,c)]):.0f} / {np.median([m['dvr'] for m in okl(R5,'h45',a,c)]):.0f} / {np.median([m['dvq'] for m in okl(R5,'h45',a,c)]):.0f}" for c in ['t156rev','t156fwd','t156dbl']))
if not (G['E3r'] or G['E3q']): P('→ E3r、E3q 均不过门：按裁决 #45 二.4，t156 Δv 定为「结构性不可判」（③ 修订须在 #47 开跑前登记）。')
# signed J & L*
P(''); P('### A+8 诊断 2：有符号 J（t_s 处 x_post−x_pre，乘 sign(v_ref)，正=超前）与 L*=8·J0s/(J0s−J8s)')
P('| 格 | J0s 中位 | J8s 中位 | L* ms | t̂_apply−tSeed 中位 ms(A0) |'); Ls={}
for c in cells:
    j0=np.median([m['Js'] for m in okl(R5,'h45','A0',c)]); j8=np.median([m['Js'] for m in okl(R5,'h45','Ap8',c)])
    Lst=8*j0/(j0-j8) if j0!=j8 else np.nan; Ls[c]=Lst; ap=np.median([m['apl'] for m in okl(R5,'h45','A0',c)])
    P(f'| {c} | {j0:.2f} | {j8:.2f} | {Lst:.2f} | {ap:.2f} |')
span=np.nanmax(list(Ls.values()))-np.nanmin(list(Ls.values()))
dd=[abs(Ls[f't{t}rev']-Ls[f't{t}fwd']) for t in ('060','156','300')]
P(f'L* 跨度 {span:.2f} ms（门≤2）；同时刻 rev/fwd 差 t060 {dd[0]:.2f} / t156 {dd[1]:.2f} / t300 {dd[2]:.2f}（门≤1）→ '+('满足立项条件' if span<=2 and max(dd)<=1 else '不满足 → A+8 永久关闭'))
# B seedV investigation
P(''); P('### B seedV 异常溯源（#45 raw）')
P('- t060（tInt≈66.6 ms）打断前只有 4–5 个捕获帧；B1 指令后前 1–2 帧仍显示上一试次终点 240/360（WGC 在 qStart 之后到达、SRT 也在 qStart 之后，但内容是 CompReset 前的旧画面/过渡帧），随后跳到 0。旧探针取到达序末两帧：若末两帧为 (240→0) 或 (2.5→−…) 即得 Δx≈−240/16.7 ms ≈ −14400；若末两帧同为 240 或同为 0 则为 0。即：读回问题（陈旧帧），而 B 的替换动画确实以该错误速度下发，所以其 ⚠M/J 是“错误种子下的实际行为”。')
P('- t156/t300：seedV≈1830 vs v_ref 522、144 vs −40——WGC 帧相对动画时钟滞后约 a≈28.8 ms（≈2 帧），末两帧反映的是 ~2 帧前的速度（t156 前 2 帧速度≈1800），属于 B 设计固有的“陈旧速度”，不是读回错误；#46 的取样修复不消除这一滞后。')
open('/tmp/rep46.txt','w').write('\n'.join(o)+'\n'); print('\n'.join(o))
