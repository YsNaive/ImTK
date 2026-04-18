#include "dashboard/DashEntity.h"

namespace gcvex {

    DashEntity::DashEntity() : m_id(0) {}

    void DashEntity::initBinding(
        const std::string& path,
        uint8_t typeID,
        void* (*createDataFunc)(),
        Dashboard::Registry::SerializeFunc serializeFunc,
        Dashboard::Registry::ReceiveFunc receiveFunc,
        void* existingDataPtr
    ) {
        m_id = Dashboard::Registry::getOrRegister(path, typeID, createDataFunc, serializeFunc, receiveFunc, existingDataPtr);
    }

    std::string DashEntity::get_path() const {
        return Dashboard::Registry::getPath(m_id);
    }

    std::string DashEntity::get_name() const {
        std::string path = get_path();
        size_t pos = path.find_last_of('/');
        if (pos == std::string::npos) {
            pos = path.find_last_of('\\');
        }
        if (pos != std::string::npos) {
            return path.substr(pos + 1);
        }
        return path;
    }

    std::string DashEntity::get_group() const {
        std::string path = get_path();
        size_t pos = path.find_last_of('/');
        if (pos == std::string::npos) {
            pos = path.find_last_of('\\');
        }
        if (pos != std::string::npos) {
            return path.substr(0, pos);
        }
        return ""; // default group if no slash
    }

    void DashEntity::send(uint8_t opcode) const {
        Dashboard::Registry::send(m_id, opcode);
    }

    void DashEntity::receive(uint8_t opcode, const std::vector<uint8_t>& data) const {
        Dashboard::Registry::receive(m_id, opcode, data);
    }

} // namespace gcvex
