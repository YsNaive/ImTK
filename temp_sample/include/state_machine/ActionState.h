#pragma once

#include "state_machine/StateObject.h"
#include <functional>

namespace gcvex {

/**
 * @brief 允許傳入 lambda 函式來建立單次執行的狀態，常用於包裹原本的阻塞式副程式 (如 loadPlateUp() 或是 wait)。
 * 此狀態將以 Mode::Once 模式執行，允許在 lambda 內使用阻塞呼叫。
 */
class ActionState : public StateObject {
public:
    using ActionFunc = std::function<void()>;

    ActionState(ActionFunc action, int delay_ms = 0)
        : m_action(action), m_delay_ms(delay_ms) {
        mode = Mode::Once;
    }

protected:
    void execute(int time, int deltaTime) override {
        if (m_delay_ms > 0) {
            vex::this_thread::sleep_for(m_delay_ms);
        }
        if (m_action) {
            m_action();
        }
    }

    bool isDone() const override {
        return false; // Mode::Once 任務交由 StateMachine 在背景任務結束後透過 m_isTaskDone 判斷
    }

    std::shared_ptr<StateObject> make_shared() const override {
        return std::make_shared<ActionState>(*this);
    }

private:
    ActionFunc m_action;
    int m_delay_ms;
};

} // namespace gcvex