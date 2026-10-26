// M0-4 probe: does a composition swapchain accept DXGI ALLOW_TEARING, and what
// present cadence does it reach? Pure Win32 + D3D11 + DXGI + DirectComposition,
// so the DXGI-level answer does not depend on the WinUI host. An HWND flip-model
// swapchain runs the same steps as the control arm.
//
// Usage:  m0_tear_probe.exe [out_dir] [frames]
// Writes  <out_dir>/m0_tear_summary.txt (key=value) and m0_tear_frames.csv.
// Run PresentMon alongside (see m0/README.md): the PresentMode column is the only
// direct evidence of whether a present was eligible to tear (independent flip).
//
// Build (MSVC):  cl /std:c++17 /EHsc /O2 m0_tear_probe.cpp /link d3d11.lib dxgi.lib dcomp.lib user32.lib
// Build (MinGW): x86_64-w64-mingw32-g++ -std=c++17 -O2 m0_tear_probe.cpp -o m0_tear_probe.exe
//                -ld3d11 -ldxgi -ldcomp -luser32 -static
#ifndef UNICODE
#define UNICODE
#endif
#include <windows.h>
#include <d3d11.h>
#include <dxgi1_6.h>
#include <dcomp.h>
#include <wrl/client.h>

#include <algorithm>
#include <cstdio>
#include <cstdlib>
#include <string>
#include <vector>

using Microsoft::WRL::ComPtr;

namespace {

FILE* g_sum = nullptr;

void Kv(const char* k, const std::string& v) {
    std::fprintf(g_sum, "%s=%s\n", k, v.c_str());
    std::fflush(g_sum);
    std::printf("%s=%s\n", k, v.c_str());
}
std::string Hr(HRESULT hr) {
    char b[16];
    std::snprintf(b, sizeof b, "0x%08lX", static_cast<unsigned long>(hr));
    return b;
}
std::string Num(double v) {
    char b[32];
    std::snprintf(b, sizeof b, "%.3f", v);
    return b;
}
std::string Narrow(const wchar_t* w) {
    std::string s;
    for (; *w; ++w) s += (*w < 128) ? static_cast<char>(*w) : '?';
    return s;
}

double QpcMs() {
    static LARGE_INTEGER f = [] { LARGE_INTEGER x; QueryPerformanceFrequency(&x); return x; }();
    LARGE_INTEGER c;
    QueryPerformanceCounter(&c);
    return 1000.0 * static_cast<double>(c.QuadPart) / static_cast<double>(f.QuadPart);
}

double Percentile(std::vector<double> v, double p) {  // nearest rank
    if (v.empty()) return 0;
    std::sort(v.begin(), v.end());
    size_t i = static_cast<size_t>(p * static_cast<double>(v.size()) + 0.999999);
    return v[std::min(v.size(), std::max<size_t>(i, 1)) - 1];
}

HWND MakeWindow(const wchar_t* title, DWORD exStyle) {
    static bool registered = false;
    if (!registered) {
        WNDCLASSW wc{};
        wc.lpfnWndProc = DefWindowProcW;
        wc.hInstance = GetModuleHandleW(nullptr);
        wc.lpszClassName = L"M0TearProbe";
        wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
        RegisterClassW(&wc);
        registered = true;
    }
    HWND h = CreateWindowExW(exStyle, L"M0TearProbe", title, WS_OVERLAPPEDWINDOW | WS_VISIBLE,
                             100, 100, 800, 600, nullptr, nullptr, GetModuleHandleW(nullptr), nullptr);
    return h;
}

void Pump() {
    MSG m;
    while (PeekMessageW(&m, nullptr, 0, 0, PM_REMOVE)) { TranslateMessage(&m); DispatchMessageW(&m); }
}

struct ArmResult {
    HRESULT createHr = E_FAIL;
    bool tearingFlagOnSwapchain = false;
    HRESULT firstPresentHr = E_FAIL;
    int presentFailures = 0;
    std::vector<double> intervals;  // ms between successive Present returns
    UINT statsPresentCount = 0, statsRefreshCount = 0;
    HRESULT statsHr = E_FAIL;
};

// Presents `frames` times with sync interval 0. With `tearing`, the swapchain is
// created with ALLOW_TEARING and every Present passes DXGI_PRESENT_ALLOW_TEARING.
void RunPresents(IDXGISwapChain1* sc, ID3D11Device* dev, ID3D11DeviceContext* ctx, bool tearing,
                 UINT syncInterval, int frames, const char* arm, FILE* csv, ArmResult& r) {
    ComPtr<ID3D11Texture2D> bb;
    sc->GetBuffer(0, IID_PPV_ARGS(&bb));
    ComPtr<ID3D11RenderTargetView> rtv;
    dev->CreateRenderTargetView(bb.Get(), nullptr, &rtv);
    double last = QpcMs();
    for (int i = 0; i < frames; ++i) {
        Pump();
        const float c[4] = {(i & 1) ? 1.f : 0.f, 0.2f, (i & 1) ? 0.f : 1.f, 1.f};
        // Flip-discard invalidates the RTV binding after Present; rebind each frame.
        if (i > 0) { rtv.Reset(); bb.Reset(); sc->GetBuffer(0, IID_PPV_ARGS(&bb)); dev->CreateRenderTargetView(bb.Get(), nullptr, &rtv); }
        ctx->ClearRenderTargetView(rtv.Get(), c);
        const UINT flags = (tearing && syncInterval == 0) ? DXGI_PRESENT_ALLOW_TEARING : 0;
        const HRESULT hr = sc->Present(syncInterval, flags);
        const double now = QpcMs();
        if (i == 0) r.firstPresentHr = hr;
        if (FAILED(hr)) ++r.presentFailures;
        if (i > 0) r.intervals.push_back(now - last);
        std::fprintf(csv, "%s,%d,%.4f,%s\n", arm, i, now, Hr(hr).c_str());
        last = now;
    }
    DXGI_FRAME_STATISTICS st{};
    r.statsHr = sc->GetFrameStatistics(&st);
    r.statsPresentCount = st.PresentCount;
    r.statsRefreshCount = st.SyncRefreshCount;
}

void Report(const char* arm, const ArmResult& r) {
    const std::string p = std::string(arm) + ".";
    Kv((p + "create_hr").c_str(), Hr(r.createHr));
    Kv((p + "swapchain_tearing_flag").c_str(), r.tearingFlagOnSwapchain ? "1" : "0");
    Kv((p + "first_present_hr").c_str(), Hr(r.firstPresentHr));
    Kv((p + "present_failures").c_str(), std::to_string(r.presentFailures));
    Kv((p + "present_interval_p50_ms").c_str(), Num(Percentile(r.intervals, 0.50)));
    Kv((p + "present_interval_p95_ms").c_str(), Num(Percentile(r.intervals, 0.95)));
    double sum = 0;
    for (double v : r.intervals) sum += v;
    Kv((p + "presents_per_s").c_str(), Num(r.intervals.empty() ? 0 : 1000.0 * r.intervals.size() / sum));
    Kv((p + "frame_stats_hr").c_str(), Hr(r.statsHr));
    Kv((p + "frame_stats_present_count").c_str(), std::to_string(r.statsPresentCount));
    Kv((p + "frame_stats_sync_refresh_count").c_str(), std::to_string(r.statsRefreshCount));
}

DXGI_SWAP_CHAIN_DESC1 Desc(bool tearing) {
    DXGI_SWAP_CHAIN_DESC1 d{};
    d.Width = 800;
    d.Height = 600;
    d.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    d.SampleDesc.Count = 1;
    d.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    d.BufferCount = 2;
    d.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    d.AlphaMode = DXGI_ALPHA_MODE_IGNORE;
    d.Flags = tearing ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;
    return d;
}

}  // namespace

int main(int argc, char** argv) {
    const std::string outDir = argc > 1 ? argv[1] : ".";
    const int frames = argc > 2 ? std::max(60, std::atoi(argv[2])) : 1200;
    SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
    g_sum = std::fopen((outDir + "/m0_tear_summary.txt").c_str(), "w");
    FILE* csv = std::fopen((outDir + "/m0_tear_frames.csv").c_str(), "w");
    if (!g_sum || !csv) { std::fprintf(stderr, "cannot open outputs in %s\n", outDir.c_str()); return 2; }
    std::fprintf(csv, "arm,frame,qpc_ms,present_hr\n");
    Kv("probe", "M0-4 m0_tear_probe v1");
    Kv("frames_per_arm", std::to_string(frames));

    // Environment.
    OSVERSIONINFOW os{};
    os.dwOSVersionInfoSize = sizeof os;
    using RtlGetVersionFn = LONG(WINAPI*)(OSVERSIONINFOW*);
    if (auto f = reinterpret_cast<RtlGetVersionFn>(
            reinterpret_cast<void*>(GetProcAddress(GetModuleHandleW(L"ntdll.dll"), "RtlGetVersion"))))
        f(&os);
    Kv("os_build", std::to_string(os.dwMajorVersion) + "." + std::to_string(os.dwMinorVersion) + "." +
                       std::to_string(os.dwBuildNumber));
    DEVMODEW dm{};
    dm.dmSize = sizeof dm;
    EnumDisplaySettingsW(nullptr, ENUM_CURRENT_SETTINGS, &dm);
    Kv("primary_refresh_hz", std::to_string(dm.dmDisplayFrequency));
    Kv("primary_mode", std::to_string(dm.dmPelsWidth) + "x" + std::to_string(dm.dmPelsHeight));

    ComPtr<IDXGIFactory5> f5;
    HRESULT hr = CreateDXGIFactory2(0, IID_PPV_ARGS(&f5));
    Kv("factory5_hr", Hr(hr));
    BOOL allow = FALSE;
    if (f5) Kv("check_feature_allow_tearing_hr", Hr(f5->CheckFeatureSupport(DXGI_FEATURE_PRESENT_ALLOW_TEARING, &allow, sizeof allow)));
    Kv("check_feature_allow_tearing", allow ? "1" : "0");

    ComPtr<IDXGIAdapter1> adapter;
    if (f5 && SUCCEEDED(f5->EnumAdapters1(0, &adapter))) {
        DXGI_ADAPTER_DESC1 ad{};
        adapter->GetDesc1(&ad);
        Kv("adapter", Narrow(ad.Description));
    }
    ComPtr<ID3D11Device> dev;
    ComPtr<ID3D11DeviceContext> ctx;
    D3D_FEATURE_LEVEL fl{};
    hr = D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                           nullptr, 0, D3D11_SDK_VERSION, &dev, &fl, &ctx);
    Kv("d3d11_create_hr", Hr(hr));
    Kv("d3d11_feature_level", Hr(fl));
    if (FAILED(hr)) { Kv("verdict", "INVALID_NO_DEVICE"); return 1; }
    ComPtr<IDXGIDevice> dxgiDev;
    dev.As(&dxgiDev);

    // Arm C: composition swapchain hosted by a DirectComposition visual.
    for (int pass = 0; pass < 2; ++pass) {
        const bool tearing = pass == 0;
        const char* arm = tearing ? "comp_tear" : "comp_sync0";
        ArmResult r;
        HWND w = MakeWindow(L"M0 tear probe (composition)", WS_EX_NOREDIRECTIONBITMAP);
        ComPtr<IDXGISwapChain1> sc;
        DXGI_SWAP_CHAIN_DESC1 d = Desc(tearing);
        r.createHr = f5->CreateSwapChainForComposition(dev.Get(), &d, nullptr, &sc);
        r.tearingFlagOnSwapchain = tearing && SUCCEEDED(r.createHr);
        if (SUCCEEDED(r.createHr)) {
            ComPtr<IDCompositionDevice> dc;
            ComPtr<IDCompositionTarget> target;
            ComPtr<IDCompositionVisual> visual;
            HRESULT h1 = DCompositionCreateDevice(dxgiDev.Get(), IID_PPV_ARGS(&dc));
            HRESULT h2 = dc ? dc->CreateTargetForHwnd(w, TRUE, &target) : E_FAIL;
            HRESULT h3 = dc ? dc->CreateVisual(&visual) : E_FAIL;
            HRESULT h4 = visual ? visual->SetContent(sc.Get()) : E_FAIL;
            HRESULT h5 = target ? target->SetRoot(visual.Get()) : E_FAIL;
            HRESULT h6 = dc ? dc->Commit() : E_FAIL;
            Kv((std::string(arm) + ".dcomp_hr").c_str(),
               Hr(h1) + "/" + Hr(h2) + "/" + Hr(h3) + "/" + Hr(h4) + "/" + Hr(h5) + "/" + Hr(h6));
            RunPresents(sc.Get(), dev.Get(), ctx.Get(), tearing, 0, frames, arm, csv, r);
        }
        Report(arm, r);
        DestroyWindow(w);
    }

    // Arm H: HWND flip-model swapchain (control: tearing is documented to work here
    // when the driver supports it and the window qualifies for independent flip).
    {
        ArmResult r;
        HWND w = MakeWindow(L"M0 tear probe (hwnd)", 0);
        ComPtr<IDXGISwapChain1> sc;
        DXGI_SWAP_CHAIN_DESC1 d = Desc(true);
        r.createHr = f5->CreateSwapChainForHwnd(dev.Get(), w, &d, nullptr, nullptr, &sc);
        r.tearingFlagOnSwapchain = SUCCEEDED(r.createHr);
        if (SUCCEEDED(r.createHr)) RunPresents(sc.Get(), dev.Get(), ctx.Get(), true, 0, frames, "hwnd_tear", csv, r);
        Report("hwnd_tear", r);
        DestroyWindow(w);
    }

    // The probe records; it does not judge whether tearing happened on screen.
    Kv("verdict", "RECORDED");
    std::fclose(csv);
    std::fclose(g_sum);
    return 0;
}
