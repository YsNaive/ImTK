using System;
using ImTK;

namespace dashboard.Dashboard.Entities
{
    public abstract class DashEntity : VisualElement
    {
        public byte id { get; private set; }
        public byte typeId { get; private set; }
        public string path { get; private set; }
        public string group { get; private set; }
        public string name { get; private set; }

        protected DashEntity(byte id, byte typeId, string path)
        {
            this.id = id;
            this.typeId = typeId;
            this.path = path;

            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                this.group = path.Substring(0, lastSlashIndex);
                this.name = path.Substring(lastSlashIndex + 1);
            }
            else
            {
                this.group = "Inspector";
                this.name = path;
            }
        }

        public abstract void receive(byte opcode, byte[] data);
    }
}