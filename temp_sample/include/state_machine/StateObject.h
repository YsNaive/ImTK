#pragma once

#include <vector>
#include <type_traits>
#include <memory>
#include <atomic>

#include "vex.h"
#include "state_machine/fd.h"

namespace gcvex {

class StateObject {
friend class StateMachine;
friend class ParallelState; // 允許 ParallelState 存取子狀態的內部成員
public:
    enum class Mode {
        Loop, // 預設模式：狀態機每個 Tick 呼叫 execute，非阻塞型
        Once  // 單次模式：狀態機將其 execute 放至背景任務 (Task) 執行，執行完畢後自動完成
    };

private:
    using NextContainer  = std::vector<StateObject*>;
    using OwnedContainer = std::vector<std::shared_ptr<StateObject>>;
    OwnedContainer m_ownedStates = OwnedContainer(); // 右值傳入的狀態，需要在 StateObject 的生命週期結束時刪除

protected:    
    StateMachine* m_owner = nullptr; // 當被 StateMachine 執行時，這個指標會指向執行它的 StateMachine，執行結束後清空
    int m_enterTime_ms = -1;          // 進入狀態的時間，單位為毫秒，-1 時代表未執行
    int m_lastUpdateTime_ms = -1;     // 上次更新狀態的時間，單位為毫秒，-1 時代表未執行
    std::shared_ptr<std::atomic<bool>> m_isTaskDone{std::make_shared<std::atomic<bool>>(false)}; // 當 Mode::Once 背景任務執行完畢時，會標記為 true，無鎖通知狀態機
    std::shared_ptr<vex::task> m_bgTask = nullptr; // 為了安全的生命週期管理，儲存背景任務的指針

    virtual void on_enter(int time) {} // 進入狀態時的回調，默認為空實現，子類可覆寫

    // 執行狀態的邏輯。
    // 如果是 Mode::Loop，每個 Tick 都會被呼叫，deltaTime 是距離上次呼叫的時間差。禁止阻塞。
    // 如果是 Mode::Once，只會被背景任務呼叫一次，deltaTime 固定為 0，允許阻塞型操作 (如 vex::wait)。
    virtual void execute(int time, int deltaTime) {}

    virtual void on_exit(int time, bool interrupted) {} // 離開狀態時的回調，interrupted 為 true 時代表被中斷
    virtual void on_timeout(int time) {} // 狀態超時時的回調，默認為空實現，子類可覆寫 
    virtual std::shared_ptr<StateObject> make_shared() const = 0; // 核心克隆介面

public:
    Mode mode = Mode::Loop; // 執行模式
    int updateInterval_ms = 20;   // 狀態更新的時間間隔，單位為毫秒
    int timeout_ms = -1;          // 狀態的超時時間，單位為毫秒，-1 代表不啟用
    bool continueOnTimeout = true; // 是否在超時後繼續觸發後續的狀態，默認為 true
    NextContainer nextStates = NextContainer(); // 這個狀態執行結束後會觸發哪些狀態，默認為空集
    
    StateObject() = default;
    virtual ~StateObject() = default; 

    // 複製建構子：必須處理 m_isTaskDone，確保新的物件擁有獨立的 shared_ptr
    StateObject(const StateObject& other)
        : m_ownedStates(other.m_ownedStates),
          m_owner(other.m_owner),
          m_enterTime_ms(other.m_enterTime_ms),
          m_lastUpdateTime_ms(other.m_lastUpdateTime_ms),
          mode(other.mode),
          updateInterval_ms(other.updateInterval_ms),
          timeout_ms(other.timeout_ms),
          continueOnTimeout(other.continueOnTimeout),
          nextStates(other.nextStates) {
        // 深拷貝 task done 標記，以避免不同實例互相影響
        m_isTaskDone = std::make_shared<std::atomic<bool>>(other.m_isTaskDone->load());
    }

    StateObject& operator=(const StateObject& other) {
        if (this != &other) {
            m_ownedStates = other.m_ownedStates;
            m_owner = other.m_owner;
            m_enterTime_ms = other.m_enterTime_ms;
            m_lastUpdateTime_ms = other.m_lastUpdateTime_ms;
            mode = other.mode;
            updateInterval_ms = other.updateInterval_ms;
            timeout_ms = other.timeout_ms;
            continueOnTimeout = other.continueOnTimeout;
            nextStates = other.nextStates;
            m_isTaskDone = std::make_shared<std::atomic<bool>>(other.m_isTaskDone->load());
        }
        return *this;
    }

    inline bool isActive() const { return m_owner != nullptr; }
    virtual bool isDone() const = 0;  // 判斷狀態是否完成，必須由子類實現，不須處理 timeout

    // 分支不等待 (Fork)
    StateObject& branch(StateObject* nextState); // 以指針的方式傳入狀態，鍊式回傳*分支的*狀態
    StateObject& branch(StateObject& nextState); // 以引用的方式傳入狀態，鍊式回傳*分支的*狀態
    StateObject& branch(StateObject&& nextState); // 以右值的方式傳入狀態，鍊式回傳*分支的*狀態，生命週期由內部管理

    StateObject& add(StateObject* nextState);  // 以指針的方式附加分支，鍊式回傳*當前*狀態
    StateObject& add(StateObject& nextState);  // 以引用的方式附加分支，鍊式回傳*當前*狀態
    StateObject& add(StateObject&& nextState);  // 以右值的方式附加分支，鍊式回傳*當前*狀態

    // 多重分支：觸發後平行執行，並回傳第一個分支節點
    template<typename... TArgs>
    StateObject& branch(TArgs&&... nextState){
        static_assert(sizeof...(TArgs) > 0, "[StateObject.branch] At least one argument is required.");
        StateObject* results[] = { &(branch(std::forward<TArgs>(nextState)))... };
        return *results[0];
    }

    // 將 then 保留給 Join/等待全部完成
    StateObject& then(StateObject* nextState);
    StateObject& then(StateObject& nextState);
    StateObject& then(StateObject&& nextState);

    // 多重並行等待 (Join)：將多個狀態包裹為一個 ParallelState 並等待它們全部完成，然後回傳該 ParallelState 以供後續串接
    template<typename... TArgs>
    StateObject& then(TArgs&&... nextState);
};

// ==========================================
// 並行群組狀態 (ParallelState)
// 用於包裹多個子狀態，等待它們全部 isDone() 才觸發自己的 nextStates
// ==========================================
class ParallelState : public StateObject {
private:
    std::vector<StateObject*> m_children;
    std::vector<bool> m_childrenDone;
    std::vector<std::shared_ptr<StateObject>> m_ownedChildren;

    void check_all_done() {
        bool allDone = true;
        for (size_t i = 0; i < m_children.size(); ++i) {
            if (!m_childrenDone[i]) {
                bool childDone = m_children[i]->isDone() ||
                    (m_children[i]->mode == Mode::Once && m_children[i]->m_isTaskDone->load(std::memory_order_acquire));

                if (childDone) {
                    m_childrenDone[i] = true;
                } else {
                    allDone = false;
                }
            }
        }
        if (allDone) {
            m_allDone = true;
        }
    }

public:
    bool m_allDone = false;

    ParallelState() {
        mode = Mode::Loop;
    }

    void addChild(StateObject* child) {
        m_children.push_back(child);
        m_childrenDone.push_back(false);
    }

    void addChild(StateObject&& child) {
        auto ptr = child.make_shared();
        m_ownedChildren.push_back(ptr);
        m_children.push_back(ptr.get());
        m_childrenDone.push_back(false);
    }

    void on_enter(int time) override {
        m_allDone = false;
        // 為了讓子狀態能夠更新，我們自己手動模擬將它們當成子狀態執行，或是我們可以把它們直接交給狀態機
        // 但因為我們希望自己控制何時才觸發自己的 nextStates，我們必須自己 tick 它們。
        for (size_t i = 0; i < m_children.size(); ++i) {
            m_childrenDone[i] = false;
            m_children[i]->m_owner = this->m_owner; // 借用 owner
            m_children[i]->on_enter(time);
        }
    }

    void execute(int time, int deltaTime) override {
        if (m_allDone) return;

        for (size_t i = 0; i < m_children.size(); ++i) {
            if (!m_childrenDone[i]) {
                if (m_children[i]->mode == Mode::Loop) {
                    m_children[i]->execute(time, deltaTime);
                } else if (m_children[i]->mode == Mode::Once && !m_children[i]->m_bgTask) {
                    // 如果子狀態是 ONCE，但還沒有被啟動，我們模擬 StateMachine 的行為，在背景幫它啟動
                    m_children[i]->m_bgTask = std::make_shared<vex::task>([](void* arg) -> int {
                        auto* s = static_cast<StateObject*>(arg);
                        // 取得系統時間
                        s->execute(vex::timer::system(), 0);
                        if (s && s->m_isTaskDone) {
                            s->m_isTaskDone->store(true, std::memory_order_release);
                        }
                        return 0;
                    }, m_children[i]);
                }
            }
        }
        check_all_done();
    }

    void on_exit(int time, bool interrupted) override {
        for (size_t i = 0; i < m_children.size(); ++i) {
            m_children[i]->on_exit(time, interrupted || !m_childrenDone[i]);
            m_children[i]->m_owner = nullptr;
        }
    }

    bool isDone() const override {
        return m_allDone;
    }

    std::shared_ptr<StateObject> make_shared() const override {
        return std::make_shared<ParallelState>(*this);
    }
};

template<typename... TArgs>
StateObject& StateObject::then(TArgs&&... nextState) {
    static_assert(sizeof...(TArgs) > 0, "[StateObject.then] At least one argument is required.");
    ParallelState pState;

    // 使用 initializer_list 技巧將所有傳入的狀態加入 ParallelState
    int dummy[] = { 0, (pState.addChild(std::forward<TArgs>(nextState)), 0)... };
    (void)dummy;

    // 將這個 ParallelState 當作右值附加，並將生命週期交給當前狀態，同時避免 memory leak
    return this->branch(std::move(pState));
}

template<typename T>
class TStateObject : public StateObject {
    std::shared_ptr<StateObject> make_shared() const override {
        return std::make_shared<T>(static_cast<const T&>(*this));
    }
};

} // namespace gcvex