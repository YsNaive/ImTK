#include "math/PidController.h"

namespace gcvex {

PidController::PidController(const Config& config)
    : m_config(config), m_integral(0), m_prevError(0) {}

double PidController::calculate(double targetValue, double currentValue, double deltaTime_ms, double feedforwardTarget) {
    double error = targetValue - currentValue;

    // P
    double P = error * m_config.kP;

    // I
    m_integral += error * deltaTime_ms;
    if (m_config.maxWindup > 0.0) {
        if (m_integral > m_config.maxWindup) m_integral = m_config.maxWindup;
        else if (m_integral < -m_config.maxWindup) m_integral = -m_config.maxWindup;
    }
    double I = m_integral * m_config.kI;

    // D
    double derivative = (error - m_prevError) / deltaTime_ms;
    double D = derivative * m_config.kD;

    // F
    // If feedforwardTarget is 0 (default), use targetValue instead.
    double fTarget = (feedforwardTarget != 0.0) ? feedforwardTarget : targetValue;
    double F = fTarget * m_config.kF;

    m_prevError = error;

    double output = P + I + D + F;

    // Min Output (to overcome static friction)
    if (m_config.minOutput > 0.0 && output != 0.0) {
        if (output > 0.0 && output < m_config.minOutput) {
            output = m_config.minOutput;
        } else if (output < 0.0 && output > -m_config.minOutput) {
            output = -m_config.minOutput;
        }
    }

    // Max Output Clamping
    if (m_config.maxOutput > 0.0) {
        if (output > m_config.maxOutput) output = m_config.maxOutput;
        else if (output < -m_config.maxOutput) output = -m_config.maxOutput;
    }

    return output;
}

void PidController::reset() {
    m_integral = 0;
    m_prevError = 0;
}

void PidController::setConfig(const Config& newConfig) {
    m_config = newConfig;
}

PidController::Config PidController::getConfig() const {
    return m_config;
}

} // namespace gcvex