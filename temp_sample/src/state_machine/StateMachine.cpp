#include "state_machine/StateMachine.h"
#include "state_machine/StateObject.h"
#include "vex.h"
#include <algorithm>

namespace gcvex {

bool StateMachine::is_contain_state(const StateList& container, StateObject* state) const {
    return std::find(container.begin(), container.end(), state) != container.end();
}

void StateMachine::add_if_not_exist(StateList& container, StateObject* state) {
    if (!is_contain_state(container, state)) {
        container.push_back(state);
    }
}

void StateMachine::update() {
    int currentTime = vex::timer::system();

    // 1. 交換 Pending 的狀態到 Processing 緩衝區。這是一個 O(1) 操作，且兩個 Vector 都可以保留原有的 Capacity，達成 0 Allocation。
    // 在 on_exit/on_enter 裡面呼叫 add/done 的狀態會被加到清空後的 m_pending，留到下一幀處理。
    m_processingAdd.swap(m_pendingAdd);
    m_processingRemove.swap(m_pendingRemove);

    // 2. 先處理待刪除的狀態，確保 on_exit 發生在下一個狀態的 on_enter 之前 (避免馬達資源等衝突)
    for (auto* state : m_processingRemove) {
        auto it = std::find(m_activeStates.begin(), m_activeStates.end(), state);
        if (it != m_activeStates.end()) {
            // 判斷是否是被強制中斷：如果是被中斷，此時 state 的 isDone() 和 task done 應該都不是 true。
            bool interrupted = false;
            if (state->mode == StateObject::Mode::Loop) {
                interrupted = !state->isDone();
            } else {
                interrupted = !(state->m_isTaskDone->load(std::memory_order_acquire));
            }

            state->on_exit(currentTime, interrupted);
            state->m_owner = nullptr;

            // 如果背景任務還在執行且這是一個 ONCE 模式的狀態，嘗試停止任務以避免指標懸掛存取
            if (state->mode == StateObject::Mode::Once && state->m_bgTask) {
                state->m_bgTask->stop();
                state->m_bgTask.reset();
            }

            m_activeStates.erase(it);
        }
    }
    m_processingRemove.clear(); // 處理完畢，清空指標但保留 Capacity 給下一幀

    // 3. 再處理待新增的狀態
    for (auto* state : m_processingAdd) {
        if (!is_contain_state(m_activeStates, state)) {
            state->m_owner = this;
            state->m_enterTime_ms = currentTime;
            state->m_lastUpdateTime_ms = currentTime;
            state->m_isTaskDone->store(false, std::memory_order_relaxed);
            m_activeStates.push_back(state);
            state->on_enter(currentTime);

            // 如果是 ONCE 模式，啟動背景任務執行
            if (state->mode == StateObject::Mode::Once) {
                // 將 task 對象存在 state 內部，如果 state 被中斷，我們可以直接強制停止 task
                state->m_bgTask = std::make_shared<vex::task>([](void* arg) -> int {
                    auto* s = static_cast<StateObject*>(arg);
                    s->execute(vex::timer::system(), 0);
                    // 安全使用 shared_ptr 解除懸空寫入的風險
                    if (s && s->m_isTaskDone) {
                        s->m_isTaskDone->store(true, std::memory_order_release);
                    }
                    return 0;
                }, state);
            }
        }
    }
    m_processingAdd.clear(); // 處理完畢，清空指標但保留 Capacity 給下一幀

    // 4. 複製 activeStates 進行 Update 跌代，避免在 execute 或 on_timeout 中改變 set 導致跌代器失效
    StateList currentStates = m_activeStates;

    // 5. 進行 Update 跌代
    for (auto* state : currentStates) {
        // 確認此狀態是否已經在剛才被加入 pendingRemove (可能是在其他狀態的 on_enter/execute 中被中斷)
        if (is_contain_state(m_pendingRemove, state)) continue;

        int deltaTime = currentTime - state->m_lastUpdateTime_ms;

        // 檢查 Timeout
        if (state->timeout_ms > 0 && (currentTime - state->m_enterTime_ms) >= state->timeout_ms) {
            state->on_timeout(currentTime);

            if (state->continueOnTimeout) {
                this->done(state); // 正常完成，並觸發後續狀態
            } else {
                this->interrupt(state); // 強制中止，不觸發後續狀態
            }
            continue;
        }

        if (state->mode == StateObject::Mode::Loop) {
            // LOOP 模式：檢查 Update Interval
            if (deltaTime >= state->updateInterval_ms) {
                state->execute(currentTime, deltaTime);
                state->m_lastUpdateTime_ms = currentTime;
            }
        }

        // 檢查是否完成
        // 使用者可能在 execute 中完成了一些邏輯並使其 isDone() 為 true
        // 或者對於 ONCE 模式，背景任務已標記 m_isTaskDone
        if (state->isDone() || (state->mode == StateObject::Mode::Once && state->m_isTaskDone->load(std::memory_order_acquire))) {
            this->done(state); // 標記為完成，排入下一幀移除並啟動 next
        }
    }
}

void StateMachine::add(StateObject* state) {
    if (!state) return;
    add_if_not_exist(m_pendingAdd, state);
}

void StateMachine::interrupt(StateObject* state) {
    if (!state) return;
    add_if_not_exist(m_pendingRemove, state);
}

void StateMachine::done(StateObject* state) {
    if (!state) return;

    if (!is_contain_state(m_pendingRemove, state)) {
        m_pendingRemove.push_back(state);
        // 將後續狀態直接塞入 pendingAdd
        for (auto* next : state->nextStates) {
            add_if_not_exist(m_pendingAdd, next);
        }
    }
}

} // namespace gcvex