// mvp/software_dispatcher.h: Stage A of the WinUI 3 MVP. PORTABLE half only.
// No Composition lowering: every value is evaluated on the app thread by
// MotionSystem::Tick and pushed through an IPropertySink. The WinUI side
// (VisualSink, CompositionTarget.Rendering subscription) is a thin shim around
// these two classes and is NOT compiled or verified in this repository yet.
#pragma once
#include <functional>
#include <string>

#include "../controller.h"

namespace motion {

// The only platform seam of Stage A. A WinUI implementation resolves TargetId to a
// Visual/UIElement (registry owned by the app) and writes one property.
struct IPropertySink {
    virtual ~IPropertySink() = default;
    virtual void Set(TargetId target, const std::string& property, double value) = 0;
};

// Forwards the per-Tick values of every owned property. Ignores Submit/Withdraw:
// there is nothing lowered to the compositor in this stage.
class SoftwareDispatcher final : public IDispatcher {
public:
    explicit SoftwareDispatcher(IPropertySink& sink) : sink_(&sink) {}
    void Submit(const ControllerSnapshot&) override {}
    void Withdraw(ControllerId, EndReason) override {}
    void Apply(TargetId t, const SampleMap& values) override {
        for (const auto& kv : values) sink_->Set(t, kv.first, kv.second.value);
    }
private:
    IPropertySink* sink_;
};

// Render-on-demand driver. `setRendering(true/false)` subscribes/unsubscribes the
// platform frame callback (CompositionTarget.Rendering in WinUI 3, unverified).
// CONTRACT (MotionSystem has no wake hook): call Wake() after ANY handle call that
// can start or change motion, and after MotionSystem::SetReducedMotion, because
// those only mark controllers dirty; values reach the sink on the next Tick.
class FramePump {
public:
    FramePump(MotionSystem& sys, std::function<void(bool)> setRendering)
        : sys_(&sys), setRendering_(std::move(setRendering)) {}
    void Wake() { if (!running_) { running_ = true; setRendering_(true); } }
    void OnFrame() {                       // platform frame callback
        sys_->Tick();
        if (sys_->ActiveCount() == 0) { running_ = false; setRendering_(false); }   // idle means idle
    }
    bool Running() const { return running_; }
private:
    MotionSystem* sys_;
    std::function<void(bool)> setRendering_;
    bool running_ = false;
};

}  // namespace motion
