using System;
using ImTK.Log;

namespace ImTK.Test.Framework
{
    public class ImTKAssertException : Exception
    {
        public ImTKAssertException(string message) : base(message) { }
    }

    /// <summary>
    /// ImTK 專屬的輕量化測試斷言庫，失敗時拋出例外並寫入 ImTKLog.Error。
    /// </summary>
    public static class ImTKAssert
    {
        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                Fail($"Expected true, but was false. {message}");
            }
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                Fail($"Expected false, but was true. {message}");
            }
        }

        public static void AreEqual<T>(T expected, T actual, string message = "")
        {
            if (!Equals(expected, actual))
            {
                Fail($"Expected <{expected}>, but was <{actual}>. {message}");
            }
        }

        public static void NotNull(object obj, string message = "")
        {
            if (obj == null)
            {
                Fail($"Expected not null, but was null. {message}");
            }
        }

        public static void Throws<TException>(Action action, string message = "") where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return; // Expected
            }
            catch (Exception ex)
            {
                Fail($"Expected {typeof(TException).Name}, but threw {ex.GetType().Name}. {message}");
            }

            Fail($"Expected {typeof(TException).Name}, but no exception was thrown. {message}");
        }

        private static void Fail(string message)
        {
            new LogContext("ImTKAssert").Error(message);
            throw new ImTKAssertException(message);
        }
    }
}
