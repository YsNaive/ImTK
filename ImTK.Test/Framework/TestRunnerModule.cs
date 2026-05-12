using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using ImTK.Core;
using ImTK.Log;
using ImTK.UI;

namespace ImTK.Test.Framework
{
    public class TestRunnerModule : ImTKModule
    {
        private static readonly LogContext s_log = new LogContext("TestRunnerModule");

        private TestRunnerModule() { }

        public class TestRecord
        {
            public IIntegrationTest Instance;
            public bool HasRun;
            public bool Passed;
            public string ErrorMessage;
            public ImTK.UI.Button runButton;
        }

        private List<TestRecord> m_tests = new List<TestRecord>();
        private TestReportWindow m_reportWindow;

        protected internal override void OnInitializeSelf()
        {
            // Scan for all IIntegrationTest
            var testTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IIntegrationTest).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            foreach (var type in testTypes)
            {
                var instance = (IIntegrationTest)Activator.CreateInstance(type);

                var record = new TestRecord
                {
                    Instance = instance,
                    HasRun = false,
                    Passed = false,
                    ErrorMessage = ""
                };
                record.runButton = new ImTK.UI.Button($"Run##{instance.TestName}", evt => RunTest(record.Instance));
                m_tests.Add(record);
            }

            // Auto run non-manual tests
            foreach (var record in m_tests.Where(t => !t.Instance.IsManualOnly))
            {
                RunTest(record);
            }

            m_reportWindow = Window.Open<TestReportWindow>();
            m_reportWindow.SetTests(m_tests, this);
        }

        public void RunTest(IIntegrationTest test)
        {
            var record = m_tests.FirstOrDefault(t => t.Instance == test);
            if (record != null)
            {
                RunTest(record);
            }
        }

        private void RunTest(TestRecord record)
        {
            try
            {
                s_log.Info($"Running Integration Test: {record.Instance.TestName}");
                record.Instance.Run();
                record.Passed = true;
                record.HasRun = true;
                record.ErrorMessage = "";
                s_log.Info($"[PASS] {record.Instance.TestName}");
            }
            catch (Exception ex)
            {
                record.Passed = false;
                record.HasRun = true;
                record.ErrorMessage = ex.Message;
                s_log.Error($"[FAIL] {record.Instance.TestName}: {ex.Message}");
            }
        }
    }

    public class TestReportWindow : Window
    {
        private List<TestRunnerModule.TestRecord> m_tests;
        private TestRunnerModule m_runner;

        public TestReportWindow() : base("Test Report")
        {
        }

        internal void SetTests(List<TestRunnerModule.TestRecord> tests, TestRunnerModule runner)
        {
            m_tests = tests;
            m_runner = runner;
        }

        protected override void OnRenderSelf()
        {
            if (m_tests == null) return;

            var headlessResults = HeadlessRunner.LastResults;

            int total = m_tests.Count + headlessResults.Count;
            int passed = m_tests.Count(t => t.HasRun && t.Passed) + headlessResults.Count(t => t.Passed);
            int failed = m_tests.Count(t => t.HasRun && !t.Passed) + headlessResults.Count(t => !t.Passed);
            int pending = m_tests.Count(t => !t.HasRun);

            ImGui.Text($"Total: {total} | Passed: {passed} | Failed: {failed} | Pending: {pending}");
            ImGui.Separator();

            if (ImGui.BeginTable("Tests", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Status");
                ImGui.TableSetupColumn("Action");
                ImGui.TableSetupColumn("Message");
                ImGui.TableHeadersRow();

                // Render Headless Tests First
                foreach (var result in headlessResults)
                {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.TextDisabled("Headless");

                    ImGui.TableNextColumn();
                    ImGui.Text(result.TestName);

                    ImGui.TableNextColumn();
                    if (result.Passed)
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Passed");
                    }
                    else
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Failed");
                    }

                    ImGui.TableNextColumn();
                    ImGui.TextDisabled("-");

                    ImGui.TableNextColumn();
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        ImGui.TextWrapped(result.ErrorMessage);
                    }
                }

                // Render Integration Tests
                foreach (var record in m_tests)
                {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.TextDisabled("Integration");

                    ImGui.TableNextColumn();
                    ImGui.Text(record.Instance.TestName);

                    if (record.Instance.IsManualOnly)
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled("(Manual)");
                    }

                    ImGui.TableNextColumn();
                    if (!record.HasRun)
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Pending");
                    }
                    else if (record.Passed)
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f), "Passed");
                    }
                    else
                    {
                        ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Failed");
                    }

                    ImGui.TableNextColumn();
                    record.runButton.Render();

                    ImGui.TableNextColumn();
                    if (!string.IsNullOrEmpty(record.ErrorMessage))
                    {
                        ImGui.TextWrapped(record.ErrorMessage);
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
