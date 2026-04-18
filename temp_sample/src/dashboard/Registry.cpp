#include "dashboard/Registry.h"
#include "dashboard/DashEntity.h"
#include "framework/Debug.h"
#include "dashboard/Dashboard.h"
#include <unordered_map>

namespace gcvex {
namespace Dashboard {
namespace Registry {

    namespace {
        struct EntityEntry {
            uint8_t typeID;
            std::string path;
            void* dataInstance;

            SerializeFunc serialize;
            ReceiveFunc receive;
        };

        // Meyers' Singleton to prevent Static Initialization Order Fiasco (SIOF)
        std::unordered_map<std::string, uint8_t>& getPathToIDMap() {
            static std::unordered_map<std::string, uint8_t> instance;
            return instance;
        }

        EntityEntry* getEntriesArray() {
            static EntityEntry instance[256];
            return instance;
        }

        // Properly encapsulate ID generation to prevent external manipulation
        uint16_t& getNextIDRef() {
            static uint16_t instance = 0;
            return instance;
        }

        uint8_t allocateNextID() {
            uint16_t& nextID = getNextIDRef();
            if (nextID > 255) {
                gcvex::Debug::raise("Dashboard Registry: Maximum of 256 entities reached.");
            }
            return static_cast<uint8_t>(nextID++);
        }
    } // anonymous namespace

    uint8_t getOrRegister(
        const std::string& path,
        uint8_t typeID,
        void* (*createDataFunc)(),
        SerializeFunc serializeFunc,
        ReceiveFunc receiveFunc,
        void* existingDataPtr
    ) {
        if (path.length() > 50) {
            gcvex::Debug::raise(("Dashboard Registry: Path length exceeds 50 characters: " + path).c_str());
        }

        auto& s_pathToID = getPathToIDMap();
        auto* s_entries = getEntriesArray();

        auto it = s_pathToID.find(path);
        if (it != s_pathToID.end()) {
            uint8_t existingId = it->second;
            // Ensure type matches
            if (s_entries[existingId].typeID != typeID) {
                gcvex::Debug::raise(("Dashboard Registry: Type mismatch for path " + path).c_str());
            }
            return existingId;
        }

        uint8_t newId = allocateNextID();
        s_pathToID[path] = newId;

        s_entries[newId].typeID = typeID;
        s_entries[newId].path = path;
        s_entries[newId].serialize = serializeFunc;
        s_entries[newId].receive = receiveFunc;

        if (existingDataPtr) {
            s_entries[newId].dataInstance = existingDataPtr;
        } else if (createDataFunc) {
            s_entries[newId].dataInstance = createDataFunc();
        } else {
            s_entries[newId].dataInstance = nullptr;
        }

        // Broadcast the newly created entity metadata directly
        Dashboard::queueCommand(CmdCreateEntity(newId, typeID, path).build());

        return newId;
    }

    void* getDataInstance(uint8_t id) {
        if (id >= getNextIDRef()) {
            gcvex::Debug::raise("Dashboard Registry: Invalid ID lookup.");
        }
        return getEntriesArray()[id].dataInstance;
    }

    std::string getPath(uint8_t id) {
        if (id >= getNextIDRef()) {
            gcvex::Debug::raise("Dashboard Registry: Invalid ID lookup for path.");
        }
        return getEntriesArray()[id].path;
    }

    void setDataInstance(uint8_t id, void* dataPtr) {
        if (id >= getNextIDRef()) {
            gcvex::Debug::raise("Dashboard Registry: Invalid ID for setDataInstance.");
        }
        getEntriesArray()[id].dataInstance = dataPtr;
    }

    void send(uint8_t id, uint8_t opcode) {
        if (id >= getNextIDRef()) return;

        auto* s_entries = getEntriesArray();
        if (s_entries[id].serialize && s_entries[id].dataInstance) {
            std::vector<uint8_t> payload = s_entries[id].serialize(s_entries[id].dataInstance, opcode);
            // Payload chunking is stripped, relying on CmdQueue system
            if (opcode == 0x00) {
                Dashboard::queueCommand(CmdSyncEntity(id, payload).build());
            } else {
                std::vector<uint8_t> fullPayload = {opcode};
                fullPayload.insert(fullPayload.end(), payload.begin(), payload.end());
                Dashboard::queueCommand(CmdSyncEntity(id, fullPayload).build());
            }
        }
    }

    void receive(uint8_t id, uint8_t opcode, const std::vector<uint8_t>& data) {
        if (id >= getNextIDRef()) return;

        auto* s_entries = getEntriesArray();
        if (s_entries[id].receive && s_entries[id].dataInstance) {
            s_entries[id].receive(s_entries[id].dataInstance, opcode, data);
        }
    }

    void fetchEntity(uint8_t id) {
        if (id >= getNextIDRef()) return;

        auto* s_entries = getEntriesArray();
        Dashboard::queueCommand(CmdCreateEntity(id, s_entries[id].typeID, s_entries[id].path).build());
        send(id, 0x00);
    }

    void queueAllConfigurations() {
        uint16_t currentMax = getNextIDRef();
        auto* s_entries = getEntriesArray();
        for (uint16_t i = 0; i < currentMax; ++i) {
            Dashboard::queueCommand(CmdCreateEntity(static_cast<uint8_t>(i), s_entries[i].typeID, s_entries[i].path).build());
        }
    }

} // namespace Registry
} // namespace Dashboard
} // namespace gcvex
