#pragma once

// ==========================================
// [1] 全局運行模式 (Global Build Profile)
// ==========================================
// 1: 開發模式 (啟用調試、日誌、Dashboard 等輔助工具)
// 0: 競賽模式 (關閉一切非必要資源消耗，確保最高效能)
#define DEVELOPMENT_MODE 1

// ==========================================
// [2] 核心模組開關 (Core Module Toggles)
// ==========================================
#if DEVELOPMENT_MODE
    #define ENABLE_DEBUG 1
    #define ENABLE_DASHBOARD 1
#else
    // 競賽模式下強制關閉所有輔助模組
    #define ENABLE_DEBUG 0
    #define ENABLE_DASHBOARD 0
#endif

// ==========================================
// [3] 詳細參數配置與相依性檢查 (Dependencies)
// ==========================================

// --- Debug 模組 ---
#if ENABLE_DEBUG
    #define ENABLE_BRAIN_LOG 1
    #define ENABLE_CONTROLLER_LOG 1
#else
    #define ENABLE_BRAIN_LOG 0
    #define ENABLE_CONTROLLER_LOG 0
#endif

// --- Dashboard 模組 ---
#if ENABLE_DASHBOARD
    #define DASHBOARD_DISPATCH_HZ 8
    #define DASHBOARD_MAX_PAYLOAD 150
    // 將競賽流程控制權交由外部 C# 端 (開啟後實體搖桿與場地控制器將無效)
    #define ENABLE_VIRTUAL_COMPETITION 1
#else
    #define DASHBOARD_DISPATCH_HZ 8
    #define DASHBOARD_MAX_PAYLOAD 150
    #define ENABLE_VIRTUAL_COMPETITION 0
#endif
