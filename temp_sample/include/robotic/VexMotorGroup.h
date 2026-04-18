#pragma once

#include "vex.h"
#include "robotic/MotorProvider.h"
#include "utils/Debug.h"
#include <vector>

namespace gcvex {

    /**
     * @brief 虛擬馬達群組
     *
     * 將多個 MotorProvider 組合在一起當作一顆馬達來控制。
     * 可接收 VexMotor 等實作 MotorProvider 介面之指標。
     */
    class VexMotorGroup : public MotorProvider {
    private:
        std::vector<MotorProvider*> m_motors;
        bool m_isReversed;
        vex::brakeType m_currentBrakeMode;

    public:
        /**
         * @brief 建構子
         * @param motors MotorProvider 指標的集合 (例如 VexMotor)
         */
        VexMotorGroup(const std::vector<MotorProvider*>& motors)
            : m_motors(motors), m_isReversed(false), m_currentBrakeMode(vex::brakeType::coast) {

            if (m_motors.empty()) {
                Debug::raise("VexMotorGroup initialized with empty motor list.");
            }

            for (size_t i = 0; i < m_motors.size(); ++i) {
                if (m_motors[i] == nullptr) {
                    Debug::raise("VexMotorGroup received nullptr in motor list.");
                }
            }
        }

        virtual ~VexMotorGroup() = default;

        // --- EncoderProvider 實作 ---

        inline float get_position() const override {
            if (m_motors.empty()) return 0.0f;
            // 回傳群組內第一顆馬達的位置做為代表
            return m_motors[0]->get_position();
        }

        inline void set_position(float position_deg) override {
            for (auto motor : m_motors) {
                motor->set_position(position_deg);
            }
        }

        inline void reset_position() override {
            for (auto motor : m_motors) {
                motor->reset_position();
            }
        }

        // --- MotorProvider 實作 ---

        inline bool get_reverse() const override {
            return m_isReversed;
        }

        inline void set_reverse(bool reverse) override {
            if (m_isReversed != reverse) {
                m_isReversed = reverse;
                for (auto motor : m_motors) {
                    // 如果整個群組反轉，需要反轉每個子馬達原有的方向
                    motor->set_reverse(!motor->get_reverse());
                }
            }
        }

        inline void on_volt(float volt) override {
            for (auto motor : m_motors) {
                motor->on_volt(volt);
            }
        }

        inline void on_rpm(float rpm) override {
            for (auto motor : m_motors) {
                motor->on_rpm(rpm);
            }
        }

        inline void off() override {
            for (auto motor : m_motors) {
                motor->off(m_currentBrakeMode);
            }
        }

        inline void off(vex::brakeType mode) override {
            m_currentBrakeMode = mode;
            for (auto motor : m_motors) {
                motor->off(mode);
            }
        }

        inline float get_volt() const override {
            if (m_motors.empty()) return 0.0f;
            return m_motors[0]->get_volt();
        }

        inline float get_rpm() const override {
            if (m_motors.empty()) return 0.0f;
            return m_motors[0]->get_rpm();
        }
    };

} // namespace gcvex
