# 專案開發進度與 TODO 列表

## 🟢 進行中 (In Progress)
*(目前無，等待使用者指定下一個開發目標)*

## 🟡 待處理 (Planned)

### 1. Vex 接收端 (V5 Brain Receiving)
- [ ] **[Dashboard]** 實作序列埠資料讀取機制，解析來自 C# 端的二進位指令
- [ ] **[Dashboard]** 處理 `[0x06] Sync Entity`：根據 Entity ID 呼叫 `Registry::receive` 來反序列化並更新變數/硬體狀態
- [ ] **[Dashboard]** 處理 `[0x01] Fetch Entity`：當 C# 端要求重傳時，透過 `Registry::fetchEntity` 回傳最新的配置與狀態

### 2. C# 架構與擴充 (C# Frontend Architecture)
- [ ] **[Dashboard-C#]** 實作 Debug Log 層級系統 (取代 Console.WriteLine，方便追蹤通訊底層狀況)
- [ ] **[Dashboard-C#]** 實作逾時或掉包偵測，自動觸發 Fetch 要求重傳

### 3. C# 發送端 (C# Frontend Sending)
- [ ] **[Dashboard-C#]** 實作 C# 端的 Command Queue，將 UI 上的數值改變包裝為二進位指令
- [ ] **[Dashboard-C#]** 實作針對 Reference Entity 的自訂 Opcode 發送功能 (如手動觸發馬達轉動)

## 🔴 待討論 (Blocked)
*(無)*

## ✅ 近期完成 (Recently Completed)

### 1. C# 接收端 (C# Frontend Receiving)
- [x] **[Dashboard-C#]** 實作二進位封包解析器 (Packet Parser)，檢查 Checksum 與 `0xFF` Payload 擴充長度
- [x] **[Dashboard-C#]** 處理 `[0x05] Create Entity`：在 C# 記憶體中建立 ID 與 UI 物件 (VisualElement) 的映射，並實作 Reflection Factory
- [x] **[Dashboard-C#]** 處理 `[0x06] Sync Entity`：根據 `Type ID` 應用對應的動態長度壓縮與 Float x100 還原解壓縮邏輯
- [x] **[Dashboard-C#]** 實作 `ImTKModule` 佇列機制處理 WebSocket 背景跨執行緒問題
- [x] **[Dashboard-C#]** 實作 Lazy-Delete UI 最佳化與連線參數快取存取

### 0. Vex 發送端 (V5 Brain Sending)
- [x] **[Dashboard]** 架構完整：重構 Entity 系統套用 Proxy Pattern，將狀態收攏至 `Registry`
- [x] **[Dashboard]** 實作 `DashEntityHandler<T>`，支援 int, float, bool 的二進位壓縮與 Fail-Fast 安全機制
- [x] **[Dashboard]** 整合 Opcode 機制，區分 Value Entity 與 Reference Entity 的發送邏輯
