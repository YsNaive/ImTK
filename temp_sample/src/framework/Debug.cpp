#include "framework/Debug.h"
#include "framework/Application.h"
#include <vector>
#include <string>
#include <algorithm>

namespace gcvex {
namespace Debug {

    BrainLogger defaultBrainLogger;
    ControllerLogger defaultControllerLogger;

    void BrainLogger::clear() {
        gcvex::Application::mainBrain.Screen.clearScreen();
        m_line = 1;
    }

    void BrainLogger::log(const std::string& msg) {
        if (m_line > 12) return;
        gcvex::Application::mainBrain.Screen.setCursor(m_line, 1);
        gcvex::Application::mainBrain.Screen.print(msg.c_str());
        m_line++;
    }

    void ControllerLogger::clear() {
        gcvex::Application::mainController.Screen.clearScreen();
        m_line = 1;
    }

    void ControllerLogger::log(const std::string& msg) {
        if (m_line > 3) return;
        gcvex::Application::mainController.Screen.setCursor(m_line, 1);
        gcvex::Application::mainController.Screen.print(msg.c_str());
        m_line++;
    }

    // 匿名 namespace，完全隱藏內部實作變數和函式
    namespace {

        struct LogEntry {
            std::string key;
            std::string message;
        };

        std::vector<LogEntry> s_logs;
        const size_t MAX_LOG_LINES = 50;

        struct LoggerState {
            LogProvider* provider;
            bool dirty;
            int lastUpdateTime;
        };

        std::vector<LoggerState> s_loggers;

        vex::mutex s_debugMutex;

        // fd for DebugRegistrar
        struct DebugRegistrar;
        static DebugRegistrar& getDebugRegistrar();
        
        void updateLog(const char* format, const char* formattedMsg) {
            getDebugRegistrar(); // 確保已註冊

            s_debugMutex.lock();

            std::string key(format);
            std::string msg(formattedMsg);
            bool localDirty = false;

            auto it = std::find_if(s_logs.begin(), s_logs.end(), [&](const LogEntry& entry) {
                return entry.key == key;
            });

            if (it != s_logs.end()) {
                if (it->message != msg || it != s_logs.end() - 1) {
                    // 如果訊息不同，或是相同但不是在最後一行，則把它移到最後一行
                    s_logs.erase(it);
                    s_logs.push_back({key, msg});
                    localDirty = true;
                }
            } else {
                s_logs.push_back({key, msg});
                localDirty = true;

                if (s_logs.size() > MAX_LOG_LINES) {
                    s_logs.erase(s_logs.begin());
                }
            }

            if (localDirty) {
                for (auto& state : s_loggers) {
                    state.dirty = true;
                }
            }

            s_debugMutex.unlock();
        }

        void debugLoop(int time, int dt) {
            s_debugMutex.lock();

            for (auto& state : s_loggers) {
                if (state.dirty && (time - state.lastUpdateTime >= state.provider->getUpdateInterval())) {
                    state.provider->clear();

                    int limit = state.provider->getLineLimit();
                    auto start_it = s_logs.begin();

                    if (limit != -1 && s_logs.size() > (size_t)limit) {
                        start_it = s_logs.end() - limit;
                    }

                    for (auto it = start_it; it != s_logs.end(); ++it) {
                        state.provider->log(it->message);
                    }

                    state.dirty = false;
                    state.lastUpdateTime = time;
                }
            }

            s_debugMutex.unlock();
        }

        // 模組載入時，自動註冊背景更新迴圈
        struct DebugRegistrar {
            DebugRegistrar() {
#if DEVELOPMENT_MODE
#if ENABLE_BRAIN_LOG
                s_loggers.push_back({&defaultBrainLogger, true, 0});
#endif
#if ENABLE_CONTROLLER_LOG
                s_loggers.push_back({&defaultControllerLogger, true, 0});
#endif

                gcvex::Application::registerSubSystem(
                    "Debug",
                    [](){ /* init */ },
                    [](){ /* start */ },
                    [](){ /* enable */ },
                    [](){ /* disable */ },
                    debugLoop,
                    100 // 100ms 更新一次畫面，避免過度佔用 CPU 或 I/O
                );
#endif
            }
        };

        static DebugRegistrar& getDebugRegistrar() {
            static DebugRegistrar inst;
            return inst;
        }

    } // anonymous namespace

    void addLogger(LogProvider* provider) {
        if (!provider) return;
        s_debugMutex.lock();
        auto it = std::find_if(s_loggers.begin(), s_loggers.end(), [&](const LoggerState& state) {
            return state.provider == provider;
        });
        if (it == s_loggers.end()) {
            s_loggers.push_back({provider, true, 0});
        }
        s_debugMutex.unlock();
    }

    void removeLogger(LogProvider* provider) {
        if (!provider) return;
        s_debugMutex.lock();
        auto it = std::find_if(s_loggers.begin(), s_loggers.end(), [&](const LoggerState& state) {
            return state.provider == provider;
        });
        if (it != s_loggers.end()) {
            s_loggers.erase(it);
        }
        s_debugMutex.unlock();
    }

    namespace detail {
        void logImpl(const char* format, const char* formattedMsg) {
            updateLog(format, formattedMsg);
        }

        void raiseImpl(const char* formattedMsg) {
            // 轉交給 Application 處理，由 Application 主線程保證最後的繪製與停止邏輯
            gcvex::Application::raise(formattedMsg);
        }
    } // namespace detail

} // namespace Debug
} // namespace gcvex