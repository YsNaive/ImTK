#pragma once
#include <functional>
#include <string>
#include "vex.h"

namespace gcvex {
namespace Application {

    // ==========================================
    // 全域硬體資源 (Hardware Resources)
    // ==========================================

    // 主機
    extern vex::brain mainBrain;

    // 主控制器 (手把)
    extern vex::controller mainController;

    // ==========================================
    // 全域操作模式 (Operations)
    // ==========================================

    // 指向目前的 Autonomous 函數
    extern std::function<void()> autoOp;

    // 指向目前的 Teleop (Driver Control) 函數
    extern std::function<void()> teleOp;

    // ==========================================
    // 動態生命週期註冊 API (Lifecycle Registration)
    // ==========================================

    // 註冊初始化回調 (pre_auton 執行，用以初始化本地資源)，回傳唯一 ID
    int registerInit(std::function<void()> cb);

    // 註冊啟動回調 (pre_auton 執行，用以綁定其他模組或資源)，回傳唯一 ID
    int registerStart(std::function<void()> cb);

    // 註冊啟用回調 (進入 auto 或 teleop 狀態時執行，用以重置數值)，回傳唯一 ID
    int registerEnable(std::function<void()> cb);

    // 註冊禁用回調 (退出模式或 disable 時執行)，回傳唯一 ID
    int registerDisable(std::function<void()> cb);

    // 註冊迴圈回調 (主迴圈中持續執行，不受 enable 狀態影響)，回傳唯一 ID
    int registerLoop(std::function<void(int time, int dt)> cb, int interval_ms = 20);

    // 快捷註冊一整個子系統的生命週期，回傳可用於一次性全部卸載的 ID 陣列
    struct SubSystemIDs {
        std::string name;
        int initId = -1;
        int startId = -1;
        int enableId = -1;
        int disableId = -1;
        int loopId = -1;
    };

    SubSystemIDs registerSubSystem(const std::string& name,
                                   std::function<void()> init,
                                   std::function<void()> start,
                                   std::function<void()> enable,
                                   std::function<void()> disable,
                                   std::function<void(int time, int dt)> loop,
                                   int interval_ms = 20);

    // 卸載指定的註冊項目
    void unregister(int id);
    void unregisterSubSystem(const SubSystemIDs& ids);

    // ==========================================
    // 系統控制
    // ==========================================

    // 暫停主迴圈派發任何 Loop 回調
    void pause();

    // 恢復主迴圈派發 Loop 回調
    void resume();

    // 檢查主迴圈是否已暫停
    bool isPaused();

    // 觸發致命錯誤 (Fatal Error)，完全接管主線程並停止所有背景程式碼
    void raise(const char* formattedMsg);

    // 接管 VEX 生命週期並進入無窮迴圈
    void run();

} // namespace Application
} // namespace gcvex