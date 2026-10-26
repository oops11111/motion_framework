import sys,math
def classify(iv, hz=60.020):
    n=len(iv); tick=15.625; tol=1.0
    k1=sum(abs(x-tick)<=tol for x in iv)/n; k2=sum(abs(x-2*tick)<=tol for x in iv)/n
    vs_ms=1000/hz; vs=sum(abs(x-vs_ms)<=tol for x in iv)/n
    s=sorted(iv); p95=s[min(n-1,math.ceil(0.95*n)-1)]
    v='timer-driven' if k1+k2>=0.8 else ('≈60Hz vsync' if vs>=0.8 and p95<=18 else 'unknown')
    return f"n={n} k1={100*k1:.1f}% k2={100*k2:.1f}% timer={100*(k1+k2):.1f}% vsync({vs_ms:.3f}±1)={100*vs:.1f}% p95={p95:.3f} mean={sum(iv)/n:.3f} -> {v}"
for name,iv in [('PS standalone raw (04:58)',[float(x) for x in open('dwmflush_raw_ps.txt').read().split()]),
                ('app --measure raw (04:58)',[float(x) for x in [l for l in open('comp_pilot_log_m8b.txt',encoding='utf-8') if 'raw intervals' in l][0].split(': ',1)[1].split(',')])]:
    print(name+': '+classify(iv))
