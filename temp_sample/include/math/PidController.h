#pragma once
#include "Units.h"

namespace gcvex {

/**
 * @brief 純粹的 PIDF 數學模型
 * 支援 \Delta t 時間差與前饋 (Feedforward) 參數
 */
class PidController {
public:
    struct Config {
        double kP = 0.0;
        double kI = 0.0;
        double kD = 0.0;
        double kF = 0.0;          // Feedforward gain (例如 Kv)
        double maxWindup = 0.0;   // 積分防飽和閾值 (0 表示不限制)
        double maxOutput = 0.0;   // 最大輸出限制 (0 表示不限制)
        double minOutput = 0.0;   // 最小啟動輸出 (克服靜摩擦)
    };

    /**
     * @brief 建構時設定 PIDF 參數
     */
    PidController(const Config& config);

    /**
     * @brief 給定目標與當前數值，計算 PIDF 輸出
     *
     * @param targetValue 目標值 (Reference)
     * @param currentValue 目前讀值 (Measurement)
     * @param deltaTime_ms 時間差（毫秒）。預設為 1.0 以兼容舊版純 Tick-based 邏輯
     * @param feedforwardTarget 可選的前饋目標 (例如理想速度 V_ref)。如果未提供，預設使用 targetValue。
     * @return 總輸出電壓或百分比
     */
    double calculate(double targetValue, double currentValue, double deltaTime_ms = 1.0, double feedforwardTarget = 0.0);

    /**
     * @brief 清除過去累積的積分與誤差歷史
     */
    void reset();

    void setConfig(const Config& newConfig);
    Config getConfig() const;

private:
    Config m_config;
    double m_integral;
    double m_prevError;
};

} // namespace gcvex