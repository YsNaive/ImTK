#include "framework/ISubSystem.h"
#include "framework/Application.h"

namespace gcvex {
namespace Application {

    ISubSystem::ISubSystem(const std::string& name, int interval_ms)
        : m_name(name) {

        // 將自己的虛擬函式綁定成 callback，透過 lambda 轉接
        m_ids = gcvex::Application::registerSubSystem(
            m_name,
            [this]() { this->init(); },
            [this]() { this->start(); },
            [this]() { this->enable(); },
            [this]() { this->disable(); },
            [this](int time, int dt) { this->loop(time, dt); },
            interval_ms
        );
    }

    ISubSystem::~ISubSystem() {
        // 在解構時自動註銷所有的 callback，防止懸空指標 (dangling pointer)
        gcvex::Application::unregisterSubSystem(m_ids);
    }

} // namespace Application
} // namespace gcvex