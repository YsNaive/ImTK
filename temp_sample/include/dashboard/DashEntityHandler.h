#pragma once

#include <vector>
#include <cstdint>
#include <cmath>
#include "framework/Debug.h"

namespace gcvex {

    // ==========================================
    // DashEntityHandler Template
    // ==========================================
    // Provides static methods to handle lifecycle and serialization for a specific type.
    // Unspecialized versions will trigger a Fail-Fast error via Debug::raise to ensure
    // new reference/hardware types are explicitly defined by developers.
    template <typename T>
    struct DashEntityHandler {
        static inline uint8_t get_typeID() {
            gcvex::Debug::raise("DashEntityHandler: get_typeID() called on unspecialized template.");
            return 0x00;
        }

        static inline void* createDataInstance() {
            gcvex::Debug::raise("DashEntityHandler: createDataInstance() called on unspecialized template.");
            return nullptr;
        }

        static inline std::vector<uint8_t> serialize(const void* dataPtr, uint8_t opcode) {
            gcvex::Debug::raise("DashEntityHandler: serialize() called on unspecialized template.");
            return std::vector<uint8_t>();
        }

        static inline void deserialize(void* dataPtr, uint8_t opcode, const std::vector<uint8_t>& data) {
            gcvex::Debug::raise("DashEntityHandler: deserialize() called on unspecialized template.");
        }
    };

    // ==========================================
    // Specialization: int (Dynamic Length Compression)
    // ==========================================
    template <>
    struct DashEntityHandler<int> {
        static inline uint8_t get_typeID() { return 0x01; }

        static inline void* createDataInstance() {
            return new int(0);
        }

        static inline std::vector<uint8_t> serialize(const void* dataPtr, uint8_t opcode) {
            const int* valuePtr = static_cast<const int*>(dataPtr);
            int value = valuePtr ? *valuePtr : 0;
            std::vector<uint8_t> data;

            // Dynamic length compression
            if (value >= -128 && value <= 127) {
                data.push_back(value & 0xFF);
            } else if (value >= -32768 && value <= 32767) {
                data.push_back(value & 0xFF);
                data.push_back((value >> 8) & 0xFF);
            } else {
                data.push_back(value & 0xFF);
                data.push_back((value >> 8) & 0xFF);
                data.push_back((value >> 16) & 0xFF);
                data.push_back((value >> 24) & 0xFF);
            }
            return data;
        }

        static inline void deserialize(void* dataPtr, uint8_t opcode, const std::vector<uint8_t>& data) {
            if (!dataPtr) return;
            int* valuePtr = static_cast<int*>(dataPtr);
            int val = 0;
            size_t len = data.size();
            if (len == 1) {
                int8_t v = data[0];
                val = v; // Sign extend
            } else if (len == 2) {
                int16_t v = data[0] | (data[1] << 8);
                val = v; // Sign extend
            } else if (len >= 4) {
                val = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
            }
            *valuePtr = val;
        }
    };

    // ==========================================
    // Specialization: float (x100 Compression)
    // ==========================================
    template <>
    struct DashEntityHandler<float> {
        static inline uint8_t get_typeID() { return 0x02; }

        static inline void* createDataInstance() {
            return new float(0.0f);
        }

        static inline std::vector<uint8_t> serialize(const void* dataPtr, uint8_t opcode) {
            const float* valuePtr = static_cast<const float*>(dataPtr);
            float value = valuePtr ? *valuePtr : 0.0f;
            int intVal = static_cast<int>(std::round(value * 100.0f));
            return DashEntityHandler<int>::serialize(&intVal, opcode);
        }

        static inline void deserialize(void* dataPtr, uint8_t opcode, const std::vector<uint8_t>& data) {
            if (!dataPtr) return;
            float* valuePtr = static_cast<float*>(dataPtr);
            int tempInt = 0;
            DashEntityHandler<int>::deserialize(&tempInt, opcode, data);
            *valuePtr = tempInt / 100.0f;
        }
    };

    // ==========================================
    // Specialization: bool
    // ==========================================
    template <>
    struct DashEntityHandler<bool> {
        static inline uint8_t get_typeID() { return 0x03; }

        static inline void* createDataInstance() {
            return new bool(false);
        }

        static inline std::vector<uint8_t> serialize(const void* dataPtr, uint8_t opcode) {
            const bool* valuePtr = static_cast<const bool*>(dataPtr);
            bool value = valuePtr ? *valuePtr : false;
            std::vector<uint8_t> data;
            data.push_back(value ? 0x01 : 0x00);
            return data;
        }

        static inline void deserialize(void* dataPtr, uint8_t opcode, const std::vector<uint8_t>& data) {
            if (!dataPtr) return;
            bool* valuePtr = static_cast<bool*>(dataPtr);
            if (data.empty()) {
                *valuePtr = false;
                return;
            }
            *valuePtr = (data[0] != 0);
        }
    };

} // namespace gcvex
