// M0-1 probe DLL: render a .riv file with the Rive D3D11 renderer into a WinUI 3
// CompositionDrawingSurface (and, as the second path, a composition swapchain).
// C ABI so the C# probe can P/Invoke it; every entry returns an HRESULT.
#pragma once
#include <stdint.h>

#ifdef M0RIVE_EXPORTS
#define M0R_API extern "C" __declspec(dllexport)
#else
#define M0R_API extern "C" __declspec(dllimport)
#endif

typedef struct m0r_stats {
    uint64_t frames;              // frames presented to the surface
    uint64_t begin_draw_failures; // BeginDraw/EndDraw HRESULT failures
    double cpu_p50_ms;            // render-thread CPU per frame (Rive advance+draw+flush+copy)
    double cpu_p95_ms;
    double cpu_max_ms;
    double gpu_p50_ms;            // D3D11 timestamp queries around Rive flush+copy; 0 if disjoint
    double gpu_p95_ms;
    double interval_p50_ms;       // render-thread loop interval (DwmFlush paced)
    double interval_p95_ms;
    int32_t last_hr;
    int32_t interop_qi_hr;        // QueryInterface for ICompositionDrawingSurfaceInterop
    int32_t offscreen_has_uav;    // 1 if the B8G8R8A8 offscreen target was created with UAV binding
    int32_t device_removed_hr;    // GetDeviceRemovedReason at stop
    uint64_t gpu_samples_dropped; // timestamp sets that never became valid (excluded from gpu_*)
} m0r_stats;

// Creates a hardware D3D11 device (feature level 11_1, BGRA support, multithread
// protected), the Rive render context, and loads `riv_path` (default artboard,
// default state machine, else animation 0). *out_device receives an AddRef'd
// ID3D11Device* for ICompositorInterop::CreateGraphicsDevice.
M0R_API int32_t m0r_create(const wchar_t* riv_path, uint32_t width, uint32_t height, void** out_device);

// Surface path: `surface_unknown` is the IUnknown of a CompositionDrawingSurface
// created on a CompositionGraphicsDevice that wraps the device above.
M0R_API int32_t m0r_attach_surface(void* surface_unknown);

// Swapchain path: creates a composition swapchain (B8G8R8A8, flip-discard). The
// caller wraps *out_swapchain (AddRef'd IDXGISwapChain1*) with
// ICompositorInterop::CreateCompositionSurfaceForSwapChain.
M0R_API int32_t m0r_create_swapchain(void** out_swapchain);

// Starts / stops the render thread (DwmFlush paced). Only one target is active:
// the swapchain if one was created, else the attached surface.
M0R_API int32_t m0r_start(void);

// M0-2: after Rive draws each frame, fill the pixel rect (x, y, w, h) of the
// rendered image with an opaque colour (0..255 per channel), so the WGC tracker can
// follow content that lives inside the Rive surface. w == 0 turns the marker off.
M0R_API int32_t m0r_set_marker(uint32_t x, uint32_t y, uint32_t w, uint32_t h, uint8_t red, uint8_t green, uint8_t blue);

// M3-RIVE-DPI (surface path only): resize the drawing surface and the offscreen
// render target to width x height pixels. While rendering, the render thread applies
// it at the start of its next frame; m0r_get_resize then reports, per resize, the
// time from this call to the end of the first frame drawn at the new size.
// E_NOTIMPL once a swapchain has been created.
M0R_API int32_t m0r_resize(uint32_t width, uint32_t height);
M0R_API int32_t m0r_get_resize(uint32_t* count, double* last_ms, double* max_ms,
                               uint32_t* width, uint32_t* height, int32_t* last_hr);

M0R_API int32_t m0r_stop(void);
M0R_API int32_t m0r_get_stats(m0r_stats* out);
// Clears all counters and samples, so a later phase (e.g. M0-1b) is measured on its own.
M0R_API int32_t m0r_reset_stats(void);

// Renders one frame headless at `t_sec` and writes raw BGRA (premultiplied) to
// `path`: 8-byte header (uint32 width, uint32 height) then rows top to bottom.
// Used as the reference image for comparing against the WGC capture. Retries for
// up to ~2 s while Rive's shaders compile; S_FALSE (no file) if nothing was drawn.
M0R_API int32_t m0r_write_reference(const wchar_t* path, double t_sec);

M0R_API void m0r_destroy(void);
