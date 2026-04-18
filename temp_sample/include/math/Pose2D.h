#pragma once
#include <cmath>
#include "math/ClampedAngle.h"

namespace gcvex {

class Pose2D {
public:
    float x;
    float y;
    ClampedAngle yaw;

    // Constructors
    inline Pose2D() : x(0.0f), y(0.0f), yaw() {}

    inline Pose2D(float x, float y, const ClampedAngle& yaw)
        : x(x), y(y), yaw(yaw) {}

    // Convenience constructor using degrees for angle directly
    inline Pose2D(float x, float y, float yaw_deg)
        : x(x), y(y), yaw(ClampedAngle::fromDeg(yaw_deg)) {}

    // Arithmetic operators
    inline Pose2D operator+(const Pose2D& other) const {
        return Pose2D(x + other.x, y + other.y, yaw + other.yaw);
    }

    inline Pose2D operator-(const Pose2D& other) const {
        return Pose2D(x - other.x, y - other.y, yaw - other.yaw);
    }

    inline Pose2D& operator+=(const Pose2D& other) {
        x += other.x;
        y += other.y;
        yaw += other.yaw;
        return *this;
    }

    inline Pose2D& operator-=(const Pose2D& other) {
        x -= other.x;
        y -= other.y;
        yaw -= other.yaw;
        return *this;
    }

    // Comparison operators with epsilon error range
    inline bool operator==(const Pose2D& other) const {
        return std::abs(x - other.x) < FLOAT_EPSILON &&
               std::abs(y - other.y) < FLOAT_EPSILON &&
               yaw == other.yaw;
    }

    inline bool operator!=(const Pose2D& other) const {
        return !(*this == other);
    }
};

} // namespace gcvex
