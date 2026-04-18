#pragma once

#include <string>
#include <vector>
#include <cstdint>
#include "dashboard/DashCmd.h"
#include "dashboard/Dashboard.h"
#include "dashboard/Registry.h"

namespace gcvex {

    class DashEntity {
    protected:
        uint8_t m_id;

        DashEntity();
        DashEntity(const DashEntity& other) = default;

        // Protected initialization method. Derived classes provide static function
        // delegates to Registry instead of polymorphic object cloning.
        void initBinding(
            const std::string& path,
            uint8_t typeID,
            void* (*createDataFunc)(),
            Dashboard::Registry::SerializeFunc serializeFunc,
            Dashboard::Registry::ReceiveFunc receiveFunc,
            void* existingDataPtr = nullptr // Optional: Provide an existing instance instead of calling createDataFunc
        );

    public:
        virtual ~DashEntity() = default;

        // Path / Name utilities (resolved directly via Registry)
        std::string get_path() const;
        std::string get_name() const;
        std::string get_group() const;

        // Bidirectional communication triggers
        virtual void send(uint8_t opcode = 0x00) const;
        virtual void receive(uint8_t opcode, const std::vector<uint8_t>& data) const;

        uint8_t get_id() const { return m_id; }
    };

} // namespace gcvex
