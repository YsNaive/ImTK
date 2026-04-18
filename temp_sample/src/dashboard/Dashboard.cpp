#include "dashboard/Dashboard.h"
#include "dashboard/Registry.h"
#include "framework/Application.h"
#include "gc_config.h"
#include <list>
#include <unordered_map>
#include <vector>
#include <cstdio>
#include <unistd.h>
#include <mutex>

namespace gcvex {
namespace Dashboard {

    namespace {
        const uint8_t HEADER_BYTE_TX = 0xEE; // VEX to C#
        const uint8_t HEADER_BYTE_RX = 0xEF; // C# to VEX
        const int PHYSICAL_MTU = 150;
        const int PACKET_OVERHEAD = 3; // Header(1) + Length(1) + Checksum(1)
        const int MAX_PAYLOAD_SIZE = PHYSICAL_MTU - PACKET_OVERHEAD;
        const int MIN_TICK_BUDGET = 15; // Minimum physical budget remaining to attempt building a new logical packet

        std::list<DashCmd>& getCmdQueue() {
            static std::list<DashCmd> instance;
            return instance;
        }

        std::unordered_map<std::string, std::list<DashCmd>::iterator>& getCmdMap() {
            static std::unordered_map<std::string, std::list<DashCmd>::iterator> instance;
            return instance;
        }

        vex::mutex& getCmdMutex() {
            static vex::mutex instance;
            return instance;
        }

        void receiveLoop(int time, int dt) {
            // Simplified check for incoming serial data (mock implementation).
            // VEXos provides `getchar` but it blocks if no data is available,
            // or returns -1 if non-blocking is set. We assume a non-blocking
            // serial read approach here.

            // For now, this is a placeholder where we would parse incoming
            // commands from C#.
            // If we receive: [V] Fetch (0x01)
            // Registry::queueAllConfigurations();

            // If we receive: [V] Fetch Entity (0x07) <Entity ID>
            // DashEntity* handler = Registry::getHandler(id);
            // if (handler) {
            //     Dashboard::queueCommand(handler->buildCreateCmd());
            //     Dashboard::queueCommand(handler->buildSyncCmd());
            // }
        }

        // Buffer dedicated exclusively for holding a large logical packet (including its header, length, and checksum)
        // that must be split across multiple physical packets.
        std::vector<uint8_t> s_largeCmdBuffer;

        void sendPhysicalPacket(const std::vector<uint8_t>& packetBytes) {
            for (uint8_t b : packetBytes) {
                putchar(b);
            }
            fflush(stdout);
        }

        void dispatchLoop(int time, int dt) {
            receiveLoop(time, dt);

            std::lock_guard<vex::mutex> lock(getCmdMutex());

            auto& s_cmdQueue = getCmdQueue();
            auto& s_cmdMap = getCmdMap();

            int budget = PHYSICAL_MTU;

            // Step 1: Consume any remaining bytes from a previously chunked large command
            if (!s_largeCmdBuffer.empty()) {
                size_t chunkToTake = (s_largeCmdBuffer.size() > static_cast<size_t>(budget)) ? static_cast<size_t>(budget) : s_largeCmdBuffer.size();
                std::vector<uint8_t> chunk(s_largeCmdBuffer.begin(), s_largeCmdBuffer.begin() + chunkToTake);
                sendPhysicalPacket(chunk);

                s_largeCmdBuffer.erase(s_largeCmdBuffer.begin(), s_largeCmdBuffer.begin() + chunkToTake);
                budget -= chunkToTake;
            }

            // Step 2: Attempt to build new logical packets with the remaining budget
            while (budget >= MIN_TICK_BUDGET && !s_cmdQueue.empty()) {

                auto& nextCmd = s_cmdQueue.front();
                size_t payloadLengthRaw = nextCmd.payload.size();

                // Calculate logical packet overhead length. Escape sequence length (3) if size >= 255
                size_t lengthFieldSize = (payloadLengthRaw >= 255) ? 3 : 1;
                size_t totalLogicalSize = 1 /*Header*/ + lengthFieldSize + payloadLengthRaw + 1 /*Checksum*/;

                if (totalLogicalSize > static_cast<size_t>(MAX_PAYLOAD_SIZE + PACKET_OVERHEAD)) {
                    // [Large Packet Mode]
                    // This command is too large to fit in a single physical packet.
                    // We extract it immediately and push its entire logical packet structure into the large buffer.
                    std::vector<uint8_t> logicalPacket;
                    logicalPacket.push_back(HEADER_BYTE_TX);
                    if (payloadLengthRaw >= 255) {
                        logicalPacket.push_back(0xFF);
                        logicalPacket.push_back(static_cast<uint8_t>(payloadLengthRaw & 0xFF));
                        logicalPacket.push_back(static_cast<uint8_t>((payloadLengthRaw >> 8) & 0xFF));
                    } else {
                        logicalPacket.push_back(static_cast<uint8_t>(payloadLengthRaw));
                    }
                    logicalPacket.insert(logicalPacket.end(), nextCmd.payload.begin(), nextCmd.payload.end());

                    uint8_t checksum = 0;
                    for (uint8_t b : logicalPacket) checksum ^= b;
                    logicalPacket.push_back(checksum);

                    // Dequeue
                    s_cmdMap.erase(nextCmd.identifier);
                    s_cmdQueue.pop_front();

                    // Send what we can using the remaining budget in this tick
                    size_t chunkToTake = (logicalPacket.size() > static_cast<size_t>(budget)) ? static_cast<size_t>(budget) : logicalPacket.size();
                    std::vector<uint8_t> chunk(logicalPacket.begin(), logicalPacket.begin() + chunkToTake);
                    sendPhysicalPacket(chunk);

                    // Save the rest for the next ticks
                    if (chunkToTake < logicalPacket.size()) {
                        s_largeCmdBuffer.insert(s_largeCmdBuffer.end(), logicalPacket.begin() + chunkToTake, logicalPacket.end());
                    }
                    budget -= chunkToTake;
                    break; // Budget is fully consumed by the large packet
                }
                else {
                    // [Small Packet Mode]
                    // We must ensure this entire logical packet can fit within the REMAINING budget.
                    // If it cannot fit completely, we do NOT slice it. We leave it in the queue for deduplication.

                    std::vector<uint8_t> smallPacketPayloads;
                    size_t currentLogicalPayloadSize = 0;

                    // Gather as many small commands as possible that fit the budget
                    while (!s_cmdQueue.empty()) {
                        auto& smCmd = s_cmdQueue.front();
                        size_t smSize = smCmd.payload.size();

                        // Condition A: Standard physical limit for small logical packet
                        // Condition B: Can it fit in the current remaining Tick budget?
                        size_t projectedLogicalSize = 1 + 1 + currentLogicalPayloadSize + smSize + 1;

                        if (currentLogicalPayloadSize + smSize > static_cast<size_t>(MAX_PAYLOAD_SIZE) || projectedLogicalSize > static_cast<size_t>(budget)) {
                            break; // Stop packing. Send what we have.
                        }

                        // Accept the command
                        smallPacketPayloads.insert(smallPacketPayloads.end(), smCmd.payload.begin(), smCmd.payload.end());
                        currentLogicalPayloadSize += smSize;

                        s_cmdMap.erase(smCmd.identifier);
                        s_cmdQueue.pop_front();
                    }

                    if (currentLogicalPayloadSize == 0) {
                        // The very first small command couldn't fit the remaining budget.
                        // Abort and yield the remaining budget.
                        break;
                    }

                    // Finalize the small logical packet
                    std::vector<uint8_t> logicalPacket;
                    logicalPacket.push_back(HEADER_BYTE_TX);
                    logicalPacket.push_back(static_cast<uint8_t>(currentLogicalPayloadSize));
                    logicalPacket.insert(logicalPacket.end(), smallPacketPayloads.begin(), smallPacketPayloads.end());

                    uint8_t checksum = 0;
                    for (uint8_t b : logicalPacket) checksum ^= b;
                    logicalPacket.push_back(checksum);

                    sendPhysicalPacket(logicalPacket);
                    budget -= logicalPacket.size();
                }
            }
        }

#if ENABLE_DASHBOARD
        auto registrar = gcvex::Application::registerSubSystem(
            "Dashboard",
            [](){ /* init */ },
            [](){
                // start -> send reset when system starts
                // Use queueCommandFront to ensure Reset is the absolute first command
                // dispatched, even if global variables were allocated beforehand.
                queueCommandFront(CmdReset().build());

                // Transmit all created global configurations right after the reset
                Registry::queueAllConfigurations();
            },
            [](){ /* enable */ },
            [](){ /* disable */ },
            dispatchLoop,
            125 // 8Hz
        );
#endif

    } // anonymous namespace

    void queueCommand(DashCmd cmd) {
#if ENABLE_DASHBOARD
        if (cmd.payload.empty() || cmd.identifier.empty()) return;

        std::lock_guard<vex::mutex> lock(getCmdMutex());

        auto& s_cmdQueue = getCmdQueue();
        auto& s_cmdMap = getCmdMap();

        auto it = s_cmdMap.find(cmd.identifier);
        if (it != s_cmdMap.end()) {
            *(it->second) = std::move(cmd);
        } else {
            s_cmdQueue.push_back(std::move(cmd));
            s_cmdMap[s_cmdQueue.back().identifier] = std::prev(s_cmdQueue.end());
        }
#endif
    }

    void queueCommandFront(DashCmd cmd) {
#if ENABLE_DASHBOARD
        if (cmd.payload.empty() || cmd.identifier.empty()) return;

        std::lock_guard<vex::mutex> lock(getCmdMutex());

        auto& s_cmdQueue = getCmdQueue();
        auto& s_cmdMap = getCmdMap();

        auto it = s_cmdMap.find(cmd.identifier);
        if (it != s_cmdMap.end()) {
            // If it exists, remove it first so we can move it to the front
            s_cmdQueue.erase(it->second);
            s_cmdMap.erase(it);
        }

        s_cmdQueue.push_front(std::move(cmd));
        s_cmdMap[s_cmdQueue.front().identifier] = s_cmdQueue.begin();
#endif
    }

} // namespace Dashboard
} // namespace gcvex
