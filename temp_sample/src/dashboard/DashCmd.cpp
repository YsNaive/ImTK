#include "dashboard/DashCmd.h"
#include <cstdio>

namespace gcvex {
namespace Dashboard {

    DashCmd CmdReset::build() const {
        DashCmd cmd;
        cmd.identifier = "reset";
        cmd.payload.push_back(0x00);
        return cmd;
    }

    CmdCreateEntity::CmdCreateEntity(uint8_t entityId, uint8_t typeId, const std::string& path)
        : m_entityId(entityId), m_typeId(typeId), m_path(path) {}

    // Helper to safely convert uint8_t to string during static initialization
    static std::string idToString(uint8_t id) {
        if (id == 0) return "0";
        std::string result;
        while (id > 0) {
            result.insert(result.begin(), '0' + (id % 10));
            id /= 10;
        }
        return result;
    }

    DashCmd CmdCreateEntity::build() const {
        DashCmd cmd;
        // Avoid snprintf during static init which may not be fully initialized
        cmd.identifier = "ce_" + idToString(m_entityId);

        cmd.payload.push_back(0x05); // Create Entity Command ID
        cmd.payload.push_back(m_entityId);
        cmd.payload.push_back(m_typeId);
        cmd.payload.push_back(static_cast<uint8_t>(m_path.length()));
        for (char c : m_path) {
            cmd.payload.push_back(static_cast<uint8_t>(c));
        }
        return cmd;
    }

    CmdSyncEntity::CmdSyncEntity(uint8_t entityId, const std::vector<uint8_t>& data)
        : m_entityId(entityId), m_data(data) {}

    DashCmd CmdSyncEntity::build() const {
        DashCmd cmd;
        // Avoid snprintf during static init which may not be fully initialized
        cmd.identifier = "se_" + idToString(m_entityId);

        cmd.payload.push_back(0x06); // Sync Entity Command ID
        cmd.payload.push_back(m_entityId);

        size_t dataSize = m_data.size();
        if (dataSize < 255) {
            cmd.payload.push_back(static_cast<uint8_t>(dataSize));
        } else {
            // Escape sequence 0xFF for extended 16-bit length
            cmd.payload.push_back(0xFF);
            cmd.payload.push_back(static_cast<uint8_t>(dataSize & 0xFF));
            cmd.payload.push_back(static_cast<uint8_t>((dataSize >> 8) & 0xFF));
        }

        cmd.payload.insert(cmd.payload.end(), m_data.begin(), m_data.end());
        return cmd;
    }

} // namespace Dashboard
} // namespace gcvex
