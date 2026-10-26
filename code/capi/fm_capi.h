/* code/capi/fm_capi.h: C ABI over MotionSystem + SoftwareDispatcher + FramePump, for P/Invoke from
 * MotionSoftDemo (C#). Software path only: no Composition lowering.
 * RULES: call every function on ONE thread (the UI thread). Never call fm_* from inside the
 * sink/render callbacks. Strings are UTF-8. Ids are opaque; 0 means failure/none. */
#ifndef FM_CAPI_H
#define FM_CAPI_H
#include <stdint.h>
#ifdef __cplusplus
extern "C" {
#endif
#if defined(_WIN32)
#define FM_API __declspec(dllexport)
#else
#define FM_API __attribute__((visibility("default")))
#endif

typedef struct fm_system fm_system;
/* Called from fm_on_frame for every owned property, every frame while something animates. */
typedef void (*fm_sink_fn)(uint64_t target, const char* property, double value, void* user);
/* on=1: start your frame source (DispatcherQueueTimer); on=0: stop it (idle means idle). */
typedef void (*fm_render_fn)(int on, void* user);

FM_API fm_system* fm_create(fm_sink_fn sink, fm_render_fn render, void* user);       /* real clock */
FM_API fm_system* fm_create_manual(fm_sink_fn sink, fm_render_fn render, void* user); /* tests: fm_manual_advance */
FM_API void fm_manual_advance(fm_system*, double seconds);
FM_API void fm_destroy(fm_system*);

/* 0 = ok. Otherwise the number of problems; `err` receives them, one per line, JSON-Pointer paths. */
FM_API int fm_load_tokens(fm_system*, const char* json, char* err, int errCap);

FM_API void fm_set_reduced_motion(fm_system*, int on);             /* wakes the pump */

/* transition == NULL: empty animation (for Retarget only). Returns 0 if the transition is unknown. */
FM_API uint64_t fm_animation_create(fm_system*, uint64_t target, const char* transition);
FM_API void fm_animation_play(fm_system*, uint64_t anim);          /* wakes the pump */
FM_API void fm_animation_cancel(fm_system*, uint64_t anim);
FM_API void fm_animation_release(fm_system*, uint64_t anim);       /* drop OUR handle; motion keeps running */
FM_API int  fm_animation_retarget(fm_system*, uint64_t anim, const char* property, double target,
                                  double response, double damping, int hasFrom, double from);   /* 1 ok, 0 fail; wakes the pump */

FM_API void fm_on_frame(fm_system*);                               /* call from your frame callback */
FM_API int  fm_is_running(fm_system*);
FM_API int  fm_current(fm_system*, uint64_t target, const char* property, double* value, double* velocity);

#ifdef __cplusplus
}
#endif
#endif
