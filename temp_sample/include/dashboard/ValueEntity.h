#pragma once

#include "dashboard/DashEntity.h"
#include "dashboard/Registry.h"
#include "dashboard/DashEntityHandler.h"
#include "math/Units.h" // For FLOAT_EPSILON or std::abs
#include <vector>
#include <cstdint>
#include <cmath>
#include <type_traits>

namespace gcvex {

    template <typename T>
    class ValueEntity : public DashEntity {
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

        // Helper to perform safe equality check. For floats, we only care about 2 decimal places.
        bool isValueEqual(const T& a, const T& b) const {
            if (std::is_floating_point<T>::value) {
                // Since the protocol relies on x100 compression for floats, we compare at that precision.
                return std::abs(a - b) < 0.01f;
            } else {
                return a == b;
            }
        }

    public:
        ValueEntity(const std::string& path) : DashEntity() {
            this->initBinding(
                path,
                DashEntityHandler<T>::get_typeID(),
                &staticCreateData,
                &staticSerialize,
                &staticReceive,
                nullptr // No existing data, Registry should call createDataFunc
            );
        }

        ValueEntity(const ValueEntity& other) = default;
        virtual ~ValueEntity() = default;

        // Get value directly from Registry
        T get() const {
            void* ptr = Dashboard::Registry::getDataInstance(m_id);
            if (!ptr) return T();
            return *static_cast<T*>(ptr);
        }

        // Set value directly to Registry with dirty check
        void set(T v) {
            void* ptr = Dashboard::Registry::getDataInstance(m_id);
            if (!ptr) return;
            T* tPtr = static_cast<T*>(ptr);
            if (!isValueEqual(*tPtr, v)) {
                *tPtr = v;
                send(0x00);
            }
        }
    };

} // namespace gcvex
