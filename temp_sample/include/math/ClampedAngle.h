#pragma once
#include <cmath>
#include "math/Units.h" // For M_PI

namespace gcvex {

class ClampedAngle {
private:
    float m_radians;

    // Helper to wrap angle to [-π, π]
    static inline float wrap(float radians) {
        // Remainder of floating point division
        radians = std::fmod(radians, static_cast<float>(2.0 * M_PI));

        // Wrap
        if (radians > static_cast<float>(M_PI)) {
            radians -= static_cast<float>(2.0 * M_PI);
        } else if (radians <= static_cast<float>(-M_PI)) {
            radians += static_cast<float>(2.0 * M_PI);
        }
        return radians;
    }

public:
    // Constructors
    inline ClampedAngle() : m_radians(0.0f) {}
    inline explicit ClampedAngle(float degrees) {
        deg(degrees);
    }

    // Getters
    inline float rad() const { return m_radians; }
    inline float deg() const { return static_cast<float>(m_radians * (180.0 / M_PI)); }

    // Setters
    inline void rad(float radians) { m_radians = wrap(radians); }
    inline void deg(float degrees) { m_radians = wrap(static_cast<float>(degrees * (M_PI / 180.0))); }

    // Static constructors for ease of use
    static inline ClampedAngle fromRad(float radians) {
        ClampedAngle a;
        a.rad(radians);
        return a;
    }

    static inline ClampedAngle fromDeg(float degrees) {
        ClampedAngle a;
        a.deg(degrees);
        return a;
    }

    // Arithmetic operators
    inline ClampedAngle operator+(const ClampedAngle& other) const {
        return ClampedAngle::fromRad(m_radians + other.m_radians);
    }

    inline ClampedAngle operator-(const ClampedAngle& other) const {
        return ClampedAngle::fromRad(m_radians - other.m_radians);
    }

    inline ClampedAngle& operator+=(const ClampedAngle& other) {
        this->rad(this->m_radians + other.m_radians);
        return *this;
    }

    inline ClampedAngle& operator-=(const ClampedAngle& other) {
        this->rad(this->m_radians - other.m_radians);
        return *this;
    }

    // Comparison operators with epsilon error range
    inline bool operator==(const ClampedAngle& other) const {
        return std::abs(wrap(m_radians - other.m_radians)) < FLOAT_EPSILON;
    }

    inline bool operator!=(const ClampedAngle& other) const {
        return !(*this == other);
    }
};

} // namespace gcvex
