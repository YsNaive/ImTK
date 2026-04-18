#include "robotic/PoseProvider.h"

namespace gcvex {

    PoseProvider::PoseProvider(int interval_ms)
        : Application::ISubSystem(SR::SubSystem::PoseProvider, interval_ms) {
        // Constructor body left intentionally blank.
        // ISubSystem handles the initialization with the provided string constant.
    }

} // namespace gcvex