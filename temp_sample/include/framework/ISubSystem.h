#pragma once
#include <string>
#include "framework/Application.h"

namespace gcvex {
namespace Application {

    /**
     * @brief ISubSystem
     *
     * 透過 RAII (Resource Acquisition Is Initialization) 管理的子系統介面。
     * 繼承此介面的類別，在建構時將會自動註冊到 gcvex::Application 生命週期中，
     * 並於解構時自動註銷。
     *
     * 此類別強制要求子類在建構時提供一個唯一的名稱 (name)，
     * 若發現重複名稱，將會印出錯誤並拋出例外，確保系統的唯一性。
     */
    class ISubSystem {
    public:
        virtual ~ISubSystem();

        // 禁用 Copy 與 Move，確保生命週期唯一性
        ISubSystem(const ISubSystem&) = delete;
        ISubSystem& operator=(const ISubSystem&) = delete;
        ISubSystem(ISubSystem&&) = delete;
        ISubSystem& operator=(ISubSystem&&) = delete;

        // 生命週期虛擬方法，子類可選擇性覆寫
        virtual void init() {}
        virtual void start() {}
        virtual void enable() {}
        virtual void disable() {}
        virtual void loop(int time, int dt) {}

        // 取得此子系統的唯一名稱
        const std::string& getName() const { return m_name; }

    protected:
        /**
         * @brief 建構子，受保護以強制子類呼叫並提供名稱
         *
         * @param name 此子系統的唯一名稱
         * @param interval_ms 主迴圈執行間隔，預設 20 毫秒
         */
        explicit ISubSystem(const std::string& name, int interval_ms = 20);

    private:
        std::string m_name;
        SubSystemIDs m_ids;
    };

} // namespace Application
} // namespace gcvex