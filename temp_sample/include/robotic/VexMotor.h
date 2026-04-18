#pragma once

#include "vex.h"
#include "robotic/MotorProvider.h"

namespace gcvex {

    /**
     * @brief Vex 實體馬達封裝
     *
     * 實作 MotorProvider 介面，控制單顆 VEX V5 智慧馬達。
     */
    class VexMotor : public MotorProvider {
    private:
        mutable vex::motor m_motor;
        bool m_isReversed;
        vex::brakeType m_currentBrakeMode;

    public:
        /**
         * @brief 建構子
         * @param port 馬達埠號
         * @param ratio 齒輪比 (例如 vex::ratio18_1)
         * @param reverse 是否反轉預設方向，預設為 false
         */
        VexMotor(int port, vex::gearSetting ratio, bool reverse = false)
            : m_motor(port, ratio, reverse), m_isReversed(reverse), m_currentBrakeMode(vex::brakeType::coast) {
        }

        virtual ~VexMotor() = default;

        // --- EncoderProvider 實作 ---

        inline float get_position() const override {
            return m_motor.position(vex::rotationUnits::deg);
        }

        inline void set_position(float position_deg) override {
            m_motor.setPosition(position_deg, vex::rotationUnits::deg);
        }

        inline void reset_position() override {
            m_motor.resetPosition();
        }

        // --- MotorProvider 實作 ---

        inline bool get_reverse() const override {
            return m_isReversed;
        }

        inline void set_reverse(bool reverse) override {
            if (m_isReversed != reverse) {
                m_isReversed = reverse;
                m_motor.setReversed(m_isReversed);
            }
        }

        inline void on_volt(float volt) override {
            m_motor.spin(vex::directionType::fwd, volt, vex::voltageUnits::volt);
        }

        inline void on_rpm(float rpm) override {
            m_motor.spin(vex::directionType::fwd, rpm, vex::velocityUnits::rpm);
        }

        inline void off() override {
            m_motor.stop(m_currentBrakeMode);
        }

        inline void off(vex::brakeType mode) override {
            m_currentBrakeMode = mode;
            m_motor.stop(m_currentBrakeMode);
        }

        inline float get_volt() const override {
            return m_motor.voltage(vex::voltageUnits::volt);
        }

        inline float get_rpm() const override {
            return m_motor.velocity(vex::velocityUnits::rpm);
        }
    };

} // namespace gcvex
