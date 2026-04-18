#include "robot_config.h"
#include "framework/Application.h"
#include "framework/Debug.h"
#include "dashboard/ValueEntity.h"
#include "dashboard/DashEntityHandler.h"
#include <iostream>
#include <string>

namespace robot_config {
using namespace gcvex;
    // 實例化機器人硬體與全域設定
    // 範例：
    // vex::motor left_motor(vex::PORT1, vex::ratio18_1, false);
    // auto test_sys = Application::registerSubSystem("Debug test",
    //     [](){ Debug::log("Init    at %d ms", vex::timer::system()); },
    //     [](){ Debug::log("Start   at %d ms", vex::timer::system()); },
    //     [](){ Debug::log("Enable  at %d ms", vex::timer::system()); },
    //     [](){ Debug::log("Disable at %d ms", vex::timer::system()); },
    //     [](int time,int dt){ Debug::log("Loop at %d ms (dt: %d ms)", time, dt); },
    //         200 // 1000ms 執行一次
    //     );
    
    // auto test_init = Application::registerInit([]() {
    //     Application::autoOp = []() {
    //         Debug::log("Running auto at %d ms", vex::timer::system());
    //     };
    //     Application::teleOp = []() {
    //         Debug::raise("Teleop error at %d ms", vex::timer::system());
    //         Debug::log("Running teleop at %d ms", vex::timer::system());
    //     };
    // });

    // auto test_loop = Application::registerLoop([](int time, int dt) {
    //     Debug::log("Time: %d ms, Delta: %d ms", time, dt);
        
    // }, 20); // 20ms 執行一次

    ValueEntity<float> testEntity0("float");
    ValueEntity<int32_t> testEntity1("int1");
    ValueEntity<int32_t> testEntity2("int2");
    ValueEntity<bool> testEntity3("bool");

    auto init = Application::registerInit([]() {
        testEntity0.set(3.14);
        testEntity1.set(42);
        testEntity2.set(42);
        testEntity3.set(true);
    });

    auto loop = Application::registerLoop([](int time, int dt) {
        
        
    }, 100);

} // namespace robot_config
