// spring.h: closed-form damped spring. Pure math, no OS dependencies.
// Units: DIPs (or any value unit) and seconds. Revision 2.
#pragma once
#include <algorithm>
#include <cmath>

namespace motion {

struct SpringParams {
    double response = 0.35;   // undamped period in seconds (> 0)
    double damping  = 0.8;    // damping ratio (>= 0)
};

struct SpringState { double value; double velocity; };

class Spring {
public:
    // Starts at `from` with `velocity`, moving toward `target`.
    Spring(SpringParams p, double from, double target, double velocity = 0.0)
        : target_(target)
    {
        p_.response = std::max(p.response, 1e-4);
        p_.damping  = std::max(p.damping, 0.0);          // negative would grow
        w0_ = 2.0 * 3.14159265358979323846 / p_.response;
        x0_ = from - target;
        v0_ = velocity;
    }

    // Closed-form state at time t >= 0 since (re)start.
    SpringState At(double t) const {
        t = std::max(t, 0.0);
        double x, v;
        const double z = p_.damping;
        if (z < 1.0 - kEps) {                       // underdamped
            double a = z * w0_, wd = w0_ * std::sqrt(1.0 - z * z);
            double B = (v0_ + a * x0_) / wd;
            double e = std::exp(-a * t), c = std::cos(wd * t), s = std::sin(wd * t);
            x = e * (x0_ * c + B * s);
            v = e * (-a * (x0_ * c + B * s) + wd * (B * c - x0_ * s));
        } else if (z <= 1.0 + kEps) {               // critically damped
            double C = v0_ + w0_ * x0_;
            double e = std::exp(-w0_ * t);
            x = e * (x0_ + C * t);
            v = e * (C - w0_ * (x0_ + C * t));
        } else {                                    // overdamped
            double r1, r2;
            Roots(r1, r2);
            double c2 = (v0_ - r1 * x0_) / (r2 - r1), c1 = x0_ - c2;
            double e1 = std::exp(r1 * t), e2 = std::exp(r2 * t);
            x = c1 * e1 + c2 * e2;
            v = c1 * r1 * e1 + c2 * r2 * e2;
        }
        return { target_ + x, v };
    }

    // Continue from the current state toward a new target. `t` = time since
    // the last (re)start. Prefer the timeline/player, which tracks this for you.
    void Retarget(double t, double newTarget) {
        SpringState s = At(t);
        target_ = newTarget;
        x0_ = s.value - newTarget;
        v0_ = s.velocity;
    }

    // O(1) conservative bound on the time after which |displacement| < posEps
    // and |velocity| < velEps for the rest of the motion, measured from the last
    // (re)start. It is an upper bound built from the decay envelope: it may
    // exceed the true settle time by up to roughly one oscillation period.
    // Undamped (or nearly) springs never settle and return `cap`.
    double SettleTime(double posEps = 0.1, double velEps = 0.5, double cap = 10.0) const {
        posEps = std::max(posEps, 1e-12);
        velEps = std::max(velEps, 1e-12);
        const double z = p_.damping;
        double t;
        if (z < 1.0 - kEps) {
            const double a = z * w0_;
            if (a <= 1e-12) return cap;
            const double wd = w0_ * std::sqrt(1.0 - z * z);
            const double B = (v0_ + a * x0_) / wd;
            const double A = std::hypot(x0_, B);   // |x| <= A e^{-at}, |v| <= w0 A e^{-at}
            t = std::max(DecayTime(A, posEps, a), DecayTime(w0_ * A, velEps, a));
        } else if (z <= 1.0 + kEps) {
            const double C = std::abs(v0_ + w0_ * x0_), X = std::abs(x0_);
            t = std::max(CriticalTime(X, C, w0_, posEps),
                         CriticalTime(C + w0_ * X, w0_ * C, w0_, velEps));
        } else {
            double r1, r2;
            Roots(r1, r2);
            const double c2 = (v0_ - r1 * x0_) / (r2 - r1), c1 = x0_ - c2;
            const double rate = -r1;                 // slow mode dominates the tail
            const double K1 = std::abs(c1) + std::abs(c2);
            const double K2 = std::abs(c1) * std::abs(r1) + std::abs(c2) * std::abs(r2);
            t = std::max(DecayTime(K1, posEps, rate), DecayTime(K2, velEps, rate));
        }
        return std::min(t, cap);
    }

    // Brute-force fixed-step scan. TEST ORACLE ONLY (about cap/step exp+sincos calls).
    double SettleTimeScan(double posEps = 0.1, double velEps = 0.5,
                          double step = 1.0 / 240.0, double cap = 10.0) const {
        double last = 0.0;
        for (double t = 0.0; t <= cap; t += step) {
            SpringState s = At(t);
            if (std::abs(s.value - target_) >= posEps || std::abs(s.velocity) >= velEps)
                last = t + step;
        }
        return std::min(last, cap);
    }

    double Target() const { return target_; }
    const SpringParams& Params() const { return p_; }

private:
    static constexpr double kEps = 1e-6;

    // Overdamped roots, both negative, |r1| < |r2|. Computed without
    // cancellation: r2 = -(z w0 + q), then r1 = w0^2 / r2 (product of roots).
    void Roots(double& r1, double& r2) const {
        const double z = p_.damping;
        const double q = w0_ * std::sqrt(z * z - 1.0);
        r2 = -(z * w0_ + q);
        r1 = (w0_ * w0_) / r2;
    }

    static double DecayTime(double K, double eps, double rate) {
        return K <= eps ? 0.0 : std::log(K / eps) / rate;
    }

    // Smallest t after which g(t) = e^{-w t}(p + q t) stays <= eps (p, q >= 0).
    static double CriticalTime(double p, double q, double w, double eps) {
        auto g = [&](double t) { return std::exp(-w * t) * (p + q * t); };
        const double tpeak = q > 0.0 ? std::max(0.0, 1.0 / w - p / q) : 0.0;
        if (g(tpeak) <= eps) return 0.0;             // global max is already below eps
        double lo = tpeak, hi = tpeak + 1.0 / w;
        for (int i = 0; i < 64 && g(hi) > eps; ++i) hi *= 2.0;
        for (int i = 0; i < 80; ++i) {
            const double mid = 0.5 * (lo + hi);
            (g(mid) > eps ? lo : hi) = mid;
        }
        return hi;
    }

    SpringParams p_;
    double target_, w0_, x0_ = 0, v0_ = 0;
};

} // namespace motion
