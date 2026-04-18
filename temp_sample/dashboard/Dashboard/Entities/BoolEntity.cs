using System;
using ImGuiNET;
using ImTK;

namespace dashboard.Dashboard.Entities
{
    [EntityType(0x03, isReference: false)]
    public class BoolEntity : DashEntity
    {
        private bool m_value;

        public BoolEntity(byte id, byte typeId, string path) : base(id, typeId, path)
        {
        }

        public override void receive(byte opcode, byte[] data)
        {
            if (data == null || data.Length == 0) return;

            m_value = (data[0] != 0x00);
        }

        public override void Render(double deltaTime)
        {
            ImGui.BeginDisabled();
            ImGui.Checkbox(name, ref m_value);
            ImGui.EndDisabled();
        }
    }
}