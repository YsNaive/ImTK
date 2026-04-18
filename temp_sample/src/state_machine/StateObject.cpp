#include <utility>

#include "state_machine/StateObject.h"

using namespace gcvex;

StateObject& StateObject::branch(StateObject *nextState)
{
    nextStates.push_back(nextState);
    return *nextState;
}

StateObject& StateObject::branch(StateObject& nextState)
{
    nextStates.push_back(&nextState);
    return nextState;
}

StateObject& StateObject::branch(StateObject&& nextState)
{
    auto ptr = nextState.make_shared();
    m_ownedStates.push_back(ptr);
    nextStates.push_back(ptr.get());
    return *ptr;
}

StateObject& StateObject::then(StateObject *nextState)
{
    return this->branch(nextState);
}

StateObject& StateObject::then(StateObject& nextState)
{
    return this->branch(nextState);
}

StateObject& StateObject::then(StateObject&& nextState)
{
    return this->branch(std::move(nextState));
}

StateObject& StateObject::add(StateObject *nextState)
{
    this->branch(nextState);
    return *this;
}

StateObject& StateObject::add(StateObject& nextState)
{
    this->branch(nextState);
    return *this;
}

StateObject& StateObject::add(StateObject&& nextState)
{
    this->branch(std::move(nextState));
    return *this;
}