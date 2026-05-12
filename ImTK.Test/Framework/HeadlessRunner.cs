using System;
using System.Linq;
using System.Reflection;
using ImTK.Log;

namespace ImTK.Test.Framework
{
    public static class HeadlessRunner
    {
        private static readonly LogContext s_log = new LogContext("HeadlessRunner");

        public static bool RunAllHeadlessTests()
        {
            s_log.Info("========== Starting Headless Tests ==========");

            var testTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IHeadlessTest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (testTypes.Count == 0)
            {
                s_log.Info("No IHeadlessTest implementations found.");
                return true;
            }

            int passed = 0;
            int failed = 0;

            foreach (var type in testTypes)
            {
                try
                {
                    var testInstance = (IHeadlessTest)Activator.CreateInstance(type);
                    s_log.Info($"Running {type.Name}...");
                    testInstance.Run();
                    passed++;
                    s_log.Info($"[PASS] {type.Name}");
                }
                catch (Exception ex)
                {
                    failed++;
                    s_log.Error($"[FAIL] {type.Name}: {ex.Message}");
                }
            }

            s_log.Info($"========== Headless Tests Completed ==========");
            s_log.Info($"Total: {testTypes.Count}, Passed: {passed}, Failed: {failed}");

            return failed == 0;
        }
    }
}
