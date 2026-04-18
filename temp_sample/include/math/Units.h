#pragma once
#include <cmath>

namespace gcvex {

// ==========================================
// 核心系統單位 (Internal Base Units)
// - 距離: 英吋 (Inches)
// - 角度: 弧度 (Radians)
// - 時間: 毫秒 (Milliseconds)
// - 電壓: 伏特 (Volts)
// ==========================================

#ifndef M_PI
#define M_PI 3.14159265358979323846
#endif

// ==========================================
// Global Math Constants
// ==========================================
constexpr float FLOAT_EPSILON = 1e-2f; // Used for floating point equality comparisons

// ==========================================
// C++11 Polyfills
// ==========================================
template<class T>
inline const T& clamp(const T& v, const T& lo, const T& hi) {
    return (v < lo) ? lo : (hi < v) ? hi : v;
}

template<class T, class Compare>
inline const T& clamp(const T& v, const T& lo, const T& hi, Compare comp) {
    return comp(v, lo) ? lo : comp(hi, v) ? hi : v;
}

// --- 距離單位轉換 ---
// 系統預設接受英吋，如果您輸入公分，使用此函式轉換為英吋
inline constexpr double unitCm(double cm) { return cm / 2.54; }
inline constexpr double unitM(double m) { return m * 100.0 / 2.54; }
inline constexpr double unitIn(double in) { return in; }

// --- 角度單位轉換 ---
// 系統預設接受弧度，如果您輸入度數，使用此函式轉換為弧度
inline constexpr double unitDeg(double degrees) { return degrees * (M_PI / 180.0); }
inline constexpr double unitRad(double radians) { return radians; }

// --- 時間單位轉換 ---
// 系統預設接受毫秒，如果您輸入秒，使用此函式轉換為毫秒
inline constexpr double unitSec(double seconds) { return seconds * 1000.0; }
inline constexpr double unitMsec(double milliseconds) { return milliseconds; }

// ==========================================
// C++11 User-Defined Literals (語法糖可選使用)
// ==========================================
namespace literals {
    inline constexpr double operator"" _in(long double in) { return unitIn(static_cast<double>(in)); }
    inline constexpr double operator"" _cm(long double cm) { return unitCm(static_cast<double>(cm)); }
    inline constexpr double operator"" _deg(long double deg) { return unitDeg(static_cast<double>(deg)); }
    inline constexpr double operator"" _rad(long double rad) { return unitRad(static_cast<double>(rad)); }
    inline constexpr double operator"" _s(long double s) { return unitSec(static_cast<double>(s)); }
    inline constexpr double operator"" _ms(long double ms) { return unitMsec(static_cast<double>(ms)); }
} // namespace literals

} // namespace gcvex