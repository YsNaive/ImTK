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
            string originalOrg = ImTKEnvironment.OrganizationName;
            string originalApp = ImTKEnvironment.ApplicationName;

            try
            {
                TestGlobalAssetPath();
                TestLocalDataPathWithOrg();
                TestLocalDataPathWithoutOrg();
            }
            finally
            {
                ImTKEnvironment.OrganizationName = originalOrg;
                ImTKEnvironment.ApplicationName = originalApp;
            }
        }

        private void TestGlobalAssetPath()
        {
            ImTKAssert.AreEqual(AppDomain.CurrentDomain.BaseDirectory, ImTKEnvironment.GlobalAssetPath, "GlobalAssetPath should point to BaseDirectory.");
        }

        private void TestLocalDataPathWithOrg()
        {
            ImTKEnvironment.OrganizationName = "TestOrg";
            ImTKEnvironment.ApplicationName = "TestApp";

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestOrg", "TestApp");
            ImTKAssert.AreEqual(expected, ImTKEnvironment.LocalDataPath, $"LocalDataPath mismatch. Expected {expected}, got {ImTKEnvironment.LocalDataPath}");
        }

        private void TestLocalDataPathWithoutOrg()
        {
            ImTKEnvironment.OrganizationName = "";
            ImTKEnvironment.ApplicationName = "TestAppOnly";

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestAppOnly");
            ImTKAssert.AreEqual(expected, ImTKEnvironment.LocalDataPath, "LocalDataPath should ignore empty OrganizationName.");

            ImTKEnvironment.OrganizationName = null;
            ImTKAssert.AreEqual(expected, ImTKEnvironment.LocalDataPath, "LocalDataPath should ignore null OrganizationName.");
        }
    }
}
