// bezier.h: CSS-compatible cubic-bezier(x1, y1, x2, y2). Pure math, no OS deps.
#pragma once
#include <algorithm>
#include <array>
#include <cmath>
#include <limits>
#include <string_view>

namespace motion {

class CubicBezier {
public:
    constexpr CubicBezier(double x1, double y1, double x2, double y2)
        : x1_(x1), y1_(y1), x2_(x2), y2_(y2),
          cx_(3.0 * x1), bx_(3.0 * (x2 - 2.0 * x1)), ax_(1.0 + 3.0 * x1 - 3.0 * x2),
          cy_(3.0 * y1), by_(3.0 * (y2 - 2.0 * y1)), ay_(1.0 + 3.0 * y1 - 3.0 * y2),
          linear_(x1 == y1 && x2 == y2) {}

    // Control points as given (module 6 hands them to the compositor's easing function).
    constexpr double X1() const { return x1_; }
    constexpr double Y1() const { return y1_; }
    constexpr double X2() const { return x2_; }
    constexpr double Y2() const { return y2_; }
    constexpr bool IsLinear() const { return linear_; }

    static bool IsValid(double x1, double y1, double x2, double y2) {
        return std::isfinite(x1) && std::isfinite(y1) && std::isfinite(x2) &&
               std::isfinite(y2) && x1 >= 0.0 && x1 <= 1.0 && x2 >= 0.0 && x2 <= 1.0;
    }

    double Ease(double x) const {
        if (!(x > 0.0)) return 0.0;
        if (x >= 1.0) return 1.0;
        if (linear_) return x;
        return SampleY(SolveT(x));
    }

    double Slope(double x) const {
        if (linear_) return 1.0;
        x = std::clamp(x, 1e-6, 1.0 - 1e-6);
        double t = SolveT(x);
        double dx = std::max(DerivX(t), 1e-12);
        return DerivY(t) / dx;
    }

private:
    static constexpr double kXEps = 1e-9;
    double SampleX(double t) const { return ((ax_ * t + bx_) * t + cx_) * t; }
    double SampleY(double t) const { return ((ay_ * t + by_) * t + cy_) * t; }
    double DerivX(double t) const { return (3.0 * ax_ * t + 2.0 * bx_) * t + cx_; }
    double DerivY(double t) const { return (3.0 * ay_ * t + 2.0 * by_) * t + cy_; }

    double SolveT(double x) const {
        double t = x;
        for (int i = 0; i < 8; ++i) {
            double err = SampleX(t) - x;
            if (std::abs(err) < kXEps) return t;
            double d = DerivX(t);
            if (std::abs(d) < 1e-6) break;
            t -= err / d;
            if (t < 0.0 || t > 1.0) break;
        }
        double lo = 0.0, hi = 1.0;
        t = x;
        for (int i = 0; i < 60; ++i) {
            double err = SampleX(t) - x;
            if (std::abs(err) < kXEps) break;
            (err > 0.0 ? hi : lo) = t;
            t = 0.5 * (lo + hi);
        }
        return t;
    }

    double x1_, y1_, x2_, y2_;
    double cx_, bx_, ax_, cy_, by_, ay_;
    bool linear_;
};

struct EasingToken { std::string_view name; CubicBezier curve; };

// PLACEHOLDER values; the designer owns final numbers via the token file.
inline constexpr std::array<EasingToken, 5> kEasingTokens{{
    {"linear",     CubicBezier(0.0, 0.0, 1.0, 1.0)},
    {"standard",   CubicBezier(0.33, 0.0, 0.67, 1.0)},
    {"emphasized", CubicBezier(0.2, 0.0, 0.0, 1.0)},
    {"decelerate", CubicBezier(0.1, 0.9, 0.2, 1.0)},
    {"accelerate", CubicBezier(0.9, 0.1, 1.0, 0.2)},
}};

inline const CubicBezier* FindEasing(std::string_view name) {
    for (const auto& t : kEasingTokens)
        if (t.name == name) return &t.curve;
    return nullptr;
}

} // namespace motion
