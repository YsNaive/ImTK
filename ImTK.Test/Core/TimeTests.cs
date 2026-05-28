using ImTK.Core;
using ImTK.Test.Framework;
using System;

namespace ImTK.Test.Core
{
    public class TimeTests : IHeadlessTest
    {
        public void Run()
        {
            TestTimeScale();
        }

        private void TestTimeScale()
        {
            Time.TimeScale = 1.0f;
            Time.Update(0.016); // Simulate 60fps frame

            ImTKAssert.IsTrue(Math.Abs(0.016 - Time.UnscaledDeltaTime) < 0.0001, "Unscaled delta time should match raw input.");
            ImTKAssert.IsTrue(Math.Abs(0.016 - Time.DeltaTime) < 0.0001, "Scaled delta time should match when scale is 1.");
            ImTKAssert.IsFalse(Time.IsPaused, "Time should not be paused.");

            Time.TimeScale = 0.5f;
            Time.Update(0.016);

            ImTKAssert.IsTrue(Math.Abs(0.016 - Time.UnscaledDeltaTime) < 0.0001, "Unscaled delta time should remain unaffected by time scale.");
            ImTKAssert.IsTrue(Math.Abs(0.008 - Time.DeltaTime) < 0.0001, "Scaled delta time should be halved when scale is 0.5.");

            Time.TimeScale = 0f;
            Time.Update(0.016);

            ImTKAssert.IsTrue(Math.Abs(0.016 - Time.UnscaledDeltaTime) < 0.0001, "Unscaled delta time should remain unaffected by time scale.");
            ImTKAssert.IsTrue(Math.Abs(0.0 - Time.DeltaTime) < 0.0001, "Scaled delta time should be zero when paused.");
            ImTKAssert.IsTrue(Time.IsPaused, "Time should be paused.");
            
            // Reset for other tests
            Time.TimeScale = 1.0f;
        }
    }
}
