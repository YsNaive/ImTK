#pragma once

#include "math/Pose2D.h"
#include "framework/ISubSystem.h"
#include "utils/SR.h"

namespace gcvex {

    /**
     * @brief 定位提供者 (Pose Provider) 基礎介面
     *
     * 繼承自 `gcvex::Application::ISubSystem`，提供機器人在全域座標系下的
     * 二維位姿 (Pose2D) 資訊。此介面被設計為系統中唯一的定位資訊來源，
     * 透過傳入 `gcvex::SR::SubSystem::PoseProvider` 確保同一生命週期內
     * 只會有一個實體存活，方便開發者抽換不同的底層定位方案 (例如: Odom、GPS 等)。
     */
    class PoseProvider : public Application::ISubSystem {
    public:
        /**
         * @brief 建構子
         * @param interval_ms 定位更新的主迴圈間隔，預設為 20 毫秒
         */
        explicit PoseProvider(int interval_ms = 20);

        virtual ~PoseProvider() = default;

        /**
         * @brief 取得當前機器人的絕對位姿
         * @return 回傳包含 x, y, yaw 的 Pose2D 結構
         */
        virtual Pose2D get_pose() const = 0;

        /**
         * @brief 設定(覆寫)當前機器人的絕對位姿
         * @param pose 欲設定的新位姿
         */
        virtual void set_pose(const Pose2D& pose) = 0;

        /**
         * @brief 取得當前機器人 X 軸座標
         * @return X 座標值
         */
        inline float get_x() const {
            return get_pose().x;
        }

        /**
         * @brief 取得當前機器人 Y 軸座標
         * @return Y 座標值
         */
        inline float get_y() const {
            return get_pose().y;
        }

        /**
         * @brief 取得當前機器人的朝向角度 (Yaw)
         * @return 被限制於 [-PI, PI] 的 ClampedAngle 物件
         */
        inline ClampedAngle get_yaw() const {
            return get_pose().yaw;
        }
    };

} // namespace gcvex