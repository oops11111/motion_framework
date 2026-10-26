// MotionNative/mf_probe.cpp -> mfprobe.dll (派题 #7). Helper for the M1b compositor pilot:
//   * spring shadow state: closed form from code/spring.h (included, not modified)
//   * clock probes: DCompositionGetFrameId / DCompositionGetStatistics (resolved at runtime from
//     dcomp.dll, build 22000+; the installed SDK 10.0.18362 headers do not declare them) and
//     DwmGetCompositionTimingInfo
//   * Windows.Graphics.Capture of a window (IGraphicsCaptureItemInterop::CreateForWindow) with a
//     free-threaded Direct3D11CaptureFramePool; each frame is scanned for the #3B82F6 box.
#include <unknwn.h>
#include <windows.h>
#include <d3d11.h>
#include <dxgi.h>
#include <dwmapi.h>
#include <Windows.Graphics.Capture.Interop.h>
#include <windows.graphics.directx.direct3d11.interop.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Graphics.Capture.h>
#include <winrt/Windows.Graphics.DirectX.Direct3D11.h>
#include <atomic>
#include <cstdint>
#include <mutex>
#include <vector>
#include "spring.h"
#include "composition.h"   // 派题 #44: formal S1 v2 mapping (MapSpringToCompositor), not modified

#define MF_API extern "C" __declspec(dllexport)

// ---------------------------------------------------------------- spring shadow (spring.h)
MF_API int mf_spring_at(double response, double damping, double from, double target, double v0,
                        double t, double* value, double* velocity) {
    motion::Spring s({response, damping}, from, target, v0);
    auto st = s.At(t);
    if (value) *value = st.value;
    if (velocity) *velocity = st.velocity;
    return 1;
}
// 派题 #44: expose composition.h MapSpringToCompositor (S1 v2) to the probe; returns compositor Period / DampingRatio.
MF_API int mf_map_spring(double response, double damping, double* period, double* dampingRatio) {
    if (!(response > 0.0)) return 0;
    const motion::SpringParams c = motion::MapSpringToCompositor({response, damping});
    if (period) *period = c.response;
    if (dampingRatio) *dampingRatio = c.damping;
    return 1;
}
MF_API double mf_spring_settle(double response, double damping, double from, double target, double v0) {
    motion::Spring s({response, damping}, from, target, v0);
    return s.SettleTime();
}

// ---------------------------------------------------------------- DComp / DWM clock probes
typedef struct { UINT64 startTime, targetTime, framePeriod; } MF_FRAME_STATS;   // = COMPOSITION_FRAME_STATS
typedef HRESULT(WINAPI* PFN_GetFrameId)(int, UINT64*);
typedef HRESULT(WINAPI* PFN_GetStats)(UINT64, MF_FRAME_STATS*, UINT, void*, UINT*);
static PFN_GetFrameId g_getFrameId = nullptr;
static PFN_GetStats g_getStats = nullptr;
static void LoadDComp() {
    static bool done = false;
    if (done) return;
    done = true;
    if (HMODULE m = LoadLibraryW(L"dcomp.dll")) {
        g_getFrameId = (PFN_GetFrameId)GetProcAddress(m, "DCompositionGetFrameId");
        g_getStats = (PFN_GetStats)GetProcAddress(m, "DCompositionGetStatistics");
    }
}
// type: 0 = COMPOSITION_FRAME_ID_CREATED, 1 = CONFIRMED, 2 = COMPLETED. Returns HRESULT (or 0x80004001 if not exported).
MF_API int mf_dcomp_frame_id(int type, uint64_t* id) {
    LoadDComp();
    if (!g_getFrameId) return (int)0x80004001;
    UINT64 v = 0;
    HRESULT hr = g_getFrameId(type, &v);
    if (id) *id = v;
    return hr;
}
MF_API int mf_dcomp_stats(uint64_t frameId, uint64_t* startTime, uint64_t* targetTime, uint64_t* framePeriod, uint32_t* targets) {
    LoadDComp();
    if (!g_getStats) return (int)0x80004001;
    MF_FRAME_STATS st{};
    UINT actual = 0;
    HRESULT hr = g_getStats(frameId, &st, 0, nullptr, &actual);
    if (startTime) *startTime = st.startTime;
    if (targetTime) *targetTime = st.targetTime;
    if (framePeriod) *framePeriod = st.framePeriod;
    if (targets) *targets = actual;
    return hr;
}
MF_API int mf_dwm_timing(uint64_t* qpcVBlank, uint64_t* qpcCompose, uint64_t* cRefresh, uint64_t* cFrame,
                         uint32_t* rateRefreshNum, uint32_t* rateRefreshDen, uint64_t* qpcRefreshPeriod) {
    DWM_TIMING_INFO ti{};
    ti.cbSize = sizeof(ti);
    HRESULT hr = DwmGetCompositionTimingInfo(nullptr, &ti);
    if (qpcVBlank) *qpcVBlank = ti.qpcVBlank;
    if (qpcCompose) *qpcCompose = ti.qpcCompose;
    if (cRefresh) *cRefresh = ti.cRefresh;
    if (cFrame) *cFrame = ti.cFrame;
    if (rateRefreshNum) *rateRefreshNum = ti.rateRefresh.uiNumerator;
    if (rateRefreshDen) *rateRefreshDen = ti.rateRefresh.uiDenominator;
    if (qpcRefreshPeriod) *qpcRefreshPeriod = ti.qpcRefreshPeriod;
    return hr;
}

// ---------------------------------------------------------------- Windows.Graphics.Capture
namespace wgc = winrt::Windows::Graphics::Capture;
namespace wd3d = winrt::Windows::Graphics::DirectX::Direct3D11;
struct CapRec { int64_t srt100ns, qpcArrive; int w, h, minx, maxx, miny, maxy, n; double cx; };
static std::mutex g_mx;
static std::vector<CapRec> g_recs;
static winrt::com_ptr<ID3D11Device> g_dev;
static winrt::com_ptr<ID3D11DeviceContext> g_ctx;
static winrt::com_ptr<ID3D11Texture2D> g_staging;
static wd3d::IDirect3DDevice g_wdev{nullptr};
static wgc::GraphicsCaptureItem g_item{nullptr};
static wgc::Direct3D11CaptureFramePool g_pool{nullptr};
static wgc::GraphicsCaptureSession g_sess{nullptr};
static std::atomic<int> g_frames{0};
static int g_lastHr = 0;

static void OnFrame(wgc::Direct3D11CaptureFramePool const& pool) {
    auto frame = pool.TryGetNextFrame();
    if (!frame) return;
    LARGE_INTEGER q; QueryPerformanceCounter(&q);
    CapRec r{};
    r.srt100ns = frame.SystemRelativeTime().count();
    r.qpcArrive = q.QuadPart;
    r.minx = r.maxx = r.miny = r.maxy = -1;
    try {
        auto access = frame.Surface().as<::Windows::Graphics::DirectX::Direct3D11::IDirect3DDxgiInterfaceAccess>();
        winrt::com_ptr<ID3D11Texture2D> tex;
        winrt::check_hresult(access->GetInterface(__uuidof(ID3D11Texture2D), tex.put_void()));
        D3D11_TEXTURE2D_DESC d{};
        tex->GetDesc(&d);
        std::lock_guard<std::mutex> lk(g_mx);
        D3D11_TEXTURE2D_DESC sd{};
        if (g_staging) g_staging->GetDesc(&sd);
        if (!g_staging || sd.Width != d.Width || sd.Height != d.Height) {
            g_staging = nullptr;
            D3D11_TEXTURE2D_DESC n = d;
            n.Usage = D3D11_USAGE_STAGING; n.BindFlags = 0; n.CPUAccessFlags = D3D11_CPU_ACCESS_READ; n.MiscFlags = 0;
            n.MipLevels = 1; n.ArraySize = 1; n.SampleDesc.Count = 1; n.SampleDesc.Quality = 0;
            winrt::check_hresult(g_dev->CreateTexture2D(&n, nullptr, g_staging.put()));
        }
        g_ctx->CopyResource(g_staging.get(), tex.get());
        D3D11_MAPPED_SUBRESOURCE m{};
        winrt::check_hresult(g_ctx->Map(g_staging.get(), 0, D3D11_MAP_READ, 0, &m));
        r.w = (int)d.Width; r.h = (int)d.Height;
        double sumx = 0; int n = 0;
        for (UINT y = 0; y < d.Height; ++y) {
            const uint8_t* row = (const uint8_t*)m.pData + (size_t)y * m.RowPitch;
            for (UINT x = 0; x < d.Width; ++x) {
                const uint8_t* p = row + x * 4;   // BGRA
                if (abs((int)p[0] - 0xF6) < 30 && abs((int)p[1] - 0x82) < 30 && abs((int)p[2] - 0x3B) < 30) {
                    if (r.minx < 0 || (int)x < r.minx) r.minx = (int)x;
                    if ((int)x > r.maxx) r.maxx = (int)x;
                    if (r.miny < 0) r.miny = (int)y;
                    r.maxy = (int)y;
                    sumx += x; ++n;
                }
            }
        }
        g_ctx->Unmap(g_staging.get(), 0);
        r.n = n; r.cx = n ? sumx / n : -1.0;
        g_recs.push_back(r);
    } catch (winrt::hresult_error const& e) {
        g_lastHr = e.code();
        std::lock_guard<std::mutex> lk(g_mx);
        g_recs.push_back(r);
    }
    g_frames++;
}

// Returns 0 on success, else an HRESULT.
MF_API int mf_cap_start(HWND hwnd) {
    try {
        {
            std::lock_guard<std::mutex> lk(g_mx);
            g_recs.clear(); g_frames = 0;
        }
        winrt::check_hresult(D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                                               nullptr, 0, D3D11_SDK_VERSION, g_dev.put(), nullptr, g_ctx.put()));
        auto dxgi = g_dev.as<IDXGIDevice>();
        winrt::com_ptr<::IInspectable> insp;
        winrt::check_hresult(CreateDirect3D11DeviceFromDXGIDevice(dxgi.get(), insp.put()));
        g_wdev = insp.as<wd3d::IDirect3DDevice>();
        auto interop = winrt::get_activation_factory<wgc::GraphicsCaptureItem, IGraphicsCaptureItemInterop>();
        winrt::check_hresult(interop->CreateForWindow(hwnd, winrt::guid_of<wgc::GraphicsCaptureItem>(), winrt::put_abi(g_item)));
        g_pool = wgc::Direct3D11CaptureFramePool::CreateFreeThreaded(
            g_wdev, winrt::Windows::Graphics::DirectX::DirectXPixelFormat::B8G8R8A8UIntNormalized, 2, g_item.Size());
        g_pool.FrameArrived([](wgc::Direct3D11CaptureFramePool const& p, winrt::Windows::Foundation::IInspectable const&) { OnFrame(p); });
        g_sess = g_pool.CreateCaptureSession(g_item);
        g_sess.StartCapture();
        return 0;
    } catch (winrt::hresult_error const& e) {
        g_lastHr = e.code();
        return e.code();
    } catch (...) {
        return (int)0x80004005;
    }
}
MF_API int mf_cap_item_size(int* w, int* h) {
    if (!g_item) return 0;
    auto s = g_item.Size();
    if (w) *w = s.Width;
    if (h) *h = s.Height;
    return 1;
}
MF_API void mf_cap_stop() {
    try {
        if (g_sess) { g_sess.Close(); g_sess = nullptr; }
        if (g_pool) { g_pool.Close(); g_pool = nullptr; }
        g_item = nullptr; g_wdev = nullptr;
        std::lock_guard<std::mutex> lk(g_mx);
        g_staging = nullptr; g_ctx = nullptr; g_dev = nullptr;
    } catch (...) {}
}
MF_API int mf_cap_frames() { return g_frames.load(); }
MF_API int mf_cap_last_hr() { return g_lastHr; }
MF_API int mf_cap_get(int i, int64_t* srt100ns, int64_t* qpcArrive, int* w, int* h, int* minx, int* maxx, int* miny, int* maxy, int* n, double* cx) {
    std::lock_guard<std::mutex> lk(g_mx);
    if (i < 0 || i >= (int)g_recs.size()) return 0;
    const CapRec& r = g_recs[i];
    *srt100ns = r.srt100ns; *qpcArrive = r.qpcArrive; *w = r.w; *h = r.h;
    *minx = r.minx; *maxx = r.maxx; *miny = r.miny; *maxy = r.maxy; *n = r.n; *cx = r.cx;
    return 1;
}
MF_API int mf_cap_count() { std::lock_guard<std::mutex> lk(g_mx); return (int)g_recs.size(); }
