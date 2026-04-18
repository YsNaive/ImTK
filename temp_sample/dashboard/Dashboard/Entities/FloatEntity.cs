using System;
using ImGuiNET;
using ImTK;

namespace dashboard.Dashboard.Entities
{
    [EntityType(0x02, isReference: false)]
    public class FloatEntity : DashEntity
    {
        private float m_value;

        public FloatEntity(byte id, byte typeId, string path) : base(id, typeId, path)
        {
        }

        public override void receive(byte opcode, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            int intVal = 0;
            if (data.Length == 1)
            {
                intVal = (sbyte)data[0];
            }
            else if (data.Length == 2)
            {
                intVal = (short)(data[0] | (data[1] << 8));
            }
            else if (data.Length >= 4)
            {
                intVal = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
            }

            m_value = intVal / 100.0f;
        }

        public override void Render(double deltaTime)
        {
            ImGui.InputFloat(name, ref m_value, 0f, 0f, "%.2f", ImGuiInputTextFlags.ReadOnly);
        }
    }
}