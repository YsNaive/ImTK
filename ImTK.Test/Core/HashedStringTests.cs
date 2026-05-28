using System;
using ImTK;
using ImTK.Test.Framework;

namespace ImTK.Test.Core
{
    public class HashedStringTests : IHeadlessTest
    {
        public void Run()
        {
            TestBasicHashing();
            TestEquality();
            TestImplicitConversion();
        }

        private void TestBasicHashing()
        {
            HashedString hs1 = new HashedString("Hello");
            HashedString hs2 = new HashedString("Hello");
            
            ImTKAssert.AreEqual(hs1.Hash, hs2.Hash, "Hashes for the same string should be identical.");
            ImTKAssert.AreEqual("Hello", hs1.Value, "Value should be preserved.");
            
            HashedString hs3 = new HashedString(null);
            ImTKAssert.AreEqual(string.Empty, hs3.Value, "Null should be treated as empty string.");
        }

        private void TestEquality()
        {
            HashedString hs1 = new HashedString("Test");
            HashedString hs2 = new HashedString("Test");
            HashedString hs3 = new HashedString("Other");

            ImTKAssert.IsTrue(hs1 == hs2, "Equality operator should work.");
            ImTKAssert.IsFalse(hs1 == hs3, "Equality operator should detect difference.");
            ImTKAssert.IsTrue(hs1 != hs3, "Inequality operator should work.");
            ImTKAssert.IsTrue(hs1.Equals(hs2), "Equals method should work.");
        }

        private void TestImplicitConversion()
        {
            HashedString hs = "ImplicitString";
            ImTKAssert.AreEqual("ImplicitString", hs.Value, "Implicit string conversion should work.");

            string str = hs;
            ImTKAssert.AreEqual("ImplicitString", str, "Implicit to string conversion should work.");
        }
    }
}
