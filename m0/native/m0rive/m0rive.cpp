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
#include <csignal>
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

// Same header: ICompositionGraphicsDeviceInterop, used to hand a new D3D11 device
// to the existing CompositionGraphicsDevice after a device loss (M3-RIVE-LOST).
MIDL_INTERFACE("4AFA8030-BC70-4B0C-B1C7-6E69F933DC83")
ICompositionGraphicsDeviceInteropM0 : public IUnknown {
    virtual HRESULT STDMETHODCALLTYPE GetRenderingDevice(IUnknown** value) = 0;
    virtual HRESULT STDMETHODCALLTYPE SetRenderingDevice(IUnknown* value) = 0;
};
static const IID kIidGraphicsDeviceInterop = {0x4AFA8030, 0xBC70, 0x4B0C, {0xB1, 0xC7, 0x6E, 0x69, 0xF9, 0x33, 0xDC, 0x83}};

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
    ComPtr<ICompositionGraphicsDeviceInteropM0> graphicsDevice;
    std::vector<uint8_t> rivBytes;  // kept so the file can be re-imported on a new device
    ComPtr<IDXGISwapChain1> swapchain;

    // GPU timing: a small ring of disjoint + begin/end timestamp queries.
    static constexpr int kQ = 4;
    ComPtr<ID3D11Query> qDisjoint[kQ], qBegin[kQ], qEnd[kQ];
    uint64_t qIssued = 0, qRead = 0;
    std::atomic<uint64_t> gpuDropped{0};  // query sets that never became valid (render thread writes)

    std::thread thread;
    std::atomic<bool> running{false};
    std::mutex statsMu;
    std::vector<double> cpuMs, gpuMs, intervalMs;
    uint64_t frames = 0, drawFailures = 0;
    HRESULT lastHr = S_OK;

    // M3-RIVE-DPI: a resize requested by the UI thread is applied by the render
    // thread at the start of its next frame (resizeMu guards the request).
    std::mutex resizeMu;
    bool resizePending = false;
    uint32_t pendW = 0, pendH = 0;
    double resizeReqMs = 0;
    std::vector<double> rerasterMs;  // request -> first frame at the new size (statsMu)
    uint32_t appliedW = 0, appliedH = 0;
    HRESULT lastResizeHr = S_OK;

    // M3-RIVE-LOST: device-loss detection and recovery on the render thread.
    std::atomic<bool> simLost{false};
    std::atomic<bool> lostNow{false};
    std::atomic<uint32_t> sehCaught{0};
    uint32_t lostDetected = 0;            // statsMu
    std::vector<m0r_lost_event> lost;     // completed recoveries (statsMu)
};

Probe* g = nullptr;

// Hardware device at feature level 11_1 (Rive's D3D11 backend requires it) with
// BGRA support (required for a device behind a CompositionGraphicsDevice).
HRESULT MakeDevice(ComPtr<ID3D11Device>& dev, ComPtr<ID3D11DeviceContext>& ctx) {
    D3D_FEATURE_LEVEL levels[] = {D3D_FEATURE_LEVEL_11_1};
    HRESULT hr = D3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                                   levels, 1, D3D11_SDK_VERSION, &dev, nullptr, &ctx);
    if (FAILED(hr)) return hr;
    // The composition graphics device uses the device from its own thread too.
    ComPtr<ID3D11Multithread> mt;
    if (SUCCEEDED(ctx.As(&mt))) mt->SetMultithreadProtected(TRUE);
    return S_OK;
}

// Rive render context on p.dev, then imports p.rivBytes (default artboard, default
// state machine, else animation 0). The scene starts from t = 0 (K6).
HRESULT MakeRive(Probe& p) {
    ComPtr<IDXGIDevice> dxgiDev;
    ComPtr<IDXGIAdapter> adapter;
    DXGI_ADAPTER_DESC ad{};
    if (SUCCEEDED(p.dev.As(&dxgiDev)) && SUCCEEDED(dxgiDev->GetAdapter(&adapter))) adapter->GetDesc(&ad);
    rive::gpu::D3DContextOptions opts;
    opts.isIntel = ad.VendorId == 0x163C || ad.VendorId == 0x8086 || ad.VendorId == 0x8087;
    p.rc = rive::gpu::RenderContextD3DImpl::MakeContext(p.dev, p.ctx, opts);
    if (!p.rc) return E_FAIL;
    p.file = rive::File::import(rive::Span<const uint8_t>(p.rivBytes.data(), p.rivBytes.size()), p.rc.get());
    if (!p.file) return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
    p.artboard = p.file->artboardDefault();
    if (!p.artboard) return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
    if (p.artboard->stateMachineCount() > 0) p.scene = p.artboard->defaultStateMachine();
    if (!p.scene && p.artboard->animationCount() > 0) p.scene = p.artboard->animationAt(0);
    return S_OK;
}

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

// Resizes the drawing surface and rebuilds the offscreen texture, the Rive render
// target and the marker RTV at w x h. Render thread only (or while it is stopped).
HRESULT ApplyResize(Probe& p, uint32_t w, uint32_t h) {
    if (p.surface) {
        const HRESULT hr = p.surface->Resize(SIZE{static_cast<LONG>(w), static_cast<LONG>(h)});
        if (FAILED(hr)) return hr;
    }
    p.w = w;
    p.h = h;
    p.markerRtv.Reset();
    p.rt.reset();
    p.offscreen.Reset();
    HRESULT hr = MakeOffscreen(p);
    if (SUCCEEDED(hr)) p.dev->CreateRenderTargetView(p.offscreen.Get(), nullptr, &p.markerRtv);
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

// Reads every finished query set; returns GPU ms values. A set is read only when
// the disjoint query and both timestamps report S_OK: GetData with flags 0 does
// not block, and M0-B (AMD) showed the disjoint result can be ready before the
// timestamps, which left a timestamp at 0 and wrapped e - b. When the ring is full
// the oldest set is waited for by polling. Sets that still fail are dropped and
// counted in gpuDropped.
void DrainQueries(Probe& p, std::vector<double>& out) {
    // On a removed device GetData never returns S_OK; waiting would delay recovery by 2 s.
    if (p.dev->GetDeviceRemovedReason() != S_OK) return;
    while (p.qRead < p.qIssued) {
        const int i = static_cast<int>(p.qRead % Probe::kQ);
        D3D11_QUERY_DATA_TIMESTAMP_DISJOINT dj{};
        UINT64 b = 0, e = 0;
        auto ready = [&] {
            return p.ctx->GetData(p.qDisjoint[i].Get(), &dj, sizeof dj, 0) == S_OK &&
                   p.ctx->GetData(p.qBegin[i].Get(), &b, sizeof b, 0) == S_OK &&
                   p.ctx->GetData(p.qEnd[i].Get(), &e, sizeof e, 0) == S_OK;
        };
        bool ok = ready();
        if (!ok) {
            if (p.qIssued - p.qRead < Probe::kQ) return;  // not ready yet, ring has room
            for (int spin = 0; spin < 2000 && !ok; ++spin) {  // ring full: wait up to ~2 s
                Sleep(1);
                ok = ready();
            }
        }
        if (ok && !dj.Disjoint && dj.Frequency && e >= b)
            out.push_back(1000.0 * static_cast<double>(e - b) / static_cast<double>(dj.Frequency));
        else
            ++p.gpuDropped;
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

// Replaces a lost device: new device -> SetRenderingDevice on the existing
// CompositionGraphicsDevice -> release everything built on the old device -> new
// Rive context, re-imported file, offscreen target, marker RTV and queries at the
// current size. Render thread only. On failure the old objects may already be gone;
// the caller retries.
HRESULT RecoverDevice(Probe& p, uint32_t* oldRefs) {
    ComPtr<ID3D11Device> nd;
    ComPtr<ID3D11DeviceContext> nc;
    HRESULT hr = MakeDevice(nd, nc);
    if (FAILED(hr)) return hr;
    if (p.graphicsDevice) {
        hr = p.graphicsDevice->SetRenderingDevice(nd.Get());
        if (FAILED(hr)) return hr;
    }
    // Rive objects first (the file and the render target hold resources of the
    // context), then our own views, textures and queries, then the context.
    p.scene.reset();
    p.artboard.reset();
    p.file.reset();
    p.rt.reset();
    p.markerRtv.Reset();
    p.offscreen.Reset();
    for (int i = 0; i < Probe::kQ; ++i) {
        p.qDisjoint[i].Reset();
        p.qBegin[i].Reset();
        p.qEnd[i].Reset();
    }
    p.qIssued = p.qRead = 0;
    p.rc.reset();
    p.ctx1.Reset();
    if (p.ctx) {
        p.ctx->ClearState();
        p.ctx->Flush();
    }
    p.ctx.Reset();
    {
        std::lock_guard<std::mutex> lk(p.statsMu);  // m0r_get_stats reads p.dev on the UI thread
        if (ID3D11Device* old = p.dev.Detach()) *oldRefs = old->Release();
        p.dev = nd;
        p.ctx = nc;
    }
    hr = MakeRive(p);
    if (SUCCEEDED(hr)) hr = MakeOffscreen(p);
    if (FAILED(hr)) return hr;
    p.ctx.As(&p.ctx1);
    p.dev->CreateRenderTargetView(p.offscreen.Get(), nullptr, &p.markerRtv);
    MakeQueries(p);
    return S_OK;
}

bool IsLossHr(HRESULT hr) {
    return hr == DXGI_ERROR_DEVICE_REMOVED || hr == DXGI_ERROR_DEVICE_RESET || hr == DXGI_ERROR_DEVICE_HUNG ||
           hr == DXGI_ERROR_DRIVER_INTERNAL_ERROR;
}

#ifdef _MSC_VER
// Rive's D3D11 buffer ring writes through the pointer from an unchecked Map
// (render_context_d3d_impl.cpp onMap at the pinned commit), so a frame that is in
// flight when the device is removed can fault. Only that case is caught: an access
// violation while the device reports removal (or a simulated loss) is turned into a
// loss; anything else still crashes. Objects inside the faulting frame are not
// unwound (/EHsc); the whole context is rebuilt right after.
int LossFilter(DWORD code, Probe* p) {
    if (code != EXCEPTION_ACCESS_VIOLATION) return EXCEPTION_CONTINUE_SEARCH;
    if (p->dev->GetDeviceRemovedReason() == S_OK && !p->simLost.load()) return EXCEPTION_CONTINUE_SEARCH;
    return EXCEPTION_EXECUTE_HANDLER;
}

HRESULT GuardedFrame(Probe* p, float dt, std::vector<double>* gpuOut) {
    __try {
        return Frame(*p, dt, *gpuOut);
    } __except (LossFilter(GetExceptionCode(), p)) {
        ++p->sehCaught;
        return DXGI_ERROR_DEVICE_REMOVED;
    }
}
#else
HRESULT GuardedFrame(Probe* p, float dt, std::vector<double>* gpuOut) { return Frame(*p, dt, *gpuOut); }
#endif

void RenderLoop(Probe* p) {
    double last = NowMs();
    double lastGood = last;      // end of the last frame presented
    bool lost = false, awaitFirstGood = false, forceLost = false;
    m0r_lost_event ev{};
    double lostAt = 0, lastAttempt = -1e9;
    while (p->running.load()) {
        if (FAILED(DwmFlush())) Sleep(15);  // DWM can fail while the GPU resets; don't spin
        const double t0 = NowMs();

        // M3-RIVE-LOST: detect, then retry recovery every >= 50 ms until it succeeds.
        // Nothing is drawn (and no Rive call is made) while the device is lost.
        if (!lost) {
            const HRESULT reason = p->dev->GetDeviceRemovedReason();
            const bool sim = p->simLost.exchange(false);
            if (reason != S_OK || sim || forceLost) {
                lost = true;
                forceLost = false;
                lostAt = t0;
                lastAttempt = -1e9;
                ev = {};
                ev.removed_reason = reason;
                ev.simulated = (sim && reason == S_OK) ? 1 : 0;
                p->sehCaught.store(0);
                p->lostNow.store(true);
                std::lock_guard<std::mutex> lk(p->statsMu);
                ++p->lostDetected;
            }
        }
        if (lost) {
            if (t0 - lastAttempt >= 50) {
                lastAttempt = t0;
                ++ev.attempts;
                ev.recover_hr = RecoverDevice(*p, &ev.old_device_refs);
                if (SUCCEEDED(ev.recover_hr)) {
                    lost = false;
                    awaitFirstGood = true;
                }
            }
            if (lost) {
                ++ev.failures_during;
                last = t0;
                continue;
            }
        }

        bool resized = false;
        double reqMs = 0;
        HRESULT resizeHr = S_OK;
        {
            std::unique_lock<std::mutex> rl(p->resizeMu);
            if (p->resizePending) {
                const uint32_t w = p->pendW, h = p->pendH;
                reqMs = p->resizeReqMs;
                p->resizePending = false;
                rl.unlock();
                resizeHr = ApplyResize(*p, w, h);
                resized = true;
            }
        }
        std::vector<double> gpu;
        // Checked again right before Rive touches the device (see GuardedFrame).
        HRESULT hr = resizeHr;
        if (SUCCEEDED(resizeHr))
            hr = p->dev->GetDeviceRemovedReason() != S_OK ? DXGI_ERROR_DEVICE_REMOVED
                                                          : GuardedFrame(p, static_cast<float>((t0 - last) / 1000.0), &gpu);
        const double t1 = NowMs();
        if (IsLossHr(hr)) forceLost = true;  // picked up at the top of the next iteration
        std::lock_guard<std::mutex> lk(p->statsMu);
        if (resized) {
            p->lastResizeHr = resizeHr;
            if (SUCCEEDED(resizeHr) && SUCCEEDED(hr)) {
                p->rerasterMs.push_back(t1 - reqMs);
                p->appliedW = p->w;
                p->appliedH = p->h;
            }
        }
        if (awaitFirstGood && SUCCEEDED(hr)) {
            ev.recover_ms = t1 - lostAt;
            ev.gap_ms = t1 - lastGood;
            ev.seh_caught = p->sehCaught.load();
            p->lost.push_back(ev);
            awaitFirstGood = false;
            p->lostNow.store(false);
        }
        if (SUCCEEDED(hr)) lastGood = t1;
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
    HRESULT hr = MakeDevice(p->dev, p->ctx);
    if (FAILED(hr)) return hr;

    FILE* f = _wfopen(riv_path, L"rb");
    if (!f) return HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND);
    uint8_t buf[65536];
    for (size_t n; (n = std::fread(buf, 1, sizeof buf, f)) > 0;) p->rivBytes.insert(p->rivBytes.end(), buf, buf + n);
    std::fclose(f);
    hr = MakeRive(*p);
    if (FAILED(hr)) return hr;

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

M0R_API int32_t m0r_resize(uint32_t width, uint32_t height) {
    if (!g || !width || !height) return E_INVALIDARG;
    if (g->swapchain) return E_NOTIMPL;  // M3 F1: only the surface path resizes here
    if (!g->running.load()) {
        const double t0 = NowMs();
        const HRESULT hr = ApplyResize(*g, width, height);
        std::lock_guard<std::mutex> lk(g->statsMu);
        g->lastResizeHr = hr;
        if (SUCCEEDED(hr)) {
            g->rerasterMs.push_back(NowMs() - t0);
            g->appliedW = width;
            g->appliedH = height;
        }
        return hr;
    }
    std::lock_guard<std::mutex> rl(g->resizeMu);
    g->pendW = width;
    g->pendH = height;
    g->resizeReqMs = NowMs();
    g->resizePending = true;
    return S_OK;
}

M0R_API int32_t m0r_get_resize(uint32_t* count, double* last_ms, double* max_ms,
                               uint32_t* width, uint32_t* height, int32_t* last_hr) {
    if (!g || !count || !last_ms || !max_ms || !width || !height || !last_hr) return E_INVALIDARG;
    std::lock_guard<std::mutex> lk(g->statsMu);
    *count = static_cast<uint32_t>(g->rerasterMs.size());
    *last_ms = g->rerasterMs.empty() ? 0 : g->rerasterMs.back();
    *max_ms = g->rerasterMs.empty() ? 0 : *std::max_element(g->rerasterMs.begin(), g->rerasterMs.end());
    *width = g->appliedW;
    *height = g->appliedH;
    *last_hr = g->lastResizeHr;
    return S_OK;
}

M0R_API int32_t m0r_attach_graphics_device(void* graphics_device_unknown) {
    if (!g || !graphics_device_unknown) return E_INVALIDARG;
    if (g->running.load()) return E_NOT_VALID_STATE;
    return static_cast<IUnknown*>(graphics_device_unknown)->QueryInterface(
        kIidGraphicsDeviceInterop, reinterpret_cast<void**>(g->graphicsDevice.ReleaseAndGetAddressOf()));
}

M0R_API int32_t m0r_get_lost(uint32_t* detected, uint32_t* recovered, int32_t* lost_now,
                             m0r_lost_event* events, uint32_t capacity) {
    if (!g || !detected || !recovered || !lost_now || (capacity && !events)) return E_INVALIDARG;
    std::lock_guard<std::mutex> lk(g->statsMu);
    *detected = g->lostDetected;
    *recovered = static_cast<uint32_t>(g->lost.size());
    *lost_now = g->lostNow.load() ? 1 : 0;
    const size_t n = std::min<size_t>(capacity, g->lost.size());
    std::copy(g->lost.begin(), g->lost.begin() + static_cast<std::ptrdiff_t>(n), events);
    return S_OK;
}

M0R_API int32_t m0r_simulate_lost(void) {
    if (!g) return E_NOT_VALID_STATE;
    if (g->swapchain) return E_NOTIMPL;  // M3 F1: surface path only
    if (!g->running.load()) return E_NOT_VALID_STATE;
    g->simLost.store(true);
    return S_OK;
}

namespace {
wchar_t g_abortNote[MAX_PATH] = {};
extern "C" void OnAbort(int) {
    if (g_abortNote[0]) {
        if (FILE* f = _wfopen(g_abortNote, L"ab")) {
            std::fputs("rive_abort=1\nverdict=FAIL_ABORT\n", f);
            std::fclose(f);
        }
    }
}
}  // namespace

M0R_API int32_t m0r_set_abort_note(const wchar_t* path) {
    if (!path || wcslen(path) >= MAX_PATH) return E_INVALIDARG;
    wcscpy(g_abortNote, path);
    std::signal(SIGABRT, OnAbort);
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
    out->gpu_samples_dropped = g->gpuDropped.load();
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
    g->gpuDropped.store(0);
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
