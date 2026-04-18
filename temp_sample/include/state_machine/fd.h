#pragma once

#include <unordered_set>

namespace gcvex {
    
class StateMachine; // 狀態機，用於執行狀態物件
class StateObject;  // 狀態物件預實作
template<typename T>
class TStateObject; // 包含 clone 的預實作

} // namespace gcvex