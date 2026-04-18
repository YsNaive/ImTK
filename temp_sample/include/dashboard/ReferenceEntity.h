#pragma once

#include "dashboard/DashEntity.h"
#include "dashboard/Registry.h"
#include "dashboard/DashEntityHandler.h"
#include <vector>
#include <cstdint>

namespace gcvex {

    // ==========================================
    // ReferenceEntity Template
    // ==========================================
    // Base template for complex C# classes or hardware objects (e.g., Motors, Odometry).
    // It relies on Opcodes to differentiate full synchronization from custom actions.
    template <typename T>
    class ReferenceEntity : public DashEntity {
    private:
        static void* staticCreateData() {
            return DashEntityHandler<T>::createDataInstance();
        }

        static std::vector<uint8_t> staticSerialize(const void* dataPtr, uint8_t opcode) {
            return DashEntityHandler<T>::serialize(dataPtr, opcode);
        }

        static void staticReceive(void* dataPtr, uint8_t opcode, const std::vector<uint8_t>& data) {
            DashEntityHandler<T>::deserialize(dataPtr, opcode, data);
        }

    public:
        // Constructor to bind a new hardware instance or complex object.
        // Passes the targetPtr to the Registry, taking over pointer management.
        ReferenceEntity(const std::string& path, T* targetPtr) : DashEntity() {
            this->initBinding(
                path,
                DashEntityHandler<T>::get_typeID(),
                nullptr, // createDataFunc is not needed since we supply the instance
                &staticSerialize,
                &staticReceive,
                static_cast<void*>(targetPtr)
            );
        }

        // Constructor to retrieve an existing proxy.
        // If the path isn't registered, it will attempt to call staticCreateData(),
        // which for unspecialized Reference Entities will trigger Debug::raise.
        ReferenceEntity(const std::string& path) : DashEntity() {
            this->initBinding(
                path,
                DashEntityHandler<T>::get_typeID(),
                &staticCreateData,
                &staticSerialize,
                &staticReceive,
                nullptr
            );
        }

        ReferenceEntity(const ReferenceEntity& other) = default;
        virtual ~ReferenceEntity() = default;

        // Fetch the underlying pointer directly from the Registry O(1)
        T* get() const {
            return static_cast<T*>(Dashboard::Registry::getDataInstance(m_id));
        }

        // Updates the backend pointer in the Registry
        void set(T* ptr) {
            Dashboard::Registry::setDataInstance(m_id, static_cast<void*>(ptr));
        }

        // Syntactic sugar to interact directly with the hardware/object
        T* operator->() const {
            return get();
        }
    };

} // namespace gcvex
