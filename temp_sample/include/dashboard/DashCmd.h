#pragma once

#include <string>
#include <vector>
#include <cstdint>

namespace gcvex {
namespace Dashboard {

    struct DashCmd {
        std::string identifier;
        std::vector<uint8_t> payload;
    };

    class DashCmdProvider {
    public:
        virtual ~DashCmdProvider() = default;
        virtual DashCmd build() const = 0;
    };

    // ==========================================
    // Binary Protocol Commands
    // ==========================================

    class CmdReset : public DashCmdProvider {
    public:
        DashCmd build() const override;
    };

    class CmdCreateEntity : public DashCmdProvider {
    public:
        CmdCreateEntity(uint8_t entityId, uint8_t typeId, const std::string& path);
        DashCmd build() const override;
    private:
        uint8_t m_entityId;
        uint8_t m_typeId;
        std::string m_path;
    };

    class CmdSyncEntity : public DashCmdProvider {
    public:
        CmdSyncEntity(uint8_t entityId, const std::vector<uint8_t>& data);
        DashCmd build() const override;
    private:
        uint8_t m_entityId;
        std::vector<uint8_t> m_data;
    };

} // namespace Dashboard
} // namespace gcvex
