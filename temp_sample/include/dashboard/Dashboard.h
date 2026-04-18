#pragma once
#include "dashboard/DashCmd.h"

namespace gcvex {
namespace Dashboard {

    void queueCommand(DashCmd cmd);
    void queueCommandFront(DashCmd cmd);

} // namespace Dashboard
} // namespace gcvex
