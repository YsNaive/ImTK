using System;
using System.Collections.Generic;

namespace ImTK.UI
{
    /// <summary>
    /// Manages the dynamic rendering state during the VisualElement layout and render pass.
    /// Helps defer commands that require an active ImGui Window and tracks inherited state (like Font Families).
    /// </summary>
    public static class RenderingContext
    {
        // --- Font State Tracking ---
        private static readonly Stack<int> s_fontFamilyHashStack = new Stack<int>();

        public static int CurrentFontFamilyHash
        {
            get
            {
                if (s_fontFamilyHashStack.Count > 0)
                {
                    return s_fontFamilyHashStack.Peek();
                }
                return ImTKTheme.GlobalTheme.fontFamilyHash;
            }
        }

        public static void PushFontState(int familyHash)
        {
            s_fontFamilyHashStack.Push(familyHash);
        }

        public static void PopFontState()
        {
            if (s_fontFamilyHashStack.Count > 0)
            {
                s_fontFamilyHashStack.Pop();
            }
        }

        // --- Window Scoped Commands ---
        private static bool s_isInsideWindow = false;
        private static readonly Queue<Action> s_pendingWindowCommands = new Queue<Action>();

        public static float CurrentDpiScale { get; set; } = 1.0f;

        public static bool IsInsideWindow
        {
            get => s_isInsideWindow;
            set => s_isInsideWindow = value;
        }

        /// <summary>
        /// Enqueues a command that must be executed inside an ImGui.Begin() / ImGui.End() block.
        /// If currently inside a window, it executes immediately.
        /// </summary>
        public static void EnqueueWindowCommand(Action command)
        {
            if (s_isInsideWindow)
            {
                command?.Invoke();
            }
            else
            {
                s_pendingWindowCommands.Enqueue(command);
            }
        }

        /// <summary>
        /// Flushes all pending window commands. Called by Window.Begin().
        /// </summary>
        public static void FlushPendingCommands()
        {
            while (s_pendingWindowCommands.Count > 0)
            {
                var command = s_pendingWindowCommands.Dequeue();
                command?.Invoke();
            }
        }

        /// <summary>
        /// Clears context state. Useful for resetting state between frames or on application close.
        /// </summary>
        public static void Reset()
        {
            s_fontFamilyHashStack.Clear();
            s_pendingWindowCommands.Clear();
            s_isInsideWindow = false;
            CurrentDpiScale = 1.0f;
        }
    }
}
