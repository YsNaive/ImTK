#pragma once

#include <string>
#include <cstdint>
#include <vector>

namespace gcvex {
namespace Dashboard {
namespace Registry {

    typedef std::vector<uint8_t> (*SerializeFunc)(const void* dataPtr, uint8_t opcode);
    typedef void (*ReceiveFunc)(void* dataPtr, uint8_t opcode, const std::vector<uint8_t>& data);

    // Attempts to register a new path or retrieves the ID for an existing one.
    // Instantiates its data via createDataFunc if new, and stores function pointers
    // for seamless, stateless serialization calls later.
    uint8_t getOrRegister(
        const std::string& path,
        uint8_t typeID,
        void* (*createDataFunc)(),
        SerializeFunc serializeFunc,
        ReceiveFunc receiveFunc,
        void* existingDataPtr = nullptr
    );

    // O(1) array access to the actual data pointer via ID.
    void* getDataInstance(uint8_t id);

    // Reverse lookup path from ID
    std::string getPath(uint8_t id);

    // Updates the pointer backing an entity
    void setDataInstance(uint8_t id, void* dataPtr);

    // Pushes the payload (serialized using the stored function pointer with the given opcode)
    // into the Dashboard network queue. Opcode 0x00 is for general synchronization.
    void send(uint8_t id, uint8_t opcode);

    // Receives a payload and delegates it to the stored receive function pointer
    void receive(uint8_t id, uint8_t opcode, const std::vector<uint8_t>& data);

    // Helper method used when processing an incoming [V] Fetch Entity
    void fetchEntity(uint8_t id);

    // Enqueues all existing [C] Create Entity commands to the network queue
    void queueAllConfigurations();

} // namespace Registry
} // namespace Dashboard
} // namespace gcvex
