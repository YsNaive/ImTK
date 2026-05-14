using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImTK.Log;
using ImTK.UI;

namespace ImTK.Test.Framework
{
    public static class HeadlessRunner
    {
        private static readonly LogContext s_log = new LogContext("HeadlessRunner");

        public class HeadlessTestResult
        {
            public string TestName { get; set; }
            public bool Passed { get; set; }
            public string ErrorMessage { get; set; }
        }

        public static List<HeadlessTestResult> LastResults { get; private set; } = new List<HeadlessTestResult>();

        public static bool RunAllHeadlessTests()
        {
            s_log.Info("========== Starting Headless Tests ==========");
            LastResults.Clear();

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
                var result = new HeadlessTestResult { TestName = type.Name };

                try
                {
                    // Ensure the environment is perfectly clean before test
                    EventDispatcher.ClearQueue();

                    var testInstance = (IHeadlessTest)Activator.CreateInstance(type);
                    s_log.Info($"Running {type.Name}...");
                    testInstance.Run();

                    // Cleanup after test in case it left dangling events
                    EventDispatcher.ClearQueue();

                    passed++;
                    result.Passed = true;
                    s_log.Info($"[PASS] {type.Name}");
                }
                catch (Exception ex)
                {
                    // Cleanup even if failed
                    EventDispatcher.ClearQueue();

                    failed++;
                    result.Passed = false;
                    result.ErrorMessage = ex.Message;
                    s_log.Error($"[FAIL] {type.Name}: {ex.Message}");
                }

                LastResults.Add(result);
            }

            s_log.Info($"========== Headless Tests Completed ==========");
            s_log.Info($"Total: {testTypes.Count}, Passed: {passed}, Failed: {failed}");

            return failed == 0;
        }
    }
}
