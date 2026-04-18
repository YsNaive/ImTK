using System;
using System.Collections.Generic;

namespace dashboard.Dashboard.Protocol
{
    public static class PacketParser
    {
        private const byte HEADER_TX = 0xEE;

        public static void ProcessBuffer(List<byte> buffer)
        {
            // Console.WriteLine($"[PacketParser] Entering ProcessBuffer. Total Buffer Size: {buffer.Count} bytes"); // TODO: Integrate into Debug Log system

            while (buffer.Count > 0)
            {
                int headerIndex = buffer.IndexOf(HEADER_TX);
                if (headerIndex == -1)
                {
                    // Console.WriteLine($"[PacketParser] Buffer length {buffer.Count} contains no 0xEE header. Clearing buffer."); // TODO: Integrate into Debug Log system
                    // string hexDump = BitConverter.ToString(buffer.ToArray());
                    // Console.WriteLine($"[PacketParser] Discarded Hex: {hexDump}");

                    // try {
                    //     string ascii = System.Text.Encoding.ASCII.GetString(buffer.ToArray());
                    //     Console.WriteLine($"[PacketParser] Discarded ASCII: {ascii}");
                    // } catch {}

                    buffer.Clear();
                    return;
                }

                if (headerIndex > 0)
                {
                    // Console.WriteLine($"[PacketParser] Found header at index {headerIndex}. Discarding {headerIndex} bytes of garbage prefix."); // TODO: Integrate into Debug Log system
                    buffer.RemoveRange(0, headerIndex);
                }

                if (buffer.Count < 2)
                {
                    // Console.WriteLine("[PacketParser] Found header, but waiting for length byte (Buffer < 2)."); // TODO: Integrate into Debug Log system
                    return;
                }

                int payloadLength = buffer[1];
                int headerAndLengthBytes = 2;

                if (payloadLength == 0xFF)
                {
                    if (buffer.Count < 4)
                    {
                        // Console.WriteLine("[PacketParser] Found 0xFF length marker, but waiting for UInt16 bytes (Buffer < 4)."); // TODO: Integrate into Debug Log system
                        return;
                    }
                    payloadLength = buffer[2] | (buffer[3] << 8);
                    headerAndLengthBytes = 4;
                }

                int totalPacketSize = headerAndLengthBytes + payloadLength + 1;

                if (buffer.Count < totalPacketSize)
                {
                    // Console.WriteLine($"[PacketParser] Fragmented Stream: Expected {totalPacketSize} bytes, but only have {buffer.Count}. Waiting..."); // TODO: Integrate into Debug Log system
                    return;
                }

                byte calculatedChecksum = 0;
                for (int i = 0; i < totalPacketSize - 1; i++)
                {
                    calculatedChecksum ^= buffer[i];
                }

                byte receivedChecksum = buffer[totalPacketSize - 1];

                if (calculatedChecksum == receivedChecksum)
                {
                    byte[] payload = new byte[payloadLength];
                    buffer.CopyTo(headerAndLengthBytes, payload, 0, payloadLength);

                    // Console.WriteLine($"[PacketParser] Valid Packet Received. Payload Length: {payloadLength} bytes."); // TODO: Integrate into Debug Log system
                    CommandDispatcher.DispatchPayload(payload);

                    buffer.RemoveRange(0, totalPacketSize);
                }
                else
                {
                    buffer.RemoveAt(0);
                    // Console.WriteLine($"[PacketParser] Checksum mismatch! Calc: {calculatedChecksum:X2}, Recv: {receivedChecksum:X2}. Dropping header and searching again."); // TODO: Integrate into Debug Log system
                }
            }
        }
    }
}