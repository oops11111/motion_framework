// probe_cli.cpp: console check of mfprobe.dll clock probes + spring shadow (no window needed).
#include <windows.h>
#include <cstdio>
#include <cstdint>
extern "C" {
__declspec(dllimport) int mf_spring_at(double, double, double, double, double, double, double*, double*);
__declspec(dllimport) double mf_spring_settle(double, double, double, double, double);
__declspec(dllimport) int mf_dcomp_frame_id(int, uint64_t*);
__declspec(dllimport) int mf_dcomp_stats(uint64_t, uint64_t*, uint64_t*, uint64_t*, uint32_t*);
__declspec(dllimport) int mf_dwm_timing(uint64_t*, uint64_t*, uint64_t*, uint64_t*, uint32_t*, uint32_t*, uint64_t*);
}
int main() {
    double v, vel;
    mf_spring_at(0.32, 0.78, 0, 240, 0, 0.15, &v, &vel);
    std::printf("spring(0.32,0.78) 0->240 at t=0.15: value=%.6f vel=%.6f settle=%.4f s\n", v, vel, mf_spring_settle(0.32, 0.78, 0, 240, 0));
    LARGE_INTEGER f; QueryPerformanceFrequency(&f);
    for (int i = 0; i < 5; ++i) {
        uint64_t id[3] = {0, 0, 0}; int hr[3];
        for (int k = 0; k < 3; ++k) hr[k] = mf_dcomp_frame_id(k, &id[k]);
        uint64_t st = 0, tt = 0, fp = 0; uint32_t nt = 0;
        int hs = mf_dcomp_stats(id[2], &st, &tt, &fp, &nt);
        uint64_t vb = 0, cp = 0, cr = 0, cf = 0, rp = 0; uint32_t rn = 0, rd = 0;
        int hd = mf_dwm_timing(&vb, &cp, &cr, &cf, &rn, &rd, &rp);
        LARGE_INTEGER q; QueryPerformanceCounter(&q);
        std::printf("t=%d00ms frameId created=%llu(hr=0x%08X) confirmed=%llu(0x%08X) completed=%llu(0x%08X) | stats(hr=0x%08X) start=%llu target=%llu period=%llu targets=%u | DWM hr=0x%08X qpcVBlank=%llu qpcCompose=%llu cRefresh=%llu cFrame=%llu rate=%u/%u qpcNow=%lld freq=%lld\n",
                    i * 5, id[0], hr[0], id[1], hr[1], id[2], hr[2], hs, st, tt, fp, nt, hd, vb, cp, cr, cf, rn, rd, q.QuadPart, f.QuadPart);
        Sleep(500);
    }
    return 0;
}
