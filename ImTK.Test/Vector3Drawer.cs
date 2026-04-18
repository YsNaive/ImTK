using ImGuiNET;
using ImTK;
using System.Numerics;

namespace ImTK.Test;

public class Vector3Drawer : RuntimeDrawer<Vector3>
{
    public Vector3Drawer(string label = "", Vector3 initialValue = default) : base(label, initialValue)
    {
        var xDrawer = new FloatDrawer("X", initialValue.X);
        xDrawer.RegisterValueChanged(() =>
        {
            var tmp = value;
            tmp.X = (float)xDrawer.GetValue();
            SetValueWithoutNotify(tmp); // 避免重複觸發，依靠冒泡機制通知父層
        });

        var yDrawer = new FloatDrawer("Y", initialValue.Y);
        yDrawer.RegisterValueChanged(() =>
        {
            var tmp = value;
            tmp.Y = (float)yDrawer.GetValue();
            SetValueWithoutNotify(tmp); // 避免重複觸發，依靠冒泡機制通知父層
        });

        var zDrawer = new FloatDrawer("Z", initialValue.Z);
        zDrawer.RegisterValueChanged(() =>
        {
            var tmp = value;
            tmp.Z = (float)zDrawer.GetValue();
            SetValueWithoutNotify(tmp); // 避免重複觸發，依靠冒泡機制通知父層
        });

        Add(xDrawer);
        Add(yDrawer);
        Add(zDrawer);
    }
}
