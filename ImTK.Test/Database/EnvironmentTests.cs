using System;
using System.IO;
using ImTK.Core;

namespace ImTK.Test.Database
{
    public static class EnvironmentTests
    {
        public static void RunTests()
        {
            Console.WriteLine("--- Running EnvironmentTests ---");
            string originalOrg = ImTKEnvironment.OrganizationName;
            string originalApp = ImTKEnvironment.ApplicationName;

            try
            {
                TestGlobalAssetPath();
                TestLocalDataPathWithOrg();
                TestLocalDataPathWithoutOrg();
                Console.WriteLine("EnvironmentTests: All passed.");
            }
            finally
            {
                ImTKEnvironment.OrganizationName = originalOrg;
                ImTKEnvironment.ApplicationName = originalApp;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception($"Test failed: {message}");
        }

        private static void TestGlobalAssetPath()
        {
            Assert(ImTKEnvironment.GlobalAssetPath == AppDomain.CurrentDomain.BaseDirectory, "GlobalAssetPath should point to BaseDirectory.");
        }

        private static void TestLocalDataPathWithOrg()
        {
            ImTKEnvironment.OrganizationName = "TestOrg";
            ImTKEnvironment.ApplicationName = "TestApp";

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestOrg", "TestApp");
            Assert(ImTKEnvironment.LocalDataPath == expected, $"LocalDataPath mismatch. Expected {expected}, got {ImTKEnvironment.LocalDataPath}");
        }

        private static void TestLocalDataPathWithoutOrg()
        {
            ImTKEnvironment.OrganizationName = "";
            ImTKEnvironment.ApplicationName = "TestAppOnly";

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestAppOnly");
            Assert(ImTKEnvironment.LocalDataPath == expected, "LocalDataPath should ignore empty OrganizationName.");

            ImTKEnvironment.OrganizationName = null;
            Assert(ImTKEnvironment.LocalDataPath == expected, "LocalDataPath should ignore null OrganizationName.");
        }
    }
}
