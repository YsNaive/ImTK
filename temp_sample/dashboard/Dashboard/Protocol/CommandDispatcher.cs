using System;
using System.Collections.Generic;

namespace dashboard.Dashboard.Protocol
{
    public static class CommandDispatcher
    {
        public static void DispatchPayload(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return;

            int index = 0;
            while (index < payload.Length)
            {
                byte commandId = payload[index];
                index++;

                switch (commandId)
                {
                    case 0x00: // Reset
                        // Console.WriteLine("[CommandDispatcher] Dispatched Command: [0x00] Reset"); // TODO: Integrate into Debug Log system
                        Core.Registry.Reset();
                        break;

                    case 0x05: // Create Entity
                        if (index + 2 > payload.Length) return;

                        byte entityId = payload[index++];
                        byte typeId = payload[index++];

                        int pathLength = ParseLength(payload, ref index);
                        if (index + pathLength > payload.Length) return;

                        string path = System.Text.Encoding.ASCII.GetString(payload, index, pathLength);
                        index += pathLength;

                        // Console.WriteLine($"[CommandDispatcher] Dispatched Command: [0x05] Create Entity | ID: {entityId}, Type: {typeId:X2}, Path: {path}"); // TODO: Integrate into Debug Log system
                        Core.Registry.CreateEntity(entityId, typeId, path);
                        break;

                    case 0x06: // Sync Entity
                        if (index + 1 > payload.Length) return;

                        byte syncEntityId = payload[index++];
                        int dataLength = ParseLength(payload, ref index);
                        if (index + dataLength > payload.Length) return;

                        byte[] entityData = new byte[dataLength];
                        Array.Copy(payload, index, entityData, 0, dataLength);
                        index += dataLength;

                        // Console.WriteLine($"[CommandDispatcher] Dispatched Command: [0x06] Sync Entity | ID: {syncEntityId}, DataLen: {dataLength}"); // TODO: Integrate into Debug Log system
                        Core.Registry.SyncEntity(syncEntityId, entityData);
                        break;

                    default:
                        Console.WriteLine($"[CommandDispatcher] Unsupported command: {commandId:X2}"); // Error level log, keeping it
                        return;
                }
            }
        }

        private static int ParseLength(byte[] payload, ref int index)
        {
            if (index >= payload.Length) return 0;

            byte lenByte = payload[index++];
            if (lenByte != 0xFF)
            {
                return lenByte;
            }

            if (index + 1 >= payload.Length) return 0;

            int extendedLen = payload[index] | (payload[index + 1] << 8);
            index += 2;
            return extendedLen;
        }
    }
}