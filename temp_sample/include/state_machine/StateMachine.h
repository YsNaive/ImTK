#pragma once

#include <vector>
#include "state_machine/fd.h"

namespace gcvex {

// 單執行緒設計狀態機，非執行緒安全 (Not Thread-Safe)
class StateMachine {
    using StateList = std::vector<StateObject*>;
private:
    StateList m_activeStates = StateList();  // 當前正在執行的狀態集合
    StateList m_pendingAdd = StateList();    // 待新增的狀態
    StateList m_pendingRemove = StateList(); // 待移除的狀態

    // 雙緩衝區，用於 update() 內部，避免記憶體重新分配
    StateList m_processingAdd = StateList();
    StateList m_processingRemove = StateList();

    // 內部輔助函數：檢查容器中是否已包含該狀態
    bool is_contain_state(const StateList& container, StateObject* state) const;
    void add_if_not_exist(StateList& container, StateObject* state);

public:
    StateMachine() = default;
    ~StateMachine() = default;

    void update(); // 更新狀態機
    void add(StateObject* state); // 添加一個狀態到狀態機，會在下一次 update 時執行
    void interrupt(StateObject* state); // 中斷一個正在執行的狀態，會立即停止它的執行
    void done(StateObject* state); // 完成一個正在執行的狀態，會立即停止它的執行並觸發後續狀態
};

} // namespace gcvex