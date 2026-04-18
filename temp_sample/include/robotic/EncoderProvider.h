#pragma once

namespace gcvex {

    /**
     * @brief 編碼器提供者介面
     *
     * 定義了編碼器或是具備編碼器功能之裝置（如馬達、旋轉感測器）的共通介面。
     * 所有角度與位置的單位皆預設為度 (deg)。
     */
    class EncoderProvider {
    public:
        virtual ~EncoderProvider() = default;

        /**
         * @brief 取得當前位置
         * @return 回傳當前位置，單位為度 (deg)
         */
        virtual float get_position() const = 0;

        /**
         * @brief 設定當前位置
         * @param position_deg 欲設定的當前位置，單位為度 (deg)
         */
        virtual void set_position(float position_deg) = 0;

        /**
         * @brief 重置編碼器位置為 0
         */
        virtual void reset_position() = 0;
    };

} // namespace gcvex
