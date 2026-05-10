using System;

namespace ImTK.Core
{
    /// <summary>
    /// Provides global access to time information and frame duration.
    /// </summary>
    public static class Time
    {
        /// <summary>
        /// The absolute system time when the application first initialized this static class.
        /// </summary>
        public static readonly DateTime StartupTime = DateTime.Now;

        /// <summary>
        /// The time elapsed since the last frame, scaled by TimeScale.
        /// </summary>
        public static double DeltaTime { get; private set; }

        /// <summary>
        /// The true time elapsed since the last frame, unaffected by TimeScale.
        /// </summary>
        public static double UnscaledDeltaTime { get; private set; }

        /// <summary>
        /// The total accumulated scaled time since the application started.
        /// </summary>
        public static double TotalTime { get; private set; }

        /// <summary>
        /// The scale at which time passes. Can be used for slow motion or pausing.
        /// </summary>
        public static float TimeScale { get; set; } = 1.0f;

        /// <summary>
        /// True if TimeScale is exactly 0.
        /// </summary>
        public static bool IsPaused => TimeScale == 0f;

        /// <summary>
        /// Updates the internal time state. Driven internally by ImTKApplication.
        /// </summary>
        internal static void Update(double rawDeltaTime)
        {
            UnscaledDeltaTime = rawDeltaTime;
            DeltaTime = rawDeltaTime * TimeScale;
            TotalTime += DeltaTime;
        }
    }
}
