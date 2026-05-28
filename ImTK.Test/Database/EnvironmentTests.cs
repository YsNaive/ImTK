using System;
using System.IO;
using ImTK.Core;
using ImTK.Test.Framework;

namespace ImTK.Test.Database
{
    public class EnvironmentTests : IHeadlessTest
    {
        public void Run()
        {
            string originalOrg = ImTKEnvironment.CompanyName;
            string originalApp = ImTKEnvironment.ApplicationName;

            try
            {
                TestGlobalAssetPath();
                TestLocalDataPathWithOrg();
                TestLocalDataPathWithoutOrg();
            }
            finally
            {
                ImTKEnvironment.CompanyName = originalOrg;
                ImTKEnvironment.ApplicationName = originalApp;
                ImTKEnvironment.LocalDataPath = null;
                ImTKEnvironment.GlobalAssetPath = null;
            }
        }

        private void TestGlobalAssetPath()
        {
            ImTKAssert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, ImTKEnvironment.GlobalAssetPath, "GlobalAssetPath should point to BaseDirectory.");
        }

        private void TestLocalDataPathWithOrg()
        {
            ImTKEnvironment.LocalDataPath = null;
            ImTKEnvironment.CompanyName = "TestOrg";
            ImTKEnvironment.ApplicationName = "TestApp";

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestOrg", "TestApp");
            ImTKAssert.AreEqual(expected, ImTKEnvironment.LocalDataPath, $"LocalDataPath mismatch. Expected {expected}, got {ImTKEnvironment.LocalDataPath}");
        }

        private void TestLocalDataPathWithoutOrg()
        {
            ImTKEnvironment.LocalDataPath = null;
            ImTKEnvironment.CompanyName = "";
            ImTKEnvironment.ApplicationName = "TestAppOnly";

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestAppOnly");
            ImTKAssert.AreEqual(expected, ImTKEnvironment.LocalDataPath, "LocalDataPath should ignore empty CompanyName.");

            ImTKEnvironment.LocalDataPath = null;
            ImTKEnvironment.CompanyName = "   "; // Test whitespace
            ImTKAssert.AreEqual(expected, ImTKEnvironment.LocalDataPath, "LocalDataPath should ignore whitespace CompanyName.");
        }
    }
}
