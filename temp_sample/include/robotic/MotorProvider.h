#pragma once

#include "vex.h"
#include "robotic/EncoderProvider.h"
#include <cmath>

namespace gcvex {

    /**
     * @brief 馬達提供者介面
     *
     * 繼承自 EncoderProvider。提供馬達控制共通介面。
     * 具備電壓映射與各種不同的驅動、煞車方式。
     */
    class MotorProvider : public EncoderProvider {
    public:
        // 電壓映射上下限
        float max_volt = 12.0f;
        float min_volt = 0.0f;

        virtual ~MotorProvider() = default;

        /**
         * @brief 取得是否設定為反轉
         * @return 是否反轉
         */
        virtual bool get_reverse() const = 0;

        /**
         * @brief 設定馬達反轉
         * @param reverse true 表示反轉，false 表示正轉
         */
        virtual void set_reverse(bool reverse) = 0;

        /**
         * @brief 以百分比驅動馬達
         *
         * 將輸入的百分比 [-100, 100] 映射到實際的電壓區間。
         * 根據百分比正負，會考慮設定的最低啟動電壓(min_volt)至最大電壓(max_volt)。
         *
         * @param pct 驅動百分比 [-100.0, 100.0]
         */
        inline void on(float pct) {
            if (pct == 0.0f) {
                on_volt(0.0f);
                return;
            }

            // 限制 pct 在 -100 到 100 之間
            if (pct > 100.0f) pct = 100.0f;
            if (pct < -100.0f) pct = -100.0f;

            float absPct = std::abs(pct);

            // 將 0~100 的百分比映射到 min_volt ~ max_volt
            float mappedVolt = min_volt + (absPct / 100.0f) * (max_volt - min_volt);

            // 根據原本 pct 的符號給定電壓方向
            if (pct < 0.0f) {
                mappedVolt = -mappedVolt;
            }

            on_volt(mappedVolt);
        }

        /**
         * @brief 以指定電壓轉動馬達
         * @param volt 電壓 (伏特)
         */
        virtual void on_volt(float volt) = 0;

        /**
         * @brief 以指定 RPM 轉動馬達
         * @param rpm 轉速 (RPM)
         */
        virtual void on_rpm(float rpm) = 0;

        /**
         * @brief 停止馬達 (使用上一次設定的模式)
         */
        virtual void off() = 0;

        /**
         * @brief 停止馬達並更新模式
         * @param mode 煞車模式
         */
        virtual void off(vex::brakeType mode) = 0;

        /**
         * @brief 取得當前輸出電壓
         * @return 電壓 (伏特)
         */
        virtual float get_volt() const = 0;

        /**
         * @brief 取得當前轉速
         * @return 轉速 (RPM)
         */
        virtual float get_rpm() const = 0;

        // ---------------------------------------------------------------------
        // 語法糖 Syntax Sugar
        // ---------------------------------------------------------------------

        /**
         * @brief 煞車模式 (Brake)
         */
        inline void brake() {
            off(vex::brakeType::brake);
        }

        /**
         * @brief 鎖死模式 (Hold)
         */
        inline void hold() {
            off(vex::brakeType::hold);
        }

        /**
         * @brief 滑行模式 (Coast)
         */
        inline void coast() {
            off(vex::brakeType::coast);
        }
    };

} // namespace gcvex
