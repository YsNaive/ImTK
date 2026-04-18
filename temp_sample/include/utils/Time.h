#pragma once

#include <chrono>
#include <cstdint>

namespace gcvex {

class Time {
public:
    // Delete constructor to act as a static class
    Time() = delete;

    inline static int64_t now() {
        auto now = std::chrono::system_clock::now();
        return std::chrono::duration_cast<std::chrono::seconds>(now.time_since_epoch()).count();
    }

    inline static int64_t now_milli() {
        auto now = std::chrono::system_clock::now();
        return std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()).count();
    }

    inline static int64_t now_nano() {
        auto now = std::chrono::system_clock::now();
        return std::chrono::duration_cast<std::chrono::nanoseconds>(now.time_since_epoch()).count();
    }
};

} // namespace gcvex