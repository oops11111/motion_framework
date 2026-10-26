import numpy as np
SRC=open('/workspace/r2/p9.py').read().splitlines()
HEAD='\n'.join(SRC[3:52])  # after L= line, up to arms/AN (excl cells override)
def load(raw):
    ns={'np':np,'re':__import__('re'),'sys':__import__('sys')}
    ns['L']=open(raw,encoding='utf-8').read().splitlines()
    exec(HEAD,ns); ns['SP']=0.5
    return ns
def e3x(ns,r,mode='h45',drop2=False):
    b1=ns['b1'];basis=ns['basis'];fitdelta=ns['fitdelta'];a0=ns['a0'];T60=ns['T60'];W0=ns['W0'];z_=ns['z_'];sig_t=ns['sig_t'];SP=0.5
    t,x=r['t'],r['x']
    pre=np.where(t<r['tApply']/1000+a0-T60/2)[0]
    d0,_=fitdelta(t[pre],x[pre])
    if np.isnan(d0): return dict(NA=True)
    tau0=t-d0; ts0=r['tApply']/1000-(d0-a0); ks=np.where(tau0>ts0-T60/2)[0]
    if len(ks)<7 or ks[0]<4: return dict(NA=True)
    kc=ks[0]
    pidx=np.arange(0,kc+1 if mode=='pre' else kc)
    if drop2: pidx=pidx[:-2]
    d,_=fitdelta(t[pidx],x[pidx]); fo=d-a0; tm=r['tInt_m']/1000-fo; ts=r['tApply']/1000-fo; tau=t-d
    xp,vp=b1(tau); kcpre=abs(x[kc]-xp[kc])<=1.5
    idx=np.arange(kc,kc+5) if mode=='post' else np.arange(kc+1,kc+6)
    Tg=240.0 if r['arm']=='base' else (0.0 if r['dbl']==1 else r['final'])
    s=tau[idx]-tm; e=x[idx]-Tg
    def fit(w0,z,off=False,w=None):
        p1,p2,_,_=basis(s,w0,z); A=np.c_[p1,p2]
        if off: A=np.c_[A,np.ones_like(s)]
        if w is None: w=np.ones_like(s)
        sq=np.sqrt(w); sol,*_=np.linalg.lstsq(A*sq[:,None],e*sq,rcond=None); rr=A@sol-e
        return sol,(w*rr*rr).sum()
    sol,_=fit(W0,z_); _,_,d1,d2=basis(s); v=sol[0]*d1+sol[1]*d2; w=1/(SP**2+v**2*sig_t**2)
    s0,c0=fit(W0,z_,False,w); s1,c1=fit(W0,z_,True,w); M0=c0/3>2; M1=c1/2>2; use=(not M0) or ((not M1) and abs(s1[2])<=1)
    sol_=s0 if not M0 else s1[:2]
    def post(tt):
        q1,q2,dq1,dq2=basis(tt-tm); return Tg+sol_[0]*q1+sol_[1]*q2, sol_[0]*dq1+sol_[1]*dq2
    lo,hi=(tau[kc],tau[kc+1]) if mode=='pre' else (tau[kc-1],tau[kc])
    g=np.arange(lo,hi+1e-9,0.0005); xpo,_=post(g); xpr,_=b1(g); J=np.min(np.abs(xpo-xpr))
    TS=np.array([ts]); xpo_s,vpo_s=post(TS); xpr_s,vpr_s=b1(TS); vref=vpr_s[0]
    Js=(xpo_s[0]-xpr_s[0])*np.sign(vref)
    dv=vpo_s[0]-vpr_s[0]
    # E3r: residual vs continuous continuation to Tg
    xs,vs=xpr_s[0],vref
    q1,q2,_,_=basis(np.maximum(tau-ts,0)); cont=np.where(tau<ts,xp,Tg+(xs-Tg)*q1+vs*q2)
    rs=x-cont; pi=np.arange(max(kc-3,0),kc); qi=np.arange(kc,kc+3) if mode!='pre' else np.arange(kc+1,kc+4)
    dvr=np.polyfit(tau[qi],rs[qi],1)[0]-np.polyfit(tau[pi],rs[pi],1)[0]
    # E3q: local quadratics
    pq=np.arange(max(kc-5,0),kc); qq=np.arange(kc,kc+5)
    cp=np.polyfit(tau[pq]-ts,x[pq],2); cq=np.polyfit(tau[qq]-ts,x[qq],2); dvq=cq[1]-cp[1]
    return dict(NA=False,J=J,Js=Js,dv=abs(dv),dvs=dv,dvr=abs(dvr),dvq=abs(dvq),vref=vref,thr=max(20,0.05*abs(vref)),M0=M0,M1=M1,kcpre=kcpre,use=use,
                lag=r['tInt_m']-r['tInt_c'],seedV=(r['vB'] if r['arm']=='B' else r['shV']),apl=r['tApply']-r['tSeed'],phim=r['phi_m'],k=r.get('k'))
rng=np.random.default_rng(46)
def ciup(a):
    a=np.asarray(a); return np.percentile([np.median(rng.choice(a,len(a))) for _ in range(2000)],97.5)
def nullq(ms,key):
    J=np.array([m['J'] for m in ms]); V=np.array([m[key] for m in ms])
    q1,q3=np.percentile(J,[25,75]); out=np.where(J>q3+3*max(q3-q1,0.1))[0]; keep=np.ones(len(J),bool)
    if len(out)<=2: keep[out]=False
    return np.quantile(J[keep],0.95),np.quantile(V[keep],0.95)
def verdict(ok,Nh,Nv,key='dv'):
    J=np.array([m['J'] for m in ok]); thr=np.median([m['thr'] for m in ok]); eff=max(Nh,1); M0=sum(m['M0'] for m in ok)
    du=[m[key] for m in ok if m['use']] or [np.nan]; vj=Nv<=0.5*thr and M0<=2; up=ciup(J); dvm=np.median(du)
    if Nh>4: c='J门失败'
    elif vj: c='PASS' if (up<=eff and dvm<=thr) else 'FAIL'
    else: c='J-PASS' if up<=eff else 'J-FAIL'
    return dict(Jm=np.median(J),up=up,dvm=dvm,thr=thr,M0=M0,vj=vj,c=c,n=len(ok))
