import sys,numpy as np,pickle,warnings; warnings.filterwarnings('ignore'); sys.path.insert(0,'/workspace/r2'); import lib46 as l
A='/workspace/motion_framework/artifacts/'; o=[];P=o.append
d=pickle.load(open('/tmp/p11.pkl','rb')); TAB=d['TAB']; NUL=d['NUL']
cells=['t060rev','t060fwd','t156rev','t156fwd','t300rev','t300fwd','t156dbl']; T156=['t156rev','t156fwd','t156dbl']; NS=['t060rev','t060fwd','t300rev','t300fwd']
P('## ③（修订）判定 B_v（h45）')
P('| 格 | J 中位[CI上界] | J 阈 | J 通过 | Δv 中位 | 阈 | Nv_h | Δv 可判 | Δv 达标 | FAIL-确定界(阈+3·Nv_h) | 项结论 |')
ra=True; rb=True; rc=True
for c in cells:
    q=TAB[('Bv',c)]; N=NUL[c]; eff=max(N['Nh'],1); jp=q['jg'] and q['up']<=eff
    if c in T156:
        bound=q['thr']+3*N['Nv']; fc=q['dvm']>bound; rb&=not fc; rc&=jp
        res=('J 通过' if jp else 'J 不通过')+('；FAIL-确定' if fc else '；非 FAIL-确定')+'（Δv 结构性不可判）'
        P(f"| {c} | {q['Jm']:.2f}[{q['up']:.2f}] | {eff:.2f} | {'✓' if jp else '✗'} | {q['dvm']:.1f} | {q['thr']:.0f} | {N['Nv']:.1f} | 结构性不可判 | — | {bound:.1f} | {res} |")
    else:
        vp=q['vj'] and q['dvm']<=q['thr']; ok=jp and vp; ra&=ok
        P(f"| {c} | {q['Jm']:.2f}[{q['up']:.2f}] | {eff:.2f} | {'✓' if jp else '✗'} | {q['dvm']:.1f} | {q['thr']:.0f} | {N['Nv']:.1f} | {'是' if q['vj'] else '否'} | {'✓' if vp else '✗'} | — | {'达标' if ok else '不达标'} |")
P(f"(a) 非结构性可判项全部可判且达标：{'✓' if ra else '✗'}；(b) t156 三格无 FAIL-确定：{'✓' if rb else '✗'}；(c) t156 三格 J 通过：{'✓' if rc else '✗'}")
ok3=ra and rb and rc; d['ok3']=ok3
P('→ B_v '+('满足 ③（修订）：M1b 冻结对象由 B 改为 B_v，R-2 本机通过，仅剩 C5(b)' if ok3 else '不满足 ③（修订）：R-2 本机以 FAIL 结题，仅剩 C5(b)；M1b 以 B_v 作为降级实现，文档标注「不满足 R-2」'))
# k_c sensitivity
ns=l.load(A+'win_r2_p11_raw.txt'); R={}
for mode in ('h45','pre','post'):
    for r in ns['tr']: R.setdefault((mode,r['arm'],r['cell']),[]).append(l.e3x(ns,r,mode))
okl=lambda mode,a,c:[m for m in R[(mode,a,c)] if not m['NA']]
P(''); P('## k_c 敏感性列（仅敏感性，结论以 h45 为准）')
P('| 格 | h45 J[上界]/Δv | 强制归前 | 强制不归前 | 标记 |')
for c in cells:
    vs=[]
    for mode in ('h45','pre','post'):
        Nh,Nv=l.nullq(okl(mode,'base',c),'dv'); vs.append(l.verdict(okl(mode,'Bv',c),Nh,Nv))
    jp=[v['up']<=max(l.nullq(okl(m,'base',c),'dv')[0],1) for v,m in zip(vs,('h45','pre','post'))]
    tag='k_c 敏感' if len(set(jp))>1 else ''
    P(f"| {c} | "+' | '.join(f"{v['Jm']:.2f}[{v['up']:.2f}]/{v['dvm']:.0f}" for v in vs)+f" | {tag} |")
# fresh frames content-independent (offline): frames with SRT>=B1 t_apply(=first vblank after start) and arrived before tInt_m
P(''); P('## 内容无关新鲜帧计数（离线，仅信息）：B1 后 SRT ≥ B1 生效帧、且到达时刻 ≤ tInt_m 的帧数')
import re
L=open(A+'win_r2_p11_raw.txt',encoding='utf-8').read().splitlines()
for c in cells:
    n=[]
    for i,s in enumerate(L):
        if s.startswith('TRIAL Bv '+c):
            dd=dict(re.findall(r'(\w+)=(\S+)',s)); ph=float(dd['phiStart']); ap=(16.6611-ph)%16.6611; tm=float(dd['tInt_m'])
            fr=[tuple(map(float,x.split(':'))) for x in L[i+1][4:].split()]; n.append(sum(1 for f in fr if f[2]>=ap and f[0]<=tm))
    P(f"{c}: 中位 {np.median(n):.0f}，范围 {min(n)}–{max(n)}")
open('/tmp/rep47.txt','w').write('\n'.join(o)+'\n'); pickle.dump(d,open('/tmp/p11.pkl','wb')); print('\n'.join(o))
