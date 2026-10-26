import numpy as np,re,sys
RAW=sys.argv[1] if len(sys.argv)>1 else '/workspace/motion_framework/artifacts/win_r2_p11_raw.txt'
L=open(RAW,encoding='utf-8').read().splitlines()
r_,z_=0.32,0.78; W0=2*np.pi/r_
def basis(s,w0=W0,z=z_):
    wd=w0*np.sqrt(1-z*z); e=np.exp(-z*w0*s); c=np.cos(wd*s); sn=np.sin(wd*s)
    return e*(c+(z*w0/wd)*sn), e*sn/wd, -e*(w0**2/wd)*sn, e*(c-(z*w0/wd)*sn)
def b1(t):
    t=np.asarray(t,float); s=np.maximum(t,0); p1,p2,d1,d2=basis(s)
    return np.where(t>0,240-240*p1,0.0),np.where(t>0,-240*d1,0.0)
P0=[];tr=[];cur=None;hdr=[]
for l in L:
    if l.startswith('#') or l.startswith('cal') or l.startswith('Dwm'): hdr.append(l)
    if l.startswith('TRIAL'):
        s=l.split(); d=dict(re.findall(r'(\w+)=(\S+)',l)); cur=dict(arm=s[1],cell=s[2],k=int(s[3]),**{a:(float(b) if re.match(r'^-?[\d.]+$',b) else b) for a,b in d.items()})
    elif l.startswith('RAW ') and cur:
        a=np.array([tuple(map(float,x.split(':'))) for x in l[4:].split()]) if len(l)>5 else np.zeros((0,3))
        if len(a):
            t,x=a[:,2]/1000,a[:,1]; k0=next((i for i,b in enumerate(x) if abs(b)<0.5),None)
            if k0 is not None:
                cur['t'],cur['x']=t[k0:],x[k0:]; (P0 if cur['arm']=='P0base' else tr).append(cur)
        cur=None
DG=np.arange(0,0.15001,0.0005)
def fitdelta(t,x):
    if len(t)<3: return np.nan,np.nan
    X=b1(t[None,:]-DG[:,None])[0]; sse=((x[None,:]-X)**2).sum(1); i=int(np.argmin(sse)); return DG[i],np.sqrt(sse[i]/len(t))
rng=np.random.default_rng(20261045)
def bootci(a,f=np.median,n=3000):
    a=np.asarray(a); 
    if len(a)==0: return (np.nan,np.nan)
    m=[f(rng.choice(a,len(a))) for _ in range(n)]; return np.percentile(m,[2.5,97.5])
o=[];P=o.append
T60=1/60
# ---- P0
F=[]
for r in P0:
    m=r['t']<=0.65; d,rms=fitdelta(r['t'][m],r['x'][m]); F.append((d,rms,r['phiStart']/1000,r['phi']))
F=np.array(F); dP,rP,phm,lay=F.T; u=(T60-phm)%T60
A=np.c_[np.ones_like(u),u]; (a0,b0),*_=np.linalg.lstsq(A,dP,rcond=None); res=dP-A@[a0,b0]
mad=1.4826*np.median(np.abs(res-np.median(res))); sig_t=mad if mad>0 else 0.001
layIQR={p:np.subtract(*np.percentile(res[lay==p],[75,25])) for p in sorted(set(lay))}
# b CI
bb=[]
for _ in range(2000):
    i=rng.integers(0,len(u),len(u)); 
    if np.ptp(u[i])>0: bb.append(np.polyfit(u[i],dP[i],1)[0])
bci=np.percentile(bb,[2.5,97.5])
ok_b=0.8<=b0<=1.2; ok_i=all(v<=0.004 for v in layIQR.values()); ok_m=mad<=0.003; ok_r=np.median(rP)<=1
P0ok=ok_b and ok_i and ok_m and ok_r
DIAG='' if P0ok else ' `DIAG`'
cells=['t060rev','t060fwd','t156rev','t156fwd','t300rev','t300fwd','t156dbl']
arms=['Bv']; AN={'Bv':'B_v','B':'B','A0':'A0','Aphi':'Aφ','Ap8':'A+8'}
SP=0.5
def e3(r):
    t,x=r['t'],r['x']
    pre=t<r['tApply']/1000+a0-T60/2
    d,_=fitdelta(t[pre],x[pre])
    fo=d-a0; tm=r['tInt_m']/1000-fo; ts=r['tApply']/1000-fo  # 动画时钟域（原点=B1 首个合成帧）
    if np.isnan(d): return dict(NA=True)
    tau=t-d; ks=np.where(tau>ts-T60/2)[0]
    if len(ks)<6: return dict(NA=True)
    kc=ks[0]; xp,vp=b1(tau); devkc=abs(x[kc]-xp[kc]); kcpre=devkc<=1.5
    idx=np.arange(kc+1,kc+6); Tg=240.0 if r['arm']=='base' else (0.0 if r['dbl']==1 else r['final'])
    s=tau[idx]-tm; e=x[idx]-Tg
    def fit(w0,z,off=False,w=None):
        p1,p2,d1,d2=basis(s,w0,z); A=np.c_[p1,p2]+0
        if off: A=np.c_[A,np.ones_like(s)]
        if w is None: w=np.ones_like(s)
        sq=np.sqrt(w); sol,*_=np.linalg.lstsq(A*sq[:,None],e*sq,rcond=None); rr=A@sol-e
        return sol,(w*rr*rr).sum(),A
    sol,_,_=fit(W0,z_); p1,p2,d1,d2=basis(s); v=sol[0]*d1+sol[1]*d2
    w=1/(SP**2+v**2*sig_t**2)
    s0,c0,_=fit(W0,z_,False,w); s1,c1,_=fit(W0,z_,True,w)
    x0=c0/3; x1=c1/2 if len(s)>3 else np.nan
    best=min(((fit(W0*sc,zz,False,w)[1],sc,zz) for sc in np.linspace(0.8,1.25,19) for zz in np.linspace(0.6,0.99,17)))
    x2=best[0]/3; M0=x0>2; M1=x1>2; M2=x2>2; PM=M0 and best[0]<0.5*c0
    use=(not M0) or ((not M1) and abs(s1[2])<=1)
    sol_=s0 if not M0 else s1[:2]
    def post(tt):
        q1,q2,dq1,dq2=basis(tt-tm); return Tg+sol_[0]*q1+sol_[1]*q2, sol_[0]*dq1+sol_[1]*dq2
    g=np.arange(tau[kc-1],tau[kc]+1e-9,0.0005); xpo,_=post(g); xpr,_=b1(g); J=np.min(np.abs(xpo-xpr))
    vpo=post(np.array([ts]))[1][0]; vpr=b1(np.array([ts]))[1][0]
    return dict(NA=False,J=J,dv=abs(vpo-vpr),dvs=vpo-vpr,vref=vpr,thr=max(20,0.05*abs(vpr)),M0=M0,M1=M1,M2=M2,PM=PM,chi=x0,use=use,kcpre=kcpre,phim=r['phi_m']/1000,
                L=(r['seedT']-r['tInt_m']),seedV=(r['vB'] if r['arm']=='B' else r['shV']),lag=r['tInt_m']-r['tInt_c'])
R={(a,c,p):[] for a in arms+['base'] for c in cells for p in (2,6,10,14)}
for r in tr: R[(r['arm'],r['cell'],int(r['phi']))].append(e3(r))
def cellstats(a,c,lays=(2,6,10,14)):
    res=sum((R[(a,c,p)] for p in lays),[]); ok=[m for m in res if not m['NA']]; return res,ok
NUL={}
for c in cells:
    _,ok=cellstats('base',c); J=np.array([m['J'] for m in ok]); V=np.array([m['dv'] for m in ok])
    q1,q3=np.percentile(J,[25,75]); out=np.where(J>q3+3*max(q3-q1,0.1))[0]; Ecell=len(out)>2
    keep=np.ones(len(J),bool); 
    if not Ecell: keep[out]=False
    NUL[c]=dict(Nh=np.quantile(J[keep],0.95),Nv=np.quantile(V[keep],0.95),nE=len(out),Ecell=Ecell,M=sum(m['M0'] for m in ok),n=len(ok),Jm=np.median(J),Vm=np.median(V))
def row(a,c,lays=(2,6,10,14)):
    res,ok=cellstats(a,c,lays); n=len(res); nNA=n-len(ok); N=NUL[c]
    if not ok: return None
    J=np.array([m['J'] for m in ok]); thr=np.median([m['thr'] for m in ok]); eff=max(N['Nh'],1.0)
    dvu=[m['dv'] for m in ok if m['use']]; M0=sum(m['M0'] for m in ok); M1=sum(m['M1'] for m in ok); M2=sum(m['M2'] for m in ok)
    jg=N['Nh']<=4; vj=N['Nv']<=0.5*thr and M0<=2*len(ok)/20+1e-9
    up=bootci(J)[1]; dvm=np.median(dvu) if dvu else np.nan
    pJ=up<=eff; pV=dvm<=thr
    strict=all(m['J']<=eff and m['dv']<=m['thr'] for m in ok)
    if not jg: cc='J门失败'
    elif vj: cc=('PASS'+('(严格)' if strict else '')) if (pJ and pV) else 'FAIL'
    else: cc='J-PASS(Δv不可判)' if pJ else 'J-FAIL'
    return dict(n=n,nNA=nNA,Jm=np.median(J),up=up,dvm=dvm,thr=thr,M0=M0,M1=M1,M2=M2,PM=sum(m['PM'] for m in ok),chi=np.median([m['chi'] for m in ok]),
        L=np.median([m['L'] for m in ok]),seedV=np.median([m['seedV'] for m in ok]),vref=np.median([m['vref'] for m in ok]),kcpre=sum(m['kcpre'] for m in ok),jg=jg,vj=vj,c=cc,pass_=cc.startswith('PASS'),lag=np.median([m['lag'] for m in ok]))
P(f'# win_r2_p11_47.txt — 派题 #47{DIAG}（docs/notion_r2_46_ab.md）：新臂 B_v（InitialValue 不设=原 B 位置路径；InitialVelocity=S1 v2 remap 影子解析速度 @tSeed；无 vB=0 回退）＋同场 base，7 格×4 相位层×5')
P('# 环境：composition.h a55d4952…e418（MapSpringToCompositor），60Hz，pxPerDip=1.0；DESKTOP-K93N3P8 Win10 19045；只用程序化动画，无合成输入，没动鼠标；脚本 /workspace/r2/p9.py；源 win_r2_p11_raw.txt；判据 ③ 修订 + k_c 口径 h45；D4 新鲜帧规则仅测试工具，B_v 不读 WGC 速度')
P('# ω0=2π/r=1/Period=19.635 rad/s，ζ=0.78；时钟 WGC SRT+srtOff')
P('# 与文档偏差：(1) Win10 19045 无 DCompositionWaitForCompositorClock → 用 DwmGetCompositionTimingInfo(qpcVBlank, qpcRefreshPeriod) 推算帧相位 + QPC 自旋等到 φ*；(2) WinUI3 无同步 Commit：tInt_m=替换 StartAnimation 返回时 QPC（隐式批次），另记 RequestCommitAsync 完成时刻 tCommitDone 作参考；种子计算时刻 tSeed 在 tInt_m 前 <0.1 ms；(3) t̂_apply=tInt_m 后下一个 DWM vblank 预测；(4) t_s=t̂_apply 换域后（同相位起止时 ≈ QPC 原始 tInt_m）；k_c=τ≥t_s−T/2 的首帧（替换动画首个可见帧）；(5) ⚠E 离群判据 J>Q3+3·IQR（文档未给）；J 阈=max(N_h,1)，J 门 N_h≤4（沿用 r2_nh_ruling），Δv 阈=max(20,5%|v_ref|)')
for h in hdr[:6]: P('# raw: '+h[:200])
P(''); P('## P0（48 次 base，起始相位受控 4 层×12）锯齿拟合 δ̂=a+b·((T−φ)mod T)')
P(f'a={a0*1000:.2f} ms，b={b0:.3f}（boot 95% CI [{bci[0]:.2f},{bci[1]:.2f}]）门[0.8,1.2] {"✓" if ok_b else "✗"}；1.4826·MAD(r)={mad*1000:.2f} ms 门≤3 {"✓" if ok_m else "✗"}；RMS 中位={np.median(rP):.3f} DIP 门≤1 {"✓" if ok_r else "✗"}；σ_t={sig_t*1000:.2f} ms')
P('| 层 φ* | n | φ_m 中位 ms | δ̂ 中位 ms | δ̂ IQR ms | IQR(r) ms 门≤4 | RMS 中位 |')
for p in sorted(set(lay)):
    m=lay==p; P(f'| {p:.0f} | {m.sum()} | {np.median(phm[m])*1000:.2f} | {np.median(dP[m])*1000:.1f} | {np.subtract(*np.percentile(dP[m],[75,25]))*1000:.2f} | {layIQR[p]*1000:.2f} {"✓" if layIQR[p]<=0.004 else "✗"} | {np.median(rP[m]):.3f} |')
P(f'→ P0 {"通过" if P0ok else "失败 → MODEL-MISMATCH，以下全部仅 DIAG，不出判定"}')
# tInt_m - tInt_c vs phi (P1, all)
lg=np.array([[ (r['tInt_m']-r['tInt_c']), r['phi_m']] for r in tr]); sl=np.polyfit(lg[:,1],lg[:,0],1)
P(f'tInt_m−tInt_c（P1 全部 {len(lg)} 次）：中位 {np.median(lg[:,0]):.2f} ms，范围 {lg[:,0].min():.2f}–{lg[:,0].max():.2f}；对 φ_m 斜率 {sl[0]:.3f} ms/ms；φ_m−φ* 中位 {np.median([r["phi_m"]-r["phi"] for r in tr]):.3f} ms')
P(''); P('## 零假设基线（base，E3）')
P('| 格 | n | J_null 中位 | N_h | Δv_null 中位 | Nv_h | ⚠E | base ⚠M(M0) |')
for c in cells:
    N=NUL[c]; P(f"| {c} | {N['n']} | {N['Jm']:.2f} | {N['Nh']:.2f} | {N['Vm']:.1f} | {N['Nv']:.1f} | {N['nE']}{'(⚠E格)' if N['Ecell'] else ''} | {N['M']}/{N['n']} |")
badJ=sum(NUL[c]['Nh']>4 for c in cells); badM=sum(NUL[c]['M']>2*NUL[c]['n']/20 for c in cells)
void=(not P0ok) or badJ>=3 or badM>=2
P(f'会话有效性：P0 {"✓" if P0ok else "✗"}；base J 门未过 {badJ}/7（≥3 作废）；base ⚠M>2/20 格数 {badM}（≥2 作废）→ {"会话作废" if void else "会话有效"}')
TAB={}
for a in arms:
    P(''); P(f'===== 臂 {AN[a]}{DIAG} =====')
    P('| 格 | 相位层 | n(NA) | k_c≤1.5归前 | N_h | Nv_h | 可判 | J 中位 [CI上界] | Δv 中位 (阈) | ⚠M M0/M1/M2 | χ²/dof 中位 | L ms | seedV | v_ref | 结论 |')
    for c in cells:
        q=row(a,c); TAB[(a,c)]=q; N=NUL[c]
        P(f"| {c} | 全部 | {q['n']}({q['nNA']}) | {q['kcpre']} | {N['Nh']:.2f} | {N['Nv']:.1f} | {'J+Δv' if q['jg'] and q['vj'] else ('J' if q['jg'] else '否')} | {q['Jm']:.2f} [{q['up']:.2f}] | {q['dvm']:.1f} ({q['thr']:.0f}) | {q['M0']}/{q['M1']}/{q['M2']}{' PM'+str(q['PM']) if q['PM'] else ''} | {q['chi']:.2f} | {q['L']:.2f} | {q['seedV']:.0f} | {q['vref']:.0f} | {q['c']} |")
        for p in (2,6,10,14):
            z=row(a,c,(p,))
            if z: P(f"|  | φ*={p} | {z['n']}({z['nNA']}) | {z['kcpre']} |  |  |  | {z['Jm']:.2f} | {z['dvm']:.1f} | {z['M0']}/{z['M1']}/{z['M2']} | {z['chi']:.2f} | {z['L']:.2f} | {z['seedV']:.0f} | {z['vref']:.0f} |  |")
    np_=sum(TAB[(a,c)]['pass_'] for c in cells); Jall=np.median([TAB[(a,c)]['Jm'] for c in cells]); nvj=sum(TAB[(a,c)]['vj'] for c in cells)
    nvp=sum(TAB[(a,c)]['vj'] and TAB[(a,c)]['dvm']<=TAB[(a,c)]['thr'] for c in cells); Mok=all(TAB[(a,c)]['M0']<=2 for c in cells)
    TAB[a]=dict(np=np_,J=Jall,nvj=nvj,nvp=nvp,Mok=Mok)
    P(f'{AN[a]}：通过格 {np_}/7；J 全格中位 {Jall:.2f}；Δv 可判 {nvj}/7，其中 Δv 达标 {nvp}；⚠M≤2/20 全格 {"是" if Mok else "否"}')

import pickle; pickle.dump(dict(TAB={k:v for k,v in TAB.items() if isinstance(k,tuple)},NUL=NUL),open('/tmp/p11.pkl','wb'))
out='/workspace/motion_framework/artifacts/win_r2_p11_47.txt' if len(sys.argv)<3 else sys.argv[2]
open(out,'w',encoding='utf-8').write('\n'.join(o)+'\n'); print('\n'.join(o))
