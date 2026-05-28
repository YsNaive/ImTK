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

        public class HeadlessTestResult
        {
            public string TestName { get; set; }
            public bool Passed { get; set; }
            public string ErrorMessage { get; set; }
        }

        public static List<HeadlessTestResult> LastResults { get; private set; } = new List<HeadlessTestResult>();

        public static bool RunAllHeadlessTests()
        {
            ImTKLog.Info("========== Starting Headless Tests ==========");
            LastResults.Clear();

            var context = Hexa.NET.ImGui.ImGui.GetCurrentContext();
            unsafe {
                if (context.Handle == null)
                {
                    Hexa.NET.ImGui.ImGui.CreateContext();
                }
            }

            var testTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IHeadlessTest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (testTypes.Count == 0)
            {
                ImTKLog.Info("No IHeadlessTest implementations found.");
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
                    ImTKLog.Info($"Running {type.Name}...");
                    testInstance.Run();

                    // Cleanup after test in case it left dangling events
                    EventDispatcher.ClearQueue();

                    passed++;
                    result.Passed = true;
                    ImTKLog.Info($"[PASS] {type.Name}");
                }
                catch (Exception ex)
                {
                    // Cleanup even if failed
                    EventDispatcher.ClearQueue();

                    failed++;
                    result.Passed = false;
                    result.ErrorMessage = ex.Message;
                    Console.WriteLine($"[FAIL] {type.Name}: {ex.Message}\n{ex.StackTrace}");
                    ImTKLog.Error($"[FAIL] {type.Name}: {ex.Message}");
                }

                LastResults.Add(result);
            }

            ImTKLog.Info($"========== Headless Tests Completed ==========");
            ImTKLog.Info($"Total: {testTypes.Count}, Passed: {passed}, Failed: {failed}");

            return failed == 0;
        }
    }
}
