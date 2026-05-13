using System;
using ImTK.UI;
using ImTK.Test.Framework;

namespace ImTK.Test.UI.VisualTree
{
    [Flags]
    public enum DummyFlags
    {
        None = 0,
        FlagA = 1 << 0,
        FlagB = 1 << 1,
        FlagC = 1 << 2
    }

    public class TestFlags : ElementFlags<DummyFlags>
    {
        public bool hasFlagA { get => GetFlag(DummyFlags.FlagA); set => SetFlag(DummyFlags.FlagA, value); }
        public bool hasFlagB { get => GetFlag(DummyFlags.FlagB); set => SetFlag(DummyFlags.FlagB, value); }
        public bool hasFlagC { get => GetFlag(DummyFlags.FlagC); set => SetFlag(DummyFlags.FlagC, value); }
    }

    public class ElementFlagsTests : IHeadlessTest
    {
        public void Run()
        {
            TestInitialState();
            TestSetAndGet();
            TestMultipleFlags();
        }

        private void TestInitialState()
        {
            TestFlags flags = new TestFlags();
            ImTKAssert.AreEqual(DummyFlags.None, flags.Value, "Initial flags value should be None");
            ImTKAssert.IsFalse(flags.hasFlagA, "hasFlagA should be false initially");
            ImTKAssert.IsFalse(flags.hasFlagB, "hasFlagB should be false initially");
        }

        private void TestSetAndGet()
        {
            TestFlags flags = new TestFlags();

            flags.hasFlagA = true;
            ImTKAssert.IsTrue(flags.hasFlagA, "hasFlagA should be true after setting");
            ImTKAssert.AreEqual(DummyFlags.FlagA, flags.Value, "Value should match FlagA");

            flags.hasFlagB = true;
            ImTKAssert.IsTrue(flags.hasFlagB, "hasFlagB should be true after setting");
            ImTKAssert.AreEqual(DummyFlags.FlagA | DummyFlags.FlagB, flags.Value, "Value should match FlagA | FlagB");

            flags.hasFlagA = false;
            ImTKAssert.IsFalse(flags.hasFlagA, "hasFlagA should be false after unsetting");
            ImTKAssert.AreEqual(DummyFlags.FlagB, flags.Value, "Value should match FlagB after unsetting FlagA");
        }

        private void TestMultipleFlags()
        {
            TestFlags flags = new TestFlags();
            flags.Value = DummyFlags.FlagB | DummyFlags.FlagC;

            ImTKAssert.IsFalse(flags.hasFlagA, "hasFlagA should be false");
            ImTKAssert.IsTrue(flags.hasFlagB, "hasFlagB should be true");
            ImTKAssert.IsTrue(flags.hasFlagC, "hasFlagC should be true");

            flags.hasFlagB = false;
            ImTKAssert.AreEqual(DummyFlags.FlagC, flags.Value, "Value should be FlagC after unsetting B");
        }
    }
}
