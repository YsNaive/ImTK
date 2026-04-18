using System;
using ImTK;

namespace dashboard.Dashboard.Core
{
    public class DashEntityWindow : WindowView
    {
        private readonly string _groupName;
        public override string displayName => _groupName;

        public DashEntityWindow(string groupName)
        {
            _groupName = groupName;

            minSize = new System.Numerics.Vector2(250, 100);
        }

        public new void Clear()
        {
            base.Clear();
        }
    }
}