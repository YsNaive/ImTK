#pragma once
#include <string>
#include <cstdio>
#include "gc_config.h"

namespace gcvex {
namespace Debug {

    class LogProvider {
    public:
        virtual ~LogProvider() = default;
        virtual void clear() = 0;
        virtual void log(const std::string& msg) = 0;
        virtual int getLineLimit() const { return -1; }
        virtual int getUpdateInterval() const { return 100; }
    };

    class BrainLogger : public LogProvider {
    public:
        void clear() override;
        void log(const std::string& msg) override;
        int getLineLimit() const override { return 12; }
        int getUpdateInterval() const override { return 100; } // Brain 螢幕通常 100ms 刷新即可
    private:
        int m_line = 1;
    };

    class ControllerLogger : public LogProvider {
    public:
        void clear() override;
        void log(const std::string& msg) override;
        int getLineLimit() const override { return 3; }
        int getUpdateInterval() const override { return 500; } // 遙控器受限於通訊協定，加長刷新間隔以防堵塞
    private:
        int m_line = 1;
    };

    extern BrainLogger defaultBrainLogger;
    extern ControllerLogger defaultControllerLogger;

    void addLogger(LogProvider* provider);
    void removeLogger(LogProvider* provider);

    namespace detail {
        // 內部使用：將格式化後的字串註冊到對應的目標
        void logImpl(const char* format, const char* formattedMsg);
        void raiseImpl(const char* formattedMsg);
    } // namespace detail

    // ==========================================
    // 同時輸出日誌 (統一管理)
    // ==========================================
    template<typename... Args>
    void log(const char* format, Args... args) {
#if DEVELOPMENT_MODE
        char buffer[256];
        snprintf(buffer, sizeof(buffer), format, args...);
        detail::logImpl(format, buffer);
#endif
    }

    template<typename... Args>
    void logif(bool condition, const char* format, Args... args) {
#if DEVELOPMENT_MODE
        if (condition) {
            log(format, args...);
        }
#endif
    }

    // ==========================================
    // 致命錯誤 (Fatal Error)
    // ==========================================
    template<typename... Args>
    void raise(const char* format, Args... args) {
        char buffer[256];
        snprintf(buffer, sizeof(buffer), format, args...);
        detail::raiseImpl(buffer);
    }

} // namespace Debug
} // namespace gcvex