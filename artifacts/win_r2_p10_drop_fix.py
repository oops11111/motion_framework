import re,collections,numpy as np
L=open('/workspace/motion_framework/artifacts/win_r2_p10_raw.txt',encoding='utf-8').read().splitlines()
import sys
OFF=float(sys.argv[1]) if len(sys.argv)>1 else 0
out=[]
for cell in ['t060rev','t060fwd']:
    D=[];F=[];logged=[]
    for i,l in enumerate(L):
        if l.startswith('TRIAL B '+cell):
            d=dict(re.findall(r'(\w+)=(\S+)',l)); trip=float(d['tInt_c'])
            fr=[tuple(map(float,x.split(':'))) for x in L[i+1][4:].split()]
            fr=[f for f in fr if f[0]<=trip+OFF and f[2]>0]
            kept=[];drop=0;fresh=False
            for f in fr:
                if not fresh:
                    if abs(f[1])<0.5: fresh=True
                    else: drop+=1; continue
                if kept and (f[0]<=kept[-1][0] or f[2]<=kept[-1][2] or abs(f[1]-kept[-1][1])<0.01): drop+=1; continue
                kept.append(f)
            D.append(drop);F.append(len(kept)); logged.append(d.get('vBdrop'))
    D=np.array(D);F=np.array(F)
    out.append(f"{cell}: n={len(D)}；每试次剔除帧分布 {dict(sorted(collections.Counter(D.tolist()).items()))}（中位 {np.median(D):.0f}，合计 {D.sum()}）；保留新鲜帧数分布 {dict(sorted(collections.Counter(F.tolist()).items()))}；探针内 vBdrop 字段仅在新鲜帧≥2 时写出：{sum(x is not None for x in logged)} 次，值 {[x for x in logged if x]}")
print('\n'.join(out))
