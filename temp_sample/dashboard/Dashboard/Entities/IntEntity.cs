using System;
using ImGuiNET;
using ImTK;

namespace dashboard.Dashboard.Entities
{
    [EntityType(0x01, isReference: false)]
    public class IntEntity : DashEntity
    {
        private int m_value;

        public IntEntity(byte id, byte typeId, string path) : base(id, typeId, path)
        {
        }

        public override void receive(byte opcode, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            if (data.Length == 1)
            {
                m_value = (sbyte)data[0];
            }
            else if (data.Length == 2)
            {
                m_value = (short)(data[0] | (data[1] << 8));
            }
            else if (data.Length >= 4)
            {
                m_value = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
            }
        }

        public override void Render(double deltaTime)
        {
            ImGui.InputInt(name, ref m_value, 0, 0, ImGuiInputTextFlags.ReadOnly);
        }
    }
}