// M0-1 probe DLL implementation. See m0rive.h.
//
// Rive's RenderTargetD3D must match its target texture's size exactly, while
// CompositionDrawingSurface::BeginDraw hands back a slice of a shared atlas plus an
// offset. So Rive draws into a private B8G8R8A8 texture of the surface's size and
// each frame is copied into the atlas slice with CopySubresourceRegion. B8G8R8A8
// is required on both sides: copies cannot cross the RGBA/BGRA format groups.
#define M0RIVE_EXPORTS
#include "m0rive.h"

#include <windows.h>
#include <d3d11_4.h>
#include <dxgi1_3.h>
#include <dwmapi.h>
#include <wrl/client.h>

#include <algorithm>
#include <atomic>
#include <chrono>
#include <cstdio>
#include <memory>
#include <mutex>
#include <thread>
#include <vector>

#include "rive/file.hpp"
#include "rive/artboard.hpp"
#include "rive/scene.hpp"
#include "rive/animation/state_machine_instance.hpp"
#include "rive/animation/linear_animation_instance.hpp"
#include "rive/renderer/rive_renderer.hpp"
#include "rive/renderer/texture.hpp"
#include "rive/renderer/d3d11/render_context_d3d_impl.hpp"

using Microsoft::WRL::ComPtr;

#ifndef E_NOT_VALID_STATE  // MinGW headers lack it; same value as winerror.h
#define E_NOT_VALID_STATE HRESULT_FROM_WIN32(ERROR_INVALID_STATE)
#endif

// Microsoft.UI.Composition's drawing-surface interop, declared here so the DLL
// builds without the Windows App SDK headers. IID and vtable order are copied from
// Microsoft.UI.Composition.Interop.h in Microsoft.WindowsAppSDK.InteractiveExperiences
// 2.1.9 (pulled in by Microsoft.WindowsAppSDK 2.5.1). They differ from the system
// Windows.UI.Composition interop IIDs; the probe records the QueryInterface HRESULT.
MIDL_INTERFACE("2D6355C2-AD57-4EAE-92E4-4C3EFF65D578")
ICompositionDrawingSurfaceInteropM0 : public IUnknown {
    virtual HRESULT STDMETHODCALLTYPE BeginDraw(const RECT* updateRect, REFIID iid, void** updateObject, POINT* updateOffset) = 0;
    virtual HRESULT STDMETHODCALLTYPE EndDraw() = 0;
    virtual HRESULT STDMETHODCALLTYPE Resize(SIZE sizePixels) = 0;
    virtual HRESULT STDMETHODCALLTYPE Scroll(const RECT* scrollRect, const RECT* clipRect, int offsetX, int offsetY) = 0;
    virtual HRESULT STDMETHODCALLTYPE ResumeDraw() = 0;
    virtual HRESULT STDMETHODCALLTYPE SuspendDraw() = 0;
};
static const IID kIidSurfaceInterop = {0x2D6355C2, 0xAD57, 0x4EAE, {0x92, 0xE4, 0x4C, 0x3E, 0xFF, 0x65, 0xD5, 0x78}};

namespace {

double Pct(std::vector<double> v, double p) {  // nearest rank
    if (v.empty()) return 0;
    std::sort(v.begin(), v.end());
    size_t i = static_cast<size_t>(p * static_cast<double>(v.size()) + 0.999999);
    return v[std::min(v.size(), std::max<size_t>(i, 1)) - 1];
}

double NowMs() {
    using namespace std::chrono;
    return duration<double, std::milli>(steady_clock::now().time_since_epoch()).count();
}

struct Probe {
    uint32_t w = 0, h = 0;
    ComPtr<ID3D11Device> dev;
    ComPtr<ID3D11DeviceContext> ctx;
    std::unique_ptr<rive::gpu::RenderContext> rc;
    rive::rcp<rive::gpu::RenderTargetD3D> rt;
    ComPtr<ID3D11Texture2D> offscreen;
    bool offscreenUav = false;
    rive::rcp<rive::File> file;
    std::unique_ptr<rive::ArtboardInstance> artboard;
    std::unique_ptr<rive::Scene> scene;
    ComPtr<ID3D11DeviceContext1> ctx1;
    ComPtr<ID3D11RenderTargetView> markerRtv;
    D3D11_RECT marker{};  // guarded by statsMu; right == left means off
    float markerColor[4] = {0, 0, 0, 1};
    ComPtr<ICompositionDrawingSurfaceInteropM0> surface;
    HRESULT interopQiHr = E_PENDING;
    ComPtr<IDXGISwapChain1> swapchain;

    // GPU timing: a small ring of disjoint + begin/end timestamp queries.
    static constexpr int kQ = 4;
    ComPtr<ID3D11Query> qDisjoint[kQ], qBegin[kQ], qEnd[kQ];
    uint64_t qIssued = 0, qRead = 0;

    std::thread thread;
    std::atomic<bool> running{false};
    std::mutex statsMu;
    std::vector<double> cpuMs, gpuMs, intervalMs;
    uint64_t frames = 0, drawFailures = 0;
    HRESULT lastHr = S_OK;
};

Probe* g = nullptr;

HRESULT MakeOffscreen(Probe& p) {
    D3D11_TEXTURE2D_DESC d{};
    d.Width = p.w;
    d.Height = p.h;
    d.MipLevels = 1;
    d.ArraySize = 1;
    d.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    d.SampleDesc.Count = 1;
    d.Usage = D3D11_USAGE_DEFAULT;
    d.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_UNORDERED_ACCESS;
    HRESULT hr = p.dev->CreateTexture2D(&d, nullptr, &p.offscreen);
    p.offscreenUav = SUCCEEDED(hr);
    if (FAILED(hr)) {  // BGRA typed UAVs are optional hardware support.
        d.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
        hr = p.dev->CreateTexture2D(&d, nullptr, &p.offscreen);
    }
    if (SUCCEEDED(hr)) {
        p.rt = p.rc->static_impl_cast<rive::gpu::RenderContextD3DImpl>()->makeRenderTarget(p.w, p.h);
        p.rt->setTargetTexture(p.offscreen);
    }
    return hr;
}

void MakeQueries(Probe& p) {
    D3D11_QUERY_DESC dj{D3D11_QUERY_TIMESTAMP_DISJOINT, 0}, ts{D3D11_QUERY_TIMESTAMP, 0};
    for (int i = 0; i < Probe::kQ; ++i) {
        p.dev->CreateQuery(&dj, &p.qDisjoint[i]);
        p.dev->CreateQuery(&ts, &p.qBegin[i]);
        p.dev->CreateQuery(&ts, &p.qEnd[i]);
    }
}

// Reads every finished query set without stalling; returns GPU ms values.
void DrainQueries(Probe& p, std::vector<double>& out) {
    while (p.qRead < p.qIssued) {
        const int i = static_cast<int>(p.qRead % Probe::kQ);
        D3D11_QUERY_DATA_TIMESTAMP_DISJOINT dj{};
        UINT64 b = 0, e = 0;
        if (p.ctx->GetData(p.qDisjoint[i].Get(), &dj, sizeof dj, D3D11_ASYNC_GETDATA_DONOTFLUSH) != S_OK) {
            if (p.qIssued - p.qRead < Probe::kQ) return;  // not ready yet, ring has room
            p.ctx->GetData(p.qDisjoint[i].Get(), &dj, sizeof dj, 0);  // ring full: wait
        }
        p.ctx->GetData(p.qBegin[i].Get(), &b, sizeof b, 0);
        p.ctx->GetData(p.qEnd[i].Get(), &e, sizeof e, 0);
        if (!dj.Disjoint && dj.Frequency) out.push_back(1000.0 * static_cast<double>(e - b) / static_cast<double>(dj.Frequency));
        ++p.qRead;
    }
}

// Advances the scene by dt and draws it into the offscreen texture.
void RenderRive(Probe& p, float dt) {
    if (p.scene) p.scene->advanceAndApply(dt);
    rive::gpu::RenderContext::FrameDescriptor fd;
    fd.renderTargetWidth = p.w;
    fd.renderTargetHeight = p.h;
    fd.loadAction = rive::gpu::LoadAction::clear;
    fd.clearColor = 0x00000000;
    p.rc->beginFrame(fd);
    {
        rive::RiveRenderer renderer(p.rc.get());
        renderer.save();
        renderer.transform(rive::computeAlignment(rive::Fit::contain, rive::Alignment::center,
                                                  rive::AABB(0, 0, static_cast<float>(p.w), static_cast<float>(p.h)),
                                                  p.artboard->bounds()));
        if (p.scene) p.scene->draw(&renderer);
        else p.artboard->draw(&renderer);
        renderer.restore();
    }
    rive::gpu::RenderContext::FlushResources fr;
    fr.renderTarget = p.rt.get();
    p.rc->flush(fr);

    D3D11_RECT m;
    float c[4];
    {
        std::lock_guard<std::mutex> lk(p.statsMu);
        m = p.marker;
        std::copy(p.markerColor, p.markerColor + 4, c);
    }
    if (m.right > m.left && p.ctx1 && p.markerRtv) p.ctx1->ClearView(p.markerRtv.Get(), c, &m, 1);
}

// One frame to the active target. Returns the BeginDraw/Present HRESULT.
HRESULT Frame(Probe& p, float dt, std::vector<double>& gpuOut) {
    const int qi = static_cast<int>(p.qIssued % Probe::kQ);
    p.ctx->Begin(p.qDisjoint[qi].Get());
    p.ctx->End(p.qBegin[qi].Get());
    RenderRive(p, dt);
    HRESULT hr = S_OK;
    if (p.swapchain) {
        ComPtr<ID3D11Texture2D> bb;
        hr = p.swapchain->GetBuffer(0, IID_PPV_ARGS(&bb));
        if (SUCCEEDED(hr)) p.ctx->CopyResource(bb.Get(), p.offscreen.Get());
    } else if (p.surface) {
        ComPtr<ID3D11Texture2D> atlas;
        POINT off{};
        hr = p.surface->BeginDraw(nullptr, __uuidof(ID3D11Texture2D), reinterpret_cast<void**>(atlas.GetAddressOf()), &off);
        if (SUCCEEDED(hr)) {
            p.ctx->CopySubresourceRegion(atlas.Get(), 0, static_cast<UINT>(off.x), static_cast<UINT>(off.y), 0,
                                         p.offscreen.Get(), 0, nullptr);
        }
    }
    p.ctx->End(p.qEnd[qi].Get());
    p.ctx->End(p.qDisjoint[qi].Get());
    ++p.qIssued;
    if (p.swapchain && SUCCEEDED(hr)) hr = p.swapchain->Present(0, 0);
    else if (p.surface && SUCCEEDED(hr)) hr = p.surface->EndDraw();
    DrainQueries(p, gpuOut);
    return hr;
}

void RenderLoop(Probe* p) {
    double last = NowMs();
    while (p->running.load()) {
        DwmFlush();  // one iteration per DWM composition (verified ~vsync on DESKTOP-K93N3P8)
        const double t0 = NowMs();
        std::vector<double> gpu;
        const HRESULT hr = Frame(*p, static_cast<float>((t0 - last) / 1000.0), gpu);
        const double t1 = NowMs();
        std::lock_guard<std::mutex> lk(p->statsMu);
        p->intervalMs.push_back(t0 - last);
        p->cpuMs.push_back(t1 - t0);
        p->gpuMs.insert(p->gpuMs.end(), gpu.begin(), gpu.end());
        p->lastHr = hr;
        if (FAILED(hr)) ++p->drawFailures; else ++p->frames;
        last = t0;
    }
}

}  // namespace

M0R_API int32_t m0r_create(const wchar_t* riv_path, uint32_t width, uint32_t height, void** out_device) {
    if (g || !riv_path || !out_device || !width || !height) return E_INVALIDARG;
    auto p = std::make_unique<Probe>();
    p->w = width;
    p->h = height;
    // Rive's D3D11 backend requires feature level 11_1; BGRA support is required
    // for a device that backs a CompositionGraphicsDevice.
    D3D_FEATURE_LEVEL levels[] = {D3D_FEATURE_LEVEL_11_1};
    HRESULT hr = D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                                   levels, 1, D3D11_SDK_VERSION, &p->dev, nullptr, &p->ctx);
    if (FAILED(hr)) return hr;
    // The composition graphics device uses the device from its own thread too.
    ComPtr<ID3D11Multithread> mt;
    if (SUCCEEDED(p->ctx.As(&mt))) mt->SetMultithreadProtected(TRUE);

    ComPtr<IDXGIDevice> dxgiDev;
    ComPtr<IDXGIAdapter> adapter;
    DXGI_ADAPTER_DESC ad{};
    if (SUCCEEDED(p->dev.As(&dxgiDev)) && SUCCEEDED(dxgiDev->GetAdapter(&adapter))) adapter->GetDesc(&ad);
    rive::gpu::D3DContextOptions opts;
    opts.isIntel = ad.VendorId == 0x163C || ad.VendorId == 0x8086 || ad.VendorId == 0x8087;
    p->rc = rive::gpu::RenderContextD3DImpl::MakeContext(p->dev, p->ctx, opts);
    if (!p->rc) return E_FAIL;

    FILE* f = _wfopen(riv_path, L"rb");
    if (!f) return HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
    std::vector<uint8_t> bytes;
    uint8_t buf[65536];
    for (size_t n; (n = std::fread(buf, 1, sizeof buf, f)) > 0;) bytes.insert(bytes.end(), buf, buf + n);
    std::fclose(f);
    p->file = rive::File::import(rive::Span<const uint8_t>(bytes.data(), bytes.size()), p->rc.get());
    if (!p->file) return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
    p->artboard = p->file->artboardDefault();
    if (!p->artboard) return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
    if (p->artboard->stateMachineCount() > 0) p->scene = p->artboard->defaultStateMachine();
    if (!p->scene && p->artboard->animationCount() > 0) p->scene = p->artboard->animationAt(0);

    hr = MakeOffscreen(*p);
    if (FAILED(hr)) return hr;
    p->ctx.As(&p->ctx1);
    p->dev->CreateRenderTargetView(p->offscreen.Get(), nullptr, &p->markerRtv);
    MakeQueries(*p);
    p->dev.CopyTo(reinterpret_cast<ID3D11Device**>(out_device));
    g = p.release();
    return S_OK;
}

M0R_API int32_t m0r_attach_surface(void* surface_unknown) {
    if (!g || !surface_unknown) return E_INVALIDARG;
    g->interopQiHr = static_cast<IUnknown*>(surface_unknown)->QueryInterface(kIidSurfaceInterop, reinterpret_cast<void**>(g->surface.ReleaseAndGetAddressOf()));
    return g->interopQiHr;
}

M0R_API int32_t m0r_create_swapchain(void** out_swapchain) {
    if (!g || !out_swapchain) return E_INVALIDARG;
    ComPtr<IDXGIDevice> dxgiDev;
    ComPtr<IDXGIAdapter> adapter;
    ComPtr<IDXGIFactory2> factory;
    HRESULT hr = g->dev.As(&dxgiDev);
    if (SUCCEEDED(hr)) hr = dxgiDev->GetAdapter(&adapter);
    if (SUCCEEDED(hr)) hr = adapter->GetParent(IID_PPV_ARGS(&factory));
    if (FAILED(hr)) return hr;
    DXGI_SWAP_CHAIN_DESC1 d{};
    d.Width = g->w;
    d.Height = g->h;
    d.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    d.SampleDesc.Count = 1;
    d.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    d.BufferCount = 2;
    d.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    d.AlphaMode = DXGI_ALPHA_MODE_PREMULTIPLIED;
    hr = factory->CreateSwapChainForComposition(g->dev.Get(), &d, nullptr, &g->swapchain);
    if (SUCCEEDED(hr)) g->swapchain.CopyTo(reinterpret_cast<IDXGISwapChain1**>(out_swapchain));
    return hr;
}

M0R_API int32_t m0r_start(void) {
    if (!g || (!g->surface && !g->swapchain)) return E_NOT_VALID_STATE;
    if (g->running.exchange(true)) return S_FALSE;
    g->thread = std::thread(RenderLoop, g);
    return S_OK;
}

M0R_API int32_t m0r_set_marker(uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t red, uint8_t green, uint8_t blue) {
    if (!g) return E_NOT_VALID_STATE;
    if (!g->ctx1 || !g->markerRtv) return E_NOTIMPL;
    std::lock_guard<std::mutex> lk(g->statsMu);
    g->marker = {static_cast<LONG>(x), static_cast<LONG>(y), static_cast<LONG>(x + w), static_cast<LONG>(y + h)};
    g->markerColor[0] = red / 255.f;
    g->markerColor[1] = green / 255.f;
    g->markerColor[2] = blue / 255.f;
    return S_OK;
}

M0R_API int32_t m0r_stop(void) {
    if (!g) return E_NOT_VALID_STATE;
    if (g->running.exchange(false) && g->thread.joinable()) g->thread.join();
    return S_OK;
}

M0R_API int32_t m0r_get_stats(m0r_stats* out) {
    if (!g || !out) return E_INVALIDARG;
    std::lock_guard<std::mutex> lk(g->statsMu);
    *out = {};
    out->frames = g->frames;
    out->begin_draw_failures = g->drawFailures;
    out->cpu_p50_ms = Pct(g->cpuMs, 0.50);
    out->cpu_p95_ms = Pct(g->cpuMs, 0.95);
    out->cpu_max_ms = g->cpuMs.empty() ? 0 : *std::max_element(g->cpuMs.begin(), g->cpuMs.end());
    out->gpu_p50_ms = Pct(g->gpuMs, 0.50);
    out->gpu_p95_ms = Pct(g->gpuMs, 0.95);
    out->interval_p50_ms = Pct(g->intervalMs, 0.50);
    out->interval_p95_ms = Pct(g->intervalMs, 0.95);
    out->last_hr = g->lastHr;
    out->interop_qi_hr = g->interopQiHr;
    out->offscreen_has_uav = g->offscreenUav ? 1 : 0;
    out->device_removed_hr = g->dev->GetDeviceRemovedReason();
    return S_OK;
}

M0R_API int32_t m0r_reset_stats(void) {
    if (!g) return E_NOT_VALID_STATE;
    std::lock_guard<std::mutex> lk(g->statsMu);
    g->cpuMs.clear();
    g->gpuMs.clear();
    g->intervalMs.clear();
    g->frames = 0;
    g->drawFailures = 0;
    return S_OK;
}

M0R_API int32_t m0r_write_reference(const wchar_t* path, double t_sec) {
    if (!g || !path) return E_INVALIDARG;
    if (g->running.load()) return E_NOT_VALID_STATE;  // the immediate context is single-threaded here
    D3D11_TEXTURE2D_DESC d{};
    g->offscreen->GetDesc(&d);
    d.Usage = D3D11_USAGE_STAGING;
    d.BindFlags = 0;
    d.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
    ComPtr<ID3D11Texture2D> staging;
    HRESULT hr = g->dev->CreateTexture2D(&d, nullptr, &staging);
    if (FAILED(hr)) return hr;
    // Rive compiles shaders asynchronously by default and skips draws whose
    // pipeline is not ready, so the first frames can come out empty (M0-A: all
    // three references were blank). Render at t_sec, then re-render the same
    // instant (dt = 0) until some pixel is non-transparent, for up to ~2 s.
    D3D11_MAPPED_SUBRESOURCE m{};
    bool drawn = false;
    for (int attempt = 0; attempt < 120 && !drawn; ++attempt) {
        if (attempt > 0) Sleep(16);
        RenderRive(*g, attempt == 0 ? static_cast<float>(t_sec) : 0.f);
        g->ctx->CopyResource(staging.Get(), g->offscreen.Get());
        hr = g->ctx->Map(staging.Get(), 0, D3D11_MAP_READ, 0, &m);
        if (FAILED(hr)) return hr;
        for (uint32_t y = 0; y < g->h && !drawn; ++y) {
            const uint8_t* row = static_cast<uint8_t*>(m.pData) + y * m.RowPitch;
            for (uint32_t x = 0; x < g->w; ++x)
                if (row[4 * x + 3]) { drawn = true; break; }
        }
        if (!drawn) g->ctx->Unmap(staging.Get(), 0);
    }
    if (!drawn) return S_FALSE;  // nothing drawn after the retries; no file written
    FILE* f = _wfopen(path, L"wb");
    if (f) {
        const uint32_t hdr[2] = {g->w, g->h};
        std::fwrite(hdr, sizeof hdr, 1, f);
        for (uint32_t y = 0; y < g->h; ++y) std::fwrite(static_cast<uint8_t*>(m.pData) + y * m.RowPitch, 4, g->w, f);
        std::fclose(f);
    }
    g->ctx->Unmap(staging.Get(), 0);
    return f ? S_OK : HRESULT_FROM_WIN32(ERROR_CANNOT_MAKE);
}

M0R_API void m0r_destroy(void) {
    if (!g) return;
    m0r_stop();
    delete g;
    g = nullptr;
}
