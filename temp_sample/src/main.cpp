/*----------------------------------------------------------------------------*/
/*                                                                            */
/*    Module:       main.cpp                                                  */
/*    Author:       gcvex                                                     */
/*    Created:      2/1/2026                                                  */
/*    Description:  V5 project entry point. Handled entirely by Application.  */
/*                                                                            */
/*----------------------------------------------------------------------------*/

#include "vex.h"
#include "framework/Application.h"

int main() {
    // 啟動主程式生命週期管理器
    // 所有的 SubSystem 與 Callback 皆受 gcvex::Application 管理
    gcvex::Application::run();
    return 0;
}
