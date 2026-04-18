#include "framework/Application.h"
#include "framework/Debug.h"
#include "vex.h"
#include <vector>
#include <algorithm>
#include <cstdlib>
#include <atomic>

using namespace vex;

namespace gcvex {
namespace Application {

    // VEX 專用的 Lock Guard (RAII)，確保即便有 return 也能安全解鎖
    class VexLockGuard {
        vex::mutex& m_mut;
    public:
        VexLockGuard(vex::mutex& mut) : m_mut(mut) { m_mut.lock(); }
        ~VexLockGuard() { m_mut.unlock(); }
    };

    // 為了安全支援動態註冊/卸載，此處內部使用 vex::mutex 保護 vector
    static vex::mutex& getRegistryMutex() {
        static vex::mutex inst;
        return inst;
    }

    static int s_nextId = 1;

    // 已註冊的 SubSystem 名稱清單 (用於檢查重複)
    static std::vector<std::string>& getRegisteredSubsystems() {
        static std::vector<std::string> inst;
        return inst;
    }

    struct InitEntry { int id; std::function<void()> cb; };
    struct StartEntry { int id; std::function<void()> cb; };
    struct EnableEntry { int id; std::function<void()> cb; };
    struct DisableEntry { int id; std::function<void()> cb; };
    struct LoopEntry {
        int id;
        std::function<void(int, int)> cb;
        int interval;
        int lastRunTime;
    };

    static std::vector<InitEntry>& getInitCallbacks() { static std::vector<InitEntry> inst; return inst; }
    static std::vector<StartEntry>& getStartCallbacks() { static std::vector<StartEntry> inst; return inst; }
    static std::vector<EnableEntry>& getEnableCallbacks() { static std::vector<EnableEntry> inst; return inst; }
    static std::vector<DisableEntry>& getDisableCallbacks() { static std::vector<DisableEntry> inst; return inst; }
    static std::vector<LoopEntry>& getLoopCallbacks() { static std::vector<LoopEntry> inst; return inst; }

    // 定義全局變數的初始值 (空操作防崩潰)
    std::function<void()> autoOp   = [](){};
    std::function<void()> teleOp = [](){};

    // 硬體全域資源實例
    vex::brain mainBrain;
    vex::controller mainController;

    // 內部 VEX 競賽狀態追蹤
    static vex::competition s_comp;
    static vex::task* s_backgroundTask = nullptr;
    static bool s_wasDisabled = false;

    // 暫停狀態控制
    static std::atomic<bool> s_paused{false};

    // 致命錯誤狀態控制
    static std::atomic<bool> s_raiseLock{false};
    static std::atomic<bool> s_hasFatalError{false};
    static std::string s_fatalErrorMsg = "";

    // ==========================================
    // 註冊 API 實作
    // ==========================================

    int registerInit(std::function<void()> cb) {
        VexLockGuard lock(getRegistryMutex());
        int id = s_nextId++;
        if(cb) getInitCallbacks().push_back({id, cb});
        return id;
    }

    int registerStart(std::function<void()> cb) {
        VexLockGuard lock(getRegistryMutex());
        int id = s_nextId++;
        if(cb) getStartCallbacks().push_back({id, cb});
        return id;
    }

    int registerEnable(std::function<void()> cb) {
        VexLockGuard lock(getRegistryMutex());
        int id = s_nextId++;
        if(cb) getEnableCallbacks().push_back({id, cb});
        return id;
    }

    int registerDisable(std::function<void()> cb) {
        VexLockGuard lock(getRegistryMutex());
        int id = s_nextId++;
        if(cb) getDisableCallbacks().push_back({id, cb});
        return id;
    }

    int registerLoop(std::function<void(int, int)> cb, int interval_ms) {
        VexLockGuard lock(getRegistryMutex());
        int id = s_nextId++;
        if(cb) getLoopCallbacks().push_back({id, cb, interval_ms, static_cast<int>(vex::timer::system())});
        return id;
    }

    SubSystemIDs registerSubSystem(const std::string& name,
                                   std::function<void()> init,
                                   std::function<void()> start,
                                   std::function<void()> enable,
                                   std::function<void()> disable,
                                   std::function<void(int, int)> loop,
                                   int interval_ms) {
        {
            VexLockGuard lock(getRegistryMutex());
            auto& subs = getRegisteredSubsystems();
            if (std::find(subs.begin(), subs.end(), name) != subs.end()) {
                gcvex::Debug::raise("Dup SubSystem [%s] appear", name.c_str());
            }
            subs.push_back(name);
        }

        SubSystemIDs ids;
        ids.name       = name;
        ids.initId     = registerInit(init);
        ids.startId    = registerStart(start);
        ids.enableId   = registerEnable(enable);
        ids.disableId  = registerDisable(disable);
        ids.loopId     = registerLoop(loop, interval_ms);
        return ids;
    }

    void unregister(int id) {
        VexLockGuard lock(getRegistryMutex());

        auto& inits = getInitCallbacks();
        inits.erase(std::remove_if(inits.begin(), inits.end(),
            [id](const InitEntry& e){ return e.id == id; }), inits.end());

        auto& starts = getStartCallbacks();
        starts.erase(std::remove_if(starts.begin(), starts.end(),
            [id](const StartEntry& e){ return e.id == id; }), starts.end());

        auto& enables = getEnableCallbacks();
        enables.erase(std::remove_if(enables.begin(), enables.end(),
            [id](const EnableEntry& e){ return e.id == id; }), enables.end());

        auto& disables = getDisableCallbacks();
        disables.erase(std::remove_if(disables.begin(), disables.end(),
            [id](const DisableEntry& e){ return e.id == id; }), disables.end());

        auto& loops = getLoopCallbacks();
        loops.erase(std::remove_if(loops.begin(), loops.end(),
            [id](const LoopEntry& e){ return e.id == id; }), loops.end());
    }

    void unregisterSubSystem(const SubSystemIDs& ids) {
        if (!ids.name.empty()) {
            VexLockGuard lock(getRegistryMutex());
            auto& subs = getRegisteredSubsystems();
            subs.erase(
                std::remove(subs.begin(), subs.end(), ids.name),
                subs.end()
            );
        }

        unregister(ids.initId);
        unregister(ids.startId);
        unregister(ids.enableId);
        unregister(ids.disableId);
        unregister(ids.loopId);
    }

    // ==========================================
    // 生命週期派發 (Dispatchers)
    // ==========================================

    static void dispatchInit() {
        std::vector<InitEntry> copy;
        {
            VexLockGuard lock(getRegistryMutex());
            copy = getInitCallbacks();
        }
        for(auto& entry : copy) {
            entry.cb();
        }
    }

    static void dispatchStart() {
        std::vector<StartEntry> copy;
        {
            VexLockGuard lock(getRegistryMutex());
            copy = getStartCallbacks();
        }
        for(auto& entry : copy) {
            entry.cb();
        }
    }

    static void dispatchEnable() {
        std::vector<EnableEntry> copy;
        {
            VexLockGuard lock(getRegistryMutex());
            copy = getEnableCallbacks();
        }
        for(auto& entry : copy) {
            entry.cb();
        }
    }

    static void dispatchDisable() {
        std::vector<DisableEntry> copy;
        {
            VexLockGuard lock(getRegistryMutex());
            copy = getDisableCallbacks();
        }
        for(auto& entry : copy) {
            entry.cb();
        }
    }

    static void dispatchLoop() {
        int currentTime = vex::timer::system();
        std::vector<LoopEntry> copy;
        {
            VexLockGuard lock(getRegistryMutex());
            copy = getLoopCallbacks();
        }

        for(auto& entry : copy) {
            if(currentTime - entry.lastRunTime >= entry.interval) {
                int dt = currentTime - entry.lastRunTime;

                // 執行時，mutex 是處於未鎖定狀態的，所以 callback 內部呼叫 register/unregister 是安全的
                entry.cb(currentTime, dt);

                // 執行完畢後寫回時間戳，必須加鎖確保寫回時此項目尚未被註銷
                VexLockGuard lock(getRegistryMutex());
                auto& loops = getLoopCallbacks();
                auto it = std::find_if(loops.begin(), loops.end(),
                                       [&](const LoopEntry& e){ return e.id == entry.id; });
                if(it != loops.end()) {
                    it->lastRunTime = currentTime;
                }
            }
        }
    }

    // ==========================================
    // VEX 系統回調 (VEX Callbacks)
    // ==========================================

    static int backgroundMonitorTask() {
        while (true) {
            bool isDisabled = !s_comp.isEnabled();

            // 處理進入 Disable 狀態的 disable 回調
            if (isDisabled && !s_wasDisabled) {
                dispatchDisable();
            }
            // 處理離開 Disable 狀態的 enable 回調
            else if (!isDisabled && s_wasDisabled) {
                dispatchEnable();
            }

            s_wasDisabled = isDisabled;
            vex::this_thread::sleep_for(50);
        }
        return 0;
    }

    static void onAutonomous() {
        while (s_comp.isAutonomous() && s_comp.isEnabled()) {
            if (autoOp) autoOp();
            vex::this_thread::sleep_for(10);
        }
    }

    static void onUsercontrol() {
        while (s_comp.isDriverControl() && s_comp.isEnabled()) {
            if (teleOp) teleOp();
            vex::this_thread::sleep_for(10);
        }
    }

    // ==========================================
    // 系統控制與進入點
    // ==========================================

    void pause() {
        s_paused = true;
    }

    void resume() {
        s_paused = false;
    }

    bool isPaused() {
        return s_paused.load();
    }

    void raise(const char* formattedMsg) {
        // 使用另外一個鎖先搶佔寫入權，防止其他線程競爭寫入
        if (!s_raiseLock.exchange(true)) {
            // 安全地寫入 string，此時主線程尚未被通知
            s_fatalErrorMsg = formattedMsg;

            // 寫入完成後，正式通知主線程接管
            s_hasFatalError.store(true);
        }

        // 中止所有其他 VEX 任務，確保機器人完全停止動作。
        // 若是非主線程 (如背景任務) 呼叫 raise，則執行到這裡時該任務會直接被 VEX 系統砍掉，不會再往下執行。
        // 若是主線程 (Application::run) 呼叫 raise，因為 stopAll 不會中止主線程，
        // 所以主線程會平安 return，並回到外層的 run() 迴圈負責畫圖。
        vex::task::stopAll();
    }

    void run() {
        s_comp.autonomous(onAutonomous);
        s_comp.drivercontrol(onUsercontrol);

        // 執行 pre-auton 邏輯
        dispatchInit();
        dispatchStart();

        // 將初始狀態設為 disabled
        s_wasDisabled = !s_comp.isEnabled();
        // 如果一開機就處於 enabled 狀態（例如直接在手把執行非賽事模式），則手動呼叫 enable
        if (!s_wasDisabled) {
            dispatchEnable();
        }

        // 啟動 Disable 監聽背景任務
        s_backgroundTask = new vex::task(backgroundMonitorTask);

        // 主迴圈完全轉作子系統生命週期派發，持續運作，不受 auto/teleop 切換影響
        while (true) {
            // 如果發生致命錯誤，主線程完全接管控制權
            if (s_hasFatalError.load()) {
                // 中止所有其他 VEX 背景任務，確保硬體停止
                vex::task::stopAll();

                // 由主線程最後一次親自清空畫面並輸出紅色錯誤訊息
                mainBrain.Screen.clearScreen(vex::color::red);
                mainBrain.Screen.setCursor(1, 1);
                mainBrain.Screen.print("FATAL ERROR:");
                mainBrain.Screen.setCursor(2, 1);
                mainBrain.Screen.print(s_fatalErrorMsg.c_str());

                mainController.Screen.clearScreen();
                mainController.Screen.setCursor(1, 1);
                mainController.Screen.print("FATAL ERROR");
                mainController.Screen.setCursor(2, 1);
                mainController.Screen.print(s_fatalErrorMsg.c_str());

                // 主線程進入無窮迴圈卡死，不讓程式回到 VEXos 主畫面
                while (true) {
                    vex::this_thread::sleep_for(100);
                }
            }

            if (!s_paused.load()) {
                dispatchLoop();
            }
            vex::this_thread::sleep_for(5); // 設定 5ms 高頻掃描，由 dispatchLoop 內部判斷 interval
        }
    }

} // namespace Application
} // namespace gcvex