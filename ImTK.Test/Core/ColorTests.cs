using System;
using System.Numerics;
using ImTK;
using ImTK.Test.Framework;

namespace ImTK.Test.Core
{
    public class ColorTests : IHeadlessTest
    {
        public void Run()
        {
            TestConstructorAndProperties();
            TestVector4Conversion();
            TestUInt32Conversion();
            TestHSVConversion();
            TestEquality();
        }

        private void TestConstructorAndProperties()
        {
            Color c = new Color(0.1f, 0.2f, 0.3f, 0.4f);
            ImTKAssert.AreEqual(0.1f, c.r, "Color.r mismatch");
            ImTKAssert.AreEqual(0.2f, c.g, "Color.g mismatch");
            ImTKAssert.AreEqual(0.3f, c.b, "Color.b mismatch");
            ImTKAssert.AreEqual(0.4f, c.a, "Color.a mismatch");

            c.rgb = new Vector3(0.5f, 0.6f, 0.7f);
            ImTKAssert.AreEqual(0.5f, c.r, "Color.r after rgb set mismatch");
            ImTKAssert.AreEqual(0.6f, c.g, "Color.g after rgb set mismatch");
            ImTKAssert.AreEqual(0.7f, c.b, "Color.b after rgb set mismatch");
            ImTKAssert.AreEqual(0.4f, c.a, "Color.a should not change after rgb set");
        }

        private void TestVector4Conversion()
        {
            Color c = new Color(1.0f, 0.5f, 0.0f, 1.0f);
            Vector4 v = c;
            ImTKAssert.AreEqual(1.0f, v.X, "Vector4.X mismatch");
            ImTKAssert.AreEqual(0.5f, v.Y, "Vector4.Y mismatch");

            Color c2 = v;
            ImTKAssert.AreEqual(1.0f, c2.r, "Color.r after implicit conversion mismatch");
            ImTKAssert.AreEqual(0.5f, c2.g, "Color.g after implicit conversion mismatch");
        }

        private void TestUInt32Conversion()
        {
            // Fully Red
            Color red = Color.Red;
            uint u = red; // implicit conversion

            // Expected ImGui uint format: ABGR
            // A=255 (FF), B=0 (00), G=0 (00), R=255 (FF)
            // Hex: 0xFF0000FF
            ImTKAssert.AreEqual(0xFF0000FF, u, "UInt32 conversion for Red mismatch");

            // Convert back
            Color back = (Color)u;
            ImTKAssert.AreEqual(1.0f, back.r, "Color.r after uint conversion mismatch");
            ImTKAssert.AreEqual(0.0f, back.g, "Color.g after uint conversion mismatch");
            ImTKAssert.AreEqual(1.0f, back.a, "Color.a after uint conversion mismatch");
        }

        private void TestHSVConversion()
        {
            Color c = Color.Red;
            Vector3 hsv = c.hsv;

            // Red: H=0, S=1, V=1
            ImTKAssert.AreEqual(0.0f, hsv.X, "HSV Hue for Red mismatch");
            ImTKAssert.AreEqual(1.0f, hsv.Y, "HSV Saturation for Red mismatch");
            ImTKAssert.AreEqual(1.0f, hsv.Z, "HSV Value for Red mismatch");

            // Modify Hue to Green (120 degrees = 120/360 = 0.333f)
            c.h = 120f / 360f;
            ImTKAssert.IsTrue(Math.Abs(c.r - 0.0f) < 0.01f, "Color.r after Hue change mismatch");
            ImTKAssert.IsTrue(Math.Abs(c.g - 1.0f) < 0.01f, "Color.g after Hue change mismatch");
            ImTKAssert.IsTrue(Math.Abs(c.b - 0.0f) < 0.01f, "Color.b after Hue change mismatch");
        }

        private void TestEquality()
        {
            Color c1 = new Color(0.1f, 0.2f, 0.3f);
            Color c2 = new Color(0.1f, 0.2f, 0.3f);
            Color c3 = new Color(0.1f, 0.2f, 0.4f);

            ImTKAssert.IsTrue(c1 == c2, "Color equality check failed");
            ImTKAssert.IsTrue(c1 != c3, "Color inequality check failed");
            ImTKAssert.IsTrue(c1.Equals(c2), "Color.Equals failed");
        }
    }
}
